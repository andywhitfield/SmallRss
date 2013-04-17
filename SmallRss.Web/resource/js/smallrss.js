var SmallRssModel = function () {
    this.treeApi = null;
    this.loadingFeeds = ko.observable(true);
    this.anyFeeds = ko.observable(false);
    this.selectedFeed = ko.observable("");
    this.isFeedSelected = ko.computed(function () { return this.selectedFeed() != ""; }, this);
    this.selectedArticle = ko.observable(0);
    this.isArticleSelected = ko.computed(function () { return this.selectedArticle() != 0; }, this);
    this.feedItems = ko.observableArray([]);
    this.anyItemsInFeed = ko.computed(function () { return this.feedItems().length > 0; }, this);
    this.articleTitle = ko.observable("");
    this.articleBody = ko.observable("");
    this.articleUrl = ko.observable("");
    this.findFeedItem = function (storyId) {
        var items = this.feedItems();
        for (var i = 0; i < items.length; i++)
            if (items[i].story == storyId) return items[i];
        return null;
    };
    this.markStory = function (id, asRead) {
        var story = this.findFeedItem(id);
        if (story != null) story.read = asRead;
    };
    this.splitWestPosition = '200';
    this.splitNorthPosition = '300';
}
var model = new SmallRssModel();

function findUnreadCount(unreadCounts, isFeed, feedId, feedGroup) {
    for (var g = 0; g < unreadCounts.length; g++) {
        if (isFeed) {
            for (var i = 0; i < unreadCounts[g].items.length; i++) {
                if (unreadCounts[g].items[i].value == feedId) return unreadCounts[g].items[i].unread;
            }
        } else {
            if (unreadCounts[g].label == feedGroup) return unreadCounts[g].unread;
        }
    }
    return null;
}

function updateUnreadCounts() {
    $.get(urls.feedstatus_api, function (result) {
        if (model.treeApi == null) return;

        var group = model.treeApi.first(null);
        while (group != null) {
            if (group.data('originalText') == null)
                group.data('originalText', model.treeApi.getText(group));

            var count = findUnreadCount(result, false, model.treeApi.getId(group), group.data('originalText'));
            var groupEl = $("span.aciTreeText:first", group);
            if (count == null || count == 0) {
                model.treeApi.setText(group, group.data('originalText'));
                groupEl.removeClass('unread-item');
            } else {
                model.treeApi.setText(group, group.data('originalText') + ' (' + count + ')');
                groupEl.addClass('unread-item');
            }

            var feed = model.treeApi.first(group);
            while (feed != null) {
                if (feed.data('originalText') == null)
                    feed.data('originalText', model.treeApi.getText(feed));

                var count = findUnreadCount(result, true, model.treeApi.getId(feed), feed.data('originalText'));
                var feedEl = $("span.aciTreeText:first", feed);
                if (count == null || count == 0) {
                    model.treeApi.setText(feed, feed.data('originalText'));
                    feedEl.removeClass('unread-item');
                } else {
                    model.treeApi.setText(feed, feed.data('originalText') + ' (' + count + ')');
                    feedEl.addClass('unread-item');
                }

                if (!model.treeApi.hasNext(feed)) break;
                feed = model.treeApi.next(feed);
            }

            if (!model.treeApi.hasNext(group)) break;
            group = model.treeApi.next(group);
        }
    });
}

