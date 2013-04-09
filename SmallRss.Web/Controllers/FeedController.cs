using HtmlAgilityPack;
using Raven.Client;
using Raven.Client.Linq;
using SmallRss.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class FeedController : ApiController
    {
        private readonly IDocumentSession documentSession;

        public FeedController(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        // GET api/feed
        public IEnumerable<object> Get()
        {
            Trace.WriteLine("Getting all feeds from db");
            
            var currentUser = this.CurrentUser(documentSession);

            return documentSession.Load<Feed>(currentUser.Feeds.Cast<ValueType>()).Take(FeedConstants.MaxFeeds).GroupBy(f => f.GroupName).OrderBy(g => g.Key).Select(group =>
                new { label = group.Key, expanded = currentUser.Expanded.Contains(group.Key), items = group.OrderBy(g => g.Name).Select(g =>
                    new { label = g.Name, value = g.Id }) });
        }

        // GET api/feed/5
        public IEnumerable<object> Get(int id)
        {
            Trace.TraceInformation("Getting articles for feed {0} from db", id);

            var currentUser = this.CurrentUser(documentSession);
            var showAllItems = currentUser.ShowAllItems;
            var feed = documentSession.Load<Feed>(id);
            var readArticles = feed.ArticlesRead.Take(FeedConstants.MaxArticles).ToList();

            return documentSession
                .Query<Article>()
                .Where(a => a.RssId == feed.RssId)
                .Take(FeedConstants.MaxArticles)
                .OrderBy(a => a.Date)
                .ToList()
                .Where(a => showAllItems || !readArticles.Contains(a.Id))
                .Select(a => new { read = readArticles.Contains(a.Id), feed = id, story = a.Id, heading = a.Heading, article = Preview(a.ArticleBody), posted = Date(a.Date) });
        }

        private string Date(DateTime articleDate)
        {
            var format = "dd-MMM-yyyy HH:mm";
            var age = DateTime.UtcNow - articleDate;

            if (age < TimeSpan.FromDays(1))
                format = "HH:mm";
            else if (age < TimeSpan.FromDays(7))
                format = "ddd HH:mm";
            else if (age < TimeSpan.FromDays(100))
                format = "dd-MMM HH:mm";

            return articleDate.ToString(format);
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
                        writer.Write(HtmlEntity.DeEntitize(html));

                    break;
                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            writer.Write("\r\n");
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
