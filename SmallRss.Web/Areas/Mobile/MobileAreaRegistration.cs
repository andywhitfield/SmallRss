using System.Web.Mvc;

namespace SmallRss.Web.Areas.Mobile
{
    public class MobileAreaRegistration : AreaRegistration
    {
        public override string AreaName { get { return "Mobile"; } }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Mobile_default",
                "m/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
