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


ASC.CRM.FileUploader = (function () {
    var onPreUploadStart = function () {
        FileUploadManager.DisableBrowseBtn(true);
        jq("#pm_upload_btn").hide();

        ASC.CRM.FileUploader.OnBeginCallback_function();
    };

    var onUploadDone = function (e, data) {
        var res = jq.parseJSON(data.result);
        ASC.CRM.FileUploader.fileIDs.push(res.Data);
        FileUploadManager._uploadDone(e, data);
    };

    var onUploadStop = function (e, data) {
        FileUploadManager.ClearFiles();
        ASC.CRM.FileUploader.OnAllUploadCompleteCallback_function();
        FileUploadManager._uploadStop(e, data);
    };

    var onRenderItemInUploadingProcess = function (data) {
        var $progressObj = jq('#fu_item_{0} div.studioFileUploaderProgressBar'.format(data.id));
        if ($progressObj.length === 0) {
            jq('#fu_item_' + data.id).replaceWith(jq.tmpl(FileUploadManager.renderedItemTemplate, data));
        } else {
            $progressObj.replaceWith(jq.tmpl(FileUploadManager.renderedItemTemplate, data).find('div.studioFileUploaderProgressBar:first'));
        }
    };

    return {
        fileNames: function () {
            return jq(FileUploadManager.GetFiles()).map(function (i, file) {
                return file.name;
            });
        },

        fileIDs: new Array(),

        activateUploader: function () {

            FileUploadManager.Init({
                DropZone: 'pm_DragDropHolder',
                TargetContainerID: 'history_uploadContainer',
                BrowseButton: 'pm_upload_btn',
                UploadButton: "fakeUploadButton",
                MaxSize: window.uploadFileSizeLimit,
                FileUploadHandler: 'ASC.Web.CRM.Classes.FileUploaderHandler, ASC.Web.CRM',
                Data: { 'UserID': Teamlab.profile.id },
                DeleteLinkCSSClass: 'pm_deleteLinkCSSClass',
                LoadingImageCSSClass: 'pm_loadingCSSClass loader-middle',
                CompleteCSSClass: 'pm_completeCSSClass',
                DeleteAfterUpload: false,
                Events:
                    {
                        OnPreUploadStart: onPreUploadStart,
                        OnUploadDone: onUploadDone,
                        OnUploadStop: onUploadStop,
                        OnRenderItemInUploadingProcess: onRenderItemInUploadingProcess
                    },
                EmptyFileErrorMsg: ASC.CRM.Resources.CRMCommonResource.EmptyFileErrorMsg,
                FileSizeErrorMsg: ASC.CRM.Resources.CRMCommonResource.FileSizeErrorMsg
            });
        },

        OnAllUploadCompleteCallback_function: function () {
        },

        OnBeginCallback_function: function () {
        },

        start: function () {
            FileUploadManager.StartUpload();
        },

        getUploadFileCount: function () {
            return FileUploadManager.GetFiles().length;
        },

        showFileUploaderDialog: function () {
            var margintop = jq(window).scrollTop() - 135;
            margintop = margintop + 'px';

            jq.blockUI({
                message: jq("#fileUploaderPanel"),
                css: {
                    left: '50%',
                    top: '25%',
                    opacity: '1',
                    border: 'none',
                    padding: '0px',
                    width: '550px',

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
                fadeOut: 0
            });
        },

        reinit: function () {
            jq("#history_uploadContainer").empty();

            FileUploadManager.DisableBrowseBtn(false);
            jq("#pm_upload_btn").show();

            ASC.CRM.FileUploader.fileIDs.clear();
        }
    };
})(jQuery);