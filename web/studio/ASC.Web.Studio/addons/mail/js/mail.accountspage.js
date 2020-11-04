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


window.accountsPage = (function($) {
    var isInit = false,
        $page,
        $header,
        buttons = [];

    function init() {
        if (isInit === false) {
            isInit = true;

            $page = $('#id_accounts_page');
            
            $header = $('#pageActionContainer');

            $header.on('click', '#createNewMailbox', function () {
                accountsModal.addMailbox();
            });

            $header.on('click', '#createNewAccount', function () {
                accountsModal.addBox();
            });
            
            $header.on('click', '#accountsHelpCenterSwitcher', function (e) {
                jq('#accountsHelpCenterSwitcher').helper({ BlockHelperID: 'AccountsHelperBlock' });
                e.preventDefault();
                e.stopPropagation();
            });

            buttons = [
                { selector: "#accountActionMenu .activateAccount", handler: activate },
                { selector: "#accountActionMenu .deactivateAccount", handler: deactivate },
                { selector: "#accountActionMenu .selectAttachmentsFolder", handler: selectAttachmentsFolder },
                { selector: "#accountActionMenu .setMailAutoreply", handler: setMailAutoreply },
                { selector: "#accountActionMenu .editAccount", handler: editAccount },
                { selector: "#accountActionMenu .deleteAccount", handler: removeAccount },
                { selector: "#accountActionMenu .changePassword", handler: changePassword },
                { selector: "#accountActionMenu .viewAccountSettings", handler: viewMailboxSettings }];

            var additionalButtonHtml = $.tmpl('fileSelectorMailAdditionalButton');
            $('#fileSelectorAdditionalButton').replaceWith(additionalButtonHtml);
        }
    }

    function show() {
        if (checkEmpty()) {
            hide();
        } else {
            $header.html($.tmpl('accountsPageHeaderTmpl'));
            $page.show();
        }
    }

    function hide() {
        $page.hide();
        $header.empty();
    }

    function clear() {
        var accountsRows = $page.find('.accounts_list');
        $('#accountActionMenu').hide();
        if (accountsRows) {
            accountsRows.remove();
        }
    }

    function refreshAccount(accountName, isActivate) {
        var account = accountsManager.getAccountByAddress(accountName);
        if (!account)
            return;

        var accountListLength = accountsManager.getAccountList().length,
            showSetDefaultIcon = accountListLength > 1,
            tmplName = 'mailboxItemTmpl';

        if (account.is_alias)
            tmplName = 'aliasItemTmpl';
        else if (account.is_group)
            return;

        var showEnabledAutoreply = accountsManager.isAutoreplyEnabled(account);

        var html = $.tmpl(tmplName,
        {
            email: account.email,
            enabled: isActivate,
            autoreply: account.autoreply,
            isDefault: account.is_default,
            oAuthConnection: account.oauth,
            isTeamlabMailbox: account.is_teamlab,
            aliases: [],
            mailboxId: account.mailbox_id,
            showEnabledAutoreply: showEnabledAutoreply
        },
        { showSetDefaultIcon: showSetDefaultIcon });

        var $html = $(html);

        $html.actionMenu('accountActionMenu', buttons, pretreatment);

        var accountDiv = $page.find('tr[data_id="' + accountName + '"]');
        if (!accountDiv)
            return;

        accountDiv.replaceWith($html);

        $html.find('.default_account_icon_block').on("click", setDefaultButtonClickEvent);

        if (showSetDefaultIcon) {
            $('.accounts_list .item-row').each(function() {
                var $this = $(this),
                    html = $.tmpl('setDefaultIconItemTmpl', { isDefault: false });
                if (!$this.children(":first").hasClass('default_account_button_column')) {
                    $this.prepend(html);
                    $(html).find('.set_as_default_account_icon').on("click", setDefaultButtonClickEvent);
                }
            });
        }
    }

    function addAccount(accountName, autoreply, enabled, oauth, isTeamlab) {
        accountName = accountName.toLowerCase();
        if (!isContain(accountName)) {
            var accountListLength = accountsManager.getAccountList().length,
                showSetDefaultIcon = accountListLength > 1,
                addSetDefaultIcon = accountListLength === 1;

            var account = {
                email: accountName,
                enabled: enabled,
                isDefault: false,
                oAuthConnection: oauth,
                isTeamlabMailbox: isTeamlab,
                autoreply: autoreply,
                aliases: [],
                showEnabledAutoreply: false
            };

            account.showEnabledAutoreply = accountsManager.isAutoreplyEnabled(account);

            var html = $.tmpl('mailboxItemTmpl',
                account,
                {
                    showSetDefaultIcon: showSetDefaultIcon
                });

            var $html = $(html);

            $html.actionMenu('accountActionMenu', buttons, pretreatment);
            $('#common_mailboxes').append($html);
            $html.find('.default_account_icon_block').on("click", setDefaultButtonClickEvent);

            if (addSetDefaultIcon) {
                $('.accounts_list .item-row').each(function () {
                    var $this = $(this);
                    if (!$this.children(":first").hasClass('default_account_button_column')) {
                        var html = $.tmpl('setDefaultIconItemTmpl', { isDefault: false });
                        $this.prepend(html);
                        $(html).find('.set_as_default_account_icon').on("click", setDefaultButtonClickEvent);
                    }
                });
            }
        }
        if (TMMail.pageIs('accounts') && !checkEmpty()) {
            $header.html($.tmpl('accountsPageHeaderTmpl'));
            $page.show();
        }
    }

    function deleteAccount(id) {
        $page.find('tr[data_id="' + id + '"]').remove();
        if (checkEmpty() && TMMail.pageIs('accounts')) {
            hide();
        }
        var removeSetDefaultIcon = accountsManager.getAccountList().length === 2;
        if (removeSetDefaultIcon) {
            $('.accounts_list .item-row').each(function () {
                var $this = $(this);
                $this.find('.default_account_icon_block').off("click");
                $this.find('.default_account_button_column').remove();
            });
        }
    }

    function activateAccount(accountName, isActivate) {
        refreshAccount(accountName, isActivate);
    }

    function pretreatment(id) {
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

            if (!account.is_shared_domain && ASC.Resources.Master.Standalone) {
                $("#accountActionMenu .changePassword").show();
                $("#accountActionMenu .viewAccountSettings").show();
            } else {
                $("#accountActionMenu .changePassword").hide();
                $("#accountActionMenu .viewAccountSettings").hide();
            }

            if (!account.is_shared_domain && !Teamlab.profile.isAdmin && !ASC.Resources.Master.IsProductAdmin) {
                $("#accountActionMenu .deleteAccount").addClass('disable');
                $("#accountActionMenu .deleteAccount").attr('title', MailScriptResource.ServerMailboxNotificationText);
            } else {
                $("#accountActionMenu .deleteAccount").removeClass('disable');
                $("#accountActionMenu .deleteAccount").removeAttr('title');
            }
        } else {
            $("#accountActionMenu .changePassword").hide();
            $("#accountActionMenu .viewAccountSettings").hide();

            $("#accountActionMenu .editAccount").show();
            $("#accountActionMenu .deleteAccount").removeClass('disable');
            $("#accountActionMenu .deactivateAccount").removeAttr('title');
            $("#accountActionMenu .editAccount").removeAttr('title');
            $("#accountActionMenu .deleteAccount").removeAttr('title');
        }
    }

    function activate(id) {
        accountsModal.activateAccount(id, true);
    }

    function deactivate(id) {
        accountsModal.activateAccount(id, false);
    }

    function selectAttachmentsFolder(email) {
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
            $('#filesFolderUnlinkButton').show().toggleClass('disable', true);
        } else {
            ASC.Files.FileSelector.openDialog(account.emailInFolder, true);
            $('#filesFolderUnlinkButton').show().toggleClass('disable', false);
        }
    }

    function unselectAttachmentsFolder(event) {
        if ($(this).hasClass('disable')) {
            return;
        }

        var account = event.data.account;
        serviceManager.setEMailInFolder(
            account.mailbox_id, null,
            { id: account.mailbox_id, emailInFolder: null, resetFolder: true },
            { error: onErrorResetEMailInFolder },
            ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function setMailAutoreply(event) {
        var account = accountsManager.getAccountByAddress(event),
            html = $.tmpl("mailAutoreplyTmpl", {
                turnOn: account.autoreply.turnOn,
                onlyContacts: account.autoreply.onlyContacts,
                turnOnToDate: account.autoreply.turnOnToDate,
                subject: account.autoreply.subject
            }),
            config = {
                toolbar: 'MailSignature',
                removePlugins: 'resize, magicline',
                filebrowserUploadUrl: 'fckuploader.ashx?newEditor=true&esid=mail',
                height: 200,
                startupFocus: true
            };

        ckeditorConnector.load(function () {
            var editor = html.find('#ckMailAutoreplyEditor').ckeditor(config).editor;
            editor.setData(account.autoreply.html);
        });

        popup.addBig(window.MailScriptResource.MailAutoreplyLabel, html, undefined, false, { bindEvents: false });

        var $autoreplyStartDate = $('#autoreplyStartDate'),
            $autoreplyDueDate = $('#autoreplyDueDate'),
            $mailAutoreplyFromDate = $('.mail_autoreply_from_date'),
            $mailAutoreplyToDate = $('.mail_autoreply_to_date'),
            $turnOnToDateFlag = $('#turnOnToDateFlag');

        html.find('.buttons .ok').unbind('click').bind('click', function () {
            if ($('#ckMailAutoreplyEditor').val() == "") {
                ShowRequiredError($('#MailAutoreplyWYSIWYGEditor'));
            } else {
                RemoveRequiredErrorClass($('#MailAutoreplyWYSIWYGEditor'));
            }
            if (!$mailAutoreplyFromDate.hasClass('requiredFieldError') &&
                !$mailAutoreplyToDate.hasClass('requiredFieldError') &&
                !$('.mail_autoreply_body').hasClass('requiredFieldError')) {
                updateAutoreply(account);
                return false;
            }
        });

        $turnOnToDateFlag.off('change').on('change', function () {
            var $this = $(this);
            if ($this[0].checked) {
                $autoreplyDueDate.datepicker('option', 'disabled', false);
                changeAutoreplyDueDate($autoreplyDueDate.val());
            } else {
                $autoreplyDueDate.datepicker('option', 'disabled', true);
                $mailAutoreplyToDate.removeClass('requiredFieldError');
                checkDate($.trim($autoreplyStartDate.val()), $mailAutoreplyFromDate);
            }
        });

        $autoreplyStartDate.mask(ASC.Resources.Master.DatePatternJQ);
        $autoreplyDueDate.mask(ASC.Resources.Master.DatePatternJQ);
        $autoreplyStartDate.off('change').on('change', function () {
            var fromDateString = $.trim($(this).val()),
                toDateString = $.trim($('#autoreplyDueDate').val()),
                fromDate = $autoreplyStartDate.datepicker('getDate'),
                toDate = $autoreplyDueDate.datepicker('getDate');

            checkDate($.trim($(this).val()), $mailAutoreplyFromDate);

            if ($.isDateFormat(fromDateString) && $.isDateFormat(toDateString)) {
                if (fromDate > toDate && $turnOnToDateFlag[0].checked) {
                    $mailAutoreplyFromDate.addClass('requiredFieldError');
                } else {
                    $mailAutoreplyToDate.removeClass('requiredFieldError');
                    $mailAutoreplyFromDate.removeClass('requiredFieldError');
                }
            }
        });
        $autoreplyStartDate.keyup(function () {
            checkDate($.trim($(this).val()), $mailAutoreplyFromDate);
        });

        $autoreplyDueDate.off('change').on('change', function () { changeAutoreplyDueDate($(this).val()); });
        $autoreplyDueDate.keyup(function () {
            var fromDateString = $.trim($('#autoreplyStartDate').val()),
                toDateString = $.trim($(this).val());
            if (toDateString != '') {
                checkDate(toDateString, $mailAutoreplyToDate);
            } else {
                $mailAutoreplyToDate.removeClass('requiredFieldError');
                if (fromDateString != '') {
                    checkDate(fromDateString, $mailAutoreplyFromDate);
                }
            }
        });
        $autoreplyStartDate.datepicker({ minDate: 0 });
        $autoreplyDueDate.datepicker({ minDate: 0 });

        var fromDate, fromDateUtc;

        if ($.trim(account.autoreply.toDate) != '' && $.trim(account.autoreply.fromDate) != '') {
            fromDate = new Date(account.autoreply.fromDate);
            fromDateUtc = new Date(fromDate.getFullYear(), fromDate.getMonth(), fromDate.getDate());
            var toDate = new Date(account.autoreply.toDate),
                toDateUtc = new Date(toDate.getFullYear(), toDate.getMonth(), toDate.getDate());
            if (fromDateUtc.getYear() === 1 && fromDateUtc.getMonth() === 0 && fromDateUtc.getDate() === 1 &&
                toDateUtc.getYear() === 1 && toDateUtc.getMonth() === 0 && toDateUtc.getDate() === 1) {
                toDateUtc = new Date();
                toDateUtc.setDate(toDateUtc.getDate() + 7);
            }
            $autoreplyDueDate.datepicker('setDate', toDateUtc);
            if ((fromDateUtc.getYear() !== 1 || fromDateUtc.getMonth() !== 0 || fromDateUtc.getDate() !== 1) &&
                toDateUtc.getYear() === 1 && toDateUtc.getMonth() === 0 && toDateUtc.getDate() === 1) {
                $autoreplyDueDate.datepicker('setDate', '');
            }
        }
        if (!$turnOnToDateFlag[0].checked) {
            $autoreplyDueDate.datepicker("option", "disabled", true);
        }
        if ($.trim(account.autoreply.fromDate) != '') {
            fromDate = new Date(account.autoreply.fromDate);
            fromDateUtc = new Date(fromDate.getFullYear(), fromDate.getMonth(), fromDate.getDate());
            if (fromDateUtc.getYear() === 1 && fromDateUtc.getMonth() === 0 && fromDateUtc.getDate() === 1) {
                fromDateUtc = new Date();
            }
            $autoreplyStartDate.datepicker('setDate', fromDateUtc);
        }
    }

    function changeAutoreplyDueDate(toDateStr) {
        var $autoreplyStartDate = $('#autoreplyStartDate'),
            $autoreplyDueDate = $('#autoreplyDueDate'),
            $mailAutoreplyFromDate = $('.mail_autoreply_from_date'),
            $mailAutoreplyToDate = $('.mail_autoreply_to_date'),
            fromDateString = $.trim($autoreplyStartDate.val()),
            toDateString = $.trim(toDateStr),
            fromDate = $autoreplyStartDate.datepicker('getDate'),
            toDate = $autoreplyDueDate.datepicker('getDate');
        if (toDateString != '') {
            checkDate(toDateString, $mailAutoreplyToDate);
            if ($.isDateFormat(fromDateString) && $.isDateFormat(toDateString)) {
                if (fromDate > toDate) {
                    $mailAutoreplyToDate.addClass('requiredFieldError');
                } else {
                    $mailAutoreplyToDate.removeClass('requiredFieldError');
                    $mailAutoreplyFromDate.removeClass('requiredFieldError');
                }
            }
        } else {
            $mailAutoreplyToDate.removeClass('requiredFieldError');
            if (fromDateString != "") {
                checkDate(fromDateString, $mailAutoreplyFromDate);
            }
        }
    }

    function checkDate(dateString, mailAutoreplyDate) {
        var $mailAutoreplyDate = $(mailAutoreplyDate);
        if ($.isDateFormat(dateString)) {
            $mailAutoreplyDate.removeClass('requiredFieldError');
            return true;
        } else {
            $mailAutoreplyDate.addClass('requiredFieldError');
            return false;
        }
    }

    function turnAutoreply(email, turnOn) {
        var account = accountsManager.getAccountByAddress(email);
        account.turnOn = turnOn;
        serviceManager.updateMailboxAutoreply(account.mailbox_id, account.turnOn, account.autoreply.onlyContacts,
            account.autoreply.turnOnToDate, account.autoreply.fromDate, account.autoreply.toDate,
            account.autoreply.subject, account.autoreply.html, { id: account.mailbox_id },
            { error: window.accountsModal.hideLoader }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function updateAutoreply(account) {
        var turnOn = $('#turnAutoreplyFlag')[0].checked,
            onlyContacts = $('#onlyContactsFlag')[0].checked,
            turnOnToDate = $('#turnOnToDateFlag')[0].checked,
            fromDate = $('#autoreplyStartDate').datepicker('getDate'),
            toDate = $('#autoreplyDueDate').datepicker('getDate'),
            subject = $('#autoreplySubject').val(),
            html = $('#ckMailAutoreplyEditor').val();

        fromDate = Teamlab.serializeTimestamp(fromDate);
        toDate = Teamlab.serializeTimestamp(toDate);
        serviceManager.updateMailboxAutoreply(account.mailbox_id, turnOn, onlyContacts,
            turnOnToDate, fromDate, toDate, subject, html, { id: account.mailbox_id },
            { error: window.accountsModal.hideLoader }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function onErrorResetEMailInFolder() {
        window.toastr.error(window.MailScriptResource.ResetAccountEMailInFolderFailure);
    }

    function onErrorSetEMailInFolder() {
        window.toastr.error(window.MailScriptResource.SetAccountEMailInFolderFailure);
    }

    function editAccount(id) {
        accountsModal.editBox(id);
    }

    function removeAccount(id) {
        accountsModal.removeBox(id);
    }

    function changePassword(address) {
        var account = accountsManager.getAccountByAddress(address);

        if (!account)
            return;

        accountsModal.changePassword(account.email, account.mailbox_id);
    }

    function viewMailboxSettings(address) {
        var account = accountsManager.getAccountByAddress(address);

        if (!account)
            return;

        accountsModal.viewMailboxSettings(account);
    }

    function isContain(accountName) {
        var account = $page.find('tr[data_id="' + accountName + '"]');
        return (account.length > 0);
    }

    function checkEmpty() {
        if ($page.find('.accounts_list tr').length) {
            $page.find('.accounts_list').show();
            blankPages.hide();
            return false;
        } else {
            blankPages.showEmptyAccounts();
            return true;
        }
    }

    function setDefaultButtonClickEvent() {
        var $this = $(this),
            accountName = $this.parent().parent().attr('data_id'),
            account = accountsManager.getAccountByAddress(accountName);

        if (!account)
            return;

        if (!account.enabled) {
            function setDefailt() {
                $('#id_accounts_page .accounts_list .row[data_id="' + accountName + '"] .set_as_default_account_icon').click();
            }

            accountsModal.activateAccount(accountName, true, setDefailt);
        } else
            setDefaultButtonClick($this);
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
            toastr.success(MailScriptResource.DefaultAccountText + " <b>{0}</b>".format(accountName));
        }
    }

    function setDefaultAccountIfItDoesNotExist() {
        if ($('.default_account_icon').length === 0) {
            var $defaultAccountIcon = $('.set_as_default_account_icon'),
                email = undefined,
                account = undefined;
            if ($defaultAccountIcon.length > 0) {
                for (var i = 0; i < $defaultAccountIcon.length; i++) {
                    email = $($defaultAccountIcon[i]).parent().parent('.item-row').attr('data_id');
                    account = accountsManager.getAccountByAddress(email);
                    if (account.enabled) {
                        setDefaultButtonClick($defaultAccountIcon[i]);
                        return;
                    }
                }
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
            var account = accounts[index];

            if (accounts[index].isGroup) {
                groups.push(account);
            } else if (accounts[index].isAlias) {
                account.showEnabledAutoreply = accountsManager.isAutoreplyEnabled(account);
                aliases.push(account);
            } else if (accounts[index].isTeamlabMailbox) {
                account.showEnabledAutoreply = accountsManager.isAutoreplyEnabled(account);
                serverMailboxes.push(account);
            } else {
                account.showEnabledAutoreply = accountsManager.isAutoreplyEnabled(account);
                commonMailboxes.push(account);
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
        $('#id_accounts_page .containerBodyBlock').html($html);
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
        turnAutoreply: turnAutoreply,
        isContain: isContain,
        clear: clear,
        loadAccounts: loadAccounts,
        setDefaultAccountIfItDoesNotExist: setDefaultAccountIfItDoesNotExist
    };
})(jQuery);