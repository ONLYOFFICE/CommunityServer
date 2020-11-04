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


(function($) {

    $(function () {
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

        $(".mainPageTableSidePanel").on("resize", function (event, ui) {
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

        if(!jq.browser.mobile)
        {
            $('main')
                .selectable({
                    delay: 150,
                    filter: ".row",
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
    }
})(jQuery);