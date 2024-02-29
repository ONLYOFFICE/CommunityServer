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


window.ASC.Files.Actions = (function () {
    var isInit = false;
    var clipGetLink = null;
    var clipGetExternalLink = null;

    var currentEntryData = null;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            jq(document).on("click", function (event) {
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
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetExternalLink, onGetExternalLink);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SetAceFileLink, onSetAceFileLink);
        }
    };

    /* Parent Folder DropDown*/

    var initParentFolderDropDown = function () {
        jq.dropdownToggle({
            dropdownID: "folderParentActionPanel",
            switcherSelector: "#filesBreadCrumbs .to-parent-folder-link",
            addTop: 2,
            addLeft: -2,
            fixWinSize: true,
            beforeShowFunction: function (switcherObj, dropdownItem) {
                showParentFolderActionMenu(switcherObj);
            },
            showFunction: function (entryData, dropdownItem) {
            },
            afterShowFunction: function (switcherObj, dropdownItem) {
                showSeporators(dropdownItem);
                createClipboardLinks();
            }
        });
    }

    function showParentFolderActionMenu(event) {
        entryData = ASC.Files.UI.getObjectData(event);
        ASC.Files.Actions.currentEntryData = entryData;

        var entryObj = entryData.entryObject;
        var accessibleObj = ASC.Files.UI.accessEdit(entryData, entryObj);
        var accessAdminObj = ASC.Files.UI.accessDelete(entryObj);
        var isVisitor = Teamlab.profile.isVisitor === true;
        var isAnonymous = !ASC.Resources.Master.IsAuthenticated;
        jq("#folderParentComeBack,\
                #folderParentShareAccess,\
                #folderParentGetLink,\
                #folderParentDownload,\
                #folderParentRename,\
                #folderParentRemoveThirdparty,\
                #folderParentRemove,\
                #folderParentUnsubscribe").show().removeClass("display-none");

        if (!accessAdminObj) {
            jq("#folderParentRemoveThirdparty, #folderParentRemove").hide().addClass("display-none");
            if (isVisitor) {
                jq("#folderParentRename").hide().addClass("display-none");
            }
        }

        if (ASC.Files.UI.denyDownload(entryData)) {
            jq("#folderParentDownload").hide().addClass("display-none");
        }

        if (ASC.Files.Folders.folderContainer == "privacy") {
            jq("#folderParentGetLink").hide().addClass("display-none");
        }
        if (!accessibleObj || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_PROJECT) {
            jq("#folderParentRename").hide().addClass("display-none");
        }

        if (ASC.Files.Folders.folderContainer != "forme") {
            jq("#folderParentUnsubscribe").hide().addClass("display-none");
        }

        if (isAnonymous) {
            jq("#folderParentGetLink").hide().addClass("display-none");
        }
        if (!ASC.Resources.Master.IsAuthenticated) {
            jq("#folderParentRemoveThirdparty, #folderParentRemove").hide().addClass("display-none");
        }
        if (jq("#filesMainContent").is(".without-share *:not(.can-share), .without-share") || ASC.Files.UI.denySharing(entryData)) {
            jq("#folderParentShareAccess").hide().addClass("display-none");
        }

        if (ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty(entryData)) {
            if (entryData.parent_folder_id != ASC.Files.Constants.FOLDER_ID_COMMON_FILES &&
                entryData.parent_folder_id != ASC.Files.Constants.FOLDER_ID_MY_FILES) {
                jq("#folderParentRemoveThirdparty").hide().addClass("display-none");
            } else {
                if (ASC.Desktop) {
                    jq("#folderParentRemoveThirdparty").hide().addClass("display-none");
                }

                jq("#folderParentRemove").hide().addClass("display-none");
            }
        } else {
            jq("#folderParentRemoveThirdparty").hide().addClass("display-none");
        }
    }

    /* Methods*/

    var afterShowFunction = function (dropdownPanel) {
        showSeporators(dropdownPanel);
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
        var contents = dropdownPanel.find(".dropdown-content:not(.display-none)").filter(function(index, item) {
            return item.style.display != "none";
        });

        var content = contents.length > 0 ? contents[0] : null;

        if (!content) return;
        if (content.style.minWidth) return;

        var tmpDropdownPanel = dropdownPanel.clone().appendTo('body');
        tmpDropdownPanel.show().removeClass("display-none");
        var tmpContent = tmpDropdownPanel.find(".dropdown-content:visible");
        tmpContent.css({overflow: "auto", maxHeight: "none"});
        tmpContent.find("li").show().removeClass("display-none");
        jq(content).css({ minWidth: tmpContent.width() });
        tmpDropdownPanel.remove();
    };

    var createClipboardLinks = function () {
        var link = jq("#filesGetLink:visible, #foldersGetLink:visible, #folderParentGetLink:visible").first();

        if (link.length) {
            createClipboardLink(link.attr("id"));
        }

        if (jq("#filesGetExternalLink").is(":visible")) {
            getFileSharedInfo();
        }

        if (jq("#filesGetExternalInheritedLink").length && jq("#filesGetExternalInheritedLink").is(":visible")) {
            getExternalLink();
        }
    };

    var createClipboardLink = function (id) {
        var url = ASC.Files.Actions.currentEntryData.entryObject.find(".entry-title .name a").prop("href");

        if (id == "folderParentGetLink") {
            var url = jq("#linkCurrentFolder").prop("href");
        }

        ASC.Files.Actions.clipGetLink = ASC.Clipboard.destroy(ASC.Files.Actions.clipGetLink);

        ASC.Files.Actions.clipGetLink = ASC.Clipboard.create(url, id, {
            onComplete: function () {
                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.LinkCopySuccess);
                ASC.Files.Actions.hideAllActionPanels();
            }
        });
    };

    var getExternalLink = function () {
        ASC.Files.ServiceManager.getExternalLink(ASC.Files.ServiceManager.events.GetExternalLink,
            {
                showLoading: true,
                entryId: ASC.Files.Actions.currentEntryData.entryType + "_" + ASC.Files.Actions.currentEntryData.id
            });
    }

    var onGetExternalLink = function (jsonData, params, errorMessage) {
        if (errorMessage) {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        ASC.Files.Actions.clipGetExternalLink = ASC.Clipboard.destroy(ASC.Files.Actions.clipGetExternalLink);
        
        ASC.Files.Actions.clipGetExternalLink = ASC.Clipboard.create(jsonData, "filesGetExternalInheritedLink", {
            onComplete: function () {
                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.LinkCopySuccess);
                ASC.Files.Actions.hideAllActionPanels();
            }
        });
    }

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

        var showFormFillingSettings = jsonData.some(function (item) {
            return item.ace_status == ASC.Files.Constants.AceStatusEnum.FillForms;
        })

        if (false && showFormFillingSettings) {
            jq("#filesFormFillingSettings").show().removeClass("display-none");
        }

        var data = jsonData[0];

        var toggleSwitcher = jq("#filesGetExternalLink .toggle");

        toggleSwitcher.toggleClass("off", data.ace_status == ASC.Files.Constants.AceStatusEnum.Restrict);

        ASC.Files.Actions.clipGetExternalLink = ASC.Clipboard.destroy(ASC.Files.Actions.clipGetExternalLink);

        ASC.Files.Actions.clipGetExternalLink = ASC.Clipboard.create(data.link, "filesGetExternalLink", {
            onComplete: function () {
                if (toggleSwitcher.hasClass("off")) {
                    ASC.Files.Actions.setAceFileLink();
                }

                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.LinkCopySuccess);

                if (!ASC.Files.Actions.clipGetExternalLink.fromToggleBtn) {
                    ASC.Files.Actions.hideAllActionPanels();
                }

                ASC.Files.Actions.clipGetExternalLink.fromToggleBtn = false;
            }
        });
    };

    var getLinkDefaultAccesssRigth = function (entryTitle) {

        var getLinkAccessRights = function (title) {
            var accessRights = [ASC.Files.Constants.AceStatusEnum.Read];

            if (ASC.Files.Utility.CanWebEdit(title) && !ASC.Files.Utility.MustConvert(title)) {
                accessRights.push(ASC.Files.Constants.AceStatusEnum.ReadWrite);
            }

            if (ASC.Files.Utility.CanWebCustomFilterEditing(title)) {
                accessRights.push(ASC.Files.Constants.AceStatusEnum.CustomFilter);
            }

            if (ASC.Files.Utility.CanWebReview(title)) {
                accessRights.push(ASC.Files.Constants.AceStatusEnum.Review);
            }

            //if (ASC.Files.Utility.CanWebRestrictedEditing(title)) {
            //    accessRights.push(ASC.Files.Constants.AceStatusEnum.FillForms);
            //}

            if (ASC.Files.Utility.CanWebComment(title)) {
                accessRights.push(ASC.Files.Constants.AceStatusEnum.Comment);
            }

            return accessRights;
        }

        var $settingsObj = jq("#defaultAccessRightsSetting");

        if ($settingsObj.length) {
            var common = +$settingsObj.find("input[type=radio]:checked").val();

            var specific = [];
            $settingsObj.find("input[type=checkbox]:checked").each(function () {
                specific.push(+this.value);
            });

            var accessRights = getLinkAccessRights(entryTitle);
            for (var access of accessRights) {
                if (specific.indexOf(access) != -1) {
                    return access;
                }
            }

            if (accessRights.indexOf(common) != -1) {
                return common;
            }
        }

        return ASC.Files.Constants.AceStatusEnum.Read;
    }

    var setAceFileLink = function () {
        ASC.Files.ServiceManager.setAceLink(ASC.Files.ServiceManager.events.SetAceFileLink, {
            showLoading: true,
            fileId: ASC.Files.Actions.currentEntryData.entryId,
            share: jq("#filesGetExternalLink .toggle").hasClass("off") ? getLinkDefaultAccesssRigth(ASC.Files.Actions.currentEntryData.title) : ASC.Files.Constants.AceStatusEnum.Restrict
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
        jq("#buttonUnsubscribe, #buttonDelete, #buttonMoveto, #buttonCopyto, #buttonShare, #buttonMarkRead, #buttonSendInEmail, #buttonAddFaforite, #buttonDownload, #buttonConvert").hide();
        jq("#mainContentHeader .unlockAction").removeClass("unlockAction");

        var selectedItems = jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)");
        var count = selectedItems.length;
        var filesCount = selectedItems.filter(":not(.folder-row)").length;

        var countNotFavorite = 0;
        if (ASC.Files.Tree.displayFavorites()
            && ASC.Files.Folders.folderContainer != "trash"
            && ASC.Files.Folders.folderContainer != "privacy") {
            countNotFavorite = selectedItems.filter(":not(.on-favorite)").length;
        }

        var countFavorite = 0;
        if (ASC.Files.Tree.displayFavorites()
            && ASC.Files.Folders.folderContainer != "trash"
            && ASC.Files.Folders.folderContainer != "privacy") {
            countFavorite = selectedItems.filter(".on-favorite").length;
        }

        var countWithRights = count;
        var countWithAccessible = count;
        var countIsNew = 0;
        var onlyThirdParty = (ASC.Files.ThirdParty && !ASC.Files.ThirdParty.isThirdParty());
        var countThirdParty = 0;
        var canConvert = false;
        var countCanShare = 0;
        var countCannotDownload = 0;

        selectedItems.each(function () {
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

            if (!ASC.Files.UI.accessEdit(entryData, entryObj)) {
                countWithAccessible--;
            }

            var denyDownload = ASC.Files.UI.denyDownload(entryData);
            if (denyDownload) {
                countCannotDownload++;
            }

            if (!canConvert && !entryObj.hasClass("folder-row")) {
                var entryTitle = entryData.title;
                var formats = ASC.Files.Utility.GetConvertFormats(entryTitle);
                canConvert = formats.length > 0 && !entryData.encrypted && entryData.content_length <= ASC.Files.Constants.AvailableFileSize && !denyDownload;
            }

            if (!ASC.Resources.Master.Personal && entryObj.is(":not(.without-share)")
                && ASC.Files.Share && ASC.Files.Folders.currentFolder.shareable
                && !ASC.Files.UI.denySharing(entryData)
                && (!entryData.encrypted
                    || ASC.Files.Folders.folderContainer == "privacy"
                        && ASC.Files.Utility.CanWebEncrypt(entryData.title)
                        && ASC.Desktop && ASC.Desktop.setAccess)) {
                countCanShare++;
            }
        });

        if (count > 0) {
            if (count - countCannotDownload > 0) {
                jq("#buttonDownload, #buttonCopyto").show().find("span").html(count - countCannotDownload);
                jq("#mainDownload, #mainCopy").addClass("unlockAction");
            }
            jq("#buttonRemoveTemplate, #buttonUnsubscribe, #buttonRestore").show().find("span").html(count);
            jq("#mainRemoveFavorite, #mainRemoveTemplate, #mainUnsubscribe, #mainRestore").addClass("unlockAction");
            if (ASC.Files.Folders.folderContainer == "privacy") {
                jq("#buttonCopyto").hide();
            }
            if (ASC.Files.Folders.folderContainer != "forme") {
                jq("#buttonUnsubscribe").hide();
            }
            if (ASC.Files.Folders.folderContainer == "favorites") {
                jq("#buttonMoveto").hide();
            } 
            if (ASC.Files.Folders.folderContainer == "templates") {
                jq("#buttonMoveto").hide();
            } else {
                jq("#buttonRemoveTemplate").hide();
            }
        }

        var countCanSendInEmail = filesCount - countCannotDownload;
        if (countCanSendInEmail > 0 && (countCanShare > 0 || ASC.Files.Folders.folderContainer == "project")) {
            jq("#buttonSendInEmail").show().find("span").html(countCanSendInEmail);
        }

        if (countNotFavorite > 0) {
            jq("#buttonAddFavorite").show().find("span").html(countNotFavorite);
        } else {
            jq("#buttonAddFavorite").hide();
        }

        if (countFavorite > 0) {
            jq("#buttonRemoveFavorite").show().find("span").html(countFavorite);
        } else {
            jq("#buttonRemoveFavorite").hide();
        }

        if (canConvert) {
            jq("#buttonConvert").show().find("span").html(count - countCannotDownload);
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
        } else if (ASC.Files.Folders.folderContainer != "project"
            && ASC.Files.Folders.folderContainer != "privacy"
            && countCanShare > 0) {
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

            resizeDropdownPanel(dropdownItem);
            dropdownItem.toggle();
            afterShowFunction(dropdownItem);
            ASC.Files.Mouse.disableHover = true;
        }

        if (!ASC.Resources.Master.IsAuthenticated) {
            jq("#buttonDelete").hide();
            
            if (countWithAccessible === 0) {
                jq("#buttonCopyto").hide();
            }
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
        var isAnonymous = !ASC.Resources.Master.IsAuthenticated;

        jq("#actionPanelFolders, #actionPanelFiles").hide();
        if (entryData.entryType === "file") {
            jq("#filesOpen,\
                #filesGotoParent,\
                #filesEdit,\
                #filesFillForm,\
                #filesCreateForm,\
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

            if (ASC.Files.UI.denyDownload(entryData)) {
                jq("#filesDownload, #filesConvert, #filesSendInEmail, #filesMoveto, #filesCopyto, #filesCopy").hide().addClass("display-none");
            }

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
                || entryData.encrypted
                || (jq("#thirdpartyToDocuSign").length + jq("#thirdpartyToDocuSignHelper").length == 0)) {
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
                jq("#filesEdit,\
                    #filesFillForm,\
                    #filesCreateForm").hide().addClass("display-none");
            } else if (ASC.Files.Utility.CanWebRestrictedEditing(entryTitle) && !entryData.encrypted) {
                jq("#filesEdit, #filesCreateForm").hide().addClass("display-none");
            } else {
                jq("#filesFillForm").hide().addClass("display-none");
                if (!ASC.Files.Utility.FileIsMasterForm(entryTitle)) {
                    jq("#filesCreateForm").hide().addClass("display-none");
                }
            }

            if (entryData.encrypted) {
                jq("#filesEdit,\
                    #filesFillForm,\
                    #filesCreateForm,\
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
                    #filesFillForm,\
                    #filesCreateForm,\
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

            if (isAnonymous) {
                jq("#filesGetLink,\
                    #filesLock,\
                    #filesUnlock,\
                    #filesRemove").hide().addClass("display-none");
                if (!accessibleObj){
                    jq("#filesEdit,\
                        #filesMove").hide().addClass("display-none");
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

            if (entryObj.is(".without-share *:not(.can-share), .without-share") || ASC.Files.UI.denySharing(entryData)) {
                jq("#filesShareAccess").hide().addClass("display-none");
                jq("#filesGetExternalLink").hide().addClass("display-none");
            } else if (jq("#filesGetExternalLink").data("trial")
                && !ASC.Files.Utility.CanWebView(entryTitle)) {
                jq("#filesGetExternalLink").hide().addClass("display-none");
            }

            if (!(jq("#cbxExternalShare").prop("checked") || jq("#sharingDialog input.link-settings").data("available"))) {
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
                jq("#filesCreateForm").hide().addClass("display-none");
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

            jq("#filesFormFillingSettings").hide().addClass("display-none");

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
                #foldersAddFavorite,\
                #foldersRemoveFavorite,\
                #foldersRestore,\
                #foldersRemove,\
                #foldersRemoveThirdparty,\
                #foldersChangeThirdparty").show().removeClass("display-none");

            if (ASC.Files.UI.denyDownload(entryData)) {
                jq("#foldersDownload, #foldersMove, #foldersMoveto, #foldersCopyto").hide().addClass("display-none");
            }

            if (ASC.Files.Folders.folderContainer == "privacy") {
                jq("#foldersCopyto, #foldersAddFavorite, #foldersRemoveFavorite, #foldersGetLink").hide().addClass("display-none");
            }
            if (entryObj.hasClass("on-favorite")) {
                jq("#foldersAddFavorite").hide().addClass("display-none");
            } else {
                jq("#foldersRemoveFavorite").hide().addClass("display-none");

                if (!ASC.Files.Tree.displayFavorites()) {
                    jq("#foldersAddFavorite").hide().addClass("display-none");
                }
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
                    #foldersAddFavorite,\
                    #foldersRemoveFavorite,\
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

            if (isAnonymous) {
                jq("#foldersGetLink").hide().addClass("display-none");
                if (!accessibleObj) {
                    jq("#foldersMove").hide().addClass("display-none");
                }
            }

            if (entryObj.is(".without-share *:not(.can-share), .without-share") || ASC.Files.UI.denySharing(entryData)) {
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

            if (ASC.Files.Folders.folderContainer == "favorites") {
                jq("#foldersGotoParent").show().removeClass("display-none");
            }

            if (!jq("#foldersMovePanel li:not(.display-none)").length) {
                jq("#foldersMove").hide().addClass("display-none");
            }

            jq("#actionPanelFolders").show();
        }

        if (!ASC.Clipboard.enable) {
            jq("#filesGetLink, #foldersGetLink").remove();
        }
        
        if (!ASC.Resources.Master.IsAuthenticated) {
            jq("#foldersRemove").hide();
            
            if (jq("#filesGetExternalInheritedLink").data("trial") && !ASC.Files.Utility.CanWebView(entryTitle)){
                jq("#filesGetExternalInheritedLink").hide().addClass("display-none")
            }
            else {
                jq("#filesGetExternalInheritedLink").show().removeClass("display-none")
            }
        }

        var dropdownItem = jq("#filesActionPanel");

        resizeDropdownPanel(dropdownItem);

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

        ASC.Files.Mouse.disableHover = false;
    };

    var createIframe = function (target, url) {
        var iframe = target.createElement("iframe");
        iframe.src = url;
        iframe.id = "hiddenIframe";
        iframe.style.display = "none";
        target.body.appendChild(iframe);
        return iframe;
    };

    var openCustomProtocolInIframe = function (uri) {
        var iframe = document.querySelector("#hiddenIframe");
        if (!iframe) {
            iframe = createIframe(document, "about:blank");
        }
        iframe.contentWindow.location.href = uri;
    };

    var openDocumentPrivacyCheck = function (fileId, winEditor, fileObj) {
        var urlForFileOpenWebEditor = ASC.Files.Utility.GetFileWebEditorUrl(fileId);
        var customUrlForFileOpenDesktopEditor = ASC.Files.Utility.GetFileCustomProtocolEditorUrl(fileId);
        var urlForOpenPrivate = ASC.Files.Utility.GetOpenPrivate(fileId);
        if (!fileObj.length) {
            fileObj = jq("#filesNewsList " + ASC.Files.UI.getSelectorId(fileId));
        }
        var entryData = ASC.Files.UI.getObjectData(fileObj);
        if (!ASC.Desktop && entryData && entryData.encrypted) {
            if (winEditor && winEditor.location) {
                winEditor.location.href = urlForFileOpenWebEditor;
            } else {
                if (sessionStorage.getItem("protocoldetector") == 1) {
                    openCustomProtocolInIframe(customUrlForFileOpenDesktopEditor);
                } else {
                    window.open(urlForOpenPrivate, "_blank");
                }
            }
        } else {
            if (winEditor && winEditor.location) {
                winEditor.location.href = urlForFileOpenWebEditor;
            } else {
                winEditor = window.open(urlForFileOpenWebEditor, "_blank");
            }
        }
    };

    var checkEditFile = function (fileId, winEditor) {
        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);
        ASC.Files.UI.lockEditFile(fileObj, true);
        openDocumentPrivacyCheck(fileId, winEditor, fileObj);
    
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

    var checkViewFile = function (fileId, version) {
        var viewerUrl = ASC.Files.Utility.GetFileWebViewerUrl(fileId, version);
        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);
        var entryData = ASC.Files.UI.getObjectData(fileObj);
        if (!ASC.Desktop && entryData && entryData.encrypted) {
            var viewerParameters = viewerUrl.slice(viewerUrl.indexOf("&"));
            if (sessionStorage.getItem("protocoldetector") == 1) {
                var customProtocolViewerUrl = ASC.Files.Utility.GetFileCustomProtocolEditorUrl(fileId) + viewerParameters;
                openCustomProtocolInIframe(customProtocolViewerUrl);
            } else {
                var openPrivateUrl = ASC.Files.Utility.GetOpenPrivate(fileId) + viewerParameters;
                window.open(openPrivateUrl, "_blank");
            }
        } else {
            window.open(viewerUrl, "_blank");
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
                    ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.InfoMoveGroup.format(0));
                    return;
                }
            }
        }

        ASC.Files.Tree.updateTreePath();

        if (!isCopy
            && jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):not(.on-edit):has(.checkbox input:checked)").length == 0) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.InfoMoveGroup.format(0));
            return;
        }

        if (ASC.Files.ThirdParty
            && !ASC.Files.Folders.isCopyTo
            && !ASC.Files.ThirdParty.isThirdParty()
            && jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):not(.third-party-entry):has(.checkbox input:checked)").length == 0) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.InfoMoveGroup.format(0));
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

        jq("body").on("click", ASC.Files.Actions.registerHideTree);
    };

    var registerHideTree = function (event) {
        if (!jq((event.target) ? event.target : event.srcElement).parents().addBack()
            .is("#treeViewPanelSelector, #foldersMoveto, #filesMoveto, #foldersCopyto, #filesCopyto,\
                 #foldersRestore, #filesRestore, #buttonMoveto, #buttonCopyto, #buttonRestore,\
                 #mainMove, #mainCopy, #mainRestore")) {
            ASC.Files.Actions.hideAllActionPanels();
            jq("body").off("click", ASC.Files.Actions.registerHideTree);
        }
    };

    return {
        init: init,
        showActionsViewPanel: showActionsViewPanel,
        showActionsPanel: showActionsPanel,
        onContextMenu: onContextMenu,
        initParentFolderDropDown: initParentFolderDropDown,
        checkEditFile: checkEditFile,
        checkViewFile: checkViewFile,

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

        jq("#filesSelectAll").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.UI.checkSelectAll(true);
        });

        jq("#filesSelectFile, #filesSelectFolder, #filesSelectDocument,\
            #filesSelectPresentation, #filesSelectSpreadsheet, #filesSelectImage, #filesSelectMedia, #filesSelectArchive").on("click", function () {
            var filter = this.id.replace("filesSelect", "").toLowerCase();
            ASC.Files.UI.checkSelect(filter);
        });

        jq("#filesDownload, #foldersDownload, #folderParentDownload").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.download(ASC.Files.Actions.currentEntryData.entryType, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#folderParentComeBack").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.clickOnFolder(ASC.Files.Actions.currentEntryData.parent_folder_id);
        });

        jq("#folderParentRename").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.renameParent(ASC.Files.Actions.currentEntryData);
        });

        jq("#filesRename, #foldersRename").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.rename(ASC.Files.Actions.currentEntryData.entryType, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesAddFavorite, #filesRemoveFavorite, #foldersAddFavorite, #foldersRemoveFavorite").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.toggleFavorite(ASC.Files.Actions.currentEntryData.entryObject);
        });

        jq("#filesAddTemplate, #filesRemoveTemplate").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.toggleTemplate(ASC.Files.Actions.currentEntryData.entryObject);
        });

        jq("#filesRemove, #foldersRemove, #folderParentRemove").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.deleteItem(ASC.Files.Actions.currentEntryData.entryType, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesShareAccess, #foldersShareAccess, #folderParentShareAccess").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            if (!ASC.Files.UI.denySharing(ASC.Files.Actions.currentEntryData)) {
                ASC.Files.Share.getSharedInfo(ASC.Files.Actions.currentEntryData.entryType + "_" + ASC.Files.Actions.currentEntryData.id, ASC.Files.Actions.currentEntryData.title);
            }
        });

        jq("#filesFormFillingSettings").on("click", function () {
            window.ASC.Files.FormFilling.setFileId(ASC.Files.Actions.currentEntryData.id);
            window.ASC.Files.FormFilling.getPropertiesAndShowDialog();
        });

        jq("#filesGetExternalLink .toggle").on("click", function(e) {
            if (!ASC.Files.UI.denySharing(ASC.Files.Actions.currentEntryData)) {
                if (jq(this).hasClass("off")) {
                    ASC.Files.Actions.clipGetExternalLink.fromToggleBtn = true;
                } else {
                    e.stopPropagation();
                    ASC.Files.Actions.setAceFileLink();
                }
            }
        });

        jq("#filesMarkRead, #foldersMarkRead").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Marker.markAsRead(ASC.Files.Actions.currentEntryData);
        });

        jq("#filesChangeOwner, #foldersChangeOwner").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.changeOwnerDialog(ASC.Files.Actions.currentEntryData);
        });

        jq("#filesSendInEmail").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            if (!ASC.Files.UI.denyDownload(ASC.Files.Actions.currentEntryData)) {
                window.location.href = ASC.Mail.Utility.GetDraftUrl(ASC.Files.Actions.currentEntryData.entryId);
            }
        });

        jq("#studioPageContent").on("click", "#buttonSendInEmail", function () {
            ASC.Files.Actions.hideAllActionPanels();

            var fileIds = jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):not(.folder-row):has(.checkbox input:checked)").map(function () {
                var entryRowData = ASC.Files.UI.getObjectData(this);
                return ASC.Files.UI.denyDownload(entryRowData) ? 0 : entryRowData.entryId;
            }).filter(function (index, item) {
                return item > 0
            });

            window.location.href = ASC.Mail.Utility.GetDraftUrl(fileIds.get());
        });

        jq("#filesUnsubscribe, #foldersUnsubscribe, #folderParentUnsubscribe").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Share.unSubscribeMe(ASC.Files.Actions.currentEntryData.entryType, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesConvert").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Converter.showToConvert(ASC.Files.Actions.currentEntryData.entryObject);
        });

        jq("#filesOpen").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.clickOnFile(ASC.Files.Actions.currentEntryData, false);
            return false;
        });

        jq("#filesLock, #filesUnlock").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.lockFile(ASC.Files.Actions.currentEntryData.entryObject, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesLock .toggle, #filesUnlock .toggle").on("click", function (e) {
            e.stopPropagation();
            jq("#filesLock, #filesUnlock").toggle();
            ASC.Files.Folders.lockFile(ASC.Files.Actions.currentEntryData.entryObject, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesCompleteVersion").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.versionComplete(ASC.Files.Actions.currentEntryData.id, 0, false);
        });

        jq("#filesVersions").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.showVersions(ASC.Files.Actions.currentEntryData.entryObject, ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesEdit, #filesFillForm").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.clickOnFile(ASC.Files.Actions.currentEntryData, true);
        });

        jq("#filesCreateForm").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.createNewForm(ASC.Files.Actions.currentEntryData);
        });

        jq("#filesByTemplate").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.createNewDoc(ASC.Files.Actions.currentEntryData);
        });

        jq("#filesCopy").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.createDuplicate(ASC.Files.Actions.currentEntryData);
        });

        jq("#foldersOpen").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.clickOnFolder(ASC.Files.Actions.currentEntryData.id);
        });

        jq("#filesGotoParent, #foldersGotoParent").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            var folderId = ASC.Files.Actions.currentEntryData.folder_id;
            if (ASC.Files.Actions.currentEntryData.entryType == "folder" && folderId == 0) {
                folderId = ASC.Files.Folders.folderContainer == "favorites"
                    ? ASC.Files.Constants.FOLDER_ID_PROJECT
                    : ASC.Files.Actions.currentEntryData.entryId;
            }
            ASC.Files.Folders.clickOnFolder(folderId);
        });

        jq("#foldersRemoveThirdparty, #folderParentRemoveThirdparty").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.ThirdParty.showDeleteDialog(null, null, null, ASC.Files.Actions.currentEntryData.title, ASC.Files.Actions.currentEntryData);
        });

        jq("#foldersChangeThirdparty").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.ThirdParty.showChangeDialog(ASC.Files.Actions.currentEntryData);
        });

        jq("body").on("contextmenu", function (event) {
            return ASC.Files.Actions.onContextMenu(event);
        });

        jq("#filesMainContent").on("click", ".menu-small", ASC.Files.Actions.showActionsPanel);

        ASC.Files.Actions.initParentFolderDropDown();

        jq("#studioPageContent").on("click",
            "#buttonMoveto, #buttonCopyto, #buttonRestore,\
            #mainMove.unlockAction, #mainCopy.unlockAction, #mainRestore.unlockAction,\
            #filesMoveto, #filesRestore, #filesCopyto,\
            #foldersMoveto, #foldersRestore, #foldersCopyto",
            function () {
                ASC.Files.Actions.showMoveToSelector(this.id == "buttonCopyto" || this.id == "mainCopy" || this.id == "filesCopyto" || this.id == "foldersCopyto");
            });

        jq("#filesDocuSign").on("click", function () {
            ASC.Files.ThirdParty.showDocuSignDialog(ASC.Files.Actions.currentEntryData);
            ASC.Files.Actions.hideAllActionPanels();
            return false;
        });
    });
})(jQuery);