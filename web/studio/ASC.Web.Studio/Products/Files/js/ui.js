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


window.ASC.Files.UI = (function () {
    var isInit = false;

    var canSetDocumentTitle = true;

    var timeCheckEditing = null;
    var timeTooltip = null;

    var currentPage = 0;
    var amountPage = 0;
    var countTotal = 0;

    var lastSelectedEntry = null;

    var filesUserProfileInfo = new PopupBox("pb_filesUserProfileInfo", 320, 140, "tintLight", "borderBaseShadow", "",
        {
            apiMethodName: "Teamlab.getProfile",
            tmplName: "userProfileCardTmpl"
        });

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        jq(window).resize(function () {
            fixContentHeaderLeft();
            ASC.Files.UI.fixContentHeaderWidth();
        });

        jq(window).bind("resizeWinTimerWithMaxDelay", function (event) {
            fixContentHeaderLeft();
            ASC.Files.UI.fixContentHeaderWidth();
        });
    };

    var filesSelectedHandler = function () {
    };

    var getEntryObject = function (entryType, entryId) {
        if (entryType && entryId) {
            if (!ASC.Files.Common.isCorrectId(entryId)) {
                return null;
            }
            return jq("#filesMainContent .file-row" + ASC.Files.UI.getSelectorId(entryType + "_" + entryId));
        }
        return null;
    };

    var getSelectorId = function (id) {
        return "[data-id=\"" + (id + "").replace(/\\/g, "\\\\").replace(/\"/g, "\\\"") + "\"]";
    };

    var getObjectData = function (entryObject) {
        entryObject = jq(entryObject);
        var selectorData = "input:hidden[name=\"entry_data\"]";

        if (!entryObject.is(".file-row")) {
            var selectorObject = ".file-row";
            if (entryObject.is(".jstree *")) {
                selectorObject = ".tree-node";
                selectorData = "> a input:hidden[name=\"entry_data\"]";
            }
            entryObject = entryObject.closest(selectorObject);
        }

        if (entryObject.length == 0) {
            return null;
        }

        var dataObject = entryObject.find(selectorData);
        if (!dataObject.length) {
            return null;
        }

        var result = {};
        result.id = result.entryId = dataObject.attr("data-id");
        if (!ASC.Files.Common.isCorrectId(result.id)) {
            return null;
        }
        result.entryObject = entryObject;

        result.access = dataObject.attr("data-access");
        result.comment = dataObject.attr("data-comment");
        result.content_length = dataObject.attr("data-content_length");
        result.content_length_string = dataObject.attr("data-content_length_string");
        result.create_by = dataObject.attr("data-create_by");
        result.create_by_id = dataObject.attr("data-create_by_id");
        result.create_on = dataObject.attr("data-create_on");
        result.entryType = (dataObject.attr("data-entryType") || "").trim() === "file" ? "file" : "folder";
        result.error = dataObject.attr("data-error");
        result.error = (result.error != "" ? result.error : false);
        result.file_status = dataObject.attr("data-file_status");
        result.isnew = dataObject.attr("data-isnew") | 0;
        result.modified_by = dataObject.attr("data-modified_by");
        result.modified_on = dataObject.attr("data-modified_on");
        result.provider_id = dataObject.attr("data-provider_id") | 0;
        result.provider_key = dataObject.attr("data-provider_key");
        result.shared = dataObject.attr("data-shared") === "true";
        result.title = (dataObject.attr("data-title") || "").trim();
        result.version = dataObject.attr("data-version") | 0;
        result.version_group = dataObject.attr("data-version_group") | 0;
        result.folderUrl = dataObject.attr("data-folder_url") ;
        result.folder_id = dataObject.attr("data-folder_id");
        result.encrypted = dataObject.attr("data-encrypted") === "true";

        return result;
    };

    var parseItemId = function (itemId) {
        if (typeof itemId == "undefined" || itemId == null) {
            return null;
        }

        var entryType = itemId.indexOf("file_") == "0" ? "file" : "folder";
        var entryId = itemId.substring((entryType + "_").length);

        if (!ASC.Files.Common.isCorrectId(entryId)) {
            return null;
        }
        return { entryType: entryType, entryId: entryId };
    };

    var getEntryTitle = function (entryType, entryId) {
        if (!ASC.Files.Common.isCorrectId(entryId)) {
            return null;
        }
        return ASC.Files.UI.getObjectTitle(ASC.Files.UI.getEntryObject(entryType, entryId));
    };

    var getObjectTitle = function (entryObject) {
        entryObject = jq(entryObject);
        if (!entryObject.is(".file-row")) {
            entryObject = entryObject.closest(".file-row");
        }
        if (entryObject.length == 0) {
            return null;
        }

        return ASC.Files.UI.getObjectData(entryObject).title.trim();
    };

    var getEntryLink = function (entryType, entryId, entryTitle) {
        if (entryType == "file") {
            var entryUrl = ASC.Files.Utility.GetFileDownloadUrl(entryId);

            if (ASC.Files.Utility.CanWebEdit(entryTitle)
                && !ASC.Files.Utility.MustConvert(entryTitle)) {
                entryUrl = ASC.Files.Utility.GetFileWebEditorUrl(entryId);
            } else if (ASC.Files.Utility.CanWebView(entryTitle)) {
                entryUrl = ASC.Files.Utility.GetFileWebViewerUrl(entryId);
            } else if (typeof ASC.Files.ImageViewer != "undefined" && ASC.Files.Utility.CanImageView(entryTitle)) {
                entryUrl = "#" + ASC.Files.Common.getCorrectHash(ASC.Files.ImageViewer.getPreviewHash(entryId));
            } else if (typeof ASC.Files.MediaPlayer != "undefined" && ASC.Files.MediaPlayer.canPlay(entryTitle, true)) {
                entryUrl = "#" + ASC.Files.Common.getCorrectHash(ASC.Files.MediaPlayer.getPlayHash(entryId));
            }
        } else {
            entryUrl = ASC.Files.Constants.URL_BASE + "#" + ASC.Files.Common.getCorrectHash(entryId);
        }

        return entryUrl;
    };

    var updateFolderView = function () {
        if (!ASC.Files.Common.isCorrectId(ASC.Files.Folders.currentFolder.id)) {
            ASC.Files.Anchor.move("");
            return;
        }

        ASC.Files.UI.hideAllContent(true);

        ASC.Files.UI.stickContentHeader();

        ASC.Files.Folders.getFolderItems(false);
    };

    var switchFolderView = function (toCompact) {
        var storageKey = ASC.Files.Constants.storageKeyCompactView;
        if (typeof toCompact == "undefined") {
            toCompact = localStorageManager.getItem(storageKey) === true;
        }
        localStorageManager.setItem(storageKey, toCompact);

        jq("#switchViewFolder").toggleClass("compact", toCompact === true);
        jq("#filesMainContent").toggleClass("compact", toCompact === true);
        if (ASC.Files.Folders.showMore && toCompact !== true) {
            ASC.Files.UI.trackShowMore();
        }

        if (ASC.Files.Mouse) {
            ASC.Files.Mouse.updateMainContentArea();
        }
    };

    var isSettingsPanel = function () {
        return jq("#settingCommon, #settingAdmin, #settingThirdPartyPanel, #helpPanel").is(":visible");
    };

    var fixContentHeaderWidth = function () {
        var headerFixed = jq("#mainContentHeader.stick-panel:visible");
        headerFixed.css("width",
            headerFixed.parent().innerWidth()
                - parseInt(headerFixed.css("margin-left"))
                - parseInt(headerFixed.css("margin-right")));
    };

    var fixContentHeaderLeft = function () {
        var headerFixed = jq("#mainContentHeader.stick-panel:visible"),
            jqWindow = jq(window);
        if (headerFixed.length == 0) return;

        headerFixed.css("left", (headerFixed.parent().offset().left - jqWindow.scrollLeft()));
    };

    var stickContentHeader = function () {
        var boxTop = jq("#mainContentHeader .down_arrow");
        if (!boxTop.length) {
            return;
        }

        ASC.Files.UI.stickMovingPanel("mainContentHeader", jq("#filesSelectorPanel:visible"));

        ASC.Files.UI.fixContentHeaderWidth();
    };

    var stickMovingPanel = function (toggleObjId, hideObj) {
        var toggleObj = jq("#" + toggleObjId);
        if (!toggleObj.is(":visible")) {
            return;
        }

        if (hideObj) {
            hideObj.hide();
        }

        var spacerName = toggleObjId + "Spacer";
        var toggleObjSpacer = jq("#" + spacerName);

        if (jq("#" + spacerName + ":visible").length == 0) {
            var absTop = toggleObj.offset().top;
        } else {
            absTop = toggleObjSpacer.offset().top;
        }

        var jqWindow = jq(window);
        var winScroll = jqWindow.scrollTop();

        if (winScroll >= absTop) {
            if (!toggleObj.hasClass("stick-panel")) {
                if (toggleObjSpacer.length == 0) {
                    var createToggleObjSpacer = document.createElement("div");
                    createToggleObjSpacer.id = spacerName;
                    document.body.appendChild(createToggleObjSpacer);
                    toggleObjSpacer = jq("#" + spacerName);
                    toggleObjSpacer.insertAfter(toggleObj);
                    toggleObjSpacer.css(
                        {
                            "height": toggleObj.css("height"),
                            "padding-bottom": toggleObj.css("padding-bottom"),
                            "padding-top": toggleObj.css("padding-top")
                        });
                }
                toggleObjSpacer.show();

                toggleObj.addClass("stick-panel");
            }

            toggleObj.css("left", (toggleObj.parent().offset().left - jqWindow.scrollLeft()));
        } else {
            if (toggleObj.hasClass("stick-panel")) {
                toggleObjSpacer.hide();
                toggleObj.removeClass("stick-panel");
                jq("#mainContentHeader").css("width", "auto");
            }
        }
    };

    var trackShowMore = function(wideScreen) {
        if (typeof(wideScreen) == "undefined") {
            wideScreen = jq(window).width() >= 1200;
        }
        if (wideScreen) {
            if (jq(".files-content-panel").height() - jq(".mainPageContent").height() <= jq(".mainPageContent").scrollTop() + 350) {
                ASC.Files.Folders.showMore();
            }
        } else {
            if (jq(document).height() - jq(window).height() <= jq(window).scrollTop() + 350) {
                ASC.Files.Folders.showMore();
            }
        }
    };

    var trackShowMoreTemplates = function () {
        var listTemplate = jq("#filesTemplateList .file-row");

        if (listTemplate.eq(listTemplate.length - 1).position().top == 271) {
            ASC.Files.Folders.getTemplateList(true);
        }
    }

    var blockObjectById = function (entryType, entryId, value, message, incycle) {
        return ASC.Files.UI.blockObject(ASC.Files.UI.getEntryObject(entryType, entryId), value, message, incycle);
    };

    var blockObject = function (entryObj, value, message, incycle) {
        value = value === true;
        entryObj = jq(entryObj);
        if (!entryObj.length
            || entryObj.hasClass("checkloading") == value) {
            return;
        }

        jq(entryObj).removeClass("row-hover");

        entryObj.toggleClass("loading checkloading", value === true);
        if (value === true) {
            entryObj.block({ message: "", baseZ: 99 });
            if (typeof message != "undefined" && message) {
                entryObj.children("div.blockUI.blockOverlay").attr("title", message);
            }
        } else {
            entryObj.unblock();
            entryObj.css("position", "static");
        }

        var selectedNow = entryObj.hasClass("row-selected");
        var selectedBefore = entryObj.attr("data-selected") == "true";
        ASC.Files.UI.selectRow(entryObj, !value && selectedBefore);
        if (value) {
            entryObj.attr("data-selected", selectedNow);
        }

        if (!incycle) {
            ASC.Files.UI.updateMainContentHeader();
        }
    };

    var editingFile = function (entryObj) {
        return !entryObj.hasClass("folder-row") && entryObj.hasClass("on-edit");
    };

    var lockedForMe = function (entryObj) {
        return entryObj.hasClass("file-locked-by");
    };

    var editableFile = function (fileData) {
        var fileObj = fileData.entryObject;
        var fileType = fileData.entryType;
        var title = fileData.title;

        return fileType == "file"
            && ASC.Files.Folders.folderContainer != "trash"
            && !fileObj.hasClass("row-rename")
            && ASC.Files.UI.accessEdit(fileData, fileObj, true)
            && ASC.Files.Utility.CanWebEdit(title);
    };

    var highlightExtension = function (rowLink, entryTitle) {
        var fileExt = ASC.Files.Utility.GetFileExtension(entryTitle);
        var entrySplitTitle = entryTitle.substring(0, entryTitle.length - fileExt.length);
        rowLink.html("{0}<span class=\"file-extension\">{1}</span>".format(entrySplitTitle, fileExt));
    };

    var addRowHandlers = function (entryObject) {
        var listEntry = (entryObject || jq("#filesMainContent .file-row"));

        listEntry.each(function () {
            var entryData = ASC.Files.UI.getObjectData(this);

            var entryId = entryData.entryId;
            var entryType = entryData.entryType;
            var entryObj = entryData.entryObject;
            var entryTitle = entryData.title;

            if (ASC.Files.Folders.folderContainer == "trash") {
                entryObj.find(".file-lock").remove();
                entryObj.find(".template-action").remove();
                entryObj.find(".entry-descr .title-created").remove();
                entryObj.find(".entry-descr .title-removed").show();
                if (entryType == "folder") {
                    entryObj.find(".create-date").remove();
                    entryObj.find(".modified-date").show();
                }
            } else {
                entryObj.find(".entry-descr .title-removed").remove();
                if (entryType == "folder") {
                    entryObj.find(".modified-date").remove();
                }
            }
            var rowLink = entryObj.find(".entry-title .name a");

            var ftClass = (entryType == "file" ? ASC.Files.Utility.getCssClassByFileTitle(entryTitle) : ASC.Files.Utility.getFolderCssClass());
            entryObj.find(".thumb-" + entryType).addClass(ftClass);

            if (!entryObj.hasClass("checkloading")) {

                if (entryType == "file") {
                    if (rowLink.is(":not(:has(.file-extension))")) {
                        ASC.Files.UI.highlightExtension(rowLink, entryTitle);
                    }

                    if (ASC.Files.Folders.folderContainer != "trash") {
                        var entryUrl = ASC.Files.UI.getEntryLink(entryType, entryId, entryTitle);
                        rowLink.attr("href", entryUrl).attr("target", "_blank");

                        var lockObj = entryObj.find(".file-lock");
                        if (ASC.Files.UI.accessEdit(entryData, entryObj)) {
                            if (entryObj.hasClass("file-locked")) {
                                var lockHint = ASC.Files.FilesJSResources.TitleLockedFile;
                                if (ASC.Files.UI.lockedForMe(entryObj)) {
                                    var lockedBy = lockObj.attr("data-name");
                                    lockHint = ASC.Files.FilesJSResources.TitleLockedFileBy.format(lockedBy);
                                    if (ASC.Files.Constants.ADMIN || ASC.Files.Folders.folderContainer == "my") {
                                        entryObj.addClass("file-can-unlock");
                                    }
                                }
                                lockObj.attr("title", lockHint);
                            }
                        } else {
                            lockObj.remove();
                        }
                    }

                    if (ASC.Files.Utility.MustConvert(entryTitle) && !entryData.encrypted) {
                        entryObj.find(".pencil:not(.convert-action)").remove();
                        if (Teamlab.profile.isVisitor && !ASC.Files.UI.accessEdit()
                            || ASC.Files.Folders.folderContainer == "trash") {
                            entryObj.find(".convert-action").remove();
                        } else {
                            entryObj.find(".convert-action").show();
                        }
                    } else {
                        if (entryData.encrypted) {
                            entryUrl = ASC.Files.Utility.GetFileDownloadUrl(entryId);
                            rowLink.attr("href", entryUrl);
                            entryObj.find(".file-edit").attr("href", entryUrl);
                        }

                        if (ASC.Files.UI.editableFile(entryData)
                            && (!entryData.encrypted
                            || ASC.Files.Folders.folderContainer == "privacy"
                                && ASC.Files.Utility.CanWebEncrypt(entryTitle)
                                && ASC.Desktop && ASC.Desktop.encryptionSupport())) {
                            ASC.Files.UI.lockEditFile(entryObj, ASC.Files.UI.editingFile(entryObj));
                            entryObj.find(".convert-action").remove();
                            if (ASC.Files.Utility.CanCoAuhtoring(entryTitle) && !entryObj.hasClass("on-edit-alone")) {
                                entryObj.addClass("can-coauthoring");
                            }

                            if (!entryData.encrypted) {
                                entryUrl = ASC.Files.Utility.GetFileWebEditorUrl(entryId);
                                entryObj.find(".file-edit").attr("href", entryUrl);
                            }

                        } else {
                            entryObj.addClass("cannot-edit");
                            entryObj.find(".pencil").remove();
                        }
                    }

                    if (entryData.encrypted
                        && (ASC.Files.Folders.folderContainer != "privacy"
                            || !ASC.Desktop
                            || !ASC.Desktop.encryptionSupport()
                            || ASC.Resources.Master.Personal)) {
                        entryObj.addClass("without-share");
                    } else if (ASC.Resources.Master.Personal
                        && !ASC.Files.Utility.CanWebView(entryTitle)) {
                        entryObj.addClass("without-share");
                    }
                } else if (ASC.Files.Folders.folderContainer != "trash") {
                    entryUrl = ASC.Files.UI.getEntryLink(entryType, entryId);
                    rowLink.attr("href", entryUrl);
                }

                if (ASC.Files.Folders.folderContainer == "trash"
                    || entryData.error) {
                    rowLink.attr("href", ASC.Files.UI.getEntryLink("folder", ASC.Files.Folders.currentFolder.id));
                }

                if (!jq("#filesMainContent").hasClass("without-share")
                    && (ASC.Files.Folders.folderContainer == "forme" && !ASC.Files.UI.accessEdit(entryData, entryObj)
                        || ASC.Files.Folders.folderContainer == "privacy" && (entryType == "folder" || !ASC.Files.UI.accessEdit(entryData, entryObj))
                        || ASC.Resources.Master.Personal && entryType == "folder"
                        || Teamlab.profile.isVisitor === true)) {
                    entryObj.addClass("without-share");
                }

                if (jq.browser.msie) {
                    entryObj.find("*").attr("unselectable", "on");
                }
            }
        });

        if (jq("#switchToNormal").length) {
            ASC.Files.UI.switchFolderView();
        }
    };

    var clickRow = function (event, target) {
        var e = jq.fixEvent(event);

        if (!(e.button == 0 || (jq.browser.msie && e.button == 1))) {
            return true;
        }

        target = target || jq(e.srcElement || e.target);

        try {
            if (target.is("a, .menu-small")) {
                return true;
            }
        } catch (e) {
            return true;
        }

        var entryObj =
            target.is(".file-row")
                ? target
                : target.closest(".file-row");

        var select = !jq(entryObj).hasClass("row-selected") || jq(entryObj).find(".checkbox input:visible").is("[type=radio]");

        if (e.shiftKey){
            if (!ASC.Files.UI.lastSelectedEntry) {
                ASC.Files.UI.lastSelectedEntry = entryObj;
            }
            select = true;
            var i1 = jq(".file-row").index(entryObj);
            var i2 = jq(".file-row").index(ASC.Files.UI.lastSelectedEntry.entryObj);

            var minItem = Math.min(i1, i2);
            var maxItem = Math.max(i1, i2);

            jq(".file-row:lt(" + maxItem + "):gt(" + minItem + ")").each(function () {
                ASC.Files.UI.selectRow(jq(this), select);
            });
        } else if (!e.ctrlKey && !target.is(".checkbox, .checkbox input")) {
            if (jq(".row-selected").length > 1) {
                select = true;
            }
            ASC.Files.UI.checkSelectAll(false);
        }

        ASC.Files.UI.selectRow(entryObj, select);
        ASC.Files.UI.updateMainContentHeader();

        ASC.Files.UI.lastSelectedEntry = select ? {entryObj: entryObj} : null;

        return true;
    };

    var updateMainContentHeader = function () {
        ASC.Files.UI.resetSelectAll(jq("#filesMainContent .file-row:has(.checkbox input:visible:not(:checked))").length == 0);
        if (jq("#filesMainContent .file-row:has(.checkbox input:checked)").length == 0) {
            jq("#mainContentHeader .menuAction.unlockAction").removeClass("unlockAction");
            if (ASC.Files.Folders.folderContainer == "trash") {
                jq("#mainEmptyTrash").addClass("unlockAction");
            }
        } else if (ASC.Files.Actions) {
            ASC.Files.Actions.showActionsViewPanel();
        }

        if (typeof ASC.Files.UI.filesSelectedHandler == "function") {
            ASC.Files.UI.filesSelectedHandler();
        }
    }; 

    var selectRow = function (entryObj, value) {
        if (!entryObj) {
            return false;
        }
        if (!entryObj.hasClass("file-row")) {
            entryObj = entryObj.closest(".file-row");
        }

        if (entryObj.hasClass("row-selected") == value) {
            return false;
        }

        if (entryObj.hasClass("checkloading") && value) {
            return false;
        }

        if (entryObj.hasClass("new-folder") || jq(entryObj).hasClass("row-rename")) {
            value = false;
        }

        if (value === true && entryObj.find(".checkbox input:visible").is("[type=radio]")) {
            jq("#filesMainContent .checkbox input:checked").prop("checked", false);
            jq("#filesMainContent .file-row.row-selected").removeClass("row-selected");
        }

        entryObj.find(".checkbox input").prop("checked", value === true);
        entryObj.toggleClass("row-selected", value).removeAttr("data-selected");
        jq("#filesMainContent .menu-small.active").removeClass("active");

        return true;
    };

    var resetSelectAll = function (param) {
        var indeterminate = param !== true && jq("#filesMainContent .file-row .checkbox input:checked").length != 0;
        jq("#filesSelectAllCheck").prop("checked", param === true || indeterminate === true);

        jq("#filesSelectAllCheck").prop("indeterminate", indeterminate);
    };

    var checkSelectAll = function (value) {
        var selectionChanged = false;
        var selectedString = ".row-selected";
        if (value) {
            selectedString = ":not(" + selectedString + ")";
        }
        jq("#filesMainContent .file-row" + selectedString + ":not(.checkloading)").each(function () {
            selectionChanged = ASC.Files.UI.selectRow(jq(this), value) || selectionChanged;
        });
        if (selectionChanged) {
            ASC.Files.UI.updateMainContentHeader();
        }

        ASC.Files.UI.lastSelectedEntry = null;
    };

    var checkSelect = function (filter) {
        ASC.Files.Actions.hideAllActionPanels();
        jq("#filesMainContent .file-row:not(.checkloading)").each(function () {
            var sel;
            var fileTitle = ASC.Files.UI.getObjectTitle(this);
            switch (filter) {
                case "folder":
                    sel = jq(this).hasClass("folder-row");
                    break;
                case "file":
                    sel = !jq(this).hasClass("folder-row");
                    break;
                case "document":
                    sel = ASC.Files.Utility.FileIsDocument(fileTitle) && !jq(this).hasClass("folder-row");
                    break;
                case "presentation":
                    sel = ASC.Files.Utility.FileIsPresentation(fileTitle) && !jq(this).hasClass("folder-row");
                    break;
                case "spreadsheet":
                    sel = ASC.Files.Utility.FileIsSpreadsheet(fileTitle) && !jq(this).hasClass("folder-row");
                    break;
                case "image":
                    sel = ASC.Files.Utility.FileIsImage(fileTitle) && !jq(this).hasClass("folder-row");
                    break;
                case "media":
                    sel = (ASC.Files.Utility.FileIsAudio(fileTitle) || ASC.Files.Utility.FileIsVideo(fileTitle)) && !jq(this).hasClass("folder-row");
                    break;
                case "archive":
                    sel = ASC.Files.Utility.FileIsArchive(fileTitle) && !jq(this).hasClass("folder-row");
                    break;
                default:
                    return false;
            }

            ASC.Files.UI.selectRow(jq(this), sel);
            return true;
        });
        ASC.Files.UI.updateMainContentHeader();
    };

    var displayInfoPanel = function (str, warn) {
        if (str === "" || typeof str === "undefined") {
            return;
        }

        if (warn === true) {
            toastr.error(str);
        } else {
            toastr.success(str);
        }
    };

    var accessEdit = function (entryData, entryObj, restrictedEditing) {
        if (Teamlab.profile.isOutsider) {
            return false;
        }

        entryData = entryData ||
            (entryObj
                ? ASC.Files.UI.getObjectData(entryObj)
                : ASC.Files.Folders.currentFolder);

        var entryId = entryData ? entryData.entryId : null;
        var entryType = entryData ? entryData.entryType : null;
        if (entryType == "folder") {
            if (entryId == ASC.Files.Constants.FOLDER_ID_COMMON_FILES && !ASC.Files.Constants.ADMIN) {
                return false;
            }

            if (entryId == ASC.Files.Constants.FOLDER_ID_SHARE) {
                return false;
            }

            if (entryId == ASC.Files.Constants.FOLDER_ID_RECENT) {
                return false;
            }

            if (entryId == ASC.Files.Constants.FOLDER_ID_FAVORITES) {
                return false;
            }

            if (entryId == ASC.Files.Constants.FOLDER_ID_TEMPLATES) {
                return false;
            }

            if (entryId == ASC.Files.Constants.FOLDER_ID_TRASH) {
                return false;
            }

            if (entryId == ASC.Files.Constants.FOLDER_ID_PROJECT) {
                return false;
            }
        }

        var curAccess = parseInt(entryData ? entryData.access : ASC.Files.Constants.AceStatusEnum.None);

        if (entryType == "folder" && ASC.Files.Folders && ASC.Files.Folders.currentFolder && entryId == ASC.Files.Folders.currentFolder.id) {
            curAccess = parseInt(ASC.Files.Folders.currentFolder.access);
        }

        switch (curAccess) {
            case ASC.Files.Constants.AceStatusEnum.None:
            case ASC.Files.Constants.AceStatusEnum.ReadWrite:
                return true;
            case ASC.Files.Constants.AceStatusEnum.Read:
            case ASC.Files.Constants.AceStatusEnum.Restrict:
                return false;
            case ASC.Files.Constants.AceStatusEnum.CustomFilter:
            case ASC.Files.Constants.AceStatusEnum.Review:
            case ASC.Files.Constants.AceStatusEnum.FillForms:
            case ASC.Files.Constants.AceStatusEnum.Comment:
                return !!restrictedEditing;
            default:
                if (entryData
                    && entryData.entryType == "folder"
                    && (entryData.entryId === ASC.Files.Constants.FOLDER_ID_SHARE
                        || entryData.entryId === ASC.Files.Constants.FOLDER_ID_RECENT
                        || entryData.entryId === ASC.Files.Constants.FOLDER_ID_FAVORITES
                        || entryData.entryId === ASC.Files.Constants.FOLDER_ID_TEMPLATES
                        || entryData.entryId === ASC.Files.Constants.FOLDER_ID_PROJECT
                        || entryData.entryId === ASC.Files.Constants.FOLDER_ID_TRASH)) {
                    return false;
                }

                return ASC.Files.Constants.ADMIN || !entryData || entryData.create_by_id == Teamlab.profile.id;
        }
    };

    var accessDelete = function (entryObj) {
        if (Teamlab.profile.isOutsider) {
            return false;
        }

        var entryData = entryObj
            ? ASC.Files.UI.getObjectData(entryObj)
            : ASC.Files.Folders.currentFolder;

        var access = parseInt(entryData ? entryData.access : ASC.Files.Constants.AceStatusEnum.None);

        var entryId = entryData ? entryData.entryId : null;
        var entryType = entryData ? entryData.entryType : null;

        if (entryType == "folder" && entryId == ASC.Files.Folders.currentFolder.id) {
            access = ASC.Files.Folders.currentFolder.access;
        }

        if (access == ASC.Files.Constants.AceStatusEnum.Restrict) {
            entryObj.remove();
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.AceStatusEnum_Restrict, true);
            return false;
        }

        if (ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_PROJECT
            || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_RECENT
            || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_FAVORITES
            || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_TEMPLATES) {
            return false;
        }

        if (access == ASC.Files.Constants.AceStatusEnum.Read) {
            return false;
        }

        return (access == ASC.Files.Constants.AceStatusEnum.None
            || ASC.Files.Folders.folderContainer == "corporate" && ASC.Files.Constants.ADMIN
            || ASC.Files.Folders.currentFolder.id != ASC.Files.Constants.FOLDER_ID_PROJECT && entryData.create_by_id == Teamlab.profile.id);

    };

    var lockEditFileById = function (fileId, edit, listBy) {
        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);
        return ASC.Files.UI.lockEditFile(fileObj, edit, listBy);
    };

    var lockEditFile = function (fileObj, edit, listBy) {
        if (fileObj.hasClass("folder-row")) {
            return;
        }

        if (edit) {
            fileObj.addClass("on-edit");

            var strBy = ASC.Files.FilesJSResources.TitleEditingFile;
            if (listBy) {
                strBy = ASC.Files.FilesJSResources.TitleEditingFileBy.format(listBy);
            }

            fileObj.find(".pencil.file-editing").attr("title", strBy);
        } else {
            fileObj.removeClass("on-edit");
        }
    };

    var checkEditing = function () {

        var list = jq(".files-content-panel #filesMainContent .file-row.on-edit:visible:not(.cannot-edit)");
        if (list.length == 0) {
            return;
        }

        var data = {
            entry: jq(list).map(function (i, item) {
                var fileId = ASC.Files.UI.getObjectData(item).entryId;
                return fileId;
            }).toArray()
        };

        ASC.Files.ServiceManager.checkEditing(ASC.Files.ServiceManager.events.CheckEditing, { list: data.entry }, { stringList: data });
    };

    var checkEditingDefer = function () {
        clearTimeout(ASC.Files.UI.timeCheckEditing);
        ASC.Files.UI.timeCheckEditing = setTimeout(ASC.Files.UI.checkEditing, 20000);
    };

    var displayEntryTooltip = function (entryObj, entryType, entryId) {
        entryObj = entryObj || ASC.Files.UI.getEntryObject(entryType, entryId);

        var entryData = ASC.Files.UI.getObjectData(entryObj);

        var jsonData = {
            entryTooltip:
                {
                    type: entryType,
                    title: entryData.title,
                    create_by: entryData.create_by,
                    date_type:
                        (ASC.Files.Folders.folderContainer == "trash"
                            ? "remove"
                            : (entryType == "folder"
                                ? "create"
                                : (entryData.version > 1
                                    ? "update"
                                    : "upload"))),
                    modified_on:
                        (entryType == "file"
                            ? entryData.modified_on
                            : (ASC.Files.Folders.folderContainer == "trash"
                                ? entryData.modified_on
                                : entryData.create_on)),
                    version: entryData.version, //file
                    length_string: entryData.content_length_string, //file
                    error: entryData.error,

                    provider_key: entryData.provider_key,
                    total_files: parseInt(entryObj.find(".countFiles").html()) || 0, //folder
                    total_sub_folder: parseInt(entryObj.find(".countFolders").html()) || 0,//folder
                    comment: entryData.comment,
                }
        };

        var stringData = ASC.Files.Common.jsonToXml(jsonData);

        var htmlTootlip = ASC.Files.TemplateManager.translateFromString(stringData);

        var $selector = entryObj.find(".thumb-" + entryType);
        var $dropdown = jq("#entryTooltip");
        var $window = jq(window);
        var addTop = 12;

        $dropdown.html(htmlTootlip);

        var toUp = $selector.offset().top + $selector.outerHeight() + $dropdown.outerHeight() + addTop > $window.height() + $window.scrollTop();

        jq.dropdownToggle().toggle($selector, "entryTooltip", toUp ? addTop : 0, 0, false, null, null, null, toUp);
    };

    var hideEntryTooltip = function () {
        clearTimeout(ASC.Files.UI.timeTooltip);
        jq("#entryTooltip").hide();

        var linkRow = jq("#filesMainContent .file-row .entry-title .name a[data-title]").removeAttr("data-title");
        linkRow.attr("title", linkRow.text());
    };

    var checkEmptyContent = function () {
        var countAppend = ASC.Files.Constants.COUNT_ON_PAGE - jq("#filesMainContent .file-row").length;
        if (countAppend > 0) {
            if (ASC.Files.UI.currentPage < ASC.Files.UI.amountPage) {
                ASC.Files.Folders.getFolderItems(true, countAppend);
            } else {
                if (ASC.Files.Folders.folderContainer && countAppend >= ASC.Files.Constants.COUNT_ON_PAGE) {
                    ASC.Files.EmptyScreen.displayEmptyScreen();
                }
            }
        }
    };

    var checkButtonBack = function (buttonSelector, panelSelector) {
        if (ASC.Files.Tree && ASC.Files.Tree.pathParts.length > 1) {
            var parentFolderId = ASC.Files.Tree.pathParts[ASC.Files.Tree.pathParts.length - 2];
            var parentFolderTitle = ASC.Files.Tree.getFolderTitle(parentFolderId);

            jq(buttonSelector)
                .attr("data-id", parentFolderId)
                .attr("href", ASC.Files.UI.getEntryLink("folder", parentFolderId))
                .attr("title", parentFolderTitle)
                .show();

            jq(panelSelector || buttonSelector).show();
        } else {
            jq(panelSelector || buttonSelector).hide();
        }
    };

    var checkCharacter = function (input) {
        jq(input).unbind("keyup").bind("keyup", function () {
            var str = jq(this).val();
            if (str.search(ASC.Files.Common.characterRegExp) != -1) {
                jq(this).val(ASC.Files.Common.replaceSpecCharacter(str));
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.ErrorMassage_SpecCharacter.format(ASC.Files.Common.characterString), true);
            }
        });
    };

    var setDocumentTitle = function (titlePrefix) {
        if (!ASC.Files.UI.canSetDocumentTitle) {
            return;
        }
        titlePrefix = titlePrefix || ASC.Files.Folders.currentFolder.title;
        titlePrefix = titlePrefix ? titlePrefix + " - " : "";
        var titlePostfix = jq(".files-content-panel").attr("data-title") || "TeamLab";
        document.title = titlePrefix + titlePostfix;
    };

    var hideAllContent = function (show) {
        if (ASC.Files.MediaPlayer && ASC.Files.MediaPlayer.isView) {
            ASC.Files.MediaPlayer.closePlayer();
        }

        show = !!show;

        if (!show) {
            jq("#treeViewContainer .selected").removeClass("selected");
            jq("#treeViewContainer .parent-selected").removeClass("parent-selected");
            jq("#mainContentHeader, .files-filter").hide();
        }

        jq("#treeSecondary .currentCategory").removeClass("currentCategory");
        jq("#treeSecondary .active").removeClass("active");

        jq("#settingCommon, #settingAdmin").hide();
        jq("#settingThirdPartyPanel").hide();
        jq("#helpPanel").hide();
        jq("#moreFeatures").hide();
        jq(".files-content-panel").toggle(show);

        jq(".mainPageContent").children(".loader-page").remove();
    };

    var displayHelp = function (helpId) {
        if (!jq("#helpPanel").length) {
            ASC.Files.Anchor.defaultFolderSet();
            return;
        }

        ASC.Files.UI.hideAllContent();

        var params = { helpId: helpId };

        if (!jq("#helpPanel").text().trim().length) {
            params.update = true;
            ASC.Files.ServiceManager.getHelpCenter(ASC.Files.ServiceManager.events.GetHelpCenter, params);
        } else {
            ASC.Files.EventHandler.onGetHelpCenter(null, params);
        }
    };

    var displayCommonSetting = function () {
        ASC.Files.UI.hideAllContent();
        LoadingBanner.hideLoading();
        jq("#treeSetting").addClass("currentCategory open");
        jq(".settings-link-common").addClass("active");
        jq("#settingCommon").show();

        ASC.Files.UI.setDocumentTitle(ASC.Files.FilesJSResources.TitleSettingsCommon);
    };

    var displayAdminSetting = function () {
        ASC.Files.UI.hideAllContent();
        LoadingBanner.hideLoading();
        jq("#treeSetting").addClass("currentCategory open");
        jq(".settings-link-admin").addClass("active");
        jq("#settingAdmin").show();

        ASC.Files.UI.setDocumentTitle(ASC.Files.FilesJSResources.TitleSettingsAdmin);
    };

    var displayMoreFeaturs = function () {
        ASC.Files.UI.hideAllContent();
        LoadingBanner.hideLoading();
        ASC.Files.CreateMenu.disableMenu();
        jq("#moreFeatures").show();

        ASC.Files.UI.setDocumentTitle(ASC.Files.FilesJSResources.TitleMoreFeaturs);
    };

    var displayPersonalLimitStorageExceed = function () {
        if (!jq("#personalLimitExceedStoragePanel").length) {
            return false;
        }
        ASC.Files.UI.blockUI("#personalLimitExceedStoragePanel", 500);
        return true;
    };

    var displayTariffLimitStorageExceed = function () {
        if (!jq("#tariffLimitExceedStoragePanel").length) {
            return false;
        }
        ASC.Files.UI.blockUI("#tariffLimitExceedStoragePanel", 500);
        return true;
    };

    var displayTariffFileSizeExceed = function () {
        if (!jq("#tariffLimitExceedFileSizePanel").length) {
            return false;
        }
        ASC.Files.UI.blockUI("#tariffLimitExceedFileSizePanel", 500);
        return true;
    };

    var blockUI = function (obj, width) {
        if (ASC.Files.Mouse) {
            ASC.Files.Mouse.finishMoveTo();
            ASC.Files.Mouse.finishSelecting();
        }
        return StudioBlockUIManager.blockUI(obj, width);
    };

    var setProgressValue = function (progressBar, value) {
        value = value | 0;
        progressBar = jq(progressBar);
        if (!progressBar.is("progress")) {
            progressBar = progressBar.find("progress");
        }

        var dt = 50;
        var timer = progressBar.data("timer");
        clearInterval(timer);

        var curValue = progressBar.val();
        if (!value || curValue > value) {
            progressBar.val(value);
        } else {
            var nextProgressValue = function (dValue, maxValue) {
                var v = Math.min(maxValue, progressBar.val() + dValue);
                progressBar.val(v);
                if (v == maxValue) {
                    clearInterval(timer);
                }
            };

            var dV = Math.max(1, (value - curValue) / dt);
            timer = setInterval(function () {
                nextProgressValue(dV, value);
            }, 1);
            progressBar.data("timer", timer);
        }

        var prValue = progressBar.find(".asc-progress-value");
        if (!value) {
            prValue.css("width", value + "%");
        } else {
            prValue.animate({ "width": value + "%" });
        }
    };

    var registerUserProfilePopup = function (searchArea) {
        searchArea.find(".userLink").each(function () {
            var id = jq(this).attr("id").trim();
            if (id != null && id != "") {
                filesUserProfileInfo.RegistryElement(id, "\"" + jq(this).attr("data-uid") + "\"");
            }
        });
    };

    return {
        init: init,
        parseItemId: parseItemId,
        getEntryObject: getEntryObject,
        getObjectData: getObjectData,
        getSelectorId: getSelectorId,

        getEntryTitle: getEntryTitle,
        getObjectTitle: getObjectTitle,

        getEntryLink: getEntryLink,

        currentPage: currentPage,
        amountPage: amountPage,
        countTotal: countTotal,

        updateFolderView: updateFolderView,
        switchFolderView: switchFolderView,
        isSettingsPanel: isSettingsPanel,

        blockObjectById: blockObjectById,
        blockObject: blockObject,

        highlightExtension: highlightExtension,

        addRowHandlers: addRowHandlers,

        updateMainContentHeader: updateMainContentHeader,

        lastSelectedEntry: lastSelectedEntry,
        checkSelectAll: checkSelectAll,
        checkSelect: checkSelect,
        selectRow: selectRow,
        clickRow: clickRow,
        resetSelectAll: resetSelectAll,

        displayInfoPanel: displayInfoPanel,

        editingFile: editingFile,
        editableFile: editableFile,
        lockedForMe: lockedForMe,

        checkEditing: checkEditing,
        timeCheckEditing: timeCheckEditing,
        checkEditingDefer: checkEditingDefer,

        lockEditFileById: lockEditFileById,
        lockEditFile: lockEditFile,

        accessEdit: accessEdit,
        accessDelete: accessDelete,

        checkCharacter: checkCharacter,
        canSetDocumentTitle: canSetDocumentTitle,
        setDocumentTitle: setDocumentTitle,

        stickContentHeader: stickContentHeader,
        fixContentHeaderWidth: fixContentHeaderWidth,
        stickMovingPanel: stickMovingPanel,

        trackShowMore: trackShowMore,
        trackShowMoreTemplates: trackShowMoreTemplates,

        displayEntryTooltip: displayEntryTooltip,
        hideEntryTooltip: hideEntryTooltip,
        timeTooltip: timeTooltip,

        checkEmptyContent: checkEmptyContent,
        checkButtonBack: checkButtonBack,

        hideAllContent: hideAllContent,
        displayHelp: displayHelp,
        displayCommonSetting: displayCommonSetting,
        displayAdminSetting: displayAdminSetting,
        displayMoreFeaturs: displayMoreFeaturs,

        displayPersonalLimitStorageExceed: displayPersonalLimitStorageExceed,
        displayTariffLimitStorageExceed: displayTariffLimitStorageExceed,
        displayTariffFileSizeExceed: displayTariffFileSizeExceed,

        blockUI: blockUI,

        setProgressValue: setProgressValue,

        filesSelectedHandler: filesSelectedHandler,

        registerUserProfilePopup: registerUserProfilePopup,
    };
})();

