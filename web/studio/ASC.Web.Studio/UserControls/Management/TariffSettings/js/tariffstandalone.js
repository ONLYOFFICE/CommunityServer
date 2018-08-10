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


var TariffStandalone = new function () {
    var isInit = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }


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

                var licenseResult = result.Message;
                if (!result.Success) {
                    toastr.error(licenseResult);
                    licenseResult = "";
                }
                jq("#licenseKeyText").text(licenseResult);

                licenseKeyEdit();
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

    return {
        init: init,

        activate: activate,
        licenseKeyEdit: licenseKeyEdit,
    };
};

jq(function () {
    TariffStandalone.init();

    jq("#activatePanel").on("click", "#activateButton:not(.disable)", TariffStandalone.activate);

    jq("#policyAccepted").click(TariffStandalone.licenseKeyEdit);
});