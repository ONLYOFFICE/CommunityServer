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


/*******************************************************************************/
if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.Projects === "undefined")
    ASC.Projects = {};

ASC.Projects.Base = (function () {
    var isFirstLoad = true;
    var cookiePagination = "";

    var $filterContainer = jq("#filterContainer");
    var $commonListContainer = jq("#CommonListContainer");
    var $commonPopupContainer = jq("#commonPopupContainer");
    var $loader = jq(".mainPageContent .loader-page");
    var $taskList = jq(".taskList");
    var $noContentBlock = jq(".noContentBlock");


    var showLoader = function () {
        if (this.isFirstLoad) {
            $filterContainer.hide();
            $commonListContainer.hide();
            $loader.show();
        } else {
            LoadingBanner.displayLoading(true);
        }
    };

    var hideLoader = function (groupPanel) {
        var self = this;
        if (self.isFirstLoad) {
            
            self.isFirstLoad = false;
            $loader.hide();
            $filterContainer.show();
            $commonListContainer.show();

            if (groupPanel) {
                ScrolledGroupMenu.resizeContentHeaderWidth(groupPanel);
            }
            if (typeof(ASC.Projects.ProjectsAdvansedFilter.filter) != "undefined") {
                ASC.Projects.ProjectsAdvansedFilter.resize();
            }
        } else {
            LoadingBanner.hideLoading();
        }
    };

    var checkElementNotFound = function (str) {
        if (location.hash.indexOf("elementNotFound") > 0) {
            ASC.Projects.Common.displayInfoPanel(str, true);
        }
    };

    var setDocumentTitle = function (module) {
        document.title = jq.format("{0} - {1}", module, ASC.Projects.Resources.ProjectsJSResource.ProductName);
    };

    var showCommonPopup = function (tmplName, width, height, position) {
        $commonPopupContainer.find(".commonPopupContent").empty().append(jq.tmpl(tmplName, {}));
        $commonPopupContainer.find(".commonPopupHeaderTitle").empty().text($commonPopupContainer.find(".hidden-title-text").text());

        StudioBlockUIManager.blockUI($commonPopupContainer, width, height, position);
    };

    var clearTables = function () {
        ASC.Projects.PageNavigator.clear();
        $taskList.empty();
        jq("#tableListProjects, #milestonesList, .taskList, #discussionsList, #timeSpendsList, [id$='EmptyScreenForFilter'], [id^='emptyList']").hide();
        jq("#totalTimeText").remove();
    };

    return {
        clearTables: clearTables,
        checkElementNotFound: checkElementNotFound,
        isFirstLoad: isFirstLoad,

        showLoader: showLoader,
        hideLoader: hideLoader,

        cookiePagination: cookiePagination,

        setDocumentTitle: setDocumentTitle,
        showCommonPopup: showCommonPopup,

        $commonListContainer: $commonListContainer,
        $commonPopupContainer: $commonPopupContainer,

        $noContentBlock: $noContentBlock
    };
})();

