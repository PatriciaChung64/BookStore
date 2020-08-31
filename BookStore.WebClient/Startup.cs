using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BookStore.WebClient.Startup))]
namespace BookStore.WebClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
       
        }
    }
}
