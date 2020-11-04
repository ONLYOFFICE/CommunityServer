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


var defineBodyMediaClass = function () {
    var $body = jq("body"),
               list = [
               { min: 0,    max: 1024, classname: 'media-width-0-1024' },
               { min: 0,    max: 1030, classname: 'media-width-0-1030' },
               { min: 0,    max: 1048, classname: 'media-width-0-1048' },
               { min: 0,    max: 1072, classname: 'media-width-0-1072' },
               { min: 0,    max: 1080, classname: 'media-width-0-1080' },
               { min: 0,    max: 1096, classname: 'media-width-0-1096' },

               { min: 0,    max: 1100, classname: 'media-width-0-1100' },
               { min: 0,    max: 1120, classname: 'media-width-0-1120' },
               { min: 0,    max: 1140, classname: 'media-width-0-1140' },
               { min: 0,    max: 1144, classname: 'media-width-0-1144' },
               { min: 0,    max: 1150, classname: 'media-width-0-1150' },
               { min: 0,    max: 1160, classname: 'media-width-0-1160' },
               { min: 0,    max: 1180, classname: 'media-width-0-1180' },

               { min: 0,    max: 1200, classname: 'media-width-0-1200' },
               { min: 0,    max: 1210, classname: 'media-width-0-1210' },
               { min: 1145, max: 0,    classname: 'media-width-1145-0' },
               { min: 0,    max: 1250, classname: 'media-width-0-1250' },
               { min: 0,    max: 1260, classname: 'media-width-0-1260' },
               { min: 0,    max: 1270, classname: 'media-width-0-1270' },

               { min: 0,    max: 1300, classname: 'media-width-0-1300' },
               { min: 0,    max: 1340, classname: 'media-width-0-1340' },
               { min: 0,    max: 1350, classname: 'media-width-0-1350' },
               { min: 0,    max: 1390, classname: 'media-width-0-1390' },

               { min: 0,    max: 1400, classname: 'media-width-0-1400' },
               { min: 0,    max: 1410, classname: 'media-width-0-1410' },
               { min: 0,    max: 1420, classname: 'media-width-0-1420' },
               { min: 0,    max: 1450, classname: 'media-width-0-1450' },
               { min: 0,    max: 1470, classname: 'media-width-0-1470' },

               { min: 0,    max: 1500, classname: 'media-width-0-1500' },
               { min: 0,    max: 1575, classname: 'media-width-0-1575' },
              
               { min: 0,    max: 1620, classname: 'media-width-0-1620' },
               { min: 0,    max: 1650, classname: 'media-width-0-1650' },
               { min: 1620, max: 0,    classname: 'media-width-1620-0' },


               ],

        width = jq("#studioPageContent").width() - jq(".mainPageTableSidePanel").width() + 240;

    if ($body.hasClass("sailfish")) {
        return;
    }


    for (var i = 0, n = list.length; i < n; i++) {
        if (width >= list[i].min && (list[i].max == 0 || width < list[i].max)) {
            if (!$body.hasClass(list[i].classname))
                $body.addClass(list[i].classname);
        } else {
            if ($body.hasClass(list[i].classname))
                $body.removeClass(list[i].classname);
        }
    }
    $body.removeClass('media-width-min');
};


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
    ServiceHelper.init(ASC.Resources.Master.ApiPath);
    ServiceFactory.init({
        responses: {
            isme: ASC.Resources.Master.ApiResponsesMyProfile
        },
        portaldatetime: {
            utcoffsettotalminutes: ASC.Resources.Master.CurrentTenantTimeZone.UtcOffset,
            displayname: ASC.Resources.Master.CurrentTenantTimeZone.DisplayName
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

    var isDesktop = jq("body").hasClass("desktop");

    // init RenderPromoBar
    if (ASC.Resources.Master.SetupInfoNotifyAddress &&
        ASC.Resources.Master.IsAuthenticated == true &&
        ASC.Resources.Master.ApiResponsesMyProfile.response && 
        ASC.Resources.Master.ShowPromotions) {

        Teamlab.getBarPromotions({}, isDesktop, {
            success: function(params, content) {
                try {
                    if (content) {
                        eval(content);
                    }
                } catch (e) {
                    console.error(e);
                }
            }
        });
    }

    var tipsWasClosed = false;
    if (window.sessionStorage)
        tipsWasClosed = !!window.sessionStorage.getItem("tipsWasClosed");

    // init Tips
    if (!tipsWasClosed &&
        ASC.Resources.Master.SetupInfoTipsAddress &&
        ASC.Resources.Master.IsAuthenticated == true &&
        ASC.Resources.Master.ApiResponsesMyProfile.response &&
        !ASC.Resources.Master.ApiResponsesMyProfile.response.isOutsider &&
        ASC.Resources.Master.ShowTips) {

        Teamlab.getBarTips({}, isDesktop, {
            success: function (params, content) {
                try {
                    if (content) {
                        eval(content);
                    }
                } catch (e) {
                    console.error(e);
                }
            }
        });
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



    /***workaround for media screen***/
    defineBodyMediaClass();
    /******/

    /***resizable left navigation menu***/
    var sidePanelMinW = 240,
        mainPageContentMinOuterW = // 711
               1024 // min-width for #studioPageContent
               - 24 * 2 // - padding for .mainPageLayout
               - sidePanelMinW // 240
               - 24 - 1;//  - 'padding-right' - 'border-width' for .mainPageTableSidePanel 

    var setResizableMaxWidth = function () {
        var sidePanelMaxW = jq("#studioPageContent").width()
                 - 24 * 2 // - padding for .mainPageLayout
                 - mainPageContentMinOuterW
                 - 24 - 1; // - 'padding-right' - 'border-width' for .mainPageTableSidePanel 

        jq(".mainPageTableSidePanel").resizable("option", "maxWidth", sidePanelMaxW);
    }


    if (jq(".mainPageTableSidePanel").length == 1) {
        jq(".mainPageTableSidePanel").resizable({
            maxWidth: sidePanelMinW,
            minWidth: sidePanelMinW,
            handles: 'e',
            start: function (event) {
                jq("body:not(.media-width-min)").addClass('media-width-min');
                jq(".popup_helper").hide();
            },
            resize: function (event, ui) {
                jq("#studio_sidePanel").css("width", ui.size.width);

                if (jq('body').hasClass('desktop')){
                    var columnWidth = jq("#studio_sidePanel").outerWidth();
                    jq("#studioPageContent").css("grid-template-columns", columnWidth + "px 1px 1fr");
                }

                jq(window).trigger("resizeSidePanel", [event, ui]);
            },
            create: function () {
            },
            stop: function (event, ui) {
            }
        });
        setResizableMaxWidth();
    }
    /******/



    jq(window).on("resize", function () {
        // hide all popup's
        jq(".studio-action-panel:not(.freeze-display)").hide();

        jq("body:not(.media-width-min)").addClass('media-width-min');

        clearTimeout(jq.data(this, 'resizeWinTimer'));
        jq.data(this, 'resizeWinTimer', setTimeout(function () {

            defineBodyMediaClass();

            /***resizable left navigation menu***/
            if (jq(".mainPageTableSidePanel").length == 1) {
                setResizableMaxWidth();
                if (jq("#studioPageContent").width() < jq(".mainPageTable.with-mainPageTableSidePanel").width() ||
                    jq(".mainPageTable.with-mainPageTableSidePanel .mainPageContent").outerWidth() < mainPageContentMinOuterW) {
                    jq("#studio_sidePanel").width(sidePanelMinW);
                    jq(".mainPageTableSidePanel").width(sidePanelMinW);
                }
            }
            /******/

            jq(window).trigger("resizeWinTimer", null);

            jq("table.mainPageTable").tlBlock('resize');


            jq("div.advansed-filter").each(function () {
                jq(this).advansedFilter('resize');
            });

        }, 51));

        clearTimeout(jq.data(this, 'resizeWinTimerWithMaxDelay'));
        jq.data(this, 'resizeWinTimerWithMaxDelay', setTimeout(function () {
            defineBodyMediaClass();

            jq(window).trigger("resizeWinTimerWithMaxDelay", null);

            jq("table.mainPageTable").tlBlock('resize');
        }, 91));
    });

    var navContent = jq("#studioPageContent nav > .nav-content");
    var navContentScrollPosition = navContent.scrollTop();
    var pageContent = jq("#studioPageContent main > .page-content");
    var pageContentScrollPosition = pageContent.scrollTop();

    var hideContextMenu = function() {
        pageContent.find(".studio-action-panel, .popup_helper, .advanced-selector-container").hide();
        jq("#studioPageContent main .menu-small.active").removeClass("active");
        jq("#studioPageContent main .entity-menu.active").removeClass("active");
        jq('#ui-datepicker-div, .asc-popupmenu').hide();
    };

    navContent.on("scroll", function() {
        var newPosition = navContent.scrollTop();
        if (navContentScrollPosition != newPosition) {
            navContentScrollPosition = newPosition;
            hideContextMenu();
        }
    });

    pageContent.on("scroll", function () {
        var newPosition = pageContent.scrollTop();
        if (pageContentScrollPosition != newPosition) {
            pageContentScrollPosition = newPosition;
            hideContextMenu();
        }
    });

})();


jq("table.mainPageTable").tlBlock();

//hack for resizing filter
setTimeout("jq(window).resize()", 500);

jq(window).one("resize", function () {
    clearTimeout(jq.data(this, 'resizeWinTimer'));
    jq.data(this, 'resizeWinTimer', setTimeout(function () {
        defineBodyMediaClass();
    }, 51));
});

// init uvOptions
var uvOptions = {
    custom_fields: {
        "premium": ASC.Resources.Master.TenantIsPremium
    }
};