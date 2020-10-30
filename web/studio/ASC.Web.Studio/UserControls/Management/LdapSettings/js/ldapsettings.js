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


var LdapSettings = new function () {
    var NULL_PERCENT = 0,
        DEFAULT_LDAP_PORT = 389,
        alreadyChecking = false,
        already = false,
        progressBarIntervalId = null,
        objectSettings = null,
        isRestoreDefault = jq(".ldap-settings-main-container").hasClass("ldap-settings-is-default"),
        isMono = jq(".ldap-settings-main-container").hasClass("ldap-settings-is-mono");

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
            if ($ldapSettingsAuthCheckbox.is(":checked") || isMono) {
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
        var $ldapSettingsSave = jq(".ldap-settings-save"),
            $ldapSettingsMainContainer = jq(".ldap-settings-main-container");
        PopupKeyUpActionProvider.CloseDialog();

        Teamlab.getLdapDefaultSettings({},
        {
            success: function(params, result) {
                HideRequiredError();
                loadSettings(result);
                jq("#ldapSettingsError").addClass("display-none");
                jq(".ldap-settings-progressbar-container").addClass("display-none");
                setPercentsExactly(NULL_PERCENT);

                jq(".ldap-settings-restore-default-settings").addClass("disable");
                $ldapSettingsMainContainer.off("click", ".ldap-settings-restore-default-settings");

                $ldapSettingsSave.removeClass("disable");
                $ldapSettingsMainContainer.on("click", ".ldap-settings-save", saveSettings);

                jq(".ldap-settings-sync-users").addClass("disable");

                isRestoreDefault = true;
            },
            error: function(params, response) {
            }
        });
    }

    function syncUsersLDAP() {
        if (already) {
            return;
        }

        already = true;
        HideRequiredError();

        jq("#ldapSettingsError").addClass("display-none");

        PopupKeyUpActionProvider.CloseDialog();
        disableInterface();

        Teamlab.syncLdap({},
        {
            success: function(params, response) {
                progressBarIntervalId = setInterval(checkStatus, 600);
            },
            error: function(params, response) {
            }
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
        StudioBlockUIManager.blockUI("#ldapSettingsInviteDialog", 500);
        PopupKeyUpActionProvider.EnterAction = "LdapSettings.restoreDefaultSettings();";
    }

    function disableInterface() {
        var $ldapSettingsMainContainer = jq(".ldap-settings-main-container");
        setPercentsExactly(NULL_PERCENT);
        jq("#ldapSettingsError").addClass("display-none");
        jq(".ldap-settings-progressbar-container").removeClass("display-none");
        jq(".ldap-settings-save").addClass("disable");
        jq(".ldap-settings-restore-default-settings").addClass("disable");
        jq(".ldap-settings-sync-users").addClass("disable");
        $ldapSettingsMainContainer.addClass("ldap-settings-disabled-all");
        $ldapSettingsMainContainer.find("input").attr("disabled", "");
        $ldapSettingsMainContainer.off("click", ".ldap-settings-save");
        $ldapSettingsMainContainer.off("click", ".ldap-settings-restore-default-settings");
        $ldapSettingsMainContainer.off("click", ".ldap-settings-sync-users");
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
            if ($ldapSettingsAuthCheckbox.is(":checked") || isMono) {
                jq(".ldap-settings-auth-container").find("input").removeAttr("disabled");
            }
        }
        $ldapSettingsMainContainer.on("click", ".ldap-settings-save", saveSettings);
        $ldapSettingsMainContainer.on("click", ".ldap-settings-restore-default-settings", restoreDefault);
        $ldapSettingsMainContainer.on("click", ".ldap-settings-sync-users", syncUsersLDAP);
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
            portNumber = jq("#ldapSettingsPortNumber").val(),
            userFilter = jq("#ldapSettingsUserFilter").val(),
            loginAttribute = jq("#ldapSettingsLoginAttribute").val(),
            firstNameAttribute = jq("#ldapSettingsFirstNameAttribute").val(),
            secondNameAttribute = jq("#ldapSettingsSecondNameAttribute").val(),
            mailAttribute = jq("#ldapSettingsMailAttribute").val(),
            titleAttribute = jq("#ldapSettingsTitleAttribute").val(),
            mobilePhoneAttribute = jq("#ldapSettingsMobilePhoneAttribute").val(),
            locationAttribute = jq("#ldapSettingsLocationAttribute").val(),
            groupMembership = jq("#ldapSettingsGroupCheckbox").is(":checked"),
            groupDN = jq("#ldapSettingsGroupDN").val(),
            userAttribute = jq("#ldapSettingsUserAttribute").val(),
            groupFilter = jq("#ldapSettingsGroupFilter").val(),
            groupAttribute = jq("#ldapSettingsGroupAttribute").val(),
            groupNameAttribute = jq("#ldapSettingsGroupNameAttribute").val(),
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
                if (groupNameAttribute == "") {
                    result = true;
                    ShowRequiredError(jq("#ldapSettingsGroupNameAttribute"));
                }
            }
            if (authentication || isMono) {
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
            PortNumber: portNumber,
            UserFilter: userFilter,
            LoginAttribute: loginAttribute,
            FirstNameAttribute: firstNameAttribute,
            SecondNameAttribute: secondNameAttribute,
            MailAttribute: mailAttribute,
            TitleAttribute: titleAttribute,
            MobilePhoneAttribute: mobilePhoneAttribute,
            LocationAttribute: locationAttribute,
            GroupMembership: groupMembership,
            GroupDN: groupDN,
            UserAttribute: userAttribute,
            GroupFilter: groupFilter,
            GroupAttribute: groupAttribute,
            GroupNameAttribute: groupNameAttribute,
            Authentication: authentication,
            Login: login,
            Password: password
        }
        if (jq("#ldapSettingsCheckbox").is(":checked")) {
            StudioBlockUIManager.blockUI("#ldapSettingsImportUserLimitPanel", 500);
            PopupKeyUpActionProvider.EnableEsc = false;
            PopupKeyUpActionProvider.EnterAction = "LdapSettings.continueSaveSettings();";
        } else {
            continueSaveSettings();
        }
    }

    function continueSaveSettings() {
        PopupKeyUpActionProvider.CloseDialog();
        disableInterface();
        Teamlab.saveLdapSettings({},
            JSON.stringify(objectSettings),
            {
                success: function (params, response) {
                    progressBarIntervalId = setInterval(checkStatus, 600);
                },
                error: function(params, response) {
                }
            });
    }

    function checkStatus() {
        if (alreadyChecking) {
            return;
        }
        alreadyChecking = true;
        Teamlab.getLdapStatus({},
        {
            success: function(params, status) {
                if (!status) {
                    alreadyChecking = false;
                    return;
                }

                setPercents(status.percents);
                setStatus(status.status);

                if (!status.completed) {
                    alreadyChecking = false;
                    return;
                }

                clearInterval(progressBarIntervalId);
                enableInterface();

                if (status.error) {
                    setTimeout(function() {
                            var $ldapSettingsError = jq("#ldapSettingsError");
                            $ldapSettingsError.text(status.error);
                            $ldapSettingsError.removeClass("display-none");
                            setStatus("");
                            setPercentsExactly(NULL_PERCENT);
                        },
                        500);
                } else {
                    setStatus(ASC.Resources.Master.LdapSettingsSuccess);
                    if (!isRestoreDefault) {
                        jq(".ldap-settings-sync-users").removeClass("disable");
                    }
                    if (!isRestoreDefault) {
                        jq(".ldap-settings-sync-users").removeClass("disable");
                    }
                }

                jq(".ldap-settings-save").addClass("disable");
                jq(".ldap-settings-main-container").off("click", ".ldap-settings-save");
                if (isRestoreDefault) {
                    jq(".ldap-settings-restore-default-settings").addClass("disable");
                    jq(".ldap-settings-main-container").off("click", ".ldap-settings-restore-default-settings");
                }

                already = false;
                alreadyChecking = false;
            },
            error: function(params, response) {
            }
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

    function loadSettings(settings) {
        if (!settings || typeof(settings) !== "object")
            return;

        jq("#ldapSettingsCheckbox").prop("checked", settings["enableLdapAuthentication"]);
        jq("#ldapSettingsServer").val(settings["server"]);
        jq("#ldapSettingsUserDN").val(settings["userDN"]);
        jq("#ldapSettingsPortNumber").val(settings["portNumber"]);
        jq("#ldapSettingsUserFilter").val(settings["userFilter"]);
        jq("#ldapSettingsLoginAttribute").val(settings["loginAttribute"]);
        jq("#ldapSettingsFirstNameAttribute").val(settings["firstNameAttribute"]);
        jq("#ldapSettingsSecondNameAttribute").val(settings["secondNameAttribute"]);
        jq("#ldapSettingsMailAttribute").val(settings["mailAttribute"]);
        jq("#ldapSettingsTitleAttribute").val(settings["titleAttribute"]);
        jq("#ldapSettingsMobilePhoneAttribute").val(settings["mobilePhoneAttribute"]);
        jq("#ldapSettingsLocationAttribute").val(settings["locationAttribute"]);

        jq("#ldapSettingsGroupCheckbox").prop("checked", settings["groupMembership"]);
        jq("#ldapSettingsGroupDN").val(settings["groupDN"]);
        jq("#ldapSettingsUserAttribute").val(settings["userAttribute"]);
        jq("#ldapSettingsGroupFilter").val(settings["groupFilter"]);
        jq("#ldapSettingsGroupAttribute").val(settings["groupAttribute"]);
        jq("#ldapSettingsGroupNameAttribute").val(settings["groupNameAttribute"]);

        jq("#ldapSettingsAuthenticationCheckbox").prop("checked", settings["authentication"]);
        jq("#ldapSettingsLogin").val(settings["login"]);
        jq("#ldapSettingsPassword").val(settings["password"]);

        disableNeededBlocks(settings["enableLdapAuthentication"],
            settings["groupMembership"],
            settings["authentication"]);
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

    jq(window).on("load", function () {
        var $ldapSettingsMainContainer = jq(".ldap-settings-main-container"),
            $ldapSettingsInviteDialog = jq("#ldapSettingsInviteDialog"),
            $ldapSettingsImportUserLimitPanel = jq("#ldapSettingsImportUserLimitPanel"),
            $ldapSettingsSave = jq(".ldap-settings-save"),
            $ldapSettingsRestoreDefaultSettings = jq(".ldap-settings-restore-default-settings");

        $ldapSettingsMainContainer.on("click", ".ldap-settings-restore-default-settings", restoreDefault);
        $ldapSettingsMainContainer.on("click", ".ldap-settings-sync-users", syncUsersLDAP);
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
        jq(".ldap-settings-main-container input").change(function () {
            isRestoreDefault = false;
            if ($ldapSettingsSave.hasClass("disable")) {
                $ldapSettingsSave.removeClass("disable");
                $ldapSettingsMainContainer.on("click", ".ldap-settings-save", saveSettings);
            }
            if ($ldapSettingsRestoreDefaultSettings.hasClass("disable")) {
                $ldapSettingsRestoreDefaultSettings.removeClass("disable");
                $ldapSettingsMainContainer.on("click", ".ldap-settings-restore-default-settings", restoreDefault);
            }
        });
        jq(".ldap-settings-main-container .textEdit").keyup(function () {
            isRestoreDefault = false;
            if ($ldapSettingsSave.hasClass("disable")) {
                $ldapSettingsSave.removeClass("disable");
                $ldapSettingsMainContainer.on("click", ".ldap-settings-save", saveSettings);
            }
            if ($ldapSettingsRestoreDefaultSettings.hasClass("disable")) {
                $ldapSettingsRestoreDefaultSettings.removeClass("disable");
                $ldapSettingsMainContainer.on("click", ".ldap-settings-restore-default-settings", restoreDefault);
            }
        });
    });
    return {
        restoreDefaultSettings: restoreDefaultSettings,
        continueSaveSettings: continueSaveSettings,
        syncUsersLDAP: syncUsersLDAP
    };
};