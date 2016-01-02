using log4net;
using SmallRss.Data.Models;
using SmallRss.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Http;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class PocketController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PocketController));

        private readonly IDatastore datastore;

        public PocketController(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        // POST api/pocket
        public object Post(PocketViewModel model)
        {
            var userAccount = this.CurrentUser(datastore);
            if (!userAccount.HasPocketAccessToken)
            {
                return new { saved = false, reason = "Your account is not connected to Pocket." };
            }

            var article = datastore.Load<Article>(model.ArticleId.GetValueOrDefault());
            if (article == null)
            {
                return new { saved = false, reason = "Could not find article with id " + model.ArticleId };
            }
            
            var requestJson = "{\"consumer_key\":\"" + ManageController.PocketConsumerKey +
                "\", \"access_token\":\"" + userAccount.PocketAccessToken +
                "\", \"url\":\"" + HttpUtility.UrlPathEncode(article.Url) +
                "\", \"title\":\"" + HttpUtility.UrlEncode(article.Heading) +
                "\"}";

            var webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=UTF-8");
            webClient.Headers.Add("X-Accept", "application/json");
            var result = webClient.UploadString("https://getpocket.com/v3/add", requestJson);

            var jsonDeserializer = new DataContractJsonSerializer(typeof(AddResponse));
            var requestToken = jsonDeserializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(result))) as AddResponse;
            // TODO: handle response and return appropriate json response to client
            return new { saved = true };
        }

        [DataContract]
        private class AddResponse
        {
            [DataMember]
            public string status;
        }

    }
}
