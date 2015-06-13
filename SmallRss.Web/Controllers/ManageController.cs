using log4net;
using SmallRss.Data.Models;
using SmallRss.Web.Models;
using SmallRss.Web.Models.Manage;
using SmallRss.Web.ServiceApi;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Mvc;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private const string consumerKey = "41619-1a5decf504173a588fd1b492";

        private static readonly ILog log = LogManager.GetLogger(typeof(ManageController));

        private readonly IDatastore datastore;
        private readonly SmallRssApi serviceApi;

        public ManageController(IDatastore datastore, SmallRssApi serviceApi)
        {
            this.datastore = datastore;
            this.serviceApi = serviceApi;
        }

        public ActionResult Index()
        {
            return View(CreateIndexViewModel(this.CurrentUser(datastore)));
        }

        public ActionResult Edit(int id)
        {
            var vm = CreateEditViewModel(this.CurrentUser(datastore), id);
            if (vm == null)
                return RedirectToAction("index");

            return View(vm);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var user = this.CurrentUser(datastore);
            var feeds = datastore.LoadAll<UserFeed>("UserAccountId", user.Id);

            var feed = feeds.FirstOrDefault(f => f.Id == id);
            if (feed == null)
                return RedirectToAction("index");

            var removeCount = datastore.RemoveUserArticleRead(user, feed);
            log.InfoFormat("Removed {0} user article read records: {1}:{2}", removeCount, feed.Id, feed.Name);

            removeCount = datastore.Remove(feed);
            log.InfoFormat("Removed {0} feed: {1}:{2}", removeCount, feed.Id, feed.Name);

            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult Add(AddFeedViewModel addFeed)
        {
            var user = this.CurrentUser(datastore);

            if (!ModelState.IsValid || (string.IsNullOrWhiteSpace(addFeed.GroupSel) && string.IsNullOrWhiteSpace(addFeed.Group)))
            {
                var vm = CreateIndexViewModel(user);
                vm.Error = "Missing feed URL, group or name. Please complete all fields and try again.";
                return View("Index", vm);
            }

            var rss = datastore.LoadAll<RssFeed>("Uri", addFeed.Url).FirstOrDefault();
            if (rss == null)
            {
                rss = datastore.Store(new RssFeed { Uri = addFeed.Url });
                log.InfoFormat("Created new RSS feed: {0}", addFeed.Url);
                serviceApi.RefreshFeed(user.Id, rss.Id);
            }

            var newFeed = new UserFeed {
                GroupName = string.IsNullOrWhiteSpace(addFeed.GroupSel) ? addFeed.Group : addFeed.GroupSel,
                Name = addFeed.Name,
                RssFeedId = rss.Id,
                UserAccountId = user.Id
            };
            datastore.Store(newFeed);

            log.InfoFormat("Created new user feed: {0} - {1}", addFeed.Name, addFeed.Url);

            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult Save(SaveFeedViewModel saveFeed)
        {
            var user = this.CurrentUser(datastore);
            if (!ModelState.IsValid || (string.IsNullOrWhiteSpace(saveFeed.GroupSel) && string.IsNullOrWhiteSpace(saveFeed.Group)))
            {
                var vm = CreateEditViewModel(user, saveFeed.Id);
                if (vm == null)
                    return RedirectToAction("index");

                vm.Error = "Could not update feed due to a missing feed URL, group or name. Please complete all fields and try again.";
                return View("Edit", vm);
            }

            var rss = datastore.LoadAll<RssFeed>("Uri", saveFeed.Url).FirstOrDefault();
            if (rss == null)
            {
                rss = datastore.Store(new RssFeed { Uri = saveFeed.Url });
                log.InfoFormat("Updating user feed, created new rss: {0}", saveFeed.Url);
            }

            var feed = datastore.Load<UserFeed>(saveFeed.Id);
            if (feed == null || feed.UserAccountId != user.Id)
            {
                var vm = CreateEditViewModel(user, saveFeed.Id);
                if (vm == null)
                    return RedirectToAction("index");

                vm.Error = "Could not update feed due to a security check failure. Please try again";
                return View("Edit", vm);
            }

            feed.GroupName = string.IsNullOrWhiteSpace(saveFeed.GroupSel) ? saveFeed.Group : saveFeed.GroupSel;
            feed.Name = saveFeed.Name;
            feed.RssFeedId = rss.Id;
            datastore.Update(feed);

            log.InfoFormat("Updating user feed: {0}", saveFeed.Name);

            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult Pocket()
        {
            var userAccount = this.CurrentUser(datastore);
            if (userAccount.HasPocketAccessToken)
            {
                // disconnect from pocket requested
                userAccount.PocketAccessToken = string.Empty;
                datastore.UpdateAccount(userAccount);
                return RedirectToAction("Index");
            }

            var redirectUri = Url.Action("PocketAuth", "Manage", routeValues: null, protocol: Request.Url.Scheme);
            var requestJson = "{\"consumer_key\":\"" + consumerKey + "\", \"redirect_uri\":\"" + redirectUri + "\"}";

            var webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=UTF-8");
            webClient.Headers.Add("X-Accept", "application/json");
            var result = webClient.UploadString("https://getpocket.com/v3/oauth/request", requestJson);

            var jsonDeserializer = new DataContractJsonSerializer(typeof(RequestToken));
            var requestToken = jsonDeserializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(result))) as RequestToken;
            if (requestToken == null)
            {
                throw new InvalidOperationException("Cannot deserialize response: " + result);
            }
            // parse result: {"code":"f5efd910-9415-7fb1-a1f7-981402","state":null}
            Session["POCKET_CODE"] = requestToken.code;
            var redirectToPocket = string.Format("https://getpocket.com/auth/authorize?request_token={0}&redirect_uri={1}", Url.Encode(requestToken.code), Url.Encode(redirectUri));

            return Redirect(redirectToPocket);
        }

        public ActionResult PocketAuth()
        {
            var code = Session["POCKET_CODE"];
            var requestJson = "{\"consumer_key\":\"" + consumerKey + "\", \"code\":\"" + code + "\"}";

            var webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=UTF-8");
            webClient.Headers.Add("X-Accept", "application/json");
            var result = webClient.UploadString("https://getpocket.com/v3/oauth/authorize", requestJson);

            var jsonDeserializer = new DataContractJsonSerializer(typeof(PocketAuthResult));
            var authResult = jsonDeserializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(result))) as PocketAuthResult;
            if (authResult == null)
            {
                throw new InvalidOperationException("Cannot deserialize response: " + result);
            }

            // save access token into the user's account
            var userAccount = this.CurrentUser(datastore);
            userAccount.PocketAccessToken = authResult.access_token;
            datastore.UpdateAccount(userAccount);

            return RedirectToAction("Index");
        }

        private IndexViewModel CreateIndexViewModel(UserAccount user)
        {
            var feeds = datastore.LoadUserRssFeeds(user.Id);
            return new IndexViewModel { UserAccount = user, Feeds = feeds.Select(f => new FeedSubscriptionViewModel(f.Item1, f.Item2)).OrderBy(f => f.Name).OrderBy(f => f.Group) };
        }

        private EditViewModel CreateEditViewModel(UserAccount user, int feedId)
        {
            var feeds = datastore.LoadAll<UserFeed>("UserAccountId", user.Id);
            var feed = feeds.FirstOrDefault(f => f.Id == feedId);
            if (feed == null)
                return null;

            var rss = datastore.Load<RssFeed>(feed.RssFeedId);
            return new EditViewModel { Feed = new FeedSubscriptionViewModel(feed, rss), CurrentGroups = feeds.Select(f => f.GroupName).Distinct().OrderBy(g => g) };
        }

        [DataContract]
        private class RequestToken
        {
            [DataMember]
            public string code;
        }

        [DataContract]
        private class PocketAuthResult
        {
            [DataMember]
            public string access_token;
            [DataMember]
            public string username;
        }

    }
}
