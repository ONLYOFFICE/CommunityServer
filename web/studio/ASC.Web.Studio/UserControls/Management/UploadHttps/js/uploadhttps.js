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

UploadHttpsCert = (function() {
    this.fileName = "";

    var upload = new AjaxUpload('certUploader', {
        action: 'ajaxupload.ashx?type=ASC.Web.Studio.UserControls.Management.CertificateUploader,ASC.Web.Studio',
        onSubmit: function() {
            jq.blockUI();
        },
        onComplete: function (file, response) {
            UploadHttpsCert.fileName = jq.parseJSON(response).Message;
            UploadHttps.CheckAttachment(function(checkResponse) {
                if (checkResponse.value.status == "success") {
                    uploadHttpsCertificate();
                } else {
                    StudioBlockUIManager.blockUI('#checkAttachmentContainer', 500, 400, 0);
                }
            });
        }
    });


    upload.disable();

    jq("#certPwd").on("keyup", function() {
        if (jq(this).val()) {
            upload.enable();
        } else {
            upload.disable();
        }
        jq(this).parents(".settings-block .requiredField").removeClass("requiredFieldError");
    });

    jq("#certUploader").on("click", function() {
        var cert = jq("#certPwd");
        if (!cert.val()) {
            cert.parents(".settings-block .requiredField").addClass("requiredFieldError");
        }
    });

    jq("#okButton").on("click", function() {
        uploadHttpsCertificate();
        jq.unblockUI();
    });

    jq("#cancelButton").on("click", function() {
        jq.unblockUI();
    });

    function uploadHttpsCertificate() {
        UploadHttps.UploadCertificate(UploadHttpsCert.fileName, jq("#certPwd").val(),
            function(uploadResponse) {
                jq("#certPwd").val('');
                jq("#statusContainer").addClass(uploadResponse.value.status == "error" ? "error-popup" : "success-popup")
                    .removeClass("display-none").text(uploadResponse.value.message);
                jq.unblockUI();
                if (uploadResponse.value.status == "success") {
                    location.reload();
                }
            });
    }

    return {
        fileName: fileName
    };
})(jQuery);