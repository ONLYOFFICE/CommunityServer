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
ASC.CRM.FileUploader = (function () {
    var onPreUploadStart = function () {
        FileUploadManager._uploader.disableBrowse(true);
        jq("#pm_upload_btn").hide();

        ASC.CRM.FileUploader.OnBeginCallback_function();
    };

    var onFileUploaded = function (up, file, resp) {
        var data = jq.parseJSON(resp.response);
        ASC.CRM.FileUploader.fileIDs.push(data.Data);
        FileUploadManager.FileUploaded(up, file, resp);
    };

    var onUploadComplete = function (up, files) {
        FileUploadManager._uploader.files.clear();
        ASC.CRM.FileUploader.OnAllUploadCompleteCallback_function();
        FileUploadManager.UploadComplete(up, files);
    };

    return {
        fileNames: function () {
            return jq(FileUploadManager._uploader.files).map(function (i, file) {
                return file.name;
            });
        },

        fileIDs: new Array(),

        activateUploader: function () {
            FileUploadManager.InitFileUploader({
                DropPanel: 'pm_DragDropHolder',
                Container: 'pm_DragDropHolder',
                TargetContainerID: 'history_uploadContainer',
                BrowseButton: 'pm_upload_btn',
                UploadButton: "fakeUploadButton",
                MaxSize: uploadFileSizeLimit,
                FileUploadHandler: 'ASC.Web.CRM.Classes.FileUploaderHandler, ASC.Web.CRM',
                Data: { 'UserID': Teamlab.profile.id },
                DeleteLinkCSSClass: 'pm_deleteLinkCSSClass',
                LoadingImageCSSClass: 'pm_loadingCSSClass',
                CompleteCSSClass: 'pm_completeCSSClass',
                DeleteAfterUpload: false,
                Events:
                    {
                        OnPreUploadStart: onPreUploadStart,
                        OnFileUploaded: onFileUploaded,
                        OnUploadComplete: onUploadComplete
                    },
                Switcher:
                    {
                        ToFlash: uploadToFlashButton,
                        ToBasic: uploadToBasicButton
                    }
            });

            if ((typeof FileReader == 'undefined' && !jQuery.browser.safari) || FileUploadManager.IsFlash(FileUploadManager._uploader))
                jq('#switcher').show();
        },

        OnAllUploadCompleteCallback_function: function () {
        },

        OnBeginCallback_function: function () {
        },

        start: function () {
            FileUploadManager._uploader.start();
        },

        getUploadFileCount: function () {
            return FileUploadManager._uploader.files.length;
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

            FileUploadManager._uploader.disableBrowse(false);
            jq("#pm_upload_btn").show();

            ASC.CRM.FileUploader.fileIDs.clear();
        }
    };
})(jQuery);