/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


window.accountsManager = (function($) {
    var isInit = false,
        accountList = [],
        getAccountsHandler;

    var init = function() {
        if (isInit === false) {
            var accounts;
            isInit = true;

            getAccountsHandler = serviceManager.bind(window.Teamlab.events.getAccounts, onGetMailAccounts);
            serviceManager.bind(window.Teamlab.events.updateMailMailbox, onUpdateMailMailbox);
            serviceManager.bind(window.Teamlab.events.setMailMailboxState, onSetMailboxState);
            serviceManager.bind(window.Teamlab.events.updateMailboxSignature, onUpdateMailboxSignature);
            serviceManager.bind(window.Teamlab.events.updateMailboxAutoreply, onUpdateMailboxAutoreply);
            serviceManager.bind(window.Teamlab.events.setEMailInFolder, onSetEMailInFolder);

            accountsModal.init();
            accountsPage.init();

            if (ASC.Mail.Presets.Accounts) {
                var showDisabledAccountToast = false,
                    showEnabledAutoreply = false,
                    now = new Date();
                accounts = $.map(ASC.Mail.Presets.Accounts, function(el) {
                    el.signature.html = TMMail.htmlDecode(el.signature.html);
                    el.autoreply.html = TMMail.htmlDecode(el.autoreply.html);
                    if (!el.enabled && el.authError) {
                        showDisabledAccountToast = true;
                    }
                    var toDate = new Date(el.autoreply.toDate),
                        fromDate = new Date(el.autoreply.fromDate);
                    if (el.enabled && el.autoreply.turnOn &&
                        (!el.autoreply.turnOnToDate || toDate > now) && fromDate <= now) {
                        showEnabledAutoreply = true;
                    }
                    return el;
                });
                if (showEnabledAutoreply && !TMMail.pageIs('accounts') && !TMMail.pageIs('print')) {
                    window.toastr.info(MailScriptResource.GoToAccountsForChangeAutoreply.format(
                        "<a class=\"mail-autoreply-disable\">" + MailScriptResource.AutoreplyDisable + "</a>",
                        "<a href=\"#accounts\">" + MailResource.AccountsSettingsLabel + "</a>"),
                        MailScriptResource.EnabledAutoreplyNotification,
                        { "closeButton": true, "timeOut": "0", "extendedTimeOut": "0" });
                    $(".mail-autoreply-disable").off("click").on("click", function () {
                        for (var i = 0; i < accounts.length; i++) {
                            var autoreply = accounts[i].autoreply;
                            if (autoreply.turnOn) {
                                autoreply.turnOn = false;
                                serviceManager.updateMailboxAutoreply(accounts[i].mailboxId, autoreply.turnOn, autoreply.onlyContacts,
                                    autoreply.turnOnToDate, autoreply.fromDate, autoreply.toDate, autoreply.subject, autoreply.html,
                                    { id: accounts[i].mailboxId }, { error: window.accountsModal.hideLoader },
                                    ASC.Resources.Master.Resource.LoadingProcessing);
                            }
                        }
                    });
                }
                if (showDisabledAccountToast && !TMMail.pageIs('accounts')) {
                    window.toastr.error(MailScriptResource.GoToAccountsOnDeactivationText.format(
                        "<a href=\"#accounts\">" + MailResource.AccountsSettingsLabel + "</a>"),
                        MailScriptResource.DeactivatedAccountsNotification,
                        { "closeButton": true, "timeOut": "0", "extendedTimeOut": "0" });
                }
            }

            initAccounts(accounts);
            accountsPage.loadAccounts(accounts);
            accountsPanel.init();
            accountsPanel.update();
        }
    };

    var onGetMailAccounts = function(params, accounts) {
        accountsPage.clear();
        initAccounts(accounts);
        accountsPage.loadAccounts(accounts);
    };

    var initAccounts = function(accounts) {
        accountList = [];
        $.each(accounts, function(index, value) {
            var account = {
                name: TMMail.ltgt(value.name),
                email: TMMail.ltgt(value.email),
                enabled: value.enabled,
                signature: value.signature,
                autoreply: value.autoreply,
                is_alias: value.isAlias,
                is_group: value.isGroup,
                oauth: value.oAuthConnection,
                emailInFolder: value.eMailInFolder,
                is_teamlab: value.isTeamlabMailbox,
                mailbox_id: value.mailboxId,
                is_default: value.isDefault,
                is_shared_domain: value.isSharedDomainMailbox,
                authError: value.authError,
                quotaError: value.quotaError
            };
            addAccount(account);
        });
    };

    var onUpdateMailMailbox = function(params, account) {
        accountsModal.hide();

        var i = getAccountIndexByAddress(account.email);

        if (i > -1) {
            var accountData = {
                name: TMMail.ltgt(account.name),
                email: TMMail.ltgt(account.email),
                enabled: account.enabled,
                signature: account.signature,
                autoreply: account.autoreply,
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

            accountList[i] = accountData;
        }

        if (params.activateOnSuccess) {
            accountsModal.activateAccountWithoutQuestion(account.email);
        }
    };

    var removeAccount = function (email) {
        accountsPage.deleteAccount(email);
        var index = getAccountIndexByAddress(email);
        if (index !== -1) {
            accountList.splice(index, 1);
        }

        mailBox.markFolderAsChanged(TMMail.sysfolders.inbox.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.sent.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.drafts.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.trash.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.spam.id);
    };

    var onSetMailboxState = function (params, mailboxId) {
        var aliases = getAliasesByMailboxId(mailboxId);
        for (var i = 0; i < aliases.length; i++) {
            enableMailbox(aliases[i].email, params.enabled);
        }

        var accountMailbox = getAccountById(mailboxId);
        if (!accountMailbox)
            return;

        enableMailbox(accountMailbox.email, params.enabled);

        if (TMMail.pageIs('writemessage')) {
            var account = getAccountByAddress(params.email);
            messagePage.updateFromAccountField(account);
        }
        else if (TMMail.pageIs('viewmessage')) {
            jq(".from-disabled-warning").hide();
        }


        if (params.onSuccessOperationCallback && $.isFunction(params.onSuccessOperationCallback)) {
            params.onSuccessOperationCallback.call();
        }
    };

    function enableMailbox(email, enabled) {
        accountsPage.activateAccount(email, enabled);
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].email == email.toLowerCase()) {
                accountList[i].enabled = enabled;
                break;
            }
        }
    }

    var onUpdateMailboxSignature = function(params, signature) {
        accountsModal.hide();
        var account = window.accountsManager.getAccountById(params.id);
        if (account) {
            account.signature = signature;
        }
        var aliases = getAliasesByMailboxId(params.id);
        for (var i = 0; i < aliases.length; i++) {
            aliases[i].signature = signature;
        }
    };

    var onUpdateMailboxAutoreply = function (params, autoreply) {
        accountsModal.hide();
        var account = window.accountsManager.getAccountById(params.id);
        if (account) {
            account.autoreply = autoreply;
            accountsModal.refreshAccount(account.email, account.enabled);
        }
        var aliases = getAliasesByMailboxId(params.id);
        for (var i = 0; i < aliases.length; i++) {
            aliases[i].autoreply = autoreply;
            accountsModal.refreshAccount(aliases[i].email, aliases[i].enabled);
        }
    };

    var onSetEMailInFolder = function(params) {
        accountsModal.hide();
        var account = getAccountById(params.id);
        account.emailInFolder = params.emailInFolder;

        if (params.resetFolder) {
            window.toastr.success(window.MailScriptResource.ResetAccountEMailInFolderSuccess);
        } else {
            window.toastr.success(window.MailScriptResource.SetAccountEMailInFolderSuccess);
        }
    };

    var getAccountList = function() {
        return accountList;
    };

    var setDefaultAccount = function(email, setDefault) {
        var emailToLowerCase = email.toLowerCase(),
            currentAccount;

        for (var i = 0; i < accountList.length; i++) {
            currentAccount = accountList[i];
            currentAccount.is_default = false;
            if (currentAccount.email == emailToLowerCase) {
                currentAccount.is_default = setDefault;
            }
        }

        accountsPanel.update();
    };

    var getDefaultAccount = function () {
        var defaultAccounts = jq.grep(accountList, function (a) { return a.is_default === true });
        return defaultAccounts.length !== 0 ? defaultAccounts[0] : accountList[0];
    };

    var getAccountIndexByAddress = function (email) {
        var index = -1;
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].email == email.toLowerCase()) {
                index = i;
                break;
            }
        }
        return index;
    };

    var getAccountByAddress = function(email) {
        var mailBox = null;
        var i = getAccountIndexByAddress(email);
        if (i > -1) {
            mailBox = accountList[i];
        }
        return mailBox;
    };

    var getAccountById = function(id) {
        var mailBox = null;
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].mailbox_id == id && !accountList[i].is_group && !accountList[i].is_alias) {
                mailBox = accountList[i];
                break;
            }
        }
        return mailBox;
    };

    var getAliasesByMailboxId = function(id) {
        var aliases = [];
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].mailbox_id == id && accountList[i].is_alias) {
                aliases.push(accountList[i]);
            }
        }
        return aliases;
    };

    var addAccount = function(account) {
        account.email = account.email.toLowerCase();
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].email == account.email) {
                return;
            }
        }
        accountList.push(account);
        contactsManager.init();
    };

    function any() {
        return accountList.length > 0;
    }

    return {
        init: init,
        getAccountList: getAccountList,
        setDefaultAccount: setDefaultAccount,
        getDefaultAccount: getDefaultAccount,
        getAccountByAddress: getAccountByAddress,
        getAccountById: getAccountById,
        addAccount: addAccount,
        any: any,
        enableMailbox: enableMailbox,
        removeAccount: removeAccount
    };

})(jQuery);