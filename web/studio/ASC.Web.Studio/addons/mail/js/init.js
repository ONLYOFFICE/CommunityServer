/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

        $('#createNewMailBtn').on("click", function (e) {
            if (e.isPropagationStopped()) {
                return;
            }
            createNewMail();
        });

        $('#check_email_btn').on("click", function (e) {
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
            serviceManager.updateFolders({}, {}, ASC.Resources.Master.ResourceJS.LoadingProcessing);
            mailAlerts.check();
        });

        $('#settingsLabel').on("click", function () {
            var $settingsPanel = $(this).parents('.menu-item.sub-list');
            if ($settingsPanel.hasClass('open')) {
                $settingsPanel.removeClass('open');
            } else {
                $settingsPanel.addClass('open');
            }
        });

        $('#addressBookLabel').on("click", function () {
            var $settingsPanel = $(this).parents('.menu-item.sub-list');
            if ($settingsPanel.hasClass('open')) {
                $settingsPanel.removeClass('open');
            } else {
                $settingsPanel.addClass('open');
            }
        });

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

        $(window).on("resize", function () {
            TMMail.resizeContent();
        });

        setupSelectable();
    });

    var createNewMail = function () {
        messagePage.compose();
    };

    function setupSelectable() {
        var minVisibleY = 0;

        function deselectAll() {
            $(window).trigger("click"); // initiate global event for other dropdowns close

            if (TMMail.pageIs("sysfolders") || TMMail.pageIs("userfolder")) {
                mailBox.deselectAll();
            } else if (TMMail.pageIs("tlContact") ||
                TMMail.pageIs("crmContact") ||
                TMMail.pageIs("personalContact")) {
                contactsPage.deselectAll();
            }

            minVisibleY = $(".mainPageContent").offset().top;
        }

        function updateSelection() {
            var $row = $(this);
            var middle = $row.offset().top + this.offsetHeight / 2;
            if (middle < minVisibleY) {
                return;
            }
            if (TMMail.pageIs("sysfolders") || TMMail.pageIs("userfolder")) {
                mailBox.selectRow($row);
            }
            else if (TMMail.pageIs("tlContact") || TMMail.pageIs("crmContact") || TMMail.pageIs("personalContact")) {
                contactsPage.selectRow($row);
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