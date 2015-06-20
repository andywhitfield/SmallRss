using System.Web.Optimization;

namespace SmallRss.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/resource/js/alljs").Include(
                "~/resource/js/jquery-{version}.js",
                "~/resource/js/simpleStorage-{version}.js",
                "~/resource/js/smallrss.js"
            ));
            bundles.Add(new StyleBundle("~/resource/css/allcss").Include(
                "~/resource/css/smallrss.css"
            ));
        }
    }
}