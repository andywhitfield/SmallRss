function initGroups() {
    $('.ui-tree').children().click(function () {
        var group = $(this).attr('data-id');
        $('ul', this).slideToggle('fast', function () {
            $.post(urls.expandcollapse, { group: group, expanded: $(this).css('display') != 'none' });
        });
    });
    $('.ui-tree li[data-open="False"] ul').hide();
    $('.ui-tree li ul li').click(function () {
        window.location = $(this).attr('data-url') + '?offset=' + new Date().getTimezoneOffset();
        return false;
    });
    $('.ui-tree li ul').each(function () {
        $('li:odd', this).addClass('ui-tree-alt');
    });
}

function initFeedItems() {
    $('.articles li').each(function () {
        $('div:first', this).css('float', 'right');
        $(this).click(function () {
            window.location = $(this).attr('data-url');
        });
    });
    $('.articles li:odd').addClass('articlesAlt');
    $('.filterButtons > div').css('cursor', 'pointer').click(function () {
        window.location = urls.home;
    });
    $('input.showAll').click(function () {
        $.post(urls.showAllOrUnread, { showAll: true }, function () {
            window.location.reload();
        });
    });
    $('input.showUnread').click(function () {
        $.post(urls.showAllOrUnread, { showAll: false }, function () {
            window.location.reload();
        });
    });
    $('input.markAllRead').click(function () {
        $.post(urls.markAllRead, { feed: $(this).attr('data-id') }, function () {
            window.location.reload();
        });
    });
}

function initArticle() {
    $('.articles').css('cursor', 'default');
    $('.articles li div:first').click(function () {
        window.open($(this).attr('data-url'), '_blank');
    }).css('cursor', 'pointer');
    $('input[value="Back"]').click(function () {
        window.location = $(this).attr('data-url') + '?offset=' + new Date().getTimezoneOffset();
    });
    $('.filterButtons > div').css('cursor', 'pointer').click(function () {
        window.location = $(this).attr('data-url') + '?offset=' + new Date().getTimezoneOffset();
    });
    $('input.keepUnread').click(function () {
        var articleId = $(this).attr('data-id');
        var feedUrl = $(this).attr('data-url');
        $.post(urls.keepUnread, { article: articleId }, function () {
            window.location = feedUrl + '?offset=' + new Date().getTimezoneOffset();
        });
    });
}
