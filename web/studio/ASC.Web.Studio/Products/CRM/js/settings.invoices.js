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
    ASC.CRM = function() { return {} };
}

ASC.CRM.InvoiceItemsView = (function () {

    var _setCookie = function (page, countOnPage) {
        if (ASC.CRM.Data.CookieKeyForPagination["invoiceitems"] && ASC.CRM.Data.CookieKeyForPagination["invoiceitems"] != "") {
            var cookie = {
                page: page,
                countOnPage: countOnPage
            };
            jq.cookies.set(ASC.CRM.Data.CookieKeyForPagination["invoiceitems"], cookie, { path: location.pathname });
        }
    };

    var _getFilterSettings = function (startIndex) {
        startIndex = startIndex || 0;
        var settings = {
            startIndex: startIndex,
            count: ASC.CRM.InvoiceItemsView.entryCountOnPage
        };

        if (!ASC.CRM.InvoiceItemsView.advansedFilter) return settings;

        var param = ASC.CRM.InvoiceItemsView.advansedFilter.advansedFilter();

        jq(param).each(function (i, item) {
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
        ASC.CRM.InvoiceItemsView.deselectAll();

        var defaultStartIndex = 0;
        if (ASC.CRM.InvoiceItemsView.defaultCurrentPageNumber != 0) {
            _setCookie(ASC.CRM.InvoiceItemsView.defaultCurrentPageNumber, window.invoiceItemsPageNavigator.EntryCountOnPage);
            defaultStartIndex = (ASC.CRM.InvoiceItemsView.defaultCurrentPageNumber - 1) * window.invoiceItemsPageNavigator.EntryCountOnPage;
            ASC.CRM.InvoiceItemsView.defaultCurrentPageNumber = 0;
        } else {
            _setCookie(0, window.invoiceItemsPageNavigator.EntryCountOnPage);
        }

        _renderContent(defaultStartIndex);
    };

    var _renderContent = function (startIndex) {
        ASC.CRM.InvoiceItemsView.invoiceItemsList = new Array();

        LoadingBanner.displayLoading();
        jq("#mainSelectAllInvoiceItems").prop("checked", false);

        _getInvoiceItems(startIndex);
    };

    var _initPageNavigatorControl = function (countOfRows, currentPageNumber) {
        window.invoiceItemsPageNavigator = new ASC.Controls.PageNavigator.init("invoiceItemsPageNavigator", "#divForInvoiceItemsPager", countOfRows, ASC.CRM.Data.VisiblePageCount, currentPageNumber,
                                                                        ASC.CRM.Resources.CRMJSResource.Previous, ASC.CRM.Resources.CRMJSResource.Next);

        window.invoiceItemsPageNavigator.changePageCallback = function (page) {
            _setCookie(page, window.invoiceItemsPageNavigator.EntryCountOnPage);

            var startIndex = window.invoiceItemsPageNavigator.EntryCountOnPage * (page - 1);
            _renderContent(startIndex);
        };
    };

    var _renderInvoiceItemsPageNavigator = function (startIndex) {
        var tmpTotal;
        if (startIndex >= ASC.CRM.InvoiceItemsView.Total) {
            tmpTotal = startIndex + 1;
        } else {
            tmpTotal = ASC.CRM.InvoiceItemsView.Total;
        }
        window.invoiceItemsPageNavigator.drawPageNavigator((startIndex / ASC.CRM.InvoiceItemsView.entryCountOnPage).toFixed(0) * 1 + 1, tmpTotal);
        jq("#tableForInvoiceItemsNavigation").show();
    };

    var _renderSimpleInvoiceItemsPageNavigator = function () {
        jq("#invoiceItemsHeaderMenu .menu-action-simple-pagenav").html("");
        var $simplePN = jq("<div></div>"),
            lengthOfLinks = 0;
        if (jq("#divForInvoiceItemsPager .pagerPrevButtonCSSClass").length != 0) {
            lengthOfLinks++;
            jq("#divForInvoiceItemsPager .pagerPrevButtonCSSClass").clone().appendTo($simplePN);
        }
        if (jq("#divForInvoiceItemsPager .pagerNextButtonCSSClass").length != 0) {
            lengthOfLinks++;
            if (lengthOfLinks === 2) {
                jq("<span style='padding: 0 8px;'>&nbsp;</span>").clone().appendTo($simplePN);
            }
            jq("#divForInvoiceItemsPager .pagerNextButtonCSSClass").clone().appendTo($simplePN);
        }
        if ($simplePN.children().length != 0) {
            $simplePN.appendTo("#invoiceItemsHeaderMenu .menu-action-simple-pagenav");
            jq("#invoiceItemsHeaderMenu .menu-action-simple-pagenav").show();
        } else {
            jq("#invoiceItemsHeaderMenu .menu-action-simple-pagenav").hide();
        }
    };

    var _renderCheckedInvoiceItemsCount = function (count) {
        if (count != 0) {
            jq("#invoiceItemsHeaderMenu .menu-action-checked-count > span").text(jq.format(ASC.CRM.Resources.CRMJSResource.ElementsSelectedCount, count));
            jq("#invoiceItemsHeaderMenu .menu-action-checked-count").show();
        } else {
            jq("#invoiceItemsHeaderMenu .menu-action-checked-count > span").text("");
            jq("#invoiceItemsHeaderMenu .menu-action-checked-count").hide();
        }
    };

    var _renderNoInvoiceItemsEmptyScreen = function () {
        jq("#invoiceItemsTable tbody tr").remove();
        jq("#invoiceItemsFilterContainer, #invoiceItemsHeaderMenu, #invoiceItemsList, #tableForInvoiceItemsNavigation").hide();
        ASC.CRM.Common.hideExportButtons();
        jq("#emptyContentForInvoiceItemsFilter").addClass("display-none");
        jq("#invoiceItemsEmptyScreen").removeClass("display-none");
    };

    var _renderNoInvoiceItemsForQueryEmptyScreen = function () {
        jq("#invoiceItemsTable tbody tr").remove();
        jq("#invoiceItemsHeaderMenu, #invoiceItemsList, #tableForInvoiceItemsNavigation").hide();
        ASC.CRM.Common.hideExportButtons();
        jq("#mainSelectAllInvoiceItems").attr("disabled", true);
        jq("#invoiceItemsEmptyScreen").addClass("display-none");
        jq("#emptyContentForInvoiceItemsFilter").removeClass("display-none");
    };

    var _showActionMenu = function (invoiceItemID) {
        var invoiceItem = null;
        for (var i = 0, n = ASC.CRM.InvoiceItemsView.invoiceItemsList.length; i < n; i++) {
            if (invoiceItemID == ASC.CRM.InvoiceItemsView.invoiceItemsList[i].id) {
                invoiceItem = ASC.CRM.InvoiceItemsView.invoiceItemsList[i];
                break;
            }
        }
        if (invoiceItem == null) return;

        if (invoiceItem.canEdit) {
            jq("#invoiceItemsActionMenu .editInvoiceItemLink").attr("href", jq.format("Settings.aspx?type=invoice_items&action=manage&id={0}", invoiceItemID)).show();
        } else {
            jq("#invoiceItemsActionMenu .editInvoiceItemLink").removeAttr("href").hide();
        }

        if (invoiceItem.canDelete) {
            jq("#invoiceItemsActionMenu .deleteInvoiceItemLink").unbind("click").click(function () {
                jq("#invoiceItemsActionMenu").hide();
                jq("#invoiceItemsTable .entity-menu.active").removeClass("active");
                _showConfirmationPanelForDelete(invoiceItem.title, invoiceItem.id);
            }).show();
        } else {
            jq("#invoiceItemsActionMenu .deleteInvoiceItemLink").unbind("click").hide();
        }
    };

    var _showConfirmationPanelForDelete = function (title, itemID) {
        jq("#confirmationDeleteOneItemPanel .confirmationAction>b").text(jq.format(ASC.CRM.Resources.CRMInvoiceResource.DeleteItemConfirmMessage, Encoder.htmlDecode(title)));

        jq("#confirmationDeleteOneItemPanel .middle-button-container>.button.blue.middle").unbind("click").bind("click", function () {
            _deleteInvoiceItem(itemID);
        });
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#confirmationDeleteOneItemPanel", 500);
    };

    var _deleteInvoiceItem = function (id) {

        var ids = new Array();
        ids.push(id);
        var params = { itemIDsForDelete: ids };

        Teamlab.removeCrmInvoiceItem(params, ids, {
            before: function () {
                jq("#invoiceItemsActionMenu").hide();
                jq("#invoiceItemsTable .entity-menu.active").removeClass("active");

            },
            success: callback_delete_batch_items
        });
    };


    var _getInvoiceItems = function (startIndex) {
        var filters = _getFilterSettings(startIndex);
        Teamlab.getCrmInvoiceItems({ startIndex: startIndex || 0 },
        {
            filter: filters,
            success: callback_get_invoice_items_by_filter
        });
    };

    var _resizeFilter = function () {
        var visible = jq("#invoiceItemsFilterContainer").is(":hidden") == false;
        if (ASC.CRM.InvoiceItemsView.isFilterVisible == false && visible) {
            ASC.CRM.InvoiceItemsView.isFilterVisible = true;
            if (ASC.CRM.InvoiceItemsView.advansedFilter) {
                jq("#invoiceItemsAdvansedFilter").advansedFilter("resize");
            }
        }
    };

    var _invoiceItemFactory = function (invoiceItem, selectedIDs) {
        var index = jq.inArray(invoiceItem.id, selectedIDs);
        invoiceItem.isChecked = index != -1;
        invoiceItem.priceFormat = ASC.CRM.Common.numberFormat(invoiceItem.price,
                                  {
                                      before: invoiceItem.currency.symbol,
                                      after: " " + invoiceItem.currency.abbreviation,
                                      thousands_sep: " ",
                                      dec_point: ASC.CRM.Data.CurrencyDecimalSeparator
                                  });
    };

    var callback_get_invoice_items_by_filter = function (params, invoiceItems) {
        ASC.CRM.InvoiceItemsView.Total = params.__total || 0;
        var startIndex = params.__startIndex || 0;

        if (ASC.CRM.InvoiceItemsView.Total === 0 &&
                    typeof (ASC.CRM.InvoiceItemsView.advansedFilter) != "undefined" &&
                    ASC.CRM.InvoiceItemsView.advansedFilter.advansedFilter().length == 1) {
            ASC.CRM.InvoiceItemsView.noInvoiceItems = true;
            ASC.CRM.InvoiceItemsView.noInvoiceItemsForQuery = true;
        } else {
            ASC.CRM.InvoiceItemsView.noInvoiceItems = false;
            if (ASC.CRM.InvoiceItemsView.Total === 0) {
                ASC.CRM.InvoiceItemsView.noInvoiceItemsForQuery = true;
            } else {
                ASC.CRM.InvoiceItemsView.noInvoiceItemsForQuery = false;
            }
        }

        if (ASC.CRM.InvoiceItemsView.noInvoiceItems) {
            _renderNoInvoiceItemsEmptyScreen();
            LoadingBanner.hideLoading();
            return false;
        }

        if (ASC.CRM.InvoiceItemsView.noInvoiceItemsForQuery) {
            _renderNoInvoiceItemsForQueryEmptyScreen();

            jq("#invoiceItemsFilterContainer").show();
            _resizeFilter();
            LoadingBanner.hideLoading();
            return false;
        }

        if (invoiceItems.length == 0) {//it can happen when select page without elements after deleting
            jq("invoiceItemsEmptyScreen").addClass("display-none");
            jq("#emptyContentForInvoiceItemsFilter").addClass("display-none");
            jq("#invoiceItemsTable tbody tr").remove();
            jq("#invoiceItemsFilterContainer, #invoiceItemsHeaderMenu, #tableForInvoiceItemsNavigation").show();
            jq("#mainSelectAllInvoiceItems").attr("disabled", true);
            ASC.CRM.Common.hideExportButtons();
            LoadingBanner.hideLoading();
            return false;
        }

        jq("#totalInvoiceItemsOnPage").text(ASC.CRM.InvoiceItemsView.Total);

        jq("#emptyContentForInvoiceItemsFilter").addClass("display-none");
        jq("#invoiceItemsEmptyScreen").addClass("display-none");
        ASC.CRM.Common.showExportButtons();
        jq("#invoiceItemsFilterContainer").show();
        _resizeFilter();
        jq("#mainSelectAllInvoiceItems").removeAttr("disabled");
        var selectedIDs = new Array();
        for (var i = 0, n = ASC.CRM.InvoiceItemsView.selectedItems.length; i < n; i++) {
            selectedIDs.push(ASC.CRM.InvoiceItemsView.selectedItems[i].id);
        }

        for (var i = 0, n = invoiceItems.length; i < n; i++) {
            _invoiceItemFactory(invoiceItems[i], selectedIDs);
        }
        ASC.CRM.InvoiceItemsView.invoiceItemsList = invoiceItems;


        jq("#invoiceItemsFilterContainer, #invoiceItemsHeaderMenu, #invoiceItemsList").show();
        jq("#invoiceItemsTable tbody").replaceWith(jq.tmpl("invoiceItemsListTmpl", { invoiceItems: ASC.CRM.InvoiceItemsView.invoiceItemsList }));
        ASC.CRM.Common.tooltip(".invoiceItemTitle", "tooltip", false);
        ASC.CRM.Common.tooltip(".invoiceTax1>div", "tooltipTax1", true);
        ASC.CRM.Common.tooltip(".invoiceTax2>div", "tooltipTax2", true);
        ASC.CRM.InvoiceItemsView.checkFullSelection();

        ASC.CRM.Common.RegisterContactInfoCard();
        _renderInvoiceItemsPageNavigator(startIndex);
        _renderSimpleInvoiceItemsPageNavigator();

        window.scrollTo(0, 0);
        ScrolledGroupMenu.fixContentHeaderWidth(jq('#invoiceItemsHeaderMenu'));
        LoadingBanner.hideLoading();
    };


    var callback_delete_batch_items = function (params, invoiceItems) {
        var newItemsList = new Array();
        for (var i = 0, len_i = ASC.CRM.InvoiceItemsView.invoiceItemsList.length; i < len_i; i++) {
            var isDeleted = false;
            for (var j = 0, len_j = params.itemIDsForDelete.length; j < len_j; j++)
                if (params.itemIDsForDelete[j] == ASC.CRM.InvoiceItemsView.invoiceItemsList[i].id) {
                    isDeleted = true;
                    break;
                }
            if (!isDeleted) {
                newItemsList.push(ASC.CRM.InvoiceItemsView.invoiceItemsList[i]);
            }
        }
        ASC.CRM.InvoiceItemsView.invoiceItemsList = newItemsList;

        ASC.CRM.InvoiceItemsView.Total -= params.itemIDsForDelete.length;
        jq("#totalInvoiceItemsOnPage").text(ASC.CRM.InvoiceItemsView.Total);

        var selectedIDs = new Array();
        for (var i = 0, n = ASC.CRM.InvoiceItemsView.selectedItems.length; i < n; i++) {
            selectedIDs.push(ASC.CRM.InvoiceItemsView.selectedItems[i].id);
        }

        for (var i = 0, len = params.itemIDsForDelete.length; i < len; i++) {
            var $objForRemove = jq("#invoiceItem_" + params.itemIDsForDelete[i]);
            if ($objForRemove.length != 0) {
                $objForRemove.remove();
            }

            var index = jq.inArray(params.itemIDsForDelete[i], selectedIDs);
            if (index != -1) {
                selectedIDs.splice(index, 1);
                ASC.CRM.InvoiceItemsView.selectedItems.splice(index, 1);
            }
        }
        jq("#mainSelectAllInvoiceItems").prop("checked", false);

        _checkForLockMainActions();
        _renderCheckedInvoiceItemsCount(ASC.CRM.InvoiceItemsView.selectedItems.length);

        if (ASC.CRM.InvoiceItemsView.Total == 0
            && (typeof (ASC.CRM.InvoiceItemsView.advansedFilter) == "undefined"
            || ASC.CRM.InvoiceItemsView.advansedFilter.advansedFilter().length == 1)) {
            ASC.CRM.InvoiceItemsView.noCases = true;
            ASC.CRM.InvoiceItemsView.noCasesForQuery = true;
        } else {
            ASC.CRM.InvoiceItemsView.noCases = false;
            if (ASC.CRM.InvoiceItemsView.Total === 0) {
                ASC.CRM.InvoiceItemsView.noInvoiceItemsForQuery = true;
            } else {
                ASC.CRM.InvoiceItemsView.noInvoiceItemsForQuery = false;
            }
        }
        PopupKeyUpActionProvider.EnableEsc = true;
        if (ASC.CRM.InvoiceItemsView.noInvoiceItems) {
            _renderNoInvoiceItemsEmptyScreen();
            jq.unblockUI();
            return false;
        }

        if (ASC.CRM.InvoiceItemsView.noInvoiceItemsForQuery) {
            _renderNoInvoiceItemsForQueryEmptyScreen();
            jq.unblockUI();
            return false;
        }

        if (jq("#invoiceItemsTable tbody tr").length == 0) {
            jq.unblockUI();

            var startIndex = ASC.CRM.InvoiceItemsView.entryCountOnPage * (window.invoiceItemsPageNavigator.CurrentPageNumber - 1);
            if (startIndex >= ASC.CRM.InvoiceItemsView.Total) {
                startIndex -= ASC.CRM.InvoiceItemsView.entryCountOnPage;
            }
            _renderContent(startIndex);
        } else {
            jq.unblockUI();
        }
    };

    var _lockMainActions = function () {
        //jq("#invoiceItemsHeaderMenu .menuActionDelete").removeClass("unlockAction").unbind("click");
    };

    var _checkForLockMainActions = function () {
        if (ASC.CRM.InvoiceItemsView.selectedItems.length === 0) {
            _lockMainActions();
            return;
        }
    };

    var _initInvoiceItemsActionMenu = function () {
        jq.dropdownToggle({
            dropdownID: "invoiceItemsActionMenu",
            switcherSelector: "#invoiceItemsTable .entity-menu",
            addTop: 0,
            addLeft: 10,
            rightPos: true,
            beforeShowFunction: function (switcherObj, dropdownItem) {
                var invoiceId = switcherObj.attr("id").split('_')[1];
                if (!invoiceId) { return; }
                _showActionMenu(parseInt(invoiceId));
            },
            showFunction: function (switcherObj, dropdownItem) {
                jq("#invoiceItemsTable .entity-menu.active").removeClass("active");
                if (dropdownItem.is(":hidden")) {
                    switcherObj.addClass("active");
                }
            },
            hideFunction: function () {
                jq("#invoiceItemsTable .entity-menu.active").removeClass("active");
            }
        });


        jq.tmpl("template-blockUIPanel", {
            id: "confirmationDeleteOneItemPanel",
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
            progressText: ASC.CRM.Resources.CRMInvoiceResource.DeleteInvoiceItemInProgress
        }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");
    };

    var _initScrolledGroupMenu = function () {
        ScrolledGroupMenu.init({
            menuSelector: "#invoiceItemsHeaderMenu",
            menuAnchorSelector: "#invoiceItemsHeaderMenu .menuActionCreateNew",
            menuSpacerSelector: "main .filter-content .header-menu-spacer",
            userFuncInTop: function () { jq("#invoiceItemsHeaderMenu .menu-action-on-top").hide(); },
            userFuncNotInTop: function () { jq("#invoiceItemsHeaderMenu .menu-action-on-top").show(); }
        });


        jq("#invoiceItemsHeaderMenu .menuActionCreateNew").on("click", function() {
            location.href = "Settings.aspx?type=invoice_items&action=manage";
        });

    };

    var _initOtherActionMenu = function () {
        jq("#menuCreateNewTask").on("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };


    var _createShortInvoiceItem = function (invoiceItem) {
        var shortInvoiceItem = {
            id: invoiceItem.id,
            title: invoiceItem.title
        };
        return shortInvoiceItem;
    };

    var _preInitPage = function (entryCountOnPage) {
        jq("#mainSelectAllInvoiceItems").prop("checked", false);//'cause checkboxes save their state between refreshing the page

        jq("#tableForInvoiceItemsNavigation select:first")
            .val(entryCountOnPage)
            .change(function () {
                ASC.CRM.InvoiceItemsView.changeCountOfRows(this.value);
            })
            .tlCombobox();
    };

    var _initFilter = function () {
        if (!jq("#invoiceItemsAdvansedFilter").advansedFilter) return;

        ASC.CRM.InvoiceItemsView.advansedFilter = jq("#invoiceItemsAdvansedFilter")
            .advansedFilter({
                anykey: false,
                hintDefaultDisable: true,
                maxfilters: -1,
                colcount: 1,
                maxlength: "100",
                store: true,
                inhash: true,
                filters:
                [
                //{
                //    type: "combobox",
                //    id: "active",
                //    apiparamname: "status",
                //    title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemActiveStatus,
                //    filtertitle: ASC.CRM.Resources.CRMInvoiceResource.InvoicesByStatus,
                //    group: ASC.CRM.Resources.CRMInvoiceResource.InvoicesByStatus,
                //    groupby: "invoiceItemStatus",
                //    options: [
                //            { value: "active", classname: '', title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemActiveStatus, def: true },
                //            { value: "archived", classname: '', title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemArchivedStatus }
                //    ]
                //},
                //{
                //    type: "combobox",
                //    id: "archived",
                //    apiparamname: "status",
                //    title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemArchivedStatus,
                //    filtertitle: ASC.CRM.Resources.CRMInvoiceResource.InvoicesByStatus,
                //    group: ASC.CRM.Resources.CRMInvoiceResource.InvoicesByStatus,
                //    groupby: "invoiceItemStatus",
                //    options: [
                //            { value: "active", classname: '', title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemActiveStatus },
                //            { value: "archived", classname: '', title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemArchivedStatus, def: true }
                //    ]
                //},
                {
                    type: "combobox",
                    id: "withInventoryStock",
                    apiparamname: "inventoryStock",
                    title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemWithIventoryStock,
                    group: ASC.CRM.Resources.CRMCommonResource.Other,
                    groupby: "other",
                    options: [
                            { value: true, classname: '', title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemWithIventoryStock, def: true },
                            { value: false, classname: '', title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemWithoutIventoryStock }
                    ]
                },
                {
                    type: "combobox",
                    id: "withoutInventoryStock",
                    apiparamname: "inventoryStock",
                    title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemWithoutIventoryStock,
                    group: ASC.CRM.Resources.CRMCommonResource.Other,
                    groupby: "other",
                    options: [
                            { value: true, classname: '', title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemWithIventoryStock },
                            { value: false, classname: '', title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemWithoutIventoryStock, def: true }
                    ]
                }],
                sorters: [
                            { id: "name", title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemName, dsc: false, def: true },
                            { id: "sku", title: ASC.CRM.Resources.CRMInvoiceResource.StockKeepingUnit, dsc: false, def: false },
                            { id: "price", title: ASC.CRM.Resources.CRMInvoiceResource.InvoiceItemPrice, dsc: false, def: false },
                            { id: "quantity", title: ASC.CRM.Resources.CRMInvoiceResource.QuantityItem, dsc: false, def: false },
                            { id: "created", title: ASC.CRM.Resources.CRMCommonResource.CreateDate, dsc: false, def: false }
                ]
            })
            .bind("setfilter", ASC.CRM.InvoiceItemsView.setFilter)
            .bind("resetfilter", ASC.CRM.InvoiceItemsView.resetFilter);

        jq("#invoiceItemsAdvansedFilter .advansed-filter-input").prop("placeholder", ASC.CRM.Resources.CRMInvoiceResource.InfoiceItemsFilterWatermarkText);
    };

    var _initEmptyScreen = function () {
        //init emptyScreen for all list
        var buttonHtml = ["<a class='link dotline plus' href='Settings.aspx?type=invoice_items&action=manage'>",
            ASC.CRM.Resources.CRMInvoiceResource.CreateFirstInvoiceItem,
            "</a>"].join('');

        jq.tmpl("template-emptyScreen",
            {
                ID: "invoiceItemsEmptyScreen",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_products_services"],
                Header: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoiceItemsHeader,
                Describe: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoiceItemsDescribe,
                ButtonHTML: buttonHtml,
                CssClass: "display-none"
            }).insertAfter("#invoiceItemsList");

        //init emptyScreen for filter
        jq.tmpl("template-emptyScreen",
            {
                ID: "emptyContentForInvoiceItemsFilter",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_filter"],
                Header: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoiceItemsFilterHeader,
                Describe: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoiceItemsFilterDescribe,
                ButtonHTML: ["<a class='clearFilterButton link dotline' href='javascript:void(0);'",
                    "onclick='ASC.CRM.InvoiceItemsView.advansedFilter.advansedFilter(null);'>",
                    ASC.CRM.Resources.CRMCommonResource.ClearFilter,
                    "</a>"].join(''),
                CssClass: "display-none"
            }).insertAfter("#invoiceItemsList");
    };

    return {
        invoiceItemsList: new Array(),
        selectedItems: new Array(),

        isFilterVisible: false,

        entryCountOnPage: 0,
        defaultCurrentPageNumber: 0,
        noInvoiceItems: false,
        noInvoiceItemsForQuery: false,

        init: function () {
            
            var settings = {
                page: 1,
                countOnPage: jq("#tableForInvoiceItemsNavigation select:first>option:first").val()
            },
                key = location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') + location.pathname + location.search,
                currentAnchor = location.hash,
                cookieKey = encodeURIComponent(key);

            currentAnchor = currentAnchor && typeof currentAnchor === 'string' && currentAnchor.charAt(0) === '#'
                ? currentAnchor.substring(1)
                : currentAnchor;

            var cookieAnchor = jq.cookies.get(cookieKey);
            if (currentAnchor == "" || cookieAnchor == currentAnchor) {
                var tmp = ASC.CRM.Common.getPagingParamsFromCookie(ASC.CRM.Data.CookieKeyForPagination["invoiceitems"]);
                if (tmp != null) {
                    settings = tmp;
                }
            } else {
                _setCookie(settings.page, settings.countOnPage);
            }

            ASC.CRM.InvoiceItemsView.entryCountOnPage = settings.countOnPage;
            ASC.CRM.InvoiceItemsView.defaultCurrentPageNumber = settings.page;

            _preInitPage(ASC.CRM.InvoiceItemsView.entryCountOnPage);
            _initEmptyScreen();

            _initPageNavigatorControl(ASC.CRM.InvoiceItemsView.entryCountOnPage, ASC.CRM.InvoiceItemsView.defaultCurrentPageNumber);

            _initInvoiceItemsActionMenu();

            _initScrolledGroupMenu();

            _initOtherActionMenu();

            _initFilter();

            /*tracking events*/
            ASC.CRM.InvoiceItemsView.advansedFilter.one("adv-ready", function () {
                var crmAdvansedFilterContainer = jq("#invoiceItemsAdvansedFilter .advansed-filter-list");
                jq("#invoiceItemsAdvansedFilter .btn-toggle-sorter").trackEvent(ga_Categories.invoice_item, ga_Actions.filterClick, "sort");
                jq("#invoiceItemsAdvansedFilter .advansed-filter-input").trackEvent(ga_Categories.invoice_item, ga_Actions.filterClick, "search_text", "enter");
            });
            
            ASC.CRM.PartialExport.init(ASC.CRM.InvoiceItemsView.advansedFilter, "invoiceitem");
        },

        setFilter: function (evt, $container, filter, params, selectedfilters) { _changeFilter(); },
        resetFilter: function (evt, $container, filter, selectedfilters) { _changeFilter(); },

        selectAll: function (obj) {
            var isChecked = jq(obj).is(":checked"),
                selectedIDs = new Array();

            for (var i = 0, n = ASC.CRM.InvoiceItemsView.selectedItems.length; i < n; i++) {
                selectedIDs.push(ASC.CRM.InvoiceItemsView.selectedItems[i].id);
            }

            for (var i = 0, len = ASC.CRM.InvoiceItemsView.invoiceItemsList.length; i < len; i++) {
                var invoiceItem = ASC.CRM.InvoiceItemsView.invoiceItemsList[i],
                    index = jq.inArray(invoiceItem.id, selectedIDs);
                if (isChecked && index == -1) {
                    ASC.CRM.InvoiceItemsView.selectedItems.push(_createShortInvoiceItem(invoiceItem));
                    selectedIDs.push(invoiceItem.id);
                    jq("#invoiceItem_" + invoiceItem.id).addClass("selected");
                    jq("#checkInvoiceItem_" + invoiceItem.id).prop("checked", true);
                }
                if (!isChecked && index != -1) {
                    ASC.CRM.InvoiceItemsView.selectedItems.splice(index, 1);
                    selectedIDs.splice(index, 1);
                    jq("#invoiceItem_" + invoiceItem.id).removeClass("selected");
                    jq("#checkInvoiceItem_" + invoiceItem.id).prop("checked", false);
                }
            }
            _renderCheckedInvoiceItemsCount(ASC.CRM.InvoiceItemsView.selectedItems.length);
            _checkForLockMainActions();
        },

        selectItem: function (obj) {
            var id = parseInt(jq(obj).attr("id").split("_")[1]),
                selectedInvoiceItem = null,
                selectedIDs = new Array();

            for (var i = 0, n = ASC.CRM.InvoiceItemsView.invoiceItemsList.length; i < n; i++) {
                if (id == ASC.CRM.InvoiceItemsView.invoiceItemsList[i].id) {
                    selectedInvoiceItem = _createShortInvoiceItem(ASC.CRM.InvoiceItemsView.invoiceItemsList[i]);
                }
            }

            for (var i = 0, n = ASC.CRM.InvoiceItemsView.selectedItems.length; i < n; i++) {
                selectedIDs.push(ASC.CRM.InvoiceItemsView.selectedItems[i].id);
            }

            var index = jq.inArray(id, selectedIDs);

            if (jq(obj).is(":checked")) {
                jq(obj).parents("tr:first").addClass("selected");
                if (index == -1) {
                    ASC.CRM.InvoiceItemsView.selectedItems.push(selectedInvoiceItem);
                }
                ASC.CRM.InvoiceItemsView.checkFullSelection();
            } else {
                jq("#mainSelectAllInvoiceItems").prop("checked", false);
                jq(obj).parents("tr:first").removeClass("selected");
                if (index != -1) {
                    ASC.CRM.InvoiceItemsView.selectedItems.splice(index, 1);
                }
            }
            _renderCheckedInvoiceItemsCount(ASC.CRM.InvoiceItemsView.selectedItems.length);
            _checkForLockMainActions();
        },

        deselectAll: function () {
            ASC.CRM.InvoiceItemsView.selectedItems = new Array();
            _renderCheckedInvoiceItemsCount(0);
            jq("#invoiceItemsTable input:checkbox").prop("checked", false);
            jq("#mainSelectAllInvoiceItems").prop("checked", false);
            jq("#invoiceItemsTable tr.selected").removeClass("selected");
            _lockMainActions();
        },

        checkFullSelection: function () {
            var rowsCount = jq("#invoiceItemsTable tbody tr").length,
                selectedRowsCount = jq("#invoiceItemsTable input[id^=checkInvoiceItem_]:checked").length;
            jq("#mainSelectAllInvoiceItems").prop("checked", rowsCount == selectedRowsCount);
        },

        changeCountOfRows: function (newValue) {
            if (isNaN(newValue)) {
                return;
            }
            var newCountOfRows = newValue * 1;
            ASC.CRM.InvoiceItemsView.entryCountOnPage = newCountOfRows;
            invoiceItemsPageNavigator.EntryCountOnPage = newCountOfRows;

            _setCookie(1, newCountOfRows);
            _renderContent(0);
        }
    };
})();


ASC.CRM.InvoiceItemActionView = (function () {

    var _initOtherActionMenu = function () {
        jq("#menuCreateNewTask").on("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    var _saveItem = function (backToList) {
        if (jq("#crm_invoiceItemMakerDialog .button.disable").length != 0) { return; }

        var isValid = true;
        jq("#crm_invoiceItemMakerDialog .button:not(.disable)").addClass("disable");

        var priceVal = jq.trim(jq("#crm_invoiceItemMakerDialog .invoiceItemPrice").val()),
            trackInventory = jq("#invoiceItemInventoryStock").prop("checked"),
            stockQuantityVal = trackInventory == true ? jq.trim(jq("#crm_invoiceItemMakerDialog .invoiceItemStockQuantity").val()) : "0",

            item = {
                title: jq.trim(jq("#crm_invoiceItemMakerDialog .invoiceItemTitle").val()),
                description: jq.trim(jq("#crm_invoiceItemMakerDialog .invoiceItemDescr").val()),
                price: Number(priceVal.replace(ASC.CRM.Data.CurrencyDecimalSeparator, '.')),
                sku: jq.trim(jq("#crm_invoiceItemMakerDialog .invoiceItemSKU").val()),
                stockQuantity: Number(stockQuantityVal.replace(ASC.CRM.Data.CurrencyDecimalSeparator, '.')),
                trackInventory: trackInventory,
                invoiceTax1id: jq("#tax1Select").val(),
                invoiceTax2id: jq("#tax2Select").val()
            };
        HideRequiredError();
        if (item.title == "") {
            ShowRequiredError(jq("#crm_invoiceItemMakerDialog .invoiceItemTitle"));
            isValid = false;
        }

        if (priceVal == "" || isNaN(item.price) || item.price <= 0 || item.price > ASC.CRM.Data.MaxInvoiceItemPrice) {
            if (priceVal == "") {
                AddRequiredErrorText(jq("#crm_invoiceItemMakerDialog .invoiceItemPrice"), ASC.CRM.Resources.CRMInvoiceResource.ErrorEmptyPrice);
            } else {
                AddRequiredErrorText(jq("#crm_invoiceItemMakerDialog .invoiceItemPrice"), ASC.CRM.Resources.CRMInvoiceResource.ErrorIncorrectPrice + " (max " + ASC.CRM.Data.MaxInvoiceItemPrice + ")");
            }
            ShowRequiredError(jq("#crm_invoiceItemMakerDialog .invoiceItemPrice"));
            isValid = false;
        }

        if (isNaN(item.stockQuantity)) {
            jq("#crm_invoiceItemMakerDialog .invoiceItemStockQuantity").parent().children(".requiredErrorText").show();
            isValid = false;
        }
        if (!isValid) {
            jq("#crm_invoiceItemMakerDialog .button.disable").removeClass("disable");
            return;
        }

        LoadingBanner.strLoading = jq.getURLParam("id") == null ?
            ASC.CRM.Resources.CRMInvoiceResource.CreateInvoiceItemProggress :
            ASC.CRM.Resources.CRMCommonResource.SaveChangesProggress;
        LoadingBanner.showLoaderBtn("#crm_invoiceItemMakerDialog");

        jq("#crm_invoiceItemMakerDialog input, #crm_invoiceItemMakerDialog textarea,  #crm_invoiceItemMakerDialog select")
            .attr("disabled", "disabled")
            .attr("readonly", "readonly")
            .addClass('disabled');

        if (jq.getURLParam("id") == null) {
            Teamlab.addCrmInvoiceItem({}, item,
            {
                success: function (params, item) {
                    if (backToList) {
                        location.href = "Settings.aspx?type=invoice_items";
                    } else {
                        location.href = "Settings.aspx?type=invoice_items&action=manage";
                    }
                },
                before: function () {
                },
                after: function () {
                    jq("#crm_invoiceItemMakerDialog input, #crm_invoiceItemMakerDialog textarea,  #crm_invoiceItemMakerDialog select")
                        .removeAttr("readonly")
                        .removeAttr("disabled")
                        .removeClass("disabled");
                    jq("#crm_invoiceItemMakerDialog .button.disabled").removeClass("disabled");
                    LoadingBanner.hideLoaderBtn("#crm_invoiceItemMakerDialog");
                }
            });
        } else {
            item.id = jq.getURLParam("id");
            Teamlab.updateCrmInvoiceItem({}, item.id, item,
            {
                success: function (params, item) {
                    if (backToList) {
                        location.href = "Settings.aspx?type=invoice_items";
                    } else {
                        location.href = "Settings.aspx?type=invoice_items&action=manage";
                    }
                },
                before: function () { },
                after: function () {
                    jq("#crm_invoiceItemMakerDialog input, #crm_invoiceItemMakerDialog textarea,  #crm_invoiceItemMakerDialog select")
                        .removeAttr("readonly")
                        .removeAttr("disabled")
                        .removeClass("disabled");
                    jq("#crm_invoiceItemMakerDialog .button.disabled").removeClass("disabled");
                    LoadingBanner.hideLoaderBtn("#crm_invoiceItemMakerDialog");
                }
            });
        }

    };

    return {
        init: function (invItem, invItemCurrency) {
            var selectedInvoiceTax1ID = 0,
                selectedInvoiceTax2ID = 0,
                trackInventory = false;

            if (typeof (invItem) != "undefined" && invItem != "" && invItem != null) {
                invItem = jq.parseJSON(jq.base64.decode(invItem));
                if (invItem!= null){
                    jq("#crm_invoiceItemMakerDialog .invoiceItemSKU").val(invItem.stockKeepingUnit);
                    jq("#crm_invoiceItemMakerDialog .invoiceItemTitle").val(invItem.title);
                    jq("#crm_invoiceItemMakerDialog .invoiceItemDescr").text(invItem.description);
                    jq("#crm_invoiceItemMakerDialog .invoiceItemPrice").val(invItem.price.toFixed(2).replace('.', ASC.CRM.Data.CurrencyDecimalSeparator));
                    jq("#crm_invoiceItemMakerDialog .invoiceItemStockQuantity").val(invItem.stockQuantity.toFixed(2).replace('.', ASC.CRM.Data.CurrencyDecimalSeparator));
                    selectedInvoiceTax1ID = invItem.invoiceTax1ID;
                    selectedInvoiceTax2ID = invItem.invoiceTax2ID;
                    trackInventory = invItem.trackInventory;
                }
            }

            jq("#crm_invoiceItemMakerDialog .invoiceItemCurrency").text(invItemCurrency.abbreviation);

            //var html = "";
            //for (var i = 0, n = window.ASC.CRM.Data.currencies.length; i < n; i++) {
            //     html += ["<option value='",
            //        window.ASC.CRM.Data.currencies[i].Abbreviation,
            //        "'>",
            //        jq.htmlEncodeLight([window.ASC.CRM.Data.currencies[i].Abbreviation, " - ", window.ASC.CRM.Data.currencies[i].Title].join('')),
            //        "</option>"
            //    ].join('');
            //}
            //jq("#currencySelect").html(html);

            var html1 = "<option value='0'></option>",
                html2 = "<option value='0'></option>";

            for (var i = 0, n = window.ASC.CRM.Data.taxes.length; i < n; i++) {
                var elt = window.ASC.CRM.Data.taxes[i];
                html1 += ["<option value='",
                   elt.id,
                   "'",
                   selectedInvoiceTax1ID == elt.id ? " selected='selected'" : "",
                   ">",
                   jq.htmlEncodeLight(elt.name),
                   "</option>"
                ].join('');

                html2 += ["<option value='",
                   elt.id,
                   "'",
                   selectedInvoiceTax2ID == elt.id ? " selected='selected'" : "",
                   ">",
                   jq.htmlEncodeLight(elt.name),
                   "</option>"
                ].join('');
            }

            jq("#tax1Select").html(html1);
            jq("#tax2Select").html(html2);

            if (trackInventory) {
                jq("#invoiceItemInventoryStock").prop("checked", true);
                jq("#crm_invoiceItemMakerDialog .currentQuantity").removeClass("display-none");
            } else {
                jq("#invoiceItemInventoryStock").prop("checked", false);
            }
            jq("#invoiceItemInventoryStock").on("change", function () {
                if (jq(this).prop("checked")) {
                    jq("#crm_invoiceItemMakerDialog .currentQuantity").removeClass("display-none");
                } else {
                    jq("#crm_invoiceItemMakerDialog .currentQuantity").addClass("display-none");
                }
            });
            
            jq("#iventoryStockHelpSwitcher").on("click", function () {
                jq(this).helper({ BlockHelperID: 'iventoryStockHelpInfo' });
            });
            
            jq("#itemCurrencyHelpSwitcher").on("click", function () {
                jq(this).helper({ BlockHelperID: 'itemCurrencyHelpInfo', inRelativeCnt: true });
            });

            //jq.dropdownToggle({
            //    dropdownID: "addTagDialog",
            //    switcherSelector: "#addNewTag",
            //    addTop: 1,
            //    addLeft: 0,
            //    simpleToggle: true
            //});

            jq.forceNumber({
                parent: "#crm_invoiceItemMakerDialog",
                input: ".invoiceItemStockQuantity, .invoiceItemPrice",
                integerOnly: false,
                positiveOnly: true,
                separator: ASC.CRM.Data.CurrencyDecimalSeparator,
                lengthAfterSeparator: 2
            });


            jq("#saveItemButton").click(function () {
                _saveItem(true);
            });
            jq("#saveAndCreateItemButton").click(function () {
                _saveItem(false);
            });


            _initOtherActionMenu();

        }

    };
})();


ASC.CRM.InvoiceTaxesView = (function () {

    var _showNoTaxesEmptyScreen = function () {
        jq("#invoiceTaxesList").hide();
        jq("#invoiceTaxesEmptyScreen.display-none").removeClass("display-none");
    };

    var _showActionMenu = function (invoiceTaxID) {
        var invoiceTax = null;
        for (var i = 0, n = ASC.CRM.InvoiceTaxesView.invoiceTaxesList.length; i < n; i++) {
            if (invoiceTaxID == ASC.CRM.InvoiceTaxesView.invoiceTaxesList[i].id) {
                invoiceTax = ASC.CRM.InvoiceTaxesView.invoiceTaxesList[i];
                break;
            }
        }
        if (invoiceTax == null) return;

        if (invoiceTax.canEdit) {
            jq("#invoiceTaxesActionMenu .editInvoiceTaxLink").unbind("click").click(function () {
                jq("#invoiceTaxesActionMenu").hide();
                jq("#invoiceTaxesTable .entity-menu.active").removeClass("active");
                _showAddOrUpdateTaxPanel(invoiceTax);
            }).show();
        } else {
            jq("#invoiceTaxesActionMenu .editInvoiceTaxLink").unbind("click").hide();
        }

        if (invoiceTax.canDelete) {
            jq("#invoiceTaxesActionMenu .deleteInvoiceTaxLink").unbind("click").click(function () {
                jq("#invoiceTaxesActionMenu").hide();
                jq("#invoiceTaxesTable .entity-menu.active").removeClass("active");
                _showConfirmationPanelForDelete(invoiceTax.name, invoiceTax.id);
            }).show();
        } else {
            jq("#invoiceTaxesActionMenu .deleteInvoiceTaxLink").unbind("click").hide();
        }
    };

    var _invoiceTaxFactory = function (invoiceTax) { };

    var callback_get_invoice_taxes = function (params, invoiceTaxes) {
        if (invoiceTaxes.length == 0) {
            _showNoTaxesEmptyScreen();
            LoadingBanner.hideLoading();
            return false;
        }

        jq("#invoiceTaxesEmptyScreen").addClass("display-none");

        for (var i = 0, n = invoiceTaxes.length; i < n; i++) {
            _invoiceTaxFactory(invoiceTaxes[i]);
        }
        ASC.CRM.InvoiceTaxesView.invoiceTaxesList = invoiceTaxes;

        jq("#invoiceTaxesList").show();
        jq("#invoiceTaxesTable tbody").replaceWith(jq.tmpl("invoiceTaxesListTmpl", { invoiceTaxes: ASC.CRM.InvoiceTaxesView.invoiceTaxesList }));

        LoadingBanner.hideLoading();
    };

    var _initInvoiceTaxesActionMenu = function () {
        jq.dropdownToggle({
            dropdownID: "invoiceTaxesActionMenu",
            switcherSelector: "#invoiceTaxesTable .entity-menu",
            addTop: 0,
            addLeft: 10,
            rightPos: true,
            beforeShowFunction: function (switcherObj, dropdownItem) {
                var invoiceId = switcherObj.attr("id").split('_')[1];
                if (!invoiceId) { return; }
                _showActionMenu(parseInt(invoiceId));
            },
            showFunction: function (switcherObj, dropdownItem) {
                jq("#invoiceTaxesTable .entity-menu.active").removeClass("active");
                if (dropdownItem.is(":hidden")) {
                    switcherObj.addClass("active");
                }
            },
            hideFunction: function () {
                jq("#invoiceTaxesTable .entity-menu.active").removeClass("active");
            }
        });

        jq.tmpl("template-blockUIPanel", {
            id: "confirmationDeleteOneTaxPanel",
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
            progressText: ASC.CRM.Resources.CRMInvoiceResource.DeleteTaxInProgress
        }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");
    };

    var _initOtherActionMenu = function () {
        jq("#menuCreateNewTask").on("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    var _initEmptyScreen = function () {
        //init emptyScreen for all list
        var buttonHtml = ["<a class='link dotline plus'>",
            ASC.CRM.Resources.CRMInvoiceResource.CreateFirstInvoiceTax,
            "</a>"].join('');

        jq.tmpl("template-emptyScreen",
            {
                ID: "invoiceTaxesEmptyScreen",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_taxes"],
                Header: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoiceTaxesHeader,
                Describe: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoiceTaxesDescribe,
                ButtonHTML: buttonHtml,
                CssClass: "display-none"
            }).insertAfter("#invoiceTaxesList");

        jq('#invoiceTaxesEmptyScreen .emptyScrBttnPnl a.link.plus').on('click', function () {
            _showAddOrUpdateTaxPanel(null);
        });
    };

    var _resetManageTaxPanel = function (tax) {

        if (typeof (tax) != "undefined" && tax != null) {
            jq('#manageTax div.containerHeaderBlock td:first').text(jq.format(ASC.CRM.Resources.CRMInvoiceResource.UpdateTaxHeaderText, tax.name));
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMInvoiceResource.UpdateTaxProcessText;
        } else {
            jq('#manageTax div.containerHeaderBlock td:first').text(ASC.CRM.Resources.CRMInvoiceResource.AddTaxHeaderText);
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMInvoiceResource.AddTaxProcessText;
            tax = { name: "", rate: 0, description: "" };
        }

        jq("#manageTax .taxName").val(tax.name);
        jq("#manageTax .taxRate").val(tax.rate.toFixed(2).replace('.', ASC.CRM.Data.CurrencyDecimalSeparator));
        jq("#manageTax textarea").val(tax.description);
        RemoveRequiredErrorClass(jq("#manageTax .taxName"));
        RemoveRequiredErrorClass(jq("#manageTax .taxRate"));
    };

    var _showAddOrUpdateTaxPanel = function (tax) {
        _resetManageTaxPanel(tax);

        jq('#manageTax .middle-button-container a.button.blue.middle').unbind('click').click(function () {
            _addOrUpdateTax(typeof(tax) != "undefined" && tax != null ? tax.id : 0);
        });

        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#manageTax", 400);
    };

    var _showConfirmationPanelForDelete = function (title, taxID) {
        jq("#confirmationDeleteOneTaxPanel .confirmationAction>b").text(jq.format(ASC.CRM.Resources.CRMInvoiceResource.DeleteTaxConfirmMessage, Encoder.htmlDecode(title)));

        jq("#confirmationDeleteOneTaxPanel .middle-button-container>.button.blue.middle").unbind("click").bind("click", function () {
            _deleteTax(taxID);
        });
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#confirmationDeleteOneTaxPanel", 500);
    };

    var _addOrUpdateTax = function (id) {
        var isValid = true;
        var $obj = jq("#manageTax .taxName");

        if ($obj.val().trim() == "") {
            ShowRequiredError($obj, true);
            isValid = false;
        } else {
            RemoveRequiredErrorClass($obj);
        }

        $obj = jq("#manageTax .taxRate");
        if ($obj.val().trim() == "") {
            AddRequiredErrorText($obj, ASC.CRM.Resources.CRMInvoiceResource.EmptyTaxRateError);
            ShowRequiredError($obj, true);
            isValid = false;
        } else {
            var rate = Number($obj.val().replace(ASC.CRM.Data.CurrencyDecimalSeparator, '.'));
            if (Math.abs(rate) > 100) {
                AddRequiredErrorText($obj, ASC.CRM.Resources.CRMInvoiceResource.ErrorIncorrectRate);
                ShowRequiredError($obj, true);
                isValid = false;
            } else RemoveRequiredErrorClass($obj);
        }
        if (!isValid) { return; }

        var $listItems = jq("#invoiceTaxesTable tr").not("#invoiceTax_" + id),
            tax = _readTaxData(),
            index = 0;
        if (id > 0) {
            tax.id = id;
        }

        for (var i = 0, len = $listItems.length; i < len; i++) {
            if (jq($listItems[i]).find(".invoiceTaxName").text().trim() == tax.name) {
                index = i + 1;
                break;
            }
        }

        if (index == 0) {
            if (id > 0) {
                Teamlab.updateCrmInvoiceTax({}, tax.id, tax, {
                    before: function () {
                        jq("#invoiceTaxesActionMenu").hide();
                        jq("#invoiceTaxesTable .entity-menu.active").removeClass("active");
                        LoadingBanner.showLoaderBtn("#manageTax");
                    },
                    after: function () {
                        LoadingBanner.hideLoaderBtn("#manageTax");
                    },
                    success: ASC.CRM.InvoiceTaxesView.callback_edit_tax
                });
            } else {
                Teamlab.addCrmInvoiceTax({}, tax, {
                    before: function () {
                        LoadingBanner.showLoaderBtn("#manageTax");
                    },
                    after: function () {
                        LoadingBanner.hideLoaderBtn("#manageTax");
                    },
                    success: ASC.CRM.InvoiceTaxesView.callback_add_tax
                });
            }
        } else {
            ASC.CRM.Common.animateElement({
                element: jq($listItems[index - 1]),
                afterScrollFunc: jq.unblockUI()
            });
        }
    };

    var _deleteTax = function (id) {
        Teamlab.removeCrmInvoiceTax({}, id, {
            before: function () {
                jq("#invoiceTaxesActionMenu").hide();
                jq("#invoiceTaxesTable .entity-menu.active").removeClass("active");

            },
            success: ASC.CRM.InvoiceTaxesView.callback_delete_tax
        });
    };

    var _readTaxData = function() {
        var tax = {
            name: jq("#manageTax .taxName").val().trim(),
            description: jq("#manageTax textarea").val().trim(),
            rate: 0
        };

        var rate = Number(jq("#manageTax .taxRate").val().replace(ASC.CRM.Data.CurrencyDecimalSeparator, '.'));

        if (rate > 100)
            rate = 100;

        if (rate < -100)
            rate = -100;

        tax.rate = rate;

        return tax;
    };

    var _initManagePanel = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "manageTax",
            headerTest: ASC.CRM.Resources.CRMInvoiceResource.CreateInvoiceTax,
            questionText: "",
            innerHtmlText: jq.tmpl("taxSettingsActionPanelBodyTmpl", {}).html(),
            OKBtn: ASC.CRM.Resources.CRMCommonResource.Save,
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ""
        }).insertAfter("#invoiceTaxesList");

        jq("#manageTax .button.gray").on("click", function () { PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI(); });
    };

    return {
        invoiceTaxesList: new Array(),

        init: function () {
            _initEmptyScreen();

            _initManagePanel();

            _initInvoiceTaxesActionMenu();

            _initOtherActionMenu();

            ASC.CRM.InvoiceTaxesView.invoiceTaxesList = new Array();

            LoadingBanner.displayLoading();

            Teamlab.getCrmInvoiceTaxes({},
            {
                success: callback_get_invoice_taxes
            });
            
            jq.forceNumber({
                parent: "#manageTax",
                input: ".taxRate",
                integerOnly: false,
                positiveOnly: false,
                separator: ASC.CRM.Data.CurrencyDecimalSeparator,
                lengthAfterSeparator: 2
            });

            jq("#createNewTax").on("click", function () {
                _showAddOrUpdateTaxPanel();
            });
        },

        callback_add_tax: function (params, tax) {
            jq("#invoiceTaxesEmptyScreen:not(.display-none)").addClass("display-none");
            _invoiceTaxFactory(tax);
            ASC.CRM.InvoiceTaxesView.invoiceTaxesList.push(tax);

            jq("#invoiceTaxesList").show();
            jq("#invoiceTaxesTable tbody").append(jq.tmpl("invoiceTaxTmpl", tax));
            jq.unblockUI();
        },

        callback_edit_tax: function (params, tax) {
            jq("#invoiceTaxesEmptyScreen:not(.display-none)").addClass("display-none");
            _invoiceTaxFactory(tax);

            for (var i = 0, n = ASC.CRM.InvoiceTaxesView.invoiceTaxesList.length; i < n; i++) {
                if (ASC.CRM.InvoiceTaxesView.invoiceTaxesList[i].id == tax.id) {
                    ASC.CRM.InvoiceTaxesView.invoiceTaxesList[i] = tax;
                    break;
                }
            }

            jq("#invoiceTaxesList").show();
            jq("#invoiceTax_" + tax.id).replaceWith(jq.tmpl("invoiceTaxTmpl", tax));
            jq.unblockUI();
        },

        callback_delete_tax: function (params, tax) {
            for (var i = 0, n = ASC.CRM.InvoiceTaxesView.invoiceTaxesList.length; i < n; i++) {
                if (ASC.CRM.InvoiceTaxesView.invoiceTaxesList[i].id == tax.id) {
                    ASC.CRM.InvoiceTaxesView.invoiceTaxesList.splice(i, 1);
                    break;
                }
            }

            jq("#invoiceTax_" + tax.id).remove();
            
            if (jq("#invoiceTaxesTable tr").length == 0) {
                _showNoTaxesEmptyScreen();
            }

            jq.unblockUI();
        }
    };
})();



ASC.CRM.SettingsOrganisationProfileView = (function () {
    _clearAddress = function ($o) {
        $o.find(".address_category").val($o.find(".address_category").children(".default").val());
        $o.find(".contact_street").val("");
        $o.find(".contact_city").val("");
        $o.find(".contact_state").val("");
        $o.find(".contact_zip").val("");
        $o.find(".contact_country").val("");
    };
    return {
        init: function () {
            ASC.CRM.SettingsOrganisationProfileView.DefaultLogo = jq("#settings_organisation_profile .contact_photo").prop("src");

            jq("#menuCreateNewTask").on("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
            jq("#settings_organisation_profile .address_category").attr("readonly", "readonly").addClass('disabled').attr("disabled", "disabled");

            if (typeof (ASC.CRM.Data.InvoiceSetting.CompanyName) != "undefined") {
                jq("#settings_organisation_profile .settingsOrganisationProfileName").val(ASC.CRM.Data.InvoiceSetting.CompanyName);
            }

            if (typeof (ASC.CRM.Data.InvoiceSetting.CompanyLogoID) != "undefined" && ASC.CRM.Data.InvoiceSetting.CompanyLogoID != 0 &&
                typeof (ASC.CRM.Data.InvoiceSetting_logo_src) != "undefined" && ASC.CRM.Data.InvoiceSetting_logo_src != null && ASC.CRM.Data.InvoiceSetting_logo_src != "") {

                jq("#settings_organisation_profile .contact_photo").prop("src", ASC.CRM.Data.InvoiceSetting_logo_src);
                jq("#settings_organisation_profile .restore_default_logo").removeClass("disable");

            }

            var $addressContainer = jq("#settings_organisation_profile .address-tbl");

            if (ASC.CRM.Data.InvoiceSetting.CompanyAddress) {
                try {
                    var address = jq.parseJSON(ASC.CRM.Data.InvoiceSetting.CompanyAddress);
                    $addressContainer.find(".address_category").val(address.type);
                    $addressContainer.find(".contact_street").val(address.street);
                    $addressContainer.find(".contact_city").val(address.city);
                    $addressContainer.find(".contact_state").val(address.state);
                    $addressContainer.find(".contact_zip").val(address.zip);
                    $addressContainer.find(".contact_country").val(address.country);
                }
                catch (e) {
                    _clearAddress($addressContainer);
                    console.log(e);
                }
            } else {
                _clearAddress($addressContainer);
            }

            jq("#settings_organisation_profile .save_addresses").on("click", function () {
                if (jq("#settings_organisation_profile .save_addresses").hasClass("disable")) return;
                jq("#settings_organisation_profile .save_addresses").addClass("disable");

                var data = {
                    type: $addressContainer.find(".address_category").val(),
                    street: jq.trim($addressContainer.find(".contact_street").val()),
                    city: jq.trim($addressContainer.find(".contact_city").val()),
                    state: jq.trim($addressContainer.find(".contact_state").val()),
                    zip: jq.trim($addressContainer.find(".contact_zip").val()),
                    country: jq.trim($addressContainer.find(".contact_country").val())
                };

                Teamlab.updateOrganisationSettingsAddresses({}, data,
                    function () {
                        jq("<div></div>").addClass("okBox").text(ASC.CRM.Resources.CRMInvoiceResource.AddressesUpdated).insertAfter(".settingsHeaderAddress");
                        jq("#settings_organisation_profile .save_addresses").removeClass("disable");
                        setTimeout(function () {
                            jq("#settings_organisation_profile div.okBox").remove();
                        }, 3000);
                    });
            });

            jq("#settings_organisation_profile .save_base_info").on("click", function () {
                if (jq("#settings_organisation_profile .save_base_info").hasClass("disable")) return;
                jq("#settings_organisation_profile .save_base_info").addClass("disable");
                Teamlab.updateOrganisationSettingsCompanyName({}, jq.trim(jq("#settings_organisation_profile .settingsOrganisationProfileName").val()),
                    function () {
                        jq("<div></div>").addClass("okBox").text(ASC.CRM.Resources.CRMJSResource.SettingsUpdated).insertAfter(".settingsHeaderBase");
                        jq("#settings_organisation_profile .save_base_info").removeClass("disable");
                        setTimeout(function () {
                            jq("#settings_organisation_profile div.okBox").remove();
                        }, 3000);

                    });
            });

            new AjaxUpload('changeOrganisationLogo', {
                action: 'ajaxupload.ashx?type=ASC.Web.CRM.Classes.OrganisationLogoHandler,ASC.Web.CRM',
                autoSubmit: true,
                data: {},
                onSubmit: function () {},
                onChange: function (file, extension) {
                    if (jQuery.inArray("." + extension, ASC.Files.Utility.Resource.ExtsImage) == -1) {
                        jq("#settings_organisation_profile .fileUploadError").text(ASC.CRM.Resources.CRMJSResource.ErrorMessage_NotImageSupportFormat).show();
                        return false;
                    }
                    jq("#settings_organisation_profile .fileUploadError").hide();

                    jq("#settings_organisation_profile .under_logo .linkChangePhoto").addClass("disable");
                    LoadingBanner.displayLoading();
                    return true;
                },
                onComplete: function (file, response) {
                    var responseObj = jq.evalJSON(response);
                    if (!responseObj.Success) {
                        jq("#settings_organisation_profile .fileUploadError").text(responseObj.Message).show();
                        jq("#settings_organisation_profile .under_logo .linkChangePhoto").removeClass("disable");
                        jq("#settings_organisation_profile .save_logo").addClass("disable");
                        LoadingBanner.hideLoading();
                        return;
                    }
                    jq("#settings_organisation_profile .fileUploadDscr").show();
                    PopupKeyUpActionProvider.CloseDialog();
                    jq("#uploadOrganisationLogoPath").val(responseObj.Data);

                    var now = new Date();
                    jq("#settings_organisation_profile .contact_photo").attr("src", responseObj.Data + '?' + now.getTime());
                    jq("#settings_organisation_profile .under_logo .linkChangePhoto").removeClass("disable");
                    jq("#settings_organisation_profile .save_logo").removeClass("disable")
                    LoadingBanner.hideLoading();
                },
                parentDialog: null,
                isInPopup: false,
                name: "changeOrganisationLogo"
            });

            jq("#settings_organisation_profile .save_logo").on("click", function () {
                if (jq("#settings_organisation_profile .save_logo").hasClass("disable")) return;
                jq("#settings_organisation_profile .save_logo").addClass("disable");
                Teamlab.updateOrganisationSettingsLogo({}, { reset: false },
                    function (params, logo_id) {
                        jq("<div></div>").addClass("okBox").text(ASC.CRM.Resources.CRMJSResource.SettingsUpdated).insertAfter(".settingsHeaderLogo");
                        jq("#settings_organisation_profile .restore_default_logo").removeClass("disable");
                        setTimeout(function () {
                            jq("#settings_organisation_profile div.okBox").remove();
                        }, 3000);

                    });
            });

            jq("#settings_organisation_profile .restore_default_logo").on("click", function () {
                if (jq("#settings_organisation_profile .restore_default_logo").hasClass("disable")) return;
                jq("#settings_organisation_profile .restore_default_logo").addClass("disable");
                Teamlab.updateOrganisationSettingsLogo({}, { reset: true }, // jq.trim(jq("#uploadOrganisationLogoPath").val()) != "" : {},
                    function (params, logo_id) {
                        jq("#settings_organisation_profile .contact_photo").prop("src", ASC.CRM.SettingsOrganisationProfileView.DefaultLogo);
                        jq("<div></div>").addClass("okBox").text(ASC.CRM.Resources.CRMJSResource.SettingsUpdated).insertAfter(".settingsHeaderLogo");

                        setTimeout(function () {
                            jq("#settings_organisation_profile div.okBox").remove();
                        }, 3000);

                    });
            });

        }
    };
})();
