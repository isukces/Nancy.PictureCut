using System.IO;

namespace Nancy.PictureCut
{
    public interface IImageCutStorage
    {
        void SaveImage(string name, Stream stream);
        Stream GetSavedImageStream(string name);
    }
}
