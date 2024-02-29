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


window.ASC.Files.Share = (function () {
    var ResourceJS = ASC.Resources.Master.ResourceJS;
    var FilesJSResource = ASC.Files.FilesJSResource;
    var that = {};

    var selectionModeEnum = {
        none: 0,
        root: 1,
        group: 2
    };

    //todo: use private properties
    that.userCache = window.UserManager.getAllUsers(true);
    that.groupCache = window.GroupManager.getAllGroups(),

    that.accessRights = []
    that.defaultAccessRight = null;
    that.defaultAccessRightForVisitor = null;
    that.selected = {};

    that.tmpSelected = {};

    that.pagging = {
        renderBy: 100,
        searchItemsNumber: 0,
        contentItemsNumber: 0,
        usersItemsNumber: 0
    };

    that.searchResult = null;

    that.selectionMode = selectionModeEnum.none;
    that.currentElement = null;
    that.openedGroupElement = null;

    that.tooltipTimeout = null;

    that.clipboard = {
        internalLink: null,
        externalLinkAction: null,
        externalLinkShare: null,
        externalLinkEmbed: null,
        externalLinkPassword: null
    };

    that.targetData = {
        id: null,
        sharingInfo: null,
        needUpdate: false,
        ownerId: null,

        multiple: false,
        encrypted: false,
        entryData: null,
        entryLink: null,
        isThirdparty: false
    };

    that.viewSettings = {
        title: "",

        notifyAvailable: false,
        externalLinksAvailable: false,

        asFlat: false,
        showTooltop: false,
        rootFolderTypeCommon: false,
        originForPost: "*",

        displaySettings: false,
        denyDownload: false,
        denySharing: false,

        canWebCustomFilterEditing: false,
        canWebReview: false,
        canWebRestrictedEditing: false,
        canWebComment: false,

        canLinkReadWrite: false,

        hasMailAccounts: undefined
    };

    that.$dialog = jq("#sharingDialog");
    that.$dialogHeader = that.$dialog.find(".containerHeaderBlock td:first-of-type");

    that.$topContentContainer = that.$dialog.find(".dialog-top-content-container");

    that.$topButtonContainer = that.$dialog.find(".top-button-container");
    that.$addUserBtn = that.$topButtonContainer.find(".user-button");
    that.$addLinkBtn = that.$topButtonContainer.find(".link-button");
    that.$addLinkBtnArrow = that.$topButtonContainer.find(".arrow-part");
    that.$linkSettingsInput = that.$topButtonContainer.find(".link-settings");

    that.$bottomButtonContainer = that.$dialog.find(".bottom-button-container");
    that.$saveBtnMain = that.$bottomButtonContainer.find(".button.blue.main-part");
    that.$saveBtnArrow = that.$bottomButtonContainer.find(".button.blue.arrow-part");
    that.$cancelBtn = that.$bottomButtonContainer.find(".button.gray:not(.settings-link)");
    that.$settingsBtnContainer = that.$bottomButtonContainer.find(".settings-link-container");
    that.$settingsBtns = that.$settingsBtnContainer.find(".settings-link");

    that.$actionsContainer = that.$dialog.find(".group-actions");
    that.$menuActionSelectAll = that.$actionsContainer.find(".menuActionSelectAll");
    that.$menuActionSelectAllCbx = that.$menuActionSelectAll.find("input[type=checkbox]");
    that.$menuActionChangeBtn = that.$actionsContainer.find(".menuActionChange");
    that.$menuActionRemoveBtn = that.$actionsContainer.find(".menuActionRemove");
    that.$menuActionCloseBtn = that.$actionsContainer.find(".menuActionClose");
    that.$menuSelectDialog = that.$actionsContainer.find(".menu-select-dialog");
    that.$menuAccessRightsDialog = that.$actionsContainer.find(".menu-access-rights-dialog");

    that.$searchContainer = that.$dialog.find(".us-hot-search");
    that.$searchInput = that.$searchContainer.find(".textEdit");
    that.$searchDialog = that.$searchContainer.find(".us-popup-dialog");
    that.$searchDialogContentListParent = that.$searchDialog.find(".us-list-parent");

    that.$contentListParent = that.$dialog.find(".content-list");
    that.$itemAccessRightsDialog = that.$contentListParent.find(".item-access-rights-dialog");
    that.$linkActionDialog = that.$contentListParent.find(".link-action-dialog");
    that.$linkShareDialog = that.$contentListParent.find(".link-share-dialog");
    that.$linkEmbedDialog = that.$contentListParent.find(".link-embed-dialog");
    that.$linkPasswordDialog = that.$contentListParent.find(".link-password-dialog");
    that.$linkLifetimeDialog = that.$contentListParent.find(".link-lifetime-dialog");
    that.$linkDeleteDialog = that.$contentListParent.find(".link-delete-dialog");

    that.$itemTooltipDialog = that.$contentListParent.find(".item-tooltip-dialog");

    that.$addLinkActionDialog = that.$topContentContainer.find(".addlink-action-dialog");
    that.$addLinkActionDialogNew = that.$addLinkActionDialog.find(".addlink-new-action");
    that.$addLinkActionDialogCopy = that.$addLinkActionDialog.find(".addlink-copy-action");

    that.$linkShareDialogSocialContainer = that.$linkShareDialog.find(".link-share-social");
    that.$linkShareDialogInput = that.$linkShareDialog.find(".link-share-around input");

    that.$linkEmbedDialogSizeItem = that.$linkEmbedDialog.find(".embed-size-item");
    that.$linkEmbedDialogSizeCustom = that.$linkEmbedDialog.find(".embed-size-custom");
    that.$linkEmbedDialogSizeCustomWidth = that.$linkEmbedDialog.find(".embed-size-width");
    that.$linkEmbedDialogSizeCustomHeight = that.$linkEmbedDialog.find(".embed-size-height");
    that.$linkEmbedDialogInput = that.$linkEmbedDialog.find(".link-embed-around input.border-none");

    that.$linkPasswordDialogCbx = that.$linkPasswordDialog.find(".on-off-checkbox");
    that.$linkPasswordDialogInput = that.$linkPasswordDialog.find(".textEdit");
    that.$linkPasswordDialogShow = that.$linkPasswordDialog.find(".img-btn.show");
    that.$linkPasswordDialogRandom = that.$linkPasswordDialog.find(".img-btn.random");
    that.$linkPasswordDialogClean = that.$linkPasswordDialog.find(".link.dotline.clean");
    that.$linkPasswordDialogCopy = that.$linkPasswordDialog.find(".link.dotline.copy");
    that.$linkPasswordDialogSaveBtn = that.$linkPasswordDialog.find(".button.blue");
    that.$linkPasswordDialogCancelBtn = that.$linkPasswordDialog.find(".button.gray");

    that.$linkLifetimeDialogCbx = that.$linkLifetimeDialog.find(".on-off-checkbox");
    that.$linkLifetimeDialogDateInput = that.$linkLifetimeDialog.find(".textEditCalendar.date");
    that.$linkLifetimeDialogTimeInput = that.$linkLifetimeDialog.find(".textEdit.time");
    that.$linkLifetimeDialogClean = that.$linkLifetimeDialog.find(".link.dotline");
    that.$linkLifetimeDialogAutodeleteCbx = that.$linkLifetimeDialog.find(".autodelete");
    that.$linkLifetimeDialogSaveBtn = that.$linkLifetimeDialog.find(".button.blue");
    that.$linkLifetimeDialogCancelBtn = that.$linkLifetimeDialog.find(".button.gray");

    that.$linkDeleteDialogOkBtn = that.$linkDeleteDialog.find(".button.blue");
    that.$linkDeleteDialogCancelBtn = that.$linkDeleteDialog.find(".button.gray");

    that.$linkActionDialogCopy = that.$linkActionDialog.find(".link-action-copy");
    that.$linkShareDialogCopy = that.$linkShareDialog.find(".link-share-action-copy");
    that.$linkEmbedDialogCopy = that.$linkEmbedDialog.find(".link-embed-action-copy");

    that.$linkActionDialogEmbed = that.$linkActionDialog.find(".link-action-embed");
    that.$linkActionDialogPassword = that.$linkActionDialog.find(".link-action-password");
    that.$linkActionDialogLifetime = that.$linkActionDialog.find(".link-action-lifetime");
    that.$linkActionDialogRename = that.$linkActionDialog.find(".link-action-rename");
    that.$linkActionDialogDelete = that.$linkActionDialog.find(".link-action-delete");

    that.$linkActionDialogShort = that.$linkActionDialog.find(".link-action-short");
    that.$linkShareDialogShort = that.$linkShareDialog.find(".link-share-action-short");

    that.$emptyList = that.$dialog.find(".empty-list");

    that.$saveActionDialog = that.$dialog.find(".save-action-dialog");
    that.$saveActionDialogMessage = that.$saveActionDialog.find(".save-action-message");
    that.$saveActionDialogMessageParent = that.$saveActionDialogMessage.parent();
    that.$saveActionDialogSilent = that.$saveActionDialog.find(".save-action-silent");
    that.$saveActionDialogSilentParent = that.$saveActionDialogSilent.parent();
    that.$saveActionDialogCopy = that.$saveActionDialog.find(".save-action-copy");
    that.$saveActionDialogCopyParent = that.$saveActionDialogCopy.parent();
    that.$saveActionDialogSeporator = that.$saveActionDialogCopyParent.prev();

    that.$messageDialog = that.$dialog.find(".message-dialog");
    that.$messageDialogTextarea = that.$messageDialog.find("textarea");
    that.$messageDialogSaveBtn = that.$messageDialog.find(".button.blue");
    that.$messageDialogCancelBtn = that.$messageDialog.find(".button.gray");

    that.$advancedSettingsDialog = that.$dialog.find(".advanced-settings-dialog");
    that.$advancedSettingsDialogDenyDownloadCbx = that.$advancedSettingsDialog.find(".deny-download");
    that.$advancedSettingsDialogDenyDownloadLabel = that.$advancedSettingsDialogDenyDownloadCbx.parent();
    that.$advancedSettingsDialogDenySharingCbx = that.$advancedSettingsDialog.find(".deny-sharing");
    that.$advancedSettingsDialogDenySharingLabel = that.$advancedSettingsDialogDenySharingCbx.parent();
    that.$advancedSettingsDialogSaveBtn = that.$advancedSettingsDialog.find(".button.blue");
    that.$advancedSettingsDialogCancelBtn = that.$advancedSettingsDialog.find(".button.gray");

    that.searchDialogListParentElement = that.$searchDialogContentListParent.get(0);
    that.contentListParentElement = that.$contentListParent.get(0);
    that.contentListElement = that.contentListParentElement.querySelector(".us-list");

    that.userDomGenerator = new UserDomGenerator();

    that.ownerSelector = null;

    that.userSelector = new UserSelector({
        userCache: that.userCache,
        groupCache: that.groupCache,
        accessRights: that.accessRights,
        defaultAccessRight: that.defaultAccessRight,
        selected: that.selected,
        userDomGenerator: that.userDomGenerator,
        singleChoice: false,
        groupSelection: true,
        onSave: function (items, access) {
            var accessRightForVisitor = that.accessRights.find(function (x) { return x.id == access && !x.hideForVisitor });
            var accessForVisitor = accessRightForVisitor ? accessRightForVisitor.id : that.defaultAccessRightForVisitor;

            for (var itemId in that.selected) {
                if (items[itemId] === undefined) {
                    delete that.selected[itemId];
                }
            }

            for (var itemId in items) {
                if (that.selected[itemId] !== undefined) {
                    continue;
                }

                var item = items[itemId];
                var selectedAccess = access;
                if (!item.isGroup && that.userCache[itemId].isVisitor) {
                    selectedAccess = accessForVisitor;
                }
                item.access = selectedAccess;
                item.accessName = getAceString(selectedAccess);
                item.canEdit = true;
                item.canRemove = true;
                that.selected[itemId] = item;
            }

            renderContent();
            showSaveBtn();
        }
    });

    that.userSearcher = new UserSearcher(that.groupCache, that.userCache, that.$searchContainer.get(0), onChangeSearchText)

    that.show = function () {

        var displayLinkBtn = that.viewSettings.externalLinksAvailable && !that.targetData.multiple;

        if (displayLinkBtn && that.targetData.entryData && that.targetData.entryData.entryType === "file" && that.$addLinkBtn.data("trial")) {
            displayLinkBtn = ASC.Files.Utility.CanWebView(that.targetData.entryData.title);
        }

        if (displayLinkBtn) {
            that.$addLinkBtn.removeClass("display-none");
            if (that.targetData.entryLink) {
                that.$addLinkBtn.addClass("main-part");
                that.$addLinkBtnArrow.removeClass("display-none");
            } else {
                that.$addLinkBtn.removeClass("main-part");
                that.$addLinkBtnArrow.addClass("display-none");
            }
        } else {
            that.$addLinkBtn.addClass("display-none");
            that.$addLinkBtnArrow.addClass("display-none");
        }

        if (that.targetData.entryLink) {
            that.$saveActionDialogCopyParent.show();
        } else {
            that.$saveActionDialogCopyParent.hide();
        }

        that.viewSettings.displaySettings = that.viewSettings.displaySettings && that.targetData.ownerId == Teamlab.profile.id;
        if (that.viewSettings.displaySettings) {
            that.$settingsBtnContainer.show();
        } else {
            that.$settingsBtnContainer.hide();
        }

        if (that.viewSettings.notifyAvailable) {
            that.$saveActionDialogMessageParent.show();
            that.$saveActionDialogSilentParent.show();
        } else {
            that.$saveActionDialogMessageParent.hide();
            that.$saveActionDialogSilentParent.hide();
        }

        if (that.targetData.entryLink && that.viewSettings.notifyAvailable) {
            that.$saveActionDialogSeporator.show();
        } else {
            that.$saveActionDialogSeporator.hide();
        }

        hideSaveBtn();

        if (!that.targetData.entryLink && !that.viewSettings.notifyAvailable) {
            that.$saveBtnMain.removeClass("main-part").addClass("single-btn");
            that.$saveBtnArrow.hide();
        } else {
            that.$saveBtnMain.addClass("main-part").removeClass("single-btn");
            that.$saveBtnArrow.removeAttr("style");
        }

        var headerTitle = (that.viewSettings.rootFolderTypeCommon || ASC.Files.Folders && ASC.Files.Folders.folderContainer == "corporate")
            ? ASC.Files.FilesJSResource.AccessSettingsHeader.format(that.viewSettings.title)
            : ASC.Files.FilesJSResource.SharingSettingsHeader.format(that.viewSettings.title);

        if (!that.viewSettings.showTooltop) {
            that.$contentListParent.off("mouseover mouseout");
        }

        if (that.viewSettings.asFlat) {
            that.$dialog.addClass("flat").show();

            PopupKeyUpActionProvider.EnterAction = PopupKeyUpActionProvider.CtrlEnterAction = "jq('.bottom-button-container .button:visible:first').trigger('click');";
            PopupKeyUpActionProvider.CloseDialogAction = "ASC.Files.Share.updateForParent();";
            PopupKeyUpActionProvider.ForceBinding = true;
        } else {
            that.$dialogHeader.attr("title", headerTitle).text(headerTitle);
            StudioBlockUIManager.blockUI(that.$dialog);
        }

        if (ASC.Files.Actions) {
            ASC.Files.Actions.hideAllActionPanels();
        }

        that.$dialog.trigger("click"); // hide all inner dialogs;
    }

    that.hide = function () {
        PopupKeyUpActionProvider.CloseDialog();
    }

    that.save = function (notify, message) {
        notify = notify && that.viewSettings.notifyAvailable;
        message = notify ? message : "";

        setAccess(notify, message);

        if (!that.targetData.encrypted) {
            that.hide();
        }
    }

    that.changeData = function (selected, accessRights, defaultAccessRight) {
        that.selected = selected;
        that.accessRights = accessRights;
        that.defaultAccessRight = defaultAccessRight;

        var accessRightForVisitor = accessRights.find(function (x) { return x.id == defaultAccessRight && !x.hideForVisitor });
        that.defaultAccessRightForVisitor = accessRightForVisitor ? accessRightForVisitor.id : ASC.Files.Constants.AceStatusEnum.Read;

        that.userSelector.changeData(that.selected, that.accessRights, that.defaultAccessRight);

        that.pagging.contentItemsNumber = 0;

        that.currentElement = null;
        that.openedGroupElement = null;

        renderAccessRightsDialogs();

        clickOnMenuActionCloseElement();
    }

    var isInit = false;


    function init() {
        if (isInit) return;

        renderAccessRightsDialogs();
        renderContent();
        setBindings();

        isInit = true;
    };

    function onBeforeShowUserSelectorDialog() {
        var addUserBtnElement = that.$addUserBtn.get(0);
        if (addUserBtnElement.classList.contains("disable")) {
            return false;
        }
        that.userSelector.resetTempData();
        that.userSelector.render();
        if (!that.viewSettings.asFlat) {
            that.userSelector.dialogElement.style.top = (addUserBtnElement.offsetTop + addUserBtnElement.offsetHeight + 4) + "px";
        }
        return true;
    }

    function onBeforeShowOwnerSelectorDialog() {
        that.ownerSelector.resetTempData();
        that.ownerSelector.render();
        return true;
    }

    function showSaveBtn() {
        that.$saveBtnMain.removeClass("display-none");
        that.$saveBtnArrow.removeClass("display-none");
        checkSettingsBtnAvailability();
    }

    function hideSaveBtn() {
        that.$saveBtnMain.addClass("display-none");
        that.$saveBtnArrow.addClass("display-none");
        checkSettingsBtnAvailability();
    }

    function checkSettingsBtnAvailability() {
        if (!that.viewSettings.displaySettings) {
            return;
        }
        var available = false;
        for (var itemId in that.selected) {
            var item = that.selected[itemId];
            if (item.isOwner || (item.isLink && item.access == ASC.Files.Constants.AceStatusEnum.Restrict)) {
                continue;
            }
            available = true;
            break;
        }
        that.$settingsBtns.toggleClass("disable", !available);
    }

    function clickOnAddLinkBtn() {
        if (that.$addLinkBtn.hasClass("disable") || !that.viewSettings.externalLinksAvailable) {
            return;
        }

        var entryId = that.targetData.entryData ? that.targetData.entryData.id : ASC.Files.UI.parseItemId(that.targetData.id).entryId;
        var entryType = that.targetData.entryData ? that.targetData.entryData.entryType : ASC.Files.UI.parseItemId(that.targetData.id).entryType;

        var access = that.defaultAccessRight == ASC.Files.Constants.AceStatusEnum.FillForms
            ? ASC.Files.Constants.AceStatusEnum.Read
            : that.defaultAccessRight == ASC.Files.Constants.AceStatusEnum.ReadWrite
                ? that.viewSettings.canLinkReadWrite ? that.defaultAccessRight : ASC.Files.Constants.AceStatusEnum.Read
                : that.defaultAccessRight;

        var data = {
            isFolder: entryType === "folder"
        }

        Teamlab.getSharedLinkTemplate(entryId, data, {
            before: function () {
                LoadingBanner.displayLoading();
            },
            success: function (_, data) {
                var item = {
                    id: data.id,
                    link: data.link,
                    linkSettings: data.linkSettings,
                    name: data.title || FilesJSResource.ExternalLink,
                    isLink: true,
                    canEdit: true,
                    canRemove: true,
                    access: access,
                    accessName: getAceString(access),
                    infoText: that.viewSettings.externalLinksAvailable ? isShortenedLink(data.link) ? FilesJSResource.Shortened : "" : FilesJSResource.ExternalLinkDisabled,
                    entryType: data.entryType
                };

                var newSelected = {};
                newSelected[item.id] = item;
                that.selected = Object.assign(newSelected, that.selected);
                that.userSelector.changeData(that.selected, that.accessRights, that.defaultAccessRight);

                var linkElement = that.userDomGenerator.createLink(item);
                linkElement.classList.add("new");
                that.contentListElement.prepend(linkElement);

                that.currentElement = linkElement;

                displayLinkRenameBlock(linkElement, true);

                that.pagging.contentItemsNumber++;
                that.$emptyList.hide();

                showSaveBtn();
            },
            after: function () {
                LoadingBanner.hideLoading();
            },
            error: function (_, error) {
                console.log(error);
            },
            processUrl: function (url) {
                return ASC.Files.Utility.AddExternalShareKey(url);
            }
        });
    }

    function renderSearch() {
        var data = getSearchFragment(0, that.pagging.renderBy - 1);

        that.pagging.searchItemsNumber = data.count;

        var newContentListElement = that.userDomGenerator.createContentList();
        newContentListElement.append(data.fragment);

        that.searchDialogListParentElement.firstElementChild.replaceWith(newContentListElement);
    }

    function getSearchFragment(from, to) {
        var result = {
            fragment: document.createDocumentFragment(),
            count: -1
        }

        for (var itemId in that.searchResult) {

            result.count++;

            if (result.count < from) {
                continue;
            }

            if (result.count > to) {
                break;
            }

            var item = Object.assign({}, that.searchResult[itemId]);
            item.disabled = that.selected[itemId] !== undefined;
            item.selected = item.disabled || that.tmpSelected[itemId] !== undefined;

            var itemElement = that.userDomGenerator.createGroupItem(item);
            result.fragment.append(itemElement);
        }

        if (result.count > to) {
            result.fragment.append(that.userDomGenerator.createShowMoreRow({ name: ResourceJS.UserSelectorShowMore }));
            return result;
        }

        result.count++;

        if (result.count == 0) {
            result.fragment.append(that.userDomGenerator.createEmptySearchRow());
        }

        return result;
    }

    function onBeforeShowSearchDialog() {
        var inputPosition = that.$searchInput.position();
        var inputHeight = that.$searchInput.outerHeight();
        that.$searchDialog.css("top", (inputPosition.top + inputHeight + 4) + "px");
        return true;
    }

    function onBeforeHideSearchDialog() {
        that.userSearcher.clear();
        return true;
    }

    function onChangeSearchText(text, items) {
        if (text.length) {
            that.searchResult = Object.assign({}, items.groups, items.users);
            renderSearch();
            that.searchDialogHelper.show();
        } else {
            that.searchDialogHelper.hide();
        }
    }

    function clickOnSearchItem(element) {
        if (element.classList.contains("disabled")) {
            return;
        }

        var item = Object.assign({}, that.searchResult[element.id]);
        var selectedAccessRight = that.defaultAccessRight;

        if (!item.isGroup && that.userCache[element.id].isVisitor) {
            selectedAccessRight = that.defaultAccessRightForVisitor;
        }

        item.access = selectedAccessRight;
        item.accessName = getAceString(selectedAccessRight);
        item.canEdit = true;
        item.canRemove = true;

        that.selected[element.id] = item;

        var lastContenElement = that.contentListElement.lastElementChild;
        var isShowMoreVisible = lastContenElement && lastContenElement.classList.contains("us-show-more-row");

        if (!isShowMoreVisible) {
            if (item.isGroup) {
                var groupElement = that.userDomGenerator.createGroup(item);
                that.contentListElement.append(groupElement);
            } else {
                item.withoutTitle = that.viewSettings.showTooltop;
                var userElement = that.userDomGenerator.createGroupItem(item);
                that.contentListElement.append(userElement);
            }

            that.pagging.contentItemsNumber++;
            that.$emptyList.hide();
        }

        if (!item.isGroup && that.openedGroupElement) {
            var index = that.groupCache[that.openedGroupElement.id].users.indexOf(element.id);
            if (index != -1) {
                var itemElement = that.openedGroupElement.nextElementSibling.children[element.id];
                if (itemElement && itemElement.classList.contains("us-name")) {
                    itemElement.querySelector(".us-access").setAttribute("class", "us-access access-" + selectedAccessRight);
                    var infoText = that.userDomGenerator.createNameInfoText(FilesJSResource.IndividualRights);
                    itemElement.querySelector(".us-name-text").append(infoText);
                    itemElement.classList.add("info");
                }
            }
        }

        that.searchDialogHelper.hide();
        showSaveBtn();
    }

    function clickOnSearchShowMore(element) {
        var data = getSearchFragment(that.pagging.searchItemsNumber, that.pagging.searchItemsNumber + that.pagging.renderBy - 1);

        that.pagging.searchItemsNumber = data.count;

        that.searchDialogListParentElement.firstElementChild.append(data.fragment);

        element.parentElement.remove();
    }


    function markItemSelected(itemId, selected) {
        if (selected) {
            that.tmpSelected[itemId] = true;
        } else {
            delete that.tmpSelected[itemId];
        }
    }

    function setSelectionMode(mode) {
        if (mode == selectionModeEnum.none) {
            that.selectionMode = mode;
            that.contentListParentElement.classList.remove("rootonly");
            that.tmpSelected = {};
            if (that.openedGroupElement) {
                openGroup(renderContent(that.openedGroupElement.id));
            } else {
                renderContent();
            }
            return;
        }

        if (mode == selectionModeEnum.root) {
            if (that.selectionMode == mode) {
                return;
            }
            that.selectionMode = mode;
            that.contentListParentElement.classList.add("rootonly");
            that.openedGroupElement = null;
            renderContent();
            return;
        }

        if (mode == selectionModeEnum.group) {
            if (that.selectionMode == mode) {
                return;
            }
            that.selectionMode = mode;
            that.contentListParentElement.classList.remove("rootonly");
            openGroup(renderContent(that.openedGroupElement.id));
        }
    }

    function openActions(force) {
        var tmpSelectedLength = Object.keys(that.tmpSelected).length;

        var hasSelected = tmpSelectedLength > 0;

        if (hasSelected || force) {
            that.$actionsContainer.show();
            that.$topContentContainer.addClass("hidden");
            that.$addUserBtn.addClass("disable");
            that.$addLinkBtn.addClass("disable");
            that.$addLinkBtnArrow.addClass("disable");
            that.$menuActionRemoveBtn.toggle(that.selectionMode != selectionModeEnum.group);
        } else {
            that.$actionsContainer.hide();
            that.$topContentContainer.removeClass("hidden");
            that.$addUserBtn.removeClass("disable");
            that.$addLinkBtn.removeClass("disable");
            that.$addLinkBtnArrow.removeClass("disable");
            return false;
        }

        var allSelected = true;
        var canRemove = true;

        if (that.selectionMode == selectionModeEnum.root) {
            for (var itemId in that.selected) {
                var item = that.selected[itemId];
                if (!item.canEdit) {
                    continue;
                }

                if (that.tmpSelected[itemId] === undefined) {
                    allSelected = false;
                    break;
                }
            }

            for (var itemId in that.tmpSelected) {
                var item = that.selected[itemId];
                if (!item.canRemove) {
                    canRemove = false;
                    break;
                }
            }
        }

        if (that.selectionMode == selectionModeEnum.group) {
            var usersInGroup = that.groupCache[that.openedGroupElement.id].users;
            for (var userId of usersInGroup) {
                if (!that.userCache[userId]) {
                    continue; //terminated
                }

                var user = that.selected[userId];
                if (user !== undefined && !user.canEdit) {
                    continue;
                }

                if (that.tmpSelected[userId] === undefined) {
                    allSelected = false;
                    break;
                }
            }
        }

        that.$menuActionSelectAllCbx.prop("checked", hasSelected && allSelected);
        that.$menuActionSelectAllCbx.prop("indeterminate", hasSelected && !allSelected);

        that.$menuActionChangeBtn.toggleClass("unlockAction", hasSelected);
        that.$menuActionRemoveBtn.toggleClass("unlockAction", hasSelected && canRemove);

        return true;
    }


    function clickOnMenuActionSelectAllElement(element, event) {

        that.tmpSelected = {};

        if (event.target.checked) {
            var canRemove = true;
            if (that.selectionMode == selectionModeEnum.group) {
                for (var userId of that.groupCache[that.openedGroupElement.id].users) {
                    if (!that.userCache[userId]) {
                        continue; //terminated
                    }

                    var userItem = that.selected[userId];
                    if (userItem === undefined || userItem.canEdit) {
                        markItemSelected(userId, true);
                    }
                }
            } else {
                for (var itemId in that.selected) {
                    if (that.selected[itemId].canEdit) {
                        markItemSelected(itemId, true);
                    }
                }
                for (var itemId in that.tmpSelected) {
                    var item = that.selected[itemId];
                    if (!item.canRemove) {
                        canRemove = false;
                        break;
                    }
                }
            }
            that.$menuActionChangeBtn.addClass("unlockAction");
            that.$menuActionRemoveBtn.toggleClass("unlockAction", canRemove);
        } else {
            that.$menuActionChangeBtn.removeClass("unlockAction");
            that.$menuActionRemoveBtn.removeClass("unlockAction");
        }

        if (that.selectionMode == selectionModeEnum.group) {
            openGroup(renderContent(that.openedGroupElement.id));
        } else {
            renderContent();
        }
    }

    function clickOnSelectDialogItemElement(element, event) {

        var selectAll = event.target.classList.contains("select-all");
        var selectLinks = event.target.classList.contains("select-links");
        var selectGroups = event.target.classList.contains("select-groups");
        var selectUsers = event.target.classList.contains("select-users");
        var selectByType = event.target.getAttribute("data-access") === null;
        var selectedAccess = +event.target.getAttribute("data-access");

        if (that.selectionMode == selectionModeEnum.group) {
            for (var userId of that.groupCache[that.openedGroupElement.id].users) {
                if (!that.userCache[userId]) {
                    continue; //terminated
                }

                var user = that.selected[userId];
                if (user !== undefined && !user.canEdit) {
                    continue;
                }

                if (selectByType) {
                    markItemSelected(userId, selectAll || selectUsers);
                    continue;
                }

                var userAccess = user === undefined
                    ? that.selected[that.openedGroupElement.id].access
                    : user.access;

                markItemSelected(userId, userAccess == selectedAccess);
            }
        } else {
            for (var itemId in that.selected) {
                var item = that.selected[itemId];
                if (!item.canEdit) {
                    continue;
                }

                var selected = selectByType
                    ? (selectAll || (item.isLink && selectLinks) || (item.isGroup && selectGroups) || (!item.isLink && !item.isGroup && selectUsers))
                    : item.access == selectedAccess;

                markItemSelected(itemId, selected);
            }
        }

        openActions(true);

        if (that.selectionMode == selectionModeEnum.group) {
            openGroup(renderContent(that.openedGroupElement.id));
        } else {
            renderContent();
        }

        that.menuSelectDialogHelper.hide();
    }

    function onBeforeShowMenuAccessRightsDialog(element, event) {
        if (!that.$menuActionChangeBtn.hasClass("unlockAction")) {
            return false;
        }

        that.currentElement = null;

        var canReadWrite = true;
        var canFillForms = true;
        var canRemove = false;
        var hasVisitors = false;
        var onlyRestrict = false;

        for (var itemId in that.tmpSelected) {
            var item = that.selected[itemId];
            if (item && item.isLink) {
                canReadWrite = that.viewSettings.canLinkReadWrite;
                canFillForms = false;
                onlyRestrict = !that.viewSettings.externalLinksAvailable;
                continue;
            }
            var user = that.userCache[itemId];
            if (user && user.isVisitor) {
                hasVisitors = true;
            }
        }

        adjustAccessRightsDialogs(that.$menuAccessRightsDialog, canReadWrite, canFillForms, canRemove, hasVisitors, onlyRestrict);
        return true;
    }

    function clickOnMenuActionRemoveElement(element, event) {
        that.currentElement = null;

        for (var itemId in that.tmpSelected) {
            var item = that.selected[itemId];

            if (!item.canRemove) {
                continue;
            }

            if (item.isLink) {
                deleteLink(itemId);
            } else {
                delete that.selected[itemId];
            }
        }

        clickOnMenuActionCloseElement();
    }

    function clickOnMenuActionCloseElement(element, event) {
        that.$actionsContainer.hide();
        that.$topContentContainer.removeClass("hidden");
        that.$addUserBtn.removeClass("disable");
        that.$addLinkBtn.removeClass("disable");
        that.$addLinkBtnArrow.removeClass("disable");

        setSelectionMode(selectionModeEnum.none);
    }

    function clickOnLinkPasswordElement(element, event) {
        if (!that.viewSettings.externalLinksAvailable) {
            return;
        }
        displayContentListDialog(element, that.linkPasswordDialogHelper, that.$linkPasswordDialog);
    }

    function clickOnLinkLifetimeElement(element, event) {
        if (!that.viewSettings.externalLinksAvailable) {
            return;
        }
        displayContentListDialog(element, that.linkLifetimeDialogHelper, that.$linkLifetimeDialog);
    }

    function clickOnLinkNameElement(element, event) {
        that.$linkActionDialog.find("li:not(:last-of-type)").toggle(that.viewSettings.externalLinksAvailable);
        displayContentListDialog(element, that.linkActionDialogHelper, that.$linkActionDialog);
    }

    function displayContentListDialog(element, dialogHelper, $dialog, indent) {
        if (element && that.currentElement == element && $dialog.is(":visible")) {
            $dialog.hide();
            return;
        }

        dialogHelper.hide();

        that.currentElement = element;

        var padding = indent === undefined ? 4 : indent;

        var parentHeight = that.$contentListParent.outerHeight();
        var parentScrollTop = that.$contentListParent.scrollTop();

        var $element = jq(element);
        var elementPosition = $element.position();
        var elementHeight = $element.outerHeight();

        var dialogTop = parentScrollTop + elementPosition.top + elementHeight - padding;

        $dialog.css("top", dialogTop + "px");

        setTimeout(function () { dialogHelper.show(); }, 0);

        var dialogHeight = $dialog.outerHeight();

        if (parentHeight + parentScrollTop < dialogTop + dialogHeight) {
            var expectedTop = parentScrollTop + elementPosition.top + padding - dialogHeight;
            if (expectedTop < parentScrollTop) {
                $dialog.css("top", parentHeight + parentScrollTop - dialogHeight + "px");
            } else {
                $dialog.css("top", expectedTop + "px");
            }
        }
    }

    function clickOnLinkActionDialogItemElement(element, event) {

        var id = that.currentElement.id;
        var classList = event.target.classList;

        if (classList.contains("link-action-share")) {
            displayContentListDialog(that.currentElement, that.linkShareDialogHelper, that.$linkShareDialog);
            that.$linkShareDialogInput.trigger("focus").trigger("select");
        }
        else if (classList.contains("link-action-embed")) {
            displayContentListDialog(that.currentElement, that.linkEmbedDialogHelper, that.$linkEmbedDialog);
            that.$linkEmbedDialogInput.trigger("focus").trigger("select");
        }
        else if (classList.contains("link-action-short")) {
            getShortenLink();
        }
        else if (classList.contains("link-action-password")) {
            displayContentListDialog(that.currentElement, that.linkPasswordDialogHelper, that.$linkPasswordDialog);
        }
        else if (classList.contains("link-action-lifetime")) {
            displayContentListDialog(that.currentElement, that.linkLifetimeDialogHelper, that.$linkLifetimeDialog);
        }
        else if (classList.contains("link-action-rename")) {
            displayLinkRenameBlock(that.currentElement, true);
        }
        else if (classList.contains("link-action-delete")) {
            displayContentListDialog(that.currentElement, that.linkDeleteDialogHelper, that.$linkDeleteDialog);
        }

        that.linkActionDialogHelper.hide();
    }

    function displayLinkRenameBlock(linkElement, display) {
        var nameBlock = linkElement.querySelector(".us-name-text");
        var renameBlock = linkElement.querySelector(".us-rename");
        if (display) {
            linkElement.classList.add("rename");
            var newElement = false;
            if (renameBlock) {
                renameBlock.style.display = "flex";
            } else {
                renameBlock = that.userDomGenerator.createLinkRenameBlock();
                nameBlock.after(renameBlock);
                newElement = true;
            }
            var input = renameBlock.querySelector(".textEdit");
            if (newElement) {
                input.addEventListener("keyup", function (event) {
                    if (event.keyCode === 27) {
                        event.stopPropagation();
                        displayLinkRenameBlock(linkElement, false);
                    }
                    if (event.keyCode === 13) {
                        event.stopPropagation();
                        renameLink(linkElement);
                    }
                });
            }
            input.value = that.selected[linkElement.id].name;
            input.focus();
        } else {
            if (linkElement.classList.contains("new")) {
                that.currentElement = linkElement;
                deleteLink(linkElement.id);
            } else {
                linkElement.classList.remove("rename");
                renameBlock.remove();
            }
        }
    }

    function renameLink(linkElement) {
        linkElement.classList.remove("new");
        var linkName = linkElement.querySelector(".textEdit").value;
        if (linkName) {
            var textElement = linkElement.querySelector(".us-name-text span");
            textElement.setAttribute("title", linkName);
            textElement.textContent = linkName;
            if (that.selected[linkElement.id].name != linkName) {
                that.selected[linkElement.id].name = linkName;
                showSaveBtn();
            }
        }
        displayLinkRenameBlock(linkElement, false);
    }

    function deleteLink(linkId) {
        if (linkId === ASC.Files.Constants.ShareLinkId) {
            var linkItem = that.selected[linkId];
            linkItem.access = ASC.Files.Constants.AceStatusEnum.Restrict;
            linkItem.accessName = getAceString(ASC.Files.Constants.AceStatusEnum.Restrict);
            linkItem.canEdit = false;
            linkItem.canRemove = false;
        } else {
            delete that.selected[linkId];
        }

        removeCurrentElement();
    }

    function removeCurrentElement() {
        if (that.currentElement) {
            that.currentElement.remove();
            that.pagging.contentItemsNumber--;
            that.$emptyList.toggle(that.pagging.contentItemsNumber == 0);
        }

        showSaveBtn();
    }

    function clickOnShowMore(element, event) {
        var element = element.parentElement;

        var groupId = element.getAttribute("data-group");

        if (groupId == null) {
            var data = getContentFragment(groupId, that.pagging.contentItemsNumber, that.pagging.contentItemsNumber + that.pagging.renderBy - 1);
            that.pagging.contentItemsNumber = data.count;
            that.contentListElement.append(data.fragment);
        } else {
            var data = getGroupItemsFragment(groupId, that.pagging.usersItemsNumber, that.pagging.usersItemsNumber + that.pagging.renderBy - 1);
            that.pagging.usersItemsNumber = data.count;
            element.parentElement.append(data.fragment);
        }

        element.remove();
    }

    function clickOnChangeOwner(element, event) {
        if (!that.targetData.ownerIdTmp || !that.selected[that.targetData.ownerIdTmp]) {
            that.targetData.ownerIdTmp = that.targetData.ownerId
        }

        var selectedOwner = {};
        selectedOwner[that.targetData.ownerIdTmp] = that.selected[that.targetData.ownerIdTmp];

        if (!that.ownerSelector) {

            var onlyUserCache = {};
            for (var userId in that.userCache) {
                var user = that.userCache[userId];
                if (!user.isVisitor) {
                    onlyUserCache[userId] = user;
                }
            }

            that.ownerSelector = new UserSelector({
                userCache: onlyUserCache,
                groupCache: that.groupCache,
                selected: selectedOwner,
                userDomGenerator: that.userDomGenerator,
                singleChoice: true,
                groupSelection: false,
                onSave: function (items) {
                    for (var itemId in items) {
                        var item = items[itemId];
                        var currentOwner = that.selected[that.targetData.ownerIdTmp];
                        var newOwner = Object.assign({}, currentOwner, { id: item.id, name: item.name, avatar: item.avatar })

                        delete that.selected[currentOwner.id];

                        that.selected[item.id] = newOwner;
                        that.targetData.ownerIdTmp = item.id;
                    }

                    renderContent();
                    showSaveBtn();
                }
            });

            that.$topContentContainer.append(that.ownerSelector.dialogElement);

            that.ownerSelector.dialogElement.style.top = "90px";

            that.ownerSelectorDialogHelper = new DialogHelper(null, that.ownerSelector.dialogElement, onBeforeShowOwnerSelectorDialog, null, false, true);

            that.ownerSelector.onCancel = that.ownerSelectorDialogHelper.hide;

        } else {
            that.ownerSelector.changeData(selectedOwner);
        }

        that.ownerSelectorDialogHelper.show();
    }

    function clickOnAccessElement(element, event) {
        if (event.target.classList.contains("uneditable")) {
            return;
        }

        var isLink = element.classList.contains("us-linkname") || element.classList.contains("us-folder-linkname");
        var canReadWrite = isLink ? that.viewSettings.canLinkReadWrite : true;
        var canFillForms = !isLink;
        var user = that.userCache[element.id];
        var hasVisitor = user && user.isVisitor;
        var onlyRestrict = isLink && !that.viewSettings.externalLinksAvailable;
        var selectedItem = that.selected[element.id];
        var canRemove = selectedItem && selectedItem.canRemove && !element.getAttribute("data-group");

        adjustAccessRightsDialogs(that.$itemAccessRightsDialog, canReadWrite, canFillForms, canRemove, hasVisitor, onlyRestrict);

        displayContentListDialog(element, that.itemAccessRightsDialogHelper, that.$itemAccessRightsDialog);
    }

    function clickOnItemAccessRightsElement(element, event) {
        var attr = element.getAttribute("data-id");

        if (attr == "remove") {
            clickOnItemAccessRightsRemoveElement(element);
            return;
        }

        var access = +attr;
        var accessName = getAceString(access);
        var id = that.currentElement.id;
        var isGroup = that.currentElement.classList.contains("us-groupname");
        var isLink = that.currentElement.classList.contains("us-linkname") || that.currentElement.classList.contains("use-folder-linkname");
        var accessElement = that.currentElement.querySelector(".us-access");
        var static = accessElement.classList.contains("static");
        accessElement.setAttribute("class", "us-access access-" + access + (static ? " static" : ""));
        accessElement.setAttribute("title", accessName);

        if (isGroup) {
            that.selected[id].access = access;
            that.selected[id].accessName = accessName;
            openGroup(that.currentElement, true);
        } else if (isLink) {
            that.selected[id].access = access;
            that.selected[id].accessName = accessName;
        } else {
            var groupId = that.currentElement.getAttribute("data-group");
            if (groupId) {
                if (that.selected[id] !== undefined) {
                    that.selected[id].access = access;
                    that.selected[id].accessName = accessName;
                    var itemElement = that.contentListElement.children[id];
                    if (itemElement && itemElement.classList.contains("us-name")) {
                        var itemAccessElement = itemElement.querySelector(".us-access");
                        itemAccessElement.setAttribute("class", "us-access access-" + access);
                        itemAccessElement.setAttribute("title", accessName);
                    }
                } else {
                    var infoText = that.userDomGenerator.createNameInfoText(FilesJSResource.IndividualRights);
                    that.currentElement.querySelector(".us-name-text").append(infoText);
                    that.currentElement.classList.add("info");

                    var user = that.userCache[id];
                    that.selected[id] = { id: id, name: user.displayName, avatar: user.avatarSmall, access: access, accessName: accessName, canEdit: true, canRemove: true };

                    var lastContenElement = that.contentListElement.lastElementChild;
                    var isShowMoreVisible = lastContenElement && lastContenElement.classList.contains("us-show-more-row");

                    if (!isShowMoreVisible) {
                        var item = Object.assign({}, that.selected[id]);
                        item.selected = false;
                        item.disabled = that.selectionMode == selectionModeEnum.group;
                        item.withoutTitle = that.viewSettings.showTooltop;

                        var userElement = that.userDomGenerator.createGroupItem(item)
                        that.contentListElement.append(userElement);
                        that.pagging.contentItemsNumber++;
                    }
                }
            } else {
                that.selected[id].access = access;
                that.selected[id].accessName = accessName;
                if (that.openedGroupElement) {
                    var index = that.groupCache[that.openedGroupElement.id].users.indexOf(id);
                    if (index != -1) {
                        var itemElement = that.openedGroupElement.nextElementSibling.children[id];
                        if (itemElement && itemElement.classList.contains("us-name")) {
                            var itemAccessElement = itemElement.querySelector(".us-access");
                            itemAccessElement.setAttribute("class", "us-access access-" + access);
                            itemAccessElement.setAttribute("title", accessName);
                        }
                    }
                }
            }
        }

        showSaveBtn();

        that.itemAccessRightsDialogHelper.hide();
    }

    function clickOnItemAccessRightsRemoveElement(element) {

        var id = that.currentElement.id;
        var isGroup = that.currentElement.classList.contains("us-groupname");
        var isLink = that.currentElement.classList.contains("us-linkname") || that.currentElement.classList.contains("us-folder-linkname");

        if (isGroup) {
            if (that.currentElement == that.openedGroupElement) {
                that.openedGroupElement = null;
            }
            delete that.selected[id];
            removeCurrentElement();
        } else if (isLink) {
            displayContentListDialog(that.currentElement, that.linkDeleteDialogHelper, that.$linkDeleteDialog);
        } else {
            if (that.openedGroupElement) {
                var index = that.groupCache[that.openedGroupElement.id].users.indexOf(id);
                if (index != -1) {
                    var itemElement = that.openedGroupElement.nextElementSibling.children[id];
                    if (itemElement && itemElement.classList.contains("us-name")) {
                        var openedGroup = that.selected[that.openedGroupElement.id];
                        var accessElement = itemElement.querySelector(".us-access");
                        accessElement.setAttribute("class", "us-access access-" + openedGroup.access);
                        accessElement.setAttribute("title", openedGroup.accessName);
                        itemElement.classList.remove("info");
                    }
                }
            }

            delete that.selected[id];
            removeCurrentElement();
        }

        that.itemAccessRightsDialogHelper.hide();
    }

    function clickOnMenuAccessRightsElement(element, event) {
        var access = +element.getAttribute("data-id");

        for (var itemId in that.tmpSelected) {
            var item = that.selected[itemId];
            if (item) {
                item.access = access;
                item.accessName = getAceString(access);
            } else {
                var user = that.userCache[itemId];
                that.selected[itemId] = { id: itemId, name: user.displayName, avatar: user.avatarSmall, access: access, accessName: getAceString(access), canEdit: true, canRemove: true };
            }
        }

        clickOnMenuActionCloseElement();

        that.menuAccessRightsDialogHelper.hide();
    }

    function clickOnAvatarElement(element, event) {
        if (element.classList.contains("unselectable")) {
            return;
        }

        var item = that.selected[element.id];
        var canEdit = item === undefined || item.canEdit;

        if (canEdit) {
            var selected = element.classList.toggle("selected");
            markItemSelected(element.id, selected);
        }

        if (element.classList.contains("us-groupname") || element.classList.contains("us-linkname") || element.classList.contains("us-folder-linkname")) {
            setSelectionMode(selectionModeEnum.root);
        } else {
            var groupId = element.getAttribute("data-group");
            if (groupId === null) {
                setSelectionMode(selectionModeEnum.root);
            } else {
                setSelectionMode(selectionModeEnum.group);
            }
        }

        if (!openActions(!canEdit)) {
            that.selectionMode = selectionModeEnum.none;
            that.contentListParentElement.classList.remove("rootonly");
            if (that.openedGroupElement) {
                openGroup(renderContent(that.openedGroupElement.id));
            } else {
                renderContent();
            }
        }
    }

    function clickOnGroupElement(element, event) {
        if (that.selectionMode == selectionModeEnum.root || element.classList.contains("unopenable")) {
            //clickOnAvatarElement(element, event);
        } else {
            openGroup(element);
        }
    }

    function clickOnLinkElement(element, event) {
        if (that.selectionMode == selectionModeEnum.root) {
            //clickOnAvatarElement(element, event);
        } else {
            if (event.target.classList.contains("us-settings-password")) {
                clickOnLinkPasswordElement(element, event);
            } else if (event.target.classList.contains("us-settings-lifetime")) {
                clickOnLinkLifetimeElement(element, event);
            } else if (event.target.classList.contains("textEdit")) {
                //event.stopPropagation();
            } else if (event.target.classList.contains("__apply")) {
                renameLink(element);
            } else if (event.target.classList.contains("__reset")) {
                displayLinkRenameBlock(element, false);
            } else {
                clickOnLinkNameElement(element, event);
            }
        }
    }

    function clickOnGroupItemElement(element, event) {
        //clickOnAvatarElement(element, event);
    }


    function openGroup(element, force) {
        if (!element || element.classList.contains("unopenable")) {
            return;
        }

        if (element != that.openedGroupElement || force) {
            if (that.openedGroupElement) {
                that.openedGroupElement.parentElement.classList.remove("opened");
                if (element.id != that.openedGroupElement.id) {
                    that.pagging.usersItemsNumber = 0;
                }
            }
            renderGroupItems(element.nextElementSibling, element.id);
            element.parentElement.classList.add("opened");
            that.openedGroupElement = element;
        } else {
            element.parentElement.classList.toggle("opened");
        }
    }

    function renderGroupItems(groupItemsElement, groupId) {
        var data = getGroupItemsFragment(groupId, 0, Math.max(that.pagging.usersItemsNumber, that.pagging.renderBy) - 1);
        that.pagging.usersItemsNumber = data.count;
        var newGroupItemsElement = that.userDomGenerator.createGroupItems();
        newGroupItemsElement.append(data.fragment);
        groupItemsElement.replaceWith(newGroupItemsElement);
    }

    function getGroupItemsFragment(groupId, from, to) {
        var result = {
            fragment: document.createDocumentFragment(),
            count: -1
        }

        var groupAccess = that.selected[groupId].access;
        var groupAccessName = getAceString(groupAccess);
        var usersInGroup = that.groupCache[groupId].users;

        for (var userId of usersInGroup) {
            var user = that.userCache[userId];
            if (!user) {
                continue; //terminated
            }

            result.count++;

            if (result.count < from) {
                continue;
            }

            if (result.count > to) {
                break;
            }

            var selected = that.tmpSelected[userId] !== undefined;
            var userItem = that.selected[userId];
            if (userItem) {
                var item = Object.assign({}, userItem);
                item.group = groupId;
                item.selected = selected;
                item.disabled = that.selectionMode == selectionModeEnum.group && !userItem.canEdit;
                item.withoutTitle = that.viewSettings.showTooltop;
                item.infoText = FilesJSResource.IndividualRights;
                result.fragment.append(that.userDomGenerator.createGroupItem(item));
            } else {
                var item = { id: userId, group: groupId, name: user.displayName, avatar: user.avatarSmall, selected: selected, access: groupAccess, accessName: groupAccessName };
                item.withoutTitle = that.viewSettings.showTooltop;
                result.fragment.append(that.userDomGenerator.createGroupItem(item));
            }
        }

        if (result.count > to) {
            result.fragment.append(that.userDomGenerator.createShowMoreRow({ name: ResourceJS.UserSelectorShowMore, group: groupId }));
            return result;
        }

        result.count++;
        return result;
    }

    function renderContent(outGroupId) {

        var data = getContentFragment(outGroupId, 0, Math.max(that.pagging.contentItemsNumber, that.pagging.renderBy) - 1);

        that.pagging.contentItemsNumber = data.count;

        var newContentListElement = that.userDomGenerator.createContentList();
        newContentListElement.append(data.fragment);

        that.contentListElement.replaceWith(newContentListElement);
        that.contentListElement = newContentListElement;

        that.$emptyList.toggle(that.pagging.contentItemsNumber == 0);

        return data.outGroupElement;
    }

    function getContentFragment(outGroupId, from, to) {

        var result = {
            fragment: document.createDocumentFragment(),
            count: -1,
            outGroupElement: null
        };

        var groupSelectionMode = that.selectionMode == selectionModeEnum.group;
        var rootSelectionMode = that.selectionMode == selectionModeEnum.root;

        for (var itemId in that.selected) {
            var item = Object.assign({}, that.selected[itemId]);

            if (item.isLink && !item.canEdit && !item.inherited) {
                continue;
            }

            result.count++;

            if (result.count < from) {
                continue;
            }

            if (result.count > to) {
                break;
            }

            item.selected = !groupSelectionMode && that.tmpSelected[itemId] != undefined;
            item.disabled = groupSelectionMode || (rootSelectionMode && !item.canEdit);

            if (item.isLink) {
                item.infoText = that.viewSettings.externalLinksAvailable ? isShortenedLink(item.link) ? FilesJSResource.Shortened : "" : FilesJSResource.ExternalLinkDisabled
                var linkElement = that.userDomGenerator.createLink(item);
                result.fragment.append(linkElement);
            } else if (item.isGroup) {
                var groupElement = that.userDomGenerator.createGroup(item);
                if (itemId == outGroupId) {
                    result.outGroupElement = groupElement.querySelector(".us-name");
                }
                result.fragment.append(groupElement);
            } else {
                item.withoutTitle = that.viewSettings.showTooltop;
                var userElement = that.userDomGenerator.createGroupItem(item);
                result.fragment.append(userElement);
            }
        }

        if (result.count > to) {
            result.fragment.append(that.userDomGenerator.createShowMoreRow({ name: ResourceJS.UserSelectorShowMore }));
            return result;
        }

        result.count++;
        return result;
    }

    function renderAccessRightsDialogs() {
        function rerenderImgDialog(element) {
            var fragment = that.userDomGenerator.createAccessRightsImgDialogContent(that.accessRights);
            that.userDomGenerator.clearElement(element);
            element.append(fragment);
        }

        function rerenderDialog(element) {
            var content = that.userDomGenerator.createAccessRightsDialogContent(that.accessRights);
            element.replaceWith(content);
        }

        rerenderImgDialog(that.$menuAccessRightsDialog.get(0));
        rerenderImgDialog(that.$itemAccessRightsDialog.get(0));

        rerenderDialog(that.$menuSelectDialog.find(".dropdown-content:last").get(0));
    }

    function adjustAccessRightsDialogs($accessRightsDialog, canReadWrite, canFillForms, canRemove, hasVisitor, onlyRestrict) {
        for (var item of that.accessRights) {
            if (item.disabled) {
                continue;
            }

            if (onlyRestrict && item.id != ASC.Files.Constants.AceStatusEnum.Restrict) {
                $accessRightsDialog.find(".access-rights-item[data-id=" + item.id + "]").hide();
                continue;
            }

            var display = !(item.hideForVisitor && hasVisitor)
                && (item.id == ASC.Files.Constants.AceStatusEnum.ReadWrite ? canReadWrite : true)
                && (item.id == ASC.Files.Constants.AceStatusEnum.FillForms ? canFillForms : true);

            $accessRightsDialog.find(".access-rights-item[data-id=" + item.id + "]").toggle(display);
        }

        $accessRightsDialog.find(".dropdown-item-seporator, .access-rights-item[data-id=remove]").toggle(canRemove);
    }

    function onBeforeShowAddLinkActionDialog() {
        if (that.$addLinkBtnArrow.hasClass("disable")) {
            return false;
        }

        var btnArrowPosition = that.$addLinkBtnArrow.position();
        var btnArrowHeight = that.$addLinkBtnArrow.outerHeight();
        var btnArrowWidth = that.$addLinkBtnArrow.outerWidth();
        var addLinkActionDialogWidth = that.$addLinkActionDialog.outerWidth();
        that.$addLinkActionDialog.css("top", (btnArrowPosition.top + btnArrowHeight + 4) + "px");
        var left = btnArrowPosition.left + btnArrowWidth - addLinkActionDialogWidth;
        that.$addLinkActionDialog.css("left", (left > 0 ? left : 0) + "px");
        return true;
    }

    function clickOnAddLinkActionDialogItemElement() {
        that.addLinkActionDialogHelper.hide();

        if (jq(this).is(that.$addLinkActionDialogNew)) {
            that.$addLinkBtn.trigger("click");
        }
    }

    function clickOnSaveActionDialogItemElement() {
        that.saveActionDialogHelper.hide();

        var $element = jq(this);

        if ($element.is(that.$saveActionDialogMessage)) {
            that.messageDialogHelper.show();
            return;
        }

        if ($element.is(that.$saveActionDialogSilent)) {
            that.save(false, "");
            return;
        }

        if ($element.is(that.$saveActionDialogCopy)) {
            that.save(true, "");
            return;
        }
    }

    function onBeforeShowAdvancedSettingsDialog() {
        if (that.$settingsBtns.hasClass("disable")) {
            return false;
        }

        that.$advancedSettingsDialogDenyDownloadCbx.prop("disabled", false);
        that.$advancedSettingsDialogDenyDownloadLabel.removeClass("gray-text").show();

        that.$advancedSettingsDialogDenySharingCbx.prop("disabled", false);
        that.$advancedSettingsDialogDenySharingLabel.removeClass("gray-text").show();

        if (that.viewSettings.rootFolderTypeCommon || ASC.Files.Folders && ASC.Files.Folders.folderContainer == "corporate") {
            that.viewSettings.denySharing = false;
            that.$advancedSettingsDialogDenySharingCbx.prop("disabled", true);
            that.$advancedSettingsDialogDenySharingLabel.addClass("gray-text").hide();
        }

        if (that.targetData.encrypted) {
            that.viewSettings.denyDownload = false;
            that.$advancedSettingsDialogDenyDownloadCbx.prop("disabled", true);
            that.$advancedSettingsDialogDenyDownloadLabel.addClass("gray-text").hide();
        }

        that.$advancedSettingsDialogDenyDownloadCbx.prop("checked", that.viewSettings.denyDownload);
        that.$advancedSettingsDialogDenySharingCbx.prop("checked", that.viewSettings.denySharing);
        return true;
    }

    function onBeforeShowLinkActionDialog() {
        checkIsEmbedEnabled();
        checkIsShortenedLink(false);
        checkIsSettingsEnabled();
        checkIsDeletionEnabled();
        return checkVisibleElements();
    }

    function onBeforeShowLinkShareDialog() {
        checkMailAccounts();
        updateSocialLink();
        that.$linkShareDialogSocialContainer.find(".facebook, .twitter").toggle(that.$linkSettingsInput.data("social"));
        return true;
    }

    function checkMailAccounts() {
        if (that.viewSettings.hasMailAccounts !== undefined) {
            return;
        }

        window.Teamlab.getAccounts({}, {
            success: function (params, res) {
                if (res && res.length) {
                    that.viewSettings.hasMailAccounts = res.some(function (item) {
                        return item.enabled && !item.isGroup;
                    });
                } else {
                    that.viewSettings.hasMailAccounts = false;
                }
            },
            error: function (params, err) {
                that.viewSettings.hasMailAccounts = false;
                console.log(err);
            }
        });
    }

    function updateSocialLink() {
        var url = that.selected[that.currentElement.id].link;
        var entryType = that.targetData.entryData ? that.targetData.entryData.entryType : ASC.Files.UI.parseItemId(that.targetData.id).entryType;

        that.$linkShareDialogInput.val(url);

        var link = encodeURIComponent(url);

        if (!!ASC.Resources.Master.UrlShareFacebook) {
            that.$linkShareDialogSocialContainer.find(".facebook").attr("href", ASC.Resources.Master.UrlShareFacebook.format(link, encodeURIComponent(that.viewSettings.title), "", ""));
        } else {
            that.$linkShareDialogSocialContainer.find(".facebook").remove();
        }
        if (!!ASC.Resources.Master.UrlShareTwitter) {
            that.$linkShareDialogSocialContainer.find(".twitter").attr("href", ASC.Resources.Master.UrlShareTwitter.format(link));
        } else {
            that.$linkShareDialogSocialContainer.find(".twitter").remove();
        }

        var urlShareMail = "mailto:?subject={1}&body={0}";
        var subject = entryType === "file" ? 
            ASC.Files.FilesJSResource.shareLinkMailSubject.format(that.viewSettings.title) : 
            ASC.Files.FilesJSResource.shareFolderLinkMailSubject.format(that.viewSettings.title);
        
        var body = entryType === "file" ? 
            ASC.Files.FilesJSResource.shareLinkMailBody.format(that.viewSettings.title, url) : 
            ASC.Files.FilesJSResource.shareFolderLinkMailBody.format(that.viewSettings.title, url);
        
        that.$linkShareDialogSocialContainer.find(".mail").attr("href", urlShareMail.format(encodeURIComponent(body), encodeURIComponent(subject)));
    }

    function onBeforeShowLinkEmbedDialog() {
        setEmbeddedSize();
        return true;
    }

    function clickOnLinkShareDialogSocialItemElement(element, event) {
        var openLink = element.getAttribute("href");
        var entryType = that.targetData.entryData ? that.targetData.entryData.entryType : ASC.Files.UI.parseItemId(that.targetData.id).entryType;

        if (element.classList.contains("mail") && !ASC.Resources.Master.Personal) {

            if (that.viewSettings.hasMailAccounts === false) {
                return true;
            }

            var winMail = window.open(ASC.Desktop ? "" : ASC.Files.Constants.URL_LOADER);

            var message = new ASC.Mail.Message();
            message.subject = entryType === "file" ?
                ASC.Files.FilesJSResource.shareLinkMailSubject.format(that.viewSettings.title) :
                ASC.Files.FilesJSResource.shareFolderLinkMailSubject.format(that.viewSettings.title);

            var linkFormat = "<a href=\"{0}\">{1}</a>";
            var linkUrl = that.selected[that.currentElement.id].link;
            var linkName = linkFormat.format(Encoder.htmlEncode(linkUrl), Encoder.htmlEncode(that.viewSettings.title));
            var link = linkFormat.format(Encoder.htmlEncode(linkUrl), Encoder.htmlEncode(linkUrl));
            var body = entryType === "file" ?
                ASC.Files.FilesJSResource.shareLinkMailBody.format(linkName, link) :
                ASC.Files.FilesJSResource.shareFolderLinkMailBody.format(linkName, link);

            message.body = body;

            ASC.Mail.Utility.SaveMessageInDrafts(message)
                .done(function (_, data) {
                    var url = data.messageUrl;
                    if (winMail && winMail.location) {
                        winMail.location.href = url;
                    } else {
                        winMail = window.open(url, "_blank");
                    }
                })
                .fail(function () {
                    winMail.close();
                    window.location.href = openLink;
                });

            return false;
        }

        window.open(openLink, "new", "height=600,width=1020,fullscreen=0,resizable=0,status=0,toolbar=0,menubar=0,location=1");
        return false;
    }

    function setEmbeddedSize() {
        var target = jq(this);
        if (target.is(".embed-size-item")) {
            if (target.hasClass("embed-size-6x8")) {
                that.$linkEmbedDialogSizeCustomWidth.val("600px");
                that.$linkEmbedDialogSizeCustomHeight.val("800px");
            } else if (target.hasClass("embed-size-4x6")) {
                that.$linkEmbedDialogSizeCustomWidth.val("400px");
                that.$linkEmbedDialogSizeCustomHeight.val("600px");
            } else {
                that.$linkEmbedDialogSizeCustomWidth.val("100%");
                that.$linkEmbedDialogSizeCustomHeight.val("100%");
            }
        }

        var width = that.$linkEmbedDialogSizeCustomWidth.val();
        var height = that.$linkEmbedDialogSizeCustomHeight.val();

        width = (/\d+(px|%)?/gim.exec(width.toLowerCase().trim()) || ["100%"])[0];
        if (width == Math.abs(width)) {
            width += "px";
        }
        height = (/\d+(px|%)?/gim.exec(height.toLowerCase().trim()) || ["100%"])[0];
        if (height == Math.abs(height)) {
            height += "px";
        }

        that.$linkEmbedDialogSizeCustomWidth.val(width);
        that.$linkEmbedDialogSizeCustomHeight.val(height);

        jq(".embed-size-item").removeClass("selected");
        if (width == "600px" && height == "800px") {
            jq(".embed-size-6x8").addClass("selected");
        } else if (width == "400px" && height == "600px") {
            jq(".embed-size-4x6").addClass("selected");
        } else if (width == "100%" && height == "100%") {
            jq(".embed-size-1x1").addClass("selected");
        }

        generateEmbeddedString(width, height);
    }

    function generateEmbeddedString(width, height) {
        width = width || "100%";
        height = height || "100%";

        var url = that.selected[that.currentElement.id].link;
        var embeddedString = "";

        if (ASC.Files.Utility.CanWebView(that.viewSettings.title)) {
            embeddedString = '<iframe src="{0}" width="{1}" height="{2}" frameborder="0" scrolling="no" allowtransparency></iframe>';
            url = url + "&action=embedded";
        } else if (typeof ASC.Files.ImageViewer != "undefined" && ASC.Files.Utility.CanImageView(that.viewSettings.title)) {
            embeddedString = '<img src="{0}" width="{1}" height="{2}" alt="" />';
        }

        embeddedString = embeddedString.format(url, width, height);

        that.$linkEmbedDialogInput.val(embeddedString).attr("title", embeddedString);
    }

    function checkIsShortenedLink(afterShortening) {
        var item = that.selected[that.currentElement.id];
        var shortened = isShortenedLink(item.link);

        that.$linkActionDialogShort.toggle(!shortened);
        that.$linkShareDialogShort.toggle(!shortened);

        if (afterShortening && shortened) {
            item.infoText = FilesJSResource.Shortened;
            var newLink = that.userDomGenerator.createLink(item);
            that.currentElement.replaceWith(newLink);
        } else {
            that.currentElement.classList.toggle("info", shortened || !that.viewSettings.externalLinksAvailable);
        }
    }

    function isShortenedLink(url) {
        return url ? !(new RegExp("/Products/Files/", "i").test(url)) : false;
    }

    function getShortenLink() {
        var fileId = that.targetData.entryData ? that.targetData.entryData.id : ASC.Files.UI.parseItemId(that.targetData.id).entryId;
        var linkId = that.currentElement.id;
        var entryType = that.targetData.entryData ? that.targetData.entryData.entryType : ASC.Files.UI.parseItemId(that.targetData.id).entryType;
        var isFolder = entryType !== "file";

        ASC.Files.ServiceManager.getShortenLink(ASC.Files.ServiceManager.events.GetShortenLink, { fileId: fileId, linkId: linkId, isFolder: isFolder });
    };

    function onGetShortenLink(result, params, errorMessage) {
        if (!result || typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.OperationFailedMsg, true);
            return;
        }

        that.selected[that.currentElement.id].link = result;

        updateSocialLink();
        checkIsShortenedLink(true);

        if (tryCopyTextToClipboard(result)) {
            ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.LinkCopySuccess);
        } else {
            ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.OperationSuccededMsg);
        }
    };

    function tryCopyTextToClipboard(text) {
        var copyElement = document.createElement("input");
        copyElement.setAttribute("type", "text");
        copyElement.setAttribute("value", text);
        copyElement = document.body.appendChild(copyElement);
        copyElement.select();

        try {
            return document.execCommand("copy");
        } catch (e) {
            return false;
        } finally {
            copyElement.remove();
        }
    }

    function checkIsEmbedEnabled() {
        var enabled = ASC.Files.Utility.CanWebView(that.viewSettings.title)
            || (typeof ASC.Files.ImageViewer != "undefined" && ASC.Files.Utility.CanImageView(that.viewSettings.title));

        that.$linkActionDialogEmbed.toggle(enabled);
    }

    function checkIsSettingsEnabled() {
        var currentElement = that.selected[that.currentElement.id];
        
        var hasSettings = that.viewSettings.externalLinksAvailable && !!currentElement.linkSettings 
            && !currentElement.inherited;

        that.$linkActionDialog.find(".dropdown-item-seporator:first").toggle(hasSettings);
        that.$linkActionDialogPassword.toggle(hasSettings);
        that.$linkActionDialogLifetime.toggle(hasSettings);
        that.$linkActionDialogRename.toggle(hasSettings);
    }

    function checkIsDeletionEnabled() {
        var currentElement = that.selected[that.currentElement.id];

        that.$linkActionDialog.find(".dropdown-item-seporator:last").toggle(!currentElement.inherited && that.viewSettings.externalLinksAvailable);
        that.$linkActionDialogDelete.parent().toggle(!currentElement.inherited);
    }

    function checkVisibleElements() {
        return that.$linkActionDialog.find("li").filter(function (_, li) {
            return li.style.display !== "none";
        }).length > 0;
    }

    function mousedownOnReadonlyInput() {
        this.select();
        return false;
    }

    function keypressOnReadonlyInput(event) {
        return event.ctrlKey && (event.charCode === ASC.Files.Common.keyCode.C || event.keyCode === ASC.Files.Common.keyCode.insertKey);
    }

    function clearDialogElements(link, dialog) {
        if (!link.classList.contains("disabled")) {
            dialog.find(".textEdit, .textEditCalendar").val("");
        }
    }

    function disableDialogElements(disable, dialog) {
        dialog.find(".textEdit, .textEditCalendar, .autodelete").prop("disabled", disable);
        dialog.find(".img-btn, .link").toggleClass("disabled", disable);
    }

    function onBeforeShowLinkPasswordDialog() {
        var settings = that.selected[that.currentElement.id].linkSettings;
        that.$linkPasswordDialogInput.attr("type", "password").val(settings.password || "");
        that.$linkPasswordDialogShow.removeClass("hide");
        that.$linkPasswordDialogCbx.prop("checked", !!settings.password).change();
        return true;
    }

    function showHidePassword() {
        if (that.$linkPasswordDialogShow.hasClass("disabled")) {
            return;
        }

        if (that.$linkPasswordDialogShow.hasClass("hide")) {
            that.$linkPasswordDialogInput.attr("type", "password");
            that.$linkPasswordDialogShow.removeClass("hide");
            return;
        }

        that.$linkPasswordDialogInput.attr("type", "text");
        that.$linkPasswordDialogShow.addClass("hide");
    }

    function setRandomPassword() {
        if (that.$linkPasswordDialogRandom.hasClass("disabled")) {
            return;
        }

        Teamlab.getRandomPassword(
            null,
            {
                before: function () {
                    LoadingBanner.displayLoading();
                },
                success: function (_, pwd) {
                    that.$linkPasswordDialogInput.attr("type", "text").val(pwd);
                    that.$linkPasswordDialogShow.addClass("hide");
                },
                after: function () {
                    LoadingBanner.hideLoading();
                },
                error: function (_, error) {
                    console.log(error);
                }
            });
    }

    function onSaveLinkPasswordSettings() {
        var password = null;
        if (that.$linkPasswordDialogCbx.prop("checked")) {
            password = that.$linkPasswordDialogInput.val().trim() || null;
        }
        var settings = that.selected[that.currentElement.id].linkSettings;
        if (settings.password == password) {
            return;
        }
        settings.password = password;
        var iconElement = that.currentElement.querySelector(".us-settings-password");
        iconElement.classList.toggle("enabled", password);
        showSaveBtn();
    }

    function onBeforeShowLinkLifetimeDialog() {
        var settings = that.selected[that.currentElement.id].linkSettings;
        if (settings.expirationDate) {
            var expirationDate = new Date(settings.expirationDate);
            //that.$linkLifetimeDialogDateInput.datepicker("setDate", expirationDate); can't set past date
            that.$linkLifetimeDialogDateInput.val(jQuery.datepicker.formatDate(ASC.Resources.Master.DatepickerDatePattern, expirationDate));
            that.$linkLifetimeDialogTimeInput.val(expirationDate.toLocaleTimeString("en-US", { hourCycle: 'h23' }).substring(0, 5));
        } else {
            that.$linkLifetimeDialogDateInput.val("");
            that.$linkLifetimeDialogTimeInput.val("");
        }
        that.$linkLifetimeDialogDateInput.removeClass("error");
        that.$linkLifetimeDialogTimeInput.removeClass("error");
        that.$linkLifetimeDialogCbx.prop("checked", !!settings.expirationDate).change();
        that.$linkLifetimeDialogAutodeleteCbx.prop("checked", !!settings.autoDelete);
        return true;
    }

    function onSaveLinkLifetimeSettings() {
        var autoDelete = false;
        var expirationDate = null;
        var expired = false;
        if (that.$linkLifetimeDialogCbx.prop("checked")) {
            var date = that.$linkLifetimeDialogDateInput.datepicker("getDate");
            var datestr = that.$linkLifetimeDialogDateInput.val().trim();

            if (!date || !jq.isDateFormat(datestr)) {
                that.$linkLifetimeDialogDateInput.addClass("error");
                return;
            }

            autoDelete = that.$linkLifetimeDialogAutodeleteCbx.prop("checked");

            var timestr = that.$linkLifetimeDialogTimeInput.val().trim();
            var timeParts = timestr.split(":");
            date.setHours(timeParts[0]);
            date.setMinutes(timeParts.length > 1 ? timeParts[1] : 0);
            expirationDate = ServiceFactory.serializeTimestamp(date);

            var localDate = new Date();
            var utcDate = new Date(localDate.getTime() + localDate.getTimezoneOffset() * 60000);
            var tenantDate = new Date(utcDate.getTime() + ASC.Resources.Master.CurrentTenantTimeZone.UtcOffset * 60000);
            expired = tenantDate > date;

            if (expired) {
                that.$linkLifetimeDialogDateInput.addClass("error");
                that.$linkLifetimeDialogTimeInput.addClass("error");
                return;
            }
        }

        var settings = that.selected[that.currentElement.id].linkSettings;
        if (settings.expirationDate == expirationDate && settings.autoDelete == autoDelete) {
            that.linkLifetimeDialogHelper.hide();
            return;
        }

        settings.autoDelete = autoDelete;
        settings.expirationDate = expirationDate;
        settings.expired = expired;

        var iconElement = that.currentElement.querySelector(".us-settings-lifetime");
        iconElement.classList.toggle("enabled", expirationDate);
        iconElement.classList.toggle("warning", expired);

        that.linkLifetimeDialogHelper.hide();
        showSaveBtn();
    }

    function initClipboard() {
        if (!ASC.Clipboard.enable) {
            that.$addLinkActionDialogCopy.parent().remove();
            that.$linkActionDialogCopy.remove();
            that.$linkShareDialogCopy.remove();
            that.$linkEmbedDialogCopy.remove();
            that.$saveActionDialogCopyParent.remove();
            that.$saveActionDialogSeporator.remove();
            that.$linkPasswordDialogCopy.remove();
            return;
        }

        that.clipboard.directLink = ASC.Clipboard.createManually(
            that.$addLinkActionDialogCopy.get(0),
            {
                text: function () {
                    return window.location.protocol + '//' + window.location.hostname + that.targetData.entryLink;
                }
            },
            function () {
                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.LinkCopySuccess);
            }
        );

        that.clipboard.internalLink = ASC.Clipboard.createManually(
            that.$saveActionDialogCopy.get(0),
            {
                text: function () {
                    return window.location.protocol + '//' + window.location.hostname + that.targetData.entryLink;
                }
            },
            function () {
                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.LinkCopySuccess);
            }
        );

        that.clipboard.externalLinkAction = ASC.Clipboard.createManually(
            that.$linkActionDialogCopy.get(0),
            {
                text: function () {
                    return that.selected[that.currentElement.id].link;
                }
            },
            function () {
                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.LinkCopySuccess);
            }
        );

        that.clipboard.externalLinkShare = ASC.Clipboard.createManually(
            that.$linkShareDialogCopy.get(0),
            {
                text: function () {
                    return that.selected[that.currentElement.id].link;
                }
            },
            function () {
                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.LinkCopySuccess);
            }
        );

        that.clipboard.externalLinkEmbed = ASC.Clipboard.createManually(
            that.$linkEmbedDialogCopy.get(0),
            {
                target: function () {
                    return that.$linkEmbedDialogInput.get(0);
                }
            },
            function () {
                ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.CodeCopySuccess);
            }
        );

        that.clipboard.externalLinkPassword = ASC.Clipboard.createManually(
            that.$linkPasswordDialogCopy.get(0),
            {
                text: function () {
                    return that.$linkPasswordDialogCopy.hasClass("disabled") ? false : 'link: ' + that.selected[that.currentElement.id].link + '\npassword: ' + that.$linkPasswordDialogInput.val()
                }
            },
            function () {
                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.LinkCopySuccess);
            }
        );
    }

    function initTargetData(ids, titles, asFlat, providerEntry) {
        var isArray = Array.isArray(ids);

        that.targetData.id = isArray && ids.length == 1 ? ids[0] : ids;
        that.targetData.sharingInfo = null;
        that.targetData.ownerId = null;

        that.targetData.multiple = Array.isArray(that.targetData.id);
        that.targetData.encrypted = false;
        that.targetData.entryData = null;
        that.targetData.entryLink = null;
        that.targetData.isThirdparty = asFlat ? providerEntry : false;

        if (that.targetData.multiple) {
            if (ASC.Files.ThirdParty) {
                that.targetData.isThirdparty = ids.some(function (id) {
                    var itemId = ASC.Files.UI.parseItemId(id);
                    var entryObject = ASC.Files.UI.getEntryObject(itemId.entryType, itemId.entryId);
                    var entryData = ASC.Files.UI.getObjectData(entryObject);
                    return ASC.Files.ThirdParty.isThirdParty(entryData);
                });
            }
            return;
        }

        var itemId = ASC.Files.UI.parseItemId(isArray ? ids[0] : ids);
        var entryObject = ASC.Files.UI.getEntryObject(itemId.entryType, itemId.entryId);

        that.targetData.entryData = ASC.Files.UI.getObjectData(entryObject);

        if (that.targetData.entryData) {
            that.targetData.encrypted = that.targetData.entryData.encrypted;
            that.targetData.isThirdparty = ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty(that.targetData.entryData);
        } else if (asFlat === true && ASC.Desktop && ASC.Desktop.setAccess) {
            that.targetData.encrypted = true;
        }

        if (!that.targetData.encrypted) {
            var entryLink = ASC.Files.UI.getEntryLink(itemId.entryType, itemId.entryId, isArray ? titles[0] : titles);
            that.targetData.entryLink = entryLink.indexOf(ASC.Files.Constants.URL_BASE) > -1 ? entryLink : ASC.Files.Constants.URL_BASE + entryLink;
        }
    }

    function initViewSettings(ids, titles, asFlat, rootFolderTypeCommon, origin, denyDownload, denySharing) {
        var isArray = Array.isArray(ids);

        that.viewSettings.title = that.targetData.multiple
            ? ids.length.toString()
            : that.targetData.entryData ? that.targetData.entryData.title : isArray ? titles[0] : titles;

        that.viewSettings.asFlat = asFlat;
        that.viewSettings.showTooltop = !asFlat && !ASC.Desktop;
        that.viewSettings.rootFolderTypeCommon = rootFolderTypeCommon;

        that.viewSettings.notifyAvailable = !rootFolderTypeCommon
            && (!ASC.Files.Folders || ASC.Files.Folders.folderContainer == "my" || ASC.Files.Folders.folderContainer == "forme" || ASC.Files.Folders.folderContainer == "privacy");

        that.viewSettings.externalLinksAvailable = false;

        that.viewSettings.originForPost = asFlat && origin ? origin : "*";

        that.viewSettings.displaySettings = !that.targetData.isThirdparty;
        that.viewSettings.denyDownload = asFlat ? denyDownload : that.targetData.entryData ? that.targetData.entryData.deny_download : false;
        that.viewSettings.denySharing = asFlat ? denySharing : that.targetData.entryData ? that.targetData.entryData.deny_sharing : false;

        that.viewSettings.canWebCustomFilterEditing = false;
        that.viewSettings.canWebReview = false;
        that.viewSettings.canWebRestrictedEditing = false;
        that.viewSettings.canWebComment = false;
        that.viewSettings.canLinkReadWrite = false;

        if (that.targetData.multiple) {
            that.viewSettings.canWebCustomFilterEditing = true;
            that.viewSettings.canWebReview = true;
            that.viewSettings.canWebRestrictedEditing = true;
            that.viewSettings.canWebComment = true;

            titles.forEach(function (title, index) {
                if (ASC.Files.UI.parseItemId(ids[index]).entryType == "folder") {
                    that.viewSettings.displaySettings = that.viewSettings.canWebCustomFilterEditing = that.viewSettings.canWebReview = that.viewSettings.canWebRestrictedEditing = that.viewSettings.canWebComment = false;
                    return false;
                }
                if (!ASC.Files.Utility.CanWebCustomFilterEditing(title)) {
                    that.viewSettings.canWebCustomFilterEditing = false;
                }
                if (!ASC.Files.Utility.CanWebReview(title)) {
                    that.viewSettings.canWebReview = false;
                }
                if (!ASC.Files.Utility.CanWebRestrictedEditing(title)) {
                    that.viewSettings.canWebRestrictedEditing = false;
                }
                if (!ASC.Files.Utility.CanWebComment(title)) {
                    that.viewSettings.canWebComment = false;
                }
                if (!ASC.Files.Utility.CanWebView(title)) {
                    that.viewSettings.displaySettings = false;
                }
                return that.viewSettings.canWebCustomFilterEditing || that.viewSettings.canWebReview || that.viewSettings.canWebRestrictedEditing || that.viewSettings.canWebComment;
            });

            if (that.targetData.isThirdparty || ASC.Files.Folders.folderContainer == "privacy") {
                that.viewSettings.canWebRestrictedEditing = false;
            }

        } else {
            var entryType = that.targetData.entryData ? that.targetData.entryData.entryType : ASC.Files.UI.parseItemId(isArray ? ids[0] : ids).entryType;
            var entryTitle = that.viewSettings.title;
            if (entryType == "file") {
                that.viewSettings.canWebCustomFilterEditing = ASC.Files.Utility.CanWebCustomFilterEditing(entryTitle);
                that.viewSettings.canWebReview = ASC.Files.Utility.CanWebReview(entryTitle);
                that.viewSettings.canWebRestrictedEditing = ASC.Files.Utility.CanWebRestrictedEditing(entryTitle);
                that.viewSettings.canWebComment = ASC.Files.Utility.CanWebComment(entryTitle);
                that.viewSettings.canLinkReadWrite = ASC.Files.Utility.CanWebEdit(entryTitle) && !ASC.Files.Utility.MustConvert(entryTitle);
                that.viewSettings.displaySettings = that.viewSettings.displaySettings && ASC.Files.Utility.CanWebView(entryTitle);

                if (that.targetData.isThirdparty) {
                    that.viewSettings.canWebRestrictedEditing = false;
                }

            } else {
                that.viewSettings.canLinkReadWrite = true;
                that.viewSettings.displaySettings = false;
            }
        }
    }

    function getSharedInfo(ids, titles, asFlat, rootFolderTypeCommon, origin, denyDownload, denySharing, providerEntry, defaultAccessRights) {

        initTargetData(ids, titles, asFlat, providerEntry);

        initViewSettings(ids, titles, asFlat, rootFolderTypeCommon, origin, denyDownload, denySharing);

        var data = {
            entry: (that.targetData.multiple ? ids : [ids])
        };

        ASC.Files.ServiceManager.getSharedInfo(ASC.Files.ServiceManager.events.GetSharedInfo, { showLoading: true, defaultAccessRights }, { stringList: data });
    };

    function getSelectedFromSharingInfo(sharingInfo) {
        that.targetData.sharingInfo = sharingInfo;
        that.targetData.needUpdate = false;
        that.viewSettings.externalLinksAvailable = that.$linkSettingsInput.data("available");

        var selected = {};

        sharingInfo.forEach(function (item) {
            if (item.id === ASC.Files.Constants.ShareLinkId || item.linkSettings) {
                that.targetData.encrypted = false;

                var canEdit = item.id === ASC.Files.Constants.ShareLinkId ? 
                    item.ace_status != ASC.Files.Constants.AceStatusEnum.Restrict : !item.inherited;

                selected[item.id] = {
                    id: item.id,
                    link: item.link,
                    linkSettings: item.linkSettings ? Object.assign({}, item.linkSettings) : item.linkSettings,
                    name: item.title || FilesJSResource.QuickExternalLink,
                    isLink: true,
                    canEdit: canEdit,
                    canRemove: canEdit,
                    access: item.ace_status,
                    accessName: getAceString(item.ace_status),
                    entryType: item.entryType,
                    inherited: item.inherited
                };
            } else if (item.id !== ASC.Files.Constants.GUEST_USER_ID) {
                if (item.is_group) {
                    selected[item.id] = {
                        id: item.id,
                        name: item.title,
                        isGroup: true,
                        canEdit: !item.locked,
                        canRemove: !(item.locked || item.disable_remove),
                        canOpen: that.groupCache[item.id] !== undefined,
                        access: item.ace_status,
                        accessName: getAceString(item.ace_status)
                    };
                } else {
                    if (!that.userCache[item.id]) {
                        that.userCache[item.id] = window.UserManager.getUser(item.id); //terminated
                    }

                    selected[item.id] = {
                        id: item.id,
                        name: item.title,
                        avatar: that.userCache[item.id].avatarSmall,
                        isOwner: item.owner,
                        canEdit: !(item.locked || item.owner),
                        canRemove: !(item.locked || item.owner || item.disable_remove),
                        access: item.ace_status,
                        accessName: getAceString(item.owner ? "owner" : item.ace_status)
                    };

                    if (item.owner) {
                        that.targetData.ownerId = item.id;
                        if (ASC.Files.Folders && ASC.Files.Folders.folderContainer == "corporate"
                            && !(ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty())
                            && !that.targetData.isThirdparty) {
                            selected[item.id].canChange = true;
                        }
                    }
                }
            }
        });

        return selected;
    }

    function getAccessRights() {
        var accessRights = [{
            "id": ASC.Files.Constants.AceStatusEnum.ReadWrite,
            "name": getAceString(ASC.Files.Constants.AceStatusEnum.ReadWrite),
            "hideForVisitor": (!that.targetData.entryData || that.targetData.entryData.entryType == "file")
        }];

        if (!that.targetData.encrypted) {
            if (that.viewSettings.canWebCustomFilterEditing) {
                accessRights.push({
                    "id": ASC.Files.Constants.AceStatusEnum.CustomFilter,
                    "name": getAceString(ASC.Files.Constants.AceStatusEnum.CustomFilter),
                    "hideForVisitor": true
                });
            }

            if (that.viewSettings.canWebReview) {
                accessRights.push({
                    "id": ASC.Files.Constants.AceStatusEnum.Review,
                    "name": getAceString(ASC.Files.Constants.AceStatusEnum.Review),
                    "hideForVisitor": true
                });
            }

            if (that.viewSettings.canWebRestrictedEditing) {
                accessRights.push({
                    "id": ASC.Files.Constants.AceStatusEnum.FillForms,
                    "name": getAceString(ASC.Files.Constants.AceStatusEnum.FillForms),
                    "hideForVisitor": true
                });
            }

            if (that.viewSettings.canWebComment) {
                accessRights.push({
                    "id": ASC.Files.Constants.AceStatusEnum.Comment,
                    "name": getAceString(ASC.Files.Constants.AceStatusEnum.Comment),
                    "hideForVisitor": true
                });
            }

            accessRights.push({
                "id": ASC.Files.Constants.AceStatusEnum.Read,
                "name": getAceString(ASC.Files.Constants.AceStatusEnum.Read),
                "hideForVisitor": false
            });
        }

        accessRights.push({
            "id": ASC.Files.Constants.AceStatusEnum.Restrict,
            "name": getAceString(ASC.Files.Constants.AceStatusEnum.Restrict),
            "hideForVisitor": false
        });

        accessRights.push({
            "id": ASC.Files.Constants.AceStatusEnum.Varies,
            "name": getAceString(ASC.Files.Constants.AceStatusEnum.Varies),
            "hideForVisitor": false,
            "disabled": true
        });

        return accessRights;
    }

    function getDefaultAccessRights(accessRights, defaultAccessRights) {
        var $settingsObj = jq("#defaultAccessRightsSetting");
        if ($settingsObj.length) {
            var common = +$settingsObj.find("input[type=radio]:checked").val();
            var specific = [];
            $settingsObj.find("input[type=checkbox]:checked").each(function () {
                specific.push(+this.value);
            });
        } else if (defaultAccessRights) {
            var common = defaultAccessRights.pop();
            var specific = defaultAccessRights;
        }

        var available = [];
        for (var item of accessRights) {
            if (item.disabled) {
                continue;
            }
            if (specific.indexOf(item.id) != -1) {
                return item.id;
            }
            available.push(item.id);
        }

        if (available.indexOf(common) != -1) {
            return common;
        }

        return that.targetData.encrypted ? ASC.Files.Constants.AceStatusEnum.ReadWrite : ASC.Files.Constants.AceStatusEnum.Read;
    }

    function onGetSharedInfo(sharingInfo, params, errorMessage) {
        if (typeof errorMessage != "undefined" || typeof sharingInfo == "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var selected = getSelectedFromSharingInfo(sharingInfo);

        var accessRights = getAccessRights();

        var defaultAccessRight = getDefaultAccessRights(accessRights, params.defaultAccessRights);

        that.changeData(selected, accessRights, defaultAccessRight);

        that.show();
    };

    function getAceString(aceStatus) {
        if (aceStatus == "owner") {
            return ASC.Files.FilesJSResource.AceStatusEnum_Owner;
        }
        aceStatus = parseInt(aceStatus);
        switch (aceStatus) {
            case ASC.Files.Constants.AceStatusEnum.Read:
                return ASC.Files.FilesJSResource.AceStatusEnum_Read;
            case ASC.Files.Constants.AceStatusEnum.ReadWrite:
                return ASC.Files.FilesJSResource.AceStatusEnum_ReadWrite;
            case ASC.Files.Constants.AceStatusEnum.CustomFilter:
                return ASC.Files.FilesJSResource.AceStatusEnum_CustomFilter;
            case ASC.Files.Constants.AceStatusEnum.Restrict:
                return ASC.Files.FilesJSResource.AceStatusEnum_Restrict;
            case ASC.Files.Constants.AceStatusEnum.Varies:
                return ASC.Files.FilesJSResource.AceStatusEnum_Varies;
            case ASC.Files.Constants.AceStatusEnum.Review:
                return ASC.Files.FilesJSResource.AceStatusEnum_Review;
            case ASC.Files.Constants.AceStatusEnum.FillForms:
                return ASC.Files.FilesJSResource.AceStatusEnum_FillForms;
            case ASC.Files.Constants.AceStatusEnum.Comment:
                return ASC.Files.FilesJSResource.AceStatusEnum_Comment;
            default:
                return "";
        }
    };

    function updateForParent() {
        if (that.targetData.needUpdate) {
            getSharedInfoShort();
        } else {
            onGetSharedInfoShort(null);
        }
    }

    function getSharedInfoShort() {
        ASC.Files.ServiceManager.getSharedInfoShort(ASC.Files.ServiceManager.events.GetSharedInfoShort,
            {
                showLoading: true,
                objectID: that.targetData.id
            });
    }

    function onGetSharedInfoShort(jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined" || typeof jsonData == "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        if (jsonData) {
            var data =
            {
                needUpdate: true,
                sharingSettings: jsonData
            };
        } else {
            data = { needUpdate: false };
        }
        data.Referer = "onlyoffice";

        var message = JSON.stringify(data);
        window.parent.postMessage(message, that.viewSettings.originForPost);
    }

    function setAccess(notify, message) {
        var data = new Array();
        var newOwner = null;
        var isDefault = true;

        for (var itemId in that.selected) {
            var item = that.selected[itemId];
            var change = true;
            for (var oldItem of that.targetData.sharingInfo) {
                if (oldItem.id === item.id) {
                    change = oldItem.ace_status != item.access
                        ? true
                        : oldItem.linkSettings
                            ? oldItem.title != item.name || JSON.stringify(oldItem.linkSettings) != JSON.stringify(item.linkSettings)
                            : false;
                    break;
                }
            }
            if (change) {
                data.push(getAceEntryItem(item, item.access));
            }
            if (item.isOwner && item.id != that.targetData.ownerId) {
                newOwner = item.id;
            }
            if (isDefault) {
                isDefault = item.isOwner || (item.isLink && item.access == ASC.Files.Constants.AceStatusEnum.Restrict);
            }
        }

        //remove
        for (var oldItem of that.targetData.sharingInfo) {
            if (!that.selected[oldItem.id]) {
                data.push(getAceEntryItem(oldItem, ASC.Files.Constants.AceStatusEnum.None));
            }
        }

        if (data.length) {
            that.targetData.needUpdate = true;
        }

        if (isDefault) {
            that.viewSettings.denyDownload = false;
            that.viewSettings.denySharing = false;
        }

        var advancedSettings = that.viewSettings.displaySettings ? { denyDownload: !!that.viewSettings.denyDownload, denySharing: !!that.viewSettings.denySharing } : undefined;

        var dataJson = {
            entries: { entry: that.targetData.id },
            aces: { entry: data },
            message: message,
            advancedSettings: advancedSettings
        };

        ASC.Files.ServiceManager.setAceObject(ASC.Files.ServiceManager.events.SetAceObject,
            {
                owner: newOwner,
                showLoading: true,
                notify: notify,
                advancedSettings: advancedSettings
            },
            { ace_collection: dataJson });
    };

    function getAceEntryItem(item, access) {
        function removeFieldsWithDefaultValue(obj) {
            for (var propName in obj) {
                if (!obj[propName]) {
                    delete obj[propName];
                }
            }
            return obj;
        }

        if (item.isLink && item.linkSettings) {
            return {
                id: item.id,
                title: item.name,
                ace_status: access,
                linkSettings: removeFieldsWithDefaultValue(item.linkSettings)
            }
        }

        return {
            id: item.id,
            is_group: item.isGroup,
            ace_status: access
        }
    }

    function onSetAccess(jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var entryArray = that.targetData.multiple ? that.targetData.id : [that.targetData.id];
        var itemId, entryObj;

        for (var entry of entryArray) {
            itemId = ASC.Files.UI.parseItemId(entry);
            entryObj = ASC.Files.UI.getEntryObject(itemId.entryType, itemId.entryId);

            entryObj.toggleClass("__active", jsonData.indexOf(entry) > -1);

            if (params.advancedSettings) {
                ASC.Files.UI.setObjectData(entryObj, "data-deny_download", params.advancedSettings.denyDownload);
                ASC.Files.UI.setObjectData(entryObj, "data-deny_sharing", params.advancedSettings.denySharing);
            }
        }

        if (params.owner) {
            ASC.Files.Folders.changeOwner(entryArray, params.owner);
        }

        if (that.targetData.encrypted && ASC.Desktop && ASC.Desktop.setAccess) {
            if (LoadingBanner) {
                LoadingBanner.displayLoading();
            }

            ASC.Files.UI.blockObject(that.targetData.id, true, ASC.Files.FilesJSResource.DescriptCreate);

            ASC.Desktop.setAccess(itemId.entryId, function (encryptedFile) {
                if (encryptedFile) {
                    ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResource.DesktopMessageStoring);

                    if (ASC.Files.Folders) {
                        ASC.Files.Folders.replaceFileStream(itemId.entryId, "", encryptedFile, true, null, true);
                    } else {
                        Teamlab.updateFileStream(
                            {
                                fileId: itemId.entryId,
                                encrypted: true,
                                forcesave: true,
                            },
                            encryptedFile
                        );
                    }
                }

                if (LoadingBanner) {
                    LoadingBanner.hideLoading();
                }

                that.hide();
            });
        }
    };

    function showTooltip(userId, groupId) {
        that.tooltipTimeout = setTimeout(function () {
            var element = groupId && that.openedGroupElement
                ? that.openedGroupElement.nextElementSibling.children[userId]
                : that.contentListElement.children[userId];

            if (!element) {
                return;
            }

            var user = that.userCache[userId];
            var group = that.groupCache[groupId || user.groups[0]];
            var data = {
                avatar: user.avatarBig,
                userName: new URL(window.location.protocol + '//' + window.location.hostname + user.profileUrl).searchParams.get("user"),
                displayName: user.displayName,
                groups: group ? [group] : null,
                title: user.title
            }

            var content = jq.tmpl("userProfileCardTmpl", data);
            that.$itemTooltipDialog.html(content);

            displayContentListDialog(element, that.itemTooltipDialogHelper, that.$itemTooltipDialog, 8);
        }, 1000);
    }

    function hideTooltip(event) {
        if (that.$itemTooltipDialog.is(event.relatedTarget) || that.$itemTooltipDialog.has(event.relatedTarget).length) {
            return;
        }

        clearTimeout(that.tooltipTimeout);
        that.itemTooltipDialogHelper.hide();
    }

    function changeExternalShareSettings() {
        that.$linkSettingsInput
            .data("available", jq("#cbxExternalShare").prop("checked"))
            .data("social", jq("#cbxExternalShareSocialMedia").prop("checked"));
    };

    function setBindings() {

        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetShortenLink, onGetShortenLink);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetSharedInfoShort, onGetSharedInfoShort);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetSharedInfo, onGetSharedInfo);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SetAceObject, onSetAccess);

        that.$topContentContainer.append(that.userSelector.dialogElement);

        that.userSelectorDialogHelper = new DialogHelper(that.$addUserBtn.get(0), that.userSelector.dialogElement, onBeforeShowUserSelectorDialog, null, false, true);

        that.userSelector.onCancel = that.userSelectorDialogHelper.hide;

        that.$addLinkBtn.on("click", clickOnAddLinkBtn);

        that.addLinkActionDialogHelper = new DialogHelper(that.$addLinkBtnArrow.get(0), that.$addLinkActionDialog.get(0), onBeforeShowAddLinkActionDialog);

        that.$addLinkActionDialog.on("click", ".dropdown-item", clickOnAddLinkActionDialogItemElement);

        that.searchDialogHelper = new DialogHelper(null, that.$searchDialog.get(0), onBeforeShowSearchDialog, onBeforeHideSearchDialog, true, true, that.$searchInput.get(0));

        that.$searchInput.on("mousedown", function () {
            that.searchDialogHelper.addSelection();
        })

        that.$searchInput.on("mouseup blur", function () {
            that.searchDialogHelper.removeSelection();
        })

        that.$searchDialogContentListParent.on("click", ".us-name, .us-show-more", function (event) {
            if (this.classList.contains("us-show-more")) {
                clickOnSearchShowMore(this, event);
            } else {
                clickOnSearchItem(this, event);
            }
        })

        that.$contentListParent.on("click", ".us-name, .us-show-more", function (event) {
            if (this.classList.contains("disabled")) {
                return;
            }

            if (this.classList.contains("us-show-more")) {
                clickOnShowMore(this, event);
            } else if (this.classList.contains("changeable") && event.target == this.querySelector(".us-name-text span")) {
                clickOnChangeOwner(this, event);
            } else if (event.target.classList.contains("us-access")) {
                clickOnAccessElement(this, event);
            } else if (event.target.classList.contains("us-avatar")) {
                clickOnAvatarElement(this, event);
            } else if (this.classList.contains("us-groupname")) {
                clickOnGroupElement(this, event);
            } else if (this.classList.contains("us-linkname")) {
                clickOnLinkElement(this, event);
            } else if (this.classList.contains("us-folder-linkname")) {
                clickOnLinkElement(this, event);
            } else {
                clickOnGroupItemElement(this, event);
            }
        })

        that.itemTooltipDialogHelper = new DialogHelper(null, that.$itemTooltipDialog.get(0));

        that.$contentListParent.on("mouseover mouseout", ".us-name:not(.us-linkname):not(.us-folder-linkname):not(.us-groupname):not(.disabled) .us-name-text span", function (event) {
            var element = this.closest(".us-name");

            if (event.type == "mouseover") {
                showTooltip(element.id, element.getAttribute("data-group"));
            }

            if (event.type == "mouseout") {
                hideTooltip(event);
            }
        });

        that.$itemTooltipDialog.on("mouseout", hideTooltip);

        that.menuSelectDialogHelper = new DialogHelper(that.$menuActionSelectAll.get(0), that.$menuSelectDialog.get(0));

        that.$menuSelectDialog.on("click", ".dropdown-item", function (event) {
            clickOnSelectDialogItemElement(this, event);
        })

        that.$menuActionSelectAllCbx.on("click", function (event) {
            event.stopPropagation();
            that.menuSelectDialogHelper.hide();
            clickOnMenuActionSelectAllElement(this, event);
        })

        that.menuAccessRightsDialogHelper = new DialogHelper(that.$menuActionChangeBtn.get(0), that.$menuAccessRightsDialog.get(0), onBeforeShowMenuAccessRightsDialog);

        that.$menuAccessRightsDialog.on("click", ".access-rights-item", function (event) {
            clickOnMenuAccessRightsElement(this, event);
            showSaveBtn();
        })

        that.$menuActionRemoveBtn.on("click", function (event) {
            if (!this.classList.contains("unlockAction")) {
                return;
            }

            clickOnMenuActionRemoveElement(this, event);
            showSaveBtn();
        })

        that.$menuActionCloseBtn.on("click", function (event) {
            clickOnMenuActionCloseElement(this, event);
        })

        that.linkActionDialogHelper = new DialogHelper(null, that.$linkActionDialog.get(0), onBeforeShowLinkActionDialog);

        that.$linkActionDialog.on("click", ".dropdown-item", function (event) {
            clickOnLinkActionDialogItemElement(this, event);
        })

        that.linkShareDialogHelper = new DialogHelper(null, that.$linkShareDialog.get(0), onBeforeShowLinkShareDialog);

        that.$linkShareDialogInput.on("mousedown", mousedownOnReadonlyInput);

        that.$linkShareDialogInput.on("keypress", keypressOnReadonlyInput);

        that.$linkShareDialogSocialContainer.on("click", "a", function (e) {
            return clickOnLinkShareDialogSocialItemElement(this, e);
        });

        that.$linkShareDialogShort.on("click", getShortenLink);

        that.linkEmbedDialogHelper = new DialogHelper(null, that.$linkEmbedDialog.get(0), onBeforeShowLinkEmbedDialog);

        that.$linkEmbedDialogSizeItem.on("click", setEmbeddedSize);

        that.$linkEmbedDialogSizeCustom.on("change", setEmbeddedSize);

        that.$linkEmbedDialogInput.on("mousedown", mousedownOnReadonlyInput);

        that.$linkEmbedDialogInput.on("keypress", keypressOnReadonlyInput);


        that.linkPasswordDialogHelper = new DialogHelper(null, that.$linkPasswordDialog.get(0), onBeforeShowLinkPasswordDialog);

        that.$linkPasswordDialogCbx.on("change", function () {
            disableDialogElements(!this.checked, that.$linkPasswordDialog);
            if (this.checked) {
                setTimeout(function () {
                    that.$linkPasswordDialogInput.get(0).focus();
                }, 0);
            }
        })

        that.$linkPasswordDialogShow.on("click", showHidePassword);

        that.$linkPasswordDialogRandom.on("click", setRandomPassword);

        that.$linkPasswordDialogClean.on("click", function () {
            clearDialogElements(this, that.$linkPasswordDialog);
        })

        that.$linkPasswordDialogSaveBtn.on("click", function () {
            that.linkPasswordDialogHelper.hide();
            onSaveLinkPasswordSettings();
        })

        that.$linkPasswordDialogCancelBtn.on("click", function () {
            that.linkPasswordDialogHelper.hide();
        })


        that.$linkLifetimeDialogCbx.on("change", function () {
            disableDialogElements(!this.checked, that.$linkLifetimeDialog);
        })

        that.$linkLifetimeDialogDateInput.datepicker({ selectDefaultDate: false, minDate: new Date() });
        that.$linkLifetimeDialogDateInput.mask(ASC.Resources.Master.DatePatternJQ);

        var datepicker = jq("#ui-datepicker-div").addClass("blockMsg");

        that.linkLifetimeDialogHelper = new DialogHelper(null, that.$linkLifetimeDialog.get(0), onBeforeShowLinkLifetimeDialog, null, false, true, datepicker.get(0));

        var maskBehavior = function (val) {
            val = val.split(":");
            return (parseInt(val[0]) > 19) ? "Hh:M0" : "H0:M0";
        }

        spOptions = {
            onKeyPress: function (val, e, field, options) {
                field.mask(maskBehavior.apply({}, arguments), options);
            },
            translation: {
                'H': { pattern: /[0-2]/, optional: false },
                'h': { pattern: /[0-3]/, optional: false },
                'M': { pattern: /[0-5]/, optional: false }
            }
        };

        that.$linkLifetimeDialogTimeInput.mask(maskBehavior, spOptions);

        that.$linkLifetimeDialogClean.on("click", function () {
            clearDialogElements(this, that.$linkLifetimeDialog);
        })

        that.$linkLifetimeDialogSaveBtn.on("click", function () {
            onSaveLinkLifetimeSettings();
        })

        that.$linkLifetimeDialogCancelBtn.on("click", function () {
            that.linkLifetimeDialogHelper.hide();
        })

        that.linkDeleteDialogHelper = new DialogHelper(null, that.$linkDeleteDialog.get(0));

        that.$linkDeleteDialogOkBtn.on("click", function () {
            that.linkDeleteDialogHelper.hide();
            deleteLink(that.currentElement.id);
        })

        that.$linkDeleteDialogCancelBtn.on("click", function () {
            that.linkDeleteDialogHelper.hide();
        })

        that.itemAccessRightsDialogHelper = new DialogHelper(null, that.$itemAccessRightsDialog.get(0));

        that.$itemAccessRightsDialog.on("click", ".access-rights-item", function (event) {
            clickOnItemAccessRightsElement(this, event);
        })

        that.$saveBtnMain.on("click", function () {
            that.save(true, "");
        })

        that.saveActionDialogHelper = new DialogHelper(that.$saveBtnArrow.get(0), that.$saveActionDialog.get(0));

        that.$saveActionDialog.on("click", ".dropdown-item", clickOnSaveActionDialogItemElement)

        that.$cancelBtn.on("click", function (event) {
            that.hide();
        })

        that.messageDialogHelper = new DialogHelper(null, that.$messageDialog.get(0));

        that.$messageDialogSaveBtn.on("click", function (event) {
            that.messageDialogHelper.hide();
            that.save(true, that.$messageDialogTextarea.val().trim());
        })

        that.$messageDialogCancelBtn.on("click", function (event) {
            that.messageDialogHelper.hide();
        })

        that.advancedSettingsDialogHelper = new DialogHelper(that.$settingsBtnContainer.get(0), that.$advancedSettingsDialog.get(0), onBeforeShowAdvancedSettingsDialog);

        that.$advancedSettingsDialogSaveBtn.on("click", function (event) {
            that.viewSettings.denyDownload = that.$advancedSettingsDialogDenyDownloadCbx.prop("checked");
            that.viewSettings.denySharing = that.$advancedSettingsDialogDenySharingCbx.prop("checked");

            that.advancedSettingsDialogHelper.hide();
            showSaveBtn();
        })

        that.$advancedSettingsDialogCancelBtn.on("click", function (event) {
            that.advancedSettingsDialogHelper.hide();
        })

        initClipboard();
    }

    return {
        init: init,
        updateForParent: updateForParent,
        getSharedInfo: getSharedInfo,
        changeExternalShareSettings: changeExternalShareSettings,
        unSubscribeMe: function (entryType, entryId) {
            ASC.Files.Unsubscribe.unSubscribeMe(entryType, entryId)
        }
    }

})();


