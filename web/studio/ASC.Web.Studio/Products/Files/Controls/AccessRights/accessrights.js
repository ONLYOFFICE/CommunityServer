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


window.ASC.Files.Share = (function () {
    var isInit = false;
    var objectID;
    var objectTitle;
    var encrypted;
    var sharingInfo = [];
    var linkInfo;
    var sharingManager = null;
    var shareLink;
    var needUpdate = false;
    var originForPost = "*";

    var clip = null;
    var clipEmbed = null;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetSharedInfo, onGetSharedInfo);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetSharedInfoShort, onGetSharedInfoShort);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SetAceObject, onSetAceObject);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SetAceLink, onSetAceLink);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.UnSubscribeMe, onUnSubscribeMe);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetShortenLink, onGetShortenLink);

            sharingManager = new SharingSettingsManager(undefined, null);
            sharingManager.OnSave = setAceObject;
            sharingManager.OnChange = onChangeAce;
            sharingManager.OnCopyLink = function () {
                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.Resource.LinkCopySuccess);
            };

            jq(".embedded-size-item").on("click", setEmbeddedSize);
            jq(".embedded-size-custom").on("change", setEmbeddedSize);

            jq.dropdownToggle(
                {
                    switcherSelector: "#toggleEmbeddPanel",
                    dropdownID: "shareEmbeddedPanel",
                    inPopup: true,
                    rightPos: true,
                    addTop: 10,
                    afterShowFunction: function () {
                        jq("#shareEmbedded").focus().select();
                    }
                });

            jq(".sharing-empty").html(jq.format(ASC.Files.FilesJSResources.SharingSettingsEmpty, "<br />"));
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
            case ASC.Files.Constants.AceStatusEnum.CustomFilter:
                return ASC.Files.FilesJSResources.AceStatusEnum_CustomFilter;
            case ASC.Files.Constants.AceStatusEnum.Restrict:
                return ASC.Files.FilesJSResources.AceStatusEnum_Restrict;
            case ASC.Files.Constants.AceStatusEnum.Varies:
                return ASC.Files.FilesJSResources.AceStatusEnum_Varies;
            case ASC.Files.Constants.AceStatusEnum.Review:
                return ASC.Files.FilesJSResources.AceStatusEnum_Review;
            case ASC.Files.Constants.AceStatusEnum.FillForms:
                return ASC.Files.FilesJSResources.AceStatusEnum_FillForms;
            case ASC.Files.Constants.AceStatusEnum.Comment:
                return ASC.Files.FilesJSResources.AceStatusEnum_Comment;
            default:
                return "";
        }
    };

    var updateClip = function () {
        if (!ASC.Clipboard.enable) {
            jq("#shareLinkCopy, #shareEmbedCopy").remove();
            return;
        }

        ASC.Files.Share.clip = ASC.Clipboard.destroy(ASC.Files.Share.clip);

        ASC.Files.Share.clip = ASC.Clipboard.create(jq("#shareLink").val(), "shareLinkCopy", {
            onComplete: function () {
                if (typeof(window.toastr) !== "undefined") {
                    toastr.success(ASC.Resources.Master.Resource.LinkCopySuccess);
                } else {
                    jq("#shareLink, #shareLinkCopy").yellowFade();
                }
            },
            textareaId: "shareLink",
        });

        ASC.Files.Share.clipEmbed = ASC.Clipboard.destroy(ASC.Files.Share.clipEmbed);

        ASC.Files.Share.clipEmbed = ASC.Clipboard.create(jq("#shareEmbedded").val(), "shareEmbedCopy", {
            onComplete: function () {
                if (typeof(window.toastr) !== "undefined") {
                    toastr.success(ASC.Files.FilesJSResources.CodeCopySuccess);
                } else {
                    jq("#shareEmbedded, #shareEmbedCopy").yellowFade();
                }
            },
            textareaId: "shareEmbedded",
        });
    };

    var updateSocialLink = function (url) {
        var linkPanel = jq("#shareViaSocPanel");
        var link = encodeURIComponent(url);

        if (!!ASC.Resources.Master.UrlShareFacebook) {
            linkPanel.find(".facebook").attr("href", ASC.Resources.Master.UrlShareFacebook.format(link, encodeURIComponent(objectTitle), "", ""));
        } else {
            linkPanel.find(".facebook").remove();
        }
        if (!!ASC.Resources.Master.UrlShareTwitter) {
            linkPanel.find(".twitter").attr("href", ASC.Resources.Master.UrlShareTwitter.format(link));
        } else {
            linkPanel.find(".twitter").remove();
        }

        var urlShareMail = "mailto:?subject={1}&body={0}";
        var subject = ASC.Files.FilesJSResources.shareLinkMailSubject.format(objectTitle);
        var body = ASC.Files.FilesJSResources.shareLinkMailBody.format(objectTitle, url);
        linkPanel.find(".mail").attr("href", urlShareMail.format(encodeURIComponent(body), encodeURIComponent(subject)));
    };

    var openShareLinkAce = function () {
        var ace = jq("#shareLinkOpen").is(":checked")
            ? ASC.Files.Constants.AceStatusEnum.Read
            : ASC.Files.Constants.AceStatusEnum.Restrict;

        if (jq("#sharingLinkAce select").val() != ace) {
            jq("#sharingLinkAce select").val(ace).change();
        }
    };

    var changeShareLinkAce = function () {
        var url = shareLink;

        var ace = parseInt(jq("#sharingLinkAce select").val());
        if (linkInfo != ace) {
            needUpdate = true;
        }
        linkInfo = ace;

        var restrict = linkInfo == ASC.Files.Constants.AceStatusEnum.Restrict;
        if (jq("#shareLinkOpen").is(":checked") == restrict) {
            jq("#shareLinkOpen").prop("checked", !restrict);
        }

        if (restrict) {
            url = "";
        } else {
            jq("#getShortenLink").hide();

            if (linkInfo != ASC.Files.Constants.AceStatusEnum.Restrict && !isShortenLink()) {
                jq("#getShortenLink").show();
            }
        }

        jq("#shareLinkPanel, #sharingLinkAce").toggle(!restrict);
        jq("#shareLinkDescr").toggle(restrict);
        jq("#sharingSettingsItems").toggleClass("with-share-link", !restrict);

        jq("#shareLink").val(url);
        if (jq("#shareLink").is(":visible")) {
            jq("#shareLink").focus().select();
        }

        updateSocialLink(url);

        updateClip();

        if (needUpdate) {
            saveAccessLink();
        }
    };

    var renderGetLink = function () {
        jq("#shareLinkBody").show();

        if (jq("#studio_sharingSettingsDialog #shareLinkBody").length == 0) {
            jq("#sharingSettingsDialogBody").prepend(jq("#shareLinkBody"));

            jq("#shareLinkPanel").on("click", "#getShortenLink", getShortenLink);

            jq("#shareViaSocPanel").on("click", "a:not(.mail)", function () {
                window.open(jq(this).attr("href"), "new", "height=600,width=1020,fullscreen=0,resizable=0,status=0,toolbar=0,menubar=0,location=1");
                return false;
            });

            if (!ASC.Resources.Master.Personal) {
                jq("#shareViaSocPanel").on("click", "a.mail", function () {

                    var openLink = jq(this).attr("href");

                    var winMail = window.open(ASC.Desktop ? "" : ASC.Files.Constants.URL_LOADER);

                    var message = new ASC.Mail.Message();
                    message.subject = ASC.Files.FilesJSResources.shareLinkMailSubject.format(objectTitle);

                    var linkFormat = "<a href=\"{0}\">{1}</a>";
                    var linkName = linkFormat.format(Encoder.htmlEncode(shareLink), Encoder.htmlEncode(objectTitle));
                    var link = linkFormat.format(Encoder.htmlEncode(shareLink), Encoder.htmlEncode(shareLink));
                    var body = ASC.Files.FilesJSResources.shareLinkMailBody.format(linkName, link);

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
                });
            }

            jq("#shareLinkOpen").on("click", openShareLinkAce);
            jq("#sharingLinkAce select").on("change", changeShareLinkAce);
        }

        if (!ASC.Files.Utility.CanWebView(objectTitle)
            && (typeof ASC.Files.ImageViewer == "undefined" || !ASC.Files.Utility.CanImageView(objectTitle))) {
            jq("#shareLinkPanel").addClass("without-emb");
        } else {
            jq("#shareLinkPanel").removeClass("without-emb");
            setEmbeddedSize();
        }

        jq("#sharingLinkAce select").val(linkInfo).change();

        jq("#sharingLinkAce select [value=" + ASC.Files.Constants.AceStatusEnum.ReadWrite + "]").attr("disabled", !ASC.Files.Utility.CanWebEdit(objectTitle) || ASC.Files.Utility.MustConvert(objectTitle));
        jq("#sharingLinkAce select [value=" + ASC.Files.Constants.AceStatusEnum.CustomFilter + "]").attr("disabled", !ASC.Files.Utility.CanWebCustomFilterEditing(objectTitle));
        jq("#sharingLinkAce select [value=" + ASC.Files.Constants.AceStatusEnum.Review + "]").attr("disabled", !ASC.Files.Utility.CanWebReview(objectTitle));
        jq("#sharingLinkAce select [value=" + ASC.Files.Constants.AceStatusEnum.FillForms + "]").attr("disabled", !ASC.Files.Utility.CanWebRestrictedEditing(objectTitle));
        jq("#sharingLinkAce select [value=" + ASC.Files.Constants.AceStatusEnum.Comment + "]").attr("disabled", !ASC.Files.Utility.CanWebComment(objectTitle));
        jq("#sharingLinkAce select").tlcombobox();
    };

    var setEmbeddedSize = function () {
        var target = jq(this);
        if (target.is(".embedded-size-item")) {
            if (target.hasClass("embedded-size-6x8")) {
                jq("#embeddSizeWidth").val("600px");
                jq("#embeddSizeHeight").val("800px");
            } else if (target.hasClass("embedded-size-4x6")) {
                jq("#embeddSizeWidth").val("400px");
                jq("#embeddSizeHeight").val("600px");
            } else {
                jq("#embeddSizeWidth").val("100%");
                jq("#embeddSizeHeight").val("100%");
            }
        }

        var width = jq("#embeddSizeWidth").val();
        var height = jq("#embeddSizeHeight").val();

        width = (/\d+(px|%)?/gim.exec(width.toLowerCase().trim()) || ["100%"])[0];
        if (width == Math.abs(width)) {
            width += "px";
        }
        height = (/\d+(px|%)?/gim.exec(height.toLowerCase().trim()) || ["100%"])[0];
        if (height == Math.abs(height)) {
            height += "px";
        }

        jq("#embeddSizeWidth").val(width);
        jq("#embeddSizeHeight").val(height);

        jq(".embedded-size-item").removeClass("selected");
        if (width == "600px" && height == "800px") {
            jq(".embedded-size-6x8").addClass("selected");
        } else if (width == "400px" && height == "600px") {
            jq(".embedded-size-4x6").addClass("selected");
        } else if (width == "100%" && height == "100%") {
            jq(".embedded-size-1x1").addClass("selected");
        }

        generateEmbeddedString(width, height);
    };

    var generateEmbeddedString = function (width, height) {
        width = width || "100%";
        height = height || "100%";
        var url = shareLink;

        if (ASC.Files.Utility.CanWebView(objectTitle)) {
            var embeddedString = '<iframe src="{0}" width="{1}" height="{2}" frameborder="0" scrolling="no" allowtransparency></iframe>';
            url = url + "&action=embedded";

        } else if (typeof ASC.Files.ImageViewer != "undefined" && ASC.Files.Utility.CanImageView(objectTitle)) {
            embeddedString = '<img src="{0}" width="{1}" height="{2}" alt="" />';
        } else {
            return;
        }

        embeddedString = embeddedString.format(url, width, height);

        jq("#shareEmbedded").val(embeddedString).attr("title", embeddedString);
    };

    var unSubscribeMe = function (entryType, entryId) {
        if (ASC.Files.Folders.folderContainer != "forme"
            && ASC.Files.Folders.folderContainer != "privacy") {
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

        ASC.Files.UI.blockUI("#filesConfirmUnsubscribe", 420);

        PopupKeyUpActionProvider.EnterAction = "jq(\"#unsubscribeConfirmBtn\").click();";
    };

    var updateForParent = function () {
        if (needUpdate) {
            ASC.Files.Share.getSharedInfoShort();
        } else {
            onGetSharedInfoShort(null);
        }
    };

    var isShortenLink = function () {
        return !new RegExp("/Products/Files/", "i").test(shareLink);
    };

    //request

    var getSharedInfo = function (dataIds, objTitle, asFlat, rootFolderType, origin) {
        objectID = dataIds;
        encrypted = false;
        if (typeof objectID == "object" && objectID.length == 1) {
            objectID = objectID[0];
            if (!objTitle) {
                var itemId = ASC.Files.UI.parseItemId(objectID);
                objTitle = ASC.Files.UI.getEntryTitle(itemId.entryType, itemId.entryId);
            } else if (typeof objTitle == "object" && objTitle.length == 1) {
                objTitle = objTitle[0];
            }
        }
        if (typeof objectID == "string") {
            itemId = ASC.Files.UI.parseItemId(objectID);
            var entryObject = ASC.Files.UI.getEntryObject(itemId.entryType, itemId.entryId);
            var entryData = ASC.Files.UI.getObjectData(entryObject);
            if (entryData) {
                encrypted = entryData.encrypted;
            } else if (asFlat === true && ASC.Desktop && ASC.Desktop.setAccess) {
                encrypted = true;
            }
        }

        if (asFlat === true && origin) {
            originForPost = origin;
        }

        objectTitle = (typeof objectID == "object"
            ? ASC.Files.FilesJSResources.SharingSettingsCount.format(objectID.length)
            : objTitle);

        var canWebCustomFilterEditing = false;
        var canWebReview = false;
        var canWebRestrictedEditing = false;
        var canWebComment = false;
        if (typeof objectID != "object") {
            canWebCustomFilterEditing = ASC.Files.Utility.CanWebCustomFilterEditing(objectTitle);
            canWebReview = ASC.Files.Utility.CanWebReview(objectTitle);
            canWebRestrictedEditing = ASC.Files.Utility.CanWebRestrictedEditing(objectTitle);
            canWebComment = ASC.Files.Utility.CanWebComment(objectTitle);
        } else if (typeof objTitle == "object") {
            canWebCustomFilterEditing = true;
            canWebReview = true;
            canWebRestrictedEditing = true;
            canWebComment = true;
            jq(objTitle).each(function (i, title) {
                if (ASC.Files.UI.parseItemId(dataIds[i]).entryType == "folder") {
                    canWebCustomFilterEditing = canWebReview = canWebRestrictedEditing = canWebComment = false;
                    return false;
                }
                if (!ASC.Files.Utility.CanWebCustomFilterEditing(title)) {
                    canWebCustomFilterEditing = false;
                }
                if (!ASC.Files.Utility.CanWebReview(title)) {
                    canWebReview = false;
                }
                if (!ASC.Files.Utility.CanWebRestrictedEditing(title)) {
                    canWebRestrictedEditing = false;
                }
                if (!ASC.Files.Utility.CanWebComment(title)) {
                    canWebComment = false;
                }
                return canWebCustomFilterEditing || canWebReview || canWebRestrictedEditing || canWebComment;
            });
        }

        var data = {
            entry: jq(typeof objectID == "object" ? objectID : [objectID]).map(function (i, id) {
                return id;
            }).toArray()
        };

        ASC.Files.ServiceManager.getSharedInfo(ASC.Files.ServiceManager.events.GetSharedInfo,
            {
                showLoading: true,
                asFlat: asFlat === true,
                rootFolderType: rootFolderType,
                canWebCustomFilterEditing: canWebCustomFilterEditing,
                canWebReview: canWebReview,
                canWebRestrictedEditing: canWebRestrictedEditing,
                canWebComment: canWebComment,
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

    var onChangeAce = function (value) {
        if (value
            && (!ASC.Files.Folders
                || ASC.Files.Folders.folderContainer == "my"
                || ASC.Files.Folders.folderContainer == "forme"
                || ASC.Files.Folders.folderContainer == "privacy")) {
            jq("#studio_sharingSettingsDialog #shareMessagePanel").show();
            jq("#sharingSettingsItems").addClass("with-message-panel");
        }
    };

    var setAceObject = function (data) {
        var dataItems = data.items;
        var aceWrapperList = new Array();

        jq(dataItems).each(function (i, dataItem) {
            var change = true;
            jq(sharingInfo).each(function (j, dataItemOld) {
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

        var owner = jq("#shareOwnerSelector").attr("data-id");
        owner = (owner != jq("#shareOwnerSelector").attr("data-id-old")) ? owner : null;

        if (aceWrapperList.length) {
            needUpdate = true;
        } else {
            if (owner) {
                ASC.Files.Folders.changeOwner(typeof objectID == "object" ? objectID : [objectID], owner);
            }

            return;
        }

        var dataJson = {
            entries: { entry: objectID },
            aces: { entry: aceWrapperList },
            message: ""
        };

        var notify = jq("#shareMessageSend").prop("checked") == true;
        if (notify) {
            dataJson.message = (jq("#shareMessage:visible").val() || "").trim();
        }

        ASC.Files.ServiceManager.setAceObject(ASC.Files.ServiceManager.events.SetAceObject,
            {
                owner: owner,
                showLoading: true,
                notify: notify
            },
            { ace_collection: dataJson });

        return !encrypted;
    };

    var saveAccessLink = function () {
        ASC.Files.ServiceManager.setAceLink(ASC.Files.ServiceManager.events.SetAceLink,
            {
                showLoading: true,
                fileId: ASC.Files.UI.parseItemId(objectID).entryId,
                share: linkInfo,
            });
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
    };

    //event handler

    var onGetSharedInfo = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined" || typeof jsonData == "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }
        sharingInfo = jsonData;
        needUpdate = false;

        var ownerItem;
        var translateItems = [];
        var linkAccess;
        linkInfo = ASC.Files.Constants.AceStatusEnum.None;
        jq(jsonData).each(function (i) {
            var item = jsonData[i];
            if (item.id === ASC.Files.Constants.ShareLinkId) {
                shareLink = item.link;
                linkInfo = item.ace_status;
                linkAccess = item.ace_status;
                encrypted = false;
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
            if (item.owner) {
                ownerItem = item;
            }
        });

        var arrayActions = [
            {
                "id": ASC.Files.Constants.AceStatusEnum.ReadWrite,
                "name": getAceString(ASC.Files.Constants.AceStatusEnum.ReadWrite),
                "defaultAction": encrypted,
                "defaultStyle": "full",
            }];

        if (!encrypted) {
            if (params.canWebCustomFilterEditing) {
                arrayActions =
                    arrayActions.concat(
                        [{
                            "id": ASC.Files.Constants.AceStatusEnum.CustomFilter,
                            "name": getAceString(ASC.Files.Constants.AceStatusEnum.CustomFilter),
                            "defaultAction": false,
                            "defaultStyle": "customfilterEditing",
                        }]);
            }

            if (params.canWebReview) {
                arrayActions =
                    arrayActions.concat(
                        [{
                            "id": ASC.Files.Constants.AceStatusEnum.Review,
                            "name": getAceString(ASC.Files.Constants.AceStatusEnum.Review),
                            "defaultAction": false,
                            "defaultStyle": "review",
                        }]);
            }

            if (params.canWebRestrictedEditing) {
                arrayActions =
                    arrayActions.concat(
                        [{
                            "id": ASC.Files.Constants.AceStatusEnum.FillForms,
                            "name": getAceString(ASC.Files.Constants.AceStatusEnum.FillForms),
                            "defaultAction": false,
                            "defaultStyle": "restrictedEditing",
                        }]);
            }

            if (params.canWebComment) {
                arrayActions =
                    arrayActions.concat(
                        [{
                            "id": ASC.Files.Constants.AceStatusEnum.Comment,
                            "name": getAceString(ASC.Files.Constants.AceStatusEnum.Comment),
                            "defaultAction": false,
                            "defaultStyle": "comment",
                        }]);
            }

            arrayActions =
                arrayActions.concat(
                    [{
                        "id": ASC.Files.Constants.AceStatusEnum.Read,
                        "name": getAceString(ASC.Files.Constants.AceStatusEnum.Read),
                        "defaultAction": true,
                        "defaultStyle": "read",
                    }]);
        }

        arrayActions =
            arrayActions.concat(
                [
                    {
                        "id": ASC.Files.Constants.AceStatusEnum.Restrict,
                        "name": getAceString(ASC.Files.Constants.AceStatusEnum.Restrict),
                        "defaultAction": false,
                        "defaultStyle": "restrict",
                    },
                    {
                        "id": ASC.Files.Constants.AceStatusEnum.Varies,
                        "name": getAceString(ASC.Files.Constants.AceStatusEnum.Varies),
                        "defaultAction": false,
                        "disabled": true
                    }
                ]);

        var translateData = {
            "actions": arrayActions,
            "items": translateItems
        };

        sharingInfo = translateItems;

        var entryLink = null;
        if (typeof objectID != "object") {
            if (!encrypted) {
                var itemId = ASC.Files.UI.parseItemId(objectID);
                entryLink = ASC.Files.UI.getEntryLink(itemId.entryType, itemId.entryId, objectTitle);
                var aElement = document.createElement("a");
                aElement.href = entryLink;
                entryLink = jq(aElement).prop("href");
            } else {
                linkAccess = false;
            }
        }

        sharingManager.UpdateSharingData(translateData, entryLink);

        jq("#shareLinkBody").hide();
        var height = 460;
        if (linkAccess) {
            renderGetLink();

            height = 550;
            if (ASC.Resources.Master.Personal) {
                height = 260;
            }
        }

        sharingManager.ShowDialog(null, height, params.asFlat, encrypted);
        if (params.asFlat) {
            PopupKeyUpActionProvider.CloseDialogAction = "ASC.Files.Share.updateForParent();";

            PopupKeyUpActionProvider.ForceBinding = true;
        }

        if (ASC.Files.Actions) {
            ASC.Files.Actions.hideAllActionPanels();
        }

        updateClip();

        var shareHead = jq(".share-container-head");
        if (shareHead.is("span")) {
            shareHead.replaceWith("<div class=\"share-container-head\"></div>");
            shareHead = jq(".share-container-head");
            shareHead.html(ASC.Files.FilesJSResources.SharingSettingsHeader.format("<span></span>"));
        }

        var accessHead = jq(".share-container-head-corporate");

        if (params.rootFolderType) {
            jq("#studio_sharingSettingsDialog #shareMessagePanel").remove();
        } else {
            jq("#studio_sharingSettingsDialog #shareMessagePanel").hide();
        }
        jq("#sharingSettingsItems").removeClass("with-message-panel");

        if (params.rootFolderType || ASC.Files.Folders && ASC.Files.Folders.folderContainer == "corporate") {
            if (!accessHead.length) {
                shareHead.after("<div class=\"share-container-head-corporate\"></div>");
                accessHead = jq(".share-container-head-corporate");
                accessHead.html(ASC.Files.FilesJSResources.AccessSettingsHeader.format("<span></span>"));
            }
            accessHead.show()
                .find("span").attr("title", objectTitle).text(objectTitle);
            shareHead.hide();
        } else {
            shareHead.show()
                .find("span").attr("title", objectTitle).text(objectTitle);
            accessHead.hide();
        }

        if (itemId) {
            var entryObject = ASC.Files.UI.getEntryObject(itemId.entryType, itemId.entryId);
            var entryData = ASC.Files.UI.getObjectData(entryObject);
            var isThirdpartyOnly = ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty(entryData);
        }

        if (ASC.Files.Folders && ASC.Files.Folders.folderContainer == "corporate"
            && !(ASC.Files.ThirdParty && ASC.Files.ThirdParty.isThirdParty())
            && !isThirdpartyOnly
            && ownerItem) {
            var ownerLink = jq("#sharingSettingsItems .userLink[data-uid='" + ownerItem.id + "']");
            ownerLink.replaceWith("<div id=\"shareOwnerSelector\" class=\"advanced-selector-select\" data-id=\"\"><div class=\"change-owner-selector\"></div></div>");

            jq("#shareOwnerSelector")
                .useradvancedSelector({
                    inPopup: true,
                    itemsChoose: [],
                    itemsDisabledIds: [ownerItem.id],
                    canadd: false,
                    showGroups: true,
                    onechosen: true,
                    withGuests: false,
                })
                .on("showList", function (event, item) {
                    jq(this)
                        .attr("data-id", item.id)
                        .find(".change-owner-selector").html(item.title);

                    if (item.id != ownerItem.id) {
                        sharingManager.WhereChanges(true);
                    }
                    jq("#shareOwnerSelector").useradvancedSelector("reset");
                    jq("#shareOwnerSelector").useradvancedSelector("disable", [item.id]);
                })
                .attr("data-id", ownerItem.id)
                .attr("data-id-old", ownerItem.id)
                .find(".change-owner-selector").text(ownerItem.title);
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
        data.Referer = "onlyoffice";

        var message = JSON.stringify(data);
        window.parent.postMessage(message, originForPost);
    };

    var onSetAceObject = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        jq(typeof objectID == "object" ? objectID : [objectID]).each(function (i, entry) {
            itemId = ASC.Files.UI.parseItemId(entry);
            var entryObj = ASC.Files.UI.getEntryObject(itemId.entryType, itemId.entryId);

            entryObj.toggleClass("__active", jq.inArray(entry, jsonData) != -1);
        });

        if (params.owner) {
            ASC.Files.Folders.changeOwner(typeof objectID == "object" ? objectID : [objectID], params.owner);
        }

        if (encrypted && ASC.Desktop.setAccess) {
            if (LoadingBanner) {
                LoadingBanner.displayLoading();
            }

            var itemId = ASC.Files.UI.parseItemId(objectID);
            ASC.Files.UI.blockObject(objectID, true, ASC.Files.FilesJSResources.DescriptCreate);

            ASC.Desktop.setAccess(itemId.entryId, function (encryptedFile) {
                if (encryptedFile) {
                    ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.DesktopMessageStoring);

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

                sharingManager.CloseDialog();
            });
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
            ASC.Files.Tree.reloadFolder(params.parentFolderID);
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

        var ace = parseInt(jq("#sharingLinkAce select").val());
        if (ace != ASC.Files.Constants.AceStatusEnum.Restrict) {
            shareLink = jsonData;
        }

        updateSocialLink(jsonData);
        updateClip();
    };

    var onSetAceLink = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var itemId = ASC.Files.UI.parseItemId(objectID);
        var entryObj = ASC.Files.UI.getEntryObject(itemId.entryType, itemId.entryId);

        entryObj.toggleClass("__active", jsonData);
    };

    return {
        init: init,

        updateForParent: updateForParent,
        getSharedInfo: getSharedInfo,
        getSharedInfoShort: getSharedInfoShort,

        setAceObject: setAceObject,
        unSubscribeMe: unSubscribeMe,
        confirmUnSubscribe: confirmUnSubscribe,

        clip: clip,
        clipEmbed: clipEmbed,
    };
})();

jq(document).ready(function () {
    (function ($) {
        if (jq("#shareLinkBody").length == 0)
            return;

        ASC.Files.Share.init();
        $(function () {

            jq("#studioPageContent").on("click", "#buttonUnsubscribe, #mainUnsubscribe.unlockAction", function () {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Share.unSubscribeMe();
            });

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

            jq("#unsubscribeConfirmBtn").click(function () {
                ASC.Files.Share.confirmUnSubscribe();
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

            jq("#studio_sharingSettingsDialog .containerBodyBlock").addClass("clearFix");

            jq("#shareLink, #shareEmbedded").on("mousedown", function () {
                jq(this).select();
                return false;
            });

            jq("#shareLink, #shareEmbedded").on("keypress", function (e) {
                return e.ctrlKey && (e.charCode === ASC.Files.Common.keyCode.C || e.keyCode === ASC.Files.Common.keyCode.insertKey);
            });
        });
    })(jQuery);
});