ASC.Projects.PageNavigator = (function () {
    var $smallRowCounter = jq("#countOfRowsSmall");
    var $bigRowCounter = jq("#countOfRowsBig");
    var $tableForNavigation = jq("#tableForNavigation");
    var $simplePageNavigator = jq(".simplePageNavigator");

    var entryCountOnPage = 0;
    var currentPage = 0;
    var cookiePaginationKey;
    var self;
    var pgNavigator = {};
    var wasUpdate = false;

    var init = function (obj, small) {
        self = this;
        cookiePaginationKey = obj.cookiePagination;
        var pagination = getPaginationCookie();

        if (pagination != null) {
            self.entryCountOnPage = pagination.countOnPage;
            self.currentPage = pagination.currentPage || 0;
        } else {
            self.entryCountOnPage = cookiePaginationKey === "discussionsKeyForPagination" ? 10 : ASC.Projects.Master.EntryCountOnPage;
            self.currentPage = 0;
        }

        self.entryCountOnPage = parseInt(self.entryCountOnPage);
        self.currentPage = parseInt(self.currentPage);

        if (!wasUpdate) {
            $smallRowCounter.tlCombobox();
            $bigRowCounter.tlCombobox();
            wasUpdate = true;
        }

        var rowCounter;

        if (small) {
            rowCounter = $smallRowCounter;
            $bigRowCounter.tlCombobox("hide");
        } else {
            rowCounter = $bigRowCounter;
            $smallRowCounter.tlCombobox("hide");
        }

        rowCounter.parent().attr("data-value", self.entryCountOnPage);
        rowCounter.siblings(".combobox-title").children(".combobox-title-inner-text").attr("title", self.entryCountOnPage).text(self.entryCountOnPage);
        var items = rowCounter.siblings(".combobox-wrapper").find(".option-item");
        items.each(function(index, item) {
            if (jq(item).attr("data-value") != self.entryCountOnPage) {
                jq(item).removeClass("selected-item");
            } else {
                jq(item).addClass("selected-item");
            }
        });
        rowCounter.tlCombobox("show");

        rowCounter.on("change.changeCountOfRows", function () {
            changeCountOfRows(this.value, obj);
        });

        if (!self.pgNavigator.length) {
            self.pgNavigator = new ASC.Controls.PageNavigator.init(
                "ASC.Projects.PageNavigator.pgNavigator", 
                "#divForTaskPager",
                self.entryCountOnPage,
                parseInt(ASC.Projects.Master.VisiblePageCount),
                self.currentPage + 1,
                ASC.Projects.Resources.ProjectsJSResource.PreviousPage, 
                ASC.Projects.Resources.ProjectsJSResource.NextPage);
            self.pgNavigator.NavigatorParent = '#divForTaskPager';
        } else {
            self.pgNavigator.EntryCountOnPage = self.entryCountOnPage;
            self.pgNavigator.CurrentPageNumber = self.currentPage + 1;
        }
        
        self.pgNavigator.changePageCallback = function (page) {
            LoadingBanner.displayLoading();
            self.currentPage = page - 1;
            obj.getData(true);
            setPaginationCookie();
        };
        
    };

    var setPaginationCookie = function () {
        if (cookiePaginationKey && cookiePaginationKey != "") {
            var cookie = {
                countOnPage: self.entryCountOnPage,
                currentPage: self.currentPage
            };
            jq.cookies.set(cookiePaginationKey, cookie);
        }
    };

    var changeCountOfRows = function (newValue, obj) {
        if (isNaN(newValue)) {
            return;
        }
        var newCountOfRows = newValue * 1;
        self.entryCountOnPage = newCountOfRows;
        self.currentPage = 0;
        self.pgNavigator.EntryCountOnPage = newCountOfRows;

        obj.showLoader();
        obj.getData(false);
    };

    var renderSimplePageNavigator = function (nav) {
        var navig = nav || $simplePageNavigator;
        navig.html("");
        var $simplePn = jq("<div></div>");
        var lengthOfLinks = 0;
        if ($tableForNavigation.find(".pagerPrevButtonCSSClass").length != 0) {
            lengthOfLinks++;
            $tableForNavigation.find(".pagerPrevButtonCSSClass").clone().appendTo($simplePn);
        }
        if ($tableForNavigation.find(".pagerNextButtonCSSClass").length != 0) {
            lengthOfLinks++;
            if (lengthOfLinks === 2) {
                jq("<span style='padding: 0 8px;'>&nbsp;</span>").clone().appendTo($simplePn);
            }
            $tableForNavigation.find(".pagerNextButtonCSSClass").clone().appendTo($simplePn);
        }

        if ($simplePn.children().length != 0) {
            $simplePn.appendTo(navig);
            navig.show();
        }
        else {
            navig.hide();
        }
    };

    var update = function (filterCount, nav) {
        jq("#totalCount").text(filterCount);
        self.pgNavigator.drawPageNavigator(self.currentPage + 1, filterCount);
        setPaginationCookie();

        if (filterCount) {
            $tableForNavigation.show();
        }
        renderSimplePageNavigator(nav);
    };

    var unbindListEvents = function () {
        $smallRowCounter.off("change.changeCountOfRows");
        $bigRowCounter.off("change.changeCountOfRows");
    };

    var getPaginationCookie = function () {
        return jq.cookies.get(cookiePaginationKey);
    };

    var hide = function() {
        $tableForNavigation.hide();
    };

    var show = function() {
        $tableForNavigation.show();
    };

    var clear = function() {
        $simplePageNavigator.empty();
        hide();
    };

    return {
        init: init,
        update: update,
        unbindListEvents: unbindListEvents,
        entryCountOnPage: entryCountOnPage,
        currentPage: currentPage,
        pgNavigator: pgNavigator,
        hide: hide,
        show: show,
        clear: clear
    };
})();