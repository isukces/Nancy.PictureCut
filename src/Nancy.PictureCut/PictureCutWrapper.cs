using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Nancy.Extensions;
using Nancy.ViewEngines.Razor;

namespace Nancy.ImageCut
{
    public class PictureCutWrapper
    {
        #region Constructors

        static PictureCutWrapper()
        {
            ExtensionToMime = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var info in ImageCodecInfo.GetImageDecoders())
                foreach (var extension in info.FilenameExtension.Split(';'))
                    ExtensionToMime[extension.TrimStart('.', '*')] = info.MimeType;
        }

        public PictureCutWrapper(NancyModule module, IImageCutStorage storage)
        {
            _module = module;
            _storage = storage;
            ServerSideAction = "/03CA6440-825E-4F73-BD55-92CFD3E15AA5/F5017C23-2240-4406-8B4B-48292D2C9BAD";
            ContainerId = "container_image";
            InputOfImageDirectory = "image" + Guid.NewGuid().ToString("N");
            PluginFolderOnServer = "~/";
            CropMode = CropModes.Free | CropModes.Square;

        }

        #endregion Constructors

        #region Static Methods

        // Private Methods 

        private static string GetMimeTypeByExtension(string extension)
        {
            string mime;
            return ExtensionToMime.TryGetValue(extension.TrimStart('.', '*'), out mime) ? mime : "";
        }

        #endregion Static Methods

        #region Methods

        // Public Methods 

        public void Init()
        {
            Func<string, string> processPath = (path) => ("/" + (path ?? "").TrimStart('~', '/')).TrimEnd('/') + "/";
            // this method is called in module's constructor so _context is null and we cannod use 
            // _context.ToFullPath
            var modulePath = processPath(_module.ModulePath);

            #region Handle upload/crop action
            {
                var path = processPath(ServerSideAction);

                if (path.StartsWith(modulePath))
                {
                    path = path.Substring(modulePath.Length);
                    path = "/" + path.TrimStart('/');
                }
                else
                    throw new Exception(
                        string.Format(
                            "ActionToUploadProperty value must start from module.ModulePath, currently: '{0}'",
                            _module.ModulePath));
                // Post["/Photo/Upload/"] = _PostPhotoUpload;
                _module.Post[path] = (_) => ProcessPost();
            }

            #endregion

            #region Handle image request

            {
                var folderOnServer = processPath(FolderOnServer);

                if (folderOnServer.StartsWith(modulePath))
                {
                    folderOnServer = folderOnServer.Substring(modulePath.Length);
                    folderOnServer = "/" + folderOnServer.TrimStart('/');
                    folderOnServer += "{id}";
                }
                else
                    throw new Exception(
                        string.Format(
                            "ActionToUploadProperty value must start from module.ModulePath, currently: '{0}'",
                            _module.ModulePath));
                // Post["/Photo/Upload/"] = _PostPhotoUpload;
                _module.Get[folderOnServer] = (_) => ProcessGetImage(_.id);
            }

            #endregion

        }

        public IHtmlString RenderContainer()
        {
            return new NonEncodedHtmlString(string.Format("<div id=\"{0}\"></div>", ContainerId));
        }


        public IHtmlString RenderJsLinks(NancyContext context)
        {
            const string remplate = "<script language=\"javascript\" src=\"{0}\"></script>\r\n";
            var url = context.ToFullPath("~" + StaticContentModule.MainJs);
            return new NonEncodedHtmlString(string.Format(remplate, url));
        }

        public IHtmlString RenderJsCode()
        {
            const bool withScriptTagBound = true;
            var context = _module.Context;
            if (context == null)
                throw new Exception("Unable to RenderJs because module.Context is null ");
            var sb = new StringBuilder();
            if (withScriptTagBound)
                sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendFormat("$({0}).PictureCut({{\r\n", ("#" + ContainerId).JsSerialize());
            {
                var dir = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(InputOfImageDirectory))
                    dir["InputOfImageDirectory"] = InputOfImageDirectory;
                if (!string.IsNullOrEmpty(PluginFolderOnServer))
                    dir["PluginFolderOnServer"] = context.ToFullPath(PluginFolderOnServer);
                if (!string.IsNullOrEmpty(FolderOnServer))
                    dir["FolderOnServer"] = context.ToFullPath(FolderOnServer);
                dir["EnableCrop"] = EnableCrop;
                dir["CropWindowStyle"] = CropWindowStyle.ToString();
                if (!string.IsNullOrEmpty(DefaultImageButton))
                    dir["DefaultImageButton"] = DefaultImageButton;




                if (!string.IsNullOrEmpty(ServerSideAction))
                {
                    dir["ActionToSubmitUpload"] = context.ToFullPath(ServerSideAction);
                    dir["ActionToSubmitCrop"] = context.ToFullPath(ServerSideAction);
                }
                dir["CropOrientation"] = CropOrientation;

                if (!string.IsNullOrEmpty(UploadedCallback))
                    dir["UploadedCallback"] = new EncodedText(UploadedCallback);
                {
                    var cm = Enum.GetValues(typeof(CropModes)).OfType<CropModes>().Select(xx => string.Format("{0}: {1}", xx, (CropMode & xx) > 0)).ToList();
                    //x.Add("    CropModes: {" + string.Join(", ", cm).ToLower() + "}");
                    dir["CropModes"] = new EncodedText("{" + string.Join(", ", cm).ToLower() + "}");
                }
                var x =
                    dir.Select(
                        keyValuePair => string.Format("    {0}: {1}", keyValuePair.Key, keyValuePair.Value.JsSerialize()))
                        .ToList();
                sb.AppendLine("    " + string.Join(",\r\n", x));
            }
            sb.AppendLine("});");
            if (withScriptTagBound)
                sb.AppendLine("</script>");


