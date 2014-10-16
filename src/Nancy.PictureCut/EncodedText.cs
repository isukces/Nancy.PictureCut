using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nancy.ImageCut
{
    public class EncodedText
    {
        public EncodedText(string text)
        {
            Text = text;
        }

        public string Text { get; private set; }
    }
}
