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


;
window.ASC.Files.Editor = (function () {
    var isInit = false;

    var docIsChanged = false;
    var fixedVersion = false;
    var canShowHistory = false;

    var docEditor = null;
    var docServiceParams = null;

    var trackEditTimeout = null;
    var shareLinkParam = "";
    var docKeyForTrack = "";
    var tabId = "";
    var serverErrorMessage = null;
    var editByUrl = false;
    var canCreate = true;
    var thirdPartyApp = false;
    var options = null;
    var openinigDate;
    var doStartEdit = true;

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        jq("body").css("overflow-y", "hidden");

        window.onbeforeunload = ASC.Files.Editor.finishEdit;

        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.TrackEditFile, completeTrack);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.CanEditFile, completeCanEdit);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.StartEdit, onStartEdit);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetEditHistory, completeGetEditHistory);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetDiffUrl, completeGetDiffUrl);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetMails, completeGetMails);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.StartMailMerge, completeStartMailMerge);
    };

    var createFrameEditor = function (serviceParams) {

        jq("#iframeEditor").parents().css("height", "100%");

        var eventsConfig = {
            "onReady": ASC.Files.Editor.readyEditor,
        };

        if (serviceParams) {
            var embedded = (serviceParams.type == "embedded");

            var documentConfig = {
                title: serviceParams.file.title,
                url: serviceParams.url,
                fileType: serviceParams.fileType,
                key: serviceParams.key,
                vkey: serviceParams.vkey,
                options: ASC.Files.Editor.options,
                info: {
                    author: serviceParams.file.create_by,
                    created: serviceParams.file.create_on,
                },
                permissions: {
                    edit: serviceParams.canEdit,
                },
            };

            if (serviceParams.filePath.length) {
                documentConfig.info.folder = serviceParams.filePath;
            }
            if (serviceParams.sharingSettings.length) {
                documentConfig.info.sharingSettings = serviceParams.sharingSettings;
            }

            var editorConfig = {
                mode: serviceParams.mode,
                lang: serviceParams.lang,
                customization: {
                    logo: {
                        image: ASC.Files.Editor.brandingLogoUrl || "",
                        imageEmbedded: ASC.Files.Editor.brandingLogoEmbeddedUrl || "",
                        url: "",
                    },
                    customer: {
                        logo: ASC.Files.Editor.brandingCustomerLogo || "",
                        name: ASC.Files.Editor.brandingCustomer || "",
                    },
                },
            };

            if (ASC.Files.Editor.showAbout) {
                editorConfig.customization.about = true;
            }

            if (ASC.Files.Editor.feedbackUrl) {
                editorConfig.customization.feedback = {
                    url: ASC.Files.Editor.feedbackUrl,
                    visible: true,
                };
            }

            if (ASC.Files.Editor.licenseUrl) {
                editorConfig.licenseUrl = ASC.Files.Editor.licenseUrl;
            }

            if (ASC.Files.Editor.customerId) {
                editorConfig.customerId = ASC.Files.Editor.customerId;
            }

            if (serviceParams.folderUrl.length > 0) {
                editorConfig.customization.goback = {
                    url: serviceParams.folderUrl
                };

                //todo: delete
                eventsConfig.onBack = ASC.Files.Editor.backEditor;
            }

            if (embedded) {
                editorConfig.embedded = {
                    saveUrl: serviceParams.downloadUrl,
                    embedUrl: serviceParams.embeddedUrl,
                    shareUrl: serviceParams.viewerUrl,
                    toolbarDocked: "top"
                };

                var keyFullscreen = "fullscreen";
                if (location.hash.indexOf(keyFullscreen) < 0) {
                    editorConfig.embedded.fullscreenUrl = serviceParams.embeddedUrl + "#" + keyFullscreen;
                }
            } else {
                if (ASC.Files.Constants.URL_HANDLER_CREATE) {
                    editorConfig.createUrl = ASC.Files.Constants.URL_HANDLER_CREATE + "?action=create&doctype=" + serviceParams.documentType;

                    editorConfig.templates =
                        jq(serviceParams.templates).map(
                            function (i, item) {
                                return {
                                    name: item.Key,
                                    icon: item.Value,
                                    url: ASC.Files.Constants.URL_HANDLER_CREATE
                                        + "?action=create"
                                        + "&doctype=" + serviceParams.documentType
                                        + "&template=" + item.Key
                                };
                            }).toArray();

                    //todo: delete
                    eventsConfig.onCreateFile = ASC.Files.Editor.createFile;
                }

                if (serviceParams.sharingSettingsUrl) {
                    editorConfig.sharingSettingsUrl = serviceParams.sharingSettingsUrl;
                }

                if (serviceParams.fileChoiceUrl) {
                    editorConfig.fileChoiceUrl = serviceParams.fileChoiceUrl;
                }

                if (serviceParams.mergeFolderUrl) {
                    editorConfig.mergeFolderUrl = serviceParams.mergeFolderUrl;
                }

                editorConfig.user = {
                    id: serviceParams.user[0],
                    firstname: serviceParams.user[1],
                    lastname: serviceParams.user[2],
                    //todo: delete
                    name: serviceParams.user[3],
                };

                if (serviceParams.type != "embedded") {
                    var listRecent = getRecentList();
                    if (listRecent && listRecent.length) {
                        editorConfig.recent = listRecent.toArray();
                    }
                }
            }

            var typeConfig = serviceParams.type;
            var documentTypeConfig = serviceParams.documentType;

            eventsConfig.onDocumentStateChange = ASC.Files.Editor.documentStateChangeEditor;
            eventsConfig.onRequestEditRights = ASC.Files.Editor.requestEditRightsEditor;
            eventsConfig.onError = ASC.Files.Editor.errorEditor;
            eventsConfig.onOutdatedVersion = ASC.Files.Editor.reloadPage;
            eventsConfig.onInfo = ASC.Files.Editor.infoEditor;
            eventsConfig.onRequestEmailAddresses = ASC.Files.Editor.getMails;
            eventsConfig.onRequestStartMailMerge = ASC.Files.Editor.requestStartMailMerge;

            if (!serviceParams.file.provider_key
                && !ASC.Files.Editor.editByUrl
                && !ASC.Files.Editor.thirdPartyApp) {

                ASC.Files.Editor.canShowHistory = true;
                eventsConfig.onRequestHistory = ASC.Files.Editor.requestHistory;
                eventsConfig.onRequestHistoryData = ASC.Files.Editor.getDiffUrl;
                eventsConfig.onRequestHistoryClose = ASC.Files.Editor.reloadPage;
            }
        }

        ASC.Files.Editor.docEditor = new DocsAPI.DocEditor("iframeEditor", {
            width: "100%",
            height: "100%",

            type: typeConfig || "desktop",
            documentType: documentTypeConfig,
            document: documentConfig,
            editorConfig: editorConfig,
            events: eventsConfig
        });
    };

    var fixSize = function () {
        var wrapEl = document.getElementById("wrap");
        if (wrapEl) {
            wrapEl.style.height = screen.availHeight + "px";
            window.scrollTo(0, -1);
            wrapEl.style.height = window.innerHeight + "px";
        }
    };

    var readyEditor = function () {
        if (ASC.Files.Editor.serverErrorMessage) {
            docEditorShowError(ASC.Files.Editor.serverErrorMessage);
            return;
        }

        if (checkMessageFromHash()) {
            location.hash = "";
            return;
        }

        if (ASC.Files.Editor.docServiceParams && ASC.Files.Editor.docServiceParams.mode === "edit") {
            ASC.Files.Editor.trackEdit();
        }
    };

    var backEditor = function (event) {
        if (event && event.data) {
            window.open(href, "_blank");
        } else {
            clearTimeout(trackEditTimeout);
            location.href = ASC.Files.Editor.docServiceParams.folderUrl;
        }
    };

    var documentStateChangeEditor = function (event) {
        if (docIsChanged != event.data) {
            document.title = ASC.Files.Editor.docServiceParams.file.title + (event.data ? " *" : "");
            docIsChanged = event.data;
        }

        if (event.data) {
            subscribeEdit(ASC.Files.ServiceManager.events.StartEdit);
        }
    };

    var subscribeEdit = function (event) {
        if (doStartEdit) {
            doStartEdit = false;

            ASC.Files.ServiceManager.startEdit(event, {
                fileID: ASC.Files.Editor.docServiceParams.file.id,
                docKeyForTrack: ASC.Files.Editor.docServiceParams.key,
                asNew: ASC.Files.Editor.options.asNew,
                shareLinkKey: ASC.Files.Editor.shareLinkParam,
            });

            return true;
        }
        return false;
    };

    var errorEditor = function () {
        ASC.Files.Editor.finishEdit();
    };

    var requestEditRightsEditor = function () {
        location.href = ASC.Files.Editor.docServiceParams.linkToEdit + ASC.Files.Editor.shareLinkParam;

        // Request for old scheme
        //if (ASC.Files.Editor.editByUrl) {
        //    location.href = ASC.Files.Editor.docServiceParams.linkToEdit + ASC.Files.Editor.shareLinkParam;
        //} else {
        //    ASC.Files.ServiceManager.canEditFile(ASC.Files.ServiceManager.events.CanEditFile,
        //        {
        //            fileID: ASC.Files.Editor.docServiceParams.file.id,
        //            shareLinkParam: ASC.Files.Editor.shareLinkParam
        //        });
        //}
    };

    var requestHistory = function () {
        if (!ASC.Files.Editor.canShowHistory) {
            return;
        }

        ASC.Files.ServiceManager.getEditHistory(ASC.Files.ServiceManager.events.GetEditHistory,
            {
                fileID: ASC.Files.Editor.docServiceParams.file.id,
                shareLinkParam: ASC.Files.Editor.shareLinkParam
            });
    };

    var getDiffUrl = function (versionData) {
        if (!ASC.Files.Editor.canShowHistory) {
            return;
        }

        ASC.Files.ServiceManager.getDiffUrl(ASC.Files.ServiceManager.events.GetDiffUrl,
            {
                fileID: ASC.Files.Editor.docServiceParams.file.id,
                version: versionData.data | 0,
                shareLinkParam: ASC.Files.Editor.shareLinkParam
            });
    };

    var getMails = function () {
        ASC.Files.ServiceManager.getMailAccounts(ASC.Files.ServiceManager.events.GetMails);
    };

    var requestStartMailMerge = function () {
        if (!subscribeEdit(ASC.Files.ServiceManager.events.StartMailMerge)) {
            completeStartMailMerge();
        }
    };

    var infoEditor = function (event) {
        if (event && event.data && event.data.mode == "view") {
            clearTimeout(trackEditTimeout);
        }
    };

    var createFile = function () {
        var url = ASC.Files.Constants.URL_HANDLER_CREATE + "?action=create&doctype=" + ASC.Files.Editor.docServiceParams.documentType;
        window.open(url, "_blank");
    };

    var reloadPage = function () {
        location.reload(true);
    };

    var trackEdit = function () {
        clearTimeout(trackEditTimeout);
        if (ASC.Files.Editor.editByUrl || ASC.Files.Editor.thirdPartyApp) {
            return;
        }
        if (!doStartEdit) {
            return;
        }

        ASC.Files.ServiceManager.trackEditFile(ASC.Files.ServiceManager.events.TrackEditFile,
            {
                fileID: ASC.Files.Editor.docServiceParams.file.id,
                tabId: ASC.Files.Editor.tabId,
                docKeyForTrack: ASC.Files.Editor.docKeyForTrack,
                shareLinkParam: ASC.Files.Editor.shareLinkParam,
                fixedVersion: ASC.Files.Editor.fixedVersion
            });
    };

    var finishEdit = function () {
        if (trackEditTimeout !== null && doStartEdit) {
            ASC.Files.ServiceManager.trackEditFile("FinishTrackEditFile",
                {
                    fileID: ASC.Files.Editor.docServiceParams.file.id,
                    tabId: ASC.Files.Editor.tabId,
                    docKeyForTrack: ASC.Files.Editor.docKeyForTrack,
                    shareLinkParam: ASC.Files.Editor.shareLinkParam,
                    finish: true,
                    ajaxsync: true
                });
        }
    };

    var completeTrack = function (jsonData, params, errorMessage) {
        clearTimeout(trackEditTimeout);
        if (typeof errorMessage != "undefined") {
            if (errorMessage == null) {
                docEditorShowInfo("Connection is lost");
            } else {
                docEditorShowWarning(errorMessage || "Connection is lost");
            }
            return;
        }

        if (jsonData.key == true) {
            trackEditTimeout = setTimeout(ASC.Files.Editor.trackEdit, 5000);
        } else {
            errorMessage = jsonData.value;
            denyEditingRights(errorMessage);
        }
    };

    var onStartEdit = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            denyEditingRights(errorMessage);
            return;
        }
    };

    var denyEditingRights = function (message) {
        if (ASC.Files.Editor.docEditor.denyEditingRights) {
            ASC.Files.Editor.docEditor.denyEditingRights(message || "Connection is lost");
        } else if (ASC.Files.Editor.docEditor.processRightsChange) {
            //todo: delete
            ASC.Files.Editor.docEditor.processRightsChange(false, message || "Connection is lost");
        }
    };

    var docEditorShowError = function (message) {
        ASC.Files.Editor.docEditor.showMessage("ONLYOFFICE™", message, "error");
    };

    var docEditorShowWarning = function (message) {
        ASC.Files.Editor.docEditor.showMessage("ONLYOFFICE™", message, "warning");
    };

    var docEditorShowInfo = function (message) {
        ASC.Files.Editor.docEditor.showMessage("ONLYOFFICE™", message, "info");
    };

    var checkMessageFromHash = function () {
        var regExpError = /^#error\/(\S+)?/;
        if (regExpError.test(location.hash)) {
            var errorMessage = regExpError.exec(location.hash)[1];
            errorMessage = decodeURIComponent(errorMessage).replace(/\+/g, " ");
            if (errorMessage.length) {
                docEditorShowWarning(errorMessage);
                return true;
            }
        }
        var regExpMessage = /^#message\/(\S+)?/;
        if (regExpMessage.test(location.hash)) {
            errorMessage = regExpMessage.exec(location.hash)[1];
            errorMessage = decodeURIComponent(errorMessage).replace(/\+/g, " ");
            if (errorMessage.length) {
                docEditorShowInfo(errorMessage);
                return true;
            }
        }
        return false;
    };

    var getRecentList = function () {
        if (!localStorageManager.isAvailable) {
            return null;
        }
        var localStorageKey = ASC.Files.Constants.storageKeyRecent;
        var localStorageCount = 50;
        var recentCount = 10;

        var result = new Array();

        try {
            var recordsFromStorage = localStorageManager.getItem(localStorageKey);
            if (!recordsFromStorage) {
                recordsFromStorage = new Array();
            }

            if (recordsFromStorage.length > localStorageCount) {
                recordsFromStorage = recordsFromStorage.pop();
            }

            var currentRecord = {
                url: location.href,
                id: ASC.Files.Editor.docServiceParams.file.id,
                title: ASC.Files.Editor.docServiceParams.file.title,
                folder: ASC.Files.Editor.docServiceParams.filePath,
                fileType: ASC.Files.Editor.docServiceParams.fileTypeNum
            };

            var containRecord = jq(recordsFromStorage).is(function () {
                return this.id == currentRecord.id;
            });

            if (!containRecord) {
                recordsFromStorage.unshift(currentRecord);

                localStorageManager.setItem(localStorageKey, recordsFromStorage);
            }

            result = jq(recordsFromStorage).filter(function () {
                return this.id != currentRecord.id &&
                    this.fileType === currentRecord.fileType;
            });
        } catch (e) {
        }

        return result.slice(0, recentCount);
    };

    var completeCanEdit = function (jsonData, params, errorMessage) {
        var result = typeof jsonData != "undefined";
        // occurs whenever the user tryes to enter edit mode
        ASC.Files.Editor.docEditor.applyEditRights(result, errorMessage);

        if (result) {
            ASC.Files.Editor.tabId = jsonData;
            ASC.Files.Editor.trackEdit();
        }
    };

    var completeGetEditHistory = function (jsonData, params, errorMessage) {
        if (typeof ASC.Files.Editor.docEditor.refreshHistory != "function") {
            if (typeof errorMessage != "undefined") {
                docEditorShowError(errorMessage || "Connection is lost");
            } else {
                docEditorShowError("Function is not available");
            }
            return;
        }

        if (typeof errorMessage != "undefined") {
            var data = {
                error: errorMessage || "Connection is lost"
            };
        } else {
            clearTimeout(trackEditTimeout);

            data = {
                currentVersion: ASC.Files.Editor.docServiceParams.file.version,
                history: jsonData
            };
        }

        ASC.Files.Editor.docEditor.refreshHistory(data);
    };

    var completeGetDiffUrl = function (jsonData, params, errorMessage) {
        if (typeof ASC.Files.Editor.docEditor.setHistoryData != "function") {
            if (typeof errorMessage != "undefined") {
                docEditorShowError(errorMessage || "Connection is lost");
            } else {
                docEditorShowError("Function is not available");
            }
            return;
        }

        if (typeof errorMessage != "undefined") {
            var data = {
                error: errorMessage || "Connection is lost"
            };
        } else {
            data = {
                version: params.version,
                url: jsonData.key,
                urlDiff: jsonData.value
            };
        }

        ASC.Files.Editor.docEditor.setHistoryData(data);
    };

    var completeGetMails = function (jsonData, params, errorMessage) {
        if (typeof ASC.Files.Editor.docEditor.setEmailAddresses != "function") {
            if (typeof errorMessage != "undefined") {
                docEditorShowError(errorMessage || "Connection is lost");
            } else {
                docEditorShowError("Function is not available");
            }
            return;
        }

        if (typeof errorMessage != "undefined") {
            var data = {
                error: errorMessage || "Connection is lost",
                createEmailAccountUrl: ASC.Files.Constants.URL_MAIL_ACCOUNTS,
            };
        } else {
            data = {
                emailAddresses: jsonData,
                createEmailAccountUrl: ASC.Files.Constants.URL_MAIL_ACCOUNTS,
            };
        }

        ASC.Files.Editor.docEditor.setEmailAddresses(data);
    };

    var completeStartMailMerge = function (jsonData, params, errorMessage) {
        if (typeof ASC.Files.Editor.docEditor.processMailMerge != "function") {
            if (typeof errorMessage != "undefined") {
                docEditorShowError(errorMessage || "Connection is lost");
            } else {
                docEditorShowError("Function is not available");
            }
            return;
        }

        ASC.Files.Editor.docEditor.processMailMerge(typeof errorMessage == "undefined", errorMessage);
    };

    return {
        init: init,
        createFrameEditor: createFrameEditor,
        fixSize: fixSize,

        docEditor: docEditor,

        //set in .cs
        docServiceParams: docServiceParams,
        shareLinkParam: shareLinkParam,
        docKeyForTrack: docKeyForTrack,
        tabId: tabId,
        serverErrorMessage: serverErrorMessage,
        editByUrl: editByUrl,
        canCreate: canCreate,
        options: options,
        thirdPartyApp: thirdPartyApp,
        openinigDate: openinigDate,

        trackEdit: trackEdit,
        finishEdit: finishEdit,

        //event
        readyEditor: readyEditor,
        backEditor: backEditor,
        documentStateChangeEditor: documentStateChangeEditor,
        requestEditRightsEditor: requestEditRightsEditor,
        errorEditor: errorEditor,
        reloadPage: reloadPage,
        requestHistory: requestHistory,
        getDiffUrl: getDiffUrl,
        getMails: getMails,
        requestStartMailMerge: requestStartMailMerge,
        infoEditor: infoEditor,
        createFile: createFile,

        fixedVersion: fixedVersion,
        canShowHistory: canShowHistory,
    };
})();