            return new NonEncodedHtmlString(sb.ToString());
        }
        // Private Methods 

        private dynamic ProcessGetImage(string id)
        {
            var s = _storage.GetSavedImageStream(id);
            var mime = GetMimeTypeByExtension(new FileInfo(id).Extension);
            return _module.Response.FromStream(s, mime);
        }

        private dynamic ProcessPost()
        {
            var file = _module.Request.Files.FirstOrDefault();
            Dictionary<string, object> post = _module.Request.Form.ToDictionary();

            object request1;
            post.TryGetValue("request", out request1);
            switch ((request1 ?? "").ToString())
            {
                case "upload":
                    {
                        var pictureCut = new PictureCut(post, file);
                        var result = pictureCut.Upload(_storage)
                            ? pictureCut.ToJson()
                            : pictureCut.ExceptionsToJson();
                        return _module.Response.AsJson(result);
                    }

                case "crop":
                    {
                        //var file = _module.Request.Files.FirstOrDefault();
                        // Dictionary<string, object> post = _module.Request.Form.ToDictionary();
                        //foreach (var o in post)
                        //   System.Diagnostics.Debug.WriteLine("{0}=>{1}", o.Key, o.Value);
                        //return null;
                        var pictureCut = new PictureCut(post, null);
                        var result = pictureCut.Crop(_storage)
                            ? pictureCut.ToJson()
                            : pictureCut.ExceptionsToJson();
                        return _module.Response.AsJson(result);
                    }
                default:
                    return HttpStatusCode.NotFound;
            }
        }

        #endregion Methods

        #region Static Fields

        private static readonly Dictionary<string, string> ExtensionToMime;

        #endregion Static Fields

        #region Fields

        private readonly NancyModule _module;
        private readonly IImageCutStorage _storage;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Id of div tag in html, default value is container_image
        /// </summary>
        public string ContainerId { get; set; }

        public CropModes CropMode { get; set; }

        public bool CropOrientation { get; set; }

        public CropWindowStyles CropWindowStyle { get; set; }

        public string DefaultImageButton { get; set; }

        public bool EnableCrop { get; set; }

        public string FolderOnServer { get; set; }

        /// <summary>
        /// Name of input control and some other elements, default random value
        /// </summary>
        public string InputOfImageDirectory { get; set; }

        public string PluginFolderOnServer { get; set; }

        public string ServerSideAction { get; set; }

        public IImageCutStorage Storage
        {
            get { return _storage; }
        }

        /// <summary>
        /// Object { status: true, currentFileName: "242627a9b8104774920737ba60a73187.jpg", currentWidth: 128, currentHeight: 128, currentFileSize: 10904, request: "upload", options: Object }
        /// </summary>
        public string UploadedCallback { get; set; }

        #endregion Properties

        /*<script type="text/javascript">
    $("#container_image").PictureCut({
        InputOfImageDirectory: "image",
        PluginFolderOnServer: '@RenderContext.Context.ToFullPath("~/jquery.picture.cut/")',
        FolderOnServer: '@RenderContext.Context.ToFullPath("~/uploads/")',
        EnableCrop: true,
         CropWindowStyle: "Bootstrap",
        ActionToSubmitUpload: '@RenderContext.Context.ToFullPath(AccountModule.Base+"/Photo/Upload/")',
        bootstrap: "dupa.txt"
        /*,
        CropWindowStyle: {
            "jqueryui": "aa/windows/window.jqueryui.php",
            "popstyle": "aaa/windows/window.popstyle.php",
            "bootstrap": "aaaa/windows/window.bootstrap.php"
        } * /
    });
</script> */
    }
    [Flags]
    public enum CropModes
    {
        Widescreen = 1,
        Letterbox = 2,
        Free = 4,
        Square = 8
    }
}
