/**
 * Project: CityLove (v1.0.0)
 * Author: Romanov Artem
 * Contact: hardxiii@gmail.com
 * Date: Mon Jun 30 2014
 */

"use strict";

function showRestore() {
    $.get('/Profile/Restore', {}, function (d) {
        $('#dialog-login').html(d);
    });
}

function showLogin() {
    $.get('/Profile/Login', {}, function (d) {
        $('#dialog-login').html(d);
        $('#dialog-login #dialog-login input[type="checkbox"]').styler();
    });

}

function setPlacesOrder(order) {
    $('.place-order a').removeClass('active');
    $('.place-order a[arg="' + order + '"]').addClass('active');
    $('input[name="Order"]').val(order);
    $('.place-order').parents('form').submit();
    return false;
}

function initFriendTab() {
    
    var cook = $.cookie('FriendTab');
    if (cook && cook.length) {

        $('.tabs-friend').tabs({ active: parseInt(cook.replace('col', '') - 1) });

    } else {
        $('.tabs-friend').tabs({ active: 0 });
    }


    $('.tabs-friend li').click(function () {

        var role = $(this).find('a').attr('arg');
        $.cookie('FriendTab', role, {
            path: '/',
            expires: 365

        });
    });

}

function initEventTab() {
    
    var cook = $.cookie('EventTab');
    if (cook && cook.length) {

        $('.tabs-ev').tabs({ active: parseInt(cook.replace('col', '') - 1) });

    } else {
        $('.tabs-ev').tabs({ active: 0 });
    }

    $('.tabs-ev li').click(function () {
        var role = $(this).find('a').attr('arg');
        $.cookie('EventTab', role, {
            path: '/',
            expires: 365

        });
    });


}

function initUIAgain(obj) {
    obj.find('input[type="checkbox"]').styler();
/*
    obj.find('input[type="checkbox"]').change(function() {
        $(this).trigger('refresh');
    });
*/
    obj.find('select').styler({ selectSearch: false, });

    obj.find('input[arg="date"]').pickmeup({
        position: 'right',
        hide_on_select: true,
        format: 'd.m.Y'
    });

    obj.find('input[arg="time"]').timepicker({
        showPeriodLabels: false,
    });
}

function visit() {
    $.post('/Home/Visit', {}, function() {
        setTimeout(function() {
            visit();
        }, 4000);
    });
}

function pay(obj) {
    if (!$('.order .order-checks:visible input[type="radio"]:checked').length) {
        alert('Необходимо выбрать Vip-опцию');
        return false;
    }

    if (parseInt($('.order .order-checks:visible input[type="radio"]:checked').attr('cost')) > parseFloat($(obj).attr('arg'))) {
        alert('У вас недостаточно средств. Необходимо пополнить баланс');
        return false;
    }
    var radio = $('.order .order-checks:visible input[type="radio"]:checked');
    var arg = (document.location.hash == '#place' || $('input[type="radio"][value="place"]').is(':checked') ? $('.order-info:visible input[name="placelist"]:checked').attr('arg') : '');
    $.post('/Cabinet/Buy', { cost: radio.attr('cost'), type: radio.attr('value'), duration: radio.attr('duration'), arg : arg }, function(d) {
        alert(d);
        setTimeout(function () {
            if (arg.length) {
                document.location.href = "/Places/Places";
            } else {
                document.location.href = "/Cabinet/Index";
            }
            
        }, 2000);
    });

    return false;
}

