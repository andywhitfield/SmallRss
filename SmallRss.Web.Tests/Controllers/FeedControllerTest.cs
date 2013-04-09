using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmallRss.Web.Controllers;
using System.Diagnostics;

namespace SmallRss.Web.Tests.Controllers
{
    [TestClass]
    public class FeedControllerTest
    {
        [TestMethod]
        public void Dummy()
        {
            Trace.WriteLine("HERE: 1");
            var controller = new FeedController(null);
            foreach (var i in controller.Get())
                Trace.WriteLine(i);
            Trace.WriteLine("HERE: 2");
        }
    }
}
