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
        sb += '<input type="text" value="" id="studio_domain_box_' + maxNumb + '" class="textEdit" maxlength="60" style="width:300px;"/>'
        sb += '<a class="removeDomain" href="javascript:void(0);" onclick="MailDomainSettingsManager.RemoveTrustedDomain(\'' + maxNumb + '\');"><img alt="" align="absmiddle" border="0" src="' + StudioManager.GetImage("trash_16.png") + '"/></a>';
        sb += '</div>';

        jq('#studio_domainListBox').append(sb);

        var count = jq('div[id^="studio_domain_box_"]').length;
        if (count >= 10)
            jq('#addTrustDomainBtn').hide();
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