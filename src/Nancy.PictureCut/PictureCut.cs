using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Nancy.PictureCut
{
    public class PictureCut
    {
		#region Constructors 

        static PictureCut()
        {
            R1 = new Dictionary<string, string[]>
                {
                    {"request"               , new []{"required"}},
			        {"inputOfFile"           , new []{"required"}},
			        {"enableResize"          , new []{"bool"}},
			        {"minimumWidthToResize"  , new []{"int"}},
			        {"minimumHeightToResize" , new []{"int"}},
			        {"folderOnServer"        , new []{"required"}},
			        {"imageNameRandom"       , new []{"bool"}},
			        {"maximumSize"           , new []{"int"}},
			        {"enableMaximumSize"	 , new []{"required"}}
                };
            R2 = new Dictionary<string, string[]>
            {

                {"folderOnServer", new[] {"required"}},
                {"inputOfFile", new[] {"required"}},
                {"maximumSize", new[] {"int"}},
                {"enableMaximumSize", new[] {"required"}},
                {"toCropImgX", new[] {"int"}},
                {"toCropImgY", new[] {"int"}},
                {"toCropImgW", new[] {"int"}},
                {"toCropImgH", new[] {"int"}},
                 {"status", new []{"bool"}},
            };
        }

        public PictureCut(Dictionary<string, object> post, HttpFile file)
        {
            Dictionary<string, string[]> uploadValidations;

            object request1;
            post.TryGetValue("request", out request1);
            Request = (request1 ?? "").ToString();
            //  this.request = request;
            switch (Request)
            {
                case "upload":
                    uploadValidations = R1;
                    if (Validation(uploadValidations, post))
                    {
                        PopulateFromArray(post);
                        PopulateFileFromStream(file);
                    }
                    break;
                case "crop":
                    uploadValidations = R2;
                    if (Validation(uploadValidations, post))
                    {
                        PopulateFromArray(post);
                        CurrentFile = FolderPath + CurrentFileName;
                        // populateFileFromStream(file);
                    }
                    break;
                default:
                    throw new Exception("request variable is not valid");
            }
        }

		#endregion Constructors 

		#region Static Methods 

		// Private Methods 

        private static bool Validation(Dictionary<string, string[]> rules, Dictionary<string, object> data)
        {
            foreach (var kv in rules)
            {
                var key = kv.Key;
                var value = kv.Value;
                if (value == null || value.Length <= 0) continue;
                if (!data.ContainsKey(key) || data[key] == null)
                    throw new Exception(key + " variable is required");
                foreach (var rule in value)
                {
                    switch (rule)
                    {
                        case "int":
                            decimal intValue;
                            var tmp = data[key].ToString();
                            if (!decimal.TryParse(tmp, NumberStyles.Any, CultureInfo.InvariantCulture, out intValue))
                                throw new Exception(key + " variable is not " + rule);
                            data[key] = (int)Math.Round(intValue);
                            break;
                        case "bool":
                            bool boolValue;
                            var v = data[key].ToString().ToLower().Trim();
                            if (!bool.TryParse(v, out boolValue))
                                throw new Exception(key + " variable is not " + rule);
                            data[key] = boolValue;
                            break;
                    }
                }
            }
            return true;
        }

		#endregion Static Methods 

		#region Methods 

		// Public Methods 

        public bool Crop(IImageCutStorage storage)
        {
            try
            {
                byte[] bs;
                using (var ms = new MemoryStream())
                {
                    using (var src = storage.GetSavedImageStream(CurrentFile))
                    {
                        src.CopyTo(ms);
                    }
                    bs = ms.ToArray();
                }
                var image = new ImageManipulatior(bs);
                image.Crop(_cropRectangle);
                image.Save(CurrentFile, storage);
                CurrentFileSize = image.CurrentFilesize;
                CurrentSize = image.CurrentSize;
                Status = true;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                Status = false;
            }
            return Status;
        }

        public object ExceptionsToJson()
        {
            return new Dictionary<string, object>
            {
                {"status", Status},
                {"request", Request},
                {"errorMessage", ErrorMessage},
            };
        }

        public object ToJson()
        {
            return new Dictionary<string, object>
            {
                {"status", Status},
                {"currentFileName", CurrentFileName},
                {"currentWidth", CurrentSize.Width},
                {"currentHeight", CurrentSize.Height},
                {"currentFileSize", CurrentFileSize},
                {"request", Request}
            };
        }

        public bool Upload(IImageCutStorage storage)
        {
            try
            {
                _imageManipulatior = new ImageManipulatior(BinaryFileData);
                CurrentSize = _imageManipulatior.CurrentSize;

                var doSave = true;

                if (EnableResize)
                {

                    if ((CurrentSize.Width > MinimumWidthToResize) ||
                        (CurrentSize.Height > MinimumHeightToResize))
                    {
                        _imageManipulatior.Resize(MinimumWidthToResize, MinimumHeightToResize);
                        _imageManipulatior.Save(CurrentFile, storage);
                        doSave = false;
                    }

                }
                if (doSave)
                    _imageManipulatior.Save(CurrentFile, storage);


                CurrentFileSize = _imageManipulatior.CurrentFilesize;
                CurrentSize = _imageManipulatior.CurrentSize;
                Status = true;

                return true;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                Status = false;
                return Status;
            }
        }
		// Private Methods 

        private void PopulateFileFromStream(HttpFile aFileStream)
        {
            FileStream = aFileStream;
            FileName = aFileStream.Name;
            var tmp = FileName.Split('.');
            var fileExtension = tmp.Last();

            using (var myStream = new MemoryStream())
            {
                aFileStream.Value.CopyTo(myStream);
                BinaryFileData = myStream.ToArray();
            }

            FileType = fileExtension;

            if (ImageNameRandom)
            {

                var newName = Guid.NewGuid().ToString("N") + "." + FileType;
                // dechex(round(rand(0,999999999999999))).".".this.fileType;
#warning Opuszczone
                //			while(file_exists(this.folderPath.newName))
                //			{
                //				newName = dechex(round(rand(0,999999999999999))).".".this.fileType;
                //			}
                CurrentFileName = newName;
            }
            else
            {
                CurrentFileName = FileName;
            }

            CurrentFile = FolderPath + CurrentFileName;
        }

        private void PopulateFromArray(Dictionary<string, object> data)
        {
            var allProperties = typeof(PictureCut)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(aa => aa.GetIndexParameters().Length == 0);
            var propertyDictionary = allProperties.ToDictionary(aa => aa.Name.ToLower(), aa => aa);
            foreach (var k in data)
            {
                PropertyInfo pi;
                if (propertyDictionary.TryGetValue(k.Key.ToLower(), out pi))
                {
                    var value = k.Value;
                    if (pi.PropertyType == typeof(int) && value is string)
                        value = int.Parse((string)value);
                    pi.SetValue(this, value, null);
                }
                else
                {
                    switch (k.Key)
                    {
                        case "toCropImgX":
                            _cropRectangle = new Rectangle((int)k.Value, _cropRectangle.Y, _cropRectangle.Width, _cropRectangle.Height);
                            break;
                        case "toCropImgY":
                            _cropRectangle = new Rectangle(_cropRectangle.X, (int)k.Value, _cropRectangle.Width, _cropRectangle.Height);
                            break;
                        case "toCropImgW":
                            _cropRectangle = new Rectangle(_cropRectangle.X, _cropRectangle.Y, (int)k.Value, _cropRectangle.Height);
                            break;
                        case "toCropImgH":
                            _cropRectangle = new Rectangle(_cropRectangle.X, _cropRectangle.Y, _cropRectangle.Width, (int)k.Value);
                            break;
                    }
                }
            }
#warning TempPath  $_SERVER["DOCUMENT_ROOT"]
            FolderPath = Path.Combine(Path.GetTempPath(), FolderOnServer);
            //	$this->maximumSize				= $this->maximumSize * 1024;
        }

		#endregion Methods 

		#region Static Fields 

        private static readonly Dictionary<string, string[]> R1;
        private static readonly Dictionary<string, string[]> R2;

		#endregion Static Fields 

		#region Fields 

        private Rectangle _cropRectangle;
        private ImageManipulatior _imageManipulatior;

		#endregion Fields 

		#region Properties 

        public byte[] BinaryFileData { get; set; }

        public string CurrentFile { get; set; }

        public string CurrentFileName { get; set; }

        public int CurrentFileSize { get; set; }

        public Size CurrentSize { get; set; }

        public bool EnableResize { get; set; }

        public object ErrorMessage { get; set; }

        private string FileName { get; set; }

        private HttpFile FileStream { get; set; }

        public string FileType { get; set; }

        /// <summary>
        /// To dostaję requestem i (DANGEROUS)
        /// </summary>
        public string FolderOnServer { get; set; }

        /// <summary>
        /// Full path
        /// </summary>
        public string FolderPath { get; set; }

        public bool ImageNameRandom { get; set; }

        public int MaximumSize { get; set; }

        public int MinimumHeightToResize { get; set; }

        public int MinimumWidthToResize { get; set; }

        public string Request { get; set; }

        public bool Status { get; set; }

		#endregion Properties 
    }
}
