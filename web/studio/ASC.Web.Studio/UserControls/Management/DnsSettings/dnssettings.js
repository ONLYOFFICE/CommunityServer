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


;
var DnsSettings = new function () {

    this.CheckEnableDns = function (isChecked) {
        jq('#studio_dnsName').attr('disabled', !isChecked);
        jq('#studio_dnsName').removeClass('with-error');
    };

    this.SaveDnsSettings = function (bthObj) {
        if (jq(bthObj).hasClass("disable")) return;
        
        var dnsName = jq.trim(jq('#studio_dnsName').val()),
            enableDns = jq('#studio_enableDnsName').is(':checked');
        if (enableDns === true && dnsName === "") {
            jq("#studio_dnsName").addClass("with-error");
            return;
        } else {
            jq("#studio_dnsName").removeClass("with-error");
        }

        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#studio_enterDns");
            } else {
                LoadingBanner.hideLoaderBtn("#studio_enterDns");
            }
        };

        DnsSettingsAjax.SaveDnsSettings(dnsName, enableDns,
            function (result) {
                if (result.value.rs1 == "0") {
                    LoadingBanner.showMesInfoBtn("#studio_enterDns", result.value.rs2, "error");
                } else if (result.value.rs1 == "1") {
                    LoadingBanner.showMesInfoBtn("#studio_enterDns", result.value.rs2 || ASC.Resources.Master.Resource.SuccessfullySaveSettingsMessage, "success");
                }
            }
        );
    };
};