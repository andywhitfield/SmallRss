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
}

function initArticle() {
    $('.articles').css('cursor', 'default');
    $('.articles li div:first').click(function () {
        window.open($(this).attr('data-url'), '_blank');
    }).css('cursor', 'pointer');
    $('input[value="Back"]').click(function () {
        window.location = $(this).attr('data-url');
    });
    $('.filterButtons > div').css('cursor', 'pointer').click(function () {
        window.location = $(this).attr('data-url');
    });
}
