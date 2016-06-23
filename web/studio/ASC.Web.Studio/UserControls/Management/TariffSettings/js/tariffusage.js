/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

        PhoneController.Init(jq(".text-edit-phone"), CountriesManager.countriesList, ["US"]);

        initMainProperties();
        initSlider();
    };

    var initMainProperties = function () {
        _tariffsMaxUsers = jq(".tariff-user-descr-item").map(function (i, item) {
            return parseInt(jq(item).attr("data-users"));
        }).toArray();


        _maxTariff = _tariffsMaxUsers[_tariffsMaxUsers.length - 1];
        _defaultTariff = jq(".tariff-slider-container").attr("data-default");
        _minTariff = jq(".tariff-slider-container").attr("data-min");

        for (var i = 1; i <= _maxTariff; i++) {
            _sliderVals.push(i);
            var curClass = '';
            if (_tariffsMaxUsers.indexOf(i) != -1)
                curClass = "withVertLine";

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

        sliderHandle.append("<div class=\"ui-slider-handle-text\">" + curTariffMaxUsers + "</div><div class=\"ui-slider-handle-q\"></div>");
        sliderHandle.children(".ui-slider-handle-q").append(jq(".tariff-user-question:first"));
        if (_defaultTariff / _sliderVals.length < 0.3) {
            jq(".tariff-user-question").addClass("tariff-user-question-right");
        }

        selectUserCount(sliderHandle, curTariffMaxUsers);
    };

    var onSliderStop = function (event, ui) {
        var curTariffMaxUsers = getTariffMaxUsersByCurUsrCnt(ui.value);

        slideExt(null,
            {
                handle: ui.handle,
                value: _maxTariff < ui.value ? _sliderVals[_sliderVals.length - 1] : curTariffMaxUsers,
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
            width: Math.floor(parseInt(jq("#pricingPlanSlider").css("width")) / (_maxTariff + 10)),
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
        jq(".tariff-user-warn-max, .tariff-request-panel").toggle(userMaxWarn);
        jq(".tariffs-panel, .see-full-price, #currencyPanel").toggle(!userMaxWarn);
        if (userMinWarn || userMaxWarn) {
            jq(".tariff-user-descr-item").hide();
        }
    };

    var clickOnBuy = function () {
        if (!jq("#buyRecommendationDialog").length) {
            return true;
        }

        jq("#buyRecommendationOk").attr("href", jq(this).attr("href"));

        StudioBlockUIManager.blockUI("#buyRecommendationDialog", 550, 300, 0);
        PopupKeyUpActionProvider.EnterAction = "location.href = jq(\"#buyRecommendationOk\").attr(\"href\");";
        PopupKeyUpActionProvider.CloseDialogAction = "TariffSettings.dialogRecommendationClose();";

        return false;
    };

    var hideBuyRecommendation = function (obj) {
        var dontDisplay = jq(obj).is(":checked");
        TariffUsageController.SaveHideRecommendation(dontDisplay,
            function (result) {
                if (result.error != null) {
                    toastr.error(result.error.Message);
                    return;
                }
            });
    };

    var dialogRecommendationClose = function () {
        if (jq("#buyRecommendationDisplay").is(":checked")) {
            jq("#buyRecommendationDialog").remove();
        }
    };

    var showDowngradeDialog = function () {
        var tariff = jq(".tariff-item:visible");
        var quotaActiveUsers = tariff.attr("data-users");
        var quotaStorageSize = tariff.attr("data-storage");
        jq("#downgradeUsers").html(quotaActiveUsers);
        jq("#downgradeStorage").html(quotaStorageSize);

        StudioBlockUIManager.blockUI("#tafirrDowngradeDialog", 450, 300, 0);
    };

    var requestTariff = function () {
        var fname = jq(".text-edit-fname").val().trim();
        var lname = jq(".text-edit-lname").val().trim();
        var title = jq(".text-edit-title").val().trim();
        var email = jq(".text-edit-email").val().trim();
        var phone = jq(".text-edit-phone").val().trim();
        var ctitle = jq(".text-edit-ctitle").val().trim();
        var csize = jq(".text-edit-csize").val().trim();
        var site = jq(".text-edit-site").val().trim();
        var message = jq(".text-edit-message").val().trim();
        if (!fname.length || !email.length || !message.length || !phone.length || !ctitle.length || !csize.length || !site.length) {
            toastr.error(ASC.Resources.Master.Resource.ErrorEmptyField);
            return;
        }

        TariffUsageController.RequestTariff(fname, lname, title, email, phone, ctitle, csize, site, message,
            function (result) {
                if (result.error != null) {
                    toastr.error(result.error.Message);
                    return;
                }
                toastr.success(ASC.Resources.Master.Resource.SendTariffRequest);
            });
    };

    return {
        init: init,

        clickOnBuy: clickOnBuy,

        showDowngradeDialog: showDowngradeDialog,
        hideBuyRecommendation: hideBuyRecommendation,
        dialogRecommendationClose: dialogRecommendationClose,

        requestTariff: requestTariff,
    };
};

jq(function () {
    TariffSettings.init();

    jq(".tariffs-panel").on("click", ".tariffs-buy-action:not(.disable)", TariffSettings.clickOnBuy);

    jq(".tariff-user-warn-link").click(function () {
        TariffSettings.showDowngradeDialog();
        return false;
    });

    jq("#buyRecommendationDisplay").click(function () {
        TariffSettings.hideBuyRecommendation(this);
        return true;
    });

    jq(".tariff-request").click(TariffSettings.requestTariff);
});