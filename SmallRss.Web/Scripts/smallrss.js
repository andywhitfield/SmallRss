var SmallRssModel = function () {
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
}
var model = new SmallRssModel();

function sizeSplitter() {
    $('#mainSplitter').height($(window).innerHeight() - 80);
}

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
        $.each($('#jqxTree').jqxTree('getItems'), function (i, item) {
            var label = $("#" + item.element.id + " .jqx-tree-item:first");

            var isFeed = item.parentElement != null;
            var count = findUnreadCount(result, isFeed, item.value, item.label);
            if (count == null || count == 0) {
                label.text(item.label);
                label.removeClass('unread-items');
            } else {
                label.text(item.label + " (" + count + ")");
                label.addClass('unread-items');
            }
        });
    });
}

$(document).ready(function () {
    var itemsSource = {
        localdata: model.feedItems(), datatype: "json"
    };

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
                model.articleBody(result.articleBody);
                model.articleUrl(result.articleUrl);
            });
            $.post(urls.article_api, { story: story, read: true }, function () {
                model.markStory(story, true);
                $('#jqxgrid').jqxGrid('refreshdata');

                updateUnreadCounts();
            });
        });
        this.get('#/feed/:feed', function () {
            model.selectedArticle(0);
            model.selectedFeed(this.params['feed']);
            model.feedItems.removeAll();
            $("#jqxgrid").jqxGrid('clearselection');

            $.get(urls.feed_api + '/' + this.params['feed'], function (result) {
                $.each(result, function (i, a) { model.feedItems.push(a); });
                itemsSource.localdata = model.feedItems();
                $('#jqxgrid').jqxGrid('updatebounddata', 'cells');
            });
        });
    });

    $('#mainSplitter').jqxSplitter({ width: '100%', height: 500, panels: [{ size: 200 }, { size: 450 }] });
    $('#rightSplitter').jqxSplitter({ orientation: 'horizontal', panels: [{ size: 300, collapsible: false }, { size: 200 }] });
    $(window).resize(function () { sizeSplitter(); });
    sizeSplitter();

    // initialise the left-hand tree, showing the available feeds
    $.get(urls.feed_api, function (result) {
        model.loadingFeeds(false);
        model.anyFeeds(result.length > 0);
        $('#jqxTree').jqxTree({ height: '100%', width: '100%', source: result });
        $('#jqxTree').bind('select', function (event) {
            var item = $('#jqxTree').jqxTree('getItem', event.args.element);
            if (item.value == null || item.value == '') {
                app.setLocation('#/');
            } else {
                app.setLocation('#/feed/' + item.value);
            }
        });
        $('#jqxTree').on('expand', function (event) {
            var item = $('#jqxTree').jqxTree('getItem', event.args.element);
            $.post(urls.feedstatus_api, { group: item.label, expanded: true });
        });
        $('#jqxTree').on('collapse', function (event) {
            var item = $('#jqxTree').jqxTree('getItem', event.args.element);
            $.post(urls.feedstatus_api, { group: item.label, expanded: false });
        });
        // set the current node selected if applicable - i.e. when F5'ing the page
        if (model.isFeedSelected()) {
            var treeItems = $('#jqxTree').jqxTree('getItems');

            for (var i = 0; i < treeItems.length; i++) {
                if (model.selectedFeed() == treeItems[i].value) {
                    $('#jqxTree').jqxTree('selectItem', treeItems[i].element);
                    break;
                }
            }
        }

        updateUnreadCounts();
    });

    var cellsrenderer = function (row, columnfield, value, defaulthtml, columnproperties) {
        if (model.feedItems()[row].read) {
            return defaulthtml;
        } else {
            return '<strong>' + defaulthtml + '</strong>';
        }
    }
    var dataAdapter = new $.jqx.dataAdapter(itemsSource, {
        loadComplete: function (data) { },
        loadError: function (xhr, status, error) { }
    });
    $("#jqxgrid").jqxGrid({
        source: dataAdapter,
        columns: [
          { text: 'Heading', datafield: 'heading', width: '40%', cellsrenderer: cellsrenderer },
          { text: 'Article', datafield: 'article', width: '40%', cellsrenderer: cellsrenderer },
          { text: 'Posted', datafield: 'posted', width: '20%', cellsrenderer: cellsrenderer }
        ],
        columnsresize: false, /* this is buggy, so don't allow this */
        width: '100%',
        autoheight: true
    });
    $('#jqxgrid').on('rowselect', function (event) {
        var row = event.args.rowindex;
        var feedDetails = model.feedItems()[row];
        app.setLocation('#/feed/' + feedDetails.feed + '/' + feedDetails.story);
    });
    $('#keepUnread').click(function () {
        $.post(urls.article_api, { story: model.selectedArticle(), read: false }, function () {
            model.markStory(model.selectedArticle(), false);
            $('#jqxgrid').jqxGrid('refreshdata');

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
            $('#jqxgrid').jqxGrid('refreshdata');

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

    app.run();
    ko.applyBindings(model);

    // TODO: F5 not working when pointing to an article
});
