using Nancy;
using Nancy.Bootstrapper;

namespace Nancy.PictureCut.Demo.Code
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AppStartup : IApplicationStartup
    {
        #region Constructors

        public AppStartup(IRootPathProvider provider)
        {
            RootPathProvider = provider;
        }

        #endregion Constructors

        #region Methods

        // Public Methods 

        public void Initialize(IPipelines pipelines)
        {

        }

        #endregion Methods

        #region Static Properties

        // ReSharper disable once MemberCanBePrivate.Global
        internal static IRootPathProvider RootPathProvider { get; set; }

        #endregion Static Properties
    }
}