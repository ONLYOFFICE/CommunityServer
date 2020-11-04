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


jq(function () {
    jq('#saveMailDomainSettingsBtn').click(MailDomainSettingsManager.SaveSettings);
    jq('input[name="signInType"]').click(MailDomainSettingsManager.SwitchSignInType);
    jq('#addTrustDomainBtn').click(MailDomainSettingsManager.AddTrustedDomain);
    jq("input[type=radio][name=signInType][checked=checked]").prop("checked", true);
});

MailDomainSettingsManager = new function() {

    this.SwitchSignInType = function () {
        if (jq('#trustedMailDomains').is(':checked')) {
            jq('#offMailDomainsDescription').hide();
            jq('#allMailDomainsDescription').hide();
            jq('#trustedMailDomainsDescription').show();
            jq('#domainSettingsCbxContainer').show();
        } else if (jq('#allMailDomains').is(':checked')) {
            jq('#offMailDomainsDescription').hide();
            jq('#trustedMailDomainsDescription').hide();
            jq('#allMailDomainsDescription').show();
            jq('#domainSettingsCbxContainer').show();
        } else {
            jq('#trustedMailDomainsDescription').hide();
            jq('#allMailDomainsDescription').hide();
            jq('#offMailDomainsDescription').show();
            jq('#domainSettingsCbxContainer').hide();
        }
    };

    this.RemoveTrustedDomain = function(number) {
        jq('#studio_domain_box_' + number).remove();
        var count = jq('div[id^="studio_domain_box_"]').length;
        if (count < 10)
            jq('#addTrustDomainBtn').show();
    };

    this.AddTrustedDomain = function () {
        var maxNumb = -1;
        jq('div[id^="studio_domain_box_"]').each(function (i, pel) {

            var n = parseInt(jq(this).attr('name'));
            if (n > maxNumb)
                maxNumb = n + 1;
        });

        maxNumb++;
        var sb = new String();
        sb += '<div name="' + maxNumb + '" id="studio_domain_box_' + maxNumb + '" class="clearFix" style="margin-bottom:15px;">';
        sb += '<input type="text" value="" id="studio_domain_' + maxNumb + '" class="textEdit" maxlength="60" style="width:300px;"/>';
        sb += '<a class="removeDomain" href="javascript:void(0);" onclick="MailDomainSettingsManager.RemoveTrustedDomain(\'' + maxNumb + '\');"><img alt="" align="absmiddle" border="0" src="' + StudioManager.GetImage("trash_16.png") + '"/></a>';
        sb += '</div>';

        jq('#studio_domainListBox').append(sb);

        var count = jq('div[id^="studio_domain_box_"]').length;
        if (count >= 10)
            jq('#addTrustDomainBtn').hide();

        document.getElementById('studio_domain_' + maxNumb).focus();
    };

    this.SaveSettings = function () {

        if (jq(this).hasClass("disable")) return;

        var domains = new Array();
        var type = '';
        if (jq('#trustedMailDomains').is(':checked')) {

            type = jq('#trustedMailDomains').val();
            jq('input[id^="studio_domain_"]').each(function (i, pel) {
                domains.push(jq(this).val());
            });
        }
        else if (jq('#allMailDomains').is(':checked')) {
            type = jq('#allMailDomains').val();
        }
        else
            type = jq('#offMailDomains').val();

        var inviteUsersAsVisitors = jq("#cbxInviteUsersAsVisitors").is(":checked");

        AjaxPro.onLoading = function (b) {
            if (b)
                LoadingBanner.showLoaderBtn("#studio_domainSettings");
            else
                LoadingBanner.hideLoaderBtn("#studio_domainSettings");
        };

        MailDomainSettingsController.SaveMailDomainSettings(type, domains, inviteUsersAsVisitors, function (result) {
            var res = result.value;
            LoadingBanner.showMesInfoBtn("#studio_domainSettings", res.Message, res.Status == 1 ? "success" : "error");
        });
    };

}