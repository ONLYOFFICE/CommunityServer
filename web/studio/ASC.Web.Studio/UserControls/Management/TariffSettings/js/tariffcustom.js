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


var TariffSettings = new function () {
    var isInit = false;

    var _tariffsMaxUsers = [],
        _defaultTariff = 0,
        _maxTariff = 0,
        _minTariff = 0,
        _sliderVals = [],
        _sliderClasses = [];

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        jq.switcherAction("#switcherPayments", "#paymentsContainer");

        jq.dropdownToggle({
            switcherSelector: "#currencySelector",
            dropdownID: "currencyList",
            rightPos: true,
        });

        jq("#currencyHelpSwitcher").click(function () { jq(this).helper({BlockHelperID: "currencyHelp"}) });

        var country = jq(".text-edit-phone").attr("data-country");
        PhoneController.Init(jq(".text-edit-phone"), CountriesManager.countriesList, [country, "US"]);

        initMainProperties();
        initSlider();

        updateSelectedTariff("year");
        updatePrices(_defaultTariff);
        jq(".tarrifs-button-year").click(function() { updateSelectedTariff("year"); }); 
        jq(".tarrifs-button-month").click(function() { updateSelectedTariff("month"); }); 
    };

    function updateSelectedTariff (t) {
        if (t == "year") {
            jq(".tarrifs-button-year").addClass('green');
            jq(".tarrifs-button-year").removeClass("gray");
            jq(".tarrifs-button-month").removeClass('green');
            jq(".tarrifs-button-month").addClass("gray");
            jq(".text-edit-message").val("Year tariff");
        } else {
            jq(".tarrifs-button-month").addClass('green');
            jq(".tarrifs-button-month").removeClass("gray");
            jq(".tarrifs-button-year").removeClass('green');
            jq(".tarrifs-button-year").addClass("gray");
            jq(".text-edit-message").val("Month tariff");
        }
    }

    var initMainProperties = function () {
        _tariffsMaxUsers = jq(".tariff-user-descr-item").map(function (i, item) {
            return parseInt(jq(item).attr("data-users"));
        }).toArray();


        _maxTariff = _tariffsMaxUsers[_tariffsMaxUsers.length - 1] || 0;
        jq(".tariff-header").toggle(_maxTariff != 0);
        _defaultTariff = jq(".tariff-slider-container").attr("data-default");
        _minTariff = jq(".tariff-slider-container").attr("data-min");

        for (var i = 1; i <= _maxTariff; i++) {
            _sliderVals.push(i);
            var curClass = '';

            _sliderClasses.push(curClass);
        }

        for (var i = 1; i <= 10; i++) { // 10 steps (100px) for max zone ">50" users
            _sliderVals.push(_maxTariff + i);
            _sliderClasses.push("darkGrey" + (i == 10 ? " rightRadius" : ""));
        }
    };

    var slideRefreshText = function (slideHandle, curUsrCount) {
        slideHandle.children(".ui-slider-handle-text")
            .text(_maxTariff < curUsrCount
                ? (">" + _maxTariff)
                : curUsrCount);
    };

    var slideExt = function (e, ui) {
        var $thisHandle = jq(ui.handle),
            liItems = $thisHandle.parent().children('ol.ui-slider-scale').children('li'),
            opts = $thisHandle.data("options");

        $thisHandle.attr('aria-valuenow', ui.value);

        for (var i = 0, n = liItems.length; i < n; i++) {
            if (i < ui.value) {
                jq(liItems[i]).addClass(opts.baseCssClass);
            } else {
                jq(liItems[i]).removeClass(opts.baseCssClass);
            }
        }

        if (e != null) {
            opts.value = ui.value;
            $thisHandle.data("options", opts);

            slideRefreshText($thisHandle, ui.value);
        } else {
            selectUserCount(jq(ui.handle), ui.curTariffMaxUsers);
        }
    };

    var onSliderCreate = function (event, ui) {
        var sliderHandle = jq("#pricingPlanSlider .ui-slider a.ui-slider-handle:first");
        var curTariffMaxUsers = getTariffMaxUsersByCurUsrCnt(_defaultTariff);

        sliderHandle.append("<div class=\"ui-slider-handle-text\">" + _defaultTariff + "</div><div class=\"ui-slider-handle-q\"></div>");
        sliderHandle.children(".ui-slider-handle-q").append(jq(".tariff-user-question:first"));
        if (_defaultTariff / _sliderVals.length < 0.3) {
            jq(".tariff-user-question").addClass("tariff-user-question-right");
        }

        selectUserCount(sliderHandle, curTariffMaxUsers);
    };

    function formatNumber(num) {
        return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1 ');
    }

    function updatePrices(value) {
        jq('.tariffs-body-month .price-string').html((290) + '<span class="tariff-price-cur">&#x20BD;</span>');
        jq('.tariffs-body-month .tariffs-price').html(formatNumber((290 * value)) + '<span class="tariff-price-cur">&#x20BD;</span>');

        jq('.tariffs-body-year .price-string').html((175) + '<span class="tariff-price-cur">&#x20BD;</span>');
        jq('.tariffs-body-year .tariffs-price').html(formatNumber((175 * 12 * value)) + '<span class="tariff-price-cur">&#x20BD;</span>');

        jq('.text-edit-csize').val(value>_maxTariff ? (">" + _maxTariff) : ("" + value));
    }

    var onSliderStop = function (event, ui) {
        var curTariffMaxUsers = getTariffMaxUsersByCurUsrCnt(ui.value);

        updatePrices(ui.value);

        slideExt(null,
            {
                handle: ui.handle,
                value: _maxTariff < ui.value ? _sliderVals[_sliderVals.length - 1] : ui.value,
                curTariffMaxUsers: curTariffMaxUsers
            });

    };

    var onSliderStart = function (event, ui) {
        jq(ui.handle).children(".ui-slider-handle-q").remove();
    };

    var initMainSliderOpts = function () {
        return {
            values: _sliderVals,
            defaultClasses: _sliderClasses,
            width: parseInt(jq("#pricingPlanSlider").css("width")) / (_maxTariff + 10),
            height: '6px',

            sliderOptions: {
                baseCssClass: 'blue',
                step: 1,
                animate: 0,
                min: 1,
                max: _sliderVals.length,
                orientation: 'horizontal',
                range: false,
                value: _defaultTariff,

                create: onSliderCreate,

                start: onSliderStart,
                slide: slideExt,
                stop: onSliderStop
            }
        };
    };

    var initSlider = function () {
        var $this = jq("#pricingPlanSlider"),
            options = initMainSliderOpts(),
            sliderComponent = jq('<div></div>');

        jq(['<a tabindex="0" class="ui-slider-handle" role="slider" aria-valuenow="',
                options.sliderOptions.value,
                '"></a>'
        ].join(''))
            .data("options", options.sliderOptions)
            .appendTo(sliderComponent);

        var scale = sliderComponent.append('<ol class="ui-slider-scale" role="presentation" style="width: 100%;' +
            ' height: ' + (options.height != null ? options.height : "100%") + ';"></ol>')
            .find('.ui-slider-scale:eq(0)'),
            liClass = '';

        for (var i = 0, n = options.values.length; i < n; i++) {
            liClass = (i < options.sliderOptions.value) ? options.sliderOptions.baseCssClass : '';

            if (i == 0) {
                liClass = "leftRadius " + options.sliderOptions.baseCssClass;
            }
            if (options.defaultClasses != null && typeof (options.defaultClasses[i]) !== "undefined" && options.defaultClasses[i] != "") {
                liClass += " " + options.defaultClasses[i];
            }


            scale.append(
                ['<li class="',
                liClass,
                '" style="left:',
                options.width * i, 'px;',
                ' height: ',
                (options.height != null ? options.height : "100%"),
                '; width:',
                options.width, "px",
                ';"></li>'
                ]
                .join(''));
        }

        //inject and return
        sliderComponent.appendTo($this).slider(options.sliderOptions).attr('role', 'application');

       jq(".tariff-request-panel").show();

    };

    var getTariffMaxUsersByCurUsrCnt = function (curUserCount) {
        for (var i = 0, n = _tariffsMaxUsers.length; i < n; i++) {
            if (curUserCount <= _tariffsMaxUsers[i]) {
                return _tariffsMaxUsers[i];
            }
        }
        return curUserCount;
    };

    var selectUserCount = function (handle, curTariffMaxUsers) {
        var userMinWarn = curTariffMaxUsers < _minTariff,
            userMaxWarn = _maxTariff < curTariffMaxUsers;

        jq(".tariff-user-descr-item, .tariff-item").hide();
        jq(".tariff-user-descr-item[data-users=\"" + curTariffMaxUsers + "\"], .tariff-item[data-users=\"" + curTariffMaxUsers + "\"]").show();


        jq("#pricingPlanSlider").toggleClass("warn-slider", userMinWarn);
        jq(".tariff-user-warn-min").toggle(userMinWarn);
        jq(".tariff-user-warn-max").toggle(userMaxWarn);
        jq(".tariffs-panel, .see-full-price, #currencyPanel").toggle(!userMaxWarn);
    };

    var requestTariff = function () {
        var fname = jq(".text-edit-fname").val().trim();
        var lname = jq(".text-edit-lname").val().trim();
        var title = (jq(".text-edit-title").val() || "").trim();
        var email = jq(".text-edit-email").val().trim();
        var phone = jq(".text-edit-phone").val().trim();
        var ctitle = jq(".text-edit-ctitle").val().trim();
        var csize = jq(".text-edit-csize").val().trim();
        var site = (jq(".text-edit-site").val() || "").trim();
        var message = (jq(".text-edit-message").val() || "").trim();
        if (!fname.length || !email.length || !phone.length || !ctitle.length || !csize.length) {
            toastr.error(ASC.Resources.Master.Resource.ErrorEmptyField);
            return;
        }
        if (!site.length && jq(".text-edit-site").is(":visible")
            || !message.length && jq(".text-edit-message").is(":visible")) {
            toastr.error(ASC.Resources.Master.Resource.ErrorEmptyField);
            return;
        }

        TariffUsageController.RequestTariff(fname, lname, title, email, phone, ctitle, csize + " / " + message, site, message,
            function (result) {
                if (result.error != null) {
                    toastr.error(result.error.Message);
                    return;
                }
                toastr.success(ASC.Resources.Master.Resource.SendTariffRequest1);
            });
    };

    return {
        init: init,

        requestTariff: requestTariff,
    };
};

jq(function () {
    TariffSettings.init();

    jq(".tariff-request").click(TariffSettings.requestTariff);
});