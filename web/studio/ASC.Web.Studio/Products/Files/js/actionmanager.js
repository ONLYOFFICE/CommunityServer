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


window.ASC.Files.Actions = (function () {
    var isInit = false;
    var clipGetLink = null;

    var currentEntryData = null;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            jq(document).click(function (event) {
                jq.dropdownToggle().registerAutoHide(event, "#filesActionsPanel", "#filesActionsPanel");
                jq.dropdownToggle().registerAutoHide(event, ".row-selected .menu-small", "#filesActionPanel",
                    function (e) {
                        jq(".row-selected.row-lonely-select").removeClass("row-lonely-select");
                        e = ASC.Files.Common.fixEvent(e);
                        return e.button != 2;
                    });
            });

            jq.dropdownToggle(
                {
                    switcherSelector: "#mainContentHeader .down_arrow",
                    dropdownID: "filesSelectorPanel",
                    anchorSelector: ".menuActionSelectAll",
                    addTop: 4
                });
        }
    };

    /* Methods*/

    var showActionsViewPanel = function (event) {
        jq("#buttonUnsubscribe, #buttonDelete, #buttonMoveto, #buttonCopyto, #buttonShare").hide();
        jq("#mainContentHeader .unlockAction").removeClass("unlockAction");
        var count = jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)").length;
        var countWithRights = count;
        var countIsNew = 0;
        var onlyThirdParty = (ASC.Files.ThirdParty && !ASC.Files.ThirdParty.isThirdParty());
        var countThirdParty = 0;
        var canConvert = false;
        var countCanShare = 0;

        jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)").each(function () {
            var entryObj = jq(this);

            if (ASC.Files.UI.editingFile(entryObj) || !ASC.Files.UI.accessDelete(entryObj) || ASC.Files.UI.lockedForMe(entryObj)) {
                countWithRights--;
            }

            if (entryObj.find(".is-new:visible").length > 0) {
                countIsNew++;
            }

            if (ASC.Files.ThirdParty) {
                var entryData = ASC.Files.UI.getObjectData(entryObj);
                if (!ASC.Files.ThirdParty.isThirdParty() && ASC.Files.ThirdParty.isThirdParty(entryData)) {
                    countThirdParty++;
                } else {
                    onlyThirdParty = false;
                }
            } else {
                onlyThirdParty = false;
            }

            if (!canConvert && !entryObj.hasClass("folder-row")) {
                entryData = entryData || ASC.Files.UI.getObjectData(entryObj);
                var entryTitle = entryData.title;
                var formats = ASC.Files.Utility.GetConvertFormats(entryTitle);
                canConvert = formats.length > 0;
            }

            if (!ASC.Resources.Master.Personal && entryObj.is(":not(.without-share)")) {
                countCanShare++;
            }
        });

        if (count > 0) {
            jq("#buttonDownload, #buttonUnsubscribe, #buttonRestore, #buttonCopyto").show().find("span").html(count);
            jq("#mainDownload, #mainUnsubscribe, #mainRestore, #mainCopy").addClass("unlockAction");
            if (ASC.Files.Folders.folderContainer != "forme") {
                jq("#buttonUnsubscribe").hide();
            }
        }

        if (canConvert) {
            jq("#buttonConvert").show().find("span").html(count);
        } else {
            jq("#buttonConvert").hide();
        }
        jq("#mainConvert").toggleClass("unlockAction", canConvert);

        if (countIsNew > 0) {
            jq("#mainMarkRead").addClass("unlockAction");
        }

        if (countWithRights > 0) {
            jq("#buttonDelete, #buttonMoveto").show().find("span").html(countWithRights - countThirdParty);
            jq("#mainDelete, #mainMove").addClass("unlockAction");
        }

        if (ASC.Files.Folders.folderContainer == "trash") {
            jq("#buttonDelete, #buttonMoveto, #buttonCopyto").hide();
            jq("#mainDelete, #mainMove, #mainCopy").removeClass("unlockAction");
        }

        jq("#buttonRestore, #buttonEmptyTrash").hide();
        jq("#mainRestore, #mainEmptyTrash").removeClass("unlockAction");
        if (ASC.Files.Folders.folderContainer == "trash") {
            if (count > 0) {
                jq("#buttonDownload, #buttonDelete, #buttonRestore").show();
                jq("#mainDownload, #mainDelete, #mainRestore").addClass("unlockAction");
            } else {
                jq("#buttonDownload, #buttonConvert").hide();
                jq("#mainDownload, #mainConvert").removeClass("unlockAction");
            }
            jq("#buttonEmptyTrash").show();
            jq("#mainEmptyTrash").addClass("unlockAction");
        } else if (countCanShare > 0) {
            jq("#buttonShare").show().find("span").html(countCanShare);
            jq("#mainShare").addClass("unlockAction");
        }

        if (onlyThirdParty) {
            jq("#buttonDelete, #buttonMoveto").hide();
            jq("#mainDelete, #mainMove").removeClass("unlockAction");
        }

        if (typeof event != "undefined") {
            var e = ASC.Files.Common.fixEvent(event);
            var dropdownItem = jq("#filesActionsPanel");
            var correctionX = document.body.clientWidth - (e.pageX - pageXOffset + dropdownItem.innerWidth()) > 0 ? 0 : dropdownItem.innerWidth();
            var correctionY = document.body.clientHeight - (e.pageY - pageYOffset + dropdownItem.innerHeight()) > 0 ? 0 : dropdownItem.innerHeight();

            dropdownItem.css({
                "top": e.pageY - correctionY,
                "left": e.pageX - correctionX
            });

            dropdownItem.toggle();
        }
    };

    var showActionsPanel = function (event, entryData) {
        if (ASC.Files.Actions.clipGetLink) {
            ASC.Files.Actions.clipGetLink.destroy();
        }

        var e = ASC.Files.Common.fixEvent(event);

        var target = jq(e.srcElement || e.target);

        entryData = entryData || ASC.Files.UI.getObjectData(target);

        var entryObj = entryData.entryObject;
        if (entryObj.is(".loading")) {
            return true;
        }

        ASC.Files.Actions.currentEntryData = entryData;

        ASC.Files.UI.checkSelectAll(false);
        ASC.Files.UI.selectRow(entryObj, true);
        ASC.Files.UI.updateMainContentHeader();

        var accessibleObj = ASC.Files.UI.accessEdit(entryData, entryObj);
        var accessAdminObj = ASC.Files.UI.accessDelete(entryObj);

        jq("#actionPanelFolders, #actionPanelFiles").hide();
        if (entryData.entryType === "file") {
            jq("#filesOpen,\
                #filesEdit,\
                #filesDownload,\
                #filesGetLink,\
                #filesShareAccess,\
                #filesLock,\
                #filesUnlock,\
                #filesUnsubscribe,\
                #filesCompleteVersion,\
                #filesVersions,\
                #filesMoveto,\
                #filesCopyto,\
                #filesCopy,\
                #filesRename,\
                #filesRestore,\
                #filesRemove").show();

            if (ASC.Files.Utility.GetConvertFormats(entryTitle).length) {
                jq("#filesConvert").show();
            } else {
                jq("#filesConvert").hide();
            }

            var entryTitle = entryData.title;

            if (!ASC.Files.Utility.CanWebView(entryTitle)
                && (typeof ASC.Files.ImageViewer == "undefined" || !ASC.Files.Utility.CanImageView(entryTitle))) {
                jq("#filesOpen").hide();
            }

            var countVersion = entryData.version || 1;
            if (countVersion < 2
                || ASC.Files.Folders.folderContainer == "trash"
                || entryObj.find("#contentVersions").length != 0) {
                jq("#filesVersions").hide();
            }

            var editingFile = ASC.Files.UI.editingFile(entryObj);
            if (editingFile) {
                jq("#filesCompleteVersion,\
                    #filesVersions,\
                    #filesMoveto,\
                    #filesRename,\
                    #filesRemove").hide();
            }

            if (!ASC.Files.UI.editableFile(entryData)
                || editingFile && (!ASC.Files.Utility.CanCoAuhtoring(entryTitle) || entryObj.hasClass("on-edit-alone"))) {
                jq("#filesEdit").hide();
            }

            jq("#filesLock").toggle(!entryObj.hasClass("file-locked"));
            jq("#filesUnlock").toggle(entryObj.hasClass("file-locked"));

            if (ASC.Files.Folders.folderContainer == "trash") {
                jq("#filesOpen,\
                    #filesEdit,\
                    #filesGetLink,\
                    #filesShareAccess,\
                    #filesCompleteVersion,\
                    #filesLock,\
                    #filesUnlock,\
                    #filesVersions,\
                    #filesMoveto,\
                    #filesCopyto,\
                    #filesRename").hide();

                jq("#filesRemove, #filesRestore").show();
            } else {
                jq("#filesRestore").hide();
                if (jq.browser.mobile) {
                    jq("#filesGetLink").hide();
                }
            }

            var lockedForMe = ASC.Files.UI.lockedForMe(entryObj);
            if (!accessibleObj || lockedForMe) {
                jq("#filesEdit,\
                    #filesCompleteVersion,\
                    #filesRename,\
                    #filesLock,\
                    #filesUnlock").hide();
            }
            
            if (Teamlab.profile.isVisitor === true) {
                jq("#filesLock,\
                    #filesCompleteVersion").hide();
            }

            if (ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty(entryData)) {
                jq("#filesCompleteVersion,\
                    #filesCopy").hide();
            }

            if (!accessAdminObj || lockedForMe) {
                jq("#filesMoveto,\
                    #filesRemove").hide();
            }

            if (entryObj.is(".without-share *, .without-share")) {
                jq("#filesShareAccess").hide();
            }

            if (ASC.Files.Folders.folderContainer != "forme") {
                jq("#filesUnsubscribe").hide();
            }

            if (!ASC.Files.UI.accessEdit()) {
                jq("#filesCopy").hide();
            }

            jq("#actionPanelFiles").show();

        } else {
            jq("#foldersOpen,\
                #foldersDownload,\
                #foldersShareAccess,\
                #foldersUnsubscribe,\
                #foldersMoveto,\
                #foldersCopyto,\
                #foldersRename,\
                #foldersRestore,\
                #foldersRemove,\
                #foldersRemoveThirdparty,\
                #foldersChangeThirdparty").show();

            if (ASC.Files.Folders.folderContainer == "trash") {
                jq("#foldersOpen,\
                    #foldersShareAccess,\
                    #foldersMoveto,\
                    #foldersCopyto,\
                    #foldersRename").hide();
            } else {
                jq("#foldersRestore").hide();
            }

            if (!accessibleObj || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_PROJECT) {
                jq("#foldersRename,\
                    #foldersRemoveThirdparty,\
                    #foldersChangeThirdparty").hide();
            }

            if (!accessAdminObj) {
                jq("#foldersMoveto,\
                    #foldersRemove,\
                    #foldersRemoveThirdparty,\
                    #foldersChangeThirdparty").hide();
            }

            if (entryObj.is(".without-share *, .without-share")) {
                jq("#foldersShareAccess").hide();
            }

            if (ASC.Files.Folders.folderContainer != "forme") {
                jq("#foldersUnsubscribe").hide();
            }

            if (ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty(entryData)) {
                if (ASC.Files.ThirdParty.isThirdParty()
                    || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_SHARE) {
                    jq("#foldersRemoveThirdparty,\
                        #foldersChangeThirdparty").hide();
                } else {
                    jq("#foldersRemove,\
                        #foldersMoveto").hide();

                    if (entryData.error) {
                        jq("#foldersOpen,\
                            #foldersDownload,\
                            #foldersCopyto,\
                            #foldersRename").hide();
                    }
                }
            } else {
                jq("#foldersRemoveThirdparty,\
                    #foldersChangeThirdparty").hide();
            }

            jq("#actionPanelFolders").show();
        }

        var dropdownItem = jq("#filesActionPanel");

        if (target.is(".menu-small")) {
            entryObj.addClass("row-lonely-select");

            dropdownItem.css(
                {
                    "top": target.offset().top + target.outerHeight(),
                    "left": "auto",
                    "right": jq(window).width() - target.offset().left - target.width() - 2,
                    "margin": "0 -8px 0 0"
                });
        } else {
            var correctionX = document.body.clientWidth - (e.pageX - pageXOffset + dropdownItem.innerWidth()) > 0 ? 0 : dropdownItem.innerWidth();
            var correctionY = document.body.clientHeight - (e.pageY - pageYOffset + dropdownItem.innerHeight()) > 0 ? 0 : dropdownItem.innerHeight();

            dropdownItem.css(
                {
                    "top": e.pageY - correctionY,
                    "left": e.pageX - correctionX,
                    "right": "auto",
                    "margin": "0"
                });
        }

        dropdownItem.toggle();

        if (!jq.browser.flashEnabled()) {
            jq("#filesGetLink").remove();
        }

        if (jq("#filesGetLink").is(":visible")) {
            var url = entryObj.find(".entry-title .name a").prop("href");
            var offsetLink = jq("#filesGetLink").offset();
            var offsetDialog = jq("#filesActionPanel").offset();

            if (typeof ZeroClipboard != 'undefined' && ZeroClipboard.moviePath === 'ZeroClipboard.swf') {
                ZeroClipboard.setMoviePath(ASC.Resources.Master.ZeroClipboardMoviePath);
            }

            ASC.Files.Actions.clipGetLink = new ZeroClipboard.Client();
            ASC.Files.Actions.clipGetLink.setText(url);
            ASC.Files.Actions.clipGetLink.glue("filesGetLink", "filesGetLink",
                {
                    zIndex: 670,
                    left: offsetLink.left - offsetDialog.left + "px",
                    top: offsetLink.top - offsetDialog.top + "px"
                });

            ASC.Files.Actions.clipGetLink.addEventListener("onComplete", function () {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoCopyLink);
                ASC.Files.Actions.hideAllActionPanels();
            });
            ASC.Files.Actions.clipGetLink.addEventListener("onMouseOver", function () {
                jq("#filesGetLink").children("a").css({ "text-decoration": "underline" });
            });
            ASC.Files.Actions.clipGetLink.addEventListener('onMouseOut', function () {
                jq("#filesGetLink").children("a").css({ "text-decoration": "" });
            });
        }

        return true;
    };

    var onContextMenu = function (event) {
        ASC.Files.Actions.hideAllActionPanels();

        var e = ASC.Files.Common.fixEvent(event);

        if (typeof e == "undefined" || !e) {
            return true;
        }

        var target = jq(e.srcElement || e.target);
        if (target.is("input[type=text]")) {
            return true;
        }

        var entryData = ASC.Files.UI.getObjectData(target);
        if (!entryData) {
            if (target.is("a[href]")) {
                return true;
            }
            return false;
        }
        var entryObj = entryData.entryObject;

        if (entryObj.hasClass("new-folder") || entryObj.hasClass("row-rename") || entryObj.hasClass("error-entry")) {
            return true;
        }

        jq("#filesMainContent .row-hover").removeClass("row-hover");
        jq("#filesMainContent .row-lonely-select").removeClass("row-lonely-select");
        var count = jq("#filesMainContent .file-row.row-selected").length;

        if (count > 1 && entryObj.hasClass("row-selected")) {
            ASC.Files.Actions.showActionsViewPanel(event);
        } else {
            ASC.Files.Actions.showActionsPanel(event, entryData);
        }

        return false;
    };

    var hideAllActionPanels = function () {
        if (ASC.Files.Actions.clipGetLink) {
            ASC.Files.Actions.clipGetLink.destroy();
        }

        jq(".studio-action-panel:visible").hide();
        jq("#filesMainContent .file-row.row-lonely-select").removeClass("row-lonely-select");
        ASC.Files.UI.hideEntryTooltip();
    };

    var checkEditFile = function (fileId, winEditor, isNew) {
        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);
        ASC.Files.UI.lockEditFile(fileObj, true);
        ASC.Files.UI.checkEditing();

        var url = ASC.Files.Utility.GetFileWebEditorUrl(fileId) + (isNew === true ? "&new=true" : "");

        if (winEditor && winEditor.location) {
            winEditor.location.href = url;
        } else {
            winEditor = window.open(url, "_blank");
        }

        try {
            var onloadFunction = function () {
                var fileIdLocal = fileId;
                if (fileIdLocal) {
                    ASC.Files.UI.lockEditFileById(fileIdLocal, true);
                    ASC.Files.UI.checkEditing();
                }
            };
            winEditor.onload = onloadFunction;

            if (jq.browser.msie) {
                var bodyIe;
                var checkIeLoaded = function () {
                    bodyIe = winEditor.document.getElementsByTagName("body");
                    if (bodyIe[0] == null) {
                        setTimeout(checkIeLoaded, 10);
                    } else {
                        onloadFunction();
                    }
                };
                checkIeLoaded();
            }
        } catch (ex) {
        }

        try {
            winEditor.onunload = function () {
                if (fileId) {
                    ASC.Files.UI.checkEditing();
                }
            };
        } catch (ex) {
        }

        return ASC.Files.Marker.removeNewIcon("file", fileId);
    };

    var showMoveToSelector = function (isCopy) {
        ASC.Files.Folders.isCopyTo = (isCopy === true);

        if (ASC.Files.Folders.folderContainer != "trash"
            && !ASC.Files.Folders.isCopyTo) {
            if (!ASC.Files.UI.accessEdit()) {
                var listWithAccess = jq.grep(jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)"), function (i, item) {
                    return ASC.Files.UI.accessDelete(item);
                });

                if (!listWithAccess.length) {
                    return;
                }
            }
        }

        ASC.Files.Tree.updateTreePath();
        ASC.Files.Actions.hideAllActionPanels();

        if (!isCopy
            && jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):not(.on-edit):has(.checkbox input:checked)").length == 0) {
            return;
        }

        if (ASC.Files.ThirdParty
            && !ASC.Files.Folders.isCopyTo
            && !ASC.Files.ThirdParty.isThirdParty()
            && jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):not(.third-party-entry):has(.checkbox input:checked)").length == 0) {
            return;
        }

        if (jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)").length == 0) {
            return;
        }

        jq("#treeViewPanelSelector").removeClass("without-third-party");

        jq.dropdownToggle().toggle(".menuActionSelectAll", "treeViewPanelSelector");

        jq("body").bind("click", ASC.Files.Actions.registerHideTree);
    };

    var registerHideTree = function (event) {
        if (!jq((event.target) ? event.target : event.srcElement).parents().addBack()
            .is("#treeViewPanelSelector, #foldersMoveto, #filesMoveto, #foldersCopyto, #filesCopyto,\
                 #foldersRestore, #filesRestore, #buttonMoveto, #buttonCopyto, #buttonRestore,\
                 #mainMove, #mainCopy, #mainRestore")) {
            ASC.Files.Actions.hideAllActionPanels();
            jq("body").unbind("click", ASC.Files.Actions.registerHideTree);
        }
    };

    return {
        init: init,
        showActionsViewPanel: showActionsViewPanel,
        showActionsPanel: showActionsPanel,
        onContextMenu: onContextMenu,

        checkEditFile: checkEditFile,

        hideAllActionPanels: hideAllActionPanels,

        clipGetLink: clipGetLink,
        currentEntryData: currentEntryData,

        showMoveToSelector: showMoveToSelector,
        registerHideTree: registerHideTree,
    };
})();

