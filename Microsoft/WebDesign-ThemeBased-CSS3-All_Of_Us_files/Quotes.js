$(function () {
    function rotateQuotes() {
        $('div.quote.visible').each(function () {

            $(this).fadeOut(500, function () {
                $(this).removeClass('visible');

                var next = $(this).next();
                if (next.length == 0)
                    next = $('div.quote:first');

                next.fadeIn(500).addClass('visible');

            });
        });
        setTimeout(rotateQuotes, 10000);
    }
    rotateQuotes();

});




