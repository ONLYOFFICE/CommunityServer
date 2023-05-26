/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


window.ASC.Files.Folders = (function () {
    var tasksTimeout = null;
    var bulkStatuses = false;

    var isFirstLoad = true;

    var currentFolder = {};
    var folderContainer = "";

    var eventAfter = null;
    var typeNewDoc = "";

    var isCopyTo = false;

    /* Methods*/

    var getFolderItems = function (isAppend, countAppend) {
        var filterSettings = ASC.Files.Filter.getFilterSettings();
        ASC.Files.ServiceManager.getFolderItems(ASC.Files.ServiceManager.events.GetFolderItems,
            {
                folderId: ASC.Files.Folders.currentFolder.id,
                from: (isAppend ? jq("#filesMainContent .file-row[name!=\"addRow\"]").length : 0),
                count: countAppend || ASC.Files.Constants.COUNT_ON_PAGE,
                append: isAppend === true,
                filter: filterSettings.filter,
                subjectGroup: filterSettings.subjectGroup,
                subjectId: filterSettings.subject,
                search: filterSettings.text,
                orderBy: filterSettings.sorter,
                withSubfolders: filterSettings.withSubfolders,
                searchInContent: filterSettings.searchInContent
            }, { orderBy: filterSettings.sorter });
    };

    var getItems = function (dataIds, folderToId) {
        var data = {
            entry: jq(typeof dataIds == "object" ? dataIds : [dataIds]).map(function (i, id) {
                return id;
            }).toArray()
        };

        var filterSettings = ASC.Files.Filter.getFilterSettings();

        ASC.Files.ServiceManager.getItems(ASC.Files.ServiceManager.events.GetItems,
            {
                parentFolderID: folderToId,
                filter: filterSettings.filter,
                subjectGroup: filterSettings.subjectGroup,
                subjectId: filterSettings.subject,
                search: filterSettings.text,
            },
            { stringList: data });
    };

    var clickOnFolder = function (folderId) {
        var folderObj = ASC.Files.UI.getEntryObject("folder", folderId);

        if (ASC.Files.Folders.folderContainer == "trash"
            && jq("#filesMainContent").find(folderObj).is(":visible")) {
            return;
        }

        if (folderId == ASC.Files.Constants.FOLDER_ID_PROJECT
            && ASC.Files.Tree.folderIdCurrentRoot
            && ASC.Files.Tree.folderIdCurrentRoot != ASC.Files.Constants.FOLDER_ID_PROJECT) {
            location.href = "TMDocs.aspx";
            return;
        }

        ASC.Files.Filter.clearFilter(true);
        ASC.Files.Anchor.navigationSet(folderId);
    };

    var clickOnFile = function (fileData, forEdit, version) {
        var fileObj = fileData.entryObject;
        if (ASC.Files.Folders.folderContainer == "trash"
            && jq("#filesMainContent").find(fileObj).is(":visible")) {
            return false;
        }

        var fileId = fileData.id;
        var fileTitle = fileData.title || ASC.Files.UI.getEntryTitle("file", fileId);
        if (forEdit != false) {
            version = version || fileData.version || 0;
        }

        if (ASC.Files.Utility.CanWebView(fileTitle)) {
            if (ASC.Files.Utility.MustConvert(fileTitle) && (fileData.encrypted || ASC.Files.UI.denyDownload(fileData))) {
                forEdit = false;
            }
            return ASC.Files.Converter.checkCanOpenEditor(fileId, fileTitle, version, forEdit != false);
        }

        if (typeof ASC.Files.ImageViewer != "undefined" && ASC.Files.Utility.CanImageView(fileTitle)) {
            if (ASC.Files.UI.denyDownload(fileData)) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.ErrorMassage_SecurityException, true);
                return false;
            }
            var hash = ASC.Files.ImageViewer.getPreviewHash(fileId);
            ASC.Files.Anchor.move(hash);
            return false;
        }

        if (typeof ASC.Files.MediaPlayer != "undefined" && ASC.Files.MediaPlayer.canPlay(fileTitle, true)) {
            if (ASC.Files.UI.denyDownload(fileData)) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.ErrorMassage_SecurityException, true);
                return false;
            }
            hash = ASC.Files.MediaPlayer.getPlayHash(fileId);
            ASC.Files.Anchor.move(hash);
            return false;
        }

        location.href = ASC.Files.Utility.GetFileDownloadUrl(fileId, version);
        return ASC.Files.Marker.removeNewIcon("file", fileId);
    };

    var updateIfExist = function (target) {
        var value = jq(target).prop("checked");

        ASC.Files.ServiceManager.updateIfExist(ASC.Files.ServiceManager.events.UpdateIfExist, { value: value });
    };

    var toggleFavorite = function (fileObj, toFavorite) {
        var listObj;
        if (fileObj) {
            fileObj = jq(fileObj);
            if (!fileObj.is(".file-row")) {
                fileObj = fileObj.closest(".file-row");
            }

            listObj = fileObj;
        } else {
            if (toFavorite) {
                listObj = jq("#filesMainContent .file-row:not(.checkloading):not(.new-file):has(.checkbox input:checked):not(.on-favorite)");
            } else {
                listObj = jq("#filesMainContent .file-row:not(.checkloading):not(.new-file):has(.checkbox input:checked).on-favorite");
            }
        }

        var fileIds = [];
        var folderIds = [];
        var isFavorite;
        listObj = listObj.filter(function () {
            var entryRowData = ASC.Files.UI.getObjectData(this);
            var entryRowType = entryRowData.entryType;
            var entryObject = entryRowData.entryObject;

            if (entryRowType == "file") {
                var curIsFavorite = entryObject.hasClass("on-favorite");

                if (toFavorite !== true || !curIsFavorite) {
                    isFavorite = curIsFavorite;
                    var entryRowId = entryRowData.entryId;
                    fileIds.push(entryRowId);

                    return true;
                }
            }

            if (entryRowType == "folder") {
                var curIsFavorite = entryObject.hasClass("on-favorite");

                if (toFavorite !== true || !curIsFavorite) {
                    isFavorite = curIsFavorite;
                    var entryRowId = entryRowData.entryId;
                    folderIds.push(entryRowId);

                    return true;
                }
            }
            return false;
        });

        listObj.toggleClass("on-favorite", !isFavorite);

        var method = toFavorite !== true && isFavorite ? Teamlab.removeFilesFavorites : Teamlab.addFilesFavorites;
        method(
            {},
            {
                fileIds: fileIds,
                folderIds: folderIds
            },
            function () {
                listObj.toggleClass("on-favorite", !isFavorite);
                if (ASC.Files.Folders.folderContainer == "favorites") {
                    ASC.Files.UI.removeEntryObject(listObj);
                    ASC.Files.UI.updateMainContentHeader();
                    ASC.Files.UI.checkEmptyContent();
                }
                ASC.Files.ServiceManager.removeFromCache(ASC.Files.ServiceManager.events.GetFolderItems);
            },
            {
                error: function (params, error) {
                    ASC.Files.UI.displayInfoPanel(error, true);
                }
            }
        );
    };

    var toggleTemplate = function (fileObj) {
        var listObj;
        if (fileObj) {
            fileObj = jq(fileObj);
            if (!fileObj.is(".file-row")) {
                fileObj = fileObj.closest(".file-row");
            }

            listObj = fileObj;
        } else {
            listObj = jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):has(.checkbox input:checked)");
        }

        var fileIds = [];
        var isTemplate;
        listObj.each(function () {
            var entryRowData = ASC.Files.UI.getObjectData(this);
            var entryRowType = entryRowData.entryType;
            var entryObject = entryRowData.entryObject;

            if (entryRowType == "file") {
                var entryRowId = entryRowData.entryId;
                fileIds.push(entryRowId);

                isTemplate = entryObject.hasClass("is-template");
            }
        });

        listObj.toggleClass("is-template", !isTemplate);

        var method = isTemplate ? Teamlab.removeFilesTemplates : Teamlab.addFilesTemplates;
        method(
            {},
            {
                fileIds: fileIds
            },
            function () {
                listObj.toggleClass("is-template", !isTemplate);

                var message =
                    isTemplate
                        ? ASC.Files.FilesJSResource.InfoTemplateRemove
                        : ASC.Files.FilesJSResource.InfoTemplateAdd;

                ASC.Files.UI.displayInfoPanel(message);
                if (ASC.Files.Folders.folderContainer == "templates") {
                    ASC.Files.UI.removeEntryObject(listObj);
                    ASC.Files.UI.updateMainContentHeader();
                    ASC.Files.UI.checkEmptyContent();
                }
            },
            {
                error: function (params, error) {
                    ASC.Files.UI.displayInfoPanel(error, true);
                }
            }
        );
    };

    var download = function (entryType, entryId, version) {
        var list = new Array();
        if (!ASC.Files.Common.isCorrectId(entryId)) {
            list = jq("#filesMainContent .file-row:has(.checkbox input:checked)");
            if (list.length == 0) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.EmptyListSelectedForDownload, true);
                return;
            }
            if (list.length == 1) {
                var itemData = ASC.Files.UI.getObjectData(list[0]);
                entryId = itemData.entryId;
                entryType = itemData.entryType;
            }
        }

        if (entryType === "file") {
            ASC.Files.Marker.removeNewIcon(entryType, entryId);

            location.href = ASC.Files.Utility.GetFileDownloadUrl(entryId, version);
            return;
        }

        if (ASC.Files.Folders.bulkStatuses == true) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.ErrorMassage_SecondDownload, true);
            return;
        }

        var data = new Array();

        if (ASC.Files.Common.isCorrectId(entryId)) {
            data.push(
                {
                    Key: entryType + "_" + entryId,
                    Value: ""
                });

            ASC.Files.Marker.removeNewIcon(entryType, entryId);
        } else {
            list.each(function () {
                var curItemData = ASC.Files.UI.getObjectData(this);

                data.push(
                    {
                        Key: curItemData.entryType + "_" + curItemData.entryId,
                        Value: ""
                    });

                ASC.Files.Marker.removeNewIcon(curItemData.entryType, curItemData.entryId);
            });
        }
        ASC.Files.Folders.bulkDownload(data);
    };

    var bulkDownload = function (data) {
        ASC.Files.Folders.bulkStatuses = true;
        ASC.Files.ServiceManager.download(ASC.Files.ServiceManager.events.Download, { doNow: true }, data);
    };

    var getActionHtml = function () {
        return "" +
            "<div class=\"rename-action\">" +
            "<div class=\"button gray btn-action __apply name-aplly\" title=" + ASC.Files.FilesJSResource.TitleCreate + "></div>" +
            "<div class=\"button gray btn-action __reset name-cancel\" title=" + ASC.Files.FilesJSResource.TitleCancel + "></div>" +
            "</div>";
    };

    var cancelEnter = function (event, cancelButton) {
        var entryObject = jq(cancelButton || this).closest(".file-row");
        if (!entryObject.attr("name")) {
            entryObject
                .removeClass("row-rename")
                .find("#promptRename, .rename-action").remove();

            ASC.Files.UI.selectRow(entryObject, true);
            ASC.Files.UI.updateMainContentHeader();
        } else {
            ASC.Files.UI.removeEntryObject(entryObject);
            if (jq("#filesMainContent .file-row").length == 0) {
                ASC.Files.EmptyScreen.displayEmptyScreen();
            }
        }
    };

    var createFolder = function () {
        if (ASC.Files.MediaPlayer && ASC.Files.MediaPlayer.isView) {
            return;
        }

        if (!ASC.Files.UI.accessEdit() || ASC.Files.UI.isSettingsPanel()) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.ErrorMassage_SecurityException, true);
            return;
        }

        jq(document).scrollTop(0);

        var newFolderObj = ASC.Files.UI.getEntryObject("folder", "0");
        if (newFolderObj.length != 0) {
            return;
        }

        var emptyFolder = {
            folder:
            {
                id: "0",
                title: ASC.Files.FilesJSResource.TitleNewFolder,
                access: 0,
                shared: false,
                isnew: 0,
            }
        };
        var stringData = ASC.Files.Common.jsonToXml(emptyFolder);

        var htmlXML = ASC.Files.TemplateManager.translateFromString(stringData);

        ASC.Files.EmptyScreen.hideEmptyScreen();
        var mainContent = jq("#filesMainContent");
        mainContent.prepend(htmlXML);

        jq("#filesMainContent .new-folder").yellowFade().removeClass("new-folder");

        newFolderObj = ASC.Files.UI.getEntryObject("folder", "0");
        newFolderObj.addClass("row-rename");

        var obj = newFolderObj.find(".entry-title .name a");

        var ftClass = ASC.Files.Utility.getFolderCssClass();
        newFolderObj.find(".thumb-img, .thumb-folder").addClass(ftClass);

        var newContainer = document.createElement("input");
        newContainer.id = "promptCreateFolder";
        newContainer.type = "text";
        newContainer.style.display = "none";
        document.body.appendChild(newContainer);

        newContainer = jq("#promptCreateFolder");
        newContainer.attr("maxlength", ASC.Files.Constants.MAX_NAME_LENGTH);
        newContainer.addClass("textEdit input-rename");
        newContainer.val(ASC.Files.FilesJSResource.TitleNewFolder);
        newContainer.insertAfter(obj);
        newContainer.show();
        obj.hide();
        newContainer.trigger("focus");
        if (!jq.browser.mobile) {
            newContainer.trigger("select");
        }

        ASC.Files.UI.checkCharacter(newContainer);

        var saveFolder = function (event, saveButton) {
            var newFolderSaveObj = jq(saveButton || this).closest(".file-row");

            var newName = ASC.Files.Common.replaceSpecCharacter(jq("#promptCreateFolder").val().trim());
            if (newName == "" || newName == null || ASC.Resources.Master.CustomMode && newName === "...") {
                newName = ASC.Files.FilesJSResource.TitleNewFolder;
            }

            newFolderSaveObj.find(".entry-title .name a").show().text(newName);
            newFolderSaveObj.removeClass("row-rename");
            jq("#promptCreateFolder").remove();
            newFolderSaveObj.find(".rename-action").remove();

            var params = { parentFolderID: ASC.Files.Folders.currentFolder.id, title: newName };

            ASC.Files.UI.blockObject(newFolderSaveObj, true, ASC.Files.FilesJSResource.DescriptCreate);
            ASC.Files.ServiceManager.createFolder(ASC.Files.ServiceManager.events.CreateFolder, params);
        };

        newFolderObj.append(getActionHtml());
        newFolderObj.find(".name-aplly").on("click", saveFolder);

        jq("#promptCreateFolder").on("keydown", function (event) {
            if (jq("#promptCreateFolder").length == 0) {
                return;
            }

            if (!e) {
                var e = event;
            }
            var code = e.keyCode || e.which;

            switch (code) {
                case ASC.Files.Common.keyCode.esc:
                    ASC.Files.Folders.cancelEnter(e, this);
                    break;
                case ASC.Files.Common.keyCode.enter:
                    saveFolder(e, this);
                    break;
            }
        });
    };

    var createNewDoc = function (fileData, defaultName, createFileMethod) {
        if (ASC.Files.MediaPlayer && ASC.Files.MediaPlayer.isView) {
            return;
        }

        if (ASC.Files.UI.isSettingsPanel()) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.ErrorMassage_SecurityException, true);
            return;
        }

        var createFile = function (folderID, title, templateId) {
            var winEditor = window.open(ASC.Desktop ? "" : ASC.Files.Constants.URL_LOADER);

            var params =
            {
                folderID: folderID,
                fileTitle: title,
                templateId: templateId,
                winEditor: winEditor,
            };

            if (typeof createFileMethod == "function") {
                createFileMethod(params);
                return;
            }

            ASC.Files.ServiceManager.createNewFile(ASC.Files.ServiceManager.events.CreateNewFile, params);
        };

        if (!ASC.Files.UI.accessEdit()) {
            if (fileData) {
                createFile(null, fileData.title, fileData.entryId);
            } else {
                createFile();
            }
            return;
        }

        jq(document).scrollTop(0);

        var newFileObj = ASC.Files.UI.getEntryObject("file", "0");
        ASC.Files.UI.removeEntryObject(newFileObj);

        if (fileData) {
            var titleNewDoc = fileData.title;
        } else {
            switch (ASC.Files.Folders.typeNewDoc) {
                case "document":
                    titleNewDoc = ASC.Files.FilesJSResource.TitleNewFileText + ASC.Files.Utility.Resource.InternalFormats.Document;
                    break;
                case "spreadsheet":
                    titleNewDoc = ASC.Files.FilesJSResource.TitleNewFileSpreadsheet + ASC.Files.Utility.Resource.InternalFormats.Spreadsheet;
                    break;
                case "presentation":
                    titleNewDoc = ASC.Files.FilesJSResource.TitleNewFilePresentation + ASC.Files.Utility.Resource.InternalFormats.Presentation;
                    break;
                case "masterform":
                    titleNewDoc = ASC.Files.FilesJSResource.TitleNewFileFormTemplate + ASC.Files.Utility.Resource.MasterFormExtension;
                    break;
                default:
                    return;
            }
        }

        if (!ASC.Files.Utility.CanWebEdit(titleNewDoc)) {
            return;
        }

        var emptyFile = {
            file:
            {
                id: "0",
                title: titleNewDoc,
                access: 0,
                shared: false,
                version: 0,
                version_group: 0
            }
        };

        var stringData = ASC.Files.Common.jsonToXml(emptyFile);

        var htmlXML = ASC.Files.TemplateManager.translateFromString(stringData);

        ASC.Files.EmptyScreen.hideEmptyScreen();
        var mainContent = jq("#filesMainContent");
        mainContent.prepend(htmlXML);

        jq("#filesMainContent .new-file").show().yellowFade().removeClass("new-file");

        newFileObj = ASC.Files.UI.getEntryObject("file", "0");

        var saveFile = function (event, saveButton) {
            var newFileSaveObj = jq(saveButton || this).closest(".file-row");

            var newName = ASC.Files.Common.replaceSpecCharacter(jq("#promptCreateFile").val().trim());
            var oldName = ASC.Files.UI.getObjectTitle(newFileSaveObj);
            if (newName == "" || newName == null || ASC.Resources.Master.CustomMode && newName === "...") {
                newName = oldName;
            } else {
                var curLenExt = ASC.Files.Utility.GetFileExtension(oldName).length;
                newName += oldName.substring(oldName.length - curLenExt);
            }

            newFileSaveObj.removeClass("row-rename");
            jq("#promptCreateFile").remove();
            newFileSaveObj.find(".rename-action").remove();

            newFileSaveObj.find(".entry-title .name a").show().text(newName);

            var rowLink = newFileSaveObj.find(".entry-title .name a");
            ASC.Files.UI.highlightExtension(rowLink, newName);
            ASC.Files.UI.blockObject(newFileSaveObj, true, ASC.Files.FilesJSResource.DescriptCreate);

            createFile(ASC.Files.Folders.currentFolder.id, newName, fileData ? fileData.entryId : null);
        };

        if (defaultName) {
            saveFile(null, newFileObj);
            return;
        }

        newFileObj.addClass("row-rename");

        var obj = newFileObj.find(".entry-title .name a");

        var ftClass = ASC.Files.Utility.getCssClassByFileTitle(titleNewDoc);
        newFileObj.find(".thumb-img, .thumb-file").addClass(ftClass);

        var lenExt = ASC.Files.Utility.GetFileExtension(titleNewDoc).length;
        titleNewDoc = titleNewDoc.substring(0, titleNewDoc.length - lenExt);

        var newContainer = document.createElement("input");
        newContainer.id = "promptCreateFile";
        newContainer.type = "text";
        newContainer.style.display = "none";
        document.body.appendChild(newContainer);
        newContainer = jq("#promptCreateFile");
        newContainer.attr("maxlength", ASC.Files.Constants.MAX_NAME_LENGTH - lenExt);
        newContainer.addClass("textEdit input-rename");
        newContainer.val(titleNewDoc);
        newContainer.insertAfter(obj);
        newContainer.show().trigger("focus");
        if (!jq.browser.mobile) {
            newContainer.trigger("select");
        }

        ASC.Files.UI.checkCharacter(newContainer);

        newFileObj.append(getActionHtml());
        newFileObj.find(".name-aplly").on("click", saveFile);

        jq("#promptCreateFile").on("keydown", function (event) {
            if (jq("#promptCreateFile").length == 0) {
                return;
            }

            if (!e) {
                var e = event;
            }
            var code = e.keyCode || e.which;

            switch (code) {
                case ASC.Files.Common.keyCode.esc:
                    ASC.Files.Folders.cancelEnter(e, this);
                    break;
            }
        });

        jq("#promptCreateFile").on("keypress", function (event) {
            if (jq("#promptCreateFile").length == 0) {
                return;
            }

            if (!e) {
                var e = event;
            }
            var code = e.keyCode || e.which;

            switch (code) {
                case ASC.Files.Common.keyCode.enter:
                    saveFile(e, this);
                    break;
            }
        });
    };

    var createNewForm = function (fileData) {
        var title = fileData.title;
        var ext = ASC.Files.Utility.GetFileExtension(title);
        var newTitle = title.substring(0, title.length - ext.length) + ASC.Files.Utility.FileExtensionLibrary.OformExts[0];

        Teamlab.copyDocFileAs(null, fileData.id,
            {
                destFolderId: fileData.folder_id,
                destTitle: newTitle
            },
            {
                success: function (_, data) {
                    ASC.Files.ServiceManager.getFile(ASC.Files.ServiceManager.events.CreateNewFile,
                        {
                            fileId: data.id,
                            show: true,
                            isStringXml: false,
                            folderID: fileData.folder_id,
                            winEditor: false
                        });
                },
                error: function (_, error) {
                    ASC.Files.UI.displayInfoPanel(error[0], true);
                },
                processUrl: function (url) {
                    return ASC.Files.Utility.AddExternalShareKey(url);
                }
            });
    };

    var replaceFileStream = function (fileId, fileTitle, file, encrypted, winEditor, forcesave) {

        Teamlab.updateFileStream({
            fileId: fileId,
            encrypted: !!encrypted,
            forcesave: !!forcesave,
        },
            file,
            {
                success: function () {
                    //todo: think about share page in editor
                    ASC.Files.ServiceManager.getFile(ASC.Files.ServiceManager.events.ReplaceVersion,
                        {
                            fileId: fileId,
                            show: true,
                            isStringXml: false
                        }
                    );

                    if (forcesave) {
                        return;
                    }

                    ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.InfoCrateFile.format(fileTitle));

                    ASC.Files.Actions.checkEditFile(fileId, winEditor);
                },
                error: function (params, error) {
                    if (winEditor) {
                        winEditor.close();
                    }
                    ASC.Files.UI.displayInfoPanel(error[0], true);
                }
            }
        );
    };

    var rename = function (entryType, entryId) {
        var entryObj = ASC.Files.UI.getEntryObject(entryType, entryId);

        if (ASC.Files.Folders.folderContainer == "trash"
            || !ASC.Files.UI.accessEdit(null, entryObj)
            || ASC.Files.UI.lockedForMe(entryObj)
            || !ASC.Files.UI.accessDelete(entryObj) && Teamlab.profile.isVisitor === true
            || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_PROJECT
            || entryType === "file" && ASC.Files.UI.editingFile(entryObj) && ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty()) {
            return;
        }

        var lenExt = 0;

        if (jq("#promptRename").length != 0) {
            jq("#filesMainContent .file-row.row-rename .name-cancel").trigger("click");
        }

        if (entryObj.find("#contentVersions").length) {
            ASC.Files.Folders.closeVersions();
        }

        entryObj.addClass("row-rename");
        ASC.Files.UI.selectRow(entryObj, false);
        ASC.Files.UI.updateMainContentHeader();

        var entryTitle = ASC.Files.UI.getObjectTitle(entryObj);

        if (entryType == "file") {
            lenExt = ASC.Files.Utility.GetFileExtension(entryTitle).length;
            entryTitle = entryTitle.substring(0, entryTitle.length - lenExt);
        }

        var newContainer = document.createElement("input");
        newContainer.id = "promptRename";
        newContainer.type = "text";
        newContainer.style.display = "none";
        document.body.appendChild(newContainer);

        newContainer = jq("#promptRename");
        newContainer.attr("maxlength", ASC.Files.Constants.MAX_NAME_LENGTH - lenExt);
        newContainer.addClass("textEdit input-rename");
        newContainer.val(entryTitle);
        newContainer.insertAfter(entryObj.find(".entry-title .name a"));
        newContainer.show().trigger("focus");
        if (!jq.browser.mobile) {
            newContainer.trigger("select");
        }

        ASC.Files.UI.checkCharacter(newContainer);

        var saveRename = function () {
            var entryRenameData = ASC.Files.UI.getObjectData(jq("#promptRename"));
            var entryRenameObj = entryRenameData.entryObject;
            var entryRenameType = entryRenameData.entryType;
            var entryRenameId = entryRenameData.entryId;
            var oldName = ASC.Files.UI.getObjectTitle(entryRenameObj);

            var newName = ASC.Files.Common.replaceSpecCharacter(jq("#promptRename").val().trim());
            if (newName == "" || newName == null || ASC.Resources.Master.CustomMode && newName === "...") {
                ASC.Files.UI.selectRow(entryRenameObj, true);
                ASC.Files.UI.updateMainContentHeader();
                return;
            }

            entryRenameObj.removeClass("row-rename");
            entryRenameObj.find(".rename-action").remove();
            jq("#promptRename").remove();

            if (entryRenameType == "file") {
                var lenExtRename = ASC.Files.Utility.GetFileExtension(oldName).length;
                newName += oldName.substring(oldName.length - lenExtRename);
            }

            if (newName == oldName) {
                ASC.Files.UI.selectRow(entryRenameObj, true);
                ASC.Files.UI.updateMainContentHeader();
                return;
            }

            entryRenameObj.find(".entry-title .name a").show().text(newName);

            if (entryRenameType == "file") {
                var rowLink = entryRenameObj.find(".entry-title .name a");
                ASC.Files.UI.highlightExtension(rowLink, newName);
            }

            ASC.Files.UI.blockObject(entryRenameObj, true, ASC.Files.FilesJSResource.DescriptRename);

            if (entryRenameType == "file") {
                ASC.Files.ServiceManager.renameFile(ASC.Files.ServiceManager.events.FileRename, { fileId: entryRenameId, name: oldName, newname: newName, show: true });
            } else {
                ASC.Files.ServiceManager.renameFolder(ASC.Files.ServiceManager.events.FolderRename, { parentFolderID: ASC.Files.Folders.currentFolder.id, folderId: entryRenameId, name: oldName, newname: newName });
            }
        };

        entryObj.append(getActionHtml());
        entryObj.find(".name-aplly").on("click", saveRename);

        entryObj.removeClass("row-selected");

        jq("#promptRename").on("keydown", function (event) {
            if (jq("#promptRename").length === 0) {
                return;
            }

            if (!e) {
                var e = event;
            }
            var code = e.keyCode || e.which;

            if ((code == ASC.Files.Common.keyCode.esc || code == ASC.Files.Common.keyCode.enter)
                && jq("#promptRename").length != 0) {
                switch (code) {
                    case ASC.Files.Common.keyCode.esc:
                        ASC.Files.Folders.cancelEnter(e, this);
                        break;
                    case ASC.Files.Common.keyCode.enter:
                        saveRename();
                        break;
                }
            }
        });
    };

    var lockFile = function (fileObj, fileId) {
        if (ASC.Files.Folders.folderContainer == "trash") {
            return;
        }

        fileObj = jq(fileObj);
        if (!fileObj.is(".file-row")) {
            fileObj = fileObj.closest(".file-row");
        }

        if (ASC.Files.UI.lockedForMe(fileObj) && !ASC.Files.Constants.ADMIN && ASC.Files.Folders.folderContainer != "my") {
            return;
        }

        var locked = fileObj.hasClass("file-locked");

        fileObj.toggleClass("file-locked", !locked);

        fileId = fileId || ASC.Files.UI.getObjectData(fileObj).entryId;
        ASC.Files.ServiceManager.lockFile(ASC.Files.ServiceManager.events.LockFile, { fileId: fileId, lock: !locked, fileObj: fileObj });
    };

    var showVersions = function (fileObj, fileId) {
        if (ASC.Files.Folders.folderContainer == "trash") {
            return;
        }

        fileObj = jq(fileObj);
        if (!fileObj.is(".file-row")) {
            fileObj = fileObj.closest(".file-row");
        }

        if (fileObj.hasClass("folder-row")) {
            return;
        }

        if (jq("#contentVersions:visible").length != 0) {
            var close = fileObj.hasClass("display-versions");
            ASC.Files.Folders.closeVersions();
            if (close) {
                return;
            }
        }

        ASC.Files.UI.blockObject(fileObj, true, ASC.Files.FilesJSResource.DescriptLoadVersion);

        fileId = fileId || ASC.Files.UI.getObjectData(fileObj).entryId;

        ASC.Files.ServiceManager.getFileHistory(ASC.Files.ServiceManager.events.GetFileHistory, { fileId: fileId });
    };

    var toggleVersionSublist = function (versionGroup) {
        ASC.Files.Actions.hideAllActionPanels();
        jq("#contentVersions .version-row[data-version-group='" + versionGroup + "']:not(.version-group-head)").toggle();
    };

    var makeCurrentVersion = function (fileId, version) {
        jq(".version-operation").css("visibility", "hidden");
        ASC.Files.UI.blockObjectById("file", fileId, true, ASC.Files.FilesJSResource.DescriptSetVersion);
        ASC.Files.ServiceManager.setCurrentVersion(ASC.Files.ServiceManager.events.SetCurrentVersion, { fileId: fileId, version: version });
    };

    var versionComplete = function (fileId, version, continueVersion) {
        ASC.Files.Actions.hideAllActionPanels();

        jq(".version-operation").css("visibility", "hidden");
        ASC.Files.UI.blockObjectById("file", fileId, true, ASC.Files.FilesJSResource.DescriptCompleteVersion);
        ASC.Files.ServiceManager.completeVersion(ASC.Files.ServiceManager.events.CompleteVersion,
            {
                fileId: fileId,
                version: version,
                continueVersion: continueVersion
            });
        return false;
    };

    var enterComment = function () {
        ASC.Files.Actions.hideAllActionPanels();

        var fileObj = jq(this).closest(".file-row");
        if (!ASC.Files.UI.accessEdit(null, fileObj)
            || ASC.Files.UI.editingFile(fileObj)
            || ASC.Files.UI.lockedForMe(fileObj)) {
            return true;
        }

        var versionObj = jq(this).closest(".version-row");
        var commentObj = versionObj.find(".version-comment");

        var comment = commentObj.attr("data-comment");
        ASC.Files.Folders.eraseComment();
        commentObj.empty();

        versionObj.addClass("version-row-comment");
        commentObj.attr("colspan", 4);

        var newContainer = document.createElement("input");
        newContainer.id = "promptVersionComment";
        newContainer.type = "text";
        newContainer.style.display = "none";
        document.body.appendChild(newContainer);

        newContainer = jq("#promptVersionComment");
        newContainer.attr("maxlength", 255);
        newContainer.attr("placeholder", ASC.Files.FilesJSResource.EnterComment);
        newContainer.addClass("textEdit input-rename");
        newContainer.val(comment);
        commentObj.append(newContainer);
        newContainer.show();
        newContainer.trigger("focus");
        if (!jq.browser.mobile) {
            newContainer.trigger("select");
        }

        var saveComment = function () {
            var editor = jq("#promptVersionComment");
            commentObj = editor.closest(".version-comment");
            comment = commentObj.attr("data-comment");
            var entryObject = jq(".display-versions");
            var fileId = ASC.Files.UI.getObjectData(entryObject).id;
            var version = commentObj.closest(".version-row").attr("data-version");
            var newComment = editor.val().trim();

            if (newComment != comment) {
                ASC.Files.ServiceManager.updateComment(ASC.Files.ServiceManager.events.UpdateComment, { fileId: fileId, version: version, comment: newComment });
            }
            ASC.Files.Folders.eraseComment();
        };

        commentObj.append(getActionHtml());
        commentObj.find(".name-aplly").on("click", saveComment);
        jq("#promptVersionComment").on("keydown", function (event) {
            if (jq("#promptVersionComment").length == 0) {
                return;
            }

            if (!e) {
                var e = event;
            }
            var code = e.keyCode || e.which;

            switch (code) {
                case ASC.Files.Common.keyCode.esc:
                    ASC.Files.Folders.eraseComment();
                    break;
                case ASC.Files.Common.keyCode.enter:
                    saveComment();
                    break;
            }
        });

        jq(document).on("click.Comment", function (e) {
            e = jq.fixEvent(e);
            if (!jq(e.target || e.srcElement).is(".version-comment:has(#promptVersionComment) *")) {
                ASC.Files.Folders.eraseComment();
                jq(document).off("click.Comment");
            }
        });

        return false;
    };

    var eraseComment = function () {
        var oldComment = jq(".version-row-comment .version-comment").attr("data-comment");
        jq(".version-row-comment .version-comment").html("<div class=\"version-comment-fix\">" + Encoder.htmlEncode(oldComment) + "</div>").removeAttr("colspan");
        jq(".version-row-comment").removeClass("version-row-comment");
    };

    var closeVersions = function () {
        jq("#contentVersions").remove();
        jq(".file-row-fix").removeClass("file-row-fix");
        jq(".display-versions").removeClass("display-versions");
    };

    var replaceVersion = function (fileId, show) {
        ASC.Files.Actions.hideAllActionPanels();
        ASC.Files.ServiceManager.getFile(ASC.Files.ServiceManager.events.ReplaceVersion,
            {
                fileId: fileId,
                show: show,
                isStringXml: false
            });
    };

    var showOverwriteMessage = function (listData, folderId, folderToTitle, isCopyOperation, data) {
        var folderTitle = ASC.Files.UI.getEntryTitle("folder", folderId) || (ASC.Files.Tree ? ASC.Files.Tree.getFolderTitle(folderId) : "");

        var message;
        if (data.length > 1) {
            var files = "";
            for (var i = 0; i < data.length; i++) {
                files += "<li title=\"{0}\">{0}</li>".format(data[i].Value);
            }
            jq("#overwriteList").html(files);
            jq("#overwriteListShow").show();

            message = ASC.Files.FilesJSResource.FilesAlreadyExist.format(data.length, "<b title=\"" + folderTitle + "\">" + folderTitle + "</b>");
        } else {
            jq("#overwriteListShow").hide();

            message = ASC.Files.FilesJSResource.FileAlreadyExist.format("<span title=\"" + data[0].Value + "\">" + data[0].Value + "</span>", "<b title=\"" + folderTitle + "\">" + folderTitle + "</b>");
        }
        jq("#overwriteList, #overwriteListHide").hide();

        jq("#overwriteMessage").html(message);

        jq("#buttonConfirmResolve").off("click").on("click", function () {
            var resolve = jq(".overwrite-resolve input:checked").val();

            if (resolve == ASC.Files.Constants.ConflictResolveType.Skip) {
                for (i = 0; i < data.length; i++) {
                    ASC.Files.UI.blockObjectById("file", data[i].Key, false, null, true);
                    var pos = jq.inArray("file_" + data[i].Key, listData.entry);
                    if (pos != -1) {
                        listData.entry.splice(pos, 1);
                    }
                }
                ASC.Files.UI.updateMainContentHeader();
            }

            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();

            isCopyOperation = isCopyOperation == true;

            if (resolve == ASC.Files.Constants.ConflictResolveType.Overwrite) {
                owerwriteManager.set(isCopyOperation, listData.entry, data.length);
            }

            ASC.Files.ServiceManager.moveItems(ASC.Files.ServiceManager.events.MoveItems,
                {
                    folderToId: folderId,
                    resolve: resolve,
                    isCopyOperation: isCopyOperation,
                    doNow: true
                },
                { stringList: listData });
        });

        jq("#buttonCancelOverwrite").off("click").on("click", function () {
            for (i = 0; i < listData.entry.length; i++) {
                var itemId = ASC.Files.UI.parseItemId(listData.entry[i]);
                ASC.Files.UI.blockObjectById(itemId.entryType, itemId.entryId, false, null, true);
            }
            ASC.Files.UI.updateMainContentHeader();

            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();
        });

        ASC.Files.UI.blockUI("#confirmOverwriteFiles", 465);

        PopupKeyUpActionProvider.EnterAction = "jq(\"#buttonConfirmResolve\").trigger('click');";
        PopupKeyUpActionProvider.CloseDialogAction = "jq(\"#buttonCancelOverwrite\").trigger('click');";
    };

    var owerwriteManager = function () {
        function strHash(str) {
            var hash = 0, i, l;
            if (str.length == 0) {
                return hash;
            }
            for (i = 0, l = str.length; i < l; i++) {
                hash = ((hash << 5) - hash) + str.charCodeAt(i);
                hash |= 0;
            }
            return hash;
        }

        function getKey(isCopyOperation, entries) {
            var sortedArrayCopy = entries.concat().sort();
            return (isCopyOperation ? "copy" : "move") + strHash(sortedArrayCopy.toString());
        }

        function get(isCopyOperation, entries) {
            var key = getKey(isCopyOperation, entries);
            var value = +sessionStorage.getItem(key);
            sessionStorage.removeItem(key)
            return value;
        }

        function set(isCopyOperation, entries, value) {
            var key = getKey(isCopyOperation, entries);
            sessionStorage.setItem(key, value)
        }

        return {
            get: get,
            set: set
        }
    }();

    var curItemFolderMoveTo = function (folderToId, folderToTitle, pathDest, confirmedThirdParty) {
        ASC.Files.Actions.hideAllActionPanels();
        if (folderToId === ASC.Files.Folders.currentFolder.entryId) {
            ASC.Files.Folders.isCopyTo = false;
            return;
        }

        var thirdParty = typeof ASC.Files.ThirdParty != "undefined";

        if (!confirmedThirdParty
            && !ASC.Files.Folders.isCopyTo
            && thirdParty
            && ASC.Files.ThirdParty.isThirdParty()
            && ASC.Files.ThirdParty.isDifferentThirdParty(folderToId, ASC.Files.Folders.currentFolder.entryId)) {
            ASC.Files.ThirdParty.showMoveThirdPartyMessage(folderToId, folderToTitle, pathDest);
            return;
        }

        var takeThirdParty = thirdParty && (ASC.Files.Folders.isCopyTo == true || ASC.Files.ThirdParty.isThirdParty());
        var moveAccessDeny = false;

        var data = {};
        data.entry = new Array();

        jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)").each(function () {
            var entryData = ASC.Files.UI.getObjectData(this);
            var entryType = entryData.entryType;
            var entryObj = entryData.entryObject;
            var entryId = entryData.entryId;

            if (ASC.Files.Folders.isCopyTo == true
                || (!ASC.Files.UI.editingFile(entryObj) && !ASC.Files.UI.lockedForMe(entryObj))) {

                if (ASC.Files.Folders.isCopyTo == false && !ASC.Files.UI.accessDelete(entryObj)) {
                    moveAccessDeny = true;
                } else {
                    if (entryType == "folder" && jq.inArray(entryId, pathDest) != -1) {
                        ASC.Files.UI.displayInfoPanel(((ASC.Files.Folders.isCopyTo == true) ? ASC.Files.FilesJSResource.InfoFolderCopyError : ASC.Files.FilesJSResource.InfoFolderMoveError), true);
                    } else {
                        if (takeThirdParty
                            || !thirdParty
                            || !ASC.Files.ThirdParty.isThirdParty(entryData)) {
                            ASC.Files.UI.blockObject(entryObj,
                                true,
                                (ASC.Files.Folders.isCopyTo == true) ?
                                    ASC.Files.FilesJSResource.DescriptCopy :
                                    ASC.Files.FilesJSResource.DescriptMove,
                                true);

                            data.entry.push(entryType + "_" + entryId);
                        }
                    }
                }
            }
        });

        if (moveAccessDeny) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.InfoMoveAccessError, true);
        }

        ASC.Files.UI.updateMainContentHeader();

        if (data.entry && data.entry.length != 0) {
            ASC.Files.ServiceManager.moveFilesCheck(ASC.Files.ServiceManager.events.MoveFilesCheck,
                {
                    folderToTitle: folderToTitle,
                    folderToId: folderToId,
                    list: data,
                    isCopyOperation: (ASC.Files.Folders.isCopyTo == true)
                },
                { stringList: data });
        }

        ASC.Files.Folders.isCopyTo = false;
    };

    var createDuplicate = function (listEntries) {
        var listData = {};
        listData.entry = new Array();
        jq(listEntries).each(function (i, e) {
            listData.entry.push(e.entryType + "_" + e.id);
        });

        ASC.Files.ServiceManager.moveItems(ASC.Files.ServiceManager.events.MoveItems,
            {
                folderToId: ASC.Files.Folders.currentFolder.entryId,
                resolve: 2,
                isCopyOperation: true,
                doNow: true
            },
            { stringList: listData });
    };

    var showMore = function () {
        if (jq("#pageNavigatorHolder:visible").length == 0
            || jq("#pageNavigatorHolder a").text() == ASC.Files.FilesJSResource.ButtonShowMoreLoad) {
            return;
        }

        jq("#pageNavigatorHolder a").text(ASC.Files.FilesJSResource.ButtonShowMoreLoad);

        ASC.Files.Folders.getFolderItems(true);
    };

    var changeOwnerDialog = function (entryData) {

        var ownerSelectorObj = jq("#ownerSelector").attr("data-id", entryData.create_by_id);

        var ownerSelectorInnerObj = ownerSelectorObj.find(".change-owner-selector").text(entryData.create_by);

        if (!ownerSelectorObj.data().useradvancedSelector) {
            ownerSelectorObj.useradvancedSelector({
                inPopup: true,
                itemsSelectedIds: [entryData.create_by_id],
                canadd: false,
                showGroups: true,
                onechosen: true,
                withGuests: false,
            })
            .on("showList", function (event, item) {
                jq("#buttonSaveChangeOwner").removeClass("disable");
                ownerSelectorInnerObj.html(item.title);
                ownerSelectorObj.attr("data-id", item.id);
            });
        } else {
            ownerSelectorObj.useradvancedSelector("select", [entryData.create_by_id]);
        }

        jq("#changeOwnerTitle").text(entryData.title);
        jq("#buttonSaveChangeOwner").addClass("disable");

        jq("#changeOwner").off("click").on("click", "#buttonSaveChangeOwner:not(.disable)", function () {
            var userId = jq("#ownerSelector").attr("data-id");

            PopupKeyUpActionProvider.CloseDialog();
            ASC.Files.Folders.changeOwner([entryData.entryType + "_" + entryData.entryId], userId);
        });

        PopupKeyUpActionProvider.EnterAction = "jq(\"#buttonSaveChangeOwner\").trigger('click');";

        ASC.Files.UI.blockUI("#changeOwner", 420);
    };

    var changeOwner = function (entries, userId) {
        var data = { entry: entries };

        ASC.Files.ServiceManager.changeOwner(ASC.Files.ServiceManager.events.ChangeOwner,
            {
                list: entries,
                userId: userId,
                parentFolderID: ASC.Files.Folders.currentFolder.id,
            },
            { stringList: data });
    };

    var emptyTrash = function () {
        if (ASC.Files.Folders.folderContainer != "trash") {
            return;
        }

        ASC.Files.Actions.hideAllActionPanels();

        ASC.Files.UI.checkSelectAll(true);

        jq("#confirmRemoveText").html(ASC.Files.FilesJSResource.ConfirmEmptyTrash);

        jq("#confirmRemoveList").hide();
        jq("#confirmRemoveTextToContinue").hide();
        jq("#confirmRemoveSharpBoxTextDescription").hide();

        jq("#confirmRemoveTextDescription").show();

        jq("#removeConfirmBtn").off("click").on("click", function () {
            PopupKeyUpActionProvider.CloseDialog();

            ASC.Files.ServiceManager.emptyTrash(ASC.Files.ServiceManager.events.EmptyTrash, { doNow: true });
        });

        ASC.Files.UI.blockUI("#confirmRemove", 420);
        PopupKeyUpActionProvider.EnterAction = "jq(\"#removeConfirmBtn\").trigger('click');";
    };

    var deleteItem = function (entryType, entryId, successfulDeletion) {
        ASC.Files.Actions.hideAllActionPanels();

        var data = ASC.Files.UI.autoCleanUpSetting.gap;
        var settingAutoCleanUp = null;
        switch (data) {
            case -1: settingAutoCleanUp = null; break;
            case 1: settingAutoCleanUp = ASC.Files.FilesJSResource.DateOneWeek; break;
            case 2: settingAutoCleanUp = ASC.Files.FilesJSResource.DateTwoWeeks; break;
            case 3: settingAutoCleanUp = ASC.Files.FilesJSResource.DateOneMonth; break;
            case 4: settingAutoCleanUp = ASC.Files.FilesJSResource.DateTwoMonths; break;
            case 5: settingAutoCleanUp = ASC.Files.FilesJSResource.DateThreeMonths; break;
        }
        var folderContainer = ASC.Files.Folders.folderContainer;
        var isThirdParty = ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty();
        var isAutoCleanUpOn = settingAutoCleanUp != null && folderContainer != "trash" && folderContainer != "privacy" && !isThirdParty;
        var caption = isAutoCleanUpOn
            ? ASC.Files.FilesJSResource.ConfirmRemoveListPermanently.format(settingAutoCleanUp)
            : ASC.Files.FilesJSResource.ConfirmRemoveList;
        var list = new Array();

        if (entryType && entryId) {
            var entryObj = ASC.Files.UI.getEntryObject(entryType, entryId);
            if (!ASC.Files.UI.accessDelete(entryObj)
                || ASC.Files.UI.editingFile(entryObj)
                || ASC.Files.UI.lockedForMe(entryObj)) {
                return;
            }

            list.push({ entryType: entryType, entryId: entryId });

        } else {
            jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)").each(function () {
                var entryRowData = ASC.Files.UI.getObjectData(this);
                var entryRowObj = entryRowData.entryObject;
                var entryRowType = entryRowData.entryType;
                var entryRowId = entryRowData.entryId;

                if (ASC.Files.ThirdParty && !ASC.Files.ThirdParty.isThirdParty()
                    && ASC.Files.ThirdParty.isThirdParty(entryRowData)) {
                    return true;
                }
                if (ASC.Files.UI.accessDelete(entryRowObj)
                    && !ASC.Files.UI.editingFile(entryRowObj)
                    && !ASC.Files.UI.lockedForMe(entryRowObj)) {
                    list.push({ entryType: entryRowType, entryId: entryRowId });
                }
            });
        }

        if (list.length == 0) {
            return;
        }

        if (list.length == 1) {
            if (isAutoCleanUpOn) {
                if (list[0].entryType == "file") {
                    caption = ASC.Files.FilesJSResource.ConfirmRemoveFilePermanently.format(settingAutoCleanUp);
                } else {
                    caption = ASC.Files.FilesJSResource.ConfirmRemoveFolderPermanently.format(settingAutoCleanUp);
                }
            }
            else {
                if (list[0].entryType == "file") {
                    caption = ASC.Files.FilesJSResource.ConfirmRemoveFile;
                } else {
                    caption = ASC.Files.FilesJSResource.ConfirmRemoveFolder;
                }
            }
        }

        var textFolder = "";
        var textFile = "";
        var strHtml = "<label title=\"{0}\"><input type=\"checkbox\" class=\"checkbox\" entryType=\"{1}\" entryId=\"{2}\" checked=\"checked\">{0}</label>";
        for (var i = 0; i < list.length; i++) {
            var entryRowTitle = ASC.Files.UI.getEntryTitle(list[i].entryType, list[i].entryId);
            if (list[i].entryType == "file") {
                if (entryRowTitle == null && ASC.Files.ChunkUploads) {
                    var uploadData = ASC.Files.ChunkUploads.getUploadDataByFileId(list[i].entryId, 1);
                    if (uploadData) {
                        entryRowTitle = uploadData.name;
                    }
                }
                textFile += strHtml.format(entryRowTitle, list[i].entryType, list[i].entryId);
            } else {
                textFolder += strHtml.format(entryRowTitle, list[i].entryType, list[i].entryId);
            }
        }

        jq("#confirmRemoveText").html(caption);
        jq("#confirmRemoveList dd.confirm-remove-files").html(textFile);
        jq("#confirmRemoveList dd.confirm-remove-folders").html(textFolder);

        jq("#confirmRemoveList .confirm-remove-folders, #confirmRemoveList .confirm-remove-files").show();
        if (textFolder == "") {
            jq("#confirmRemoveList .confirm-remove-folders").hide();
        }
        if (textFile == "") {
            jq("#confirmRemoveList .confirm-remove-files").hide();
        }

        var checkRemoveItem = function () {
            jq("#confirmRemoveList").show();
            var foldersCount = jq("#confirmRemoveList dd.confirm-remove-folders :checked").length;
            jq("#confirmRemoveList .confirm-remove-folders-count").text(foldersCount);
            var filesCount = jq("#confirmRemoveList dd.confirm-remove-files :checked").length;
            jq("#confirmRemoveList .confirm-remove-files-count").text(filesCount);
            jq("#removeConfirmBtn").toggleClass("disable", foldersCount + filesCount == 0)
        };
        checkRemoveItem();
        jq("#confirmRemoveList dd [type='checkbox']").on("change", checkRemoveItem);

        var mustConfirm = jq(".files-content-panel").attr("data-deleteConfirm");
        if (jq("#cbxDeleteConfirm").length) {
            mustConfirm = jq("#cbxDeleteConfirm").prop("checked") == true;
        }
        if (ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty()) {
            mustConfirm = true;
            jq("#confirmRemoveSharpBoxTextDescription").show();
        } else {
            jq("#confirmRemoveSharpBoxTextDescription").hide();
        }

        if (ASC.Files.Folders.folderContainer == "trash"
            || ASC.Files.Folders.folderContainer == "privacy") {
            mustConfirm = true;
            jq("#confirmRemoveTextDescription").show();
        } else {
            jq("#confirmRemoveTextDescription").hide();
        }

        if (isAutoCleanUpOn) {
            jq("#confirmRemoveTextToContinue").show();
        }
        else {
            jq("#confirmRemoveTextToContinue").hide();
        }

        var doDeletion = function () {
            if (jq(this).hasClass("disable")) return;

            PopupKeyUpActionProvider.CloseDialog();

            var data = {};
            var listChecked = jq("#confirmRemoveList input:checked");
            if (listChecked.length == 0) {
                return;
            }

            data.entry = jq(listChecked).map(function (j, item) {
                var entryConfirmType = jq(item).attr("entryType");
                var entryConfirmId = jq(item).attr("entryId");
                var entryConfirmObj = ASC.Files.UI.getEntryObject(entryConfirmType, entryConfirmId);
                ASC.Files.UI.blockObject(entryConfirmObj, true, ASC.Files.FilesJSResource.DescriptRemove, true);
                return entryConfirmType + "_" + entryConfirmId;
            }).toArray();

            ASC.Files.UI.updateMainContentHeader();
            ASC.Files.ServiceManager.deleteItem(ASC.Files.ServiceManager.events.DeleteItem, { list: data.entry, doNow: true }, { stringList: data });
            if (typeof successfulDeletion == "function") {
                successfulDeletion();
            }
        };

        if (mustConfirm) {
            jq("#removeConfirmBtn").off("click").on("click", doDeletion);

            ASC.Files.UI.blockUI("#confirmRemove", 420);

            PopupKeyUpActionProvider.EnterAction = "jq(\"#removeConfirmBtn\").trigger('click');";
        } else {
            doDeletion();
        }
    };

    var cancelTasksStatuses = function () {
        ASC.Files.Folders.bulkStatuses = false;

        ASC.Files.UI.setProgressValue("#tasksProgress", 0);
        jq("#tasksProgress .asc-progress-percent").text("0%");
        jq("#tasksProgress .progress-dialog-header span").text("");
        jq("#tasksProgress").hide();
    };

    var terminateTasks = function () {
        clearTimeout(ASC.Files.Folders.tasksTimeout);

        ASC.Files.ServiceManager.terminateTasks(ASC.Files.ServiceManager.events.TerminateTasks, { isTerminate: true, doNow: true });
    };

    var getTasksStatuses = function (doNow) {
        clearTimeout(ASC.Files.Folders.tasksTimeout);

        ASC.Files.Folders.tasksTimeout = setTimeout(
            function () {
                ASC.Files.ServiceManager.getTasksStatuses(ASC.Files.ServiceManager.events.GetTasksStatuses, { doNow: false });
            }, ASC.Files.Constants.REQUEST_STATUS_DELAY / (doNow === true ? 8 : 1));
    };

    var getTemplateList = function (isAppend) {
        ASC.Files.ServiceManager.getTemplates(ASC.Files.ServiceManager.events.GetTemplates,
            {
                filter: ASC.Files.Constants.FilterType.None,
                from: (isAppend ? jq("#filesTemplateList .file-row").length : 0),
                count: ASC.Files.Constants.COUNT_ON_PAGE,
                append: isAppend === true,
                subjectGroup: false,
                subjectId: "",
                search: "",
                searchInContent: false
            });
    };

    return {
        eventAfter: eventAfter,

        currentFolder: currentFolder,
        folderContainer: folderContainer,

        isCopyTo: isCopyTo,

        cancelEnter: cancelEnter,

        createFolder: createFolder,
        replaceVersion: replaceVersion,

        lockFile: lockFile,
        showVersions: showVersions,
        closeVersions: closeVersions,
        toggleVersionSublist: toggleVersionSublist,
        makeCurrentVersion: makeCurrentVersion,
        enterComment: enterComment,
        eraseComment: eraseComment,
        versionComplete: versionComplete,

        showOverwriteMessage: showOverwriteMessage,
        owerwriteManager: owerwriteManager,
        curItemFolderMoveTo: curItemFolderMoveTo,
        createDuplicate: createDuplicate,

        changeOwnerDialog: changeOwnerDialog,
        changeOwner: changeOwner,

        rename: rename,
        deleteItem: deleteItem,
        emptyTrash: emptyTrash,

        toggleFavorite: toggleFavorite,
        toggleTemplate: toggleTemplate,

        getFolderItems: getFolderItems,
        getItems: getItems,

        clickOnFolder: clickOnFolder,
        clickOnFile: clickOnFile,

        updateIfExist: updateIfExist,

        showMore: showMore,

        createNewDoc: createNewDoc,
        createNewForm: createNewForm,
        typeNewDoc: typeNewDoc,
        replaceFileStream: replaceFileStream,

        getTemplateList: getTemplateList,

        download: download,
        bulkDownload: bulkDownload,

        getTasksStatuses: getTasksStatuses,
        cancelTasksStatuses: cancelTasksStatuses,
        terminateTasks: terminateTasks,
        tasksTimeout: tasksTimeout,
        bulkStatuses: bulkStatuses,

        isFirstLoad: isFirstLoad
    };
})();

