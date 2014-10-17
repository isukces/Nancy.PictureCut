using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Nancy.PictureCut.Demo.Modules
{
    public class DemoModule : NancyModule
    {
        private PictureCutWrapper _pictureCutWrapper;

        public DemoModule()
            : base(Base.Substring(1))
        {

            Get["/"] = _GetPhoto;
            //  Post["/"] = _PostPhoto;

            CreateImageCutWrapper();
        }
        private object _GetPhoto(dynamic arg)
        {
            // var blob = Storage.GetPhotoBlob(id);
            // _myImageCutWrapper.DefaultImageButton = blob.Uri.ToString() + "?rand=" + Guid.NewGuid().ToString("N");
            ViewBag["pictureCut"] = _pictureCutWrapper;
            return View["Index", new object()];
        }

        /*
        private object _PostPhoto(dynamic arg)
        {
            // var a = (Request.Form as DynamicDictionary).ToDictionary().Keys.ToArray();
            string imageGuid = Request.Form.imageGuid;
            Guid id = Request.Form.id;
          
            {


                byte[] bytes;
                var cf = StorageHelper.GetPhotoBlobName(id);
                using (var stream = _myImageCutWrapper.Storage.GetSavedImageStream(imageGuid))
                {
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        bytes = ms.ToArray();
                    }
                }

                using (var ms = new MemoryStream())
                {
                    using (var imageResizer = new ImageManipulatior(bytes))
                    {
                        imageResizer.Resize(128, 128);
                        imageResizer.SaveToStream(ms, cf, 100);
                    }
                    ms.Position = 0;
                    var blob = Storage.GetPhotoBlob(id);
                    blob.UploadFromStream(ms);
                }


            }
            var isCurrentUser = id == LoggedConexxPerson.Id;
            // var url = id == LoggedConexxPerson.Id ? Base : (Base + id.ToString("D"));
            return Response.AsRedirect(Base + (isCurrentUser ? "" : ("/" + id.ToString("B"))));
            // return Response.AsRedirect(Context.ToFullPath(url));
            //return _GetPhotoX(id);
        }
         */



        private void CreateImageCutWrapper()
        {

            var imageCutStorage = new FileImageCutStorage(Path.Combine(Path.GetTempPath(), "photoStorage"), true);
            _pictureCutWrapper = new PictureCutWrapper(this, imageCutStorage)
            {
                ServerSideAction = Base + "/Photo/ImageCutter/",
                FolderOnServer = Base + "/MyTemporaryImages/",
                EnableCrop = true,
                CropWindowStyle = CropWindowStyles.Bootstrap,
                CropMode = CropModes.Square | CropModes.Widescreen | CropModes.Free,
                UploadedCallback = @"function(data){ alert('This is custom javascript code');}"
            };
            _pictureCutWrapper.Init();
        }

        public const string Base = "~/Demo";
    }
}