using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Nancy.PictureCut
{
    public sealed class ImageManipulatior : IDisposable
    {
        #region Constructors

        ~ImageManipulatior()
        {
            Dispose(false);
        }

        public ImageManipulatior(byte[] binaryFileData)
        {


            // inicializar variáveis
            //this.errmsg = "";
            //this.error = false;
            //_currentDimensions = new Size();

            // this._fileName = fileName;
            //this.imageMeta = new object[0];
            // this.percent = 100;
            _max = new Size(0, 0);


            /*
        // verifique se o arquivo existe
        if(!file_exists(this.fileName)) {
            this.errmsg = 'Arquivo não encontrado';
            this.error = true;
        }
        //verifique se o arquivo é legível
        else if(!is_readable(this.fileName)) {
            this.errmsg = 'O arquivo não é legível';
            this.error = true;
        }
             */


            using (var ms = new MemoryStream(binaryFileData))
                SetImg(Image.FromStream(ms));



#warning z errorem coś trzeba zrobić

        }

        #endregion Constructors

        #region Methods

        // Public Methods 

        public void Crop(Rectangle cropRectangle)
        {
            var size = cropRectangle.Size;
            var newImage = new Bitmap(size.Width, size.Height);

            using (var gnew = Graphics.FromImage(newImage))
            {
                gnew.DrawImage(_img, new Rectangle(-cropRectangle.X, -cropRectangle.Y, _img.Width, _img.Height));
            }
            SetImg(newImage);
        }

        public void Dispose()
        {
            // ta metoda MUSI zawsze wyglądać tak samo
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int CurrentFilesize
        {
            get
            {
                return _lastSavedFileLength;
            }
        }

        public Size CurrentSize
        {
            get { return _currentDimensions; }
        }

        public void Resize(int width = 0, int height = 0)
        {
            _max = new Size(width, height);

            var newDimensions = CalcImageSize(_currentDimensions.Width, _currentDimensions.Height);

            var newImage = new Bitmap(newDimensions.Width, newDimensions.Height);

            using (var gnew = Graphics.FromImage(newImage))
            {
                gnew.DrawImage(_img, new Rectangle(0, 0, newDimensions.Width, newDimensions.Height));
            }
            SetImg(newImage);
        }

        public void Save(string currentFile, IImageCutStorage storage, int quality = 100)
        {
            using (var stream = new MemoryStream())
            {

                SaveToStream(stream, currentFile, quality);
                storage.SaveImage(currentFile, stream);
                _lastSavedFileLength = (int)stream.Length;
            }
        }

        public void SaveToStream(Stream stream, string currentFile, long quality = 100)
        {
            using (var bmp = new Bitmap(_img))
            {

                var ext = new FileInfo(currentFile).Extension.ToLower().TrimStart('.');
                switch (ext)
                {
                    case "jpg":
                        var jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                        var myEncoder = Encoder.Quality;
                        var myEncoderParameters = new EncoderParameters(1);
                        var myEncoderParameter = new EncoderParameter(myEncoder, quality);
                        myEncoderParameters.Param[0] = myEncoderParameter;
                        bmp.Save(stream, jgpEncoder, myEncoderParameters);
                        break;
                    case "png":
                        bmp.Save(stream, ImageFormat.Png);
                        break;
                    case "gif":
                        bmp.Save(stream, ImageFormat.Gif);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("unable to save to {0} file", ext));
                }
            }
        }
        // Protected Methods 

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_img != null)
                _img.Dispose();
        }
        // Private Methods 

        private Size CalcHeight(int width, int height)
        {

            var newHp = (100.0 * _max.Height) / height;
            var newWidth = (int)Math.Round((width * newHp) / 100);
            return new Size(newWidth, _max.Height);


        }

        private Size CalcImageSize(int width, int height)
        {
            var newSize = new Size(width, height); //  array('newWidth'=>width,'newHeight'=>height);

            if (_max.Width > 0)
            {

                newSize = CalcWidth(width, height);

                if (_max.Height > 0 && newSize.Height > _max.Height)
                    newSize = CalcHeight(newSize.Width, newSize.Height);

                //this.newDimensions = newSize;
            }

            // ReSharper disable once InvertIf
            if (_max.Height > 0)
            {
                newSize = CalcHeight(width, height);
                if (_max.Width > 0 && newSize.Width > _max.Width)
                    newSize = CalcWidth(newSize.Width, newSize.Height);
            }

            return newSize;
        }

        private Size CalcWidth(int width, int height)
        {

            var newWp = (100.0 * _max.Width) / width;
            var newHeight = (int)Math.Round((height * newWp) / 100);
            return new Size(_max.Width, newHeight);

        }


        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        #endregion Methods

        #region Fields

        private int _lastSavedFileLength;
        private Size _currentDimensions;
        private Size _max;
        private Image _img;

        void SetImg(Image newImage)
        {
            if (ReferenceEquals(_img, newImage))
                return;
            if (_img != null)
                _img.Dispose();
            _img = newImage;
            _currentDimensions = _img != null ? _img.Size : Size.Empty;

        }

        // private int percent;

        #endregion Fields
    }
}
