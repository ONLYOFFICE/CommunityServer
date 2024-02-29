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


;
window.ASC.Files.Editor = (function () {
    var isInit = false;

    var docIsChanged = false;
    var docIsChangedTimeout = null;
    var canShowHistory = false;

    var docEditor = null;
    var docServiceParams = {};
    var configurationParams = null;

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        jq("body").css("overflow-y", "hidden");

        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetEditHistory, completeGetEditHistory);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetDiffUrl, completeGetDiffUrl);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.RestoreVersion, completeGetEditHistory);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetReferenceData, completeGetReferenceData);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetReferenceLink, completeGetReferenceLink);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetMails, completeGetMails);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.FileRename, completeRename);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetUsers, completeGetUsers);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SendEditorNotify, completeSendEditorNotify);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.ToggleFavorite, completeToggleFavorite);
    };

    var createFrameEditor = function (configuration) {

        jq("#iframeEditor").parents().css("height", "100%");

        var eventsConfig = {
            "onAppReady": ASC.Files.Editor.readyEditor,
        };

        if (configuration) {
            var embedded = (configuration.type == "embedded");

            var documentConfig = configuration.document;

            var options = getOptions();
            if (options) {
                documentConfig.options = options;
            }

            var editorConfig = configuration.editorConfig;

            if (embedded) {
                var keyFullscreen = "fullscreen";
                if (location.hash.indexOf(keyFullscreen) < 0) {
                    editorConfig.embedded.fullscreenUrl = editorConfig.embedded.embedUrl + "#" + keyFullscreen;
                }
            } else {
                if (configuration.type != "embedded" && !editorConfig.recent) {
                    var listRecent = getRecentList();
                    if (listRecent && listRecent.length) {
                        editorConfig.recent = listRecent.toArray();
                    }
                }
            }

            var typeConfig = configuration.type;
            var documentTypeConfig = configuration.documentType;

            eventsConfig.onDocumentStateChange = ASC.Files.Editor.documentStateChangeEditor;
            if (ASC.Files.Editor.docServiceParams.linkToEdit) {
                eventsConfig.onRequestEditRights = ASC.Files.Editor.requestEditRightsEditor;
            }
            eventsConfig.onDocumentReady = ASC.Files.Editor.readyDocument;
            eventsConfig.onOutdatedVersion = ASC.Files.Editor.reloadPage;

            if (documentConfig.permissions.rename) {
                eventsConfig.onRequestRename = ASC.Files.Editor.rename;
            }

            eventsConfig.onMetaChange = ASC.Files.Editor.metaChange;
            eventsConfig.onRequestClose = ASC.Files.Editor.requestClose;
            eventsConfig.onMakeActionLink = ASC.Files.Editor.makeActionLink;

            if (ASC.Files.Constants.URL_MAIL_ACCOUNTS) {
                eventsConfig.onRequestEmailAddresses = ASC.Files.Editor.getMails;
                //todo: remove after release 12.0
                eventsConfig.onRequestStartMailMerge = ASC.Files.Editor.requestStartMailMerge;
            }

            if (!ASC.Files.Editor.docServiceParams.fileProviderKey
                && !ASC.Files.Editor.docServiceParams.editByUrl
                && !ASC.Files.Editor.docServiceParams.thirdPartyApp) {

                if (ASC.Files.Editor.docServiceParams.isAuthenticated) {
                    ASC.Files.Editor.canShowHistory = true;
                    eventsConfig.onRequestHistory = ASC.Files.Editor.requestHistory;
                    eventsConfig.onRequestHistoryData = ASC.Files.Editor.getDiffUrl;
                    eventsConfig.onRequestHistoryClose = ASC.Files.Editor.historyClose;
                    if (!!documentConfig.permissions.changeHistory) {
                        eventsConfig.onRequestRestore = ASC.Files.Editor.restoreVersion;
                    }
                }

                eventsConfig.onRequestReferenceData = ASC.Files.Editor.requestReferenceData;
                eventsConfig.onRequestOpen = ASC.Files.Editor.requestOpen;
            }

            if (ASC.Files.Editor.docServiceParams.canGetUsers) {
                eventsConfig.onRequestUsers = ASC.Files.Editor.requestUsers;
                eventsConfig.onRequestSendNotify = ASC.Files.Editor.requestSendNotify;
            }

            var token = configuration.token;
        }

        ASC.Files.Editor.docEditor = new DocsAPI.DocEditor("iframeEditor", {
            width: "100%",
            height: "100%",

            type: typeConfig || ASC.Files.Editor.docServiceParams.defaultType || "desktop",
            documentType: documentTypeConfig,
            document: documentConfig,
            editorConfig: editorConfig,
            token: token,
            events: eventsConfig
        });
    };

    var getOptions = function () {
        try {
            var param = /(?:\?|\&)options=([^&]*)/g.exec(location.search);
            if (param && param[1]) {
                return JSON.parse(decodeURIComponent(param[1]));
            }
        } catch (e) {
        }
        return null;
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
        if (ASC.Files.Editor.docServiceParams.serverErrorMessage) {
            ASC.Files.Editor.docEditor.showMessage(ASC.Files.Editor.docServiceParams.serverErrorMessage);
            return;
        }

        if (checkMessageFromHash()) {
            location.hash = "";
            return;
        }
    };

    var readyDocument = function () {
        if (ASC.Files.Editor.canShowHistory
            && ASC.Files.Editor.docServiceParams.openHistory) {
            ASC.Files.Editor.requestHistory();
        }
    };

    var documentStateChangeEditor = function (event) {
        clearTimeout(docIsChangedTimeout);

        if (docIsChanged != event.data) {
            var titleChange = function () {
                updateDocumentTitle(event.data);
                docIsChanged = event.data;
            };

            if (event.data) {
                titleChange();
            } else {
                docIsChangedTimeout = setTimeout(titleChange, 500);
            }
        }
    };

    var updateDocumentTitle = function (changed) {
        document.title = (changed ? "*" : "")
            + ASC.Files.Editor.configurationParams.document.title
            + ASC.Files.Editor.docServiceParams.pageTitlePostfix;
    };

    var requestEditRightsEditor = function () {
        location.href = ASC.Files.Editor.docServiceParams.linkToEdit + ASC.Files.Editor.docServiceParams.shareLinkParam;
    };

    var requestHistory = function () {
        if (!ASC.Files.Editor.canShowHistory) {
            return;
        }

        ASC.Files.ServiceManager.getEditHistory(ASC.Files.ServiceManager.events.GetEditHistory,
            {
                fileID: ASC.Files.Editor.docServiceParams.fileId,
                shareLinkParam: ASC.Files.Editor.docServiceParams.shareLinkParam
            });
    };

    var historyClose = function () {
        if (location.search.indexOf("action=view") == -1) {
            ASC.Files.Editor.reloadPage();
            return;
        }

        var url = location.href;
        var indexAnchor = url.indexOf("#");
        if (indexAnchor != -1) {
            url = url.substring(0, indexAnchor);
        }

        if (/[\?\&]history=close/g.test(location.search)) {
            ASC.Files.Editor.reloadPage();
            return;
        }

        url += (url.indexOf("?") == -1) ? "?" : "&";
        url += "history=close";

        location.href = url;
    };

    var getDiffUrl = function (versionData) {
        if (!ASC.Files.Editor.canShowHistory) {
            return;
        }

        ASC.Files.ServiceManager.getDiffUrl(ASC.Files.ServiceManager.events.GetDiffUrl,
            {
                fileID: ASC.Files.Editor.docServiceParams.fileId,
                version: versionData.data | 0,
                shareLinkParam: ASC.Files.Editor.docServiceParams.shareLinkParam
            });
    };

    var restoreVersion = function (versionData) {
        ASC.Files.ServiceManager.restoreVersion(ASC.Files.ServiceManager.events.RestoreVersion,
            {
                fileID: ASC.Files.Editor.docServiceParams.fileId,
                version: versionData.data.version,
                shareLinkParam: ASC.Files.Editor.docServiceParams.shareLinkParam,
                setLast: true,
                url: versionData.data.url || "",
            });
    };

    var requestReferenceData = function (event) {
        var reference = event.data;

        ASC.Files.ServiceManager.getReferenceData(ASC.Files.ServiceManager.events.GetReferenceData,
            {
                fileKey: reference.referenceData ? reference.referenceData.fileKey : "",
                instanceId: reference.referenceData ? reference.referenceData.instanceId : "",
                sourceFileId: ASC.Files.Editor.docServiceParams.fileId,
                path: reference.path || "",
            });
    };

    var requestOpen = function (event) {
        var windowName = event.data.windowName;
        var reference = event.data;

        ASC.Files.ServiceManager.getReferenceData(ASC.Files.ServiceManager.events.GetReferenceLink,
            {
                fileKey: reference.referenceData ? reference.referenceData.fileKey : "",
                instanceId: reference.referenceData ? reference.referenceData.instanceId : "",
                sourceFileId: ASC.Files.Editor.docServiceParams.fileId,
                path: reference.path || "",
                windowName: windowName,
            });
    };

    var getMails = function () {
        ASC.Files.ServiceManager.getMailAccounts(ASC.Files.ServiceManager.events.GetMails);
    };

    var requestUsers = function (event) {
        if (event && event.data) {
            var c = event.data.c;
        }

        switch (c) {
            case "protect":
                ASC.Files.ServiceManager.getProtectUsers(ASC.Files.ServiceManager.events.GetUsers, {
                    c: c,
                    fileId: ASC.Files.Editor.docServiceParams.fileId
                });
                break;
            default:
                ASC.Files.ServiceManager.getSharedUsers(ASC.Files.ServiceManager.events.GetUsers, {
                    c: c,
                    fileId: ASC.Files.Editor.docServiceParams.fileId
                });
        }
    };

    var requestSendNotify = function (event) {
        var message = event.data;

        ASC.Files.ServiceManager.sendEditorNotify(ASC.Files.ServiceManager.events.SendEditorNotify,
            {
                ajaxcontentType: "application/json",
                fileId: ASC.Files.Editor.docServiceParams.fileId
            },
            message);
    };

    var rename = function (event) {
        ASC.Files.ServiceManager.renameFile(ASC.Files.ServiceManager.events.FileRename,
            {
                fileId: ASC.Files.Editor.docServiceParams.fileId,
                newname: event.data,
            });
    };

    var metaChange = function (event) {
        if (event && event.data) {
            if (event.data.title !== undefined) {
                ASC.Files.Editor.configurationParams.document.title = event.data.title;

                updateDocumentTitle(docIsChanged);
            } else if (event.data.favorite !== undefined) {
                toggleFavorite(!!event.data.favorite);
            }
        }
    };
    var toggleFavorite = function (favorite) {
        ASC.Files.ServiceManager.toggleFavorite(ASC.Files.ServiceManager.events.ToggleFavorite,
            {
                ajaxcontentType: "application/json",
                fileId: ASC.Files.Editor.docServiceParams.fileId,
                favorite: favorite
            });
    }

    var requestClose = function () {
        if (!window.opener
            && ASC.Files.Editor.configurationParams.editorConfig.customization.goback
            && ASC.Files.Editor.configurationParams.editorConfig.customization.goback.url) {
            location.href = ASC.Files.Editor.configurationParams.editorConfig.customization.goback.url;
            return;
        }
        window.close();
    };

    var makeActionLink = function (event) {
        var url = location.href;
        if (event && event.data) {
            var indexAnchor = url.indexOf("#");
            if (indexAnchor != -1) {
                url = url.substring(0, indexAnchor);
            }

            var data = JSON.stringify(event.data);
            data = "anchor=" + encodeURIComponent(data);

            var anchorRegex = /anchor=([^&]*)/g;
            if (anchorRegex.test(location.search)) {
                url = url.replace(anchorRegex, data);
            } else {
                url += (url.indexOf("?") == -1) ? "?" : "&";
                url += data;
            }
        }

        ASC.Files.Editor.docEditor.setActionLink(url);
    };

    var reloadPage = function () {
        location.reload(true);
    };

    var checkMessageFromHash = function () {
        var regExpError = /^#error\/(\S+)?/;
        if (regExpError.test(location.hash)) {
            var errorMessage = regExpError.exec(location.hash)[1];
            errorMessage = decodeURIComponent(errorMessage).replace(/\+/g, " ");
            if (errorMessage.length) {
                ASC.Files.Editor.docEditor.showMessage(errorMessage);
                return true;
            }
        }
        var regExpMessage = /^#message\/(\S+)?/;
        if (regExpMessage.test(location.hash)) {
            errorMessage = regExpMessage.exec(location.hash)[1];
            errorMessage = decodeURIComponent(errorMessage).replace(/\+/g, " ");
            if (errorMessage.length) {
                ASC.Files.Editor.docEditor.showMessage(errorMessage);
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
                recordsFromStorage.pop();
            }

            var currentRecord = {
                url: location.href,
                id: ASC.Files.Editor.docServiceParams.fileId,
                title: ASC.Files.Editor.configurationParams.document.title,
                folder: ASC.Files.Editor.configurationParams.document.info.folder,
                fileType: ASC.Files.Editor.configurationParams.documentType,
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

    var completeGetEditHistory = function (jsonData, params, errorMessage) {
        if (typeof ASC.Files.Editor.docEditor.refreshHistory != "function") {
            if (typeof errorMessage != "undefined") {
                ASC.Files.Editor.docEditor.showMessage(errorMessage || "Connection is lost");
            } else {
                ASC.Files.Editor.docEditor.showMessage("Function is not available");
            }
            return;
        }

        if (typeof errorMessage != "undefined") {
            var data = {
                error: errorMessage || "Connection is lost"
            };
        } else {
            var currentVersion = params.setLast
                ? jsonData.length
                : ASC.Files.Editor.docServiceParams.fileVersion;

            jq.each(jsonData, function (i, fileVersion) {
                if (fileVersion.version >= currentVersion) {
                    currentVersion = fileVersion.version;
                    return false;
                }
                return true;
            });

            data = {
                currentVersion: currentVersion,
                history: jsonData,
            };
        }

        ASC.Files.Editor.docEditor.refreshHistory(data);
    };

    var completeGetDiffUrl = function (jsonData, params, errorMessage) {
        if (typeof ASC.Files.Editor.docEditor.setHistoryData != "function") {
            if (typeof errorMessage != "undefined") {
                ASC.Files.Editor.docEditor.showMessage(errorMessage || "Connection is lost");
            } else {
                ASC.Files.Editor.docEditor.showMessage("Function is not available");
            }
            return;
        }

        if (typeof errorMessage != "undefined") {
            jsonData = {
                error: errorMessage || "Connection is lost",
                version: params.version,
            };
        }

        ASC.Files.Editor.docEditor.setHistoryData(jsonData);
    };

    var completeGetReferenceData = function (jsonData, params, errorMessage) {
        if (typeof ASC.Files.Editor.docEditor.setReferenceData != "function") {
            if (typeof errorMessage != "undefined") {
                ASC.Files.Editor.docEditor.showMessage(errorMessage || "Connection is lost");
            } else {
                ASC.Files.Editor.docEditor.showMessage("Function is not available");
            }
            return;
        }

        if (typeof errorMessage != "undefined") {
            jsonData = {
                error: errorMessage || "Connection is lost"
            };
        }

        ASC.Files.Editor.docEditor.setReferenceData(jsonData);
    };

    var completeGetReferenceLink = function (jsonData, params, errorMessage) {
        var windowName = params.windowName;

        if (typeof errorMessage != "undefined" || jsonData.error) {
            var winEditor = window.open("", windowName);
            winEditor.close();
            ASC.Files.Editor.docEditor.showMessage(errorMessage || jsonData.error || "Connection is lost");
            return;
        }

        var link = jsonData.link;
        window.open(link, windowName);
    };

    var completeGetMails = function (jsonData, params, errorMessage) {
        if (typeof ASC.Files.Editor.docEditor.setEmailAddresses != "function") {
            if (typeof errorMessage != "undefined") {
                ASC.Files.Editor.docEditor.showMessage(errorMessage || "Connection is lost");
            } else {
                ASC.Files.Editor.docEditor.showMessage("Function is not available");
            }
            return;
        }

        if (typeof errorMessage != "undefined") {
            var data = {
                error: errorMessage || "Connection is lost",
            };
        } else {
            data = {
                emailAddresses: jsonData,
            };
        }

        if (ASC.Files.Constants.URL_MAIL_ACCOUNTS) {
            data.createEmailAccountUrl = ASC.Files.Constants.URL_MAIL_ACCOUNTS;
        }

        ASC.Files.Editor.docEditor.setEmailAddresses(data);
    };

    var requestStartMailMerge = function () {
        ASC.Files.Editor.docEditor.processMailMerge(true);
    };

    var completeRename = function (xmlData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.Editor.docEditor.showMessage(errorMessage || "Connection is lost");
        }
        return;
    };

    var completeGetUsers = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.Editor.docEditor.showMessage(errorMessage || "Connection is lost");
        }

        ASC.Files.Editor.docEditor.setUsers({
            c: params.c,
            users: jsonData
        });
    };

    var completeSendEditorNotify = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.Editor.docEditor.showMessage(errorMessage || "Connection is lost");
        }

        if (jsonData) {
            ASC.Files.Editor.docEditor.setSharingSettings({sharingSettings: jsonData});

            ASC.Files.Editor.docEditor.showSharingSettings();
        }
    };

    var completeToggleFavorite = function (data, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.Editor.docEditor.showMessage(errorMessage || "Connection is lost");
        }

        ASC.Files.Editor.docEditor.setFavorite(data);
    };

    return {
        init: init,
        createFrameEditor: createFrameEditor,
        fixSize: fixSize,

        docEditor: docEditor,

        //set in .cs
        docServiceParams: docServiceParams,
        configurationParams: configurationParams,

        //event
        readyEditor: readyEditor,
        readyDocument: readyDocument,
        documentStateChangeEditor: documentStateChangeEditor,
        requestEditRightsEditor: requestEditRightsEditor,
        reloadPage: reloadPage,
        requestHistory: requestHistory,
        historyClose: historyClose,
        getDiffUrl: getDiffUrl,
        restoreVersion: restoreVersion,
        requestReferenceData: requestReferenceData,
        requestOpen: requestOpen,
        getMails: getMails,
        requestStartMailMerge: requestStartMailMerge,
        rename: rename,
        metaChange: metaChange,
        requestClose: requestClose,
        makeActionLink: makeActionLink,
        requestUsers: requestUsers,
        requestSendNotify: requestSendNotify,

        canShowHistory: canShowHistory,updateDocumentTitle:updateDocumentTitle
    };
})();

