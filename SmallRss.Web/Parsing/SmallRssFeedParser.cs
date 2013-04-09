﻿using QDFeedParser;
using QDFeedParser.Xml;
using System;
using System.Linq;
using System.Xml.Linq;

namespace SmallRss.Web.Parsing
{
    public class SmallRssFeedParser : IFeedXmlParser
    {
        public void ParseFeed(IFeed feed, string xml)
        {
            switch (feed.FeedType)
            {
                case FeedType.Rss20:
                    var rssFeed = feed as Rss20Feed;
                    ParseRss20Header(rssFeed, xml);
                    ParseRss20Items(rssFeed, xml);
                    break;
                case FeedType.Atom10:
                    var atomFeed = feed as Atom10Feed;
                    ParseAtom10Header(atomFeed, xml);
                    ParseAtom10Items(atomFeed, xml);
                    break;
            }
        }

        public FeedType CheckFeedType(string feedxml)
        {
            var doc = XDocument.Parse(feedxml);
            var xmlRootElement = doc.Root;
            if (xmlRootElement.Name.LocalName.Contains(FeedXmlParserBase.RssRootElementName) && xmlRootElement.Attribute(FeedXmlParserBase.RssVersionAttributeName).Value == "2.0")
            {
                return FeedType.Rss20;
            }
            else if (xmlRootElement.Name.LocalName.Contains(FeedXmlParserBase.AtomRootElementName))
                return FeedType.Atom10;
            else
                throw new InvalidFeedXmlException("Unable to determine feedtype (but was able to parse file) for feed");
        }

        readonly XNamespace ns = "http://www.w3.org/2005/Atom";

        private void ParseAtom10Header(Atom10Feed atomFeed, string xml)
        {
            var document = XDocument.Parse(xml);
            var channel = document.Root;

            var titleNode = channel.Element(ns+ "title");
            atomFeed.Title = titleNode.Value;

            var linkNode = channel.Element(ns + "author") != null ? channel.Element(ns + "author").Element(ns + "uri") : null;

            if(linkNode == null)
            {
                linkNode = channel.Elements(ns + "link").SingleOrDefault(x => x.HasAttributes && x.Attribute("rel") == null) ??
                           channel.Elements(ns + "link").SingleOrDefault(x => x.HasAttributes && x.Attribute("rel") != null && x.Attribute("rel").Value == "alternate");

                atomFeed.Link = linkNode == null ? string.Empty : linkNode.Attribute("href").Value;
            }
            else
            {
                atomFeed.Link = linkNode == null ? string.Empty : linkNode.Value;
            }

            var dateTimeNode = channel.Element(ns + "updated");

            DateTime timeOut;
            DateTime.TryParse(dateTimeNode.Value, out timeOut);
            atomFeed.LastUpdated = timeOut.ToUniversalTime();

            var generatorNode = channel.Element(ns + "generator");
            atomFeed.Generator = generatorNode == null ? string.Empty : generatorNode.Value;

        }

        private void ParseAtom10Items(Atom10Feed atomFeed, string xml)
        {
            var document = XDocument.Parse(xml);
            var feedItemNodes = document.Root.Elements(ns+"entry");
            foreach (var item in feedItemNodes)
            {
                atomFeed.Items.Add(ParseAtom10SingleItem(item));
            }
        }

        private BaseFeedItem ParseAtom10SingleItem(XElement itemNode)
        {
            var titleNode = itemNode.Element(ns + "title");
            var datePublishedNode = itemNode.Element(ns + "updated");
            var authorNode = itemNode.Element(ns + "author") == null ? null : itemNode.Element(ns + "author").Element(ns + "name");
            var idNode = itemNode.Element(ns + "id");
            var contentNode = itemNode.Element(ns + "content");
            var summaryNode = itemNode.Element(ns + "summary");
            var linkNode = itemNode.Element(ns + "link") == null ? null : itemNode.Element(ns + "link").Attribute("href");

            DateTime datePublished;
            BaseFeedItem item = new Atom10FeedItem
            {
                Title = titleNode == null ? string.Empty : titleNode.Value,
                DatePublished = datePublishedNode == null ? DateTime.UtcNow : (DateParser.TryParseRfc3339DateTime(datePublishedNode.Value, out datePublished) ? datePublished : DateTime.UtcNow),
                Author = authorNode == null ? string.Empty : authorNode.Value,
                Id = idNode == null ? string.Empty : idNode.Value,
                Content = contentNode == null ? (summaryNode == null ? string.Empty : summaryNode.Value) : contentNode.Value,
                Link = linkNode == null ? string.Empty : linkNode.Value
            };

            var categoryNode = itemNode.Element(ns + "category");
                                
            if (categoryNode != null)
            {
                var categoryNodes = categoryNode.Elements(ns + "term");
                foreach (var termNode in categoryNodes)
                {
                    item.Categories.Add(termNode.Value);
                }
            }

            return item;
        }

        private void ParseRss20Header(Rss20Feed rssFeed, string xml)
        {
            var document = XDocument.Parse(xml);
            var channel = document.Root.Element("channel");

            rssFeed.Title = channel.Element("title").Value;
            rssFeed.Description = channel.Element("description").Value;

            var linkNode = channel.Element("link");
            rssFeed.Link = linkNode == null ? string.Empty : linkNode.Value;

            var dateTimeNode = (from dateSelector in channel.Elements("pubDate")
                                select dateSelector).FirstOrDefault();
            if (dateTimeNode == null)
            {
                rssFeed.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                DateTime timeOut;
                DateTime.TryParse(dateTimeNode.Value, out timeOut);
                rssFeed.LastUpdated = timeOut.ToUniversalTime();
            }

            var generatorNode = channel.Element("generator");
            rssFeed.Generator = generatorNode == null ? string.Empty : generatorNode.Value;

            var languageNode = channel.Element("language");
            rssFeed.Language = languageNode == null ? string.Empty : languageNode.Value;
        }

        private void ParseRss20Items(Rss20Feed rssFeed, string xml)
        {
            var document = XDocument.Parse(xml);
            var feedItemNodes = document.Root.Element("channel").Elements("item");
            foreach (var item in feedItemNodes)
            {
                rssFeed.Items.Add(ParseRss20SingleItem(item));
            }
        }

        private BaseFeedItem ParseRss20SingleItem(XElement itemNode)
        {
            var titleNode = itemNode.Element("title");
            var datePublishedNode = itemNode.Element("pubDate");
            var authorNode = itemNode.Element("author");
            var commentsNode = itemNode.Element("comments");
            var idNode = itemNode.Element("guid");
            var contentNode = itemNode.Element("description");
            var linkNode = itemNode.Element("link");

            DateTime datePublished;
            BaseFeedItem item = new Rss20FeedItem
            {
                Title = titleNode == null ? string.Empty : titleNode.Value,
                DatePublished = datePublishedNode == null ? DateTime.UtcNow : (DateParser.TryParseRfc822DateTime(datePublishedNode.Value, out datePublished) ? datePublished : DateTime.UtcNow),
                Author = authorNode == null ? string.Empty : authorNode.Value,
                Comments = commentsNode == null ? string.Empty : commentsNode.Value,
                Id = idNode == null ? string.Empty : idNode.Value,
                Content = contentNode == null ? string.Empty : contentNode.Value,
                Link = linkNode == null ? string.Empty : linkNode.Value
            };

            var categoryNodes = itemNode.Elements("category");
            if (categoryNodes != null)
            {
                foreach (var categoryNode in categoryNodes)
                {
                    item.Categories.Add(categoryNode.Value);
                }
            }

            return item;
        }
    }
}