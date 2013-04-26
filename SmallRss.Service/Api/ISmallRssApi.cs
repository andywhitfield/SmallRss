using System.ServiceModel;

namespace SmallRss.Service.Api
{
    [ServiceContract(Name="SmallRss.Api", Namespace="http://smallrss")]
    public interface ISmallRssApi
    {
        [OperationContract]
        void RefreshAllFeeds(int userAccountId);

        [OperationContract]
        void RefreshFeed(int userAccountId, int feedId);
    }
}
