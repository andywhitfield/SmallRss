var localSettings;
var feeds;

function loadLocalSettings() {
    localSettings = simpleStorage.get('smallrss');
    if (localSettings == undefined) {
        localSettings = { showAll: false };
    }
}
function saveLocalSettings() {
    simpleStorage.set('smallrss', localSettings);
}

function initialiseFeeds(data) {
    feeds = data;

    feeds.selectedFeed = null;
    feeds.selectedFeedGroup = null;
    feeds.selectedFeedArticles = null;
    feeds.selectedFeedArticle = null;
    buildTreeFromFeeds();
    refreshFeedCounts();
    $('body').keydown(handleKeyPress);
}
function buildTreeFromFeeds() {
    feeds.allGroupsSection = $('section[name="GroupsSection"]');
    feeds.selectedFeedSection = $('section[name="SelectedFeedSection"]');
    feeds.selectedArticleSection = $('section[name="SelectedArticleSection"]');
    for (var i = 0; i < feeds.length; i++) {
        var group = feeds[i];
        var newSection = '<section id="' + group.id + '" data-count="0" class="group-section"><div>';
        newSection += group.item;
        newSection += ' <span class="group-unread-count"></span></div>';
        newSection += buildItemsFromFeed(group);
        newSection += '</section>';
        feeds.allGroupsSection.append(newSection);
    }
    $('.group-section div', feeds.allGroupsSection).click(toggleShowAll);
    $('.feed-list li', feeds.allGroupsSection).click(onFeedClicked);
}
function buildItemsFromFeed(feed) {
    var itemsHtml = '<article><ul class="feed-list">';
    for (var i = 0; i < feed.items.length; i++) {
        var group = feed.items[i];
        itemsHtml += '<li id="' + group.id + '" data-count="0">';
        itemsHtml += group.item;
        itemsHtml += ' <span class="item-unread-count"></span>';
        itemsHtml += '</li>';
    }
    itemsHtml += '</ul></article>';
    return itemsHtml;
}
function refreshFeedCounts(onRefreshCompleteFunc) {
    console.log('refreshing feed count');
    $.getJSON(urls.feedstatus_api, function (data) {
        // reset all groups & item counts to zero
        for (var grpIdx = 0; grpIdx < feeds.length; grpIdx++) {
            var group = feeds[grpIdx];
            group.count = 0;
            for (var itmIdx = 0; itmIdx < group.items.length; itmIdx++) {
                var item = group.items[itmIdx];
                item.count = 0;
            }
        }
        // now update from the received status
        for (var grpIdx = 0; grpIdx < data.length; grpIdx++) {
            var group = data[grpIdx];
            var feedGroup = findGroup(group.label);
            if (feedGroup == null) return;
            feedGroup.count = group.unread;

            for (var itmIdx = 0; itmIdx < group.items.length; itmIdx++) {
                var item = group.items[itmIdx];
                var feedItem = findItemInGroup(feedGroup, item.value);
                if (feedItem == null) return;
                feedItem.count = item.unread;
            }
        }
        console.log('feed counts updated; updating UI');
        updateUI();

        if (onRefreshCompleteFunc != undefined)
            onRefreshCompleteFunc();
    });
}
function findGroup(groupId) {
    for (var i = 0; i < feeds.length; i++) {
        if (feeds[i].id == groupId) return feeds[i];
    }
    return null;
}
function findItemInGroup(group, feedId) {
    var groupItems = group.items;
    for (var i = 0; i < groupItems.length; i++) {
        if (groupItems[i].id == feedId) return groupItems[i];
    }
    return null;
}
function updateUI() {
    feeds.allGroupsSection.hide();
    feeds.selectedFeedSection.hide();
    feeds.selectedArticleSection.hide();

    if (feeds.selectedFeedArticle != null) {
        updateSelectedArticle();
        feeds.selectedArticleSection.show();
        return;
    }

    if (feeds.selectedFeed != null) {
        updateSelectedFeed();
        feeds.selectedFeedSection.show();
        return;
    }

    updateFeedGroups();
    feeds.allGroupsSection.show();
}
function updateFeedGroups() {
    for (var grpIdx = 0; grpIdx < feeds.length; grpIdx++) {
        var feedGroup = feeds[grpIdx];
        if (feedGroup.count == undefined) feedGroup.count = 0;

        var groupSection = $('section[id="' + feedGroup.id + '"]', feeds.allGroupsSection);
        groupSection.attr('data-count', feedGroup.count);
        var unreadCount = $('.group-unread-count', groupSection);
        if (feedGroup.count == 0) unreadCount.text('');
        else unreadCount.text('(' + feedGroup.count + ')');

        // and the items
        for (var itmIdx = 0; itmIdx < feedGroup.items.length; itmIdx++) {
            var feedItem = feedGroup.items[itmIdx];
            if (feedItem.count == undefined) feedItem.count = 0;

            var feedListItem = $('li[id="' + feedItem.id + '"]', groupSection);
            feedListItem.attr('data-count', feedItem.count);
            var unreadCount = $('.item-unread-count', feedListItem);
            if (feedItem.count == 0) unreadCount.text('');
            else unreadCount.text('(' + feedItem.count + ')');
        }
    }

    // show all groups and items
    $('section', feeds.allGroupsSection).children('article').show();
    $('li', feeds.allGroupsSection).show();

    // then hide those that have no unread articles, if we're not showing all
    // TODO: This has the unfortunate affect of causing them all to show then shrink
    //       again, which isn't great. But this does fix the issue where new unread
    //       articles arrive for previously hidden sections and the section remains
    //       collapsed.
    if (!localSettings.showAll) {
        $('section[data-count="0"]', feeds.allGroupsSection).children('article').hide();
        $('li[data-count="0"]', feeds.allGroupsSection).hide();
    }
}
function updateSelectedFeed() {
    feeds.selectedFeedSection.empty();
    feeds.selectedFeedSection.append(buildFeedArticles());
    $('.feed-title').click(backToAllFeeds);
    $('.article-title, .article-summary, .article-date').click(showArticle);
    $('tbody > tr > td.article-read button').click(toggleArticleRead);
    $('thead > tr > td.article-read button').click(markAllArticlesRead);
    $('.article-pocket button').click(saveArticleToPocket);
    $('button.show-all-articles').click(showAllArticles);
}
function buildFeedArticles() {
    var feedHtml = '<div class="feed-title">' + feeds.selectedFeedGroup.item + ' &gt; ' + feeds.selectedFeed.item + ' (' + feeds.selectedFeed.count + ')</div>';
    feedHtml += '<table class="article-list">';
    feedHtml += '<thead><tr><td class="article-title">Title</td><td class="article-summary">Summary</td><td class="article-date">Posted</td><td class="article-pocket">&nbsp;</td><td class="article-read"><button class="image"><img src="' + urls.imageroot + 'image/markread.png" alt="Mark all as read"></button></td></tr></thead>';
    feedHtml += '<tbody>';
    for (var i = 0; i < feeds.selectedFeedArticles.length; i++) {
        var article = feeds.selectedFeedArticles[i];
        feedHtml += '<tr data-article-id="' + article.story + '" class="article' + (article.read ? ' article-marked-read' : '') + (feeds.focusedArticle != null && feeds.focusedArticle.story == article.story ? ' focused' : '') + '">';
        feedHtml += '<td class="article-title">' + article.heading + '</td>';
        feedHtml += '<td class="article-summary">' + article.article + '</td>';
        feedHtml += '<td class="article-date">' + article.posted + '</td>';
        feedHtml += '<td class="article-pocket"><button class="image"><img src="' + urls.imageroot + 'image/pocket.png" alt="Save to Pocket"></button></td>';
        feedHtml += '<td class="article-read"><button class="image">' + (article.read ? '<img src="' + urls.imageroot + 'image/markunread.png" alt="Mark as unread">' : '<img src="' + urls.imageroot + 'image/markread.png" alt="Mark as read">') + '</button></td>';
        feedHtml += '</tr>';
    }
    feedHtml += '</tbody></table>';
    feedHtml += '<div><button class="show-all-articles">' + (showingAllArticles ? 'Show unread articles' : 'Show all articles') + '</button></div>';
    return feedHtml;
}
function showAllArticles() {
    console.log('toggle showing all articles');
    showingAllArticles = !showingAllArticles;
    $(this).text(showingAllArticles ? 'Show unread articles' : 'Show all articles');
    $.post(urls.feedstatus_api, { showall: showingAllArticles }, function () {
        if (feeds.selectedFeed == null || feeds.selectedFeedGroup == null) return;
        loadArticlesForFeedAndGroup(feeds.selectedFeed, feeds.selectedFeedGroup);
    });
}
function showArticle() {
    var clickedArticle = $(this).parent('tr').attr('data-article-id');
    console.log('show article: ' + clickedArticle);
    handleArticleClicked(clickedArticle);
}
function handleArticleClicked(clickedArticle) {
    console.log('calling article.json for article ' + clickedArticle);
    $.getJSON(urls.article_api + '/' + clickedArticle, function (data) {
        feeds.selectedFeedArticle = data;
        markArticleId(clickedArticle, true, function () {
            updateUI();
            // and scroll to the top
            $(window).scrollTop(0);
        });
    });
}
function saveArticleToPocket() {
    if (feeds.selectedFeedArticles == null) return;
    var saveArticle = $(this).parents('tr').attr('data-article-id');

    console.log('saving article ' + saveArticle + ' to pocket');
    saveArticleIdToPocket(saveArticle, function () {
        console.log('saved article ' + saveArticle + ' to pocket, now marking as read');
        markArticleId(saveArticle, true, function () {
            updateUI();
        });
    });
}
function saveArticleIdToPocket(articleId, onSaveCompleted) {
    console.log('calling pocket api for article ' + articleId);
    $.post(urls.pocket_api, { articleId: articleId }, function (result) {
        console.log('pocket api completed for article ' + articleId + ': ' + result.saved);
        if (onSaveCompleted != undefined && onSaveCompleted != null)
            onSaveCompleted(articleId);
    });
}
function markAllArticlesRead() {
    console.log('mark all articles read');
    if (feeds.selectedFeedArticles == null) return;

    var maxArticleId = 0;
    var serverUpdateRequired = false;
    for (var i = 0; i < feeds.selectedFeedArticles.length; i++) {
        var feedArticle = feeds.selectedFeedArticles[i];
        if (feedArticle.story > maxArticleId) maxArticleId = feedArticle.story;

        if (!feedArticle.read) {
            feedArticle.read = true;
            serverUpdateRequired = true;
        }
    }
    if (serverUpdateRequired) {
        $.post(urls.article_api, { feed: feeds.selectedFeed.id, read: true, maxStory: maxArticleId, offset: getUtcOffset() }, function () {
            console.log('marked all articles read');
            feeds.selectedFeed.count = 0;
            updateUI();
        });
    } else {
        console.log('all articles already read, nothing to do');
    }
}
function toggleArticleRead() {
    if (feeds.selectedFeedArticles == null) return;
    var markArticle = $(this).parents('tr').attr('data-article-id');
    toggleArticleIdRead(markArticle);
}
function toggleSelectedArticleRead() {
    if (feeds.selectedFeedArticle == null) return;
    toggleArticleIdRead(feeds.selectedFeedArticle.id);
}
function toggleArticleIdRead(articleId) {
    console.log('mark article read: ' + articleId);
    var unreadCount = 0;
    var feedToUpdate = null;
    for (var i = 0; i < feeds.selectedFeedArticles.length; i++) {
        var feedArticle = feeds.selectedFeedArticles[i];
        if (feedArticle.story == articleId) {
            feedArticle.read = !feedArticle.read;
            feedToUpdate = feedArticle;
        }
        unreadCount += feedArticle.read ? 0 : 1;
    }
    feeds.selectedFeed.count = unreadCount;

    if (feedToUpdate != null) {
        $.post(urls.article_api, { feed: feeds.selectedFeed.id, story: articleId, read: feedToUpdate.read }, function () {
            updateUI();
        });
    } else {
        updateUI();
    }
}
function updateSelectedArticle() {
    feeds.selectedArticleSection.empty();
    feeds.selectedArticleSection.append(buildFeedArticle());
    $('.feed-title').click(backToFeedArticles);
    $('button.toggle-read').click(toggleSelectedArticleRead);
    $('.next-article').click(markCurrentlySelectedArticleAsRead);
    $('button.send-to-pocket').click(saveCurrentlySelectedArticleToPocket);
}
function buildFeedArticle() {
    var articleSummary = null;
    for (var i = 0; i < feeds.selectedFeedArticles.length; i++) {
        if (feeds.selectedFeedArticles[i].story == feeds.selectedFeedArticle.id) {
            articleSummary = feeds.selectedFeedArticles[i];
            break;
        }
    }
    if (articleSummary == null) {
        console.log('Cannot find article summary for story ' + feeds.selectedFeedArticle.id);
    }
    var articleHtml = '<div class="feed-title">' + feeds.selectedFeedGroup.item;
    articleHtml += ' &gt; ' + feeds.selectedFeed.item + ' (' + feeds.selectedFeed.count + ')';
    articleHtml += '</div>';

    articleHtml += '<div>';
    articleHtml += '<div><span>' + articleSummary.posted + '</span><span class="article-actions"><button class="send-to-pocket image"><img src="' + urls.imageroot + 'image/pocket.png" alt="Send to Pocket"></button><button class="toggle-read image">' + (articleSummary.read ? '<img src="' + urls.imageroot + 'image/markunread.png" alt="Mark as unread">' : '<img src="' + urls.imageroot + 'image/markread.png" alt="Mark as read">') + '</button></span></div>';
    articleHtml += '<div class="article-heading"><a href="' + feeds.selectedFeedArticle.url + '" target="_blank">' + articleSummary.heading + '</a></div>';
    articleHtml += '<div>' + feeds.selectedFeedArticle.body + '</div>';
    articleHtml += '<div><span class="article-actions"><button class="next-article image"><img src="'+urls.imageroot+'image/next.png" alt="Next article"></button></span></div>';
    articleHtml += '</div>';

    return articleHtml;
}
function toggleShowAll() {
    localSettings.showAll = !localSettings.showAll;
    saveLocalSettings();
    updateUI();

    var group = $(this);
    console.log('group clicked: ' + group.parent().attr('id'));
    $(window).scrollTop(group.position().top);
}
function onFeedClicked() {
    handleFeedClicked($(this));
}
function handleFeedClicked(feedElement) {
    var group = findGroup(feedElement.parents('section').attr('id'));
    if (group == null) return;
    var feed = findItemInGroup(group, feedElement.attr('id'));
    if (feed == null) return;

    loadArticlesForFeedAndGroup(feed, group);
}
function loadArticlesForFeedAndGroup(feed, group) {
    console.log('feed clicked: #' + feed.id + '=' + feed.item + ' (group#' + group.id + '=' + group.item + ')');

    $.getJSON(urls.feed_api + "/" + feed.id + "?offset=" + getUtcOffset(), function (data) {
        feeds.selectedFeed = feed;
        feeds.selectedFeedGroup = group;
        feeds.selectedFeedArticles = data;
        updateUI();
    });
}
function backToAllFeeds() {
    if (feeds.selectedFeed == null || feeds.selectedFeedGroup == null) return;

    var feedEl = $('section[id="' + feeds.selectedFeedGroup.id + '"] li[id="' + feeds.selectedFeed.id + '"]', feeds.allGroupsSection);

    feeds.selectedFeed = null;
    feeds.selectedFeedGroup = null;
    feeds.selectedFeedArticles = null;
    // refresh feed count details
    refreshFeedCounts(function () {
        feeds.selectedFeedSection.empty();

        console.log('back to feed: ' + feedEl.attr('id'));
        $(window).scrollTop(feedEl.position().top);
    });
}
function backToFeedArticles() {
    if (feeds.selectedFeedArticle == null) return;

    var selectedFeedArticleId = feeds.selectedFeedArticle.id;
    feeds.selectedFeedArticle = null;
    feeds.selectedArticleSection.empty();
    updateUI();

    var feedEl = $('table.article-list tbody tr[data-article-id="' + selectedFeedArticleId + '"]', feeds.selectedFeedSection);
    console.log('back to feed articles: ' + selectedFeedArticleId);
    $(window).scrollTop(feedEl.position().top);
}
function markArticleId(articleId, read, markedAsReadCompleted) {
    var unreadArticle = null;
    var foundCurrentArticle = false;
    var unreadCount = 0;
    var serverUpdateRequired = false;
    for (var i = 0; i < feeds.selectedFeedArticles.length; i++) {
        var feedArticle = feeds.selectedFeedArticles[i];
        if (feedArticle.story == articleId) {
            if (feedArticle.read != read) {
                feedArticle.read = read;
                serverUpdateRequired = true;
            }
            foundCurrentArticle = true;
        } else if (foundCurrentArticle && unreadArticle == null && !feedArticle.read) {
            unreadArticle = feedArticle;
        }
        if (!foundCurrentArticle && !feedArticle.read) {
            unreadArticle = feedArticle;
        }
        unreadCount += feedArticle.read ? 0 : 1;
    }
    feeds.selectedFeed.count = unreadCount;

    if (serverUpdateRequired) {
        console.log('letting server know that story ' + articleId + ' should be marked read/unread: ' + read);
        $.post(urls.article_api, { feed: feeds.selectedFeed.id, story: articleId, read: read }, function () {
            if (markedAsReadCompleted != undefined && markedAsReadCompleted != null)
                markedAsReadCompleted(unreadArticle);
        });
    } else {
        if (markedAsReadCompleted != undefined && markedAsReadCompleted != null)
            markedAsReadCompleted(unreadArticle);
    }
}
function markCurrentlySelectedArticleAsRead() {
    markArticleIdAsReadAndMoveToNext(feeds.selectedFeedArticle.id);
}
function markArticleIdAsReadAndMoveToNext(articleId) {
    console.log('marking story as read: ' + articleId);
    markArticleId(articleId, true, function (unreadArticle) {
        if (unreadArticle != null) {
            console.log('show next article: ' + unreadArticle.story);
            handleArticleClicked(unreadArticle.story);
        } else {
            console.log('no next articles, going back to all');
            backToFeedArticles();
        }
    });
}
function saveCurrentlySelectedArticleToPocket() {
    var articleId = feeds.selectedFeedArticle.id;
    console.log('save story to pocket: ' + articleId);

    saveArticleIdToPocket(articleId, function () {
        markArticleIdAsReadAndMoveToNext(articleId);
    });
}
function markCurrentlyFocusedArticleAsRead(focusNextArticle) {
    if (feeds.focusedArticle != undefined && feeds.focusedArticle != null) {
        markArticleId(feeds.focusedArticle.story, true, function () {
            updateUI();
            if (focusNextArticle)
                focusArticle(true);
        });
    }
}
function saveCurrentlyFocusedArticleToPocket() {
    if (feeds.focusedArticle != undefined && feeds.focusedArticle != null) {
        var articleId = feeds.focusedArticle.story;
        saveArticleIdToPocket(articleId, function () {
            markArticleId(articleId, true, function () {
                updateUI();
                focusArticle(true);
            });
        });
    }
}
function handleKeyPress(evt) {
    //console.log('handle key: ' + evt.which);

    if (feeds.selectedFeedArticle != null) {
        if (evt.which == 65 || evt.which == 82 || evt.which == 78) {
            // 'a' or 'r' or 'n' mark the currently viewing article
            // as read and shows the next article (if there is one)
            // if this is the last article, return to the all groups/feeds
            // view

            markCurrentlySelectedArticleAsRead();
        } else if (evt.which == 66 || evt.which == 87) {
            // 'b' or 'w' takes you back
            backToFeedArticles();
        } else if (evt.which == 85) {
            // 'u' marks as unread
            markArticleId(feeds.selectedFeedArticle.id, false, function () { updateUI(); });
        } else if (evt.which == 80) {
            // 'p' adds the article to pocket
            saveArticleIdToPocket(feeds.selectedFeedArticle.id, function () { markCurrentlySelectedArticleAsRead(); });
        }

        return;
    }

    if (feeds.selectedFeed != null) {
        if (evt.which == 66 || evt.which == 87) {
            // 'b' or 'w' takes you back
            backToAllFeeds();
        } else if (evt.which == 74 || evt.which == 40) {
            // 'j' or 'arrow down' focuses the next article
            evt.preventDefault();
            focusArticle(true);
        } else if (evt.which == 75 || evt.which == 38) {
            // 'k' or 'arrow up' focuses the previous article
            evt.preventDefault();
            focusArticle(false);
        } else if (evt.which == 65 || evt.which == 82 || evt.which == 78) {
            // 'a' or 'r' or 'n' mark the focused article as read
            markCurrentlyFocusedArticleAsRead(evt.which == 78);
        } else if (evt.which == 85) {
            // 'u' marks the focused article as unread
            if (feeds.focusedArticle != undefined && feeds.focusedArticle != null) {
                markArticleId(feeds.focusedArticle.story, false, function () {
                    updateUI();
                });
            }
        } else if (evt.which == 80) {
            // 'p' adds the focused article to pocket and then marks as read
            saveCurrentlyFocusedArticleToPocket();
        } else if (evt.which == 77) {
            // 'm' marks all the articles as read
            markAllArticlesRead();
        } else if (evt.which == 13) {
            // 'return' shows the focused article
            if (feeds.focusedArticle != undefined && feeds.focusedArticle != null) {
                evt.preventDefault();
                handleArticleClicked(feeds.focusedArticle.story);
            }
        }
        return;
    }

    // showing all groups
    if (evt.which == 74 || evt.which == 40) {
        // 'j' or 'arrow down' focuses the next feed
        evt.preventDefault();
        focusNextFeed();
    } else if (evt.which == 75 || evt.which == 38) {
        // 'k' or 'arrow up' focuses the previous feed
        evt.preventDefault();
        focusPreviousFeed();
    } else if (evt.which == 13) {
        // 'return' shows the focused feed

        // find currently focused feed: TODO: fix copy/paste from above
        if (feeds.focusedFeed == undefined) feeds.focusedFeed = null;
        else {
            evt.preventDefault();
            // find element
            var focusedEl = $('section.group-section li[id="' + feeds.focusedFeed.id + '"]', feeds.allGroupsSection);
            handleFeedClicked(focusedEl);
        }
    }
}

