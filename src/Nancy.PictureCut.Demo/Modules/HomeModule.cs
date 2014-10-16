using Nancy;

namespace Nancy.PictureCut.Demo.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = o => View["Index", new Model()];
        }

        public class Model
        {
            
        }
    }
}