(function ($) {

    if (jq("#iframeEditor").length == 0)
        return;

    ASC.Files.Editor.init();
    $(function () {
        if (typeof DocsAPI === "undefined") {
            alert(ASC.Files.Constants.DocsAPIundefined);

            return;
        }

        if (ASC.Files.Editor.configurationParams
            && (ASC.Files.Editor.configurationParams.document.fileType === "docxf"
                || ASC.Files.Editor.configurationParams.document.fileType === "oform")
            && DocsAPI.DocEditor.version().split(".")[0] < 7) {
            alert("Please update Docs to version 7.0");
            return;
        }

        var fixPageCaching = function (delta) {
            if (location.hash.indexOf("reload") == -1) {
                var openingDateParse = Date.parse(ASC.Files.Editor.docServiceParams.openinigDate);
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
        if ($icon.attr('href').indexOf('logo_favicon_general.ico') !== -1) { //not default
            $icon.attr('href', $icon.attr('href'));
        }

        ASC.Files.Editor.createFrameEditor(ASC.Files.Editor.configurationParams);

        if (jq("body").hasClass("mobile") || ASC.Files.Editor.configurationParams && ASC.Files.Editor.configurationParams.type === "mobile") {
            if (window.addEventListener) {
                window.addEventListener("load", ASC.Files.Editor.fixSize);
                window.addEventListener("resize", ASC.Files.Editor.fixSize);
            } else if (window.attachEvent) {
                window.attachEvent("onload", ASC.Files.Editor.fixSize);
                window.attachEvent("onresize", ASC.Files.Editor.fixSize);
            }
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