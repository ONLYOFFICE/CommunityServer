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


window.ASC.Files.Share = (function () {
    var isInit = false;
    var objectID;
    var objectTitle;
    var sharingInfo = [];
    var linkInfo;
    var sharingManager = null;
    var shareLink;
    var shortenLink = "";
    var needUpdate = false;

    var clip = null;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetSharedInfo, onGetSharedInfo);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetSharedInfoShort, onGetSharedInfoShort);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SetAceObject, onSetAceObject);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.UnSubscribeMe, onUnSubscribeMe);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetShortenLink, onGetShortenLink);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SendLinkToEmail, onSendLinkToEmail);

            sharingManager = new SharingSettingsManager(undefined, null);
            sharingManager.OnSave = setAceObject;
        }
    };

    var getAceString = function (aceStatus) {
        if (aceStatus == "owner") {
            return ASC.Files.FilesJSResources.AceStatusEnum_Owner;
        }
        aceStatus = parseInt(aceStatus);
        switch (aceStatus) {
            case ASC.Files.Constants.AceStatusEnum.Read:
                return ASC.Files.FilesJSResources.AceStatusEnum_Read;
            case ASC.Files.Constants.AceStatusEnum.ReadWrite:
                return ASC.Files.FilesJSResources.AceStatusEnum_ReadWrite;
            case ASC.Files.Constants.AceStatusEnum.Restrict:
                return ASC.Files.FilesJSResources.AceStatusEnum_Restrict;
            case ASC.Files.Constants.AceStatusEnum.Varies:
                return ASC.Files.FilesJSResources.AceStatusEnum_Varies;
            default:
                return "";
        }
    };

    var updateClip = function () {
        if (jq.browser.mobile) {
            return;
        }

        if (ASC.Files.Share.clip) {
            ASC.Files.Share.clip.destroy();
        }

        if (!jq.browser.flashEnabled()) {
            jq("#shareLinkCopy, #embeddedCopy").remove();
            return;
        }

        if (jq("#shareLinkCopy:visible").length != 0) {
            var linkButton = "shareLinkCopy";
            var url = jq("#shareLink").val();
        } else if (jq("#embeddedCopy:visible").length != 0) {
            linkButton = "embeddedCopy";
            url = jq("#shareEmbedded").val();
        } else {
            return;
        }

        var offsetLink = jq("#" + linkButton).offset();
        var offsetDiff = jq(".blockUI #studio_sharingSettingsDialog").offset() || { left: 0, top: 0 };

        if (typeof ZeroClipboard != 'undefined' && ZeroClipboard.moviePath === 'ZeroClipboard.swf') {
            ZeroClipboard.setMoviePath(ASC.Resources.Master.ZeroClipboardMoviePath);
        }

        ASC.Files.Share.clip = new ZeroClipboard.Client();
        ASC.Files.Share.clip.setText(url);
        ASC.Files.Share.clip.glue(linkButton, linkButton,
            {
                zIndex: 670,
                left: offsetLink.left - offsetDiff.left + "px",
                top: offsetLink.top - offsetDiff.top + "px"
            });
        ASC.Files.Share.clip.addEventListener("onComplete", function () {
            if (typeof (window.toastr) !== "undefined") {
                toastr.success(ASC.Resources.Master.Resource.LinkCopySuccess);
            } else {
                jq("#shareEmbedded, #embeddedCopy, #shareLink, #shareLinkCopy").yellowFade();
            }
        });
    };

    var updateSocialLink = function (url) {
        var linkPanel = jq("#shareViaSocPanel");
        var link = encodeURIComponent(url);

        linkPanel.find(".google").attr("href", ASC.Resources.Master.UrlShareGooglePlus.format(link));
        linkPanel.find(".facebook").attr("href", ASC.Resources.Master.UrlShareFacebook.format(link, objectTitle, "", ""));
        linkPanel.find(".twitter").attr("href", ASC.Resources.Master.UrlShareTwitter.format(link));
    };

    var addShareMail = function () {
        var tmp = jq(".recipient-mail:first").clone(false);
        tmp.find(".textEdit").val("");
        jq(".recipient-mail:last").after(tmp);
        jq(".share-mail-list").scrollTo(".recipient-mail:last");

        jq(".recipient-mail:not(:last) .textEdit").each(function (i, e) {
            e = jq(e);
            if (e.val().trim() == "") {
                jq(e).closest(".recipient-mail").remove();
            }
        });

        jq(".recipient-mail-add").hide();
        jq(".recipient-mail-add:last").show();

        if (jq(".recipient-mail").length >= 30) {
            jq(".recipient-mail-add").hide();
        }
    };

    var deleteShareMail = function (obj) {
        var target = obj.target || obj.srcElement;
        var parent = jq(target).parent();
        if (jq(".recipient-mail").length > 1) {
            parent.remove();
        } else {
            parent.find(".textEdit").val("");
        }
        jq(".recipient-mail-add:last").show();
    };

    var showEmbeddedPanel = function () {
        jq("#studio_sharingSettingsDialog").removeClass("outside-panel");
        jq("#studio_sharingSettingsDialog").addClass("embedded-panel");
        jq("#shareSidePanel .active").removeClass("active");
        jq("#shareSideEmbedded").addClass("active");
        jq("#shareMailPanel").hide();

        jq("#sharingAcePanel").hide();

        var embeddedAccess =
            (linkInfo === ASC.Files.Constants.AceStatusEnum.Restrict
                ? ASC.Files.Constants.AceStatusEnum.Restrict
                : ASC.Files.Constants.AceStatusEnum.Read);
        jq(".link-share-opt[value='" + embeddedAccess + "']").prop("checked", true);

        jq(".share-link-label").removeClass("checked");
        if (jq(".link-share-opt[value='" + ASC.Files.Constants.AceStatusEnum.Restrict + "']").is(":checked")) {
            jq("#chareLinkClose").addClass("checked");
        } else {
            jq("#chareLinkOpen").addClass("checked");
        }

        updateClip();
    };

    var showOutsidePanel = function () {
        jq("#studio_sharingSettingsDialog").removeClass("embedded-panel");
        jq("#studio_sharingSettingsDialog").addClass("outside-panel");
        jq("#shareSidePanel .active").removeClass("active");
        jq("#shareSideOutside").addClass("active");

        if (ASC.Files.Utility.CanWebEdit(objectTitle, true) && ASC.Resources.Master.TenantTariffDocsEdition
            && !ASC.Files.Utility.MustConvert(objectTitle)) {
            jq("#sharingAcePanel").show();
        }

        jq(".link-share-opt[value='" + linkInfo + "']").prop("checked", true);
        jq(".share-link-label").removeClass("checked");
        if (jq(".link-share-opt[value='" + ASC.Files.Constants.AceStatusEnum.Restrict + "']").is(":checked")) {
            jq("#chareLinkClose").addClass("checked");
        } else {
            jq("#chareLinkOpen").addClass("checked");
        }
        updateClip();
    };

    var showMainPanel = function () {
        jq("#studio_sharingSettingsDialog").removeClass("outside-panel embedded-panel");
        jq("#shareSidePanel .active").removeClass("active");
        jq("#shareSidePortal").addClass("active");
    };

    var showShareLink = function (ace) {
        jq("#studio_sharingSettingsDialog").removeClass("deny-access");
        jq("#shareMailPanel").hide();
        jq("#getShortenLink").hide();
        jq("#sharingLinkDeny").hide();

        switch (ace) {
            case ASC.Files.Constants.AceStatusEnum.Read:
            case ASC.Files.Constants.AceStatusEnum.ReadWrite:
                if (shortenLink == "") {
                    jq("#getShortenLink").show();
                }
                break;
        }
    };

    var openShareLinkAce = function (event) {
        jq(".link-share-opt[value='" + ASC.Files.Constants.AceStatusEnum.Read + "']").prop("checked", true);
        changeShareLinkAce(event);
    };

    var changeShareLinkAce = function (event) {
        if (event) {
            needUpdate = true;
        }
        var url = shareLink;

        if (shortenLink != "") {
            url = shortenLink;
        }

        var ace = parseInt(jq(".link-share-opt:checked").val());
        jq(".share-link-label").removeClass("checked");
        if (jq(".link-share-opt[value='" + ASC.Files.Constants.AceStatusEnum.Restrict + "']").is(":checked")) {
            jq("#chareLinkClose").addClass("checked");
        } else {
            jq("#chareLinkOpen").addClass("checked");
        }
        switch (ace) {
            case ASC.Files.Constants.AceStatusEnum.Restrict:
                url = "";
                jq("#studio_sharingSettingsDialog").addClass("deny-access");
                jq("#sharingLinkDeny").show();
                break;
            case ASC.Files.Constants.AceStatusEnum.Read:
            case ASC.Files.Constants.AceStatusEnum.ReadWrite:
                showShareLink(ace);
                break;
        }

        jq("#shareLink").val(url);

        updateSocialLink(url);

        updateClip();
        linkInfo = ace;

        if (typeof event != "undefined") {
            saveShareLinkAccess();
        }
    };

    var renderGetLink = function (arrayActions) {
        jq("#studio_sharingSettingsDialog").addClass("with-link");

        if (jq("#studio_sharingSettingsDialog #shareSelectorBody").length == 0) {
            jq("#shareSelectorBody").prependTo("#studio_sharingSettingsDialog .containerBodyBlock");

            if (jq.browser.mobile) {
                jq("#shareLinkCopy").remove();
                jq("#shareLink").attr("readonly", "false").attr("readonly", "").removeAttr("readonly");
            } else {
                jq("#shareLinkBody").on("mousedown", "#shareLink, #shareEmbedded", function () {
                    jq(this).select();
                    return false;
                });
            }
            jq("#shareLinkPanel").on(jq.browser.webkit ? "keydown" : "keypress", "#shareLink", function (e) {
                return e.ctrlKey && (e.charCode === 99 || e.keyCode === ASC.Files.Common.keyCode.insertKey);
            });

            jq("#shareSideEmbedded").on("click", showEmbeddedPanel);
            jq("#shareSideOutside").on("click", showOutsidePanel);
            jq("#shareSidePortal").on("click", showMainPanel);
            jq("#shareMailPanel").on("click", ".recipient-mail-remove", deleteShareMail);
            jq("#shareLinkPanel").on("click", ".recipient-mail-add", addShareMail);
            jq("#shareLinkPanel").on("click", "#getShortenLink", getShortenLink);
            jq("#shareLinkPanel").on("click", "#shareSendLinkToEmail", sendLinkToEmail);
            jq("#shareEmbeddedPanel").on("click", ".embedded-size-item", setEmbeddedSize);
            jq("#shareEmbeddedPanel").on("change", ".embedded-size-custom input", setEmbeddedSize);

            jq("#shareViaSocPanel").on("click", "a", function () {
                if (jq(this).is(".mail")) {
                    jq("#shareMailPanel").toggle();
                } else {
                    window.open(jq(this).attr("href"), "new", "height=600,width=1020,fullscreen=0,resizable=0,status=0,toolbar=0,menubar=0,location=1");
                }
                return false;
            });

            jq(".sharing-link-items").on("click", "#chareLinkOpen:not(.checked)", openShareLinkAce);
            jq(".link-share-opt").on("change", changeShareLinkAce);
            jq("#sharingSettingsItems").on("scroll", updateClip);
        }

        jq(".link-share-opt[value='" + linkInfo + "']").prop("checked", true);
        jq(".share-link-label").removeClass("checked");
        if (jq(".link-share-opt[value='" + ASC.Files.Constants.AceStatusEnum.Restrict + "']").is(":checked")) {
            jq("#chareLinkClose").addClass("checked");
        } else {
            jq("#chareLinkOpen").addClass("checked");
        }

        updateClip();

        if (ASC.Files.Utility.CanWebEdit(objectTitle, true) && ASC.Resources.Master.TenantTariffDocsEdition
            && !ASC.Files.Utility.MustConvert(objectTitle)) {
            jq("#sharingAcePanel").show();
        } else {
            jq("#sharingAcePanel").hide();
        }

        shortenLink = "";

        changeShareLinkAce();

        jq("#shareMailText").val("");
        jq(".recipient-mail-remove").click();
    };

    var renderEmbeddedPanel = function () {
        if (ASC.Files.Utility.CanWebView(objectTitle) && ASC.Resources.Master.TenantTariffDocsEdition) {
            jq("#shareSideEmbedded").show();
            setEmbeddedSize();
        } else {
            jq("#shareSideEmbedded").hide();
            showMainPanel();
        }
    };

    var setEmbeddedSize = function () {
        jq(".embedded-size-item").removeClass("selected");
        var target = jq(this);
        if (target.is(".embedded-size-custom input")) {
            var heightTmp = jq("#embeddedSizeTemplate .embedded-size-custom input[name='height']").val();
            heightTmp = Math.abs(heightTmp) || 0;
            if (heightTmp) {
                var height = heightTmp + "px";
            } else {
                jq("#embeddedSizeTemplate .embedded-size-custom input[name='height']").val("");
            }
            var widthTmp = jq("#embeddedSizeTemplate .embedded-size-custom input[name='width']").val();
            widthTmp = Math.abs(widthTmp) || 0;
            if (widthTmp) {
                var width = widthTmp + "px";
            } else {
                jq("#embeddedSizeTemplate .embedded-size-custom input[name='width']").val("");
            }
        } else if (target.is(".embedded-size-item")) {
            target.addClass("selected");
            jq("#embeddedSizeTemplate .embedded-size-custom input").val("");
            if (target.hasClass("embedded-size-8x6")) {
                height = "800px";
                width = "600px";
            } else if (target.hasClass("embedded-size-6x4")) {
                height = "600px";
                width = "400px";
            }
        } else {
            jq(".embedded-size-item:first").addClass("selected");
        }

        generateEmbeddedString(height, width);
    };

    var generateEmbeddedString = function (height, width) {
        height = height || "100%";
        width = width || "100%";
        var embeddedString = '<iframe src="{0}" height="{1}" width="{2}" frameborder="0" scrolling="no" allowtransparency></iframe>';

        var url = shareLink + "&action=embedded";

        embeddedString = embeddedString.format(url, height, width);

        jq("#shareEmbedded").val(embeddedString);
        updateClip();
    };

    var unSubscribeMe = function (entryType, entryId) {
        if (ASC.Files.Folders.folderContainer != "forme") {
            return;
        }
        var list = new Array();

        var textFolder = "";
        var textFile = "";
        var strHtml = "<label title=\"{0}\"><input type=\"checkbox\" class=\"checkbox\" entryType=\"{1}\" entryId=\"{2}\" checked=\"checked\">{0}</label>";

        if (entryType && entryId) {
            list.push({ entryType: entryType, entryId: entryId });

            var entryRowTitle = ASC.Files.UI.getEntryTitle(entryType, entryId);

            if (entryType == "file") {
                textFile += strHtml.format(entryRowTitle, entryType, entryId);
            } else {
                textFolder += strHtml.format(entryRowTitle, entryType, entryId);
            }
        } else {
            jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):has(.checkbox input:checked)").each(function () {
                var entryRowData = ASC.Files.UI.getObjectData(this);
                var entryRowType = entryRowData.entryType;
                var entryRowId = entryRowData.entryId;

                list.push({ entryType: entryRowType, entryId: entryRowId });

                entryRowTitle = entryRowData.title;
                if (entryRowType == "file") {
                    textFile += strHtml.format(entryRowTitle, entryRowType, entryRowId);
                } else {
                    textFolder += strHtml.format(entryRowTitle, entryRowType, entryRowId);
                }
            });
        }

        if (list.length == 0) {
            return;
        }

        jq("#confirmUnsubscribeList dd.confirm-remove-files").html(textFile);
        jq("#confirmUnsubscribeList dd.confirm-remove-folders").html(textFolder);

        jq("#confirmUnsubscribeList .confirm-remove-folders, #confirmUnsubscribeList .confirm-remove-files").show();
        if (textFolder == "") {
            jq("#confirmUnsubscribeList .confirm-remove-folders").hide();
        }
        if (textFile == "") {
            jq("#confirmUnsubscribeList .confirm-remove-files").hide();
        }

        ASC.Files.UI.blockUI("#filesConfirmUnsubscribe", 420, 0, -150);

        PopupKeyUpActionProvider.EnterAction = "jq(\"#unsubscribeConfirmBtn\").click();";
    };

    var updateForParent = function () {
        if (needUpdate) {
            ASC.Files.Share.getSharedInfoShort();
        } else {
            onGetSharedInfoShort(null);
        }
    };

    //request

    var getSharedInfo = function (dataIds, objTitle, asFlat, rootFolderType) {
        objectID = dataIds;
        if (typeof objectID == "object" && objectID.length == 1) {
            objectID = objectID[0];
            if (!objTitle) {
                var itemId = ASC.Files.UI.parseItemId(objectID);
                objTitle = ASC.Files.UI.getEntryTitle(itemId.entryType, itemId.entryId);
            }
        }

        objectTitle = (typeof objectID == "object"
            ? ASC.Files.FilesJSResources.SharingSettingsCount.format(objectID.length)
            : objTitle);

        Encoder.EncodeType = "!entity";
        var data = {
            entry: jq(typeof objectID == "object" ? objectID : [objectID]).map(function (i, id) {
                return Encoder.htmlEncode(id);
            }).toArray()
        };
        Encoder.EncodeType = "entity";

        ASC.Files.ServiceManager.getSharedInfo(ASC.Files.ServiceManager.events.GetSharedInfo,
            {
                showLoading: true,
                asFlat: asFlat === true,
                rootFolderType: rootFolderType
            },
            { stringList: data });
    };

    var getSharedInfoShort = function () {
        ASC.Files.ServiceManager.getSharedInfoShort(ASC.Files.ServiceManager.events.GetSharedInfoShort,
            {
                showLoading: true,
                objectID: objectID
            });
    };

    var setAceObject = function (data) {
        var dataItems = data.items;
        var aceWrapperList = new Array();

        jq(dataItems).each(function (i, dataItem) {
            var change = true;
            jq(sharingInfo).each(function (j, dataItemOld){
                if (dataItemOld.id === dataItem.id) {
                    change = dataItemOld.selectedAction.id != dataItem.selectedAction.id;
                    return false;
                }
                return true;
            });

            if (change) {
                aceWrapperList.push(
                    {
                        id: dataItem.id,
                        is_group: dataItem.isGroup,
                        ace_status: dataItem.selectedAction.id
                    });
            }
        });

        //remove
        jq(sharingInfo).each(function (j, dataItemOld) {
            var change = true;
            jq(dataItems).each(function (i, dataItem) {
                if (dataItemOld.id === dataItem.id) {
                    change = false;
                    return false;
                }
                return true;
            });
            if (change) {
                aceWrapperList.push(
                    {
                        id: dataItemOld.id,
                        is_group: dataItemOld.isGroup,
                        ace_status: ASC.Files.Constants.AceStatusEnum.None
                    });
            }
        });

        if (aceWrapperList.length) {
            needUpdate = true;
        } else {
            return;
        }

        Encoder.EncodeType = "!entity";
        var dataJson = {
            entries: {
                entry: jq(typeof objectID == "object" ? objectID : [objectID]).map(function (i, id) {
                    return Encoder.htmlEncode(id);
                }).toArray()
            },
            aces: { entry: aceWrapperList },
            message: ""
        };
        Encoder.EncodeType = "entity";

        var notify = jq("#shareMessageSend").prop("checked") == true;
        if (notify) {
            dataJson.message = Encoder.htmlEncode((jq("#shareMessage:visible").val() || "").trim());
        }

        var dataXml = ASC.Files.Common.jsonToXml({ ace_collection: dataJson });

        ASC.Files.ServiceManager.setAceObject(ASC.Files.ServiceManager.events.SetAceObject,
            {
                showLoading: true,
                notify: notify
            },
            dataXml);
    };

    var saveShareLinkAccess = function () {
        var aceWrapperList =
            [{
                id: ASC.Files.Constants.ShareLinkId,
                is_group: true,
                ace_status: linkInfo
            }];

        if (!aceWrapperList.length) {
            return;
        }

        Encoder.EncodeType = "!entity";
        var dataJson = {
            entries: { entry: Encoder.htmlEncode(objectID) },
            aces: { entry: aceWrapperList },
            message: ""
        };
        Encoder.EncodeType = "entity";
        var dataXml = ASC.Files.Common.jsonToXml({ ace_collection: dataJson });

        ASC.Files.ServiceManager.setAceObject(ASC.Files.ServiceManager.events.SetAceObject,
            {
                showLoading: true,
                clearData: false
            },
            dataXml);
    };

    var confirmUnSubscribe = function () {
        if (jq("#filesConfirmUnsubscribe:visible").length == 0) {
            return;
        }

        PopupKeyUpActionProvider.CloseDialog();

        var listChecked = jq("#confirmUnsubscribeList input:checked");
        if (listChecked.length == 0) {
            return;
        }

        var data = {};
        data.entry = new Array();

        var list =
            listChecked.map(function (i, item) {
                var entryConfirmType = jq(item).attr("entryType");
                var entryConfirmId = jq(item).attr("entryId");
                var entryConfirmObj = ASC.Files.UI.getEntryObject(entryConfirmType, entryConfirmId);
                ASC.Files.UI.blockObject(entryConfirmObj, true, ASC.Files.FilesJSResources.DescriptRemove, true);
                data.entry.push(entryConfirmType + "_" + entryConfirmId);
                return { entryId: entryConfirmId, entryType: entryConfirmType };
            }).toArray();
        ASC.Files.UI.updateMainContentHeader();

        ASC.Files.ServiceManager.unSubscribeMe(ASC.Files.ServiceManager.events.UnSubscribeMe,
            {
                parentFolderID: ASC.Files.Folders.currentFolder.id,
                showLoading: true,
                list: list
            },
            { stringList: data });
    };

    var getShortenLink = function () {
        var fileId = ASC.Files.UI.parseItemId(objectID).entryId;

        ASC.Files.ServiceManager.getShortenLink(ASC.Files.ServiceManager.events.GetShortenLink, { fileId: fileId });

        jq("#getShortenLink").hide();

        updateClip();
    };

    var sendLinkToEmail = function () {
        var fileId = ASC.Files.UI.parseItemId(objectID).entryId;
        var message = jq("#shareMailText").val().trim();

        var stringList = new Array();

        jq(".recipient-mail .textEdit").each(function () {
            var mail = jq(this).val();
            if (mail) {
                mail = mail.trim();
                if (jq.isValidEmail(mail)) {
                    stringList.push(mail);
                }
            }
        });

        if (!stringList.length) {
            jq(".recipient-mail .textEdit:first").focus();
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.ErrorMassage_EmptyField, true);
            return;
        }

        Encoder.EncodeType = "!entity";
        var dataResult =
            {
                address: { entry: stringList },
                message: Encoder.htmlEncode(message)
            };
        Encoder.EncodeType = "entity";

        var dataXml = ASC.Files.Common.jsonToXml({ message_data: dataResult });

        ASC.Files.ServiceManager.sendLinkToEmail(ASC.Files.ServiceManager.events.SendLinkToEmail,
            { fileId: fileId },
            dataXml);
    };

    //event handler

    var onGetSharedInfo = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined" || typeof jsonData == "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }
        sharingInfo = jsonData;

        var translateItems = [];
        var linkAccess;
        linkInfo = ASC.Files.Constants.AceStatusEnum.None;
        jq(jsonData).each(function (i) {
            var item = jsonData[i];
            if (item.id === ASC.Files.Constants.ShareLinkId) {
                shareLink = item.title;
                linkInfo = item.ace_status;
                linkAccess = item.ace_status;
            } else {
                translateItems.push(
                    {
                        "id": item.id,
                        "name": item.title,
                        "isGroup": item.is_group,
                        "canEdit": !(item.locked || item.owner),
                        "hideRemove": item.disable_remove,
                        "selectedAction": {
                            "id": item.ace_status,
                            "name": getAceString(item.owner ? "owner" : item.ace_status),
                            "defaultAction": false
                        }
                    });
            }
        });
        var arrayActions = [
            {
                "id": ASC.Files.Constants.AceStatusEnum.Read,
                "name": getAceString(ASC.Files.Constants.AceStatusEnum.Read),
                "defaultAction": true
            },
            {
                "id": ASC.Files.Constants.AceStatusEnum.ReadWrite,
                "name": getAceString(ASC.Files.Constants.AceStatusEnum.ReadWrite),
                "defaultAction": false
            },
            {
                "id": ASC.Files.Constants.AceStatusEnum.Restrict,
                "name": getAceString(ASC.Files.Constants.AceStatusEnum.Restrict),
                "defaultAction": false
            },
            {
                "id": ASC.Files.Constants.AceStatusEnum.Varies,
                "name": getAceString(ASC.Files.Constants.AceStatusEnum.Varies),
                "defaultAction": false,
                "disabled": true
            }
        ];

        var translateData = {
            "actions": arrayActions,
            "items": translateItems
        };

        sharingInfo = translateItems;

        sharingManager.UpdateSharingData(translateData);

        jq("#studio_sharingSettingsDialog").removeClass("with-link");
        var width = 600;
        if (linkAccess) {
            renderGetLink(arrayActions);

            renderEmbeddedPanel();

            showMainPanel();

            width = 830;
            if (ASC.Resources.Master.Personal) {
                showOutsidePanel();
            }
        }

        sharingManager.ShowDialog(width, params.asFlat);
        PopupKeyUpActionProvider.EnterAction = "jq('#shareSendLinkToEmail:visible').click();";
        if (params.asFlat) {
            PopupKeyUpActionProvider.CloseDialogAction = "ASC.Files.Share.updateForParent();";
        }

        updateClip();

        var shareHead = jq(".share-container-head");
        if (shareHead.is("span")) {
            shareHead.replaceWith("<div class=\"share-container-head\"></div>");
            shareHead = jq(".share-container-head");
            shareHead.html(ASC.Files.FilesJSResources.SharingSettingsTitle.format("(<span></span>)"));
        }

        var accessHead = jq(".share-container-head-corporate");

        if (params.rootFolderType || ASC.Files.Folders && ASC.Files.Folders.folderContainer == "corporate") {
            jq("#studio_sharingSettingsDialog #shareMessagePanel").hide();
            if (!accessHead.length) {
                shareHead.after("<div class=\"share-container-head-corporate\"></div>");
                accessHead = jq(".share-container-head-corporate");
                accessHead.html(ASC.Files.FilesJSResources.AccessSettingsTitle.format("(<span></span>)"));
            }
            accessHead.show()
                .find("span").attr("title", objectTitle).text(objectTitle);
            shareHead.hide();
        } else {
            jq("#studio_sharingSettingsDialog #shareMessagePanel").show();
            shareHead.show()
                .find("span").attr("title", objectTitle).text(objectTitle);
            accessHead.hide();
        }
    };

    var onGetSharedInfoShort = function (jsonData, params, errorMessage) {
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

        var message = JSON.stringify(data);
        window.parent.postMessage(message, "*");
    };

    var onSetAceObject = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        jq(typeof objectID == "object" ? objectID : [objectID]).each(function (i, entry) {
            var itemId = ASC.Files.UI.parseItemId(entry);
            var entryObj = ASC.Files.UI.getEntryObject(itemId.entryType, itemId.entryId);

            entryObj.toggleClass("__active", jq.inArray(entry, jsonData) != -1);
        });

        if (params.clearData) {
            objectID = null;
        }
    };

    var onUnSubscribeMe = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var list = params.list;
        var foldersCountChange = false;

        jq(list).each(function (i, item) {
            if (!foldersCountChange && item.entryType == "folder") {
                foldersCountChange = true;
            }

            ASC.Files.Marker.removeNewIcon(item.entryType, item.entryId);
            ASC.Files.UI.getEntryObject(item.entryType, item.entryId).remove();
        });

        if (foldersCountChange && ASC.Files.Tree) {
            ASC.Files.Tree.resetFolder(params.parentFolderID);
        }

        ASC.Files.UI.checkEmptyContent();
    };

    var onGetShortenLink = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }
        if (jsonData == null || jsonData == "") {
            return;
        }

        jq("#shareLink").val(jsonData);

        var ace = parseInt(jq(".link-share-opt:checked").val());
        switch (ace) {
            case ASC.Files.Constants.AceStatusEnum.Read:
            case ASC.Files.Constants.AceStatusEnum.ReadWrite:
                shortenLink = jsonData;
                break;
        }

        updateSocialLink(jsonData);
        updateClip();
    };

    var onSendLinkToEmail = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        sharingManager.SaveAndCloseDialog();
    };

    return {
        init: init,

        updateForParent: updateForParent,
        getSharedInfo: getSharedInfo,
        getSharedInfoShort: getSharedInfoShort,

        setAceObject: setAceObject,
        unSubscribeMe: unSubscribeMe,
        confirmUnSubscribe: confirmUnSubscribe,

        clip: clip
    };
})();

