using System.IO;

namespace Nancy.PictureCut
{
    public class FileImageCutStorage : IImageCutStorage
    {
        #region Constructors

        public FileImageCutStorage(string directoryName, bool ignoreSavedFileDirectories)
        {
            _directoryName = directoryName;
            _ignoreSavedFileDirectories = ignoreSavedFileDirectories;
        }

        #endregion Constructors

        #region Methods

        // Public Methods 

        public Stream GetSavedImageStream(string name)
        {
            var fi = PrepareFileInfo(name);
            return new FileStream(fi.FullName, FileMode.Open);
        }

        public void SaveImage(string name, Stream stream)
        {
            var fi = PrepareFileInfo(name);

            using (var fs = new FileStream(fi.FullName, FileMode.Create))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fs);
            }
        }
        // Private Methods 

        private FileInfo PrepareFileInfo(string name)
        {
            name = name.Replace("/", "\\").TrimStart('\\');
            var fullName = Path.Combine(_directoryName, name);
            if (_ignoreSavedFileDirectories)
                fullName = Path.Combine(_directoryName, new FileInfo(fullName).Name);
            var fi = new FileInfo(fullName);
            if (fi.Directory != null)
                fi.Directory.Create();
            return fi;
        }

        #endregion Methods

        #region Fields

        private readonly string _directoryName;
        private readonly bool _ignoreSavedFileDirectories;

        #endregion Fields
    }
}
