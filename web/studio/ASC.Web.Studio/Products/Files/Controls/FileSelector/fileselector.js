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

window.ASC.Files.FileSelector = (function () {
    var isInit = false;
    var fileSelectorTree = {};
    var isFolderSelector = false;
    var filesFilter = ASC.Files.Constants.FilterType.None;

    var onInit = function () {
    };
    var onSubmit = function () {
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
                    ASC.Files.FileSelector.onSubmit(ASC.Files.FileSelector.fileSelectorTree.selectedFolderId);
                } else {
                    var files = jq("#filesMainContent .file-row:not(.folder-row):has(.checkbox input:checked)").map(function () {
                        return ASC.Files.UI.getObjectData(this);
                    });
                    ASC.Files.FileSelector.onSubmit(files);
                }

                PopupKeyUpActionProvider.CloseDialog();
            });

            jq("#pageNavigatorHolder a").click(function () {
                selectFolder(ASC.Files.FileSelector.fileSelectorTree.selectedFolderId, true);
            });

            jq("#filesMainContent").on("click", ".folder-row:not(.error-entry) .entry-title .name a, .folder-row:not(.error-entry) .thumb-folder", function () {
                var folderId = ASC.Files.UI.getObjectData(this).id;
                if (folderId != 0) {
                    selectFolder(folderId, false, true);
                }
                return false;
            });
            
            jq("#filesMainContent").on("click", ".file-row:not(.folder-row):not(.error-entry) .entry-title .name a, .file-row:not(.folder-row):not(.error-entry) .thumb-file", function (event) {
                ASC.Files.UI.clickRow(event, jq(this).closest(".file-row"));
                return false;
            });
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
        jq("#submitFileSelector").addClass("disable");
        var filterSettings =
            {
                sorter: {
                    is_asc: false,
                    property: "DateAndTime"
                },
                text: "",
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
                subject: filterSettings.subject,
                text: filterSettings.text,
                orderBy: filterSettings.sorter,
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
        isFolderSelector = jq("#mainContent").length && onlyFolder;

        ASC.Files.FileSelector.fileSelectorTree.clickOnFolder = isFolderSelector ? checkFolder : selectFolder;

        jq("#fileSelectorDialog").toggleClass("only-folder", isFolderSelector);

        ASC.Files.UI.blockUI("#fileSelectorDialog", isFolderSelector ? 440 : 1030, 650);

        PopupKeyUpActionProvider.EnterAction = "jq(\"#submitFileSelector\").click();";

        if (typeof thirdParty != "undefined") {
            jq("#fileSelectorTree>ul>li:not(.third-party-entry)").toggle(!thirdParty);

            ASC.Files.FileSelector.toggleThirdParty(!thirdParty);
        }

        ASC.Files.FileSelector.fileSelectorTree.rollUp();

        folderId = folderId || ASC.Files.FileSelector.fileSelectorTree.getDefaultFolderId();
        ASC.Files.FileSelector.fileSelectorTree.setCurrent(folderId);

        if (!isFolderSelector) {
            ASC.Files.UI.filesSelectedHandler = selectFile;

            selectFolder(folderId);
        }

        jq("#submitFileSelector").toggleClass("disable",
            !isFolderSelector || !ASC.Files.Common.isCorrectId(ASC.Files.FileSelector.fileSelectorTree.selectedFolderId));
    };

    var setTitle = function (newTitle) {
        jq("#fileSelectorTitle").text((newTitle || "").trim());
    };

    var createThirdPartyTree = function () {
        ASC.Files.ServiceManager.getThirdParty(ASC.Files.ServiceManager.events.GetThirdPartyTree, {folderType: 1});
    };

    var onGetThirdParty = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        if (jsonData.length > 0) {
            var stringXml = ASC.Files.Common.jsonToXml({ folderList: { entry: jsonData } });

            var htmlXml = ASC.Files.TemplateManager.translateFromString(stringXml);
            jq("#fileSelectorTree>ul").prepend(htmlXml);

            ASC.Files.FileSelector.onThirdPartyTreeCreated();
        }
    };

    var onGetFolderItemsTree = function (xmlData, params, errorMessage) {
        ASC.Files.EventHandler.onGetFolderItems(xmlData, params, errorMessage);

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

        fileSelectorTree: fileSelectorTree,
        filesFilter: filesFilter,

        onInit: onInit,
        onSubmit: onSubmit,
        onThirdPartyTreeCreated: onThirdPartyTreeCreated,
    };
})();

(function ($) {
    $(function () {
        ASC.Files.FileSelector.init();
    });
})(jQuery);