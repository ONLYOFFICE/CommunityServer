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


window.ASC.Files.ThirdParty = (function () {
    var isInit = false;
    var thirdPartyList = {
        Box: { key: "Box", customerTitle: ASC.Files.FilesJSResources.FolderTitleBoxNet, providerTitle: ASC.Files.FilesJSResources.TypeTitleBoxNet, getTokenUrl: ASC.Files.Constants.URL_OAUTH_BOX },
        BoxNet: { key: "BoxNet", customerTitle: ASC.Files.FilesJSResources.FolderTitleBoxNet, providerTitle: ASC.Files.FilesJSResources.TypeTitleBoxNet },
        DropBox: { key: "DropBox", customerTitle: ASC.Files.FilesJSResources.FolderTitleDropBox, providerTitle: ASC.Files.FilesJSResources.TypeTitleDropBox },
        DropboxV2: { key: "DropboxV2", customerTitle: ASC.Files.FilesJSResources.FolderTitleDropBox, providerTitle: ASC.Files.FilesJSResources.TypeTitleDropBox, getTokenUrl: ASC.Files.Constants.URL_OAUTH_DROPBOXV2 },
        DocuSign: { key: "DocuSign", customerTitle: ASC.Files.FilesJSResources.FolderTitleDocuSign, providerTitle: ASC.Files.FilesJSResources.TypeTitleDocuSign, getTokenUrl: ASC.Files.Constants.URL_OAUTH_DOCUSIGN, link: ASC.Files.Constants.URL_OAUTH_DOCUSIGN_LINK },
        Google: { key: "Google", customerTitle: ASC.Files.FilesJSResources.FolderTitleGoogle, providerTitle: ASC.Files.FilesJSResources.TypeTitleGoogle, getTokenUrl: "http://www.onlyoffice.com" },
        GoogleDrive: { key: "GoogleDrive", customerTitle: ASC.Files.FilesJSResources.FolderTitleGoogle, providerTitle: ASC.Files.FilesJSResources.TypeTitleGoogle, getTokenUrl: ASC.Files.Constants.URL_OAUTH2_GOOGLE },
        OneDrive: { key: "OneDrive", customerTitle: ASC.Files.FilesJSResources.FolderTitleSkyDrive, providerTitle: ASC.Files.FilesJSResources.TypeTitleSkyDrive, getTokenUrl: ASC.Files.Constants.URL_OAUTH_SKYDRIVE },
        SharePoint: { key: "SharePoint", customerTitle: ASC.Files.FilesJSResources.FolderTitleSharePoint, providerTitle: ASC.Files.FilesJSResources.TypeTitleSharePoint, urlRequest: true },
        SkyDrive: { key: "SkyDrive", customerTitle: ASC.Files.FilesJSResources.FolderTitleSkyDrive, providerTitle: ASC.Files.FilesJSResources.TypeTitleSkyDrive, getTokenUrl: ASC.Files.Constants.URL_OAUTH_SKYDRIVE },
        WebDav: { key: "WebDav", customerTitle: ASC.Files.FilesJSResources.FolderTitleWebDav, providerTitle: ASC.Files.FilesJSResources.TypeTitleWebDav, urlRequest: true },
        kDrive: { key: "kDrive", customerTitle: ASC.Files.FilesJSResources.FolderTitlekDrive, providerTitle: ASC.Files.FilesJSResources.TypeTitlekDrive },
        Yandex: { key: "Yandex", customerTitle: ASC.Files.FilesJSResources.FolderTitleYandex, providerTitle: ASC.Files.FilesJSResources.TypeTitleYandex }
    };

    var docuSignFolderSelector;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetThirdParty, onGetThirdParty);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SaveThirdParty, onSaveThirdParty);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.DeleteThirdParty, onDeleteThirdParty);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.ChangeAccessToThirdparty, onChangeAccessToThirdparty);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SaveDocuSign, onSaveDocuSign);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SendDocuSign, onSendDocuSign);

            jq(document).click(function (event) {
                jq.dropdownToggle().registerAutoHide(event, ".account-row .menu-small", "#thirdPartyActionPanel");
            });
        }

        jq("#thirdpartyToDocuSignAddMessage").on("click", function () {
            addDocuSignMessage();
            return false;
        });

        jq("#thirdpartyToDocuSignRemoveMessage").on("click", function () {
            removeDocuSignMessage();
            jq("#thirdpartyToDocuSignMessage").val("");
            return false;
        });

        jq.dropdownToggle(
            {
                dropdownID: "thirdpartyToDocuSignFolderSelector",
                inPopup: true,
                switcherSelector: "#thirdpartyToDocuSignFolder",
            });

        docuSignFolderSelector = new ASC.Files.TreePrototype("#docuSignFolderSelector");
        docuSignFolderSelector.clickOnFolder = docuSignFolderSelect;

        jq("#thirdpartyToDocuSignUserSelector")
            .useradvancedSelector(
                {
                    inPopup: true,
                    showGroups: true,
                    showme: false,
                })
            .on("showList", docuSignAddRecipients);

        jq("#thirdpartyToDocuSignRecipientsList").on("click", ".ds-user-link-delete", docuSignRemoveRecipients);
    };

    var isThirdParty = function (entryData, entryType, entryId) {
        if (!entryData && !entryType) {
            entryData = ASC.Files.Folders.currentFolder;
        } else if (!entryData) {
            entryData = ASC.Files.UI.getObjectData(ASC.Files.UI.getEntryObject(entryType, entryId));
            if (!entryData && entryType == "folder" && ASC.Files.Tree) {
                entryData = ASC.Files.Tree.getFolderData(entryId);
            }
        }

        return getThirdPartyItem(entryData) != null;
    };

    var getThirdPartyItem = function (entryData) {
        return ASC.Files.ThirdParty.thirdPartyList[entryData.provider_key];
    };

    var showSettingThirdParty = function () {
        if (jq("#thirdPartyEditor").length == 0) {
            return;
        }
        getThirdParty();
        ASC.Files.UI.hideAllContent();
        jq("#treeSetting").addClass("currentCategory open");
        jq(".settings-link-thirdparty").addClass("active");
        jq("#settingThirdPartyPanel").show();

        ASC.Files.UI.setDocumentTitle(ASC.Files.FilesJSResources.TitleSettingsThirdParty);
    };

    var showChangeDialog = function (folderData) {
        var thirdPartyItem = getThirdPartyItem(folderData);
        var thirdParty = jq.extend(thirdPartyItem,
            {
                id: folderData.provider_id,
                customerTitle: folderData.title,
                folderId: folderData.id
            });

        jq("#thirdPartyEditor div[id$=\"InfoPanel\"]").hide();

        jq("#thirdPartyConnectionUrl, #thirdPartyPassword").val("");
        jq("#thirdPartyConnectionUrl").closest(".thirdparty-editor-row").hide();
        jq("#thirdPartyPassword").closest(".thirdparty-editor-row").hide();

        if (!thirdParty.getTokenUrl) {
            jq("#thirdPartyAccount").closest(".thirdparty-editor-row").hide();
            jq("#thirdPartyPassword").closest(".thirdparty-editor-row").show();
            if (thirdParty.urlRequest) {
                jq("#thirdPartyConnectionUrl").closest(".thirdparty-editor-row").show();
            }
        } else {
            jq("#thirdPartyAccount").attr("data-provider", thirdParty.key).attr("data-providerId", thirdParty.id).closest(".thirdparty-editor-row").show();
        }

        for (var classItem in ASC.Files.ThirdParty.thirdPartyList) {
            jq("#thirdPartyAccount span, #thirdPartyTitle").removeClass(classItem);
        }
        jq("#thirdPartyAccount").attr("data-token", "");
        jq("#thirdPartyAccount span, #thirdPartyTitle").addClass(thirdParty.key);
        jq("#thirdPartyTitle").focus();

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

        ASC.Files.UI.blockUI("#thirdPartyEditor", 400);
        PopupKeyUpActionProvider.EnterAction = "jq(\"#submitThirdParty\").click();";
    };

    var submitEditor = function (thirdParty) {
        var connectUrl = jq("#thirdPartyConnectionUrl").val().trim();
        var password = jq("#thirdPartyPassword").val().trim();
        var folderTitle = jq("#thirdPartyTitle").val().trim();
        folderTitle = ASC.Files.Common.replaceSpecCharacter(folderTitle);
        var corporate = (jq("#thirdPartyCorporate").prop("checked") === true);
        var token = jq("#thirdPartyAccount").attr("data-token");

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

        saveProvider(thirdParty.id, thirdParty.key, folderTitle, connectUrl, "", password, token, corporate, thirdParty.folderId);
    };

    var showDeleteDialog = function (providerId, providerKey, providerTitle, customerTitle, folderData) {
        providerId = providerId || (folderData ? folderData.provider_id : null);
        if (providerId == null && providerKey != ASC.Files.ThirdParty.thirdPartyList.DocuSign.key) {
            return;
        }

        var folderId = folderData ? folderData.entryId : null;

        providerTitle = providerTitle || getThirdPartyItem(folderData).providerTitle;
        jq("#thirdPartyDeleteDescr").html(ASC.Files.FilesJSResources.ConfirmDeleteThirdParty.format(customerTitle, providerTitle));

        jq("#deleteThirdParty").unbind("click").click(function () {
            ASC.Files.UI.blockObjectById("folder", folderId, true, ASC.Files.FilesJSResources.DescriptRemove);
            PopupKeyUpActionProvider.CloseDialog();
            deleteProvider(providerId, providerKey, customerTitle, folderId);
            return false;
        });

        ASC.Files.UI.blockUI("#thirdPartyDelete", 400);
        PopupKeyUpActionProvider.EnterAction = "jq(\"#deleteThirdParty\").click();";
    };

    var addNewThirdPartyAccount = function (thirdParty) {
        if (!thirdParty) {
            return false;
        }
        var accountPanel = jq("#account_" + thirdParty.key);
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
            OAuthPopup(thirdParty.getTokenUrl, thirdParty.key);
        }
        return false;
    };

    var editTokenThirdPartyAccount = function (thirdParty) {
        OAuthCallback = function (token) {
            var accountPanel = jq("#account_" + thirdParty.provider_id);
            accountPanel.find(".account-hidden-token").val(token);
        };
        OAuthPopup(thirdParty.getTokenUrl, null, thirdParty.provider_id);

        return false;
    };

    var changeTokenThirdPartyAccount = function () {
        if (!jq("#thirdPartyEditor").is(":visible")) {
            return false;
        }

        var thirdPartyKey = jq("#thirdPartyAccount").attr("data-provider");
        var thirdParty = ASC.Files.ThirdParty.thirdPartyList[thirdPartyKey];
        if (!thirdParty) {
            return false;
        }

        OAuthCallback = function (token) {
            jq("#thirdPartyAccount").attr("data-token", token);
        };
        OAuthPopup(thirdParty.getTokenUrl, null, jq("#thirdPartyAccount").attr("data-providerId"));

        return false;
    };

    var addNewThirdParty = function (thirdParty, token) {
        var data = {
            canCorporate: (ASC.Files.Constants.ADMIN && !ASC.Resources.Master.Personal),
            canEdit: true,
            corporate: false,
            customer_title: thirdParty.customerTitle,
            isNew: true,
            link: thirdParty.link,
            max_name_length: ASC.Files.Constants.MAX_NAME_LENGTH,
            provider_id: thirdParty.key,
            provider_key: thirdParty.key,
            provider_title: thirdParty.providerTitle,
            getTokenUrl: thirdParty.getTokenUrl
        };

        if (thirdParty.key == ASC.Files.ThirdParty.thirdPartyList.DocuSign.key) {
            if (token) {
                ASC.Files.ServiceManager.saveDocuSign(ASC.Files.ServiceManager.events.SaveDocuSign, {thirdParty: thirdParty}, token);
                return;
            }
            data.canEdit = false;
            data.isNew = false;
            ASC.Files.ThirdParty.docuSignAttached(true);
        }

        var xmlData = ASC.Files.Common.jsonToXml({ third_partyList: { entry: data } });
        var htmlXml = ASC.Files.TemplateManager.translateFromString(xmlData);
        jq("#thirdPartyAccountList").append(htmlXml);

        jq("#emptyThirdPartyContainer").hide();
        jq("#thirdPartyAccountContainer").show();

        var accountPanel = jq("#account_" + data.provider_id);

        if (data.isNew) {
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
        }

        jq(accountPanel).yellowFade();
    };

    var editThirdPartyAccount = function (obj) {
        var accountPanel = jq(obj).is(".account-row") ? jq(obj) : jq(obj).parents(".account-row");

        ASC.Files.UI.checkCharacter(jq(accountPanel).find(".account-input-folder"));
        var customerTitle = jq(accountPanel).find(".account-hidden-customer-title").val().trim();
        jq(accountPanel).find(".account-input-folder").val(customerTitle);
        jq(accountPanel).find(".account-settings-container").show();

        var providerKey = jq(accountPanel).find(".account-hidden-provider-key").val().trim();
        var thirdParty = ASC.Files.ThirdParty.thirdPartyList[providerKey];

        if (thirdParty && !thirdParty.getTokenUrl) {
            jq(accountPanel).find(".account-log-pass-container").show();
            if (thirdParty.urlRequest) {
                jq(accountPanel).find(".account-field-url").show();
            }
        }

        jq(accountPanel).find("input:visible:first").focus();
    };

    var cancelThirdPartyAccount = function (obj) {
        var account = jq(obj).parents(".account-row");
        var providerId = parseInt(jq(account).find(".account-hidden-provider-id").val());
        if (!providerId) {
            jq(account).remove();
            if (jq("#thirdPartyAccountList .account-row").length == 0) {
                jq("#thirdPartyAccountContainer").hide();
                jq("#emptyThirdPartyContainer").show();
            }
            return;
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
        var providerId = null;
        var providerIdTmp = parseInt(jq(account).find(".account-hidden-provider-id").val());
        if (providerIdTmp) {
            providerId = providerIdTmp;
        }
        var providerKey = jq(account).find(".account-hidden-provider-key").val().trim();
        var customerTitle = jq(account).find(".account-input-folder").val().trim();
        var connectUrl = jq(account).find(".account-input-url").val().trim();
        var login = (jq(account).find(".account-input-login").val() || "").trim();
        var password = jq(account).find(".account-input-pass").val().trim();
        var token = jq(account).find(".account-hidden-token").val().trim();
        var corporate = (jq(account).find(".account-cbx-corporate").prop("checked") === true);

        var thirdParty = ASC.Files.ThirdParty.thirdPartyList[providerKey];

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

        jq(".settings-link-thirdparty, .tree-thirdparty").toggleClass("display-none", enable !== true);
    };

    var showThirdPartyNewAccount = function () {
        ASC.Files.Actions.hideAllActionPanels();
        ASC.Files.UI.blockUI("#thirdPartyNewAccount", 500);
    };

    var showThirdPartyActionsPanel = function (event) {
        var e = jq.fixEvent(event);
        var target = jq(e.srcElement || e.target);

        var account = jq(target).parents(".account-row");
        if (jq(account).is(":has(.account-settings-container)")) {
            jq("#accountEditLinkContainer").show().unbind("click").click(function () {
                ASC.Files.Actions.hideAllActionPanels();
                editThirdPartyAccount(target);
            });
        } else {
            jq("#accountEditLinkContainer").hide();
        }

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

    var isDifferentThirdParty = function (id1, id2) {
        if (!ASC.Files.Common.isCorrectId(id1)
            || !ASC.Files.Common.isCorrectId(id2)) {
            return false;
        }

        var getAccId = function (id) {
            var onlyofficeRegExp = /^\d+/;
            var accId = onlyofficeRegExp.exec(id);
            if (accId != null) {
                return "onlyoffice";
            }

            var accountRegExp = /([^\|]+)(\|.*)?/;
            accId = accountRegExp.exec(id)[1];

            return (accId || "").replace(/-$/, "");
        };

        var acc1 = getAccId(id1);
        var acc2 = getAccId(id2);

        return acc1 != acc2;
    };

    var showMoveThirdPartyMessage = function (folderToId, folderToTitle, pathDest) {
        var providerTitle = getThirdPartyItem(ASC.Files.Folders.currentFolder).providerTitle;
        jq("#moveThirdPartyMessage").html(jq.format(ASC.Files.FilesJSResources.ConfirmThirdPartyMoveMessage, "<br/><br/>", providerTitle));

        jq("#buttonMoveThirdParty").unbind("click").click(function () {
            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();
            ASC.Files.Folders.curItemFolderMoveTo(folderToId, folderToTitle, pathDest, true);
        });

        jq("#buttonCopyThirdParty").unbind("click").click(function () {
            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();
            ASC.Files.Folders.isCopyTo = true;
            ASC.Files.Folders.curItemFolderMoveTo(folderToId, folderToTitle, pathDest, true);
        });

        jq("#buttonCancelMoveThirdParty").unbind("click").click(function () {
            ASC.Files.Folders.isCopyTo = false;

            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();
        });

        ASC.Files.UI.blockUI("#thirPartyConfirmMove", 420);

        PopupKeyUpActionProvider.EnterAction = "jq(\"#buttonMoveThirdParty\").click();";
        PopupKeyUpActionProvider.CloseDialogAction = "jq(\"#buttonCancelMoveThirdParty\").click();";
    };


    var thirdpartyAvailable = function () {
        return !!jq(".settings-link-thirdparty").length;
    };

    var docuSignAttached = function (value) {
        if (typeof value != "undefined") {
            jq(".add-account-button.DocuSign").attr("data-signed", !!value);
        }
        return jq(".add-account-button.DocuSign").attr("data-signed") == "true";
    };

    var showDocuSignDialog = function (fileData) {
        if (jq("#thirdpartyToDocuSignHelper").length) {
            ASC.Files.UI.blockUI("#thirdpartyToDocuSignHelper", 500);
            return;
        } else if (!ASC.Files.ThirdParty.docuSignAttached()) {
            ASC.Files.ThirdParty.addAccountButton(null, ASC.Files.ThirdParty.thirdPartyList.DocuSign.key);
            return;
        }

        var header = ASC.Files.FilesJSResources.DocuSignDialogHeader.format(fileData.title);
        jq("#thirdpartyToDocuSign .thirdparty-todocusign-header").attr("title", header).text(header);

        var titleObj = jq("#thirdpartyToDocuSign .thirdparty-todocusign-title");
        titleObj.attr("title", fileData.title).attr("placeholder", fileData.title).val(fileData.title);
        ASC.Files.UI.checkCharacter(titleObj);

        var folderId = ASC.Files.Folders.currentFolder.id;
        var folderData = docuSignFolderSelector.getFolderData(folderId);
        if (!ASC.Files.UI.accessEdit(folderData)) {
            folderId = ASC.Files.Constants.FOLDER_ID_MY_FILES;
        }

        docuSignFolderSelect(folderId);
        docuSignFolderSelector.setCurrent(folderId);

        jq("#thirdpartyToDocuSignRecipientsList").hide();

        jq("#thirdpartyToDocuSignUserSelector").useradvancedSelector("reset");

        var sendDocuSign = function () {
            var fileTitle = titleObj.val();
            if (fileTitle.length < 1) {
                titleObj.focus();
                return false;
            }
            folderId = jq("#thirdpartyToDocuSignFolder").attr("data-id");
            var message = jq("#thirdpartyToDocuSignMessage:visible").val();
            var users = jq("#thirdpartyToDocuSignRecipientsList .userLink").map(function (i, item) {
                return jq(item).attr("data-uid");
            }).toArray();

            PopupKeyUpActionProvider.CloseDialog();
            ASC.Files.UI.blockObject(fileData.entryObject, true, ASC.Files.FilesJSResources.DescriptDocuSign);

            var data = {
                docusign_data: {
                    folderId: folderId,
                    message: message,
                    name: fileTitle,
                    users: {entry: users},
                }
            };

            var winSign = window.open(ASC.Desktop ? "" : ASC.Files.Constants.URL_LOADER);

            var params = {
                fileId: fileData.id,
                entryObject: fileData.entryObject,
                winSign: winSign,
            };

            ASC.Files.ServiceManager.sendDocuSign(ASC.Files.ServiceManager.events.SendDocuSign, params, data);

            return false;
        };

        jq("#thirdpartyToDocuSignSend").unbind("click").click(sendDocuSign);

        jq("#thirdpartyToDocuSign").bind("keydown", function (event) {
            if (!jq("#thirdpartyToDocuSign").is(":visible")) {
                return true;
            }

            if (!e) {
                var e = event;
            }

            var target = jq(e.srcElement || e.target);
            if (jq(target).is("#thirdpartyToDocuSignMessage") && !e.ctrlKey) {
                return true;
            }

            var code = e.keyCode || e.which;

            switch (code) {
                case ASC.Files.Common.keyCode.enter:
                    sendDocuSign();
                    break;
            }
            return true;
        });

        removeDocuSignMessage();

        ASC.Files.UI.blockUI("#thirdpartyToDocuSign", 400);
    };

    var removeDocuSignMessage = function () {
        jq("#thirdpartyToDocuSignRemoveMessage").hide();
        jq("#thirdpartyToDocuSign").removeClass("with-message");
        jq("#thirdpartyToDocuSignAddMessage").show();
    };

    var addDocuSignMessage = function () {
        jq("#thirdpartyToDocuSignAddMessage").hide();
        jq("#thirdpartyToDocuSign").addClass("with-message");
        jq("#thirdpartyToDocuSignRemoveMessage").show();
    };

    var docuSignAddRecipients = function (event, users) {
        jq("#thirdpartyToDocuSignRecipientsList").empty();

        users.forEach(function (user) {
            user.decodedTitle = Encoder.htmlDecode(user.title);

            var stringXml = ASC.Files.Common.jsonToXml({
                userItem: user
            });
            var htmlXML = ASC.Files.TemplateManager.translateFromString(stringXml);

            jq("#thirdpartyToDocuSignRecipientsList").append(htmlXML);
        });
        jq("#thirdpartyToDocuSignRecipientsList").show();
        ASC.Files.UI.registerUserProfilePopup(jq("#thirdpartyToDocuSignRecipientsList"));
    };

    var docuSignRemoveRecipients = function () {
        var row = jq(this).closest(".ds-user-link");
        var itemId = row.find(".userLink").attr("data-uid");
        jq("#thirdpartyToDocuSignUserSelector").useradvancedSelector("unselect", [itemId]);
        row.remove();
        if (!jq("#thirdpartyToDocuSignRecipientsList .userLink").length) {
            jq("#thirdpartyToDocuSignRecipientsList").hide();
        }
    };

    var docuSignFolderSelect = function (folderId) {

        docuSignFolderSelector.rollUp();
        docuSignFolderSelector.setCurrent(folderId);

        var folderData = docuSignFolderSelector.getFolderData(folderId);

        if (ASC.Files.UI.accessEdit(folderData)) {
            var title = docuSignFolderSelector.getFolderTitle(folderId);
            jq("#thirdpartyToDocuSignFolder a").attr("title", title).text(title);
            jq("#thirdpartyToDocuSignFolder").attr("data-id", folderId);

            ASC.Files.Actions.hideAllActionPanels();

            return true;
        } else {
            var errorString = ASC.Files.FilesJSResources.ErrorMassage_SecurityException;
            if (folderId == ASC.Files.Constants.FOLDER_ID_PROJECT
                || folderId == ASC.Files.Constants.FOLDER_ID_SHARE) {
                errorString = ASC.Files.FilesJSResources.ErrorMassage_SecurityException_PrivateRoot;
            }
            ASC.Files.UI.displayInfoPanel(errorString, true);

            docuSignFolderSelector.expandFolder(folderId);
            return false;
        }
    };

    var docuSignFolderSelectorReset = function (folderId) {
        if (!ASC.Files.Common.isCorrectId(folderId) || jq("#thirdPartyEditor").length == 0) {
            return;
        }

        docuSignFolderSelector.resetFolder(folderId);
    };

    var processAnchor = function () {
        var providerKey;
        var providerId;
        var code;

        var segm = location.hash.substr(1).split("&");
        jq(segm).each(function (i, item) {
            var param = item.split("=");
            if (param[0] == "providerKey") {
                providerKey = param[1];
            } else if (param[0] == "providerId") {
                providerId = param[1];
            } else if (param[0] == "code") {
                code = param[1];
            }
        });

        if (providerKey) {
            var thirdParty = ASC.Files.ThirdParty.thirdPartyList[providerKey];
            if (!thirdParty) {
                return;
            }
            addNewThirdParty(thirdParty, code);
            return;
        }

        if (providerId) {
            editThirdPartyAccount("#account_" + providerId);
            jq("#thirdPartyAccount").attr("data-token", code);
        }
    };

    var addAccountButton = function (e, thirdPartyKey) {
        ASC.Files.Actions.hideAllActionPanels();

        if (!thirdPartyKey) {
            thirdPartyKey = jq(this).attr("data-provider");
        }
        var thirdParty = ASC.Files.ThirdParty.thirdPartyList[thirdPartyKey];
        if (!thirdParty) {
            return true;
        }

        if (jq("#thirdPartyEditor").is(":visible")) {
            ASC.Files.ThirdParty.changeTokenThirdPartyAccount();
            return false;
        }

        PopupKeyUpActionProvider.CloseDialog();

        var account = jq(this).closest(".account-row");
        if (account.length) {
            thirdParty.provider_id = jq(account).find(".account-hidden-provider-id").val();

            ASC.Files.ThirdParty.editTokenThirdPartyAccount(thirdParty);
            return false;
        }

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

        if (providerKey == ASC.Files.ThirdParty.thirdPartyList.DocuSign.key) {
            params.providerId = 0;
            ASC.Files.ServiceManager.deleteDocuSign(ASC.Files.ServiceManager.events.DeleteThirdParty, params);
        } else {
            ASC.Files.ServiceManager.deleteThirdParty(ASC.Files.ServiceManager.events.DeleteThirdParty, params);
        }
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

        var docuSign = ASC.Files.ThirdParty.docuSignAttached();

        if (jsonData.length > 0 || docuSign) {
            var data =
                jq(jsonData).map(function (i, item) {
                    var thirdparty = ASC.Files.ThirdParty.thirdPartyList[item.provider_key];
                    return {
                        canCorporate: (ASC.Files.Constants.ADMIN && !ASC.Resources.Master.Personal),
                        canEdit: true,
                        corporate: (item.isnew == 1),
                        customer_title: item.title,
                        error: item.error,
                        id: item.id,
                        isNew: false,
                        max_name_length: ASC.Files.Constants.MAX_NAME_LENGTH,
                        provider_id: item.provider_id,
                        provider_key: item.provider_key,
                        provider_title: thirdparty.providerTitle,
                        getTokenUrl: thirdparty.getTokenUrl,
                    };
                }).toArray();

            if (docuSign) {
                data.push({
                    canEdit: false,
                    canCorporate: false,
                    corporate: false,
                    customer_title: ASC.Files.ThirdParty.thirdPartyList.DocuSign.customerTitle,
                    id: 0,
                    isNew: false,
                    link: ASC.Files.ThirdParty.thirdPartyList.DocuSign.link,
                    max_name_length: ASC.Files.Constants.MAX_NAME_LENGTH,
                    provider_id: ASC.Files.ThirdParty.thirdPartyList.DocuSign.key,
                    provider_key: ASC.Files.ThirdParty.thirdPartyList.DocuSign.key,
                    provider_title: ASC.Files.ThirdParty.thirdPartyList.DocuSign.providerTitle,
                });
            }
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
                    jq("#account_" + (params.providerId || params.providerKey)));

                panel.find("input").removeAttr("disabled");
                panel.find("a.button.account-save-link").removeClass("disable");

                var jqThis = panel.find(".account-input-url");
                if (!jqThis.is(":visible")) {
                    jqThis = panel.find(".account-input-login");
                    if (!jqThis.is(":visible")) {
                        jqThis = panel.find(".account-input-pass");
                        if (!jqThis.is(":visible")) {
                            jqThis = panel.find(".account-input-folder");
                        }
                    }
                }
                jqThis.focus();
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
            var thirdparty = ASC.Files.ThirdParty.thirdPartyList[jsonData.provider_key];
            var data = {
                id: jsonData.id,
                corporate: params.corporate,
                customer_title: jsonData.title,
                provider_id: jsonData.provider_id,
                provider_title: thirdparty.providerTitle,
                provider_key: jsonData.provider_key,
                isNew: false,
                max_name_length: ASC.Files.Constants.MAX_NAME_LENGTH,
                canCorporate: (ASC.Files.Constants.ADMIN && !ASC.Resources.Master.Personal),
                error: jsonData.error,
                canEdit: jsonData.provider_key != ASC.Files.ThirdParty.thirdPartyList.DocuSign.key,
                getTokenUrl: thirdparty.getTokenUrl,
            };

            var xmlData = ASC.Files.Common.jsonToXml({ third_partyList: { entry: data } });
            var htmlXml = ASC.Files.TemplateManager.translateFromString(xmlData);
            var accountPanel = jq("#account_" + params.providerId);

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

        var fromFolderid = ASC.Files.Tree.getParentId(jsonData.id);
        ASC.Files.Tree.reloadFolder(fromFolderid);
        if (params.corporate) {
            if (fromFolderid != ASC.Files.Constants.FOLDER_ID_COMMON_FILES) {
                ASC.Files.Tree.reloadFolder(ASC.Files.Constants.FOLDER_ID_COMMON_FILES);
            }
        } else {
            if (fromFolderid != ASC.Files.Constants.FOLDER_ID_MY_FILES) {
                ASC.Files.Tree.reloadFolder(ASC.Files.Constants.FOLDER_ID_MY_FILES);
            }
        }

        ASC.Files.UI.displayInfoPanel(
            folderId
                ? ASC.Files.FilesJSResources.InfoChangeThirdParty.format(folderTitle)
                : ASC.Files.FilesJSResources.InfoSaveThirdParty.format(folderTitle,
                    params.corporate ? ASC.Files.FilesJSResources.CorporateFiles : ASC.Files.FilesJSResources.MyFiles));

        if (!params.providerId) {
            ASC.Files.Filter.clearFilter(true);
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

        if (folderObj != null && folderObj.length) {
            folderObj.remove();

            ASC.Files.UI.checkEmptyContent();
        } else if (providerKey == ASC.Files.ThirdParty.thirdPartyList.DocuSign.key) {
            ASC.Files.ThirdParty.docuSignAttached(false);
        }

        var entryId = providerKey == ASC.Files.ThirdParty.thirdPartyList.DocuSign.key ? providerKey : providerId;
        var accountPanel = jq("#account_" + entryId);
        if (accountPanel.length > 0) {
            jq(accountPanel).remove();
            if (jq("#thirdPartyAccountList .account-row").length == 0) {
                jq("#thirdPartyAccountContainer").hide();
                jq("#emptyThirdPartyContainer").show();
            }
        }

        var parentId = ASC.Files.Tree.getParentId(folderId);
        ASC.Files.Tree.reloadFolder(parentId);

        ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoRemoveThirdParty.format(folderTitle));
    };

    var onChangeAccessToThirdparty = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        jq("#cbxEnableSettings").prop("checked", jsonData === true);
    };

    var onSaveDocuSign = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        addNewThirdParty(params.thirdParty);
    };

    var onSendDocuSign = function (url, params, errorMessage) {
        var winSign = params.winSign;
        ASC.Files.UI.blockObject(params.entryObject);

        if (typeof errorMessage != "undefined") {
            winSign.close();
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }
        
        if (winSign && winSign.location) {
            winSign.location.href = url;
        } else {
            winSign = window.open(url, "_blank");
        }
    };

    return {
        init: init,
        thirdPartyList: thirdPartyList,

        isThirdParty: isThirdParty,
        isDifferentThirdParty: isDifferentThirdParty,

        showChangeDialog: showChangeDialog,
        showDeleteDialog: showDeleteDialog,

        addAccountButton: addAccountButton,
        addNewThirdPartyAccount: addNewThirdPartyAccount,
        editTokenThirdPartyAccount: editTokenThirdPartyAccount,
        changeTokenThirdPartyAccount: changeTokenThirdPartyAccount,
        cancelThirdPartyAccount: cancelThirdPartyAccount,
        saveThirdPartyAccount: saveThirdPartyAccount,

        showSettingThirdParty: showSettingThirdParty,
        changeAccessToThirdpartySettings: changeAccessToThirdpartySettings,
        showThirdPartyNewAccount: showThirdPartyNewAccount,
        processAnchor: processAnchor,

        showThirdPartyActionsPanel: showThirdPartyActionsPanel,

        showMoveThirdPartyMessage: showMoveThirdPartyMessage,

        thirdpartyAvailable: thirdpartyAvailable,
        docuSignAttached: docuSignAttached,
        showDocuSignDialog: showDocuSignDialog,
        docuSignFolderSelectorReset: docuSignFolderSelectorReset,
    };
})();

