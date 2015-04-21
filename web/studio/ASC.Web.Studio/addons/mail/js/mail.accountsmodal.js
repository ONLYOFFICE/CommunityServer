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


window.accountsModal = (function($) {
    var isInit = false,
        accountEmail = '',
        wndQuestion = undefined,
        storedSettings = undefined;

    var ids = {
        'email': 'email',
        'password': 'password',
        'server_type': 'server-type',
        'server': 'server',
        'smtp_server': 'smtp_server',
        'port': 'port',
        'name': 'name',
        'smtp_port': 'smtp_port',
        'account': 'account',
        'smtp_account': 'smtp_account',
        'smtp_password': 'smtp_password',
        'outcoming_encryption_type': 'outcoming_encryption_type',
        'incoming_encryption_type': 'incoming_encryption_type',
        'auth_type_in_sel': 'auth_type_in_sel',
        'auth_type_smtp_sel': 'auth_type_smtp_sel',
        'mail_limit': 'mail-limit'
    };

    var defaultPorts = {
        smtp: 25,
        smtp_ssl: 465,
        imap: 143,
        imap_ssl: 993,
        pop: 110,
        pop_ssl: 995
    };

    var encriptionTypes = {
        plain: 0,
        ssl: 1,
        starttls: 2
    };

    var authTypes = {
        none: 0,
        simple: 1,
        encrypted: 4
    };

    var init = function() {
        if (isInit === false) {
            isInit = true;

            serviceManager.bind(window.Teamlab.events.getMailMailbox, onGetBox);
            serviceManager.bind(window.Teamlab.events.getMailDefaultMailboxSettings, onGetDefaultMailboxSettings);
            serviceManager.bind(window.Teamlab.events.createMailMailboxSimple, onCreateAccount);
            serviceManager.bind(window.Teamlab.events.createMailMailboxOAuth, onCreateAccount);
            serviceManager.bind(window.Teamlab.events.createMailMailbox, onCreateAccount);

            wndQuestion = $('#questionWnd');
            wndQuestion.find('.buttons .cancel').bind('click', function() {
                hide();
                return false;
            });

            wndQuestion.find('.buttons .remove').bind('click', function() {
                hide();

                if (!accountEmail)
                    return false;

                var account = accountsManager.getAccountByAddress(accountEmail);

                if (account.is_teamlab && (account.is_shared_domain || Teamlab.profile.isAdmin)) {
                    removeMailbox(account);
                } else {
                    serviceManager.removeBox(account.email, {}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
                }

                serviceManager.updateFolders();
                serviceManager.getTags();
                return false;
            });

            wndQuestion.find('.buttons .activate').bind('click', function() {
                hide();

                if (!accountEmail)
                    return false;

                activateAccountWithoutQuestion(accountEmail);

                return false;
            });

            wndQuestion.find('.buttons .deactivate').bind('click', function() {
                hide();

                if (!accountEmail)
                    return false;

                serviceManager.setMailboxState(accountEmail, false, { email: accountEmail, enabled: false }, {
                    error: function (e, error) {
                        administrationError.showErrorToastr("setMailboxState", error);
                    }
                }, ASC.Resources.Master.Resource.LoadingProcessing);
                return false;
            });

            $('#manageWindow .cancelButton').css('cursor', 'pointer');
            $('#manageWindow .cancelButton').removeAttr('onclick');
            $('#manageWindow .cancelButton').click(function() {
                hide(true);
            });
        }
    };

    function activateAccountWithoutQuestion(email) {
        var account = accountsManager.getAccountByAddress(email);

        if (!account)
            return false;

        var mailboxEmail = account.is_alias ? accountsManager.getAccountById(account.mailbox_id).email : accountEmail;

        serviceManager.setMailboxState(mailboxEmail, true, { email: accountEmail, enabled: true }, {
            error: function (e, errors) {
                if (errors && errors.length && errors.length > 1) {
                    if (errors[1].hresult == ASC.Mail.ErrorConstants.COR_E_AUTHENTICATION) {
                        if (account && account.mailbox_id) {
                            mailAlerts.showAlert({ type: 6, id_mailbox: account.mailbox_id, data: null, redirectToAccounts: false, activateOnSuccess: true });
                            return;
                        }
                    }
                }

                administrationError.showErrorToastr("setMailboxState", errors);
            }
        }, ASC.Resources.Master.Resource.LoadingProcessing);

        return true;
    }

    function addBox() {
        showWizard();
    }

    function addMailbox() {
        var $rootEl;
        var currentDomain;

        if (window.MailCommonDomain) {
            showMy(window.MailCommonDomain);
        } else {
            serviceManager.getCommonMailDomain({}, {
                success: function (e, domain) {
                    window.MailCommonDomain = domain;
                    showMy(domain);
                },
                error: function (e, error) {
                    administrationError.showErrorToastr("getCommonMailDomain", error);
                }
            }, ASC.Resources.Master.Resource.LoadingProcessing);
        }

        function showMy(domain) {

            currentDomain = domain;

            var html = $.tmpl('createMyMailboxPopupTmpl', { domain: domain });

            $(html).find('.save').unbind('click').bind('click', addMyMailbox);

            $(html).find('.cancel').unbind('click').bind('click', function () {
                if ($(this).hasClass('disable')) {
                    return false;
                }
                popup.hide();
                return false;
            });

            popup.hide();
            
            popup.addPopup(window.MailScriptResource.CreateMyMailboxPopupHeader, html, 450);

            $rootEl = $('#mail_server_create_my_mailbox_popup');

            $rootEl.find('#mail_server_add_my_mailbox .mailbox_name').unbind('textchange').bind('textchange', function () {
                turnOffAllRequiredError();
            });

            PopupKeyUpActionProvider.EnterAction = "jq('#mail_server_create_my_mailbox_popup:visible .save').trigger('click');";

            setFocusToInput();
        }
        
        function addMyMailbox() {
            if ($(this).hasClass('disable')) {
                return false;
            }

            window.LoadingBanner.hideLoading();

            var isValid = true;

            var mailboxName = $rootEl.find('#mail_server_add_my_mailbox .mailbox_name').val();
            if (mailboxName.length === 0) {
                TMMail.setRequiredHint('mail_server_add_my_mailbox', window.MailScriptResource.ErrorEmptyField);
                TMMail.setRequiredError('mail_server_add_my_mailbox', true);
                isValid = false;
            } else if (!TMMail.reMailServerEmailStrict.test(mailboxName + '@' + currentDomain.name)) {
                TMMail.setRequiredHint('mail_server_add_my_mailbox', window.MailScriptResource.ErrorIncorrectEmail);
                TMMail.setRequiredError('mail_server_add_my_mailbox', true);
                isValid = false;
            }

            if (!isValid) {
                setFocusToInput();
                return false;
            }

            turnOffAllRequiredError();
            displayLoading(true);
            disableButtons(true);
            
            window.ASC.Mail.ga_track(ga_Categories.accauntsSettings, ga_Actions.createNew, "create_my_mailbox");

            showLoader(window.MailScriptResource.MailboxCreation);
            
            serviceManager.addMyMailbox(mailboxName,
                { email: mailboxName + "@" + currentDomain.name, name: Teamlab.profile.displayName, enabled: true, restrict: false, oauth: false },
                {
                    success: function (params, mailbox) {
                        displayLoading(false);
                        disableButtons(false);

                        if (TMMail.pageIs('sysfolders') && accountsManager.getAccountList().length == 0) {
                            blankPages.showEmptyFolder();
                        }

                        accountsPage.addAccount(mailbox.address.email, params.enabled, params.oauth, true, true);

                        var account = {
                            name: TMMail.ltgt(params.name),
                            email: TMMail.ltgt(mailbox.address.email),
                            enabled: params.enabled,
                            signature: {
                                html: "",
                                isActive: false,
                                mailboxId: mailbox.id,
                                tenant: 0
                            },
                            is_alias: false,
                            is_group: false,
                            oauth: params.oauth,
                            emailInFolder: null,
                            is_teamlab: true,
                            mailbox_id: mailbox.id,
                            is_default: false,
                            is_shared_domain: true,
                            authError: false,
                            quotaError: false
                        };

                        accountsManager.addAccount(account);

                        accountsPanel.update();

                        hide();
                        window.toastr.success(window.MailActionCompleteResource.AddMailboxSuccess.format(mailbox.address.email));
                    },
                    error: function (ev, error) {
                        popup.error(administrationError.getErrorText("addMailbox", error));
                        displayLoading(false);
                        disableButtons(false);
                    }
                });

            return false;
        }

        function turnOffAllRequiredError() {
            TMMail.setRequiredError('mail_server_add_mailbox', false);
        }

        function displayLoading(isVisible) {
            var loader = $rootEl.find('.progressContainer .loader');
            var toastr = $rootEl.find('.progressContainer .toast-popup-container');
            if (loader) {
                if (isVisible) {
                    toastr.remove();
                    loader.show();
                } else {
                    loader.hide();
                }
            }
        }
        
        function disableButtons(disable) {
            TMMail.disableButton($rootEl.find('.cancel'), disable);
            TMMail.disableButton($rootEl.find('.save'), disable);
            TMMail.disableButton($('#commonPopup .cancelButton'), disable);
            popup.disableCancel(disable);
            TMMail.disableInput($rootEl.find('.mailbox_name'), disable);
        }

        function setFocusToInput() {
            $rootEl.find('.mailbox_name').focus();
        }
    }

    function removeBox(account) {
        if (!account)
            return;

        accountEmail = account;
        questionBox('remove');
    }

    function removeMailbox(account) {
        var id = account.mailbox_id;
        serviceManager.removeMailbox(id, { account: account }, {
            success: function (params, data) {
                serviceManager.getAccounts();

                mailBox.markFolderAsChanged(TMMail.sysfolders.inbox.id);
                mailBox.markFolderAsChanged(TMMail.sysfolders.sent.id);
                mailBox.markFolderAsChanged(TMMail.sysfolders.drafts.id);
                mailBox.markFolderAsChanged(TMMail.sysfolders.trash.id);
                mailBox.markFolderAsChanged(TMMail.sysfolders.spam.id);
            },
            error: function (params, error) {
                administrationError.showErrorToastr("getCommonMailDomain", error);
            }
        }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function editBox(account, activateOnSuccess) {
        serviceManager.getBox(account, { action: 'edit', activateOnSuccess: activateOnSuccess }, {}, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function setDefaultAccount(account, setDefault) {
        serviceManager.setDefaultAccount(account, setDefault, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function blockUi(width, message) {
        var defaultOptions = {
            css: {
                top: '-100%'
            }
        };

        StudioBlockUIManager.blockUI(message, width, null, null, null, defaultOptions);

        message.closest(".blockUI").css({
            'top': '50%',
            'margin-top': '-{0}px'.format(message.height() / 2)
        });

        $('#manageWindow .cancelButton').css('cursor', 'pointer');
        $('.containerBodyBlock .buttons .cancel').unbind('click').bind('click', function() {
            $.unblockUI();
            return false;
        });
    }

    // Simple wizard window

    function showWizard(email) {

        var html = $.tmpl('accountWizardTmpl');

        $('#manageWindow').removeClass('show-error').attr('className', 'addInbox')
            .find('div.containerBodyBlock:first').html(html);

        $('#manageWindow div.containerHeaderBlock:first').find('td:first').html(window.MailScriptResource.NewAccount);

        if ($(html).find('#oauth_frame_blocker').length) {
            blockUi(540, $("#manageWindow"));
        } else {
            blockUi(400, $("#manageWindow"));
        }

        if (window.addEventListener) {
            window.addEventListener('message', onGetOAuthInfo, false);
        } else {
            window.attachEvent('onmessage', onGetOAuthInfo);
        }

        $('.containerBodyBlock .buttons #save').unbind('click').bind('click', function() {
            if ($('.containerBodyBlock .buttons #save').attr('disabled')) {
                return false;
            }

            createMailBoxSimple();
            return false;
        });

        $('.containerBodyBlock .buttons #advancedLinkButton').unbind('click').bind('click', function() {
            if ($('.containerBodyBlock .buttons #advancedLinkButton').attr('disabled')) {
                return false;
            }

            switchToAdvanced("get_imap_pop_settings");
            return false;
        });

        $('.containerBodyBlock #password').keyup(setupPasswordView);
        $('.containerBodyBlock a.password-view').unbind('click').bind('click', togglePassword);

        window.PopupKeyUpActionProvider.EnterAction = "jq('.containerBodyBlock .buttons #save').click();";

        if (email) {
            setVal(ids.email, email);
        }
    }

    function setupPasswordView() {
        var containerId = $(this).closest('.requiredField').attr('id');
        var isSmtp = containerId == 'mail_SMTPPasswordContainer';
        var password = getVal(isSmtp ? ids.smtp_password : ids.password, true);
        var passwordViewLink = $('.containerBodyBlock #' + containerId + ' .headerPanelSmall a.password-view');
        if (password.length > 0) {
            TMMail.setRequiredError(containerId, false);
        }
        passwordViewLink.toggleClass('off', password.length == 0);
    }

    function togglePassword() {
        var passwordInput = $(this).closest('.requiredField').find('input');
        if (passwordInput.attr('type') == "password") {
            passwordInput.attr('type', 'text');
            $(this).text(window.MailScriptResource.HidePasswordLinkLabel);
        } else {
            passwordInput.attr('type', 'password');
            $(this).text(window.MailScriptResource.ShowPasswordLinkLabel);
        }
    }

    function onGetOAuthInfo(evt) {
        var obj;
        if (typeof evt.data == "string") {
            try {
                obj = window.jQuery.parseJSON(evt.data);
            } catch(err) {
                return;
            }
        } else {
            obj = evt.data;
        }

        if (obj.Tpr == "OAuthImporter" && obj.Data.length != 0) {
            if (obj.error && obj.error != "Canceled at provider") {
                onErrorCreateMailboxSimple({ email: obj.Data.Email || '' }, [obj.error]);
            } else if (obj.Data.Email != null && obj.Data.RefreshToken != null) {
                showLoader(window.MailScriptResource.MailboxCreation);
                serviceManager.createOAuthBox(obj.Data.Email, obj.Data.RefreshToken, 1, //google service
                    { email: obj.Data.Email, name: '', enabled: true, restrict: true, oauth: true },
                    { error: onErrorCreateMailboxSimple });
            }
        } else {
            hide();
        }
    }

    function switchToAdvanced(action) {
        storedSettings = undefined;
        var email = getVal(ids.email);
        var password = getVal(ids.password, false);

        var options = { action: action };

        // If the email entered is OK
        if (TMMail.reEmailStrict.test(email)) {
            options.password = password;
            serviceManager.getDefaultMailboxSettings(email, options, { error: onErrorGetDefaultMailboxSettings }, window.MailScriptResource.ObtainingDefaultDomainSettings);
        }
            // If nothing is entered as email or the email is wrong
        else {
            var data = {
                email: email,
                password: password,
                smtp_auth: true,
                imap: true,
                incoming_encryption_type: encriptionTypes.ssl,
                port: defaultPorts.imap_ssl,
                auth_type_in: authTypes.simple,
                restrict: true,
                outcoming_encryption_type: encriptionTypes.ssl,
                smtp_port: defaultPorts.smtp_ssl,
                auth_type_smtp: authTypes.simple
            };
            setBoxData(data, options);
        }
    }

    function showLoader(message) {
        $('.popupMailBox').find('div.error-popup').hide();
        $('.popupMailBox').find('div.progressContainer').show();
        $('.popupMailBox').find('div.progressContainer .loader').show().html(message || '');

        $('.containerBodyBlock .buttons #save').attr('disabled', 'true').removeClass("disable").addClass("disable");
        $('.containerBodyBlock .buttons #cancel').attr('disabled', 'true').removeClass("disable").addClass("disable");
        $('.popupMailBox #oauth_frame_blocker').show();
        $('.containerBodyBlock .buttons #getDefaultSettings').attr('disabled', 'true').removeClass("disable").addClass("disable");
        $('.containerBodyBlock .buttons #advancedLinkButton').attr('disabled', 'true').removeClass("disable").addClass("disable");

        $('#manageWindow .containerHeaderBlock').find('.popupCancel .cancelButton').css('cursor', 'default');

        disable(ids.name);
        disable(ids.mail_limit);
        disableControls();
    }

    function hideLoader(forceHideContainer) {
        if (forceHideContainer) {
            $('.popupMailBox').find('div.progressContainer').hide();
        }

        $('.popupMailBox').find('div.progressContainer .loader').hide();

        $('.containerBodyBlock .buttons #save').removeAttr('disabled').removeClass("disable");
        $('.containerBodyBlock .buttons #cancel').removeAttr('disabled').removeClass("disable");
        $('.popupMailBox #oauth_frame_blocker').hide();
        $('.containerBodyBlock .buttons #getDefaultSettings').removeAttr('disabled').removeClass("disable");
        $('.containerBodyBlock .buttons #advancedLinkButton').removeAttr('disabled').removeClass("disable");

        $('#manageWindow .containerHeaderBlock').find('.popupCancel .cancelButton').css('cursor', 'pointer');

        enableControls();
        if (getVal(ids.auth_type_smtp_sel, true) == 0) {
            enable(ids.smtp_account);
            enable(ids.smtp_password);
        }
    }

    function enableControls() {
        enable(ids.email);
        enable(ids.password);
        enable(ids.name);
        enable(ids.server_type);
        enable(ids.server);
        enable(ids.smtp_server);
        enable(ids.port);
        enable(ids.smtp_port);
        enable(ids.account);
        enable(ids.mail_limit);
        enable(ids.outcoming_encryption_type);
        enable(ids.incoming_encryption_type);
        enable(ids.auth_type_in_sel);
        enable(ids.auth_type_smtp_sel);

    }

    function disableControls() {
        disable(ids.email);
        disable(ids.password);
        disable(ids.server_type);
        disable(ids.server);
        disable(ids.smtp_server);
        disable(ids.port);
        disable(ids.smtp_port);
        disable(ids.account);
        disable(ids.smtp_account);
        disable(ids.smtp_password);
        disable(ids.outcoming_encryption_type);
        disable(ids.incoming_encryption_type);
        disable(ids.auth_type_in_sel);
        disable(ids.auth_type_smtp_sel);
    }

    function onGetBox(params, account) {
        if (params.action == "get_imap_server" ||
            params.action == "get_pop_server") {
            setVal(ids.account, account.account);
            setVal(ids.server, account.server);
            setVal(ids.port, account.port);
            $('.popupMailBox #ssl').prop('checked', account.ssl);
            return;
        }

        if ((params.action == 'get_imap_server_full' || params.action == 'get_pop_server_full') && $('#manageWindow').attr('classname') == 'editInbox') {
            params.action = 'edit';
        }

        var html = $.tmpl('accountTmpl', account);
        $('#manageWindow').removeClass('show-error').attr('className', params.action === 'edit' ? 'editInbox' : 'addInbox')
            .find('div.containerBodyBlock:first').html(html);
        $('#manageWindow div.containerHeaderBlock:first').find('td:first').html(params.action === 'edit' ? window.MailScriptResource.AccountEdit : window.MailScriptResource.NewAccount);

        blockUi(523, $("#manageWindow"));

        if ($('#manageWindow').attr('className') != 'addInbox') {
            $('#smtp_password').attr('placeholder', '**********');
            $('#password').attr('placeholder', '**********');
            $('#smtp_password').placeholder();
            $('#password').placeholder();
        }

        if ("refresh_token" in account && account.refresh_token != null) {
            disableControls();
            $('.popupMailBox #getDefaultSettings').hide();
        }

        $('.containerBodyBlock .buttons #save').unbind('click').bind('click', function() {
            if ($('.containerBodyBlock .buttons #save').attr('disabled')) {
                return false;
            }

            $('.popupMailBox').find('div.progressContainer').show();
            $('.popupMailBox').find('div.error-popup').hide();
            if ($('#manageWindow').attr('className') == 'addInbox') {
                createMailbox();
            } else {
                updateMailbox(false, params.activateOnSuccess);
            }

            return false;
        });

        $('.popupMailBox .buttons #getDefaultSettings').unbind('click').bind('click', function() {
            if ($('.containerBodyBlock .buttons #getDefaultSettings').attr('disabled')) {
                return false;
            }

            var email = getVal(ids.email);

            var emailCorrect = isEmailCorrect(email);

            TMMail.setRequiredError("mail_EMailContainer", !emailCorrect);

            if (getVal(ids.server_type, true) == 'imap') {
                params.action = "get_imap_server_full";
            } else {
                params.action = "get_pop_server_full";
            }

            if (emailCorrect) {
                switchToAdvanced(params.action);
            }

            return false;
        });

        if (params.action == 'edit') {
            disable(ids.email);
            disable(ids.server_type);
        }

        $('.popupMailBox #auth_type_smtp_sel').unbind('change').bind('change', function() {
            if ($(this).val() != 0) {
                enable(ids.smtp_account);
                enable(ids.smtp_password);
                $("#mail_SMTPLoginContainer").addClass('requiredField');
                $("#mail_SMTPPasswordContainer").addClass('requiredField');
            } else {
                disable(ids.smtp_account);
                disable(ids.smtp_password);
                TMMail.setRequiredError("mail_SMTPLoginContainer", false);
                TMMail.setRequiredError("mail_SMTPPasswordContainer", false);

                $("#mail_SMTPLoginContainer").removeClass('requiredField');
                $("#mail_SMTPPasswordContainer").removeClass('requiredField');
            }
        });

        $('.popupMailBox #server').unbind('blur').bind('blur', function() {
            var addr = getVal(ids.server, true);
            if (addr.length > 0 && getVal(ids.smtp_server, true).length == 0) {
                setVal(ids.smtp_server, 'smtp' + addr.substring(addr.indexOf('.')));
            }
        });

        $('.popupMailBox #server-type').unbind('change').bind('change', function() {
            var email = getVal(ids.email);

            var emailCorrect = TMMail.reEmailStrict.test(email);

            var action;
            if ($(this).val() == 'pop') {
                $('.popupMailBox #receive-server-header').html('POP ' + window.MailScriptResource.ServerLabel);
                $('.popupMailBox #use_incomming_ssl_label').html(window.MailResource.UsePOP3SSL);
                action = "get_pop_server";
            } else {
                $('.popupMailBox #receive-server-header').html('IMAP ' + window.MailScriptResource.ServerLabel);
                $('.popupMailBox #use_incomming_ssl_label').html(window.MailResource.UseImapSSL);
                action = "get_imap_server";
            }

            if (emailCorrect) {
                serviceManager.getDefaultMailboxSettings(email, { action: action, password: getVal(ids.password, true), smtp_password: getVal(ids.smtp_password, true) }, { error: onErrorGetDefaultMailboxSettings }, window.MailScriptResource.ObtainingDefaultDomainSettings);
            }
        });

        $('.containerBodyBlock #password').keyup(setupPasswordView);
        $('.containerBodyBlock #smtp_password').keyup(setupPasswordView);
        $('.containerBodyBlock a.password-view').unbind('click').bind('click', togglePassword);

        if (params.error) {
            setErrorToPopupMailbox(params.error);
        }
    }

    function setErrorToPopupMailbox(errorText) {
        hideLoader(true);
        $('.popupMailBox').find('div.error-popup').show().find('.text').text(errorText);
    }

    function createMailbox() {
        return updateMailbox(true, false);
    }

    function createMailBoxSimple() {
        var email = getVal(ids.email),
            password = getVal(ids.password, true);

        var emailCorrect = false;
        var passwordCorrect = false;

        if (email.length === 0) {
            TMMail.setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorEmptyField);
        } else if (!TMMail.reEmailStrict.test(email)) {
            TMMail.setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorIncorrectEmail);
        } else if (accountsPage.isContain(email.toLowerCase())) {
            TMMail.setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorAccountAlreadyExists);
        } else {
            emailCorrect = true;
        }

        TMMail.setRequiredError("mail_EMailContainer", !emailCorrect);

        if (password.length !== 0) {
            passwordCorrect = true;
        }

        TMMail.setRequiredError("mail_PasswordContainer", !passwordCorrect);

        if (emailCorrect && passwordCorrect) {

            window.ASC.Mail.ga_track(ga_Categories.accauntsSettings, ga_Actions.createNew, "create_min_account");

            showLoader(window.MailScriptResource.MailboxCreation);
            serviceManager.createMinBox(email, password,
                { email: email, name: '', enabled: true, restrict: true, oauth: false },
                { error: onErrorCreateMailboxSimple });
        }
    }

    function updateMailbox(newFlag, activateOnSuccess) {
        var email = getVal(ids.email),
            server = getVal(ids.server),
            account = getVal(ids.account),
            name = getVal(ids.name),
            password = getVal(ids.password, true),
            port = getVal(ids.port),
            smtpAuth = $('.popupMailBox #auth_type_smtp_sel').val() != 0,
            smtpServer = getVal(ids.smtp_server),
            smtpAccount = getVal(ids.smtp_account),
            smtpPassword = getVal(ids.smtp_password),
            smtpPort = getVal(ids.smtp_port),
            imap = (getVal(ids.server_type, true) == 'imap'),
            mailLimit = $('.popupMailBox #mail-limit').is(':checked'),
            incomingEncryptionType = getVal(ids.incoming_encryption_type + ' option:selected', true),
            outcomingEncryptionType = getVal(ids.outcoming_encryption_type + ' option:selected', true),
            authTypeIn = getVal(ids.auth_type_in_sel + ' option:selected', true),
            authTypeSmtp = getVal(ids.auth_type_smtp_sel + ' option:selected', true);

        storedSettings = {};
        storedSettings.account = account;
        storedSettings.auth_type_in = authTypeIn;
        storedSettings.auth_type_smtp = authTypeSmtp;
        storedSettings.email = email;
        storedSettings.imap = imap;
        storedSettings.incoming_encryption_type = incomingEncryptionType;
        storedSettings.name = name;
        storedSettings.outcoming_encryption_type = outcomingEncryptionType;
        storedSettings.password = "";
        storedSettings.port = port;
        storedSettings.server = server;
        storedSettings.smtp_account = smtpAccount;
        storedSettings.smtp_auth = smtpAuth;
        storedSettings.smtp_password = "";
        storedSettings.smtp_port = smtpPort;
        storedSettings.smtp_server = smtpServer;

        if (password == $('#password').attr('placeholder')) {
            password = '';
        }
        if (smtpPassword == $('#smtp_password').attr('placeholder')) {
            smtpPassword = '';
        }

        var emailIncorrect = true;
        var serverIncorrect = false;
        var portIncorrect = false;
        var accountIncorrect = false;
        var passwordIncorrect = false;
        var smtpServerIncorrect = false;
        var smtpPortIncorrect = false;
        var smtpAccountIncorrect = false;
        var smtpPasswordIncorrect = false;

        if (email.length === 0) {
            $("#mail_EMailContainer.requiredField span").text(window.MailScriptResource.ErrorEmptyField);
        } else if (!TMMail.reEmailStrict.test(email)) {
            TMMail.setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorIncorrectEmail);
        } else if (newFlag && accountsPage.isContain(email.toLowerCase())) {
            TMMail.setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorAccountAlreadyExists);
        } else {
            emailIncorrect = false;
        }

        TMMail.setRequiredError("mail_EMailContainer", emailIncorrect);
        TMMail.setRequiredError("mail_POPServerContainer", serverIncorrect = server.length === 0);
        TMMail.setRequiredError("mail_POPPortContainer", portIncorrect = port.length === 0);
        TMMail.setRequiredError("mail_POPLoginContainer", accountIncorrect = account.length === 0);
        TMMail.setRequiredError("mail_POPPasswordContainer", passwordIncorrect = (password.length === 0 && newFlag));

        TMMail.setRequiredError("mail_SMTPServerContainer", smtpServerIncorrect = smtpServer.length === 0);
        TMMail.setRequiredError("mail_SMTPPortContainer", smtpPortIncorrect = smtpPort.length === 0);

        if (smtpAuth) {
            TMMail.setRequiredError("mail_SMTPLoginContainer", smtpAccountIncorrect = smtpAccount.length === 0);
            TMMail.setRequiredError("mail_SMTPPasswordContainer", smtpPasswordIncorrect = (smtpPassword.length === 0 && newFlag));
        }

        if (!emailIncorrect &&
            !serverIncorrect &&
            !portIncorrect &&
            !accountIncorrect &&
            !passwordIncorrect &&
            !smtpServerIncorrect &&
            !smtpPortIncorrect &&
            !smtpAccountIncorrect &&
            !smtpPasswordIncorrect) {
            if (true === newFlag) {
                window.ASC.Mail.ga_track(ga_Categories.accauntsSettings, ga_Actions.createNew, "create_advanced_account");
                showLoader(window.MailScriptResource.MailboxCreation);
                serviceManager.createBox(name,
                    email,
                    account,
                    password,
                    port,
                    server,
                    smtpAccount,
                    smtpPassword,
                    smtpPort,
                    smtpServer,
                    smtpAuth,
                    imap,
                    mailLimit,
                    incomingEncryptionType,
                    outcomingEncryptionType,
                    authTypeIn,
                    authTypeSmtp,
                    { email: email, name: name, enabled: true, restrict: mailLimit, oauth: false, action: 'add' },
                    { error: onErrorCreateMailbox });
            } else {
                showLoader(window.MailScriptResource.MailBoxUpdate);
                serviceManager.updateBox(name,
                    email,
                    account,
                    password,
                    port,
                    server,
                    smtpAccount,
                    smtpPassword,
                    smtpPort,
                    smtpServer,
                    smtpAuth,
                    mailLimit,
                    incomingEncryptionType,
                    outcomingEncryptionType,
                    authTypeIn,
                    authTypeSmtp,
                    { email: email, name: name, enabled: true, restrict: mailLimit, action: 'edit', activateOnSuccess: activateOnSuccess },
                    { error: onErrorCreateMailbox });
            }
        }
    }

    function questionBox(operation) {
        var header = '';
        wndQuestion.find('.activate').hide();
        wndQuestion.find('.deactivate').hide();
        wndQuestion.find('.remove').hide();
        var question = '';
        switch (operation) {
            case 'remove':
                header = wndQuestion.attr('delete_header');
                wndQuestion.find('.remove').show();
                question = window.MailScriptResource.DeleteAccountShure;
                break;
            case 'activate':
                header = wndQuestion.attr('activate_header');
                wndQuestion.find('.activate').show();
                question = window.MailScriptResource.ActivateAccountShure;
                break;
            case 'deactivate':
                header = wndQuestion.attr('deactivate_header');
                wndQuestion.find('.deactivate').show();
                question = window.MailScriptResource.DeactivateAccountShure;
                break;
        }

        wndQuestion.find('div.containerHeaderBlock:first td:first').html(header);
        question = question.replace(/%1/g, accountEmail);
        wndQuestion.find('.mail-confirmationAction p.questionText').text(question);

        blockUi(523, wndQuestion);

        window.PopupKeyUpActionProvider.EnterAction = "jq('#questionWnd .containerBodyBlock .buttons .button.blue:visible').click();";
    }

    function informationBox(params) {
        var content = window.MailScriptResource.ImportAccountText;
        if (params.restrict === true) {
            content += '<br><br>' + window.MailScriptResource.ImportAccountRestrictLabel;
        }

        var footer;
        if (/@gmail.com$/.test(params.email.toLowerCase())) {
            footer = window.MailScriptResource.ImportGmailAttention;
        } else {
            footer = window.MailScriptResource.ImportProblemText;
        }

        footer = footer
            .replace('{0}', '<a href="{0}"class="linkDescribe" target="blank">'
                .format(TMMail.getFaqLink(params.email.toLowerCase())))
            .replace('{1}', '</a>');

        var body = $($.tmpl("accountSuccessTmpl", {
            errorBody: content,
            errorBodyFooter: footer
        }));

        body.find('.errorImg').toggleClass('errorImg', false).toggleClass('successImg', true);

        popup.addBig(window.MailScriptResource.DoneLabel, body);
    }

    function warningAlreadyExist() {
        html = $.tmpl('accountExistErrorTmpl', {});
        popup.addBig(window.MailScriptResource.ExistingAccountLabel, html);
    }

    function activateSelectedAccount(activated) {
        var account = $('#newmessageFromSelected').attr('mailbox_email');
        activateAccount(account, activated);
    }

    function activateAccount(account, activated) {
        if (!account)
            return;

        accountEmail = account;
        if (activated) {
            questionBox('activate');
        } else {
            questionBox('deactivate');
        }
    }

    function hide(stopOnSaving) {
        if (stopOnSaving) {
            var loader = $('.popupMailBox div.progressContainer div.loader');
            if (loader != undefined && loader.is(':visible')) {
                return;
            }
        }

        if (window.removeEventListener) {
            window.removeEventListener("message", onGetOAuthInfo, false);
        } else {
            window.detachEvent("message", onGetOAuthInfo);
        }

        window.PopupKeyUpActionProvider.ClearActions();
        $.unblockUI();
    }

    function onError(error) {
        setErrorToPopupMailbox(error.message + (error.comment ? ': ' + error.comment : ''));
    }

    function setBoxData(defaults, params, error) {
        onGetBox(params, defaults, error);
    }

    function onErrorGetDefaultMailboxSettings() {
        setErrorToPopupMailbox(window.MailScriptResource.ErrorNotification);
    }

    function onGetDefaultMailboxSettings(params, defaults) {
        if (params.password != undefined) {
            defaults.password = params.password;
            if (defaults.smtp_auth) {
                defaults.smtp_password = params.smtp_password != undefined ? params.smtp_password : params.password;
            }
        }
        onGetBox(params, defaults);
    }

    function onCreateAccount(params, account) {
        if (!accountsManager.getAccountByAddress(account.email)) {
            if (TMMail.pageIs('sysfolders') && accountsManager.getAccountList().length == 0) {
                blankPages.showEmptyFolder();
            }
            accountsPage.addAccount(params.email, params.enabled, params.oauth);
            var accountData = {
                name: TMMail.ltgt(account.name),
                email: TMMail.ltgt(account.email),
                enabled: account.enabled,
                signature: account.signature,
                is_alias: account.isAlias,
                is_group: account.isGroup,
                oauth: account.oAuthConnection,
                emailInFolder: account.eMailInFolder,
                is_teamlab: account.isTeamlabMailbox,
                mailbox_id: account.mailboxId,
                is_default: account.isDefault,
                is_shared_domain: account.isSharedDomainMailbox,
                authError: account.authError,
                quotaError: account.quotaError
            };

            accountsManager.addAccount(accountData);
            hide();
            informationBox(params);
        } else {
            hide();
            warningAlreadyExist();
        }
    }

    function onErrorCreateMailboxSimple(params, error) {
        showErrorModal({ email: params.email, simple: true, error_text: error });
    }

    function getAccountErrorFooter(address) {
        return window.MailScriptResource.AccountCreationErrorGmailFooter.replace('{0}', '<a target="blank" class="linkDescribe" href="' + TMMail.getFaqLink(address) + '">').replace('{1}', '</a>');
    }

    function showErrorModal(params) {
        // ToDo: Reimplement to popup mechanism instead of #manageWindow
        hideLoader();
        var footer = getAccountErrorFooter(params.email.toLowerCase()),
            html = $.tmpl('accountErrorTmpl', {
                errorBodyFooter: footer,
                errorAdvancedInfo: params.error_text[0]
            });

        $('#manageWindow').removeClass('show-error').find('div.containerBodyBlock:first').html(html);
        $('#manageWindow div.containerHeaderBlock:first').find('td:first').html(window.MailScriptResource.AccountCreationErrorHeader);

        blockUi(523, $("#manageWindow"));

        $('#advancedErrorLinkButton').click(function() {
            $('#mail_advanced_error_container').slideToggle('slow');
        });

        $('#account_error_container .buttons .tryagain').click(function() {
            if (true === params.simple) {
                showWizard(params.email);

            } else {
                if (params.action == "edit") {
                    editBox(params.email, params.activateOnSuccess);
                } else {
                    if (storedSettings != undefined) {
                        onGetBox({ action: "get_pop_server_full", activateOnSuccess: params.activateOnSuccess }, storedSettings);
                    } else {
                        serviceManager.getDefaultMailboxSettings(params.email,
                            { action: "get_pop_server_full", password: params.password, smtp_password: params.smtp_password },
                            {
                                success: function() { setErrorToPopupMailbox(window.MailScriptResource.CorrectAccountRecommendation); },
                                error: function() { setErrorToPopupMailbox(window.MailScriptResource.ErrorNotificationEx); }
                            },
                            window.MailScriptResource.ObtainingDefaultDomainSettings);
                    }
                }
            }
        });

        $('#account_error_container .buttons .tryagain').keypress(function(event) {
            event.preventDefault();
        });

        window.PopupKeyUpActionProvider.EnterAction = "jq('#account_error_container .buttons #tryagain').click()";
    }

    function onErrorCreateMailbox(params, error) {
        params.error_text = error;
        showErrorModal(params);
    }

    function getVal(id, skipTrim) {
        var res = $('.popupMailBox #' + id).val();
        return skipTrim ? res : $.trim(res);
    }

    function setVal(id, v) {
        $('.popupMailBox #' + id).val(v);
    }

    function disable(id) {
        $('.popupMailBox #' + id).attr('disabled', 'true');
    }

    function enable(id) {
        $('.popupMailBox #' + id).removeAttr('disabled');
    }

    function isEmailCorrect(email) {
        var emailCorrect = false;

        if (email.length === 0) {
            TMMail.setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorEmptyField);
        } else if (!TMMail.reEmailStrict.test(email)) {
            TMMail.setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorIncorrectEmail);
        } else {
            emailCorrect = true;
        }

        return emailCorrect;
    }

    function signatureBox(accountAddress) {
        var account = accountsManager.getAccountByAddress(accountAddress);
        if (account) {
            var isActive = (account.signature.html == "" && !account.signature.isActive) ? true : account.signature.isActive; // Default is true;

            html = $.tmpl("manageSignatureTmpl", { 'isActive': isActive });

            var config = {
                toolbar: 'MailSignature',
                removePlugins: 'resize, magicline',
                filebrowserUploadUrl: 'fckuploader.ashx?newEditor=true&esid=mail',
                height: 200,
                startupFocus: true,
                on: {
                    instanceReady: function(instance) {
                        instance.editor.setData(account.signature.html);
                    }
                }
            };

            html.find('#ckMailSignatureEditor').ckeditor(config);

            html.find('.buttons .ok').unbind('click').bind('click', function() {
                updateSignature(account);
                return false;
            });

            popup.addBig(window.MailScriptResource.ManageSignatureLabel, html, undefined, false, { bindEvents: false });
        }
    }

    function updateSignature(account) {
        var html = $('#ckMailSignatureEditor').val();
        var isActive = $('#useSignatureFlag').is(':checked') ? true : false;

        if (html == "" && isActive) {
            isActive = false;
        }

        serviceManager.updateMailboxSignature(account.mailbox_id, html, isActive, { id: account.mailbox_id, html: html, is_active: isActive },
            { error: onErrorUpdateMailboxSignature }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function onErrorUpdateMailboxSignature() {
        hideLoader();
    }

    return {
        init: init,
        addBox: addBox,
        addMailbox: addMailbox,
        removeBox: removeBox,
        activateSelectedAccount: activateSelectedAccount,
        activateAccount: activateAccount,
        activateAccountWithoutQuestion: activateAccountWithoutQuestion,
        editBox: editBox,
        setDefaultAccount: setDefaultAccount,
        hide: hide,
        onError: onError,
        setBoxData: setBoxData,
        showInformationBox: informationBox,
        showSignatureBox: signatureBox
    };
})(jQuery);