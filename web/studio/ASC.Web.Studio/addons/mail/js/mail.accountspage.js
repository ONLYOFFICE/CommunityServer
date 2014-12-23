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

window.accountsPage = (function($) {
    var 
        isInit = false,
        $page,
        buttons = [];

    var init = function() {
        if (isInit === false) {
            isInit = true;

            $page = $('#id_accounts_page');

            $page.find('#createNewAccount').click(function() {
                accountsModal.addBox();
                return false;
            });

            buttons = [
                { selector: "#accountActionMenu .activateAccount", handler: _activate },
                { selector: "#accountActionMenu .deactivateAccount", handler: _deactivate },
                { selector: "#accountActionMenu .selectAttachmentsFolder", handler: _selectAttachmentsFolder },
                { selector: "#accountActionMenu .editAccount", handler: _editAccount },
                { selector: "#accountActionMenu .deleteAccount", handler: _removeAccount }];

            var additional_button_html = $.tmpl('fileSelectorMailAdditionalButton');
            $('#fileSelectorAdditionalButton').replaceWith(additional_button_html);
        }
    };

    var show = function() {
        if (_checkEmpty())
            $page.hide();
        else
            $page.show();
    };

    var hide = function() {
        $page.hide();
    };

    function clear() {
        var accounts_rows = $page.find('.accounts_list');
        $('#accountActionMenu').hide();
        if (accounts_rows)
            accounts_rows.remove();
    }

    var addAccount = function(accountName, enabled, oauth, is_teamlab) {
        accountName = accountName.toLowerCase();
        if (!isContain(accountName)) {
            
            var html = $.tmpl('mailboxItemTmpl',
            {
                email: accountName,
                enabled: enabled,
                oAuthConnection: oauth,
                isTeamlabMailbox: is_teamlab
            });

            var $html = $(html);
            $html.actionMenu('accountActionMenu', buttons, _pretreatment);
            $('#common_mailboxes').append($html);
            
        }
        if (TMMail.pageIs('accounts') && !_checkEmpty()) {
            $page.show();
        }
    };

    var deleteAccount = function(id) {
        $page.find('tr[data_id="' + id + '"]').remove();
        if (_checkEmpty() && TMMail.pageIs('accounts'))
            $page.hide();
    };

    var activateAccount = function(accountName, activate) {
        var account_div = $page.find('tr[data_id="' + accountName + '"]');

        if (activate) {
            account_div.removeClass('disabled');
        }
        else {
            account_div.toggleClass('disabled', true);
        }

    };

    var _pretreatment = function(id) {
        if ($page.find('tr[data_id="' + id + '"]').hasClass('disabled')) {
            $("#accountActionMenu .activateAccount").show();
            $("#accountActionMenu .deactivateAccount").hide();
        }
        else {
            $("#accountActionMenu .activateAccount").hide();
            $("#accountActionMenu .deactivateAccount").show();
        }

        var account = accountsManager.getAccountByAddress(id);
        if (account.is_teamlab) {
            $("#accountActionMenu .deactivateAccount").addClass('disable');
            $("#accountActionMenu .editAccount").addClass('disable');
            $("#accountActionMenu .deleteAccount").addClass('disable');
            $("#accountActionMenu .deactivateAccount").attr('title', MailScriptResource.ServerMailboxNotificationText);
            $("#accountActionMenu .editAccount").attr('title', MailScriptResource.ServerMailboxNotificationText);
            $("#accountActionMenu .deleteAccount").attr('title', MailScriptResource.ServerMailboxNotificationText);
        } else {
            $("#accountActionMenu .deactivateAccount").removeClass('disable');
            $("#accountActionMenu .editAccount").removeClass('disable');
            $("#accountActionMenu .deleteAccount").removeClass('disable');
            $("#accountActionMenu .deactivateAccount").removeAttr('title');
            $("#accountActionMenu .editAccount").removeAttr('title');
            $("#accountActionMenu .deleteAccount").removeAttr('title');
        }
    };

    var _activate = function(id) {
        accountsModal.activateBox(id, true);
    };

    var _deactivate = function(id) {
        accountsModal.activateBox(id, false);
    };

    var _selectAttachmentsFolder = function (email) {
        var account = window.accountsManager.getAccountByAddress(email);
        ASC.Files.FileSelector.onSubmit = function (folderId) {
            serviceManager.setEMailInFolder(
                account.mailbox_id, folderId,
                { id: account.mailbox_id, emailInFolder: folderId, resetFolder: false },
                { error: onErrorSetEMailInFolder },
                ASC.Resources.Master.Resource.LoadingProcessing);
        };

        $('#filesFolderUnlinkButton').unbind('click').bind('click', { account: account }, _unselectAttachmentsFolder);

        ASC.Files.FileSelector.fileSelectorTree.resetFolder();
        if (account.emailInFolder == null) {
            ASC.Files.FileSelector.openDialog(null, true);
            $('#filesFolderUnlinkButton').toggleClass('disable', true);
        } else {
            ASC.Files.FileSelector.openDialog(account.emailInFolder, true);
            $('#filesFolderUnlinkButton').toggleClass('disable', false);
        }
    };

    var _unselectAttachmentsFolder = function (event) {
        if ($(this).hasClass('disable'))
            return;

        var account = event.data.account;
        serviceManager.setEMailInFolder(
                account.mailbox_id, null,
                { id: account.mailbox_id, emailInFolder: null, resetFolder: true },
                { error: onErrorResetEMailInFolder },
                ASC.Resources.Master.Resource.LoadingProcessing);
    };

    function onErrorResetEMailInFolder() {
        window.toastr.error(window.MailScriptResource.ResetAccountEMailInFolderFailure);
    }

    function onErrorSetEMailInFolder() {
        window.toastr.error(window.MailScriptResource.SetAccountEMailInFolderFailure);
    }

    var _editAccount = function(id) {
        accountsModal.editBox(id);
    };

    var _removeAccount = function(id) {
        accountsModal.removeBox(id);
    };

    var isContain = function(accountName) {
        var account = $page.find('tr[data_id="' + accountName + '"]');
        return (account.length > 0);
    };

    var _checkEmpty = function() {
        if ($page.find('.accounts_list tr').length) {
            $page.find('.accounts_list').show();
            blankPages.hide();
            return false;
        }
        else {
            blankPages.showEmptyAccounts();
            return true;
        }
    };

    function loadAccounts(accounts) {
        var common_mailboxes = [],
            server_mailboxes = [],
            aliases = [],
            groups = [],
            index, length;

        clear();

        for (index = 0, length = accounts.length; index < length; index++) {
            if (accounts[index].isGroup) groups.push(accounts[index]);
            else if (accounts[index].isAlias) aliases.push(accounts[index]);
            else if (accounts[index].isTeamlabMailbox) server_mailboxes.push(accounts[index]);
            else common_mailboxes.push(accounts[index]);
        }

        server_mailboxes.forEach(function (mailbox) {
            mailbox.aliases = [];
            for (index = 0, length = aliases.length; index < length; index++) {
                if (aliases[index].mailboxId == mailbox.mailboxId) mailbox.aliases.push(aliases[index]);
            }
        });

        var html = $.tmpl('accountsTmpl',
            {
                common_mailboxes: common_mailboxes, 
                server_mailboxes: server_mailboxes, 
                groups: groups
            });

        var $html = $(html);
        $('#id_accounts_page .containerBodyBlock .content-header').after($html);
        $('#id_accounts_page').actionMenu('accountActionMenu', buttons, _pretreatment);

        server_mailboxes.forEach(function (mailbox) {
            if (mailbox.aliases.length > 1) {
                var items = [];
                for (index = 1, length = mailbox.aliases.length; index < length; index++) {
                    items.push({ 'text': mailbox.aliases[index].email, 'disabled': true });
                }
                $('#id_accounts_page').find('.row[data_id="' +
                    mailbox.email + '"] .more-aliases').actionPanel({ 'buttons': items});
            }
 
        });
    }


    return {
        init: init,

        show: show,
        hide: hide,

        addAccount: addAccount,
        deleteAccount: deleteAccount,
        activateAccount: activateAccount,
        isContain: isContain,
        clear: clear,
        loadAccounts: loadAccounts
    };

})(jQuery);