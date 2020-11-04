/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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