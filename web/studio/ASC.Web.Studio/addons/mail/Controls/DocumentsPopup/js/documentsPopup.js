/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


window.DocumentsPopup = (function($) {
    var isInit = false,
        el,
        elFiles,
        elButtons,
        elEmpty,
        elError,
        elLoader,
        $attachFilesAsLinksSelector,
        elDocumentSelectorTree,
        lastId = -1,
        supportedCustomEvents = { SelectFiles: "on_documents_selected" },
        eventsHandler = jq({}),
        documentSelectorTree,
        folderFiles = [],
        isShareableFolder = false,
        showTypes = {
            loader: 0,
            files: 1,
            error: 2,
            empty: 3
        };

    function toggleButtons(hide, all) {
        if (hide) {
            elButtons.find("#attach_btn").toggleClass("disable", true);
            if (all) {
                elButtons.find("#cancel_btn").toggleClass("disable", true);
            }
        } else {
            elButtons.find("#attach_btn").removeClass("disable");
            if (all) {
                elButtons.find("#cancel_btn").removeClass("disable");
            }
        }
    }

    function showContent(type) {
        elFiles.hide();
        elLoader.hide();
        elEmpty.hide();
        elError.hide();
        toggleButtons(true, false);

        switch (type) {
        case showTypes.loader:
            elLoader.show();
            break;
        case showTypes.files:
            elFiles.show();
            break;
        case showTypes.error:
            elError.show();
            break;
        case showTypes.empty:
        default:
            elEmpty.show();
            break;
        }
    }

    function testCheckboxesState() {
        var files = elFiles.find("li input:checked");
        var hasSelected = false;
        for (var i = 0; i < files.length; i++) {
            if (jq(files[i]).is(':checked')) {
                hasSelected = true;
                break;
            }
        }
        toggleButtons(!hasSelected, false);
    }

    function onGetFolderFiles(params, folderInfo) {
        var content = new Array();
        var folders = folderInfo.folders;
        var i;
        for (i = 0; i < folders.length; i++) {
            var folderName = folders[i].title;
            var folId = folders[i].id;
            var folder = { title: folderName, exttype: "", id: folId, type: "folder" };
            content.push(folder);
        }
        
        folderFiles = folderInfo.files;
        isShareableFolder = folderInfo.isShareable === undefined ? folderInfo.current.isShareable : folderInfo.isShareable;

        for (i = 0; i < folderFiles.length; i++) {
            var fileName = decodeURIComponent(folderFiles[i].title);

            var file = {
                title: fileName,
                access: folderFiles[i].access,
                exttype: ASC.Files.Utility.getCssClassByFileTitle(fileName, true),
                version: folderFiles[i].version,
                id: folderFiles[i].id,
                type: "file",
                ViewUrl: folderFiles[i].viewUrl,
                webUrl: folderFiles[i].webUrl,
                size_string: folderFiles[i].contentLength,
                size: folderFiles[i].pureContentLength,
                original: folderFiles[i]
            };
            content.push(file);
        }

        elFiles.empty();
        if (content.length === 0) {
            showContent(showTypes.empty);
            return;
        }

        jq.tmpl("docAttachTmpl", content).appendTo(elFiles);
        
        showContent(showTypes.files);

        toggleButtons(false, true);
        toggleButtons(true, false);

        elFiles.find("li input:checked").change(testCheckboxesState);
    }

    function onError(params, errors) {
        if (errors.length > 1 && errors[1].hresult) {
            if (errors[1].hresult === ASC.Mail.Constants.Errors.COR_E_UNAUTHORIZED_ACCESS) {
                var html = $.tmpl('filesFolderOpenErrorTmpl');
                elError.html(html);
                showContent(showTypes.error);
                return;
            }
        }

        window.toastr.error(errors[0]);
        showContent(showTypes.empty);
    }

    function getListFolderFiles(id) {
        if (id == undefined || id === '') {
            return;
        }

        lastId = id;
        showContent(showTypes.loader);
        Teamlab.getDocFolder({}, id, { success: onGetFolderFiles, error: onError, max_request_attempts: 1 });
    }

    function initElements() {
        el = jq("#popupDocumentUploader");
        elFiles = el.find(".fileList");
        elButtons = el.find(".buttonContainer");
        elEmpty = el.find("#emptyFileList");
        elError = el.find("#errorOpenFolder");
        elLoader = el.find(".loader");
        $attachFilesAsLinksSelector = el.find("#attachFilesAsLinksSelector");
        documentSelectorTree = new ASC.Files.TreePrototype("#documentSelectorTree");
        documentSelectorTree.clickOnFolder = getListFolderFiles;
        elDocumentSelectorTree = jq("#documentSelectorTree");
        var treeNodes = elDocumentSelectorTree.find(".tree-node");
        if (treeNodes.length > 0) {
            lastId = jq(treeNodes[0]).attr('data-id');
        }
    }

    function attachSelectedFiles() {
        try {
            toggleButtons(true, true);

            var listfiles = [];

            var fileIds = jq.map(elFiles.find("li input:checked"), function(el) {
                var id = jq(el).attr('id');
                return $.isNumeric(id) ? parseInt(id) : id;
            });
            
            var selectedFiles = jq.grep(folderFiles, function(f) {
                return jq.inArray(f.id, fileIds) !== -1;
            });

            for (var i = 0; i < selectedFiles.length; i++) {
                var file = selectedFiles[i];

                var type;
                if (ASC.Files.Utility.CanImageView(file.title)) {
                    type = "image";
                } else {
                    if (ASC.Files.Utility.CanWebEdit(file.title)) {
                        type = "editedFile";
                    } else {
                        if (ASC.Files.Utility.CanWebView(file.title)) {
                            type = "viewedFile";
                        } else {
                            type = "noViewedFile";
                        }
                    }
                }

                var downloadUrl = ASC.Files.Utility.GetFileDownloadUrl(file.id);
                var viewUrl = ASC.Files.Utility.GetFileViewUrl(file.id);
                var docViewUrl = ASC.Files.Utility.GetFileWebViewerUrl(file.id);
                var editUrl = ASC.Files.Utility.GetFileWebEditorUrl(file.id);

                var fileTmpl = {
                    title: file.title,
                    access: file.access,
                    type: type,
                    exttype: ASC.Files.Utility.getCssClassByFileTitle(file.title),
                    id: file.id,
                    version: file.version,
                    ViewUrl: viewUrl,
                    downloadUrl: downloadUrl,
                    editUrl: editUrl,
                    docViewUrl: docViewUrl,
                    webUrl: file.webUrl,
                    size: file.pureContentLength,
                    inShareableFolder: isShareableFolder
                };

                listfiles.push(fileTmpl);
            }

            PopupKeyUpActionProvider.EnableEsc = true;

            eventsHandler.trigger(supportedCustomEvents.SelectFiles, { data: listfiles, asLinks: $attachFilesAsLinksSelector.is(':checked') });

            jq.unblockUI();
        } catch (e) {
            console.error(e);
            toggleButtons(false, true);
        }
    }

    function bindEvents() {
        elButtons.find("#attach_btn").unbind('click').bind('click', function () {
            if (!jq(this).hasClass('disable')) {
                attachSelectedFiles();
            }
            return false;
        });

        elButtons.find("#cancel_btn").unbind('click').bind('click', function () {
            if (!jq(this).hasClass('disable')) {
                DocumentsPopup.EnableEsc = true;
                jq.unblockUI();
            }
            return false;
        });

        elFiles.find("input").unbind('click').bind('click', function () {
            if (!jq(this).is(":checked")) {
                jq("#checkAll").prop("checked", false);
                return;
            }
            var checkedAll = true;
            elFiles.find("input").each(function () {
                if (!jq(this).is(":checked")) {
                    checkedAll = false;
                    return;
                }
            });
            if (checkedAll) {
                el.find("#checkAll").prop("checked", true);
            }
        });
    }

    function init() {
        if (!isInit) {
            isInit = true;

            initElements();
            bindEvents();

            toggleButtons(true, false);
        }
    }

    function selectFile(id) {
        var checkbox = elFiles.find('input[id="' + id + '"]');
        if (checkbox.prop("checked")) {
            checkbox.removeAttr("checked");
        } else {
            checkbox.prop("checked", true);
        }

        testCheckboxesState();
    }

    function openFolder(id) {
        getListFolderFiles(id);
        documentSelectorTree.rollUp();
        documentSelectorTree.setCurrent(id);
    }

    function showPortalDocUploader() {
        if (!isInit) {
            init();
        }

        documentSelectorTree.rollUp();
        documentSelectorTree.setCurrent(lastId);
        getListFolderFiles(lastId);

        elFiles.find("li input").removeAttr("checked");

        var margintop = jq(window).scrollTop() - 135;
        margintop = margintop + 'px';

        toggleButtons(true, false);

        PopupKeyUpActionProvider.EnableEsc = false;
        jq.blockUI({
            message: el,
            css: {
                left: '50%',
                top: '25%',
                opacity: '1',
                border: 'none',
                padding: '0px',
                width: '650px',

                cursor: 'default',
                textAlign: 'left',
                position: 'absolute',
                'margin-left': '-300px',
                'margin-top': margintop,
                'background-color': 'White'
            },

            overlayCSS: {
                backgroundColor: '#AAA',
                cursor: 'default',
                opacity: '0.3'
            },
            focusInput: false,
            baseZ: 666,

            fadeIn: 0,
            fadeOut: 0,

            onBlock: function() {
            }
        });
    }

    function bind(eventName, fn) {
        eventsHandler.bind(eventName, fn);
    }

    function unbind(eventName) {
        eventsHandler.unbind(eventName);
    }

    return {
        init: init,
        bind: bind,
        unbind: unbind,
        events: supportedCustomEvents,
        onGetFolderFiles: onGetFolderFiles,
        showPortalDocUploader: showPortalDocUploader,
        openFolder: openFolder,
        attachSelectedFiles: attachSelectedFiles,
        selectFile: selectFile
    };
})(jQuery);