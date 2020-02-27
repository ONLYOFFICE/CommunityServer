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


window.accountsPanel = (function($) {
    var isInit = false,
        $panel,
        $content,
        $more,
        maxHeight,
        $list;

    // Should be initialized after AccountsManager - to allow it handle accounts changes primarily.
    // Reason: Account list generate with AccountsManager
    var init = function() {
        if (isInit === false) {
            isInit = true;

            $panel = $('#accountsPanel');
            $content = $panel.find('> .content');
            $list = $content.find('> ul');
            $more = $panel.find('.more');

            maxHeight = $content.css("max-height").replace(/[^-\d\.]/g, '');
            $panel.hover(expand, collapse);

            window.Teamlab.bind(window.Teamlab.events.getAccounts, update);
            window.Teamlab.bind(window.Teamlab.events.removeMailMailbox, update);
            window.Teamlab.bind(window.Teamlab.events.updateMailMailbox, update);
            window.Teamlab.bind(window.Teamlab.events.setMailMailboxState, update);
            window.Teamlab.bind(window.Teamlab.events.createMailMailboxSimple, update);
            window.Teamlab.bind(window.Teamlab.events.createMailMailboxOAuth, update);
            window.Teamlab.bind(window.Teamlab.events.updateMailMailboxOAuth, update);
            window.Teamlab.bind(window.Teamlab.events.createMailMailbox, update);
        }
    };

    var expand = function() {
        $content.stop().animate({ "max-height": $list.height() }, 200, function() {
            $more.css({ 'visibility': 'hidden' });
        });
    };

    var collapse = function() {
        $content.stop().animate({ "max-height": maxHeight }, 200, function() {
            $more.css({ 'visibility': 'visible' });
        });
    };

    var renderAccountsPanelTmpl = function (index, acc) {
        $list.append($.tmpl("accountsPanelTmpl", acc));
    };

    function update() {
        var accounts = accountsManager.getAccountList();

        if (accounts.length < 2) {
            $panel.hide();
            return;
        }

        var active = getActive();

        var defaultAccount = [],
            commonAccounts = [],
            serverAccounts = [],
            aliases = [],
            groups = [],
            selected = active ? active.email : '';

        $.each(accounts, function (index, acc) {
            var marked = selected == acc.email.toLowerCase(),
                obj = {
                    email: acc.email,
                    id: TMMail.strHash(acc.email.toLowerCase()),
                    marked: marked,
                    enabled: acc.enabled
                };
            if (acc.is_default) {
                defaultAccount.push(obj);
            } else if (acc.is_group) {
                groups.push(obj);
            } else if (acc.is_alias) {
                aliases.push(obj);
            } else if (acc.is_teamlab) {
                serverAccounts.push(obj);
            } else {
                commonAccounts.push(obj);
            }
        });
        $list.empty();
        $.each(defaultAccount, renderAccountsPanelTmpl);
        $.each(commonAccounts, renderAccountsPanelTmpl);
        $.each(serverAccounts, renderAccountsPanelTmpl);
        $.each(aliases, renderAccountsPanelTmpl);
        $.each(groups, renderAccountsPanelTmpl);

        updateAnchors();
        accountsPage.setDefaultAccountIfItDoesNotExist();

        window.setImmediate(function() {
            $panel.show();
            if (maxHeight < $list.height()) {
                $more.show();
            } else {
                $more.hide();
            }
        });
    }

    // unselect selected account row

    function unmark() {
        if (!TMMail.pageIs('writemessage')) {
            $list.find('.tag.tagArrow').removeClass('tag tagArrow');
        }
    }

    // select account if any is in filter

    function mark(from) {
        var filterTo = from || MailFilter.getTo();
        if (undefined == filterTo) {
            unmark();
            return;
        }

        // skip action if account allready marked
        var id = TMMail.strHash(filterTo.toLowerCase());
        var current = $list.find('.tag.tagArrow');
        if (id == current.prop('id')) {
            return;
        }

        current.removeClass('tag tagArrow');
        $list.find('#' + id).addClass('tag tagArrow');
    }

    function getActive() {
        var current = $list.find('.tag.tagArrow');
        var accountEmail = current.text();

        var id = TMMail.strHash(accountEmail.toLowerCase());
        if (id != current.prop('id')) {
            return undefined;
        }

        var activeAccount = accountsManager.getAccountByAddress(accountEmail);
        if (!activeAccount) {
            return undefined;
        }

        return activeAccount;
    }

    function updateAnchors() {
        var accounts = accountsManager.getAccountList();
        if (accounts.length < 2) {
            return;
        }

        $('#accountsPanel .accounts a').unbind('click.accountsPanel').bind('click.accountsPanel', function() {
            var accountEmail = $(this).text();
            var account = accountsManager.getAccountByAddress(accountEmail);
            if (!account) {
                return;
            }

            if (TMMail.pageIs('sysfolders')) {
                var filterTo = (MailFilter.getTo() || '').toLowerCase();
                var folder = TMMail.getSysFolderNameById(MailFilter.getFolder());
                var sysfolderAnchor = folder +
                    (TMMail.pageIs("userfolder") ? "=" + TMMail.extractUserFolderIdFromAnchor() : "") +
                     MailFilter.toAnchor(false, { to: filterTo == account.email.toLowerCase() ? '' : account.email }, true);

                ASC.Controls.AnchorController.move(sysfolderAnchor);
            } else if (TMMail.pageIs('writemessage')) {
                messagePage.selectFromAccount({}, {
                    account: account
                });
                TMMail.scrollTop();
            } else {
                ASC.Controls.AnchorController.move('#inbox/to=' + encodeURIComponent(account.email) + '/');
            }
        });
    }

    return {
        init: init,
        update: update,
        unmark: unmark,
        mark: mark,
        updateAnchors: updateAnchors,
        getActive: getActive
    };
})(jQuery);