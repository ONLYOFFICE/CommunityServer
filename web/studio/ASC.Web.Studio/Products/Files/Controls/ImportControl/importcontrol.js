/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
window.ASC.Files.Import = (function () {
    var importStatus = false;
    var isImport = false;
    var isInit = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.IsZohoAuthentificated, onIsZohoAuthentificated);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetImportData, onGetImportData);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.ExecImportData, ASC.Files.EventHandler.onGetTasksStatuses);
        }
    };

    var canStart = function () {
        if (ASC.Files.Import.importStatus) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoImprotNotFinish, true);
            return false;
        }
        return true;
    };

    var selectEventBySource = function (source) {
        switch ((source || "").toLowerCase()) {
            case "boxnet":
                return ASC.Files.Import.importBoxnet;
            case "google":
                return ASC.Files.Import.importGoogle;
            case "zoho":
                return ASC.Files.Import.importZoho;
            default:
                return new function () {
                };
        }
    };

    var importBoxnet = function () {
        ASC.Files.Actions.hideAllActionPanels();
        if (canStart()) {
            OAuthCallback = ASC.Files.Import.getImportData;
            OAuthPopup(ASC.Files.Constants.URL_OAUTH_BOXNET);
        }
    };

    var importGoogle = function () {
        ASC.Files.Actions.hideAllActionPanels();
        if (canStart()) {
            OAuthCallback = ASC.Files.Import.getImportData;
            OAuthPopup(ASC.Files.Constants.URL_OAUTH_GOOGLE);
        }
    };

    var importZoho = function () {
        ASC.Files.Actions.hideAllActionPanels();
        if (canStart()) {
            showSubmitLoginDialog();
        }
    };

    var setFolderImportTo = function (idTo, folderName) {
        jq("#importToFolderTitle").text(folderName);
        jq("#files_importToFolderId").val(idTo);

        ASC.Files.Actions.hideAllActionPanels();
    };

    var showSubmitLoginDialog = function () {
        jq("#filesImportPass").val("");
        jq("#filesImportLogin, #filesImportPass").removeAttr("disabled");
        LoadingBanner.hideLoaderBtn("#importLoginDialog");
        jq("#importLoginDialog div[id$=\"InfoPanel\"]").hide();

        ASC.Files.UI.blockUI(jq("#importLoginDialog"), 400, 500);
        PopupKeyUpActionProvider.EnterAction = "jq(\"#filesSubmitLoginDialog\").click();";
        PopupKeyUpActionProvider.CloseDialogAction = "jq(\"#filesImportPass\").val(\"\");";

        jq("#filesImportLogin").focus();
    };

    var submitLoginDialog = function () {
        var login = jq("#filesImportLogin").val().trim();
        var password = jq("#filesImportPass").val().trim();

        var infoBlock = jq("#importLoginDialog div[id$=\"InfoPanel\"]").hide();

        if (login == "" || password == "") {
            infoBlock.show().find("div").text(ASC.Files.FilesJSResources.ErrorMassage_FieldsIsEmpty);
            return;
        }

        jq("#filesImportPass").val("");

        jq("#filesImportLogin, #filesImportPass").attr("disabled", "disabled");
        LoadingBanner.showLoaderBtn("#importLoginDialog");

        ASC.Files.Import.getImportData("", "zoho", login, password);
    };

    var execImportData = function (params) {
        var checkedDocuments = jq("#importDialog input[name=\"checked_document\"]:checked");
        var infoBlock = jq("#importDialog div[id$=\"InfoPanel\"]");

        if (checkedDocuments.length == 0) {
            infoBlock
                .removeClass("infoPanel")
                .addClass("errorBox")
                .css("margin", "10px 16px 0")
                .show()
                .find("div").text(ASC.Files.FilesJSResources.EmptyListSelectedForImport);

            return;
        }

        var dataToSend = "<ArrayOfDataToImport>";
        Encoder.EncodeType = "!entity";
        checkedDocuments.each(function () {
            var cells = jq(this).closest("tr").find("td");
            dataToSend += "<DataToImport>";
            dataToSend += "<content_link>" + Encoder.htmlEncode(jq("<div/>").text(jq(this).val().trim()).html()) + "</content_link>";
            dataToSend += "<title>" + Encoder.htmlEncode(jq(cells[1]).text().trim()) + "</title>";
            dataToSend += "</DataToImport>";
        });
        Encoder.EncodeType = "entity";

        dataToSend += "</ArrayOfDataToImport>";

        params.tofolder = jq("#files_importToFolderId").val();
        params.ignoreCoincidenceFiles = jq("#importDialog input[name=\"file_conflict\"]:checked").val();

        jq("#importDialog input:checkbox").attr("disabled", "disabled");
        jq("#importDialog input[name=\"file_conflict\"]").attr("disabled", "disabled");
        jq("#importToFolderBtn").css("visibility", "hidden");

        infoBlock.hide();
        ASC.Files.Import.createImportProgress();

        PopupKeyUpActionProvider.CloseDialog();
        requestImportData(params, dataToSend);
    };

    var showImportToSelector = function () {
        ASC.Files.Import.isImport = true;

        ASC.Files.Tree.updateTreePath();
        ASC.Files.Tree.showSelect(jq("#files_importToFolderId").val());

        ASC.Files.Actions.hideAllActionPanels();

        jq("#treeViewPanelSelector").addClass("without-third-party");

        jq.dropdownToggle().toggle("#importToFolderBtn", "treeViewPanelSelector");
        jq("body").bind("click", ASC.Files.Actions.registerHideTree);
    };

    var cancelImportData = function (text, isError) {
        ASC.Files.Import.importStatus = false;
        var idTo = jq("#files_importToFolderId").val();
        ASC.Files.Tree.resetFolder(idTo);

        if (jq("#importDialog:visible").length != 0) {
            PopupKeyUpActionProvider.CloseDialog();
        }
        if (ASC.Files.Folders.currentFolder.id == idTo) {
            ASC.Files.Anchor.navigationSet(idTo);
        }

        ASC.Files.UI.displayInfoPanel(text, isError === true);
    };

    var createImportProgress = function () {
        if (jq("#importDataProcess").length != 0) {
            return;
        }
        jq("#progressTemplate").clone().attr("id", "importDataProcess").prependTo("#bottomLoaderPanel");
        jq("#importDataProcess .progress-dialog-header").append("<a title=\"{0}\" class=\"actions-container close\"></a>".format(ASC.Files.FilesJSResources.ButtonCancelImport));
        jq("#importDataProcess .progress-dialog-header").append("<span>{0}</span>".format(ASC.Files.FilesJSResources.ImportProgress));
        ASC.Files.UI.setProgressValue("#importDataProcess", 0);
        jq("#importDataProcess .asc-progress-percent").text("0%");
        jq("#importDataProcess").show();
    };

    //request

    var getImportData = function (token, source, login, password) {
        var authData =
            {
                login: login,
                password: password,
                token: token
            };

        var params =
            {
                source: source,
                showLoading: (source != "zoho"),
                authData: authData
            };

        ASC.Files.ServiceManager.getImportData(
            (source != "zoho"
                ? ASC.Files.ServiceManager.events.GetImportData
                : ASC.Files.ServiceManager.events.IsZohoAuthentificated),
            params,
            { AuthData: authData });
    };

    var requestImportData = function (params, data) {
        params.showLoading = true;

        ASC.Files.ServiceManager.execImportData(ASC.Files.ServiceManager.events.ExecImportData, params, data);
    };

    //event handler
    var onIsZohoAuthentificated = function (xmlData, params, errorMessage) {
        var infoBlock = jq("#importLoginDialog div[id$=\"InfoPanel\"]").hide();

        jq("#filesImportPass").val("");

        if (typeof errorMessage != "undefined") {
            infoBlock.show().find("div").text(errorMessage);
            jq("#filesImportLogin, #filesImportPass").removeAttr("disabled");
            LoadingBanner.hideLoaderBtn("#importLoginDialog");
            return;
        }

        infoBlock.show().find("div").text(ASC.Files.FilesJSResources.ErrorMassage_AuthentificatedFalse);

        jq("#filesImportLogin, #filesImportPass").removeAttr("disabled");
        LoadingBanner.hideLoaderBtn("#importLoginDialog");

        onGetImportData(xmlData, params, errorMessage);
    };

    var onGetImportData = function (xmlData, params, errorMessage) {
        PopupKeyUpActionProvider.CloseDialog();

        if (typeof errorMessage != "undefined" || xmlData == null) {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var elemments = xmlData.getElementsByTagName("DataToImportList");
        if (elemments.length == 0 || elemments[0].childElementCount == 0) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.EmptyDataToImport, true);
            return;
        }

        var htmlXML = ASC.Files.TemplateManager.translate(xmlData);

        jq("#importData").html(htmlXML);

        jq("#startImportData").unbind("click").click(function () {
            ASC.Files.Import.execImportData(params);
            return false;
        });

        var importToFolderId = ASC.Files.Constants.FOLDER_ID_MY_FILES;
        var toFolderTitle = ASC.Files.Tree.getFolderTitle(importToFolderId);

        if (ASC.Files.UI.accessibleItem()
            && (!ASC.Files.ThirdParty || !ASC.Files.ThirdParty.isThirdParty())) {
            importToFolderId = ASC.Files.Folders.currentFolder.id;
            toFolderTitle = ASC.Files.Tree.getPathTitle(importToFolderId);
        }
        ASC.Files.Import.setFolderImportTo(importToFolderId, toFolderTitle);

        var resourceImport;
        switch (params.source) {
            case "boxnet":
                resourceImport = ASC.Files.FilesJSResources.ImportFromBoxNet;
                break;
            case "google":
                resourceImport = ASC.Files.FilesJSResources.ImportFromGoogle;
                break;
            case "zoho":
                resourceImport = ASC.Files.FilesJSResources.ImportFromZoho;
                break;
            default:
                PopupKeyUpActionProvider.CloseDialog();
        }
        jq("#importDialogHeader, #importToFolderName").text(resourceImport);

        jq("#importDialog input:checkbox").removeAttr("disabled");
        jq("#importDialog input[name=\"file_conflict\"]").removeAttr("disabled");
        jq("#importToFolderBtn").css("visibility", "visible");

        jq("#importDialog div[id$=\"InfoPanel\"]").hide();

        PopupKeyUpActionProvider.EnterAction = "jq(\"#startImportData\").click();";

        var dataTable = jq("#importData table.sortable")[0];
        sorttable.makeSortable(dataTable);

        ASC.Files.UI.blockUI(jq("#importDialog"), 900, 540);
    };

    var onGetImportStatus = function (data, isTerminate) {
        ASC.Files.Import.importStatus = true;

        try {
            var progress = parseInt(data.progress) || 0;
        } catch (e) {
            progress = 0;
        }

        if (jq("#importDialogHeader").text().length == 0) {
            var resourceImport = "";
            switch (data.source) {
                case "boxnet":
                    resourceImport = ASC.Files.FilesJSResources.ImportFromBoxNet;
                    break;
                case "google":
                    resourceImport = ASC.Files.FilesJSResources.ImportFromGoogle;
                    break;
                case "zoho":
                    resourceImport = ASC.Files.FilesJSResources.ImportFromZoho;
                    break;
                default:
                    PopupKeyUpActionProvider.CloseDialog();
            }
            jq("#importDialogHeader").text(resourceImport);
        }

        if (progress > 0) {
            ASC.Files.UI.setProgressValue("#importDataProcess", progress);
            jq("#importDataProcess .asc-progress-percent").text(progress + "%");
        }

        if (progress == 100) {
            ASC.Files.Import.cancelImportData(data.error || (isTerminate === true ? ASC.Files.FilesJSResources.InfoCancelImport : ASC.Files.FilesJSResources.InfoFinishImport), data.error != null);
            jq("#importDataProcess").hide();
            return true;
        }

        return false;
    };

    return {
        init: init,

        selectEventBySource: selectEventBySource,

        importBoxnet: importBoxnet,
        importGoogle: importGoogle,
        importZoho: importZoho,

        submitLoginDialog: submitLoginDialog,
        getImportData: getImportData,
        execImportData: execImportData,
        cancelImportData: cancelImportData,
        showImportToSelector: showImportToSelector,
        createImportProgress: createImportProgress,

        setFolderImportTo: setFolderImportTo,
        isImport: isImport,
        importStatus: importStatus,
        onGetImportStatus: onGetImportStatus
    };
})();

