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


window.Attachments = (function () {
    var moduleName,
        isInit = false,
        isLoaded = false,
        entityId = null,
        projectId = null,
        rootFolderId = null,
        entityType = "",
        emptyScreenVisible = true,
        createNewEntityFlag = false,
        banOnEditingFlag = false,
        characterString = "@#$%&*+:;\"'<>?|\/",
        characterRegExp = new RegExp("[@#$%&*\+:;\"'<>?|\\\\/]", 'gim');

    var uploadWithAttach = true;

    var replaceSpecCharacter = function(str) {
        return str.replace(characterRegExp, '_');
    };
    var checkCharacter = function(input) {
        jq(input).unbind("keyup").bind("keyup", function() {
            var str = jq(this).val();
            if (str.search(characterRegExp) != -1) {
                jq(this).val(replaceSpecCharacter(str));
                jq("#wrongSign").show();
                setInterval('jq("#wrongSign").hide();', 15000);
            }
        });
    };

    var setCreateNewEntityFlag = function(flag) {
        createNewEntityFlag = flag;
    };

    var getEntityFiles = function (files) {
        if (Array.isArray(files)) {
            onGetFiles([, files]);
            return;
        }
        switch (moduleName) {
            case "projects":
                {
                    Teamlab.getPrjEntityFiles(null, entityId(), entityType, function() { onGetFiles(arguments); });
                    break;
                }
            case "crm":
                {
                    if (entityType == "report") {
                        Teamlab.getCrmReportFiles(null, function () { onGetFiles(arguments); });
                        break;
                    } else {
                        Teamlab.getCrmEntityFiles(null, entityId, entityType, function () { onGetFiles(arguments); });
                        break;
                    }
                }
            default:
                LoadingBanner.hideLoading();
        }
    };
    var loadFiles = function(files) {
        if (!isLoaded) {
            LoadingBanner.displayLoading();
            getEntityFiles(files);
        }
    };
    var checkEditingSupport = function() {
        var listTypes = "";
        listTypes += ASC.Files.Utility.CanWebEdit(ASC.Files.Utility.Resource.InternalFormats.Document) ? "" : "#files_create_text,";
        listTypes += ASC.Files.Utility.CanWebEdit(ASC.Files.Utility.Resource.InternalFormats.Spreadsheet) ? "" : "#files_create_spreadsheet,";
        listTypes += ASC.Files.Utility.CanWebEdit(ASC.Files.Utility.Resource.InternalFormats.Presentation) ? "" : "#files_create_presentation";

        jq(listTypes).hide();
        if (jq("#files_newDocumentPanel ul li").length == listTypes.split(",").length) {
            jq("#createFirstDocument, #showDocumentPanel, #emptyDocumentPanel .newDocComb").hide();
        }
    };
    var init = function (entityTypeParam, entityIdParam) {
        if (!isInit) {
            isInit = true;

            checkEditingSupport();

            entityId = entityIdParam || jq.getURLParam("id");

            emptyScreenVisible = jq("#emptyDocumentPanel").length == 0 ? false : true;

            var module = jq(".wrapperFilesContainer").attr("moduleName");

            if (module != "") moduleName = module;

            entityType = entityTypeParam || jq(".wrapperFilesContainer").attr("entityType");
            var warnText = jq(".infoPanelAttachFile #wrongSign").text() + " " + characterString;
            jq(".infoPanelAttachFile #wrongSign").text(warnText);

            jq.tmpl("template-blockUIPanel", {
                id: "questionWindowAttachments",
                headerTest: ASC.Resources.Master.UserControlsCommonResource.DeleteFile,
                innerHtmlText: ["<p>",
                                ASC.Resources.Master.UserControlsCommonResource.QuestionDeleteFile,
                                "</p>",
                                "<p>",
                                ASC.Resources.Master.UserControlsCommonResource.NotBeUndone,
                                "</p>",
                                "<p>",
                                    "<a class=\"button blue marginLikeButton\" id=\"okButton\">",
                                        ASC.Resources.Master. UserControlsCommonResource.DeleteFile,
                                    "</a>",
                                    "<a id=\"noButton\" class=\"button gray\">",
                                        ASC.Resources.Master.UserControlsCommonResource.CancelButton,
                                    "</a>",
                                "</p>"]
                            .join('')
            }).insertAfter(".wrapperFilesContainer");
        }

        ASC.Controls.AnchorController.bind(/files/, initUploader);

        jq(document).on("keypress", "#newDocTitle", function(evt) {
            if (evt.keyCode == 13) {
                createFile();
            } else {
                if (evt.keyCode == 27) {
                    removeNewDocument();
                } else {
                    checkCharacter(jq("#newDocTitle"));
                }
            }
        });

        jq.dropdownToggle({
            switcherSelector: "#showDocumentPanel, #createFirstDocument, #emptyDocumentPanel .newDocComb",
            anchorSelector: ".newDocComb:visible",
            dropdownID: "files_newDocumentPanel",
            addTop: 9,
            addLeft: -47
        });
        if (emptyScreenVisible) {
            jq.dropdownToggle({
                switcherSelector: "#emptyDocumentPanel .hintCreate",
                dropdownID: "files_hintCreatePanel",
                fixWinSize: false
            });

            jq.dropdownToggle({
                switcherSelector: "#emptyDocumentPanel .hintUpload",
                dropdownID: "files_hintUploadPanel",
                fixWinSize: false
            });

            jq.dropdownToggle({
                switcherSelector: "#emptyDocumentPanel .hintOpen",
                dropdownID: "files_hintOpenPanel",
                fixWinSize: false
            });

            jq.dropdownToggle({
                switcherSelector: "#emptyDocumentPanel .hintEdit",
                dropdownID: "files_hintEditPanel",
                fixWinSize: false
            });
        }

        jq('#questionWindowAttachments #noButton').bind('click', function() {
            jq.unblockUI();
            return false;
        });
        jq(".wrapperFilesContainer").on("click", ".unlinkDoc", function() {
            var fileId = parseInt(jq(this).attr("data-fileId"));
            jq(document).trigger("deleteFile", fileId);
            return false;
        });
        jq(".wrapperFilesContainer").on("click", ".deleteDoc", function() {
            var fileId = parseInt(jq(this).attr("data-fileId"));
            showQuestionWindow(fileId);
            return false;
        });
        jq("#storeOriginalFileFlag").change(function() {
            onChangeStoreFlag();
        });
    };
    var hideNewFileMenu = function() {
        jq("#files_newDocumentPanel").hide();
    };

    var initImageZoom = function() {
        StudioManager.initImageZoom();
    };

    var initUploader = function() {
        createAjaxUploader("linkNewDocumentUpload");

        if (moduleName == "crm") {
            createAjaxUploader("uploadFirstFile");
        }
    };

    var onChangeStoreFlag = function() {
        initUploader();
    };

    var createAjaxUploader = function(buttonId) {
        var storeFlag = jq("#storeOriginalFileFlag").is(":checked");
        if (moduleName == 'crm') {
            Teamlab.createCrmUploadFile(
                null,
                entityType, entityId,
                {
                    buttonId: buttonId,
                    data: {
                        storeOriginalFileFlag: storeFlag
                    },
                    autoSubmit: true
                },
                {
                    before: LoadingBanner.displayLoading,
                    error: function(params, errors) { onError(errors); },
                    success: onUploadFiles
                }
            );
        } else {
            if (!uploadWithAttach) {
                Teamlab.uploadFilesToPrjEntity(
                    null,
                    entityId(),
                    {
                        buttonId: buttonId,
                        autoSubmit: true,
                        data: {
                            entityType: entityType,
                            folderid: rootFolderId,
                            createNewIfExist: true,
                            storeOriginalFileFlag: storeFlag
                        }
                    },
                    {
                        before: LoadingBanner.displayLoading,

                        error: function(params, errors) { onError(errors); },

                        success: onUploadFiles
                    }
                );
            } else {
                Teamlab.createDocUploadFile(
                    null,
                    rootFolderId,
                    {
                        buttonId: buttonId,
                        autoSubmit: true,
                        data: {
                            createNewIfExist: true,
                            storeOriginalFileFlag: storeFlag
                        }
                    },
                    {
                        before: LoadingBanner.displayLoading,

                        error: function(params, errors) { onError(errors); },

                        success: onUploadFiles
                    }
                );
            }
        }
        return;
    };

    var showQuestionWindow = function(fileId) {
        jq('#questionWindowAttachments #okButton').unbind('click');
        StudioBlockUIManager.blockUI("#questionWindowAttachments", 400);
        PopupKeyUpActionProvider.EnterAction = "jq(\"#okButton\").click();";
        jq('#questionWindowAttachments #okButton').bind('click', function() {
            jq.unblockUI();
            jq(document).trigger("deleteFile", fileId);
            return false;
        });
    };

    var createNewDocument = function(type) {
        hideNewFileMenu();

        jq("#emptyDocumentPanel:not(.display-none)").addClass("display-none");
        jq("#attachmentsContainer tr.newDoc").remove();

        var tdClass = ASC.Files.Utility.getCssClassByFileTitle(type, true);
        var tmpl = {
            tdclass: tdClass, type: type,
            onCreateFile: "Attachments.createFile();",
            onRemoveNewDocument: "Attachments.removeNewDocument();"
        };
        var htmlNewDoc = jq.tmpl("template-newFile", tmpl);
        jq("#attachmentsContainer tbody").prepend(htmlNewDoc);
        jq("#attachmentsContainer tr.newDoc").show();
        jq("#newDocTitle").focus().select();
    };
    var removeNewDocument = function() {
        jq("#attachmentsContainer tr.newDoc").remove();
        if (jq("#attachmentsContainer tbody tr").length == 0) {
            jq("#emptyDocumentPanel.display-none").removeClass("display-none");
        }
    };
    var createFile = function() {
        var hWindow = window.open("");
        hWindow.document.write(ASC.Resources.Master.Resource.LoadingPleaseWait);
        hWindow.document.close();

        var title = jq("#newDocTitle").val();
        if (jq.trim(title) == "") {
            title = jq("#newDocTitle").attr("data");
        }
        var ext = jq(".createFile").attr("id");
        title = replaceSpecCharacter(title) + ext;
        Teamlab.addDocFile(
            {},
            rootFolderId,
            "file",
            { title: title, content: '', createNewIfExist: true },
            function() { onCreateFile(arguments, hWindow); },
            { error: function() { hWindow.close(); } }
        );
        removeNewDocument();
    };

    var createFileTmpl = function (fileData) {
        var fileTmpl = {};

        fileTmpl.title = fileData.title;

        fileTmpl.exttype = ASC.Files.Utility.getCssClassByFileTitle(fileTmpl.title, true);

        fileTmpl.access = fileData.access;

        var type;
        if (ASC.Files.Utility.CanImageView(fileTmpl.title)) {
            type = "image";
        } else if (ASC.Files.MediaPlayer && ASC.Files.MediaPlayer.canPlay(fileTmpl.title)) {
            type = "media"
        } else {
            if (ASC.Files.Utility.CanWebEdit(fileTmpl.title) && !ASC.Files.Utility.MustConvert(fileTmpl.title)) {
                type = "editedFile";
            } else {
                if (ASC.Files.Utility.CanWebView(fileTmpl.title) && !ASC.Files.Utility.MustConvert(fileTmpl.title)) {
                    type = "viewedFile";
                } else {
                    type = "noViewedFile";
                }
            }
        }
        fileTmpl.type = type;
        fileTmpl.id = fileData.id;
        fileTmpl.viewUrl = fileData.viewUrl;
        var version = parseInt(fileData.version);
        var versionGroup = parseInt(fileData.versionGroup);
        if (!version) {
            version = 1;
            fileTmpl.viewUrl = ASC.Files.Utility.GetFileDownloadUrl(fileTmpl.id, version);
        }
        fileTmpl.version = version;
        if (!versionGroup) {
            versionGroup = 1;
        }
        fileTmpl.versionGroup = versionGroup;
        fileTmpl.docEditUrl = ASC.Files.Utility.GetFileWebEditorUrl(fileTmpl.id);
        fileTmpl.editUrl = ASC.Files.Utility.GetFileWebEditorUrl(fileTmpl.id);
        fileTmpl.fileStatus = fileData.fileStatus;
        fileTmpl.trashAction = fileData.trashAction ? fileData.trashAction : "delete";
        if (createNewEntityFlag && !fileData.fromProjectDocs) {
            fileTmpl.trashAction = "delete";
        } else {
            if (moduleName == "crm") {
                fileTmpl.trashAction = "delete";
            } else {
                fileTmpl.trashAction = "deattach";
            }
        }
        return fileTmpl;
    };

    var isAddedFile = function(title, fileId) {
        var listAttachFiles = jq("#attachmentsContainer tbody tr td:first-child");
        for (var i = 0, n = listAttachFiles.length; i < n; i++) {
            var fileName = jq.trim(jq(listAttachFiles[i]).children("a").children(".attachmentsTitle").text());
            var id = jq(listAttachFiles[i]).attr("id").split("_")[1];
            if (fileName == title && id == fileId) {
                return listAttachFiles[i];
            }
        }
        return false;
    };

    var showEmptyScreen = function() {
        if (!emptyScreenVisible) {
            jq(".containerAction").show();
            return;
        }
        var files = jq("#attachmentsContainer tr");
        if (!files.length) {
            jq("#attachmentsContainer tbody").empty();
            jq("#emptyDocumentPanel.display-none").removeClass("display-none");
            jq(".containerAction").hide();
            createAjaxUploader("uploadFirstFile");
        }
    };

    var deleteFileFromLayout = function(fileId) {
        if (isLoaded) {
            var td = jq("#af_" + fileId);
            jq(td).parent().remove();
            showEmptyScreen();
        }
    };

    var appendToListAttachFiles = function (listFiles) {
        jq("#emptyDocumentPanel:not(.display-none)").addClass("display-none");
        jq(".containerAction").show();
        jq("#attachmentsContainer tbody").prepend(jq.tmpl("template-fileAttach", listFiles));

        jq("#attachmentsContainer tbody tr").show();
        initImageZoom();
        LoadingBanner.hideLoading();
    };

    var publicAppendToListAttachFiles = function (listFiles) {
        if (isLoaded) {
            jq("#emptyDocumentPanel:not(.display-none)").addClass("display-none");
            jq(".containerAction").show();
            var listFileTempl = new Array();
            for (var i = 0, n = listFiles.length; i < n; i++) {
                var fileTmpl = createFileTmpl(listFiles[i]);
                listFileTempl.push(fileTmpl);
                jq("#attachmentsContainer tr:has(#af_" + listFiles[i].id + ")").remove();
            }
            jq("#attachmentsContainer tbody").prepend(jq.tmpl("template-fileAttach", listFileTempl));

            jq("#attachmentsContainer tbody tr").show();
            initImageZoom();
        }
        LoadingBanner.hideLoading();
    };

    var showAttachedFiles = function(documents) {
        var files = new Array();
        for (var i = 0, n = documents.length; i < n; i++) {
            var file = createFileTmpl(documents[i]);
            files.push(file);
        }
        jq("#attachmentsContainer tbody").empty();
        appendToListAttachFiles(files);
        showEmptyScreen();
        LoadingBanner.hideLoading();
    };

    var showAddedFile = function(files) {
        if (files.length) {
            jq(".containerAction").show();
            for (var i = 0, n = files.length; i < n; i++) {
                var file = files[i];
                var elem = isAddedFile(file.title, file.id);
                if (elem) {
                    jq(elem).parents("tr").remove();
                } else {
                    jq(document).trigger("addFile", file);
                }
                appendToListAttachFiles(createFileTmpl(file));
            }
        }
        LoadingBanner.hideLoading();
    };

    var setRootFolderId = function(folderId, uploadParam) {
        rootFolderId = folderId;
        if (!uploadParam) {
            uploadWithAttach = uploadParam;
        }
        initUploader();
    };

    var events = [];
    var bind = function(eventName, handler) {
        jq(document).bind(eventName, handler);
        events.push(eventName);
    };

    var unbind = function () {
        var $doc = jq(document);
        while (events.length) {
            var item = events.shift();
            $doc.unbind(item);
        }
    };

    var banOnEditing = function() {
        banOnEditingFlag = true;
    };
    var onError = function(error) {
        LoadingBanner.hideLoading();

        jq("#errorFileUpload").text(error[0]).show();
        setInterval('jq("#errorFileUpload").hide();', 15000);
    };
    var onGetFiles = function(response) {
        isLoaded = true;
        showAttachedFiles(response[1]);

        if (response[1].length != 0) {
            jq(document).trigger("loadAttachments", response[1].length);
            if (moduleName == "crm") {
                rootFolderId = response[1][0].parentId;
                createAjaxUploader('linkNewDocumentUpload');
            }
        } else {
            if (moduleName == "crm") {
                Teamlab.getCrmFolder(null, "root", function(params, folder) {
                    rootFolderId = folder.id;
                    createAjaxUploader('uploadFirstFile');
                });
            }
        }
        LoadingBanner.hideLoading();
        if (banOnEditingFlag) {
            jq(".containerAction, .information-upload-panel, .infoPanelAttachFile").hide();
            jq("#emptyDocumentPanel .emptyScrBttnPnl").hide();
            jq("#attachmentsContainer").find(".unlinkDoc").hide();
        }
    };

    var onUploadFiles = function(params, file) {
        createAjaxUploader('linkNewDocumentUpload');
        if (moduleName == "crm") {
            jq(document).trigger("addFile", file);
        } else {
            showAddedFile([file]);
        }
    };

    var onCreateFile = function(response, hWindow) {
        var file = response[1];

        file.fileStatus = 1;
        file.isNewFile = true;

        if (jq("#attachmentsContainer tr").length) {
            createAjaxUploader('linkNewDocumentUpload');
        }
        jq(document).trigger("addFile", file);

        if (moduleName == "projects") {
            appendToListAttachFiles(createFileTmpl(file));
        }
        jq(".containerAction").show();

        if (hWindow.location) {
            hWindow.location.href = ASC.Files.Utility.GetFileWebEditorUrl(file.id);
        }
    };

    return {
        init: init,
        bind: bind,
        unbind: unbind,
        loadFiles: loadFiles,
        get isLoaded() { return isLoaded; },
        set isLoaded(value) { isLoaded = value; },
        appendToListAttachFiles: appendToListAttachFiles,
        isAddedFile: isAddedFile,
        appendFilesToLayout: publicAppendToListAttachFiles,
        deleteFileFromLayout: deleteFileFromLayout,
        createNewDocument: createNewDocument,
        removeNewDocument: removeNewDocument,
        createFile: createFile,
        banOnEditing: banOnEditing,

        setFolderId: setRootFolderId,
        setCreateNewEntityFlag: setCreateNewEntityFlag
    };
})(jQuery);

(jq(document).ready(function () {
    jq("#mediaPlayer").appendTo("body");
}));
