using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using Raven.Client;
using Raven.Client.Linq;
using SmallRss.Web.Models;
using SmallRss.Web.Models.Home;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace SmallRss.Web.Controllers
{
    public class HomeController : Controller
    {
        private static OpenIdRelyingParty openid = new OpenIdRelyingParty();
        private readonly IDocumentSession documentSession;

        public HomeController(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        [Authorize]
        public ActionResult Index()
        {
            Response.AppendHeader(
                "X-XRDS-Location",
                new Uri(Request.Url, Response.ApplyAppPathModifier("~/home/xrds")).AbsoluteUri);

            return View(new IndexViewModel { ShowAllItems = this.CurrentUser(documentSession).ShowAllItems });
        }

        public ActionResult Xrds() { return View(); }

        [ValidateInput(false)]
        public ActionResult Login(string returnUrl)
        {
            Response.AppendHeader(
                "X-XRDS-Location",
                new Uri(Request.Url, Response.ApplyAppPathModifier("~/home/xrds")).AbsoluteUri);

            Trace.TraceInformation("Attempting to authenticate");

            var response = openid.GetResponse();
            if (response == null)
            {
                var openIdIdentifier = Request.Form["openid"];
                Trace.TraceInformation("Logging in user [{0}] via open ip provider: ", openIdIdentifier);

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
                Trace.TraceInformation("Got response from OpenID provider - status: {0}", response.Status);

                // Stage 3: OpenID Provider sending assertion response
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        string openIdIdentifier = response.ClaimedIdentifier;
                        Trace.TraceInformation("Authenticated: {0}", openIdIdentifier);

                        var existingAccount = documentSession.Query<UserAccount>().Where(a => a.AuthencationIds.Any(id => id == openIdIdentifier));
                        if (!existingAccount.Any())
                        {
                            Trace.TraceInformation("Creating new user account: {0}", openIdIdentifier);
                            var newUserAccount = new UserAccount();
                            newUserAccount.AuthencationIds.Add(openIdIdentifier);
                            documentSession.Store(newUserAccount);
                        }

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
    }
}
