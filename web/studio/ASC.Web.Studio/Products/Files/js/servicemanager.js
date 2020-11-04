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


window.ASC.Files.ServiceManager = (function () {
    var isInit = false;
    var servicePath;
    var cmdSeparator = "/";
    var requestTimeout = 60 * 1000;
    var customEvents = {};

    var checkReady = true;
    var ajaxStek = new Array();

    var init = function (path) {
        if (typeof path !== "string" || path.length === 0) {
            throw "Incorrect service path";
        }

        servicePath = path[path.length - 1] === "/" ? path : path + "/";
        isInit = true;
    };

    var cacheFiles = new Array();
    var cacheTime = 5000;
    var cacheEventType = function () {
        return [
            ASC.Files.ServiceManager.events.GetFolderItems,
            ASC.Files.ServiceManager.events.GetFileHistory,
            ASC.Files.ServiceManager.events.GetNews,
            ASC.Files.ServiceManager.events.GetSiblingsImage,
            ASC.Files.ServiceManager.events.GetThirdParty,
            ASC.Files.ServiceManager.events.GetHelpCenter,
            ASC.Files.ServiceManager.events.GetTreePath,
        ];
    };

    var tryGetFromCache = function (eventType, params) {
        for (var i = 0; i < cacheFiles.length; i++) {
            var value = cacheFiles[i];
            if (value.eventType == eventType
                && value.params == JSON.stringify(params)) {
                var dateStamp = new Date() - value.timeStamp;
                if (0 < dateStamp && dateStamp < cacheTime) {
                    completeRequest.apply(this, value.data);
                    return true;
                } else {
                    cacheFiles.splice(i, 1);
                    i--;
                }
            }
        }

        return false;
    };

    var getRandomId = function (prefix) {
        return (typeof prefix !== "undefined" ? prefix + "-" : "") + Math.floor(Math.random() * 1000000);
    };

    var getUniqueId = function (o, prefix) {
        var iterCount = 0,
            maxIterations = 1000,
            uniqueId = getRandomId();

        while (o.hasOwnProperty(uniqueId) && iterCount++ < maxIterations) {
            uniqueId = getRandomId(prefix);
        }
        return uniqueId;
    };

    var execCustomEvent = function (eventType, thisArg, argsArray) {
        eventType = eventType.toLowerCase();
        thisArg = thisArg || window;
        argsArray = argsArray || [];

        if (!customEvents.hasOwnProperty(eventType)) {
            return;
        }
        var customEvent = customEvents[eventType];

        for (var eventId in customEvent) {
            if (customEvent.hasOwnProperty(eventId)) {
                customEvent[eventId].handler.apply(thisArg, argsArray);
                if (customEvent[eventId].type & 1) {
                    delete customEvent[eventId];
                }
            }
        }
    };

    var addCustomEvent = function (eventType, handler, params) {
        if (typeof eventType !== "string" || typeof handler !== "function") {
            return undefined;
        }

        eventType = eventType.toLowerCase();

        if (typeof params !== "object") {
            params = {};
        }
        var isOnceExec = params.hasOwnProperty("once") ? params.once : false;

        // collect the flags mask the new handler
        var handlerType = 0;
        handlerType |= +isOnceExec * 1; // isOnceExec - process once and delete

        if (!customEvents.hasOwnProperty(eventType)) {
            customEvents[eventType] = {};
        }

        var eventId = getUniqueId(customEvents[eventType]);

        customEvents[eventType][eventId] = {
            handler: handler,
            type: handlerType
        };

        return eventId;
    };

    var removeCustomEvent = function (eventType, eventId) {
        if (typeof eventType !== "string" || typeof eventId === "undefined") {
            return false;
        }

        if (customEvents(eventType) && customEvents[eventType].hasOwnProperty(eventId)) {
            delete userEventHandlers[eventType][eventId];
        }
        return true;
    };

    var getUrl = function () {
        if (!isInit) {
            init(ASC.Files.Constants.URL_WCFSERVICE);
        }
        var url = servicePath;

        if (arguments.length === 0) {
            return url;
        }
        for (var i = 0, n = arguments.length - 1; i < n; i++) {
            url += arguments[i] + cmdSeparator;
        }

        var res = url + arguments[i];
        res += (res.search(/\?/) > 0 ? "&" : "?") + "_=" + new Date().getTime();

        return res;
    };

    var getNodeContent = function (obj) {
        if (!obj || typeof obj !== "object") {
            return "";
        }

        return obj.text || obj.textContent || (function (o) {
            var result = "",
                childrens = o.childNodes;

            if (!childrens) {
                return result;
            }
            for (var i = 0, n = childrens.length; i < n; i++) {
                var child = childrens.item(i);
                switch (child.nodeType) {
                    case 1:
                    case 5:
                        result += arguments.callee(child);
                        break;
                    case 3:
                    case 2:
                    case 4:
                        result += child.nodeValue;
                        break;
                    default:
                        break;
                }
            }
            return result;
        })(obj);
    };

    var completeRequest = function (eventType, params, dataType, xmlHttpRequest, textStatus) {

        checkReady = true;
        if (typeof LoadingBanner != "undefined" && typeof LoadingBanner.hideLoading != "undefined") {
            LoadingBanner.hideLoading();
        }

        if (textStatus === "error") {
            var errorMessage = "",
                commentMessage = "",
                messageNode = null,
                innerNode;
            var innerMessageNode = null;

            if (xmlHttpRequest.responseXML) {
                messageNode = xmlHttpRequest.responseXML.getElementsByTagName("message")[0];
                innerNode = xmlHttpRequest.responseXML.getElementsByTagName("inner")[0];
                if (innerNode) {
                    innerMessageNode = innerNode.getElementsByTagName("message")[0];
                }
                if (errorMessage === "") {
                    try {
                        errorMessage = eval("[" + xmlHttpRequest.responseText + "]")[0].Detail;
                    } catch (e) {
                        var div = document.createElement("div");
                        errorMessage = jq("#content", jq(div).html(xmlHttpRequest.responseText)).text();
                    }
                }
            } else if (xmlHttpRequest.responseText) {
                div = document.createElement("div");
                errorMessage = jq("#content", jq(div).html(xmlHttpRequest.responseText)).text();
                if (errorMessage === "") {
                    try {
                        errorMessage = eval("[" + xmlHttpRequest.responseText + "]")[0].Detail;
                    } catch (e) {
                    }
                }
            }
            if (messageNode && typeof messageNode === "object") {
                errorMessage = getNodeContent(messageNode);
            }
            if (innerMessageNode && typeof innerMessageNode === "object") {
                commentMessage = getNodeContent(innerMessageNode);
            }

            execCustomEvent(eventType, window, [undefined, params, commentMessage || errorMessage]);
            return;
        }

        var data;

        var ignorResponse = "<!DOCTYPE";
        if (xmlHttpRequest.responseText && xmlHttpRequest.responseText.indexOf(ignorResponse) != 0) {
            try {
                switch (dataType) {
                    case "xml":
                        data = ASC.Files.TemplateManager.createXML(xmlHttpRequest.responseText);
                        break;
                    case "json":
                        data = jq.parseJSON(xmlHttpRequest.responseText);
                        break;
                    default:
                        if (xmlHttpRequest.responseXML.xml.indexOf(ignorResponse) != 0) {
                            data = ASC.Files.TemplateManager.createXML(xmlHttpRequest.responseXML.xml)
                                || jq.parseJSON(xmlHttpRequest.responseText);
                        }
                }
            } catch (e) {
                data = xmlHttpRequest.responseText;
            }
        }

        execCustomEvent(eventType, window, [data, params]);

        if (ajaxStek.length != 0 && checkReady == true) {
            var req = ajaxStek.shift();
            checkReady = false;
            execAjax(req);
        }
    };

    var getCompleteCallbackMethod = function (eventType, params, dataType) {
        return function () {
            var argsArray = [eventType, params, dataType];
            for (var i = 0, n = arguments.length; i < n; i++) {
                argsArray.push(arguments[i]);
            }

            if (jq.inArray(eventType, cacheEventType()) >= 0) {
                cacheFiles.push({
                    eventType: eventType,
                    params: JSON.stringify(params),
                    timeStamp: new Date(),
                    data: argsArray
                });
            }

            completeRequest.apply(this, argsArray);
        };
    };

    var request = function (type, dataType, eventType, params) {
        if (typeof type === "undefined" || typeof dataType === "undefined" || typeof eventType !== "string") {
            return;
        }

        if (typeof params == "undefined") {
            params = {};
        }

        if (tryGetFromCache(eventType, params)) {
            return;
        }

        if (typeof LoadingBanner == "undefined" || typeof LoadingBanner.displayLoading == "undefined") {
            params.showLoading = false;
        }

        var data = {},
            argsArray = [];
        var contentType = (params.ajaxcontentType || "application/xml");

        if (typeof params !== "object") {
            params = {};
        }

        switch ((type || "").toLowerCase()) {
            case "delete":
            case "get":
                for (var i = 4, n = arguments.length; i < n; i++) {
                    argsArray.push(arguments[i]);
                }
                break;
            case "post":
                data = (contentType == "text/xml" || contentType == "application/xml"
                    ? ASC.Files.Common.jsonToXml(arguments[4])
                    : (contentType == "application/json"
                        ? JSON.stringify(arguments[4])
                        : arguments[4]));

                for (i = 5, n = arguments.length; i < n; i++) {
                    argsArray.push(arguments[i]);
                }
                break;
            default:
                return;
        }

        var req = {
            async: (params.ajaxsync != true),
            data: data,
            type: type,
            dataType: dataType,
            contentType: contentType,
            cache: true,
            url: getUrl.apply(this, argsArray),
            timeout: requestTimeout,
            beforeSend: function () {
                if (params.showLoading) {
                    if (!ASC.Files.Folders || !ASC.Files.Folders.isFirstLoad) {
                        LoadingBanner.displayLoading();
                        jq('.advansed-filter').advansedFilter("resize");
                    }
                } else {
                    return null;
                }
            },
            complete: getCompleteCallbackMethod(eventType, params, dataType)
        };

        if (ajaxStek.length == 0 && checkReady == true) {
            checkReady = false;
            execAjax(req);
        } else {
            ajaxStek.push(req);
        }
    };

    var execAjax = function (req) {
        jq.ajax({
            async: req.async,
            data: req.data,
            type: req.type,
            dataType: req.dataType,
            contentType: req.contentType,
            cache: req.cache,
            url: req.url,
            timeout: req.timeout,
            beforeSend: req.beforeSend,
            complete: req.complete
        });
    };

    var events = {
        CreateNewFile: "createnewfile",
        CreateFolder: "createfolder",

        CheckEditing: "checkediting",

        GetTreeSubFolders: "gettreesubfolders",
        GetTreePath: "gettreepath",
        GetThirdPartyTree: "getthirdpartytree",
        GetFolderInfo: "getfolderinfo",
        GetFolderItems: "getfolderitems",
        GetItems: "getitems",
        GetFolderItemsTree: "getfolderitemstree",
        GetMediaFile: "getmediafile",

        SetAceLink: "setacelink",
        GetSharedInfo: "getsharedinfo",
        GetSharedInfoShort: "getsharedinfoshort",

        GetFileSharedInfo: "getfilesharedinfo",
        SetAceFileLink: "setacefilelink",
        GetEncryptionAccess: "getencryptionaccess",

        SetAceObject: "setaceobject",
        UnSubscribeMe: "unsubscribeme",
        GetShortenLink: "getshortenlink",
        GetPresignedUri: "getpresigneduri",

        GetUsers: "getusers",
        SendEditorNotify: "sendeditornotify",

        MarkAsRead: "markasread",
        GetNews: "getnews",
        GetTemplates: "gettemplates",

        ChangeOwner: "changeowner",

        FolderRename: "folderrename",
        FileRename: "filerename",

        DeleteItem: "deleteitem",
        EmptyTrash: "emptytrash",

        GetFileHistory: "getfilehistory",
        ReplaceVersion: "replaceversion",
        SetCurrentVersion: "setcurrentversion",
        UpdateComment: "updatecomment",
        CompleteVersion: "completeversion",

        Download: "download",
        GetTasksStatuses: "getTasksStatuses",
        TerminateTasks: "terminatetasks",

        MoveFilesCheck: "movefilescheck",
        MoveItems: "moveitems",

        GetSiblingsImage: "getsiblingsimage",

        GetThirdParty: "getthirdparty",
        SaveThirdParty: "savethirdparty",
        DeleteThirdParty: "deletethirdparty",
        ChangeAccessToThirdparty: "changeaccesstothirdparty",
        SaveDocuSign: "savedocusign",
        SendDocuSign: "senddocusign",

        UpdateIfExist: "updateifexist",
        Forcesave: "forcesave",
        StoreForcesave: "storeforcesave",

        GetHelpCenter: "gethelpcenter",
        ChangeDeleteConfrim: "changedeleteconfrim",

        ConvertCurrentFile: "convertcurrentfile",
        ChunkUploadCheckConversion: "chunkuploadcheckconversion",
        ChunkUploadGetFileFromServer: "chunkuploadgetfilefromserver",

        TrackEditFile: "trackeditfile",
        StartEdit: "startedit",

        LockFile: "lockfile",

        GetEditHistory: "getedithistory",
        GetDiffUrl: "getdiffurl",
        RestoreVersion: "restoreversion",

        GetMails: "getmails",
        StartMailMerge: "startmailmerge",
    };

    var createFolder = function (eventType, params) {
        params.ajaxsync = true;
        request("get", "xml", eventType, params, "folders-create?parentId=" + encodeURIComponent(params.parentFolderID) + "&title=" + encodeURIComponent(params.title));
    };

    var createNewFile = function (eventType, params) {
        params.ajaxsync = true;
        request("get", "xml", eventType, params, "folders-files-createfile?parentId=" + encodeURIComponent(params.folderID || "") + "&title=" + encodeURIComponent(params.fileTitle) + "&templateId=" + encodeURIComponent(params.templateId || ""));
    };

    var getFolderItems = function (eventType, params, data) {
        params.showLoading = params.append != true;
        request("post", "xml", eventType, params, data, "folders?parentId=" + encodeURIComponent(params.folderId) + "&from=" + params.from + "&count=" + params.count + "&filter=" + params.filter + "&subjectGroup=" + !!params.subjectGroup + "&subjectID=" + params.subjectId + "&withSubfolders=" + !!params.withSubfolders + "&searchInContent=" + !!params.searchInContent + "&search=" + encodeURIComponent(params.search));
    };

    var getTreeSubFolders = function (eventType, params) {
        request("get", "xml", eventType, params, "folders-subfolders?parentId=" + encodeURIComponent(params.folderId));
    };

    var getTreePath = function (eventType, params) {
        request("get", "json", eventType, params, "folders-path?folderId=" + encodeURIComponent(params.folderId));
    };

    var getFolder = function (eventType, params) {
        request("get", "json", eventType, params, "folders-folder?folderId=" + encodeURIComponent(params.folderId));
    };

    var getItems = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "folders-entries?filter=" + params.filter + "&subjectGroup=" + !!params.subjectGroup + "&subjectID=" + params.subjectId + "&search=" + encodeURIComponent(params.search));
    };

    var getFile = function (eventType, params) {
        params.ajaxsync = true;
        request("get", params.dataType || "xml", eventType, params, "folders-files-getversion?fileId=" + encodeURIComponent(params.fileId) + "&version=" + (params.version || -1));
    };

    var getFileHistory = function (eventType, params) {
        request("get", "xml", eventType, params, "folders-files-history?fileId=" + encodeURIComponent(params.fileId));
    };

    var setCurrentVersion = function (eventType, params) {
        request("get", "json", eventType, params, "folders-files-updateToVersion?fileId=" + encodeURIComponent(params.fileId) + "&version=" + params.version);
    };

    var updateComment = function (eventType, params) {
        request("get", "json", eventType, params, "folders-files-updateComment?fileId=" + encodeURIComponent(params.fileId) + "&version=" + params.version + "&comment=" + encodeURIComponent(params.comment));
    };

    var completeVersion = function (eventType, params) {
        request("get", "json", eventType, params, "folders-files-completeversion?fileId=" + encodeURIComponent(params.fileId) + "&version=" + params.version + "&continueVersion=" + params.continueVersion);
    };

    var getSiblingsImage = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "folders-files-siblings?fileId=" + encodeURIComponent(params.fileId) + "&parentId=" + encodeURIComponent(params.folderId || "") + "&filter=" + params.filter + "&subjectGroup=" + !!params.subjectGroup + "&subjectID=" + params.subjectId + "&withSubfolders=" + !!params.withSubfolders + "&searchInContent=" + !!params.searchInContent + "&search=" + encodeURIComponent(params.search));
    };

    var getPresignedUri = function (eventType, params) {
        request("get", "json", eventType, params, "presigned?fileId=" + encodeURIComponent(params.fileId));
    };

    var renameFolder = function (eventType, params) {
        request("get", "xml", eventType, params, "folders-rename?folderId=" + encodeURIComponent(params.folderId) + "&title=" + encodeURIComponent(params.newname));
    };

    var renameFile = function (eventType, params) {
        request("get", "xml", eventType, params, "folders-files-rename?fileId=" + encodeURIComponent(params.fileId) + "&title=" + encodeURIComponent(params.newname));
    };

    var moveFilesCheck = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "folders-files-moveOrCopyFilesCheck?destFolderId=" + encodeURIComponent(params.folderToId));
    };

    var moveItems = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "moveorcopy?destFolderId=" + encodeURIComponent(params.folderToId) + "&resolve=" + params.resolve + "&ic=" + (params.isCopyOperation == true));
    };

    var deleteItem = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "folders-files?action=delete");
    };

    var emptyTrash = function (eventType, params) {
        request("get", "json", eventType, params, "emptytrash");
    };

    var download = function (eventType, params, data) {
        params.showLoading = true;
        params.ajaxcontentType = "application/json";
        request("post", "json", eventType, params, data, "bulkdownload");
    };

    var getTasksStatuses = function (eventType, params) {
        request("get", "json", eventType, params, "tasks-statuses");
    };

    var terminateTasks = function (eventType, params) {
        request("get", "json", eventType, params, "tasks");
    };

    var trackEditFile = function (eventType, params) {
        request("get", "json", eventType, params, "trackeditfile?fileId=" + encodeURIComponent(params.fileID) + "&tabId=" + params.tabId + "&docKeyForTrack=" + params.docKeyForTrack + "&isFinish=" + (params.finish == true) + params.shareLinkParam);
    };

    var checkEditing = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "checkediting");
    };

    var startEdit = function (eventType, params) {
        request("get", "json", eventType, params, "startEdit?fileId=" + encodeURIComponent(params.fileID) + params.shareLinkParam);
    };

    var setAceLink = function (eventType, params) {
        request("get", "json", eventType, params, "setacelink?fileId=" + encodeURIComponent(params.fileId) + "&share=" + params.share);
    };

    var getSharedInfo = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "sharedinfo");
    };

    var getSharedInfoShort = function (eventType, params) {
        request("get", "json", eventType, params, "sharedinfoshort?objectId=" + encodeURIComponent(params.objectID));
    };

    var setAceObject = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "setaceobject?notify=" + (params.notify === true));
    };

    var unSubscribeMe = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "removeace");
    };

    var getShortenLink = function (eventType, params) {
        request("get", "json", eventType, params, "shorten?fileId=" + encodeURIComponent(params.fileId));
    };

    var getEncryptionAccess = function (eventType, params) {
        request("get", "json", eventType, params, "publickeys?fileId=" + encodeURIComponent(params.fileId));
    };

    var getUsers = function (eventType, params) {
        request("get", "json", eventType, params, "sharedusers?fileId=" + encodeURIComponent(params.fileId));
    };

    var sendEditorNotify = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "sendeditornotify?fileId=" + encodeURIComponent(params.fileId));
    };

    var checkConversion = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "checkconversion");
    };

    var updateIfExist = function (eventType, params) {
        request("get", "json", eventType, params, "updateifexist?set=" + params.value);
    };

    var forcesave = function (eventType, params) {
        request("get", "json", eventType, params, "forcesave?set=" + params.value);
    };

    var storeForcesave = function (eventType, params) {
        request("get", "json", eventType, params, "storeforcesave?set=" + params.value);
    };

    var changeDeleteConfrim = function (eventType, params) {
        request("get", "json", eventType, params, "changedeleteconfrim?set=" + params.value);
    };

    var getThirdParty = function (eventType, params) {
        params.showLoading = true;
        ASC.Files.Folders.isFirstLoad = false;
        request("get", "json", eventType, params, "thirdparty-list?folderType=" + (params.folderType || 0));
    };

    var saveThirdParty = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "thirdparty-save");
    };

    var deleteThirdParty = function (eventType, params) {
        request("get", "json", eventType, params, "thirdparty-delete?providerId=" + params.providerId);
    };

    var changeAccessToThirdparty = function (eventType, params) {
        request("get", "json", eventType, params, "thirdparty?enable=" + (params.enable === true));
    };

    var saveDocuSign = function (eventType, params, data) {
        params.ajaxcontentType = "application/json";
        request("post", "json", eventType, params, data, "docusign-save");
    };

    var deleteDocuSign = function (eventType, params) {
        request("get", "json", eventType, params, "docusign-delete");
    };

    var sendDocuSign = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "docusign?fileId=" + encodeURIComponent(params.fileId));
    };

    var markAsRead = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "markasread");
    };

    var getNews = function (eventType, params) {
        request("get", "xml", eventType, params, "getnews?folderId=" + encodeURIComponent(params.folderId));
    };

    var getTemplates = function (eventType, params) {
        request("get", "xml", eventType, params, "gettemplates?filter=" + params.filter + "&from=" + params.from + "&count=" + params.count + "&subjectGroup=" + !!params.subjectGroup + "&subjectID=" + params.subjectId + "&searchInContent=" + !!params.searchInContent + "&search=" + encodeURIComponent(params.search));
    
    };

    var lockFile = function (eventType, params) {
        request("get", "xml", eventType, params, "folders-files-lock?fileId=" + encodeURIComponent(params.fileId) + "&lockfile=" + (params.lock === true));
    };

    var getEditHistory  = function (eventType, params) {
        request("get", "json", eventType, params, "edit-history?fileId=" + encodeURIComponent(params.fileID) + params.shareLinkParam);
    };

    var getDiffUrl = function (eventType, params) {
        request("get", "json", eventType, params, "edit-diff-url?fileId=" + encodeURIComponent(params.fileID) + "&version=" + params.version + params.shareLinkParam);
    };

    var restoreVersion = function (eventType, params) {
        request("get", "json", eventType, params, "restore-version?fileId=" + encodeURIComponent(params.fileID) + "&version=" + params.version + "&url=" + encodeURIComponent(params.url) + params.shareLinkParam);
    };

    var getMailAccounts = function (eventType, params) {
        request("get", "json", eventType, params, "mailaccounts");
    };

    var getHelpCenter = function (eventType, params) {
        request("get", "json", eventType, params, "gethelpcenter");
    };

    var changeOwner = function (eventType, params, data) {
        request("post", "json", eventType, params, data, "changeowner?userId=" + params.userId);
    };

    return {
        bind: addCustomEvent,
        unbind: removeCustomEvent,

        events: events,

        createFolder: createFolder,
        createNewFile: createNewFile,

        getFolderItems: getFolderItems,
        getTreeSubFolders: getTreeSubFolders,
        getTreePath: getTreePath,
        getFolder: getFolder,

        getItems: getItems,
        getFile: getFile,
        getFileHistory: getFileHistory,
        setCurrentVersion: setCurrentVersion,
        updateComment: updateComment,
        completeVersion: completeVersion,
        getSiblingsImage: getSiblingsImage,
        getPresignedUri: getPresignedUri,

        renameFolder: renameFolder,
        renameFile: renameFile,
        
        changeOwner: changeOwner,
        moveFilesCheck: moveFilesCheck,
        moveItems: moveItems,
        deleteItem: deleteItem,
        emptyTrash: emptyTrash,
        download: download,

        getTasksStatuses: getTasksStatuses,
        terminateTasks: terminateTasks,

        trackEditFile: trackEditFile,
        checkEditing: checkEditing,
        startEdit: startEdit,

        setAceLink: setAceLink,
        getSharedInfo: getSharedInfo,
        getSharedInfoShort: getSharedInfoShort,
        setAceObject: setAceObject,
        unSubscribeMe: unSubscribeMe,
        getShortenLink: getShortenLink,
        getEncryptionAccess: getEncryptionAccess,

        getUsers: getUsers,
        sendEditorNotify: sendEditorNotify,

        checkConversion: checkConversion,
        updateIfExist: updateIfExist,
        forcesave: forcesave,
        storeForcesave: storeForcesave,
        changeDeleteConfrim: changeDeleteConfrim,

        getThirdParty: getThirdParty,
        saveThirdParty: saveThirdParty,
        deleteThirdParty: deleteThirdParty,
        changeAccessToThirdparty: changeAccessToThirdparty,
        saveDocuSign: saveDocuSign,
        deleteDocuSign: deleteDocuSign,
        sendDocuSign: sendDocuSign,

        markAsRead: markAsRead,
        getNews: getNews,
        getTemplates: getTemplates,

        lockFile: lockFile,

        getEditHistory: getEditHistory,
        getDiffUrl: getDiffUrl,
        restoreVersion: restoreVersion,

        getMailAccounts: getMailAccounts,

        getHelpCenter: getHelpCenter
    };
})();