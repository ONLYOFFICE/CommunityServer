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


window.ASC.Files.FileSelector = (function () {
    var isInit = false;
    var fileSelectorTree = {};
    var isFolderSelector = false;
    var filesFilter = ASC.Files.Constants.FilterType.None;
    var filesFilterText = "";

    var onInit = function () {
    };
    var onSubmit = function () {
    };
    var onCancel = function () {
        PopupKeyUpActionProvider.CloseDialog();
        return false;
    };
    var onThirdPartyTreeCreated = function () {
    };

    var init = function () {
        if (!isInit) {
            isInit = true;
            if (!ASC.Files.Folders) {
                ASC.Files.Folders = {};
            }
            ASC.Files.UI.canSetDocumentTitle = false;

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetThirdPartyTree, onGetThirdParty);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetFolderItemsTree, onGetFolderItemsTree);

            ASC.Files.FileSelector.fileSelectorTree = new ASC.Files.TreePrototype("#fileSelectorTree");

            jq("#filesMainContent").addClass("compact");

            jq("#fileSelectorDialog").on("click", "#submitFileSelector:not(.disable)", function () {
                if (isFolderSelector) {
                    var result = ASC.Files.FileSelector.onSubmit(ASC.Files.FileSelector.fileSelectorTree.selectedFolderId);
                } else {
                    var files = jq("#filesMainContent .file-row:not(.folder-row):has(.checkbox input:checked)").map(function () {
                        return ASC.Files.UI.getObjectData(this);
                    });
                    result = ASC.Files.FileSelector.onSubmit(files);
                }

                if (result !== false) {
                    PopupKeyUpActionProvider.CloseDialog();
                }
            });

            jq("#pageNavigatorHolder a").click(function () {
                selectFolder(ASC.Files.FileSelector.fileSelectorTree.selectedFolderId, true);
            });

            jq("#mainContent").scroll(function () {
                if (jq("#filesMainContent").height() - jq("#fileSelectorDialog").height() <= jq("#mainContent").scrollTop()) {
                    selectFolder(ASC.Files.FileSelector.fileSelectorTree.selectedFolderId, true);
                }

                return true;
            });

            jq("#filesMainContent").on("click", ".folder-row:not(.error-entry) .entry-title .name a, .folder-row:not(.error-entry) .thumb-folder", function () {
                var folderId = ASC.Files.UI.getObjectData(this).id;
                selectFolder(folderId, false, true);
                return false;
            });
            
            jq("#filesMainContent").on("click", ".file-row:not(.folder-row):not(.error-entry) .entry-title .name a, .file-row:not(.folder-row):not(.error-entry) .thumb-file", function (event) {
                ASC.Files.UI.clickRow(event, jq(this).closest(".file-row"));
                return false;
            });

            jq("#closeFileSelector").click(function () { ASC.Files.FileSelector.onCancel(); });
            
            if (window.location.href.indexOf("compact") != -1) {
                jq("body").addClass("file-selector-compact");
            }
        }
        ASC.Files.FileSelector.onInit();
    };

    var checkFolder = function (folderId) {
        jq("#submitFileSelector").addClass("disable");

        var folderData = ASC.Files.FileSelector.fileSelectorTree.getFolderData(folderId);
        if (ASC.Files.UI.accessEdit(folderData) && ASC.Files.Common.isCorrectId(folderId)) {
            jq("#submitFileSelector").removeClass("disable");
            return true;
        } else {
            ASC.Files.FileSelector.fileSelectorTree.expandFolder(folderId);
            return false;
        }
    };

    var selectFolder = function (folderId, isAppend, expandTree) {
        if ((folderId || 0) == 0) {
            return;
        }

        if (isAppend
            && (jq("#pageNavigatorHolder:visible").length == 0
                || jq("#pageNavigatorHolder a").text() == ASC.Files.FilesJSResources.ButtonShowMoreLoad)) {
            return;
        }

        jq("#pageNavigatorHolder a").text(ASC.Files.FilesJSResources.ButtonShowMoreLoad);

        jq("#submitFileSelector").addClass("disable");
        var filterSettings =
            {
                sorter: {
                    is_asc: false,
                    property: "DateAndTime"
                },
                text: ASC.Files.FileSelector.filesFilterText,
                filter: ASC.Files.FileSelector.filesFilter,
                subject: ""
            };

        ASC.Files.ServiceManager.getFolderItems(ASC.Files.ServiceManager.events.GetFolderItemsTree,
            {
                folderId: folderId,
                from: (isAppend ? jq("#filesMainContent .file-row[name!=\"addRow\"]").length : 0),
                count: ASC.Files.Constants.COUNT_ON_PAGE,
                append: isAppend === true,
                filter: filterSettings.filter,
                subjectGroup: false,
                subjectId: filterSettings.subject,
                search: filterSettings.text,
                orderBy: filterSettings.sorter,
                searchInContent: false,
                withSubfolders: false,
                currentFolderId: ASC.Files.Folders && ASC.Files.Folders.currentFolder ? ASC.Files.Folders.currentFolder.id : null,
                expandTree: expandTree
            }, { orderBy: filterSettings.sorter });
    };

    var selectFile = function () {
        jq("#submitFileSelector").toggleClass("disable", !jq("#filesMainContent .file-row:not(.folder-row):has(.checkbox input:checked)").length);
    };

    var toggleThirdParty = function (hide) {
        jq("#fileSelectorDialog").toggleClass("hide-thirdparty", hide);
    };

    var openDialog = function (folderId, onlyFolder, thirdParty) {
        isFolderSelector = !jq("#mainContent").length || onlyFolder;

        ASC.Files.FileSelector.fileSelectorTree.clickOnFolder = isFolderSelector ? checkFolder : selectFolder;

        jq("#fileSelectorDialog").toggleClass("only-folder", isFolderSelector);

        if (jq("#fileSelectorDialog").hasClass("popup-modal")) {
            ASC.Files.UI.blockUI("#fileSelectorDialog", isFolderSelector ? 440 : 1030, 650);
        }

        PopupKeyUpActionProvider.EnterAction = "jq(\"#submitFileSelector\").click();";

        if (typeof thirdParty != "undefined") {
            jq("#fileSelectorTree>ul>li:not(.third-party-entry)").toggle(!thirdParty);

            ASC.Files.FileSelector.toggleThirdParty(!thirdParty);
        }

        ASC.Files.FileSelector.fileSelectorTree.rollUp();

        folderId = folderId || ASC.Files.FileSelector.fileSelectorTree.getDefaultFolderId();
        ASC.Files.FileSelector.fileSelectorTree.setCurrent(folderId);

        ASC.Files.FileSelector.fileSelectorTree.clickOnFolder(folderId);

        if (!isFolderSelector) {
            ASC.Files.UI.filesSelectedHandler = selectFile;

            jq("#submitFileSelector").toggleClass("disable",
                !isFolderSelector || !ASC.Files.Common.isCorrectId(ASC.Files.FileSelector.fileSelectorTree.selectedFolderId));
        }
    };

    var setTitle = function (newTitle) {
        jq("#fileSelectorTitle").text((newTitle || "").trim());
    };

    var showThirdPartyOnly = function (providerKey) {
        jq("#fileSelectorTree>ul>li.third-party-entry").each(function(i, treeNode) {
            var entryData = ASC.Files.UI.getObjectData(treeNode);
            if (entryData.provider_key != providerKey) {
                jq(treeNode).hide();
            }
        });
    };

    var createThirdPartyTree = function (callback) {
        ASC.Files.ServiceManager.getThirdParty(ASC.Files.ServiceManager.events.GetThirdPartyTree, {folderType: 1, callback: callback});
    };

    var onGetThirdParty = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        if (jsonData && jsonData.length > 0) {
            var stringXml = ASC.Files.Common.jsonToXml({ folderList: { entry: jsonData } });

            var htmlXml = ASC.Files.TemplateManager.translateFromString(stringXml);
            jq("#fileSelectorTree>ul").prepend(htmlXml);

            ASC.Files.FileSelector.onThirdPartyTreeCreated();
        }

        if (params.callback) {
            params.callback();
        }
    };

    var onGetFolderItemsTree = function (xmlData, params, errorMessage) {
        ASC.Files.EventHandler.onGetFolderItems(xmlData, params, errorMessage);

        if (typeof errorMessage != "undefined" || typeof xmlData == "undefined") {
            return;
        }

        if (params.expandTree) {
            ASC.Files.FileSelector.fileSelectorTree.expandFolder(params.currentFolderId, true, true);
        }
        ASC.Files.FileSelector.fileSelectorTree.setCurrent(ASC.Files.Folders.currentFolder.id);
    };

    return {
        init: init,

        setTitle: setTitle,
        openDialog: openDialog,

        toggleThirdParty: toggleThirdParty,
        createThirdPartyTree: createThirdPartyTree,
        showThirdPartyOnly: showThirdPartyOnly,

        fileSelectorTree: fileSelectorTree,
        filesFilter: filesFilter,
        filesFilterText: filesFilterText,

        onInit: onInit,
        onSubmit: onSubmit,
        onCancel: onCancel,
        onThirdPartyTreeCreated: onThirdPartyTreeCreated,
    };
})();

(function ($) {
    $(function () {
        if (jq("#fileSelectorDialog").length == 0)
            return;

        ASC.Files.FileSelector.init();
    });
})(jQuery);