namespace Nancy.PictureCut
{
    public class EncodedText
    {
		#region Constructors 

        public EncodedText(string text)
        {
            Text = text;
        }

		#endregion Constructors 

		#region Properties 

        public string Text { get; private set; }

		#endregion Properties 
    }
}
