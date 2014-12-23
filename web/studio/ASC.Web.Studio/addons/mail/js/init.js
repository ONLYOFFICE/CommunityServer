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

(function($) {
    var addNewMailbox = (function() {
        if ($.browser.msie && $.browser.version < 8) {
            return function($textfield, mailbox) {
                if ($textfield.length > 0) {
                    if ($textfield.is('input')) {
                        $textfield.val(mailbox);
                    } else if ($textfield.is('textarea')) {
                        var val = $textfield.val().split(',');
                        val[val.length - 1] = mailbox;
                        $textfield.val(val.join(',') + ', ');
                        $textfield.focus();
                    }
                }
            };
        }
        return function($textfield, mailbox) {
            if ($textfield.length > 0) {
                if ($textfield.is('input')) {
                    $textfield.val(mailbox);
                } else if ($textfield.is('textarea')) {
                    var val = $textfield.val().split(/\s*,\s*/);
                    val[val.length - 1] = mailbox;
                    $textfield.val(val.join(', ') + ', ');
                    $textfield.focus();
                }
            }
        };
    })();

    function _onGetMailFolders() {
        filterCache.init();
        mailBox.init();
        folderFilter.init();
        contactsPage.init();
        contactsPanel.init();
        contactsManager.init();
        CrmLinkPopup.init();

        var current_anchor = ASC.Controls.AnchorController.getAnchor();
        ASC.Controls.AnchorController.move(current_anchor);

        $('#createNewMailBtn').click(function(e) {
            if (e.isPropagationStopped())
                return;
            CreateNewMail();
        });

        $('#check_email_btn').click(function(e) {
            if (e.isPropagationStopped())
                return;

            if (!accountsManager.any()) return;

            if (!TMMail.pageIs('inbox')) {
                mailBox.unmarkAllPanels();
                ASC.Controls.AnchorController.move(TMMail.sysfolders.inbox.name);
            }
            serviceManager.updateFolders(ASC.Resources.Master.Resource.LoadingProcessing);
        });

        $('#settingsLabel').click(function() {
            var $settingsPanel = $(this).parents('.menu-item.sub-list');
            if ($settingsPanel.hasClass('open'))
                $settingsPanel.removeClass('open');
            else $settingsPanel.addClass('open');
        });

        $('#foldersContainer').find('a[folderid="1"]').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "inbox");
        $('#foldersContainer').find('a[folderid="2"]').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "sent");
        $('#foldersContainer').find('a[folderid="3"]').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "drafts");
        $('#foldersContainer').find('a[folderid="4"]').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "trash");
        $('#foldersContainer').find('a[folderid="5"]').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "spam");
        $('#teamlab').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "teamlab-contacts");
        $('#crm').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "crm-contacts");
        $('#accountsSettings').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "accounts-settings");
        $('#tagsSettings').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "tags-settings");
        $('#MessagesListGroupButtons .menuActionDelete').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "delete");
        $('#MessagesListGroupButtons .menuActionSpam').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "spam");
        $('#MessagesListGroupButtons .menuActionRead').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "read");
        $('#MessagesListGroupButtons .menuActionNotSpam').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "not-spam");
        $('#MessagesListGroupButtons .menuActionRestore').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "not-spam");


        mailBox.groupButtonsMenuHandlers();

        if (accountsManager.getAccountList().length > 0 && window.blankModal != undefined)
            window.blankModal.close();
    }

    $(function () {
        folderPanel.init();

        TMMail.checkAnchor();

        TMMail.init(service_ckeck_time, crm_available, tl_available);
        window.MailResource = ASC.Mail.Resources.MailResource;
        window.MailScriptResource = ASC.Mail.Resources.MailScriptResource;
        window.MailAttachmentsResource = ASC.Mail.Resources.MailAttachmentsResource;
        window.MailActionCompleteResource = ASC.Mail.Resources.MailActionCompleteResource;
        window.MailAdministrationResource = ASC.Mail.Resources.MailAdministrationResource;
        window.MailApiErrorsResource = ASC.Mail.Resources.MailApiErrorsResource;

        blankPages.init();
        popup.init();

        MailFilter.init();
        tagsManager.init();
        accountsManager.init();
        settingsPanel.init();
        helpPanel.init();
        helpPage.init();
        accountsPanel.init();
        administrationManager.init();
        trustedAddresses.init();

        window.Teamlab.getMailTags();
        window.Teamlab.getAccounts();
        window.Teamlab.getMailFolders({}, TMMail.messages_modify_date, {
            success: _onGetMailFolders,
            error: function () {
                window.toastr.error(TMMail.getErrorMessage([window.MailScriptResource.ErrorNotification]));
        }});

        // alerts should be initialized after full initialization
        // because alert extended text may need some additional info: ex. account name
        mailAlerts.init();
    });

    var CreateNewMail = function() {
        var last_folder_id = MailFilter.getFolder();

        if (last_folder_id < 1 ||
                last_folder_id == TMMail.sysfolders.sent.id ||
                last_folder_id == TMMail.sysfolders.drafts.id ||
                mailBox._Selection.Count() == 0) {
            messagePage.compose();
        }
        else {
            var selected_addresses = mailBox.getMessagesAddresses();
            messagePage.setToEmailAddresses(selected_addresses);
            messagePage.composeTo();
        }
    };
})(jQuery); 