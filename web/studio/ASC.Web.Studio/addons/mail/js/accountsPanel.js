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

window.accountsPanel = (function ($) {
    var 
        is_init = false,
        $panel,
        $content,
        $more,
        max_height,
        $list;

    // Should be initialized after AccountsManager - to allow it handle accounts changes primarily.
    // Reason: Account list generate with AccountsManager
    var init = function () {
        if (is_init === false) {
            is_init = true;

            $panel = $('#accountsPanel');
            $content = $panel.find('> .content');
            $list = $content.find('> ul');
            $more = $panel.find('.more');

            max_height = $content.css("max-height").replace(/[^-\d\.]/g, '');
            $panel.hover(expand, collapse);

            serviceManager.bind(window.Teamlab.events.getAccounts, update);
            serviceManager.bind(window.Teamlab.events.removeMailMailbox, update);
            serviceManager.bind(window.Teamlab.events.updateMailMailbox, update);
            serviceManager.bind(window.Teamlab.events.setMailMailboxState, update);
            serviceManager.bind(window.Teamlab.events.createMailMailboxSimple, update);
            serviceManager.bind(window.Teamlab.events.createMailMailboxOAuth, update);
            serviceManager.bind(window.Teamlab.events.createMailMailbox, update);
        }
    };

    var expand = function() {
        $content.stop().animate({ "max-height": $list.height() }, 200, function() {
            $more.css({ 'visibility': 'hidden' });
        });
    };

    var collapse = function() {
        $content.stop().animate({ "max-height": max_height }, 200, function() {
            $more.css({ 'visibility': 'visible' });
        });
    };

    function update() {
        var accounts = accountsManager.getAccountList();

        if (accounts.length < 2) {
            $panel.hide();
            return;
        }

        var info = [];
        var selected = (MailFilter.getTo() || '').toLowerCase();
        var folder_page = TMMail.pageIs('sysfolders');

        $.each(accounts, function (index, acc) {
            var marked = selected == acc.email.toLowerCase() && folder_page;
            info.push({
                email: acc.email,
                id: TMMail.strHash(acc.email.toLowerCase()),
                marked: marked
            });
        });

        $list.html($.tmpl("accountsPanelTmpl", info));
        updateAnchors();

        window.setImmediate(function () {
            $panel.show();
            if (max_height < $list.height())
                $more.show();
            else
                $more.hide();
        });
    };

    // unselect selected account row
    function unmark() {
        if(!TMMail.pageIs('writemessage'))
            $list.find('.tag.tagArrow').removeClass('tag tagArrow');
    };

    // select account if any is in filter
    function mark(from){
        var filter_to = MailFilter.getTo() || from;
        if (undefined == filter_to) {
            unmark();
            return;
        }

        // skip action if account allready marked
        var id = TMMail.strHash(filter_to.toLowerCase());
        var current = $list.find('.tag.tagArrow');
        if(id == current.prop('id'))
            return;

        current.removeClass('tag tagArrow');
        $list.find('#'+id).addClass('tag tagArrow');
    };

    function getActive() {
        var current = $list.find('.tag.tagArrow');
        var account_email = current.text();

        var id = TMMail.strHash(account_email.toLowerCase());
        if (id != current.prop('id'))
            return undefined;

        var active_account = accountsManager.getAccountByAddress(account_email);
        if (!active_account) return undefined;

        return active_account;
    }

    function updateAnchors(){
        var accounts = accountsManager.getAccountList();
        if (accounts.length < 2)
            return;

        $('#accountsPanel .accounts a').unbind('click.accountsPanel').bind('click.accountsPanel', function () {
            var account_email = $(this).text();
            var account = accountsManager.getAccountByAddress(account_email);
            if (!account) return;

            if (TMMail.pageIs('sysfolders')) {
                var folder = TMMail.GetSysFolderNameById(MailFilter.getFolder());
                var filter_to = (MailFilter.getTo() || '').toLowerCase();
                var sysfolder_anchor = '#' + folder + MailFilter.toAnchor(false, { to: filter_to == account.email.toLowerCase() ? '' : account.email }, true);
                ASC.Controls.AnchorController.move(sysfolder_anchor);
            }
            else if (TMMail.pageIs('writemessage')) {
                var title = TMMail.ltgt(account.name + " <" + account.email + ">");
                messagePage.selectFromAccount({}, {
                    mailbox_email: account.email,
                    title: title,
                    account_enabled: account.enabled,
                    signature: account.signature
                });
            } else {
                ASC.Controls.AnchorController.move('#inbox/to=' + encodeURIComponent(account.email) + '/');
            }

        });
    };

    return {
        init: init,
        unmark: unmark,
        mark: mark,
        updateAnchors: updateAnchors,
        getActive: getActive
    };

})(jQuery);
