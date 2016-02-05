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


window.mailAlerts = (function($) {
    var lock = false,
        storedAlerts = [],
        timer = null;

    function check() {
        if (true === lock) {
            return;
        }
        lock = true;
        serviceManager.getAlerts({}, { success: onGetAlerts });
    }

    function onGetAlerts(options, alerts) {
        lock = false;
        storeAlerts(alerts);
        showTopAlert();
    }

    function storeAlerts(alerts) {
        if (!alerts || 0 == alerts.length) {
            return;
        }
        storedAlerts = [];
        $.each(alerts, function(index, value) {
            storeAlert(value);
        });
    }

    function showTopAlert() {
        if (!storedAlerts || 0 == storedAlerts.length) {
            return;
        }

        //do not show alerts on print page
        if (TMMail.pageIs('print')) {
            clearTimeout(timer);
            return;
        }

        if (TMMail.isPopupVisible()) {
            clearTimeout(timer);
            timer = setTimeout(showTopAlert, TMMail.showNextAlertTimeout);
            return;
        }

        popup.addBig(storedAlerts[0].header, storedAlerts[0].body, function() {
            var alert = storedAlerts[0].alert;
            deleteAlert(alert.id);
            storedAlerts.splice(0, 1);
            clearTimeout(timer);
            timer = setTimeout(showTopAlert, TMMail.showNextAlertTimeout);
        });

    }

    function storeAlert(alert) {
        var alertPopup = getAlertPopup(alert);
        if (alertPopup) {
            storedAlerts.push(alertPopup);
        }
    }

    function getAlertPopup(alert) {
        var header,
            body,
            data = $.parseJSON(alert.data),
            content,
            buttons,
            account,
            accountEmail,
            cancelBtn = { text: MailScriptResource.CancelBtnLabel, css_class: "gray cancel" },
            closeBtn = { text: MailScriptResource.CloseBtnLabel, css_class: "gray cancel" },
            okBtn = { text: MailScriptResource.OkBtnLabel, css_class: "blue cancel" };

        switch (alert.type) {
            case ASC.Mail.Constants.Alerts.DeliveryFailure:
                header = MailScriptResource.DeliveryFailurePopupHeader;
                buttons = [{ href: "#draftitem/" + data.message_id, text: MailScriptResource.TryAgainButton, css_class: "blue tryagain" }, cancelBtn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: data.subject.Length > 0 ? MailScriptResource.DeliveryFailurePopupBodyHeader.replace(/{subject}/g, data.subject) : MailScriptResource.DeliveryFailurePopupBodyHeader.replace(/ "{subject}"/g, ''),
                    errorBody: MailScriptResource.DeliveryFailurePopupBody
                        .replace(/{account_name}/g, '<b>' + data.from + '</b>'),
                    errorBodyFooter: MailScriptResource.DeliveryFailurePopupBodyFooter
                        .replace(/{faq_link_open_tag}/g, "<a class=\"linkDescribe\" target=\"blank\" href=\"" + TMMail.getFaqLink(data.from) + "\">")
                        .replace(/{faq_link_close_tag}/g, "</a>"),
                    buttons: buttons
                }));
                body.find('.tryagain').click(function () { popup.hide(); });
                break;
            case ASC.Mail.Constants.Alerts.LinkFailure:
                header = MailScriptResource.LinkFailurePopupHeader;
                buttons = [okBtn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: MailScriptResource.LinkFailurePopupName,
                    errorBody: MailScriptResource.LinkFailurePopupText,
                    errorBodyFooter: undefined,
                    buttons: buttons
                }));
                break;
            case ASC.Mail.Constants.Alerts.ExportFailure:
                header = MailScriptResource.ExportFailurePopupHeader;
                buttons = [okBtn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: MailScriptResource.ExportFailurePopupName,
                    errorBody: MailScriptResource.ExportFailurePopupText,
                    errorBodyFooter: undefined,
                    buttons: buttons
                }));
                break;
            case ASC.Mail.Constants.Alerts.UploadFailure:
                account = accountsManager.getAccountById(alert.id_mailbox);
                if (!account) {
                    deleteAlert(alert.id);
                    break;
                }
                header = MailScriptResource.EmailInFailurePopupHeader;
                accountEmail = account ? account.email : "";
                switch (data.error_type) {
                    case 1:
                        // folder not found
                        content = MailScriptResource.EmailInFolderNotFoundFailurePopupText;
                        break;
                    case 2:
                        // no access rights
                        content = MailScriptResource.EmailInFolderAccessRightsFailurePopupText;
                        break;
                    default:
                        return null;
                }
                content = content.replace(/{account}/g, '<b>' + accountEmail + '</b>');
                buttons = [okBtn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: MailScriptResource.EmailInFailurePopupBodyHeader,
                    errorBody: content,
                    errorBodyFooter: undefined,
                    buttons: buttons
                }));
                break;
            case ASC.Mail.Constants.Alerts.DisableAllMailboxes:
                header = MailScriptResource.DisableAllMailboxesPopupHeader;
                buttons = [{ href: "#accounts", text: MailScriptResource.ManageAccountsLabel, css_class: "blue manage_accounts" }, closeBtn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: MailScriptResource.DisableAllMailboxesPopupBodyHeader,
                    errorBody: TMMail.htmlEncode(MailScriptResource.DisableAllMailboxesPopupText),
                    errorBodyFooter: undefined,
                    buttons: buttons
                }));
                body.find('.manage_accounts').click(function () { popup.hide(); });
                break;
            case ASC.Mail.Constants.Alerts.AuthConnectFailure:
                account = accountsManager.getAccountById(alert.id_mailbox);
                if (!account) {
                    deleteAlert(alert.id);
                    break;
                }
                header = window.MailScriptResource.AccountCreationErrorHeader;
                accountEmail = account ? account.email : "";
                buttons = [{ href: "#accounts", text: MailScriptResource.ChangeAccountSettingsBtn, css_class: "blue manage_account_settings" }, closeBtn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: window.MailScriptResource.AuthErrorPopupBodyHeader,
                    errorBody: window.MailScriptResource.AuthErrorPopupBody
                        .replace('{0}', '<b>' + accountEmail + '</b>')
                        .replace('{1}', '<br><br>'),
                    errorBodyFooter: window.MailScriptResource.AuthErrorPopupBodyFooter
                        .replace('{2}', '<a class=\"linkDescribe\" target=\"blank\" href="' + TMMail.getFaqLink(accountEmail) + '">')
                        .replace('{3}', '</a>'),
                    buttons: buttons
                }));

                if (!alert.redirectToAccounts)
                    body.find('.manage_account_settings').removeAttr('href');

                body.find('.manage_account_settings').click(function () {
                    popup.hide();
                    accountsModal.editBox(account.email, alert.activateOnSuccess);
                });
                break;
            case ASC.Mail.Constants.Alerts.TooManyAuthError:
                account = accountsManager.getAccountById(alert.id_mailbox);
                if (!account) {
                    deleteAlert(alert.id);
                    break;
                }
                header = window.MailScriptResource.AuthErrorDisablePopupHeader;
                accountEmail = account ? account.email : "";
                window.accountsManager.enableMailbox(account.email, false);
                buttons = [{ href: "#accounts", text: MailScriptResource.ManageAccountsLabel, css_class: "blue manage_accounts" }, closeBtn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: window.MailScriptResource.AuthErrorDisablePopupBodyHeader,
                    errorBody: window.MailScriptResource.AuthErrorDisablePopupBody
                        .replace('{0}', '<b>' + accountEmail + '</b>')
                        .replace('{1}', '<br><br>'),
                    errorBodyFooter: undefined,
                    buttons: buttons
                }));
                body.find('.manage_accounts').click(function () { popup.hide(); });
                break;
            case ASC.Mail.Constants.Alerts.QuotaError:
                header = window.MailScriptResource.QuotaPopupHeader;
                buttons = [closeBtn];
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: window.MailScriptResource.QuotaPopupHeader,
                    errorBody: window.MailScriptResource.QuotaPopupBody,
                    errorBodyFooter: undefined,
                    buttons: buttons
                }));
                break;
        }
        
        if (header && body) {
            return { header: header, body: body, alert: alert };
        }

        return null;
    }

    function showAlert(alert) {
        var alertPopup = getAlertPopup(alert);
        if (alertPopup) {
            popup.addBig(alertPopup.header, alertPopup.body);
        }
    }

    function deleteAlert(id) {
        serviceManager.deleteAlert(id);
    }

    return {
        check: check,
        showAlert: showAlert
    };
})(jQuery);