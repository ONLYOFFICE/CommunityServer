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


window.SmtpSettingsView = function ($) {
    var $view,
        $settingsSwitch,
        $customSettingsRadio,
        $mailserverSettingsRadio,
        $customSettingsBox,
        $mailserverSettingsBox,
        currentSettings,
        server,
        domains = [],
        buttonsIds =
        {
            "save": "saveSettingsBtn",
            "restore": "saveDefaultCustomSettingsBtn",
            "test": "sendTestMailBtn",
            "switchCustom": "customSettingsRadio",
            "switchMserver": "mailserverSettingsRadio"
        },
        isDefault,
        progressBarIntervalId = null,
        checkStatusTimeout = 1000;;

    function generatePassword() {
        var lowercase = "abcdefghijklmnopqrstuvwxyz",
            uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            numbers = "0123456789",
            specials = "_";

        var all = lowercase + uppercase + numbers + specials;

        function generate() {
            var password = "";

            password += pick(lowercase, 1);
            password += pick(uppercase, 1);
            password += pick(numbers, 1);
            password += pick(specials, 1);
            password += pick(all, 3, 8);

            password = shuffle(password);

            password = pick(uppercase, 1) + password;

            return password;
        }

        function pick(str, min, max) {
            var n;
            var chars = "";

            if (typeof max === "undefined") {
                n = min;
            } else {
                n = min + Math.floor(Math.random() * (max - min));
            }

            for (var i = 0; i < n; i++) {
                chars += str.charAt(Math.floor(Math.random() * str.length));
            }

            return chars;
        }

        function shuffle(str) {
            var chars = str.split("");
            var tempChar;
            var currentChar;
            var topChar = chars.length;

            if (topChar) {
                while (--topChar) {
                    currentChar = Math.floor(Math.random() * (topChar + 1));
                    tempChar = chars[currentChar];
                    chars[currentChar] = chars[topChar];
                    chars[topChar] = tempChar;
                }
            }

            return chars.join("");
        }

        return generate();
    }

    function currentHostUseMailserver() {
        return window.SmtpSettingsConstants.IsMailServerAvailable && server && server.dns.mxRecord.host === currentSettings.host;
    }

    function saveCurrentSettings(settings) {
        currentSettings = settings;
        if (currentSettings && currentSettings.port && typeof (currentSettings.port) === "string")
            currentSettings.port = parseInt(currentSettings.port);
    }

    function init() {
        window.LoadingBanner.displayLoading();

        initElements();
        bindEvents();

        saveCurrentSettings(getInitCurrentSettings());

        if (!window.SmtpSettingsConstants.IsMailServerAvailable) {
            renderView();
            window.LoadingBanner.hideLoading();
            return;
        }

        window.async.parallel([
            function (cb) {
                window.Teamlab.getMailServer(null, {
                    success: function (params, res) {
                        if (res && res.id) {
                            server = res;
                            cb(null);
                        } else {
                            cb("mailserver does not exist");
                        }
                    },
                    error: function () {
                        cb(null);
                    }
                });
            },
            function (cb) {
                window.Teamlab.getMailDomains(null, {
                    success: function (params, res) {
                        domains = res;
                        cb(null);
                    },
                    error: function () {
                        // ignore
                        cb(null);
                    }
                });
            }
        ], function (err) {
            if (err) {
                toastr.error(err);
            } else {
                renderView();
            }

            window.LoadingBanner.hideLoading();
        });
    }

    function initElements() {
        $view = $("#smtpSettingsView");
        $settingsSwitch = $view.find("#settingsSwitch");
        $customSettingsRadio = $settingsSwitch.find("#customSettingsRadio");
        $mailserverSettingsRadio = $settingsSwitch.find("#mailserverSettingsRadio");
        $customSettingsBox = $view.find("#customSettingsBox");
        $mailserverSettingsBox = $view.find("#mailserverSettingsBox");
    }

    function bindEvents() {
        $customSettingsRadio.on("change", $.proxy(switchToCustomSettingsBox, this));
        $mailserverSettingsRadio.on("change", $.proxy(switchToMailserverSettingsBox, this));
        $customSettingsBox.on("change", "#customSettingsAuthenticationRequired", changeSettingsAuthenticationRequired);

        $view.find("#saveSettingsBtn").unbind("click").bind("click", function (e) {
            e.preventDefault();
            e.stopPropagation();

            if (!$(this).hasClass("disable"))
                saveSettings();

            return false;
        });
        $view.find("#saveDefaultCustomSettingsBtn").unbind("click").bind("click", function (e) {
            e.preventDefault();
            e.stopPropagation();

            if (!$(this).hasClass("disable"))
                restoreDefaults();

            return false;
        });
        $view.find("#sendTestMailBtn").unbind("click").bind("click", function (e) {
            e.preventDefault();
            e.stopPropagation();

            if (!$(this).hasClass("disable"))
                sendTestMail();

            return false;
        });

        AjaxPro.onError = function (e) {
            hideLoader();
            LoadingBanner.showMesInfoBtn("#smtpSettingsView", e && e.Message ? e.Message : ASC.Resources.Master.Resource.OperationFailedMsg, "error");
            console.error("SmtpSettingsView: AjaxPro.onError", e);
        }

        AjaxPro.onTimeout = function () {
            hideLoader();
            LoadingBanner.showMesInfoBtn("#smtpSettingsView", ASC.Resources.Master.Resource.OperationFailedMsg, "error");
            console.error("SmtpSettingsView: AjaxPro.onTimeout", arguments);
        }
    }

    function bindChanges() {
        $("#smtpSettingsView input")
            .off('input')
            .on('input',
                function() {
                    setupButtons($customSettingsRadio.prop("checked"));
                });

        $("#smtpSettingsView select, #smtpSettingsView input[type=\"checkbox\"]")
            .off("change")
            .on("change",
                function() {
                    setupButtons($customSettingsRadio.prop("checked"));
                });
    }

    function renderView() {
        if (currentHostUseMailserver()) {
            renderCustomSettingsBox(getEmptyCustomSettings());
            renderMailserverSettingsBox(currentSettings);
            switchToMailserverSettingsBox();
        } else {
            renderCustomSettingsBox(currentSettings);
            renderMailserverSettingsBox(getEmptyCustomSettings());
            switchToCustomSettingsBox();
        }

        if (server) {
            $settingsSwitch.show();
        }
        $view.show();
    }

    function renderCustomSettingsBox(settings) {
        var html = $customSettingsBox.siblings("#customSettingsBoxTmpl").tmpl(settings);
        $customSettingsBox.html(html);
        changeSettingsAuthenticationRequired();
    }

    function renderMailserverSettingsBox(settings) {
        var customDomains = [];
        for (var i = 0; i < domains.length; i++) {
            if (!domains[i].isSharedDomain) {
                customDomains.push(domains[i]);
            }
        }

        var html = $mailserverSettingsBox.siblings("#mailserverSettingsBoxTmpl").tmpl(
            {
                domains: customDomains,
                login: settings.credentialsUserName || "",
                password: settings.credentialsUserPassword || "",
                senderDisplayName: settings.senderDisplayName || ""
            });
        $mailserverSettingsBox.html(html);
    }

    function setupButtons(forCustom) {
        var showBtnIds = [],
            hideBtnIds = [];

        if (forCustom) {
            if (equalsSettings(getCustomSettings(false), currentSettings)) {
                hideBtnIds.push(buttonsIds.save);
                showBtnIds.push(buttonsIds.test);
            } else {
                showBtnIds.push(buttonsIds.save);
                hideBtnIds.push(buttonsIds.test);
            }

            if (isDefault) {
                hideBtnIds.push(buttonsIds.restore);
            } else {
                showBtnIds.push(buttonsIds.restore);
            }

        } else {
            var msSettings = getMailServerSettings(false);

            if (msSettings.login.length === 0) {
                hideBtnIds.push(buttonsIds.test);
                hideBtnIds.push(buttonsIds.save);
            }
            else if (((msSettings.login + "@" + msSettings.domain) === currentSettings.senderAddress) &&
                msSettings.senderDisplayName === currentSettings.senderDisplayName) {
                hideBtnIds.push(buttonsIds.save);
                showBtnIds.push(buttonsIds.test);
            } else {
                showBtnIds.push(buttonsIds.save);
                if(currentHostUseMailserver())
                    showBtnIds.push(buttonsIds.test);
            }

            if (isDefault)
                hideBtnIds.push(buttonsIds.restore);
            else
                showBtnIds.push(buttonsIds.restore);
        }

        disableButtonsByIds(hideBtnIds, true);
        disableButtonsByIds(showBtnIds, false);
    }

    function switchToCustomSettingsBox() {
        $mailserverSettingsRadio.prop("checked", false);
        $customSettingsRadio.prop("checked", true);
        $mailserverSettingsBox.hide();
        $customSettingsBox.show();

        setupButtons(true);
        bindChanges();
    }

    function switchToMailserverSettingsBox() {
        $customSettingsRadio.prop("checked", false);
        $mailserverSettingsRadio.prop("checked", true);
        $customSettingsBox.hide();
        $mailserverSettingsBox.show();

        setupButtons(false);
        bindChanges();
    }

    function changeSettingsAuthenticationRequired() {
        var checked = $customSettingsBox.find("#customSettingsAuthenticationRequired").is(":checked"),
            $loginEl = $customSettingsBox.find(".host-login"),
            $passwordEl = $customSettingsBox.find(".host-password");

        $loginEl.find(".textEdit").attr("disabled", !checked);
        $passwordEl.find(".textEdit").attr("disabled", !checked);

        $loginEl.toggleClass('requiredField', checked);
        $passwordEl.toggleClass('requiredField', checked);

        if (!checked) {
            $loginEl.toggleClass('requiredFieldError', false);
            $passwordEl.toggleClass('requiredFieldError', false);
        }

    }

    function disableButton(btnId, disable) {
        var $btn = $view.find("#" + btnId);
        if (!$btn)
            return;

        $btn.toggleClass("disable", disable).attr('disabled', disable);
    }

    function blockControls(disable) {
        disableButtonsByIds([
            buttonsIds.save,
            buttonsIds.restore,
            buttonsIds.test,
            buttonsIds.switchCustom,
            buttonsIds.switchMserver
        ], disable);

        var $senderNameEl,
            $senderAddressEl,
            fromCustom = $customSettingsRadio.prop("checked");

        if (fromCustom) {
            $senderNameEl = $customSettingsBox.find(".display-name .textEdit");
            $senderAddressEl = $customSettingsBox.find(".email-address .textEdit");
            var $hostEl = $customSettingsBox.find(".host .textEdit"),
                $portEl = $customSettingsBox.find(".port .textEdit"),
                $authCheckbox = $customSettingsBox.find("#customSettingsAuthenticationRequired"),
                $loginEl = $customSettingsBox.find(".host-login .textEdit"),
                $passwordEl = $customSettingsBox.find(".host-password .textEdit"),
                $sslCheckbox = $customSettingsBox.find("#customSettingsEnableSsl");

            $hostEl.attr("disabled", disable);
            $portEl.attr("disabled", disable);
            $authCheckbox.attr("disabled", disable);

            $loginEl.attr("disabled", disable);
            $passwordEl.attr("disabled", disable);

            $senderNameEl.attr("disabled", disable);
            $senderAddressEl.attr("disabled", disable);

            $sslCheckbox.attr("disabled", disable);

            if (!disable)
                changeSettingsAuthenticationRequired();
        } else {
            $senderNameEl = $mailserverSettingsBox.find(".display-name .textEdit");
            $senderAddressEl = $mailserverSettingsBox.find(".email-address .textEdit");
            var $domainSelectEl = $mailserverSettingsBox.find("#notificationDomain");

            $senderNameEl.attr("disabled", disable);
            $senderAddressEl.attr("disabled", disable);

            $domainSelectEl.attr("disabled", disable);
        }

        if (!disable) {
            setupButtons(fromCustom);
        }
    }

    function disableButtonsByIds(ids, disable) {
        for (var i = 0, len = ids.length; i < len; i++) {
            disableButton(ids[i], disable);
        }
    }

    function equalsSettings(settings1, settings2) {
        return settings1.host === settings2.host &&
            settings1.port === settings2.port &&
            settings1.credentialsUserName === (settings2.credentialsUserName || "") &&
            settings1.credentialsUserPassword === (settings2.credentialsUserPassword || "") &&
            settings1.senderDisplayName === settings2.senderDisplayName &&
            settings1.senderAddress === settings2.senderAddress &&
            settings1.enableSSL === settings2.enableSSL &&
            settings1.enableAuth === settings2.enableAuth;
    }

    function getSettingsForTest() {
        return $customSettingsRadio.prop("checked") ? getCustomSettings(true, true) : getEmptyCustomSettings();
    }

    function getCustomSettings(checkRequired, skipPassword) {
        var settingsCorrected = true;

        var host = $customSettingsBox.find(".host .textEdit").val(),
            port = $customSettingsBox.find(".port .textEdit").val(),
            enableAuth = $customSettingsBox.find("#customSettingsAuthenticationRequired").is(":checked"),
            credentialsUserName = $customSettingsBox.find(".host-login .textEdit").val(),
            credentialsUserPassword = $customSettingsBox.find(".host-password .textEdit").val(),
            senderDisplayName = $customSettingsBox.find(".display-name .textEdit").val(),
            senderAddress = $customSettingsBox.find(".email-address .textEdit").val(),
            enableSsl = $customSettingsBox.find("#customSettingsEnableSsl").is(":checked");

        host = !host ? "" : host.trim();
        port = !port ? null : parseInt(port);
        credentialsUserName = !credentialsUserName ? "" : credentialsUserName.trim();
        senderDisplayName = !senderDisplayName ? "" : senderDisplayName.trim();
        senderAddress = !senderAddress ? "" : senderAddress.trim();

        if (checkRequired) {
            if (!host) {
                $customSettingsBox.find(".host").toggleClass("requiredFieldError", true);
                settingsCorrected = false;
            } else
                $customSettingsBox.find(".host").toggleClass("requiredFieldError", false);

            if (!port || port === NaN) {
                $customSettingsBox.find(".port").toggleClass("requiredFieldError", true);
                settingsCorrected = false;
            } else
                $customSettingsBox.find(".port").toggleClass("requiredFieldError", false);

            if (enableAuth && !credentialsUserName) {
                $customSettingsBox.find(".host-login").toggleClass("requiredFieldError", true);
                settingsCorrected = false;
            } else
                $customSettingsBox.find(".host-login").toggleClass("requiredFieldError", false);

            if (!skipPassword) {
                if (enableAuth && !credentialsUserPassword) {
                    $customSettingsBox.find(".host-password").toggleClass("requiredFieldError", true);
                    settingsCorrected = false;
                } else
                    $customSettingsBox.find(".host-password").toggleClass("requiredFieldError", false);
            }

            if (!ASC.Mail.Utility.IsValidEmail(senderAddress)) {
                $customSettingsBox.find(".email-address .requiredErrorText").text(ASC.Resources.Master.Resource.ErrorNotCorrectEmail);
                $customSettingsBox.find(".email-address").toggleClass("requiredFieldError", true);
                settingsCorrected = false;
            } else
                $customSettingsBox.find(".email-address").toggleClass("requiredFieldError", false);
        }

        return settingsCorrected ? {
            host: host,
            port: port,
            credentialsUserName: credentialsUserName,
            credentialsUserPassword: credentialsUserPassword,
            senderDisplayName: senderDisplayName,
            senderAddress: senderAddress,
            enableSSL: enableSsl,
            enableAuth: enableAuth
        } : null;
    }

    function getMailServerSettings(checkRequired) {
        var settingsCorrected = true;

        var login = $mailserverSettingsBox.find("#notificationLogin").val(),
            domainId = $mailserverSettingsBox.find("#notificationDomain").val(),
            domain = $mailserverSettingsBox.find("#notificationDomain option:selected").text(),
            senderDisplayName = $mailserverSettingsBox.find("#notificationSenderDisplayName").val(),
            password = "";

        login = !login ? "" : login.trim();
        domain = !domain ? "" : domain.trim();
        senderDisplayName = !senderDisplayName ? "" : senderDisplayName.trim();

        if (checkRequired) {
            if (!login || !ASC.Mail.Utility.IsValidEmail(login + "@" + domain)) {
                $mailserverSettingsBox.find(".email-address .requiredErrorText").text(ASC.Resources.Master.Resource.ErrorNotCorrectEmail);
                $mailserverSettingsBox.find(".email-address").addClass("requiredFieldError");
                settingsCorrected = false;
            }
            password = generatePassword();
        }

        return settingsCorrected ? {
            login: login,
            domain: domain,
            domainId: domainId,
            password: password,
            senderDisplayName: senderDisplayName
        } : null;
    }

    function getInitCurrentSettings() {
        var $box = $view.find("#currentSettingsBox");

        isDefault = $("#currentIsDefault").val().toLowerCase() === "true";

        return {
            host: $box.find("#currentHost").val(),
            port: $box.find("#currentPort").val(),
            credentialsUserName: $box.find("#currentCredentialsUserName").val(),
            credentialsUserPassword: $box.find("#currentCredentialsUserPassword").val(),
            senderDisplayName: $box.find("#currentSenderDisplayName").val(),
            senderAddress: $box.find("#currentSenderAddress").val(),
            enableSSL: $("#currentEnableSsl").val().toLowerCase() === "true",
            enableAuth: $("#currentEnableAuth").val().toLowerCase() === "true"
        };
    }

    function clearErrors() {
        $view.find(".requiredFieldError").removeClass("requiredFieldError");
    }

    function getEmptyCustomSettings() {
        return {
            host: "",
            port: "",
            credentialsUserName: "",
            credentialsUserPassword: "",
            senderDisplayName: "",
            senderAddress: "",
            enableSSL: false,
            enableAuth: false
        };
    }

    function showLoader() {
        LoadingBanner.showLoaderBtn("#smtpSettingsView");
        blockControls(true);
    }

    function hideLoader() {
        LoadingBanner.hideLoaderBtn("#smtpSettingsView");
        blockControls(false);
    }

    function saveSettings() {
        if ($customSettingsRadio.prop("checked")) {
            saveCustomSettings();
        } else {
            saveMailserverSettings();
        }
    }

    function saveCustomSettings() {
        clearErrors();

        var settings = getCustomSettings(true);
        if (!settings || equalsSettings(settings, currentSettings)) {
            return false;
        }

        showLoader();
        var useMailServer = currentHostUseMailserver();
        var oldNotificationAddress = currentSettings.credentialsUserName;

        window.async.waterfall([
            function (cb) {
                Teamlab.savePortalSmtpSettings(null, settings, {
                    success: function (e, result) {
                        saveCurrentSettings(result);
                        renderCustomSettingsBox(currentSettings);
                        renderMailserverSettingsBox(getEmptyCustomSettings());
                        switchToCustomSettingsBox();

                        cb(null, result);
                    },
                    error: function (e, err) {
                        cb(err[0]);
                    }
                });
            },
            function (result, cb) {
                if (!useMailServer) {
                    cb(result.error ? result.error.Message : null);
                    return;
                }

                Teamlab.removeNotificationAddress(null, oldNotificationAddress, {
                    success: function () {
                        cb(null);
                    },
                    error: function () {
                        cb(null);
                    }
                });
            }
        ], function (err) {
            hideLoader();

            if (err) {
                LoadingBanner.showMesInfoBtn("#smtpSettingsView", err, "error");
            } else {
                LoadingBanner.showMesInfoBtn("#smtpSettingsView", ASC.Resources.Master.Resource.OperationSuccededMsg, "success");
                isDefault = false;
            }

            setupButtons(true);
        });

        return false;
    }

    function saveMailserverSettings() {
        clearErrors();

        var mailserverSettings = getMailServerSettings(true);
        if (!mailserverSettings || equalsSettings(mailserverSettings, currentSettings)) {
            return false;
        }

        showLoader();

        var useMailServer = currentHostUseMailserver();

        window.async.waterfall([
            function (cb) {
                if (!useMailServer) {
                    cb(null);
                    return;
                }

                Teamlab.removeNotificationAddress(null, currentSettings.credentialsUserName, {
                    success: function () {
                        cb(null);
                    },
                    error: function () {
                        cb(null);
                    }
                });
            },
            function (cb) {
                Teamlab.createNotificationAddress(null, mailserverSettings.login, mailserverSettings.password, mailserverSettings.domainId, {
                    success: function (params, res) {
                        cb(null, res);
                    },
                    error: function (params, err) {
                        cb(err[0]);
                    }
                });
            },
            function (res, cb) {
                var settings = {
                    host: res.smtp_server,
                    port: res.smtp_port,
                    credentialsUserName: res.smtp_account,
                    credentialsUserPassword: mailserverSettings.password,
                    senderDisplayName: mailserverSettings.senderDisplayName,
                    senderAddress: res.email,
                    enableSSL: res.smtp_encryption_type === "STARTTLS" || res.smtp_encryption_type === "SSL",
                    enableAuth: true
                };

                Teamlab.savePortalSmtpSettings(null, settings, {
                    success: function (e, result) {
                        saveCurrentSettings(result);
                        renderCustomSettingsBox(getEmptyCustomSettings());
                        renderMailserverSettingsBox(currentSettings);
                        switchToMailserverSettingsBox();
                        cb(null);
                    },
                    error: function (e, err) {
                        cb(err[0]);
                    }
                });

            }
        ], function (err) {
            hideLoader();

            if (err) {
                LoadingBanner.showMesInfoBtn("#smtpSettingsView", err, "error");
            } else {
                LoadingBanner.showMesInfoBtn("#smtpSettingsView", ASC.Resources.Master.Resource.OperationSuccededMsg, "success");
                isDefault = false;
            }

            setupButtons(false);
        });

        return false;
    }

    function restoreDefaults() {
        showLoader();
        var useMailServer = currentHostUseMailserver();

        window.async.waterfall([
            function (cb) {
                if (!useMailServer) {
                    cb(null);
                    return;
                }

                Teamlab.removeNotificationAddress(null, currentSettings.credentialsUserName, {
                    success: function () {
                        cb(null);
                    },
                    error: function () {
                        cb(null);
                    }
                });
            },
            function (cb) {
                Teamlab.resetPortalSmtpSettings(null, {
                    success: function (e, result) {
                        saveCurrentSettings(result);
                        renderCustomSettingsBox(currentSettings);
                        renderMailserverSettingsBox(getEmptyCustomSettings());
                        switchToCustomSettingsBox();
                        cb(null);
                    },
                    error: function (e, err) {
                        cb(err[0]);
                    }
                });
            }
        ], function (err) {
            hideLoader();

            if (err) {
                LoadingBanner.showMesInfoBtn("#smtpSettingsView", err, "error");
            } else {
                LoadingBanner.showMesInfoBtn("#smtpSettingsView", ASC.Resources.Master.Resource.OperationSuccededMsg, "success");
                isDefault = true;
            }

            setupButtons(true);
        });

        return false;
    }

    function sendTestMail() {
        clearErrors();

        var settings = getSettingsForTest();
        if (!settings) {
            return false;
        }

        showLoader();

        Teamlab.testPortalSmtpSettings(null, {
            success: function (e, operation) {

                if (!operation || !operation.id) {
                    LoadingBanner.showMesInfoBtn("#smtpSettingsView", ASC.Resources.Master.Resource.OperationFailedMsg, "error");
                    hideLoader();
                    return;
                }

                progressBarIntervalId = setInterval(function() {
                        return checkStatus(operation);
                    },
                    checkStatusTimeout);
            },
            error: function (e, err) {
                LoadingBanner.showMesInfoBtn("#smtpSettingsView", err[0], "error");
                hideLoader();
            }
        });

        return false;
    }

    function checkStatus() {
        Teamlab.getTestPortalSmtpSettingsResult(
            null,
            {
                success: function (params, data) {
                    // console.log("Test SMTP settings in progress", data);

                    if (!data || typeof(data.completed) === "undefined" || data.completed) {
                        clearInterval(progressBarIntervalId);
                        progressBarIntervalId = null;
                        hideLoader();

                        if (!data || typeof (data.completed) === "undefined") {
                            data = {
                                completed: true,
                                error: ASC.Resources.Master.Resource.OperationFailedMsg
                            };
                        }
                    }

                    if(data.completed) {
                        if (data.error.length > 0) {
                            LoadingBanner.showMesInfoBtn("#smtpSettingsView", data.error, "error");
                        } else {
                            LoadingBanner.showMesInfoBtn("#smtpSettingsView",
                                ASC.Resources.Master.Resource.OperationSuccededMsg,
                                "success");
                        }
                    }
                },
                error: function(e, err) {
                    LoadingBanner.showMesInfoBtn("#smtpSettingsView", err[0], "error");
                    hideLoader();
                }
            });
    }

    return {
        init: init
    };
}(jq);

jq(function () {
    window.SmtpSettingsView.init();
});