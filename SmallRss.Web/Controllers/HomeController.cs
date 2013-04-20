using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using log4net;
using SmallRss.Web.Models;
using SmallRss.Web.Models.Home;
using System;
using System.Web.Mvc;
using System.Web.Security;

namespace SmallRss.Web.Controllers
{
    public class HomeController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HomeController));
        private static OpenIdRelyingParty openid = new OpenIdRelyingParty();

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

            var currentUser = this.CurrentUser(datastore);
            var vm = new IndexViewModel { ShowAllItems = currentUser.ShowAllItems };

            Func<string, int?> setSplitterPosition = layoutKey =>
            {
                string posString;
                if (currentUser.SavedLayout.TryGetValue(layoutKey, out posString))
                {
                    int posInt;
                    if (int.TryParse(posString, out posInt) && posInt > 0)
                        return posInt;
                }
                return null;
            };

            vm.SplitterWestPosition = setSplitterPosition(UserLayoutViewModel.LayoutKeySplitWest);
            vm.SplitterNorthPosition = setSplitterPosition(UserLayoutViewModel.LayoutKeySplitNorth);

            return View(vm);
        }

        public ActionResult Xrds() { return View(); }

        [ValidateInput(false)]
        public ActionResult Login(string returnUrl)
        {
            Response.AppendHeader(
                "X-XRDS-Location",
                new Uri(Request.Url, Response.ApplyAppPathModifier("~/home/xrds")).AbsoluteUri);

            log.Debug("Attempting to authenticate");

            var response = openid.GetResponse();
            if (response == null)
            {
                var openIdIdentifier = Request.Form["openid"];
                log.InfoFormat("Logging in user [{0}] via open ip provider: ", openIdIdentifier);

                // Stage 2: user submitting Identifier
                Identifier id;
                if (Identifier.TryParse(openIdIdentifier, out id))
                {
                    try
                    {
                        return openid.CreateRequest(openIdIdentifier).RedirectingResponse.AsActionResult();
                    }
                    catch (ProtocolException ex)
                    {
                        return View(new LoginViewModel { Message = ex.Message });
                    }
                }
                else if (!string.IsNullOrWhiteSpace(openIdIdentifier))
                {
                    return View(new LoginViewModel { Message = "Invalid identifier" });
                }
                else
                {
                    return View(new LoginViewModel());
                }
            }
            else
            {
                log.DebugFormat("Got response from OpenID provider - status: {0}", response.Status);

                // Stage 3: OpenID Provider sending assertion response
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        string openIdIdentifier = response.ClaimedIdentifier;
                        log.DebugFormat("Authenticated: {0}", openIdIdentifier);

                        var account = datastore.GetOrCreateAccount(openIdIdentifier);
                        log.InfoFormat("Created/loaded user account: {0} - {1}", openIdIdentifier, account.Id);

                        FormsAuthentication.SetAuthCookie(openIdIdentifier, true);
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("", "");
                        }
                    case AuthenticationStatus.Canceled:
                        return View(new LoginViewModel { Message = "Canceled at provider" });
                    case AuthenticationStatus.Failed:
                        return View(new LoginViewModel { Message = response.Exception.Message });
                }
            }
            return new EmptyResult();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("login");
        }
    }
}