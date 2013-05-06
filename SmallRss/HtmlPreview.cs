using HtmlAgilityPack;
using System;
using System.IO;

namespace SmallRss
{
    public static class HtmlPreview
    {
        public static string Preview(string articleBody)
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

        private static void ConvertTo(HtmlNode node, TextWriter writer)
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

        private static void ConvertContentTo(HtmlNode node, TextWriter writer)
        {
            foreach (var subnode in node.ChildNodes)
                ConvertTo(subnode, writer);
        }
    }
}
