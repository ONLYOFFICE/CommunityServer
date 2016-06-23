/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
        var filterSettings = ASC.Files.Filter.getFilterSettings(ASC.Files.Folders.currentFolder.id);

        ASC.Files.ServiceManager.getFolderItems(ASC.Files.ServiceManager.events.GetFolderItems,
            {
                folderId: ASC.Files.Folders.currentFolder.id,
                from: (isAppend ? jq("#filesMainContent .file-row[name!=\"addRow\"]").length : 0),
                count: countAppend || ASC.Files.Constants.COUNT_ON_PAGE,
                append: isAppend === true,
                filter: filterSettings.filter,
                subject: filterSettings.subject,
                text: filterSettings.text,
                orderBy: filterSettings.sorter
            }, { orderBy: filterSettings.sorter });
    };

    var getItems = function (dataIds, folderToId) {
        var data = {
            entry: jq(typeof dataIds == "object" ? dataIds : [dataIds]).map(function (i, id) {
                return id;
            }).toArray()
        };

        var filterSettings = ASC.Files.Filter.getFilterSettings(ASC.Files.Folders.currentFolder.id);

        ASC.Files.ServiceManager.getItems(ASC.Files.ServiceManager.events.GetItems,
            {
                parentFolderID: folderToId,
                filter: filterSettings.filter,
                subject: filterSettings.subject,
                text: filterSettings.text,
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
            location.href = "tmdocs.aspx";
            return;
        }

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
        version = version || fileData.version || -1;

        if (ASC.Files.Utility.CanWebView(fileTitle) || ASC.Files.Utility.CanWebEdit(fileTitle)) {
            return ASC.Files.Converter.checkCanOpenEditor(fileId, fileTitle, version, forEdit != false);
        }

        if (typeof ASC.Files.ImageViewer != "undefined" && ASC.Files.Utility.CanImageView(fileTitle)) {
            var hash = ASC.Files.ImageViewer.getPreviewHash(fileId);
            ASC.Files.Anchor.move(hash);
            return true;
        }

        var url = ASC.Files.Utility.GetFileViewUrl(fileId, version);
        window.open(url, "_blank");
        return ASC.Files.Marker.removeNewIcon("file", fileId);
    };

    var updateIfExist = function (target) {
        var value = jq(target).prop("checked");

        ASC.Files.ServiceManager.updateIfExist(ASC.Files.ServiceManager.events.UpdateIfExist, { value: value });
    };

    var download = function (entryType, entryId, version) {
        var list = new Array();
        if (!ASC.Files.Common.isCorrectId(entryId)) {
            list = jq("#filesMainContent .file-row:has(.checkbox input:checked)");
            if (list.length == 0) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.EmptyListSelectedForDownload, true);
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

            window.open(ASC.Files.Utility.GetFileDownloadUrl(entryId, version), "_blank");
            return;
        }

        if (ASC.Files.Folders.bulkStatuses == true) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.ErrorMassage_SecondDownload, true);
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
            "<div class=\"button gray btn-action __apply name-aplly\" title=" + ASC.Files.FilesJSResources.TitleCreate + "></div>" +
            "<div class=\"button gray btn-action __reset name-cancel\" title=" + ASC.Files.FilesJSResources.TitleCancel + "></div>" +
            "</div>";
    };

    var cancelEnter = function (event, cancelButton) {
        var entryObject = jq(cancelButton || this).closest(".file-row");
        if (!entryObject.attr("name")) {
            entryObject
                .removeClass("row-rename")
                .find("#promptRename, .rename-action").remove();
        } else {
            entryObject.remove();
            if (jq("#filesMainContent .file-row").length == 0) {
                ASC.Files.EmptyScreen.displayEmptyScreen();
            }
        }
    };

    var createFolder = function () {
        if (ASC.Files.ImageViewer && ASC.Files.ImageViewer.isView()) {
            return;
        }

        if (!ASC.Files.UI.accessEdit() || ASC.Files.UI.isSettingsPanel()) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.ErrorMassage_SecurityException, true);
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
                    title: ASC.Files.FilesJSResources.TitleNewFolder,
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
        newFolderObj.find(".thumb-folder").addClass(ftClass);

        var newContainer = document.createElement("input");
        newContainer.id = "promptCreateFolder";
        newContainer.type = "text";
        newContainer.style.display = "none";
        document.body.appendChild(newContainer);

        newContainer = jq("#promptCreateFolder");
        newContainer.attr("maxlength", ASC.Files.Constants.MAX_NAME_LENGTH);
        newContainer.addClass("textEdit input-rename");
        newContainer.val(ASC.Files.FilesJSResources.TitleNewFolder);
        newContainer.insertAfter(obj);
        newContainer.show();
        obj.hide();
        newContainer.focus();
        if (!jq.browser.mobile) {
            newContainer.select();
        }

        ASC.Files.UI.checkCharacter(newContainer);

        var saveFolder = function (event, saveButton) {
            var newFolderSaveObj = jq(saveButton || this).closest(".file-row");

            var newName = ASC.Files.Common.replaceSpecCharacter(jq("#promptCreateFolder").val().trim());
            if (newName == "" || newName == null) {
                newName = ASC.Files.FilesJSResources.TitleNewFolder;
            }

            newFolderSaveObj.find(".entry-title .name a").show().text(newName);
            newFolderSaveObj.removeClass("row-rename");
            jq("#promptCreateFolder").remove();
            newFolderSaveObj.find(".rename-action").remove();

            var params = { parentFolderID: ASC.Files.Folders.currentFolder.id, title: newName };

            ASC.Files.UI.blockObject(newFolderSaveObj, true, ASC.Files.FilesJSResources.DescriptCreate);
            ASC.Files.ServiceManager.createFolder(ASC.Files.ServiceManager.events.CreateFolder, params);
        };

        newFolderObj.append(getActionHtml());
        newFolderObj.find(".name-aplly").click(saveFolder);

        jq("#promptCreateFolder").bind(jq.browser.webkit || jq.browser.msie ? "keydown" : "keypress", function (event) {
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

    var createNewDoc = function () {
        if (ASC.Files.ImageViewer && ASC.Files.ImageViewer.isView()) {
            return;
        }

        if (!ASC.Files.UI.accessEdit() || ASC.Files.UI.isSettingsPanel()) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.ErrorMassage_SecurityException, true);
            return;
        }

        jq(document).scrollTop(0);

        var newFileObj = ASC.Files.UI.getEntryObject("file", "0");
        newFileObj.remove();

        var titleNewDoc;
        switch (ASC.Files.Folders.typeNewDoc) {
            case "document":
                titleNewDoc = ASC.Files.FilesJSResources.TitleNewFileText + ASC.Files.Utility.Resource.InternalFormats.Document;
                break;
            case "spreadsheet":
                titleNewDoc = ASC.Files.FilesJSResources.TitleNewFileSpreadsheet + ASC.Files.Utility.Resource.InternalFormats.Spreadsheet;
                break;
            case "presentation":
                titleNewDoc = ASC.Files.FilesJSResources.TitleNewFilePresentation + ASC.Files.Utility.Resource.InternalFormats.Presentation;
                break;
            default:
                return;
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
        newFileObj.addClass("row-rename");

        var obj = newFileObj.find(".entry-title .name a");

        var ftClass = ASC.Files.Utility.getCssClassByFileTitle(titleNewDoc);
        newFileObj.find(".thumb-file").addClass(ftClass);

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
        newContainer.show().focus();
        if (!jq.browser.mobile) {
            newContainer.select();
        }

        ASC.Files.UI.checkCharacter(newContainer);

        var saveFile = function (event, saveButton) {
            var newFileSaveObj = jq(saveButton || this).closest(".file-row");

            var newName = ASC.Files.Common.replaceSpecCharacter(jq("#promptCreateFile").val().trim());
            var oldName = ASC.Files.UI.getObjectTitle(newFileSaveObj);
            if (newName == "" || newName == null) {
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

            var params =
                {
                    folderID: ASC.Files.Folders.currentFolder.id,
                    fileTitle: newName,
                    winEditor: window.open("")
                };

            ASC.Files.UI.blockObject(newFileObj, true, ASC.Files.FilesJSResources.DescriptCreate);
            ASC.Files.ServiceManager.createNewFile(ASC.Files.ServiceManager.events.CreateNewFile, params);
        };

        newFileObj.append(getActionHtml());
        newFileObj.find(".name-aplly").click(saveFile);

        jq("#promptCreateFile").bind(jq.browser.webkit || jq.browser.msie ? "keydown" : "keypress", function (event) {
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
                case ASC.Files.Common.keyCode.enter:
                    saveFile(e, this);
                    break;
            }
        });
    };

    var rename = function (entryType, entryId) {
        var entryObj = ASC.Files.UI.getEntryObject(entryType, entryId);

        var lenExt = 0;

        if (jq("#promptRename").length != 0) {
            jq("#filesMainContent .file-row.row-rename .name-cancel").click();
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
        newContainer.show().focus();
        if (!jq.browser.mobile) {
            newContainer.select();
        }

        ASC.Files.UI.checkCharacter(newContainer);

        var saveRename = function () {
            var entryRenameData = ASC.Files.UI.getObjectData(jq("#promptRename"));
            var entryRenameObj = entryRenameData.entryObject;
            var entryRenameType = entryRenameData.entryType;
            var entryRenameId = entryRenameData.entryId;
            var oldName = ASC.Files.UI.getObjectTitle(entryRenameObj);

            var newName = ASC.Files.Common.replaceSpecCharacter(jq("#promptRename").val().trim());
            if (newName == "" || newName == null) {
                return;
            }

            entryRenameObj.removeClass("row-rename");
            entryRenameObj.find(".rename-action").remove();
            jq("#promptRename").remove();

            if (entryRenameType == "file") {
                var lenExtRename = ASC.Files.Utility.GetFileExtension(oldName).length;
                newName += oldName.substring(oldName.length - lenExtRename);
            }

            if (newName != oldName) {
                entryRenameObj.find(".entry-title .name a").show().text(newName);

                if (entryRenameType == "file") {
                    var rowLink = entryRenameObj.find(".entry-title .name a");
                    ASC.Files.UI.highlightExtension(rowLink, newName);
                }

                ASC.Files.UI.blockObject(entryRenameObj, true, ASC.Files.FilesJSResources.DescriptRename);

                if (entryRenameType == "file") {
                    ASC.Files.ServiceManager.renameFile(ASC.Files.ServiceManager.events.FileRename, { fileId: entryRenameId, name: oldName, newname: newName, show: true });
                } else {
                    ASC.Files.ServiceManager.renameFolder(ASC.Files.ServiceManager.events.FolderRename, { parentFolderID: ASC.Files.Folders.currentFolder.id, folderId: entryRenameId, name: oldName, newname: newName });
                }
            }
        };

        entryObj.append(getActionHtml());
        entryObj.find(".name-aplly").click(saveRename);

        entryObj.removeClass("row-selected");

        jq("#promptRename").bind(jq.browser.webkit || jq.browser.msie ? "keydown" : "keypress", function (event) {
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

        if (ASC.Files.UI.lockedForMe(fileObj) && !ASC.Files.Constants.ADMIN) {
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
            var close = fileObj.find("#contentVersions").length != 0;
            ASC.Files.Folders.closeVersions();
            if (close) {
                return;
            }
        }

        ASC.Files.UI.blockObject(fileObj, true, ASC.Files.FilesJSResources.DescriptLoadVersion);

        fileId = fileId || ASC.Files.UI.getObjectData(fileObj).entryId;
        ASC.Files.ServiceManager.getFileHistory(ASC.Files.ServiceManager.events.GetFileHistory, { fileId: fileId });
    };

    var toggleVersionSublist = function (versionGroup) {
        ASC.Files.Actions.hideAllActionPanels();
        jq("#contentVersions .version-row[data-version-group='" + versionGroup + "']:not(.version-group-head)").toggle();
    };

    var makeCurrentVersion = function (fileId, version) {
        jq(".version-operation").css("visibility", "hidden");
        ASC.Files.UI.blockObjectById("file", fileId, true, ASC.Files.FilesJSResources.DescriptSetVersion);
        ASC.Files.ServiceManager.setCurrentVersion(ASC.Files.ServiceManager.events.SetCurrentVersion, { fileId: fileId, version: version });
    };

    var versionComplete = function (fileId, version, continueVersion) {
        ASC.Files.Actions.hideAllActionPanels();

        jq(".version-operation").css("visibility", "hidden");
        ASC.Files.UI.blockObjectById("file", fileId, true, ASC.Files.FilesJSResources.DescriptCompleteVersion);
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
        newContainer.attr("placeholder", ASC.Files.FilesJSResources.EnterComment);
        newContainer.addClass("textEdit input-rename");
        newContainer.val(comment);
        commentObj.append(newContainer);
        newContainer.show();
        newContainer.focus();
        if (!jq.browser.mobile) {
            newContainer.select();
        }

        var saveComment = function () {
            var editor = jq("#promptVersionComment");
            commentObj = editor.closest(".version-comment");
            comment = commentObj.attr("data-comment");
            var fileId = ASC.Files.UI.getObjectData(commentObj).id;
            var version = commentObj.closest(".version-row").attr("data-version");
            var newComment = editor.val().trim();

            if (newComment != comment) {
                ASC.Files.ServiceManager.updateComment(ASC.Files.ServiceManager.events.UpdateComment, { fileId: fileId, version: version, comment: newComment });
            }
            ASC.Files.Folders.eraseComment();
        };

        commentObj.append(getActionHtml());
        commentObj.find(".name-aplly").click(saveComment);
        jq("#promptVersionComment").bind(jq.browser.webkit || jq.browser.msie ? "keydown" : "keypress", function (event) {
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
            message = "<b>" + ASC.Files.FilesJSResources.FilesAlreadyExist.format(data.length, folderTitle) + "</b>";
            jq("#overwriteList").html(files).show();
        } else {
            jq("#overwriteList").hide();
            message = ASC.Files.FilesJSResources.FileAlreadyExist.format("<span title=\"" + data[0].Value + "\">" + data[0].Value + "</span>", "<b>" + folderTitle + "</b>");
        }

        jq("#overwriteMessage").html(message);

        jq("#buttonOverwrite").unbind("click").click(function () {
            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();
            ASC.Files.ServiceManager.moveItems(ASC.Files.ServiceManager.events.MoveItems,
                {
                    folderToId: folderId,
                    resolve: 1,
                    isCopyOperation: (isCopyOperation == true),
                    doNow: true
                },
                { stringList: listData });
        });

        jq("#buttonSkipOverwrite").unbind("click").click(function () {

            for (i = 0; i < data.length; i++) {
                ASC.Files.UI.blockObjectById("file", data[i].Key, false, null, true);
                var pos = jq.inArray("file_" + data[i].Key, listData.entry);
                if (pos != -1) {
                    listData.entry.splice(pos, 1);
                }
            }
            ASC.Files.UI.updateMainContentHeader();

            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();
            ASC.Files.ServiceManager.moveItems(ASC.Files.ServiceManager.events.MoveItems,
                {
                    folderToId: folderId,
                    resolve: 0,
                    isCopyOperation: (isCopyOperation == true),
                    doNow: true
                },
                { stringList: listData });
        });

        jq("#buttonCancelOverwrite").unbind("click").click(function () {
            for (i = 0; i < listData.entry.length; i++) {
                var itemId = ASC.Files.UI.parseItemId(listData.entry[i]);
                ASC.Files.UI.blockObjectById(itemId.entryType, itemId.entryId, false, null, true);
            }
            ASC.Files.UI.updateMainContentHeader();

            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();
        });

        ASC.Files.UI.blockUI("#confirmOverwriteFiles", 420, 300);

        PopupKeyUpActionProvider.EnterAction = "jq(\"#buttonOverwrite\").click();";
        PopupKeyUpActionProvider.CloseDialogAction = "jq(\"#buttonCancelOverwrite\").click();";
    };

    var curItemFolderMoveTo = function (folderToId, folderToTitle, pathDest, confirmedThirdParty) {
        if (folderToId === ASC.Files.Folders.currentFolder.entryId) {
            ASC.Files.Actions.hideAllActionPanels();
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
                        ASC.Files.UI.displayInfoPanel(((ASC.Files.Folders.isCopyTo == true) ? ASC.Files.FilesJSResources.InfoFolderCopyError : ASC.Files.FilesJSResources.InfoFolderMoveError), true);
                    } else {
                        if (takeThirdParty
                            || !thirdParty
                            || !ASC.Files.ThirdParty.isThirdParty(entryData)) {
                            ASC.Files.UI.blockObject(entryObj,
                                true,
                                (ASC.Files.Folders.isCopyTo == true) ?
                                    ASC.Files.FilesJSResources.DescriptCopy :
                                    ASC.Files.FilesJSResources.DescriptMove,
                                true);

                            data.entry.push(entryType + "_" + entryId);
                        }
                    }
                }
            }
        });

        if (moveAccessDeny) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoMoveAccessError, true);
        }

        ASC.Files.UI.updateMainContentHeader();
        ASC.Files.Actions.hideAllActionPanels();

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
            || jq("#pageNavigatorHolder a").text() == ASC.Files.FilesJSResources.ButtonShowMoreLoad) {
            return;
        }

        jq("#pageNavigatorHolder a").text(ASC.Files.FilesJSResources.ButtonShowMoreLoad);

        ASC.Files.Folders.getFolderItems(true);
    };

    var emptyTrash = function () {
        if (ASC.Files.Folders.folderContainer != "trash") {
            return;
        }

        ASC.Files.Actions.hideAllActionPanels();

        ASC.Files.UI.checkSelectAll(true);

        jq("#confirmRemoveText").html("<b>" + ASC.Files.FilesJSResources.ConfirmEmptyTrash + "</b>");
        jq("#confirmRemoveList").hide();
        jq("#confirmRemoveTextDescription").show();

        jq("#removeConfirmBtn").unbind("click").click(function () {
            PopupKeyUpActionProvider.CloseDialog();

            ASC.Files.ServiceManager.emptyTrash(ASC.Files.ServiceManager.events.EmptyTrash, { doNow: true });
        });

        ASC.Files.UI.blockUI("#confirmRemove", 420, 0, -150);
        PopupKeyUpActionProvider.EnterAction = "jq(\"#removeConfirmBtn\").click();";
    };

    var deleteItem = function (entryType, entryId) {
        ASC.Files.Actions.hideAllActionPanels();

        var caption = ASC.Files.FilesJSResources.ConfirmRemoveList;
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
            if (list[0].entryType == "file") {
                caption = ASC.Files.FilesJSResources.ConfirmRemoveFile;
            } else {
                caption = ASC.Files.FilesJSResources.ConfirmRemoveFolder;
            }
        }

        var textFolder = "";
        var textFile = "";
        var strHtml = "<label title=\"{0}\"><input type=\"checkbox\" class=\"checkbox\" entryType=\"{1}\" entryId=\"{2}\" checked=\"checked\">{0}</label>";
        for (var i = 0; i < list.length; i++) {
            var entryRowTitle = ASC.Files.UI.getEntryTitle(list[i].entryType, list[i].entryId);
            if (list[i].entryType == "file") {
                textFile += strHtml.format(entryRowTitle, list[i].entryType, list[i].entryId);
            } else {
                textFolder += strHtml.format(entryRowTitle, list[i].entryType, list[i].entryId);
            }
        }

        jq("#confirmRemoveText").html("<b>" + caption + "</b>");
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
            jq("#confirmRemoveList .confirm-remove-folders-count").text(jq("#confirmRemoveList dd.confirm-remove-folders :checked").length);
            jq("#confirmRemoveList .confirm-remove-files-count").text(jq("#confirmRemoveList dd.confirm-remove-files :checked").length);
        };
        checkRemoveItem();
        jq("#confirmRemoveList dd [type='checkbox']").change(checkRemoveItem);

        if (ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty()) {
            jq("#confirmRemoveSharpBoxTextDescription").show();
        } else {
            jq("#confirmRemoveSharpBoxTextDescription").hide();
        }

        if (ASC.Files.Folders.folderContainer == "trash") {
            jq("#confirmRemoveTextDescription").show();
        } else {
            jq("#confirmRemoveTextDescription").hide();
        }

        jq("#removeConfirmBtn").unbind("click").click(function () {
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
                ASC.Files.UI.blockObject(entryConfirmObj, true, ASC.Files.FilesJSResources.DescriptRemove, true);
                return entryConfirmType + "_" + entryConfirmId;
            }).toArray();

            ASC.Files.UI.updateMainContentHeader();
            ASC.Files.ServiceManager.deleteItem(ASC.Files.ServiceManager.events.DeleteItem, { list: data.entry, doNow: true }, { stringList: data });
        });

        ASC.Files.UI.blockUI("#confirmRemove", 420, 0, -150);

        PopupKeyUpActionProvider.EnterAction = "jq(\"#removeConfirmBtn\").click();";
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
        curItemFolderMoveTo: curItemFolderMoveTo,
        createDuplicate: createDuplicate,

        rename: rename,
        deleteItem: deleteItem,
        emptyTrash: emptyTrash,

        getFolderItems: getFolderItems,
        getItems: getItems,

        clickOnFolder: clickOnFolder,
        clickOnFile: clickOnFile,

        updateIfExist: updateIfExist,

        showMore: showMore,

        createNewDoc: createNewDoc,
        typeNewDoc: typeNewDoc,

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

        jq("#pageNavigatorHolder a").click(function () {
            ASC.Files.Folders.showMore();
            return false;
        });

        jq("#topNewFolder a").click(function () {
            ASC.Files.Folders.createFolder();
        });

        jq("#buttonDelete, #mainDelete").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.deleteItem();
        });

        jq("#buttonEmptyTrash, #mainEmptyTrash").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.emptyTrash();
        });

        jq("#studioPageContent").on("click", "#buttonDownload, #mainDownload.unlockAction", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.download();
        });

        jq("#filesSelectAllCheck").click(function () {
            ASC.Files.UI.checkSelectAll(jq("#filesSelectAllCheck").prop("checked") == true);
            jq(this).blur();
        });

        jq("#filesMainContent").on("click", ".file-lock", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.lockFile(this);
            return false;
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

        jq("#filesMainContent").on("click", ".folder-row:not(.error-entry) .entry-title .name a, .folder-row:not(.error-entry) .thumb-folder", function (event) {
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

        jq("#filesMainContent").on("click", ".file-row:not(.folder-row):not(.error-entry) .entry-title .name a, .file-row:not(.folder-row):not(.error-entry) .thumb-file", function (event) {
            ASC.Files.Actions.hideAllActionPanels();

            if (ASC.Files.Folders.folderContainer == "trash") {
                ASC.Files.UI.clickRow(event, jq(this).closest(".file-row"));
                return false;
            }

            var fileData = ASC.Files.UI.getObjectData(this);
            if (fileData.id != 0) {
                var fileObj = fileData.entryObject;
                ASC.Files.Folders.clickOnFile(fileData);
                fileObj.removeClass("isNewForWebEditor");
            }
            return false;
        });

        jq("#filesMainContent").on("mouseup", ".file-row:not(.folder-row):not(.error-entry) .entry-title .name a, .file-row:not(.folder-row):not(.error-entry) .thumb-file", function (event) {
            if (event.which == 2) {
                var fileData = ASC.Files.UI.getObjectData(this);
                if (fileData.id != 0) {
                    var fileTitle = fileData.title || ASC.Files.UI.getEntryTitle("file", fileId);
                    if (!ASC.Files.Utility.MustConvert(fileTitle) && (ASC.Files.Utility.CanWebView(fileTitle) || ASC.Files.Utility.CanWebEdit(fileTitle))) {
                        var fileObj = fileData.entryObject;

                        ASC.Files.UI.lockEditFile(fileObj, true);
                        setTimeout(ASC.Files.UI.checkEditing, 5000);
                        
                        ASC.Files.Actions.hideAllActionPanels();
                        fileObj.removeClass("isNewForWebEditor");
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
            var fileId = ASC.Files.UI.getObjectData(this).id;
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
            var fileData = ASC.Files.UI.getObjectData(this);
            var version = jq(this).closest(".version-row").attr("data-version");
            ASC.Files.Folders.clickOnFile(fileData, false, version);
            return false;
        });

        jq("#filesMainContent").on("click", ".version-download", function () {
            var fileId = ASC.Files.UI.getObjectData(this).id;
            var version = jq(this).closest(".version-row").attr("data-version");
            ASC.Files.Folders.download("file", fileId, version);
            return false;
        });

        jq("#filesMainContent").on("click", ".version-restore span", function () {
            var fileId = ASC.Files.UI.getObjectData(this).id;
            var version = jq(this).closest(".version-row").attr("data-version");
            ASC.Files.Folders.makeCurrentVersion(fileId, version);
            return false;
        });

        jq("#filesMainContent").on("click", ".version-comment .name-cancel", ASC.Files.Folders.eraseComment);

        jq("#filesMainContent").on("click", ".file-row > .rename-action > .name-cancel", ASC.Files.Folders.cancelEnter);

        jq(".update-if-exist").change(function () {
            ASC.Files.Folders.updateIfExist(this);
        });

        if (typeof ASC.Files.Share == "undefined") {
            jq("#files_shareaccess_folders, #filesShareAccess,\
                #filesUnsubscribe, #foldersUnsubscribe").remove();
        }

        jq(window).scroll(function () {
            ASC.Files.UI.stickContentHeader();
            if (jq(document).height() - jq(window).height() <= jq(window).scrollTop() + 350) {
                ASC.Files.Folders.showMore();
            }

            return true;
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
            LoadingBanner.displayLoading(true);
        }
        if (jq.browser.msie) {
            //fix Flash & IE URL hash problem
            setInterval(function () {
                ASC.Files.UI.setDocumentTitle(ASC.Files.Folders.currentFolder.title);
            }, 200);
        }
    });
})(jQuery);