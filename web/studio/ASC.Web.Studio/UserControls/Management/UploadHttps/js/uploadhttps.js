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

UploadHttpsCert = (function() {
    var certFile;
    var saveButton = jq("#save");
    var passwordInput = jq("#certPwd");
    var fileInput = jq("#certFile .name");
    var certUploader = jq("#certUploader");
    var statusContainer = jq("#statusContainer");

    var config = {
        browse_button: "certUploader",
        runtimes: ASC.Resources.Master.UploadDefaultRuntimes,
        multi_selection: false,
        flash_swf_url: ASC.Resources.Master.UploadFlashUrl,
        url: "ajaxupload.ashx?type=ASC.Web.Studio.UserControls.Management.CertificateUploader,ASC.Web.Studio",
        filters: [
            { title: "PFX Files", extensions: "pfx" }
        ],
    };

    var uploader = new window.plupload.Uploader(config);

    uploader.init();

    uploader.bind('FilesAdded', function (up, files) {
        certFile = files[0];
        fileInput.text(certFile.name);
        
        jq("#certFile").show();
        
        if (passwordInput.val()) {
            saveButton.removeClass("disable");
        }
        this.disableBrowse();
        certUploader.addClass("gray");
        statusContainer.addClass("display-none");
    });

    uploader.bind('BeforeUpload', function () {
        jq.blockUI();
    });

    uploader.bind('Error', function (up, error) {
        if (error.code == -601) {
            statusContainer.addClass("error-popup").removeClass("display-none").text(ASC.Resources.Master.Resource.UploadHttpsFileTypeError);
        }
    });

    uploader.bind('FileUploaded', function (up, file, info) {
        UploadHttps.CheckAttachment(function (checkResponse) {
            if (checkResponse.value.status == "success") {
                uploadHttpsCertificate();
            } else {
                StudioBlockUIManager.blockUI('#checkAttachmentContainer', 500, 400, 0);
            }
        });
    });
    
    saveButton.on('click', function () {
        if (!jq(this).hasClass("disable"))
            uploader.start();
    });
    
    passwordInput.on("keyup", function () {
        passwordInput.parents(".settings-block .requiredField").removeClass("requiredFieldError");
        if (passwordInput.val() && certFile) {
            saveButton.removeClass("disable");
        } else {
            saveButton.addClass("disable");
        }
    });

    jq("#okButton").on("click", function() {
        uploadHttpsCertificate();
        jq.unblockUI();
    });

    jq("#certFile .trash").on("click", function () {
        uploader.removeFile(certFile.id);
        certFile = undefined;
        
        saveButton.addClass("disable");
        uploader.disableBrowse(false);
        certUploader.removeClass("gray");
        jq("#certFile").hide();
        statusContainer.addClass("display-none");
    });

    jq("#cancelButton").on("click", function() {
        jq.unblockUI();
    });

    function uploadHttpsCertificate() {
        UploadHttps.UploadCertificate(certFile.name, passwordInput.val(),
            function(uploadResponse) {
                passwordInput.val('');
                statusContainer.addClass(uploadResponse.value.status == "error" ? "error-popup" : "success-popup")
                    .removeClass("display-none").text(uploadResponse.value.message);
                jq.unblockUI();
                if (uploadResponse.value.status == "success") {
                    location.reload();
                }
            });
    }

    return {};
})(jQuery);