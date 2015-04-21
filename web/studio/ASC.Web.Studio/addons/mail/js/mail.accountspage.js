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


/*
    Copyright (c) Ascensio System SIA 2015. All rights reserved.
    https://www.onlyoffice.com
*/
window.accountsPage = (function($) {
    var isInit = false,
        $page,
        buttons = [];

    var init = function() {
        if (isInit === false) {
            isInit = true;

            $page = $('#id_accounts_page');

            $page.find('#createNewMailbox').click(function () {
                accountsModal.addMailbox();
                return false;
            });

            $page.find('#createNewAccount').click(function() {
                accountsModal.addBox();
                return false;
            });

            buttons = [
                { selector: "#accountActionMenu .activateAccount", handler: activate },
                { selector: "#accountActionMenu .deactivateAccount", handler: deactivate },
                { selector: "#accountActionMenu .selectAttachmentsFolder", handler: selectAttachmentsFolder },
                { selector: "#accountActionMenu .editAccount", handler: editAccount },
                { selector: "#accountActionMenu .deleteAccount", handler: removeAccount }];

            var additionalButtonHtml = $.tmpl('fileSelectorMailAdditionalButton');
            $('#fileSelectorAdditionalButton').replaceWith(additionalButtonHtml);
        }
    };

    var show = function() {
        if (checkEmpty()) {
            $page.hide();
        } else {
            $page.show();
        }
    };

    var hide = function() {
        $page.hide();
    };

    function clear() {
        var accountsRows = $page.find('.accounts_list');
        $('#accountActionMenu').hide();
        if (accountsRows) {
            accountsRows.remove();
        }
    }

    var addAccount = function (accountName, enabled, oauth, isTeamlab) {
        accountName = accountName.toLowerCase();
        if (!isContain(accountName)) {
            var accountListLength = accountsManager.getAccountList().length,
                showSetDefaultIcon = accountListLength > 1,
                addSetDefaultIcon = accountListLength == 1,
                html = $.tmpl('mailboxItemTmpl',
                    {
                        email: accountName,
                        enabled: enabled,
                        isDefault: false,
                        oAuthConnection: oauth,
                        isTeamlabMailbox: isTeamlab,
                        aliases: []
                    }, { showSetDefaultIcon: showSetDefaultIcon }),
                $html = $(html);

            $html.actionMenu('accountActionMenu', buttons, pretreatment);
            $('#common_mailboxes').append($html);
            $html.find('.default_account_icon_block').on("click", setDefaultButtonClickEvent);

            if (addSetDefaultIcon) {
                $('.accounts_list .item-row').each(function () {
                    var $this = $(this),
                        html = $.tmpl('setDefaultIconItemTmpl', { isDefault: false });
                    if (!$this.children(":first").hasClass('default_account_button_column')) {
                        $this.prepend(html);
                        $(html).find('.set_as_default_account_icon').on("click", setDefaultButtonClickEvent);
                        setDefaultAccountIfItDoesNotExist();
                    }
                });
            }
        }
        if (TMMail.pageIs('accounts') && !checkEmpty()) {
            $page.show();
        }
    };

    var deleteAccount = function(id) {
        $page.find('tr[data_id="' + id + '"]').remove();
        if (checkEmpty() && TMMail.pageIs('accounts')) {
            $page.hide();
        }
        var removeSetDefaultIcon = accountsManager.getAccountList().length == 2;
        if (removeSetDefaultIcon) {
            $('.accounts_list .item-row').each(function () {
                var $this = $(this);
                $this.find('.default_account_icon_block').off("click");
                $this.find('.default_account_button_column').remove();
            });
        }
    };

    var activateAccount = function(accountName, isActivate) {
        var accountDiv = $page.find('tr[data_id="' + accountName + '"]');

        if (isActivate) {
            accountDiv.removeClass('disabled');
        } else {
            accountDiv.toggleClass('disabled', true);
        }
    };

    var pretreatment = function(id) {
        if ($page.find('tr[data_id="' + id + '"]').hasClass('disabled')) {
            $("#accountActionMenu .activateAccount").show();
            $("#accountActionMenu .deactivateAccount").hide();
        } else {
            $("#accountActionMenu .activateAccount").hide();
            $("#accountActionMenu .deactivateAccount").show();
        }

        var account = accountsManager.getAccountByAddress(id);
        if (account.is_teamlab) {
            $("#accountActionMenu .editAccount").hide();

            if (!account.is_shared_domain && !Teamlab.profile.isAdmin) {
                $("#accountActionMenu .deleteAccount").addClass('disable');
                $("#accountActionMenu .deleteAccount").attr('title', MailScriptResource.ServerMailboxNotificationText);
            } else {
                $("#accountActionMenu .deleteAccount").removeClass('disable');
                $("#accountActionMenu .deleteAccount").removeAttr('title');
            }
        } else {
            $("#accountActionMenu .editAccount").show();
            $("#accountActionMenu .deleteAccount").removeClass('disable');
            $("#accountActionMenu .deactivateAccount").removeAttr('title');
            $("#accountActionMenu .editAccount").removeAttr('title');
            $("#accountActionMenu .deleteAccount").removeAttr('title');
        }
    };

    var activate = function(id) {
        accountsModal.activateAccount(id, true);
    };

    var deactivate = function(id) {
        accountsModal.activateAccount(id, false);
    };

    var selectAttachmentsFolder = function(email) {
        var account = window.accountsManager.getAccountByAddress(email);
        ASC.Files.FileSelector.onSubmit = function(folderId) {
            serviceManager.setEMailInFolder(
                account.mailbox_id, folderId,
                { id: account.mailbox_id, emailInFolder: folderId, resetFolder: false },
                { error: onErrorSetEMailInFolder },
                ASC.Resources.Master.Resource.LoadingProcessing);
        };

        $('#filesFolderUnlinkButton').unbind('click').bind('click', { account: account }, unselectAttachmentsFolder);

        ASC.Files.FileSelector.fileSelectorTree.resetFolder();
        if (account.emailInFolder == null) {
            ASC.Files.FileSelector.openDialog(null, true);
            $('#filesFolderUnlinkButton').toggleClass('disable', true);
        } else {
            ASC.Files.FileSelector.openDialog(account.emailInFolder, true);
            $('#filesFolderUnlinkButton').toggleClass('disable', false);
        }
    };

    var unselectAttachmentsFolder = function(event) {
        if ($(this).hasClass('disable')) {
            return;
        }

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

    var editAccount = function(id) {
        accountsModal.editBox(id);
    };

    var removeAccount = function(id) {
        accountsModal.removeBox(id);
    };

    var isContain = function(accountName) {
        var account = $page.find('tr[data_id="' + accountName + '"]');
        return (account.length > 0);
    };

    var checkEmpty = function() {
        if ($page.find('.accounts_list tr').length) {
            $page.find('.accounts_list').show();
            blankPages.hide();
            return false;
        } else {
            blankPages.showEmptyAccounts();
            return true;
        }
    };

    function setDefaultButtonClickEvent() {
        setDefaultButtonClick($(this));
    }

    function setDefaultButtonClick($defaultAccountIcon) {
        var $this = $($defaultAccountIcon),
            $defaultAccountIconBlock = $('.default_account_icon_block'),
            accountName = $this.parent().parent().attr('data_id');

        if ($this.hasClass('set_as_default_account_icon')) {
            $defaultAccountIconBlock.prop('title', MailScriptResource.SetAsDefaultAccountText);
            $defaultAccountIconBlock.removeClass('default_account_icon');
            $defaultAccountIconBlock.addClass('set_as_default_account_icon');
            $this.prop('title', MailScriptResource.DefaultAccountText);
            $this.addClass('default_account_icon');
            $this.removeClass('set_as_default_account_icon');
            accountsModal.setDefaultAccount(accountName, true);
            accountsManager.setDefaultAccount(accountName, true);
        }
    }

    function setDefaultAccountIfItDoesNotExist() {
        if ($('.default_account_icon').length == 0) {
            var $defaultAccountIcon = $('.set_as_default_account_icon');
            if ($defaultAccountIcon.length != 0) {
                setDefaultButtonClick($defaultAccountIcon[0]);
            }
        }
    }

    function loadAccounts(accounts) {
        var commonMailboxes = [],
            serverMailboxes = [],
            aliases = [],
            groups = [],
            index, length;

        clear();

        for (index = 0, length = accounts.length; index < length; index++) {
            if (accounts[index].isGroup) {
                groups.push(accounts[index]);
            } else if (accounts[index].isAlias) {
                aliases.push(accounts[index]);
            } else if (accounts[index].isTeamlabMailbox) {
                serverMailboxes.push(accounts[index]);
            } else {
                commonMailboxes.push(accounts[index]);
            }
        }

        serverMailboxes.forEach(function(mailbox) {
            mailbox.aliases = [];
            for (index = 0, length = aliases.length; index < length; index++) {
                if (aliases[index].mailboxId == mailbox.mailboxId) {
                    mailbox.aliases.push(aliases[index]);
                    aliases[index].realEmail = mailbox.email;
                }
            }
        });

        var html = $.tmpl('accountsTmpl',
            {
                common_mailboxes: commonMailboxes,
                server_mailboxes: serverMailboxes,
                aliases: aliases,
                groups: groups,
                showSetDefaultIcon: commonMailboxes.length + serverMailboxes.length + aliases.length > 1
            });

        var $html = $(html);
        $('#id_accounts_page .containerBodyBlock .content-header').after($html);
        $('#id_accounts_page').actionMenu('accountActionMenu', buttons, pretreatment);
        $('.default_account_icon_block').on("click", setDefaultButtonClickEvent);
        serverMailboxes.forEach(function(mailbox) {
            if (mailbox.aliases.length > 1) {
                var items = [];
                for (index = 1, length = mailbox.aliases.length; index < length; index++) {
                    items.push({ 'text': mailbox.aliases[index].email, 'disabled': true });
                }
                $('#id_accounts_page').find('.row[data_id="' +
                    mailbox.email + '"] .more-aliases').actionPanel({ 'buttons': items });
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
        loadAccounts: loadAccounts,
        setDefaultAccountIfItDoesNotExist: setDefaultAccountIfItDoesNotExist
    };
})(jQuery);