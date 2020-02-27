/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


window.editMailboxModal = (function($) {
    var mailbox,
        domain,
        needSaveAliases,
        needRemoveAliases,
        events = $({});

    function show(idMailbox) {
        needSaveAliases = [];
        needRemoveAliases = [];
        idMailbox = parseInt(idMailbox);

        mailbox = administrationManager.getMailbox(idMailbox);
        if (mailbox == undefined) {
            return;
        }

        domain = administrationManager.getDomain(mailbox.address.domainId);
        if (domain == undefined) {
            return;
        }

        var html = $.tmpl('editMailboxTmpl', { mailbox: mailbox, domain: domain });
        var $html = $(html);
        $html.find('.mailbox_aliases').toggleClass('empty_list', mailbox.aliases.length == 0);
        $html.find('.cancel').unbind('click').bind('click', window.PopupKeyUpActionProvider.CloseDialog);
        $html.find('.addAlias').unbind('click').bind('click', addAlias);
        $html.find('.delete_entity').unbind('click').bind('click', deleteAlias);
        
        $html.find('.save').unbind('click').bind('click', function() {
            saveInfo()
                .then(function() {
                        events.trigger('onupdatemailbox', mailbox);
                        window.LoadingBanner.hideLoading();
                        window.toastr.success(window.MailActionCompleteResource.updateMailboxSuccess.format(mailbox.address.email));
                    },
                    function (ev, error) {
                        events.trigger('onupdatemailbox', mailbox);
                        window.LoadingBanner.hideLoading();
                        administrationError.showErrorToastr("updateMailbox", error);
                    }
                );
        });

        popup.addBig(window.MailAdministrationResource.EditMailboxLabel, html, null, null, { focusInput: false });

        PopupKeyUpActionProvider.EnterAction = "jq('#mail_server_edit_mailbox:visible .addAlias').trigger('click');";

        setFocusToInput();
    }

    function saveInfo() {
        var dfd = jq.Deferred();

        TMMail.setRequiredHint('mailbox_add_alias', '');
        TMMail.setRequiredError('mailbox_add_alias', false);

        var senderName = $('#mailboxSenderName').find('.senderName').val().trim();
        var aliasName = $('#mail_server_edit_mailbox').find('.aliasName').val();

        if (aliasName.length > 0) {
            addAlias();
        }

        var functionArray = [];

        if (senderName != TMMail.htmlDecode(mailbox.name)) {
            functionArray.push(deferredUpdateMailbox(mailbox.id, senderName));
        }

        var i, len = needSaveAliases.length, alias;
        for (i = 0; i < len; i++) {
            alias = needSaveAliases[i];
            functionArray.push(deferredAddAlias(mailbox.id, alias));
        }

        len = needRemoveAliases.length;
        for (i = 0; i < len; i++) {
            alias = needRemoveAliases[i];
            if (alias.id > 0) {
                functionArray.push(deferredRemoveAlias(mailbox.id, alias));
            }
        }

        window.PopupKeyUpActionProvider.CloseDialog();

        if (functionArray.length > 0) {
            window.LoadingBanner.displayMailLoading();
            jq.when.apply(jq, functionArray).done(function () {
                dfd.resolve();
            })
            .fail(dfd.reject);
        } else {
            dfd.resolve();
        }

        return dfd.promise();
    }

    function deferredUpdateMailbox(id, senderName) {
        var dfd = jq.Deferred();
        serviceManager.updateMailbox(id, senderName, { mailbox_id: id, name: senderName },
            {
                success: function (params, serverMailbox) {
                    mailbox.name = params.name;
                    dfd.resolve(params, serverMailbox);
                },
                error: dfd.reject
            });

        return dfd.promise();
    }

    function deferredAddAlias(mailboxId, alias) {
        var dfd = jq.Deferred();
        serviceManager.addMailBoxAlias(mailboxId, alias.name, { mailbox_id: mailboxId, alias: alias },
            {
                success: function (params, serverAlias) {
                    mailbox.aliases.push(serverAlias);
                    dfd.resolve(params, serverAlias);
                },
                error: dfd.reject
            });
        return dfd.promise();
    }

    function deferredRemoveAlias(mailboxId, alias) {
        var dfd = jq.Deferred();
        serviceManager.removeMailBoxAlias(mailboxId, alias.id, { mailbox_id: mailboxId, alias: alias },
            {
                success: function (params, id) {
                    var aliasIndex = mailbox.aliases.indexOf(params.alias);
                    if (aliasIndex > -1) {
                        mailbox.aliases.splice(aliasIndex, 1);
                    }
                    dfd.resolve(params, id);
                },
                error: dfd.reject
            });
        return dfd.promise();
    }

    function deleteAlias() {
        var row = $(this).closest('.alias_row');
        var idAlias = row.attr('alias_id');
        var aliasEmail = row.find('.alias_address').text().trim();
        var pos;

        if (idAlias == -1) {
            pos = searchAliasIndex(needSaveAliases, aliasEmail);
            if (pos > -1) {
                needSaveAliases.splice(pos, 1);
            }
        } else {
            pos = searchAliasIndex(mailbox.aliases, aliasEmail);
            if (pos > -1) {
                needRemoveAliases.push(mailbox.aliases[pos]);
            }
        }

        $(this).closest('.mailbox_aliases').toggleClass('empty_list', needSaveAliases.length == 0 && mailbox.aliases.length == needRemoveAliases.length);

        row.remove();
        setFocusToInput();
    }

    function addAlias() {
        var domainName = domain.name;
        var aliasName = $('#mail_server_edit_mailbox').find('.aliasName').val();
        var aliasEmail = aliasName + '@' + domainName;

        var errorExists = false;

        if (aliasName.length === 0) {
            TMMail.setRequiredHint('mailbox_add_alias', window.MailScriptResource.ErrorEmptyField);
            errorExists = true;
        } else if (!ASC.Mail.Utility.IsValidEmail(aliasEmail)) {
            TMMail.setRequiredHint("mailbox_add_alias", window.MailScriptResource.ErrorIncorrectEmail);
            errorExists = true;
        } else {
            if (isAlreadyExists(aliasEmail)) {
                TMMail.setRequiredHint('mailbox_add_alias', window.MailResource.ErrorEmailExist);
                errorExists = true;
            }
        }

        if (errorExists) {
            TMMail.setRequiredError('mailbox_add_alias', true);
            return;
        }

        TMMail.setRequiredError('mailbox_add_alias', false);

        var alias = {
            domainId: domain.id,
            email: aliasEmail,
            name: aliasName,
            id: -1
        };

        var html = $.tmpl('mailboxAliasTableRowTmpl', alias);
        var $html = $(html);
        $html.find('.delete_entity').unbind('click').bind('click', deleteAlias);
        $('#mail_server_edit_mailbox').find('.mailbox_aliases table').append(html);
        needSaveAliases.push(alias);
        $('#mail_server_edit_mailbox').find('.aliasName').val('');
        $('#mail_server_edit_mailbox').find('.mailbox_aliases').toggleClass('empty_list', false);
        setFocusToInput();
    }

    function isAlreadyExists(aliasAddress) {
        return searchAliasIndex(mailbox.aliases, aliasAddress) != -1 ||
            searchAliasIndex(needSaveAliases, aliasAddress) != -1;
    }

    function searchAliasIndex(collection, aliasAddress) {
        var pos = -1;
        var i, len = collection.length;
        for (i = 0; i < len; i++) {
            var alias = collection[i];
            if (alias.email == aliasAddress) {
                pos = i;
                break;
            }
        }
        return pos;
    }

    function setFocusToInput() {
        $('#mail_server_edit_mailbox').find('.aliasName').focus();
    }

    return {
        show: show,
        events: events
    };

})(jQuery);