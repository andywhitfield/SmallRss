﻿using SmallRss.Data.Models;
using System;
using System.Collections.Generic;

namespace SmallRss
{
    public interface IDatastore
    {
        UserAccount GetOrCreateAccount(string authenticationId);
        void UpdateAccount(UserAccount userAccount);

        /// <summary>
        /// Load all the UserFeed and RssFeed instances by for the given account.
        /// </summary>
        IEnumerable<Tuple<UserFeed, RssFeed>> LoadUserRssFeeds(int userAccountId);

        IEnumerable<Article> LoadUnreadArticlesInUserFeed(UserFeed feed);
        IEnumerable<UnreadArticleCountPerFeed> GetUnreadFeeds(int userAccountId);
        int RemoveUserArticleRead(UserArticlesRead userArticleRead);
        int RemoveUserArticleRead(UserAccount user, UserFeed feed);

        /// <summary>
        /// Remove (or archive) all the articles in the given feed, leaving only 'leave' number.
        /// </summary>
        int RemoveArticles(RssFeed feed, int leave);

        T Load<T>(object primaryKey);
        IEnumerable<T> LoadAll<T>(string foreignKeyColumn, object foreignKeyValue);
        IEnumerable<T> LoadAll<T>(params Tuple<string, object, ClauseComparsion>[] loadClauses);
        IEnumerable<T> LoadAll<T>(string sql, params object[] args);

        T Store<T>(T entity);
        T Update<T>(T entity);
        int Remove<T>(T entity);
        int RemoveAll<T>(params Tuple<string, object, ClauseComparsion>[] removeClauses);
    }
}