(function ($) {
    $(function () {

        jq("#pageNavigatorHolder a").on("click", function () {
            ASC.Files.Folders.showMore();
            return false;
        });

        jq("#topNewFolder a").on("click", function () {
            ASC.Files.Folders.createFolder();
        });

        jq("#buttonDelete, #mainDelete").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.deleteItem();
        });

        jq("#buttonEmptyTrash, #mainEmptyTrash").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.emptyTrash();
        });

        jq("#studioPageContent").on("click", "#buttonDownload, #mainDownload.unlockAction", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.download();
        });

        jq("#filesSelectAllCheck").on("click", function (e) {
            e.stopPropagation();

            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.UI.checkSelectAll(jq("#filesSelectAllCheck").prop("checked") == true);
            jq(this).trigger("blur");
        });

        jq("#filesMainContent").on("click", ".file-lock", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.lockFile(this);
            return false;
        });

        jq("#filesMainContent").on("click", ".favorite", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.toggleFavorite(this);
            return false;
        });

        jq("#filesMainContent").on("click", ".template-action", function () {
            ASC.Files.Actions.hideAllActionPanels();
            var fileData = ASC.Files.UI.getObjectData(this);
            if (fileData.id != 0) {
                ASC.Files.Folders.createNewDoc(fileData);
            }
            return false;
        });

        jq("#filesTemplatesPanel").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
        });

        jq("#filesTemplatesPanel").on("click", ".file-row:not(.folder-row):not(.error-entry) .entry-title .name a, .file-row:not(.folder-row):not(.error-entry) .thumb-file, .file-row:not(.folder-row):not(.error-entry) .thumb-img", function () {
            ASC.Files.Actions.hideAllActionPanels();
            var fileData = ASC.Files.UI.getObjectData(this);
            if (fileData.id != 0) {
                ASC.Files.Folders.createNewDoc(fileData);
            }
            return false;
        });

        jq("#buttonRemoveFavorite, #mainRemoveFavorite, #buttonAddFavorite").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.toggleFavorite(null, this.id == "buttonAddFavorite");
        });

        jq("#buttonRemoveTemplate, #mainRemoveTemplate").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.toggleTemplate();
        });

        jq("#filesMainContent").on("click", ".version", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.showVersions(this);
            return false;
        });

        jq("#filesMainContent").on("click", ".version-close", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.closeVersions();
            return false;
        });

        jq("#filesMainContent").on("click", ".folder-row:not(.error-entry) .entry-title .name a, .folder-row:not(.error-entry) .thumb-folder, .folder-row:not(.error-entry) .thumb-img, .folder-row.min:not(.error-entry):not(.row-rename) .entry-title .name", function (event) {
            ASC.Files.Actions.hideAllActionPanels();

            if (ASC.Files.Folders.folderContainer == "trash") {
                ASC.Files.UI.clickRow(event, jq(this).closest(".file-row"));
                return false;
            }

            var folderId = ASC.Files.UI.getObjectData(this).id;
            if (folderId != 0) {
                ASC.Files.Folders.clickOnFolder(folderId);
            }
            return false;
        });

        jq(".to-parent-folder, .empty-folder-toparent").on("click", function () {
            var folderId = jq(this).attr("data-id");
            ASC.Files.Folders.clickOnFolder(folderId);
            return false;
        });

        jq("#filesMainContent").on("click", ".file-row:not(.folder-row):not(.error-entry) .entry-title .name a, .file-row:not(.folder-row):not(.error-entry) .thumb-file, .file-row:not(.folder-row):not(.error-entry) .thumb-img", function (event) {
            ASC.Files.Actions.hideAllActionPanels();

            if (ASC.Files.Folders.folderContainer == "trash") {
                ASC.Files.UI.clickRow(event, jq(this).closest(".file-row"));
                return false;
            }

            var fileData = ASC.Files.UI.getObjectData(this);
            if (fileData.id != 0) {
                if (fileData.entryObject.hasClass("is-template")) {
                    ASC.Files.Folders.createNewDoc(fileData);
                } else {
                    ASC.Files.Folders.clickOnFile(fileData);
                }
            }
            return false;
        });

        jq("#filesMainContent").on("mouseup", ".file-row:not(.folder-row):not(.error-entry) .entry-title .name a, .file-row:not(.folder-row):not(.error-entry) .thumb-file, .file-row:not(.folder-row):not(.error-entry) .thumb-img", function (event) {
            if (event.which == 2) {
                var fileData = ASC.Files.UI.getObjectData(this);
                if (fileData.id != 0) {
                    var fileTitle = fileData.title || ASC.Files.UI.getEntryTitle("file", fileData.id);
                    if (!ASC.Files.Utility.MustConvert(fileTitle) && (ASC.Files.Utility.CanWebView(fileTitle) || ASC.Files.Utility.CanWebEdit(fileTitle))) {
                        var fileObj = fileData.entryObject;

                        ASC.Files.UI.lockEditFile(fileObj, true);
                        ASC.Files.Socket.subscribeChangeEditors(fileData.id);
                        ASC.Files.UI.checkEditingDefer();

                        ASC.Files.Actions.hideAllActionPanels();
                    }
                }
            }
            return true;
        });

        jq("#filesMainContent").on("click", "#contentVersions", function () {
            return false;
        });

        jq("#filesMainContent").on("click", ".version-comment:not(:has(input)):not(:has(.version-comment-fix:empty)), .version-comment-edit", ASC.Files.Folders.enterComment);

        jq("#filesMainContent").on("click", ".version-complete, .version-continue", function () {
            var entryObject = jq(".display-versions");
            var fileId = ASC.Files.UI.getObjectData(entryObject).id;
            var version = jq(this).closest(".version-row").attr("data-version");
            var continueVersion = jq(this).hasClass("version-continue");

            ASC.Files.Folders.versionComplete(fileId, version, continueVersion);
        });

        jq("#filesMainContent").on("click", ".version-group-head .version-sublist-toggle", function () {
            var versionGroup = jq(this).closest(".version-row").attr("data-version-group");
            ASC.Files.Folders.toggleVersionSublist(versionGroup);
            return false;
        });

        jq("#filesMainContent").on("click", ".version-preview", function () {
            ASC.Files.Actions.hideAllActionPanels();
            var entryObject = jq(".display-versions");
            var fileData = ASC.Files.UI.getObjectData(entryObject);
            var version = jq(this).closest(".version-row").attr("data-version") || 0;
            ASC.Files.Folders.clickOnFile(fileData, false, version);
            return false;
        });

        jq("#filesMainContent").on("click", ".version-download", function () {
            var entryObject = jq(".display-versions");
            var fileId = ASC.Files.UI.getObjectData(entryObject).id;
            var version = jq(this).closest(".version-row").attr("data-version");
            ASC.Files.Folders.download("file", fileId, version);
            return false;
        });

        jq("#filesMainContent").on("click", ".version-restore span", function () {
            var entryObject = jq(".display-versions");
            var fileId = ASC.Files.UI.getObjectData(entryObject).id;
            var version = jq(this).closest(".version-row").attr("data-version");
            ASC.Files.Folders.makeCurrentVersion(fileId, version);
            return false;
        });

        jq("#filesMainContent").on("click", ".version-comment .name-cancel", ASC.Files.Folders.eraseComment);

        jq("#filesMainContent").on("click", ".file-row > .rename-action > .name-cancel", function (event) {
            ASC.Files.Folders.cancelEnter(event, this);
            return false;
        });

        jq(".overwrite-resolve").on("click", "input", function () {
            jq(".overwrite-resolve.selected").removeClass("selected");
            jq(this).closest(".overwrite-resolve").addClass("selected");
        });

        jq("#overwriteListShow").on("click", function () {
            jq("#overwriteList, #overwriteListHide").show();
            jq("#overwriteListShow").hide();
        });

        jq("#overwriteListHide").on("click", function () {
            jq("#overwriteListShow").show();
            jq("#overwriteList, #overwriteListHide").hide();
        });

        jq("#cbxDeleteConfirm").on("change", function () {
            var enable = jq(this).prop("checked") == true;
            ASC.Files.ServiceManager.changeDeleteConfrim(ASC.Files.ServiceManager.events.ChangeDeleteConfrim, { value: enable === true });

            return false;
        });

        jq("#cbxDownloadTarGz").on("change", function () {
            var value = jq(this).prop("checked");
            Teamlab.filesDownloadTarGz(value, {
                success: function (_, data) {
                    jq("#convertFileZip").html(data.title.format("<b>", "</b>"));
                },
                error: function (params, error) {
                    ASC.Files.UI.displayInfoPanel(error[0], true);
                }
            });

            return false;
        });

        jq("#cbxAutomaticallyCleanUp").on("change", function () {
            var value = jq(this).prop("checked");
            var dataValue = jq("#selectGapToAutoCleanUp").val();
            Teamlab.changeAutomaticallyCleanUp(value, value ? dataValue : -1, {
                success: function (_, data) {
                    ASC.Files.UI.autoCleanUpSetting = data;
                    if (data.isAutoCleanUp) {
                        jq("#selectGapToAutoCleanUp").removeClass("disabled");
                        jq("#selectGapToAutoCleanUp").tlcombobox(true);
                    }
                    else {
                        jq("#selectGapToAutoCleanUp").tlcombobox(false);
                    }
                },
                error: function (params, error) {
                    ASC.Files.UI.displayInfoPanel(error[0], true);
                }
            });
            return false;
        });

        jq("#selectGapToAutoCleanUp").on("change", function () {
            jq("#selectGapToAutoCleanUp").tlcombobox();
            var value = jq("#cbxAutomaticallyCleanUp").prop("checked");
            var dataValue = jq("#selectGapToAutoCleanUp").val();
            Teamlab.changeAutomaticallyCleanUp(value, value ? dataValue : -1, {
                success: function (_, data) {
                    ASC.Files.UI.autoCleanUpSetting = data;
                },
                error: function (params, error) {
                    ASC.Files.UI.displayInfoPanel(error[0], true);
                }
            });

            return false;
        });

        jq("#selectGapToAutoCleanUp").tlcombobox();

        jq("#cbxFavorites").on("change", function () {
            var value = jq(this).prop("checked") == true;

            Teamlab.filesDisplayFavorites(value, {
                success: function (_, data) {
                    ASC.Files.Tree.displayFavorites(data === true);
                    jq("#cbxFavorites").prop("checked", ASC.Files.Tree.displayFavorites());
                },
                error: function (params, error) {
                    ASC.Files.UI.displayInfoPanel(error[0], true);
                }
            });

            return false;
        });

        jq("#cbxRecent").on("change", function () {
            var value = jq(this).prop("checked") == true;

            Teamlab.filesDisplayRecent(value, {
                success: function (_, data) {
                    ASC.Files.Tree.displayRecent(data === true);
                    jq("#cbxRecent").prop("checked", ASC.Files.Tree.displayRecent());
                },
                error: function (params, error) {
                    ASC.Files.UI.displayInfoPanel(error[0], true);
                }
            });

            return false;
        });

        jq("#cbxTemplates").on("change", function () {
            var value = jq(this).prop("checked") == true;

            Teamlab.filesDisplayTemplates(value, {
                success: function (_, data) {
                    ASC.Files.Tree.displayTemplates(data === true);
                    ASC.Files.CreateMenu.toggleCreateByTemplate(data === true);

                    jq("#cbxTemplates").prop("checked", ASC.Files.Tree.displayTemplates());
                },
                error: function (params, error) {
                    ASC.Files.UI.displayInfoPanel(error[0], true);
                }
            });

            return false;
        });

        jq("#defaultAccessRightsSetting input").on("change", function () {
            var data = [];
            jq("#defaultAccessRightsSetting input:checked").each(function () {
                data.push(+this.value);
            });

            Teamlab.filesChangeDafaultAccessRightsSetting(data, {
                success: function (_, res) {},
                error: function (_, error) {
                    ASC.Files.UI.displayInfoPanel(error[0], true);
                }
            });

            return false;
        });

        jq(".update-if-exist").on("change", function () {
            ASC.Files.Folders.updateIfExist(this);
        });

        jq("#cbxForcesave").on("change", function () {
            var forcesave = jq(this).prop("checked") == true;
            ASC.Files.ServiceManager.forcesave(ASC.Files.ServiceManager.events.Forcesave, { value: forcesave === true });

            return false;
        });

        jq("#cbxStoreForcesave").on("change", function () {
            var replace = jq(this).prop("checked") == true;
            ASC.Files.ServiceManager.storeForcesave(ASC.Files.ServiceManager.events.StoreForcesave, { value: replace === true });

            return false;
        });

        jq("#cbxExternalShare").on("change", function changeExternalShareSettings() {
            ASC.Files.Actions.hideAllActionPanels();
            var enable = jq(this).prop("checked") === true;
            ASC.Files.ServiceManager.changeExternalShareSettings(ASC.Files.ServiceManager.events.ChangeExternalShareSettings, { enable });
            return false;
        });

        jq("#cbxExternalShareSocialMedia").on("change", function changeExternalShareSocialMediaSettings() {
            ASC.Files.Actions.hideAllActionPanels();
            var enable = jq(this).prop("checked") === true;
            ASC.Files.ServiceManager.changeExternalShareSocialMediaSettings(ASC.Files.ServiceManager.events.ChangeExternalShareSocialMediaSettings, { enable });
            return false;
        });

        if (typeof ASC.Files.Share == "undefined") {
            jq("#files_shareaccess_folders, #filesShareAccess,\
                #filesUnsubscribe, #foldersUnsubscribe").remove();
        }

        jq(window).on("scroll", function () {
            ASC.Files.UI.stickContentHeader();
            ASC.Files.UI.trackShowMore(false);
            return true;
        });

        jq(".mainPageContent").on("scroll", function () {
            ASC.Files.UI.trackShowMore(true);
            return true;
        });

        jq("#filesTemplateList").on("scroll", function () {
            ASC.Files.UI.trackShowMoreTemplates();
        });

        ASC.Files.Folders.eventAfter = function () {
            if (ASC.Files.Folders.isFirstLoad) {
                ASC.Files.Folders.isFirstLoad = false;
                if (!jq("#emptyContainer").is(":visible")) {
                    jq('.advansed-filter').advansedFilter("resize");
                }
            } else {
                LoadingBanner.hideLoading();
            }
        };
        if (!ASC.Files.Folders.isFirstLoad) {
            LoadingBanner.displayLoading();
        }
        if (jq.browser.msie) {
            //fix Flash & IE URL hash problem
            setInterval(function () {
                ASC.Files.UI.setDocumentTitle(ASC.Files.Folders.currentFolder.title);
            }, 200);
        }
    });
})(jQuery);