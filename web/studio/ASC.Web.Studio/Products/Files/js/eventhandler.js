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


window.ASC.Files.EventHandler = (function () {
    var isInit = false;
    var timoutTasksStatuses = null;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetFolderItems, ASC.Files.EventHandler.onGetFolderItems);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetItems, ASC.Files.EventHandler.onGetItems);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.CheckEditing, ASC.Files.EventHandler.onCheckEditing);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.LockFile, ASC.Files.EventHandler.onLockFile);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.UpdateIfExist, ASC.Files.EventHandler.onUpdateIfExist);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetHelpCenter, ASC.Files.EventHandler.onGetHelpCenter);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.CreateFolder, ASC.Files.EventHandler.onCreateFolder);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.CreateNewFile, ASC.Files.EventHandler.onCreateNewFile);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.FolderRename, ASC.Files.EventHandler.onRenameFolder);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.FileRename, ASC.Files.EventHandler.onRenameFile);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.ChangeOwner, ASC.Files.EventHandler.onChangeOwner);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetFileHistory, ASC.Files.EventHandler.onGetFileHistory);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SetCurrentVersion, ASC.Files.EventHandler.onUpdateHistory);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.CompleteVersion, ASC.Files.EventHandler.onUpdateHistory);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.ReplaceVersion, ASC.Files.EventHandler.onGetFile);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.UpdateComment, ASC.Files.EventHandler.onUpdateComment);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.MoveFilesCheck, ASC.Files.EventHandler.onMoveFilesCheck);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetTemplates, ASC.Files.EventHandler.onGetTemplates);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.MoveItems, ASC.Files.EventHandler.onGetTasksStatuses);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.DeleteItem, ASC.Files.EventHandler.onGetTasksStatuses);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.EmptyTrash, ASC.Files.EventHandler.onGetTasksStatuses);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.Download, ASC.Files.EventHandler.onGetTasksStatuses);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.TerminateTasks, ASC.Files.EventHandler.onGetTasksStatuses);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetTasksStatuses, ASC.Files.EventHandler.onGetTasksStatuses);
        }
    };

    /* Events */

    var onGetFolderItems = function (xmlData, params, errorMessage) {
        if (typeof errorMessage != "undefined" || typeof xmlData == "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);

            if (ASC.Files.Tree) {
                ASC.Files.Tree.resetFolder(ASC.Files.Tree.getParentId(params.folderId));
            }

            ASC.Files.Anchor.defaultFolderSet();
            return;
        }

        if (!isCorrectCurrentFolder(xmlData.getElementsByTagName("folder_info")[0])) {
            ASC.Files.Anchor.defaultFolderSet();
            return;
        }

        if (!isCorrectPathParts(xmlData.getElementsByTagName("path_parts")[0])) {
            ASC.Files.Anchor.defaultFolderSet();
            return;
        }

        if (ASC.Files.Marker) {
            ASC.Files.Marker.markRootAsNew(xmlData.getElementsByTagName("root_folders_id_marked_as_new")[0]);
        }

        var xmlTotal = xmlData.getElementsByTagName("total")[0];
        ASC.Files.UI.countTotal = parseInt(xmlTotal.text || xmlTotal.textContent) || 0;

        if (!params.append) {
            jq("#filesMainContent").empty();
            jq(document).scrollTop(0);
        }

        var htmlXML = ASC.Files.TemplateManager.translate(xmlData);

        insertFolderItems(htmlXML);

        var filterSettings;
        if (htmlXML !== "" && ASC.Files.Filter && (filterSettings = ASC.Files.Filter.getFilterSettings()).isSet) {
            if (!jq("#filesBreadCrumbs").is(":visible")) {
                jq("#filesBreadCrumbs").show();
                jq(".to-parent-folder").hide();
            }
            jq("#filesBreadCrumbs").addClass("with-search-crumbs");
            jq("#searchBreadCrumbs")
                .attr("data-id", ASC.Files.Folders.currentFolder.id)
                .attr("href", ASC.Files.UI.getEntryLink("folder", ASC.Files.Folders.currentFolder.id))
                .attr("title", ASC.Files.Folders.currentFolder.title)
                .text(ASC.Files.Folders.currentFolder.title);

            jq("#searchBreadCrumbsSub").toggle(filterSettings.withSubfolders
                && (!ASC.Files.ThirdParty || !ASC.Files.ThirdParty.isThirdParty()));
        } else {
            jq("#filesBreadCrumbs").removeClass("with-search-crumbs");
        }

        jq(ASC.Files.UI.lastSelectedEntry).each(function () {
            var entryObj = ASC.Files.UI.getEntryObject(this.entryType, this.entryId);
            ASC.Files.UI.selectRow(entryObj, true);
        });
        ASC.Files.UI.updateMainContentHeader();
        ASC.Files.UI.lastSelectedEntry = null;

        if (ASC.Files.Folders.eventAfter != null && typeof ASC.Files.Folders.eventAfter == "function") {
            ASC.Files.Folders.eventAfter();
            ASC.Files.Folders.eventAfter = null;
        }

        if (ASC.Files.CreateMenu) {
            ASC.Files.CreateMenu.disableMenu(ASC.Files.UI.accessEdit());
        }

        ASC.Files.UI.stickContentHeader();
    };

    var insertFolderItems = function (htmlXML, replaceWith) {
        ASC.Files.UI.resetSelectAll();

        ASC.Files.EmptyScreen.hideEmptyScreen();

        if (htmlXML == "") {
            if (!replaceWith) {
                ASC.Files.EmptyScreen.displayEmptyScreen();
            }
        } else {
            if (replaceWith) {
                if (replaceWith.length) {
                    var selectedBefore = (replaceWith.hasClass("row-selected") || replaceWith.attr("data-selected") == "true");
                    replaceWith.after(htmlXML);
                    var newFolderItems = replaceWith.next();
                    replaceWith.remove();
                } else {
                    jq("#filesMainContent").prepend(htmlXML);
                    newFolderItems = jq("#filesMainContent .file-row[name=\"addRow\"]");
                }
            } else {
                jq("#filesMainContent").append(htmlXML);
                newFolderItems = jq("#filesMainContent .file-row:gt(-" + (ASC.Files.Constants.COUNT_ON_PAGE + 1) + ")");
            }

            //remove duplicate row
            jq("#filesMainContent .file-row[name=\"addRow\"]").each(function () {
                var objData = ASC.Files.UI.getObjectData(this);
                var entryObj = ASC.Files.UI.getEntryObject(objData.entryType, objData.id);
                entryObj.filter("[name!=\"addRow\"]").remove();
            });

            var countShowOnPage = parseInt(ASC.Files.Constants.COUNT_ON_PAGE) || 0;
            ASC.Files.UI.amountPage = parseInt((ASC.Files.UI.countTotal / countShowOnPage).toFixed(0));

            if (ASC.Files.UI.amountPage - (ASC.Files.UI.countTotal / countShowOnPage) < 0) {
                ASC.Files.UI.amountPage++;
            }

            ASC.Files.UI.currentPage = parseInt((jq("#filesMainContent .file-row[name!=\"addRow\"]").length - 1) / countShowOnPage) + 1;
            var countLeft = ASC.Files.UI.countTotal - jq("#filesMainContent .file-row").length;
            if (ASC.Files.UI.currentPage < ASC.Files.UI.amountPage && countLeft > 0) {
                jq("#pageNavigatorHolder").show();
                if (ASC.Files.FilesJSResources) {
                    jq("#pageNavigatorHolder a")
                        .text(countShowOnPage < countLeft ?
                            ASC.Files.FilesJSResources.ButtonShowMoreOf.format(countShowOnPage, countLeft) :
                            ASC.Files.FilesJSResources.ButtonShowMore.format(countLeft));
                }
            } else {
                jq("#pageNavigatorHolder").hide();
            }

            if (ASC.Files.Mouse) {
                ASC.Files.Mouse.collectEntryItems();
            }

            ASC.Files.UI.addRowHandlers(newFolderItems);
            if (selectedBefore) {
                ASC.Files.UI.selectRow(newFolderItems, selectedBefore);
                ASC.Files.UI.updateMainContentHeader();
            }

            var list = jq(".files-content-panel #filesMainContent .file-row.on-edit:not(.cannot-edit)");
            if (list.length > 0) {
                var fileIds = jq(list).map(function (i, item) {
                    return ASC.Files.UI.getObjectData(item).entryId;
                }).toArray();
                ASC.Files.Socket.subscribeChangeEditors(fileIds);
            }

            ASC.Files.UI.checkEditingDefer();
        }
        if (newFolderItems) {
            newFolderItems.attr("name", "");
        }
        return newFolderItems;
    };

    var isCorrectCurrentFolder = function (xmlData) {
        if (typeof xmlData == "undefined" || xmlData == null) {
            return false;
        }
        if (!ASC.Files.Folders) {
            return true;
        }

        ASC.Files.Folders.currentFolder = {};

        var xmlArray = jq(xmlData.childNodes);
        for (var item in xmlArray) {
            if (item && typeof xmlArray[item] == "object") {
                ASC.Files.Folders.currentFolder[xmlArray[item].tagName]
                    = (xmlArray[item].textContent || xmlArray[item].text || "");
            }
        }

        if (ASC.Files.Common.isCorrectId(ASC.Files.Folders.currentFolder.id)) {
            ASC.Files.Folders.currentFolder.entryId = ASC.Files.Folders.currentFolder.id;
        } else {
            return false;
        }

        ASC.Files.Folders.currentFolder.entryType = "folder";

        if (ASC.Files.Share) {
            ASC.Files.Folders.currentFolder.access = parseInt(ASC.Files.Folders.currentFolder.access || ASC.Files.Constants.AceStatusEnum.None);

            if (ASC.Files.Folders.currentFolder.access == ASC.Files.Constants.AceStatusEnum.Restrict) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.AceStatusEnum_Restrict, true);
                return false;
            }

            ASC.Files.Folders.currentFolder.shareable = (ASC.Files.Folders.currentFolder.shareable === "true");
        }

        if (ASC.Files.Tree && ASC.Files.Folders.currentFolder.id == ASC.Files.Tree.folderIdCurrentRoot) {
            ASC.Files.Folders.currentFolder.title = ASC.Files.FilesJSResources.ProjectFiles;
        }

        ASC.Files.UI.setDocumentTitle(ASC.Files.Folders.currentFolder.title);

        return true;
    };

    var isCorrectPathParts = function (xmlData) {
        if (typeof xmlData == "undefined" || xmlData == null) {
            return false;
        }

        var data = jq(xmlData.childNodes).map(function (i, childNode) {
            var xmlKey = childNode.childNodes[0];
            return xmlKey.textContent || xmlKey.text;
        });

        if (data.length == 0) {
            return false;
        }

        var rootFolderId = parseInt(data[0]);
        if (ASC.Files.Tree) {
            ASC.Files.Tree.pathParts = data;
        }

        jq("#mainContentHeader .menuAction").removeClass("unlockAction").hide();

        jq("#filesMainContent")
            .removeClass("myFiles")
            .removeClass("corporateFiles")
            .removeClass("shareformeFiles")
            .removeClass("recentFiles")
            .removeClass("favoritesFiles")
            .removeClass("templatesFiles")
            .removeClass("privacyFiles")
            .removeClass("trashFiles")
            .removeClass("projectFiles");

        switch (rootFolderId) {
            case ASC.Files.Constants.FOLDER_ID_MY_FILES:
                ASC.Files.Folders.folderContainer = "my";
                jq("#filesMainContent").addClass("myFiles");
                jq("#mainDownload, #mainMove, #mainCopy, #mainDelete, #mainConvert").show();
                break;
            case ASC.Files.Constants.FOLDER_ID_SHARE:
                ASC.Files.Folders.folderContainer = "forme";
                jq("#filesMainContent").addClass("shareformeFiles");
                jq("#mainDownload, #mainCopy, #mainMarkRead, #mainUnsubscribe, #mainConvert").show();
                break;
            case ASC.Files.Constants.FOLDER_ID_RECENT:
                ASC.Files.Folders.folderContainer = "recent";
                jq("#filesMainContent").addClass("recentFiles");
                jq("#mainDownload, #mainCopy, #mainConvert").show();
                break;
            case ASC.Files.Constants.FOLDER_ID_FAVORITES:
                ASC.Files.Folders.folderContainer = "favorites";
                jq("#filesMainContent").addClass("favoritesFiles");
                jq("#mainDownload, #mainCopy, #mainConvert, #mainRemoveFavorite").show();
                break;
            case ASC.Files.Constants.FOLDER_ID_TEMPLATES:
                ASC.Files.Folders.folderContainer = "templates";
                jq("#filesMainContent").addClass("templatesFiles");
                jq("#mainDownload, #mainCopy, #mainConvert, #mainRemoveTemplate").show();
                break;
            case ASC.Files.Constants.FOLDER_ID_PRIVACY:
                ASC.Files.Folders.folderContainer = "privacy";
                jq("#filesMainContent").addClass("privacyFiles");
                jq("#mainDownload, #mainMove, #mainDelete").show();
                break;
            case ASC.Files.Constants.FOLDER_ID_COMMON_FILES:
                ASC.Files.Folders.folderContainer = "corporate";
                jq("#filesMainContent").addClass("corporateFiles");
                jq("#mainDownload, #mainMove, #mainCopy, #mainMarkRead, #mainDelete, #mainConvert").show();
                break;
            case ASC.Files.Constants.FOLDER_ID_PROJECT:
                ASC.Files.Folders.folderContainer = "project";
                jq("#filesMainContent").addClass("projectFiles");
                jq("#mainDownload, #mainMove, #mainCopy, #mainMarkRead, #mainDelete, #mainConvert").show();
                break;
            case ASC.Files.Constants.FOLDER_ID_TRASH:
                ASC.Files.Folders.folderContainer = "trash";
                jq("#filesMainContent").addClass("trashFiles");
                jq("#mainDownload, #mainRestore, #mainDelete, #mainEmptyTrash, #mainConvert").show();
                jq("#mainEmptyTrash").addClass("unlockAction");
                break;
            case ASC.Files.Tree.folderIdCurrentRoot:
                ASC.Files.Folders.folderContainer = "project";
                jq("#filesMainContent").addClass("projectFiles");
                jq("#mainDownload, #mainMove, #mainCopy, #mainDelete, #mainConvert").show();
                break;
            default:
                if (!ASC.Files.Common.isCorrectId(ASC.Files.Folders.currentFolder.id)) {
                    return false;
                }
        }

        if (ASC.Files.Share && ASC.Files.Folders.currentFolder.shareable
            && !ASC.Resources.Master.Personal) {
            jq("#mainShare").show();
        }

        if (ASC.Files.Filter) {
            ASC.Files.Filter.disableFilter();
        }

        jq("#filesMainContent").toggleClass("without-share", !(ASC.Files.Share && ASC.Files.Folders.currentFolder.shareable));

        if (ASC.Files.Tree) {
            ASC.Files.Tree.updateTreePath();
        }

        ASC.Files.UI.checkButtonBack(".to-parent-folder", "#filesBreadCrumbs");

        return true;
    };

    var onGetPrivacy = function () {
        //fake duplicate from onGetFolderItems

        ASC.Files.Folders.currentFolder = {
            access: ASC.Files.Constants.AceStatusEnum.Read,
            entryId: ASC.Files.Constants.FOLDER_ID_PRIVACY,
            entryType: "folder",
            id: ASC.Files.Constants.FOLDER_ID_PRIVACY,
            shareable: false,
            title: ASC.Files.FilesJSResources.PrivacyRoom,
        };
        ASC.Files.UI.setDocumentTitle(ASC.Files.Folders.currentFolder.title);

        jq("#filesMainContent")
            .removeClass("myFiles")
            .removeClass("corporateFiles")
            .removeClass("shareformeFiles")
            .removeClass("recentFiles")
            .removeClass("favoritesFiles")
            .removeClass("templatesFiles")
            .removeClass("privacyFiles")
            .removeClass("trashFiles")
            .removeClass("projectFiles");

        ASC.Files.Folders.folderContainer = "privacy";
        jq("#filesMainContent").addClass("privacyFiles");

        if (ASC.Files.Tree) {
            ASC.Files.Tree.pathParts = [ASC.Files.Constants.FOLDER_ID_PRIVACY];
            ASC.Files.Tree.updateTreePath();
        }

        if (ASC.Files.Filter) {
            ASC.Files.Filter.disableFilter();
        }

        ASC.Files.UI.countTotal = 0;

        jq("#filesMainContent").empty();
        jq(document).scrollTop(0);

        jq("#emptyContainer_privacy .emptyScrBttnPnl").remove();
        ASC.Files.EmptyScreen.displayEmptyScreen();

        ASC.Files.UI.lastSelectedEntry = null;
        ASC.Files.Folders.eventAfter = null;

        if (ASC.Files.CreateMenu) {
            ASC.Files.CreateMenu.disableMenu(false);
        }
    };

    var onGetItems = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        if (ASC.Files.Folders.currentFolder.id != params.parentFolderID) {
            return;
        }

        if (params.lonelyType) {
            jsonData.__type = params.lonelyType;
            jsonData = [jsonData];
        }

        var htmlXml = "";

        for (var i = 0; i < jsonData.length; i++) {
            var stringXml = ASC.Files.Common.jsonToXml(
                jsonData[i].__type.toLowerCase().indexOf("folder") == -1
                    ? {"file": jsonData[i]}
                    : {"folder": jsonData[i]});
            htmlXml += ASC.Files.TemplateManager.translateFromString(stringXml);
        }

        var newEntries = insertFolderItems(htmlXml, {});
        if (newEntries) {
            var doReset = newEntries.hasClass("new-folder");
            newEntries.removeClass("new-file new-folder").show().yellowFade();

            if (ASC.Files.Tree && doReset) {
                ASC.Files.Tree.resetFolder(params.parentFolderID);
            }
        }
    };

    var onCreateNewFile = function (xmlData, params, errorMessage) {
        var fileNewObj = ASC.Files.UI.getEntryObject("file", "0");
        ASC.Files.UI.blockObject(fileNewObj);

        var winEditor = params.winEditor;
        if (typeof errorMessage != "undefined") {
            fileNewObj.remove();
            if (winEditor) {
                winEditor.close();
            }
            if (jq("#filesMainContent .file-row").length == 0) {
                ASC.Files.EmptyScreen.displayEmptyScreen();
            }
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var htmlXML = ASC.Files.TemplateManager.translate(xmlData);
        if (ASC.Files.Folders.currentFolder.id == params.folderID) {

            var fileObj = insertFolderItems(htmlXML, fileNewObj);

            fileObj.show().yellowFade().removeClass("new-file");
            var fileData = ASC.Files.UI.getObjectData(fileObj);
            var fileTitle = fileData.title;
            var fileId = fileData.entryId;

            if (ASC.Files.Folders.folderContainer == "privacy"
                && ASC.Desktop && ASC.Desktop.setAccess) {
                ASC.Files.UI.blockObject(fileObj, true, ASC.Files.FilesJSResources.DescriptCreate);

                ASC.Desktop.setAccess(fileId, function (encryptedFile) {
                    if (encryptedFile) {
                        ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.DesktopMessageStoring);
                        ASC.Files.Folders.replaceFileStream(fileId, fileTitle, encryptedFile, true, winEditor);
                    } else {
                        ASC.Files.UI.blockObject(fileObj);
                        ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoCrateFile.format(fileTitle));
                    }
                });
                return;
            } else {
                ASC.Files.Actions.checkEditFile(fileId, winEditor);
            }
        } else {
            fileObj = jq(htmlXML);
            fileData = ASC.Files.UI.getObjectData(fileObj);
            fileTitle = fileData.title;
            fileId = fileData.entryId;

            ASC.Files.Actions.checkEditFile(fileId, winEditor);
        }

        ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoCrateFile.format(fileTitle));
    };

    var onCreateFolder = function (xmlData, params, errorMessage) {
        var folderNewObj = ASC.Files.UI.getEntryObject("folder", "0");

        if (typeof errorMessage != "undefined") {
            folderNewObj.remove();
            if (jq("#filesMainContent .file-row").length == 0) {
                ASC.Files.EmptyScreen.displayEmptyScreen();
            }

            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        if (ASC.Files.Folders.currentFolder.id == params.parentFolderID) {
            var htmlXML = ASC.Files.TemplateManager.translate(xmlData);

            var folderObj = insertFolderItems(htmlXML, folderNewObj);

            folderObj.yellowFade().removeClass("new-folder");
            var folderTitle = ASC.Files.UI.getObjectData(folderObj).title;
        } else {
            folderTitle = params.title;
        }

        if (ASC.Files.Tree) {
            ASC.Files.Tree.reloadFolder(params.parentFolderID);
        }

        ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoCrateFolder.format(folderTitle));

        //track event

        trackingGoogleAnalytics("documents", "create", "folder");
    };

    var onRenameFolder = function (xmlData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var htmlXML = ASC.Files.TemplateManager.translate(xmlData);

        var folderId = params.folderId;
        var folderObj = ASC.Files.UI.getEntryObject("folder", folderId);

        folderObj = insertFolderItems(htmlXML, folderObj);

        folderObj.yellowFade().removeClass("new-folder");

        ASC.Files.UI.selectRow(folderObj, true);
        ASC.Files.UI.updateMainContentHeader();

        var folderNewTitle = ASC.Files.UI.getObjectData(folderObj).title;

        if (ASC.Files.Tree) {
            ASC.Files.Tree.reloadFolder(params.parentFolderID);
        }

        ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoRenameFolder.format(params.name, folderNewTitle));
    };

    var onRenameFile = function (xmlData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var fileData = ASC.Files.EventHandler.onGetFile(xmlData, params, errorMessage);

        ASC.Files.UI.selectRow(fileData.entryObject, true);
        ASC.Files.UI.updateMainContentHeader();

        var newName = fileData.title;

        ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoRenameFile.format(params.name, newName));
    };

    var onChangeOwner = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var createBy;
        var title;
        jq(params.list).each(function (i, entry) {
            var itemId = ASC.Files.UI.parseItemId(entry);

            var newData;
            jq(jsonData).each(function (j, data) {
                if (data.id == itemId.entryId) {
                    newData = data;
                    return false;
                }
                return true;
            });

            if (itemId.entryType == "folder") {
                var stringXml = ASC.Files.Common.jsonToXml({folder: newData});
                var htmlXML = ASC.Files.TemplateManager.translateFromString(stringXml);

                var folderId = itemId.entryId;
                var folderObj = ASC.Files.UI.getEntryObject("folder", folderId);

                folderObj = insertFolderItems(htmlXML, folderObj);

                folderObj.yellowFade().removeClass("new-folder");

                ASC.Files.UI.selectRow(folderObj, true);
                ASC.Files.UI.updateMainContentHeader();

                if (ASC.Files.Tree) {
                    ASC.Files.Tree.reloadFolder(params.parentFolderID);
                }

                var folderData = ASC.Files.UI.getObjectData(folderObj);
                createBy = folderData.create_by;
                title = folderData.title;
            } else {
                stringXml = ASC.Files.Common.jsonToXml({file: newData});

                var fileData = ASC.Files.EventHandler.onGetFile(stringXml,
                    {
                        fileId: itemId.entryId,
                        isStringXml: true,
                        show: true,
                    }, errorMessage);

                ASC.Files.UI.selectRow(fileData.entryObject, true);
                ASC.Files.UI.updateMainContentHeader();

                createBy = fileData.create_by;
                title = fileData.title;
            }
        });

        if (params.list.length > 1) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoChangeOwnerArray.format(Encoder.htmlEncode(createBy), params.list.length));
        } else {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoChangeOwner.format(Encoder.htmlEncode(createBy), title));
        }
    };

    var onUpdateHistory = function (jsonData, params, errorMessage) {
        var fileId = params.fileId;
        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.blockObject(fileObj);
            jq(".version-operation").css("visibility", "");
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        jq.extend(params, {isStringXml: true, show: true});
        var xmlFile = ASC.Files.Common.jsonToXml({file: jsonData.key});
        ASC.Files.EventHandler.onGetFile(xmlFile, params);

        var xmlHistory = ASC.Files.Common.jsonToXml({fileList: {entry: jsonData.value}});
        ASC.Files.EventHandler.onGetFileHistory(xmlHistory, params);
    };

    var onGetFileHistory = function (xmlData, params, errorMessage) {
        var fileId = params.fileId;
        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);

        ASC.Files.UI.blockObject(fileObj);
        ASC.Files.UI.updateMainContentHeader();

        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }
        var htmlXML =
        (params.isStringXml === true
            ? ASC.Files.TemplateManager.translateFromString(xmlData)
            : ASC.Files.TemplateManager.translate(xmlData));

        jq("#contentVersions").remove();
        fileObj.append(htmlXML).addClass("file-row-fix");

        var fileData = ASC.Files.UI.getObjectData(fileObj);

        var canEdit = true;
        if (!ASC.Files.UI.accessEdit(fileData, fileObj)
            || ASC.Files.UI.editingFile(fileObj)
            || ASC.Files.UI.lockedForMe(fileObj)
            || Teamlab.profile.isVisitor
            || fileData.encrypted) {
            jq(".version-comment-edit").remove();
            jq(".version-operation.version-restore").empty();
            jq(".version-complete, .version-continue").remove();
            jq(".version-num span").addClass("display-num");
            canEdit = false;
        } else {
            jq("#contentVersions").addClass("version-edit");
        }

        jq("#contentVersions").addClass("version-highlight");

        if (ASC.Files.Utility.CanWebView(fileData.title)) {
            jq(".not-preview").removeClass("not-preview");
        }

        var curVersion = jq("#contentVersions .version-row:first").attr("data-version");
        jq("#contentVersions .version-row[data-version='" + curVersion + "'] .version-restore").empty();

        var curVersionGroup = -1;
        jq("#contentVersions .version-row").each(function () {
            var row = jq(this);
            var versionGroup = row.attr("data-version-group");
            var version = row.attr("data-version");

            if (versionGroup != curVersionGroup) {
                if (version == curVersion) {
                    row.find(".version-continue").remove();
                } else {
                    row.find(".version-complete").remove();
                }
                curVersionGroup = versionGroup;
                row.addClass("version-group-head");

                var groupList = jq("#contentVersions .version-row[data-version-group=" + curVersionGroup + "]");
                if (groupList.length > 1) {
                    row.find(".version-sublist span").addClass("version-sublist-toggle").text(jq.format(ASC.Files.FilesJSResources.RevisionCount, groupList.length));
                }
            } else if (!canEdit) {
                row.find(".version-num").empty();
            } else {
                row.find(".version-num span, .version-continue").remove();
            }
        });

        ASC.Files.UI.registerUserProfilePopup(jq("#contentVersions"));
    };

    var onUpdateComment = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        jsonData = jsonData || "";
        var fileObj = ASC.Files.UI.getEntryObject("file", params.fileId);
        fileObj.find(".version-row[data-version=" + params.version + "]")
            .removeClass("version-row-comment")
            .find(".version-comment")
            .attr("data-comment", jsonData)
            .attr("title", (jsonData.length ? jsonData : ""))
            .html("<div class=\"version-comment-fix\">" + Encoder.htmlEncode(jsonData) + "</div>");
    };

    var onGetFile = function (xmlData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }
        var fileId = params.fileId;
        if (!ASC.Files.Common.isCorrectId(fileId)) {
            return;
        }

        var htmlXML =
        (params.isStringXml === true
            ? ASC.Files.TemplateManager.translateFromString(xmlData)
            : ASC.Files.TemplateManager.translate(xmlData));

        ASC.Files.Marker.removeNewIcon("file", fileId);

        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);

        fileObj = insertFolderItems(htmlXML, fileObj);

        var fileData = ASC.Files.UI.getObjectData(fileObj);
        if (!!fileData) {
            fileObj = fileData.entryObject;
            if (params.show) {
                ASC.Files.EmptyScreen.hideEmptyScreen();
                fileObj.removeClass("new-file").show().yellowFade();
            }

            if (fileObj.find(".is-new").is(":visible")) {
                ASC.Files.Marker.setNewCount(fileData.entryType, fileData.entryId, 1);
            }
        }

        ASC.Files.UI.updateMainContentHeader();
        return fileData;
    };

    var onMoveFilesCheck = function (data, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            jq(params.list.entry).each(function () {
                var curItem = ASC.Files.UI.parseItemId(this);
                ASC.Files.UI.blockObjectById(curItem.entryType, curItem.entryId, false, null, true);
            });
            ASC.Files.UI.updateMainContentHeader();
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        if (data != null && data.length > 0) {
            ASC.Files.Folders.showOverwriteMessage(params.list, params.folderToId, params.folderToTitle, params.isCopyOperation, data);
        } else {
            ASC.Files.ServiceManager.moveItems(ASC.Files.ServiceManager.events.MoveItems,
                {
                    folderToId: params.folderToId,
                    resolve: ASC.Files.Constants.ConflictResolveType.Duplicate,
                    isCopyOperation: params.isCopyOperation,
                    doNow: true
                },
                {stringList: params.list});
            ASC.Files.Folders.isCopyTo = false;
        }
    };

    var onMoveItemsFinish = function (listData, isCopyOperation, countProcessed) {
        var folderToId = ASC.Files.UI.parseItemId(listData[0]).entryId;
        listData = listData.slice(1);
        var listFromId = new Array();
        var listToId = new Array();
        for (var i = 0; i < listData.length; i++) {
            var curItem = ASC.Files.UI.parseItemId(listData[i]);
            if (curItem == null) {
                continue;
            }
            if (i % 2) {
                listToId.push(curItem);
            } else {
                listFromId.push(curItem);
                ASC.Files.UI.blockObjectById(curItem.entryType, curItem.entryId, false, null, true);
            }
        }
        ASC.Files.UI.updateMainContentHeader();

        var folderToObj = ASC.Files.UI.getEntryObject("folder", folderToId);
        folderToObj.removeClass("row-to");
        var folderFromId;

        var foldersCount = 0, filesCount = 0;
        var entryTitle = "";

        if (listFromId.length == 1) {
            entryTitle = ASC.Files.UI.getEntryTitle(listFromId[0].entryType, listFromId[0].entryId);
            if (typeof entryTitle == undefined || entryTitle == null) {
                entryTitle = "";
            }
        }

        for (i = 0; i < listFromId.length; i++) {
            var entryRowObj = ASC.Files.UI.getEntryObject(listFromId[i].entryType, listFromId[i].entryId);

            if (listFromId[i].entryType == "file") {
                filesCount++;
            } else {
                foldersCount += 1 + (parseInt(entryRowObj.find(".countFolders").html()) || 0);
                filesCount += parseInt(entryRowObj.find(".countFiles").html()) || 0;

                if (ASC.Files.Tree && !folderFromId) {
                    folderFromId = ASC.Files.Tree.getParentId(listFromId[i].entryId);
                }
            }

            if (!isCopyOperation && ASC.Files.Folders.currentFolder.id != folderToId) {
                ASC.Files.Marker.removeNewIcon(listFromId[i].entryType, listFromId[i].entryId);
                entryRowObj.remove();
            }
        }

        if (foldersCount > 0) {
            var folderCountObj = folderToObj.find(".countFolders");

            folderCountObj.html((parseInt(folderCountObj.html()) || 0) + foldersCount);

            if (ASC.Files.Tree) {
                ASC.Files.Tree.reloadFolder(folderToId);
                if (!isCopyOperation && folderToId != ASC.Files.Folders.currentFolder.id) {
                    ASC.Files.Tree.reloadFolder(folderFromId || ASC.Files.Folders.currentFolder.id);
                }
            }
        }

        if (filesCount > 0) {
            var fileCountObj = folderToObj.find(".countFiles");

            fileCountObj.html((parseInt(fileCountObj.html()) || 0) + filesCount);
        }

        if (listFromId.length > 0 && ASC.Files.Folders.currentFolder.id != folderToId) {
            ASC.Files.UI.checkEmptyContent();
        }

        if (isCopyOperation) {
            if (listFromId.length == 1 && entryTitle != "") {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoCopyItem.format(entryTitle));
            } else {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoCopyGroup.format(countProcessed));
            }
        } else {
            if (listFromId.length == 1 && entryTitle != "") {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoMoveItem.format(entryTitle));
            } else {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoMoveGroup.format(countProcessed));
            }
        }

        if (ASC.Files.Folders.currentFolder.id == folderToId) {
            var dataIds = new Array();

            for (i = 0; i < listToId.length; i++) {
                if (!ASC.Files.UI.getEntryObject(listToId[i].entryType, listToId[i].entryId).length) {
                    dataIds.push(listToId[i].entryType + "_" + listToId[i].entryId);
                }
            }

            ASC.Files.Folders.getItems(dataIds, folderToId);
        }
    };

    var onDeleteItemFinish = function (listData, totalCount) {
        var fromRootId = ASC.Files.UI.parseItemId(listData[0]).entryId;
        listData = listData.slice(1);
        var listItemId = new Array();
        for (var i = 0; i < listData.length; i++) {
            var curItem = ASC.Files.UI.parseItemId(listData[i]);
            if (curItem == null) {
                continue;
            }
            listItemId.push(curItem);
            ASC.Files.UI.blockObjectById(curItem.entryType, curItem.entryId, false, null, true);
        }
        ASC.Files.UI.updateMainContentHeader();

        var folderFromId;
        var foldersCountChange = false;
        var entryTitle = "";
        var redrawItems =
            ASC.Files.Tree &&
            (ASC.Files.Tree.pathParts.length > 0
                && (ASC.Files.Tree.pathParts[0] != ASC.Files.Constants.FOLDER_ID_TRASH
                    || fromRootId == ASC.Files.Constants.FOLDER_ID_TRASH
                    || (ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty())));

        if (listItemId.length == 1) {
            entryTitle = ASC.Files.UI.getEntryTitle(listItemId[0].entryType, listItemId[0].entryId);
            if (typeof entryTitle == undefined || entryTitle == null) {
                entryTitle = "";
            }
        }

        for (i = 0; i < listItemId.length; i++) {
            var entryRowObj = ASC.Files.UI.getEntryObject(listItemId[i].entryType, listItemId[i].entryId);

            if (listItemId[i].entryType == "folder") {
                if (!foldersCountChange) {
                    foldersCountChange = true;
                }
                if (ASC.Files.Tree && !folderFromId) {
                    folderFromId = ASC.Files.Tree.getParentId(listItemId[i].entryId);
                }
            }

            if (redrawItems) {
                ASC.Files.Marker.removeNewIcon(listItemId[i].entryType, listItemId[i].entryId);
                entryRowObj.remove();
            }
        }

        if (foldersCountChange && ASC.Files.Tree) {
            ASC.Files.Tree.reloadFolder(folderFromId || ASC.Files.Folders.currentFolder.id);
        }

        if (listItemId.length > 0 && redrawItems) {
            ASC.Files.UI.checkEmptyContent();
        }

        if (listItemId.length == 1 && entryTitle != "") {
            if (foldersCountChange > 0) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoRemoveFolder.format(entryTitle));
            } else {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoRemoveFile.format(entryTitle));
            }
        } else {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoRemoveGroup.format(listItemId.length, totalCount));
        }

        if (fromRootId == ASC.Files.Constants.FOLDER_ID_TRASH) {
            ASC.Files.ChunkUploads.initTenantQuota();
        }
    };

    var onCheckEditing = function (jsonData, params, errorMessage) {
        clearTimeout(ASC.Files.UI.timeCheckEditing);
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        if (!jsonData) {
            jsonData = [];
        }

        var list = jq(".files-content-panel #filesMainContent .file-row.on-edit:not(.cannot-edit)");

        for (var i = 0; i < list.length; i++) {
            var fileData = ASC.Files.UI.getObjectData(list[i]);
            var fileObj = fileData.entryObject;
            var fileId = fileData.entryId;
            ASC.Files.UI.lockEditFile(fileObj, false);

            var repl = true;
            for (var j = 0; j < jsonData.length && repl; j++) {
                if (fileId == jsonData[j].Key) {
                    repl = false;
                }
            }

            if (repl) {
                ASC.Files.Folders.replaceVersion(fileId, true);
            }
        }

        for (var k = 0; k < jsonData.length; k++) {
            fileId = jsonData[k].Key;
            var listBy = jsonData[k].Value;
            ASC.Files.UI.lockEditFileById(fileId, true, listBy);
        }

        if (jsonData.length > 0) {
            ASC.Files.UI.checkEditingDefer();
        }
    };

    var onUpdateIfExist = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        jq(".update-if-exist").prop("checked", jsonData === true);
    };

    var onLockFile = function (xmlData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var lock = params.lock;
        if (lock) {
            var fileObj = params.fileObj;
            fileObj.addClass("file-locked");
            ASC.Files.UI.addRowHandlers(fileObj);
        } else {
            params.show = true;
            ASC.Files.Actions.currentEntryData = ASC.Files.EventHandler.onGetFile(xmlData, params, errorMessage);
        }
    };

    var onGetTemplates = function (xmlData, params, errorMessage) {
        if (typeof errorMessage != "undefined" || typeof xmlData == "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            ASC.Files.Actions.hideAllActionPanels();

            return;
        }

        jq("#filesTemplateLoader").hide();

        var htmlData = ASC.Files.TemplateManager.translate(xmlData);
        var empty = !htmlData.length;

        if (!params.append) {
            jq("#filesTemplateEmpty").toggle(empty);
            jq("#filesTemplateList").toggle(!empty);
        }
        if (empty) {
            return;
        }
        if (params.append) {
            jq("#filesTemplateList").append(htmlData);
        }
        else {
            jq("#filesTemplateList").html(htmlData);
        }

        jq("#filesTemplateList .file-row").each(function () {
            var entryData = ASC.Files.UI.getObjectData(this);

            var entryId = entryData.entryId;
            var entryType = entryData.entryType;
            var entryObj = entryData.entryObject;
            var entryTitle = entryData.title;

            var ftClass = (entryType == "file" ? ASC.Files.Utility.getCssClassByFileTitle(entryTitle, true) : ASC.Files.Utility.getFolderCssClass(true));
            entryObj.find(".thumb-" + entryType).addClass(ftClass);

            var rowLink = entryObj.find(".entry-title .name a");

            if (entryType == "file" && rowLink.is(":not(:has(.file-extension))")) {
                ASC.Files.UI.highlightExtension(rowLink, entryTitle);
            }

            var entryUrl = ASC.Files.UI.getEntryLink(entryType, entryId, entryTitle);
            rowLink.attr("href", entryUrl).attr("target", "_blank");
        });
    };

    var onGetHelpCenter = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            ASC.Files.Anchor.defaultFolderSet();
            return;
        }

        if (params.update) {
            jq("#helpPanel").html(jsonData);
            StudioManager.initImageZoom();
        }

        LoadingBanner.hideLoading();
        jq("#helpPanel").show();

        showHelpPage(params.helpId);

        ASC.Files.UI.setDocumentTitle(ASC.Files.FilesJSResources.TitleSettingsHelp);
        ASC.Files.CreateMenu.disableMenu();
    };

    var onGetTasksStatuses = function (data, params, errorMessage) {
        if (typeof data !== "object" && typeof errorMessage != "undefined" || data == null) {
            ASC.Files.Folders.cancelTasksStatuses();
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        if (data.length == 0) {
            ASC.Files.Folders.cancelTasksStatuses();
            return;
        }

        var progress = 0;
        var notFinished = false;
        var operationType;
        var operationTypes = [ASC.Files.FilesJSResources.TasksOperationMove,
            ASC.Files.FilesJSResources.TasksOperationCopy,
            ASC.Files.FilesJSResources.TasksOperationDelete,
            ASC.Files.FilesJSResources.TasksOperationBulkdownload,
            ASC.Files.FilesJSResources.TasksOperationMarkAsRead];
        var blockTypes = [ASC.Files.FilesJSResources.DescriptMove,
            ASC.Files.FilesJSResources.DescriptCopy,
            ASC.Files.FilesJSResources.DescriptRemove,
            ASC.Files.FilesJSResources.DescriptBulkdownload,
            ASC.Files.FilesJSResources.DescriptMarkAsRead];

        if (data.length != 0) {
            //show
            if (jq("#tasksProgress:visible").length == 0) {
                clearTimeout(timoutTasksStatuses);

                if (jq("#tasksProgress").length == 0) {
                    jq("#progressTemplate").clone().attr("id", "tasksProgress").prependTo("#bottomLoaderPanel");
                    jq("#tasksProgress .progress-dialog-header").append("<a title=\"{0}\" class=\"actions-container close\">&times;</a>".format(ASC.Files.FilesJSResources.TitleCancel));
                    jq("#tasksProgress .progress-dialog-header").append("<span></span>");
                }
                ASC.Files.UI.setProgressValue("#tasksProgress", 0);
                jq("#tasksProgress .asc-progress-percent").text("0%");
                jq("#tasksProgress .progress-dialog-header span").text("");
                jq("#tasksProgress").show();

                if (jq.browser.mobile) {
                    jq("#bottomLoaderPanel").css("bottom", "auto");
                    jq("#bottomLoaderPanel").css("top", jq(window).scrollTop() + jq(window).height() - jq("#bottomLoaderPanel").height() + "px");
                }
            }

            //type operation in progress
            if (data.length > 1) {
                operationType = ASC.Files.FilesJSResources.TasksOperationMixed.format(data.length);
            } else {
                operationType = operationTypes[data[0].operation];
            }
            jq("#tasksProgress .progress-dialog-header span").text(operationType);
        }

        //in each process
        for (i = 0; i < data.length; i++) {

            //block descript on each elemets
            var splitCharacter = ":";
            var listSource = data[i].source.trim().split(splitCharacter);
            jq(listSource).each(function () {
                var itemId = ASC.Files.UI.parseItemId(this);
                if (itemId == null) {
                    return true;
                }
                ASC.Files.UI.blockObjectById(itemId.entryType, itemId.entryId, true, blockTypes[data[i].operation], true);
            });
            ASC.Files.UI.updateMainContentHeader();

            progress += data[i].progress;

            //finish
            if (data[i].finished) {
                if (data[i].result != null) {
                    var listResult = data[i].result.trim().split(splitCharacter);

                    switch (data[i].operation) {
                        case 0:
                            //move
                            onMoveItemsFinish(listResult, false, data[i].processed);
                            break;
                        case 1:
                            //copy
                            onMoveItemsFinish(listResult, true, data[i].processed);
                            break;
                        case 2:
                            //delete
                            onDeleteItemFinish(listResult, listSource.length);
                            break;
                        case 3:
                            //download
                            if (listResult[0]) {
                                location.href = listResult[0];
                            }
                            ASC.Files.Folders.bulkStatuses = false;
                            break;
                        case 4:
                            //mark as read
                            ASC.Files.Marker.onMarkAsRead(listResult);
                            break;
                    }
                }
                //unblock
                jq(listSource).each(function () {
                    var itemId = ASC.Files.UI.parseItemId(this);
                    if (itemId == null) {
                        return true;
                    }
                    ASC.Files.UI.blockObjectById(itemId.entryType, itemId.entryId, false, null, true);
                });
                ASC.Files.UI.updateMainContentHeader();

                //on error
                if (data[i].error != null) {
                    ASC.Files.UI.displayInfoPanel(data[i].error, true);
                }
            } else {
                notFinished = true;
            }
        }

        //progress %
        progress = (data.length == 0 ? 100 : progress / data.length);

        ASC.Files.UI.setProgressValue("#tasksProgress", progress);
        jq("#tasksProgress .asc-progress-percent").text(progress + "%");

        //complete
        if (!notFinished) {
            clearTimeout(timoutTasksStatuses);
            timoutTasksStatuses = setTimeout(ASC.Files.Folders.cancelTasksStatuses, 500);
        } else {
            //next iteration
            ASC.Files.Folders.getTasksStatuses(params.doNow);
        }
    };

    return {
        init: init,

        onGetFolderItems: onGetFolderItems,
        onGetPrivacy: onGetPrivacy,
        onGetItems: onGetItems,
        onGetFile: onGetFile,
        onCreateNewFile: onCreateNewFile,
        onCreateFolder: onCreateFolder,
        onRenameFolder: onRenameFolder,
        onRenameFile: onRenameFile,
        onChangeOwner: onChangeOwner,
        onUpdateHistory: onUpdateHistory,
        onGetFileHistory: onGetFileHistory,
        onUpdateComment: onUpdateComment,
        onCheckEditing: onCheckEditing,
        onLockFile: onLockFile,
        onGetTemplates: onGetTemplates,

        onUpdateIfExist: onUpdateIfExist,
        onGetHelpCenter: onGetHelpCenter,

        onMoveFilesCheck: onMoveFilesCheck,
        onGetTasksStatuses: onGetTasksStatuses
    };
})();

(function ($) {

    $(function () {
        ASC.Files.EventHandler.init();

        jq("#bottomLoaderPanel").on("click", "#tasksProgress a.close", function () {
            ASC.Files.Folders.terminateTasks();
            return false;
        });
    });
})(jQuery);