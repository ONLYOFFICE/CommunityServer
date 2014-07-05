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
var LdapSettings = new function () {
    var NULL_PERCENT = 0,
        DEFAULT_LDAP_PORT = 389,
        alreadyChecking = false,
        already = false,
        progressBarIntervalId = null,
        objectSettings = null;

    jq("#ldapSettingsCheckbox").click(function () {
        var $ldapSettingsMainContainer = jq(".ldap-settings-main-container"),
            $ldapSettingsUserContainer = jq(".ldap-settings-user-container"),
            $ldapSettingsGroupContainer = jq(".ldap-settings-group-container"),
            $ldapSettingsAuthContainer = jq(".ldap-settings-auth-container"),
            $ldapSettingsGroupCheckbox = jq("#ldapSettingsGroupCheckbox"),
            $ldapSettingsAuthCheckbox = jq("#ldapSettingsAuthenticationCheckbox");
        if (jq(this).is(":checked")) {
            $ldapSettingsUserContainer.find("input").removeAttr("disabled");
            $ldapSettingsGroupCheckbox.removeAttr("disabled");
            $ldapSettingsUserContainer.removeClass("ldap-settings-disabled");
            jq(".ldap-settings-label-checkbox:not(.ldap-settings-never-disable)").removeClass("ldap-settings-disabled");
            if ($ldapSettingsGroupCheckbox.is(":checked")) {
                $ldapSettingsGroupContainer.find("input").removeAttr("disabled");
                $ldapSettingsGroupContainer.removeClass("ldap-settings-disabled");
            }
            $ldapSettingsAuthCheckbox.removeAttr("disabled");
            if ($ldapSettingsAuthCheckbox.is(":checked")) {
                $ldapSettingsAuthContainer.find("input").removeAttr("disabled");
                $ldapSettingsAuthContainer.removeClass("ldap-settings-disabled");             
            }
        } else {
            $ldapSettingsMainContainer.find("input:not(#ldapSettingsCheckbox)").attr("disabled", "");
            jq(".ldap-settings-label-checkbox:not(.ldap-settings-never-disable)").addClass("ldap-settings-disabled");
            $ldapSettingsUserContainer.addClass("ldap-settings-disabled");
            $ldapSettingsGroupContainer.addClass("ldap-settings-disabled");
            $ldapSettingsAuthContainer.addClass("ldap-settings-disabled");
        }
    });

    jq("#ldapSettingsGroupCheckbox").click(function () {
        var $ldapSettingsGroupContainer = jq(".ldap-settings-group-container");
        if (jq(this).is(":checked")) {
            $ldapSettingsGroupContainer.find("input").removeAttr("disabled");
            $ldapSettingsGroupContainer.removeClass("ldap-settings-disabled");
        } else {
            $ldapSettingsGroupContainer.find("input").attr("disabled", "");
            $ldapSettingsGroupContainer.addClass("ldap-settings-disabled");
        }
    });

    jq("#ldapSettingsAuthenticationCheckbox").click(function () {
        var $ldapSettingsAuthContainer = jq(".ldap-settings-auth-container");
        if (jq(this).is(":checked")) {
            $ldapSettingsAuthContainer.find("input").removeAttr("disabled");
            $ldapSettingsAuthContainer.removeClass("ldap-settings-disabled");
        } else {
            $ldapSettingsAuthContainer.find("input").attr("disabled", "");
            $ldapSettingsAuthContainer.addClass("ldap-settings-disabled");
        }
    });

    function restoreDefaultSettings() {
        PopupKeyUpActionProvider.CloseDialog();
        LdapSettingsController.RestoreDefaultSettings(function (result) {
            HideRequiredError();
            loadSettings(result);
            jq("#ldapSettingsError").addClass("display-none");
            jq("#ldapSettingsReady").addClass("display-none");
            jq(".ldap-settings-progressbar-container").addClass("display-none");
            setPercentsExactly(NULL_PERCENT);
        });
    }

    function cancelDialog() {
        PopupKeyUpActionProvider.CloseDialog();
        already = false;
    }

    function isInt(str) {
        var n = ~~Number(str);
        return String(n) === str && n >= 0;
    }

    function restoreDefault() {
        StudioBlockUIManager.blockUI("#ldapSettingsInviteDialog", 500, 500, 0);
        PopupKeyUpActionProvider.EnterAction = "LdapSettings.restoreDefaultSettings();";
    }

    function disableInterface() {
        var $ldapSettingsMainContainer = jq(".ldap-settings-main-container");
        setPercentsExactly(NULL_PERCENT);
        jq("#ldapSettingsError").addClass("display-none");
        jq("#ldapSettingsReady").addClass("display-none");
        jq(".ldap-settings-progressbar-container").removeClass("display-none");
        jq(".ldap-settings-save").addClass("disable");
        jq(".ldap-settings-restore-default-settings").addClass("disable");
        $ldapSettingsMainContainer.addClass("ldap-settings-disabled-all");
        $ldapSettingsMainContainer.find("input").attr("disabled", "");
        $ldapSettingsMainContainer.off("click", ".ldap-settings-save");
        $ldapSettingsMainContainer.off("click", ".ldap-settings-restore-default-settings");
    }

    function enableInterface() {
        var $ldapSettingsCheckbox = jq("#ldapSettingsCheckbox"),
            $ldapSettingsGroupCheckbox = jq("#ldapSettingsGroupCheckbox"),
            $ldapSettingsAuthCheckbox = jq("#ldapSettingsAuthenticationCheckbox"),
            $ldapSettingsMainContainer = jq(".ldap-settings-main-container");
        
        jq(".ldap-settings-save").removeClass("disable");
        jq(".ldap-settings-restore-default-settings").removeClass("disable");
        $ldapSettingsMainContainer.removeClass("ldap-settings-disabled-all");
        $ldapSettingsCheckbox.removeAttr("disabled");
        if ($ldapSettingsCheckbox.is(":checked")) {
            jq(".ldap-settings-user-container").find("input").removeAttr("disabled");
            $ldapSettingsGroupCheckbox.removeAttr("disabled");
            if ($ldapSettingsGroupCheckbox.is(":checked")) {
                jq(".ldap-settings-group-container").find("input").removeAttr("disabled");
            }
            $ldapSettingsAuthCheckbox.removeAttr("disabled");
            if ($ldapSettingsAuthCheckbox.is(":checked")) {
                jq(".ldap-settings-auth-container").find("input").removeAttr("disabled");
            }
        }
        $ldapSettingsMainContainer.on("click", ".ldap-settings-save", saveSettings);
        $ldapSettingsMainContainer.on("click", ".ldap-settings-restore-default-settings", restoreDefault);
    }

    function saveSettings() {
        if (already) {
            return;
        }
        already = true;
        HideRequiredError();
        var $ldapSettingsMainContainer = jq(".ldap-settings-main-container"),
            result = false,
            enableLdapAuthentication = jq("#ldapSettingsCheckbox").is(":checked"),
            server = jq("#ldapSettingsServer").val(),
            userDN = jq("#ldapSettingsUserDN").val(),
            bindAttribute = jq("#ldapSettingsBindAttribute").val(),
            portNumber = jq("#ldapSettingsPortNumber").val(),
            userFilter = jq("#ldapSettingsUserFilter").val(),
            loginAttribute = jq("#ldapSettingsLoginAttribute").val(),
            groupMembership = jq("#ldapSettingsGroupCheckbox").is(":checked"),
            groupDN = jq("#ldapSettingsGroupDN").val(),
            userAttribute = jq("#ldapSettingsUserAttribute").val(),
            groupName = jq("#ldapSettingsGroupName").val(),
            groupAttribute = jq("#ldapSettingsGroupAttribute").val(),
            authentication = jq("#ldapSettingsAuthenticationCheckbox").is(":checked"),
            login = jq("#ldapSettingsLogin").val(),
            password = jq("#ldapSettingsPassword").val();

        $ldapSettingsMainContainer.find(".ldap-settings-empty-field").addClass("display-none");
        $ldapSettingsMainContainer.find(".ldap-settings-incorrect-number").addClass("display-none");
        if (enableLdapAuthentication) {
            if (server == "") {
                result = true;
                ShowRequiredError(jq("#ldapSettingsServer"));
            }
            if (userDN == "") {
                result = true;
                ShowRequiredError(jq("#ldapSettingsUserDN"));
            }
            if (bindAttribute == "") {
                result = true;
                ShowRequiredError(jq("#ldapSettingsBindAttribute"));
            }
            if (portNumber == "") {
                result = true;
                jq("#ldapSettingsPortNumberError").text(ASC.Resources.Master.Resource.LdapSettingsEmptyField);
                ShowRequiredError(jq("#ldapSettingsPortNumber"));
            } else if (!isInt(portNumber)) {
                result = true;
                jq("#ldapSettingsPortNumberError").text(ASC.Resources.Master.Resource.LdapSettingsIncorrectPortNumber);
                ShowRequiredError(jq("#ldapSettingsPortNumber"));
            }
            if (loginAttribute == "") {
                result = true;
                ShowRequiredError(jq("#ldapSettingsLoginAttribute"));
            }
            if (groupMembership) {
                if (groupDN == "") {
                    result = true;
                    ShowRequiredError(jq("#ldapSettingsGroupDN"));
                }
                if (userAttribute == "") {
                    result = true;
                    ShowRequiredError(jq("#ldapSettingsUserAttribute"));
                }
                if (groupAttribute == "") {
                    result = true;
                    ShowRequiredError(jq("#ldapSettingsGroupAttribute"));
                }
            }
            if (authentication) {
                if (login == "") {
                    result = true;
                    ShowRequiredError(jq("#ldapSettingsLogin"));
                }
                if (password == "") {
                    result = true;
                    ShowRequiredError(jq("#ldapSettingsPassword"));
                }
            }
        }

        if (result) {
            already = false;
            return;
        }
        if (portNumber == "") {
            portNumber = DEFAULT_LDAP_PORT;
        }
        objectSettings = {
            EnableLdapAuthentication: enableLdapAuthentication,
            Server: server,
            UserDN: userDN,
            BindAttribute: bindAttribute,
            PortNumber: portNumber,
            UserFilter: userFilter,
            LoginAttribute: loginAttribute,
            GroupMembership: groupMembership,
            GroupDN: groupDN,
            UserAttribute: userAttribute,
            GroupName: groupName,
            GroupAttribute: groupAttribute,
            Authentication: authentication,
            Login: login,
            Password: password
        }
        if (jq("#ldapSettingsCheckbox").is(":checked")) {
            StudioBlockUIManager.blockUI("#ldapSettingsImportUserLimitPanel", 500, 500, 0);
            PopupKeyUpActionProvider.EnableEsc = false;
            PopupKeyUpActionProvider.EnterAction = "LdapSettings.continueSaveSettings();";
        } else {
            continueSaveSettings();
        }
    }

    function continueSaveSettings() {
        PopupKeyUpActionProvider.CloseDialog();
        disableInterface();
        LdapSettingsController.SaveSettings(JSON.stringify(objectSettings), function () {
            progressBarIntervalId = setInterval(checkStatus, 600);
        });
    }

    function checkStatus() {
        if (alreadyChecking) {
            return;
        }
        alreadyChecking = true;
        LdapSettingsController.GetStatus(function (status) {
            if (status && status.value) {
                setPercents(status.value.Percents);
                setStatus(status.value.Status);
                if (status.value.Completed) {
                    clearInterval(progressBarIntervalId);
                    enableInterface();
                    if (status.value.Error) {
                        setTimeout(function () {
                            var $ldapSettingsError = jq("#ldapSettingsError");
                            $ldapSettingsError.text(status.value.Error);
                            $ldapSettingsError.removeClass("display-none");
                            setStatus("");
                            setPercentsExactly(NULL_PERCENT);
                        }, 500);
                    } else {
                        var $ldapSettingsReady = jq("#ldapSettingsReady");
                        $ldapSettingsReady.removeClass("display-none");
                    }
                    already = false;
                }
            }
            alreadyChecking = false;
        });
    }

    function setPercentsExactly(percents) {
        jq(".asc-progress-value").css("width", percents + "%");
        jq("#ldapSettingsPercent").text(percents + "% ");
    }

    function setPercents(percents) {
        jq(".asc-progress-value").animate({ "width": percents + "%" });
        jq("#ldapSettingsPercent").text(percents + "% ");
    }

    function setStatus(status) {
        jq("#ldapSettingsStatus").text(status);
    }

    function loadSettings(result) {
        if (result) {
            var settings = result.value;
            jq("#ldapSettingsCheckbox").prop("checked", settings["EnableLdapAuthentication"]);
            jq("#ldapSettingsServer").val(settings["Server"]);
            jq("#ldapSettingsUserDN").val(settings["UserDN"]);
            jq("#ldapSettingsBindAttribute").val(settings["BindAttribute"]);
            jq("#ldapSettingsPortNumber").val(settings["PortNumber"]);
            jq("#ldapSettingsUserFilter").val(settings["UserFilter"]);
            jq("#ldapSettingsLoginAttribute").val(settings["LoginAttribute"]);

            jq("#ldapSettingsGroupCheckbox").prop("checked", settings["GroupMembership"]);
            jq("#ldapSettingsGroupDN").val(settings["GroupDN"]);
            jq("#ldapSettingsUserAttribute").val(settings["UserAttribute"]);
            jq("#ldapSettingsGroupName").val(settings["GroupName"]);
            jq("#ldapSettingsGroupAttribute").val(settings["GroupAttribute"]);

            jq("#ldapSettingsAuthenticationCheckbox").prop("checked", settings["Authentication"]);
            jq("#ldapSettingsLogin").val(settings["Login"]);
            jq("#ldapSettingsPassword").val(settings["Password"]);

            disableNeededBlocks(settings["EnableLdapAuthentication"], settings["GroupMembership"], settings["Authentication"]);
        }
    }

    function disableNeededBlocks(enableLdapAuthentication, groupMembership, authentication) {
        var $ldapSettingsGroupContainer = jq(".ldap-settings-group-container"),
            $ldapSettingsAuthContainer = jq(".ldap-settings-auth-container");
        if (!enableLdapAuthentication) {
            jq(".ldap-settings-main-container").find("input:not(#ldapSettingsCheckbox)").attr("disabled", "");
            jq(".ldap-settings-label-checkbox:not(.ldap-settings-never-disable)").addClass("ldap-settings-disabled");
            jq(".ldap-settings-user-container").addClass("ldap-settings-disabled");
            $ldapSettingsGroupContainer.addClass("ldap-settings-disabled");
            $ldapSettingsAuthContainer.addClass("ldap-settings-disabled");
        } else {
            if (!groupMembership) {
                $ldapSettingsGroupContainer.find("input").attr("disabled", "");
                $ldapSettingsGroupContainer.addClass("ldap-settings-disabled");
            }
            if (!authentication) {
                $ldapSettingsAuthContainer.find("input").attr("disabled", "");
                $ldapSettingsAuthContainer.addClass("ldap-settings-disabled");
            }
        }
    }

    jq(window).load(function () {
        var $ldapSettingsMainContainer = jq(".ldap-settings-main-container"),
            $ldapSettingsInviteDialog = jq("#ldapSettingsInviteDialog"),
            $ldapSettingsImportUserLimitPanel = jq("#ldapSettingsImportUserLimitPanel");
        $ldapSettingsMainContainer.on("click", ".ldap-settings-save", saveSettings);
        $ldapSettingsMainContainer.on("click", ".ldap-settings-restore-default-settings", restoreDefault);
        $ldapSettingsInviteDialog.on("click", ".ldap-settings-ok", restoreDefaultSettings);
        $ldapSettingsInviteDialog.on("click", ".ldap-settings-cancel", cancelDialog);
        $ldapSettingsImportUserLimitPanel.on("click", ".ldap-settings-ok", continueSaveSettings);
        $ldapSettingsImportUserLimitPanel.on("click", ".ldap-settings-cancel", cancelDialog);
        $ldapSettingsImportUserLimitPanel.on("click", ".cancelButton", cancelDialog);
        jq(document).keyup(function (e) {
            /* Escape Key */
            if (!jq("#ldapSettingsImportUserLimitPanel").is(":hidden") && e.keyCode == 27) {
                cancelDialog();
            }
        });
    });
    return {
        restoreDefaultSettings: restoreDefaultSettings,
        continueSaveSettings: continueSaveSettings
    }
};