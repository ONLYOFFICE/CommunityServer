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
            $content = $panel.find('#accounts_panel_content');
            $list = $content.find('> ul');
            $more = $panel.find('.more');

            maxHeight = $content.css("max-height").replace(/[^-\d\.]/g, '');

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

    function expandAccountsPanel() {
        $content.animate({ "max-height": $content[0].scrollHeight }, 200, function () {
            $more.hide();
        });
    }

    var renderAccountsPanelTmpl = function (index, acc) {
        $list.append($.tmpl("accountsPanelTmpl", acc));
    };

    function update() {
        var accounts = accountsManager.getAccountList();

        if (accounts.length < 2) {
            $panel.hide();
            return;
        }
        else {
            $panel.show();
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

        if (maxHeight < $content[0].scrollHeight) {
            $more.show();
            $more.find(".more_link").on("click", expandAccountsPanel);
        } else {
            $more.find(".more_link").off("click", expandAccountsPanel);
            $more.hide();
        }
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