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


(function($) {

    function onGetMailFolders() {
        filterCache.init();
        mailBox.init();
        folderFilter.init();
        contactsPage.init();
        contactsPanel.init();
        contactsManager.init();
        CrmLinkPopup.init();

        accountsPage.setDefaultAccountIfItDoesNotExist();

        var currentAnchor = ASC.Controls.AnchorController.getAnchor();
        ASC.Controls.AnchorController.move(currentAnchor);

        $('#createNewMailBtn').click(function(e) {
            if (e.isPropagationStopped()) {
                return;
            }
            createNewMail();
        });

        $('#check_email_btn').click(function(e) {
            if (e.isPropagationStopped()) {
                return;
            }

            if (!accountsManager.any()) {
                return;
            }

            if (!TMMail.pageIs('inbox')) {
                mailBox.unmarkAllPanels();
                ASC.Controls.AnchorController.move(TMMail.sysfolders.inbox.name);
            }
            serviceManager.updateFolders(ASC.Resources.Master.Resource.LoadingProcessing);
            mailAlerts.check();
        });

        $('#settingsLabel').click(function() {
            var $settingsPanel = $(this).parents('.menu-item.sub-list');
            if ($settingsPanel.hasClass('open')) {
                $settingsPanel.removeClass('open');
            } else {
                $settingsPanel.addClass('open');
            }
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

        if (accountsManager.getAccountList().length > 0 && window.blankModal != undefined) {
            window.blankModal.close();
        }
    }

    $(function() {
        folderPanel.init();

        TMMail.checkAnchor();

        TMMail.init(crm_available, tl_available);
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

        window.Teamlab.getMailFolders({}, TMMail.messages_modify_date, {
            success: onGetMailFolders,
            error: function() {
                window.toastr.error(TMMail.getErrorMessage([window.MailScriptResource.ErrorNotification]));
            }
        });

        mailAlerts.check();
    });

    var createNewMail = function() {
        var lastFolderId = MailFilter.getFolder();

        if (lastFolderId < 1 ||
            lastFolderId == TMMail.sysfolders.sent.id ||
            lastFolderId == TMMail.sysfolders.drafts.id ||
            mailBox._Selection.Count() == 0) {
            messagePage.compose();
        } else {
            var selectedAddresses = mailBox.getMessagesAddresses();
            messagePage.setToEmailAddresses(selectedAddresses);
            messagePage.composeTo();
        }
    };
})(jQuery);