$(document).ready(function () {


    var hash = document.location.hash.replace('#', '');
    var radio = $('.order-checks:visible input[type="radio"][value="' + hash + '"]');
    if (radio.length) {
        radio.parents('form').find('.order-checks:visible input[type="radio"]').removeAttr('checked');
        radio.attr('checked', 'checked');
        $('input[type="radio"]').trigger('refresh');
        $('.order-info-content:visible').html(radio.parent().attr('data-info'));
    }


    $('.city-selected select').change(function() {
        $.post('/Home/SaveTown', {townID: $(this).val()}, function(d) {
            if (d == '1') {
                document.location.reload();
            }
        })
    });

    $('.WishToMeet').change(function() {
        $.post('/Meeting/WishToMeet', { ID: $(this).val() }, function() {
            document.location.reload();
        });
    });

    try {
        $('input[arg="date"]').pickmeup({
            position: 'right',
            hide_on_select: true,
            format: 'd.m.Y'
        });

        $('input[arg="time"]').timepicker({
            showPeriodLabels: false,
        });
    } catch (e) {
        
    }
    //$('input[arg="date"]').datepicker( "option", "dateFormat", "dd.mm.yy" );


    var noms = '<li class="user-list-item noms"><b>У вас нет входящий сообщений</b></li>';
    $('[rel="message-switch"]').click(function() {
        if ($(this).attr('arg') == 'all') {
            $(this).parent().find('ul li').hide();
            var list = $(this).parent().find('ul li');
            $(this).parent().find('.noms').remove();
            if (!list.length) {
                $(this).parent().append(noms);
            }
            list.show();
            $(this).parent().find('[rel="message-switch"]').removeClass('active');
            $(this).addClass('active');
            return false;

        } else {
            $(this).parent().find('ul li').hide();
            var list = $(this).parent().find('ul li[read="0"]');
            $(this).parent().find('.noms').remove();
            if (!list.length) {
                $(this).parent().append(noms);
            }
            list.show();
            $(this).parent().find('[rel="message-switch"]').removeClass('active');
            $(this).addClass('active');
            return false;

        }
    });

    $('[rel="msg-del"]').click(function () {
        var t = $(this);
        $.post('/Cabinet/DeleteMsg', { ID: t.attr('arg') }, function() {
            t.parents('.user-list-item').remove();
        });
    });

/*
    $('input[type="checkbox"]').change(function () {
        $(this).trigger('refresh');
    });
*/

    visit();
    $('.tabs-profile li').click(function () {
        
        var role = $(this).find('a').attr('arg');
        $.cookie('ProfileTab', role , {
            path: '/',
            expires: 365

        });
    });
   $('.tabs-meet li').click(function () {
        
        var role = $(this).find('a').attr('arg');
        $.cookie('MeetTab', role , {
            path: '/',
            expires: 365

        });
    });

    $('.tabs-friend li').click(function () {

        var role = $(this).find('a').attr('arg');
        $.cookie('FriendTab', role, {
            path: '/',
            expires: 365

        });
    });

    $('.tabs-ev li').click(function () {
        var role = $(this).find('a').attr('arg');
        $.cookie('EventTab', role, {
            path: '/',
            expires: 365

        });
    });

    $('.tabs-date li').click(function () {

        var role = $(this).find('a').attr('arg');
        $.cookie('DateTab', role, {
            path: '/',
            expires: 365

        });
    });
 

    $('body').removeClass('noJS');

    /* Carousel
	-------------------------------------------------- */
    $('.users .carousel').slick({
        slide: '.slide',
        dots: false,
        infinite: true,
        speed: 300,
        slidesToShow: 8,
        slidesToScroll: 1,
        touchMove: true
    });
    $('.places-vip .carousel').slick({
        slide: '.slide',
        dots: false,
        infinite: true,
        speed: 300,
        slidesToShow: 5,
        slidesToScroll: 1,
        touchMove: true
    });

    $('.info .carousel').slick({
        slide: '.slide',
        dots: false,
        infinite: true,
        speed: 300,
        slidesToShow: 2,
        slidesToScroll: 1,
        touchMove: true
    });

    $('.calendar .carousel').slick({
        slide: '.slide',
        dots: false,
        infinite: true,
        speed: 300,
        slidesToShow: 1,
        slidesToScroll: 1,
        touchMove: true
    });

    /* Form styler
	-------------------------------------------------- */
    $('input:not(.download-img, .dialog-img), select').styler({
        selectSearch: false,
        fileBrowse: "Обзор",
        filePlaceholder: "Выберите файл"
    });

    $('input[type="file"].download-img').styler({
        fileBrowse: "Выбрать",
        filePlaceholder: "Выберите фотографию",
        onFormStyled: function () {
            $('input[type="file"].download-img').css('height', '30px');
            $('.jq-file.download-img .jq-file__browse').after('<div class="jq-file__remove">Удалить</div>');
            $('.jq-file.download-img').append('<div class="jq-file__preview"></div>');
            $('.jq-file.download-img').append('<div class="jq-file__text"></div>');
        }
    });

    $('input[type="file"].dialog-img').styler({
        fileBrowse: "Выбрать",
        filePlaceholder: "Выберите фотографию",
        onFormStyled: function () {
            $('.jq-file.dialog-img').append('<div class="jq-file__preview"></div>');
            $('.jq-file.dialog-img').append('<div class="jq-file__text"></div>');
        }
    });

    $('input[type="file"].download-img, input[type="file"].dialog-img').on('change', function (e) {
        var image = loadImage(
		    e.target.files[0],
		    function (img) {
		        //console.log(img.readAsBinaryString());
		        $(e.target).siblings('.jq-file__preview').html(img);
		        $(e.target).siblings('.jq-file__text').html('Фотография успешно<br>загружена');
		    },
		    { maxWidth: 150 } // Options
		);
        //console.log(image.readAsBinaryString());
/*
        image.onload = function (event) {
           
            //console.log("Содержимое файла: " + image);
        }
*/
    });

    $('.jq-file__remove').on('click', function (e) {
        var control = $(this).siblings('input[type="file"].download-img');
        control.replaceWith(control = control.clone(true));
        $(this).closest('.jq-file').removeClass('changed');
        $(this).siblings('.jq-file__name').html('Выберите фотографию');
        $(this).siblings('.jq-file__preview').html('');
        $(this).siblings('.jq-file__text').html('');
    });

    $('.friend-list').perfectScrollbar({
        wheelSpeed: 20,
        wheelPropagation: true,
        minScrollbarLength: 20,
        includePadding: true
    });

    $('.jq-selectbox__dropdown ul').each(function () {
        $(this).css({
            width: $(this).parent('.jq-selectbox__dropdown').width() + 20
        }).perfectScrollbar({
            wheelSpeed: 20,
            wheelPropagation: true,
            minScrollbarLength: 20,
            includePadding: true
        });
    });

    $('input[type="password"]').hidePassword(true);
    //Fix Opera
    $('.hideShowPassword-wrapper').css({
        display: 'block'
    });

    /* Accordion
	-------------------------------------------------- */
    $('.accordion').accordion({
        collapsible: true,
        heightStyle: "content"
    });

    /* Modals
	-------------------------------------------------- */
    $("#dialog-login").dialog({
        autoOpen: false,
        width: 340,
        modal: true,
        closeText: "Закрыть",
        resizable: false,
        draggable: true,
        dialogClass: "dialog-login dialog-fixed",
        position: {
            my: "center center"
        },
        open: function (event, ui) {
            $('.dialog-fixed').css('position', 'fixed');
            $('.ui-widget-overlay').bind('click', function () {
                $('#' + event.target.id).dialog('close');
            })
        }
    });

    $('#login').on('click', function (e) {
        e.preventDefault();

        $("#dialog-login").dialog("open");
    });

    $("#dialog-map").dialog({
        autoOpen: false,
        width: 560,
        modal: true,
        closeText: "Закрыть",
        resizable: false,
        draggable: true,
        dialogClass: "dialog-map dialog-fixed",
        position: {
            my: "center center"
        },
        open: function (event, ui) {
            $('.dialog-fixed').css('position', 'fixed');
            $('.ui-widget-overlay').bind('click', function () {
                $('#' + event.target.id).dialog('close');
            })
        }
    });

    $('.address').on('click', function (e) {
        e.preventDefault();

        createMap($(this).html(), $(this).data('title'));
        $("#dialog-map").dialog("open");
    });

    $("#dialog1").dialog({
        autoOpen: true,
        width: 520,
        modal: true,
        closeText: "Закрыть",
        resizable: false,
        draggable: true,
        dialogClass: "dialog-fixed",
        position: {
            my: "center center"
        },
        open: function (event, ui) {
            $('.dialog-fixed').css('position', 'fixed');
            $('.ui-widget-overlay').bind('click', function () {
                $('#' + event.target.id).dialog('close');
            })
        }
    });

    $("#dialog2").dialog({
        autoOpen: true,
        width: 520,
        modal: true,
        closeText: "Закрыть",
        resizable: false,
        draggable: true,
        dialogClass: "dialog-fixed",
        position: {
            my: "center center"
        },
        open: function (event, ui) {
            $('.dialog-fixed').css('position', 'fixed');
            $('.ui-widget-overlay').bind('click', function () {
                $('#' + event.target.id).dialog('close');
            })
        }
    });

    $("#dialog-message").dialog({
        autoOpen: false,
        width: 480,
        modal: true,
        closeText: "Закрыть",
        resizable: false,
        draggable: true,
        dialogClass: "dialog-fixed",
        position: {
            my: "center center"
        },
        open: function (event, ui) {
            $('.dialog-fixed').css('position', 'fixed');
            $('.ui-widget-overlay').bind('click', function () {
                $('#' + event.target.id).dialog('close');
            })
        }
    });

    $('#send-message').on('click', function (e) {
        e.preventDefault();

        $("#dialog-message").dialog("open");
    });

    $("#dialog-meet").dialog({
        autoOpen: false,
        width: 660,
        modal: true,
        closeText: "Закрыть",
        resizable: false,
        draggable: true,
        dialogClass: "dialog-meet dialog-fixed",
        position: {
            my: "center center"
        },
        open: function (event, ui) {
            $('.dialog-fixed').css('position', 'fixed');
            $('.ui-widget-overlay').bind('click', function () {
                $('#' + event.target.id).dialog('close');
            })
        }
    });

    $('#send-meet').on('click', function (e) {
        e.preventDefault();

        $("#dialog-meet").dialog("open");
    });

    $("#dialog-photo").dialog({
        autoOpen: false,
        width: 520,
        modal: true,
        closeText: "Закрыть",
        resizable: false,
        draggable: true,
        dialogClass: "dialog-photo dialog-fixed",
        position: {
            my: "center center"
        },
        open: function (event, ui) {
            $('.dialog-fixed').css('position', 'fixed');
            $('.ui-widget-overlay').bind('click', function () {
                $('#' + event.target.id).dialog('close');
            })
        }
    });

    $('#add-photos').on('click', function (e) {
        e.preventDefault();
        $("#dialog-photo").dialog("open");
    });

    /* Maps
	-------------------------------------------------- */
    var myMap,
		myCollection;

    if ($('#map').length > 0) {
        ymaps.ready(init);
    }

    function init() {
        myMap = new ymaps.Map('map', {
            center: [57.767265, 40.925358],
            zoom: 10
        });

        myCollection = new ymaps.GeoObjectCollection();
    }

    function createMap(address, title) {
        $('#map').html();
        myCollection.removeAll();

        var marker = ymaps.geoQuery(ymaps.geocode(address)),
			coords,
			myPlacemark1;

        marker.then(function () {
            coords = marker.get(0).geometry.getCoordinates();

            myMap.setCenter(coords);

            myPlacemark1 = new ymaps.Placemark(coords, {
                hintContent: title
            });

            myCollection.add(myPlacemark1);

            myMap.geoObjects.add(myCollection);

        }, function () {
            alert('Произошла ошибка.');
        });
    }

    /* Photos
	-------------------------------------------------- */
    $('.photos-item').magnificPopup({
        type: 'image',
        closeMarkup: '<div class="close-mfp">Закрыть</div>',
        gallery: {
            enabled: true,
            tPrev: '',
            tNext: ''
        },
        image: {
            markup: '<div class="mfp-figure">' +
						'<div class="mfp-title"></div>' +
						'<div class="mfp-close"></div>' +
			            '<div class="mfp-img"></div>' +
			          '</div>',
            titleSrc: function (item) {
                return "Просмотр фотографий";
            }
        }
    });

    $(document).on('click', '.close-mfp', function (e) {
        $.magnificPopup.close();
    });

    /* Tabs
	-------------------------------------------------- */
    
    if ($('.tabs-profile li').length) {
        var cook = $.cookie('ProfileTab');
        if (cook && cook.length) {

            $('.tabs-profile').tabs({ active: parseInt(cook.replace('col', '')-1) });
            
        } else {
            $('.tabs-profile').tabs({active:0});
        }

    }
    else if ($('.tabs-friend li').length) {
        initFriendTab();
    }
    else if ($('.tabs-date li').length) {
            var cook = $.cookie('DateTab');
            if (cook && cook.length) {

                $('.tabs-date').tabs({ active: parseInt(cook.replace('col', '') - 1) });

            } else {
                $('.tabs-date').tabs({ active: 0 });
            }

        }
    else if ($('.tabs-ev li').length) {
        initEventTab();

    }
    else if ($('.tabs-meet li').length) {
            var cook = $.cookie('MeetTab');
            if (cook && cook.length) {

                $('.tabs-meet').tabs({ active: parseInt(cook.replace('col', '') - 1) });

            } else {
                $('.tabs-meet').tabs({ active: 0 });
            }

    }

        else if (document.location.hash == "#place") {
            $('.tabs').tabs({ active: 1 });
    }
    else {
        $('.tabs').tabs({ active: 0 });
    }

    

    /* 
	-------------------------------------------------- */
    $('.order-checks-item').on('hover', function (e) {
        var content = $(this).data("info");
        $('.order-info-content:visible').html(content);
    });

    $('#more-photo').on('click', function (e) {
        e.preventDefault();

        var elements = $('<div class="row"><input type="file" name="file4" class="dialog-img"><input type="file" name="file5" class="dialog-img"><input type="file" name="file6" class="dialog-img"></div>');
        var elements1 = $('<div class="row"><input type="file" name="file7" class="dialog-img"><input type="file" name="file8" class="dialog-img"><input type="file" name="file9" class="dialog-img"></div>');
        if (!$('input[name="file4"]').length) {
            $('#dialog-photo .row:not(:last-of-type)').after(elements);
            $('.content-link').hide();
        } else {
            //$('#dialog-photo .row:not(:last-of-type)').after(elements1);
            
        }
        
        $('#dialog-photo .row > input[type="file"].dialog-img').each(function (e) {
            var el = $(this);
            el.styler({
                fileBrowse: "Выбрать",
                filePlaceholder: "Выберите фотографию",
                onFormStyled: function () {
                    el.after('<div class="jq-file__preview"></div>');
                    el.after('<div class="jq-file__text"></div>');
                }
            });
            el.on('change', function (e) {
                var fileReader =  loadImage(
				    e.target.files[0],
				    function (img) {

				        
				        $(e.target).siblings('.jq-file__preview').html(img);
				        $(e.target).siblings('.jq-file__text').html('Фотография успешно<br>загружена');
				        
				    },
				    { maxWidth: 150 } // Options
				);
                
               
            });
        });
    });
});

$(window).load(function () {

});

function afterLogin() {
    $('#dialog-login #dialog-login input[type="checkbox"]').styler();
}
function afterRestore() {
    $('a[arg="login"]').click(function() {
        $.get('/Profile/Login', {}, function(d) {
            $('#dialog-login').html(d);
            $('#dialog-login #dialog-login input[type="checkbox"]').styler();
        });

    });

}