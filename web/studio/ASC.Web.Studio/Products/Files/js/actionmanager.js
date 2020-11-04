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


window.ASC.Files.Actions = (function () {
    var isInit = false;
    var clipGetLink = null;
    var clipGetExternalLink = null;

    var currentEntryData = null;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            jq(document).click(function (event) {
                jq.dropdownToggle().registerAutoHide(event, "#filesActionsPanel", "#filesActionsPanel",
                    function () {
                        ASC.Files.Mouse.disableHover = false;
                    });
                jq.dropdownToggle().registerAutoHide(event, ".row-selected .menu-small", "#filesActionPanel",
                    function () {
                        jq(".row-selected .menu-small.active").removeClass("active");
                        jq(".row-selected.row-lonely-select").removeClass("row-lonely-select");
                        ASC.Files.Mouse.disableHover = false;
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

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetFileSharedInfo, onGetFileSharedInfo);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SetAceFileLink, onSetAceFileLink);
        }
    };

    /* Methods*/

    var afterShowFunction = function (dropdownPanel) {
        showSeporators(dropdownPanel);
        resizeDropdownPanel(dropdownPanel);
        createClipboardLinks();
    };

    var showSeporators = function(dropdownPanel) {
        var first = !!dropdownPanel.find(".dropdown-item.first-section:visible").length;
        var second = !!dropdownPanel.find(".dropdown-item.second-section:visible").length;
        var third = !!dropdownPanel.find(".dropdown-item.third-section:visible").length;
        var fourth = !!dropdownPanel.find(".dropdown-item.fourth-section:visible").length;

        dropdownPanel.find(".dropdown-item-seporator.first-section").toggle(first && (second || third || fourth));
        dropdownPanel.find(".dropdown-item-seporator.second-section").toggle(second && (third || fourth));
        dropdownPanel.find(".dropdown-item-seporator.third-section").toggle(third && fourth);
    };

    var resizeDropdownPanel = function (dropdownPanel) {
        var content = dropdownPanel.find(".dropdown-content:visible");

        if (content.get(0).style.minWidth) return;

        var items = content.find("li");
        var itemsWidth = items.map(function () { return jq(this).width(); });
        var maxItemWidth = Math.max.apply(Math, itemsWidth);
        var correction = maxItemWidth - content.width();
        content.css({ minWidth: maxItemWidth });

        var dropdownPanelPosition = dropdownPanel.position();
        if (dropdownPanelPosition.left + dropdownPanel.outerWidth() > document.documentElement.clientWidth) {
            dropdownPanel.css({ left: dropdownPanelPosition.left - correction });
        }
    };

    var createClipboardLinks = function () {
        var link = jq("#filesGetLink:visible, #foldersGetLink:visible").first();

        if (link.length) {
            createClipboardLink(link.attr("id"));
        }

        if (jq("#filesGetExternalLink").is(":visible")) {
            getFileSharedInfo();
        }
    };

    var createClipboardLink = function (id) {
        var url = ASC.Files.Actions.currentEntryData.entryObject.find(".entry-title .name a").prop("href");

        ASC.Files.Actions.clipGetLink = ASC.Clipboard.destroy(ASC.Files.Actions.clipGetLink);

        ASC.Files.Actions.clipGetLink = ASC.Clipboard.create(url, id, {
            onComplete: function () {
                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.Resource.LinkCopySuccess);
                ASC.Files.Actions.hideAllActionPanels();
            }
        });
    };

    var getFileSharedInfo = function () {
        ASC.Files.ServiceManager.getSharedInfo(ASC.Files.ServiceManager.events.GetFileSharedInfo,
            {
                showLoading: true,
            },
            {
                stringList: {
                    entry: [ASC.Files.Actions.currentEntryData.entryType + "_" + ASC.Files.Actions.currentEntryData.id]
                }
            });
    };

    var onGetFileSharedInfo = function (jsonData, params, errorMessage) {
        if (errorMessage) {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var data = jsonData[0];

        var toggleSwitcher = jq("#filesGetExternalLink .toggle");

        toggleSwitcher.toggleClass("off", data.ace_status == 3);

        ASC.Files.Actions.clipGetExternalLink = ASC.Clipboard.destroy(ASC.Files.Actions.clipGetExternalLink);

        ASC.Files.Actions.clipGetExternalLink = ASC.Clipboard.create(data.link, "filesGetExternalLink", {
            onComplete: function () {
                if (toggleSwitcher.hasClass("off")) {
                    ASC.Files.Actions.setAceFileLink();
                }

                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.Resource.LinkCopySuccess);

                if (!ASC.Files.Actions.clipGetExternalLink.fromToggleBtn) {
                    ASC.Files.Actions.hideAllActionPanels();
                }

                ASC.Files.Actions.clipGetExternalLink.fromToggleBtn = false;
            }
        });
    };

    var setAceFileLink = function () {
        ASC.Files.ServiceManager.setAceLink(ASC.Files.ServiceManager.events.SetAceFileLink, {
            showLoading: true,
            fileId: ASC.Files.Actions.currentEntryData.entryId,
            share: jq("#filesGetExternalLink .toggle").hasClass("off") ? ASC.Files.Constants.AceStatusEnum.Read : ASC.Files.Constants.AceStatusEnum.Restrict
        });
    };

    var onSetAceFileLink = function (jsonData, params, errorMessage) {
        if (errorMessage) {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        jq("#filesGetExternalLink .toggle").toggleClass("off");
        ASC.Files.Actions.currentEntryData.entryObject.toggleClass("__active", jsonData);
    };


    var showActionsViewPanel = function (event) {
        jq("#buttonUnsubscribe, #buttonDelete, #buttonMoveto, #buttonCopyto, #buttonShare, #buttonMarkRead, #buttonSendInEmail, #buttonAddFaforite").hide();
        jq("#mainContentHeader .unlockAction").removeClass("unlockAction");
        var count = jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)").length;
        var filesCount = jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):not(.folder-row):has(.checkbox input:checked)").length;

        var countNotFavorite = 0;
        if (ASC.Files.Tree.displayFavorites()
            && ASC.Files.Folders.folderContainer != "trash") {
            countNotFavorite = jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):not(.on-favorite):not(.folder-row):has(.checkbox input:checked)").length;
        }

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

            var entryData = ASC.Files.UI.getObjectData(entryObj);
            if (ASC.Files.ThirdParty) {
                if (!ASC.Files.ThirdParty.isThirdParty() && ASC.Files.ThirdParty.isThirdParty(entryData)) {
                    countThirdParty++;
                } else {
                    onlyThirdParty = false;
                }
            } else {
                onlyThirdParty = false;
            }

            if (!canConvert && !entryObj.hasClass("folder-row")) {
                var entryTitle = entryData.title;
                var formats = ASC.Files.Utility.GetConvertFormats(entryTitle);
                canConvert = formats.length > 0 && !entryData.encrypted && entryData.content_length <= ASC.Files.Constants.AvailableFileSize;
            }

            if (!ASC.Resources.Master.Personal && entryObj.is(":not(.without-share)")
                && ASC.Files.Share && ASC.Files.Folders.currentFolder.shareable
                && (!entryData.encrypted
                    || ASC.Files.Folders.folderContainer == "privacy"
                        && ASC.Files.Utility.CanWebEncrypt(entryData.title)
                        && ASC.Desktop && ASC.Desktop.setAccess)) {
                countCanShare++;
            }
        });

        if (count > 0) {
            jq("#buttonDownload, #buttonRemoveFavorite, #buttonRemoveTemplate, #buttonUnsubscribe, #buttonRestore, #buttonCopyto").show().find("span").html(count);
            jq("#mainDownload, #mainRemoveFavorite, #mainRemoveTemplate, #mainUnsubscribe, #mainRestore, #mainCopy").addClass("unlockAction");
            if (ASC.Files.Folders.folderContainer == "privacy") {
                jq("#buttonCopyto").hide();
            }
            if (ASC.Files.Folders.folderContainer != "forme") {
                jq("#buttonUnsubscribe").hide();
            }
            if (ASC.Files.Folders.folderContainer == "favorites") {
                jq("#buttonMoveto").hide();
            } else {
                jq("#buttonRemoveFavorite").hide();
            }
            if (ASC.Files.Folders.folderContainer == "templates") {
                jq("#buttonMoveto").hide();
            } else {
                jq("#buttonRemoveTemplate").hide();
            }
        }

        if (filesCount > 0 && countCanShare > 0 || ASC.Files.Folders.folderContainer == "project" && filesCount > 0) {
            jq("#buttonSendInEmail").show().find("span").html(filesCount);
        }

        if (countNotFavorite > 0) {
            jq("#buttonAddFavorite").show().find("span").html(countNotFavorite);
        } else {
            jq("#buttonAddFavorite").hide();
        }

        if (canConvert) {
            jq("#buttonConvert").show().find("span").html(count);
        } else {
            jq("#buttonConvert").hide();
        }
        jq("#mainConvert").toggleClass("unlockAction", canConvert);

        if (countIsNew > 0) {
            jq("#buttonMarkRead").show().find("span").html(countIsNew);
            jq("#mainMarkRead").addClass("unlockAction");
        }

        if (countWithRights > 0) {
            jq("#buttonDelete, #buttonMoveto").show().find("span").html(countWithRights - countThirdParty);
            jq("#mainDelete, #mainMove").addClass("unlockAction");
        }

        if (ASC.Files.Folders.folderContainer == "trash") {
            jq("#buttonDelete, #buttonMoveto, #buttonCopyto, #buttonMarkRead").hide();
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
        } else if (ASC.Files.Folders.folderContainer != "project" && countCanShare > 0) {
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
            afterShowFunction(dropdownItem);
            ASC.Files.Mouse.disableHover = true;
        }
    };

    var showActionsPanel = function (event, entryData) {
        ASC.Files.Actions.clipGetLink = ASC.Clipboard.destroy(ASC.Files.Actions.clipGetLink);

        var e = jq.fixEvent(event);

        var target = jq(e.srcElement || e.target);
        if (target.hasClass("active")) {
            ASC.Files.Actions.hideAllActionPanels();
            return true;
        }

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
        var isVisitor = Teamlab.profile.isVisitor === true;

        jq("#actionPanelFolders, #actionPanelFiles").hide();
        if (entryData.entryType === "file") {
            jq("#filesOpen,\
                #filesGotoParent,\
                #filesEdit,\
                #filesByTemplate,\
                #filesConvert,\
                #filesDownload,\
                #filesGetLink,\
                #filesShareAccess,\
                #filesGetExternalLink,\
                #filesChangeOwner,\
                #filesSendInEmail,\
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
                #filesMarkRead,\
                #filesAddFavorite,\
                #filesRemoveFavorite,\
                #filesAddTemplate,\
                #filesRemoveTemplate,\
                #filesRename,\
                #filesRestore,\
                #filesRemove").show().removeClass("display-none");

            var entryTitle = entryData.title;

            if (!ASC.Files.Utility.GetConvertFormats(entryTitle).length || entryData.encrypted || entryData.content_length > ASC.Files.Constants.AvailableFileSize) {
                jq("#filesConvert").hide().addClass("display-none");
            }

            if (!ASC.Files.Utility.CanWebView(entryTitle)
                && (typeof ASC.Files.ImageViewer == "undefined" || !ASC.Files.Utility.CanImageView(entryTitle))
                && (typeof ASC.Files.MediaPlayer == "undefined" || !ASC.Files.MediaPlayer.canPlay(entryTitle, true))) {
                jq("#filesOpen").hide().addClass("display-none");
            }

            if (!ASC.Files.ThirdParty || !ASC.Files.ThirdParty.thirdpartyAvailable()
                || jq.inArray(ASC.Files.Utility.GetFileExtension(entryTitle), ASC.Files.Constants.DocuSignFormats) == -1
                || entryData.encrypted) {
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

            if (entryObj.hasClass("on-favorite")) {
                jq("#filesAddFavorite").hide().addClass("display-none");
            } else {
                jq("#filesRemoveFavorite").hide().addClass("display-none");

                if (!ASC.Files.Tree.displayFavorites()) {
                    jq("#filesAddFavorite").hide().addClass("display-none");
                }
            }

            if (ASC.Files.Folders.folderContainer == "favorites") {
                jq("#filesMoveto,\
                    #filesRemove").hide().addClass("display-none");
            }

            if (entryObj.hasClass("is-template")) {
                jq("#filesAddTemplate").hide().addClass("display-none");
            } else {
                jq("#filesByTemplate, #filesRemoveTemplate").hide().addClass("display-none");

                if (!ASC.Files.Tree.displayTemplates()
                    || !ASC.Files.Utility.CanBeTemplate(entryTitle)) {
                    jq("#filesAddTemplate").hide().addClass("display-none");
                }
            }

            if (ASC.Files.Folders.folderContainer == "templates") {
                jq("#filesMoveto,\
                    #filesRemove").hide().addClass("display-none");
            }

            var lockedForMe = ASC.Files.UI.lockedForMe(entryObj);
            if (!ASC.Files.UI.editableFile(entryData)
                || editingFile && (!ASC.Files.Utility.CanCoAuhtoring(entryTitle) || entryObj.hasClass("on-edit-alone"))
                || lockedForMe) {
                jq("#filesEdit").hide().addClass("display-none");
            }

            if (entryData.encrypted) {
                jq("#filesEdit,\
                    #filesOpen,\
                    #filesGetLink,\
                    #filesGetExternalLink,\
                    #filesSendInEmail,\
                    #filesCompleteVersion,\
                    #filesLock,\
                    #filesUnlock,\
                    #filesAddTemplate,\
                    #filesCopyto,\
                    #filesCopy,\
                    #filesByTemplate,\
                    #filesAddFavorite").hide().addClass("display-none");
            }

            if (entryObj.hasClass("file-locked")) {
                jq("#filesLock").hide().addClass("display-none");
            } else {
                jq("#filesUnlock").hide().addClass("display-none");
            }

            if (ASC.Files.Folders.folderContainer == "trash") {
                jq("#filesOpen,\
                    #filesGotoParent,\
                    #filesEdit,\
                    #filesByTemplate,\
                    #filesGetLink,\
                    #filesShareAccess,\
                    #filesGetExternalLink,\
                    #filesSendInEmail,\
                    #filesCompleteVersion,\
                    #filesLock,\
                    #filesUnlock,\
                    #filesDocuSign,\
                    #filesVersions,\
                    #filesMoveto,\
                    #filesCopyto,\
                    #filesMarkRead,\
                    #filesAddFavorite,\
                    #filesRemoveFavorite,\
                    #filesAddTemplate,\
                    #filesRemoveTemplate,\
                    #filesRename").hide().addClass("display-none");
            } else {
                jq("#filesRestore").hide().addClass("display-none");
            }

            if (!accessibleObj || lockedForMe) {
                jq("#filesCompleteVersion,\
                    #filesRename,\
                    #filesLock").hide().addClass("display-none");

                if (!ASC.Files.Constants.ADMIN && ASC.Files.Folders.folderContainer != "my") {
                    jq("#filesUnlock").hide().addClass("display-none");
                }
            }

            if (isVisitor) {
                jq("#filesByTemplate,\
                    #filesLock,\
                    #filesDocuSign,\
                    #filesCompleteVersion,\
                    #filesAddFavorite,\
                    #filesRemoveFavorite,\
                    #filesAddTemplate,\
                    #filesRemoveTemplate").hide().addClass("display-none");
                if (!accessAdminObj) {
                    jq("#filesRename").hide().addClass("display-none");
                }
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
                jq("#filesGetExternalLink").hide().addClass("display-none");
            } else if (jq("#filesGetExternalLink").data("trial")
                && !ASC.Files.Utility.CanWebView(entryTitle)) {
                jq("#filesGetExternalLink").hide().addClass("display-none");
            }

            if (entryObj.find(".is-new:visible").length == 0) {
                jq("#filesMarkRead").hide().addClass("display-none");
            }

            if (ASC.Files.Folders.folderContainer != "forme"
                && (ASC.Files.Folders.folderContainer != "privacy"
                    || accessAdminObj)) {
                jq("#filesUnsubscribe").hide().addClass("display-none");
            }

            if (!ASC.Files.UI.accessEdit()) {
                jq("#filesCopy").hide().addClass("display-none");
            }

            if (ASC.Files.Folders.folderContainer != "favorites"
                && ASC.Files.Folders.folderContainer != "templates"
                && ASC.Files.Folders.folderContainer != "recent"
                && (!ASC.Files.Filter || !ASC.Files.Filter.getFilterSettings().isSet
                    || ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty())) {
                jq("#filesGotoParent").hide().addClass("display-none");
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
                #foldersGotoParent,\
                #foldersDownload,\
                #foldersGetLink,\
                #foldersShareAccess,\
                #foldersChangeOwner,\
                #foldersUnsubscribe,\
                #foldersMove,\
                #foldersMoveto,\
                #foldersCopyto,\
                #foldersMarkRead,\
                #foldersRename,\
                #foldersRestore,\
                #foldersRemove,\
                #foldersRemoveThirdparty,\
                #foldersChangeThirdparty").show().removeClass("display-none");

            if (ASC.Files.Folders.folderContainer == "privacy") {
                jq("#foldersCopyto").hide().addClass("display-none");
            }

            if (ASC.Files.Folders.folderContainer == "trash") {
                jq("#foldersOpen,\
                    #foldersGotoParent,\
                    #foldersGetLink,\
                    #foldersShareAccess,\
                    #foldersMove,\
                    #foldersMoveto,\
                    #foldersCopyto,\
                    #foldersMarkRead,\
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
                if (isVisitor) {
                    jq("#foldersRename").hide().addClass("display-none");
                }
            }

            if (entryObj.is(".without-share *, .without-share")) {
                jq("#foldersShareAccess").hide().addClass("display-none");
            }

            if (entryObj.find(".is-new:visible").length == 0) {
                jq("#foldersMarkRead").hide().addClass("display-none");
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

            if (!ASC.Files.Filter || !ASC.Files.Filter.getFilterSettings().isSet
                || ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty()) {
                jq("#foldersGotoParent").hide().addClass("display-none");
            }

            if (!jq("#foldersMovePanel li:not(.display-none)").length) {
                jq("#foldersMove").hide().addClass("display-none");
            }

            jq("#actionPanelFolders").show();
        }

        if (!ASC.Clipboard.enable) {
            jq("#filesGetLink, #foldersGetLink").remove();
        }

        var dropdownItem = jq("#filesActionPanel");

        jq.showDropDownByContext(e, target, dropdownItem, function () {
            ASC.Files.Mouse.disableHover = true;
        });

        afterShowFunction(dropdownItem);

        if (target.is(".menu-small")) {
            entryObj.addClass("row-lonely-select");
        }

        return true;
    };

    var onContextMenu = function (event) {
        var e = jq.fixEvent(event);

        if (typeof e == "undefined" || !e) {
            return true;
        }

        var target = jq(e.srcElement || e.target);
        if (target.is("input[type=text]")) {
            return true;
        }

        if (target.parents(".studio-action-panel").length) {
            return !ASC.Desktop;
        }

        ASC.Files.Actions.hideAllActionPanels();

        if (!target.parents("#filesMainContent").length) {
            return !ASC.Desktop;
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

        ASC.Files.Tree.showSelect(ASC.Files.Folders.currentFolder.id);
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
        clipGetExternalLink: clipGetExternalLink,

        currentEntryData: currentEntryData,

        showMoveToSelector: showMoveToSelector,
        registerHideTree: registerHideTree,

        setAceFileLink: setAceFileLink
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
            #filesSelectPresentation, #filesSelectSpreadsheet, #filesSelectImage, #filesSelectMedia, #filesSelectArchive").click(function () {
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

        jq("#filesAddFavorite, #filesRemoveFavorite").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.toggleFavorite(ASC.Files.Actions.currentEntryData.entryObject);
        });

        jq("#filesAddTemplate, #filesRemoveTemplate").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.toggleTemplate(ASC.Files.Actions.currentEntryData.entryObject);
        });

        jq("#filesRemove, #foldersRemove").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.deleteItem(ASC.Files.Actions.currentEntryData.entryType, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesShareAccess, #foldersShareAccess").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Share.getSharedInfo(ASC.Files.Actions.currentEntryData.entryType + "_" + ASC.Files.Actions.currentEntryData.id, ASC.Files.Actions.currentEntryData.title);
        });

        jq("#filesGetExternalLink .toggle").click(function(e) {
            if (jq(this).hasClass("off")) {
                ASC.Files.Actions.clipGetExternalLink.fromToggleBtn = true;
            } else {
                e.stopPropagation();
                ASC.Files.Actions.setAceFileLink();
            }
        });

        jq("#filesMarkRead, #foldersMarkRead").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Marker.markAsRead(ASC.Files.Actions.currentEntryData);
        });

        jq("#filesChangeOwner, #foldersChangeOwner").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.changeOwnerDialog(ASC.Files.Actions.currentEntryData);
        });

        jq("#filesSendInEmail").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            window.location.href = ASC.Mail.Utility.GetDraftUrl(ASC.Files.Actions.currentEntryData.entryId);
        });

        jq("#studioPageContent").on("click", "#buttonSendInEmail", function () {
            ASC.Files.Actions.hideAllActionPanels();

            var fileIds = jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):not(.folder-row):has(.checkbox input:checked)").map(function () {
                var entryRowData = ASC.Files.UI.getObjectData(this);
                var entryRowId = entryRowData.entryId;

                return entryRowId;
            });

            window.location.href = ASC.Mail.Utility.GetDraftUrl(fileIds.get());
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
            return false;
        });

        jq("#filesLock, #filesUnlock").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.lockFile(ASC.Files.Actions.currentEntryData.entryObject, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesLock .toggle, #filesUnlock .toggle").click(function (e) {
            e.stopPropagation();
            jq("#filesLock, #filesUnlock").toggle();
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

        jq("#filesByTemplate").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.createNewDoc(ASC.Files.Actions.currentEntryData);
        });

        jq("#filesCopy").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.createDuplicate(ASC.Files.Actions.currentEntryData);
        });

        jq("#foldersOpen").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.clickOnFolder(ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesGotoParent, #foldersGotoParent").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.clickOnFolder(ASC.Files.Actions.currentEntryData.folder_id);
        });

        jq("#foldersRemoveThirdparty").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.ThirdParty.showDeleteDialog(null, null, null, ASC.Files.Actions.currentEntryData.title, ASC.Files.Actions.currentEntryData);
        });

        jq("#foldersChangeThirdparty").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.ThirdParty.showChangeDialog(ASC.Files.Actions.currentEntryData);
        });

        jq("body").bind("contextmenu", function (event) {
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