jq(document).ready(function () {
    (function ($) {

        ASC.Files.Share.init();

        jq("#studioPageContent").on("click", "#buttonShare, #mainShare.unlockAction", function () {
            ASC.Files.Actions.hideAllActionPanels();

            var dataIds = new Array();
            var dataTitles = new Array();
            jq("#filesMainContent .file-row:not(.without-share):not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):not(.files-encrypted):has(.checkbox input:checked)").each(function () {
                var entryRowData = ASC.Files.UI.getObjectData(this);
                var entryRowType = entryRowData.entryType;
                var entryRowId = entryRowData.entryId;

                dataIds.push(entryRowType + "_" + entryRowId);
                dataTitles.push(entryRowData.title);
            });

            ASC.Files.Share.getSharedInfo(dataIds, dataTitles);
        });

        jq("#filesMainContent").on("click", ".btn-row", function () {
            var entryData = ASC.Files.UI.getObjectData(this);
            var entryId = entryData.entryId;
            var entryType = entryData.entryType;
            var entryTitle = entryData.title;
            ASC.Files.Actions.hideAllActionPanels();

            ASC.Files.UI.checkSelectAll(false);
            ASC.Files.UI.selectRow(entryData.entryObject, true);
            ASC.Files.UI.updateMainContentHeader();

            ASC.Files.Share.getSharedInfo(entryType + "_" + entryId, entryTitle);
            return false;
        });

    })(jQuery);
});