using System.IO;

namespace Nancy.ImageCut
{
    public interface IImageCutStorage
    {
        void SaveImage(string name, Stream stream);
        Stream GetSavedImageStream(string name);
    }
}
