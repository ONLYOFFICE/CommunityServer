/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


SmtpSettingsManager = new function () {
    this.Initialize = function () {

        checkAuthFields();

        jq('#smtpSettingsButtonSave').on("click", function () {
            SmtpSettingsManager.Save();
        });

        jq('#smtpSettingsButtonTest').on("click", function () {
            SmtpSettingsManager.Test();
        });

        jq("#smtpSettingsButtonDefault").on("click", function () {
            SmtpSettingsManager.RestoreDefaultSettings();
        });

        jq('#smtpSettingsAuthentication').on("change", function () {
            checkAuthFields();
        });
    };

    this.Save = function () {
        var $settingsBlock = jq(".smtp-settings-block");

        HideRequiredError();

        if (jq("#smtpSettingsAuthentication").is(":checked")) {
            if (!$settingsBlock.find(".host-login").find(".smtp-settings-field").val()) {
                ShowRequiredError(jq($settingsBlock.find(".host-login input")));
                return;
            }
            if (!$settingsBlock.find(".host-password").find(".smtp-settings-field").val()) {
                ShowRequiredError(jq($settingsBlock.find(".host-password input")));
                return;
            }
        }
        
        blockFields();
        SmtpSettings.Save(getSettings(), function(result) {
            if (result.error != null) {
                toastr.error(result.error.Message);
            }
            unblockFields();
        });
    };

    this.Test = function () {
        blockFields();
        SmtpSettings.Test(getSettings(), function(result) {
            if (result.error != null) {
                toastr.error(result.error.Message);
            }
            unblockFields();
        });
    };

    this.RestoreDefaultSettings = function() {
        blockFields();
        SmtpSettings.RestoreDefaults(function(response) {
            if (response.error) {
                toastr.error(response.error.Message);
            }
            clearSmtpSettings();
            unblockFields();
        });
    };

    var checkAuthFields = function() {
        var $settingsBlock = jq(".smtp-settings-block"),
            isDisable = !jq('#smtpSettingsAuthentication').is(':checked');
        $settingsBlock.find(".host-login, .host-password").find(".smtp-settings-field").attr('disabled', isDisable);
    };

    var getSettings = function() {
        var $settingsBlock = jq(".smtp-settings-block"),
            data = {
                Host: $settingsBlock.find(".host .smtp-settings-field").val(),
                SenderDisplayName: $settingsBlock.find(".display-name .smtp-settings-field").val(),
                SenderAddress: $settingsBlock.find(".email-address .smtp-settings-field").val(),
                EnableSSL: jq('#smtpSettingsEnableSsl').is(':checked')
            },
            port = $settingsBlock.find(".port .smtp-settings-field").val();

        if (port) {
            data.Port = port;
        }

        var requireAuthentication = jq('#smtpSettingsAuthentication').is(':checked');
        if (requireAuthentication) {
            data.CredentialsUserName = $settingsBlock.find(".host-login .smtp-settings-field").val();
            data.CredentialsUserPassword = $settingsBlock.find(".host-password .smtp-settings-field").val();
        }

        return data;
    };

    var blockFields = function() {
        jq(".smtp-settings-block input").attr("disabled", true);
        LoadingBanner.showLoaderBtn("#smtpSettingsContainer");
    };

    var unblockFields = function() {
        var $settingsBlock = jq(".smtp-settings-block");
        $settingsBlock.find("input").attr("disabled", false);
        if (!jq("#smtpSettingsAuthentication").is(":checked")) {
            $settingsBlock.find(".host-login, .host-password").find(".smtp-settings-field").attr('disabled', true);
        }
        LoadingBanner.hideLoaderBtn("#smtpSettingsContainer");
    };

    var clearSmtpSettings = function() {
        jq(".smtp-settings-block").find(".smtp-settings-field").val("");
        jq("#smtpSettingsAuthentication").removeAttr("checked");
        jq("#smtpSettingsEnableSsl").removeAttr("checked");
    };
};

jq(function() {
    SmtpSettingsManager.Initialize();
});