(function ($) {
    ASC.Files.Editor.init();
    $(function () {
        if (typeof DocsAPI === "undefined") {
            alert("ONLYOFFICE™  is not available. Please contact us at support@onlyoffice.com");
            ASC.Files.Editor.errorEditor();

            return;
        }

        var fixPageCaching = function (delta) {
            if (location.hash.indexOf("reload") == -1) {
                var openingDateParse = Date.parse(ASC.Files.Editor.openinigDate);
                if (!openingDateParse) {
                    return;
                }
                var openinigDate = new Date();
                openinigDate.setTime(openingDateParse);

                var currentTime = new Date();
                var currentUTCTime = new Date(currentTime.getUTCFullYear(), currentTime.getUTCMonth(), currentTime.getUTCDate(), currentTime.getUTCHours(), currentTime.getUTCMinutes());
                if (Math.abs(currentUTCTime - openinigDate) > delta) {
                    location.hash = "reload";
                    location.reload(true);
                }
            } else {
                location.hash = "";
            }
        };
        fixPageCaching(10 * 60 * 1000);

        var $icon = jq("#docsEditorFavicon");
        if ($icon.attr('href').indexOf('logo_favicon_general.ico') !== -1) {//not default
             $icon.attr('href', $icon.attr('href'));
        }

        ASC.Files.Editor.createFrameEditor(ASC.Files.Editor.docServiceParams);

        if (jq("body").hasClass("mobile") || ASC.Files.Editor.docServiceParams && ASC.Files.Editor.docServiceParams.type === "mobile") {
            window.addEventListener("load", ASC.Files.Editor.fixSize);
            window.addEventListener("orientationchange", ASC.Files.Editor.fixSize);
        }
    });
})(jQuery);

String.prototype.format = function () {
    var txt = this,
        i = arguments.length;

    while (i--) {
        txt = txt.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return txt;
};