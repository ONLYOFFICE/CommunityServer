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


window.ProjectDocumentsPopup = (function() {
    var isInit = false,
        rootFolderId,
        firstLoad = true;
    var attachButton;

    var init = function (projectFolderId, projectName) {
        if (rootFolderId && rootFolderId !== projectFolderId) {
            isInit = false;
            firstLoad = true;
            jq(".popupContainerBreadCrumbs").html('');
        }

        if (!isInit) {
            isInit = true;
            var resource = ASC.Resources.Master.UserControlsCommonResource;
            jq.tmpl("template-emptyScreen",
            {
                ImgSrc: jq("#emptyFileList").attr("data-imgSrc"),
                Header: resource.ProjectDocuments,
                HeaderDescribe: resource.EmptyDocsHeaderDescription,
                Describe: resource.EmptyDocsDescription
            }).appendTo("#emptyFileList");

            rootFolderId = projectFolderId;

            var rootName = "<a class='root' id='" + rootFolderId + "' >" + projectName + "</a>";
            jq(".popupContainerBreadCrumbs").append(rootName);
        }
        attachButton = jq("#popupDocumentUploader .buttonContainer .button.blue");

        jq("#popupDocumentUploader .buttonContainer").off('click', '.button.blue').on('click', '.button.blue', function () {

            if (!jq(this).hasClass('disable')) {
                attachSelectedFiles();
            }
            return false;
        });
        jq("#popupDocumentUploader .buttonContainer").off('click', '.button.gray').on('click', '.button.gray', function () {
            ProjectDocumentsPopup.EnableEsc = true;
            jq.unblockUI();
            return false;
        });

        jq("#popupDocumentUploader .popupContainerBreadCrumbs").off('click', 'a').on('click', 'a', function () {
            openPreviosFolder(this);
            var links = jq("#popupDocumentUploader .popupContainerBreadCrumbs").find('a');
            if (links.length - 1 > 1) {
                jq("#popupDocumentUploader .popupContainerBreadCrumbs a:first").removeClass("root");
            } else {
                jq("#popupDocumentUploader .popupContainerBreadCrumbs a:first").addClass("root");
            }
            return false;
        });

        jq("#checkAll").off("change").on("change", function () {
            var checkedFlag = jq("#checkAll").prop("checked");
            jq("ul.fileList li input").prop("checked", checkedFlag);
            if (checkedFlag) {
                attachButton.removeClass("disable");
            } else {
                attachButton.addClass("disable");
            }
        });

        jq("#popupDocumentUploader").off('click', '.fileList li').on("click", ".fileList li", function (event) {
            if (!jq(event.target).is("input") && !jq(event.target).is("label")) {
                var input = jq(this).children("input");
                if (jq(input).is(":checked")) {
                    input.removeAttr("checked");
                } else {
                    input.attr("checked", "checked");
                }
            }
            disableButtonFunc();
        });
    };

    var disableButtonFunc = function() {
        var checkedAll = true;
        var checkedFlag = false;
        var inputs = jq(".fileList input");
        for (var i = 0; i < inputs.length; i++) {
            if (!checkedFlag) {
                checkedFlag = jq(inputs[i]).is(":checked");
            }
            if (!jq(inputs[i]).is(":checked")) {
                checkedAll = false;
            }
        };
        if (checkedAll) {
            jq("#checkAll").attr("checked", "checked");
        } else {
            jq("#checkAll").removeAttr("checked");
        }
        if (checkedFlag) {
            attachButton.removeClass("disable");
        } else {
            attachButton.addClass("disable");
        }
    };

    var getListFolderFiles = function(id) {
        Teamlab.getDocFolder(null, id, function() { onGetFolderFiles(arguments); });
    };

    var showEmptyScreen = function () {
        jq("#popupDocumentUploader .emptyScrCtrl").show();
        jq("#popupDocumentUploader .fileContainer").find("#emptyFileList").show();
        jq("#popupDocumentUploader .buttonContainer .button .blue").addClass("disable");
    };
    var hideEmptyScreen = function() {
        jq("#popupDocumentUploader .fileContainer").find("#emptyFileList").hide();
        jq("#popupDocumentUploader .buttonContainer .button .blue").removeClass("disable");
    };

    var showCheckAll = function() {
        var checkboxs = jq(".fileList li input");
        if (checkboxs.length) {
            jq(".containerCheckAll").show();
        } else {
            jq(".containerCheckAll").hide();
        }
    };

    var onGetFolderFiles = function(args) {
        if (firstLoad) {
            firstLoad = false;
        }
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

            var fileName = files[i].title;

            var exttype = ASC.Files.Utility.getCssClassByFileTitle(fileName, true);

            var fileId = files[i].id;
            var access = files[i].access;
            var viewUrl = files[i].viewUrl;
            var version = files[i].version;
            var versionGroup = files[i].versionGroup;
            var file = { title: fileName, access: access, exttype: exttype, version: version, versionGroup: versionGroup, id: fileId, type: "file", viewUrl: viewUrl };
            content.push(file);
        }

        jq(".fileList").empty();
        if (content.length == 0) {
            showEmptyScreen();
            jq(".containerCheckAll").hide();
            jq(".loader").hide();
            return;
        }
        hideEmptyScreen();
        jq(".fileContainer").find("#emptyFileList").hide();
        var template = "{{if type=='folder'}}<li onclick='ProjectDocumentsPopup.openFolder(this);' id='${id}'><span class='ftFolder_21 folder'>${title}</span></li>" +
        "{{else}}" +
        "<li><input type='checkbox' version='${version}' versionGroup='${versionGroup}' access='${access}' id='${id}'/><label class='${exttype}' for='${id}'>${title}</label></li>" +
        "{{/if}}";
        jq.template("fileTmpl", template);
        jq.tmpl("fileTmpl", content).appendTo(jq(".fileList"));
        showCheckAll();
        jq(".loader").hide();
    };

    var reset = function () {
        firstLoad = true;
    }

    var addItemInBreadCrumbs = function(id, title) {
        jq(".popupContainerBreadCrumbs a.current").removeClass("current");
        jq(".popupContainerBreadCrumbs a:first").removeClass("root");
        var newItem = "<span> > </span><a class='current' id='" + id + "'>" + title + "</a>";
        jq(".popupContainerBreadCrumbs").append(newItem);
    };

    var openFolder = function(folder) {
        var id = jq(folder).attr("id");
        var text = jq(folder).children("span").text();
        getListFolderFiles(id);
        addItemInBreadCrumbs(id, text);
    };

    var removeItemInBreadCrumbs = function(folder) {
        var firstFolder = jq(".popupContainerBreadCrumbs a:first-child").attr("id");
        var lastFolder = jq(".popupContainerBreadCrumbs a:last-child").attr("id");

        var id = jq(folder).attr("id");
        while (id != lastFolder) {
            jq(".popupContainerBreadCrumbs a:last-child").remove();
            jq(".popupContainerBreadCrumbs span:last-child").remove();
            lastFolder = jq(".popupContainerBreadCrumbs a:last-child").attr("id");
        }
        if (firstFolder == lastFolder) return;
        jq(".popupContainerBreadCrumbs a:last-child").addClass("current");
    };

    var openPreviosFolder = function(folder) {
        var id = jq(folder).attr("id");
        getListFolderFiles(id);
        removeItemInBreadCrumbs(folder);
    };

    var showPortalDocUploader = function() {
        if (firstLoad) {
            getListFolderFiles(rootFolderId);
        }
        jq("#checkAll").removeAttr("checked");
        jq(".fileList li input").removeAttr("checked");
        jq(".fileList li input").prop("checked", false);

        if (!firstLoad) {
            Teamlab.call(Teamlab.events.getDocFolder, this, [undefined, undefined]);
        }

        var margintop = jq(window).scrollTop() - 135;
        margintop = margintop + 'px';

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
    };

    var attachSelectedFiles = function() {
        var listElems = jq(".fileList li input:checked").parent("li");
        var listfiles = new Array();
        var massFileId = new Array();
        for (var i = 0; i < listElems.length; i++) {
            var fileName = jq(listElems[i]).children("label").text();
            var exttype = jq(listElems[i]).children("label").attr("class");
            var fileId = parseInt(jq(listElems[i]).children("input").attr("id"));
            var version = jq(listElems[i]).children("input").attr("version");
            var versionGroup = jq(listElems[i]).children("input").attr("versionGroup");
            var access = parseInt(jq(listElems[i]).children("input").attr("access"));
            var type;
            if (ASC.Files.Utility.CanImageView(fileName)) {
                type = "image";
            }
            else {
                if (ASC.Files.Utility.CanWebEdit(fileName)) {
                    type = "editedFile";
                }
                else {
                    if (ASC.Files.Utility.CanWebView(fileName)) {
                        type = "viewedFile";
                    }
                    else {
                        type = "noViewedFile";
                    }
                }
            }
            if (!Attachments.isAddedFile(fileName, fileId)) {
                massFileId.push(fileId);
                var viewUrl = ASC.Files.Utility.GetFileDownloadUrl(fileId);
                var docEditUrl = ASC.Files.Utility.GetFileWebEditorUrl(fileId);
                var editUrl = ASC.Files.Utility.GetFileWebEditorUrl(fileId);
                var fileTmpl = { title: fileName, access: access, type: type, exttype: exttype, id: fileId, version: version, versionGroup: versionGroup,
                    viewUrl: viewUrl, editUrl: editUrl, docEditUrl: docEditUrl, fromProjectDocs: true, trashAction: "deattach"
                };
                listfiles.push(fileTmpl);
                fileTmpl.attachFromPrjDocFlag = true;
                jq(document).trigger("addFile", fileTmpl);
            }
        }
        if (massFileId.length != 0) {
            Attachments.appendToListAttachFiles(listfiles);
        }
        PopupKeyUpActionProvider.EnableEsc = true;
        jq.unblockUI();
    };
    return {
        init: init,
        onGetFolderFiles: onGetFolderFiles,
        showPortalDocUploader: showPortalDocUploader,
        openFolder: openFolder,
        attachSelectedFiles: attachSelectedFiles,
        openPreviosFolder: openPreviosFolder,
        reset: reset
    };
})(jQuery);