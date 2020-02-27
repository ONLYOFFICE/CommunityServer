/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


/*
* --------------------------------------------------------------------
* jQuery-Plugin - sliderWithSections
* --------------------------------------------------------------------
*/

jQuery.fn.sliderWithSections = function(settings) {
    //accessible slider options
    var options = jQuery.extend({
        value: null,
        colors: null,
        values: null,
        defaultColor: '#E1E1E1',
        liBorderWidth: 1,
        sliderOptions: null,
        max: 0,
        marginWidth: 1,
        slide: function(e, ui) { }
    }, settings);


    //plugin-generated slider options (can be overridden)
    var sliderOptions = {
        step: 1,
        min: 0,
        orientation: 'horizontal',
        max: options.max,
        range: false, //multiple select elements = true
        slide: function(e, ui) {//slide function

            var thisHandle = jQuery(ui.handle);

            thisHandle.attr('aria-valuetext', options.values[ui.value]).attr('aria-valuenow', ui.value);

            if (ui.value != 0) {
                thisHandle.find('.ui-slider-tooltip .ttContent').html(options.values[ui.value]);
                thisHandle.removeClass("ui-slider-tooltip-hide");
            }
            else {
                thisHandle.addClass("ui-slider-tooltip-hide");
            }


            var liItems = jQuery(this).children('ol.ui-slider-scale').children('li');

            for (var i = 0; i < sliderOptions.max; i++) {
                if (i < ui.value) {
                    var color = options.colors != null && options.colors[i] ? options.colors[i] : 'transparent';
                    jQuery(liItems[i]).css('background-color', color);
                }
                else {
                    jQuery(liItems[i]).css('background-color', options.defaultColor);
                }
            }

            options.slide(e, ui);

        },

        value: options.value
    };

    //slider options from settings
    options.sliderOptions = (settings) ? jQuery.extend(sliderOptions, settings.sliderOptions) : sliderOptions;


    //create slider component div
    var sliderComponent = jQuery('<div></div>');

    var $tooltip = jQuery('<a href="#" tabindex="0" ' +
			'class="ui-slider-handle" ' +
			'role="slider" ' +
			'aria-valuenow="' + options.value + '" ' +
			'aria-valuetext="' + options.values[options.value] + '"' +
		'><span class="ui-slider-tooltip ui-widget-content ui-corner-all"><span class="ttContent"></span>' +
			'<span class="ui-tooltip-pointer-down ui-widget-content"><span class="ui-tooltip-pointer-down-inner"></span></span>' +
		'</span></a>')
		.data('handleNum', options.value)
		.appendTo(sliderComponent);
    sliderComponent.find('.ui-slider-tooltip .ttContent').html(options.values[options.value]);
    if (options.values[options.value] == "") {
        sliderComponent.children(".ui-slider-handle").addClass("ui-slider-tooltip-hide");
    }

    var scale = sliderComponent.append('<ol class="ui-slider-scale ui-helper-reset" role="presentation" style="width: 100%; height: 100%;"></ol>').find('.ui-slider-scale:eq(0)');

    //var widthVal = (1 / sliderOptions.max * 100).toFixed(2) + '%';
    var sliderWidth = jQuery(this).css('width').replace('px', '') * 1;
    var widthVal = ((sliderWidth - options.marginWidth * (sliderOptions.max - 1) - 2 * options.liBorderWidth * sliderOptions.max) / sliderOptions.max).toFixed(4);
    for (var i = 0; i <= sliderOptions.max; i++) {
        var style = (i == sliderOptions.max || i == 0) ? 'display: none;' : '';
        var liStyle = (i == sliderOptions.max) ? 'display: none;' : '';
        var color = 'transparent';

        if (i < options.value) {
            color = options.colors != null && options.colors[i] ? options.colors[i] : 'transparent';
        }
        else {
            color = options.defaultColor;
        }

        scale.append('<li style="left:' + leftVal(i, sliderWidth) + '; background-color:' + color + '; height: 100%; width:' + widthVal + 'px;' + liStyle + '"></li>');
    };

    function leftVal(i, sliderWidth) {
        var widthVal = ((sliderWidth - options.marginWidth * (sliderOptions.max - 1) - 2 * options.liBorderWidth * sliderOptions.max) / sliderOptions.max);
        return ((widthVal + 2 * options.liBorderWidth + options.marginWidth) * i).toFixed(4) + 'px';

    }
    //inject and return 
    sliderComponent.appendTo(jQuery(this)).slider(options.sliderOptions).attr('role', 'application');
    sliderComponent.find('.ui-tooltip-pointer-down-inner').each(function() {
        var bWidth = jQuery('.ui-tooltip-pointer-down-inner').css('borderTopWidth');
        var bColor = jQuery(this).parents('.ui-slider-tooltip').css('backgroundColor')
        jQuery(this).css('border-top', bWidth + ' solid ' + bColor);
    });

    return this;
}