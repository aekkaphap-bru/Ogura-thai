using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OCTWEB_NET45.Startup))]
namespace OCTWEB_NET45
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
