using System.Web;
using System.Web.Optimization;

namespace SmallRss.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/resource/js/alljs").Include(
                "~/resource/js/jquery-{version}.js",
                "~/resource/js/knockout-{version}.js",
                "~/resource/js/modernizer-{version}.js",
                "~/resource/js/sammy-{version}.js",
                "~/resource/js/jquery.splitter-{version}.js",
                "~/resource/js/jquery.aciPlugin.js",
                "~/resource/js/jquery.aciTree.js",
                "~/resource/js/jquery.jqGrid.js",
                "~/resource/js/grid.locale-en.js",
                "~/resource/js/smallrss.js"
            ));
            bundles.Add(new StyleBundle("~/resource/css/allcss").Include(
                "~/resource/css/jquery-ui-1.10.2.custom.css",
                "~/resource/css/jquery.splitter.css",
                "~/resource/css/aciTree.css",
                "~/resource/css/ui.jqgrid.css",
                "~/resource/css/smallrss.css"
            ));
        }
    }
}