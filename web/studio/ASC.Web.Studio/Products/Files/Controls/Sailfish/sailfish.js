/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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