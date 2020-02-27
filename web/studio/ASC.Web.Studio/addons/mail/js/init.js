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


(function($) {

    $(function() {
        TMMail.init();

        window.MailResource = ASC.Mail.Resources.MailResource;
        window.MailScriptResource = ASC.Mail.Resources.MailScriptResource;
        window.MailAttachmentsResource = ASC.Mail.Resources.MailAttachmentsResource;
        window.MailActionCompleteResource = ASC.Mail.Resources.MailActionCompleteResource;
        window.MailAdministrationResource = ASC.Mail.Resources.MailAdministrationResource;
        window.MailApiErrorsResource = ASC.Mail.Resources.MailApiErrorsResource;
        window.MailApiResource = ASC.Mail.Resources.MailApiResource;

        folderPanel.init();

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

        filterCache.init();
        mailBox.init();
        folderFilter.init();
        contactsPage.init();
        contactsPanel.init();
        commonSettingsPage.init();

        userFoldersManager.init();
        filtersManager.init();

        if (ASC.Mail.Presets.Accounts && ASC.Mail.Presets.Accounts.length) {
            contactsManager.init();
        }

        accountsPage.setDefaultAccountIfItDoesNotExist();

        $('#createNewMailBtn').click(function (e) {
                if (e.isPropagationStopped()) {
                    return;
                }
                createNewMail();
            });

        $('#check_email_btn').click(function (e) {
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
                serviceManager.updateFolders({}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
                mailAlerts.check();
            });

        $('#settingsLabel').click(function () {
                var $settingsPanel = $(this).parents('.menu-item.sub-list');
                if ($settingsPanel.hasClass('open')) {
                    $settingsPanel.removeClass('open');
                } else {
                    $settingsPanel.addClass('open');
                }
            });

        $('#addressBookLabel').click(function () {
                var $settingsPanel = $(this).parents('.menu-item.sub-list');
                if ($settingsPanel.hasClass('open')) {
                    $settingsPanel.removeClass('open');
                } else {
                    $settingsPanel.addClass('open');
                }
            });

        var foldersContainer = $('#foldersContainer');
        var messagesListGroupButtons = $('#MessagesListGroupButtons');

        foldersContainer.find('a[folderid="1"]').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "inbox");
        foldersContainer.find('a[folderid="2"]').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "sent");
        foldersContainer.find('a[folderid="3"]').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "drafts");
        foldersContainer.find('a[folderid="4"]').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "trash");
        foldersContainer.find('a[folderid="5"]').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "spam");
        foldersContainer.find('a[folderid="7"]').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "templates");
        $('#teamlab').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "teamlab-contacts");
        $('#crm').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "crm-contacts");
        $('#accountsSettings').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "accounts-settings");
        $('#tagsSettings').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "tags-settings");
        $('#foldersettings').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "folder-settings");
        $('#filtersettings').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "filter-settings");
        $('#commonSettings').trackEvent(ga_Categories.leftPanel, ga_Actions.quickAction, "common-settings");
        messagesListGroupButtons.find('.menuActionDelete').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "delete");
        messagesListGroupButtons.find('.menuActionSpam').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "spam");
        messagesListGroupButtons.find('.menuActionRead').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "read");
        messagesListGroupButtons.find('.menuActionImportant').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "important");
        messagesListGroupButtons.find('.menuActionNotSpam').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "not-spam");
        messagesListGroupButtons.find('.menuActionRestore').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "not-spam");
        messagesListGroupButtons.find('.menuActionMoveTo').trackEvent(ga_Categories.folder, ga_Actions.buttonClick, "move-to");

        mailBox.groupButtonsMenuHandlers();

        if (accountsManager.getAccountList().length > 0) {
            trustedAddresses.init();

            if (window.blankModal)
                window.blankModal.close();

        } else {
            if (window.blankModal && ASC.Controls.AnchorController.getAnchor().indexOf("help") !== 0)
                window.blankModal.show();
        }

        $(document).on("mousedown", function (e) {
                    if (e.ctrlKey) { // Dirty trick: hide FF blue selection of some tags
                        if (e.preventDefault)
                            e.preventDefault();
                        else
                            e.returnValue = false;
                    }
                });

        $(".mainPageTableSidePanel").on("resize", function(event, ui) {
            TMMail.resizeContent();
        });

        $(window).resize(function () {
            TMMail.resizeContent();
        });

        setupSelectable();
    });

    var createNewMail = function () {
        messagePage.compose();
    };

    function setupSelectable() {
        function deselectAll() {
            $(window).click(); // initiate global event for other dropdowns close

            if (TMMail.pageIs("sysfolders") || TMMail.pageIs("userfolder")) {
                mailBox.deselectAll();
            } else if (TMMail.pageIs("tlContact") ||
                TMMail.pageIs("crmContact") ||
                TMMail.pageIs("personalContact")) {
                contactsPage.deselectAll();
            }
        }

        function updateSelection() {
            if (TMMail.pageIs("sysfolders") || TMMail.pageIs("userfolder")) {
                mailBox.selectRow($(this));
            }
            else if (TMMail.pageIs("tlContact") || TMMail.pageIs("crmContact") || TMMail.pageIs("personalContact")) {
                contactsPage.selectRow($(this));
            }
        }

        $('#studioPageContent .mainPageContent')
            .selectable({
                delay: 150,
                filter: "tr.row",
                cancel:
                    "a, button, select, label, input, .checkbox, .importance, .from, .subject, .menuAction, .menu_column, " +
                        ".contentMenu, .entity-menu, .filterPanel, #id_accounts_page, #id_tags_page, #id_administration_page, " +
                        "#mailCommonSettings, #user_folders_page, #editMessagePage, .itemWrapper, #filtersEmptyScreen, " +
                        "#editFilterPage, #filtersContainer",
                start: deselectAll,
                selecting: function(event, ui) {
                    $(ui.selecting)
                        .each(updateSelection);
                },
                unselecting: function(event, ui) {
                    $(ui.unselecting)
                        .each(updateSelection);
                }
            });
    }
})(jQuery);