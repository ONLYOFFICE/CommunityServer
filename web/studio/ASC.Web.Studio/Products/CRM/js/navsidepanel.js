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

ASC.CRM.NavSidePanel = (function() {
    var isInit = false,
        entityID = 0,
        action = null,
        companiesAnchor = "eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltTnlaV0YwWldRaUxDSmtaV1lpT25SeWRXVXNJbVJ6WXlJNmRISjFaU3dpYzI5eWRFOXlaR1Z5SWpvaVpHVnpZMlZ1WkdsdVp5SjkifTt7ImlkIjoicGVyc29uIiwidHlwZSI6ImNvbWJvYm94IiwicGFyYW1zIjoiZXlKMllXeDFaU0k2SW1OdmJYQmhibmtpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCRGIyMXdZVzVwWlhNZ0lDQWdJQ0FpTENKZlgybGtJam80TnpJd01UUjkifQ==",
        personsAnchor = "eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltTnlaV0YwWldRaUxDSmtaV1lpT25SeWRXVXNJbVJ6WXlJNmRISjFaU3dpYzI5eWRFOXlaR1Z5SWpvaVpHVnpZMlZ1WkdsdVp5SjkifTt7ImlkIjoicGVyc29uIiwidHlwZSI6ImNvbWJvYm94IiwicGFyYW1zIjoiZXlKMllXeDFaU0k2SW5CbGNuTnZiaUlzSW5ScGRHeGxJam9pSUNBZ0lDQWdJQ0FnSUZCbGNuTnZibk1nSUNBZ0lDQWlMQ0pmWDJsa0lqbzROekl3TVRSOSJ9",
        pagesForHistory = ["Default.aspx", "Tasks.aspx", "Deals.aspx", "Invoices.aspx", "Cases.aspx"],
        pagesForExportToCSV = ["Default.aspx", "Tasks.aspx", "Deals.aspx", "Cases.aspx"];

    var init = function () {
        if (isInit) {
            return;
        }

        isInit = true;
        entityID = jq.getURLParam("id");
        entityID = (entityID != null && !isNaN(parseInt(entityID))) ? parseInt(entityID) : 0;
        action = jq.getURLParam("action");

        var currentPage = getCurrentPage();

        if (currentPage == "Default.aspx" && action != "manage" && entityID != 0) {
            ASC.CRM.TaskActionView.init(false);
        } else {
            ASC.CRM.TaskActionView.init(true);
        }

        if ((checkNeedExportCsvBtns(currentPage) || jq.inArray(currentPage, pagesForHistory) != -1) && entityID == 0 && action == null) {
            jq("#exportListToCSV").on("click", ASC.CRM.Common.exportCurrentListToCsv);
        } else {
            ASC.CRM.Common.hideExportButtons();
        }

        /***
        Bug Fix for Bug 23487
        ***/
        if (entityID == 0 && jq.inArray(currentPage, pagesForHistory) != -1 && checkInit("Default.aspx", 0)) {
            initMenuItem({ id: "#nav-menu-contacts .companies-menu-item:first", href: "Default.aspx", onclick: highlightMenu }, "Default.aspx#" + companiesAnchor);
            initMenuItem({ id: "#nav-menu-contacts .persons-menu-item:first", href: "Default.aspx", onclick: highlightMenu }, "Default.aspx#" + personsAnchor);
        }
        /***
        Difficult part
        ***/
        jq("#nav-menu-contacts .companies-menu-item:first").on("click", function () {
            if (!checkInit("Default.aspx", 0)) {
                location.href = "Default.aspx#" + companiesAnchor;
            } else {
                var p = getCurrentPage();
                if (checkInit(p, 0) && jq.getURLParam("action") == null && jq.getURLParam("id") == null) {
                    ASC.Controls.AnchorController.move(companiesAnchor);
                    ASC.CRM.ListContactView.filterSortersCorrection();
                } else {
                    if (p != "Default.aspx" || jq.getURLParam("action") != null || jq.getURLParam("id") != null) {
                        location.href = "Default.aspx#" + companiesAnchor;
                    } else {
                        ASC.Controls.AnchorController.move(companiesAnchor);
                        ASC.CRM.ListContactView.filterSortersCorrection();
                    }
                }
            }
        });

        jq("#nav-menu-contacts .persons-menu-item:first").on("click", function () {
            if (!checkInit("Default.aspx", 0)) {
                location.href = "Default.aspx#" + personsAnchor;
            } else {
                var p = getCurrentPage();
                if (checkInit(p, 0) && jq.getURLParam("action") == null && jq.getURLParam("id") == null) {
                    ASC.Controls.AnchorController.move(personsAnchor);
                    ASC.CRM.ListContactView.filterSortersCorrection();
                } else {
                    if (p != "Default.aspx" || jq.getURLParam("action") != null || jq.getURLParam("id") != null) {
                        location.href = "Default.aspx#" + personsAnchor;
                    } else {
                        ASC.Controls.AnchorController.move(personsAnchor);
                        ASC.CRM.ListContactView.filterSortersCorrection();
                    }
                }
            }
        });
        /*******/

        if (entityID != 0 || jq.inArray(currentPage, pagesForHistory) == -1) { return; }

        highlightMenu();
        initMenuItems([
            { id: '#nav-menu-contacts .category-wrapper a', href: 'Default.aspx', onclick: highlightMenu },
            { id: '#nav-menu-tasks a', href: 'Tasks.aspx', onclick: highlightMenu },
            { id: '#nav-menu-deals a', href: 'Deals.aspx', onclick: highlightMenu },
            { id: '#nav-menu-invoices .category-wrapper a', href: 'Invoices.aspx', onclick: highlightMenu },
            { id: '#nav-menu-cases a', href: 'Cases.aspx', onclick: highlightMenu }]);

        if (checkInit("Invoices.aspx", 0)) {

            initMenuItem({ id: "#nav-menu-invoices .drafts-menu-item:first", href: "Invoices.aspx", onclick: highlightMenu },
                "Invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJakVpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCRWNtRm1kQ0FnSUNBZ0lDSXNJbDlmYVdRaU9qRXdNelV3TTMwPSJ9");
            initMenuItem({ id: "#nav-menu-invoices .sent-menu-item:first", href: "Invoices.aspx", onclick: highlightMenu },
                "Invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJaklpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCVFpXNTBJQ0FnSUNBZ0lpd2lYMTlwWkNJNk1UQXpOVEF6ZlE9PSJ9");
            initMenuItem({ id: "#nav-menu-invoices .paid-menu-item:first", href: "Invoices.aspx", onclick: highlightMenu },
                "Invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJalFpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCUVlXbGtJQ0FnSUNBZ0lpd2lYMTlwWkNJNk1UQXpOVEF6ZlE9PSJ9");
            initMenuItem({ id: "#nav-menu-invoices .rejected-menu-item:first", href: "Invoices.aspx", onclick: highlightMenu },
                "Invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJak1pTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCU1pXcGxZM1JsWkNBZ0lDQWdJQ0lzSWw5ZmFXUWlPakV3TXpVd00zMD0ifQ==");
        }
    };

    var getCurrentPage = function () {
        var parts = location.pathname.split('/'),
            currentPage = parts[parts.length - 1];
        if (currentPage == "") {
            currentPage = "Default.aspx";
        }
        return currentPage;
    };

    var checkNeedExportCsvBtns = function (currentPage) {
        return jq.inArray(currentPage, pagesForExportToCSV) != -1 || currentPage == "Settings.aspx" && jq.getURLParam("type") == "invoice_items";
    };

    var initCurrentPage = function (currentPage) {
        ASC.CRM.Common.hideExportButtons();

        if (currentPage == "Default.aspx") {
            jq("#nav-menu-contacts").addClass("active currentCategory");
            ASC.CRM.ListContactView.init(".main-list-contacts", ".filter-content", ".paging-content");
        }

        if (currentPage == "Tasks.aspx") {
            jq("#nav-menu-tasks").addClass("active currentCategory");
            ASC.CRM.ListTaskView.init(".main-list-tasks", ".filter-content", ".paging-content");
        }

        if (currentPage == "Deals.aspx") {
            jq("#nav-menu-deals").addClass("active currentCategory");
            ASC.CRM.ListDealView.init(".main-list-deals", ".filter-content", ".paging-content");
        }

        if (currentPage == "Invoices.aspx") {
            jq("#nav-menu-invoices").addClass("active currentCategory");
            ASC.CRM.ListInvoiceView.init(".main-list-invoices", ".filter-content", ".paging-content");
        }

        if (currentPage == "Cases.aspx") {
            jq("#nav-menu-cases").addClass("active currentCategory");
            ASC.CRM.ListCasesView.init(".main-list-cases", ".filter-content", ".paging-content");
        }

    };

    var highlightMenu = function () {
        jq(".menu-list li").removeClass("active currentCategory");
        var currentPage = getCurrentPage();
        
        initCurrentPage(currentPage);
    };

    var initMenuItems = function (items) {
        var currentPage = getCurrentPage(),
            entityId = jq.getURLParam('id');
        if (checkInit(currentPage, entityId)) {
            items.forEach(function (item) {
                initMenuItem(item);
            });
        }
    };

    var checkInit = function (currentPage, entityId) {
        return !!(window.history && window.history.replaceState) &&
            !entityId && !jq.getURLParam('action') &&
            (jq.inArray(currentPage, pagesForHistory) != -1);
    };

    var initMenuItem = function (item, extHref) {
        jq(item.id).on('click', function (event) {
            if (event.which == 1 && event.ctrlKey || event.which == 2) return true;
            event.stopPropagation();
            jq('#privatePanelWrapper').appendTo('#hiddenBlockForPrivatePanel');
            var activeList = jq(".main-list:not(.display-none)");
            activeList.addClass("display-none");
            activeList.html('');
            
            jq(".filter-content, .paging-content").html('');

            var historyHref = item.href;
            if (typeof (extHref) != "undefined" && extHref != null && extHref != "") {
                historyHref = extHref;
            }
            history.pushState({ href: historyHref }, { href: historyHref },  historyHref);
            item.onclick();
            return false;
        });
    };

    return {
        init: init,
        initMenuItems: initMenuItems
    };
})(jQuery);