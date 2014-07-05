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

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
jq(function() {
    jq('#saveMailDomainSettingsBtn').click(MailDomainSettingsManager.SaveSettings);
    jq('input[name="signInType"]').click(MailDomainSettingsManager.SwitchSignInType);
    jq('#addTrustDomainBtn').click(MailDomainSettingsManager.AddTrustedDomain);

})

MailDomainSettingsManager = new function() {

    this.SwitchSignInType = function() {

        if (jq('#trustedMailDomains').is(':checked')) {

            jq('#offMailDomainsDescription').hide();
            jq('#allMailDomainsDescription').hide();
            jq('#trustedMailDomainsDescription').show();
        }
        else if (jq('#allMailDomains').is(':checked')) {
            jq('#offMailDomainsDescription').hide();
            jq('#trustedMailDomainsDescription').hide();
            jq('#allMailDomainsDescription').show();
        }
        else {
            jq('#trustedMailDomainsDescription').hide();
            jq('#allMailDomainsDescription').hide();
            jq('#offMailDomainsDescription').show();
        }
    }

    this.RemoveTrustedDomain = function(number) {
        jq('#studio_domain_box_' + number).remove();
        var count = jq('div[id^="studio_domain_box_"]').length;
        if (count < 10)
            jq('#addTrustDomainBtn').show();
    };

    this.AddTrustedDomain = function() {
        var maxNumb = -1;
        jq('div[id^="studio_domain_box_"]').each(function(i, pel) {

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
    }

    this.SaveSettings = function() {

        var domains = new Array();
        var type = '';
        if (jq('#trustedMailDomains').is(':checked')) {

            type = jq('#trustedMailDomains').val();
            jq('input[id^="studio_domain_"]').each(function(i, pel) {
                domains.push(jq(this).val());
            });
        }
        else if (jq('#allMailDomains').is(':checked')) {
            type = jq('#allMailDomains').val();
        }
        else
            type = jq('#offMailDomains').val();

        var inviteUsersAsVisitors = jq("#cbxInviteUsersAsVisitors").is(":checked");

        AjaxPro.onLoading = function(b) {
                if (b)
                    LoadingBanner.showLoaderBtn("#studio_domainSettings");
                else
                    LoadingBanner.hideLoaderBtn("#studio_domainSettings");
        };
       
        MailDomainSettingsController.SaveMailDomainSettings(type, domains, inviteUsersAsVisitors, function(result) {
            var res = result.value;
            LoadingBanner.showMesInfoBtn("#studio_domainSettings", res.Message, res.Status == 1 ? "success" : "error");
        });
    }

}