(function ($) {

    if (jq("#thirdPartyEditor").length == 0) {
        return;
    }

    $(function () {
        jq("#thirdPartyEditor div[id$=\"InfoPanel\"]")
            .removeClass("infoPanel")
            .addClass("errorBox")
            .css("margin", "10px 16px 0");

        jq(".add-account-button, .edit-account-button").click(ASC.Files.ThirdParty.addAccountButton);
        jq("#thirdPartyAccountList").on("click", ".edit-account-button", ASC.Files.ThirdParty.addAccountButton);

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

        jq(".account-connect").on("click", function () {
            var eventAfter = function () {
                return ASC.Files.ThirdParty.showThirdPartyNewAccount();
            };

            if (jq("#settingThirdPartyPanel:visible").length == 0) {
                ASC.Files.Folders.eventAfter = eventAfter;
                ASC.Files.Anchor.move("setting=thirdparty");
                return;
            }
            eventAfter();
        });

        jq("#thirdPartyAccountList").on("click", ".menu-small", ASC.Files.ThirdParty.showThirdPartyActionsPanel);

        jq("#thirdPartyAccountList").on("keyup", ".account-input-url, .account-input-login, .account-input-pass, .account-input-folder, .account-cbx-corporate", function (event) {
            if (!e) {
                var e = event;
            }
            e = jq.fixEvent(e);
            var code = e.keyCode || e.which;

            var accountRow = jq(this).closest(".account-row");
            var jqThis = jq(this);
            if (code == ASC.Files.Common.keyCode.enter) {
                if (!jqThis.val().trim().length) {
                    return;
                }
                if (jqThis.is(".account-input-url")) {
                    if (accountRow.find(".account-input-login").is(":visible")) {
                        jqThis = accountRow.find(".account-input-login").focus();
                    } else {
                        jqThis = accountRow.find(".account-input-pass").focus();
                    }
                }
                if (!jqThis.val().trim().length) {
                    return;
                }
                if (jqThis.is(".account-input-login")) {
                    jqThis = accountRow.find(".account-input-pass").focus();
                }
                if (!jqThis.val().trim().length) {
                    return;
                }
                if (jqThis.is(".account-input-pass")) {
                    jqThis = accountRow.find(".account-input-folder").focus();
                }
                if (!jqThis.val().trim().length) {
                    return;
                }
                accountRow.find(".account-save-link").click();
            } else if (code == ASC.Files.Common.keyCode.esc) {
                accountRow.find(".account-cancel-link").click();
            }
        });

        ASC.Files.ThirdParty.init();
    });
})(jQuery);
