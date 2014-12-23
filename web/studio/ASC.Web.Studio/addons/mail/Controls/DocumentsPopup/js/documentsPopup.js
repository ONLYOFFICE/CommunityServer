/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

window.DocumentsPopup = (function() {
    var isInit = false,
        lastId = -1,
        supportedCustomEvents = { SelectFiles: "on_documents_selected" },
        eventsHandler = jq({}),
        documentSelectorTree;

    function init() {
        if (!isInit) {
            isInit = true;
            documentSelectorTree = new ASC.Files.TreePrototype("#documentSelectorTree");
            documentSelectorTree.clickOnFolder = getListFolderFiles;

            var treeNode = jq("#documentSelectorTree .tree-node")[0];
            lastId = jq(treeNode).attr('data-id');

            jq("#popupDocumentUploader .buttonContainer #attach_btn").unbind('click').bind('click', function() {
                if (!jq(this).hasClass('disable')) {
                    attachSelectedFiles();
                }
                return false;
            });

            jq("#popupDocumentUploader .buttonContainer #cancel_btn").unbind('click').bind('click', function() {
                if (!jq(this).hasClass('disable')) {
                    DocumentsPopup.EnableEsc = true;
                    jq.unblockUI();
                }
                return false;
            });

            jq("#popupDocumentUploader .fileList input").unbind('click').bind('click', function() {
                if (!jq(this).is(":checked")) {
                    jq("#checkAll").prop("checked", false);
                    return;
                }
                var checkedAll = true;
                jq(".fileList input").each(function() {
                    if (!jq(this).is(":checked")) {
                        checkedAll = false;
                        return;
                    }
                });
                if (checkedAll) {
                    jq("#checkAll").prop("checked", true);
                }
            });
            toggleButtons(true, false);
        }
    }

    function getListFolderFiles(id) {
        if (id == undefined || id == '') return;

        lastId = id;
        toggleButtons(true, false);
        jq(".fileList").empty();
        jq(".loader").show();
        hideEmptyScreen();
        Teamlab.getDocFolder(null, id, function() { onGetFolderFiles(arguments); });
    }

    function showEmptyScreen() {
        jq("#popupDocumentUploader .fileContainer").find("#emptyFileList").show();
        toggleButtons(true, false);
    }

    function hideEmptyScreen() {
        jq("#popupDocumentUploader .fileContainer").find("#emptyFileList").hide();
    }

    function toggleButtons(hide, all) {
        if (hide) {
            jq("#popupDocumentUploader .buttonContainer #attach_btn").removeClass("disable").addClass("disable");
            if (all)
                jq("#popupDocumentUploader .buttonContainer #cancel_btn").removeClass("disable").addClass("disable");
        } else {
            jq("#popupDocumentUploader .buttonContainer #attach_btn").removeClass("disable");
            if (all)
                jq("#popupDocumentUploader .buttonContainer #cancel_btn").removeClass("disable");
        }
    }

    function onGetFolderFiles(args) {
        var content = new Array();
        var folders = args[1].folders;
        for (var i = 0; i < folders.length; i++) {
            var folderName = folders[i].title;
            var folId = folders[i].id;
            var folder = { title: folderName, exttype: "", id: folId, type: "folder" };
            content.push(folder);
        }
        var files = args[1].files;
        for (var i = 0; i < args[1].files.length; i++) {
            var fileName = decodeURIComponent(files[i].title);

            var exttype = ASC.Files.Utility.getCssClassByFileTitle(fileName, true);

            var fileId = files[i].id;
            var access = files[i].access;
            var viewUrl = files[i].viewUrl;
            var version = files[i].version;
            var contentLength = files[i].contentLength;
            var pureContentLength = files[i].pureContentLength;
            var file = { title: fileName, access: access, exttype: exttype, version: version, id: fileId, type: "file", ViewUrl: viewUrl, size_string: contentLength, size: pureContentLength };
            content.push(file);
        }

        jq(".fileList").empty();
        if (content.length == 0) {
            showEmptyScreen();
            jq(".loader").hide();
            return;
        }
        hideEmptyScreen();
        jq(".fileContainer").find("#emptyFileList").hide();
        var template = "{{if type=='folder'}}<li title='${title}' onclick='DocumentsPopup.openFolder(\"${id}\");' id='${id}'><span class='ftFolder_21 folder'>${title}</span></li>" +
        "{{else}}" +
        "<li title='${title} (${size_string})' onclick='DocumentsPopup.selectFile(\"${id}\");'><input type='checkbox' onclick='DocumentsPopup.selectFile(\"${id}\");' version='${version}' access='${access}' id='${id}' size='${size}'/><label class='${exttype}'>${title}</label></li>" +
        "{{/if}}";
        jq.template("fileTmpl", template);
        jq.tmpl("fileTmpl", content).appendTo(jq(".fileList"));
        jq(".loader").hide();
        toggleButtons(false, true);
        toggleButtons(true, false);

        jq("#filesViewContainer :checkbox").change(testCheckboxesState);
    }

    function selectFile(id) {
        var checkbox = jq(".fileList").find('input[id="' + id + '"]');
        if (checkbox.prop("checked"))
            checkbox.removeAttr("checked");
        else
            checkbox.prop("checked", true);

        testCheckboxesState();
    }

    function testCheckboxesState() {
        var files = jq('#filesViewContainer  :checkbox');
        var has_selected = false;
        for (var i = 0; i < files.length; i++) {
            if (jq(files[i]).is(':checked')) {
                has_selected = true;
                break;
            }
        }
        toggleButtons(!has_selected, false);
    }

    function openFolder(id) {
        getListFolderFiles(id);
        documentSelectorTree.rollUp();
        documentSelectorTree.setCurrent(id);
    }

    function showPortalDocUploader() {

        if (!isInit)
            init();

        documentSelectorTree.rollUp();
        documentSelectorTree.setCurrent(lastId);
        getListFolderFiles(lastId);

        jq(".fileList li input").removeAttr("checked");

        var margintop = jq(window).scrollTop() - 135;
        margintop = margintop + 'px';
        
        toggleButtons(true, false);

        PopupKeyUpActionProvider.EnableEsc = false;
        jq.blockUI({ message: jq("#popupDocumentUploader"),
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

            onBlock: function() { }
        });
    }

    function attachSelectedFiles() {
        try {
            toggleButtons(true, true);
            var listElems = jq(".fileList li input:checked").parent("li");
            var listfiles = new Array();
            var massFileId = new Array();
            for (var i = 0; i < listElems.length; i++) {
                var fileName = jq(listElems[i]).children("label").text();
                var exttype = jq(listElems[i]).children("label").attr("class");
                var fileId = jq(listElems[i]).children("input").attr("id");
                var size = parseInt(jq(listElems[i]).children("input").attr("size"));
                var version = jq(listElems[i]).children("input").attr("version");
                var access = parseInt(jq(listElems[i]).children("input").attr("access"));
                var type;
                if (ASC.Files.Utility.CanImageView(fileName)) {
                    type = "image";
                } else {
                    if (ASC.Files.Utility.CanWebEdit(fileName) && ASC.Resources.Master.TenantTariffDocsEdition) {
                        type = "editedFile";
                    } else {
                        if (ASC.Files.Utility.CanWebView(fileName) && ASC.Resources.Master.TenantTariffDocsEdition) {
                            type = "viewedFile";
                        } else {
                            type = "noViewedFile";
                        }
                    }
                }

                massFileId.push(fileId);
                var downloadUrl = ASC.Files.Utility.GetFileDownloadUrl(fileId);
                var viewUrl = ASC.Files.Utility.GetFileViewUrl(fileId);
                var docViewUrl = ASC.Files.Utility.GetFileWebViewerUrl(fileId);
                var editUrl = ASC.Files.Utility.GetFileWebEditorUrl(fileId);

                var fileTmpl = {
                    title: fileName,
                    access: access,
                    type: type,
                    exttype: exttype,
                    id: fileId,
                    version: version,
                    ViewUrl: viewUrl,
                    downloadUrl: downloadUrl,
                    editUrl: editUrl,
                    docViewUrl: docViewUrl,
                    size: size
                };

                listfiles.push(fileTmpl);
            }

            PopupKeyUpActionProvider.EnableEsc = true;

            eventsHandler.trigger(supportedCustomEvents.SelectFiles, { data: listfiles });

            jq.unblockUI();
        } catch (e) {
            toggleButtons(false, true);
        }
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