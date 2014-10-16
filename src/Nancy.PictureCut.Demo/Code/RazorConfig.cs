using System.Collections.Generic;
using Nancy.ViewEngines.Razor;

namespace Nancy.PictureCut.Demo.Code
{
    public class RazorConfig : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            //TODO: Uzupełnij listę assembly dla Razora
            return null;
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            //TODO: Uzupełnij listę namespace dla Razora
            return new[] {
                "System.Collections",
                "Nancy.ViewEngines.Razor",
                "System.Linq"
            };
        }

        public bool AutoIncludeModelNamespace
        {
            get { return true; }
        }
    }
}