function focusNextFeed() {
    focusFeed(
        function (f) { return 0; },
        function (f, grpIdx) { return grpIdx < f.length; },
        function (grpIdx) { return grpIdx + 1; },
        function (grpItems) { return 0; },
        function (grpItems, feedIdx) { return feedIdx < grpItems.length; },
        function (feedIdx) { return feedIdx + 1; }
    );
}
function focusPreviousFeed() {
    focusFeed(
        function (f) { return f.length - 1; },
        function (f, grpItems) { return grpItems >= 0; },
        function (grpItems) { return grpItems - 1; },
        function (groupItems) { return groupItems.length - 1; },
        function (groupItems, feedIdx) { return feedIdx >= 0; },
        function (feedIdx) { return feedIdx - 1; }
    );
}
function focusFeed(groupIdxStart, groupEnd, groupIdxChange, feedIdxStart, feedEnd, feedIdxChange) {
    // find currently focused feed
    if (feeds.focusedFeed == undefined) feeds.focusedFeed = null;
    else {
        // find element
        var focusedEl = $('section.group-section li[id="' + feeds.focusedFeed.id + '"]', feeds.allGroupsSection);
        focusedEl.removeClass('focused');
    }

    var firstFeed = null;
    var nextFeed = null;
    var foundFocusedFeed = false;
    var isFeedItemVisible = function (feedItem) {
        return localSettings.showAll || feedItem.count > 0;
    };
    for (var groupIdx = groupIdxStart(feeds) ; groupEnd(feeds, groupIdx) ; groupIdx = groupIdxChange(groupIdx)) {
        var groupItems = feeds[groupIdx].items;
        for (var feedIdx = feedIdxStart(groupItems) ; feedEnd(groupItems, feedIdx) ; feedIdx = feedIdxChange(feedIdx)) {
            var feedItem = groupItems[feedIdx];
            if (foundFocusedFeed && isFeedItemVisible(feedItem)) {
                nextFeed = feedItem;
                break;
            }

            if (feeds.focusedFeed != null && feeds.focusedFeed.id == feedItem.id)
                foundFocusedFeed = true;

            if (firstFeed == null && isFeedItemVisible(feedItem)) {
                firstFeed = feedItem;
            }
        }
        if (nextFeed != null) break;
    }
    if (nextFeed == null) nextFeed = firstFeed;
    if (nextFeed != null) {
        console.log('focusing ' + nextFeed.id + ' (' + nextFeed.item + ')');
        // find element
        var focusEl = $('section.group-section li[id="' + nextFeed.id + '"]', feeds.allGroupsSection);
        focusEl.addClass('focused');
        feeds.focusedFeed = nextFeed;
        $(window).scrollTop(focusEl.position().top);
    }
}
function focusArticle(nextOrPrevious) {
    // find currently focused article
    if (feeds.focusedArticle == undefined) feeds.focusedArticle = null;
    else {
        // find element
        var focusedEl = $('table.article-list tbody tr[data-article-id="' + feeds.focusedArticle.story + '"]', feeds.selectedFeedSection);
        focusedEl.removeClass('focused');
    }

    var firstArticle = null;
    var nextArticle = null;
    var foundFocusedArticle = false;

    var startIdx = nextOrPrevious ? 0 : feeds.selectedFeedArticles.length - 1;
    var condition = function (idx) { return nextOrPrevious ? idx < feeds.selectedFeedArticles.length : idx >= 0; };
    var idxChange = function (idx) { return nextOrPrevious ? idx + 1 : idx - 1; };
    for (var i = startIdx; condition(i) ; i = idxChange(i)) {
        var article = feeds.selectedFeedArticles[i];
        if (foundFocusedArticle) {
            nextArticle = article;
            break;
        }

        if (feeds.focusedArticle != null && feeds.focusedArticle.story == article.story)
            foundFocusedArticle = true;

        if (firstArticle == null)
            firstArticle = article;
    }

    if (nextArticle == null) nextArticle = firstArticle;
    if (nextArticle != null) {
        console.log('focusing ' + nextArticle.story + ' (' + nextArticle.heading + ')');
        // find element
        var focusEl = $('table.article-list tbody tr[data-article-id="' + nextArticle.story + '"]', feeds.selectedFeedSection);
        focusEl.addClass('focused');
        feeds.focusedArticle = nextArticle;
        $(window).scrollTop(focusEl.position().top);
    }
}
function getUtcOffset() {
    return new Date().getTimezoneOffset();
}
