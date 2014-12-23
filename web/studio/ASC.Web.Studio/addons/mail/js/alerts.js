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

window.mailAlerts = (function($) {
    var _lock = false;
    var _repeat = true;
    var _alerts = [];
    var cancel_btn = {};
    var close_btn = {};
    var ok_btn = {};

    var init = function() {
        _check();

        cancel_btn = { text: MailScriptResource.CancelBtnLabel, css_class: "gray cancel" };
        close_btn = { text: MailScriptResource.CloseBtnLabel, css_class: "gray cancel" };
        ok_btn = { text: MailScriptResource.OkBtnLabel, css_class: "blue cancel" };
    };

    var _unlock = function() {
        _lock = false;
        if (_repeat)
            setTimeout(function() {
                _repeat = true;
                _check();
            }, 180000);
    };

    var check = function() {
        _repeat = false;
        _check();
    };

    var _check = function() {
        if (true === _lock)
            return;
        _lock = true;
        serviceManager.getAlerts({}, { success: _onGetAlerts });
    };

    var _onGetAlerts = function(options, alerts) {
        $.each(alerts, function(index, value) {
            _storeAlert(value);
        });
        if (0 == _alerts.length) {
            _unlock();
            return;
        }
        $.each(_alerts, function(index, value) {
            if (_alerts.length - 1 != index) {
                popup.addBig(value.header, value.body, function() {
                    _deleteAlert(value.id);
                });
            } else {
                popup.addBig(value.header, value.body, function() {
                    _deleteAlert(value.id);
                    _unlock();
                });
            }
        });
        _alerts = [];
    };

    var _storeAlert = function(alert) {
        var header;
        var body;
        var data = $.parseJSON(alert.data);
        var content;
        var buttons;
        switch (alert.type) {
            case 1:
                header = MailScriptResource.DeliveryFailurePopupHeader;
                buttons = [{ href: "#draftitem/" + data.message_id, text: MailScriptResource.TryAgainButton, css_class: "blue tryagain" }, cancel_btn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: MailScriptResource.DeliveryFailurePopupBodyHeader.replace(/{subject}/g, data.subject),
                    errorBody: MailScriptResource.DeliveryFailurePopupBody
                        .replace(/{account_name}/g, '<b>' + data.from + '</b>'),
                    errorBodyFooter: MailScriptResource.DeliveryFailurePopupBodyFooter
                        .replace(/{faq_link_open_tag}/g, "<a class=\"linkDescribe\" target=\"blank\" href=\"" + TMMail.getFaqLink(data.from) + "\">")
                        .replace(/{faq_link_close_tag}/g, "</a>"),
                    buttons: buttons
                }));
                body.find('.tryagain').click(function() { popup.hide(); });
                break;
            case 2:
                header = MailScriptResource.LinkFailurePopupHeader;
                buttons = [ok_btn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: MailScriptResource.LinkFailurePopupName,
                    errorBody: MailScriptResource.LinkFailurePopupText,
                    errorBodyFooter: undefined,
                    buttons: buttons
                }));
                break;
            case 3:
                header = MailScriptResource.ExportFailurePopupHeader;
                buttons = [ok_btn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: MailScriptResource.ExportFailurePopupName,
                    errorBody: MailScriptResource.ExportFailurePopupText,
                    errorBodyFooter: undefined,
                    buttons: buttons
                }));
                break;
            case 4:
                header = MailScriptResource.EmailInFailurePopupHeader;
                var account = accountsManager.getAccountById(alert.id_mailbox);
                var account_email = account ? account.email : "";
                switch(data.error_type) {
                    case 1: // folder not found
                        content = MailScriptResource.EmailInFolderNotFoundFailurePopupText;
                        break;
                    case 2: // no access rights
                        content = MailScriptResource.EmailInFolderAccessRightsFailurePopupText;
                        break;
                    default:
                        return;
                }
                content = content.replace(/{account}/g, '<b>' + account_email + '</b>');
                buttons = [ok_btn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: MailScriptResource.EmailInFailurePopupBodyHeader,
                    errorBody: content,
                    errorBodyFooter: undefined,
                    buttons: buttons
                }));
                break;
            case 5:
                header = MailScriptResource.DisableAllMailboxesPopupHeader;
                buttons = [{ href: "#accounts", text: MailScriptResource.ManageAccountsLabel, css_class: "blue manage_accounts" }, close_btn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: MailScriptResource.DisableAllMailboxesPopupBodyHeader,
                    errorBody: MailScriptResource.DisableAllMailboxesPopupText,
                    errorBodyFooter: undefined,
                    buttons: buttons
                }));
                body.find('.manage_accounts').click(function () { popup.hide(); });
                break;
            case 6:
                header = window.MailScriptResource.AccountCreationErrorHeader;
                var account = accountsManager.getAccountById(alert.id_mailbox);
                var account_email = account ? account.email : "";
                buttons = [{ href: "#accounts", text: MailScriptResource.ChangeAccountSettingsBtn, css_class: "blue manage_account_settings" }, close_btn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: window.MailScriptResource.AuthErrorPopupBodyHeader,
                    errorBody: window.MailScriptResource.AuthErrorPopupBody
                        .replace('{0}', '<b>' + account_email + '</b>')
                        .replace('{1}', '<br><br>'),
                    errorBodyFooter: window.MailScriptResource.AuthErrorPopupBodyFooter
                        .replace('{2}', '<a class=\"linkDescribe\" target=\"blank\" href="' + TMMail.getFaqLink(account_email) + '">')
                        .replace('{3}', '</a>'),
                    buttons: buttons
                }));
                body.find('.manage_account_settings').click(function () { popup.hide(); accountsModal.editBox(account.email); });
                break;
            case 7:
                header = window.MailScriptResource.AuthErrorDisablePopupHeader;
                var account = accountsManager.getAccountById(alert.id_mailbox);
                var account_email = account ? account.email : "";
                window.accountsManager.enableMailbox(account.email, false);
                buttons = [{ href: "#accounts", text: MailScriptResource.ManageAccountsLabel, css_class: "blue manage_accounts" }, close_btn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: window.MailScriptResource.AuthErrorDisablePopupBodyHeader,
                    errorBody: window.MailScriptResource.AuthErrorDisablePopupBody
                        .replace('{0}', '<b>' + account_email + '</b>')
                        .replace('{1}', '<br><br>'),
                    errorBodyFooter: undefined,
                    buttons: buttons
                }));
                body.find('.manage_accounts').click(function () { popup.hide(); });
                break;
        };
        if (header && body) {
            _alerts.push({ header: header, body: body, id: alert.id });
        }
    };

    var _deleteAlert = function(id) {
        serviceManager.deleteAlert(id);
    };

    return {
        init: init,
        check: check
    };
})(jQuery);