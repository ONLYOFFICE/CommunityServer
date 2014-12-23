/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

window.accountsManager = (function($) {
    var 
        is_init = false,
        account_list = [],
        get_accounts_handler;

    var init = function() {
        if (is_init === false) {
            is_init = true;

            get_accounts_handler = serviceManager.bind(window.Teamlab.events.getAccounts, onGetMailAccounts);
            serviceManager.bind(window.Teamlab.events.removeMailMailbox, _onRemoveMailbox);
            serviceManager.bind(window.Teamlab.events.updateMailMailbox, _onUpdateMailMailbox);
            serviceManager.bind(window.Teamlab.events.setMailMailboxState, _onSetMailboxState);
            serviceManager.bind(window.Teamlab.events.updateMailboxSignature, onUpdateMailboxSignature);
            serviceManager.bind(window.Teamlab.events.setEMailInFolder, onSetEMailInFolder);

            accountsModal.init();
            accountsPage.init();
        }
    };

    var onGetMailAccounts = function(params, accounts) {
        accountsPage.clear();
        account_list = [];
        $.each(accounts, function(index, value) {
            var account = {};
            account.name = TMMail.ltgt(value.name);
            account.email = TMMail.ltgt(value.email);
            account.enabled = value.enabled;
            account.signature = value.signature;
            account.is_alias = value.isAlias;
            account.is_group = value.isGroup;
            account.oauth = value.oAuthConnection;
            account.emailInFolder = value.eMailInFolder;
            account.is_teamlab = value.isTeamlabMailbox;
            account.mailbox_id = value.mailboxId;
            addAccount(account);
        });

        accountsPage.loadAccounts(accounts);
    };

    var _onUpdateMailMailbox = function(params, mailbox) {
        accountsModal.hide();
        for (var i = 0; i < account_list.length; i++) {
            if (account_list[i].email == params.email.toLowerCase()) {
                account_list[i].name = params.name;
                break;
            }
        }
    };

    var _onRemoveMailbox = function(params, email) {
        accountsPage.deleteAccount(email);
        for (var i = 0; i < account_list.length; i++) {
            if (account_list[i].email == email.toLowerCase()) {
                account_list.splice(i, 1);
                break;
            }
        }
        mailBox.markFolderAsChanged(TMMail.sysfolders.inbox.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.sent.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.drafts.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.trash.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.spam.id);
    };

    var _onSetMailboxState = function(params, email) {
        enableMailbox(params.email, params.enabled);
    };

    function enableMailbox(email, enabled) {
        accountsPage.activateAccount(email, enabled);
        for (var i = 0; i < account_list.length; i++) {
            if (account_list[i].email == email.toLowerCase()) {
                account_list[i].enabled = enabled;
                break;
            }
        }
    }

    var onUpdateMailboxSignature = function(params, signature) {
        accountsModal.hide();
        var account = window.accountsManager.getAccountById(params.id);
        if (account)
            account.signature = signature;
        var aliases = getAliasesByMailboxId(params.id);
        for (var i = 0; i < aliases.length; i++) {
            aliases[i].signature = signature;
        }
    };

    var onSetEMailInFolder = function (params) {
        accountsModal.hide();
        var account = getAccountById(params.id);
        account.emailInFolder = params.emailInFolder;

        if (params.resetFolder)
            window.toastr.success(window.MailScriptResource.ResetAccountEMailInFolderSuccess);
        else
            window.toastr.success(window.MailScriptResource.SetAccountEMailInFolderSuccess);
    };

    var getAccountList = function() {
        return account_list;
    };

    var getAccountByAddress = function(email) {
        var mailBox = undefined;
        for (var i = 0; i < account_list.length; i++) {
            if (account_list[i].email == email.toLowerCase()) {
                mailBox = account_list[i];
                break;
            }
        }
        return mailBox;
    };
    
    var getAccountById = function (id) {
        var mailBox = undefined;
        for (var i = 0; i < account_list.length; i++) {
            if (account_list[i].mailbox_id == id && !account_list[i].is_group && !account_list[i].is_alias) {
                mailBox = account_list[i];
                break;
            }
        }
        return mailBox;
    };

    var getAliasesByMailboxId = function (id) {
        var aliases = [];
        for (var i = 0; i < account_list.length; i++) {
            if (account_list[i].mailbox_id == id && account_list[i].is_alias) {
                aliases.push(account_list[i]);
                break;
            }
        }
        return aliases;
    };

    var addAccount = function(account) {
        account.email = account.email.toLowerCase();
        for (var i = 0; i < account_list.length; i++) {
            if (account_list[i].email == account.email) return;
        }
        account_list.push(account);
    };

    function any() {
        return account_list.length > 0;
    }

    return {
        init: init,
        getAccountList: getAccountList,
        getAccountByAddress: getAccountByAddress,
        getAccountById: getAccountById,
        addAccount: addAccount,
        any: any,
        enableMailbox: enableMailbox
    };

})(jQuery);
