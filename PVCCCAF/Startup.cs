using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PVCCCAF.Startup))]
namespace PVCCCAF
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
