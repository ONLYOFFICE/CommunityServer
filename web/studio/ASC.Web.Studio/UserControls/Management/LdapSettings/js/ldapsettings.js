/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
    var alreadyChecking = false,
        already = false,
        previousSettings = null,
        progressBarIntervalId = null,
        objectSettings = null,
        isRestoreDefault = jq(".ldap-settings-main-container").hasClass("ldap-settings-is-default"),

        currentCron = null,
        currentSettings = null,

        syncInProgress = false,
        constants = {
            NULL_PERCENT: 0,
            SSL_LDAP_PORT: 636,
            DEFAULT_LDAP_PORT: 389,
            GET_STATUS_TIMEOUT: 1000
        },

        ldapCertificateProblem = {
            CertExpired: -2146762495,
            CertValidityPeriodNesting: -2146762494,
            CertRole: -2146762493,
            CertPathLenConst: -2146762492,
            CertCritical: -2146762491,
            CertPurpose: -2146762490,
            CertIssuerChaining: -2146762489,
            CertMalformed: -2146762488,
            CertUntrustedRoot: -2146762487,
            CertChainnig: -2146762486,
            CertRevoked: -2146762484,
            CertUntrustedTestRoot: -2146762483,
            CertRevocationFailure: -2146762482,
            CertCnNoMatch: -2146762481,
            CertWrongUsage: -2146762480,
            CertUntrustedCa: -2146762478,
            CertUnrecognizedError: -2146762477
        },

        ldapSecurityRes = {
            fullAccess: ASC.Resources.Master.ResourceJS.SecurityMappingFullAccess,
            documents: ASC.Resources.Master.ResourceJS.SecurityMappingDocuments,
            projects: ASC.Resources.Master.ResourceJS.SecurityMappingProjects,
            crm: ASC.Resources.Master.ResourceJS.SecurityMappingCrm,
            community: ASC.Resources.Master.ResourceJS.SecurityMappingCommunity,
            people: ASC.Resources.Master.ResourceJS.SecurityMappingPeople,
            mail: ASC.Resources.Master.ResourceJS.SecurityMappingMail
        },

        ldapMappingRes = {
            FirstNameAttribute: ASC.Resources.Master.ResourceJS.MappingFirstNameAttribute,
            SecondNameAttribute: ASC.Resources.Master.ResourceJS.MappingSecondNameAttribute,
            BirthDayAttribute: ASC.Resources.Master.ResourceJS.MappingBirthDayAttribute,
            GenderAttribute: ASC.Resources.Master.ResourceJS.MappingGenderAttribute,
            MobilePhoneAttribute: ASC.Resources.Master.ResourceJS.MappingMobilePhoneAttribute,
            MailAttribute: ASC.Resources.Master.ResourceJS.MappingMailAttribute,
            TitleAttribute: ASC.Resources.Master.ResourceJS.MappingTitleAttribute,
            LocationAttribute: ASC.Resources.Master.ResourceJS.MappingLocationAttribute,
            AvatarAttribute: ASC.Resources.Master.ResourceJS.MappingAvatarAttribute,
            AdditionalPhone: ASC.Resources.Master.ResourceJS.MappingAdditionalPhone,
            AdditionalMobilePhone: ASC.Resources.Master.ResourceJS.MappingAdditionalMobilePhone,
            AdditionalMail: ASC.Resources.Master.ResourceJS.MappingAdditionalMail,
            Skype: ASC.Resources.Master.ResourceJS.MappingSkype
        },

        $ldapSettingsServer = jq("#ldapSettingsServer"),
        $ldapSettingsUserDN = jq("#ldapSettingsUserDN"),
        $ldapSettingsPortNumber = jq("#ldapSettingsPortNumber"),
        $ldapSettingsUserFilter = jq("#ldapSettingsUserFilter"),
        $ldapSettingsLoginAttribute = jq("#ldapSettingsLoginAttribute"),
        $ldapSettingsGroupDN = jq("#ldapSettingsGroupDN"),
        $ldapSettingsUserAttribute = jq("#ldapSettingsUserAttribute"),
        $ldapSettingsGroupFilter = jq("#ldapSettingsGroupFilter"),
        $ldapSettingsGroupAttribute = jq("#ldapSettingsGroupAttribute"),
        $ldapSettingsGroupNameAttribute = jq("#ldapSettingsGroupNameAttribute"),
        $ldapSettingsLogin = jq("#ldapSettingsLogin"),
        $ldapSettingsPassword = jq("#ldapSettingsPassword"),

        $ldapSettingsError = jq("#ldapSettingsError"),
        $ldapSettingsSyncError = jq("#ldapSettingsSyncError"),

        $ldapSettingsSyncBtn = jq("#ldapSettingsSyncBtn"),
        $ldapSettingsRestoreBtn = jq("#ldapSettingsRestoreBtn"),


        $ldapSettingsSaveBtn = jq(".ldap-settings-save"),
        $ldapSettingsMainContainer = jq(".ldap-settings-main-container"),

        $ldapSettingsSyncSource = jq("#ldapSettingsSyncSource"),
        $ldapSettingsSource = jq("#ldapSettingsSource"),
        $ldapSettingsStatus = jq("#ldapSettingsStatus"),
        $ldapSettingsPercent = jq("#ldapSettingsPercent"),

        $ldapSettingsProgressbarContainer = jq(".ldap-settings-progressbar-container"),
        $ldapSettingsProgressValue = $ldapSettingsProgressbarContainer.find(".asc-progress-value"),

        $ldapSettingsSyncProgressbarContainer = jq(".ldap-settings-sync-progressbar-container"),
        $ldapSettingsSyncProgressValue = $ldapSettingsSyncProgressbarContainer.find(".asc-progress-value"),

        $ldapSettingsCronContainer = jq("#ldapSettingsCronContainer"),
        $ldapSettingsAutoSyncBtn = jq("#ldapSettingsAutoSyncBtn"),

        $ldapSettingsUserContainer = jq(".ldap-settings-user-container"),
        $ldapSettingsGroupContainer = jq(".ldap-settings-group-container"),
        $ldapSettingsAuthContainer = jq(".ldap-settings-auth-container"),
        $ldapSettingsSecurityContainer = jq(".ldap-settings-security-container"),
        $ldapSettingsAdvancedContainer = jq(".ldap-settings-advanced-container"),

        $ldapSettingsStartTlsCheckbox = jq("#ldapSettingsStartTLSCheckbox"),
        $ldapSettingsSslCheckbox = jq("#ldapSettingsSslCheckbox"),

        $ldapSettingsAuthBtn = jq("#ldapSettingsAuthenticationCheckbox"),
        $ldapSettingsBtn = jq("#ldapSettingsCheckbox"),

        $ldapSettingsSyncStatus = jq("#ldapSettingsSyncStatus"),
        $ldapSettingsSyncPercent = jq("#ldapSettingsSyncPercent"),

        $ldapSettingsGroupBtn = jq("#ldapSettingsGroupCheckbox"),

        $ldapSettingsAutoSync = jq("#ldapAutoSyncCont"),
        $ldapSettingsCron = jq("#ldapSettingsAutoSyncCron"),
        $ldapSettingsCronInput = jq("#ldapSettingsAutoSyncCronInput"),
        $ldapCronEditLink = jq("#ldapCronEditLink, #ldapCronHumanText"),
        $ldapSettingsAutoSyncDialog = jq("#ldapSettingsCronDialog"),
        $ldapNextSyncFields = jq(".cronHumanReadable"),

        $ldapSettingsSendWelcomeEmailCheckbox = jq("#ldapSettingsSendWelcomeEmail"),

        $ldapSettingsInviteDialog = jq("#ldapSettingsInviteDialog"),
        $ldapSettingsCertificateValidationDialog = jq("#ldapSettingsCertificateValidationDialog"),
        $ldapSettingsTurnOffDialog = jq("#ldapSettingsTurnOffDialog"),
        $ldapSettingsCronTurnOffDialog = jq("#ldapSettingsCronTurnOffDialog"),
        $ldapSettingsSpoilerLink = jq(".ldap-settings-spoiler-link"),
        $ldapSettingsSpoiler = jq("#ldapSettingsSpoiler"),

        $ldapMappingSecurity = jq("#ldapMappingSecurity"),
        $ldapMappingAddAccess = jq("#ldapMappingAddAccess"),

        $ldapMappingAddBtn = jq("#ldapMappingAddBtn"),
        $ldapMappingSettings = jq("#ldapMappingSettings"),
        $studioPageContent = jq('#studioPageContent'),
        ldapMappingOptions,
        ldapMappingRequiredOptions = ["FirstNameAttribute", "SecondNameAttribute", "MailAttribute"],

        $ldapSettingsImportUserLimitPanel = jq("#ldapSettingsImportUserLimitPanel"),

        $ldapSettingsRestoreDefaultSettings = jq(".ldap-settings-restore-default-settings"),
        $view = jq('.studio-container');


    var init = function () {


        function buildMappingOptions(collection) {
            var htmlMappingOptions = "";

            for (var key in collection) {
                htmlMappingOptions += "<div class=\"option\" data-value=\"" + key + "\">" + Encoder.htmlEncode(collection[key]) + "</div>";
            }

            return htmlMappingOptions;
        }

        if (jq("#ldapBlock").hasClass("disable")) {
            jq("#ldapBlock").find("input").prop("disabled", true);
            jq("#ldapSettingsSyncBtn").addClass("disable");
            return;
        }

        Teamlab.getLdapCronSettings({}, {
            success: onGetLdapCronSettings,
            error: onFailApi
        });

        $ldapSettingsSpoilerLink.on('click', function () {
            spoiler.toggle('#ldapSettingsSpoiler', jq(this));
        });

        if (!$ldapSettingsSyncBtn.hasClass("disable")) {
            enableSync(true);
        }

        ldapMappingOptions = buildMappingOptions(ldapMappingRes);
        ldapSecurityOptions = buildMappingOptions(ldapSecurityRes);

        parseMappings($ldapMappingSettings, ldapMappingRes, ASC.Resources.Master.ResourceJS.LdapAttributeOrigin, ldapMappingRequiredOptions);
        //parseMappings($ldapMappingSettings, ldapMappingRes, ldapMappingOptions, ASC.Resources.Master.ResourceJS.LdapAttributeOrigin, ldapMappingRequiredOptions);
        //parseMappings($ldapMappingSecurity, ldapSecurityRes, ldapSecurityOptions, ASC.Resources.Master.ResourceJS.LdapSecurityPlaceholder);

        currentSettings = getSettings();

        $studioPageContent.on('click', '.selectBox', showSelectOptions);
        $studioPageContent.on('click', '.selectBox .option', selectOption);

        if ($ldapSettingsBtn.hasClass("off")) {
            $ldapSettingsMainContainer.find("input").prop("disabled", true);
            $ldapSettingsMainContainer.find("textarea").prop("disabled", true);
            $ldapSettingsMainContainer.find(".selectBox:not(.locked)").addClass("disabled");
            $ldapMappingAddBtn.addClass("disable");
            $ldapMappingAddAccess.addClass("disabled");
            $ldapMappingSettings.find(".ldap-mapping-remove-row").addClass("disable");
            $ldapMappingSecurity.find(".ldap-mapping-remove-row").addClass("disable");

            $ldapSettingsGroupBtn.addClass("disable");

            $ldapSettingsAuthBtn.addClass("disable")

        } else {
            $ldapSettingsUserContainer.find("input").prop("disabled", false);
            $ldapSettingsUserContainer.find("textarea").prop("disabled", false);
            if ($ldapSettingsGroupContainer.hasClass("ldap-settings-disabled")) {
                $ldapSettingsGroupContainer.find("input").prop("disabled", true);
                $ldapSettingsGroupContainer.find("textarea").prop("disabled", true);
                $ldapMappingAddAccess.addClass("disabled");
                $ldapMappingSecurity.find(".ldap-mapping-remove-row").addClass("disable");
            } else {
                $ldapSettingsGroupContainer.find("input").prop("disabled", false);
                $ldapSettingsGroupContainer.find("textarea").prop("disabled", false);
                $ldapMappingAddAccess.removeClass("disabled");
                $ldapMappingSecurity.find(".ldap-mapping-remove-row").removeClass("disable");
            }

            if ($ldapSettingsAuthContainer.hasClass("ldap-settings-disabled")) {
                $ldapSettingsAuthContainer.find("input").prop("disabled", true);
            } else {
                $ldapSettingsAuthContainer.find("input").prop("disabled", false);
            }
        }

        if (!$ldapSettingsSaveBtn.hasClass("disable")) {
            enableSave(true);
        }

        if (!$ldapSettingsRestoreBtn.hasClass("disable")) {
            enableRestoreDefault(true);
        }

        if (!$ldapSettingsSyncBtn.hasClass("disable")) {
            enableSync(true);
        }

        jq("body").on("click", function (event) {
            var $selectors = $view.find('.selectBox');
            var target = (event.target) ? event.target : event.srcElement,
                element = jq(target);

            if (!element.is('.selectBox') && !element.is('.selectBoxValue') && !element.is('.selectBoxSwitch')) {
                $selectors.find('.selectOptionsBox').hide();
            } else {
                var curBox = element.is('.selectBox') ? element : element.parents('.selectBox:first');
                $selectors.not(curBox).find('.selectOptionsBox').hide();
            }
        });

        $ldapMappingAddBtn.on("click", function () {
            if (jq(this).hasClass("disable")) return;
            addMappingRow($ldapMappingSettings, "", "", "", ldapMappingRes, ASC.Resources.Master.ResourceJS.LdapAttributeOrigin);
        });

        $ldapMappingAddAccess.on("click", function () {
            if (jq(this).hasClass("disable")) return;
            addMappingRow($ldapMappingSecurity, "", "", "", ldapMappingRes, ASC.Resources.Master.ResourceJS.LdapSecurityPlaceholder);
        });

        $ldapSettingsTurnOffDialog.on("click", ".ldap-settings-ok", function () {
            if (jq(this).hasClass("disabled"))
                return;

            enableLdap(false);
            closeDialog();
            saveSettings();

            if ($ldapSettingsSpoiler.hasClass("display-none")) {
                spoiler.toggle('#ldapSettingsSpoiler', $ldapSettingsSpoilerLink);
            }

            jq('html, body').animate({
                scrollTop: $ldapSettingsProgressbarContainer.offset().top
            }, 2000);
        });
        $ldapSettingsTurnOffDialog.on("click", ".ldap-settings-cancel", closeDialog);

        $ldapSettingsCronTurnOffDialog.on("click", ".ldap-settings-ok", function () {
            if (jq(this).hasClass("disabled"))
                return;

            $ldapSettingsAutoSyncBtn.addClass("off").removeClass("on");
            showCronEdit(false);
            saveCronSettings(true);
            closeDialog();
        });
        $ldapSettingsCronTurnOffDialog.on("click", ".ldap-settings-cancel", closeDialog);



        $ldapSettingsSslCheckbox.on("click", function () {
            var self = jq(this);

            if (self.is(":checked")) {
                $ldapSettingsStartTlsCheckbox.prop('checked', false);
                $ldapSettingsPortNumber.val(constants.SSL_LDAP_PORT);
            }
            else {
                $ldapSettingsPortNumber.val(constants.DEFAULT_LDAP_PORT);
            }
            refreshButtons();
        });

        $ldapSettingsStartTlsCheckbox.on("click", function () {
            var self = jq(this);
            if (self.hasClass("disabled"))
                return;

            if (self.is(":checked")) {
                $ldapSettingsSslCheckbox.prop('checked', false);
                $ldapSettingsPortNumber.val(constants.DEFAULT_LDAP_PORT);
            }
            refreshButtons();
        });

        $ldapSettingsBtn.on("click", onLdapEnabled);
        $ldapSettingsAutoSyncBtn.on("click", onAutoSyncEnabled);
        jq("#ldapCronEditLink").on("click", function (e) { onAutoSyncEnabled(e, true); });

        $ldapSettingsSendWelcomeEmailCheckbox.on("click", function () {
            if (jq(this).is(":checked")) {
                jq(this).removeClass("off").addClass("on");
            } else {
                jq(this).removeClass("on").addClass("off");
            }
        });
        $ldapSettingsGroupBtn.on("click", function () {

            if (jq(this).hasClass("disable")) return;

            if (jq(this).hasClass("off")) {
                jq(this).removeClass("off").addClass("on");
                $ldapSettingsGroupContainer.find("input").prop("disabled", false);
                $ldapSettingsGroupContainer.removeClass("ldap-settings-disabled");
            } else {
                jq(this).removeClass("on").addClass("off");
                $ldapSettingsGroupContainer.find("input").prop("disabled", false);
                $ldapSettingsGroupContainer.addClass("ldap-settings-disabled");
            }
        });

        $ldapSettingsAuthBtn.on("click", function () {

            var $ldapSettingsAuthContainer = jq(".ldap-settings-auth-container");

            if (jq(this).hasClass("disable")) return;

            if (jq(this).hasClass("off")) {
                jq(this).removeClass("off").addClass("on");
                $ldapSettingsAuthContainer.find("input").prop("disabled", false);
                $ldapSettingsAuthContainer.removeClass("ldap-settings-disabled");
            } else {
                jq(this).removeClass("on").addClass("off");
                $ldapSettingsAuthContainer.find("input").prop("disabled", false);
                $ldapSettingsAuthContainer.addClass("ldap-settings-disabled");
            }
        });

        $ldapSettingsCertificateValidationDialog.on("click",
            ".ldap-settings-ok",
            function () { continueSaveSettings(null, true); });
        $ldapSettingsCertificateValidationDialog.on("click",
            ".ldap-settings-cancel",
            function () {
                closeDialog();
            });

        $ldapSettingsAutoSyncDialog.on("click", ".ldap-sync-save", function () {
            if (jq(this).hasClass("disabled"))
                return;

            $ldapSettingsAutoSyncBtn.addClass("on").removeClass("off");
            showCronEdit(true);
            saveCronSettings();
            closeDialog();
            $ldapNextSyncFields.text(getNextValidDateFromCron($ldapSettingsCron.jqCronGetInstance().getCron()));
        });
        $ldapSettingsAutoSyncDialog.on("click", ".ldap-settings-cancel", function () {
            closeDialog();
            if (currentCron) {
                setTimeout(function () { $ldapSettingsCron.jqCronGetInstance().setCron(currentCron); }, 500);
            }
        });
        $ldapSettingsMainContainer.on("click", ".ldap-settings-restore-default-settings", restoreDefault);

        $ldapSettingsInviteDialog.on("click", ".ldap-settings-ok", restoreDefaultSettings);
        $ldapSettingsInviteDialog.on("click", ".ldap-settings-cancel", cancelDialog);
        $ldapSettingsImportUserLimitPanel.on("click", ".ldap-settings-ok", continueSaveSettings);
        $ldapSettingsImportUserLimitPanel.on("click", ".ldap-settings-cancel", cancelDialog);
        $ldapSettingsImportUserLimitPanel.on("click", ".cancelButton", cancelDialog);
        jq(document).on("keyup", function (e) {
            /* Escape Key */
            if (!$ldapSettingsImportUserLimitPanel.is(":hidden") && e.keyCode == 27) {
                cancelDialog();
            }
        });
        jq(".ldap-settings-main-container input").on("change", function () {
            isRestoreDefault = false;
            if ($ldapSettingsSaveBtn.hasClass("disable")) {
                $ldapSettingsSaveBtn.removeClass("disable");
                $ldapSettingsMainContainer.on("click", ".ldap-settings-save", saveSettings);
            }
            if ($ldapSettingsRestoreDefaultSettings.hasClass("disable")) {
                $ldapSettingsRestoreDefaultSettings.removeClass("disable");
                $ldapSettingsMainContainer.on("click", ".ldap-settings-restore-default-settings", restoreDefault);
            }
        });
        jq(".ldap-settings-main-container .textEdit").on("keyup", function () {
            isRestoreDefault = false;
            if ($ldapSettingsSaveBtn.hasClass("disable")) {
                $ldapSettingsSaveBtn.removeClass("disable");
                $ldapSettingsMainContainer.on("click", ".ldap-settings-save", saveSettings);
            }
            if ($ldapSettingsRestoreDefaultSettings.hasClass("disable")) {
                $ldapSettingsRestoreDefaultSettings.removeClass("disable");
                $ldapSettingsMainContainer.on("click", ".ldap-settings-restore-default-settings", restoreDefault);
            }
        });
    }

    function onGetLdapCronSettings(params, cron) {

        currentCron = Object.keys(cron).length === 0 ? "" : cron;
        if (currentCron === "") {
            $ldapSettingsAutoSyncBtn.removeClass("on").addClass("off");
            showCronEdit(false);
        } else {
            $ldapSettingsAutoSyncBtn.removeClass("off").addClass("on");
            showCronEdit(true);
        }

        $ldapSettingsCron.jqCron({
            enabled_year: false,
            texts: {
                en: {
                    empty: ASC.Resources.Master.ResourceJS.CronEmpty,
                    empty_minutes: ASC.Resources.Master.ResourceJS.CronEmptyMinutes,
                    empty_time_hours: ASC.Resources.Master.ResourceJS.CronEmptyTimeHours,
                    empty_time_minutes: ASC.Resources.Master.ResourceJS.CronEmptyTimeMinutes,
                    empty_day_of_week: ASC.Resources.Master.ResourceJS.CronEmptyDayOfWeek,
                    empty_day_of_month: ASC.Resources.Master.ResourceJS.CronEmptyDayOfMonth,
                    empty_month: ASC.Resources.Master.ResourceJS.CronEmptyMonth,
                    name_minute: ASC.Resources.Master.ResourceJS.CronNameMinute,
                    name_hour: ASC.Resources.Master.ResourceJS.CronNameHour,
                    name_day: ASC.Resources.Master.ResourceJS.CronNameDay,
                    name_week: ASC.Resources.Master.ResourceJS.CronNameWeek,
                    name_month: ASC.Resources.Master.ResourceJS.CronNameMonth,
                    name_year: ASC.Resources.Master.ResourceJS.CronNameYear,
                    text_period: ASC.Resources.Master.ResourceJS.CronTextPeriod,
                    text_mins: ASC.Resources.Master.ResourceJS.CronTextMins,
                    text_time: ASC.Resources.Master.ResourceJS.CronTextTime,
                    text_dow: ASC.Resources.Master.ResourceJS.CronTextDow,
                    text_month: ASC.Resources.Master.ResourceJS.CronTextMonth,
                    text_dom: ASC.Resources.Master.ResourceJS.CronTextDom,
                    error1: ASC.Resources.Master.ResourceJS.CronError1,
                    error2: ASC.Resources.Master.ResourceJS.CronError2,
                    error3: ASC.Resources.Master.ResourceJS.CronError3,
                    error4: ASC.Resources.Master.ResourceJS.CronError4,
                    first_last: [ASC.Resources.Master.ResourceJS.CronFirst, ASC.Resources.Master.ResourceJS.CronLast],
                    weekdays: [ASC.Resources.Master.ResourceJS.CronMonday, ASC.Resources.Master.ResourceJS.CronTuesday, ASC.Resources.Master.ResourceJS.CronWednesday, ASC.Resources.Master.ResourceJS.CronThursday, ASC.Resources.Master.ResourceJS.CronFriday, ASC.Resources.Master.ResourceJS.CronSaturday, ASC.Resources.Master.ResourceJS.CronSunday],
                    months: [ASC.Resources.Master.ResourceJS.CronJanuary, ASC.Resources.Master.ResourceJS.CronFebruary, ASC.Resources.Master.ResourceJS.CronMarch, ASC.Resources.Master.ResourceJS.CronApril, ASC.Resources.Master.ResourceJS.CronMay, ASC.Resources.Master.ResourceJS.CronJune, ASC.Resources.Master.ResourceJS.CronJuly, ASC.Resources.Master.ResourceJS.CronAugust, ASC.Resources.Master.ResourceJS.CronSeptember, ASC.Resources.Master.ResourceJS.CronOctober, ASC.Resources.Master.ResourceJS.CronNovember, ASC.Resources.Master.ResourceJS.CronDecember]
                }
            },
            default_value: currentCron || "0 0 0 ? * *",
            bind_to: $ldapSettingsCronInput,
            bind_method: {
                set: function ($element, value) {
                    var el;
                    if ($ldapSettingsAutoSyncDialog.is(":visible")) {
                        el = $ldapSettingsAutoSyncDialog.find(".cronHumanReadable");
                    } else {
                        el = $ldapNextSyncFields;
                    }
                    el = $ldapNextSyncFields;
                    el.text(getNextValidDateFromCron(value));
                    $element.val(value);
                }
            }
        }).children().first().on("cron:change", refreshButtons);
    }

    function enableInputs(on) {
        if (on) {
            $ldapSettingsBtn.removeClass("off").addClass("on");
            $ldapMappingAddBtn.removeClass("disable");

            $ldapMappingAddAccess.addClass("disabled");
            $ldapSettingsUserContainer.find("input").prop("disabled", false);

            $ldapSettingsUserContainer.find("textarea").prop("disabled", false);
            $ldapSettingsUserContainer.find(".selectBox:not(.locked)").removeClass("disabled");
            $ldapMappingSettings.find(".ldap-mapping-remove-row").removeClass("disable");
            $ldapMappingSecurity.find(".ldap-mapping-remove-row").addClass("disable");

            $ldapSettingsGroupBtn.removeClass("disable");

            $ldapSettingsUserContainer.removeClass("ldap-settings-disabled");
            $ldapSettingsSecurityContainer.removeClass("ldap-settings-disabled");
            $ldapSettingsAdvancedContainer.removeClass("ldap-settings-disabled");

            $ldapSettingsStartTlsCheckbox.prop("disabled", false);
            $ldapSettingsSslCheckbox.prop("disabled", false);

            $ldapSettingsSendWelcomeEmailCheckbox.prop("disabled", false);

            $ldapSettingsAutoSyncBtn.addClass("disable");

            if ($ldapSettingsGroupBtn.hasClass("on")) {
                $ldapSettingsGroupContainer.find("input").prop("disabled", false);
                $ldapSettingsGroupContainer.find("textarea").prop("disabled", false);
                $ldapSettingsGroupContainer.removeClass("ldap-settings-disabled");
                $ldapMappingAddAccess.removeClass("disabled");
                $ldapMappingSecurity.find(".ldap-mapping-remove-row").removeClass("disable");
            }
            $ldapSettingsAuthBtn.removeClass("disable");

            if ($ldapSettingsAuthBtn.hasClass("on")) {
                $ldapSettingsAuthContainer.find("input").prop("disabled", false);
                $ldapSettingsAuthContainer.removeClass("ldap-settings-disabled");
            }
        } else {
            $ldapSettingsBtn.removeClass("on").addClass("off");
            $ldapMappingAddBtn.addClass("disable");

            $ldapSettingsCronContainer.find("input").prop("disabled", true);
            $ldapSettingsCronContainer.find(".on_off_button").addClass("disabled");

            $ldapMappingAddAccess.addClass("disabled");

            $ldapSettingsGroupBtn.addClass("disable");

            $ldapSettingsAuthBtn.addClass("disable");

            $ldapSettingsMainContainer.find("input").prop("disabled", true);
            $ldapSettingsMainContainer.find("textarea").prop("disabled", true);
            $ldapSettingsUserContainer.find(".selectBox:not(.locked)").addClass("disabled");
            $ldapMappingSettings.find(".ldap-mapping-remove-row").addClass("disable");
            $ldapMappingSecurity.find(".ldap-mapping-remove-row").addClass("disable");
            $ldapSettingsStartTlsCheckbox.addClass("disabled");
            $ldapSettingsSslCheckbox.addClass("disabled");

            $ldapSettingsSendWelcomeEmailCheckbox.prop("disabled", true);

            $ldapSettingsAutoSyncBtn.removeClass("disable");
            $ldapSettingsUserContainer.addClass("ldap-settings-disabled");
            $ldapSettingsSecurityContainer.addClass("ldap-settings-disabled");
            $ldapSettingsAdvancedContainer.addClass("ldap-settings-disabled");
            $ldapSettingsGroupContainer.addClass("ldap-settings-disabled");
            $ldapSettingsAuthContainer.addClass("ldap-settings-disabled");
        }
    }

    function getNextValidDateFromCron(cron) {
        var now, m = moment();

        var parts = cron.split(" ");

        var mins = parts[1],
            hrs = parts[2],
            dow = parts[5];

        var offset = m.utcOffset();
        m.utc(offset);
        m.milliseconds(0).subtract(offset, "minutes");
        now = moment(m);

        m.seconds(0);
        m.minutes(mins);

        if (hrs !== "*") {
            m.hours(hrs);

            if (dow === "*") {
                if (m < now) {
                    m.add(1, "days");
                }
            } else {
                if (dow.length === 1) {
                    m.isoWeekday(parseInt(dow));

                    if (m < now) {
                        m.add(7, "days");
                    }
                } else {
                    if (dow[1] === "L") {
                        m.date(m.daysInMonth());

                        while (m.isoWeekday() != dow[0]) {
                            m.subtract(1, "days");
                        }

                        if (m < now) {
                            m.add(7, "days");

                            var month = m.month();

                            while (month === m.month()) {
                                m.add(7, "days");
                            }

                            m.subtract(7, "days");
                        }
                    } else {
                        m.date(1);
                        var month = m.month();
                        m.isoWeekday(parseInt(dow[0]));

                        if (month !== m.month()) {
                            m.add(7, "days");
                        }

                        if (m < now) {
                            m.add(1, "month");
                            m.date(1);

                            month = m.month();
                            m.isoWeekday(parseInt(dow[0]));

                            if (month !== m.month()) {
                                m.add(7, "days");
                            }
                        }
                    }
                }
            }
        } else {
            if (m < now) {
                m.add(1, "hours");
            }
        }

        return m.add(offset, "minutes").format("LLLL");
    }

    function refreshButtons() {
        isRestoreDefault = false;
        var settingsChanged = hasChanges();
        enableSave(settingsChanged);
        enableSync(!settingsChanged && currentSettings.EnableLdapAuthentication);
        enableRestoreDefault($ldapSettingsBtn.hasClass("on"));
        enableCron(currentSettings.EnableLdapAuthentication);
        enableMailCheckboxes($ldapSettingsBtn.hasClass("on") && jq(".ldap-mapping-row .selectBox[data-value=MailAttribute]").nextAll("input").val());
    }

    function enableProgress(enabled) {
        if (enabled) {
            if (syncInProgress) {
                $ldapSettingsProgressbarContainer.css({ "visibility": "hidden" });
                $ldapSettingsSyncProgressbarContainer.css({ "visibility": "visible" });
            } else {
                $ldapSettingsProgressbarContainer.css({ "visibility": "visible" });
                $ldapSettingsSyncProgressbarContainer.css({ "visibility": "hidden" });
            }
        } else {
            $ldapSettingsProgressbarContainer.css({ "visibility": "hidden" });
            $ldapSettingsSyncProgressbarContainer.css({ "visibility": "hidden" });
        }
    }
    
    function enableSave(enabled) {
        $ldapSettingsSaveBtn.toggleClass("disable", !enabled);

        if (enabled) {
            $ldapSettingsSaveBtn.off("click").on("click", saveSettings);
        } else {
            $ldapSettingsSaveBtn.off("click");
        }
    }

    function enableSync(enabled) {
        $ldapSettingsSyncBtn.toggleClass("disable", !enabled);

        if (enabled) {
            $ldapSettingsSyncBtn.off("click").on("click", syncUsersLDAP);
        } else {
            $ldapSettingsSyncBtn.off("click");
        }
    }

    function enableRestoreDefault(enabled) {
        $ldapSettingsRestoreBtn.toggleClass("disable", !enabled);

        if (enabled) {
            $ldapSettingsRestoreBtn.off("click").on("click", restoreDefault);
        } else {
            $ldapSettingsRestoreBtn.off("click");
        }
    }

    function enableCron(on) {

        $ldapSettingsAutoSyncBtn.toggleClass("disable", !on);
        if (!on) {
            $ldapSettingsAutoSyncBtn.removeClass("on").addClass("off");
        }

        showCronEdit($ldapSettingsAutoSyncBtn.hasClass("on"));
    }

    function showCronEdit(on) {
        if (on) {
            $ldapCronEditLink.show();
        } else {
            $ldapCronEditLink.hide();
        }
    }

    function enableMailCheckboxes(on) {
        if (on) {
            $ldapSettingsSendWelcomeEmailCheckbox.prop("disabled", false);
        } else {
            $ldapSettingsSendWelcomeEmailCheckbox.prop('checked', false).prop("disabled", true);
        }
    }

    function enableLdap(on) {
        $ldapSettingsBtn.toggleClass("off", !on).toggleClass("on", on);

        if (on && $ldapSettingsSpoiler.hasClass("display-none")) {
            $ldapSettingsSpoilerLink.trigger("click");
        }

        enableInputs(on);

        HideRequiredError();

        onSettingsChanged();

    }

    function onLdapEnabled() {
        var $this = jq(this);

        if ($this.hasClass("disabled"))
            return;

        var on = $this.hasClass("off");

        if (currentSettings.EnableLdapAuthentication) {
            StudioBlockUIManager.blockUI("#ldapSettingsTurnOffDialog", 500);
        }
        else {
            enableLdap(on);
        }
    }

    function getSettings() {
        var enableLdapAuthentication = $ldapSettingsBtn.hasClass("on"),
            startTls = $ldapSettingsStartTlsCheckbox.is(":checked"),
            ssl = $ldapSettingsSslCheckbox.is(":checked"),
            sendWelcomeEmail = $ldapSettingsSendWelcomeEmailCheckbox.is(":checked"),
            server = $ldapSettingsServer.val(),
            userDN = $ldapSettingsUserDN.val(),
            portNumber = $ldapSettingsPortNumber.val(),
            userFilter = $ldapSettingsUserFilter.val(),
            loginAttribute = $ldapSettingsLoginAttribute.val(),
            ldapMapping = {},
            accessRights = {},
            groupMembership = $ldapSettingsGroupBtn.hasClass("on"),
            groupDN = $ldapSettingsGroupDN.val(),
            userAttribute = $ldapSettingsUserAttribute.val(),
            groupFilter = $ldapSettingsGroupFilter.val(),
            groupAttribute = $ldapSettingsGroupAttribute.val(),
            groupNameAttribute = $ldapSettingsGroupNameAttribute.val(),
            authentication = $ldapSettingsAuthBtn.hasClass("on"),
            login = $ldapSettingsLogin.val(),
            password = $ldapSettingsPassword.val();


        $ldapMappingSettings.children().each(function () {

            var select = (jq(this).children("select").val() || "").trim();
            var input = (jq(this).children("input").val() || "").trim();

            if (!select || !input) return;

            if (ldapMapping[select] && !select.endsWith("Attribute")) {
                ldapMapping[select] += "," + input;
            } else {
                ldapMapping[select] = input;
            }
        });

        $ldapMappingSecurity.children().each(function () {
            var select = (jq(this).children("div").attr("data-value") || "").trim();
            var input = (jq(this).children("input").val() || "").trim();

            if (!select || !input) return;

            if (accessRights[select]) {
                accessRights[select] += "," + input;
            } else {
                accessRights[select] = input;
            }
        });

        var settings = {
            EnableLdapAuthentication: enableLdapAuthentication,
            StartTls: startTls,
            Ssl: ssl,
            SendWelcomeEmail: sendWelcomeEmail,
            Server: server,
            UserDN: userDN,
            PortNumber: portNumber,
            UserFilter: userFilter,
            LoginAttribute: loginAttribute,
            LdapMapping: ldapMapping,
            AccessRights: accessRights,
            GroupMembership: groupMembership,
            GroupDN: groupDN,
            UserAttribute: userAttribute,
            GroupFilter: groupFilter,
            GroupAttribute: groupAttribute,
            GroupNameAttribute: groupNameAttribute,
            Authentication: authentication,
            Login: login,
            Password: password
        };
        return settings;
    }

    function parseMappings(elem, options, placeholder, required) {
        var data_val = elem.attr("data-value");
        if (!data_val) return;
        var data = JSON.parse(data_val);

        if (required) {
            for (var i = 0; i < required.length; i++) {
                var value = data[required[i]];
                addMappingRow(elem, required[i], value ? value : "", options[required[i]], options, placeholder, true);
            }
        }

        if (data) {
            for (var key in data) {
                if (required && required.includes(key)) {
                    continue;
                }

                if (data[key].includes(",")) {
                    var split = data[key].split(",");
                    for (var i = 0; i < split.length; i++) {
                        addMappingRow(elem, key, split[i], options[key], options, placeholder);
                    }
                } else {
                    addMappingRow(elem, key, data[key], options[key], options, placeholder);
                }
            }
        }
    }

    function addMappingRow(el, key, value, humanKey, options, placeholder, required) {

        var mappingRow = document.createElement("div");
        mappingRow.className = "ldap-mapping-row clear-fix requiredField";

        var mappingInput = document.createElement("input");
        mappingInput.className = "textEdit";
        mappingInput.type = "text"; 
        mappingInput.value = value;
        mappingInput.placeholder = placeholder;

        jq(mappingInput).on("change", refreshButtons)
            .on("keyup", refreshButtons)
            .on("paste", refreshButtons);

        var mappingErrorSpan = document.createElement("span");
        mappingErrorSpan.className = "requiredErrorText";
        mappingErrorSpan.innerHTML = ASC.Resources.Master.ResourceJS.LdapSettingsEmptyField;

        var removeBtn = document.createElement("div");
        removeBtn.className = "ldap-mapping-remove-row remove-btn-icon";
        var spanIcon =  document.createElement("span");
        spanIcon.className = "menu-item-icon";
        spanIcon.innerHTML = "<svg width=\"16\" height=\"16\" viewBox=\"0 0 16 16\" xmlns=\"http://www.w3.org/2000/svg\">"
            + "<path fill-rule=\"evenodd\" clip-rule=\"evenodd\" d=\"M8 15C11.866 15 15 11.866 15 8C15 4.13401 11.866 1 8 1C4.13401 1 1 4.13401 1 8C1 11.866 4.13401 15 8 15ZM7.96899 12.9723C10.7133 12.9913 12.9533 10.782 12.9723 8.03775C12.9913 5.29348 10.7821 3.0534 8.03779 3.0344C5.29352 3.0154 3.05345 5.22467 3.03445 7.96894C3.01545 10.7132 5.22472 12.9533 7.96899 12.9723Z\" />"
            + "<path d=\"M5.5 5.5L10.5 10.5\" stroke-width=\"2\" />"
            + "<path d=\"M5.5 10.5L10.5 5.5\" stroke-width=\"2\" />"
            + "</svg >";
        removeBtn.append(spanIcon)

        var s = document.createElement("select");
        s.className = "comboBox";
        if (required) s.setAttribute("disabled", true);
        s.setAttribute("data-default", key);

        var occupiedAttributeKeys = [];
        if (!key) {
            var currentSelect = jq(el).find("select");
            for (var i = 0; i < currentSelect.length; i++) {
                if (currentSelect[i].value.endsWith("Attribute")) occupiedAttributeKeys.push(currentSelect[i].value);
            }
        }
        for(var k in options)
        {
            if (key || occupiedAttributeKeys.indexOf(k) === -1) {
                var option = document.createElement("option");
                var lowerKey = key.toLowerCase(); //TODO bring everything to the same case
                var lowerK = k.toLowerCase();
                if (lowerKey == lowerK) option.setAttribute("selected", true);
                option.setAttribute("value", k);
                option.append(options[k]);
                s.append(option);
            }
        }

        jq(s).on("change", onSettingsChanged);
        jq(s).on("change", function () { refreshMappingOptions(el, options); });

        mappingRow.append(s);
        mappingRow.append(mappingInput);
        mappingRow.append(mappingErrorSpan);

        if (!required) mappingRow.append(removeBtn);
        el.append(mappingRow);

        el.find(".ldap-mapping-remove-row").last().on("click", function () { removeMappingRow(el, jq(this), options); });

        refreshMappingOptions(el, options);
    }

    function refreshMappingOptions(el, options) {
        var uniqueKeys = [];
        var selects = el.find(".comboBox");

        selects.each(function () {
            var val = jq(this)[0].value;
            if (val.endsWith("Attribute"))
                uniqueKeys.push(val);
        }).each(function () {
            var sel = jq(this)[0].value;
            this.innerHTML = "";

            for (var k in options) {
                var option = document.createElement("option");

                option.setAttribute("value", k);
                option.append(options[k]);
                jq(this).append(option);
            }
            var currentOptions = this.options;
            
            var myOptions = Array.prototype.slice.call(currentOptions);

            myOptions.filter(function (number) {return number.value === sel })[0].setAttribute("selected", true);

            var t = myOptions.filter(function (number) {
                return (!(uniqueKeys.indexOf(number.value) < 0) && number.value !== sel);
            });

            t.forEach(function (item, i, arr) {
                jq(item).remove();
            });
        });
    }

    function removeMappingRow(el, self, options) {
        if (self.hasClass("disable")) return;

        var row = self.parent();
        if (row.length && row.hasClass("ldap-mapping-row"))
            row.remove();
        refreshMappingOptions(el, options);
        refreshButtons();
    }

    function onSettingsChanged() {
        refreshButtons();
    }

    function hasChanges() {
        var nextSettings = getSettings();

        if (!currentSettings && !!nextSettings ||
            !!currentSettings && !nextSettings) {
            return true;
        }

        if (currentSettings.EnableLdapAuthentication !== nextSettings.EnableLdapAuthentication ||
            currentSettings.StartTls !== nextSettings.StartTls ||
            currentSettings.SendWelcomeEmail !== nextSettings.SendWelcomeEmail ||
            currentSettings.Server !== nextSettings.Server ||
            currentSettings.UserDN !== nextSettings.UserDN ||
            currentSettings.PortNumber !== nextSettings.PortNumber ||
            currentSettings.UserFilter !== nextSettings.UserFilter ||
            currentSettings.LoginAttribute !== nextSettings.LoginAttribute ||
            currentSettings.GroupMembership !== nextSettings.GroupMembership ||
            currentSettings.GroupDN !== nextSettings.GroupDN ||
            currentSettings.UserAttribute !== nextSettings.UserAttribute ||
            currentSettings.GroupFilter !== nextSettings.GroupFilter ||
            currentSettings.GroupAttribute !== nextSettings.GroupAttribute ||
            currentSettings.GroupNameAttribute !== nextSettings.GroupNameAttribute ||
            currentSettings.Authentication !== nextSettings.Authentication ||
            currentSettings.Login !== nextSettings.Login ||
            currentSettings.Password !== nextSettings.Password) {
            return true;
        }


        if (!isObjectsEqual(currentSettings.LdapMapping, nextSettings.LdapMapping)) {
            return true;
        }

        if (!isObjectsEqual(currentSettings.AccessRights, nextSettings.AccessRights)) {
            return true;
        }

        return false;
    }

    function showSelectOptions() {
        var $selector = jq(this);

        if ($selector.prop("disabled") || $selector.hasClass("disabled"))
            return;

        var $options = $selector.find('.selectOptionsBox');

        if ($options.is(':visible')) {
            $options.hide();
            $options.css('top', 0);
            $options.css('left', 0);
        } else {
            var offset = $selector.position();

            if ($options.is('.top')) {
                $options.css('top', offset.top - $options.outerHeight() - 3 + 'px');
                $options.css('left', offset.left + $selector.outerWidth() - $options.outerWidth() + 'px');
            } else {
                $options.css('top', offset.top + $selector.outerHeight() + 3 + 'px');
                $options.css('left', offset.left + $selector.outerWidth() - $options.outerWidth() + 'px');
            }

            $options.show();
        }
    }

    function selectOption() {

        var $option = jq(this),
            $select = $option.closest('.selectBox'),
            value = $option.attr('data-value');

        $select.find('.selectBoxValue').text($option.text());
        $select.attr('data-value', value);

        $option.closest('.selectOptionsBox').hide();
        $option.siblings('.option').removeClass('selected');
        $option.addClass('selected');

        $select.trigger("valueChanged", value);
    }

    function onAutoSyncEnabled(ev, edit) {

        var $this = jq(this);

        if ($this.hasClass("disable")) return;

        var on = $ldapSettingsAutoSyncBtn.hasClass("on");

        if (on && currentCron && !edit) {
            StudioBlockUIManager.blockUI("#ldapSettingsCronTurnOffDialog", 500);
            return;
        } else {
            if (currentCron) {
                $ldapSettingsCron.jqCronGetInstance().setCron(currentCron);
            }
            StudioBlockUIManager.blockUI("#ldapSettingsCronDialog", 500);
            return;
        }
    }

    function restoreDefaultSettings() {

        PopupKeyUpActionProvider.CloseDialog();
        HideRequiredError();
        setStatus("");
        setSource("");
        setPercents(constants.NULL_PERCENT);

        Teamlab.getLdapDefaultSettings({},
            {
                success: function (params, result) {

                    try {
                        if (result) {
                            loadSettings(result);
                            continueSaveSettings(null, true);
                        } else {
                            throw ASC.Resources.Master.ResourceJS.OperationFailedError;
                        }
                    } catch (error) {
                        showError(error);
                        currentSettings = previousSettings;
                        endProcess();
                    }
                },
                error: onFailApi
            });
    }

    function closeDialog() {
        PopupKeyUpActionProvider.CloseDialog();
        enableInterface(true);
        $ldapSettingsError.hide();
        $ldapSettingsSyncError.hide();
        enableProgress(false);
        refreshButtons();
        already = false;
    }

    function saveCronSettings(remove) {
        var newCron;

        if (remove) {
            newCron = null;
        } else {
            newCron = $ldapSettingsCronInput.val();
        }

        Teamlab.setLdapCronSettings({}, { cron: newCron }, {
            success: function () {
                currentCron = newCron;
            },
            error: onFailApi
        });
    }

    function syncUsersLDAP() {
        if (already) {
            return;
        }

        syncInProgress = true;
        disableInterface();
        previousSettings = currentSettings;
        setStatus("");
        setSource("");
        setPercents(constants.NULL_PERCENT);

        already = true;
        HideRequiredError();

        $ldapSettingsError.addClass("display-none");

        PopupKeyUpActionProvider.CloseDialog();
        disableInterface();

        Teamlab.syncLdap({},
            {
                success: function () {
                    setProgress(status);
                    progressBarIntervalId = setInterval(checkStatus, 600);
                },
                error: onFailApi
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
        var $this = jq(this);

        if ($this.hasClass("disable")) return;

        StudioBlockUIManager.blockUI("#ldapSettingsInviteDialog", 500);
        PopupKeyUpActionProvider.EnterAction = "LdapSettings.restoreDefaultSettings();";
    }

    function disableInterface() {
        setPercentsExactly(constants.NULL_PERCENT);
        $ldapSettingsError.hide();
        $ldapSettingsSyncError.hide();
        $ldapSettingsProgressbarContainer.removeClass("display-none");
        $ldapSettingsSaveBtn.addClass("disable");
        $ldapSettingsRestoreDefaultSettings.addClass("disable");
        jq(".ldap-settings-sync-users").addClass("disable");
        $ldapSettingsMainContainer.addClass("ldap-settings-disabled-all");
        $ldapSettingsMainContainer.find("input").prop("disabled", false);
        $ldapSettingsMainContainer.off("click", ".ldap-settings-save");
        $ldapSettingsMainContainer.off("click", ".ldap-settings-restore-default-settings");
        $ldapSettingsMainContainer.off("click", ".ldap-settings-sync-users");

        $ldapMappingSettings.find(".ldap-mapping-remove-row").addClass("disable");
        $ldapMappingAddBtn.prop("disabled", true).addClass("disable");

        enableRestoreDefault(false);
        enableSave(false);
        enableSync(false);
        enableProgress(true);
    }

    function enableInterface(cancel) {
        if (!cancel) {
            refreshButtons();
        }
        $ldapSettingsMainContainer.removeClass("ldap-settings-disabled-all");
        $ldapSettingsBtn.prop("disabled", false).removeClass("disable");
        if ($ldapSettingsBtn.hasClass("on")) {
            $ldapSettingsStartTlsCheckbox.removeClass("disabled").prop("disabled", false);
            $ldapSettingsSslCheckbox.removeClass("disabled").prop("disabled", false);
            $ldapSettingsUserContainer.find("input").prop("disabled", false);
            $ldapSettingsUserContainer.find("textarea").prop("disabled", false);
            $ldapSettingsUserContainer.find(".selectBox:not(.locked)").removeClass("disabled");
            $ldapSettingsUserContainer.find(".on-off-button").removeClass("disable");
            $ldapSettingsAutoSyncBtn.removeClass("disable");
            $ldapSettingsGroupBtn.removeClass("disable");

            $ldapMappingAddBtn.prop("disable", false).removeClass("disable");
            $ldapMappingSettings.find(".ldap-mapping-remove-row").removeClass("disable");

            if ($ldapSettingsGroupBtn.hasClass("on")) {
                $ldapSettingsGroupContainer.find("input").prop("disabled", false);
                $ldapSettingsGroupContainer.find("textarea").prop("disabled", false);
                $ldapSettingsGroupContainer.find(".selectBox:not(.locked)").removeClass("disabled");
                $ldapSettingsGroupContainer.find(".on-off-button").removeClass("disable");
                $ldapMappingAddAccess.prop("disabled", false).removeClass("disabled");
            }
            $ldapSettingsAuthBtn.removeClass("disable");
            if ($ldapSettingsAuthBtn.hasClass("on")) {
                $ldapSettingsAuthContainer.find("input").prop("disabled", false);
                $ldapSettingsAuthContainer.find(".on-off-button").removeClass("disable");
            }
            if ($ldapSettingsAutoSyncBtn.hasClass("on")) {
                $ldapSettingsAutoSync.find("input").prop("disabled", false);
                $ldapSettingsAutoSync.find(".selectBox:not(.locked)").removeClass("disabled");
            }
        }

    }

    function saveSettings() {
        if (already) {
            return;
        }

        already = true;

        HideRequiredError();

        objectSettings = getSettings();
        $ldapSettingsMainContainer.find(".ldap-settings-empty-field").addClass("display-none");
        $ldapSettingsMainContainer.find(".ldap-settings-incorrect-number").addClass("display-none");

        if (!validateSettings(objectSettings)) {
            already = false;
            return;
        }

        setStatus("");
        setSource("");
        setPercents(constants.NULL_PERCENT);

        if ($ldapSettingsBtn.hasClass("on")) {
            StudioBlockUIManager.blockUI("#ldapSettingsImportUserLimitPanel", 500);
            PopupKeyUpActionProvider.EnableEsc = false;
            PopupKeyUpActionProvider.EnterAction = "LdapSettings.continueSaveSettings();";
        } else {
            continueSaveSettings();
        }
    }

    function validateSettings(settings) {
        var isValid = true;

        var validateKeyValuePairs = function () {
            var $input = jq(this).children("input");

            var select = (jq(this).children("select").val() || "").trim();
            var input = ($input.val() || "").trim();
            if (!select || !input) {
                jq(this).children(".requiredErrorText").text(ASC.Resources.Master.ResourceJS.LdapSettingsEmptyField);
                ShowRequiredError($input, !isValid, !isValid);
                isValid = false;
            }
        };

        if (settings.EnableLdapAuthentication) {
            if (settings.Server === "") {
                ShowRequiredError($ldapSettingsServer);
                isValid = false;
            }
            if (settings.UserDN === "") {
                ShowRequiredError($ldapSettingsUserDN, !isValid, !isValid);
                isValid = false;
            }
            if (settings.UserFilter === "") {
                ShowRequiredError($ldapSettingsUserFilter, !isValid, !isValid);
                isValid = false;
            }
            if (settings.PortNumber === "") {
                jq("#ldapSettingsPortNumberError").text(ASC.Resources.Master.ResourceJS.LdapSettingsEmptyField);
                ShowRequiredError($ldapSettingsPortNumber, !isValid, !isValid);
                isValid = false;
            } else if (!isInt(settings.PortNumber)) {
                jq("#ldapSettingsPortNumberError").text(ASC.Resources.Master.ResourceJS.LdapSettingsIncorrectPortNumber);
                ShowRequiredError($ldapSettingsPortNumber, !isValid, !isValid);
                isValid = false;
            }
            if (settings.LoginAttribute === "") {
                ShowRequiredError($ldapSettingsLoginAttribute, !isValid, !isValid);
                isValid = false;
            }

            $ldapMappingSettings.children().each(validateKeyValuePairs);

            var values = {};
            var uniqueErr = false;
            $ldapMappingSettings.children().each(function () {
                var val = jq(this).children("input").val();
                if (!val) return;

                var exist = values[val];
                if (exist) {
                    exist.children(".requiredErrorText").text("");
                    ShowRequiredError(exist.children("input"), !isValid, !isValid);
                    uniqueErr = true;
                    isValid = false;
                    jq(this).children(".requiredErrorText").text("");
                    ShowRequiredError(jq(this).children("input"), !isValid, !isValid);
                } else {
                    values[val] = jq(this);
                }
            });
            if (uniqueErr) toastr.error(ASC.Resources.Master.ResourceJS.ErrorBindingSameAttribute);

            if (settings.GroupMembership) {
                if (settings.GroupDN === "") {
                    ShowRequiredError($ldapSettingsGroupDN, !isValid, !isValid);
                    isValid = false;
                }
                if (settings.GroupFilter === "") {
                    ShowRequiredError($ldapSettingsGroupFilter, !isValid, !isValid);
                    isValid = false;
                }
                if (settings.UserAttribute === "") {
                    ShowRequiredError($ldapSettingsUserAttribute, !isValid, !isValid);
                    isValid = false;
                }
                if (settings.GroupAttribute === "") {
                    ShowRequiredError($ldapSettingsGroupAttribute, !isValid, !isValid);
                    isValid = false;
                }
                if (settings.GroupNameAttribute === "") {
                    ShowRequiredError($ldapSettingsGroupNameAttribute, !isValid, !isValid);
                    isValid = false;
                }

                $ldapMappingSecurity.children().each(validateKeyValuePairs);
            }
            if (settings.Authentication) {
                if (settings.Login === "") {
                    ShowRequiredError($ldapSettingsLogin, !isValid, !isValid);
                    isValid = false;
                }
                if (settings.Password === "") {
                    ShowRequiredError($ldapSettingsPassword, !isValid, !isValid);
                    isValid = false;
                }
            }
        }

        return isValid;
    }

    function continueSaveSettings(e, acceptCertificate) {

        PopupKeyUpActionProvider.CloseDialog();

        syncInProgress = false;

        disableInterface();

        if (!acceptCertificate) {
            acceptCertificate = false;
        }
        previousSettings = currentSettings;
        currentSettings = getSettings();

        Teamlab.saveLdapSettings({},
            {
                settings: JSON.stringify(currentSettings),
                acceptCertificate: acceptCertificate
            },
            {
                success: function (params, data) {
                    try {
                        var status = data;
                        if (status && status.id) {
                            setProgress(status);
                            progressBarIntervalId = setInterval(checkStatus, constants.GET_STATUS_TIMEOUT);
                        } else {
                            throw ASC.Resources.Master.ResourceJS.OperationFailedError;
                        }
                    } catch (error) {
                        showError(error);
                        currentSettings = previousSettings;
                        endProcess();
                    }
                },
                error: onFailApi
            });
    }

    function onFailApi(jqXHR, textStatus) {
        if (textStatus !== null && textStatus === "abort") {
            return;
        }
        showError(ASC.Resources.Master.ResourceJS.OperationFailedError);
        currentSettings = previousSettings;
        endProcess();
    }

    function endProcess() {
        if (progressBarIntervalId) {
            clearInterval(progressBarIntervalId);
        }
        already = false;
        enableInterface(false);
        if (isRestoreDefault) {
            enableRestoreDefault(false);
        }
    }

    function showError(error) {
        var errorMessage;

        if (typeof (error) === "string") {
            errorMessage = error;
        }
        else if (error.message) {
            errorMessage = error.message;
        } else if (error.responseText) {
            try {
                var json = JSON.parse(error.responseText);

                if (typeof (json) === "object") {
                    if (json.ExceptionMessage) {
                        errorMessage = json.ExceptionMessage;
                    }
                    else if (json.Message) {
                        errorMessage = json.Message;
                    }
                }
                else if (typeof (json) === "string") {
                    errorMessage = error.responseText.replace(/(^")|("$)/g, "");

                    if (!errorMessage.length && error.statusText) {
                        errorMessage = error.statusText;
                    }
                }
            } catch (e) {
                errorMessage = error.responseText;
            }
        } else if (error.statusText) {
            errorMessage = error.statusText;
        } else if (error.error) {
            errorMessage = error.error;
        }

        errorMessage = !errorMessage || typeof (errorMessage) !== "string" || !errorMessage.length ?
            ASC.Resources.Master.ResourceJS.OperationFailedError :
            errorMessage.replace(/(^")|("$)/g, "");

        if (!errorMessage.length) {
            console.error('showError failed with ', error);
            return;
        }

        if (syncInProgress) {
            $ldapSettingsSyncError.text(errorMessage);
            $ldapSettingsSyncError.show();
        } else {
            $ldapSettingsError.text(errorMessage);
            $ldapSettingsError.show();
        }
        setStatus("");
        setSource("");
        setPercents(constants.NULL_PERCENT);
        toastr.error(errorMessage);
    }

    function setProgress(status) {
        setPercents(status.percents);

        if (status.completed) {
            if (!status.error) {
                setStatus(ASC.Resources.Master.ResourceJS.LdapSettingsSuccess);
                setSource("");
            }
            else {

            }
        } else {
            setStatus(status.status);
            setSource(status.source);
        }
    }

    function checkStatus() {
        if (alreadyChecking) {
            return;
        }
        alreadyChecking = true;
        Teamlab.getLdapStatus({},
            {
                success: onGetStatus,
                error: function () {
                    alreadyChecking = false;
                }
            });
    }
    
    function mapError(error) {
        switch (error) {
            case ldapCertificateProblem.CertExpired:
                return ASC.Resources.Master.ResourceJS.LdapSettingsCertExpired;
            case ldapCertificateProblem.CertCnNoMatch:
                return ASC.Resources.Master.ResourceJS.LdapSettingsCertCnNoMatch;
            case ldapCertificateProblem.CertIssuerChaining:
                return ASC.Resources.Master.ResourceJS.LdapSettingsCertIssuerChaining;
            case ldapCertificateProblem.CertUntrustedCa:
                return ASC.Resources.Master.ResourceJS.LdapSettingsCertUntrustedCa;
            case ldapCertificateProblem.CertUntrustedRoot:
                return ASC.Resources.Master.ResourceJS.LdapSettingsCertUntrustedRoot;
            case ldapCertificateProblem.CertMalformed:
                return ASC.Resources.Master.ResourceJS.LdapSettingsCertMalformed;
            case ldapCertificateProblem.CertUnrecognizedError:
                return ASC.Resources.Master.ResourceJS.LdapSettingsCertUnrecognizedError;
            case ldapCertificateProblem.CertValidityPeriodNesting:
            case ldapCertificateProblem.CertRole:
            case ldapCertificateProblem.CertPathLenConst:
            case ldapCertificateProblem.CertCritical:
            case ldapCertificateProblem.CertPurpose:
            case ldapCertificateProblem.CertChainnig:
            case ldapCertificateProblem.CertRevoked:
            case ldapCertificateProblem.CertUntrustedTestRoot:
            case ldapCertificateProblem.CertRevocationFailure:
            case ldapCertificateProblem.CertWrongUsage:
                return "";
        }

        return "";
    }

    function onGetStatus(params, data) {
        alreadyChecking = false;
        try {

            if (data.error) {
                if (data.certificateConfirmRequest && data.certificateConfirmRequest.certificateErrors) {
                    var errors = data.certificateConfirmRequest.certificateErrors
                        .map((item) => mapError(item));
                    data.certificateConfirmRequest.certificateErrors = errors;
                }
            }

            var status = data;
            if (jq.isEmptyObject(data)) {
                status = {
                    completed: true,
                    percents: 100,
                    certificateConfirmRequest: null,
                    error: ""
                };
            }

            setProgress(status);

            if (status.warning && lastWarning !== status.warning) {
                lastWarning = status.warning;
                toastr.warning(status.warning, "", { timeOut: 0, extendedTimeOut: 0 });
            }

            if (isCompleted(status)) {
                lastWarning = "";

                if (status.error)
                    throw status.error;

                endProcess();
            }

        } catch (error) {
            showError(error);
            currentSettings = previousSettings;
            endProcess();
        }
    }

    function isCompleted(status) {
        if (!status)
            return true;

        if (!status.completed)
            return false;

        if (status.certificateConfirmRequest &&
            status.certificateConfirmRequest.requested) {
            setCertificateDetails(status.certificateConfirmRequest);
            currentSettings = previousSettings;
            /* popupId, width, height, marginLeft, marginTop */
            StudioBlockUIManager.blockUI("#ldapSettingsCertificateValidationDialog", 500);
            return true;
        }

        if (status.error) {
            return true;
        }

        toastr.success(ASC.Resources.Master.ResourceJS.LdapSettingsSuccess);
        return true;
    }

    function setCertificateDetails(certificateConfirmRequest) {
        $ldapSettingsCertificateValidationDialog.find(".ldap-settings-serial-number").text(certificateConfirmRequest.serialNumber);
        $ldapSettingsCertificateValidationDialog.find(".ldap-settings-issuer-name").text(certificateConfirmRequest.issuerName);
        $ldapSettingsCertificateValidationDialog.find(".ldap-settings-subject-name").text(certificateConfirmRequest.subjectName);
        $ldapSettingsCertificateValidationDialog.find(".ldap-settings-valid-from").text(certificateConfirmRequest.validFrom);
        $ldapSettingsCertificateValidationDialog.find(".ldap-settings-valid-until").text(certificateConfirmRequest.validUntil);
        $ldapSettingsCertificateValidationDialog.find(".ldap-settings-unique-hash").text(certificateConfirmRequest.hash);
        $ldapSettingsCertificateValidationDialog.find(".toast-container").remove();
        var html = jq("#ldapCrtErrorTmpl")
            .tmpl({
                errors: function () {
                    var certificateErrors = certificateConfirmRequest.certificateErrors,
                        errors = [];
                    for (var i = 0; i < certificateErrors.length; i++) {
                        errors[i] = {};
                        errors[i].message = certificateErrors[i];
                    }
                    return errors;
                }
            });
        $ldapSettingsCertificateValidationDialog.find(".ldap-settings-crt-details-last").after(html);
    }

    function setPercentsExactly(percents) {
        jq(".asc-progress-value").css("width", percents + "%");
        jq("#ldapSettingsPercent").text(percents + "% ");
    }

    function setPercents(percent) {
        if (percent === undefined)
            return;

        var value = percent + "%";

        if (syncInProgress) {
            if (percent === constants.NULL_PERCENT || percent <= lastPercent) {
                $ldapSettingsSyncProgressValue.css({ "width": value });
            } else {
                $ldapSettingsSyncProgressValue.animate({ "width": value });
            }
            $ldapSettingsSyncPercent.text(value + " ");
        } else {
            if (percent === constants.NULL_PERCENT || percent <= lastPercent) {
                $ldapSettingsProgressValue.css({ "width": value });
            } else {
                $ldapSettingsProgressValue.animate({ "width": value });
            }
            $ldapSettingsPercent.text(value + " ");
        }

        lastPercent = percent;
    }

    function setStatus(status) {
        if (syncInProgress)
            $ldapSettingsSyncStatus.text(status);
        else
            $ldapSettingsStatus.text(status);
    }

    function setSource(source) {
        if (syncInProgress)
            $ldapSettingsSyncSource.text(source);
        else
            $ldapSettingsSource.text(source);
    }
    function clearAllMappingRows(el) {
        el.find(".ldap-mapping-row").remove();
    }
    function loadSettings(settings) {
        if (settings) {
            if (settings["enableLdapAuthentication"]) {
                $ldapSettingsBtn.removeClass("off").addClass("on");
            } else {

                $ldapSettingsStartTlsCheckbox.prop('checked', false);
                $ldapSettingsSslCheckbox.prop('checked', false);
            }
            if (settings["startTls"]) {
                $ldapSettingsStartTlsCheckbox.prop('checked', true);
            } else {
                $ldapSettingsBtn.removeClass("on").addClass("off");
            }
            if (settings["ssl"]) {
                $ldapSettingsSslCheckbox.prop('checked', true);
            } else {
                $ldapSettingsBtn.removeClass("on").addClass("off");
            }
            if (settings["sendWelcomeEmail"] == "true") {
                $ldapSettingsSendWelcomeEmailCheckbox.prop('checked', true);
            }
            $ldapSettingsServer.val(settings["server"]);
            $ldapSettingsUserDN.val(settings["userDN"]);
            $ldapSettingsPortNumber.val(settings["portNumber"]);
            $ldapSettingsUserFilter.val(settings["userFilter"]);
            $ldapSettingsLoginAttribute.val(settings["loginAttribute"]);

            var ldapMapping = settings["ldapMapping"];
            var accessRights = settings["accessRights"];

            clearAllMappingRows($ldapMappingSettings);
            if (ldapMapping) {
                for (var key in ldapMapping) {     //TODO bring everything to the same case
                    addMappingRow($ldapMappingSettings, key, ldapMapping[key], ldapMappingRes[key], ldapMappingRes, ASC.Resources.Master.ResourceJS.LdapAttributeOrigin, ldapMappingRequiredOptions.indexOf(key[0].toUpperCase() + key.slice(1)) !== -1);
                }
            }

            clearAllMappingRows($ldapMappingSecurity);
            if (accessRights) {
                for (var key in accessRights) {
                    addMappingRow($ldapMappingSecurity, key, ldapMapping[key], ldapSecurityRes[key], ldapMappingRes, ASC.Resources.Master.ResourceJS.LdapSecurityPlaceholder);
                }
            }

            if (settings["enableLdapAuthentication"]) {
                $ldapSettingsGroupBtn.removeClass("disable");
                $ldapSettingsAuthBtn.removeClass("disable");
            } else {
                $ldapSettingsGroupBtn.addClass("disable");
                $ldapSettingsAuthBtn.addClass("disable");
            }

            if (settings["groupMembership"]) {
                $ldapSettingsGroupBtn.removeClass("off").addClass("on");
            } else {
                $ldapSettingsGroupBtn.removeClass("on").addClass("off");
            }
            $ldapSettingsGroupDN.val(settings["groupDN"]);
            $ldapSettingsUserAttribute.val(settings["userAttribute"]);
            $ldapSettingsGroupFilter.val(settings["groupFilter"]);
            $ldapSettingsGroupAttribute.val(settings["groupAttribute"]);
            $ldapSettingsGroupNameAttribute.val(settings["groupNameAttribute"]);

            if (settings["authentication"]) {
                $ldapSettingsAuthBtn.removeClass("off").addClass("on");
            } else {
                $ldapSettingsAuthBtn.removeClass("on").addClass("off");
            }
            $ldapSettingsLogin.val(settings["login"]);
            $ldapSettingsPassword.val(settings["password"]);

            disableNeededBlocks(settings["enableLdapAuthentication"],
                settings["groupMembership"],
                settings["authentication"]);
        }
    }

    function disableNeededBlocks(enableLdapAuthentication, groupMembership, authentication) {
        if (!enableLdapAuthentication) {
            $ldapSettingsMainContainer.find("input:not(#ldapSettingsCheckbox)").prop("disabled", false);
            jq(".ldap-settings-label-checkbox:not(.ldap-settings-never-disable)").addClass("ldap-settings-disabled");
            $ldapSettingsUserContainer.addClass("ldap-settings-disabled");
            $ldapSettingsGroupContainer.addClass("ldap-settings-disabled");
            $ldapSettingsAuthContainer.addClass("ldap-settings-disabled");
        } else {
            if (!groupMembership) {
                $ldapSettingsGroupContainer.find("input").prop("disabled", false);
                $ldapSettingsGroupContainer.addClass("ldap-settings-disabled");
            }
            if (!authentication) {
                $ldapSettingsAuthContainer.find("input").prop("disabled", false);
                $ldapSettingsAuthContainer.addClass("ldap-settings-disabled");
            }
        }
    }


    return {
        init: init,
        restoreDefaultSettings: restoreDefaultSettings,
        continueSaveSettings: continueSaveSettings,
        syncUsersLDAP: syncUsersLDAP
    };
};

function isObjectsEqual(obj, secondObj) {
    if (!obj || !secondObj) {
        return false;
    }

    var objKeys = [];
    var secondObjKeys = [];

    for (var key in obj) {
        objKeys.push(key);
    }

    for (var key in secondObj) {
        secondObjKeys.push(key);
    }

    if (objKeys.length !== secondObjKeys.length) {
        return false;
    }

    for (var i in objKeys) {
        var key = objKeys[i];
        if (obj[key] !== secondObj[key]) {
            return false;
        }
    }

    return true;
}

var spoiler = function () {
    var toggle = function (toggleEl, spoilerEl, force, hideLinkText, showLinkText) {
        var el = jq(toggleEl);
        var $this = jq(spoilerEl);
        if (!el)
            return;

        var enabled = typeof force === "boolean" ? force : el.hasClass('display-none');
        var linkText = "";

        if (enabled) {
            el.toggleClass('display-none', false);
            if ($this) {
                linkText = hideLinkText || ASC.Resources.Master.ResourceJS.HideLink;
                $this.text(linkText).prop('title', linkText);
            }
        } else {
            el.toggleClass('display-none', true);
            if ($this) {
                linkText = showLinkText || ASC.Resources.Master.ResourceJS.ShowLink;
                $this.text(linkText).prop('title', linkText);
            }
        }
    };

    return {
        toggle: toggle
    };
}();

jq(function () {
    LdapSettings.init();
});