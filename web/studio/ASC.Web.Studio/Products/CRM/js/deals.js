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


if (typeof ASC === "undefined") {
    ASC = {};
}
if (typeof ASC.CRM === "undefined") {
    ASC.CRM = (function() { return {} })();
}

ASC.CRM.myFilter = {
    filterId: 'dealsAdvansedFilter',
    idFilterByContact: 'contactID',
    idFilterByParticipant: 'participantID',

    type: 'custom-contact',
    hiddenContainerId: 'hiddenBlockForContactSelector',
    headerContainerId: 'contactSelectorForFilter',

    onSelectContact: function(event, item) {
        jq("#" + ASC.CRM.myFilter.headerContainerId).find(".inner-text .value").text(item.title);

        var $filter = jq('#' + ASC.CRM.myFilter.filterId);
        $filter.advansedFilter(ASC.CRM.myFilter.idFilterByContact, { id: item.id, displayName: item.title, value: jq.toJSON([item.id, false]) });
        $filter.advansedFilter('resize');
    },

    onSelectParticipant: function (event, item) {
        jq("#" + ASC.CRM.myFilter.headerContainerId).find(".inner-text .value").text(item.title);

        var $filter = jq('#' + ASC.CRM.myFilter.filterId);
        $filter.advansedFilter(ASC.CRM.myFilter.idFilterByParticipant, { id: item.id, displayName: item.title, value: jq.toJSON([item.id, true]) });
        $filter.advansedFilter('resize');
    },

    createFilter: function (filter) {
        var o = document.createElement('div');
        o.classList.add("default-value");
        o.innerHTML = [
            '<span class="title">',
                filter.title,
            '</span>',
            '<span class="selector-wrapper">',
                '<span class="contact-selector"></span>',
            '</span>',
            '<span class="btn-delete">&times;</span>',
        ].join('');
        return o;
    },

    customizeFilter: function ($container, $filteritem, filter) {
        var isParticipant = $filteritem.attr("data-id") == ASC.CRM.myFilter.idFilterByParticipant;

        var $filterSwitcher = jq("#" + ASC.CRM.myFilter.headerContainerId);

        $filterSwitcher
        .off("showList")
        .on("showList", function (event, item) {
            if (isParticipant) {
                ASC.CRM.myFilter.onSelectParticipant(event, item);
            } else {
                ASC.CRM.myFilter.onSelectContact(event, item);
            }
            $filteritem.removeClass("default-value");
        });

        $filterSwitcher.next().andSelf().appendTo($filteritem.find('span.contact-selector:first'));

        if (!filter.isset) {
            setTimeout(function () {
                if ($filteritem.hasClass("default-value")) {
                    $filterSwitcher.click();
                }
            }, 0);
        }

        return {};
    },

    destroyFilter: function ($container, $filteritem, filter) {
        var $filterSwitcher = jq('#' + ASC.CRM.myFilter.headerContainerId);

        if (!$filterSwitcher.parent().is("#" + ASC.CRM.myFilter.hiddenContainerId)) {
            $filterSwitcher.off("showList");
            $filterSwitcher.find(".inner-text .value").text(ASC.CRM.Resources.CRMCommonResource.Select);
            $filterSwitcher.contactadvancedSelector("reset");
            $filterSwitcher.next().andSelf().appendTo(jq('#' + ASC.CRM.myFilter.hiddenContainerId));
        }
    },

    processFilter: function ($container, $filteritem, filtervalue, params) {
        if (params && params.id && isFinite(params.id)) {
            var $filterSwitcher = jq('#' + ASC.CRM.myFilter.headerContainerId);
            $filterSwitcher.find(".inner-text .value").text(params.displayName);
            $filterSwitcher.contactadvancedSelector("select", [params.id]);
            $filteritem.removeClass("default-value");
        }
    }
};

