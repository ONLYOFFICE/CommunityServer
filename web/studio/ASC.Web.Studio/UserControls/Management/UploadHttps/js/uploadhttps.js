/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


UploadHttpsCert = (function () {
    var uploadData = null;
    var saveButton = jq("#save");
    var passwordInput = jq("#certPwd");
    var fileInput = jq("#certFile .name");
    var certUploader = jq("#certUploader");
    var statusContainer = jq("#statusContainer");

    createFileuploadInput(certUploader);

    var uploader = jq("#fileupload").fileupload({
        url: "ajaxupload.ashx?type=ASC.Web.Studio.UserControls.Management.CertificateUploader,ASC.Web.Studio",
        autoUpload: false,
        singleFileUploads: true,
        sequentialUploads: true
    });


    uploader.bind("fileuploadadd", function (e, data) {

        if (getFileExtension(data.files[0].name) != ".pfx") {
            uploadData = null;
            statusContainer.addClass("error-popup").removeClass("display-none").text(ASC.Resources.Master.Resource.UploadHttpsFileTypeError);
        } else {
            uploadData = data;

            fileInput.text(data.files[0].name);

            jq("#certFile").show();

            if (passwordInput.val()) {
                saveButton.removeClass("disable");
            }

            jq("#fileupload").prop("disabled", true);
            certUploader.addClass("gray");
            statusContainer.addClass("display-none");
        }
    });

    uploader.bind("fileuploadstart", function () {
        jq.blockUI();
    });

    uploader.bind("fileuploadfail", function (e, data) {
        uploadData = null;
        jq.unblockUI();

        var msg = data.errorThrown || data.textStatus;
        if (data.jqXHR && data.jqXHR.responseText)
            msg = jq.parseJSON(data.jqXHR.responseText).Message;

        toastr.error(msg);
    });

    uploader.bind("fileuploaddone", function (e, data) {
        window.UploadHttps.CheckAttachment(function (checkResponse) {
            if (checkResponse.value.status == "success") {
                uploadHttpsCertificate();
            } else {
                StudioBlockUIManager.blockUI('#checkAttachmentContainer', 500, 400, 0);
            }
        });
    });

    saveButton.on("click", function () {
        if (!jq(this).hasClass("disable"))
            uploadData.submit();
    });

    passwordInput.on("keyup", function () {
        passwordInput.parents(".settings-block .requiredField").removeClass("requiredFieldError");
        if (passwordInput.val() && uploadData) {
            saveButton.removeClass("disable");
        } else {
            saveButton.addClass("disable");
        }
    });

    jq("#okButton").on("click", function () {
        uploadHttpsCertificate();
        jq.unblockUI();
    });

    jq("#certFile .trash").on("click", function () {
        uploadData = null;

        saveButton.addClass("disable");
        jq("#fileupload").prop("disabled", false);
        certUploader.removeClass("gray");
        jq("#certFile").hide();
        statusContainer.addClass("display-none");
    });

    jq("#cancelButton").on("click", function () {
        jq.unblockUI();
    });

    function uploadHttpsCertificate() {
        window.UploadHttps.UploadCertificate(uploadData.files[0].name, passwordInput.val(),
            function (uploadResponse) {
                passwordInput.val("");
                statusContainer.addClass(uploadResponse.value.status == "error" ? "error-popup" : "success-popup")
                    .removeClass("display-none").text(uploadResponse.value.message);
                jq.unblockUI();
                if (uploadResponse.value.status == "success") {
                    location.reload();
                }
            });
    }

    function createFileuploadInput(buttonObj) {
        var inputObj = jq("<input/>")
            .attr("id", "fileupload")
            .attr("type", "file")
            .css("width", "0")
            .css("height", "0");

        inputObj.appendTo(buttonObj.parent());

        buttonObj.on("click", function (e) {
            e.preventDefault();
            jq("#fileupload").click();
        });
    }

    function getFileExtension(fileTitle) {
        if (typeof fileTitle == "undefined" || fileTitle == null) {
            return "";
        }
        fileTitle = fileTitle.trim();
        var posExt = fileTitle.lastIndexOf(".");
        return 0 <= posExt ? fileTitle.substring(posExt).trim().toLowerCase() : "";
    }

    return {};
})(jQuery);