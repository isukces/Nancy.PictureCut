using System;

namespace Nancy.PictureCut
{
    public static class ImageCutExtension
    {


        public static string JsSerialize(this object x)
        {
            if (x == null)
                return "null";
            if (x is EncodedText)
                return ((EncodedText)x).Text;
            if (x is string)
                return string.Format("'{0}'", (x as string).Replace("\\", "\\\\").Replace("'", "\\'"));
            if (x is bool)
                return ((bool)x).ToString().ToLower();
            throw new NotImplementedException(string.Format("Unable to serialize {0} to javascript", x.GetType().ToString()));
        }
 
    }
}