(function ($) {
    ASC.Files.UI.init();

    $(function () {
        jq("#switchToNormal").click(function () {
            ASC.Files.UI.switchFolderView(false);
        });

        jq("#switchToCompact").click(function () {
            ASC.Files.UI.switchFolderView(true);
        });
        jq("#filesMainContent").on("click", ".file-row", ASC.Files.UI.clickRow);

        jq("#filesMainContent").on("dblclick", ".file-row:not(.checkloading)", function (event) {
            if (jq(event.srcElement || event.target).is("input, .checkbox, .file-lock, .version, .pencil, .is-new, .btn-row, .menu-small, #contentVersions, #contentVersions *")) {
                return;
            }
            jq(this).closest(".file-row").find(".entry-title .name a").trigger("click");
        });


        jq("#filesMainContent").on("click", ".checkbox input", function (event) {
            if (ASC.Files.Actions) {
                ASC.Files.Actions.hideAllActionPanels();
            }
            ASC.Files.UI.clickRow(event, jq(this));

            jq(this).blur();
            ASC.Files.Common.cancelBubble(event);
        });

        jq(".menu-action-on-top").click(function () {
            jq(document).scrollTop(0);
            return false;
        });

        jq(document).keydown(function (event) {
            if (jq(".blockUI:visible").length != 0 ||
                jq("#fileViewerDialog:visible,#mediaPlayer:visible").length != 0 ||
                jq(".studio-action-panel:visible").length != 0 ||
                jq("#promptRename").length != 0 ||
                jq("#promptCreateFolder").length != 0 ||
                jq("#promptCreateFile").length != 0) {
                return true;
            }

            if (!e) {
                var e = event;
            }

            e = jq.fixEvent(e);

            var target = e.target || e.srcElement;
            try {
                if (jq(target).is("input,textarea")) {
                    return true;
                }
            } catch (e) {
                return true;
            }

            var code = e.keyCode || e.which;

            if (code == ASC.Files.Common.keyCode.A && e.ctrlKey) {
                if (jq.browser.opera) {
                    setTimeout(function () {
                        jq("#filesSelectAllCheck").focus();
                    }, 1);
                }
                ASC.Files.UI.checkSelectAll(true);
                return false;
            }

            return true;
        });

        jq(document).keyup(function (event) {
            if (jq(".blockUI:visible").length != 0 ||
                jq("#fileViewerDialog:visible,#mediaPlayer:visible").length != 0 ||
                jq("#promptRename").length != 0 ||
                jq("#promptCreateFolder").length != 0 ||
                jq("#promptCreateFile").length != 0) {
                return true;
            }

            if (!e) {
                var e = event;
            }

            e = jq.fixEvent(e);

            var target = e.target || e.srcElement;
            try {
                if (jq(target).is("input,textarea")) {
                    return true;
                }
            } catch (e) {
                return true;
            }

            var code = e.keyCode || e.which;

            if (ASC.Files.Folders) {
                if (code == ASC.Files.Common.keyCode.deleteKey && !e.shiftKey && !e.altKey) {
                    ASC.Files.Folders.deleteItem();
                    return false;
                }

                if (code == ASC.Files.Common.keyCode.F && e.shiftKey) {
                    ASC.Files.Folders.createFolder();
                    return false;
                }

                if (code == ASC.Files.Common.keyCode.N && e.shiftKey) {
                    if (ASC.Files.Utility.CanWebEdit(ASC.Files.Utility.Resource.InternalFormats.Document)) {
                        ASC.Files.Folders.typeNewDoc = "document";
                        ASC.Files.Folders.createNewDoc();
                    }
                    return false;
                }

                if (code == ASC.Files.Common.keyCode.F2 && jq("#filesMainContent .file-row.row-selected:visible").length == 1) {
                    var entryData = ASC.Files.UI.getObjectData(jq("#filesMainContent .file-row.row-selected"));

                    ASC.Files.Actions.hideAllActionPanels();
                    ASC.Files.Folders.rename(entryData.entryType, entryData.id);
                }
            }

            if (code == ASC.Files.Common.keyCode.esc) {
                if (ASC.Files.Mouse) {
                    if (jq("#filesMovingTooltip").is(":visible")) {
                        ASC.Files.Mouse.finishMoveTo();
                        return false;
                    }

                    if (jq("#filesSelector").is(":visible")) {
                        ASC.Files.Mouse.finishSelecting();
                        return false;
                    }
                }

                if (ASC.Files.Actions) {
                    ASC.Files.Actions.hideAllActionPanels();
                }

                if (ASC.Files.ChunkUploads) {
                    ASC.Files.ChunkUploads.hideDragHighlight();
                }
                return true;
            }
            return true;
        });

        jq("#bottomLoaderPanel").draggable(
            {
                axis: "x",
                handle: ".progress-dialog-header",
                containment: "body"
            }
        );

        jq("#bottomLoaderPanel").on("drag", ".progress-dialog-header", function () {
            ASC.Files.Actions.hideAllActionPanels();
        });

    });
})(jQuery);