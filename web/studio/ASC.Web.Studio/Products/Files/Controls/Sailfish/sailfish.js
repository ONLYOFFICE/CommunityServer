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


window.ASC.Sailfish = (function () {
    var isInit = false;

    var base64ToBlob = function(b64Data, contentType, sliceSize) {
        contentType = contentType || '';
        sliceSize = sliceSize || 512;

        var byteCharacters = atob(b64Data);
        var byteArrays = [];

        for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
            var slice = byteCharacters.slice(offset, offset + sliceSize);
            var byteNumbers = new Array(slice.length);

            for (var i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }

            var byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }

        var blob = new window.Blob(byteArrays, { type: contentType });
        return blob;
    };

    var blobToFile = function(blob, fileName) {
        return new window.File([blob], fileName, { type: blob.type });
    };

    var fileToDataTransfer = function(file) {
        var dataTransfer = new window.ClipboardEvent('').clipboardData || new window.DataTransfer();
        dataTransfer.items.add(file);
        return dataTransfer;
    };

    var makeFakeFileList = function(input, file) {
        var fileList = [file];
        fileList.__proto__ = Object.create(window.FileList.prototype);

        Object.defineProperty(input, 'files', {
            value: fileList,
            writeable: false,
        });
    };

    var uploadBase64 = function (fileName, base64Data, contentType) {
        var blob = base64ToBlob(base64Data, contentType);
        var file = blobToFile(blob, fileName);

        var fileInputObj = jq("#fileupload");
        var fileInput = fileInputObj[0];

        try {
            var dataTransfer = fileToDataTransfer(file);
            fileInput.files = dataTransfer.files;
        } catch (error) {
            console.log(error);
            makeFakeFileList(fileInput, file);
        }

        fileInputObj.change();
    };

    var init = function () {
        if (isInit === false) {
            isInit = true;

            jq(document).on("click", "#createSailfishDocument:not(.disable), #createSailfishSpreadsheet:not(.disable), #createSailfishPresentation:not(.disable)", function () {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Folders.typeNewDoc = this.id.replace("createSailfish", "").toLowerCase();
                ASC.Files.Folders.createNewDoc();
            });

            jq(document).on("click", "#createSailfishNewFolder:not(.disable)", function () {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Folders.createFolder();
            });

            jq("#buttonUploadSailfish").on("click", function (e) {
                e.preventDefault();

                var userAgent = (navigator && navigator.userAgent) || '';

                if (userAgent.includes("NeedEmulateUpload") && window.emulateUpload) {
                    window.emulateUpload(uploadBase64);
                } else {
                    jq("#fileupload").click();
                }
            });
        }
    };

    return {
        init: init,
    };
})();

(function ($) {
    $(function () {
        if (ASC.Sailfish) {
            ASC.Sailfish.init();
        }
    });
})(jQuery);