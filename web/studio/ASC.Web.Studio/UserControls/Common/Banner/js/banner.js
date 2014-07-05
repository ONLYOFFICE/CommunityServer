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


jq(function () {
    if (jq("#joinAffilliateBanner").length) {
        jq("#joinAffilliateBanner").on("click", function () {
            //track event

            jq("#joinAffilliateBanner").trackEvent("affilliate_system", "action-click", "affilliate-banner");

            AjaxPro.timeoutPeriod = 1800000;
            AjaxPro.onLoading = function (b) {
                if (b) {
                    LoadingBanner.displayLoading();
                } else {
                    LoadingBanner.hideLoading();
                }
            };
            AjaxPro.BannerController.JoinToAffiliateProgram(function (result) {
                var res = result.value;
                if (res.rs1 == "1" && res.rs2) {
                    location.href = res.rs2;
                } else if (res.rs1 == "0") {
                    jq("#errorAffilliateBanner").text(res.rs2);
                }
            });
            return false;
        });

    }

    if (jq("#joinTeamlabBanner").length) {
        jq("#joinTeamlabBanner").on("click", function () {
            jq("#joinTeamlabBanner").trackEvent("personal_portal", "action-click", "join-teamlab-banner");
            return true;
        });
    }
    if (jq("#chromeStoreBanner").length) {
        jq("#chromeStoreBanner").on("click", function () {
            jq("#chromeStoreBanner").trackEvent("personal_portal", "action-click", "chrome-store-banner");
            return true;
        });
    }
});