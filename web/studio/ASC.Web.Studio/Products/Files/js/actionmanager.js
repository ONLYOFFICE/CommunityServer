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
                    function () {
                        jq(".row-selected .menu-small.active").removeClass("active");
                        jq(".row-selected.row-lonely-select").removeClass("row-lonely-select");
                    });
            });

            jq.dropdownToggle(
                {
                    switcherSelector: "#mainContentHeader .menuActionSelectAll",
                    dropdownID: "filesSelectorPanel",
                    anchorSelector: ".menuActionSelectAll",
                    addTop: 4
                });

            jq.dropdownToggle({
                addLeft: 4,
                addTop: -4,
                afterShowFunction: function () {
                    if (jq("#filesGetLink").is(":visible")) {
                        var url = ASC.Files.Actions.currentEntryData.entryObject.find(".entry-title .name a").prop("href");

                        ASC.Files.Actions.clipGetLink = ASC.Clipboard.destroy(ASC.Files.Actions.clipGetLink);
                        ASC.Files.Actions.clipGetLink = ASC.Clipboard.create(url, "filesGetLink", {
                            onComplete: function () {
                                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.Resource.LinkCopySuccess);
                                ASC.Files.Actions.hideAllActionPanels();
                            }
                        });
                    }
                },
                dropdownID: "filesAccessPanel",
                sideToggle: true,
                switcherSelector: "#filesAccess",
                toggleOnOver: true,
            });

            jq.dropdownToggle({
                addLeft: 4,
                addTop: -4,
                dropdownID: "filesVersionPanel",
                sideToggle: true,
                switcherSelector: "#filesVersion",
                toggleOnOver: true,
            });

            jq.dropdownToggle({
                addLeft: 4,
                addTop: -4,
                dropdownID: "filesMovePanel",
                sideToggle: true,
                switcherSelector: "#filesMove",
                toggleOnOver: true,
            });

            jq.dropdownToggle({
                addLeft: 4,
                addTop: -4,
                dropdownID: "foldersMovePanel",
                sideToggle: true,
                switcherSelector: "#foldersMove",
                toggleOnOver: true,
            });

            jq.dropdownToggle({
                addLeft: 4,
                addTop: -4,
                afterShowFunction: function () {
                    if (jq("#foldersGetLink").is(":visible")) {
                        var url = ASC.Files.Actions.currentEntryData.entryObject.find(".entry-title .name a").prop("href");

                        ASC.Files.Actions.clipGetLink = ASC.Clipboard.destroy(ASC.Files.Actions.clipGetLink);
                        ASC.Files.Actions.clipGetLink = ASC.Clipboard.create(url, "foldersGetLink", {
                            onComplete: function () {
                                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.Resource.LinkCopySuccess);
                                ASC.Files.Actions.hideAllActionPanels();
                            }
                        });
                    }
                },
                dropdownID: "foldersAccessPanel",
                sideToggle: true,
                switcherSelector: "#foldersAccess",
                toggleOnOver: true,
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
            var e = jq.fixEvent(event),
                dropdownItem = jq("#filesActionsPanel"),
                correctionX = document.body.clientWidth - (e.pageX - pageXOffset + dropdownItem.innerWidth()) > 0 ? 0 : dropdownItem.innerWidth(),
                correctionY = document.body.clientHeight - (e.pageY - pageYOffset + dropdownItem.innerHeight()) > 0 ? 0 : dropdownItem.innerHeight();

            dropdownItem.css({
                "top": e.pageY - correctionY,
                "left": e.pageX - correctionX
            });

            dropdownItem.toggle();
        }
    };

    var showActionsPanel = function (event, entryData) {
        ASC.Files.Actions.clipGetLink = ASC.Clipboard.destroy(ASC.Files.Actions.clipGetLink);

        var e = jq.fixEvent(event);

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
                #filesConvert,\
                #filesDownload,\
                #filesAccess,\
                #filesGetLink,\
                #filesShareAccess,\
                #filesChangeOwner,\
                #filesLock,\
                #filesUnlock,\
                #filesDocuSign,\
                #filesUnsubscribe,\
                #filesVersion,\
                #filesCompleteVersion,\
                #filesVersions,\
                #filesMove,\
                #filesMoveto,\
                #filesCopyto,\
                #filesCopy,\
                #filesRename,\
                #filesRestore,\
                #filesRemove").show().removeClass("display-none");

            var entryTitle = entryData.title;

            if (!ASC.Files.Utility.GetConvertFormats(entryTitle).length) {
                jq("#filesConvert").hide().addClass("display-none");
            }

            if (!ASC.Files.Utility.CanWebView(entryTitle)
                && (typeof ASC.Files.ImageViewer == "undefined" || !ASC.Files.Utility.CanImageView(entryTitle))) {
                jq("#filesOpen").hide().addClass("display-none");
            }

            if (!ASC.Files.ThirdParty || !ASC.Files.ThirdParty.docuSignAttached()
                || jq.inArray(ASC.Files.Utility.GetFileExtension(entryTitle), ASC.Files.Constants.DocuSignFormats) == -1) {
                jq("#filesDocuSign").hide();
            }

            if (entryObj.find("#contentVersions").length != 0
                || ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty()) {
                jq("#filesVersions").hide().addClass("display-none");
            }

            var editingFile = ASC.Files.UI.editingFile(entryObj);
            if (editingFile) {
                jq("#filesCompleteVersion,\
                    #filesMoveto,\
                    #filesRemove").hide().addClass("display-none");

                if (ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty()) {
                    jq("#filesRename").hide();
                }
            }

            var lockedForMe = ASC.Files.UI.lockedForMe(entryObj);
            if (!ASC.Files.UI.editableFile(entryData)
                || editingFile && (!ASC.Files.Utility.CanCoAuhtoring(entryTitle) || entryObj.hasClass("on-edit-alone"))
                || lockedForMe) {
                jq("#filesEdit").hide().addClass("display-none");
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
                    #filesDocuSign,\
                    #filesVersions,\
                    #filesMoveto,\
                    #filesCopyto,\
                    #filesRename").hide().addClass("display-none");
            } else {
                jq("#filesRestore").hide().addClass("display-none");
            }

            if (!accessibleObj || lockedForMe) {
                jq("#filesCompleteVersion,\
                    #filesRename,\
                    #filesLock,\
                    #filesUnlock").hide().addClass("display-none");
            }
            
            if (Teamlab.profile.isVisitor === true) {
                jq("#filesLock,\
                    #filesDocuSign,\
                    #filesCompleteVersion").hide().addClass("display-none");
            }

            if (ASC.Files.Folders.folderContainer != "corporate") {
                jq("#filesChangeOwner").hide().addClass("display-none");
            }

            if (ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty(entryData)) {
                jq("#filesChangeOwner,\
                    #filesCompleteVersion,\
                    #filesCopy").hide().addClass("display-none");
            }

            if (!accessAdminObj || lockedForMe) {
                jq("#filesChangeOwner,\
                    #filesMoveto,\
                    #filesRemove").hide().addClass("display-none");
            }

            if (entryObj.is(".without-share *, .without-share")) {
                jq("#filesShareAccess").hide().addClass("display-none");
            }

            if (ASC.Files.Folders.folderContainer != "forme") {
                jq("#filesUnsubscribe").hide().addClass("display-none");
            }

            if (!ASC.Files.UI.accessEdit()) {
                jq("#filesCopy").hide().addClass("display-none");
            }

            if (!jq("#filesAccessPanel li:not(.display-none)").length) {
                jq("#filesAccess").hide().addClass("display-none");
            }
            if (!jq("#filesVersionPanel li:not(.display-none)").length) {
                jq("#filesVersion").hide().addClass("display-none");
            }
            if (!jq("#filesMovePanel li:not(.display-none)").length) {
                jq("#filesMove").hide().addClass("display-none");
            }

            jq("#actionPanelFiles").show();

        } else {
            jq("#foldersOpen,\
                #foldersDownload,\
                #foldersAccess,\
                #foldersGetLink,\
                #foldersShareAccess,\
                #foldersChangeOwner,\
                #foldersUnsubscribe,\
                #foldersMove,\
                #foldersMoveto,\
                #foldersCopyto,\
                #foldersRename,\
                #foldersRestore,\
                #foldersRemove,\
                #foldersRemoveThirdparty,\
                #foldersChangeThirdparty").show().removeClass("display-none");

            if (ASC.Files.Folders.folderContainer == "trash") {
                jq("#foldersOpen,\
                    #foldersGetLink,\
                    #foldersShareAccess,\
                    #foldersMove,\
                    #foldersMoveto,\
                    #foldersCopyto,\
                    #foldersRename").hide().addClass("display-none");
            } else {
                jq("#foldersRestore").hide().addClass("display-none");
            }

            if (!accessibleObj || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_PROJECT) {
                jq("#foldersRename,\
                    #foldersRemoveThirdparty,\
                    #foldersChangeThirdparty").hide().addClass("display-none");
            }

            if (ASC.Files.Folders.folderContainer != "corporate") {
                jq("#foldersChangeOwner").hide().addClass("display-none");
            }

            if (!accessAdminObj) {
                jq("#foldersChangeOwner,\
                    #foldersMoveto,\
                    #foldersRemove,\
                    #foldersRemoveThirdparty,\
                    #foldersChangeThirdparty").hide().addClass("display-none");
            }

            if (entryObj.is(".without-share *, .without-share")) {
                jq("#foldersShareAccess").hide().addClass("display-none");
            }

            if (ASC.Files.Folders.folderContainer != "forme") {
                jq("#foldersUnsubscribe").hide().addClass("display-none");
            }

            if (ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty(entryData)) {
                jq("#foldersChangeOwner").hide().addClass("display-none");

                if (ASC.Files.ThirdParty.isThirdParty()
                    || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_SHARE) {
                    jq("#foldersRemoveThirdparty,\
                        #foldersChangeThirdparty").hide().addClass("display-none");
                } else {
                    if (ASC.Desktop) {
                        jq("#foldersRemoveThirdparty,\
                        #foldersChangeThirdparty").hide().addClass("display-none");
                    }

                    if (entryData.create_by_id != Teamlab.profile.id) {
                        jq("#foldersChangeThirdparty").hide().addClass("display-none");
                    }

                    jq("#foldersRemove,\
                        #foldersMoveto").hide().addClass("display-none");

                    if (entryData.error) {
                        jq("#foldersOpen,\
                            #foldersDownload,\
                            #foldersCopyto,\
                            #foldersRename").hide().addClass("display-none");
                    }
                }
            } else {
                jq("#foldersRemoveThirdparty,\
                    #foldersChangeThirdparty").hide().addClass("display-none");
            }

            if (!jq("#foldersAccessPanel li:not(.display-none)").length) {
                jq("#foldersAccess").hide().addClass("display-none");
            }

            if (!jq("#foldersMovePanel li:not(.display-none)").length) {
                jq("#foldersMove").hide().addClass("display-none");
            }

            jq("#actionPanelFolders").show();
        }

        if (!ASC.Clipboard.enable) {
            jq("#filesGetLink, #foldersGetLink").remove();
        }

        jq.showDropDownByContext(e, target, jq("#filesActionPanel"));

        if (target.is(".menu-small")) {
            entryObj.addClass("row-lonely-select");
        }

        return true;
    };

    var onContextMenu = function (event) {
        ASC.Files.Actions.hideAllActionPanels();

        var e = jq.fixEvent(event);

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
        jq("#filesMainContent .menu-small").removeClass("active");
        var count = jq("#filesMainContent .file-row.row-selected").length;

        if (count > 1 && entryObj.hasClass("row-selected")) {
            ASC.Files.Actions.showActionsViewPanel(event);
        } else {
            ASC.Files.Actions.showActionsPanel(event, entryData);
        }

        return false;
    };

    var hideAllActionPanels = function () {
        ASC.Files.Actions.clipGetLink = ASC.Clipboard.destroy(ASC.Files.Actions.clipGetLink);

        jq(".studio-action-panel:visible").hide();
        jq("#filesMainContent .file-row.row-lonely-select").removeClass("row-lonely-select");
        jq("#filesMainContent .file-row .menu-small").removeClass("active");
        ASC.Files.UI.hideEntryTooltip();
    };

    var checkEditFile = function (fileId, winEditor) {
        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);
        ASC.Files.UI.lockEditFile(fileObj, true);

        var url = ASC.Files.Utility.GetFileWebEditorUrl(fileId);

        if (winEditor && winEditor.location) {
            winEditor.location.href = url;
        } else {
            winEditor = window.open(url, "_blank");
        }

        var onloadFunction = function () {
            var fileIdLocal = fileId;
            if (fileIdLocal) {
                ASC.Files.UI.lockEditFileById(fileIdLocal, true);
                ASC.Files.UI.checkEditing();
                ASC.Files.Socket.subscribeChangeEditors(fileIdLocal);
            }
        };

        if (winEditor == undefined) {

            ASC.Files.UI.lockEditFile(fileObj, true);
            ASC.Files.Socket.subscribeChangeEditors(fileId);

            ASC.Files.UI.checkEditingDefer();

        } else {

            try {
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
        }

        return ASC.Files.Marker.removeNewIcon("file", fileId);
    };

    var showMoveToSelector = function (isCopy) {
        ASC.Files.Folders.isCopyTo = (isCopy === true);
        ASC.Files.Actions.hideAllActionPanels();

        if (ASC.Files.Folders.folderContainer != "trash"
            && !ASC.Files.Folders.isCopyTo) {
            if (!ASC.Files.UI.accessEdit()) {
                var listWithAccess = jq.grep(jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)"), function (item) {
                    return ASC.Files.UI.accessDelete(item);
                });

                if (!listWithAccess.length) {
                    ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoMoveGroup.format(0));
                    return;
                }
            }
        }

        ASC.Files.Tree.updateTreePath();

        if (!isCopy
            && jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):not(.on-edit):has(.checkbox input:checked)").length == 0) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoMoveGroup.format(0));
            return;
        }

        if (ASC.Files.ThirdParty
            && !ASC.Files.Folders.isCopyTo
            && !ASC.Files.ThirdParty.isThirdParty()
            && jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):not(.third-party-entry):has(.checkbox input:checked)").length == 0) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoMoveGroup.format(0));
            return;
        }

        if (jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)").length == 0) {
            return;
        }

        jq("#treeViewPanelSelector").removeClass("without-third-party");

        if (!jq("#treeViewPanelSelector").next().is("#mainContentHeader")) {
            jq("#treeViewPanelSelector").insertBefore("#mainContentHeader");
        }

        jq.dropdownToggle().toggle(".menuActionSelectAll", "treeViewPanelSelector");

        jq("#treeViewPanelSelector").scrollTo(jq("#treeViewPanelSelector").find(".tree-node" + ASC.Files.UI.getSelectorId(ASC.Files.Folders.currentFolder.id)));

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

        jq("#filesChangeOwner, #foldersChangeOwner").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.changeOwnerDialog(ASC.Files.Actions.currentEntryData);
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

        jq("#filesDocuSign").click(function () {
            ASC.Files.ThirdParty.showDocuSignDialog(ASC.Files.Actions.currentEntryData);
            ASC.Files.Actions.hideAllActionPanels();
            return false;
        });
    });
})(jQuery);