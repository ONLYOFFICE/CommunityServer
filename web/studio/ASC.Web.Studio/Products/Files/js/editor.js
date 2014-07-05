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
;
window.ASC.Files.Editor = (function () {
    var isInit = false;

    var docIsChanged = false;
    var fixedVersion = false;

    var docEditor = null;
    var docServiceParams = null;

    var trackEditTimeout = null;
    var shareLinkParam = "";
    var docKeyForTrack = "";
    var tabId = "";
    var serverErrorMessage = null;
    var editByUrl = false;
    var mustAuth = false;
    var canCreate = true;
    var thirdPartyApp = false;
    var options = null;
    var openinigDate;
    var newScheme = false;
    var doStartEdit = true;

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        jq("body").css("overflow-y", "hidden");

        window.onbeforeunload = ASC.Files.Editor.finishEdit;

        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.TrackEditFile, completeTrack);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.CanEditFile, completeCanEdit);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SaveEditing, completeSave);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.StartEdit, onStartEdit);
    };

    var createFrameEditor = function (serviceParams) {
        jq("#iframeEditor").parents().css("height", "100%");

        if (serviceParams) {
            var embedded = (serviceParams.type == "embedded");

            var documentConfig = {
                title: serviceParams.file.title,
                url: serviceParams.url,
                fileType: serviceParams.fileType,
                key: serviceParams.key,
                vkey: serviceParams.vkey,
                options: ASC.Files.Editor.options,
            };

            if (!embedded) {
                documentConfig.info = {
                    author: serviceParams.file.create_by,
                    created: serviceParams.file.create_on
                };
                if (serviceParams.filePath.length) {
                    documentConfig.info.folder = serviceParams.filePath;
                }
                if (serviceParams.sharingSettings.length) {
                    documentConfig.info.sharingSettings = serviceParams.sharingSettings;
                }

                documentConfig.permissions = {
                    edit: serviceParams.canEdit
                };
            }

            var editorConfig = {
                mode: serviceParams.mode,
                canBackToFolder: (serviceParams.folderUrl.length > 0),
                lang: serviceParams.lang,
                canAutosave: ASC.Files.Editor.newScheme || !serviceParams.file.provider_key
            };

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
                editorConfig.canCoAuthoring = true;
                if (ASC.Files.Constants.URL_HANDLER_CREATE) {
                    editorConfig.createUrl = ASC.Files.Constants.URL_HANDLER_CREATE;
                }

                if (serviceParams.sharingSettingsUrl) {
                    editorConfig.sharingSettingsUrl = serviceParams.sharingSettingsUrl;
                }

                editorConfig.templates =
                    jq(serviceParams.templates).map(
                        function (i, item) {
                            return {
                                name: item.Key,
                                icon: item.Value
                            };
                        }).toArray();

                editorConfig.user = {
                    id: serviceParams.user.key,
                    name: serviceParams.user.value
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
        }

        var eventsConfig = {
            "onReady": ASC.Files.Editor.readyEditor,
            "onBack": ASC.Files.Editor.backEditor,
            "onDocumentStateChange": ASC.Files.Editor.documentStateChangeEditor,
            "onRequestEditRights": ASC.Files.Editor.requestEditRightsEditor,
            "onSave": ASC.Files.Editor.saveEditor,
            "onError": ASC.Files.Editor.errorEditor
        };

        ASC.Files.Editor.docEditor = new DocsAPI.DocEditor("iframeEditor", {
            width: "100%",
            height: "100%",

            type: typeConfig || "desktop",
            documentType: documentTypeConfig,
            document: documentConfig,
            editorConfig: editorConfig || { canBackToFolder: true },
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

    var backEditor = function () {
        clearTimeout(trackEditTimeout);
        var href = ASC.Files.Editor.docServiceParams ? ASC.Files.Editor.docServiceParams.folderUrl : ASC.Files.Constants.URL_FILES_START;
        location.href = href;
    };

    var documentStateChangeEditor = function (event) {
        if (docIsChanged != event.data) {
            document.title = ASC.Files.Editor.docServiceParams.file.title + (event.data ? " *" : "");
            docIsChanged = event.data;
        }

        if (doStartEdit && event.data) {
            doStartEdit = false;
            if (ASC.Files.Editor.newScheme) {
                ASC.Files.ServiceManager.startEdit(ASC.Files.ServiceManager.events.StartEdit, {
                    fileID: ASC.Files.Editor.docServiceParams.file.id,
                    docKeyForTrack: ASC.Files.Editor.docServiceParams.key,
                    asNew: ASC.Files.Editor.options.asNew,
                    shareLinkKey: ASC.Files.Editor.shareLinkParam,
                });
            }
        }
    };

    var errorEditor = function () {
        ASC.Files.Editor.finishEdit();
    };

    var saveEditor = function (event) {
        if (ASC.Files.Editor.newScheme) {
            setTimeout(function () {
                ASC.Files.Editor.documentStateChangeEditor({ data: false });
                ASC.Files.Editor.docEditor.processSaveResult(true);
            }, 1);
            return true;
        }

        var urlSavedDoc = event.data;

        var urlRedirect = ASC.Files.Editor.FileWebEditorExternalUrlString
            .format(encodeURIComponent(urlSavedDoc), encodeURIComponent(ASC.Files.Editor.docServiceParams.file.title));
        urlRedirect += "&openfolder=true";

        if (ASC.Files.Editor.mustAuth) {
            jq("#additionalMember").val(urlRedirect);
            jq(".block-auth").show();

            ASC.Files.Editor.docEditor.processSaveResult(true);
            return true;
        } else if (ASC.Files.Editor.editByUrl) {

            ASC.Files.Editor.docEditor.processSaveResult(true);
            location.href = urlRedirect;
            return false;

        } else {

            ASC.Files.ServiceManager.saveEditing(ASC.Files.ServiceManager.events.SaveEditing,
                {
                    fileID: ASC.Files.Editor.docServiceParams.file.id,
                    version: ASC.Files.Editor.docServiceParams.file.version,
                    tabId: ASC.Files.Editor.tabId,
                    fileUri: urlSavedDoc,
                    asNew: ASC.Files.Editor.options.asNew,
                    shareLinkKey: ASC.Files.Editor.shareLinkParam
                });

            return false;
        }
    };

    var requestEditRightsEditor = function () {
        if (ASC.Files.Editor.docServiceParams.linkToEdit) {
            location.href = ASC.Files.Editor.docServiceParams.linkToEdit + ASC.Files.Editor.shareLinkParam;
        } else {
            ASC.Files.ServiceManager.canEditFile(ASC.Files.ServiceManager.events.CanEditFile,
                {
                    fileID: ASC.Files.Editor.docServiceParams.file.id,
                    shareLinkParam: ASC.Files.Editor.shareLinkParam
                });
        }
    };

    var trackEdit = function () {
        clearTimeout(trackEditTimeout);
        if (ASC.Files.Editor.editByUrl || ASC.Files.Editor.thirdPartyApp) {
            return;
        }
        if (ASC.Files.Editor.newScheme && !doStartEdit) {
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
        if (trackEditTimeout !== null && (!ASC.Files.Editor.newScheme || doStartEdit)) {
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

    var completeSave = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            var saveResult = false;
        } else {
            saveResult = true;
            ASC.Files.Editor.fixedVersion = true;
            ASC.Files.Editor.documentStateChangeEditor({ data: false });
            ASC.Files.Editor.trackEdit();
        }

        ASC.Files.Editor.docEditor.processSaveResult(saveResult === true, errorMessage);
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
            ASC.Files.Editor.docEditor.processRightsChange(false, errorMessage);
        }
    };

    var onStartEdit = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.Editor.docEditor.processRightsChange(false, errorMessage || "Connection is lost");
            return;
        }
    };

    var docEditorShowError = function (message) {
        ASC.Files.Editor.docEditor.showMessage("Teamlab Office", message, "error");
    };

    var docEditorShowWarning = function (message) {
        ASC.Files.Editor.docEditor.showMessage("Teamlab Office", message, "warning");
    };

    var docEditorShowInfo = function (message) {
        ASC.Files.Editor.docEditor.showMessage("Teamlab Office", message, "info");
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
        if (!ASC.Files.Common.localStorageManager.isAvailable) {
            return null;
        }
        var localStorageKey = ASC.Files.Constants.storageKeyRecent;
        var localStorageCount = 50;
        var recentCount = 10;

        var result = new Array();

        try {
            var recordsFromStorage = ASC.Files.Common.localStorageManager.getItem(localStorageKey);
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

                ASC.Files.Common.localStorageManager.setItem(localStorageKey, recordsFromStorage);
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
        mustAuth: mustAuth,
        canCreate: canCreate,
        options: options,
        thirdPartyApp: thirdPartyApp,
        openinigDate: openinigDate,
        newScheme: newScheme,

        trackEdit: trackEdit,
        finishEdit: finishEdit,

        //event
        readyEditor: readyEditor,
        backEditor: backEditor,
        documentStateChangeEditor: documentStateChangeEditor,
        requestEditRightsEditor: requestEditRightsEditor,
        errorEditor: errorEditor,
        saveEditor: saveEditor,

        fixedVersion: fixedVersion
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
                var openinigDate = new Date();
                openinigDate.setTime(Date.parse(ASC.Files.Editor.openinigDate));

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

        ASC.Files.Editor.createFrameEditor(ASC.Files.Editor.docServiceParams);

        if (jq("body").hasClass("mobile") || ASC.Files.Editor.docServiceParams && ASC.Files.Editor.docServiceParams.type === "mobile") {
            window.addEventListener("load", ASC.Files.Editor.fixSize);
            window.addEventListener("orientationchange", ASC.Files.Editor.fixSize);
        }

        jq(".block-auth .close").on("click", function () {
            jq(".block-auth").hide();
            jq("#wrap iframe").focus();
        });

        jq(document).bind("keyup", ".block-auth", function (event) {
            if (!e) {
                var e = event;
            }

            e = ASC.Files.Common.fixEvent(e);

            var code = e.keyCode || e.which;

            if (code == ASC.Files.Common.keyCode.esc) {
                jq(".block-auth .close").click();
            }
        });
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