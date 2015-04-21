/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


(function() {
    // init jQuery Datepicker
    if (jQuery && jQuery.datepicker) {
        jQuery.datepicker.setDefaults({
            //changeMonth: true,
            //changeYear: true,
            prevText: "",
            nextText: "",
            firstDay: ASC.Resources.Master.FirstDay,
            dateFormat: ASC.Resources.Master.DatepickerDatePattern,
            dayNamesMin: ASC.Resources.Master.DayNames,
            dayNamesShort: ASC.Resources.Master.DayNames,
            dayNames: ASC.Resources.Master.DayNamesFull,
            monthNamesShort: ASC.Resources.Master.MonthNames,
            monthNames: ASC.Resources.Master.MonthNamesFull
        });
    }

    // init API Manager
    ServiceManager.init(ASC.Resources.Master.ApiPath);
    ServiceFactory.init({
        responses: {
            isme: ASC.Resources.Master.ApiResponsesMyProfile
        },
        portaldatetime: {
            utcoffsettotalminutes: ASC.Resources.Master.TimezoneOffsetMinutes,
            displayname: ASC.Resources.Master.TimezoneDisplayName
        },
        names: {
            months: ASC.Resources.Master.MonthNamesFull,
            shortmonths: ASC.Resources.Master.MonthNames,
            days: ASC.Resources.Master.DayNamesFull,
            shortdays: ASC.Resources.Master.DayNames
        },
        formats: {
            datetime: ASC.Resources.Master.DateTimePattern,
            time: ASC.Resources.Master.TimePattern,
            date: ASC.Resources.Master.DatePattern
        },
        avatars: {
            small: ASC.Resources.Master.AvatarSmall,
            medium: ASC.Resources.Master.AvatarMedium,
            large: ASC.Resources.Master.AvatarLarge
        },
        supportedfiles: {
            imgs: ASC.Files.Utility.Resource.ExtsImagePreviewed,
            docs: ASC.Files.Utility.Resource.ExtsWebPreviewed
        }
    });
    Teamlab.init();

    // init LoadingBanner
    LoadingBanner.strLoading = ASC.Resources.Master.Resource.LoadingProcessing;
    LoadingBanner.strDescription = ASC.Resources.Master.Resource.LoadingDescription;

    // init image zoom
    StudioManager.initImageZoom();

    //for ie10 css
    if (jq.browser.msie && jq.browser.version == 10) {
        jq("body").addClass("ie10");
    }

    //settings preloaded
    jq.blockUI.defaults.css = {};
    jq.blockUI.defaults.overlayCSS = {};
    jq.blockUI.defaults.fadeOut = 0;
    jq.blockUI.defaults.fadeIn = 0;
    jq.blockUI.defaults.message = "<div class=\"loader-text-block\">" + ASC.Resources.Master.Resource.LoadingPleaseWait + "</div>";

    if (jq("#studio_sidePanel").length) {
        LeftMenuManager.init(StudioManager.getBasePathToModule(), jq(".menu-list .menu-item.sub-list, .menu-list>.menu-item.sub-list>.menu-sub-list>.menu-sub-item"));
        LeftMenuManager.restoreLeftMenu();

        jq(".support-link").on("click", function() {
            jq(".support .expander").trigger("click");
        });
    }
    // init page-menu actions
    LeftMenuManager.bindEvents();

    // init RenderPromoBar
    if (ASC.Resources.Master.SetupInfoNotifyAddress &&
        ASC.Resources.Master.IsAuthenticated == true &&
        ASC.Resources.Master.ApiResponsesMyProfile.response) {

        jq.getScript(
            [
                ASC.Resources.Master.SetupInfoNotifyAddress + 'promotions/get?',
                "userId=",
                ASC.Resources.Master.ApiResponsesMyProfile.response.id,
                "&language=",
                ASC.Resources.Master.CurrentCultureName,
                "&version=",
                ASC.Resources.Master.CurrentTenantVersion,
                "&tariff=",
                ASC.Resources.Master.TenantTariff,
                "&admin=",
                ASC.Resources.Master.IsAdmin,
                "&userCreated=",
                ASC.Resources.Master.ApiResponsesMyProfile.response.created,
                "&promo=",
                window.StudioSettings ? window.StudioSettings.ShowPromotions : ""
            ].join(""));
    }

    // init Tips
    if (ASC.Resources.Master.SetupInfoTipsAddress &&
        ASC.Resources.Master.IsAuthenticated == true &&
        ASC.Resources.Master.ApiResponsesMyProfile.response &&
        !ASC.Resources.Master.ApiResponsesMyProfile.response.isOutsider &&
        window.StudioSettings &&
        window.StudioSettings.ShowTips) {

        jq.getScript(
            [
                ASC.Resources.Master.SetupInfoTipsAddress + 'tips/get?',
                "userId=",
                ASC.Resources.Master.ApiResponsesMyProfile.response.id,
                "&tenantId=",
                ASC.Resources.Master.CurrentTenantId,
                "&page=",
                encodeURIComponent(window.location.pathname + window.location.search + window.location.hash),
                "&language=",
                ASC.Resources.Master.CurrentCultureName,
                "&admin=",
                ASC.Resources.Master.IsAdmin,
                "&productAdmin=",
                window.ProductSettings ? window.ProductSettings.IsProductAdmin : "",
                "&visitor=",
                ASC.Resources.Master.IsVisitor,
                "&userCreatedDate=",
                ASC.Resources.Master.ApiResponsesMyProfile.response.created,
                "&tenantCreatedDate=",
                ASC.Resources.Master.CurrentTenantCreatedDate
            ].join(""));
    }

    var studioUserProfileInfo = new PopupBox("pb_StudioUserProfileInfo", 320, 140, "tintLight", "borderBaseShadow", "",
        {
            apiMethodName: "Teamlab.getProfile",
            tmplName: "userProfileCardTmpl"
        });

    jq(".userLink").each(function() {
        var id = jq(this).attr("id");
        if (id != null && id != "") {
            studioUserProfileInfo.RegistryElement(id, "\"" + jq(this).attr("data-uid") + "\"");
        }
    });

    jq("#commonLogout").click(function() {
        if (typeof SmallChat != "undefined" && SmallChat.logoutEvent) {
            SmallChat.logoutEvent();
        }
        return true;
    });



    var isRetina = function () {
        if (window.devicePixelRatio > 1)
            return true;

        var mediaQuery = "(-webkit-min-device-pixel-ratio: 1.5),\
            (min--moz-device-pixel-ratio: 1.5),\
            (-o-min-device-pixel-ratio: 3/2),\
            (min-resolution: 1.5dppx),\
            (min-device-pixel-ratio: 1.5)";

        if (window.matchMedia && window.matchMedia(mediaQuery).matches)
            return true;
        return false;
    };

    if (isRetina() && jq.cookies.get("is_retina") == null) {
        jq.cookies.set("is_retina", true, { path: '/' });
    }


})();

jq("table.mainPageTable").tlBlock();

//hack for resizing filter
setTimeout("jq(window).resize()", 500);

jq(window).on("resize", function() {
    // hide all popup's
    jq(".studio-action-panel:not(.freeze-display)").hide();

    clearTimeout(jq.data(this, 'resizeFilterTimer'));
    jq.data(this, 'resizeFilterTimer', setTimeout(function() {
        jq("div.advansed-filter").each(function() {
            jq(this).advansedFilter('resize');
        });
    }, 50));

});

// init uvOptions
var uvOptions = {
    custom_fields: {
        "premium": ASC.Resources.Master.TenantIsPremium
    }
};