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

window.editMailboxModal = (function ($) {
    var mailbox,
        domain,
        need_save_aliases,
        need_remove_aliases;

    function show(id_mailbox) {
        need_save_aliases = [];
        need_remove_aliases = [];
        id_mailbox = parseInt(id_mailbox);

        mailbox = administrationManager.getMailbox(id_mailbox);
        if (mailbox == undefined) return;

        domain = administrationManager.getDomain(mailbox.address.domainId);
        if (domain == undefined) return;

        var html = $.tmpl('editMailboxTmpl', { mailbox: mailbox, domain: domain });
        var $html = $(html);
        $html.find('.mailbox_aliases').toggleClass('empty_list', mailbox.aliases.length == 0);

        $html.find('.addAlias').unbind('click').bind('click', addAlias);
        $html.find('.delete_entity').unbind('click').bind('click', deleteAlias);
        $html.find('.save').unbind('click').bind('click', saveAliases);
        $html.find('.cancel').unbind('click').bind('click', window.PopupKeyUpActionProvider.CloseDialog);

        popup.addPopup(window.MailAdministrationResource.EditMailboxAliasesLabel, html, '392px');

        $(document).unbind('keyup').bind('keyup', function (e) {
            if (e.which == 13) {
                if ($('#mail_server_edit_mailbox').is(':visible')) {
                    $('#mail_server_edit_mailbox .addAlias').trigger('click');
                }
            }
        });
        setFocusToInput();
    }

    function saveAliases() {
        var alias_name = $('#mail_server_edit_mailbox').find('.alias_name').val();
        if (alias_name.length > 0)
            addAlias();

        var i, len = need_save_aliases.length, alias;
        for (i = 0; i < len; i++) {
            alias = need_save_aliases[i];
            serviceManager.addMailBoxAlias(mailbox.id, alias.name, { mailbox_id: mailbox.id, alias: alias },
                {
                    error: function (e, error) {
                        administrationError.showErrorToastr("addMailboxAlias", error);
                    }
                });
        }

        len = need_remove_aliases.length;
        for (i = 0; i < len; i++) {
            alias = need_remove_aliases[i];
            if (alias.id > 0) {
                serviceManager.removeMailBoxAlias(mailbox.id, alias.id, { mailbox_id: mailbox.id, alias: alias },
                    {
                        error: function (e, error) {
                            administrationError.showErrorToastr("removeMailboxAlias", error);
                        }
                    });
            }
        }

        if (need_save_aliases.length > 0 || need_remove_aliases.length > 0)
            serviceManager.getMailboxes({}, { error: administrationError.getErrorHandler("getMailboxes") }, ASC.Resources.Master.Resource.LoadingProcessing);

        window.PopupKeyUpActionProvider.CloseDialog();
    }

    function deleteAlias() {
        var row = $(this).closest('.alias_row');
        var id_alias = row.attr('alias_id');
        var alias_email = row.find('.alias_address').text().trim();
        var pos;

        if (id_alias == -1) {
            pos = searchAliasIndex(need_save_aliases, alias_email);
            if (pos > -1)
                need_save_aliases.splice(pos, 1);
        } else {
            pos = searchAliasIndex(mailbox.aliases, alias_email);
            if (pos > -1)
                need_remove_aliases.push(mailbox.aliases[pos]);
        }

        $(this).closest('.mailbox_aliases').toggleClass('empty_list', need_save_aliases.length == 0 && mailbox.aliases.length == need_remove_aliases.length);

        row.remove();
        setFocusToInput();
    }

    function addAlias() {
        var domain_name = domain.name;
        var alias_name = $('#mail_server_edit_mailbox').find('.alias_name').val();
        var alias_email = alias_name + '@' + domain_name;

        var error_exists = false;

        if (alias_name.length === 0) {
            TMMail.setRequiredHint('mailbox_add_alias', window.MailScriptResource.ErrorEmptyField);
            error_exists = true;
        }
        else if (!TMMail.reMailServerEmailStrict.test(alias_email)) {
            TMMail.setRequiredHint("mailbox_add_alias", window.MailScriptResource.ErrorIncorrectEmail);
            error_exists = true;
        } else {
            if (isAlreadyExists(alias_email)) {
                TMMail.setRequiredHint('mailbox_add_alias', window.MailResource.ErrorEmailExist);
                error_exists = true;
            }
        }

        if (error_exists) {
            TMMail.setRequiredError('mailbox_add_alias', true);
            return;
        }

        TMMail.setRequiredError('mailbox_add_alias', false);

        var alias = {
            domainId: domain.id,
            email: alias_email,
            name: alias_name,
            id: -1
        };

        var html = $.tmpl('mailboxAliasTableRowTmpl', alias);
        var $html = $(html);
        $html.find('.delete_entity').unbind('click').bind('click', deleteAlias);
        $('#mail_server_edit_mailbox').find('.mailbox_aliases table').append(html);
        need_save_aliases.push(alias);
        $('#mail_server_edit_mailbox').find('.alias_name').val('');
        $('#mail_server_edit_mailbox').find('.mailbox_aliases').toggleClass('empty_list', false);
        setFocusToInput();
    }

    function isAlreadyExists(alias_address) {
        return searchAliasIndex(mailbox.aliases, alias_address) != -1 ||
            searchAliasIndex(need_save_aliases, alias_address) != -1;
    }

    function searchAliasIndex(collection, alias_address) {
        var pos = -1;
        var i, len = collection.length;
        for (i = 0; i < len; i++) {
            var alias = collection[i];
            if (alias.email == alias_address) {
                pos = i;
                break;
            }
        }
        return pos;
    }

    function setFocusToInput() {
        $('#mail_server_edit_mailbox').find('.alias_name').focus();
    }

    return {
        show: show
    };

})(jQuery);