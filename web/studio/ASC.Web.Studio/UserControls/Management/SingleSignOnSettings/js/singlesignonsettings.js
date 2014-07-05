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

var SsoSettings = (function () {
    var already = false,
        canSave = false,
        SAML = "SAML",
        JWT = "JWT",
        HMAC_SHA256 = "HMAC SHA-256",
        RSA_SHA256 = "RSA SHA-256",
        X509 = "X.509";

    function canSaveSettings() {
        canSave = true;
        jq(".sso-settings-save").removeClass("disable");
    }

    function saveSettings(e) {
        if (already || !canSave) {
            return;
        }
        already = true;
        HideRequiredError();
        var result = false,
            publicKey = jq(".sso-settings-public-key-area").val(),
            issuer = jq(".sso-settings-issuer-url").val(),
            ssoEndPoint = jq(".sso-settings-endpoint-url").val(),
            $ssoSettingsCheckbox = jq("#ssoSettingsCheckbox"),
            enableSso = $ssoSettingsCheckbox.is(":checked");
        if (enableSso) {
            if (publicKey == "") {
                result = true;
                ShowRequiredError(jq("#ssoSettingsPublicKeyError"));
            }
            if (issuer == "") {
                result = true;
                ShowRequiredError(jq("#ssoSettingsIssuerUrlError"));
            }
            if (ssoEndPoint == "") {
                result = true;
                ShowRequiredError(jq("#ssoSettingsEndpointUrlError"));
            }
            if (result) {
                already = false;
                return;
            }
        }
        var $ssoSettingsStatus = jq(".sso-settings-status"),
            $ssoSettingsLoader = jq(".sso-settings-loader"),
            $ssoSettingsContainer = jq(".sso-settings-container"),
            $ssoSettingsMainContainer = jq(".sso-settings-main-container"),
            $ssoSettingsTokenType = jq(".sso-settings-token-type"),
            $ssoSettingsValidationType = jq(".sso-settings-validation-type");

        $ssoSettingsMainContainer.addClass("sso-settings-disabled");
        $ssoSettingsMainContainer.find("input, textarea, select, checkbox").attr("disabled", "");
        $ssoSettingsStatus.text(ASC.Resources.Master.Resource.LoadingProcessing);
        $ssoSettingsStatus.removeClass("display-none");
        $ssoSettingsLoader.removeClass("display-none");
        
        SsoSettingsController.SaveSettings(JSON.stringify({
            EnableSso: enableSso,
            TokenType: $ssoSettingsTokenType.val(),
            ValidationType: $ssoSettingsValidationType.val(),
            PublicKey: publicKey,
            Issuer: issuer,
            SsoEndPoint: ssoEndPoint,
            SloEndPoint: jq(".slo-settings-endpoint-url").val()
        }), function (result) {
            $ssoSettingsCheckbox.removeAttr("disabled");
            $ssoSettingsMainContainer.removeClass("sso-settings-disabled");
            if ($ssoSettingsCheckbox.is(":checked")) {
                $ssoSettingsContainer.find("input, textarea, select:not(.sso-settings-validation-type)").removeAttr("disabled");
                if ($ssoSettingsTokenType.val() == JWT) {
                    $ssoSettingsValidationType.removeAttr("disabled");
                }
            }
            if (result.value == "") {
                result.value = ASC.Resources.Master.Resource.SavedTitle;
            } else {
                $ssoSettingsStatus.addClass("errorText");
            }
            $ssoSettingsLoader.addClass("display-none");
            $ssoSettingsStatus.text(result.value);
            canSave = false;
            jq(".sso-settings-save").addClass("disable");
            already = false;
        });
    }

    jq(window).load(function () {
        var $ssoSettingsMainContainer = jq(".sso-settings-main-container");
        $ssoSettingsMainContainer.on("click", ".sso-settings-save", saveSettings);
        jq(".sso-settings-token-type").change(function (e) {
            canSaveSettings();
            if (jq(e.currentTarget).val() == JWT) {
                jq(".sso-settings-validation-type").removeAttr("disabled");
                jq(".sso-settings-validation-text").removeClass("sso-settings-disabled");
            } else if (jq(e.currentTarget).val() == SAML) {
                var $ssoSettingsValidationType = jq(".sso-settings-validation-type");
                $ssoSettingsValidationType.find("option[value='X.509']").attr("selected", "");
                $ssoSettingsValidationType.attr("disabled", "");
                jq(".sso-settings-validation-text").addClass("sso-settings-disabled");
            }
        });
        jq(".sso-settings-validation-type").change(function (e) {
            var val = jq(e.currentTarget).val();

            if (val == X509) {

            } else if (val == RSA_SHA256) {

            } else if (val == HMAC_SHA256) {

            }
            canSaveSettings();
        });
        jq("#ssoSettingsCheckbox").click(function () {
            var $ssoSettingsContainer = jq(".sso-settings-container"),
                $ssoSettingsTokenType = jq(".sso-settings-token-type");
            canSaveSettings();
            if (jq(this).is(":checked")) {
                $ssoSettingsContainer.removeClass("sso-settings-disabled");
                $ssoSettingsContainer.find("input, textarea, select:not(.sso-settings-validation-type)").removeAttr("disabled");
                if ($ssoSettingsTokenType.val() == JWT) {
                    jq(".sso-settings-validation-type").removeAttr("disabled");
                }
            } else {
                $ssoSettingsContainer.addClass("sso-settings-disabled");
                $ssoSettingsContainer.find("input, textarea, select").attr("disabled", "");
            }
        });
        $ssoSettingsMainContainer.on("keydown", ".sso-settings-public-key-area", canSaveSettings);
        $ssoSettingsMainContainer.on("keydown", ".sso-settings-issuer-url", canSaveSettings);
        $ssoSettingsMainContainer.on("keydown", ".sso-settings-endpoint-url", canSaveSettings);
        $ssoSettingsMainContainer.on("keydown", ".slo-settings-endpoint-url", canSaveSettings);
    });
})();