$(function () {
    var app = $.sammy(function () {
        this.get('#/', function () {
            model.selectedFeed("");
            model.selectedArticle(0);
        });
        this.get('#/feed/:feed/:story', function () {
            var feed = this.params['feed'];
            var story = this.params['story'];
            var storyItem = model.findFeedItem(story);
            if (storyItem == null) return;

            model.selectedArticle(story);
            model.selectedFeed(feed);
            model.articleTitle(storyItem.heading);
            model.articleBody('');
            model.articleUrl('');
            $.get(urls.article_api + '/' + story, function (result) {
                model.articleBody(result.body);
                model.articleUrl(result.url);
            });
            $.post(urls.article_api, { story: story, read: true }, function () {
                model.markStory(story, true);

                $.each($('#articleGrid').jqGrid('getRowData'), function (i, id) {
                    if (id.story == story) {
                        if (id.heading.length > 0 && id.heading[0] == '<') {
                            id.heading = $(id.heading).text();
                            id.article = $(id.article).text();
                            id.posted = $(id.posted).text();
                            $('#articleGrid').jqGrid('setRowData', i + 1, id);
                        }
                    }
                });

                updateUnreadCounts();
            });
        });
        this.get('#/feed/:feed', function () {
            model.selectedArticle(0);
            model.selectedFeed(this.params['feed']);
            model.feedItems.removeAll();

            $("#articleGrid").jqGrid('clearGridData');
            console.log('loading articles for ' + this.params['feed']);

            $("#articleGrid").jqGrid('setGridParam', { url: urls.feed_api + '/' + this.params['feed'], datatype: 'json' });
            $("#articleGrid").trigger('reloadGrid');
        });
    });

    $('#mainSplitter').width($('#mainPanel').width()).height($('#mainPanel').height()).split({ orientation: 'vertical', limit: 80, position: model.splitWestPosition });
    $('#splitCenter').split({ orientation: 'horizontal', limit: 80, position: model.splitNorthPosition });

    $('#feedTree').aciTree({
        animateRoot: false,
        jsonUrl: urls.feed_api,
        loaderDelay: 0,
        show: { props: { 'height': 'show' }, duration: 200, easing: 'linear' },
        hide: { props: { 'height': 'hide' }, duration: 100, easing: 'linear' }
    });
    $('#feedTree').on('acitree', function (e, api, item, data) {
        if (api.isLocked()) return;
        switch (data.event) {
            case 'init':
                model.treeApi = api;
                model.loadingFeeds(false);
                model.anyFeeds(api.first(null) != null);

                // set the current node selected if applicable - i.e. when F5'ing the page
                if (model.isFeedSelected()) {
                    api.searchId(model.selectedFeed(), false, false, {
                        success: function (itemToSelect) {
                            api.select(itemToSelect, true);
                            api.setVisible(itemToSelect);
                        },
                        fail: function (itemToSelect) {
                            console.log('could not find item to select! ' + model.selectedFeed());
                        }
                    });
                }

                updateUnreadCounts();

                break;
            case 'selected':
                console.log('tree item selected: ' + api.getId(item) + ', ' + data.event);
                if (api.isFolder(item))
                    app.setLocation('#/');
                else
                    app.setLocation('#/feed/' + api.getId(item));
                break;
            case 'opened':
                $.post(urls.feedstatus_api, { group: api.getId(item), expanded: true });
                break;
            case 'closed':
                $.post(urls.feedstatus_api, { group: api.getId(item), expanded: false });
                break;
        }
    });

    $("#articleGrid").jqGrid({
        datatype: "jsonstring",
        datastr: "",
        colNames: ['Story', 'Heading', 'Article', 'Posted', 'Read'],
        colModel: [
            { name: 'story', index: 'story', hidden: true },
            { name: 'heading', index: 'heading', width: '40%', sortable: false },
            { name: 'article', index: 'article', width: '44%', sortable: false },
            { name: 'posted', index: 'posted', width: '16%', sortable: false },
            { name: 'read', index: 'read', hidden: true }
        ],
        viewrecords: true,
        height: '100%',
        forceFit: true,
        rowNum: 1000,
        onSelectRow: function (rowId, status, e) {
            var feedId = model.selectedFeed();
            var storyId = $(this.rows[rowId].cells[0]).text();
            app.setLocation('#/feed/' + feedId + '/' + storyId);
        },
        loadComplete: function (result) {
            $.each(result, function (i, a) { model.feedItems.push(a); });
            $.each(this.rows, function (i, row) {
                if ($(row.cells[4]).text() == 'false') {
                    $(row.cells[1]).wrapInner('<span class="unread-item" />');
                    $(row.cells[2]).wrapInner('<span class="unread-item" />');
                    $(row.cells[3]).wrapInner('<span class="unread-item" />');
                }
            });
        }
    });
    $("#articleGrid").jqGrid('bindKeys');
    $("#articleGrid").jqGrid('setGridWidth', $("#articleGridContainer").width() - 15);

    $('#keepUnread').click(function () {
        $.post(urls.article_api, { story: model.selectedArticle(), read: false }, function () {
            model.markStory(model.selectedArticle(), false);

            $.each($('#articleGrid').jqGrid('getRowData'), function (i, id) {
                if (id.story == model.selectedArticle()) {
                    id.heading = '<span class="unread-item">' + id.heading + '</span>';
                    id.article = '<span class="unread-item">' + id.article + '</span>';
                    id.posted = '<span class="unread-item">' + id.posted + '</span>';
                    $('#articleGrid').jqGrid('setRowData', i + 1, id);
                }
            });

            updateUnreadCounts();
        });
    });
    $('#openArticle').click(function () {
        if (model.articleUrl() == null) return;
        window.open(model.articleUrl(), '_blank');
    });
    $('#markAllRead').click(function () {
        $.post(urls.article_api, { feed: model.selectedFeed(), read: true }, function () {
            $.each(model.feedItems(), function (i, f) {
                if (f.feed == model.selectedFeed())
                    model.markStory(f.story, true);
            });
            $.each($('#articleGrid').jqGrid('getRowData'), function (i, id) {
                if (id.heading.length > 0 && id.heading[0] == '<') {
                    id.heading = $(id.heading).text();
                    id.article = $(id.article).text();
                    id.posted = $(id.posted).text();
                    $('#articleGrid').jqGrid('setRowData', i + 1, id);
                }
            });

            updateUnreadCounts();
        });
    });
    $('input[name=showAllOrUnread]').change(function () {
        $.post(urls.feedstatus_api, { showall: $('input[name=showAllOrUnread]:checked').val() == "all" }, function () {
            if (model.isFeedSelected()) {
                if (model.isArticleSelected()) app.setLocation('#/feed/' + model.selectedFeed());
                else app.refresh();
            }
        });
    });

    $(window).resize(function () {
        $('#mainSplitter').width($('#mainPanel').width()).height($('#mainPanel').height());
        $('.spliter_panel').trigger('spliter.resize');
    });
    $('#splitCenter').bind('spliter.resize', function () {
        $("#articleGrid").jqGrid('setGridWidth', $("#articleGridContainer").width());
    });
    $('#mainSplitter').bind('mouseup.spliter', function () {
        console.log('resized splitter: ' + $('#splitWest').width() + '/' + $('#splitNorth').height());
        $.post(urls.userlayout_api, { splitWest: $('#splitWest').width(), splitNorth: $('#splitNorth').height() });
    });

    app.run();
    ko.applyBindings(model);
});