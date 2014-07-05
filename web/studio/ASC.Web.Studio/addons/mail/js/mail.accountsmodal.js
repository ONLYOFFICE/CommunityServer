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
window.accountsModal = (function($) {
    var 
        is_init = false,
        account_id = '',
        wnd_question = undefined,
        stored_settings = undefined,
        required_field_error_css = "requiredFieldError";

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

    var default_ports = {
        smtp: 25,
        smtp_ssl: 465,
        imap: 143,
        imap_ssl: 993,
        pop: 110,
        pop_ssl: 995
    };

    var encription_types = {
        plain: 0,
        ssl: 1,
        starttls: 2
    };

    var auth_types = {
        none: 0,
        simple: 1,
        encrypted: 4
    };

    var init = function() {
        if (is_init === false) {
            is_init = true;

            serviceManager.bind(window.Teamlab.events.getMailMailbox, onGetBox);
            serviceManager.bind(window.Teamlab.events.getMailDefaultMailboxSettings, onGetDefaultMailboxSettings);
            serviceManager.bind(window.Teamlab.events.createMailMailboxSimple, onCreateMailbox);
            serviceManager.bind(window.Teamlab.events.createMailMailboxOAuth, onCreateMailbox);
            serviceManager.bind(window.Teamlab.events.createMailMailbox, onCreateMailbox);


            wnd_question = $('#questionWnd');
            wnd_question.find('.buttons .cancel').bind('click', function() {
                hide();
                return false;
            });

            wnd_question.find('.buttons .remove').bind('click', function() {
                hide();
                serviceManager.removeBox(account_id, {}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
                serviceManager.updateFolders();
                serviceManager.getTags();
                return false;
            });

            wnd_question.find('.buttons .activate').bind('click', function() {
                hide();
                serviceManager.setMailboxState(account_id, true, { email: account_id, enabled: true }, {}, ASC.Resources.Master.Resource.LoadingProcessing);
                return false;
            });

            wnd_question.find('.buttons .deactivate').bind('click', function() {
                hide();
                serviceManager.setMailboxState(account_id, false, { email: account_id, enabled: false }, {}, ASC.Resources.Master.Resource.LoadingProcessing);
                return false;
            });

            $('#manageWindow .cancelButton').css('cursor', 'pointer');
            $('#manageWindow .cancelButton').removeAttr('onclick');
            $('#manageWindow .cancelButton').click(function() {
                hide(true);
            });
        }
    };

    function addBox() {
        showWizard();
    }

    function removeBox(account) {
        account_id = account;
        questionBox('remove');
    }

    function editBox(account) {
        serviceManager.getBox(account, { action: 'edit' }, {}, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function blockUi(width, message) {
        var margintop = $(window).scrollTop() - 135;
        margintop = margintop + 'px';
        $.blockUI({ message: message,
            css: {
                left: '50%',
                top: '25%',
                opacity: '1',
                border: 'none',
                padding: '0px',
                width: width + 'px',

                cursor: 'default',
                textAlign: 'left',
                position: 'absolute',
                'margin-left': '-' + parseInt(width / 2) + 'px',
                'margin-top': margintop,
                'background-color': 'White'
            },
            overlayCSS: {
                backgroundColor: '#AAA',
                cursor: 'default',
                opacity: '0.3'
            },
            focusInput: true,
            baseZ: 666,

            fadeIn: 0,
            fadeOut: 0,

            onBlock: function() {

            }
        });

        $('#manageWindow .cancelButton').css('cursor', 'pointer');
        $('.containerBodyBlock .buttons .cancel').unbind('click').bind('click', function () {
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

        if ($(html).find('#oauth_frame_blocker').length) blockUi(540, $("#manageWindow"));
        else blockUi(400, $("#manageWindow"));

        if (window.addEventListener) {
            window.addEventListener('message', onGetOAuthInfo, false);
        } else {
            window.attachEvent('onmessage', onGetOAuthInfo);
        }

        $('.containerBodyBlock .buttons #save').unbind('click').bind('click', function() {
            if ($('.containerBodyBlock .buttons #save').attr('disabled'))
                return false;

            createMailBoxSimple();
            return false;
        });

        $('.containerBodyBlock .buttons #advancedLinkButton').unbind('click').bind('click', function() {
            if ($('.containerBodyBlock .buttons #advancedLinkButton').attr('disabled'))
                return false;

            switchToAdvanced("get_imap_pop_settings");
            return false;
        });

        $('.containerBodyBlock #password').keyup(setupPasswordView);
        $('.containerBodyBlock a.password-view').unbind('click').bind('click', togglePassword);

        window.PopupKeyUpActionProvider.EnterAction = "jq('.containerBodyBlock .buttons #save').click();";

        if (email)
            setVal(ids.email, email);
    }

    function setupPasswordView(event) {
        var container_id = $(this).closest('.requiredField').attr('id');
        var is_smtp = container_id == 'mail_SMTPPasswordContainer';
        var password = getVal(is_smtp ? ids.smtp_password : ids.password, true);
        var password_view_link = $('.containerBodyBlock #' + container_id + ' .headerPanelSmall a.password-view');
        if (password.length > 0)
            setRequiredError(container_id, false);
        password_view_link.toggleClass('off', password.length == 0);
    }
    
    function togglePassword() {
        var password_input = $(this).closest('.requiredField').find('input');
        if (password_input.attr('type') == "password") {
            password_input.attr('type', 'text');
            $(this).text(window.MailScriptResource.HidePasswordLinkLabel);
        } else {
            password_input.attr('type', 'password');
            $(this).text(window.MailScriptResource.ShowPasswordLinkLabel);
        }
    }

    function onGetOAuthInfo(evt) {
        var obj;
        if (typeof evt.data == "string") {
            try {
                obj = window.jQuery.parseJSON(evt.data);
            } catch (err) {
                return;
            }
        } else {
            obj = evt.data;
        }

        if (obj.Tpr == "OAuthImporter" && obj.Data.length != 0) {
            if (obj.Data.Email != null && obj.Data.RefreshToken != null) {
                showLoader(window.MailScriptResource.MailboxCreation);
                serviceManager.createOAuthBox(obj.Data.Email, obj.Data.RefreshToken, 1, //google service
                    {email: obj.Data.Email, name: '', enabled: true, restrict: true, oauth: true },
                    { error: onErrorCreateMailboxSimple });
            }
        }
        else
            hide();
    }

    function switchToAdvanced(action) {
        stored_settings = undefined;
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
                incoming_encryption_type: encription_types.ssl,
                port: default_ports.imap_ssl,
                auth_type_in: auth_types.simple,
                restrict: true,
                outcoming_encryption_type: encription_types.ssl,
                smtp_port: default_ports.smtp_ssl,
                auth_type_smtp: auth_types.simple
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
        disableConrtols();
    };

    function hideLoader(force_hide_container) {
        if (force_hide_container)
            $('.popupMailBox').find('div.progressContainer').hide();

        $('.popupMailBox').find('div.progressContainer .loader').hide();

        $('.containerBodyBlock .buttons #save').removeAttr('disabled').removeClass("disable");
        $('.containerBodyBlock .buttons #cancel').removeAttr('disabled').removeClass("disable");
        $('.popupMailBox #oauth_frame_blocker').hide();
        $('.containerBodyBlock .buttons #getDefaultSettings').removeAttr('disabled').removeClass("disable");
        $('.containerBodyBlock .buttons #advancedLinkButton').removeAttr('disabled').removeClass("disable");
        
        $('#manageWindow .containerHeaderBlock').find('.popupCancel .cancelButton').css('cursor', 'pointer');

        enableConrtols();
        if (getVal(ids.auth_type_smtp_sel, true) == 0) {
            enable(ids.smtp_account);
            enable(ids.smtp_password);
        }
    }

    function enableConrtols() {
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

    function disableConrtols() {
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

        if ((params.action == 'get_imap_server_full' || params.action == 'get_pop_server_full') && $('#manageWindow').attr('classname') == 'editInbox')
            params.action = 'edit';

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
            disableConrtols();
            $('.popupMailBox #getDefaultSettings').hide();
        }

        $('.containerBodyBlock .buttons #save').unbind('click').bind('click', function() {
            if ($('.containerBodyBlock .buttons #save').attr('disabled'))
                return false;

            $('.popupMailBox').find('div.progressContainer').show();
            $('.popupMailBox').find('div.error-popup').hide();
            if ($('#manageWindow').attr('className') == 'addInbox')
                createMailbox();
            else
                updateMailbox();

            return false;
        });

        $('.popupMailBox .buttons #getDefaultSettings').unbind('click').bind('click', function() {
            if ($('.containerBodyBlock .buttons #getDefaultSettings').attr('disabled'))
                return false;

            var email = getVal(ids.email);

            var email_correct = isEmailCorrect(email);

            setRequiredError("mail_EMailContainer", !email_correct);

            if (getVal(ids.server_type, true) == 'imap') {
                params.action = "get_imap_server_full";
            } else {
                params.action = "get_pop_server_full";
            }

            if (email_correct) {
                switchToAdvanced(params.action);
            }

            return false;
        });

        if (params.action == 'edit') {
            disable(ids.email);
            disable(ids.server_type);
        }

        $('.popupMailBox #auth_type_smtp_sel').unbind('change').bind('change', function () {
            if ($(this).val() != 0) {
                enable(ids.smtp_account);
                enable(ids.smtp_password);
                $("#mail_SMTPLoginContainer").addClass('requiredField');
                $("#mail_SMTPPasswordContainer").addClass('requiredField');
            } else {
                disable(ids.smtp_account);
                disable(ids.smtp_password);
                setRequiredError("mail_SMTPLoginContainer", false);
                setRequiredError("mail_SMTPPasswordContainer", false);

                $("#mail_SMTPLoginContainer").removeClass('requiredField');
                $("#mail_SMTPPasswordContainer").removeClass('requiredField');
            }
        });

        $('.popupMailBox #server').unbind('blur').bind('blur', function() {
            var addr = getVal(ids.server, true);
            if (addr.length > 0 && getVal(ids.smtp_server, true).length == 0)
                setVal(ids.smtp_server, 'smtp' + addr.substring(addr.indexOf('.')));
        });

        $('.popupMailBox #server-type').unbind('change').bind('change', function() {
            var email = getVal(ids.email);

            var is_email_correct = TMMail.reEmailStrict.test(email);

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
            
            if(is_email_correct)
                serviceManager.getDefaultMailboxSettings(email, { action: action, password: getVal(ids.password, true), smtp_password: getVal(ids.smtp_password, true) }, { error: onErrorGetDefaultMailboxSettings }, window.MailScriptResource.ObtainingDefaultDomainSettings);
        });

        $('.containerBodyBlock #password').keyup(setupPasswordView);
        $('.containerBodyBlock #smtp_password').keyup(setupPasswordView);
        $('.containerBodyBlock a.password-view').unbind('click').bind('click', togglePassword);

        if (params.error)
            setErrorToPopupMailbox(params.error);
    }

    function setErrorToPopupMailbox(error_text) {
        hideLoader(true);
        $('.popupMailBox').find('div.error-popup').show().find('.text').text(error_text);
    }

    function createMailbox() {
        return updateMailbox(true);
    }

    function setRequiredHint(container_id, text) {
        $("#" + container_id + ".requiredField span").text(text);
    }

    function setRequiredError(container_id, need_show) {
        if (need_show)
            $("#" + container_id + ".requiredField").addClass(required_field_error_css);
        else
            $("#" + container_id + ".requiredField").removeClass(required_field_error_css);
    }

    function createMailBoxSimple() {
        var email = getVal(ids.email),
            password = getVal(ids.password, true);

        var email_correct = false;
        var password_correct = false;

        if (email.length === 0) {
            setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorEmptyField);
        }
        else if (!TMMail.reEmailStrict.test(email)) {
            setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorIncorrectEmail);
        }
        else if (accountsPage.isContain(email.toLowerCase())) {
            setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorAccountAlreadyExists);
        }
        else
            email_correct = true;

        setRequiredError("mail_EMailContainer", !email_correct);

        if (password.length !== 0) {
            password_correct = true;
        }

        setRequiredError("mail_PasswordContainer", !password_correct);

        if (email_correct && password_correct) {

            window.ASC.Mail.ga_track(ga_Categories.accauntsSettings, ga_Actions.createNew, "create_min_account");

            showLoader(window.MailScriptResource.MailboxCreation);
            serviceManager.createMinBox(email, password,
                { email: email, name: '', enabled: true, restrict: true, oauth: false },
                { error: onErrorCreateMailboxSimple });
        }
    }

    function updateMailbox(new_flag) {
        var 
            email = getVal(ids.email),
            server = getVal(ids.server),
            account = getVal(ids.account),
            name = getVal(ids.name),
            password = getVal(ids.password, true),
            port = getVal(ids.port),
            smtp_auth = $('.popupMailBox #auth_type_smtp_sel').val() != 0,
            smtp_server = getVal(ids.smtp_server),
            smtp_account = getVal(ids.smtp_account),
            smtp_password = getVal(ids.smtp_password),
            smtp_port = getVal(ids.smtp_port),
            imap = (getVal(ids.server_type, true) == 'imap'),
            mail_limit = $('.popupMailBox #mail-limit').is(':checked'),
            incoming_encryption_type = getVal(ids.incoming_encryption_type + ' option:selected', true),
            outcoming_encryption_type = getVal(ids.outcoming_encryption_type + ' option:selected', true),
            auth_type_in = getVal(ids.auth_type_in_sel + ' option:selected', true),
            auth_type_smtp = getVal(ids.auth_type_smtp_sel + ' option:selected', true);

        stored_settings = {};
        stored_settings.account = account;
        stored_settings.auth_type_in = auth_type_in;
        stored_settings.auth_type_smtp = auth_type_smtp;
        stored_settings.email = email;
        stored_settings.imap = imap;
        stored_settings.incoming_encryption_type = incoming_encryption_type;
        stored_settings.name = name;
        stored_settings.outcoming_encryption_type = outcoming_encryption_type;
        stored_settings.password = "";
        stored_settings.port = port;
        stored_settings.server = server;
        stored_settings.smtp_account = smtp_account;
        stored_settings.smtp_auth = smtp_auth;
        stored_settings.smtp_password = "";
        stored_settings.smtp_port = smtp_port;
        stored_settings.smtp_server = smtp_server;

        if (password == $('#password').attr('placeholder')) password = '';
        if (smtp_password == $('#smtp_password').attr('placeholder')) smtp_password = '';

        var email_incorrect = true;
        var server_incorrect = false;
        var port_incorrect = false;
        var account_incorrect = false;
        var password_incorrect = false;
        var smtp_server_incorrect = false;
        var smtp_port_incorrect = false;
        var smtp_account_incorrect = false;
        var smtp_password_incorrect = false;

        if (email.length === 0) {
            $("#mail_EMailContainer.requiredField span").text(window.MailScriptResource.ErrorEmptyField);
        }
        else if (!TMMail.reEmailStrict.test(email)) {
            setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorIncorrectEmail);
        }
        else if (new_flag && accountsPage.isContain(email.toLowerCase())) {
            setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorAccountAlreadyExists);
        }
        else
            email_incorrect = false;

        setRequiredError("mail_EMailContainer", email_incorrect);
        setRequiredError("mail_POPServerContainer", server_incorrect = server.length === 0);
        setRequiredError("mail_POPPortContainer", port_incorrect = port.length === 0);
        setRequiredError("mail_POPLoginContainer", account_incorrect = account.length === 0);
        setRequiredError("mail_POPPasswordContainer", password_incorrect = (password.length === 0 && new_flag));

        setRequiredError("mail_SMTPServerContainer", smtp_server_incorrect = smtp_server.length === 0);
        setRequiredError("mail_SMTPPortContainer", smtp_port_incorrect = smtp_port.length === 0);

        if (smtp_auth) {
            setRequiredError("mail_SMTPLoginContainer", smtp_account_incorrect = smtp_account.length === 0);
            setRequiredError("mail_SMTPPasswordContainer", smtp_password_incorrect = (smtp_password.length === 0 && new_flag));
        }

        if (!email_incorrect &&
            !server_incorrect &&
            !port_incorrect &&
            !account_incorrect &&
            !password_incorrect &&
            !smtp_server_incorrect &&
            !smtp_port_incorrect &&
            !smtp_account_incorrect &&
            !smtp_password_incorrect) {
            if (true === new_flag) {
                window.ASC.Mail.ga_track(ga_Categories.accauntsSettings, ga_Actions.createNew, "create_advanced_account");
                showLoader(window.MailScriptResource.MailboxCreation);
                serviceManager.createBox(name,
                    email,
                    account,
                    password,
                    port,
                    server,
                    smtp_account,
                    smtp_password,
                    smtp_port,
                    smtp_server,
                    smtp_auth,
                    imap,
                    mail_limit,
                    incoming_encryption_type,
                    outcoming_encryption_type,
                    auth_type_in,
                    auth_type_smtp,
                    { email: email, name: name, enabled: true, restrict: mail_limit, oauth: false, action: 'add' },
                    { error: onErrorCreateMailbox });
            } else {
                showLoader(window.MailScriptResource.MailBoxUpdate);
                serviceManager.updateBox(name,
                    email,
                    account,
                    password,
                    port,
                    server,
                    smtp_account,
                    smtp_password,
                    smtp_port,
                    smtp_server,
                    smtp_auth,
                    mail_limit,
                    incoming_encryption_type,
                    outcoming_encryption_type,
                    auth_type_in,
                    auth_type_smtp,
                    { email: email, name: name, enabled: true, restrict: mail_limit, action: 'edit' },
                    { error: onErrorCreateMailbox });
            }
        }
    }

    function questionBox(operation) {
        var header = '';
        wnd_question.find('.activate').hide();
        wnd_question.find('.deactivate').hide();
        wnd_question.find('.remove').hide();
        var question = '';
        switch (operation) {
            case 'remove':
                header = wnd_question.attr('delete_header');
                wnd_question.find('.remove').show();
                question = window.MailScriptResource.DeleteAccountShure;
                break;

            case 'activate':
                header = wnd_question.attr('activate_header');
                wnd_question.find('.activate').show();
                question = window.MailScriptResource.ActivateAccountShure;
                break;

            case 'deactivate':
                header = wnd_question.attr('deactivate_header');
                wnd_question.find('.deactivate').show();
                question = window.MailScriptResource.DeactivateAccountShure;
                break;
        }

        wnd_question.find('div.containerHeaderBlock:first td:first').html(header);
        question = question.replace(/%1/g, account_id);
        wnd_question.find('.mail-confirmationAction p.questionText').text(question);

        blockUi(523, wnd_question);

        window.PopupKeyUpActionProvider.EnterAction = "jq('#questionWnd .containerBodyBlock .buttons .button.blue:visible').click();";
    }

    function informationBox(params) {
        var content = window.MailScriptResource.ImportAccountText;
        if (params.restrict === true)
            content += '<br><br>' + window.MailScriptResource.ImportAccountRestrictLabel;

        var footer;
        if (/@gmail.com$/.test(params.email.toLowerCase()))
            footer = window.MailScriptResource.ImportGmailAttention;
        else
            footer = window.MailScriptResource.ImportProblemText;

        footer = footer
            .replace('{0}', '<a href="{0}"class="linkDescribe" target="blank">'
                .format(TMMail.getFaqLink(params.email.toLowerCase())))
            .replace('{1}', '</a>');

        var body = $($.tmpl("accountSuccessTmpl", {
            errorBody: content,
            errorBodyFooter: footer
        }));
        
        body.find('td.errorImg').toggleClass('errorImg', false).toggleClass('successImg', true);

        popup.addBig(window.MailScriptResource.DoneLabel, body);
    }

    function warningAlreadyExist() {
        html = $.tmpl('accountExistErrorTmpl', {});
        popup.addBig(window.MailScriptResource.ExistingAccountLabel, html);
    }

    function activateBox(account, activated) {
        account_id = account;
        if (activated) questionBox('activate');
        else questionBox('deactivate');
    }

    function hide(stopOnSaving) {
        if (stopOnSaving) {
            var loader = $('.popupMailBox div.progressContainer div.loader');
            if (loader != undefined && loader.is(':visible'))
                return;
        }

        if (window.removeEventListener) {
            window.removeEventListener("message", onGetOAuthInfo, false);
        }
        else {
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

    function onCreateMailbox(params, mailbox) {
        if (!accountsManager.getAccountByAddress(mailbox.email)) {
            if (TMMail.pageIs('sysfolders') && accountsManager.getAccountList().length == 0)
                blankPages.showEmptyFolder();
            accountsPage.addAccount(params.email, params.enabled, params.oauth);
            accountsManager.addAccount({
                email: mailbox.email, enabled: true, id: mailbox.id, name: mailbox.name, oauth: params.oauth,
                signature: {
                    html: "", isActive: false, mailboxId: mailbox.id, tenant: 0
                }
            });
            hide();
            informationBox(params);
        }
        else {
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
                errorBodyFooter     : footer,
                errorAdvancedInfo   : params.error_text[0]
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
                if (params.action == "edit")
                    editBox(params.email);
                else {
                    if (stored_settings != undefined)
                        onGetBox({ action: "get_pop_server_full" }, stored_settings);
                    else
                        serviceManager.getDefaultMailboxSettings(params.email,
                            { action: "get_pop_server_full", password: params.password, smtp_password: params.smtp_password },
                            {
                                success: function() { setErrorToPopupMailbox(window.MailScriptResource.CorrectAccountRecommendation); },
                                error: function() { setErrorToPopupMailbox(window.MailScriptResource.ErrorNotificationEx); }
                            },
                            window.MailScriptResource.ObtainingDefaultDomainSettings);
                }
            }
        });
        
        $('#account_error_container .buttons .tryagain').keypress(function (event) {
            event.preventDefault();
        });

        window.PopupKeyUpActionProvider.EnterAction = "jq('#account_error_container .buttons #tryagain').click()";
    }

    function onErrorCreateMailbox(params, error) {
        params.error_text = error;
        showErrorModal(params);
    }

    function getVal(id, skip_trim) {
        var res = $('.popupMailBox #' + id).val();
        return skip_trim ? res : $.trim(res);
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
         var is_email_correct = false;

            if (email.length === 0) {
                setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorEmptyField);
            }
            else if (!TMMail.reEmailStrict.test(email)) {
                setRequiredHint("mail_EMailContainer", window.MailScriptResource.ErrorIncorrectEmail);
            }
            else
                is_email_correct = true;

        return is_email_correct;
    }

    function signatureBox(accountAddress) {
        var account = accountsManager.getAccountByAddress(accountAddress);
        if (account) {
            var is_active = (account.signature.html == "" && !account.signature.isActive) ? true : account.signature.isActive; // Default is true;

            html = $.tmpl("manageSignatureTmpl", { 'isActive': is_active });

            var config = {
                toolbar: 'MailSignature',
                removePlugins: 'resize, magicline',
                filebrowserUploadUrl: 'fckuploader.ashx?newEditor=true&esid=mail',
                height: 200,
                startupFocus: true,
                on: {
                    instanceReady: function (instance) {
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
        var is_active = $('#useSignatureFlag').is(':checked') ? true : false;

        if (html == "" && is_active)
            is_active = false;

        serviceManager.updateMailboxSignature(account.id, html, is_active, {id: account.id, html: html, is_active: is_active},
            { error: onErrorUpdateMailboxSignature }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function onErrorUpdateMailboxSignature(params, error) {
        hideLoader();
    }

    return {
        init: init,
        addBox: addBox,
        removeBox: removeBox,
        activateBox: activateBox,
        editBox: editBox,
        hide: hide,
        onError: onError,
        setBoxData: setBoxData,
        showInformationBox: informationBox,
        showSignatureBox: signatureBox
    };
})(jQuery);