(function ($) {
    ASC.Files.Actions.init();
    $(function () {

        jq("#filesSelectAll").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.UI.checkSelectAll(true);
        });

        jq("#filesSelectFile, #filesSelectFolder, #filesSelectDocument,\
            #filesSelectPresentation, #filesSelectSpreadsheet, #filesSelectImage, #filesSelectArchive").click(function () {
            var filter = this.id.replace("filesSelect", "").toLowerCase();
            ASC.Files.UI.checkSelect(filter);
        });

        jq("#filesDownload, #foldersDownload").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.download(ASC.Files.Actions.currentEntryData.entryType, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesRename, #foldersRename").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.rename(ASC.Files.Actions.currentEntryData.entryType, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesRemove, #foldersRemove").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.deleteItem(ASC.Files.Actions.currentEntryData.entryType, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesShareAccess, #foldersShareAccess").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Share.getSharedInfo(ASC.Files.Actions.currentEntryData.entryType + "_" + ASC.Files.Actions.currentEntryData.id, ASC.Files.Actions.currentEntryData.title);
        });

        jq("#filesUnsubscribe, #foldersUnsubscribe").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Share.unSubscribeMe(ASC.Files.Actions.currentEntryData.entryType, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesConvert").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Converter.showToConvert(ASC.Files.Actions.currentEntryData.entryObject);
        });

        jq("#filesOpen").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.clickOnFile(ASC.Files.Actions.currentEntryData, false);
            ASC.Files.Actions.currentEntryData.entryObject.removeClass("isNewForWebEditor");
            return false;
        });

        jq("#filesLock, #filesUnlock").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.lockFile(ASC.Files.Actions.currentEntryData.entryObject, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesCompleteVersion").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.versionComplete(ASC.Files.Actions.currentEntryData.id, 0, false);
        });

        jq("#filesVersions").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.showVersions(ASC.Files.Actions.currentEntryData.entryObject, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesEdit").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.clickOnFile(ASC.Files.Actions.currentEntryData, true);
        });

        jq("#filesCopy").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.createDuplicate(ASC.Files.Actions.currentEntryData);
        });

        jq("#foldersOpen").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.clickOnFolder(ASC.Files.Actions.currentEntryData.id);
        });

        jq("#foldersRemoveThirdparty").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.ThirdParty.showDeleteDialog(null, null, null, ASC.Files.Actions.currentEntryData.title, ASC.Files.Actions.currentEntryData);
        });

        jq("#foldersChangeThirdparty").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.ThirdParty.showChangeDialog(ASC.Files.Actions.currentEntryData);
        });

        jq("#filesMainContent").bind("contextmenu", function (event) {
            return ASC.Files.Actions.onContextMenu(event);
        });

        jq("#filesMainContent").on("click", ".menu-small", ASC.Files.Actions.showActionsPanel);

        jq("#studioPageContent").on("click",
            "#buttonMoveto, #buttonCopyto, #buttonRestore,\
            #mainMove.unlockAction, #mainCopy.unlockAction, #mainRestore.unlockAction,\
            #filesMoveto, #filesRestore, #filesCopyto,\
            #foldersMoveto, #foldersRestore, #foldersCopyto",
            function () {
                ASC.Files.Actions.showMoveToSelector(this.id == "buttonCopyto" || this.id == "mainCopy" || this.id == "filesCopyto" || this.id == "foldersCopyto");
            });
    });
})(jQuery);