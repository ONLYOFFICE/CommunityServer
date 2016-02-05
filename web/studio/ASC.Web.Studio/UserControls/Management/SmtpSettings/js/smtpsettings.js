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
 * form is not reasonably feasible for technical reasons, you must include the words 'Powered by ONLYOFFICE' 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


window.SmtpSettingsView = function($) {
    var $view;

    var $settingsSwitch;
    var $customSettingsRadio;
    var $mailserverSettingsRadio;

    var $customSettingsBox;
    var $mailserverSettingsBox;

    var emailRegex = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;

    var passwordGenerator = function() {
        var lowercase = 'abcdefghijklmnopqrstuvwxyz';
        var uppercase = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
        var numbers = '0123456789';
        var specials = '_';

        var all = lowercase + uppercase + numbers + specials;

        function generate() {
            var password = '';

            password += pick(lowercase, 1);
            password += pick(uppercase, 1);
            password += pick(numbers, 1);
            password += pick(specials, 1);
            password += pick(all, 3, 8);

            password = shuffle(password);
            
            password += pick(uppercase, 1);

            return password;
        }

        function pick(str, min, max) {
            var n;
            var chars = '';

            if (typeof max === 'undefined') {
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
            var chars = str.split('');
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

            return chars.join('');
        }

        return {
            generate: generate
        };
    }();

    var currentSettings;

    var server;
    var domains = [];

    function currentHostUseMailserver() {
        return server && server.dns.mxRecord.host == currentSettings.Host;
    }

    function saveCurrentSettings(settings) {
        settings.CredentialsUserPassword = '';
        currentSettings = settings;
    }

    function init() {
        window.LoadingBanner.displayLoading();

        initElements();
        bindEvents();

        currentSettings = getInitCurrentSettings();

        window.async.parallel([
                function(cb) {
                    window.Teamlab.getMailServer(null, {
                        success: function(params, res) {
                            if (res && res.id) {
                                server = res;
                                cb(null);
                            } else {
                                cb('mailserver does not exist');
                            }
                        },
                        error: function () {
                            cb(null);
                        },
                    });
                },
                function(cb) {
                    window.Teamlab.getMailDomains(null, {
                        success: function(params, res) {
                            domains = res;
                            cb(null);
                        },
                        error: function() {
                            // ignore
                            cb(null);
                        }
                    });
                }
            ], function(err) {
                if (err) {
                    toastr.error(err);
                } else {
                    renderView();
                }

                window.LoadingBanner.hideLoading();
            });
    }

    function initElements() {
        $view = $('#smtpSettingsView');

        $settingsSwitch = $view.find('#settingsSwitch');

        $customSettingsRadio = $settingsSwitch.find('#customSettingsRadio');
        $mailserverSettingsRadio = $settingsSwitch.find('#mailserverSettingsRadio');

        $customSettingsBox = $view.find('#customSettingsBox');
        $mailserverSettingsBox = $view.find('#mailserverSettingsBox');
    }

    function bindEvents() {
        $customSettingsRadio.on('change', switchToCustomSettingsBox);
        $mailserverSettingsRadio.on('change', switchToMailserverSettingsBox);

        $customSettingsBox.on('change', '#customSettingsAuthenticationRequired', changeSettingsAuthenticationRequired);

        $customSettingsBox.on('click', '#saveCustomSettingsBtn', saveCustomSettings);
        $customSettingsBox.on('click', '#saveDefaultCustomSettingsBtn', saveDefaultCustomSettings);
        $customSettingsBox.on('click', '#sendTestMailBtn', sendTestMail);

        $mailserverSettingsBox.on('click', '#saveMailserverSettingsBtn', saveMailserverSettings);
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

/*        if (server) {
            $settingsSwitch.show();
        }*/
        $view.show();
    }

    function renderCustomSettingsBox(settings) {
        var html = $customSettingsBox.siblings('#customSettingsBoxTmpl').tmpl(settings);
        $customSettingsBox.html(html);
    }

    function renderMailserverSettingsBox(settings) {
        var customDomains = [];
        for (var i = 0; i < domains.length; i++) {
            if (!domains[i].isSharedDomain) {
                customDomains.push(domains[i]);
            }
        }

        var html = $mailserverSettingsBox.siblings('#mailserverSettingsBoxTmpl').tmpl(
            {
                domains: customDomains,
                login: settings.CredentialsUserName || '',
                password: settings.CredentialsUserPassword || '',
                senderDisplayName: settings.SenderDisplayName || ''
            });
        $mailserverSettingsBox.html(html);
    }

    function switchToCustomSettingsBox() {
        $customSettingsRadio.attr('checked', true);
        $mailserverSettingsBox.hide();
        $customSettingsBox.show();
    }

    function switchToMailserverSettingsBox() {
        $mailserverSettingsRadio.attr('checked', true);
        $customSettingsBox.hide();
        $mailserverSettingsBox.show();
    }

    function changeSettingsAuthenticationRequired() {
        var $el = $customSettingsBox.find('#customSettingsAuthenticationRequired');
        if ($el.is(':checked')) {
            $customSettingsBox.find('.host-login .textEdit').attr('disabled', false);
            $customSettingsBox.find('.host-password .textEdit').attr('disabled', false);
        } else {
            $customSettingsBox.find('.host-login .textEdit').val('').attr('disabled', true);
            $customSettingsBox.find('.host-password .textEdit').val('').attr('disabled', true);
        }
    }

    function saveCustomSettings() {
        clearCustomSettingsErrors();

        var settings = getCustomSettingsForSave();
        if (!settings) {
            return false;
        }

        showOperationLoader();
        if (currentHostUseMailserver()) {
            window.async.waterfall([
                    function(cb) {
                        Teamlab.removeNotificationAddress(null, currentSettings.CredentialsUserName, {
                            success: function() {
                                cb(null);
                            },
                            error: function() {
                                cb(ASC.Resources.Master.Resource.OperationFailedMsg);
                            }
                        });
                    },
                    function(cb) {
                        window.SmtpSettings.Save(settings, function(result) {
                            if (result.error != null) {
                                cb(result.error.Message);
                            } else {
                                saveCurrentSettings(result.value);
                                renderCustomSettingsBox(currentSettings);
                                renderMailserverSettingsBox(getEmptyCustomSettings());
                                cb(null);
                            }
                        });
                    }
                ], function(err) {
                    if (err) {
                        toastr.error(err);
                    } else {
                        toastr.success(ASC.Resources.Master.Resource.OperationSuccededMsg);
                    }

                    hideOperationLoader();
                });
        } else {
            window.SmtpSettings.Save(settings, function(result) {
                if (result.error != null) {
                    toastr.error(result.error.Message);
                } else {
                    saveCurrentSettings(result.value);
                    renderCustomSettingsBox(currentSettings);
                    renderMailserverSettingsBox(getEmptyCustomSettings());
                    toastr.success(ASC.Resources.Master.Resource.OperationSuccededMsg);
                }

                hideOperationLoader();
            });
        }

        return false;
    }

    function getCustomSettingsForSave() {
        var settingsCorrected = true;

        var host = $customSettingsBox.find('.host .textEdit').val().trim();
        if (!host) {
            $customSettingsBox.find('.host .textEdit').addClass('with-error');
            settingsCorrected = false;
        }

        var port = $customSettingsBox.find('.port .textEdit').val().trim();
        if (port && parseInt(port) + '' !== port) {
            $customSettingsBox.find('.port .textEdit').addClass('with-error');
            settingsCorrected = false;
        }
        if (port == '') {
            port = null;
        }

        var authenticationRequired = $customSettingsBox.find('#customSettingsAuthenticationRequired').is(':checked');

        var credentialsUserName = $customSettingsBox.find('.host-login .textEdit').val().trim();
        if (authenticationRequired && !emailRegex.test(credentialsUserName)) {
            $customSettingsBox.find('.host-login .textEdit').addClass('with-error');
            settingsCorrected = false;
        }

        var credentialsUserPassword = $customSettingsBox.find('.host-password .textEdit').val().trim();
        if (authenticationRequired && !credentialsUserPassword) {
            $customSettingsBox.find('.host-password .textEdit').addClass('with-error');
            settingsCorrected = false;
        }

        var senderDisplayName = $customSettingsBox.find('.display-name .textEdit').val().trim();

        var senderAddress = $customSettingsBox.find('.email-address .textEdit').val().trim();
        if (!emailRegex.test(senderAddress)) {
            $customSettingsBox.find('.email-address .textEdit').addClass('with-error');
            settingsCorrected = false;
        }

        var enableSSL = $customSettingsBox.find('#customSettingsEnableSsl').is(':checked');

        return settingsCorrected ? {
            Host: host,
            Port: port,
            CredentialsUserName: credentialsUserName,
            CredentialsUserPassword: credentialsUserPassword,
            SenderDisplayName: senderDisplayName,
            SenderAddress: senderAddress,
            EnableSSL: enableSSL
        } : null;
    }

    function clearCustomSettingsErrors() {
        $customSettingsBox.find('.with-error').removeClass('with-error');
    }

    function getEmptyCustomSettings() {
        return {
            Host: '',
            Port: '',
            CredentialsUserName: '',
            CredentialsUserPassword: '',
            SenderDisplayName: '',
            SenderAddress: '',
            EnableSSL: false
        };
    }

    function saveDefaultCustomSettings() {
        showOperationLoader();
        if (currentHostUseMailserver()) {
            window.async.waterfall([
                    function(cb) {
                        Teamlab.removeNotificationAddress(null, currentSettings.CredentialsUserName, {
                            success: function() {
                                cb(null);
                            },
                            error: function() {
                                cb(ASC.Resources.Master.Resource.OperationFailedMsg);
                            }
                        });
                    },
                    function(cb) {
                        window.SmtpSettings.RestoreDefaults(function(result) {
                            if (result.error) {
                                cb(result.error.Message);
                            } else {
                                saveCurrentSettings(result.value);
                                renderCustomSettingsBox(currentSettings);
                                renderMailserverSettingsBox(currentSettings);
                                cb(null);
                            }
                        });
                    }
                ], function(err) {
                    if (err) {
                        toastr.error(err);
                    } else {
                        toastr.success(ASC.Resources.Master.Resource.OperationSuccededMsg);
                    }

                    hideOperationLoader();
                });
        } else {
            window.SmtpSettings.RestoreDefaults(function(result) {
                if (result.error) {
                    toastr.error(result.error.Message);
                } else {
                    saveCurrentSettings(result.value);
                    renderCustomSettingsBox(currentSettings);
                    toastr.success(ASC.Resources.Master.Resource.OperationSuccededMsg);
                }
                
                hideOperationLoader();
            });
        }


        return false;
    }

    function sendTestMail() {
        clearCustomSettingsErrors();

        var settings = getCustomSettingsForSave();
        if (!settings) {
            return false;
        }

        showOperationLoader();
        window.SmtpSettings.Test(settings, function(result) {
            if (result.error != null) {
                toastr.error(result.error.Message);
            } else {
                toastr.success(ASC.Resources.Master.Resource.OperationSuccededMsg);
            }

            hideOperationLoader();
        });
        return false;
    }

    function saveMailserverSettings() {
        clearMaiserverSettingsErrors();

        var mailserverSettings = getMailserverSettingsForSave();
        if (!mailserverSettings) {
            return false;
        }

        showOperationLoader();
        if (currentHostUseMailserver()) {
            window.async.waterfall([
                    function(cb) {
                        Teamlab.removeNotificationAddress(null, currentSettings.CredentialsUserName, {
                            success: function() {
                                cb(null);
                            },
                            error: function() {
                                cb(null);
                            }
                        });
                    },
                    function(cb) {
                        Teamlab.createNotificationAddress(null, mailserverSettings.login, mailserverSettings.password, mailserverSettings.domain, {
                            success: function(params, res) {
                                cb(null, res);
                            },
                            error: function(params, err) {
                                cb(err[0]);
                            }
                        });
                    },
                    function(res, cb) {
                        var settings = {
                            Host: res.smtp_server,
                            Port: res.smtp_port,
                            CredentialsUserName: res.smtp_account,
                            CredentialsUserPassword: mailserverSettings.password,
                            SenderDisplayName: mailserverSettings.senderDisplayName,
                            SenderAddress: res.email,
                            EnableSSL: res.smtp_encryption_type == 'STARTTLS'
                        };

                        window.SmtpSettings.Save(settings, function(result) {
                            if (result.error != null) {
                                toastr.error(result.error);
                            } else {
                                saveCurrentSettings(result.value);
                                renderCustomSettingsBox(getEmptyCustomSettings());
                                cb(null);
                            }
                        });
                    }
                ], function(err) {
                    if (err) {
                        toastr.error(err);
                    } else {
                        toastr.success(ASC.Resources.Master.Resource.OperationSuccededMsg);
                    }

                    hideOperationLoader();
                });
        } else {
            window.async.waterfall([
                    function(cb) {
                        Teamlab.createNotificationAddress(null, mailserverSettings.login, mailserverSettings.password, mailserverSettings.domain, {
                            success: function(params, res) {
                                cb(null, res);
                            },
                            error: function(params, err) {
                                cb(err[0]);
                            }
                        });
                    }, function(res, cb) {
                        var settings = {
                            Host: res.smtp_server,
                            Port: res.smtp_port,
                            CredentialsUserName: res.smtp_account,
                            CredentialsUserPassword: mailserverSettings.password,
                            SenderDisplayName: 'TL Postman',
                            SenderAddress: res.email,
                            EnableSSL: res.smtp_encryption_type == 'STARTTLS'
                        };

                        window.SmtpSettings.Save(settings, function(result) {
                            if (result.error != null) {
                                toastr.error(result.error);
                            } else {
                                saveCurrentSettings(result.value);
                                renderCustomSettingsBox(getEmptyCustomSettings());
                                cb(null);
                            }
                        });
                    }
                ], function(err) {
                    if (err) {
                        toastr.error(err);
                    } else {
                        toastr.success(ASC.Resources.Master.Resource.OperationSuccededMsg);
                    }

                    hideOperationLoader();
                });
        }

        return false;
    }

    function getMailserverSettingsForSave() {
        var settingsCorrected = true;

        var login = $mailserverSettingsBox.find('#notificationLogin').val().trim();
        if (!login || login.indexOf('@') > -1) {
            $mailserverSettingsBox.find('#notificationLogin').addClass('with-error');
            settingsCorrected = false;
        }

        var domain = $mailserverSettingsBox.find('#notificationDomain').val().trim();

        var password = generatePassword();

        var senderDisplayName = $mailserverSettingsBox.find('#notificationSenderDisplayName').val().trim();
        if (!senderDisplayName) {
            senderDisplayName = 'Teamlab Postman';
        }

        return settingsCorrected ? {
            login: login,
            domain: domain,
            password: password,
            senderDisplayName: senderDisplayName
        } : null;
    }

    function generatePassword() {
        return passwordGenerator.generate();
    }

    function clearMaiserverSettingsErrors() {
        $mailserverSettingsBox.find('.with-error').removeClass('with-error');
    }

    function getInitCurrentSettings() {
        var $box = $view.find('#currentSettingsBox');

        return {
            Host: $box.find('#currentHost').val(),
            Port: $box.find('#currentPort').val(),
            CredentialsUserName: $box.find('#currentCredentialsUserName').val(),
            CredentialsUserPassword: $box.find('#currentCredentialsUserPassword').val(),
            SenderDisplayName: $box.find('#currentSenderDisplayName').val(),
            SenderAddress: $box.find('#currentSenderAddress').val(),
            EnableSSL: $('#currentEnableSsl').val().toLowerCase() == 'true'
        };
    }

    function showOperationLoader() {
        window.LoadingBanner.displayLoading();
    }

    function hideOperationLoader() {
        window.LoadingBanner.hideLoading();
    }

    return {
        init: init
    };
}(jq);

jq(function() {
    window.SmtpSettingsView.init();
});