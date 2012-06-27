$(function () {
    var pop = function () {
        var caseStudyId = $(this).attr('caseStudyId');
        var boxid = '#box_' + caseStudyId;
        var closeid = '#close_' + caseStudyId;
        clearScreen();
        $('#screen').css({ 'display': 'block', opacity: 0.5, 'width': $(document).width(), 'height': $(document).height() });
        $('body').css({ 'overflow': 'hidden' });
        $(boxid).show();
        $(closeid).click(clearScreen);
    }

    $('#screen').click(function () { $('div[id*="box_"]').hide(); $('#screen').hide() });

    var clearScreen = function () {
        $('#screen').trigger('click');
    }
    $('.snapimage').click(pop);
    //$('img[alt="Back"]').click(pop);
    //$('img[alt="Next"]').click(pop);

    var showb = function () {
        $('.back').css({ 'display': 'block' });
    }
    var hideb = function () {
        $('.back').css({ 'display': 'none' });
    }
    var shown = function () {
        $('.next').css({ 'display': 'block' });
    }
    var hiden = function () {
        $('.next').css({ 'display': 'none' });
    }

    $('.lefthalf').click(pop).mouseover(showb).mouseout(hideb);
    $('.righthalf').click(pop).mouseover(shown).mouseout(hiden);

});