using System.Web;
using System.Web.Optimization;

namespace SmallRss.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // TODO: verify bundles are working when released
            // TODO: are we gzip'ing?
            bundles.Add(new ScriptBundle("~/bundles/smallrss").Include("~/Scripts/smallrss.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/bundles/sammy").Include("~/Scripts/sammy-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/knockoutjs").Include("~/Scripts/knockout-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqwidgets")
                .Include("~/Scripts/jqwidgets/jqxcore.js", "~/Scripts/jqwidgets/jqxsplitter.js",
                    "~/Scripts/jqwidgets/jqxbuttons.js", "~/Scripts/jqwidgets/jqxscrollbar.js",
                    "~/Scripts/jqwidgets/jqxpanel.js", "~/Scripts/jqwidgets/jqxtree.js",
                    "~/Scripts/jqwidgets/jqxswitchbutton.js", "~/Scripts/jqwidgets/jqxdata.js",
                    "~/Scripts/jqwidgets/jqxmenu.js", "~/Scripts/jqwidgets/jqxlistbox.js",
                    "~/Scripts/jqwidgets/jqxgrid.js", "~/Scripts/jqwidgets/jqxgrid.selection.js",
                    "~/Scripts/jqwidgets/jqxgrid.columnsresize.js", "~/Scripts/jqwidgets/jqxgrid.pager.js",
                    "~/Scripts/jqwidgets/jqxgrid.sort.js", "~/Scripts/jqwidgets/jqxgrid.filter.js",
                    "~/Scripts/jqwidgets/jqxgrid.grouping.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/main.css"));
            bundles.Add(new StyleBundle("~/Content/jqwidgets").Include("~/Scripts/jqwidgets/styles/jqx.base.css"));
        }
    }
}