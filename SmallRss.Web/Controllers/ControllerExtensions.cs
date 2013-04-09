using Raven.Client;
using Raven.Client.Linq;
using SmallRss.Web.Models;
using System.Linq;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Mvc;

namespace SmallRss.Web.Controllers
{
    public static class ControllerExtensions
    {
        public static UserAccount CurrentUser(this ApiController controller, IDocumentSession session)
        {
            return CurrentUser(controller.User, session);
        }

        public static UserAccount CurrentUser(this Controller controller, IDocumentSession session)
        {
            return CurrentUser(controller.User, session);
        }

        private static UserAccount CurrentUser(IPrincipal userPrincipal, IDocumentSession session)
        {
            return session.Query<UserAccount>().Where(a => a.AuthencationIds.Any(id => id == userPrincipal.Identity.Name)).Single();
        }
    }
}