using HtmlAgilityPack;
using log4net;
using SmallRss.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class FeedController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FeedController));

        private readonly IDatastore datastore;

        public FeedController(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        // GET api/feed
        public IEnumerable<object> Get()
        {
            log.Debug("Getting all feeds from db");
            
            var loggedInUser = this.CurrentUser(datastore);
            var userFeeds = datastore.Load<UserFeed>("UserAccountId", loggedInUser.Id);

            return userFeeds.GroupBy(f => f.GroupName).OrderBy(g => g.Key).Select(group =>
                new
                {
                    id = group.Key,
                    item = group.Key,
                    props = new { isFolder = true, open = loggedInUser.ExpandedGroups.Contains(group.Key) },
                    items = group.OrderBy(g => g.Name).Select(g =>
                        new { id = g.Id, item = g.Name, props = new { isFolder = false } })
                });
        }

        // GET api/feed/5
        public IEnumerable<object> Get(int id, int? offset)
        {
            log.DebugFormat("Getting articles for feed {0} from db, using client UTC offset {1}", id, offset);

            var loggedInUser = this.CurrentUser(datastore);
            var feed = datastore.Load<UserFeed>(id);
            var readArticles = datastore.Load<UserArticlesRead>("UserFeedId", feed.Id).ToList();

            IEnumerable<Article> articles;
            if (loggedInUser.ShowAllItems)
            {
                articles = datastore.Load<Article>("RssFeedId", feed.RssFeedId);
            }
            else
            {
                articles = datastore.LoadUnreadArticlesInUserFeed(feed);
            }

            return articles
                .OrderBy(a => a.Published)
                .Select(a => new { read = readArticles.Any(uar => uar.ArticleId == a.Id), feed = id, story = a.Id, heading = a.Heading, article = Preview(a.Body), posted = Date(a.Published, offset) });
        }

        private string Date(DateTime? articleDate, int? utcOffset)
        {
            var format = "dd-MMM-yyyy HH:mm";

            if (!articleDate.HasValue)
                articleDate = DateTime.UtcNow;

            var age = DateTime.UtcNow - articleDate;

            if (age < TimeSpan.FromDays(1) && DateTime.UtcNow.Day == articleDate.Value.Day)
                format = "HH:mm";
            else if (age < TimeSpan.FromDays(5))
                format = "ddd HH:mm";
            else if (age < TimeSpan.FromDays(100))
                format = "dd-MMM HH:mm";

            if (utcOffset.HasValue)
                articleDate = articleDate.Value.AddMinutes(-utcOffset.Value);

            return articleDate.Value.ToString(format);
        }

        private string Preview(string articleBody)
        {
            var html = new HtmlDocument();
            html.LoadHtml(articleBody);

            string previewText;
            using (var writer = new StringWriter())
            {
                ConvertTo(html.DocumentNode, writer);
                writer.Flush();
                previewText = writer.ToString();
            }
            previewText = previewText.Replace('\n', ' ').Replace('\r', ' ').Trim();

            if (previewText.Length > 100)
                previewText = previewText.Substring(0, 99) + "...";
            return previewText;
        }

        private void ConvertTo(HtmlNode node, TextWriter writer)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    break;
                case HtmlNodeType.Document:
                    ConvertContentTo(node, writer);
                    break;
                case HtmlNodeType.Text:
                    var parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    html = ((HtmlTextNode)node).Text;
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    if (html.Trim().Length > 0)
                        try { writer.Write(HtmlEntity.DeEntitize(html)); }
                        catch (Exception) { }

                    break;
                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            writer.Write("\n");
                            break;
                    }

                    if (node.HasChildNodes)
                        ConvertContentTo(node, writer);

                    break;
            }
        }

        private void ConvertContentTo(HtmlNode node, TextWriter writer)
        {
            foreach (var subnode in node.ChildNodes)
                ConvertTo(subnode, writer);
        }
    }
}
