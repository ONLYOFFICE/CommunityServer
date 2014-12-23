/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
                ASC.Resources.Master.SetupInfoNotifyAddress,
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
    if (ASC.Resources.Master.SetupInfoTipsAddress && window.StudioSettings && window.StudioSettings.ShowTips) {
        jq.getScript(
            [
                ASC.Resources.Master.SetupInfoTipsAddress,
                "userId=",
                ASC.Resources.Master.ApiResponsesMyProfile.response.id,
                "&tenantId=",
                ASC.Resources.Master.CurrentTenantId,
                "&page=",
                encodeURIComponent(window.location.pathname),
                "&hash=",
                encodeURIComponent(window.location.hash),
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
})();

jq("table.mainPageTable").tlBlock();
setTimeout("jq(window).resize()", 500); //hack for resizing filter

jq(window).bind("resize", function () {
    // hide all popup's
    jq(".studio-action-panel:not(.freeze-display)").hide();
});

// init uvOptions
var uvOptions = {
    custom_fields: {
        "premium": ASC.Resources.Master.TenantIsPremium
    }
};