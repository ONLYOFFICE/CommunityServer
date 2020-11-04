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

        jq("#licenseKey").click(function (e) {
            e.preventDefault();
            jq("#uploadButton").click();
        });

        var upload = jq("#uploadButton")
            .fileupload({
                url: "ajaxupload.ashx?type=ASC.Web.Studio.HttpHandlers.LicenseUploader,ASC.Web.Studio",
            })
            .bind("fileuploadstart", function () {
                jq("#licenseKeyText").removeClass("error");
                LoadingBanner.showLoaderBtn(".step");
            })
            .bind("fileuploaddone", function (e, data) {
                LoadingBanner.hideLoaderBtn(".step");
                try {
                    var result = jq.parseJSON(data.result);
                } catch (e) {
                    result = {Success: false};
                }

                jq("#licenseKeyText").html(result.Message);
                if (!result.Success) {
                    jq("#licenseKeyText").addClass("error");
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
        var amiid = jq("#amiid").val();

        if (email == '' || !jq.isValidEmail(email)) {
            var res = { "Status": 0, "Message": ASC.Controls.EmailAndPasswordManager.wrongEmail };
            if (parentCallback != null)
                parentCallback(res);
            return;
        }

        if (pwd == '') {
            jq(".passwordBlock .pwd #newPwd").css("border-color", "#DF1B1B");

            res = { "Status": 0, "Message": ASC.Controls.EmailAndPasswordManager.emptyPass };
            if (parentCallback != null)
                parentCallback(res);
            return;
        }

        if (!(new XRegExp(jq(".passwordBlock .pwd #newPwd").data("regex"), "ig")).test(pwd)) {
            jq(".passwordBlock .pwd #newPwd").css("border-color", "#DF1B1B");

            res = { "Status": 0, "Message": jq(".passwordBlock .pwd #newPwd").data("help") };
            if (parentCallback != null)
                parentCallback(res);
            return;
        }

        if (pwd != cpwd) {
            jq(".passwordBlock .pwd #newPwd ,.passwordBlock .pwd #confPwd").css("border-color", "#DF1B1B");

            res = { "Status": 0, "Message": ASC.Controls.EmailAndPasswordManager.wrongPass };
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

        var analytics = jq("#analyticsAcceptedOpenSource").is(":checked");
        var subscribeFromSite = jq("#subscribeFromSite").is(":checked");

        window.hashPassword(pwd, function (passwordHash) {
            window.onbeforeunload = null;
            AjaxPro.timeoutPeriod = 1800000;

            EmailAndPasswordController.SaveData(email,
                passwordHash,
                jq('#studio_lng').val() || jq('#studio_lng').data('default'),
                promocode,
                amiid,
                analytics,
                subscribeFromSite,
                function (result) {
                    if (parentCallback != null)
                        parentCallback(result.value);
                });
        });
    };
};

jq(function() {
    if (jQuery.trim(jq('.emailAddress').html()) == '') {
        ASC.Controls.EmailAndPasswordManager.ShowChangeEmailAddress();
    }
});