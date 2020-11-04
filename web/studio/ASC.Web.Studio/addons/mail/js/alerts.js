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


window.mailAlerts = (function($) {
    var lock = false,
        storedAlerts = [],
        timer = null;

    function check(options) {
        if (true === lock) {
            return;
        }

        if (!options)
            options = {}

        lock = true;
        serviceManager.getAlerts(options, { success: onGetAlerts });
    }

    function onGetAlerts(options, alerts) {
        lock = false;
        storeAlerts(alerts, options);
        showTopAlert();
    }

    function storeAlerts(alerts, options) {
        if (!alerts || 0 == alerts.length) {
            return;
        }
        storedAlerts = [];
        $.each(alerts, function (index, value) {

            if (options &&
                    options.showFailureOnlyMessageId &&
                    value.type === ASC.Mail.Constants.Alerts.DeliveryFailure) {
                var data = $.parseJSON(value.data);

                if (data.message_id === options.showFailureOnlyMessageId) {
                    storeAlert(value);
                }
            } else {
                storeAlert(value);
            }
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
            data = alert.data && $.parseJSON(alert.data),
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
                buttons = [];
                if (data.failure_id) {
                    buttons.push({ href: "#conversation/" + data.failure_id, text: MailScriptResource.MoreDetailsLabel, css_class: "blue tryagain" });
                } else {
                    buttons.push({ href: "#draftitem/" + data.message_id, text: MailScriptResource.TryAgainButton, css_class: "blue tryagain" });
                }

                buttons.push(cancelBtn);
                body = $($.tmpl("alertPopupBodyTmpl", {
                    errorBodyHeader: data.subject && data.subject.length > 0 ? MailScriptResource.DeliveryFailurePopupBodyHeader.replace(/{subject}/g, data.subject) : MailScriptResource.DeliveryFailurePopupBodyHeader.replace(/ "{subject}"/g, ''),
                    errorBody: MailScriptResource.DeliveryFailurePopupBody
                        .replace(/{account_name}/g, '<b>' + TMMail.htmlEncode(data.from) + '</b>'),
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
                content = content.replace(/{account}/g, '<b>' + TMMail.htmlEncode(accountEmail) + '</b>');
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
                    errorBody: window.MailScriptResource.AuthErrorPopupBody.format('<b>' + TMMail.htmlEncode(accountEmail) + '</b>', '<br><br>'),
                    errorBodyFooter: TMMail.getAccountErrorFooter(accountEmail),
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
                        .replace('{0}', '<b>' + TMMail.htmlEncode(accountEmail) + '</b>')
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