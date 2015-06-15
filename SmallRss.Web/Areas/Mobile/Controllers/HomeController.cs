using SmallRss.Web.Areas.Mobile.Models.Home;
using SmallRss.Web.Models;
using System;
using System.Web.Mvc;

namespace SmallRss.Web.Areas.Mobile.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDatastore datastore;

        public HomeController(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        [Authorize]
        public ActionResult Index()
        {
            Response.AppendHeader(
                "X-XRDS-Location",
                new Uri(Request.Url, Response.ApplyAppPathModifier("~/home/xrds")).AbsoluteUri);

            var loggedInUser = this.CurrentUser(datastore);
            return View(new IndexViewModel { ShowAllArticles = loggedInUser.ShowAllItems });
        }
    }
}
