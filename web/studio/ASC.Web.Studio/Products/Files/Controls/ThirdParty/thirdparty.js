/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
window.ASC.Files.ThirdParty = (function () {
    var isInit = false;
    var thirdPartyList = {
        BoxNet: { key: "BoxNet", customerTitle: ASC.Files.FilesJSResources.FolderTitleBoxNet, providerTitle: ASC.Files.FilesJSResources.TypeTitleBoxNet },
        DropBox: { key: "DropBox", customerTitle: ASC.Files.FilesJSResources.FolderTitleDropBox, providerTitle: ASC.Files.FilesJSResources.TypeTitleDropBox, getTokenUrl: ASC.Files.Constants.URL_OAUTH_DROPBOX },
        Google: { key: "Google", customerTitle: ASC.Files.FilesJSResources.FolderTitleGoogle, providerTitle: ASC.Files.FilesJSResources.TypeTitleGoogle, getTokenUrl: ASC.Files.Constants.URL_OAUTH_GOOGLE },
        GoogleDrive: { key: "GoogleDrive", customerTitle: ASC.Files.FilesJSResources.FolderTitleGoogle, providerTitle: ASC.Files.FilesJSResources.TypeTitleGoogle, getTokenUrl: ASC.Files.Constants.URL_OAUTH2_GOOGLE },
        SharePoint: { key: "SharePoint", customerTitle: ASC.Files.FilesJSResources.FolderTitleSharePoint, providerTitle: ASC.Files.FilesJSResources.TypeTitleSharePoint, urlRequest: true },
        SkyDrive: { key: "SkyDrive", customerTitle: ASC.Files.FilesJSResources.FolderTitleSkyDrive, providerTitle: ASC.Files.FilesJSResources.TypeTitleSkyDrive, getTokenUrl: ASC.Files.Constants.URL_OAUTH_SKYDRIVE }
    };

    var init = function () {
        if (isInit === false) {
            isInit = true;

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetThirdParty, onGetThirdParty);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SaveThirdParty, onSaveThirdParty);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.DeleteThirdParty, onDeleteThirdParty);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.ChangeAccessToThirdparty, onChangeAccessToThirdparty);

            jq(document).click(function (event) {
                jq.dropdownToggle().registerAutoHide(event, ".account-row .menu-small", "#thirdPartyActionPanel");
            });
        }
    };

    var isThirdParty = function (entryObject) {
        if (entryObject == null) {
            return ASC.Files.ThirdParty.getThirdPartyItem() != null;
        }

        entryObject = jq(entryObject);
        if (!entryObject.is(".file-row")) {
            entryObject = entryObject.closest(".file-row");
        }
        return entryObject.hasClass("third-party-entry");
    };

    var getThirdPartyItem = function (entryData) {
        var thirdPartyKey =
            entryData == null
                ? ASC.Files.Folders.currentFolder.provider_key
                : entryData.provider_key;

        return thirdPartyList[thirdPartyKey];
    };

    var showSettingThirdParty = function () {
        getThirdParty();
        ASC.Files.UI.hideAllContent();
        jq("#treeSetting").addClass("currentCategory open");
        jq(".settings-link-thirdparty").addClass("active");
        jq("#settingThirdPartyPanel").show();

        ASC.Files.UI.setDocumentTitle(ASC.Files.FilesJSResources.TitleSettingsThirdParty);
    };

    var showChangeDialog = function (folderData) {
        var thirdPartyItem = ASC.Files.ThirdParty.getThirdPartyItem(folderData);
        var thirdParty =
            {
                getTokenUrl: thirdPartyItem.getTokenUrl,
                key: thirdPartyItem.key,
                providerTitle: thirdPartyItem.providerTitle,

                id: folderData.provider_id,
                customerTitle: folderData.title,
                folderId: folderData.id
            };

        jq("#thirdPartyEditor div[id$=\"InfoPanel\"]").hide();

        jq("#thirdPartyTitle").val(thirdParty.customerTitle);
        ASC.Files.UI.checkCharacter(jq("#thirdPartyTitle"));

        jq("#thirdPartyPanel input").removeAttr("disabled");

        jq("#thirdPartyCorporate").removeAttr("checked");

        LoadingBanner.hideLoaderBtn("#thirdPartyEditor");

        jq("#submitThirdParty").unbind("click").click(function () {
            submitEditor(thirdParty);
            return false;
        });

        jq("#thirdPartyCorporate").prop("checked", ASC.Files.Folders.folderContainer == "corporate");

        jq("#thirdPartyDialogCaption").text(ASC.Files.FilesJSResources.ThirdPartyEditorCaption.format(thirdParty.providerTitle));

        ASC.Files.UI.blockUI(jq("#thirdPartyEditor"), 400, 300);
        PopupKeyUpActionProvider.EnterAction = "jq(\"#submitThirdParty\").click();";

        for (var classItem in ASC.Files.ThirdParty.thirdPartyList) {
            jq("#thirdPartyTitle").removeClass(classItem);
        }
        jq("#thirdPartyTitle").addClass(thirdParty.key).focus();
    };

    var submitEditor = function (thirdParty) {
        var folderTitle = jq("#thirdPartyTitle").val().trim();
        folderTitle = ASC.Files.Common.replaceSpecCharacter(folderTitle);
        var corporate = (jq("#thirdPartyCorporate").prop("checked") === true);

        var infoBlock = jq("#thirdPartyEditor div[id$=\"InfoPanel\"]");
        infoBlock.hide();

        if (folderTitle == "") {
            infoBlock.show().find("div").text(ASC.Files.FilesJSResources.ErrorMassage_FieldsIsEmpty);
            return;
        }

        jq("#thirdPartyPanel input").attr("disabled", "disabled");

        LoadingBanner.showLoaderBtn("#thirdPartyEditor");

        if (thirdParty.folderId) {
            ASC.Files.UI.blockObjectById("folder", thirdParty.folderId, true, ASC.Files.FilesJSResources.DescriptChangeInfo);
        }

        saveProvider(thirdParty.id, thirdParty.key, folderTitle, "", "", "", "", corporate, thirdParty.folderId);
    };

    var showDeleteDialog = function (providerId, providerKey, providerTitle, customerTitle, folderData) {
        providerId = providerId || (folderData ? folderData.provider_id : null);
        if (providerId == null) {
            return;
        }

        var folderId = folderData ? folderData.entryId : null;

        providerTitle = providerTitle || ASC.Files.ThirdParty.getThirdPartyItem(folderData).providerTitle;
        jq("#thirdPartyDeleteDescr").html(ASC.Files.FilesJSResources.ConfirmDeleteThirdParty.format(customerTitle, providerTitle));

        jq("#deleteThirdParty").unbind("click").click(function () {
            ASC.Files.UI.blockObjectById("folder", folderId, true, ASC.Files.FilesJSResources.DescriptRemove);
            PopupKeyUpActionProvider.CloseDialog();
            deleteProvider(providerId, providerKey, customerTitle, folderId);
            return false;
        });

        ASC.Files.UI.blockUI(jq("#thirdPartyDelete"), 400, 300);
        PopupKeyUpActionProvider.EnterAction = "jq(\"#deleteThirdParty\").click();";
    };

    var addNewThirdPartyAccount = function (thirdParty) {
        if (!thirdParty) {
            return false;
        }
        var accountPanel = jq("#account_" + thirdParty.key + "_0");
        if (accountPanel.length > 0) {
            jq(accountPanel).find("input:visible:first").focus();
            jq(accountPanel).yellowFade();
            return false;
        }

        if (!thirdParty.getTokenUrl) {
            addNewThirdParty(thirdParty);
        } else {
            OAuthCallback = function (token) {
                addNewThirdParty(thirdParty, token);
            };
            OAuthPopup(thirdParty.getTokenUrl);
        }
        return false;
    };

    var addNewThirdParty = function (thirdParty, token) {
        var data = {
            corporate: false,
            customer_title: thirdParty.customerTitle,
            provider_id: 0,
            provider_title: thirdParty.providerTitle,
            provider_key: thirdParty.key,
            isNew: true,
            max_name_length: ASC.Files.Constants.MAX_NAME_LENGTH,
            canCorporate: (ASC.Files.Constants.ADMIN && !ASC.Resources.Master.Personal)
        };

        var xmlData = ASC.Files.Common.jsonToXml({ third_partyList: { entry: data } });
        var htmlXml = ASC.Files.TemplateManager.translateFromString(xmlData);
        jq("#thirdPartyAccountList").append(htmlXml);

        jq("#emptyThirdPartyContainer").hide();
        jq("#thirdPartyAccountContainer").show();

        var accountPanel = jq("#account_" + thirdParty.key + "_0");
        ASC.Files.UI.checkCharacter(jq(accountPanel).find(".account-input-folder"));

        jq(accountPanel).find(".account-hidden-token").val(token);
        jq(accountPanel).find(".account-settings-container").show();

        if (!thirdParty.getTokenUrl) {
            jq(accountPanel).find(".account-log-pass-container").show();
            if (thirdParty.urlRequest) {
                jq(accountPanel).find(".account-field-url").show();
            }
        }

        jq(accountPanel).find("input:visible:first").focus();
        jq(accountPanel).yellowFade();
    };

    var editThirdPartyAccount = function (obj) {
        var account = jq(obj).parents(".account-row");
        ASC.Files.UI.checkCharacter(jq(account).find(".account-input-folder"));
        var customerTitle = jq(account).find(".account-hidden-customer-title").val().trim();
        jq(account).find(".account-input-folder").val(customerTitle);
        jq(account).find(".account-settings-container").show();
    };

    var cancelThirdPartyAccount = function (obj) {
        var account = jq(obj).parents(".account-row");
        var providerId = parseInt(jq(account).find(".account-hidden-provider-id").val());
        if (providerId == 0) {
            jq(account).remove();
            if (jq("#thirdPartyAccountList .account-row").length == 0) {
                jq("#thirdPartyAccountContainer").hide();
                jq("#emptyThirdPartyContainer").show();
            }
            return false;
        }
        jq(account).find(".account-settings-container").hide();
    };

    var deleteThirdPartyAccount = function (obj) {
        var account = jq(obj).parents(".account-row");
        var providerId = parseInt(jq(account).find(".account-hidden-provider-id").val());
        var providerKey = jq(account).find(".account-hidden-provider-key").val().trim();
        var providerTitle = jq(account).find(".account-hidden-provider-title").val().trim();
        var customerTitle = jq(account).find(".account-hidden-customer-title").val().trim();
        ASC.Files.ThirdParty.showDeleteDialog(providerId, providerKey, providerTitle, customerTitle);
    };

    var saveThirdPartyAccount = function (obj) {
        var account = jq(obj).parents(".account-row");
        var providerId;
        var providerIdTmp = parseInt(jq(account).find(".account-hidden-provider-id").val());
        if (providerIdTmp != 0) {
            providerId = providerIdTmp;
        }
        var providerKey = jq(account).find(".account-hidden-provider-key").val().trim();
        var customerTitle = jq(account).find(".account-input-folder").val().trim();
        var connectUrl = jq(account).find(".account-input-url").val().trim();
        var login = jq(account).find(".account-input-login").val().trim();
        var password = jq(account).find(".account-input-pass").val().trim();
        var token = jq(account).find(".account-hidden-token").val().trim();
        var corporate = (jq(account).find(".account-cbx-corporate").prop("checked") === true);

        var thirdParty = thirdPartyList[providerKey];

        if (customerTitle == ""
            || !providerId && (!thirdParty.getTokenUrl && (login == "" || password == ""))
            || !providerId && (thirdParty.urlRequest && connectUrl == "")) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.ErrorMassage_FieldsIsEmpty, true);
            return;
        }
        if (!providerId && (thirdParty.getTokenUrl && token == "")) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.ErrorMassage_MustLogin, true);
            return;
        }

        jq(account).find("input").attr("disabled", true);
        jq(account).find("a.button.account-save-link").addClass("disable");

        saveProvider(providerId, providerKey, customerTitle, connectUrl, login, password, token, corporate);
    };

    var changeAccessToThirdpartySettings = function (obj) {
        var enable = jq(obj).prop("checked");
        changeAccessToThirdparty(enable === true);
    };

    var showThirdPartyNewAccount = function () {
        var width = jq("#thirdPartyNewAccount .add-account-button").length * 100 + 100;
        ASC.Files.UI.blockUI(jq("#thirdPartyNewAccount"), Math.max(400, width), 300);
    };

    var showThirdPartyActionsPanel = function (event) {
        var e = ASC.Files.Common.fixEvent(event);
        var target = jq(e.srcElement || e.target);

        jq("#accountEditLinkContainer").unbind("click").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            editThirdPartyAccount(target);
        });

        jq("#accountDeleteLinkContainer").unbind("click").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            deleteThirdPartyAccount(target);
        });

        var dropdownItem = jq("#thirdPartyActionPanel");

        dropdownItem.css(
            {
                "top": target.offset().top + target.outerHeight(),
                "left": "auto",
                "right": jq(window).width() - target.offset().left - target.width() - 2,
                "margin": "5px -8px 0 0"
            });

        dropdownItem.toggle();
        return true;
    };

    //request

    var getThirdParty = function () {
        ASC.Files.ServiceManager.getThirdParty(ASC.Files.ServiceManager.events.GetThirdParty, { showLoading: true });
    };

    var saveProvider = function (providerId, providerKey, folderTitle, connectUrl, login, password, token, corporate, folderId) {
        var params =
            {
                providerId: providerId,
                providerKey: providerKey,
                folderTitle: folderTitle,
                login: login,
                password: password,
                token: token,
                corporate: corporate,
                folderId: folderId
            };

        var data = {
            third_party: {
                auth_data:
                    {
                        login: login,
                        password: password,
                        token: token,
                        url: connectUrl
                    },
                corporate: corporate,
                customer_title: folderTitle,
                provider_id: providerId,
                provider_key: providerKey
            }
        };

        ASC.Files.ServiceManager.saveThirdParty(ASC.Files.ServiceManager.events.SaveThirdParty, params, data);
    };

    var deleteProvider = function (providerId, providerKey, customerTitle, folderId) {
        var params =
            {
                providerId: providerId,
                providerKey: providerKey,
                folderId: folderId,
                customerTitle: customerTitle
            };

        ASC.Files.ServiceManager.deleteThirdParty(ASC.Files.ServiceManager.events.DeleteThirdParty, params);
    };

    var changeAccessToThirdparty = function (enable) {
        ASC.Files.ServiceManager.changeAccessToThirdparty(ASC.Files.ServiceManager.events.ChangeAccessToThirdparty, { enable: enable === true });
    };

    //event handler

    var onGetThirdParty = function (jsonData, params, errorMessage) {
        LoadingBanner.hideLoading();
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        if (jsonData.length > 0) {
            var data =
                jq(jsonData).map(function (i, item) {
                    return {
                        id: item.id,
                        corporate: (item.isnew == 1),
                        customer_title: item.title,
                        provider_id: item.provider_id,
                        provider_title: thirdPartyList[item.provider_key].providerTitle,
                        provider_key: item.provider_key,
                        isNew: false,
                        max_name_length: ASC.Files.Constants.MAX_NAME_LENGTH,
                        canCorporate: (ASC.Files.Constants.ADMIN && !ASC.Resources.Master.Personal),
                        error: item.error
                    };
                }).toArray();
            var jsonResult = { third_partyList: { entry: data } };
            var xmlData = ASC.Files.Common.jsonToXml(jsonResult);
            var htmlXML = ASC.Files.TemplateManager.translateFromString(xmlData);
            jq("#thirdPartyAccountList").html(htmlXML);
            jq("#emptyThirdPartyContainer").hide();
            jq("#thirdPartyAccountContainer").show();
        } else {
            jq("#thirdPartyAccountList .account-row").remove();
            jq("#thirdPartyAccountContainer").hide();
            jq("#emptyThirdPartyContainer").show();
        }

        if (ASC.Files.Folders.eventAfter != null && typeof ASC.Files.Folders.eventAfter == "function") {
            ASC.Files.Folders.eventAfter();
            ASC.Files.Folders.eventAfter = null;
        }
    };

    var onSaveThirdParty = function (jsonData, params, errorMessage) {
        var infoBlock = jq("#thirdPartyEditor div[id$=\"InfoPanel\"]");

        if (typeof errorMessage != "undefined") {
            if (jq("#settingThirdPartyPanel:visible").length > 0) {
                ASC.Files.UI.displayInfoPanel(errorMessage, true);

                var panel = (typeof params.folderId != "undefined" ?
                    jq("#" + params.folderId) :
                    jq("#account_" + params.providerKey + "_" + (params.providerId || 0)));

                panel.find("input").removeAttr("disabled");
                panel.find("a.button.account-save-link").removeClass("disable");
            } else {
                infoBlock.show().find("div").text(errorMessage);
                jq("#thirdPartyPanel input").removeAttr("disabled");
                LoadingBanner.hideLoaderBtn("#thirdPartyEditor");
            }
            return;
        }

        PopupKeyUpActionProvider.CloseDialog();

        var folderTitle = params.folderTitle;

        var folderObj = null;
        var folderId = params.folderId;

        if (jq("#settingThirdPartyPanel:visible").length > 0) {
            params.providerId = params.providerId ? params.providerId : 0;
            var data = {
                id: jsonData.id,
                corporate: params.corporate,
                customer_title: jsonData.title,
                provider_id: jsonData.provider_id,
                provider_title: thirdPartyList[jsonData.provider_key].providerTitle,
                provider_key: jsonData.provider_key,
                isNew: false,
                max_name_length: ASC.Files.Constants.MAX_NAME_LENGTH,
                canCorporate: (ASC.Files.Constants.ADMIN && !ASC.Resources.Master.Personal),
                error: jsonData.error
            };

            var xmlData = ASC.Files.Common.jsonToXml({ third_partyList: { entry: data } });
            var htmlXml = ASC.Files.TemplateManager.translateFromString(xmlData);
            var accountPanel = jq("#account_" + params.providerKey + "_" + params.providerId);

            if (accountPanel.length) {
                jq(accountPanel).replaceWith(htmlXml);
            } else {
                jq("#thirdPartyAccountList").append(htmlXml);

                jq("#emptyThirdPartyContainer").hide();
                jq("#thirdPartyAccountContainer").show();
            }
        } else {
            var folderPlaceId = params.corporate ? ASC.Files.Constants.FOLDER_ID_COMMON_FILES : ASC.Files.Constants.FOLDER_ID_MY_FILES;
            if (folderPlaceId == ASC.Files.Folders.currentFolder.id) {
                var stringData = ASC.Files.Common.jsonToXml({ folder: jsonData });
                var htmlXML = ASC.Files.TemplateManager.translateFromString(stringData);

                ASC.Files.EmptyScreen.hideEmptyScreen();

                folderObj = ASC.Files.UI.getEntryObject("folder", folderId);
                if (folderObj) {
                    folderObj.replaceWith(htmlXML);
                    folderObj = ASC.Files.UI.getEntryObject("folder", folderId);
                } else {
                    jq("#filesMainContent").prepend(htmlXML);
                    folderObj = jq("#filesMainContent .new-folder:first");
                }

                folderObj.yellowFade().removeClass("new-folder");

                ASC.Files.UI.resetSelectAll();

                folderTitle = ASC.Files.UI.getObjectTitle(folderObj);

            } else {
                if (folderId) {
                    ASC.Files.UI.getEntryObject("folder", folderId).remove();
                    ASC.Files.UI.checkEmptyContent();
                }
            }

            if (folderObj && folderObj.is(":visible")) {
                ASC.Files.UI.addRowHandlers(folderObj);
            }
        }

        ASC.Files.Tree.resetFolder(ASC.Files.Constants.FOLDER_ID_COMMON_FILES);
        ASC.Files.Tree.resetFolder(ASC.Files.Constants.FOLDER_ID_MY_FILES);

        ASC.Files.UI.displayInfoPanel(
            folderId
                ? ASC.Files.FilesJSResources.InfoChangeThirdParty.format(folderTitle)
                : ASC.Files.FilesJSResources.InfoSaveThirdParty.format(folderTitle,
                    params.corporate ? ASC.Files.FilesJSResources.CorporateFiles : ASC.Files.FilesJSResources.MyFiles));

        if (!params.providerId) {
            ASC.Files.Anchor.navigationSet(jsonData.id);
        }
    };

    var onDeleteThirdParty = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var providerId = params.providerId;
        var providerKey = params.providerKey;
        var folderId = params.folderId || jsonData;
        var folderTitle = params.customerTitle;
        var folderObj = ASC.Files.UI.getEntryObject("folder", folderId);

        if (folderObj != null) {
            folderObj.remove();

            ASC.Files.UI.checkEmptyContent();
        }

        var accountPanel = jq("#account_" + providerKey + "_" + providerId);
        if (accountPanel.length > 0) {
            jq(accountPanel).remove();
            if (jq("#thirdPartyAccountList .account-row").length == 0) {
                jq("#thirdPartyAccountContainer").hide();
                jq("#emptyThirdPartyContainer").show();
            }
        }

        var parentId = ASC.Files.Tree.getParentId(folderId);
        ASC.Files.Tree.resetFolder(parentId);

        ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoRemoveThirdParty.format(folderTitle));
    };

    var onChangeAccessToThirdparty = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        jq("#cbxEnableSettings").prop("checked", jsonData === true);
    };

    return {
        init: init,
        thirdPartyList: thirdPartyList,

        isThirdParty: isThirdParty,
        getThirdPartyItem: getThirdPartyItem,

        showChangeDialog: showChangeDialog,
        showDeleteDialog: showDeleteDialog,

        addNewThirdPartyAccount: addNewThirdPartyAccount,
        cancelThirdPartyAccount: cancelThirdPartyAccount,
        saveThirdPartyAccount: saveThirdPartyAccount,

        showSettingThirdParty: showSettingThirdParty,
        changeAccessToThirdpartySettings: changeAccessToThirdpartySettings,
        showThirdPartyNewAccount: showThirdPartyNewAccount,

        showThirdPartyActionsPanel: showThirdPartyActionsPanel
    };
})();

