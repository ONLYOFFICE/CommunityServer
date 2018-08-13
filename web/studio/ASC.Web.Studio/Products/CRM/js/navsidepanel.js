/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
        pagesForHistory = ["default.aspx", "tasks.aspx", "deals.aspx", "invoices.aspx", "cases.aspx"],
        pagesForExportToCSV = ["default.aspx", "tasks.aspx", "deals.aspx", "cases.aspx"];

    var init = function () {
        if (isInit) {
            return;
        }

        isInit = true;
        entityID = jq.getURLParam("id");
        entityID = (entityID != null && !isNaN(parseInt(entityID))) ? parseInt(entityID) : 0;
        action = jq.getURLParam("action");

        var currentPage = getCurrentPage();

        if (currentPage == "default.aspx" && action != "manage" && entityID != 0) {
            ASC.CRM.TaskActionView.init(false);
        } else {
            ASC.CRM.TaskActionView.init(true);
        }

        if ((checkNeedExportCsvBtns(currentPage) || jq.inArray(currentPage, pagesForHistory) != -1) && entityID == 0 && action == null) {
            jq("#exportListToCSV").on("click", ASC.CRM.Common.exportCurrentListToCsv);
            jq("#openListInEditor").on("click", ASC.CRM.Common.openExportFile);
        } else {
            ASC.CRM.Common.hideExportButtons();
        }

        /***
        Bug Fix for Bug 23487
        ***/
        if (entityID == 0 && jq.inArray(currentPage, pagesForHistory) != -1 && checkInit("default.aspx", 0)) {
            initMenuItem({ id: "#nav-menu-contacts .companies-menu-item:first", href: "default.aspx", onclick: highlightMenu }, "default.aspx#" + companiesAnchor);
            initMenuItem({ id: "#nav-menu-contacts .persons-menu-item:first", href: "default.aspx", onclick: highlightMenu }, "default.aspx#" + personsAnchor);
        }
        /***
        Difficult part
        ***/
        jq("#nav-menu-contacts .companies-menu-item:first").on("click", function () {
            if (!checkInit("default.aspx", 0)) {
                location.href = "default.aspx#" + companiesAnchor;
            } else {
                var p = getCurrentPage();
                if (checkInit(p, 0) && jq.getURLParam("action") == null && jq.getURLParam("id") == null) {
                    ASC.Controls.AnchorController.move(companiesAnchor);
                    ASC.CRM.ListContactView.filterSortersCorrection();
                } else {
                    if (p != "default.aspx" || jq.getURLParam("action") != null || jq.getURLParam("id") != null) {
                        location.href = "default.aspx#" + companiesAnchor;
                    } else {
                        ASC.Controls.AnchorController.move(companiesAnchor);
                        ASC.CRM.ListContactView.filterSortersCorrection();
                    }
                }
            }
        });

        jq("#nav-menu-contacts .persons-menu-item:first").on("click", function () {
            if (!checkInit("default.aspx", 0)) {
                location.href = "default.aspx#" + personsAnchor;
            } else {
                var p = getCurrentPage();
                if (checkInit(p, 0) && jq.getURLParam("action") == null && jq.getURLParam("id") == null) {
                    ASC.Controls.AnchorController.move(personsAnchor);
                    ASC.CRM.ListContactView.filterSortersCorrection();
                } else {
                    if (p != "default.aspx" || jq.getURLParam("action") != null || jq.getURLParam("id") != null) {
                        location.href = "default.aspx#" + personsAnchor;
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
            { id: '#nav-menu-contacts .category-wrapper a', href: 'default.aspx', onclick: highlightMenu },
            { id: '#nav-menu-tasks a', href: 'tasks.aspx', onclick: highlightMenu },
            { id: '#nav-menu-deals a', href: 'deals.aspx', onclick: highlightMenu },
            { id: '#nav-menu-invoices .category-wrapper a', href: 'invoices.aspx', onclick: highlightMenu },
            { id: '#nav-menu-cases a', href: 'cases.aspx', onclick: highlightMenu }]);

        if (checkInit("invoices.aspx", 0)) {

            initMenuItem({ id: "#nav-menu-invoices .drafts-menu-item:first", href: "invoices.aspx", onclick: highlightMenu },
                "invoices.aspx#invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJakVpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCRWNtRm1kQ0FnSUNBZ0lDSXNJbDlmYVdRaU9qRXdNelV3TTMwPSJ9");
            initMenuItem({ id: "#nav-menu-invoices .sent-menu-item:first", href: "invoices.aspx", onclick: highlightMenu },
                "invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJaklpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCVFpXNTBJQ0FnSUNBZ0lpd2lYMTlwWkNJNk1UQXpOVEF6ZlE9PSJ9");
            initMenuItem({ id: "#nav-menu-invoices .paid-menu-item:first", href: "invoices.aspx", onclick: highlightMenu },
                "invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJalFpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCUVlXbGtJQ0FnSUNBZ0lpd2lYMTlwWkNJNk1UQXpOVEF6ZlE9PSJ9");
            initMenuItem({ id: "#nav-menu-invoices .rejected-menu-item:first", href: "invoices.aspx", onclick: highlightMenu },
                "invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJak1pTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCU1pXcGxZM1JsWkNBZ0lDQWdJQ0lzSWw5ZmFXUWlPakV3TXpVd00zMD0ifQ==");
        }
    };

    var getCurrentPage = function () {
        var parts = location.pathname.split('/'),
            currentPage = parts[parts.length - 1];
        if (currentPage == "") {
            currentPage = "default.aspx";
        }
        return currentPage;
    };

    var checkNeedExportCsvBtns = function (currentPage) {
        return jq.inArray(currentPage, pagesForExportToCSV) != -1 || currentPage == "settings.aspx" && jq.getURLParam("type") == "invoice_items";
    };

    var initCurrentPage = function (currentPage) {
        ASC.CRM.Common.hideExportButtons();

        if (currentPage == "default.aspx") {
            jq("#nav-menu-contacts").addClass("active currentCategory");

            ASC.CRM.ListContactView.init(".main-list-contacts");
        }

        if (currentPage == "tasks.aspx") {
            jq("#nav-menu-tasks").addClass("active currentCategory");
            ASC.CRM.ListTaskView.init("export_tasks_error", ".main-list-tasks");
        }

        if (currentPage == "deals.aspx") {
            jq("#nav-menu-deals").addClass("active currentCategory");
            ASC.CRM.ListDealView.init(".main-list-deals");
        }

        if (currentPage == "invoices.aspx") {
            jq("#nav-menu-invoices").addClass("active currentCategory");
            ASC.CRM.ListInvoiceView.init(".main-list-invoices");
        }

        if (currentPage == "cases.aspx") {
            jq("#nav-menu-cases").addClass("active currentCategory");
            ASC.CRM.ListCasesView.init(".main-list-cases");
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