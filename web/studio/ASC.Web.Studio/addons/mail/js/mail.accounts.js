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


window.accountsManager = (function($) {
    var isInit = false,
        accountList = [],
        getAccountsHandler;

    function init() {
        if (isInit === false) {
            isInit = true;

            var accounts = [];

            getAccountsHandler = window.Teamlab.bind(window.Teamlab.events.getAccounts, onGetMailAccounts);
            window.Teamlab.bind(window.Teamlab.events.updateMailMailbox, onUpdateMailMailbox);
            window.Teamlab.bind(window.Teamlab.events.setMailMailboxState, onSetMailboxState);
            window.Teamlab.bind(window.Teamlab.events.updateMailboxSignature, onUpdateMailboxSignature);
            window.Teamlab.bind(window.Teamlab.events.updateMailboxAutoreply, onUpdateMailboxAutoreply);
            window.Teamlab.bind(window.Teamlab.events.setEMailInFolder, onSetEMailInFolder);

            accountsModal.init();
            accountsPage.init();

            if (ASC.Mail.Presets.Accounts) {
                var showDisabledAccountToast = false,
                    showEnabledAutoreply = false;

                accounts = $.map(ASC.Mail.Presets.Accounts, function(account) {

                    account.signature.html = TMMail.htmlDecode(account.signature.html);
                    account.autoreply.html = TMMail.htmlDecode(account.autoreply.html);

                    if (!account.enabled && account.authError) {
                        showDisabledAccountToast = true;
                    }

                    if (!showEnabledAutoreply)
                        showEnabledAutoreply = isAutoreplyEnabled(account);

                    return account;
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
    }

    function isAutoreplyEnabled(account) {
        var now = window.moment(new Date()),
            enabledAutoreply = false;

        var toDate = window.moment(new Date(account.autoreply.toDate)),
                        fromDate = window.moment(new Date(account.autoreply.fromDate));

        if (toDate.diff(fromDate) === 0) {
            toDate = toDate.add(1, 'day');
        }

        if (account.enabled
            && account.autoreply.turnOn
            && fromDate.diff(now) <= 0
            && (!account.autoreply.turnOnToDate || toDate.diff(now) >= 0)) {
            enabledAutoreply = true;
        }

        return enabledAutoreply;
    }

    function onGetMailAccounts(params, accounts) {
        accountsPage.clear();
        initAccounts(accounts);
        accountsPage.loadAccounts(accounts);
    }

    function initAccounts(accounts) {
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
    }

    function onUpdateMailMailbox(params, account) {
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
    }

    function removeAccount(email) {
        accountsPage.deleteAccount(email);
        var index = getAccountIndexByAddress(email);
        if (index !== -1) {
            accountList.splice(index, 1);
        }

        mailBox.markFolderAsChanged(TMMail.sysfolders.inbox.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.sent.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.drafts.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.templates.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.trash.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.spam.id);
    }

    function onSetMailboxState(params, mailboxId) {
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
    }

    function enableMailbox(email, enabled) {
        accountsPage.activateAccount(email, enabled);
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].email == email.toLowerCase()) {
                accountList[i].enabled = enabled;
                break;
            }
        }
    }

    function onUpdateMailboxSignature(params, signature) {
        accountsModal.hide();
        var account = window.accountsManager.getAccountById(params.id);
        if (account) {
            account.signature = signature;
        }
        var aliases = getAliasesByMailboxId(params.id);
        for (var i = 0; i < aliases.length; i++) {
            aliases[i].signature = signature;
        }
    }

    function onUpdateMailboxAutoreply(params, autoreply) {
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
    }

    function onSetEMailInFolder(params) {
        accountsModal.hide();
        var account = getAccountById(params.id);
        account.emailInFolder = params.emailInFolder;

        if (params.resetFolder) {
            window.toastr.success(window.MailScriptResource.ResetAccountEMailInFolderSuccess);
        } else {
            window.toastr.success(window.MailScriptResource.SetAccountEMailInFolderSuccess);
        }
    }

    function getAccountList() {
        return accountList;
    }

    function setDefaultAccount(email, setDefault) {
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
    }

    function getDefaultAccount() {
        var defaultAccounts = jq.grep(accountList, function (a) { return a.is_default === true });
        return defaultAccounts.length !== 0 ? defaultAccounts[0] : accountList[0];
    }

    function getAccountIndexByAddress(email) {
        var index = -1;
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].email == email.toLowerCase()) {
                index = i;
                break;
            }
        }
        return index;
    }

    function getAccountByAddress(email) {
        var mailBox = null;
        var i = getAccountIndexByAddress(email);
        if (i > -1) {
            mailBox = accountList[i];
        }
        return mailBox;
    }

    function getAccountById(id) {
        var mailBox = null;
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].mailbox_id == id && !accountList[i].is_group && !accountList[i].is_alias) {
                mailBox = accountList[i];
                break;
            }
        }
        return mailBox;
    }

    function getAliasesByMailboxId(id) {
        var aliases = [];
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].mailbox_id == id && accountList[i].is_alias) {
                aliases.push(accountList[i]);
            }
        }
        return aliases;
    }

    function addAccount(account) {
        account.email = account.email.toLowerCase();
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].email == account.email) {
                return;
            }
        }
        accountList.push(account);
        contactsManager.init();
    }

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
        removeAccount: removeAccount,
        isAutoreplyEnabled: isAutoreplyEnabled
    };

})(jQuery);