(function ($) {
    ASC.Files.ThirdParty.init();

    $(function () {
        jq("#thirdPartyEditor div[id$=\"InfoPanel\"]")
            .removeClass("infoPanel")
            .addClass("errorBox")
            .css("margin", "10px 16px 0");

        jq(".add-account-button").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            PopupKeyUpActionProvider.CloseDialog();

            var thirdPartyKey = jq(this).attr("data-provider");
            var thirdParty = ASC.Files.ThirdParty.thirdPartyList[thirdPartyKey];
            var eventAfter = function () {
                return ASC.Files.ThirdParty.addNewThirdPartyAccount(thirdParty);
            };

            if (jq("#settingThirdPartyPanel:visible").length == 0) {
                if (!thirdParty.getTokenUrl) {
                    ASC.Files.Folders.eventAfter = eventAfter;
                } else {
                    eventAfter();
                }
                ASC.Files.Anchor.move("setting=thirdparty");
                return false;
            }
            eventAfter();
            return false;
        });

        jq("#thirdPartyAccountList").on("click", "a.account-cancel-link", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.ThirdParty.cancelThirdPartyAccount(this);
            return false;
        });

        jq("#thirdPartyAccountList").on("click", "a.account-save-link:not(.disable)", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.ThirdParty.saveThirdPartyAccount(this);
            return false;
        });

        jq("#cbxEnableSettings").on("change", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.ThirdParty.changeAccessToThirdpartySettings(this);
            return false;
        });

        jq("#thirdPartyConnectAccount").on("click", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.ThirdParty.showThirdPartyNewAccount();
            return false;
        });

        jq("#thirdPartyAccountList").on("click", ".menu-small", ASC.Files.ThirdParty.showThirdPartyActionsPanel);

        jq("#thirdPartyAccountList").on("keyup", ".account-input-login, .account-input-pass, .account-input-folder", function (event) {
            if (!e) {
                var e = event;
            }
            e = ASC.Files.Common.fixEvent(e);
            var code = e.keyCode || e.which;

            if (code == ASC.Files.Common.keyCode.enter) {
                var accountRow = jq(this).closest(".account-row");
                if (jq(this).is(".account-input-login")) {
                    accountRow.find(".account-input-pass").focus();
                } else if (jq(this).is(".account-input-pass")) {
                    accountRow.find(".account-input-folder").focus();
                } else {
                    accountRow.find(".account-save-link").click();
                }
            } else if (code == ASC.Files.Common.keyCode.esc) {
                accountRow = jq(this).closest(".account-row");
                accountRow.find(".account-cancel-link").click();
            }
        });

    });
})(jQuery);