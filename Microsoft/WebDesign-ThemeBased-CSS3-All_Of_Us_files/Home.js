$(function () {
    var t;
    $('div.home.visible').show();
    function rotatehomeanim() {
        $('div.home.visible').each(function () {

            $(this).fadeOut(500, function () {
                $(this).removeClass('visible');

                var next = $(this).next();
                if (next.length == 0)
                    next = $('div.home:first');

                next.fadeIn(500).addClass('visible');
            });
        });

        t = setTimeout(rotatehomeanim, 7500);
    }
    t = setTimeout(rotatehomeanim, 7500);
    

    $('div.home').hover(function () {
        clearTimeout(t);
    }, function () {
        t = setTimeout(rotatehomeanim, 1);
    });

});