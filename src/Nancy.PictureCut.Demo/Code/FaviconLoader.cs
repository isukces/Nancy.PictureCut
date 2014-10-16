using System;
using System.Drawing;
using System.IO;

namespace Nancy.PictureCut.Demo.Code
{
    public static class FaviconLoader
    {
        #region Static Methods

        // Private Methods 

        private static byte[] PngToIcon(byte[] pngBinary)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var srcStream = new MemoryStream(pngBinary))
                {
                    var bitmap = (Bitmap)Image.FromStream(srcStream);
                    Icon.FromHandle(bitmap.GetHicon()).Save(outputStream);
                }
                return outputStream.ToArray();
            }
        }
        // Internal Methods 

        internal static byte[] LoadFavIcon()
        {
            var rootProjectNamespace = typeof(FaviconLoader).Namespace ?? "";
            var faviconPng = rootProjectNamespace.Substring(0, rootProjectNamespace.LastIndexOf('.'))
                + ".Content.images.favicon.png";
            if (_wasLoaded)
                return _faviconBinary;
            _wasLoaded = true;
            var assembly = typeof(FaviconLoader).Assembly;
            using (var resourceStream = assembly.GetManifestResourceStream(faviconPng))
            {
                if (resourceStream == null)
                    return _faviconBinary;
                _faviconBinary = new byte[resourceStream.Length];
                resourceStream.Read(_faviconBinary, 0, (int)resourceStream.Length);
            }

            _faviconBinary = PngToIcon(_faviconBinary);
            return _faviconBinary;
        }

        #endregion Static Methods

        #region Static Fields

        private static byte[] _faviconBinary;
        private static bool _wasLoaded;

        #endregion Static Fields
    }
}