ASC.CRM.ListDealView = (function() {

    //Teamlab.bind(Teamlab.events.getException, onGetException);

    function onGetException(params, errors) {
        console.log('deals.js ', errors);
        ASC.CRM.ListDealView.isFirstLoad ? hideFirstLoader() : LoadingBanner.hideLoading();
    };

    var _setCookie = function(page, countOnPage) {
        if (ASC.CRM.ListDealView.cookieKey && ASC.CRM.ListDealView.cookieKey != "") {
            var cookie = {
                page: page,
                countOnPage: countOnPage
            };
            jq.cookies.set(ASC.CRM.ListDealView.cookieKey, cookie, { path: location.pathname });
        }
    };

    var _getDeals = function(startIndex) {
        var filters = ASC.CRM.ListDealView.getFilterSettings(startIndex);

        Teamlab.getCrmOpportunities({ startIndex: startIndex || 0 }, { filter: filters, success: callback_get_opportunities_by_filter });
    };

    var _resizeFilter = function() {
        var visible = jq("#dealFilterContainer").is(":hidden") == false;
        if (ASC.CRM.ListDealView.isFilterVisible == false && visible) {
            ASC.CRM.ListDealView.isFilterVisible = true;
            if (ASC.CRM.ListDealView.advansedFilter)
                jq("#dealsAdvansedFilter").advansedFilter("resize");
        }
    };

    var _changeFilter = function() {
        ASC.CRM.ListDealView.deselectAll();

        var defaultStartIndex = 0;
        if (ASC.CRM.ListDealView.defaultCurrentPageNumber != 0) {
            _setCookie(ASC.CRM.ListDealView.defaultCurrentPageNumber, window.dealPageNavigator.EntryCountOnPage);
            defaultStartIndex = (ASC.CRM.ListDealView.defaultCurrentPageNumber - 1) * window.dealPageNavigator.EntryCountOnPage;
            ASC.CRM.ListDealView.defaultCurrentPageNumber = 0;
        } else {
            _setCookie(0, window.dealPageNavigator.EntryCountOnPage);
        }
        _renderContent(0 || defaultStartIndex);
    };

    var _renderContent = function(startIndex) {
        ASC.CRM.ListDealView.dealList = new Array();
        ASC.CRM.ListDealView.bidList = new Array();

        if (!ASC.CRM.ListDealView.isFirstLoad) {
            LoadingBanner.displayLoading();
            jq("#dealFilterContainer, #dealHeaderMenu, #dealList, #tableForDealNavigation").show();
            jq('#dealsAdvansedFilter').advansedFilter("resize");
        }
        jq("#mainSelectAllDeals").prop("checked", false);

        _getDeals(startIndex);
    };

    var _initPageNavigatorControl = function (countOfRows, currentPageNumber) {
        window.dealPageNavigator = new ASC.Controls.PageNavigator.init("dealPageNavigator", "#divForDealPager", countOfRows, ASC.CRM.Data.VisiblePageCount, currentPageNumber,
                                                                        ASC.CRM.Resources.CRMJSResource.Previous, ASC.CRM.Resources.CRMJSResource.Next);

        window.dealPageNavigator.changePageCallback = function(page) {
            _setCookie(page, window.dealPageNavigator.EntryCountOnPage);

            var startIndex = window.dealPageNavigator.EntryCountOnPage * (page - 1);
            _renderContent(startIndex);
        };
    };

    var _renderDealPageNavigator = function(startIndex) {
        var tmpTotal;
        if (startIndex >= ASC.CRM.ListDealView.Total) {
            tmpTotal = startIndex + 1;
        } else {
            tmpTotal = ASC.CRM.ListDealView.Total;
        }
        window.dealPageNavigator.drawPageNavigator((startIndex / ASC.CRM.ListDealView.entryCountOnPage).toFixed(0) * 1 + 1, tmpTotal);
    };

    var _renderSimpleDealsPageNavigator = function() {
        jq("#dealHeaderMenu .menu-action-simple-pagenav").html("");
        var $simplePN = jq("<div></div>"),
            lengthOfLinks = 0;
        if (jq("#divForDealPager .pagerPrevButtonCSSClass").length != 0) {
            lengthOfLinks++;
            jq("#divForDealPager .pagerPrevButtonCSSClass").clone().appendTo($simplePN);
        }
        if (jq("#divForDealPager .pagerNextButtonCSSClass").length != 0) {
            lengthOfLinks++;
            if (lengthOfLinks === 2) {
                jq("<span style='padding: 0 8px;'>&nbsp;</span>").clone().appendTo($simplePN);
            }
            jq("#divForDealPager .pagerNextButtonCSSClass").clone().appendTo($simplePN);
        }
        if ($simplePN.children().length != 0) {
            $simplePN.appendTo("#dealHeaderMenu .menu-action-simple-pagenav");
            jq("#dealHeaderMenu .menu-action-simple-pagenav").show();
        } else {
            jq("#dealHeaderMenu .menu-action-simple-pagenav").hide();
        }
    };

    var _renderCheckedDealsCount = function(count) {
        if (count != 0) {
            jq("#dealHeaderMenu .menu-action-checked-count > span").text(jq.format(ASC.CRM.Resources.CRMJSResource.ElementsSelectedCount, count));
            jq("#dealHeaderMenu .menu-action-checked-count").show();
        } else {
            jq("#dealHeaderMenu .menu-action-checked-count > span").text("");
            jq("#dealHeaderMenu .menu-action-checked-count").hide();
        }
    };

    var _renderNoDealsEmptyScreen = function() {
        jq("#dealTable tbody tr").remove();
        jq("#dealFilterContainer, #dealHeaderMenu, #dealList, #tableForDealNavigation").hide();
        ASC.CRM.Common.hideExportButtons();
        jq("#emptyContentForDealsFilter:not(.display-none)").addClass("display-none");
        jq("#dealsEmptyScreen.display-none").removeClass("display-none");
    };

    var _renderNoDealsForQueryEmptyScreen = function() {
        jq("#dealTable tbody tr").remove();
        jq("#dealHeaderMenu, #dealList, #tableForDealNavigation").hide();
        jq("#dealFilterContainer").show();
        ASC.CRM.Common.hideExportButtons();
        jq("#mainSelectAllDeals").attr("disabled", true);
        jq("#dealsEmptyScreen:not(.display-none)").addClass("display-none");
        jq("#emptyContentForDealsFilter.display-none").removeClass("display-none");
    };

    var callback_get_opportunities_by_filter = function(params, opportunities) {
        ASC.CRM.ListDealView.Total = params.__total || 0;
        var startIndex = params.__startIndex || 0,
            selectedIDs = new Array();

        for (var i = 0, n = ASC.CRM.ListDealView.selectedItems.length; i < n; i++) {
            selectedIDs.push(ASC.CRM.ListDealView.selectedItems[i].id);
        }

        ASC.CRM.ListDealView.bidList = [];
        for (var i = 0, n = opportunities.length; i < n; i++) {
            ASC.CRM.ListDealView._dealItemFactory(opportunities[i], selectedIDs);
        }
        ASC.CRM.ListDealView.dealList = opportunities;
        jq(window).trigger("getDealsFromApi", [params, opportunities]);

        if (ASC.CRM.ListDealView.Total === 0 &&
                    typeof (ASC.CRM.ListDealView.advansedFilter) != "undefined" &&
                    ASC.CRM.ListDealView.advansedFilter.advansedFilter().length == 1) {
            ASC.CRM.ListDealView.noDeals = true;
            ASC.CRM.ListDealView.noDealsForQuery = true;
        } else {
            ASC.CRM.ListDealView.noDeals = false;
            if (ASC.CRM.ListDealView.Total === 0) {
                ASC.CRM.ListDealView.noDealsForQuery = true;
            } else {
                ASC.CRM.ListDealView.noDealsForQuery = false;
            }
        }

        if (ASC.CRM.ListDealView.noDeals) {
            _renderNoDealsEmptyScreen();
            ASC.CRM.ListDealView.isFirstLoad ? hideFirstLoader() : LoadingBanner.hideLoading();
            return false;
        }

        if (ASC.CRM.ListDealView.noDealsForQuery) {
            _renderNoDealsForQueryEmptyScreen();

            _resizeFilter();

            ASC.CRM.ListDealView.isFirstLoad ? hideFirstLoader() : LoadingBanner.hideLoading();
            return false;
        }

        if (opportunities.length == 0) {//it can happen when select page without elements after deleting
            jq("dealsEmptyScreen:not(.display-none)").addClass("display-none");
            jq("#emptyContentForDealsFilter:not(.display-none)").addClass("display-none");
            jq("#dealList").show();
            jq("#dealTable tbody tr").remove();
            jq("#tableForDealsNavigation").show();
            jq("#mainSelectAllDeals").attr("disabled", true);
            ASC.CRM.Common.hideExportButtons();

            ASC.CRM.ListDealView.Total = parseInt(jq("#totalDealsOnPage").text()) || 0;

            var startIndex = ASC.CRM.ListDealView.entryCountOnPage * (window.dealPageNavigator.CurrentPageNumber - 1);

            while (startIndex >= ASC.CRM.ListDealView.Total && startIndex >= ASC.CRM.ListDealView.entryCountOnPage) {
                startIndex -= ASC.CRM.ListDealView.entryCountOnPage;
            }
            _renderContent(startIndex);
            return false;
        }

        jq("#totalDealsOnPage").text(ASC.CRM.ListDealView.Total);
        jq("#emptyContentForDealsFilter:not(.display-none)").addClass("display-none");
        jq("#dealsEmptyScreen:not(.display-none)").addClass("display-none");
        ASC.CRM.Common.showExportButtons();

        jq("#dealFilterContainer, #dealHeaderMenu, #tableForDealNavigation").show();
        _resizeFilter();

        jq("#mainSelectAllDeals").removeAttr("disabled");

        jq("#dealTable tbody").replaceWith(jq.tmpl("dealListTmpl", { opportunities: ASC.CRM.ListDealView.dealList }));
        jq("#dealList").show();

        ASC.CRM.ListDealView.checkFullSelection();

        ASC.CRM.Common.RegisterContactInfoCard();
        for (var i = 0, n = ASC.CRM.ListDealView.dealList.length; i < n; i++) {
            ASC.CRM.Common.tooltip("#dealTitle_" + ASC.CRM.ListDealView.dealList[i].id, "tooltip");
        }

        _renderDealPageNavigator(startIndex);
        _renderSimpleDealsPageNavigator();

        if (ASC.CRM.ListDealView.bidList.length == 0) {
            jq("#dealList .showTotalAmount").hide();
        } else {
            jq("#dealList .showTotalAmount").show();
        }

        window.scrollTo(0, 0);
        ScrolledGroupMenu.fixContentHeaderWidth(jq('#dealHeaderMenu'));
        ASC.CRM.ListDealView.isFirstLoad ? hideFirstLoader() : LoadingBanner.hideLoading();
    };
   

    var callback_add_tag = function(params, tag) {
        jq("#addTagDealsDialog").hide();
        if (params.isNewTag) {
            var tag = {
                value: params.tagName,
                title: params.tagName
            };
            ASC.CRM.Data.dealTags.push(tag);
            _renderTagElement(tag);

            ASC.CRM.ListDealView.advansedFilter = ASC.CRM.ListDealView.advansedFilter.advansedFilter(
            {
                nonetrigger: true,
                sorters: [],
                filters: [
                    { id: "tags", type: 'combobox', options: ASC.CRM.Data.dealTags, enable: ASC.CRM.Data.dealTags.length > 0 }
                ]
            });
        }
    };

    var callback_delete_batch_opportunities = function(params, data) {
        var newDealsList = new Array();
        for (var i = 0, len_i = ASC.CRM.ListDealView.dealList.length; i < len_i; i++) {
            var dealItem = ASC.CRM.ListDealView.dealList[i];
            var isDeleted = false;

            for (var j = 0, len_j = params.dealsIDsForDelete.length; j < len_j; j++)
                if (params.dealsIDsForDelete[j] == dealItem.id) {
                    isDeleted = true;
                    break;
                }

            if (!isDeleted)
                newDealsList.push(dealItem);
            else if (dealItem.bidValue)
                for (var k = 0, len_k = ASC.CRM.ListDealView.bidList.length; k < len_k; k++) 
                    if (ASC.CRM.ListDealView.bidList[k].bidCurrencyAbbreviation == dealItem.bidCurrency.abbreviation) {
                        ASC.CRM.ListDealView.bidList[k].bidValue -= dealItem.bidValue * (dealItem.perPeriodValue ? dealItem.perPeriodValue : 1);
                        break;
                    }
        }

        ASC.CRM.ListDealView.dealList = newDealsList;

        ASC.CRM.ListDealView.Total -= params.dealsIDsForDelete.length;
        jq("#totalDealsOnPage").text(ASC.CRM.ListDealView.Total);

        var selectedIDs = new Array();
        for (var i = 0, n = ASC.CRM.ListDealView.selectedItems.length; i < n; i++) {
            selectedIDs.push(ASC.CRM.ListDealView.selectedItems[i].id);
        }

        for (var i = 0, len = params.dealsIDsForDelete.length; i < len; i++) {
            var $objForRemove = jq("#dealItem_" + params.dealsIDsForDelete[i]);
            if ($objForRemove.length != 0)
                $objForRemove.remove();

            var index = jq.inArray(params.dealsIDsForDelete[i], selectedIDs);
            if (index != -1) {
                selectedIDs.splice(index, 1);
                ASC.CRM.ListDealView.selectedItems.splice(index, 1);
            }
        }
        jq("#mainSelectAllDeals").prop("checked", false);

        _checkForLockMainActions();
        _renderCheckedDealsCount(ASC.CRM.ListDealView.selectedItems.length);

        if (ASC.CRM.ListDealView.Total == 0
            && (typeof (ASC.CRM.ListDealView.advansedFilter) == "undefined"
            || ASC.CRM.ListDealView.advansedFilter.advansedFilter().length == 1)) {
            ASC.CRM.ListDealView.noDeals = true;
            ASC.CRM.ListDealView.noDealsForQuery = true;
        } else {
            ASC.CRM.ListDealView.noDeals = false;
            if (ASC.CRM.ListDealView.Total === 0) {
                ASC.CRM.ListDealView.noDealsForQuery = true;
            } else {
                ASC.CRM.ListDealView.noDealsForQuery = false;
            }
        }

        PopupKeyUpActionProvider.EnableEsc = true;
        if (ASC.CRM.ListDealView.noDeals) {
            _renderNoDealsEmptyScreen();
            jq.unblockUI();
            return false;
        }

        if (ASC.CRM.ListDealView.noDealsForQuery) {
            _renderNoDealsForQueryEmptyScreen();

            jq.unblockUI();
            return false;
        }

        if (jq("#dealTable tbody tr").length == 0) {
            jq.unblockUI();

            var startIndex = ASC.CRM.ListDealView.entryCountOnPage * (dealPageNavigator.CurrentPageNumber - 1);
            if (startIndex >= ASC.CRM.ListDealView.Total) { startIndex -= ASC.CRM.ListDealView.entryCountOnPage; }
            _renderContent(startIndex);
        } else {
            jq.unblockUI();
        }
    };

    var callback_update_opportunity_rights = function(params, opportunities) {
        for (var i = 0, n = opportunities.length; i < n; i++) {
            for (var j = 0, m = ASC.CRM.ListDealView.dealList.length; j < m; j++) {
                var opportunity_id = opportunities[i].id;
                if (opportunity_id == ASC.CRM.ListDealView.dealList[j].id) {
                    ASC.CRM.ListDealView.dealList[j].isPrivate = opportunities[i].isPrivate;
                    jq("#dealItem_" + opportunity_id).replaceWith(
                        jq.tmpl("dealTmpl", ASC.CRM.ListDealView.dealList[j])
                    );
                    if (params.isBatch) {
                        jq("#checkDeal_" + opportunity_id).prop("checked", true);
                    } else {
                        ASC.CRM.ListDealView.selectedItems = new Array();
                    }
                    break;
                }
            }
        }
        ASC.CRM.Common.RegisterContactInfoCard();
        PopupKeyUpActionProvider.EnableEsc = true;
        jq.unblockUI();
    };

    var hideFirstLoader = function () {
        ASC.CRM.ListContactView.isFirstLoad = false;
        jq(".containerBodyBlock").children(".loader-page").hide();
        if (!jq("#dealsEmptyScreen").is(":visible") && !jq("#emptyContentForDealsFilter")) {
            jq("#dealFilterContainer, #dealHeaderMenu, #dealList, #tableForDealNavigation").show();
            jq('#dealsAdvansedFilter').advansedFilter("resize");
        }
    };

    var _showActionMenu = function(dealID) {
        var deal = null;
        for (var i = 0, n = ASC.CRM.ListDealView.dealList.length; i < n; i++) {
            if (dealID == ASC.CRM.ListDealView.dealList[i].id) {
                deal = ASC.CRM.ListDealView.dealList[i];
                break;
            }
        }
        if (deal == null) return;

        jq("#dealActionMenu .editDealLink").attr("href", jq.format("Deals.aspx?id={0}&action=manage", dealID));

        jq("#dealActionMenu .deleteDealLink").unbind("click").bind("click", function() {
            jq("#dealActionMenu").hide();
            jq("#dealTable .entity-menu.active").removeClass("active");
            ASC.CRM.ListDealView.showConfirmationPanelForDelete(deal.title, dealID, true);
        });

        jq("#dealActionMenu .showProfileLink").attr("href", jq.format("Deals.aspx?id={0}", dealID));

        jq("#dealActionMenu .showProfileLinkNewTab").unbind("click").bind("click", function () {
            jq("#dealActionMenu").hide();
            jq("#dealTable .entity-menu.active").removeClass("active");
            window.open(jq.format("Deals.aspx?id={0}", dealID), "_blank");
        });

        var showSeporators = false;

        if (ASC.CRM.Data.IsCRMAdmin === true || Teamlab.profile.id == deal.createdBy.id) {
            jq("#dealActionMenu .setPermissionsLink").show();
            jq("#dealActionMenu .setPermissionsLink").unbind("click").bind("click", function() {
                jq("#dealActionMenu").hide();
                jq("#dealTable .entity-menu.active").removeClass("active");

                ASC.CRM.ListDealView.deselectAll();
                ASC.CRM.ListDealView.selectedItems.push(_createShortDeal(deal));

                _showSetPermissionsPanel({ isBatch: false });
            });
            showSeporators = true;
        } else {
            jq("#dealActionMenu .setPermissionsLink").hide();
        }

        if (ASC.CRM.Data.IsCRMAdmin === true && jq("#dealActionMenu .createProject").length == 1) {
            var basePathForLink = StudioManager.getLocationPathToModule("Projects") + "Projects.aspx?action=add&opportunityID=";
            jq("#dealActionMenu .createProject").attr("href", basePathForLink + dealID);
            jq("#dealActionMenu .createProject").unbind("click").bind("click", function() {
                jq("#dealTable .entity-menu.active").removeClass("active");
                jq("#dealActionMenu").hide();
            });
            showSeporators = true;
        }

        if (showSeporators) {
            jq("#dealActionMenu .dropdown-item-seporator:first").show();
        } else {
            jq("#dealActionMenu .dropdown-item-seporator:first").hide();
        }
    };

    var _lockMainActions = function() {
        jq("#dealHeaderMenu .menuActionDelete").removeClass("unlockAction");
        jq("#dealHeaderMenu .menuActionAddTag").removeClass("unlockAction");
        jq("#dealHeaderMenu .menuActionPermissions").removeClass("unlockAction");
    };

    var _checkForLockMainActions = function() {
        var count = ASC.CRM.ListDealView.selectedItems.length;
        if (count === 0) {
            _lockMainActions();
            return;
        }

        var unlockSetPermissions = false;

        for (var i = 0; i < count; i++) {
            if (ASC.CRM.Data.IsCRMAdmin === true || ASC.CRM.ListDealView.selectedItems[i].createdBy.id == Teamlab.profile.id) {
                unlockSetPermissions = true;
                break;
            }
        }

        if (unlockSetPermissions) {
            jq("#dealHeaderMenu .menuActionPermissions:not('.unlockAction')").addClass("unlockAction");
        } else {
            jq("#dealHeaderMenu .menuActionPermissions.unlockAction").removeClass("unlockAction");
        }

        jq("#dealHeaderMenu .menuActionDelete:not('.unlockAction')").addClass("unlockAction");
        jq("#dealHeaderMenu .menuActionAddTag:not('.unlockAction')").addClass("unlockAction");

    };

    var _renderTagElement = function(tag) {
        var $tagElem = jq("<a></a>").addClass("dropdown-item")
                        .text(ASC.CRM.Common.convertText(tag.title, false))
                        .bind("click", function() {
                            _addThisTag(this);
                        });
        jq("#addTagDealsDialog ul.dropdown-content").append(jq("<li></li>").append($tagElem));
    };

    var _renderAndInitTagsDialog = function() {
        for (var i = 0, n = ASC.CRM.Data.dealTags.length; i < n; i++) {
            _renderTagElement(ASC.CRM.Data.dealTags[i]);
        }
        jq.dropdownToggle({
            dropdownID: "addTagDealsDialog",
            switcherSelector: "#dealHeaderMenu .menuActionAddTag.unlockAction",
            addTop: 5,
            addLeft: 0,
            showFunction: function(switcherObj, dropdownItem) {
                jq("#addTagDealsDialog input.textEdit").val("");
            }
        });
    };

    var _initDealActionMenu = function() {
        jq.dropdownToggle({
            dropdownID: "dealActionMenu",
            switcherSelector: "#dealTable .entity-menu",
            addTop: 0,
            addLeft: 10,
            rightPos: true,
            beforeShowFunction: function (switcherObj, dropdownItem) {
                var dealId = switcherObj.attr("id").split('_')[1];
                if (!dealId) return;
                _showActionMenu(parseInt(dealId));
            },
            showFunction: function(switcherObj, dropdownItem) {
                jq("#dealTable .entity-menu.active").removeClass("active");
                if (dropdownItem.is(":hidden")) {
                    switcherObj.addClass("active");
                }
            },
            hideFunction: function() {
                jq("#dealTable .entity-menu.active").removeClass("active");
            }
        });


        jq("body").unbind("contextmenu").bind("contextmenu", function(event) {
            var e = jq.fixEvent(event);

            if (typeof e == "undefined" || !e) {
                return true;
            }

            var target = jq(e.srcElement || e.target);

            if (!target.parents("#dealTable").length) {
                jq("#dealActionMenu").hide();
                return true;
            }

            var dealId = parseInt(target.closest("tr.with-entity-menu").attr("id").split('_')[1]);
            if (!dealId) {
                return true;
            }
            _showActionMenu(dealId);
            jq("#dealTable .entity-menu.active").removeClass("active");

            jq.showDropDownByContext(e, target, jq("#dealActionMenu"));
            return false;
        });
    };

    var _initScrolledGroupMenu = function() {
        ScrolledGroupMenu.init({
            menuSelector: "#dealHeaderMenu",
            menuAnchorSelector: "#mainSelectAllDeals",
            menuSpacerSelector: "main .filter-content .header-menu-spacer",
            userFuncInTop: function() { jq("#dealHeaderMenu .menu-action-on-top").hide(); },
            userFuncNotInTop: function() { jq("#dealHeaderMenu .menu-action-on-top").show(); }
        });

        jq("#dealHeaderMenu").on("click", ".menuActionDelete.unlockAction", function () {
            _showDeletePanel();
        });

        jq("#dealHeaderMenu").on("click", ".menuActionPermissions.unlockAction", function () {
            _showSetPermissionsPanel({ isBatch: true });
        });
    };

    var _initConfirmationPannels = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "deleteDealsPanel",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: ASC.CRM.Resources.CRMCommonResource.ConfirmationDeleteText,
            innerHtmlText: ["<div id=\"deleteDealsList\" class=\"containerForListBatchDelete mobile-overflow\">",
                                        "<dl>",
                                            "<dt class=\"listForBatchDelete\">",
                                                ASC.CRM.Resources.CRMDealResource.Deals,
                                                ":",
                                            "</dt>",
                                            "<dd class=\"listForBatchDelete\">",
                                            "</dd>",
                                        "</dl>",
                                    "</div>"].join(''),

            OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ASC.CRM.Resources.CRMDealResource.DeletingDeals
        }).insertAfter("#dealList");

        jq("#deleteDealsPanel").on("click", ".middle-button-container .button.blue", function () {
            ASC.CRM.ListDealView.deleteBatchDeals();
        });

        jq.tmpl("template-blockUIPanel", {
            id: "setPermissionsDealsPanel",
            headerTest: ASC.CRM.Resources.CRMCommonResource.SetPermissions,
            innerHtmlText: "",
            OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
            OKBtnClass: "setPermissionsLink",
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ASC.CRM.Resources.CRMCommonResource.SaveChangesProggress
        }).insertAfter("#dealList");

        jq("#permissionsDealsPanelInnerHtml").insertBefore("#setPermissionsDealsPanel .containerBodyBlock .middle-button-container").removeClass("display-none");
    };

    var _addThisTag = function(obj) {
        var params = {
            tagName: jq(obj).text(),
            isNewTag: false
        };

        _addTag(params);
    };

    var _addTag = function(params) {
        var selectedIDs = [];
        for (var i = 0, n = ASC.CRM.ListDealView.selectedItems.length; i < n; i++) {
            selectedIDs.push(ASC.CRM.ListDealView.selectedItems[i].id);
        }
        params.selectedIDs = selectedIDs;

        Teamlab.addCrmTag(params, "opportunity", params.selectedIDs, params.tagName,
        {
            success: callback_add_tag,
            before: function(par) {
                for (var i = 0, n = par.selectedIDs.length; i < n; i++) {
                    jq("#checkDeal_" + par.selectedIDs[i]).hide();
                    jq("#loaderImg_" + par.selectedIDs[i]).show();
                }
            },
            after: function(par) {
                for (var i = 0, n = par.selectedIDs.length; i < n; i++) {
                    jq("#loaderImg_" + par.selectedIDs[i]).hide();
                    jq("#checkDeal_" + par.selectedIDs[i]).show();
                }
            }
        });
    };

    var _showDeletePanel = function() {
        jq("#deleteDealsList dd.listForBatchDelete").html("");
        for (var i = 0, len = ASC.CRM.ListDealView.selectedItems.length; i < len; i++) {
            var label = jq("<label></label>")
                            .attr("title", ASC.CRM.ListDealView.selectedItems[i].title)
                            .text(ASC.CRM.ListDealView.selectedItems[i].title);
            jq("#deleteDealsList dd.listForBatchDelete").append(
                            label.prepend(jq("<input>")
                            .attr("type", "checkbox")
                            .prop("checked", true)
                            .attr("id", "deal_" + ASC.CRM.ListDealView.selectedItems[i].id))
                        );

        }
        LoadingBanner.hideLoaderBtn("#deleteDealsPanel");
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#deleteDealsPanel", 500);
    };

    var _showSetPermissionsPanel = function(params) {
        if (jq("#setPermissionsDealsPanel div.tintMedium").length > 0) {
            jq("#setPermissionsDealsPanel div.tintMedium span.header-base").remove();
            jq("#setPermissionsDealsPanel div.tintMedium").removeClass("tintMedium").css("padding", "0px");
        }
        jq("#isPrivate").prop("checked", false);
        ASC.CRM.PrivatePanel.changeIsPrivateCheckBox();


        jq("#setPermissionsDealsPanel .addUserLink").useradvancedSelector("reset", false);
        jq("#setPermissionsDealsPanel .advanced-selector-list>li.selected").removeClass("selected")

        window.SelectedUsers.IDs.clear();
        window.SelectedUsers.Names.clear();
        jq("#selectedUsers div.selectedUser[id^=selectedUser_]").remove();


        LoadingBanner.hideLoaderBtn("#setPermissionsDealsPanel");
        jq("#setPermissionsDealsPanel .setPermissionsLink").unbind("click").bind("click", function() {
            _setPermissions(params);
        });
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#setPermissionsDealsPanel", 600);
    };

    var _setPermissions = function(params) {
        var selectedUsers = window.SelectedUsers.IDs,
            selectedIDs = [];
        selectedUsers.push(window.SelectedUsers.CurrentUserID);

        for (var i = 0, n = ASC.CRM.ListDealView.selectedItems.length; i < n; i++) {
            selectedIDs.push(ASC.CRM.ListDealView.selectedItems[i].id);
        }

        var data = {
            opportunityid: selectedIDs,
            isPrivate: jq("#isPrivate").is(":checked"),
            accessList: selectedUsers
        };

        Teamlab.updateCrmOpportunityRights(params, data,
        {
            success: callback_update_opportunity_rights,
            before: function () {
                LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.SaveChangesProggress;
                LoadingBanner.showLoaderBtn("#setPermissionsDealsPanel");               
            },
            after: function() {
                LoadingBanner.hideLoaderBtn("#setPermissionsDealsPanel");
            }
        });
    };

    var _createShortDeal = function(deal) {
        var shortDeal = {
            id: deal.id,
            isClosed: deal.isClosed,
            isPrivate: deal.isPrivate,
            title: deal.title,
            canEdit: deal.canEdit,
            createdBy: deal.createdBy
        };
        return shortDeal;
    };

    var _preInitPage = function (entryCountOnPage) {
        jq("#mainSelectAllDeals").prop("checked", false);//'cause checkboxes save their state between refreshing the page

        jq("#tableForDealNavigation select:first")
            .val(entryCountOnPage)
            .change(function () {
                ASC.CRM.ListDealView.changeCountOfRows(this.value);
            })
            .tlCombobox();
    };
    
    var _initEmptyScreen = function (emptyListImgSrc, emptyFilterListImgSrc) {
        //init emptyScreen for all list
        var buttonHTML = ["<a class='link dotline plus' href='Deals.aspx?action=manage'>",
            ASC.CRM.Resources.CRMDealResource.CreateFirstDeal,
            "</a>"].join('');
        
        if (jq.browser.mobile != true) {
            buttonHTML += ["<br/><a class='crm-importLink link' href='Deals.aspx?action=import'>",
                ASC.CRM.Resources.CRMDealResource.ImportDeals,
                "</a>"].join('');
        }

        jq.tmpl("template-emptyScreen",
            {
                ID: "dealsEmptyScreen",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_deals"],
                Header: ASC.CRM.Resources.CRMDealResource.EmptyContentDealsHeader,
                Describe: jq.format(ASC.CRM.Resources.CRMDealResource.EmptyContentDealsDescribe, "<span class='hintStages baseLinkAction'>", "</span>"),
                ButtonHTML: buttonHTML,
                CssClass: "display-none"
            }).insertAfter("#dealList");

        //init emptyScreen for filter
        jq.tmpl("template-emptyScreen",
            {
                ID: "emptyContentForDealsFilter",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_filter"],
                Header: ASC.CRM.Resources.CRMDealResource.EmptyContentDealsFilterHeader,
                Describe: ASC.CRM.Resources.CRMDealResource.EmptyContentDealsFilterDescribe,
                ButtonHTML: ["<a class='clearFilterButton link dotline' href='javascript:void(0);' onclick='ASC.CRM.ListDealView.advansedFilter.advansedFilter(null);'>",
                    ASC.CRM.Resources.CRMCommonResource.ClearFilter,
                    "</a>"].join(''),
                CssClass: "display-none"
            }).insertAfter("#dealList");

    };
    
    var _initFilter = function () {
        if (!jq("#dealsAdvansedFilter").advansedFilter) return;

        var tmpDate = new Date(),
            today = new Date(tmpDate.getFullYear(), tmpDate.getMonth(), tmpDate.getDate(), 0, 0, 0, 0),
            yesterday = new Date(new Date(today).setDate(tmpDate.getDate() - 1)),

            beginningOfThisMonth = new Date(new Date(today).setDate(1)),
            endOfThisMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0),

            endOfLastMonth = new Date(new Date(beginningOfThisMonth).setDate(beginningOfThisMonth.getDate() - 1)),
            beginningOfLastMonth = new Date(new Date(endOfLastMonth).setDate(1)),


            todayString = Teamlab.serializeTimestamp(today),
            yesterdayString = Teamlab.serializeTimestamp(yesterday),
            beginningOfThisMonthString = Teamlab.serializeTimestamp(beginningOfThisMonth),
            endOfThisMonthString = Teamlab.serializeTimestamp(endOfThisMonth),
            beginningOfLastMonthString = Teamlab.serializeTimestamp(beginningOfLastMonth),
            endOfLastMonthString = Teamlab.serializeTimestamp(endOfLastMonth);

        ASC.CRM.ListDealView.advansedFilter = jq("#dealsAdvansedFilter")
            .advansedFilter({
                anykey      : false,
                hintDefaultDisable: true,
                maxfilters  : -1,
                colcount    : 2,
                maxlength   : "100",
                store       : true,
                inhash      : true,
                filters     : [
                            {
                                type        : "person",
                                id          : "my",
                                apiparamname: "responsibleID",
                                title       : ASC.CRM.Resources.CRMCommonResource.My,
                                group       : ASC.CRM.Resources.CRMCommonResource.FilterByResponsible,
                                groupby     : "responsible",
                                filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByResponsible,
                                enable      : true,
                                bydefault   : { id: Teamlab.profile.id, value: Teamlab.profile.id }
                            },

                            {
                                type        : "person",
                                id          : "responsibleID",
                                apiparamname: "responsibleID",
                                title       : ASC.CRM.Resources.CRMDealResource.CustomResponsibleFilter,
                                group       : ASC.CRM.Resources.CRMCommonResource.FilterByResponsible,
                                groupby     : "responsible",
                                filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByResponsible
                            },

                            {
                                type        : "combobox",
                                id          : "lastMonth",
                                apiparamname: jq.toJSON(["fromDate", "toDate"]),
                                title       : ASC.CRM.Resources.CRMCommonResource.LastMonth,
                                filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByDate,
                                group       : ASC.CRM.Resources.CRMCommonResource.FilterByDate,
                                groupby     : "byDate",
                                options     :
                                        [
                                        { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth, def: true },
                                        { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday },
                                        { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today },
                                        { value: jq.toJSON([beginningOfThisMonthString, endOfThisMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth }
                                        ]
                            },
                            {
                                type        : "combobox",
                                id          : "yesterday",
                                apiparamname: jq.toJSON(["fromDate", "toDate"]),
                                title       : ASC.CRM.Resources.CRMCommonResource.Yesterday,
                                filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByDate,
                                group       : ASC.CRM.Resources.CRMCommonResource.FilterByDate,
                                groupby     : "byDate",
                                options     :
                                        [
                                        { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth },
                                        { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday, def: true },
                                        { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today },
                                        { value: jq.toJSON([beginningOfThisMonthString, endOfThisMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth }
                                        ]
                            },
                            {
                                type        : "combobox",
                                id          : "today",
                                apiparamname: jq.toJSON(["fromDate", "toDate"]),
                                title       : ASC.CRM.Resources.CRMCommonResource.Today,
                                filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByDate,
                                group       : ASC.CRM.Resources.CRMCommonResource.FilterByDate,
                                groupby     : "byDate",
                                options     :
                                        [
                                        { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth },
                                        { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday },
                                        { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today, def: true },
                                        { value: jq.toJSON([beginningOfThisMonthString, endOfThisMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth }
                                        ]
                            },
                            {
                                type        : "combobox",
                                id          : "thisMonth",
                                apiparamname: jq.toJSON(["fromDate", "toDate"]),
                                title       : ASC.CRM.Resources.CRMCommonResource.ThisMonth,
                                filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByDate,
                                group       : ASC.CRM.Resources.CRMCommonResource.FilterByDate,
                                groupby     : "byDate",
                                options     :
                                        [
                                        { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth },
                                        { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday },
                                        { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today },
                                        { value: jq.toJSON([beginningOfThisMonthString, endOfThisMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth, def: true }
                                        ]
                            },

                            {
                                type        : "daterange",
                                id          : "fromToDate",
                                title       : ASC.CRM.Resources.CRMDealResource.CustomDateFilter,
                                filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByDate,
                                group       : ASC.CRM.Resources.CRMCommonResource.FilterByDate,
                                groupby     : "byDate"
                            },

                            {
                                type        : ASC.CRM.myFilter.type,
                                id          : ASC.CRM.myFilter.idFilterByParticipant,
                                apiparamname: jq.toJSON(["contactID", "contactAlsoIsParticipant"]),
                                title       : ASC.CRM.Resources.CRMDealResource.FilterByParticipant,
                                group       : ASC.CRM.Resources.CRMCommonResource.Other,
                                groupby     : "contact",
                                hashmask    : "",
                                create      : ASC.CRM.myFilter.createFilter,
                                customize   : ASC.CRM.myFilter.customizeFilter,
                                destroy     : ASC.CRM.myFilter.destroyFilter,
                                process     : ASC.CRM.myFilter.processFilter
                            },
                            {
                                type        : ASC.CRM.myFilter.type,
                                id          : ASC.CRM.myFilter.idFilterByContact,
                                apiparamname: jq.toJSON(["contactID", "contactAlsoIsParticipant"]),
                                title       : ASC.CRM.Resources.CRMDealResource.FilterByContact,
                                group       : ASC.CRM.Resources.CRMCommonResource.Other,
                                groupby     : "contact",
                                hashmask    : "",
                                create      : ASC.CRM.myFilter.createFilter,
                                customize   : ASC.CRM.myFilter.customizeFilter,
                                destroy     : ASC.CRM.myFilter.destroyFilter,
                                process     : ASC.CRM.myFilter.processFilter
                            },
                            {
                                type        : "combobox",
                                id          : "tags",
                                apiparamname: "tags",
                                title       : ASC.CRM.Resources.CRMCommonResource.FilterWithTag,
                                group       : ASC.CRM.Resources.CRMCommonResource.Other,
                                options     : ASC.CRM.Data.dealTags,
                                defaulttitle: ASC.CRM.Resources.CRMCommonResource.Choose,
                                enable      : ASC.CRM.Data.dealTags.length > 0,
                                multiple    : true
                            },

                            {
                                type        : "combobox",
                                id          : "opportunityStagesID",
                                apiparamname: "opportunityStagesID",
                                title       : ASC.CRM.Resources.CRMDealResource.ByStage,
                                group       : ASC.CRM.Resources.CRMDealResource.FilterByStageOrStageType,
                                groupby     : "stageType",
                                options     : ASC.CRM.Data.dealMilestones,
                                defaulttitle: ASC.CRM.Resources.CRMCommonResource.Choose,
                                enable      : ASC.CRM.Data.dealMilestones.length > 0
                            },
                            {
                                type        : "combobox",
                                id          : "stageTypeOpen",
                                apiparamname: "stageType",
                                title       : ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_Open,
                                group       : ASC.CRM.Resources.CRMDealResource.FilterByStageOrStageType,
                                filtertitle : ASC.CRM.Resources.CRMDealResource.ByStageType,
                                groupby     : "stageType",
                                options     :
                                        [
                                        { value: "Open", classname: '', title: ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_Open, def: true },
                                        { value: "ClosedAndWon", classname: '', title: ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_ClosedAndWon },
                                        { value: "ClosedAndLost", classname: '', title: ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_ClosedAndLost }
                                        ]
                            },
                            {
                                type        : "combobox",
                                id          : "stageTypeClosedAndWon",
                                apiparamname: "stageType",
                                title       : ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_ClosedAndWon,
                                filtertitle : ASC.CRM.Resources.CRMDealResource.ByStageType,
                                group       : ASC.CRM.Resources.CRMDealResource.FilterByStageOrStageType,
                                groupby     : "stageType",
                                options     :
                                        [
                                        { value: "Open", classname: '', title: ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_Open },
                                        { value: "ClosedAndWon", classname: '', title: ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_ClosedAndWon, def: true },
                                        { value: "ClosedAndLost", classname: '', title: ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_ClosedAndLost }
                                        ]
                            },
                            {
                                type        : "combobox",
                                id          : "stageTypeClosedAndLost",
                                apiparamname: "stageType",
                                title       : ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_ClosedAndLost,
                                filtertitle : ASC.CRM.Resources.CRMDealResource.ByStageType,
                                group       : ASC.CRM.Resources.CRMDealResource.FilterByStageOrStageType,
                                groupby     : "stageType",
                                options     :
                                        [
                                        { value: "Open", classname: '', title: ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_Open },
                                        { value: "ClosedAndWon", classname: '', title: ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_ClosedAndWon },
                                        { value: "ClosedAndLost", classname: '', title: ASC.CRM.Resources.CRMEnumResource.DealMilestoneStatus_ClosedAndLost, def: true }
                                        ]
                            }
                ],
                sorters: [
                            { id: "title", title: ASC.CRM.Resources.CRMCommonResource.Title, dsc: false, def: false },
                            { id: "responsible", title: ASC.CRM.Resources.CRMCommonResource.Responsible, dsc: false, def: false },
                            { id: "stage", title: ASC.CRM.Resources.CRMDealResource.DealMilestone, dsc: false, def: true },
                            { id: "bidvalue", title: ASC.CRM.Resources.CRMDealResource.ExpectedValue, dsc: false, def: false },
                            { id: "dateandtime", title: ASC.CRM.Resources.CRMDealResource.Estimated, dsc: false, def: false }
                ]
            })
            .bind("setfilter", ASC.CRM.ListDealView.setFilter)
            .bind("resetfilter", ASC.CRM.ListDealView.resetFilter);
    };

    return {
        contactID: 0,

        dealList: [],
        bidList: [],
        selectedItems: [],

        isFilterVisible: false,

        entryCountOnPage: 0,
        defaultCurrentPageNumber: 0,

        noDeals: false,
        noDealsForQuery: false,

        cookieKey: "",

        clear: function () {
            ASC.CRM.ListDealView.contactID = 0;
            ASC.CRM.ListDealView.dealList = [];
            ASC.CRM.ListDealView.bidList = [];
            ASC.CRM.ListDealView.selectedItems = [];

            ASC.CRM.ListDealView.isFilterVisible = false;

            ASC.CRM.ListDealView.entryCountOnPage = 0;
            ASC.CRM.ListDealView.defaultCurrentPageNumber = 0;
            ASC.CRM.ListDealView.noDeals = false;
            ASC.CRM.ListDealView.noDealsForQuery = false;

            ASC.CRM.ListDealView.cookieKey = "";
            ASC.CRM.ListDealView.advansedFilter = null;
        },

        init: function (parentSelector, filterSelector, pagingSelector) {
            if (jq(parentSelector).length == 0) return;
            ASC.CRM.Common.setDocumentTitle(ASC.CRM.Resources.CRMDealResource.MyDeals);
            ASC.CRM.ListDealView.clear();
            jq(parentSelector).removeClass("display-none");

            jq.tmpl("dealsListFilterTmpl").appendTo(filterSelector);
            jq.tmpl("dealsListBaseTmpl", { IsCRMAdmin: ASC.CRM.Data.IsCRMAdmin, CanCreateProjects: ASC.CRM.Data.CanCreateProjects }).appendTo(parentSelector);
            jq.tmpl("dealsListPagingTmpl").appendTo(pagingSelector);

            jq('#privatePanelWrapper').appendTo("#permissionsDealsPanelInnerHtml");

            ASC.CRM.ListDealView.cookieKey = ASC.CRM.Data.CookieKeyForPagination["deals"];

            var settings = {
                    page: 1,
                    countOnPage: jq("#tableForDealNavigation select:first>option:first").val()
                },
                key = location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') + location.pathname + location.search,
                currentAnchor = location.hash,
                cookieKey = encodeURIComponent(key);

            currentAnchor = currentAnchor && typeof currentAnchor === 'string' && currentAnchor.charAt(0) === '#'
                ? currentAnchor.substring(1)
                : currentAnchor;

            var cookieAnchor = jq.cookies.get(cookieKey);

            if (currentAnchor == "" || cookieAnchor == currentAnchor) {
                var tmp = ASC.CRM.Common.getPagingParamsFromCookie(ASC.CRM.ListDealView.cookieKey);
                if (tmp != null) {
                    settings = tmp;
                }
            } else {
                _setCookie(settings.page, settings.countOnPage);
            }

            ASC.CRM.ListDealView.entryCountOnPage = settings.countOnPage;
            ASC.CRM.ListDealView.defaultCurrentPageNumber = settings.page;

            _preInitPage(ASC.CRM.ListDealView.entryCountOnPage);
            _initEmptyScreen();

            jq.tmpl("dealExtendedListTmpl", { contactID: 0 }).prependTo("#dealList");

            _initPageNavigatorControl(ASC.CRM.ListDealView.entryCountOnPage, ASC.CRM.ListDealView.defaultCurrentPageNumber);

            _renderAndInitTagsDialog();

            _initDealActionMenu();

            _initScrolledGroupMenu();

            jq("#menuCreateNewTask").on("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });

            ASC.CRM.ListDealView.initConfirmationPanelForDelete();

            _initConfirmationPannels();

            jq("#" + ASC.CRM.myFilter.headerContainerId).contactadvancedSelector(
             {
                 showme: true,
                 addtext: ASC.CRM.Resources.CRMContactResource.AddNewCompany,
                 noresults: ASC.CRM.Resources.CRMCommonResource.NoMatches,
                 noitems: ASC.CRM.Resources.CRMCommonResource.NoMatches,
                 inPopup: true,
                 onechosen: true,
                 isTempLoad: true
             });
            
            ASC.CRM.ListDealView.isFirstLoad = true;
            jq(".containerBodyBlock").children(".loader-page").show();

            _initFilter();

            ///*tracking events*/
            ASC.CRM.ListDealView.advansedFilter.one("adv-ready", function () {
                var crmAdvansedFilterContainer = jq("#dealsAdvansedFilter .advansed-filter-list");
                crmAdvansedFilterContainer.find("li[data-id='my'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'me_manager');
                crmAdvansedFilterContainer.find("li[data-id='responsibleID'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'custom_manager');
                crmAdvansedFilterContainer.find("li[data-id='company'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'company');
                crmAdvansedFilterContainer.find("li[data-id='Persons'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'persons');
                crmAdvansedFilterContainer.find("li[data-id='withopportunity'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'with_opportunity');
                crmAdvansedFilterContainer.find("li[data-id='lastMonth'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'last_month');
                crmAdvansedFilterContainer.find("li[data-id='yesterday'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'yesterday');
                crmAdvansedFilterContainer.find("li[data-id='today'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'today');
                crmAdvansedFilterContainer.find("li[data-id='thisMonth'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'this_month');
                crmAdvansedFilterContainer.find("li[data-id='fromToDate'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'from_to_date');
                crmAdvansedFilterContainer.find("li[data-id='opportunityStagesID'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'opportunity_stages');
                crmAdvansedFilterContainer.find("li[data-id='stageTypeOpen'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'stage_type_open');
                crmAdvansedFilterContainer.find("li[data-id='stageTypeClosedAndWon'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'stage_type_closed_and_won');
                crmAdvansedFilterContainer.find("li[data-id='stageTypeClosedAndLost'] .inner-text").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'stage_type_closed_and_lost');

                jq("#dealsAdvansedFilter .btn-toggle-sorter").trackEvent(ga_Categories.deals, ga_Actions.filterClick, 'sort');
                jq("#dealsAdvansedFilter .advansed-filter-input").trackEvent(ga_Categories.deals, ga_Actions.filterClick, "search_text", "enter");
            });
            
            ASC.CRM.PartialExport.init(ASC.CRM.ListDealView.advansedFilter, "opportunity");
        },

        setFilter: function(evt, $container, filter, params, selectedfilters) { _changeFilter(); },
        resetFilter: function(evt, $container, filter, selectedfilters) { _changeFilter(); },

        getFilterSettings: function(startIndex) {
            startIndex = startIndex || 0;

            var settings = {
                startIndex: startIndex,
                count: ASC.CRM.ListDealView.entryCountOnPage
            };

            if (!ASC.CRM.ListDealView.advansedFilter) return settings;

            var param = ASC.CRM.ListDealView.advansedFilter.advansedFilter();

            jq(param).each(function(i, item) {
                switch (item.id) {
                    case "sorter":
                        settings.sortBy = item.params.id;
                        settings.sortOrder = item.params.dsc == true ? 'descending' : 'ascending';
                        break;
                    case "text":
                        settings.filterValue = item.params.value;
                        break;
                    case "fromToDate":
                        settings.fromDate = new Date(item.params.from);
                        settings.toDate = new Date(item.params.to);
                        break;
                    default:
                        if (item.hasOwnProperty("apiparamname") && item.params.hasOwnProperty("value") && item.params.value != null) {
                            try {
                                var apiparamnames = jq.parseJSON(item.apiparamname),
                                    apiparamvalues = jq.parseJSON(item.params.value);
                                if (apiparamnames.length != apiparamvalues.length) {
                                    settings[item.apiparamname] = item.params.value;
                                }
                                for (var i = 0, len = apiparamnames.length; i < len; i++) {
                                    settings[apiparamnames[i]] = apiparamvalues[i];
                                }
                            } catch (err) {
                                settings[item.apiparamname] = item.params.value;
                            }
                        }
                        break;
                }
            });
            return settings;
        },

        _dealItemFactory : function(deal, selectedIDs) {

            deal.isOverdue = false;

            switch (deal.stage.stageType) {
                case 1:
                    deal.closedStatusString = ASC.CRM.Resources.CRMJSResource.SuccessfullyClosed + ": " + deal.actualCloseDateString;
                    deal.classForTitle = "linkHeaderMedium gray-text";
                    break;
                case 2:
                    deal.closedStatusString = ASC.CRM.Resources.CRMJSResource.UnsuccessfullyClosed + ": " + deal.actualCloseDateString;
                    deal.classForTitle = "linkHeaderMedium gray-text";
                    break;
                case 0:
                    deal.closedStatusString = "";
                    if (deal.expectedCloseDateString != "" && deal.expectedCloseDate < new Date()) {
                        deal.isOverdue = true;
                        deal.classForTitle = "linkHeaderMedium red-text";
                    } else {
                        deal.classForTitle = "linkHeaderMedium";
                    }
                    break;
            }

            deal.bidNumberFormat = ASC.CRM.Common.numberFormat(deal.bidValue,
                                  {
                                      before: deal.bidCurrency.symbol,
                                      thousands_sep: " ",
                                      dec_point: ASC.CRM.Data.CurrencyDecimalSeparator
                                  });

            if (typeof (deal.bidValue) != "undefined" && deal.bidValue != 0) {
                if (typeof (deal.perPeriodValue) == "undefined")// || deal.perPeriodValue == 0)
                    deal.perPeriodValue = 0;

                var isExist = false;
                for (var j = 0, len = ASC.CRM.ListDealView.bidList.length; j < len; j++) {
                    if (ASC.CRM.ListDealView.bidList[j].bidCurrencyAbbreviation == deal.bidCurrency.abbreviation) {
                        ASC.CRM.ListDealView.bidList[j].bidValue += deal.bidValue * (deal.perPeriodValue != 0 ? deal.perPeriodValue : 1);
                        isExist = true;
                        break;
                    }
                }

                if (!isExist) {
                    ASC.CRM.ListDealView.bidList.push(
                                      {
                                          bidValue: deal.bidValue * (deal.perPeriodValue != 0 ? deal.perPeriodValue : 1),
                                          bidCurrencyAbbreviation: deal.bidCurrency.abbreviation,
                                          bidCurrencySymbol: deal.bidCurrency.symbol,
                                          isConvertable: deal.bidCurrency.isConvertable
                                      });
                }
            }

            var index = jq.inArray(deal.id, selectedIDs);
            deal.isChecked = index != -1;
        },

        expectedValue: function(bidType, perPeriodPalue) {
            switch (bidType) {
                case 1:
                    return ASC.CRM.Resources.CRMJSResource.BidType_PerHour + " " + jq.format(ASC.CRM.Resources.CRMJSResource.PerPeriodHours, perPeriodPalue);
                case 2:
                    return ASC.CRM.Resources.CRMJSResource.BidType_PerDay + " " + jq.format(ASC.CRM.Resources.CRMJSResource.PerPeriodDays, perPeriodPalue);
                case 3:
                    return ASC.CRM.Resources.CRMJSResource.BidType_PerWeek + " " + jq.format(ASC.CRM.Resources.CRMJSResource.PerPeriodWeeks, perPeriodPalue);
                case 4:
                    return ASC.CRM.Resources.CRMJSResource.BidType_PerMonth + " " + jq.format(ASC.CRM.Resources.CRMJSResource.PerPeriodMonths, perPeriodPalue);
                case 5:
                    return ASC.CRM.Resources.CRMJSResource.BidType_PerYear + " " + jq.format(ASC.CRM.Resources.CRMJSResource.PerPeriodYears, perPeriodPalue);
                default:
                    return "";
            }
        },

        changeCountOfRows: function(newValue) {
            if (isNaN(newValue)) return;
            var newCountOfRows = newValue * 1;
            ASC.CRM.ListDealView.entryCountOnPage = newCountOfRows;
            dealPageNavigator.EntryCountOnPage = newCountOfRows;

            _setCookie(1, newCountOfRows);
            _renderContent(0);
        },

        showExchangeRatePopUp: function() {
            if (ASC.CRM.ListDealView.bidList.length == 0) return;

            ASC.CRM.ExchangeRateView.init(ASC.CRM.ListDealView.bidList);
            jq("#ExchangeRateTabs>a:first").click();
            PopupKeyUpActionProvider.EnableEsc = false;
            StudioBlockUIManager.blockUI('#exchangeRatePopUp', 550);
        },

        selectAll: function(obj) {
            var isChecked = jq(obj).is(":checked"),
                selectedIDs = new Array();

            for (var i = 0, n = ASC.CRM.ListDealView.selectedItems.length; i < n; i++) {
                selectedIDs.push(ASC.CRM.ListDealView.selectedItems[i].id);
            }

            for (var i = 0, len = ASC.CRM.ListDealView.dealList.length; i < len; i++) {
                var deal = ASC.CRM.ListDealView.dealList[i],
                    index = jq.inArray(deal.id, selectedIDs);
                if (isChecked && index == -1) {
                    ASC.CRM.ListDealView.selectedItems.push(_createShortDeal(deal));
                    selectedIDs.push(deal.id);
                    jq("#dealItem_" + deal.id).addClass("selected");
                    jq("#checkDeal_" + deal.id).prop("checked", true);
                }
                if (!isChecked && index != -1) {
                    ASC.CRM.ListDealView.selectedItems.splice(index, 1);
                    selectedIDs.splice(index, 1);
                    jq("#dealItem_" + deal.id).removeClass("selected");
                    jq("#checkDeal_" + deal.id).prop("checked", false);
                }
            }
            _renderCheckedDealsCount(ASC.CRM.ListDealView.selectedItems.length);
            _checkForLockMainActions();
        },

        selectItem: function(obj) {
            var id = parseInt(jq(obj).attr("id").split("_")[1]),
                selectedDeal = null,
                selectedIDs = [],
                index = 0;

            for (var i = 0, n = ASC.CRM.ListDealView.dealList.length; i < n; i++) {
                if (id == ASC.CRM.ListDealView.dealList[i].id) {
                    selectedDeal = _createShortDeal(ASC.CRM.ListDealView.dealList[i]);
                }
            }

            for (var i = 0, n = ASC.CRM.ListDealView.selectedItems.length; i < n; i++) {
                selectedIDs.push(ASC.CRM.ListDealView.selectedItems[i].id);
            }
            index = jq.inArray(id, selectedIDs);

            if (jq(obj).is(":checked")) {
                jq(obj).parents("tr:first").addClass("selected");
                if (index == -1) {
                    ASC.CRM.ListDealView.selectedItems.push(selectedDeal);
                }
                ASC.CRM.ListDealView.checkFullSelection();
            } else {
                jq("#mainSelectAllDeals").prop("checked", false);
                jq(obj).parents("tr:first").removeClass("selected");
                if (index != -1) {
                    ASC.CRM.ListDealView.selectedItems.splice(index, 1);
                }
            }
            _renderCheckedDealsCount(ASC.CRM.ListDealView.selectedItems.length);
            _checkForLockMainActions();
        },

        deselectAll: function() {
            ASC.CRM.ListDealView.selectedItems = [];
            _renderCheckedDealsCount(0);
            jq("#dealTable input:checkbox").prop("checked", false);
            jq("#mainSelectAllDeals").prop("checked", false);
            jq("#dealTable tr.selected").removeClass("selected");
            _lockMainActions();
        },

        checkFullSelection: function() {
            var rowsCount = jq("#dealTable tbody tr").length,
                selectedRowsCount = jq("#dealTable input[id^=checkDeal_]:checked").length;
            jq("#mainSelectAllDeals").prop("checked", rowsCount == selectedRowsCount);
        },

        deleteBatchDeals: function() {
            var ids = [],
                params = null;
            jq("#deleteDealsPanel input:checked").each(function() {
                ids.push(parseInt(jq(this).attr("id").split("_")[1]));
            });
            params = { dealsIDsForDelete: ids };

            Teamlab.removeCrmOpportunity(params, ids,
                {
                    success: callback_delete_batch_opportunities,
                    before: function(params) {
                        LoadingBanner.strLoading = ASC.CRM.Resources.CRMDealResource.DeletingDeals;
                        LoadingBanner.showLoaderBtn("#deleteDealsPanel");
                    },
                    after: function(params) {
                        LoadingBanner.hideLoaderBtn("#deleteDealsPanel");
                    }
                });
        },

        initConfirmationPanelForDelete: function (title, dealID, isListView) {
            jq.tmpl("template-blockUIPanel", {
                id: "confirmationDeleteOneDealPanel",
                headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
                questionText: "",
                innerHtmlText:
                ["<div class=\"confirmationAction\">",
                    "<b></b>",
                "</div>",
                "<div class=\"confirmationNote\">",
                    ASC.CRM.Resources.CRMJSResource.DeleteConfirmNote,
                "</div>"].join(''),
                OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
                CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
                progressText: ASC.CRM.Resources.CRMJSResource.DeleteDealInProgress
            }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");

        },

        showConfirmationPanelForDelete: function(title, dealID, isListView) {
            jq("#confirmationDeleteOneDealPanel .confirmationAction>b").text(jq.format(ASC.CRM.Resources.CRMJSResource.DeleteDealConfirmMessage, Encoder.htmlDecode(title)));

            jq("#confirmationDeleteOneDealPanel .middle-button-container>.button.blue.middle").unbind("click").bind("click", function () {
                ASC.CRM.ListDealView.deleteDeal(dealID, isListView);
            });
            PopupKeyUpActionProvider.EnableEsc = false;
            StudioBlockUIManager.blockUI("#confirmationDeleteOneDealPanel", 500);
        },

        deleteDeal: function(dealID, isListView) {
            if (isListView === true) {
                var ids =[];
                ids.push(dealID);
                var params = { dealsIDsForDelete: ids };
                Teamlab.removeCrmOpportunity(params, ids, callback_delete_batch_opportunities);
            } else {
                var contact_id = jq.trim(jq.getURLParam("contact_id"));

                Teamlab.removeCrmOpportunity({ contact_id: contact_id }, dealID, {
                    before: function () {
                        LoadingBanner.strLoading = ASC.CRM.Resources.CRMJSResource.DeleteDealInProgress;
                        LoadingBanner.showLoaderBtn("#confirmationDeleteOneDealPanel");

                        jq("#crm_dealMakerDialog input, #crm_dealMakerDialog select, #crm_dealMakerDialog textarea").attr("disabled", true);
                        LoadingBanner.strLoading = ASC.CRM.Resources.CRMJSResource.DeleteDealInProgress;
                        LoadingBanner.showLoaderBtn("#crm_dealMakerDialog");
                    },
                    success: function (params, opportunity) {
                        ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                        if (params.contact_id != "") {
                            location.href = jq.format("Default.aspx?id={0}#deals", params.contact_id);
                        } else {
                            location.href = "Deals.aspx";
                        }
                    }
                });
            }
        },

        addNewTag: function() {
            var newTag = jq("#addTagDealsDialog input").val().trim();
            if (newTag == "") {
                return false;
            }

            var params = {
                tagName: newTag,
                isNewTag: true
            };
            _addTag(params);
        }
    };
})();


ASC.CRM.DealActionView = (function() {

    var initFields = function() {
        if (typeof (window.customFieldList) != "undefined" && window.customFieldList.length != 0) {
            ASC.CRM.Common.renderCustomFields(customFieldList, "custom_field_", "customFieldRowTmpl", "#crm_dealMakerDialog .deal_info dl:last");
        }
        jq.registerHeaderToggleClick("#crm_dealMakerDialog .deal_info", "dt.headerToggleBlock");
        jq("#crm_dealMakerDialog .deal_info dt.headerToggleBlock").each(
                function() {
                    jq(this).nextUntil("dt.headerToggleBlock").hide();
                });

        ASC.CRM.Common.initTextEditCalendars();

        jq.forceNumber({
            parent: "#crm_dealMakerDialog",
            input: "#perPeriodValue, #probability",
            integerOnly: true,
            positiveOnly: true
        });

        jq("#probability").focusout(function(e) {
            var probability = jq.trim(jq("#probability").val());
            if (probability != "" && probability * 1 > 100) {
                jq("#probability").val(100);
            }
        });

        jq.forceNumber({
            parent: "#crm_dealMakerDialog",
            input: "#bidValue",
            integerOnly: false,
            positiveOnly: true,
            separator: ASC.CRM.Data.CurrencyDecimalSeparator,
            lengthAfterSeparator: null
        });

        for (var i = 0, n = window.dealMilestones.length; i < n; i++) {
            var dealMilestone = window.dealMilestones[i];

            jq("<option>")
                .attr("value", dealMilestone.id)
                .text(dealMilestone.title)
                .appendTo(jq("#dealMilestone"));
        }
        jq("#probability").val(window.dealMilestones[jq("#dealMilestone")[0].selectedIndex].probability);
    };

    var initOtherActionMenu = function() {
        jq("#menuCreateNewTask").bind("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    var initConfirmationGotoSettingsPanel = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "confirmationGotoSettingsPanel",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: "",
            innerHtmlText:
            ["<div class=\"confirmationNote\">",
                ASC.CRM.Resources.CRMJSResource.ConfirmGoToCustomFieldPage,
            "</div>"].join(''),
            OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
            OKBtnHref: "Settings.aspx?type=custom_field#opportunity",
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ""
        }).insertAfter("#otherDealsCustomFieldPanel");

        jq("#confirmationGotoSettingsPanel .button.blue").on("click", function () {
            ASC.CRM.Common.unbindOnbeforeUnloadEvent();
        });
    };

    var initDealMembersAndClientSelectors = function () {
        jq(window).bind("contactSelectorIsReady", function (event, objName) {
            if (objName == "dealClientSelector") {
                jq("#selectorContent_dealClientSelector_0 .crm-addNewLink").parent().remove();
                jq("#newContactContent_dealClientSelector_0 .crm-addNewLink").remove();
                jq("#infoContent_dealClientSelector_0 .crm-addNewLink").removeAttr("onclick");

                jq("#infoContent_dealClientSelector_0 .crm-addNewLink").click(function () {
                    jq("#item_dealClientSelector_0").addClass("hasMembers");
                    if (jq("div[id^='item_dealMemberSelector_']").length == 0) {
                        window.dealMemberSelector.AddNewSelector(jq(this));
                    }
                    jq("#dealMembersHeader").show();
                    jq("#dealMembersBody").show();
                });
            }
            if (objName == "dealMemberSelector") {
                if (window.dealMemberSelector.SelectedContacts.length != 0) {
                    jq("#item_dealClientSelector_0").addClass("hasMembers");
                }
            }
        });


        window["dealClientSelector"] = new ASC.CRM.ContactSelector.ContactSelector("dealClientSelector",
        {
            SelectorType: ASC.CRM.Data.ContactSelectorTypeEnum.CompaniesAndPersonsWithoutCompany,
            EntityType: 0,
            EntityID: 0,
            ShowOnlySelectorContent: false,
            DescriptionText: ASC.CRM.Resources.CRMCommonResource.FindContactByName,
            DeleteContactText: ASC.CRM.Resources.CRMCommonResource.DeleteParticipant,
            AddContactText: "",
            IsInPopup: false,
            ShowChangeButton: window.hasDealTargetClient ? false : true,
            ShowAddButton: true,
            ShowDeleteButton: false,
            ShowContactImg: true,
            ShowNewCompanyContent: true,
            ShowNewContactContent: true,
            HTMLParent: "#dealClientContainer",
            ExcludedArrayIDs: window.dealMembersIDs,
            presetSelectedContactsJson: window.presetClientContactsJson
        });

        window["dealMemberSelector"] = new ASC.CRM.ContactSelector.ContactSelector("dealMemberSelector",
        {
            SelectorType: ASC.CRM.Data.ContactSelectorTypeEnum.All,
            EntityType: 0,
            EntityID: 0,
            ShowOnlySelectorContent: true,
            DescriptionText: ASC.CRM.Resources.CRMCommonResource.FindContactByName,
            DeleteContactText: ASC.CRM.Resources.CRMCommonResource.DeleteParticipant,
            AddContactText: ASC.CRM.Resources.CRMCommonResource.AddParticipant,
            IsInPopup: false,
            ShowChangeButton: false,
            ShowAddButton: false,
            ShowDeleteButton: false,
            ShowContactImg: false,
            ShowNewCompanyContent: true,
            ShowNewContactContent: true,
            HTMLParent: "#dealMembersBody",
            ExcludedArrayIDs: window.dealClientIDs,
            presetSelectedContactsJson: window.presetMemberContactsJson
        });


        ASC.CRM.ListContactView.renderSimpleContent(true, false);
        jq(window).on("renderSimpleContactListReady", function () {
            if (window.dealClientIDs != null && window.dealClientIDs.length == 1) {
                jq("#contactItem_" + window.dealClientIDs[0]).remove();
            }
            if (window.dealMemberSelector.SelectedContacts.length > 0) {
                jq(".assignedDealContacts").removeClass('hiddenFields');
            }
        });

        window.dealMemberSelector.SelectItemEvent = _addContactToDeal;
        ASC.CRM.ListContactView.removeMember = _removeContactFromDeal;

        if (window.showMembersPanel === true) {
            jq("#dealMembersHeader").show();
            jq("#dealMembersBody").show();
        }

        jq(window).bind("editContactInSelector deleteContactFromSelector", function (event, $itemObj, objName) {
            if (objName == "dealClientSelector")
                window.dealMemberSelector.ExcludedArrayIDs = [];
        });

        jq("#infoContent_dealClientSelector_0 .crm-removeLink").bind('click', function () {
            window.dealMemberSelector.ExcludedArrayIDs = [];
        });
        
        window.dealClientSelector.SelectItemEvent = _chooseMainContact;
    };

    var initResponsibleSelector = function () {

        jq("#advUserSelectorResponsible").useradvancedSelector({
            showme: false,
            inPopup: false,
            onechosen: true,
            showGroups: true,
            withGuests: false
        });

        if (ASC.CRM.Data.isCrmAvailableForAllUsers == false) {
            var users = [],
                items = jq("#advUserSelectorResponsible").data("items");

            for (var i = 0, n = ASC.CRM.Data.crmAvailableWithAdminList.length; i < n; i++) {
                for (var j = 0, m = items.length; j < m; j++) {
                    if (ASC.CRM.Data.crmAvailableWithAdminList[i].id == items[j].id) {
                        users.push(items[j]);
                        continue;
                    }
                }
            }
            jq("#advUserSelectorResponsible").useradvancedSelector("rewriteItemList", users, []);
        }

        var items = jq("#advUserSelectorResponsible").data("items") || [],
            responsible = null;
        for (var i = 0, n = items.length; i < n; i++) {
            if (items[i].id == window.responsibleId) {
                responsible = items[i];
                break;
            }
        }
        if (responsible != null) {
            jq("#advUserSelectorResponsible").useradvancedSelector("select", [window.responsibleId]);
            jq("#advUserSelectorResponsible").attr("data-responsible-id", window.responsibleId);
            jq("#advUserSelectorResponsible .dealResponsibleLabel").text(Encoder.htmlDecode(responsible.title));
        } else {
            responsible = window.UserManager.getUser(window.responsibleId);
            if (responsible != null) {
                jq("#advUserSelectorResponsible").attr("data-responsible-id", responsible.id);
                jq("#advUserSelectorResponsible .dealResponsibleLabel").text(Encoder.htmlDecode(responsible.displayName + (responsible.isTerminated ? " (" + ASC.CRM.Resources.DisabledEmployeeTitle.toLowerCase() + ")" : "")));
            } else {
                responsible = window.UserManager.getRemovedProfile();
                jq("#advUserSelectorResponsible").attr("data-responsible-id", "");
                jq("#advUserSelectorResponsible .dealResponsibleLabel").text(Encoder.htmlDecode(responsible.displayName));
            }
        }

        jq("#advUserSelectorResponsible").on("showList", function (event, item) {
            ASC.CRM.Common.bindOnbeforeUnloadEvent();
            jq("#advUserSelectorResponsible").attr("data-responsible-id", item.id)
            jq("#advUserSelectorResponsible .dealResponsibleLabel").text(Encoder.htmlDecode(item.title));
        });

    };

    var renderDealInfo = function() {

        jq("#nameDeal").val(window.targetDeal.title);
        jq("#descriptionDeal").val(window.targetDeal.description);
        jq("#bidValue").val(window.targetDeal.bid_value.toString().replace(/[\.\,]/g, ASC.CRM.Data.CurrencyDecimalSeparator));

        jq(jq.format("#bidCurrency option[value={0}]", window.targetDeal.bid_currency)).prop("selected", "selected");

        if (window.targetDeal.bid_type > 0) {
            jq(jq.format("#bidType [value={0}]", window.targetDeal.bid_type)).prop("selected", "selected");
            jq("#bidType").nextAll().show();

            jq("#perPeriodValue").val(window.targetDeal.per_period_value);
            jq("#bidType").change();
        }

        jq("#probability").val(window.targetDeal.deal_milestone_probability);
        jq("#expectedCloseDate").val(window.targetDeal.expected_close_date);

        jq(jq.format("#dealMilestone option[value={0}]", window.targetDeal.deal_milestone)).prop("selected", "selected");

        jq("#contactID").val("contact_id");

        jq("#deleteDealButton").unbind("click").bind("click", function() {
            ASC.CRM.ListDealView.showConfirmationPanelForDelete(jq.htmlEncodeLight(window.targetDeal.title), window.targetDeal.id, false);
        });
    };

    var _bindLeaveThePageEvent = function () {
        jq("#crm_dealMakerDialog").on("keyup change paste", "input, select, textarea", function (e) {
            ASC.CRM.Common.bindOnbeforeUnloadEvent();
        });
        jq("#crm_dealMakerDialog").on("click", ".crm-deleteLink", function (e) {
            ASC.CRM.Common.bindOnbeforeUnloadEvent();
        });
        jq(window).on("addTagComplete deleteTagComplete setContactInSelector editContactInSelector deleteContactFromSelector", function (e) {
            ASC.CRM.Common.bindOnbeforeUnloadEvent();
        });
    };

    var _addContactToDeal = function (obj, params) {
        if (jq("#contactItem_" + obj.id).length > 0) return false;

        if (params.newContact == true) {
            var contact = {
                canEdit: true,
                displayName: obj.displayName,
                id: obj.id,
                isCompany: obj.smallFotoUrl.indexOf("isc=true") != -1,
                isPrivate: false,
                isShared: false,
                shareType: 0,
                smallFotoUrl: obj.smallFotoUrl
            };

            ASC.CRM.Common.contactItemFactory(contact, { showUnlinkBtn: true, showActionMenu: false });
            jq.tmpl("simpleContactTmpl", contact).prependTo("#contactTable tbody");
            jq(".assignedDealContacts").removeClass('hiddenFields');
            window.dealMemberSelector.SelectedContacts.push(contact.id);
            window.dealClientSelector.ExcludedArrayIDs.push(contact.id);
            window.dealClientSelector.Cache = {};
            ASC.CRM.Common.RegisterContactInfoCard();
        } else {

            Teamlab.getCrmContact({}, obj.id, {
                success: function (par, contact) {
                    ASC.CRM.Common.contactItemFactory(contact, { showUnlinkBtn: true, showActionMenu: false });
                    jq.tmpl("simpleContactTmpl", contact).prependTo("#contactTable tbody");
                    jq(".assignedDealContacts").removeClass('hiddenFields');
                    ASC.CRM.Common.RegisterContactInfoCard();

                    window.dealMemberSelector.SelectedContacts.push(contact.id);
                    window.dealClientSelector.ExcludedArrayIDs.push(contact.id);
                    //window.dealClientSelector.Cache = {};
                }
            });
        }
    };

    var _removeContactFromDeal = function (id) {
        if (jq("#trashImg_" + id).length == 1) {
            jq("#trashImg_" + id).hide();
            jq("#loaderImg_" + id).show();
        }

        window.dealClientSelector.Cache = {};
        window.dealMemberSelector.Cache = {};
        if (window.dealMemberSelector.SelectedContacts.length == 1) {
            jq("#dealMembersHeader").hide();
            jq("#dealMembersBody").hide();
            jq(".assignedDealContacts").addClass("hiddenFields");
            jq("#item_dealClientSelector_0").removeClass("hasMembers");
        }

        var index = jq.inArray(id, window.dealMemberSelector.SelectedContacts);
        if (index != -1) {
            window.dealMemberSelector.SelectedContacts.splice(index, 1);
        } else {
            console.log("Can't find such contact in list");
        }
        window.dealClientSelector.ExcludedArrayIDs = jq.merge([], window.dealMemberSelector.SelectedContacts);

        jq("#contactItem_" + id).animate({ opacity: "hide" }, 500);

        setTimeout(function () {
            jq("#contactItem_" + id).remove();
            if (jq("#contactTable tr").length == 0) {
                jq(".assignedDealContacts").addClass('hiddenFields');
            }
        }, 500);
    };

    var _chooseMainContact = function(obj, params) {
        window.dealClientSelector.setContact(params.input, obj.id, obj.displayName, obj.smallFotoUrl);
        window.dealClientSelector.showInfoContent(params.input);
        window.dealMemberSelector.ExcludedArrayIDs = [obj.id];
        //window.dealMemberSelector.Cache = {};
    };

    return {
        init: function (today, errorCookieKey) {

            var saveDealError = jq.cookies.get(errorCookieKey);
            if (saveDealError != null && saveDealError != "") {
                jq.cookies.del(errorCookieKey);
                jq.tmpl("template-blockUIPanel", {
                    id: "saveDealError",
                    headerTest: ASC.CRM.Resources.CRMCommonResource.Alert,
                    questionText: "",
                    innerHtmlText: ['<div>', saveDealError, '</div>'].join(''),
                    CancelBtn: ASC.CRM.Resources.CRMCommonResource.Close,
                    progressText: ""
                }).insertAfter("#crm_dealMakerDialog");

                PopupKeyUpActionProvider.EnableEsc = false;
                StudioBlockUIManager.blockUI("#saveDealError", 500);
            }

            initFields();
            initOtherActionMenu();
            ASC.CRM.ListDealView.initConfirmationPanelForDelete();
            if (ASC.CRM.Data.IsCRMAdmin === true) {
                initConfirmationGotoSettingsPanel();
            }

            var dealID = parseInt(jq.getURLParam("id"));
            if (!isNaN(dealID) && typeof (window.targetDeal) === "object") {
                renderDealInfo();
            } else {
                jq("#expectedCloseDate").val(today);
            }

            initDealMembersAndClientSelectors();
            initResponsibleSelector();

            jq(".cancelSbmtFormBtn:first").on("click", function () {
                ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                return true;
            });

            _bindLeaveThePageEvent();
        },

        selectBidTypeEvent: function(selectObj) {
            var idx = selectObj.selectedIndex;
            if (idx != 0) {
                jq(selectObj).nextAll().show();
            } else {
                jq(selectObj).nextAll().hide();
            }

            var elems = jq(selectObj).nextAll('span.splitter'),
                resourceValue = "";
            switch (idx) {
                case 1:
                    resourceValue = ASC.CRM.Resources.CRMJSResource.PerPeriodHours;
                    break;
                case 2:
                    resourceValue = ASC.CRM.Resources.CRMJSResource.PerPeriodDays;
                    break;
                case 3:
                    resourceValue = ASC.CRM.Resources.CRMJSResource.PerPeriodWeeks;
                    break;
                case 4:
                    resourceValue = ASC.CRM.Resources.CRMJSResource.PerPeriodMonths;
                    break;
                case 5:
                    resourceValue = ASC.CRM.Resources.CRMJSResource.PerPeriodYears;
                    break;
            }

            var labels = resourceValue.split("{0}");

            jq(elems).each(function(index) {
                jq(this).text(jq.trim(labels[index]));
            });

        },

        submitForm: function () {
            if (jq("[id*=saveDealButton]:first").hasClass("postInProcess")) {
                return false;
            }
            jq("[id*=saveDealButton]:first").addClass("postInProcess");

            try {
                var isValid = true;

                if (jq.trim(jq("#nameDeal").val()) == "") {
                    ShowRequiredError(jq("#nameDeal"));
                    isValid = false;
                } else {
                    RemoveRequiredErrorClass(jq("#nameDeal"));
                }

                var responsibleID = jq("#advUserSelectorResponsible").attr("data-responsible-id");
                if (responsibleID == "") {
                    ShowRequiredError(jq("#advUserSelectorResponsible"));
                    isValid = false;
                } else {
                    RemoveRequiredErrorClass(jq("#advUserSelectorResponsible"));
                }

                if (!isValid) {
                    jq("[id*=saveDealButton]:first").removeClass("postInProcess");
                    return false;
                }

                var dealMilestoneProbability = jq.trim(jq("#probability").val());

                if (dealMilestoneProbability == "") {
                    dealMilestoneProbability = 0;
                    jq("#probability").val(0);
                } else {
                    dealMilestoneProbability = dealMilestoneProbability * 1;
                    if (dealMilestoneProbability > 100) {
                        jq("#probability").val(100);
                    }
                }

                jq("#crm_dealMakerDialog input, #crm_dealMakerDialog select, #crm_dealMakerDialog textarea")
                    .prop("readonly", "readonly")
                    .addClass('disabled');

                jq("#responsibleID").val(responsibleID);

                if (window.dealClientSelector.SelectedContacts.length > 0) {
                    jq("#selectedContactID").val(window.dealClientSelector.SelectedContacts[0]);
                } else {
                    jq("#selectedContactID").val(0);
                }

                jq("#selectedMembersID").val(window.dealMemberSelector.SelectedContacts.join(","));

                if (jq.getURLParam("id") != null) {
                    LoadingBanner.strLoading = ASC.CRM.Resources.CRMJSResource.EditingDealProgress;
                } else {
                    LoadingBanner.strLoading = ASC.CRM.Resources.CRMJSResource.AddNewDealProgress;
                }
                LoadingBanner.showLoaderBtn("#crm_dealMakerDialog");


                if (jq("#crm_dealMakerDialog .dealPrivatePanel").length == 1) {
                    if (!jq("#isPrivate").is(":checked")) {
                        window.SelectedUsers.IDs = new Array();
                        jq("#cbxNotify").removeAttr("checked");
                    }

                    jq("#isPrivateDeal").val(jq("#isPrivate").is(":checked"));
                    jq("#notifyPrivateUsers").val(jq("#cbxNotify").is(":checked"));
                    jq("#selectedPrivateUsers").val(window.SelectedUsers.IDs.join(","));
                }

                var $checkboxes = jq("#crm_dealMakerDialog .deal_info input[type='checkbox'][id^='custom_field_']");
                if ($checkboxes) {
                    for (var i = 0; i < $checkboxes.length; i++) {
                        if (jq($checkboxes[i]).is(":checked")) {
                            var id = $checkboxes[i].id.replace('custom_field_', '');
                            jq("#crm_dealMakerDialog .deal_info input[name='customField_" + id + "']").val(jq($checkboxes[i]).is(":checked"));
                        }
                    }
                }
                ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                return true;
            } catch (e) {
                console.log(e);
                jq("[id*=saveDealButton]:first").removeClass("postInProcess");
                return false;
            }
        },

        showGotoAddSettingsPanel: function () {
            if (window.onbeforeunload == null) {//No need the confirmation
                location.href = "Settings.aspx?type=custom_field#opportunity";
            } else {
                PopupKeyUpActionProvider.EnableEsc = false;
                StudioBlockUIManager.blockUI("#confirmationGotoSettingsPanel", 500);
            }
        }
    };
})();

ASC.CRM.DealFullCardView = (function() {
    var _cookiePath = "/";
    var _cookieToggledBlocksKey = "dealFullCardToggledBlocks";

    var initToggledBlocks = function () {
        jq.registerHeaderToggleClick("#dealProfile .crm-detailsTable", "tr.headerToggleBlock");

        jq("#dealProfile .crm-detailsTable").on("click", ".headerToggle, .openBlockLink, .closeBlockLink", function () {
            var $cur = jq(this).parents("tr.headerToggleBlock:first"),
                toggleid = $cur.attr("data-toggleid"),
                isopen = $cur.hasClass("open"),
                toggleObjStates = jq.cookies.get(_cookieToggledBlocksKey);

            if (toggleObjStates != null) {
                toggleObjStates[toggleid] = isopen;
            } else {
                toggleObjStates = {};

                var $list = jq("#dealProfile .crm-detailsTable tr.headerToggleBlock");
                for (var i = 0, n = $list.length; i < n; i++) {
                    toggleObjStates[jq($list[i]).attr("data-toggleid")] = jq($list[i]).hasClass("open");
                }
            }
            jq.cookies.set(_cookieToggledBlocksKey, toggleObjStates, { path: _cookiePath });
        });

        var toggleObjStates = jq.cookies.get(_cookieToggledBlocksKey);
        if (toggleObjStates != null) {
            var $list = jq("#dealProfile .crm-detailsTable tr.headerToggleBlock");
            for (var i = 0, n = $list.length; i < n; i++) {
                var toggleid = jq($list[i]).attr("data-toggleid");
                if (toggleObjStates.hasOwnProperty(toggleid) && toggleObjStates[toggleid] === true) {
                    jq($list[i]).addClass("open");
                }
            }
        } else {
            jq("#dealHistoryTable .headerToggleBlock").addClass("open");
        }

        jq("#dealProfile .headerToggle").not("#dealProfile .headerToggleBlock.open .headerToggle").each(
               function () {
                   jq(this).parents("tr.headerToggleBlock:first").nextUntil(".headerToggleBlock").hide();
               });

    };

    var renderCustomFields = function() {
        if (typeof (window.customFieldList) != "undefined" && window.customFieldList.length != 0) {
            var sortedList = ASC.CRM.Common.sortCustomFieldList(window.customFieldList);
            jq.tmpl("customFieldListTmpl", sortedList).insertBefore("#dealHistoryTable");
        }
    };

    return {
        init: function () {
            for (var i = 0, n = window.dealTags.length; i < n; i++) {
                window.dealTags[i] = Encoder.htmlDecode(window.dealTags[i]);
            }
            for (var i = 0, n = window.dealAvailableTags.length; i < n; i++) {
                window.dealAvailableTags[i] = Encoder.htmlDecode(window.dealAvailableTags[i]);
            }

            jq.tmpl("tagViewTmpl",
                    { tags: window.dealTags,
                      availableTags: window.dealAvailableTags
                    })
                    .appendTo("#dealTagsTD");
            ASC.CRM.TagView.init("opportunity", false);

            if (typeof (window.dealResponsibleIDs) != "undefined" && window.dealResponsibleIDs.length != 0) {
                jq("#dealProfile .dealAccessList").html(ASC.CRM.Common.getAccessListHtml(window.dealResponsibleIDs));
            }
            renderCustomFields();

            ASC.CRM.Common.RegisterContactInfoCard();

            initToggledBlocks();

            jq.dropdownToggle({
                dropdownID: 'dealMilestoneDropDown',
                switcherSelector: '#dealMilestoneSwitcher',
                addTop: 0,
                showFunction: function(switcherObj, dropdownItem) {
                    var left = parseInt(dropdownItem.css("left")) + jq("#dealMilestoneSwitcher").width() - 12;
                    dropdownItem.css("left", left);
                }
            });
        },

        changeDealMilestone: function(dealID, milestoneID) {
            Teamlab.updateCrmOpportunityMilestone({}, dealID, milestoneID, function(params, deal) {
                jq("#dealMilestoneDropDown").hide();
                var dealMilestone = deal.stage;
                jq("#dealMilestoneSwitcher").text(dealMilestone.title);
                jq("#dealMilestoneProbability").text(dealMilestone.successProbability + "%");

                if (dealMilestone.stageType != 0) { //closed = not open
                    jq("#closeDealDate").text(deal.actualCloseDateString);
                    jq("#closeDealDate").parent().removeClass("display-none");
                } else { //opened
                    jq("#closeDealDate").text(ASC.CRM.Resources.CRMJSResource.NoCloseDate);
                    jq("#closeDealDate").parent().addClass("display-none");
                }
            });
        }
    };
})();

ASC.CRM.DealDetailsView = (function() {
    var _availableTabs = ["profile", "tasks", "contacts", "invoices", "files"];

    var _getCurrentTabAnch = function () {
        var anch = ASC.Controls.AnchorController.getAnchor();
        if (anch == null || anch == "" || jq.inArray(anch, _availableTabs) == -1) { anch = "profile"; }
        return anch;
    };

    var initTabs = function (currentTabAnch) {
        window.ASC.Controls.ClientTabsNavigator.init("DealTabs", {
            tabs: [
            {
                title: ASC.CRM.Resources.CRMCommonResource.Profile,
                selected: currentTabAnch == "profile",
                anchor: "profile",
                divID: "profileTab",
                onclick: "ASC.CRM.DealDetailsView.activateCurrentTab('profile');"
            },
            {
                title: ASC.CRM.Resources.CRMTaskResource.Tasks,
                selected: currentTabAnch == "tasks",
                anchor: "tasks",
                divID: "tasksTab",
                onclick: "ASC.CRM.DealDetailsView.activateCurrentTab('tasks');"
            },
            {
                title: ASC.CRM.Resources.CRMDealResource.PeopleInDeal,
                selected: currentTabAnch == "contacts",
                anchor: "contacts",
                divID: "contactsTab",
                onclick: "ASC.CRM.DealDetailsView.activateCurrentTab('contacts');"
            },
            {
                title: ASC.CRM.Resources.CRMInvoiceResource.Invoices,
                selected: currentTabAnch == "invoices",
                anchor: "invoices",
                divID: "invoicesTab",
                onclick: "ASC.CRM.DealDetailsView.activateCurrentTab('invoices');"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.Documents,
                selected: currentTabAnch == "files",
                anchor: "files",
                divID: "filesTab",
                onclick: "ASC.CRM.DealDetailsView.activateCurrentTab('files');"
            }]
        });
    };

    var initAttachments = function () {
        window.Attachments.init();
        window.Attachments.bind("addFile", function(ev, file) {
            //ASC.CRM.Common.changeCountInTab("add", "files");
            var dealID = jq.getURLParam("id") * 1,
                type = "opportunity",
                fileids = [];
            fileids.push(file.id);

            Teamlab.addCrmEntityFiles({}, dealID, type, {
                entityid: dealID,
                entityType: type,
                fileids: fileids
            },
                    {
                        success: function(params, data) {
                            window.Attachments.appendFilesToLayout(data.files);
                            params.fromAttachmentsControl = true;
                            ASC.CRM.HistoryView.isTabActive = false;
                        }
                    });
        });

        window.Attachments.bind("deleteFile", function(ev, fileId) {
            var $fileLinkInHistoryView = jq("#fileContent_" + fileId);
            if ($fileLinkInHistoryView.length != 0) {
                var messageID = $fileLinkInHistoryView.parents("[id^=eventAttach_]").attr("id").split("_")[1];
                ASC.CRM.HistoryView.deleteFile(fileId, messageID);
            } else {
                Teamlab.removeCrmEntityFiles({ fileId: fileId }, fileId, {
                    success: function(params) {
                        window.Attachments.deleteFileFromLayout(params.fileId);
                        //ASC.CRM.Common.changeCountInTab("delete", "files");
                    }
                });
            }
        });
    };

    var initDealDetailsMenuPanel = function() {
        jq(document).ready(function() {
            jq.dropdownToggle({
                dropdownID: "dealDetailsMenuPanel",
                switcherSelector: ".mainContainerClass .containerHeaderBlock .menu-small",
                addTop: 0,
                addLeft: -10,
                showFunction: function(switcherObj, dropdownItem) {
                    if (dropdownItem.is(":hidden")) {
                        switcherObj.addClass("active");
                    } else {
                        switcherObj.removeClass("active");
                    }
                },
                hideFunction: function() {
                    jq(".mainContainerClass .containerHeaderBlock .menu-small.active").removeClass("active");
                }
            });
        });

        jq("#dealDetailsMenuPanel .createProject").unbind("click").bind("click", function() {
            jq(".mainContainerClass .containerHeaderBlock .menu-small.active").removeClass("active");
            jq("#dealDetailsMenuPanel").hide();
        });
    };

    var initOtherActionMenu = function() {
        var params = {};
        if (window.dealResponsibleIDs.length != 0) {
            params.taskResponsibleSelectorUserIDs = window.dealResponsibleIDs;
        }

        jq("#menuCreateNewTask").bind("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, window.entityData.type, window.entityData.id, null, params); });

        ASC.CRM.ListTaskView.bindEmptyScrBtnEvent(params);
    };

    var initEmptyScreens = function () {
        jq.tmpl("template-emptyScreen",
            {
                ID: "emptyDealParticipantPanel",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_opportunity_participants"],
                Header: ASC.CRM.Resources.CRMDealResource.EmptyPeopleInDealContent,
                Describe: ASC.CRM.Resources.CRMDealResource.EmptyPeopleInDealDescript,
                ButtonHTML: ["<a class='link dotline plus' ",
                    "onclick='javascript:jq(\"#dealParticipantPanel\").show();jq(\"#emptyDealParticipantPanel\").addClass(\"display-none\");'>",
                    ASC.CRM.Resources.CRMCommonResource.AddParticipant,
                    "</a>"].join(''),
                CssClass: "display-none"
            }).insertAfter("#contactListBox");

        jq.tmpl("template-emptyScreen",
            {
                ID: "invoiceEmptyScreen",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_invoices"],
                Header: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoicesHeader,
                Describe: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoicesDescribe
            }).insertAfter("#invoiceTable");
    };

    var initParticipantsContactSelector = function() {
        window["dealContactSelector"] = new ASC.CRM.ContactSelector.ContactSelector("dealContactSelector",
                {
                    SelectorType: -1,
                    EntityType: 0,
                    EntityID: 0,
                    ShowOnlySelectorContent: true,
                    DescriptionText: ASC.CRM.Resources.CRMCommonResource.FindContactByName,
                    DeleteContactText: "",
                    AddContactText: "",
                    IsInPopup: false,
                    ShowChangeButton: false,
                    ShowAddButton: false,
                    ShowDeleteButton: false,
                    ShowContactImg: false,
                    ShowNewCompanyContent: true,
                    ShowNewContactContent: true,
                    presetSelectedContactsJson: '',
                    ExcludedArrayIDs: [],
                    HTMLParent: "#dealParticipantPanel"
                });

        window.dealContactSelector.SelectItemEvent = ASC.CRM.DealDetailsView.addMemberToDeal;
        ASC.CRM.ListContactView.removeMember = ASC.CRM.DealDetailsView.removeMemberFromDeal;

        jq(window).bind("getContactsFromApi", function(event, contacts) {
            var contactLength = contacts.length;
            if (contactLength == 0) {
                jq("#emptyDealParticipantPanel.display-none").removeClass("display-none");
            } else {
                jq("#dealParticipantPanel").show();
                var contactIDs = [];
                for (var i = 0; i < contactLength; i++) {
                    contactIDs.push(contacts[i].id);
                }
                dealContactSelector.SelectedContacts = contactIDs;
            }
        });
    };

    return {
        init: function () {
            var currentTabAnch = _getCurrentTabAnch();
            initTabs(currentTabAnch);

            ASC.CRM.ListContactView.isContentRendered = false;
            ASC.CRM.ListInvoiceView.isContentRendered = false;

            initEmptyScreens();
            initParticipantsContactSelector();

            ASC.CRM.HistoryView.init(0, window.entityData.type, window.entityData.id);
            ASC.CRM.ListTaskView.initTab(0, window.entityData.type, window.entityData.id);

            initAttachments();
            initDealDetailsMenuPanel();
            initOtherActionMenu();

            ASC.CRM.DealDetailsView.activateCurrentTab(currentTabAnch);
        },

        activateCurrentTab: function (anchor) {
            if (anchor == "profile") { }
            if (anchor == "tasks") {
                ASC.CRM.ListTaskView.activate();
            }
            if (anchor == "contacts") {
                if (ASC.CRM.ListContactView.isContentRendered == false) {
                    ASC.CRM.ListContactView.isContentRendered = true;
                    ASC.CRM.ListContactView.renderSimpleContent(false, true);
                }
            }
            if (anchor == "invoices") {
                if (ASC.CRM.ListInvoiceView.isContentRendered == false) {
                    ASC.CRM.ListInvoiceView.isContentRendered = true;
                    ASC.CRM.ListInvoiceView.renderSimpleContent();
                }
            }
            if (anchor == "files") {
                window.Attachments.loadFiles();
            }
        },

        removeMemberFromDeal: function(id) {
            Teamlab.removeCrmEntityMember({ contactID: parseInt(id) }, window.entityData.type, window.entityData.id, id, {
                before: function(params) {
                    jq("#simpleContactActionMenu").hide();
                    jq("#contactTable .entity-menu.active").removeClass("active");
                },
                after: function(params) {
                    var index = jq.inArray(params.contactID, window.dealContactSelector.SelectedContacts);
                    if (index != -1) {
                        window.dealContactSelector.SelectedContacts.splice(index, 1);
                    } else {
                        console.log("Can't find such contact in list");
                    }
                    ASC.CRM.ContactSelector.Cache = {};

                    jq("#contactItem_" + params.contactID).animate({ opacity: "hide" }, 500);

                    ASC.CRM.HistoryView.removeOption("contact", params.contactID);

                    //ASC.CRM.Common.changeCountInTab("delete", "contacts");

                    setTimeout(function() {
                        jq("#contactItem_" + params.contactID).remove();
                        if (window.dealContactSelector.SelectedContacts.length == 0) {
                            jq("#dealParticipantPanel").hide();
                            jq("#emptyDealParticipantPanel.display-none").removeClass("display-none");
                        }
                    }, 500);
                }
            });
        },

        addMemberToDeal: function(obj) {
            if (jq("#contactItem_" + obj.id).length > 0) return false;
            var data =
            {
                contactid: obj.id,
                opportunityid: window.entityData.id
            };
            Teamlab.addCrmEntityMember({
                                        showCompanyLink: true,
                                        showUnlinkBtn: false,
                                        showActionMenu: true
                                    },
                                    window.entityData.type, window.entityData.id, obj.id, data, {
                    success: function(params, contact) {
                        ASC.CRM.ListContactView.CallbackMethods.addMember(params, contact);

                        window.dealContactSelector.SelectedContacts.push(contact.id);
                        //ASC.CRM.ContactSelector.Cache = {};
                        jq("#emptyDealParticipantPanel:not(.display-none)").addClass("display-none");
                        ASC.CRM.HistoryView.appendOption("contact", { value: contact.id, title: contact.displayName });
                    }
                });
        }
    };
})();

ASC.CRM.ExchangeRateView = (function() {
    var _currenciesAndRates = null;

    var convertAmount = function () {
        var amount = jq("#amount").val().trim();
        if (amount != "") {
            jq("#introducedAmount").text(amount + " " + jq("#fromCurrency").val());
            jq("#conversionResult").text(amount * 1 * ASC.CRM.ExchangeRateView.Rate + " " + jq("#toCurrency").val());
        } else {
            jq("#introducedAmount").text("");
            jq("#conversionResult").text("");
        }
    };

    var renderBaseControl = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "exchangeRatePopUp",
            headerTest: ASC.CRM.Resources.CRMCommonResource.ConversionRates
        }).appendTo("body");

        jq.tmpl("exchangeRateViewTmpl", { ratesPublisherDisplayDate: ASC.CRM.Data.ratesPublisherDisplayDate })
            .appendTo("#exchangeRatePopUp .containerBodyBlock:first");


        window.ASC.Controls.ClientTabsNavigator.init("ExchangeRateTabs", {
            tabs: [
            {
                title: ASC.CRM.Resources.CRMDealResource.TotalAmount,
                selected: true,
                anchor: "",
                divID: "totalAmountTab"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.MoneyCalculator,
                selected: false,
                anchor: "",
                divID: "converterTab"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.SummaryTable,
                selected: false,
                anchor: "",
                divID: "exchangeTab"
            }]
        });
    };

    var renderTotalAmount = function(bidList) {
        jq("#totalOnPage .diferrentBids").html("");
        jq("#totalOnPage .totalBid").html("");
        var sum = 0,
            tmpBidNumberFormat,
            isTotalBidAndExchangeRateShow = false;

        for (var i = 0, len = bidList.length; i < len; i++) {

            if (bidList[i].bidCurrencyAbbreviation != ASC.CRM.Data.defaultCurrency.abbreviation) {
                isTotalBidAndExchangeRateShow = true;
            }

            tmpBidNumberFormat = ASC.CRM.Common.numberFormat(bidList[i].bidValue,
                {
                    before: bidList[i].bidCurrencySymbol,
                    thousands_sep: " ",
                    dec_point: ASC.CRM.Data.CurrencyDecimalSeparator
                });

            jq.tmpl("bidFormat", { number: tmpBidNumberFormat, abbreviation: bidList[i].bidCurrencyAbbreviation }).appendTo("#totalOnPage .diferrentBids");

            if (!bidList[i].isConvertable) {
                jq.tmpl("bidFormat", { number: tmpBidNumberFormat, abbreviation: bidList[i].bidCurrencyAbbreviation }).appendTo("#totalOnPage .totalBid");
            }

            var tmp_rate = ASC.CRM.ExchangeRateView.ExchangeRates[bidList[i].bidCurrencyAbbreviation];
            if (bidList[i].isConvertable && typeof (tmp_rate) != "undefined") {
                sum += bidList[i].bidValue / tmp_rate;
            }
        }

        tmpBidNumberFormat = ASC.CRM.Common.numberFormat(sum, {
            before:  ASC.CRM.Data.defaultCurrency.symbol,
            thousands_sep: " ",
            dec_point: ASC.CRM.Data.CurrencyDecimalSeparator
        });

        jq.tmpl("bidFormat", { number: tmpBidNumberFormat, abbreviation: ASC.CRM.Data.defaultCurrency.abbreviation }).prependTo("#totalOnPage .totalBid");

        if (isTotalBidAndExchangeRateShow == true) {
            jq("#totalOnPage .totalBidAndExchangeRateLink").show();
        } else {
            jq("#totalOnPage .totalBidAndExchangeRateLink").hide();
        }
        jq("#totalOnPage").show();
    };

    var getDataAndInit = function (bidList) {
        LoadingBanner.displayLoading();

        Teamlab.getCrmCurrencySummaryTable({}, ASC.CRM.Data.defaultCurrency.abbreviation, function (params, tableData) {
            _currenciesAndRates = tableData;

            var html = "",
                tmp_cur = {};

            for (var i = 0, n = _currenciesAndRates.length; i < n; i++) {
                tmp_cur = _currenciesAndRates[i];
                html += [
                    "<option value=\"",
                    tmp_cur.abbreviation,
                    "\"",
                    tmp_cur.abbreviation == ASC.CRM.Data.defaultCurrency.abbreviation ? " selected=\"selected\">" : ">",
                    jq.format("{0} - {1}", tmp_cur.abbreviation, tmp_cur.title),
                    "</option>"].join('');
            }

            jq("#fromCurrency").html(html);
            jq("#toCurrency").html(html);
            jq("#exchangeRateContent select:first").html(html);

            jq.tmpl("ratesTableTmpl", { currencyRates: _currenciesAndRates }).appendTo("#ratesTable");

            jq("#introducedFromCurrency").text(ASC.CRM.Data.defaultCurrency.title + ":");
            jq("#introducedToCurrency").text(ASC.CRM.Data.defaultCurrency.title + ":");

            jq("#conversionRate").text(jq.format("1 {0} = 1 {0}", ASC.CRM.Data.defaultCurrency.abbreviation));

            for (var i = 0, n = _currenciesAndRates.length; i < n; i++) {
                ASC.CRM.ExchangeRateView.ExchangeRates[_currenciesAndRates[i].abbreviation] = _currenciesAndRates[i].rate;
                ASC.CRM.ExchangeRateView.ExchangeRatesNames[_currenciesAndRates[i].abbreviation] = _currenciesAndRates[i].title;
            }
            renderTotalAmount(bidList);

            jq.forceNumber({
                parent: "#convertRateContent",
                input: "#amount",
                integerOnly: true,
                positiveOnly: true,
                onPasteCallback: convertAmount
            });

            jq("#amount").on("keyup", function (event) {
                convertAmount();
            });

            LoadingBanner.hideLoading();
        });
    };

    return {
        Rate : 1,
        ExchangeRates : {},
        ExchangeRatesNames : {},


        init: function (bidList) {
            ASC.CRM.ExchangeRateView.Rate = 1;

            if (jq("#exchangeRatePopUp").length == 0) {
                renderBaseControl();
                getDataAndInit(bidList);
            } else {
                renderTotalAmount(bidList);
            }

        },

        changeCurrency: function() {
            var fromcurrency = jq("#fromCurrency").val(),
                tocurrency = jq("#toCurrency").val(),
                data = {
                    amount: 1,
                    fromcurrency: fromcurrency,
                    tocurrency: tocurrency
                };
            LoadingBanner.displayLoading();
            Teamlab.getCrmCurrencyConvertion({}, data, function(params, currencyRate) {
                ASC.CRM.ExchangeRateView.Rate = currencyRate;
                jq("#conversionRate").text("1 " + fromcurrency + " = " + currencyRate + " " + tocurrency);
                jq("#introducedFromCurrency").text(ASC.CRM.ExchangeRateView.ExchangeRatesNames[fromcurrency] + ":");
                jq("#introducedToCurrency").text(ASC.CRM.ExchangeRateView.ExchangeRatesNames[tocurrency] + ":");
                convertAmount();
                LoadingBanner.hideLoading();
            });
        },

        updateSummaryTable: function(newCurrency) {
            LoadingBanner.displayLoading();
            Teamlab.getCrmCurrencySummaryTable({}, newCurrency, function (params, tableData) {
                var $ratesList = jq("#ratesTable td.rateValue");
                for (var i = 0, n = $ratesList.length; i < n; i++) {
                    var $ratesItem = jq($ratesList[i]),
                        key = $ratesItem.attr("id").toLowerCase(),
                        rate = "";
                    
                    for (var j = 0, m = tableData.length; j < m; j++) {
                        if (tableData[j].abbreviation.toLowerCase() == key) {
                            rate = tableData[j].rate;
                            break;
                        }
                    }
                    $ratesItem.text(rate);
                    
                }
                LoadingBanner.hideLoading();
            });
        }


    };
})();


ASC.CRM.DealTabView = (function () {

    var _onGetDealsComplete = function () {
        jq("#dealTable tbody tr").remove();

        if (ASC.CRM.DealTabView.Total == 0) {
            jq("#dealList:not(.display-none)").addClass("display-none");
            jq("#emptyContentForDealsFilter.display-none").removeClass("display-none");
            LoadingBanner.hideLoading();
            return false;
        }

        jq("#dealButtonsPanel").show();
        jq("#dealList.display-none").removeClass("display-none");

        jq.tmpl("dealTmpl", ASC.CRM.DealTabView.dealList).prependTo("#dealTable tbody");

        if (ASC.CRM.ListDealView.bidList.length == 0) {
            jq("#dealList .showTotalAmount").hide();
        } else {
            jq("#dealList .showTotalAmount").show();
        }
        LoadingBanner.hideLoading();
    };

    var _onGetMoreDealsComplete = function () {
        if (ASC.CRM.DealTabView.dealList.length == 0) {
            return false;
        }

        jq.tmpl("dealTmpl", ASC.CRM.DealTabView.dealList).appendTo("#dealTable tbody");
        ASC.CRM.Common.RegisterContactInfoCard();

        if (ASC.CRM.ListDealView.bidList.length == 0) {
            jq("#dealList .showTotalAmount").hide();
        } else {
            jq("#dealList .showTotalAmount").show();
        }
    };

    var _addRecordsToContent = function () {
        if (!ASC.CRM.DealTabView.showMore) return false;
        ASC.CRM.DealTabView.dealList = [];
        var startIndex = jq("#dealTable tbody tr").length;

        jq("#showMoreDealsButtons .crm-showMoreLink").hide();
        jq("#showMoreDealsButtons .loading-link").show();

        _getDealsForContact(startIndex);
    };

    var _getApiFilter = function (startIndex) {
        return {
            contactID: ASC.CRM.DealTabView.contactID,
            count: ASC.CRM.DealTabView.entryCountOnPage,
            startIndex: startIndex,
            contactAlsoIsParticipant: true
        };
    };

    var _getDealsForContact = function (startIndex) {
        var filters = _getApiFilter(startIndex);
        Teamlab.getCrmOpportunities({ startIndex: startIndex }, { filter: filters, success: callback_get_opportunities_for_contact });
    };


    var callback_get_opportunities_for_contact = function (params, opportunities) {
        for (var i = 0, n = opportunities.length; i < n; i++) {
            ASC.CRM.ListDealView._dealItemFactory(opportunities[i], []);
        }
        ASC.CRM.DealTabView.dealList = opportunities;
        jq(window).trigger("getDealsFromApi", [params, opportunities]);

        ASC.CRM.DealTabView.Total = params.__total || 0;
        if (typeof (params.__nextIndex) == "undefined") {
            ASC.CRM.DealTabView.showMore = false;
        }

        if (!params.__startIndex) {
            _onGetDealsComplete();
        } else {
            _onGetMoreDealsComplete();
        }

        for (var i = 0, n = ASC.CRM.DealTabView.dealList.length; i < n; i++) {
            ASC.CRM.Common.tooltip("#dealTitle_" + ASC.CRM.DealTabView.dealList[i].id, "tooltip");
        }

        jq("#showMoreDealsButtons .loading-link").hide();
        if (ASC.CRM.DealTabView.showMore) {
            jq("#showMoreDealsButtons .crm-showMoreLink").show();
        } else {
            jq("#showMoreDealsButtons .crm-showMoreLink").hide();
        }
    };

    var callback_get_dealstab_data = function (params, contactDeals) {
        initDealsSelector();

        if (contactDeals && contactDeals.length) {
            jq("#emptyContentForDealsFilter:not(.display-none)").addClass("display-none");
            jq("#dealList.display-none").removeClass("display-none");
            callback_get_opportunities_for_contact(params, contactDeals);
            jq("#dealsInContactPanel").show();
        } else {
            jq("#dealList:not(.display-none)").addClass("display-none");
            jq("#emptyContentForDealsFilter.display-none").removeClass("display-none");
        }
    };

    var callback_add_deal = function (params, deal) {
        jq("#dealList.display-none").removeClass("display-none");

        ASC.CRM.ListDealView._dealItemFactory(deal, []);

        jq.tmpl("dealTmpl", deal).prependTo("#dealTable tbody");

        jq("#emptyContentForDealsFilter:not(.display-none)").addClass("display-none");

        ASC.CRM.HistoryView.appendOption("opportunity", { value: deal.id, title: deal.title });
    };

    var _getDealTabViewData = function () {
        Teamlab.getCrmOpportunities({},
                {
                    filter: _getApiFilter(0),
                    before: LoadingBanner.displayLoading,
                    success: callback_get_dealstab_data,
                    error: function (params, error) {
                        console.log(error);
                    },
                    after: LoadingBanner.hideLoading
                });
    };
  
    var initDealsSelector = function () {

        window["dealsSelector"] = new ASC.CRM.DealSelector.DealSelector({
                        ObjName: "dealsSelector",
                        Description: ASC.CRM.Resources.CRMDealResource.FindDealByName,
                        ContactID: ASC.CRM.DealTabView.contactID,
                        InternalSearch: false,
                        ParentSelector: "#dealsInContactPanel",
                        SelectItemEvent: chooseDeal
                    });
    };

    var chooseDeal = function (item) {
        Teamlab.addCrmDealForContact({}, ASC.CRM.DealTabView.contactID, item.id, callback_add_deal);
    };

    return {
        contactID  : 0,
        dealList   : [],
        bidList    :[],

        isTabActive: false,
        showMore   : true,

        entryCountOnPage: 0,
        currentPageNumber: 1,

        isFirstLoad: true,

        initTab: function (contactID) {
            ASC.CRM.DealTabView.contactID = contactID;
            ASC.CRM.DealTabView.entryCountOnPage = ASC.CRM.Data.DefaultEntryCountOnPage;

            ASC.CRM.ListDealView.bidList = [];

            jq.tmpl("template-emptyScreen",
                    {
                        ID: "emptyContentForDealsFilter",
                        ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_deals"],
                        Header: ASC.CRM.Resources.CRMDealResource.EmptyContentDealsHeader,
                        Describe: jq.format(ASC.CRM.Resources.CRMDealResource.EmptyContentDealsDescribe,
                                            "<span class='hintStages baseLinkAction'>", "</span>"),
                        ButtonHTML: ["<a class='link-with-entity link dotline'>",
                                    ASC.CRM.Resources.CRMDealResource.LinkOrCreateDeal,
                                    "</a>"
                        ].join(''),
                        CssClass: "display-none"
                    }).insertAfter("#dealList");
            jq.tmpl("dealExtendedListTmpl", { contactID: contactID }).appendTo("#dealList");

            jq("#emptyContentForDealsFilter .emptyScrBttnPnl>a").bind("click", function () {
                jq("#emptyContentForDealsFilter:not(.display-none)").addClass("display-none");
                jq("#dealsInContactPanel").show();
            });

            jq("#showMoreDealsButtons .crm-showMoreLink").bind("click", function () {
                _addRecordsToContent();
            });
        },

        activate: function () {
            if (ASC.CRM.DealTabView.isTabActive == false) {
                ASC.CRM.DealTabView.isTabActive = true;

                jq("#emptyContentForDealsFilter .emptyScrBttnPnl>a").bind("click", function () {
                    jq("#emptyContentForDealsFilter:not(.display-none)").addClass("display-none");
                    jq("#dealsInContactPanel").show();
                });

                _getDealTabViewData();
            }
        }
    };
})();


jq(document).ready(function() {
    jq.dropdownToggle({
        switcherSelector: ".noContentBlock .hintStages",
        dropdownID: "files_hintStagesPanel",
        fixWinSize: false
    });
});


ASC.CRM.DealSelector = new function () {

    var getSourceAutocompleteCallback = function (selector, items) {
        if (!items.length) {
            selector.DomObjects.empty.outerWidth(selector.DomObjects.table.width()).show();
        } else {
            selector.DomObjects.empty.hide();
        }
        
        return items;
    };

    var initAutocomplete = function (selector) {
        selector.DomObjects.input.autocomplete({
            minLength: 0,
            delay: 300,
            focus: function (event, ui) {
                event.preventDefault ? event.preventDefault() : (event.returnValue = false);

                var autocomplete = jq(this).data("ui-autocomplete"),
                    menu = autocomplete.menu,
                    scroll = menu.element.scrollTop(),
                    offset = menu.active.offset().top - menu.element.offset().top,
                    elementHeight = menu.element.height();
                
                if (offset < 0) {
                    menu.element.scrollTop(scroll + offset);
                } else if (offset + menu.active.height() > elementHeight) {
                    menu.element.scrollTop(scroll + offset - elementHeight + menu.active.height());
                }
            },
            select: function (event, ui) {
                selector.Cache = {};
                selector.SelectItemEvent(ui.item);
                jq(this).val("");
                return false;
            },
            selectFirst: false,
            search: function () {
                return selector.DomObjects.parent.is(":visible");
            },
            source: function (request, response) {
                var term = request.term;

                if (term in selector.Cache) {
                    response(getSourceAutocompleteCallback(selector, selector.Cache[term]));
                    return;
                } else {
                    for (var cacheterm in selector.Cache) {
                        if (selector.Cache[cacheterm].length == 0 && term.indexOf(cacheterm) == 0) {
                            response(getSourceAutocompleteCallback(selector, []));
                            return;
                        }
                    }
                }

                var data = { prefix: term, contactID: selector.ContactID, internalSearch: selector.InternalSearch };

                Teamlab.getCrmOpportunitiesByPrefix({},
                {
                    filter: data,
                    success: function (parameters, items) {
                        selector.DomObjects.loader.hide();
                        selector.DomObjects.search.show();
                        selector.Cache[term] = items;
                        response(getSourceAutocompleteCallback(selector, items));
                    },
                    before: function () {
                        selector.DomObjects.search.hide();
                        selector.DomObjects.loader.show();
                    }
                });
            }
        });

        selector.DomObjects.input.data("ui-autocomplete")._renderMenu = function (ul, items) {
            var autocomplete = this;
            jq.each(items, function (index, item) {
                autocomplete._renderItemData(ul, item);
            });
        };

        selector.DomObjects.input.data("ui-autocomplete")._renderItem = function (ul, item) {
            return jq("<li></li>").data("item.autocomplete", item)
                        .append(jq("<a></a>").html(jq.htmlEncodeLight(item.title)))
                        .appendTo(ul);
        };

        selector.DomObjects.input.data("ui-autocomplete")._resizeMenu = function () {
            var autocomplete = this;
            autocomplete.menu.element.outerWidth(selector.DomObjects.table.width());
        };

        selector.DomObjects.input.data("ui-autocomplete")._suggest = function (items) {
            var autocomplete = this;
            var ul = autocomplete.menu.element.empty().zIndex(autocomplete.element.zIndex() + 1);
            autocomplete._renderMenu(ul, items);
            autocomplete.menu.refresh();
            ul.show();
            autocomplete._resizeMenu();
            ul.position(jq.extend({ of: selector.DomObjects.table }, autocomplete.options.position));
        };
    };

    var initEvents = function (selector) {

        function search () {
            selector.DomObjects.input.autocomplete("search", jq.trim(selector.DomObjects.input.val()));
        }

        selector.DomObjects.search.bind("click", search);

        selector.DomObjects.input.bind("click", search);
        
        selector.DomObjects.input.bind("keyup", function () {
            if (jq.trim(selector.DomObjects.input.val()) == "") {
                selector.DomObjects.cross.hide();
            } else {
                selector.DomObjects.cross.show();
            }
        });

        selector.DomObjects.cross.bind("click", function () {
            selector.DomObjects.cross.hide();
            selector.DomObjects.input.val("").blur();
            selector.DomObjects.empty.hide();
        });

        selector.DomObjects.link.click(function () {
            location.href = "Deals.aspx?action=manage&contactID=" + selector.ContactID;
        });

        jq(document).click(function (event) {
            if (selector.DomObjects.empty.is(":visible")) {
                console.log("document.click");
                var target = jq(event.target);
                if (!target.is(selector.DomObjects.input) &&
                    !target.is(selector.DomObjects.empty) &&
                    !target.parents(".noMatches").is(selector.DomObjects.empty)) {
                    selector.DomObjects.empty.hide();
                }
            }
        });
    };


    this.DealSelector = function (params) {

        if (!params || !params.ObjName || !params.ParentSelector)
            return null;

        var parentObj = jq(params.ParentSelector);

        if (!parentObj.length)
            return null;

        this.ObjName = params.ObjName;
        this.Description = params.Description || "";
        this.ContactID = params.ContactID || 0;
        this.InternalSearch = Boolean(params.InternalSearch);
        this.ParentSelector = params.ParentSelector;
        this.SelectItemEvent = params.SelectItemEvent;
        this.Cache = {};

        jq.tmpl("dealSelectorContainerTmpl", this).appendTo(this.ParentSelector);

        this.DomObjects = {
            parent: parentObj,
            table: parentObj.find("table"),
            search: parentObj.find(".searchButton"),
            loader: parentObj.find(".loaderImg"),
            input: parentObj.find("input[type=text].textEdit"),
            cross: parentObj.find(".crossButton"),
            empty: parentObj.find(".noMatches"),
            link: parentObj.find(".noMatches .link")
        };

        initAutocomplete(this);
        initEvents(this);

        return this;
    };
};
