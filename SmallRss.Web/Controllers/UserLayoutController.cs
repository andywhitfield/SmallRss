using SmallRss.Web.Models;
using System.Web.Http;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class UserLayoutController : ApiController
    {
        private readonly IDatastore datastore;

        public UserLayoutController(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        // POST api/userlayout
        public void Post(UserLayoutViewModel layout)
        {
            var user = this.CurrentUser(datastore);

            user.SavedLayout.Remove(UserLayoutViewModel.LayoutKeySplitNorth);
            if (layout.SplitNorth.HasValue)
                user.SavedLayout.Add(UserLayoutViewModel.LayoutKeySplitNorth, layout.SplitNorth.Value.ToString());

            user.SavedLayout.Remove(UserLayoutViewModel.LayoutKeySplitWest);
            if (layout.SplitWest.HasValue)
                user.SavedLayout.Add(UserLayoutViewModel.LayoutKeySplitWest, layout.SplitWest.Value.ToString());

            datastore.UpdateAccount(user);
        }
    }
}
