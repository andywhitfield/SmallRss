using log4net;
using PetaPoco.Internal;
using SmallRss.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallRss.Data
{
    public class PetaPocoDatastore : IDatastore
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PetaPocoDatastore));

        private readonly PetaPoco.Database db;

        public PetaPocoDatastore(PetaPoco.Database db)
        {
            this.db = db;
        }

        public void UpdateAccount(UserAccount userAccount)
        {
            log.InfoFormat("Updating account: {0}", userAccount.Id);

            var userAccountSettings =
                userAccount/*.AuthenticationIds.Select(a => new UserAccountSetting
                {
                    UserAccountId = userAccount.Id,
                    SettingType = "AuthenticationId",
                    SettingName = "AuthenticationId",
                    SettingValue = a
                })
                .Concat(userAccount*/.ExpandedGroups.Select(g => new UserAccountSetting
                {
                    UserAccountId = userAccount.Id,
                    SettingType = "ExpandedGroup",
                    SettingName = "ExpandedGroup",
                    SettingValue = g
                })
                .Concat(new[] { new UserAccountSetting
                {
                    UserAccountId = userAccount.Id,
                    SettingType = "ShowAllItems",
                    SettingName = "ShowAllItems",
                    SettingValue = Convert.ToString(userAccount.ShowAllItems)
                }})
                .Concat(userAccount.SavedLayout.Select(l => new UserAccountSetting {
                    UserAccountId = userAccount.Id,
                    SettingType = "SavedLayout",
                    SettingName = l.Key,
                    SettingValue = l.Value
                }));

            using (var txn = db.GetTransaction())
            {
                db.Update(userAccount);
                db.Delete<UserAccountSetting>("where useraccountid = @0 and settingtype <> 'AuthenticationId'", userAccount.Id);
                foreach (var setting in userAccountSettings)
                    db.Insert(setting);

                txn.Complete();
            }
        }

        public UserAccount GetOrCreateAccount(string authenticationId)
        {
            var accountAuth = db.Query<UserAccountSetting>("where settingtype = @0 and settingvalue = @1", "AuthenticationId", authenticationId).FirstOrDefault();

            if (accountAuth != null)
            {
                log.DebugFormat("Found existing account with auth id {0}: {1}", authenticationId, accountAuth.UserAccountId);
                var account = LoadUserAccount(accountAuth.UserAccountId);
                account.LastLogin = DateTime.UtcNow;
                db.Save(account);
                return account;
            }
            else
            {
                log.InfoFormat("No account found with auth id {0} - creating a new account.", authenticationId);
                var account = new UserAccount { LastLogin = DateTime.UtcNow };
                account.AuthenticationIds.Add(authenticationId);
                using (var txn = db.GetTransaction())
                {
                    db.Insert(account);
                    db.Insert(new UserAccountSetting { UserAccountId = account.Id, SettingType = "AuthenticationId", SettingName = "AuthenticationId", SettingValue = authenticationId });
                    db.Insert(new UserAccountSetting { UserAccountId = account.Id, SettingType = "ShowAllItems", SettingName = "ShowAllItems", SettingValue = Convert.ToString(account.ShowAllItems) });
                    txn.Complete();
                }
                return account;
            }
        }

        private UserAccount LoadUserAccount(int id)
        {
            var accountAndSettings = db.Query<UserAccount, UserAccountSetting, Tuple<UserAccount, UserAccountSetting>>(
                (ua, uas) => Tuple.Create(ua, uas),
                @"select *
                  from useraccount ua
                  join useraccountsetting uas
                  on ua.id = uas.useraccountid
                  where ua.id = @0", id);

            var account = accountAndSettings.First().Item1;
            foreach (var authId in accountAndSettings.Where(uas => uas.Item2.SettingType == "AuthenticationId"))
                account.AuthenticationIds.Add(authId.Item2.SettingValue);
            foreach (var expandedGroup in accountAndSettings.Where(uas => uas.Item2.SettingType == "ExpandedGroup"))
                account.ExpandedGroups.Add(expandedGroup.Item2.SettingValue);
            foreach (var savedLayout in accountAndSettings.Where(uas => uas.Item2.SettingType == "SavedLayout"))
                account.SavedLayout.Add(savedLayout.Item2.SettingName, savedLayout.Item2.SettingValue);
            var showAllItemsSetting = accountAndSettings.FirstOrDefault(uas => uas.Item2.SettingType == "ShowAllItems");
            account.ShowAllItems = showAllItemsSetting == null ? false : Convert.ToBoolean(showAllItemsSetting.Item2.SettingValue);
            return account;
        }

        public IEnumerable<T> LoadAll<T>(string foreignKeyColumn, object foreignKeyValue)
        {
            return db.Query<T>("where " + foreignKeyColumn + " = @0", foreignKeyValue);
        }

        public IEnumerable<T> LoadAll<T>(params Tuple<string, object, ClauseComparsion>[] loadClauses)
        {
            var sql = PetaPoco.Sql.Builder;
            foreach (var colVal in loadClauses)
                sql.Where(colVal.Item1 + " " + Operator(colVal.Item3) + " @0", colVal.Item2);

            return db.Query<T>(sql);
        }

        public T Load<T>(object primaryKey)
        {
            return db.SingleOrDefault<T>(primaryKey);
        }

        public Tuple<UserFeed, RssFeed> LoadUserRssFeed(int? userFeedId, int? rssFeedId)
        {
            if (userFeedId.HasValue)
            {
                return db.Query<UserFeed, RssFeed, Tuple<UserFeed, RssFeed>>(
                    (uf, rf) => Tuple.Create(uf, rf),
                    @"select *
                      from userfeed uf
                      join rssfeed rf
                      on uf.rssfeedid = rf.id
                      where uf.id = @0", userFeedId.Value).Single();
            }

            if (rssFeedId.HasValue)
            {
                return db.Query<UserFeed, RssFeed, Tuple<UserFeed, RssFeed>>(
                    (uf, rf) => Tuple.Create(uf, rf),
                    @"select *
                      from userfeed uf
                      join rssfeed rf
                      on uf.rssfeedid = rf.id
                      where rf.id = @0", rssFeedId.Value).Single();
            }

            throw new ArgumentException("No userFeedId or rssFeedId provided.");
        }

        public IEnumerable<Tuple<UserFeed, RssFeed>> LoadUserRssFeeds(int userAccountId)
        {
            return db.Query<UserFeed, RssFeed, Tuple<UserFeed, RssFeed>>(
                (uf, rf) => Tuple.Create(uf, rf),
                @"select *
                  from userfeed uf
                  join rssfeed rf
                  on uf.rssfeedid = rf.id
                  where uf.useraccountid = @0", userAccountId);
        }

        public IEnumerable<UnreadArticleCountPerFeed> GetUnreadFeeds(int userAccountId)
        {
            return db.Query<UnreadArticleCountPerFeed>(
@"select uf.Id as FeedId, uf.GroupName as GroupName, COUNT(a.Id) as UnreadCount
from UserAccount ua
join UserFeed uf on ua.Id = uf.UserAccountId
join RssFeed rf on rf.Id = uf.RssFeedId
left join Article a on rf.Id = a.RssFeedId
left join UserArticlesRead uar on uar.ArticleId = a.Id and uar.UserFeedId = uf.Id and uar.UserAccountId = ua.Id
where uar.Id is null
and ua.Id = @0
group by uf.Id, uf.GroupName", userAccountId);
        }

        public IEnumerable<Article> LoadUnreadArticlesInUserFeed(UserFeed feed)
        {
            return db.Query<Article>(
@"select a.*
from Article a
join RssFeed rf
on rf.Id = a.RssFeedId
left join UserArticlesRead uar
on uar.ArticleId = a.Id
and uar.UserAccountId = @1
where rf.Id = @0
and uar.Id is null", feed.RssFeedId, feed.UserAccountId);
        }

        public int RemoveUserArticleRead(UserArticlesRead userArticleRead)
        {
            return db.Delete<UserArticlesRead>(
                "where UserAccountId = @0 and UserFeedId = @1 and ArticleId = @2",
                userArticleRead.UserAccountId,
                userArticleRead.UserFeedId,
                userArticleRead.ArticleId);
        }

        public int RemoveUserArticleRead(UserAccount user, UserFeed feed)
        {
            return db.Delete<UserArticlesRead>(
                "where UserAccountId = @0 and UserFeedId = @1",
                user.Id,
                feed.Id);
        }

        public T Store<T>(T entity)
        {
            db.Insert(entity);
            return entity;
        }

        public T Update<T>(T entity)
        {
            db.Update(entity);
            return entity;
        }

        public int Remove<T>(T entity)
        {
            return db.Delete(entity);
        }

        public int RemoveAll<T>(params Tuple<string, object, ClauseComparsion>[] removeClauses)
        {
            var sql = PetaPoco.Sql.Builder;
            foreach (var colVal in removeClauses)
                sql.Where(colVal.Item1 + " " + Operator(colVal.Item3) + " @0", colVal.Item2);

            return db.Delete<T>(sql);
        }

        private string Operator(ClauseComparsion op)
        {
            switch (op)
            {
                case ClauseComparsion.Equals:
                    return "=";
                case ClauseComparsion.GreaterThan:
                    return ">";
                case ClauseComparsion.GreaterThanOrEqual:
                    return ">=";
                case ClauseComparsion.LessThan:
                    return "<";
                case ClauseComparsion.LessThanOrEqual:
                    return "<=";
                default:
                    throw new ArgumentException("Unsupported comparison operator: " + op);
            }
        }
    }
}
