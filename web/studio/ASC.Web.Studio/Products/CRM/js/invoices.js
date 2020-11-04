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

/*******************************************************************************
ListInvoiceView.ascx
*******************************************************************************/

ASC.CRM.myInvoiceContactFilter = {
    filterId: 'invoiceAdvansedFilter',
    idFilterByContact: 'contactID',

    type: 'custom-contact',
    hiddenContainerId: 'hiddenBlockForInvoiceContactSelector',
    headerContainerId: 'invoiceContactSelectorForFilter',

    onSelectContact: function (event, item) {
        jq("#" + ASC.CRM.myInvoiceContactFilter.headerContainerId).find(".inner-text .value").text(item.title);

        var $filter = jq('#' + ASC.CRM.myInvoiceContactFilter.filterId);
        $filter.advansedFilter(ASC.CRM.myInvoiceContactFilter.idFilterByContact, { id: item.id, displayName: item.title, value: jq.toJSON([item.id, "contact"]) });
        $filter.advansedFilter('resize');
    },

    createFilterByContact: function (filter) {
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

    customizeFilterByContact: function ($container, $filteritem, filter) {
        var $filterSwitcher = jq("#" + ASC.CRM.myInvoiceContactFilter.headerContainerId);

        if ($filterSwitcher.parent().is("#" + ASC.CRM.myInvoiceContactFilter.hiddenContainerId)) {
            $filterSwitcher
                .off("showList")
                .on("showList", function (event, item) {
                    ASC.CRM.myInvoiceContactFilter.onSelectContact(event, item);
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
        }
    },

    destroyFilterByContact: function ($container, $filteritem, filter) {
        var $filterSwitcher = jq("#" + ASC.CRM.myInvoiceContactFilter.headerContainerId);

        if (!$filterSwitcher.parent().is("#" + ASC.CRM.myInvoiceContactFilter.hiddenContainerId)) {
            $filterSwitcher.off("showList");
            $filterSwitcher.find(".inner-text .value").text(ASC.CRM.Resources.CRMCommonResource.Select);
            $filterSwitcher.next().andSelf().appendTo(jq('#' + ASC.CRM.myInvoiceContactFilter.hiddenContainerId));
            $filterSwitcher.contactadvancedSelector("reset");
        }
    },

    processFilter: function ($container, $filteritem, filtervalue, params) {
        if (params && params.id && isFinite(params.id)) {
            var $filterSwitcher = jq("#" + ASC.CRM.myInvoiceContactFilter.headerContainerId);
            $filterSwitcher.find(".inner-text .value").text(params.displayName);
            $filterSwitcher.contactadvancedSelector("select", [params.id]);
            $filteritem.removeClass("default-value");
        }
    }
};


ASC.CRM.ListInvoiceView = (function () {

    //Teamlab.bind(Teamlab.events.getException, _onGetException);

    function _onGetException(params, errors) {
        console.log('invoices.js ', errors);
        ASC.CRM.ListInvoiceView.isFirstLoad ? hideFirstLoader() : LoadingBanner.hideLoading();
    };

    var _setCookie = function(page, countOnPage) {
        if (ASC.CRM.ListInvoiceView.cookieKey && ASC.CRM.ListInvoiceView.cookieKey != "") {
            var cookie = {
                page        : page,
                countOnPage : countOnPage
            };
            jq.cookies.set(ASC.CRM.ListInvoiceView.cookieKey, cookie, { path: location.pathname });
        }
    };

    var _getFilterSettings = function(startIndex) {
        startIndex = startIndex || 0;
        var settings = {
            startIndex : startIndex,
            count      : ASC.CRM.ListInvoiceView.entryCountOnPage
        };

        if (!ASC.CRM.ListInvoiceView.advansedFilter) return settings;

        var param = ASC.CRM.ListInvoiceView.advansedFilter.advansedFilter();

        jq(param).each(function(i, item) {
            switch (item.id) {
                case "sorter":
                    settings.sortBy = item.params.id;
                    settings.sortOrder = item.params.dsc == true ? 'descending' : 'ascending';
                    break;
                case "text":
                    settings.filterValue = item.params.value;
                    break;
                case "issueDateFromTo":
                    settings.issueDateFrom = new Date(item.params.from);
                    settings.issueDateTo = new Date(item.params.to);
                    break;
                case "dueDateFromTo":
                    settings.dueDateFrom = new Date(item.params.from);
                    settings.dueDateTo = new Date(item.params.to);
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
    };

    var _changeFilter = function () {
        ASC.CRM.ListInvoiceView.deselectAll();

        var defaultStartIndex = 0;
        if (ASC.CRM.ListInvoiceView.defaultCurrentPageNumber != 0) {
            _setCookie(ASC.CRM.ListInvoiceView.defaultCurrentPageNumber, window.invoicePageNavigator.EntryCountOnPage);
            defaultStartIndex = (ASC.CRM.ListInvoiceView.defaultCurrentPageNumber - 1) * window.invoicePageNavigator.EntryCountOnPage;
            ASC.CRM.ListInvoiceView.defaultCurrentPageNumber = 0;
        } else {
            _setCookie(0, window.invoicePageNavigator.EntryCountOnPage);
        }

        _renderContent(defaultStartIndex);
    };

    var _renderContent = function(startIndex) {
        ASC.CRM.ListInvoiceView.invoiceList = new Array();

        if (!ASC.CRM.ListCasesView.isFirstLoad) {
            LoadingBanner.displayLoading();
            jq("#invoiceFilterContainer, #invoiceHeaderMenu, #invoiceList, #tableForInvoiceNavigation").show();
            jq('#invoiceAdvansedFilter').advansedFilter("resize");
        }
        jq("#mainSelectAllInvoices").prop("checked", false);

        _getInvoices(startIndex);
    };

    var _initPageNavigatorControl = function (countOfRows, currentPageNumber) {
        window.invoicePageNavigator = new ASC.Controls.PageNavigator.init("invoicePageNavigator", "#divForInvoicePager", countOfRows, ASC.CRM.Data.VisiblePageCount, currentPageNumber,
                                                                        ASC.CRM.Resources.CRMJSResource.Previous, ASC.CRM.Resources.CRMJSResource.Next);

        window.invoicePageNavigator.changePageCallback = function (page) {
            _setCookie(page, window.invoicePageNavigator.EntryCountOnPage);

            var startIndex = window.invoicePageNavigator.EntryCountOnPage * (page - 1);
            _renderContent(startIndex);
        };
    };

    var _renderInvoicePageNavigator = function (startIndex) {
        var tmpTotal;
        if (startIndex >= ASC.CRM.ListInvoiceView.Total) {
            tmpTotal = startIndex + 1;
        } else {
            tmpTotal = ASC.CRM.ListInvoiceView.Total;
        }
        window.invoicePageNavigator.drawPageNavigator((startIndex / ASC.CRM.ListInvoiceView.entryCountOnPage).toFixed(0) * 1 + 1, tmpTotal);
        jq("#tableForInvoiceNavigation").show();
    };

    var _renderSimpleInvoicePageNavigator = function () {
        jq("#invoiceHeaderMenu .menu-action-simple-pagenav").html("");
        var $simplePN = jq("<div></div>"),
            lengthOfLinks = 0;
        if (jq("#divForInvoicePager .pagerPrevButtonCSSClass").length != 0) {
            lengthOfLinks++;
            jq("#divForInvoicePager .pagerPrevButtonCSSClass").clone().appendTo($simplePN);
        }
        if (jq("#divForInvoicePager .pagerNextButtonCSSClass").length != 0) {
            lengthOfLinks++;
            if (lengthOfLinks === 2) {
                jq("<span style='padding: 0 8px;'>&nbsp;</span>").clone().appendTo($simplePN);
            }
            jq("#divForInvoicePager .pagerNextButtonCSSClass").clone().appendTo($simplePN);
        }
        if ($simplePN.children().length != 0) {
            $simplePN.appendTo("#invoiceHeaderMenu .menu-action-simple-pagenav");
            jq("#invoiceHeaderMenu .menu-action-simple-pagenav").show();
        } else {
            jq("#invoiceHeaderMenu .menu-action-simple-pagenav").hide();
        }
    };

    var _renderCheckedInvoicesCount = function (count) {
        if (count != 0) {
            jq("#invoiceHeaderMenu .menu-action-checked-count > span").text(jq.format(ASC.CRM.Resources.CRMJSResource.ElementsSelectedCount, count));
            jq("#invoiceHeaderMenu .menu-action-checked-count").show();
        } else {
            jq("#invoiceHeaderMenu .menu-action-checked-count > span").text("");
            jq("#invoiceHeaderMenu .menu-action-checked-count").hide();
        }
    };

    var _renderNoInvoicesEmptyScreen = function () {
        jq("#invoiceTable tbody tr").remove();
        jq("#invoiceFilterContainer, #invoiceHeaderMenu, #invoiceList, #tableForInvoiceNavigation").hide();
        jq("#emptyContentForInvoiceFilter").hide();
        jq("#invoiceEmptyScreen").show();
    };

    var _renderNoInvoicesForQueryEmptyScreen = function () {
        jq("#invoiceTable tbody tr").remove();
        jq("#invoiceHeaderMenu, #invoiceList, #tableForInvoiceNavigation").hide();
        jq("#invoiceFilterContainer").show();
        jq("#mainSelectAllInvoices").attr("disabled", true);
        jq("#invoiceEmptyScreen").hide();
        jq("#emptyContentForInvoiceFilter").show();
    };

    var _showActionMenu = function (invoiceID) {
        var invoiceItem = null;
        for (var i = 0, n = ASC.CRM.ListInvoiceView.invoiceList.length; i < n; i++) {
            if (invoiceID == ASC.CRM.ListInvoiceView.invoiceList[i].id) {
                invoiceItem = ASC.CRM.ListInvoiceView.invoiceList[i];
                break;
            }
        }
        if (invoiceItem == null) return;

        jq("#invoiceActionMenu .showProfileLink").attr("href", jq.format("Invoices.aspx?id={0}", invoiceID));

        jq("#invoiceActionMenu .showProfileLinkNewTab").unbind("click").bind("click", function () {
            jq("#invoiceActionMenu").hide();
            jq("#invoiceTable .entity-menu.active").removeClass("active");
            window.open(jq.format("Invoices.aspx?id={0}", invoiceID), "_blank");
        });

        jq("#invoiceActionMenu .downloadLink").unbind("click").bind("click", function () { _downloadInvoice(invoiceItem); });
        jq("#invoiceActionMenu .printLink").unbind("click").bind("click", function () { _printInvoice(invoiceID); });
        jq("#invoiceActionMenu .sendLink").unbind("click").bind("click", function () { _sendInvoice(invoiceItem); });
        jq("#invoiceActionMenu .duplicateInvoiceLink").attr("href", jq.format("Invoices.aspx?id={0}&action=duplicate", invoiceID));

        renderEditBtns();
        renderStatusBtns();

        function renderEditBtns() {
            if (invoiceItem.canEdit) {
                jq("#invoiceActionMenu .editInvoiceLink").attr("href", jq.format("Invoices.aspx?id={0}&action=edit", invoiceID)).show();
            } else {
                jq("#invoiceActionMenu .editInvoiceLink").removeAttr("href").hide();
            }
            
            if (invoiceItem.canDelete) {
                jq("#invoiceActionMenu .deleteInvoiceLink").unbind("click").bind("click", function () {
                    jq("#invoiceActionMenu").hide();
                    jq("#invoiceTable .entity-menu.active").removeClass("active");
                    ASC.CRM.ListInvoiceView.showConfirmationPanelForDelete(invoiceItem.number, invoiceID, true);
                }).show();
            } else {
                jq("#invoiceActionMenu .deleteInvoiceLink").unbind("click").hide();
            }

            if (invoiceItem.canEdit || invoiceItem.canDelete) {
                jq("#invoiceActionMenu ul.dropdown-content .dropdown-item-seporator:last").show();
            } else {
                jq("#invoiceActionMenu ul.dropdown-content .dropdown-item-seporator:last").hide();
            }
        }

        function renderStatusBtns () {
            jq("#invoiceActionMenu .status-btn").remove();
            if (invoiceItem.status.id == 1) { //draft
                addStatusBtn(2, ASC.CRM.Resources.CRMInvoiceResource.MarkAsSend, "invoice-send");
            }
            if (invoiceItem.status.id == 2) {
                addStatusBtn(3, ASC.CRM.Resources.CRMInvoiceResource.MarkAsRejected, "invoice-rejected");
                addStatusBtn(4, ASC.CRM.Resources.CRMInvoiceResource.MarkAsPaid, "invoice-paid");
            }
            if (invoiceItem.status.id == 3) {
                addStatusBtn(1, ASC.CRM.Resources.CRMInvoiceResource.MarkAsDraft, "invoice-draft");
            }
            if (invoiceItem.status.id == 4) {
                addStatusBtn(2, ASC.CRM.Resources.CRMInvoiceResource.MarkAsSend, "invoice-send");
            }
        }
        
        function addStatusBtn (status, text, classname) {
            var a = jq("<a></a>").addClass("dropdown-item with-icon " + classname).text(text).bind("click", function () {
                _changeStatus(invoiceItem.id, status);
            });
            var $li = jq("<li></li>").addClass("status-btn").append(a);
            $li.insertAfter(jq("#invoiceActionMenu ul.dropdown-content .dropdown-item-seporator:first"));
        }
    };

    var _getInvoices = function (startIndex) {
        var filters = _getFilterSettings(startIndex);
        Teamlab.getCrmInvoices({ startIndex: startIndex || 0 },
        {
            filter: filters,
            success: callback_get_invoices_by_filter
        });
    };

    var _resizeFilter = function() {
        var visible = jq("#invoiceFilterContainer").is(":hidden") == false;
        if (ASC.CRM.ListInvoiceView.isFilterVisible == false && visible) {
            ASC.CRM.ListInvoiceView.isFilterVisible = true;
            if (ASC.CRM.ListInvoiceView.advansedFilter) {
                jq("#invoiceAdvansedFilter").advansedFilter("resize");
            }
        }
    };

    var _invoiceItemFactory = function (invoiceItem, selectedIDs, isSimpleView) {
        var index = jq.inArray(invoiceItem.id, selectedIDs);

        invoiceItem.isChecked = index != -1;
        invoiceItem.isSimpleView = isSimpleView || false;

        if (invoiceItem.status.id == 2) {
            var
                one_day = 1000 * 60 * 60 * 24,//Get 1 day in milliseconds
                tmpDate = new Date(),
                today = new Date(tmpDate.getFullYear(), tmpDate.getMonth(), tmpDate.getDate(), 0, 0, 0, 0),

                difference_ms = invoiceItem.dueDate.getTime() - today.getTime(),
                difference_days = Math.round(difference_ms / one_day);

            if (difference_days == 0){
                invoiceItem.dueDateString = ASC.CRM.Resources.CRMCommonResource.Today;
            } else if (difference_days > 0) {
                invoiceItem.dueDateString = jq.format(ASC.CRM.Resources.CRMCommonResource.DateDueInDays, difference_days);
            } else {
                invoiceItem.dueDateString = jq.format(ASC.CRM.Resources.CRMCommonResource.DateOverueByDays, difference_days * (-1));
            }
        } else {
            invoiceItem.dueDateString = "";
        }

        invoiceItem.displaySum = ASC.CRM.Common.numberFormat(invoiceItem.cost,
                {
                    before: invoiceItem.currency.symbol,
                    after: " " + invoiceItem.currency.abbreviation,
                    thousands_sep: " ",
                    dec_point: ASC.CRM.Data.CurrencyDecimalSeparator,
                    decimals: 2
                });
    };

    var callback_get_invoices_by_filter = function (params, invoices) {
        ASC.CRM.ListInvoiceView.Total = params.__total || 0;
        var startIndex = params.__startIndex || 0;

        if (ASC.CRM.ListInvoiceView.Total === 0 &&
                    typeof (ASC.CRM.ListInvoiceView.advansedFilter) != "undefined" &&
                    ASC.CRM.ListInvoiceView.advansedFilter.advansedFilter().length == 1) {
            ASC.CRM.ListInvoiceView.noInvoices = true;
            ASC.CRM.ListInvoiceView.noInvoicesForQuery = true;
        } else {
            ASC.CRM.ListInvoiceView.noInvoices = false;
            if (ASC.CRM.ListInvoiceView.Total === 0) {
                ASC.CRM.ListInvoiceView.noInvoicesForQuery = true;
            } else {
                ASC.CRM.ListInvoiceView.noInvoicesForQuery = false;
            }
        }

        if (ASC.CRM.ListInvoiceView.noInvoices) {
            _renderNoInvoicesEmptyScreen();
            ASC.CRM.ListInvoiceView.isFirstLoad ? hideFirstLoader() : LoadingBanner.hideLoading();
            return false;
        }

        if (ASC.CRM.ListInvoiceView.noInvoicesForQuery) {
            _renderNoInvoicesForQueryEmptyScreen();
            _resizeFilter();
            ASC.CRM.ListInvoiceView.isFirstLoad ? hideFirstLoader() : LoadingBanner.hideLoading();
            return false;
        }

        if (invoices.length == 0) {//it can happen when select page without elements after deleting
            jq("invoiceEmptyScreen").hide();
            jq("#emptyContentForInvoiceFilter").hide();
            jq("#invoiceFilterContainer, #invoiceHeaderMenu, #invoiceList, #tableForInvoiceNavigation").show();
            jq("#invoiceTable tbody tr").remove();
            jq("#mainSelectAllInvoices").attr("disabled", true);

            ASC.CRM.ListInvoiceView.Total = parseInt(jq("#totalInvoicesOnPage").text()) || 0;

            var startIndex = ASC.CRM.ListInvoiceView.entryCountOnPage * (window.invoicePageNavigator.CurrentPageNumber - 1);

            while (startIndex >= ASC.CRM.ListInvoiceView.Total && startIndex >= ASC.CRM.ListInvoiceView.entryCountOnPage) {
                startIndex -= ASC.CRM.ListInvoiceView.entryCountOnPage;
            }
            _renderContent(startIndex);
            return false;
        }

        jq("#totalInvoicesOnPage").text(ASC.CRM.ListInvoiceView.Total);

        jq("#emptyContentForInvoiceFilter").hide();
        jq("#invoiceEmptyScreen").hide();
        jq("#invoiceFilterContainer").show();
        _resizeFilter();
        jq("#mainSelectAllInvoices").removeAttr("disabled");
        var selectedIDs = new Array();
        for (var i = 0, n = ASC.CRM.ListInvoiceView.selectedItems.length; i < n; i++) {
            selectedIDs.push(ASC.CRM.ListInvoiceView.selectedItems[i].id);
        }

        for (var i = 0, n = invoices.length; i < n; i++) {
            _invoiceItemFactory(invoices[i], selectedIDs);
        }
        ASC.CRM.ListInvoiceView.invoiceList = invoices;

        jq("#invoiceTable tbody").replaceWith(jq.tmpl("invoiceListTmpl", { invoices: ASC.CRM.ListInvoiceView.invoiceList }));
        jq("#invoiceHeaderMenu, #invoiceList, #tableForInvoiceNavigation").show();

        ASC.CRM.ListInvoiceView.checkFullSelection();

        ASC.CRM.Common.RegisterContactInfoCard();
        _renderInvoicePageNavigator(startIndex);
        _renderSimpleInvoicePageNavigator();

        window.scrollTo(0, 0);
        ScrolledGroupMenu.fixContentHeaderWidth(jq('#invoiceHeaderMenu'));
        ASC.CRM.ListInvoiceView.isFirstLoad ? hideFirstLoader() : LoadingBanner.hideLoading();
    };

    var hideFirstLoader = function () {
        ASC.CRM.ListInvoiceView.isFirstLoad = false;
        jq(".containerBodyBlock").children(".loader-page").hide();
        if (!jq("#invoiceEmptyScreen").is(":visible") && !jq("#emptyContentForInvoiceFilter").is(":visible")) {
            jq("#invoiceFilterContainer, #invoiceHeaderMenu, #invoiceList, #tableForInvoiceNavigation").show();
            jq('#invoiceAdvansedFilter').advansedFilter("resize");
        }
        LoadingBanner.hideLoading();
    };

    var callback_get_invoices_by_entity = function (params, invoices) {
        ASC.CRM.ListInvoiceView.Total = invoices.length || 0;

        if (ASC.CRM.ListInvoiceView.Total === 0) {
            ASC.CRM.ListInvoiceView.noInvoices = true;
        } else {
            ASC.CRM.ListInvoiceView.noInvoices = false;
        }

        if (ASC.CRM.ListInvoiceView.noInvoices) {
            _renderNoInvoicesEmptyScreen();
            ASC.CRM.ListInvoiceView.isFirstLoad ? hideFirstLoader() : LoadingBanner.hideLoading();
            return false;
        }

        if (invoices.length == 0) {//it can happen when select page without elements after deleting
            jq("invoiceEmptyScreen").hide();
            jq("#emptyContentForInvoiceFilter").hide();
            jq("#invoiceTable tbody tr").remove();
            LoadingBanner.hideLoading();
            return false;
        }

        jq("#emptyContentForInvoiceFilter").hide();
        jq("#invoiceEmptyScreen").hide();

        for (var i = 0, n = invoices.length; i < n; i++) {
            _invoiceItemFactory(invoices[i], [], true);
        }
        ASC.CRM.ListInvoiceView.invoiceList = invoices;

        jq("#invoiceList").show();
        jq("#invoiceTable tbody").replaceWith(jq.tmpl("invoiceListTmpl", { invoices: ASC.CRM.ListInvoiceView.invoiceList }));

        ASC.CRM.Common.RegisterContactInfoCard();

        LoadingBanner.hideLoading();
    };

    var callback_delete_batch_invoices = function (params, data) {
        var newInvoiceList = new Array();
        for (var i = 0, len_i = ASC.CRM.ListInvoiceView.invoiceList.length; i < len_i; i++) {
            var isDeleted = false;
            for (var j = 0, len_j = params.invoiceIDsForDelete.length; j < len_j; j++)
                if (params.invoiceIDsForDelete[j] == ASC.CRM.ListInvoiceView.invoiceList[i].id) {
                isDeleted = true;
                break;
            }
            if (!isDeleted) {
                newInvoiceList.push(ASC.CRM.ListInvoiceView.invoiceList[i]);
            }

        }
        ASC.CRM.ListInvoiceView.invoiceList = newInvoiceList;

        ASC.CRM.ListInvoiceView.Total -= params.invoiceIDsForDelete.length;
        jq("#totalInvoicesOnPage").text(ASC.CRM.ListInvoiceView.Total);

        var selectedIDs = new Array();
        for (var i = 0, n = ASC.CRM.ListInvoiceView.selectedItems.length; i < n; i++) {
            selectedIDs.push(ASC.CRM.ListInvoiceView.selectedItems[i].id);
        }

        for (var i = 0, len = params.invoiceIDsForDelete.length; i < len; i++) {
            var $objForRemove = jq("#invoice_" + params.invoiceIDsForDelete[i]);
            if ($objForRemove.length != 0) {
                $objForRemove.remove();
            }

            var index = jq.inArray(params.invoiceIDsForDelete[i], selectedIDs);
            if (index != -1) {
                selectedIDs.splice(index, 1);
                ASC.CRM.ListInvoiceView.selectedItems.splice(index, 1);
            }
        }
        jq("#mainSelectAllInvoices").prop("checked", false);

        _checkForLockMainActions();
        _renderCheckedInvoicesCount(ASC.CRM.ListInvoiceView.selectedItems.length);

        if (ASC.CRM.ListInvoiceView.Total == 0
            && (typeof (ASC.CRM.ListInvoiceView.advansedFilter) == "undefined"
            || ASC.CRM.ListInvoiceView.advansedFilter.advansedFilter().length == 1)) {
            ASC.CRM.ListInvoiceView.noInvoices = true;
            ASC.CRM.ListInvoiceView.noInvoicesForQuery = true;
        } else {
            ASC.CRM.ListInvoiceView.noInvoices = false;
            if (ASC.CRM.ListInvoiceView.Total === 0) {
                ASC.CRM.ListInvoiceView.noInvoicesForQuery = true;
            } else {
                ASC.CRM.ListInvoiceView.noInvoicesForQuery = false;
            }
        }
        PopupKeyUpActionProvider.EnableEsc = true;
        if (ASC.CRM.ListInvoiceView.noInvoices) {
            _renderNoInvoicesEmptyScreen();
            jq.unblockUI();
            return false;
        }

        if (ASC.CRM.ListInvoiceView.noInvoicesForQuery) {
            _renderNoInvoicesForQueryEmptyScreen();
            jq.unblockUI();
            return false;
        }

        if (jq("#invoiceTable tbody tr").length == 0) {
            jq.unblockUI();

            var startIndex = ASC.CRM.ListInvoiceView.entryCountOnPage * (window.invoicePageNavigator.CurrentPageNumber - 1);
            if (startIndex >= ASC.CRM.ListInvoiceView.Total) {
                startIndex -= ASC.CRM.ListInvoiceView.entryCountOnPage;
            }
            _renderContent(startIndex);
        } else {
            jq.unblockUI();
        }
    };

    var callback_change_status_batch = function (params, data) {
        var invoices = data.invoices != null ? data.invoices : [];

        var selectedIds = [];
        jq.each(ASC.CRM.ListInvoiceView.selectedItems, function (index, value) {
            selectedIds.push(value.id);
        });

        for (var i = 0, n = invoices.length; i < n; i++) {
            var inv = invoices[i];
            _invoiceItemFactory(inv, selectedIds, ASC.CRM.ListInvoiceView.hasOwnProperty("entityId") && ASC.CRM.ListInvoiceView.entityId > 0);

            for (var j = 0, m = ASC.CRM.ListInvoiceView.invoiceList.length; j < m; j++) {
                if (inv.id == ASC.CRM.ListInvoiceView.invoiceList[j].id) {
                    ASC.CRM.ListInvoiceView.invoiceList[j] = inv;
                }
            };

            for (var j = 0, m = ASC.CRM.ListInvoiceView.selectedItems.length; j < m; j++) {
                if (inv.id == ASC.CRM.ListInvoiceView.selectedItems[i].id) {
                    ASC.CRM.ListInvoiceView.selectedItems[i] = _createShortInvoice(inv);
                }
            }

            var $line = jq("#invoice_" + invoices[i].id);
            if ($line.length == 1) {
                $line.replaceWith(jq.tmpl("invoiceTmpl", invoices[i]));
            }
        }


        var warning = false;
        if (data.invoiceItems != null && data.invoiceItems.length != 0) {
            for (var i = 0, n = data.invoiceItems.length; i < n; i++) {
                if (data.invoiceItems[i].stockQuantity < 0) {
                    warning = true;
                    break;
                }
            }
        }
        if (warning) {
            if (jq("#changeInvoiceStatusError").length == 0) {
                jq.tmpl("template-blockUIPanel", {
                    id: "changeInvoiceStatusError",
                    headerTest: ASC.CRM.Resources.CRMInvoiceResource.Warning,
                    questionText: "",
                    innerHtmlText: ['<div>', ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemNegativeQuantityErrorText, '</div>'].join(''),
                    CancelBtn: ASC.CRM.Resources.CRMCommonResource.Close,
                    progressText: ""
                }).appendTo("body");
            }

            PopupKeyUpActionProvider.EnableEsc = false;
            StudioBlockUIManager.blockUI("#changeInvoiceStatusError", 500);
        }
    };

    var _lockMainActions = function() {
        jq("#invoiceHeaderMenu .menuActionChangeStatus").removeClass("unlockAction");
        jq("#invoiceHeaderMenu .menuActionDelete").removeClass("unlockAction");
    };

    var _checkForLockMainActions = function() {
        if (ASC.CRM.ListInvoiceView.selectedItems.length === 0) {
            _lockMainActions();
            return;
        }
        var deleteBatchEnabled = false;
        for (var i = 0, n = ASC.CRM.ListInvoiceView.selectedItems.length; i < n; i++) {
            if (ASC.CRM.ListInvoiceView.selectedItems[i].canDelete) {
                deleteBatchEnabled = true;
                break;
            }
        }

        jq("#invoiceHeaderMenu .menuActionChangeStatus").addClass("unlockAction");
        if (deleteBatchEnabled) {
            jq("#invoiceHeaderMenu .menuActionDelete:not(.unlockAction)").addClass("unlockAction");
        } else {
            jq("#invoiceHeaderMenu .menuActionDelete.unlockAction").removeClass("unlockAction");
        }
    };

    var _initInvoiceActionMenu = function() {
        jq.dropdownToggle({
            dropdownID: "invoiceActionMenu",
            switcherSelector: "#invoiceTable .entity-menu",
            addTop: 0,
            addLeft: 2,
            rightPos: true,
            beforeShowFunction: function (switcherObj, dropdownItem) {
                var invoiceId = switcherObj.attr("id").split('_')[1];
                if (!invoiceId) { return; }
                _showActionMenu(parseInt(invoiceId));
            },
            showFunction: function(switcherObj, dropdownItem) {
                jq("#invoiceTable .entity-menu.active").removeClass("active");
                if (dropdownItem.is(":hidden")) {
                    switcherObj.addClass("active");
                }
            },
            hideFunction: function() {
                jq("#invoiceTable .entity-menu.active").removeClass("active");
            }
        });


        jq("body").unbind("contextmenu").bind("contextmenu", function (event) {
            var e = jq.fixEvent(event);

            if (typeof e == "undefined" || !e) {
                return true;
            }

            var target = jq(e.srcElement || e.target);

            if (!target.parents("#invoiceTable").length) {
                jq("#invoiceActionMenu").hide();
                return true;
            }

            var invoiceId = parseInt(target.closest("tr.with-entity-menu").attr("id").split('_')[1]);
            if (!invoiceId) {
                return true;
            }
            _showActionMenu(invoiceId);
            jq("#invoiceTable .entity-menu.active").removeClass("active");


            jq.showDropDownByContext(e, target, jq("#invoiceActionMenu"));

            return false;
        });

    };

    var _initScrolledGroupMenu = function() {
        ScrolledGroupMenu.init({
            menuSelector: "#invoiceHeaderMenu",
            menuAnchorSelector: "#mainSelectAllInvoices",
            menuSpacerSelector: "main .filter-content .header-menu-spacer",
            userFuncInTop: function () { jq("#invoiceHeaderMenu .menu-action-on-top").hide(); },
            userFuncNotInTop: function () { jq("#invoiceHeaderMenu .menu-action-on-top").show(); }
        });
    };

    var _initConfirmationPannels = function() {
        jq.tmpl("template-blockUIPanel", {
            id: "deleteInvoicesPanel",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: ASC.CRM.Resources.CRMCommonResource.ConfirmationDeleteText,
            innerHtmlText: ["<div id=\"deleteInvoiceList\" class=\"containerForListBatchDelete mobile-overflow\">",
                                        "<dl>",
                                            "<dt class=\"listForBatchDelete\">",
                                                ASC.CRM.Resources.CRMCommonResource.InvoiceModuleName,
                                                ":",
                                            "</dt>",
                                            "<dd class=\"listForBatchDelete\">",
                                            "</dd>",
                                        "</dl>",
                                    "</div>"].join(''),
            OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ASC.CRM.Resources.CRMInvoiceResource.DeletingInvoices
        }).insertAfter("#invoiceList");

        jq("#deleteInvoicesPanel").on("click", ".middle-button-container .button.blue", function () {
            ASC.CRM.ListInvoiceView.deleteBatchInvoices();
        });
    };

    var _showDeletePanel = function() {
        jq("#deleteInvoiceList dd.listForBatchDelete").html("");
        for (var i = 0, len = ASC.CRM.ListInvoiceView.selectedItems.length; i < len; i++) {
            var item = ASC.CRM.ListInvoiceView.selectedItems[i];
            if (item.canDelete)
            {
                var label = jq("<label></label>")
                                .attr("title", item.number)
                                .text(item.number);
                jq("#deleteInvoiceList dd.listForBatchDelete").append(
                                label.prepend(jq("<input>")
                                .attr("type", "checkbox")
                                .prop("checked", true)
                                .attr("id", "invoice_" + item.id))
                            );
            }
        }
        LoadingBanner.hideLoaderBtn("#deleteInvoicesPanel");
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#deleteInvoicesPanel", 500);
    };

    var _initChangeStatusPanel = function() {
        for (var i = 0, n = ASC.CRM.Data.invoiceStatuses.length; i < n; i++) {
            var $elem = jq("<a></a>").addClass("dropdown-item")
                            .text(ASC.CRM.Common.convertText(ASC.CRM.Data.invoiceStatuses[i].displayname, false))
                            .attr('data-value', ASC.CRM.Data.invoiceStatuses[i].apiname);
            jq("#changeStatusDialog ul.dropdown-content").append(jq("<li></li>").append($elem));
        }

        jq.dropdownToggle({
            dropdownID: "changeStatusDialog",
            switcherSelector: "#invoiceHeaderMenu .menuActionChangeStatus.unlockAction",
            addTop: 5,
            addLeft: 0
        });

        jq("#changeStatusDialog .dropdown-item").bind("click", function () {
            var status = jq(this).attr("data-value");
            _changeStatusBatch(status);
        });
    };


    var _initChangeFilterByClickOnStatus = function () {
        jq("#invoiceTable").on("click", ".invoiceStatus div", function () {
            var status_id = jq(this).attr("data-status-id"),
                filters = [],
                status = "";

            for (var i = 0, n = ASC.CRM.Data.invoiceStatuses.length; i < n; i++) {
                if (ASC.CRM.Data.invoiceStatuses[i].value == status_id) {
                    status = ASC.CRM.Data.invoiceStatuses[i];
                    break;
                }
            }
            if (status.apiname != "") {
                filters.push({
                    type: "combobox",
                    id: status.apiname,
                    isset: true,
                    params: { value: status.value, title: status.displayname, classname: '', def: true }
                });

                jq("#invoiceAdvansedFilter").advansedFilter({ filters: filters });
            }
        });
    };

    var _changeStatusBatch = function (status) {
        var ids = [];
        for (var i = 0, len = ASC.CRM.ListInvoiceView.selectedItems.length; i < len; i++) {
            ids.push(ASC.CRM.ListInvoiceView.selectedItems[i].id);
        };

        var params = { ids: ids };

        Teamlab.updateCrmInvoicesStatusBatch(params, status, ids,
            {
                success: callback_change_status_batch,
                before: function (params) {
                    jq("#changeStatusDialog").hide();
                    LoadingBanner.displayLoading();
                },
                after: function (params) {
                    LoadingBanner.hideLoading();
                }
            });
    };
    
    var _changeStatus = function (invoiceId, status) {
        var ids = [invoiceId];
        var params = { ids: ids };

        Teamlab.updateCrmInvoicesStatusBatch(params, status, ids,
            {
                success: callback_change_status_batch,
                before: function () {
                    jq("#invoiceActionMenu").hide();
                    LoadingBanner.displayLoading();
                },
                after: function () {
                    LoadingBanner.hideLoading();
                }
            });
    };

    var _printInvoice = function (invoiceId) {
        jq("#invoiceActionMenu").hide();
        jq("#invoiceTable .entity-menu.active").removeClass("active");

        var win, doc;
        win = window.open("");
        doc = win.document;

        Teamlab.getCrmInvoiceJsonData({}, invoiceId,
            {
                success: function (params, data) {
                    var styles = jq.tmpl("invoiceDetailsStylesTmpl", {})[0].outerHTML;
                    var script = "<script type='text/javascript'>window.onload = function () {this.print();};</script>";
                    var html = jq.tmpl("invoiceDetailsTmpl", data)[0].outerHTML;
                    doc.write(["<html>", "<head>", styles, "</head>", "<body>", html, script, "</body>", "</html>"].join(""));
                    doc.close();
                },
                before: function () {
                    LoadingBanner.displayLoading();
                },
                after: function () {
                    LoadingBanner.hideLoading();
                }
            });
    };

    var _downloadInvoice = function (invoice) {
        LoadingBanner.displayLoading();
        jq("#invoiceActionMenu").hide();
        jq("#invoiceTable .entity-menu.active").removeClass("active");
        ASC.CRM.ListInvoiceView.checkInvoicePdfFile(invoice, "", "", null, _downloadFile);
    };

    var _sendInvoice = function (invoice) {
        LoadingBanner.displayLoading();
        jq("#invoiceActionMenu").hide();
        jq("#invoiceTable .entity-menu.active").removeClass("active");

        ASC.CRM.ListInvoiceView.checkInvoicePdfFile(invoice, "", "", null, ASC.CRM.Common.createInvoiceMail);
    };

    function _downloadFile(invoice) {
        location.href = "Invoices.aspx?id={0}&action=pdf".format(invoice.id);
    }

    var _createShortInvoice = function (invoice) {
        var shortInvoice = {
            id: invoice.id,
            number: invoice.number,
            status: invoice.status,
            canDelete: invoice.canDelete,
            canEdit: invoice.canEdit
        };
        return shortInvoice;
    };

    var _preInitPage = function (entryCountOnPage) {
        jq("#mainSelectAllInvoices").prop("checked", false);//'cause checkboxes save their state between refreshing the page

        jq("#tableForInvoiceNavigation select:first")
            .val(entryCountOnPage)
            .change(function () {
                ASC.CRM.ListInvoiceView.changeCountOfRows(this.value);
            })
            .tlCombobox();
    };

    var _initFilter = function () {
        if (!jq("#invoiceAdvansedFilter").advansedFilter) return;

        var options = [],
            statusesCount = ASC.CRM.Data.invoiceStatuses.length,


            tmpDate = new Date(),
            today = new Date(tmpDate.getFullYear(), tmpDate.getMonth(), tmpDate.getDate(), 0, 0, 0, 0),
            yesterday = new Date(new Date(today).setDate(tmpDate.getDate() - 1)),
            tomorrow = new Date(new Date(today).setDate(tmpDate.getDate() + 1)),
            beginningOfThisMonth = new Date(new Date(today).setDate(1)),
            endOfThisMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0),

            endOfLastMonth = new Date(new Date(beginningOfThisMonth).setDate(beginningOfThisMonth.getDate() - 1)),
            beginningOfLastMonth = new Date(new Date(endOfLastMonth).setDate(1)),

            todayString = Teamlab.serializeTimestamp(today),
            yesterdayString = Teamlab.serializeTimestamp(yesterday),
            tomorrowString = Teamlab.serializeTimestamp(tomorrow);
            beginningOfThisMonthString = Teamlab.serializeTimestamp(beginningOfThisMonth),
            endOfThisMonthString = Teamlab.serializeTimestamp(endOfThisMonth),
            beginningOfLastMonthString = Teamlab.serializeTimestamp(beginningOfLastMonth),
            endOfLastMonthString = Teamlab.serializeTimestamp(endOfLastMonth),

        filters = [
        {
            type        : "combobox",
            id          : "issueLastMonth",
            apiparamname: jq.toJSON(["issueDateFrom", "issueDateTo"]),
            title       : ASC.CRM.Resources.CRMCommonResource.LastMonth,
            filtertitle : ASC.CRM.Resources.CRMInvoiceResource.IssueDate,
            group       : ASC.CRM.Resources.CRMInvoiceResource.IssueDate,
            groupby     : "byIssueDate",
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
            id          : "issueYesterday",
            apiparamname: jq.toJSON(["issueDateFrom", "issueDateTo"]),
            title       : ASC.CRM.Resources.CRMCommonResource.Yesterday,
            filtertitle : ASC.CRM.Resources.CRMInvoiceResource.IssueDate,
            group       : ASC.CRM.Resources.CRMInvoiceResource.IssueDate,
            groupby     : "byIssueDate",
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
            id          : "issueToday",
            apiparamname: jq.toJSON(["issueDateFrom", "issueDateTo"]),
            title       : ASC.CRM.Resources.CRMCommonResource.Today,
            filtertitle : ASC.CRM.Resources.CRMInvoiceResource.IssueDate,
            group       : ASC.CRM.Resources.CRMInvoiceResource.IssueDate,
            groupby     : "byIssueDate",
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
            id          : "issueThisMonth",
            apiparamname: jq.toJSON(["issueDateFrom", "issueDateTo"]),
            title       : ASC.CRM.Resources.CRMCommonResource.ThisMonth,
            filtertitle : ASC.CRM.Resources.CRMInvoiceResource.IssueDate,
            group       : ASC.CRM.Resources.CRMInvoiceResource.IssueDate,
            groupby     : "byIssueDate",
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
            id          : "issueDateFromTo",
            title       : ASC.CRM.Resources.CRMCommonResource.CustomDateFilter,
            filtertitle : ASC.CRM.Resources.CRMInvoiceResource.IssueDate,
            group       : ASC.CRM.Resources.CRMInvoiceResource.IssueDate,
            groupby     : "byIssueDate"
        }];


        for (var i = 0; i < statusesCount; i++) {
            options.push({ value: ASC.CRM.Data.invoiceStatuses[i].value, classname: '', title: ASC.CRM.Data.invoiceStatuses[i].displayname });
        }

        for (var i = 0; i < statusesCount; i++) {
            var curStatus = ASC.CRM.Data.invoiceStatuses[i],
                curOpts = jQuery.extend(true, [], options);

            for (var j = 0; j < statusesCount; j++) {
                if (curOpts[j].value == curStatus.value) {
                    curOpts[j]['def'] = true;
                }
            }

            filters.push({
                type        : "combobox",
                id          : curStatus.apiname,
                apiparamname: "status",
                title       : curStatus.displayname,
                filtertitle : ASC.CRM.Resources.CRMInvoiceResource.InvoicesByStatus,
                group       : ASC.CRM.Resources.CRMInvoiceResource.InvoicesByStatus,
                groupby     : "invoiceStatus",
                options     : curOpts

            });
        }

        filters = filters.concat([
        {
            type        : "combobox",
            id          : "dueLastMonth",
            apiparamname: jq.toJSON(["dueDateFrom", "dueDateTo"]),
            title       : ASC.CRM.Resources.CRMCommonResource.LastMonth,
            filtertitle : ASC.CRM.Resources.CRMInvoiceResource.ByDueDate,
            group       : ASC.CRM.Resources.CRMInvoiceResource.ByDueDate,
            groupby     : "byDueDate",
            options     :
                        [
                        { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth, def: true },
                        { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday },
                        { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today },
                        { value: jq.toJSON([beginningOfThisMonthString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth }
                        ]
        },
        {
            type        : "combobox",
            id          : "dueYesterday",
            apiparamname: jq.toJSON(["dueDateFrom", "dueDateTo"]),
            title       : ASC.CRM.Resources.CRMCommonResource.Yesterday,
            filtertitle : ASC.CRM.Resources.CRMInvoiceResource.ByDueDate,
            group       : ASC.CRM.Resources.CRMInvoiceResource.ByDueDate,
            groupby     : "byDueDate",
            options     :
                    [
                    { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth },
                    { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday, def: true },
                    { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today },
                    { value: jq.toJSON([beginningOfThisMonthString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth }
                    ]
        },
        {
            type        : "combobox",
            id          : "dueToday",
            apiparamname: jq.toJSON(["dueDateFrom", "dueDateTo"]),
            title       : ASC.CRM.Resources.CRMCommonResource.Today,
            filtertitle : ASC.CRM.Resources.CRMInvoiceResource.ByDueDate,
            group       : ASC.CRM.Resources.CRMInvoiceResource.ByDueDate,
            groupby     : "byDueDate",
            options     :
                    [
                    { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth },
                    { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday },
                    { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today, def: true },
                    { value: jq.toJSON([beginningOfThisMonthString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth }
                    ]
        },
        {
            type        : "combobox",
            id          : "dueThisMonth",
            apiparamname: jq.toJSON(["dueDateFrom", "dueDateTo"]),
            title       : ASC.CRM.Resources.CRMCommonResource.ThisMonth,
            filtertitle : ASC.CRM.Resources.CRMInvoiceResource.ByDueDate,
            group       : ASC.CRM.Resources.CRMInvoiceResource.ByDueDate,
            groupby     : "byDueDate",
            options     :
                    [
                    { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth },
                    { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday },
                    { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today },
                    { value: jq.toJSON([beginningOfThisMonthString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth, def: true }
                    ]
        },
        {
            type        : "daterange",
            id          : "dueDateFromTo",
            title       : ASC.CRM.Resources.CRMCommonResource.CustomDateFilter,
            filtertitle : ASC.CRM.Resources.CRMInvoiceResource.ByDueDate,
            group       : ASC.CRM.Resources.CRMInvoiceResource.ByDueDate,
            groupby     : "byDueDate"
        }]);

        filters.push({
            type        : ASC.CRM.myInvoiceContactFilter.type,
            id          : ASC.CRM.myInvoiceContactFilter.idFilterByContact,
            apiparamname: jq.toJSON(["entityid", "entityType"]),
            title       : ASC.CRM.Resources.CRMContactResource.Contact,
            group       : ASC.CRM.Resources.CRMCommonResource.Other,
            hashmask    : '',
            create      : ASC.CRM.myInvoiceContactFilter.createFilterByContact,
            customize   : ASC.CRM.myInvoiceContactFilter.customizeFilterByContact,
            destroy     : ASC.CRM.myInvoiceContactFilter.destroyFilterByContact,
            process     : ASC.CRM.myInvoiceContactFilter.processFilter
        });

        options = [];
        for (var i = 0, n = ASC.CRM.Data.currencies.length; i < n; i++) {
            options.push({
                value : ASC.CRM.Data.currencies[i].abbreviation,
                title : [ASC.CRM.Data.currencies[i].abbreviation, ' - ', ASC.CRM.Data.currencies[i].title].join('')
            });
        }
        filters.push({
            type        : "combobox",
            id          : "currency",
            apiparamname: "currency",
            title       : ASC.CRM.Resources.CRMInvoiceResource.ByCurrency,
            group       : ASC.CRM.Resources.CRMCommonResource.Other,
            options     : options,
            defaulttitle: ASC.CRM.Resources.CRMCommonResource.Choose
        });


        ASC.CRM.ListInvoiceView.advansedFilter = jq("#invoiceAdvansedFilter")
            .advansedFilter({
                anykey      : false,
                hintDefaultDisable: true,
                maxfilters  : -1,
                colcount    : 2,
                maxlength   : "100",
                store       : true,
                inhash      : true,
                filters     : filters,
                sorters     : [
                            { id: "number", title: ASC.CRM.Resources.CRMInvoiceResource.Number, dsc: true, def: true },
                            { id: "issueDate", title: ASC.CRM.Resources.CRMInvoiceResource.IssueDate, dsc: false, def: false },
                            { id: "contact", title: ASC.CRM.Resources.CRMContactResource.Contact, dsc: false, def: false },
                            { id: "dueDate", title: ASC.CRM.Resources.CRMInvoiceResource.DueDate, dsc: false, def: false },
                            { id: "status", title: ASC.CRM.Resources.CRMInvoiceResource.Status, dsc: false, def: false }
                ]
            })
            .bind("setfilter", ASC.CRM.ListInvoiceView.setFilter)
            .bind("resetfilter", ASC.CRM.ListInvoiceView.resetFilter);
    };

    var _initEmptyScreen = function (emptyListImgSrc, emptyFilterListImgSrc) {
        //init emptyScreen for all list
        var buttonHtml = ["<a class='link dotline plus' href='Invoices.aspx?action=create'>",
            ASC.CRM.Resources.CRMInvoiceResource.CreateFirstInvoice,
            "</a>"].join('');

        jq.tmpl("template-emptyScreen",
            {
                ID: "invoiceEmptyScreen",
                ImgSrc: emptyListImgSrc,
                Header: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoicesHeader,
                Describe: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoicesDescribe,
                ButtonHTML: buttonHtml
            }).insertAfter("#invoiceList");

        //init emptyScreen for filter
        jq.tmpl("template-emptyScreen",
            {
                ID: "emptyContentForInvoiceFilter",
                ImgSrc: emptyFilterListImgSrc,
                Header: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoicesFilterHeader,
                Describe: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoicesFilterDescribe,
                ButtonHTML: ["<a class='clearFilterButton link dotline' href='javascript:void(0);'",
                    "onclick='ASC.CRM.ListInvoiceView.advansedFilter.advansedFilter(null);'>",
                    ASC.CRM.Resources.CRMCommonResource.ClearFilter,
                    "</a>"].join('')
            }).insertAfter("#invoiceList");
    };

    return {
        invoiceList     : new Array(),
        selectedItems   : new Array(),

        isFilterVisible: false,

        isFirstLoad: true,

        entryCountOnPage         : 0,
        defaultCurrentPageNumber : 0,
        noInvoices               : false,
        noInvoicesForQuery       : false,
        cookieKey                : "",

        clear: function(){
            ASC.CRM.ListInvoiceView.invoiceList = [];
            ASC.CRM.ListInvoiceView.selectedItems = [];

            ASC.CRM.ListInvoiceView.isFilterVisible = false;

            ASC.CRM.ListInvoiceView.entryCountOnPage = 0;
            ASC.CRM.ListInvoiceView.defaultCurrentPageNumber = 0;

            ASC.CRM.ListInvoiceView.noInvoices = false;
            ASC.CRM.ListInvoiceView.noInvoicesForQuery = false;
            ASC.CRM.ListInvoiceView.cookieKey = "";
        },

        init: function (parentSelector, filterSelector, pagingSelector) {
            if (jq(parentSelector).length == 0) return;
            ASC.CRM.Common.setDocumentTitle(ASC.CRM.Resources.CRMInvoiceResource.AllInvoices);
            ASC.CRM.ListInvoiceView.clear();
            ASC.CRM.ListInvoiceView.advansedFilter = null;
            jq(parentSelector).removeClass("display-none");

            jq.tmpl("invoicesListFilterTmpl").appendTo(filterSelector);
            jq.tmpl("invoicesListBaseTmpl").appendTo(parentSelector);
            jq.tmpl("invoicesListPagingTmpl").appendTo(pagingSelector);

            ASC.CRM.ListInvoiceView.cookieKey = ASC.CRM.Data.CookieKeyForPagination["invoices"];

            var settings = {
                page: 1,
                countOnPage: jq("#tableForInvoiceNavigation select:first>option:first").val()
            },
                key = location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') + location.pathname + location.search,
                currentAnchor = location.hash,
                cookieKey = encodeURIComponent(key);

            currentAnchor = currentAnchor && typeof currentAnchor === 'string' && currentAnchor.charAt(0) === '#'
                ? currentAnchor.substring(1)
                : currentAnchor;

            var cookieAnchor = jq.cookies.get(cookieKey);
            if (currentAnchor == "" || cookieAnchor == currentAnchor) {
                var tmp = ASC.CRM.Common.getPagingParamsFromCookie(ASC.CRM.ListInvoiceView.cookieKey);
                if (tmp != null) {
                    settings = tmp;
                }
            } else {
                _setCookie(settings.page, settings.countOnPage);
            }

            ASC.CRM.ListInvoiceView.entryCountOnPage = settings.countOnPage;
            ASC.CRM.ListInvoiceView.defaultCurrentPageNumber = settings.page;

            _preInitPage(ASC.CRM.ListInvoiceView.entryCountOnPage);
            _initEmptyScreen(ASC.CRM.Data.EmptyScrImgs["empty_screen_invoices"], ASC.CRM.Data.EmptyScrImgs["empty_screen_filter"]);

            _initPageNavigatorControl(ASC.CRM.ListInvoiceView.entryCountOnPage, ASC.CRM.ListInvoiceView.defaultCurrentPageNumber);

            _initInvoiceActionMenu();

            _initScrolledGroupMenu();

            jq("#menuCreateNewTask").bind("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });

            ASC.CRM.ListInvoiceView.initConfirmationPanelForDelete();

            _initConfirmationPannels();


            var _selectorType = 0;
            jq("#" + ASC.CRM.myInvoiceContactFilter.headerContainerId).contactadvancedSelector(
            {
                showme: true,
                addtext: ASC.CRM.Resources.CRMContactResource.AddNewCompany,
                noresults: ASC.CRM.Resources.CRMCommonResource.NoMatches,
                noitems: ASC.CRM.Resources.CRMCommonResource.NoMatches,
                inPopup: true,
                onechosen: true,
                isTempLoad: true
            });

            jq(window).bind("afterResetSelectedContact", function (event, obj, objName) {
                if (objName === "invoiceContactSelectorForFilter" && ASC.CRM.myInvoiceContactFilter.filterId) {
                    jq('#' + ASC.CRM.myInvoiceContactFilter.filterId).advansedFilter('resize');
                }
            });

            ASC.CRM.ListInvoiceView.isFirstLoad = true;
            jq(".containerBodyBlock").children(".loader-page").show();

            _initFilter();

            /*tracking events*/
            ASC.CRM.ListInvoiceView.advansedFilter.one("adv-ready", function () {
                var crmAdvansedFilterContainer = jq("#invoiceAdvansedFilter .advansed-filter-list");
                jq("#invoiceAdvansedFilter .btn-toggle-sorter").trackEvent(ga_Categories.invoice, ga_Actions.filterClick, "sort");
                jq("#invoiceAdvansedFilter .advansed-filter-input").trackEvent(ga_Categories.invoice, ga_Actions.filterClick, "search_text", "enter");
            });

            jq("#invoiceHeaderMenu").on("click", ".menuActionDelete.unlockAction", function () {
                _showDeletePanel();
            });

            _initChangeStatusPanel();
            _initChangeFilterByClickOnStatus();
        },

        setFilter: function(evt, $container, filter, params, selectedfilters) { _changeFilter(); },
        resetFilter: function(evt, $container, filter, selectedfilters) { _changeFilter(); },

        selectAll: function(obj) {
            var isChecked = jq(obj).is(":checked"),
                selectedIDs = new Array();

            for (var i = 0, n = ASC.CRM.ListInvoiceView.selectedItems.length; i < n; i++) {
                selectedIDs.push(ASC.CRM.ListInvoiceView.selectedItems[i].id);
            }

            for (var i = 0, len = ASC.CRM.ListInvoiceView.invoiceList.length; i < len; i++) {
                var invoiceItem = ASC.CRM.ListInvoiceView.invoiceList[i],
                    index = jq.inArray(invoiceItem.id, selectedIDs);
                if (isChecked && index == -1) {
                    ASC.CRM.ListInvoiceView.selectedItems.push(_createShortInvoice(invoiceItem));
                    selectedIDs.push(invoiceItem.id);
                    jq("#invoice_" + invoiceItem.id).addClass("selected");
                    jq("#checkInvoice_" + invoiceItem.id).prop("checked", true);
                }
                if (!isChecked && index != -1) {
                    ASC.CRM.ListInvoiceView.selectedItems.splice(index, 1);
                    selectedIDs.splice(index, 1);
                    jq("#invoice_" + invoiceItem.id).removeClass("selected");
                    jq("#checkInvoice_" + invoiceItem.id).prop("checked", false);
                }
            }
            _renderCheckedInvoicesCount(ASC.CRM.ListInvoiceView.selectedItems.length);
            _checkForLockMainActions();
        },

        selectItem: function(obj) {
            var id = parseInt(jq(obj).attr("id").split("_")[1]),
                selectedInvoice = null,
                selectedIDs = new Array();

            for (var i = 0, n = ASC.CRM.ListInvoiceView.invoiceList.length; i < n; i++) {
                if (id == ASC.CRM.ListInvoiceView.invoiceList[i].id) {
                    selectedInvoice = _createShortInvoice(ASC.CRM.ListInvoiceView.invoiceList[i]);
                }
            }

            for (var i = 0, n = ASC.CRM.ListInvoiceView.selectedItems.length; i < n; i++) {
                selectedIDs.push(ASC.CRM.ListInvoiceView.selectedItems[i].id);
            }

            var index = jq.inArray(id, selectedIDs);

            if (jq(obj).is(":checked")) {
                jq(obj).parents("tr:first").addClass("selected");
                if (index == -1) {
                    ASC.CRM.ListInvoiceView.selectedItems.push(selectedInvoice);
                }
                ASC.CRM.ListInvoiceView.checkFullSelection();
            } else {
                jq("#mainSelectAllInvoices").prop("checked", false);
                jq(obj).parents("tr:first").removeClass("selected");
                if (index != -1) {
                    ASC.CRM.ListInvoiceView.selectedItems.splice(index, 1);
                }
            }
            _renderCheckedInvoicesCount(ASC.CRM.ListInvoiceView.selectedItems.length);
            _checkForLockMainActions();
        },

        deselectAll: function() {
            ASC.CRM.ListInvoiceView.selectedItems = new Array();
            _renderCheckedInvoicesCount(0);
            jq("#invoiceTable input:checkbox").prop("checked", false);
            jq("#mainSelectAllInvoices").prop("checked", false);
            jq("#invoiceTable tr.selected").removeClass("selected");
            _lockMainActions();
        },

        checkFullSelection: function() {
            var rowsCount = jq("#invoiceTable tbody tr").length,
                selectedRowsCount = jq("#invoiceTable input[id^=checkInvoice_]:checked").length;
            jq("#mainSelectAllInvoices").prop("checked", rowsCount == selectedRowsCount);
        },

        deleteBatchInvoices: function () {
            var ids = new Array();
            jq("#deleteInvoicesPanel input:checked").each(function () {
                ids.push(parseInt(jq(this).attr("id").split("_")[1]));
            });
            if (ids.length == 0) return;

            var params = { invoiceIDsForDelete: ids };

            Teamlab.removeCrmInvoice(params, ids,
                {
                    success: callback_delete_batch_invoices,
                    before: function (params) {
                        LoadingBanner.strLoading = ASC.CRM.Resources.CRMInvoiceResource.DeletingInvoices;
                        LoadingBanner.showLoaderBtn("#deleteInvoicesPanel");
                    },
                    after: function(params) {
                        LoadingBanner.hideLoaderBtn("#deleteInvoicesPanel");
                    }
                });
        },

        initConfirmationPanelForDelete: function () {
            jq.tmpl("template-blockUIPanel", {
                id: "confirmationDeleteOneInvoicePanel",
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
                progressText: ASC.CRM.Resources.CRMInvoiceResource.DeleteInvoiceInProgress
            }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");
        },

        showConfirmationPanelForDelete: function(title, invoiceID, isListView) {
            jq("#confirmationDeleteOneInvoicePanel .confirmationAction>b").text(jq.format(ASC.CRM.Resources.CRMInvoiceResource.DeleteInvoiceConfirmMessage, Encoder.htmlDecode(title)));

            jq("#confirmationDeleteOneInvoicePanel .middle-button-container>.button.blue.middle").unbind("click").bind("click", function () {
                ASC.CRM.ListInvoiceView.deleteInvoice(invoiceID, isListView);
            });
            PopupKeyUpActionProvider.EnableEsc = false;
            StudioBlockUIManager.blockUI("#confirmationDeleteOneInvoicePanel", 500);
        },

        deleteInvoice: function (invoiceID, isListView) {
            if (isListView === true) {
                var ids = new Array();
                ids.push(invoiceID);
                var params = { invoiceIDsForDelete: ids };
                Teamlab.removeCrmInvoice(params, ids, callback_delete_batch_invoices);
            } else {
                Teamlab.removeCrmInvoice({}, invoiceID,
                    {
                        before: function () {
                            LoadingBanner.strLoading = ASC.CRM.Resources.CRMInvoiceResource.DeleteInvoiceInProgress;
                            LoadingBanner.showLoaderBtn("#confirmationDeleteOneInvoicePanel");

                            LoadingBanner.strLoading = ASC.CRM.Resources.CRMInvoiceResource.DeleteInvoiceInProgress;
                            LoadingBanner.showLoaderBtn("#invoiceList");
                        },
                        success: function () {
                            location.href = "Invoices.aspx";
                        }
                    });
            }
        },

        changeCountOfRows: function(newValue) {
            if (isNaN(newValue)) {
                return;
            }
            var newCountOfRows = newValue * 1;
            ASC.CRM.ListInvoiceView.entryCountOnPage = newCountOfRows;
            window.invoicePageNavigator.EntryCountOnPage = newCountOfRows;

            _setCookie(1, newCountOfRows);
            _renderContent(0);
        },
        
        initTab: function (entityId, entityType) {
            ASC.CRM.ListInvoiceView.entityId = entityId;
            ASC.CRM.ListInvoiceView.entityType = entityType;
        },

        renderSimpleContent: function () {
            if (typeof window.entityData != "undefined" && window.entityData != null) {
                ASC.CRM.ListInvoiceView.entityId = window.entityData.id;
                ASC.CRM.ListInvoiceView.entityType = window.entityData.type;
            }
            
            LoadingBanner.displayLoading();
            _initInvoiceActionMenu();
            ASC.CRM.ListInvoiceView.initConfirmationPanelForDelete();

            Teamlab.getCrmEntityInvoices({}, ASC.CRM.ListInvoiceView.entityType, ASC.CRM.ListInvoiceView.entityId, {
                success: callback_get_invoices_by_entity
            });
        },
        
        checkInvoicePdfFile: function (invoice, storageUrl, revisionId, newTab, callback) {
            var parameters = {
                invoice: invoice
            };

            var converterData = {
                invoiceId: invoice.id,
                storageUrl: storageUrl,
                revisionId: revisionId
            };

            Teamlab.getInvoiceConverterData(parameters, converterData, {
                success: function (params, data) {
                    if (data.fileId > 0) {
                        params.invoice.fileId = data.fileId;
                        LoadingBanner.hideLoading();
                        callback(params.invoice, newTab);
                    } else {
                        setTimeout(function () {
                            ASC.CRM.ListInvoiceView.checkInvoicePdfFile(params.invoice, data.storageUrl, data.revisionId, newTab, callback);
                        }, 2000);
                    }
                },
                error: function (params, error) {
                    LoadingBanner.hideLoading();
                    console.log(error);
                }
            });
        }
    };
})();



/*******************************************************************************
InvoiceActionView.ascx
*******************************************************************************/
ASC.CRM.InvoiceActionView = (function () {

    var actionTypes = {
            Create: "create",
            Edit: "edit",
            Duplicate: "duplicate"
        },
        $currentSwitcher,
        actionType = jq.getURLParam("action") || actionTypes.Create;

    var initFields = function (contactSelectorType) {

        initNumberField();

        jq("#invoiceIssueDate").mask(ASC.Resources.Master.DatePatternJQ);
        jq("#invoiceIssueDate").datepickerWithButton({ onSelect: selectIssueDate });
        jq("#invoiceIssueDate").datepicker("setDate", window.ServiceFactory.serializeDate(window.invoice.issueDate));

        initInvoiceContactSelector(contactSelectorType);

        initInvoiceConsigneeSelector(contactSelectorType);

        if (actionType == actionTypes.Create) {
            jq("#consigneeEqualClientCbx").prop("checked", false);
        } else {
            jq("#consigneeEqualClientCbx").prop("checked", jq("#invoiceContactID").val() == jq("#invoiceConsigneeID").val());
        }
        toggleConsigneeContainer();

        if (window.invoice.entity) {
            selectDeal(window.invoice.entity);
        } else {
            removeDeal();
        }

        jq("#invoiceDueDate").mask(ASC.Resources.Master.DatePatternJQ);
        jq("#invoiceDueDate").datepickerWithButton({ onSelect: selectDueDate });
        jq("#invoiceDueDate").datepicker("setDate", window.ServiceFactory.serializeDate(window.invoice.dueDate));
        jq("#invoiceDueDate").datepicker("option", "minDate", window.ServiceFactory.serializeDate(window.invoice.issueDate));
        selectDueDate();

        jq("#invoiceLanguage").val(window.invoice.language || ASC.Resources.Master.CurrentCulture);

        if (actionType == actionTypes.Create && window.invoicePresetContact && window.invoicePresetContact.currencyAbbreviation) {
            changeCurrency(jq("#invoiceCurrency").val(window.invoicePresetContact.currencyAbbreviation));
        } else {
            jq("#invoiceCurrency").val(window.invoice.currency.abbreviation);
            jq("#invoiceExchangeRate").val(window.invoice.exchangeRate);
            
            if (window.invoice.currency.abbreviation != ASC.CRM.Data.defaultCurrency.abbreviation) {
                jq("#celectedCurrency").text(window.invoice.currency.abbreviation);
                jq("#exchangeRateContainer").removeClass("display-none");
            } else {
                jq("#celectedCurrency").text("");
                jq("#exchangeRateContainer").addClass("display-none");;
            }
        }

        jq("#invoicePurchaseOrderNumber").val(window.invoice.purchaseOrderNumber);

        jq("#invoiceTerms").val(window.invoice.terms);

        jq("#invoiceDescription").val(window.invoice.description);

        jq("#invoiceLineTableContainer").append(jq.tmpl("invoiceLineTableTmpl", getInvoiceLineTableTmplData()));

        initDescriptionAutosize(jq("#invoiceLineTableContainer .description textarea"));

        if (!jq("#invoiceLineTableContainer .tbl-body-row").length) {
            addNewLine(false);
        }

        initDeleteDialog();


        if (ASC.CRM.Data.IsCRMAdmin === true) {
            initNumberFormatDialog();
            initDefaultTermsDialog();
        }
    };

    var setBindings = function () {
        jq.dropdownToggle({
            dropdownID: "addressDialog",
            switcherSelector: ".add-billing-address, .edit-billing-address, .add-delivery-address, .edit-delivery-address",
            addTop: 0,
            addLeft: 0,
            showFunction: function (switcherObj) {
                showAddressDialog(switcherObj);
            }
        });

        jq.dropdownToggle({
            dropdownID: "linkOpportunityDialog",
            switcherSelector: "#linkOpportunityButton",
            addTop: 5,
            addLeft: 0,
        });

        jq.dropdownToggle({
            dropdownID: "selectItemDialog",
            switcherSelector: "#invoiceLineTableContainer .item .custom-input",
            addTop: 0,
            addLeft: 0,
            showFunction: function (switcherObj) {
                $currentSwitcher = switcherObj;
                renderItemsList();
            }
        });

        jq.dropdownToggle({
            dropdownID: "selectTaxDialog",
            switcherSelector: "#invoiceLineTableContainer .tax1 .custom-input, #invoiceLineTableContainer .tax2 .custom-input",
            addTop: 0,
            addLeft: 0,
            showFunction: function (switcherObj) {
                $currentSwitcher = switcherObj;
                renderTaxesList();
            }
        });

        jq(window).bind("editContactInSelector", function (event, $itemObj, objName) {
            changeClient(objName);
        });

        jq("#consigneeEqualClientCbx").on("click", function () {
            toggleConsigneeContainer();
        });

        jq("#invoiceLineTableContainer").on("change", ".quantity input, .price input, .discount input", function () {
            changeLine(this);
        });

        jq("#invoiceLineTableContainer").on("input", ".discount input", function () {
            if (Number(this.value) > 100) {
                this.value = "100";
            }
        });

        jq.forceNumber({
            parent: "#invoiceLineTableContainer",
            input: ".quantity input, .discount input, .price input",
            integerOnly: false,
            positiveOnly: true
        });

        jq("#invoiceLineTableContainer").on("click", ".add-line", function () {
            addNewLine(true);
        });

        jq("#invoiceLineTableContainer").on("click", ".crm-removeLink", function () {
            removeLine(this);
        });

        jq("#crm_invoiceMakerDialog #linkOpportunityDialog").on("click", ".dropdown-item", function () {
            var entity = {
                entityType: "opportunity",
                entityId: jq(this).attr("data-value"),
                entityTitle: jq(this).text()
            };
            selectDeal(entity);
        });

        jq("#crm_invoiceMakerDialog #opportunityContainer").on("click", ".crm-removeLink", function () {
            removeDeal();
        });

        jq("#crm_invoiceMakerDialog").on("click", "#deleteButton", function () {
            showDeleteDialog();
        });

        jq("#deleteDialog .big-button-container>.button.blue.middle").on("click", function () {
            deleteInvoice();
        });

        jq("#invoiceCurrency").on("change", function () {
            changeCurrency(this);
        });

        jq("#currencyHelpSwitcher").on("click", function () {
            jq(this).helper({ BlockHelperID: 'currencyHelpInfo' });
        });

        jq.forceNumber({
            parent: "#exchangeRateContainer",
            input: "#invoiceExchangeRate",
            integerOnly: false,
            positiveOnly: true
        });

        jq("#invoiceExchangeRate").on("change", function () {
            changeExchangeRate();
        });

        jq("#exchangeRateSaveBtn").on("click", function () {
            saveExchangeRate();
        });

        jq("#invoiceLineTableContainer .tbl-body").sortable({ handle: ".crm-moveLink" });

        jq("#crm_invoiceMakerDialog .selector").on("click", ".add-new-btn", function () {
            showSelectorLeftSide(this, true);
        });

        jq("#crm_invoiceMakerDialog .selector").on("click", ".cancel-btn", function () {
            showSelectorLeftSide(this, false);
        });

        jq("#crm_invoiceMakerDialog .selector").on("click", ".set-default-btn", function () {
            selectDefaultItem(this);
        });

        jq("#crm_invoiceMakerDialog .selector").on("click", ".create-btn:not(disable)", function () {
            createNew(this);
        });

        jq("#crm_invoiceMakerDialog #selectItemDialog").on("keyup", ".custom-input input", function () {
            findItems(this);
        });

        jq("#crm_invoiceMakerDialog #selectItemDialog").on("click", ".custom-input .serch", function () {
            findItems(jq("#selectItemDialog .custom-input input"));
        });

        jq("#crm_invoiceMakerDialog .selector").on("click", ".custom-list a", function () {
            selectItem(this);
        });

        jq("#crm_invoiceMakerDialog").on("click", "#changeFormatBtn", function () {
            showNumberFormatDialog();
        });

        jq("#numberFormatDialog .big-button-container>.button.blue.middle").on("click", function () {
            saveNumberFormat();
        });

        jq("#numberFormatDialog #autogenCbx").on("click", function () {
            changeNumberFormat();
        });

        jq.forceNumber({
            parent: "#numberFormatDialog",
            input: "#numberInpt",
            integerOnly: true,
            positiveOnly: true
        });

        jq("#crm_invoiceMakerDialog .duedate-link").on("click", function () {
            changeDueDate(this);
        });

        jq("#duedatePresetHelpSwitcher").on("click", function () {
            jq(this).helper({ BlockHelperID: 'duedatePresetHelpInfo' });
        });

        jq("#crm_invoiceMakerDialog").on("click", "#setDefaultTermsBtn", function () {
            showDefaultTermsDialog();
        });

        jq("#clientNotesHelpSwitcher").on("click", function () {
            jq(this).helper({ BlockHelperID: 'clientNotesHelpInfo' });
        });

        jq("#defaultTermsDialog .big-button-container>.button.blue.middle").on("click", function () {
            saveDefaultTerms();
        });

        jq.forceNumber({
            parent: "#selectItemDialog",
            input: "#newItemPrice",
            integerOnly: false,
            positiveOnly: true,
            lengthAfterSeparator: 2
        });

        jq("#selectItemDialog ").on("keyup change", "#newItemName, #newItemPrice", function () {
            checkCreateBtnEnable(this);
        });

        jq.forceNumber({
            parent: "#selectTaxDialog",
            input: "#newTaxRate",
            integerOnly: false,
            positiveOnly: false,
            lengthAfterSeparator: 2
        });

        jq("#selectTaxDialog ").on("keyup change", "#newTaxName, #newTaxRate", function () {
            checkCreateBtnEnable(this);
        });

        jq("#addressDialog").on("click", ".button.gray.middle", function () {
            jq("#addressDialog").hide();
        });
    };

    var initOtherActionMenu = function () {
        jq("#menuCreateNewTask").bind("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    var initNumberField = function () {
        if (actionType == actionTypes.Duplicate) {
            window.invoice.id = 0;
            window.invoice.number = jq("#invoiceNumber").val();
            for (var i = 0; i < window.invoice.invoiceLines.length; i++) {
                window.invoice.invoiceLines[i].id = 0;
                window.invoice.invoiceLines[i].invoiceID = 0;
            }
        }

        if (actionType == actionTypes.Edit || (window.invoiceSettings && window.invoiceSettings.autogenerated)) {
            jq("#invoiceNumber").attr("readonly", true).addClass("disabled");
        } else {
            jq("#invoiceNumber").focus();
        }
    };

    var initInvoiceContactSelector = function (selectorType) {
        var presetSelectedContacts = [];

        if (actionType == actionTypes.Create) {
            if (window.invoicePresetContact) {
                presetSelectedContacts = [window.invoicePresetContact];
                jq("#invoiceContactID").val(window.invoicePresetContact.id);

                renderDealsList(window.invoicePresetContact.id);
            } else {
                jq("#invoiceContactID").val("0");
            }
        } else {
            if (window.invoice.contact) {
                presetSelectedContacts = [window.invoice.contact];
                jq("#invoiceContactID").val(window.invoice.contact.id);
                renderDealsList(window.invoice.contact.id);
            } else {
                jq("#invoiceContactID").val("0");
            }
        }

        window["invoiceContactSelector"] = new ASC.CRM.ContactSelector.ContactSelector("invoiceContactSelector",
            {
                SelectorType: selectorType,
                EntityType: 0,
                EntityID: 0,
                ShowOnlySelectorContent: false,
                DescriptionText: ASC.CRM.Resources.CRMCommonResource.FindContactByName,
                DeleteContactText: "",
                AddContactText: "",
                IsInPopup: false,
                ShowChangeButton: true,
                ShowAddButton: false,
                ShowDeleteButton: false,
                ShowContactImg: true,
                ShowNewCompanyContent: true,
                ShowNewContactContent: true,
                HTMLParent: "#invoiceContactSelectorContainer",
                ExcludedArrayIDs: [],
                presetSelectedContactsJson: presetSelectedContacts
            });


        window.invoiceContactSelector.SelectItemEvent = function (obj, params) {
            window.invoiceContactSelector.setContact(params.input, obj.id, obj.displayName, obj.smallFotoUrl);
            window.invoiceContactSelector.showInfoContent(params.input);
            jq("#invoiceContactID").val(obj.id);
            initInvoiceContactInfo(obj.id);
            if (obj.currency) {
                changeCurrency(jq("#invoiceCurrency").val(obj.currency.abbreviation));
            }
            renderDealsList(obj.id);
        };

        jq(window).bind("editContactInSelector", function (event, $itemObj, objName) {
            if (objName == "invoiceContactSelector") {
                jq("#invoiceOpportunityBlock").hide();
                jq("#linkOpportunityDialog").hide();
            }
        });

        showInvoiceContactInfoContainer();
    };

    var initInvoiceConsigneeSelector = function (selectorType) {
        var presetSelectedContacts = [];

        if (actionType == actionTypes.Create) {
            jq("#invoiceConsigneeID").val("0");
        } else {
            if (window.invoice.consignee) {
                presetSelectedContacts = [window.invoice.consignee];
                jq("#invoiceConsigneeID").val(window.invoice.consignee.id);
            } else {
                jq("#invoiceConsigneeID").val("0");
            }
        }

        window["invoiceConsigneeSelector"] = new ASC.CRM.ContactSelector.ContactSelector("invoiceConsigneeSelector",
            {
                SelectorType: selectorType,
                EntityType: 0,
                EntityID: 0,
                ShowOnlySelectorContent: false,
                DescriptionText: ASC.CRM.Resources.CRMCommonResource.FindContactByName,
                DeleteContactText: "",
                AddContactText: "",
                IsInPopup: false,
                ShowChangeButton: true,
                ShowAddButton: false,
                ShowDeleteButton: false,
                ShowContactImg: true,
                ShowNewCompanyContent: true,
                ShowNewContactContent: true,
                HTMLParent: "#invoiceConsigneeSelectorContainer",
                ExcludedArrayIDs: [],
                presetSelectedContactsJson: presetSelectedContacts
            });

        window.invoiceConsigneeSelector.SelectItemEvent = function (obj, params) {
            window.invoiceConsigneeSelector.setContact(params.input, obj.id, obj.displayName, obj.smallFotoUrl);
            window.invoiceConsigneeSelector.showInfoContent(params.input);
            jq("#invoiceConsigneeID").val(obj.id);
            initInvoiceConsigneeInfo(obj.id);
        };

        showInvoiceConsigneeInfoContainer();
    };

    var toggleConsigneeContainer = function () {
        var checked = jq("#consigneeEqualClientCbx").is(":checked");
        if (checked) {
            jq("#consigneeContainer").addClass("display-none");
            jq("#invoiceConsigneeID").val(invoiceContactSelector.SelectedContacts[0] || 0);
            jq("#invoiceContactInfoContainer .delivery-address").removeClass("display-none");
        } else {
            jq("#consigneeContainer").removeClass("display-none");
            jq("#invoiceConsigneeID").val(invoiceConsigneeSelector.SelectedContacts[0] || 0);
            jq("#invoiceContactInfoContainer .delivery-address").addClass("display-none");
        }
    };

    var initDescriptionAutosize = function (objects) {
        jq.each(objects, function () {
            autosize(jq(this));
            autosize.update(jq(this));
        });
    };

    var initDeleteDialog = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "deleteDialog",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: "",
            innerHtmlText: "<div class=\"deleteContainer\"></div>",
            progressText: ""
        }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");

        jq("#deleteDialog .deleteContainer").replaceWith(jq.tmpl("deleteDialogTmpl", {}));
    };

    var initNumberFormatDialog = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "numberFormatDialog",
            headerTest: ASC.CRM.Resources.CRMInvoiceResource.ChangeFormat,
            questionText: "",
            innerHtmlText: "<div class=\"numberFormatContainer\"></div>",
            progressText: ""
        }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");

        jq("#numberFormatDialog .numberFormatContainer").replaceWith(jq.tmpl("numberFormatDialogTmpl", {}));
    };

    var initDefaultTermsDialog = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "defaultTermsDialog",
            headerTest: ASC.CRM.Resources.CRMInvoiceResource.SetDefaultTerms,
            questionText: "",
            innerHtmlText: "<div class=\"defaultTermsContainer\"></div>",
            progressText: ""
        }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");

        jq("#defaultTermsDialog .defaultTermsContainer").replaceWith(jq.tmpl("defaultTermsDialogTmpl", {}));
    };

    var renderDealsList = function (contactId) {
        jq("#linkOpportunityDialog li").remove();
        Teamlab.getCrmOpportunitiesByContact({}, contactId, {
            before: function () {
                LoadingBanner.displayLoading();
            },
            success: function (params, deals) {
                if (deals.length) {
                    renderDealDataRows(deals);
                    jq("#invoiceOpportunityBlock").show();
                } else {
                    jq("#invoiceOpportunityBlock").hide();
                }
                LoadingBanner.hideLoading();
            },
            error: function (params, error) {
                LoadingBanner.hideLoading();
                console.log(error);
            }
        });
    };
    var renderDealDataRows = function (data) {
        for (var i = 0; i < data.length; i++) {
            var a = jq("<a></a>").addClass("dropdown-item").attr("data-value", data[i].id).text(data[i].title);
            var li = jq("<li></li>").append(a);
            jq("#linkOpportunityDialog ul").append(li);
        }
    };

    var renderDealEmptyRow = function () {
        var span = jq("<span></span>").addClass("gray").text(ASC.CRM.Resources.CRMInvoiceResource.EmptyOpportunitiesText);
        var li = jq("<li></li>").append(span);
        jq("#linkOpportunityDialog ul").append(li);
    };

    var renderItemsList = function () {
        var $dialog = jq("#selectItemDialog");
        $dialog.find(".custom-input input").val("");
        $dialog.find(".left-side").hide();
        $dialog.find(".custom-list li").remove();
        jq(window.invoiceItems).each(function (index, item) {
            addToItemsList($dialog, item);
        });
    };

    var addToItemsList = function (dialog, item) {
        var title = item.title.trim(),
            sku = item.stockKeepingUnit.trim(),
            text = sku ? "({0}) {1}".format(sku, title) : title,
            a = jq("<a></a>").attr("data-value", item.id).attr("title", text).text(text),
            li = jq("<li></li>").append(a);
        
        dialog.find(".custom-list").prepend(li);
    };

    var renderTaxesList = function () {
        var $dialog = jq("#selectTaxDialog");
        $dialog.find(".left-side").hide();
        $dialog.find(".custom-list li").remove();
        jq(window.invoiceTaxes).each(function (index, tax) {
            addToTaxesList($dialog, tax);
        });
    };

    var addToTaxesList = function (dialog, tax) {
        var text = tax.name.trim(),
            a = jq("<a></a>").attr("data-value", tax.id).attr("title", text).text(text),
            li = jq("<li></li>").append(a);
        
        dialog.find(".custom-list").prepend(li);
    };

    var changeClient = function (objName) {
        if (objName == "invoiceContactSelector") {
            jq("#invoiceContactID").val("0");
            if (jq("#consigneeEqualClientCbx").is(":checked")) {
                jq("#consigneeEqualClientCbx").click();
            }
            jq("#addressDialog").find("[name='billingAddressID']").val("0");
            window.invoiceContactInfo = null;
            showInvoiceContactInfoContainer();
            jq("#linkOpportunityDialog li").remove();
            removeDeal();
        }
        if (objName == "invoiceConsigneeSelector") {
            jq("#invoiceConsigneeID").val("0");
            jq("#addressDialog").find("[name='deliveryAddressID']").val("0");
            window.invoiceConsigneeInfo = null;
            showInvoiceConsigneeInfoContainer();
        }
    };

    var initInvoiceContactInfo = function (contactId) {
        Teamlab.getCrmContactInfo({}, contactId, {
            before: function () {
                LoadingBanner.displayLoading();
            },
            success: function (params, data) {
                window.invoiceContactInfo = data;

                var addresses = getInvoiceContactInfoList(7, 3),
                    address = getAddressByIdOrDefault(0, addresses);
                jq("#addressDialog").find("[name='billingAddressID']").val(address != null ? address.id : 0);

                showInvoiceContactInfoContainer();
                LoadingBanner.hideLoading();
            },
            error: function (params, error) {
                console.log(error);
                LoadingBanner.hideLoading();
            }
        });
    };

    var showInvoiceContactInfoContainer = function () {
        var $container = jq("#invoiceContactInfoContainer");
        if (window.invoiceContactInfo) {
            var billingAddress = getInvoiceContactInfo(7, 3);
            var deliveryAddress = getInvoiceContactInfo(7, 1);

            if (billingAddress) {
                $container.find(".add-billing-address").addClass("display-none");
                $container.find(".edit-billing-address").removeClass("display-none");
            } else {
                $container.find(".add-billing-address").removeClass("display-none");
                $container.find(".edit-billing-address").addClass("display-none");
            }

            if (deliveryAddress) {
                $container.find(".add-delivery-address").addClass("display-none");
                $container.find(".edit-delivery-address").removeClass("display-none");
            } else {
                $container.find(".add-delivery-address").removeClass("display-none");
                $container.find(".edit-delivery-address").addClass("display-none");
            }

            $container.removeClass("display-none");
        } else {
            $container.addClass("display-none");
        }
    };

    var initInvoiceConsigneeInfo = function (contactId) {
        Teamlab.getCrmContactInfo({}, contactId, {
            before: function () {
                LoadingBanner.displayLoading();
            },
            success: function (params, data) {
                window.invoiceConsigneeInfo = data;

                var addresses = getInvoiceContsigneeInfoList(7, 1),
                    address = getAddressByIdOrDefault(0, addresses);
                jq("#addressDialog").find("[name='deliveryAddressID']").val(address != null ? address.id : 0);

                showInvoiceConsigneeInfoContainer();
                LoadingBanner.hideLoading();
            },
            error: function (params, error) {
                console.log(error);
                LoadingBanner.hideLoading();
            }
        });
    };

    var showInvoiceConsigneeInfoContainer = function () {
        var $container = jq("#invoiceConsigneeInfoContainer");
        if (window.invoiceConsigneeInfo) {
            var deliveryAddress = getInvoiceContsigneeInfo(7, 1);

            if (deliveryAddress) {
                $container.find(".add-delivery-address").addClass("display-none");
                $container.find(".edit-delivery-address").removeClass("display-none");
            } else {
                $container.find(".add-delivery-address").removeClass("display-none");
                $container.find(".edit-delivery-address").addClass("display-none");
            }

            $container.removeClass("display-none");
        } else {
            $container.addClass("display-none");
        }
    };

    var preInitAddressDialog = function () {
        jq("#addressDialog").find("[name='deliveryAddressID']").val(0);
        jq("#addressDialog").find("[name='billingAddressID']").val(0);

        if (window.invoiceJsonData) {
            try {
                window.invoiceJsonData = jq.parseJSON(jq.base64.decode(window.invoiceJsonData));
                if (window.invoiceJsonData != null) {
                    if (!window.invoiceJsonData.hasOwnProperty("DeliveryAddressID") || isNaN(window.invoiceJsonData.DeliveryAddressID)) {
                        window.invoiceJsonData.DeliveryAddressID = 0;
                    }

                    var addresses = getInvoiceContsigneeInfoList(7, 1),
                        address = getAddressByIdOrDefault(window.invoiceJsonData.DeliveryAddressID, addresses);
                    jq("#addressDialog").find("[name='deliveryAddressID']").val(address != null ? address.id : 0);

                    if (!window.invoiceJsonData.hasOwnProperty("BillingAddressID") || isNaN(window.invoiceJsonData.BillingAddressID)) {
                        window.invoiceJsonData.BillingAddressID = 0;
                    }

                    addresses = getInvoiceContactInfoList(7, 3);
                    address = getAddressByIdOrDefault(window.invoiceJsonData.BillingAddressID, addresses);
                    jq("#addressDialog").find("[name='billingAddressID']").val(address != null ? address.id : 0);
                }
            } catch (e) {
                console.log(e);
                window.invoiceJsonData = null;
            }
        } else { window.invoiceJsonData = null; }
    };

    var showAddressDialog = function (swithcer) {
        var isConsignee = swithcer.parents("#invoiceConsigneeInfoContainer").length > 0;
        var contactId = isConsignee ? jq("#invoiceConsigneeID").val() : jq("#invoiceContactID").val();

        if (swithcer.hasClass("add-billing-address") || swithcer.hasClass("edit-billing-address")) {
            initAddressDialog(contactId, isConsignee, true, 3);
        }

        if (swithcer.hasClass("add-delivery-address") || swithcer.hasClass("edit-delivery-address")) {
            initAddressDialog(contactId, isConsignee, false, 1);
        }
    };

    var initAddressDialog = function (contactId, isConsignee, isBilling, category) {
        var addresses = isConsignee ? getInvoiceContsigneeInfoList(7, category) : getInvoiceContactInfoList(7, category),
            $dialog = jq("#addressDialog"),
            $select = $dialog.find("select.address_type_select"),
            curAddressId = isBilling ?
                            parseInt($dialog.find("[name='billingAddressID']").val()) :
                            parseInt($dialog.find("[name='deliveryAddressID']").val()),
            address = getAddressByIdOrDefault(curAddressId, addresses);

        $dialog.find(".address_category").val(category);

        curAddressId = address != null ? address.id : curAddressId;
        if (isBilling) {
            $dialog.find("[name='billingAddressID']").val(curAddressId);
        } else {
            $dialog.find("[name='deliveryAddressID']").val(curAddressId);
        }

        if (address != null && addresses.length != 0) {
            displayAddressData($dialog, address.data);

            var html = "";
            $select.html("");
            for (var i = 0, n = addresses.length; i < n; i++) {
                html += ["<option value='", addresses[i].id, "'>", isBilling ? ASC.CRM.Resources.CRMJSResource.BillingAddress : ASC.CRM.Resources.CRMJSResource.DeliveryAddress, " #", i + 1, "</option>"].join('');
            }
            $select
                .html(html)
                .val(address.id)
                .off("change").on("change", function () {
                    curAddressId = parseInt(this.value);

                    address = getAddressByIdOrDefault(curAddressId, addresses);

                    if (isBilling) {
                        $dialog.find("[name='billingAddressID']").val(curAddressId);
                    } else {
                        $dialog.find("[name='deliveryAddressID']").val(curAddressId);
                    }
                    displayAddressData($dialog, address.data);
                })
                .tlCombobox();

            $dialog.find(".address_type_select").parent().show();
        } else {
            displayAddressData($dialog, null);
            $dialog.find(".address_type_select").parent().hide();
        }

        $dialog.find(".button.blue.middle").unbind().bind("click", function () {

            var categories = ["Home", "Postal", "Office", "Billing", "Other", "Work"],
                ctg = $dialog.find(".address_category").val(),
                str = jq.trim($dialog.find(".contact_street").val()),
                cit = jq.trim($dialog.find(".contact_city").val()),
                stt = jq.trim($dialog.find(".contact_state").val()),
                zip = jq.trim($dialog.find(".contact_zip").val()),
                cnt = jq.trim($dialog.find(".contact_country").val());

            var data = {
                id: address ? address.id : 0,
                category: categories[ctg],
                infoType: 7,
                data: JSON.stringify({
                    street: str,
                    city: cit,
                    state: stt,
                    zip: zip,
                    country: cnt,
                })
            };

            if (str || cit || stt || zip || cnt) {
                saveAddress(contactId, data, isConsignee, isBilling);
            } else {
                $dialog.hide();
            }
        });
    };

    var getAddressByIdOrDefault = function (id, addresses) {
        if (addresses == null) return null;
        if (id == 0) return addresses[0];

        for (var i = 0, n = addresses.length; i < n; i++) {
            if (addresses[i].id == id) {
                return addresses[i];
            }
        }
        return addresses[0];
    };

    var displayAddressData = function ($dialog, data) {
        if (data != null) {
            var jsonData = jq.parseJSON(data);
            $dialog.find(".contact_street").val(jsonData.street);
            $dialog.find(".contact_city").val(jsonData.city);
            $dialog.find(".contact_state").val(jsonData.state);
            $dialog.find(".contact_zip").val(jsonData.zip);
            $dialog.find(".contact_country").val(jsonData.country);
        } else {
            $dialog.find(".contact_street").val("");
            $dialog.find(".contact_city").val("");
            $dialog.find(".contact_state").val("");
            $dialog.find(".contact_zip").val("");
            $dialog.find(".contact_country").val("");
        }
    };

    var saveAddress = function (contactId, addressData, isConsignee, isBilling) {
        var params = {contactId: contactId, isConsignee: isConsignee, isBilling: isBilling },
            options = {
                before: onBefore,
                success: onSuccess,
                error: onError
            };

        if (addressData.id == 0) {
            window.Teamlab.addCrmContactInfo(params, contactId, addressData, options);
        } else {
            window.Teamlab.updateCrmContactInfo(params, contactId, addressData, options);
        }

        function onBefore() {
            window.LoadingBanner.displayLoading();
        }

        function onSuccess(parameters, data) {
            if (parameters.isConsignee) {
                setInvoiceContsigneeInfo(data);
                showInvoiceConsigneeInfoContainer();
            } else {
                setInvoiceContactInfo(data);
                showInvoiceContactInfoContainer();
            }

            if (parameters.isBilling) {
                jq("#addressDialog").find("[name='billingAddressID']").val(data.id);
            } else {
                jq("#addressDialog").find("[name='deliveryAddressID']").val(data.id);
            }

            jq("#addressDialog").hide();
            jq(window).trigger("addressCrmContactSaveSuccess", [contactId, data]);
            window.LoadingBanner.hideLoading();
        }

        function onError() {
            console.log(error);
            window.LoadingBanner.hideLoading();
        }
    };

    var changeLine = function (obj) {
        var val = jq(obj).val(),
            $parent = jq(obj).parent();

        if ($parent.is(".item")) {
            var $line = getParentObj(obj, ".tbl-body-row"),
                exRate = Number(jq("#invoiceExchangeRate").val()) || 1,
                itemId = getValueFromCustomInput(obj).id,
                invoiceItem = getInvoiceItem(itemId);

            autosize.update($line.find(".description textarea").val(invoiceItem.description));
            $line.find(".price input").val(roundTo(invoiceItem.price / exRate, 2).toFixed(2));

            var tax = getInvoiceTax(invoiceItem.invoiceTax1ID);
            setValueToCustomInput($line.find(".tax1 .custom-input"), tax ? tax.name : "", tax ? tax.id : 0);

            tax = getInvoiceTax(invoiceItem.invoiceTax2ID);
            setValueToCustomInput($line.find(".tax2 .custom-input"), tax ? tax.name : "", tax ? tax.id : 0);
        }

        if ($parent.is(".quantity") || $parent.is(".discount") || $parent.is(".price")) {
            if (!val) {
                jq(obj).val("0.00");
            } else {
                jq(obj).val(Number(val).toFixed(2));
            }
        }

        recalculateInvoiceLines();
    };

    var addNewLine = function (setFocus) {
        jq("#invoiceLineTableContainer .tbl-body").append(jq.tmpl("invoiceLineTmpl", getInvoiceLineTmplData()));

        var $line = jq("#invoiceLineTableContainer .tbl-body-row:last");
        initDescriptionAutosize($line.find(".description textarea"));

        recalculateInvoiceLines();
        if (setFocus === true) {
            $line.find(".description:first textarea").focus();
        }
    };

    var removeLine = function (obj) {
        var line = getParentObj(obj, ".tbl-body-row");
        autosize.destroy(line.find(".description textarea"));
        line.remove();
        recalculateInvoiceLines();
    };

    var selectDeal = function (entity) {
        jq("#selectedOpportunity").text(entity.entityTitle);
        jq("#invoiceOpportunityID").val(entity.entityId);
        jq("#opportunitySelector").addClass("display-none");
        jq("#opportunityContainer").removeClass("display-none");
    };

    var removeDeal = function () {
        jq("#selectedOpportunity").text("");
        jq("#invoiceOpportunityID").val(0);
        jq("#opportunitySelector").removeClass("display-none");
        jq("#opportunityContainer").addClass("display-none");
    };

    var showDeleteDialog = function () {
        var text = jq.format(ASC.CRM.Resources.CRMInvoiceResource.DeleteInvoiceConfirmMessage, Encoder.htmlDecode(window.invoice.number));

        jq("#deleteDialog .header-base-small").text(text);
        jq("#deleteDialog .error-popup").text("").hide();

        StudioBlockUIManager.blockUI("#deleteDialog", 500);
    };

    var deleteInvoice = function () {
        Teamlab.removeCrmInvoice({}, window.invoice.id,
            {
                before: function () {
                    LoadingBanner.strLoading = ASC.CRM.Resources.CRMInvoiceResource.DeleteInvoiceInProgress;
                    LoadingBanner.showLoaderBtn("#deleteDialog");
                },
                success: function () {
                    ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                    location.href = "Invoices.aspx";
                },
                error: function (params, error) {
                    jq("#deleteDialog .error-popup").text(error[0]).show();
                    LoadingBanner.hideLoaderBtn("#deleteDialog");
                    console.log(error);
                }
            });
    };

    var changeCurrency = function (obj) {
        var currentCurrency = jq(obj).val();

        if (currentCurrency != ASC.CRM.Data.defaultCurrency.abbreviation) {
            jq("#celectedCurrency").text(currentCurrency);
            jq("#exchangeRateContainer").removeClass("display-none");
        } else {
            jq("#celectedCurrency").text("");
            jq("#exchangeRateContainer").addClass("display-none");;
        }

        jq("#invoiceLineTableContainer .currency").text(currentCurrency);

        var currencyRate = getCurrencyRate(currentCurrency, ASC.CRM.Data.defaultCurrency.abbreviation);
        
        if (currencyRate) {
            jq("#invoiceExchangeRate").val(currencyRate.rate.toFixed(2));
        } else {
            jq("#invoiceExchangeRate").val("1.00");
        }

        changeExchangeRate();
    };

    var changeExchangeRate = function () {
        var exRate = Number(jq("#invoiceExchangeRate").val());

        if (exRate <= 0) {
            jq("#invoiceExchangeRate").val("1.00");
            exRate = 1;
        } else {
            jq("#invoiceExchangeRate").val(exRate.toFixed(2));
        }

        jq("#exchangeRateSavedText").addClass("display-none");

        jq("#invoiceLineTableContainer .tbl-body-row .item .custom-input").each(function () {
            var itemCustomInput = jq(this),
                line = getParentObj(itemCustomInput, ".tbl-body-row"),
                itemId = getValueFromCustomInput(itemCustomInput).id,
                invoiceItem = getInvoiceItem(itemId);

            if (invoiceItem != null) {
                line.find(".price input").val(roundTo(invoiceItem.price / exRate, 2).toFixed(2));
            }
        });

        recalculateInvoiceLines();
    };
    
    var saveExchangeRate = function () {
        var data = getCurrentCurrencyRate(),
            options = {
            before: function () {
                LoadingBanner.displayLoading();
            },
            success: function (params, currencyRate) {
                setCurrencyRate(currencyRate);
                jq("#exchangeRateSavedText").removeClass("display-none");
                LoadingBanner.hideLoading();
            },
            error: function (params, error) {
                LoadingBanner.hideLoading();
                console.log(error);
            }
        };

        if (data.id) {
            Teamlab.updateCrmCurrencyRate({}, data.id, data, options);
        } else {
            Teamlab.addCrmCurrencyRate({}, data, options);
        }

        function getCurrentCurrencyRate() {
            var rate = Number(jq("#invoiceExchangeRate").val()),
                fromCurrency = jq("#invoiceCurrency").val(),
                toCurrency = ASC.CRM.Data.defaultCurrency.abbreviation,

                currencyRate = getCurrencyRate(fromCurrency, toCurrency) || {};

            currencyRate.fromCurrency = fromCurrency;
            currencyRate.toCurrency = toCurrency;
            currencyRate.rate = rate;

            return currencyRate;
        }
    };

    var showNumberFormatDialog = function () {
        jq("#numberFormatDialog .error-popup").text("").hide();
        jq("#autogenCbx").attr("checked", window.invoiceSettings.autogenerated);

        disableNumberFormatDialog(!window.invoiceSettings.autogenerated, false);

        jq("#prefixInpt").val(window.invoiceSettings.prefix);
        jq("#numberInpt").val(window.invoiceSettings.number);

        StudioBlockUIManager.blockUI("#numberFormatDialog", 500);
    };

    var disableNumberFormatDialog = function (disable, withCbx) {
        var selector = "#prefixInpt, #numberInpt";

        if (withCbx) {
            selector += ", #autogenCbx";
        }

        if (disable) {
            jq(selector).attr("disabled", true).attr("readonly", true).addClass("disabled");
        } else {
            jq(selector).removeAttr("disabled").removeAttr("readonly").removeClass("disabled");
        }
    };

    var changeNumberFormat = function () {
        var checked = jq("#autogenCbx").is(":checked");
        disableNumberFormatDialog(!checked, false);
    };

    var saveNumberFormat = function () {
        var data = {
            autogenerated: jq("#autogenCbx").is(":checked"),
            prefix: jq("#prefixInpt").val().trim(),
            number: jq("#numberInpt").val().trim()
        };
        
        if (data.autogenerated && !data.number) {
            jq("#numberFormatDialog .error-popup").text(ASC.CRM.Resources.CRMInvoiceResource.NumberFormatRequiredErrorMsg).show();
            return;
        } else {
            jq("#numberFormatDialog .error-popup").text("").hide();
        }

        Teamlab.updateCrmInvoiceSettingsName({}, data,
            {
                before: function () {
                    disableNumberFormatDialog(true, true);
                    LoadingBanner.strLoading = ASC.CRM.Resources.CRMInvoiceResource.SaveNumberSettingsProgress;
                    LoadingBanner.showLoaderBtn("#numberFormatDialog");
                },
                success: function (params, settings) {
                    window.invoiceSettings = settings;
                    if (data.autogenerated) {
                        jq("#invoiceNumber").val(settings.prefix + settings.number).attr("readonly", true).addClass("disabled");
                    } else {
                        jq("#invoiceNumber").removeAttr("readonly").removeClass("disabled");
                    }
                    disableNumberFormatDialog(false, true);
                    LoadingBanner.hideLoaderBtn("#numberFormatDialog");
                    jq.unblockUI();
                },
                error: function (params, error) {
                    jq("#numberFormatDialog .error-popup").text(ASC.CRM.Resources.CRMInvoiceResource.NumberFormatErrorMsg).show();
                    disableNumberFormatDialog(false, true);
                    LoadingBanner.hideLoaderBtn("#numberFormatDialog");
                    console.log(error);
                }
            });
    };

    var showDefaultTermsDialog = function () {
        if (ASC.CRM.Data.IsCRMAdmin === true) {
            jq("#defaultTermsDialog .error-popup").text("").hide();
            jq("#defaultTerms").val(window.invoiceSettings.terms);
            StudioBlockUIManager.blockUI("#defaultTermsDialog", 500);
        } else {
            jq("#invoiceTerms").val(window.invoiceSettings.terms);
        }
    };

    var disableDefaultTermsDialog = function (disable) {
        var selector = "#defaultTerms";

        if (disable) {
            jq(selector).attr("disabled", true).attr("readonly", true).addClass('disabled');
        } else {
            jq(selector).removeAttr("disabled").removeAttr("readonly").removeClass("disabled");
        }
    };

    var saveDefaultTerms = function () {
        var data = {
            terms: jq("#defaultTerms").val()
        };
        Teamlab.updateCrmInvoiceSettingsTerms({}, data,
            {
                before: function () {
                    disableDefaultTermsDialog(true);
                    LoadingBanner.strLoading = ASC.CRM.Resources.CRMInvoiceResource.SaveTermsSettingsProgress;
                    LoadingBanner.showLoaderBtn("#defaultTermsDialog");
                },
                success: function (params, settings) {
                    window.invoiceSettings = settings;
                    jq("#invoiceTerms").val(settings.terms);
                    disableDefaultTermsDialog(false);
                    LoadingBanner.hideLoaderBtn("#defaultTermsDialog");
                    jq.unblockUI();
                },
                error: function (params, error) {
                    jq("#numberFormatDialog .error-popup").text(error[0]).show();
                    disableDefaultTermsDialog(false);
                    LoadingBanner.hideLoaderBtn("#defaultTermsDialog");
                    console.log(error);
                }
            });
    };

    var selectIssueDate = function () {
        var issueDate = jq(this).datepicker("getDate") || new Date();
        jq("#invoiceDueDate").datepicker("option", "minDate", issueDate);
        
        var $link = jq(".duedate-link.selected");

        if ($link.length > 0) {
            changeDueDate($link);
        }

        jq("#invoiceIssueDate").datepicker("hide");
    };

    var changeDueDate = function (obj) {
        var daysCount = parseInt(jq(obj).attr("id").replace("duedate_", "")),
            issueDate = jq("#invoiceIssueDate").datepicker("getDate") || new Date(),
            dueDate = new Date(issueDate.setDate(issueDate.getDate() + daysCount));
        jq("#invoiceDueDate").datepicker("setDate", dueDate);
        selectDueDateButton(daysCount);
    };

    var selectDueDate = function () {
        var dueDate = jq("#invoiceDueDate").datepicker("getDate") || new Date(),
            issueDate = jq("#invoiceIssueDate").datepicker("getDate") || new Date(),
            daysCount = Math.floor((dueDate.getTime() - issueDate.getTime()) / (24 * 60 * 60 * 1000));

        selectDueDateButton(daysCount);

        jq("#invoiceDueDate").datepicker("hide");
    };

    var selectDueDateButton = function (daysCount) {
        jq(".duedate-link").removeClass("selected");

        if (daysCount == 0 || daysCount == 15 || daysCount == 30 || daysCount == 45) {
            jq("#duedate_" + daysCount).addClass("selected");
        }
        jq(window).trigger("selectInvoiceDueDateComplete", [daysCount]);
    };

    var showSelectorLeftSide = function (obj, display) {
        var $dialog = jq(obj).is(".selector") ? jq(obj) : getParentObj(obj, ".selector"),
            $leftSide = $dialog.find(".left-side");

        if (display) {
            if (!$leftSide.is(":visible")) {
                $leftSide.find("input").val("");
                $leftSide.find(".create-btn").addClass("disable");
                $leftSide.show();
                $dialog.css("left", $dialog.position().left - $leftSide.outerWidth(true) + "px");
                $leftSide.find("input:first").focus();
            }
        } else {
            if ($leftSide.is(":visible")) {
                $leftSide.hide();
                $dialog.css("left", $dialog.position().left + $leftSide.outerWidth(true) + "px");
                $dialog.find(".createTaxError").hide();
            }
        }
    };

    var createNew = function (obj) {
        if (jq(obj).hasClass("disable")) return;

        var $dialog = getParentObj(obj, ".selector");
        if ($dialog.attr("id") == "selectItemDialog") {
            var title = jq("#newItemName").val().trim(),
                price = jq("#newItemPrice").val().trim();

            if (!title || !price) return;

            price = Number(price);
            if (price <= 0) return;

            var item = {
                title: title,
                description: "",
                price: price,
                sku: "",
                stockQuantity: 0,
                trackInventory: false,
                invoiceTax1id: 0,
                invoiceTax2id: 0
            };

            Teamlab.addCrmInvoiceItem({}, item,
            {
                before: function () {
                    disableDialog($dialog, true);
                },
                after: function () {
                    disableDialog($dialog, false);
                },
                success: function (params, data) {
                    addToItemsList($dialog, data);
                    showSelectorLeftSide($dialog, false);
                    setInvoiceItem(data);
                }
            });
        }
        if ($dialog.attr("id") == "selectTaxDialog") {
            var name = jq("#newTaxName").val().trim(),
                rate = jq("#newTaxRate").val().trim();

            if (!name || !rate) return;

            rate = Number(rate);
            if (Math.abs(rate) > 100) return;

            var tax = {
                name: name,
                description: "",
                rate: rate
            };

            Teamlab.addCrmInvoiceTax({}, tax, {
                before: function () {
                    $dialog.find(".createTaxError").hide();
                    disableDialog($dialog, true);
                },
                after: function () {
                    disableDialog($dialog, false);
                },
                success: function (params, data) {
                    addToTaxesList($dialog, data);
                    showSelectorLeftSide($dialog, false);
                    setInvoiceTax(data);
                },
                error: function (params, error) {
                    $dialog.find(".createTaxError").show();
                }
            });
        }
    };

    var disableDialog = function (dialog, disable) {
        if (disable) {
            dialog.find(".left-side input").prop("readonly", true).addClass('disabled');
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMJSResource.PleaseWait;
            LoadingBanner.showLoaderBtn("#" + dialog.attr("id"));
        } else {
            dialog.find(".left-side input").prop("readonly", false).removeClass('disabled');
            LoadingBanner.hideLoaderBtn("#" + dialog.attr("id"));
        }
    };

    var checkCreateBtnEnable = function (obj) {
        var $dialog = getParentObj(obj, ".selector"),
            enabled = true;
        $dialog.find(".left-side input").each(function () {
            var value = jq(this).val().trim();
            if (!value) {
                enabled = false;
            }
            if (jq(this).is("#newItemPrice")) {
                var price = Number(value);
                if (price <= 0 || price > ASC.CRM.Data.MaxInvoiceItemPrice) {
                    enabled = false;
                }
            }
            if (jq(this).is("#newTaxRate")) {
                var rate = Number(value);
                if (Math.abs(rate) > 100) {
                    enabled = false;
                }
            }
        });
        if (enabled) {
            $dialog.find(".left-side .create-btn").removeClass("disable");
        } else {
            $dialog.find(".left-side .create-btn").addClass("disable");
        }
    };

    var findItems = function (obj) {
        var value = jq(obj).val().trim(),
            $dialog = getParentObj(obj, ".selector");

        $dialog.find(".custom-list li").each(function () {
            var text = jq(this).find("a").text();
            if (value != "" && text.indexOf(value) == -1) {
                jq(this).hide();
            } else {
                jq(this).show();
            }
        });

    };

    var selectItem = function (obj) {
        var $item = jq(obj),
            $dialog = getParentObj(obj, ".selector");

        setValueToCustomInput($currentSwitcher, $item.text(), $item.attr("data-value"));

        $dialog.hide();

        changeLine($currentSwitcher);
    };

    var selectDefaultItem = function (obj) {
        var $dialog = getParentObj(obj, ".selector");

        setValueToCustomInput($currentSwitcher, "", 0);

        $dialog.hide();

        changeLine($currentSwitcher);
    };

    var setValueToCustomInput = function (obj, text, id) {
        obj.find("input[type=text]").val(Number(id) > 0 ? text : "");
        obj.find("input[type=hidden]").val(id);
    };

    var getValueFromCustomInput = function (obj) {
        return {
            text: obj.find("input[type=text]").val(),
            id: obj.find("input[type=hidden]").val()
        };
    };

    var getInvoiceLineTableTmplData = function () {
        var data = {
            invoiceLines: [],
            subtotal: 0,
            discountValue: 0,
            taxValue: 0,
            total: 0,
            currency: window.invoice.currency.abbreviation,
            taxLines: [],
            isAdmin: ASC.CRM.Data.IsCRMAdmin
        };

        data.invoiceLines = jq.map(window.invoice.invoiceLines, function (line) {
            line.invoiceItem = getInvoiceItem(line.invoiceItemID);
            line.invoiceTax1 = getInvoiceTax(line.invoiceTax1ID);
            line.invoiceTax2 = getInvoiceTax(line.invoiceTax2ID);

            line.subtotal = roundTo(line.quantity * line.price, 2);
            line.discountValue = roundTo(line.subtotal * line.discount / 100, 2);

            var rate = 0;
            if (line.invoiceTax1) {
                rate += line.invoiceTax1.rate;
                addTaxLine(line.invoiceTax1, roundTo((line.subtotal - line.discountValue) * line.invoiceTax1.rate / 100, 2));
            }
            if (line.invoiceTax2) {
                rate += line.invoiceTax2.rate;
                addTaxLine(line.invoiceTax2, roundTo((line.subtotal - line.discountValue) * line.invoiceTax2.rate / 100, 2));
            }

            line.taxValue = roundTo((line.subtotal - line.discountValue) * rate / 100, 2);
            line.total = roundTo(line.subtotal - line.discountValue + line.taxValue, 2);

            data.subtotal += line.subtotal;
            data.discountValue += line.discountValue;
            data.taxValue += line.taxValue;
            data.total += line.total;

            return line;
        });

        data.subtotal = roundTo(data.subtotal, 2);
        data.discountValue = roundTo(data.discountValue, 2);
        data.taxValue = roundTo(data.taxValue, 2);
        data.total = roundTo(data.total, 2);

        return data;

        function addTaxLine(tax, value) {
            var index = -1;
            for (var i = 0; i < data.taxLines.length; i++) {
                if (data.taxLines[i].id == tax.id) {
                    index = i;
                    break;
                }
            }

            if (index > -1) {
                data.taxLines[i].value += value;
            } else {
                data.taxLines.push({
                    id: tax.id,
                    name: tax.name,
                    rate: tax.rate,
                    value: value
                });
            }
        }
    };

    var getInvoiceLineTmplData = function () {
        var invoiceLineData = {};

        invoiceLineData.description = "";
        invoiceLineData.discount = 0;
        invoiceLineData.id = 0;
        invoiceLineData.invoiceID = window.invoice.id;
        invoiceLineData.invoiceItem = null;
        invoiceLineData.invoiceTax1 = null;
        invoiceLineData.invoiceTax2 = null;
        invoiceLineData.price = 0;
        invoiceLineData.quantity = 1;
        invoiceLineData.sortOrder = 0;
        invoiceLineData.subtotal = 0;
        invoiceLineData.discountValue = 0;
        invoiceLineData.taxValue = 0;
        invoiceLineData.total = 0;

        return invoiceLineData;
    };

    var recalculateInvoiceLines = function () {
        var data = {
            subtotal: 0,
            discountValue: 0,
            taxValue: 0,
            total: 0,
            currency: jq("#invoiceCurrency").val(),
            taxLines: []
        };

        jq("#invoiceLineTableContainer .tbl-body-row").each(function () {
            var line = jq(this),
                quantity = Number(line.find(".quantity input").val()),
                price = Number(line.find(".price input").val()),
                discount = Number(line.find(".discount input").val()),

                invoiceTax1 = getInvoiceTax(getValueFromCustomInput(line.find(".tax1 .custom-input")).id),
                invoiceTax2 = getInvoiceTax(getValueFromCustomInput(line.find(".tax2 .custom-input")).id),

                subtotal = roundTo(quantity * price, 2),
                discountValue = roundTo(subtotal * discount / 100, 2),
                amountText = (subtotal - discountValue).toFixed(2);

            line.find(".amount").text(amountText).attr("title", amountText);

            var rate = 0;
            if (invoiceTax1) {
                rate += invoiceTax1.rate;
                addTaxLine(invoiceTax1, roundTo((subtotal - discountValue) * invoiceTax1.rate / 100, 2));
            }
            if (invoiceTax2) {
                rate += invoiceTax2.rate;
                addTaxLine(invoiceTax2, roundTo((subtotal - discountValue) * invoiceTax2.rate / 100, 2));
            }

            var taxValue = roundTo((subtotal - discountValue) * rate / 100, 2),
                total = roundTo(subtotal - discountValue + taxValue, 2);

            data.subtotal += subtotal;
            data.discountValue += discountValue;
            data.taxValue += taxValue;
            data.total += total;
        });

        data.subtotal = roundTo(data.subtotal, 2);
        data.discountValue = roundTo(data.discountValue, 2);
        data.taxValue = roundTo(data.taxValue, 2);
        data.total = roundTo(data.total, 2);

        var subtotalText = (data.subtotal - data.discountValue).toFixed(2);
        jq("#invoiceLineTableContainer .subtotal").text(subtotalText).attr("title", subtotalText);

        var $taxesContainer = jq("<div></div>").addClass("tbl-taxes");
        jq.each(data.taxLines, function (index, item) {
            $taxesContainer.append(jq.tmpl("invoiceTaxLineTmpl", item));
        });
        jq("#invoiceLineTableContainer .tbl-taxes").replaceWith($taxesContainer);

        jq("#invoiceLineTableContainer .currency").text(data.currency);

        var totalText = data.total.toFixed(2);
        jq("#invoiceLineTableContainer .total").text(totalText).attr("title", totalText);

        checkLinesCount();

        function addTaxLine(tax, value) {
            var index = -1;
            for (var i = 0; i < data.taxLines.length; i++) {
                if (data.taxLines[i].id == tax.id) {
                    index = i;
                    break;
                }
            }

            if (index > -1) {
                data.taxLines[i].value += value;
            } else {
                data.taxLines.push({
                    id: tax.id,
                    name: tax.name,
                    rate: tax.rate,
                    value: value
                });
            }
        }
        
        function checkLinesCount() {
            var $lines = jq("#invoiceLineTableContainer .tbl-body-row");
            if ($lines.length == 1) {
                $lines.find(".crm-moveLink, .crm-removeLink").addClass("display-none");
            } else {
                $lines.find(".crm-moveLink, .crm-removeLink").removeClass("display-none");
            }
        }
    };

    var roundTo = function (a, b) {
        b = b || 0;
        return Math.round(a * Math.pow(10, b)) / Math.pow(10, b);
    };

    var getParentObj = function (obj, jqSelector) {
        var parents = jq(obj).parents(jqSelector);
        if (parents.length) {
            return jq(parents[0]);
        }
        return null;
    };

    var getInvoiceItem = function (id) {
        for (var i = 0; i < window.invoiceItems.length; i++) {
            if (window.invoiceItems[i].id == id)
                return window.invoiceItems[i];
        }
        return null;
    };

    var setInvoiceItem = function (data) {
        var invoiceItem =
                {
                    id: data.id,
                    title: data.title,
                    stockKeepingUnit: data.stockKeepingUnit,
                    description: data.description,
                    price: data.price,
                    stockQuantity: data.stockQuantity,
                    trackInventory: data.trackInventory,
                    invoiceTax1ID: data.invoiceTax1ID,
                    invoiceTax2ID: data.invoiceTax2ID
                };

        window.invoiceItems.push(invoiceItem);
    };

    var getInvoiceTax = function (id) {
        if (Number(id) > 0) {
            for (var i = 0; i < window.invoiceTaxes.length; i++) {
                if (window.invoiceTaxes[i].id == id)
                    return window.invoiceTaxes[i];
            }
        }
        return null;
    };

    var setInvoiceTax = function (data) {
        var invoiceTax =
            {
                id: data.id,
                name: data.name,
                description: data.description,
                rate: data.rate
            };
        window.invoiceTaxes.splice(1, 0, invoiceTax);
    };

    var getCurrencyRate = function (fromCurrency, toCurrency) {
        for (var i = 0; i < window.currencyRates.length; i++) {
            if (window.currencyRates[i].fromCurrency == fromCurrency && window.currencyRates[i].toCurrency == toCurrency)
                return window.currencyRates[i];
        }
        return null;
    };

    var setCurrencyRate = function (data) {
        var index = -1;
        for (var i = 0; i < window.currencyRates.length; i++) {
            if (window.currencyRates[i].id == data.id) {
                index = i;
                break;
            }  
        }
        if (index == -1) {
            window.currencyRates.splice(1, 0, data);
        } else {
            window.currencyRates[index] = data;
        }
        
    };

    var getInvoiceContactInfo = function (infoType, category) {
        if (!window.invoiceContactInfo)
            return null;

        return jq.grep(window.invoiceContactInfo, function (item) {
            return item.infoType == infoType && item.category == category;
        })
        .sort(function (a, b) {
            return a.id - b.id;
        })[0] || null;
    };

    var getInvoiceContactInfoList = function (infoType, category) {
        if (!window.invoiceContactInfo)
            return null;

        return jq.grep(window.invoiceContactInfo, function (item) {
            return item.infoType == infoType && item.category == category;
        })
        .sort(function (a, b) {
            return a.id - b.id;
        }) || null;
    };

    var setInvoiceContactInfo = function (data) {
        var isExist = false;
        for (var i = 0; i < window.invoiceContactInfo.length; i++) {
            if (window.invoiceContactInfo[i].id == data.id) {
                window.invoiceContactInfo[i] = data;
                isExist = true;
                break;
            }
        }
        if (!isExist) {
            window.invoiceContactInfo.splice(1, 0, data);
        }
    };

    var getInvoiceContsigneeInfo = function (infoType, category) {
        if (!window.invoiceConsigneeInfo)
            return null;

        return jq.grep(window.invoiceConsigneeInfo, function (item) {
            return item.infoType == infoType && item.category == category;
        })
        .sort(function (a, b) {
            return a.id - b.id;
        })[0] || null;
    };

    var getInvoiceContsigneeInfoList = function (infoType, category) {
        if (!window.invoiceConsigneeInfo)
            return null;

        return jq.grep(window.invoiceConsigneeInfo, function (item) {
            return item.infoType == infoType && item.category == category;
        })
        .sort(function (a, b) {
            return a.id - b.id;
        }) || null;
    };

    var setInvoiceContsigneeInfo = function (data) {
        var isExist = false;
        for (var i = 0; i < window.invoiceConsigneeInfo.length; i++) {
            if (window.invoiceConsigneeInfo[i].id == data.id) {
                window.invoiceConsigneeInfo[i] = data;
                isExist = true;
                break;
            }
        }
        if (!isExist) {
            window.invoiceConsigneeInfo.splice(1, 0, data);
        }
    };

    var getValidDate = function(datepickerObj) {
        var strValue = datepickerObj.val().trim();

        if (!strValue) return null;

        try {
            jQuery.datepicker.parseDate(ASC.Resources.Master.DatepickerDatePattern, strValue);
        } catch(e) {
            console.error("Try parse " + strValue + " to " + ASC.Resources.Master.DatepickerDatePattern + " format.", e);
            return null;
        }

        return datepickerObj.datepicker("getDate");
    };

    var checkValidation = function () {
        removeAllRequiredErrorClasses();

        var isValid = true;

        if (jq("#invoiceNumber").val().trim() == "") {
            ShowRequiredError(jq("#invoiceNumber"));
            isValid = false;
        }

        var issueDate = getValidDate(jq("#invoiceIssueDate"));
        if (!issueDate) {
            ShowRequiredError(jq("#invoiceIssueDate"));
            isValid = false;
        }

        if (jq("#invoiceContactID").val() <= 0) {
            ShowRequiredError(jq("#invoiceContactID"));
            isValid = false;
        }

        var $invoiceDueDate = jq("#invoiceDueDate");
        var dueDate = getValidDate($invoiceDueDate);
        if (!dueDate) {
            AddRequiredErrorText($invoiceDueDate, ASC.CRM.Resources.CRMInvoiceResource.DueDateRequiredErrorMsg);
            ShowRequiredError($invoiceDueDate);
            isValid = false;
        }
        
        if (issueDate && dueDate && issueDate > dueDate) {
            AddRequiredErrorText($invoiceDueDate, ASC.CRM.Resources.CRMInvoiceResource.DueDateInvalidErrorMsg);
            ShowRequiredError($invoiceDueDate);
            isValid = false;
        }

        if (jq("#invoiceLanguage").val().trim() == "") {
            ShowRequiredError(jq("#invoiceLanguage"));
            isValid = false;
        }

        if (jq("#invoiceCurrency").val().trim() == "") {
            ShowRequiredError(jq("#invoiceCurrency"));
            isValid = false;
        }

        if (jq("#invoiceExchangeRate").val() <= 0 && jq("#exchangeRateContainer").is(":visible")) {
            ShowRequiredError(jq("#invoiceExchangeRate"));
            isValid = false;
        }

        if (jq("#invoiceTerms").val().trim() == "") {
            ShowRequiredError(jq("#invoiceTerms"));
            isValid = false;
        }

        if (!jq("#invoiceLineTableContainer .tbl-body-row").length) {
            AddRequiredErrorText(jq("#invoiceLineTableHeader"), ASC.CRM.Resources.CRMInvoiceResource.ProductsAndServicesRequiredErrorMsg);
            ShowRequiredError(jq("#invoiceLineTableHeader"));
            isValid = false;
        } else {
            var hasEmptyItems = false;
            jq.each(jq("#invoiceLineTableContainer .tbl-body-row .item input[type=hidden]"), function () {
                if (jq(this).val() <= 0) {
                    hasEmptyItems = true;
                }
            });
            if (hasEmptyItems) {
                AddRequiredErrorText(jq("#invoiceLineTableHeader"), ASC.CRM.Resources.CRMInvoiceResource.ProductsAndServicesEmptyItemErrorMsgs);
                ShowRequiredError(jq("#invoiceLineTableHeader"));
                isValid = false;
            } 
        }

        return isValid;

        function removeAllRequiredErrorClasses() {
            RemoveRequiredErrorClass(jq("#invoiceNumber"));
            RemoveRequiredErrorClass(jq("#invoiceIssueDate"));
            RemoveRequiredErrorClass(jq("#invoiceContactID"));
            RemoveRequiredErrorClass(jq("#invoiceConsigneeID"));
            RemoveRequiredErrorClass(jq("#invoiceDueDate"));
            RemoveRequiredErrorClass(jq("#invoiceLanguage"));
            RemoveRequiredErrorClass(jq("#invoiceCurrency"));
            RemoveRequiredErrorClass(jq("#invoiceExchangeRate"));
            RemoveRequiredErrorClass(jq("#invoiceTerms"));
            RemoveRequiredErrorClass(jq("#invoiceLineTableHeader"));
        }
    };

    var updateNameAttributes = function () {
        jq("#invoiceLineTableContainer .tbl-body-row").each(function (index, line) {
            jq(line).find("[name^=iLine]").each(function (i, item) {
                var nameParts = jq(item).attr("name").split("_");
                nameParts[2] = index;
                jq(item).attr("name", nameParts.join("_"));
            });
        });
    };

    var disablePage = function () {
        jq("#crm_invoiceMakerDialog input, #crm_invoiceMakerDialog select, #crm_invoiceMakerDialog textarea")
                    .prop("readonly", "readonly")
                    .addClass('disabled');

        if (actionType == actionTypes.Edit) {
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMJSResource.EditingInvoiceProgress;
        } else {
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMJSResource.AddNewInvoiceProgress;
        }

        LoadingBanner.showLoaderBtn("#crm_invoiceMakerDialog");
    };

    var enablePage = function () {
        jq("#crm_invoiceMakerDialog input, #crm_invoiceMakerDialog select, #crm_invoiceMakerDialog textarea")
                    .prop("readonly", false)
                    .removeClass('disabled');

        LoadingBanner.hideLoaderBtn("#crm_invoiceMakerDialog");
    };

    var checkError = function (cookieKey) {
        var errorText = jq.cookies.get(cookieKey);
        if (errorText != null && errorText != "") {
            console.log(errorText);

            jq.cookies.del(cookieKey);

            jq("#saveInvoiceError .saveInvoiceErrorText").text(jq.format(ASC.CRM.Resources.CRMInvoiceResource.SavingInvoiceServerError, errorText));

            jq("[id*=_saveButton]:first").removeClass("postInProcess");
            StudioBlockUIManager.blockUI("#saveInvoiceError", 500);
        }
    };


    var _bindLeaveThePageEvent = function () {
        jq("#crm_invoiceMakerDialog").on("keyup change paste", "input, select, textarea", function (e) {
            ASC.CRM.Common.bindOnbeforeUnloadEvent();
        });
        jq("#crm_invoiceMakerDialog").on("click", ".crm-removeLink", function (e) {
            ASC.CRM.Common.bindOnbeforeUnloadEvent();
        });
        jq(window).on("setContactInSelector editContactInSelector deleteContactFromSelector addressCrmContactSaveSuccess selectInvoiceDueDateComplete", function (e) {
            ASC.CRM.Common.bindOnbeforeUnloadEvent();
        });
    };

    return {
        init: function (contactSelectorType, errorCookieKey) {
            jq.tmpl("template-blockUIPanel", {
                id: "saveInvoiceError",
                headerTest: ASC.CRM.Resources.CRMInvoiceResource.Warning,
                questionText: "",
                innerHtmlText: "<div class='saveInvoiceErrorText'></div>",
                CancelBtn: ASC.CRM.Resources.CRMCommonResource.Close,
                progressText: ""
            }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");

            checkError(errorCookieKey);

            if (typeof (window.invoiceItems) != "undefined") {
                try {
                    window.invoiceItems = jq.parseJSON(jq.base64.decode(window.invoiceItems));
                } catch (e) {
                    console.log(e);
                    window.invoiceItems = [];
                }
            }

            if (typeof (window.invoiceTaxes) != "undefined") {
                try {
                    window.invoiceTaxes = jq.parseJSON(jq.base64.decode(window.invoiceTaxes));
                } catch (e) {
                    console.log(e);
                    window.invoiceTaxes = [];
                }
            }

            if (typeof (window.invoiceSettings) != "undefined") {
                try {
                    window.invoiceSettings = jq.parseJSON(jq.base64.decode(window.invoiceSettings));
                } catch (e) {
                    console.log(e);
                    window.invoiceSettings = null;
                }
            }

            if (typeof (window.invoicePresetContact) != "undefined") {
                try {
                    window.invoicePresetContact = window.invoicePresetContact ? jq.parseJSON(jq.base64.decode(window.invoicePresetContact)) : null;
                } catch (e) {
                    console.log(e);
                    window.invoicePresetContact = null;
                }
            }

            if (typeof (window.currencyRates) != "undefined") {
                try {
                    window.currencyRates = window.currencyRates ? jq.parseJSON(jq.base64.decode(window.currencyRates)) : null;
                } catch (e) {
                    console.log(e);
                    window.currencyRates = [];
                }
            }

            if (typeof (window.invoiceContactInfo) != "undefined") {
                try {
                    window.invoiceContactInfo = window.invoiceContactInfo ? jq.parseJSON(jq.base64.decode(window.invoiceContactInfo)).response : null;
                } catch (e) {
                    console.log(e);
                    window.invoiceContactInfo = null;
                }
            }

            if (typeof (window.invoiceConsigneeInfo) != "undefined") {
                try {
                    window.invoiceConsigneeInfo = window.invoiceConsigneeInfo ? jq.parseJSON(jq.base64.decode(window.invoiceConsigneeInfo)).response : null;
                } catch (e) {
                    console.log(e);
                    window.invoiceConsigneeInfo = null;
                }
            }

            preInitAddressDialog();

            if (typeof (window.invoice) != "undefined") {
                try {
                    window.invoice = jq.parseJSON(jq.base64.decode(window.invoice)).response;
                    initFields(contactSelectorType);
                    setBindings();
                    _bindLeaveThePageEvent();
                    initOtherActionMenu();

                    jq(".cancelSbmtFormBtn:first").on("click", function () {
                        ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                        return true;
                    });

                    return true;
                } catch (e) {
                    console.log(e);
                    return false;
                }
            } else {
                ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                window.location.href = "Invoices.aspx";
                return false;
            }
        },

        submitForm: function (buttonUnicId) {
            if (jq('[id*=_saveButton]:first').hasClass('postInProcess')) {
                return false;
            }
            jq('[id*=_saveButton]:first').addClass('postInProcess');

            try {
                if (!checkValidation()) {
                    jq('[id*=_saveButton]:first').removeClass('postInProcess');
                    return false;
                } else {
                    disablePage();
                    if (window.invoice != null && window.invoice.id != 0) {
                        updateNameAttributes();
                        ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                        return true;
                    } else {
                        Teamlab.getCrmInvoiceByNumberExistence({ buttonId: buttonUnicId }, jq("#invoiceNumber").val().trim(),
                        {
                            success: function (params, isExist) {
                                if (isExist == true) {
                                    jq("#saveInvoiceError .saveInvoiceErrorText").text(ASC.CRM.Resources.CRMInvoiceResource.InvoiceNumberBusyError);
                                    jq("[id*=_saveButton]:first").removeClass("postInProcess");
                                    StudioBlockUIManager.blockUI("#saveInvoiceError", 500);
                                    enablePage();
                                    return false;
                                } else {
                                    updateNameAttributes();
                                    ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                                    __doPostBack(params.buttonId, '');
                                    return true;
                                }
                            },
                            error: function (params) {
                                jq('[id*=_saveButton]:first').removeClass('postInProcess');
                                return false;
                            }
                        });
                        return false;
                    }
                }
            } catch (e) {
                console.log(e);
                jq('[id*=_saveButton]:first').removeClass('postInProcess');
                return false;
            }
        }
    };
})();

/*******************************************************************************
InvoiceDetailsView.ascx
*******************************************************************************/
ASC.CRM.InvoiceDetailsView = (function () {

    var initDeleteDialog = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "deleteDialog",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: "",
            innerHtmlText: "<div class=\"deleteContainer\"></div>",
            progressText: ""
        }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");

        jq("#deleteDialog .deleteContainer").replaceWith(jq.tmpl("deleteDialogTmpl", {}));
    };

    var showDeleteDialog = function () {
        jq(".mainContainerClass .containerHeaderBlock .menu-small.active").removeClass("active");
        jq("#invoiceDetailsMenuPanel").hide();

        var text = jq.format(ASC.CRM.Resources.CRMInvoiceResource.DeleteInvoiceConfirmMessage, window.invoice.number);

        jq("#deleteDialog .header-base-small").text(text);
        jq("#deleteDialog .error-popup").text("").hide();

        StudioBlockUIManager.blockUI("#deleteDialog", 500);
    };

    var deleteInvoice = function () {
        Teamlab.removeCrmInvoice({}, window.invoice.id,
            {
                before: function () {
                    LoadingBanner.strLoading = ASC.CRM.Resources.CRMInvoiceResource.DeleteInvoiceInProgress;
                    LoadingBanner.showLoaderBtn("#deleteDialog");
                },
                success: function () {
                    location.href = "Invoices.aspx";
                },
                error: function (params, error) {
                    jq("#deleteDialog .error-popup").text(error[0]).show();
                    LoadingBanner.hideLoaderBtn("#deleteDialog");
                    console.log(error);
                }
            });
    };

    var setBindings = function () {
        jq(document).ready(function () {
            jq.dropdownToggle({
                dropdownID: "invoiceDetailsMenuPanel",
                switcherSelector: ".mainContainerClass .containerHeaderBlock .menu-small",
                addTop: 0,
                addLeft: -10,
                beforeShowFunction: function () {
                    renderEditBtns();
                    renderStatusBtns();
                },
                showFunction: function (switcherObj, dropdownItem) {
                    if (dropdownItem.is(":hidden")) {
                        switcherObj.addClass('active');
                    } else {
                        switcherObj.removeClass('active');
                    }
                },
                hideFunction: function () {
                    jq(".mainContainerClass .containerHeaderBlock .menu-small.active").removeClass("active");
                }
            });
        });

        jq("#invoiceDetailsMenuPanel .print-btn").on("click", function () {
            jq(".mainContainerClass .containerHeaderBlock .menu-small.active").removeClass("active");
            jq("#invoiceDetailsMenuPanel").hide();
            print();
        });
        
        jq("#invoiceDetailsMenuPanel .download-btn").on("click", function () {
            download();
        });
        
        jq("#invoiceDetailsMenuPanel .delete-btn").on("click", function () {
            showDeleteDialog();
        });

        jq("#deleteDialog .big-button-container>.button.blue.middle").on("click", function () {
            deleteInvoice();
        });

        jq("#invoiceDetailsMenuPanel .mail-btn").on("click", function () {
            jq(".mainContainerClass .containerHeaderBlock .menu-small.active").removeClass("active");
            jq("#invoiceDetailsMenuPanel").hide();
            sendByEmail();
        });
    };

    var initOtherActionMenu = function () {
        jq("#menuCreateNewTask").bind("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    var renderContent = function () {
        if (typeof (window.invoice) != "undefined") {
            try {
                window.invoice = Teamlab.create("crm-invoice", null, jq.parseJSON(jq.base64.decode(window.invoice)).response);
            } catch (e) {
                console.log(e);
                window.invoice = null;
            }
        }
        
        if (typeof (window.invoiceData) != "undefined") {
            try {
                window.invoiceData = jq.parseJSON(jq.base64.decode(window.invoiceData));
            } catch (e) {
                console.log(e);
                window.invoiceData = null;
            }
        }

        if (!window.invoice || !window.invoiceData) {
            window.location.href = "Invoices.aspx";
        }

        if ((window.invoiceData.LogoBase64 == null || window.invoiceData.LogoBase64 == "") && window.invoiceData.hasOwnProperty("LogoBase64Id") && window.invoiceData.LogoBase64Id > 0)
        {
            Teamlab.getOrganisationSettingsLogo({}, window.invoiceData.LogoBase64Id, function (params, response) {
                window.invoiceData.LogoBase64 = response;
                jq(".invoice-container").replaceWith(jq.tmpl("invoiceDetailsTmpl", window.invoiceData));

                renderStatusLabel();
            });

        } else {
            jq(".invoice-container").replaceWith(jq.tmpl("invoiceDetailsTmpl", window.invoiceData));

            renderStatusLabel();
        }
    };

    var renderStatusLabel = function () {
        var $statusLabel = jq(".invoice-container .invoice-status");
        
        if ($statusLabel.length) {
            $statusLabel.remove();
        }

        $statusLabel = jq("<div></div>").addClass("invoice-status").text(window.invoice.status.title);

        switch (window.invoice.status.id) {
            case 1:
                $statusLabel.addClass("draft");
                break;
            case 2:
                var dueDate = typeof (window.invoice.dueDate) == "string" ? window.ServiceFactory.serializeDate(window.invoice.dueDate) : window.invoice.dueDate;
                if (dueDate < new Date()) {
                    $statusLabel.addClass("overdue");
                    $statusLabel.text(ASC.CRM.Resources.CRMInvoiceResource.OverdueInvoicesFilter);
                } else {
                    $statusLabel.addClass("sent");
                }
                break;
            case 3:
                $statusLabel.addClass("rejected");
                break;
            case 4:
                $statusLabel.addClass("paid");
                break;
            default:
                $statusLabel.addClass("draft");
                break;
        }

        jq(".invoice-container").prepend($statusLabel);
    };

    var print = function () {
        var styles = jq.tmpl("invoiceDetailsStylesTmpl", {})[0].outerHTML;
        var script = "<script type='text/javascript'>window.onload = function () {this.print();};</script>";
        var html = jq.tmpl("invoiceDetailsTmpl", window.invoiceData)[0].outerHTML;

        var win, doc;

        win = window.open("");
        doc = win.document;
        doc.write(["<html>", "<head>", styles, "</head>", "<body>", html, script, "</body>", "</html>"].join(""));
        doc.close();
    };

    var download = function () {
        LoadingBanner.displayLoading();
        jq(".mainContainerClass .containerHeaderBlock .menu-small.active").removeClass("active");
        jq("#invoiceDetailsMenuPanel").hide();
        checkPdfFile(window.invoice.id, "", "", null, downloadFile);
    };

    var sendByEmail = function () {
        LoadingBanner.displayLoading();

        checkPdfFile(window.invoice.id, "", "", null, ASC.CRM.Common.createInvoiceMail);
    };

    function downloadFile () {
        location.href = "Invoices.aspx?id={0}&action=pdf".format(window.invoice.id);
    }

    var checkPdfFile = function (invoiceId, storageUrl, revisionId, newTab, callback) {
        var converterData = {
            invoiceId: invoiceId,
            storageUrl: storageUrl,
            revisionId: revisionId
        };

        Teamlab.getInvoiceConverterData(null, converterData, {
            success: function (params, data) {
                if (data.fileId > 0) {
                    window.invoice.fileId = data.fileId;
                    LoadingBanner.hideLoading();
                    callback(window.invoice, newTab);
                } else {
                    setTimeout(function () {
                        ASC.CRM.InvoiceDetailsView.checkPdfFile(data.invoiceId, data.storageUrl, data.revisionId, newTab, callback);
                    }, 2000);
                }
            },
            error: function (params, error) {
                LoadingBanner.hideLoading();
                console.log(error);
            }
        });
    };

    var renderEditBtns = function () {
        if (window.invoice.canEdit) {
            jq("#invoiceDetailsMenuPanel .edit-btn").show();
        } else {
            jq("#invoiceDetailsMenuPanel .edit-btn").hide();
        }

        if (window.invoice.canDelete) {
            jq("#invoiceDetailsMenuPanel .delete-btn").show();
        } else {
            jq("#invoiceDetailsMenuPanel .delete-btn").hide();
        }
    };

    var renderStatusBtns = function () {
        jq("#invoiceDetailsMenuPanel .status-btn").remove();
        if (window.invoice.status.id == 1) {
            addStatusBtn(2, ASC.CRM.Resources.CRMInvoiceResource.MarkAsSend);
        }
        if (window.invoice.status.id == 2) {
            addStatusBtn(3, ASC.CRM.Resources.CRMInvoiceResource.MarkAsRejected);
            addStatusBtn(4, ASC.CRM.Resources.CRMInvoiceResource.MarkAsPaid);
        }
        if (window.invoice.status.id == 3) {
            addStatusBtn(1, ASC.CRM.Resources.CRMInvoiceResource.MarkAsDraft);
        }
        if (window.invoice.status.id == 4) {
            addStatusBtn(2, ASC.CRM.Resources.CRMInvoiceResource.MarkAsSend);
        }

        function addStatusBtn (status, text) {
            var a = jq("<a></a>").addClass("dropdown-item").text(text).bind("click", function () {
                changeStatus(window.invoice.id, status);
            });
            var $li = jq("<li></li>").addClass("status-btn").append(a);
            jq("#invoiceDetailsMenuPanel ul.dropdown-content").prepend($li);
        }

        function changeStatus (invoiceId, status) {
            var ids = [invoiceId],
                params = {ids: ids};

            Teamlab.updateCrmInvoicesStatusBatch(params, status, ids,
                {
                    success: function (parameters, data) {
                        if (data.invoices != null && data.invoices.length != 0) {
                            window.invoice = data.invoices[0];
                            renderStatusLabel();
                        }
                        var warning = false;
                        if (data.invoiceItems != null && data.invoiceItems.length != 0) {
                            for (var i = 0, n = data.invoiceItems.length; i < n; i++) {
                                if (data.invoiceItems[i].stockQuantity < 0) {
                                    warning = true;
                                    break;
                                }
                            }
                        }
                        if (warning) {
                            if (jq("#changeInvoiceStatusError").length == 0) {
                                jq.tmpl("template-blockUIPanel", {
                                    id: "changeInvoiceStatusError",
                                    headerTest: ASC.CRM.Resources.CRMInvoiceResource.Warning,
                                    questionText: "",
                                    innerHtmlText: ['<div>', ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemNegativeQuantityErrorText, '</div>'].join(''),
                                    CancelBtn: ASC.CRM.Resources.CRMCommonResource.Close,
                                    progressText: ""
                                }).appendTo("body");
                            }

                            PopupKeyUpActionProvider.EnableEsc = false;
                            StudioBlockUIManager.blockUI("#changeInvoiceStatusError", 500);
                        }
                    },
                    before: function () {
                        jq(".mainContainerClass .containerHeaderBlock .menu-small.active").removeClass("active");
                        jq("#invoiceDetailsMenuPanel").hide();
                        LoadingBanner.displayLoading();
                    },
                    after: function () {
                        LoadingBanner.hideLoading();
                    }
                });
        }
    };

    return {
        init: function () {
            initDeleteDialog();
            setBindings();
            initOtherActionMenu();
            renderContent();
        },
        checkPdfFile: checkPdfFile
    };
})();
