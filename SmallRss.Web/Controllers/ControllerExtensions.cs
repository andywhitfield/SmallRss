using log4net;
using SmallRss.Data.Models;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Mvc;

namespace SmallRss.Web.Controllers
{
    public static class ControllerExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ControllerExtensions));

        public static UserAccount CurrentUser(this ApiController controller, IDatastore datastore)
        {
            return CurrentUser(controller.User, datastore);
        }

        public static UserAccount CurrentUser(this Controller controller, IDatastore datastore)
        {
            return CurrentUser(controller.User, datastore);
        }

        private static UserAccount CurrentUser(IPrincipal userPrincipal, IDatastore datastore)
        {
            log.DebugFormat("Getting user from database with id {0}", userPrincipal.Identity.Name);
            return datastore.GetOrCreateAccount(userPrincipal.Identity.Name);
        }
    }
}