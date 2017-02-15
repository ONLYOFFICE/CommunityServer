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


var TariffStandalone = new function () {
    var isInit = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        jq.switcherAction("#switcherRequest", "#requestPanel");

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

                    var licenseResult = result.Message;
                    if (!result.Success) {
                        licenseResult = "";
                        toastr.error(ASC.Resources.Master.Resource.LicenseKeyError);
                    }
                    jq("#licenseKeyText").text(licenseResult);

                    licenseKeyEdit();
                }
            });
    };

    var licenseKeyEdit = function () {
        var err = !jq("#licenseKeyText").text().length;
        jq("#licenseKeyText").toggleClass("error", err);

        if (jq("#policyAccepted").length && !jq("#policyAccepted").is(":checked")) {
            err = true;
        }
        jq("#activateButton").toggleClass("disable", err);
    };

    var activate = function () {
        var licenseKey = jq("#licenseKeyText").text();

        if (jq("#licenseKeyText").length && !licenseKey.length) {
            var res = { Status: 0, Message: ASC.Resources.Master.Resource.LicenseKeyError };

            onActivate(res);
            return;
        }

        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#activatePanel");
            } else {
                LoadingBanner.hideLoaderBtn("#activatePanel");
            }
        };
        TariffStandaloneController.ActivateLicenseKey(function (result) {
            onActivate(result.value);
        });
    };

    var onActivate = function (res) {
        if (res.Status) {
            toastr.success(ASC.Resources.Master.Resource.OperationSuccededMsg);
            setTimeout(function () {
                location.reload(true);
            }, 500);
            return;
        }

        toastr.error(res.Message);
    };

    var request = function () {
        var fname = jq(".text-edit-fname").val().trim();
        var lname = jq(".text-edit-lname").val().trim();
        var title = jq(".text-edit-title").val().trim();
        var email = jq(".text-edit-email").val().trim();
        var phone = jq(".text-edit-phone").val().trim();
        var ctitle = jq(".text-edit-ctitle").val().trim();
        var csize = jq(".text-edit-csize").val();
        var site = jq(".text-edit-site").val().trim();
        var message = jq(".text-edit-message").val().trim();

        if (!fname.length
            || !lname.length
            || !email.length
            || !phone.length
            || !ctitle.length
            || !csize.length
            || !site.length) {
            toastr.error(ASC.Resources.Master.Resource.ErrorEmptyField);
            return;
        }

        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#requestPanel");
            } else {
                LoadingBanner.hideLoaderBtn("#requestPanel");
            }
        };
        TariffStandaloneController.SendRequest(fname, lname, title, email, phone, ctitle, csize, site, message,
            function (result) {
                if (result.error != null) {
                    toastr.error(result.error.Message);
                    return;
                }
                toastr.success(ASC.Resources.Master.Resource.SendTariffRequest);
            });
    };

    return {
        init: init,

        activate: activate,
        licenseKeyEdit: licenseKeyEdit,

        request: request,
    };
};

jq(function () {
    TariffStandalone.init();

    jq("#activatePanel").on("click", "#activateButton:not(.disable)", TariffStandalone.activate);

    jq("#licenseRequest").click(TariffStandalone.request);

    jq("#policyAccepted").click(TariffStandalone.licenseKeyEdit);
});