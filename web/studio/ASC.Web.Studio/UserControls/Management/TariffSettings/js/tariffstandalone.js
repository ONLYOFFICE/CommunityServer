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
                jq("#licenseKeyText").html(licenseResult);

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