(function ($) {
    ASC.Files.Import.init();

    $(function () {
        jq("#importLoginDialog div[id$=\"InfoPanel\"]")
            .removeClass("infoPanel")
            .addClass("errorBox")
            .css("margin", "10px 16px 0");

        jq("#importFromBoxNet a, #importFromGoogle a, #importFromZoho a").click(function () {
            var source = jq(this).parent().attr("id").replace("importFrom", "").toLowerCase();
            var importEvent = ASC.Files.Import.selectEventBySource(source);
            importEvent();
            return false;
        });

        jq("#filesSubmitLoginDialog").click(function () {
            ASC.Files.Import.submitLoginDialog();
            return false;
        });

        jq("#importToFolderBtn").click(function () {
            ASC.Files.Import.showImportToSelector();
            return false;
        });

        jq("#bottomLoaderPanel").on("click", "#importDataProcess a.close", function (event) {
            jq("#importDataProcess").hide();
            ASC.Files.Folders.terminateTasks(true);
            ASC.Files.Common.cancelBubble(event);
            return false;
        });

        jq("#importDialog").on("click", "input[name=\"all_checked_document\"]", function () {
            var value = jq(this).prop("checked");
            jq("#importDialog input[name=\"checked_document\"]").prop("checked", value);
        });

        jq("#importDialog").on("click", "input[name=\"checked_document\"]", function () {
            var value = jq("#importDialog input[name=\"checked_document\"]:not(:checked)").length;
            
            jq("#importDialog input[name=\"all_checked_document\"]")
                .prop("checked", value == 0)
                .prop("indeterminate", value != 0 && value != jq("#importDialog input[name=\"checked_document\"]").length);
        });
    });
})(jQuery);