jq(document).ready(function () {
    (function ($) {
        ASC.Files.Share.init();
        $(function () {

            jq("#studioPageContent").on("click", "#buttonUnsubscribe, #mainUnsubscribe.unlockAction", function () {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Share.unSubscribeMe();
            });

            jq("#studioPageContent").on("click", "#buttonShare, #mainShare.unlockAction", function () {
                ASC.Files.Actions.hideAllActionPanels();

                var dataIds = new Array();
                jq("#filesMainContent .file-row:not(.without-share):not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)").each(function () {
                    var entryRowData = ASC.Files.UI.getObjectData(this);
                    var entryRowType = entryRowData.entryType;
                    var entryRowId = entryRowData.entryId;

                    dataIds.push(entryRowType + "_" + entryRowId);
                });

                ASC.Files.Share.getSharedInfo(dataIds);
            });

            jq("#unsubscribeConfirmBtn").click(function () {
                ASC.Files.Share.confirmUnSubscribe();
            });

            jq("#filesMainContent").on("click", ".btn-row", function () {
                var entryData = ASC.Files.UI.getObjectData(this);
                var entryId = entryData.entryId;
                var entryType = entryData.entryType;
                var entryTitle = entryData.title;
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Share.getSharedInfo(entryType + "_" + entryId, entryTitle);
                return false;
            });

            jq("#studio_sharingSettingsDialog .containerBodyBlock").addClass("clearFix");
        });
    })(jQuery);
});