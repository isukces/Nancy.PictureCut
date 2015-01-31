using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Security;
using Nancy.Session;
using Nancy.TinyIoc;

namespace Nancy.PictureCut.Demo.Code
{
    // ReSharper disable once UnusedMember.Global
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        #region Methods

        // Protected Methods 

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            StaticConfiguration.DisableErrorTraces = false;
            CookieBasedSessions.Enable(pipelines);
            Csrf.Enable(pipelines);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            // https://github.com/NancyFx/Nancy/wiki/Managing-static-content

            nancyConventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("/Scripts", "Scripts")
                );
            nancyConventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("/fonts", "fonts")
                );
        }
        #endregion Methods

        #region Properties

        protected override byte[] FavIcon
        {
            get { return FaviconLoader.LoadFavIcon(); }
        }

        #endregion Properties
    }
}