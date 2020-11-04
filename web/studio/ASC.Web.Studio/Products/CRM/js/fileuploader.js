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