/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


window.ASC.Files.UI = (function () {
    var isInit = false;

    var canSetDocumentTitle = true;

    var timeCheckEditing = null;
    var timeTooltip = null;

    var currentPage = 0;
    var amountPage = 0;
    var countTotal = 0;

    var lastSelectedEntry = null;

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
            return jq("#filesMainContent .file-row[data-id=\"" + entryType + "_" + entryId + "\"]");
        }
        return null;
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

        var entryDataStr = entryObject.find(selectorData).val();
        var resulat = jq.parseJSON(entryDataStr || null);
        if (!resulat || !ASC.Files.Common.isCorrectId(resulat.id)) {
            return null;
        }

        resulat.entryId = resulat.id;
        resulat.entryType = (resulat.entryType === "file" ? "file" : "folder");
        resulat.create_by = entryObject.find("input:hidden[name=\"create_by\"]").val();
        resulat.modified_by = entryObject.find("input:hidden[name=\"modified_by\"]").val();
        resulat.comment = entryObject.find("input:hidden[name=\"comment\"]").val();
        resulat.entryObject = entryObject;
        resulat.error = (resulat.error != "" ? resulat.error : false);
        resulat.title = (resulat.title || "").trim();
        return resulat;
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
            if (jq(document).height() - jq(window).height() <= jq(window).scrollTop() + 350) {
                ASC.Files.Folders.showMore();
            }
        }

        if (ASC.Files.Mouse) {
            ASC.Files.Mouse.updateMainContentArea();
        }
    };

    var isSettingsPanel = function () {
        return jq("#settingCommon, #settingThirdPartyPanel, #helpPanel").is(":visible");
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
        var movingPopupShift = boxTop.offset().top + boxTop.outerHeight();
        var movingPopupObj = jq("#filesSelectorPanel:visible");

        ASC.Files.UI.stickMovingPanel("mainContentHeader", movingPopupObj, movingPopupShift, false);

        ASC.Files.UI.fixContentHeaderWidth();
    };

    var stickMovingPanel = function (toggleObjId, movingPopupObj, movingPopupShift, fixBigHeight) {
        var toggleObj = jq("#" + toggleObjId);
        if (!toggleObj.is(":visible")) {
            return;
        }
        var spacerName = "Spacer";

        var toggleObjSpacer = jq("#" + toggleObjId + spacerName);
        var absTop;

        if (jq("#" + toggleObjId + spacerName + ":visible").length == 0) {
            absTop = toggleObj.offset().top;
        } else {
            absTop = toggleObjSpacer.offset().top;
        }

        movingPopupShift = movingPopupShift || 0;
        var jqWindow = jq(window);
        var winScroll = jqWindow.scrollTop();

        if (winScroll >= absTop) {
            var toggleObjHeight = toggleObj.outerHeight();
            var parentObj = toggleObj.parent();
            var parentHeight = parentObj.innerHeight() - parseInt(parentObj.css("padding-top")) - parseInt(parentObj.css("padding-bottom"));

            if (!toggleObj.hasClass("stick-panel") || jq.browser.mobile) {
                if (!fixBigHeight || (winScroll - absTop < parentHeight - toggleObjHeight)) {
                    if (toggleObjSpacer.length == 0) {
                        var createToggleObjSpacer = document.createElement("div");
                        createToggleObjSpacer.id = toggleObjId + spacerName;
                        document.body.appendChild(createToggleObjSpacer);
                        toggleObjSpacer = jq("#" + toggleObjId + spacerName);
                        toggleObjSpacer.insertAfter(toggleObj);
                        toggleObjSpacer.css(
                            {
                                "height": toggleObj.css("height"),
                                "padding-top": toggleObj.css("padding-top"),
                                "padding-bottom": toggleObj.css("padding-bottom")
                            });
                    }
                    toggleObjSpacer.show();

                    toggleObj
                        .addClass("stick-panel")
                        .css("left", (parentObj.offset().left - jqWindow.scrollLeft()));

                    if (movingPopupObj) {
                        movingPopupObj.css({
                            "position": "fixed",
                            "top": movingPopupShift - winScroll
                        });
                    }

                    if (jq.browser.mobile) {
                        toggleObj.css(
                            {
                                "top": jq(document).scrollTop() + "px",
                                "position": "absolute"
                            });
                    }
                }
            }
            if (fixBigHeight && toggleObj.hasClass("stick-panel")) {
                toggleObj.css("top", -Math.max(0, (winScroll - absTop - (parentHeight - toggleObjHeight))));
            }
        } else {
            if (toggleObj.hasClass("stick-panel")) {
                toggleObjSpacer.hide();
                toggleObj.removeClass("stick-panel");
                jq("#mainContentHeader").css("width", "auto");

                if (movingPopupObj) {
                    movingPopupObj.css({
                        "position": "absolute",
                        "top": movingPopupShift
                    });
                }

                if (jq.browser.mobile) {
                    toggleObj.css(
                        {
                            "position": "static"
                        });
                }
            }
        }
    };

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
            && ASC.Files.UI.accessEdit(fileData, fileObj)
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
                        var entryUrl = ASC.Files.Utility.GetFileDownloadUrl(entryId);

                        if (ASC.Files.Utility.CanWebEdit(entryTitle)
                            && !ASC.Files.Utility.MustConvert(entryTitle)
                            && ASC.Resources.Master.TenantTariffDocsEdition) {
                            entryUrl = ASC.Files.Utility.GetFileWebEditorUrl(entryId);
                            rowLink.attr("href", entryUrl).attr("target", "_blank");
                        } else if (ASC.Files.Utility.CanWebView(entryTitle)
                            && ASC.Resources.Master.TenantTariffDocsEdition) {
                            entryUrl = ASC.Files.Utility.GetFileWebViewerUrl(entryId);
                            rowLink.attr("href", entryUrl).attr("target", "_blank");
                        } else if (typeof ASC.Files.ImageViewer != "undefined" && ASC.Files.Utility.CanImageView(entryTitle)) {
                            entryUrl = "#" + ASC.Files.ImageViewer.getPreviewHash(entryId);
                            rowLink.attr("href", entryUrl);
                        } else {
                            rowLink.attr("href", jq.browser.mobile ? "" : entryUrl);
                        }

                        var lockObj = entryObj.find(".file-lock");
                        if (ASC.Files.UI.accessEdit(entryData, entryObj)) {
                            if (entryObj.hasClass("file-locked")) {
                                var lockHint = ASC.Files.FilesJSResources.TitleLockedFile;
                                if (ASC.Files.UI.lockedForMe(entryObj)) {
                                    var lockedBy = lockObj.attr("data-name");
                                    lockHint = ASC.Files.FilesJSResources.TitleLockedFileBy.format(lockedBy);
                                }
                                lockObj.attr("title", lockHint);
                            }
                        } else {
                            lockObj.remove();
                        }
                    }

                    if (ASC.Files.Utility.MustConvert(entryTitle)) {
                        entryObj.find(".pencil:not(.convert-action)").remove();
                        if (Teamlab.profile.isVisitor && !ASC.Files.UI.accessEdit()
                            || ASC.Files.Folders.folderContainer == "trash") {
                            entryObj.find(".convert-action").remove();
                        } else {
                            entryObj.find(".convert-action").show();
                        }
                    } else {
                        if (ASC.Files.UI.editableFile(entryData)) {
                            ASC.Files.UI.lockEditFile(entryObj, ASC.Files.UI.editingFile(entryObj));
                            entryObj.find(".convert-action").remove();
                            if (ASC.Files.Utility.CanCoAuhtoring(entryTitle) && !entryObj.hasClass("on-edit-alone")) {
                                entryObj.addClass("can-coauthoring");
                            }
                            entryUrl = ASC.Files.Utility.GetFileWebEditorUrl(entryId);
                            entryObj.find(".file-edit").attr("href", entryUrl);
                        } else {
                            entryObj.addClass("cannot-edit");
                            entryObj.find(".pencil").remove();
                        }
                    }
                } else if (ASC.Files.Folders.folderContainer != "trash") {
                    entryUrl = ASC.Files.Constants.URL_BASE + "#" + ASC.Files.Common.fixHash(entryId);
                    rowLink.attr("href", entryUrl);
                }

                if (ASC.Files.Folders.folderContainer == "trash"
                    || entryData.error) {
                    rowLink.attr("href", ASC.Files.Constants.URL_BASE + "#" + ASC.Files.Folders.currentFolder.id);
                }

                if (!jq("#filesMainContent").hasClass("without-share")
                    && (ASC.Files.Folders.folderContainer == "forme" && !ASC.Files.UI.accessEdit(entryData, entryObj)
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

        if (e.shiftKey && ASC.Files.UI.lastSelectedEntry && ASC.Files.UI.lastSelectedEntry.entryObj) {
            var i1 = jq(".file-row").index(entryObj);
            var i2 = jq(".file-row").index(ASC.Files.UI.lastSelectedEntry.entryObj);

            var minItem = Math.min(i1, i2);
            var maxItem = Math.max(i1, i2);

            jq(".file-row:lt(" + maxItem + "):gt(" + minItem + ")").each(function () {
                ASC.Files.UI.selectRow(jq(this), !jq(this).hasClass("row-selected"));
            });
        }
        ASC.Files.UI.selectRow(entryObj, !entryObj.hasClass("row-selected"));
        ASC.Files.UI.updateMainContentHeader();

        ASC.Files.UI.lastSelectedEntry = { entryObj: entryObj };

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

        return true;
    };

    var resetSelectAll = function (param) {
        jq("#filesSelectAllCheck").prop("checked", param === true);

        jq("#filesSelectAllCheck").prop("indeterminate", param !== true && jq("#filesMainContent .file-row .checkbox input:checked").length != 0);
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

    var accessEdit = function (entryData, entryObj) {
        if (Teamlab.profile.isOutsider) {
            return false;
        }

        entryData = entryData ||
            (entryObj
                ? ASC.Files.UI.getObjectData(entryObj)
                : ASC.Files.Folders.currentFolder);

        var access = ASC.Files.Constants.ADMIN || !entryData || entryData.create_by_id == Teamlab.profile.id;

        if (entryData
            && entryData.entryType == "folder"
            && (entryData.entryId === ASC.Files.Constants.FOLDER_ID_SHARE
                || entryData.entryId === ASC.Files.Constants.FOLDER_ID_PROJECT
                || entryData.entryId === ASC.Files.Constants.FOLDER_ID_TRASH)) {
            access = false;
        }

        var entryId = entryData ? entryData.entryId : null;
        var entryType = entryData ? entryData.entryType : null;
        if (entryType == "folder") {
            if (entryId == ASC.Files.Constants.FOLDER_ID_COMMON_FILES && !ASC.Files.Constants.ADMIN) {
                return false;
            }

            if (entryId == ASC.Files.Constants.FOLDER_ID_SHARE) {
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
            default:
                return access;
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

        if (ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_PROJECT) {
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
        clearTimeout(ASC.Files.UI.timeCheckEditing);

        var list = jq("#filesMainContent .file-row.on-edit");
        if (list.length == 0) {
            return;
        }

        Encoder.EncodeType = "!entity";
        var data = {
            entry: jq(list).map(function (i, item) {
                var fileId = ASC.Files.UI.getObjectData(item).entryId;
                return Encoder.htmlEncode(fileId);
            }).toArray()
        };
        Encoder.EncodeType = "entity";

        ASC.Files.ServiceManager.checkEditing(ASC.Files.ServiceManager.events.CheckEditing, { list: data.entry }, { stringList: data });
    };

    var displayEntryTooltip = function (entryObj, entryType, entryId) {
        entryObj = entryObj || ASC.Files.UI.getEntryObject(entryType, entryId);

        var entryData = ASC.Files.UI.getObjectData(entryObj);

        Encoder.EncodeType = "!entity";
        var jsonData = {
            entryTooltip:
                {
                    type: entryType,
                    title: Encoder.htmlEncode(entryData.title),
                    create_by: Encoder.htmlEncode(entryData.create_by),
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
                    length: entryData.content_length, //file
                    error: entryData.error,

                    provider_key: entryData.provider_key,
                    total_files: parseInt(entryObj.find(".countFiles").html()) || 0, //folder
                    total_sub_folder: parseInt(entryObj.find(".countFolders").html()) || 0,//folder
                    comment: entryData.comment,
                }
        };
        Encoder.EncodeType = "entity";
        var stringData = ASC.Files.Common.jsonToXml(jsonData);

        var htmlTootlip = ASC.Files.TemplateManager.translateFromString(stringData);

        jq("#entryTooltip").html(htmlTootlip);

        var toUp = jq("#filesMainContent .file-row").length > 10 && entryObj.is("#filesMainContent .file-row:gt(-4)");
        jq.dropdownToggle().toggle(entryObj.find(".thumb-" + entryType), "entryTooltip", toUp ? -12 : 0, 0, false, null, null, null, toUp);
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
                .attr("href", ASC.Files.Constants.URL_BASE + "#" + parentFolderId)
                .attr("title", parentFolderTitle);

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
        if (typeof ASC.Files.ImageViewer != "undefined") {
            ASC.Files.ImageViewer.resetWorkspace();
        }
        show = !!show;

        if (!show) {
            jq("#treeViewContainer .selected").removeClass("selected");
            jq("#treeViewContainer .parent-selected").removeClass("parent-selected");
        }

        jq("#treeSecondary .currentCategory").removeClass("currentCategory");
        jq("#treeSecondary .active").removeClass("active");

        jq("#settingCommon").hide();
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

    var displayMoreFeaturs = function () {
        ASC.Files.UI.hideAllContent();
        LoadingBanner.hideLoading();
        ASC.Files.CreateMenu.disableMenu();
        jq("#moreFeatures").show();

        ASC.Files.UI.setDocumentTitle(ASC.Files.FilesJSResources.TitleMoreFeaturs);
    };

    var displayTariffDocsEdition = function () {
        if (!jq("#tariffLimitDocsEditionPanel").length) {
            return false;
        }
        ASC.Files.UI.blockUI("#tariffLimitDocsEditionPanel", 500, 300, 0);
        return true;
    };

    var displayHostedPartnerUnauthorized = function () {
        if (!jq("#UnauthorizedPartnerPanel").length) {
            return false;
        }
        ASC.Files.UI.blockUI("#UnauthorizedPartnerPanel", 500, 300, 0);
        return true;
    };

    var displayTariffLimitStorageExceed = function () {
        if (!jq("#tariffLimitExceedStoragePanel").length) {
            return false;
        }
        ASC.Files.UI.blockUI("#tariffLimitExceedStoragePanel", 500, 300, 0);
        return true;
    };

    var displayTariffFileSizeExceed = function () {
        if (!jq("#tariffLimitExceedFileSizePanel").length) {
            return false;
        }
        ASC.Files.UI.blockUI("#tariffLimitExceedFileSizePanel", 500, 300, 0);
        return true;
    };

    var blockUI = function (obj, width, height, top) {
        if (ASC.Files.Mouse) {
            ASC.Files.Mouse.finishMoveTo();
            ASC.Files.Mouse.finishSelecting();
        }
        return StudioBlockUIManager.blockUI(obj, width, height, top, "absolute");
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

    return {
        init: init,
        parseItemId: parseItemId,
        getEntryObject: getEntryObject,
        getObjectData: getObjectData,

        getEntryTitle: getEntryTitle,
        getObjectTitle: getObjectTitle,

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

        displayEntryTooltip: displayEntryTooltip,
        hideEntryTooltip: hideEntryTooltip,
        timeTooltip: timeTooltip,

        checkEmptyContent: checkEmptyContent,
        checkButtonBack: checkButtonBack,

        hideAllContent: hideAllContent,
        displayHelp: displayHelp,
        displayCommonSetting: displayCommonSetting,
        displayMoreFeaturs: displayMoreFeaturs,

        displayTariffDocsEdition: displayTariffDocsEdition,
        displayTariffLimitStorageExceed: displayTariffLimitStorageExceed,
        displayTariffFileSizeExceed: displayTariffFileSizeExceed,
        displayHostedPartnerUnauthorized: displayHostedPartnerUnauthorized,

        blockUI: blockUI,

        setProgressValue: setProgressValue,

        filesSelectedHandler: filesSelectedHandler,
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

        jq("#filesMainContent").on("click", ".checkbox input", function (event) {
            if (ASC.Files.Actions) {
                ASC.Files.Actions.hideAllActionPanels();
            }
            ASC.Files.UI.selectRow(jq(this), this.checked == true);
            ASC.Files.UI.updateMainContentHeader();
            jq(this).blur();
            ASC.Files.Common.cancelBubble(event);
        });

        jq(".menu-action-on-top").click(function () {
            jq(document).scrollTop(0);
            return false;
        });

        jq(document).keydown(function (event) {
            if (jq(".blockUI:visible").length != 0 ||
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

            if (code == ASC.Files.Common.keyCode.a && e.ctrlKey) {
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

            if (ASC.Files.Folders) {
                if (code == ASC.Files.Common.keyCode.deleteKey && !e.shiftKey) {
                    ASC.Files.Folders.deleteItem();
                    return false;
                }

                if (code == ASC.Files.Common.keyCode.f && e.shiftKey) {
                    ASC.Files.Folders.createFolder();
                    return false;
                }

                if (code == ASC.Files.Common.keyCode.n && e.shiftKey) {
                    ASC.Files.Folders.typeNewDoc = "document";
                    ASC.Files.Folders.createNewDoc();
                    return false;
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

        jq(".mobile-app-banner").trackEvent("mobileApp-banner", "action-click", "app-store");
    });
})(jQuery);