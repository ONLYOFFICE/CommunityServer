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


if (typeof (ASC) == 'undefined')
    ASC = {};
if (typeof (ASC.Controls) == 'undefined')
    ASC.Controls = {};

ASC.Controls.FirstTimeManager = function() {

    this.OnAfterSaveRequiredData = null;
    this.OnAfterSaveData = null;

    this.SaveRequiredData = function(parentCalllback) {
        ASC.Controls.EmailAndPasswordManager.SaveRequiredData(parentCalllback);
    };
};

ASC.Controls.EmailAndPasswordManager = new function() {

    this.PassText = '';
    this.changeIt = '';
    this.ok = '';
    this.wrongPass = '';
    this.emptyPass = '';
    this.wrongEmail = '';

    this.init = function (changeItText, okText, wrongPassText, emptyPassText, wrongEmailText) {
        ASC.Controls.EmailAndPasswordManager.changeIt = changeItText;
        ASC.Controls.EmailAndPasswordManager.ok = okText;
        ASC.Controls.EmailAndPasswordManager.wrongPass = wrongPassText;
        ASC.Controls.EmailAndPasswordManager.emptyPass = emptyPassText;
        ASC.Controls.EmailAndPasswordManager.wrongEmail = wrongEmailText;

        uploadInit();
    };

    var uploadInit = function () {
        var upload =
            new AjaxUpload("licenseKey", {
                action: 'ajaxupload.ashx?type=ASC.Web.Studio.HttpHandlers.LicenseUploader,ASC.Web.Studio',
                onChange: function (file, ext) {
                    jq("#licenseKeyText").removeClass("error");
                    LoadingBanner.showLoaderBtn(".step");
                },
                onComplete: function (file, response) {
                    LoadingBanner.hideLoaderBtn(".step");
                    try {
                        var result = jq.parseJSON(response);
                    } catch (e) {
                        result = { Success: false };
                    }

                    if (result.Success) {
                        jq("#licenseKeyText").text(result.Message);
                    } else {
                        jq("#licenseKeyText").text(ASC.Resources.Master.Resource.LicenseKeyError).addClass("error");
                    }
                }
            });
    };

    this.ShowChangeEmailAddress = function() {
        var email = jQuery.trim(jq('.emailAddress').html());
        jq('.emailAddress').html('');
        jq('.emailAddress').append('<input type="textbox" id="newEmailAddress" maxlength="64" class="textEdit newEmail">');
        jq('.emailAddress #newEmailAddress').val(email);
        jq('.changeEmail').html('');
    };

    this.AcceptNewEmailAddress = function() {
        var email = jq('.changeEmail #dvChangeMail #newEmailAddress').val();

        if (email == '')
            return;

        jq('#requiredStep .emailBlock .email .emailAddress').html(email);
        jq('.changeEmail #dvChangeMail').html('');
        jq('.changeEmail #dvChangeMail').append('<a class="info baseLinkAction" onclick="ASC.Controls.EmailAndPasswordManager.ShowChangeEmailAddress();">' + ASC.Controls.EmailAndPasswordManager.changeIt + '</a>');
    };

    this.SaveRequiredData = function (parentCallback) {
        if (jq("#saveSettingsBtn").hasClass("disable")) {
            return;
        }

        var email = jQuery.trim(jq('#requiredStep .emailBlock .email .emailAddress #newEmailAddress').val()); //
        if (email == '' || email == null)
            email = jQuery.trim(jq('#requiredStep .emailBlock .email .emailAddress').html());
        var pwd = jq('.passwordBlock .pwd #newPwd').val();
        var cpwd = jq('.passwordBlock .pwd #confPwd').val();
        var promocode = jq('.passwordBlock .promocode #promocode_input').val();

        if (email == '' || !jq.isValidEmail(email)) {
            var res = { "Status": 0, "Message": ASC.Controls.EmailAndPasswordManager.wrongEmail };
            if (parentCallback != null)
                parentCallback(res);
            return;
        }

        if (pwd != cpwd || pwd == '') {

            if (pwd != cpwd) {
                jq(".passwordBlock .pwd #newPwd ,.passwordBlock .pwd #confPwd").css("border-color", "#DF1B1B");
            }

            if (pwd == '') {
                jq(".passwordBlock .pwd #newPwd").css("border-color", "#DF1B1B");
            }

            res = { "Status": 0, "Message": pwd == '' ? ASC.Controls.EmailAndPasswordManager.emptyPass : ASC.Controls.EmailAndPasswordManager.wrongPass };
            if (parentCallback != null)
                parentCallback(res);
            return;
        }

        if (jq("#licenseKeyText").length && jq("#licenseKeyText").hasClass("error")) {
            res = { "Status": 0, "Message": ASC.Resources.Master.Resource.LicenseKeyError };
            if (parentCallback != null)
                parentCallback(res);
            return;
        }

        if (jq(".license-accept").length && !jq(".license-accept input[type=checkbox]").is(":checked")) {
            toastr.error(ASC.Resources.Master.Resource.LicenseAgreementsError);
            return;
        }

        window.onbeforeunload = null;
        AjaxPro.timeoutPeriod = 1800000;
        EmailAndPasswordController.SaveData(email, pwd, jq('#studio_lng').val(), promocode, function (result) {

            if (parentCallback != null)
                parentCallback(result.value);
        });
    };
};

jq(function() {
    if (jQuery.trim(jq('.emailAddress').html()) == '') {
        ASC.Controls.EmailAndPasswordManager.ShowChangeEmailAddress();
    }
});