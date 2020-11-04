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


window.blankPages = (function($) {
    var page,
        mailBoxContainer;

    var init = function() {
        page = $('#blankPage'),
        mailBoxContainer = $("#mailBoxContainer");
    };

    function showEmptyAccounts() {
        var buttons = [{
                text: MailScriptResource.EmptyScrAccountsButton,
                cssClass: "addFirstElement",
                handler: function () {
                    ASC.Controls.AnchorController.move('#accounts');
                    accountsModal.addBox();
                    return false;
                },
                href: null
            }];

        if (ASC.Mail.Constants.COMMON_DOMAIN_AVAILABLE)
            buttons.push({
                text: MailScriptResource.CreateMailboxBtn,
                cssClass: "addFirstElement",
                handler: function () {
                    ASC.Controls.AnchorController.move('#accounts');
                    accountsModal.addMailbox();
                    return false;
                },
                href: null
            });

        showPage(
            "accountsEmptyScreen",
            MailScriptResource.EmptyScrAccountsHeader,
            MailScriptResource.EmptyScrAccountsDescription,
            'accounts',
            buttons
        );
    }

    function showEmptyTags() {
        var buttons = [{
            text: MailScriptResource.EmptyScrTagsButton,
            cssClass: "addFirstElement",
            handler: function() {
                tagsModal.showCreate();
                return false;
            },
            href: null
        }];

        showPage(
            "tagsEmptyScreen",
            MailScriptResource.EmptyScrTagsHeader,
            MailScriptResource.EmptyScrTagsDescription,
            'tags',
            buttons
        );
    }

    function showEmptyUserFolders() {
        var buttons = [{
            text: MailScriptResource.EmptyScrUserFoldersButton,
            cssClass: "addFirstElement",
            handler: function () {
                userFoldersPage.createFolder();
                return false;
            },
            href: null
        }];

        showPage(
            "userFoldersEmptyScreen",
            MailScriptResource.EmptyScrUserFoldersHeader,
            MailScriptResource.EmptyScrUserFoldersDescription,
            'userfolder',
            buttons
        );
    }

    function showEmptyFilters() {
        var buttons = [{
            text: MailScriptResource.EmptyScrFiltersButton,
            cssClass: "addFirstElement",
            handler: function () {
                filtersPage.createFilter();
                return false;
            },
            href: null
        }];

        showPage(
            "filtersEmptyScreen",
            MailScriptResource.EmptyScrFiltersHeader,
            MailScriptResource.EmptyScrFiltersDescription,
            'userFilter',
            buttons
        );
    }

    function showNoLettersFilter() {
        var buttons = [{
            text: MailScriptResource.ResetFilterButton,
            cssClass: "clearFilterButton",
            handler: function() {
                folderFilter.reset();
                return false;
            },
            href: null
        }];

        showPage(
            "filterNoLettersEmptyScreen",
            MailScriptResource.NoLettersFilterHeader,
            MailScriptResource.NoLettersFilterDescription,
            'filter',
            buttons
        );
    }

    function showEmptyFolder() {
        var header = null;
        var description = null;
        var imgClass = null;

        var buttons = [{
            text: null,
            cssClass: "addFirstElement",
            handler: function () {
                messagePage.compose();
                return false;
            },
            href: null
        }];

        if (TMMail.pageIs('inbox')) {
            header = MailScriptResource.EmptyInboxHeader;
            description = MailScriptResource.EmptyInboxDescription;
            imgClass = 'inbox';
            buttons[0].text = MailScriptResource.EmptyInboxButton;
        } else if (TMMail.pageIs('sent')) {
            header = MailScriptResource.EmptySentHeader;
            description = MailScriptResource.EmptySentDescription;
            imgClass = 'sent';
            buttons[0].text = MailScriptResource.EmptySentButton;
        } else if (TMMail.pageIs('drafts')) {
            header = MailScriptResource.EmptyDraftsHeader;
            description = MailScriptResource.EmptyDraftsDescription;
            imgClass = 'drafts';
            buttons[0].text = MailScriptResource.EmptyDraftsButton;
        } else if (TMMail.pageIs('templates')) {
            header = MailScriptResource.EmptyTemplatesHeader;
            description = MailScriptResource.EmptyTemplatesDescription;
            imgClass = 'drafts';
            buttons[0].text = MailScriptResource.EmptyTemplatesButton;
        } else if (TMMail.pageIs('trash')) {
            header = MailScriptResource.EmptyTrashHeader;
            description = MailScriptResource.EmptyTrashDescription;
            imgClass = 'trash';
            buttons = [];
        } else if (TMMail.pageIs('spam')) {
            header = MailScriptResource.EmptySpamHeader;
            description = MailScriptResource.EmptySpamDescription;
            imgClass = 'spam';
            buttons = [];
        } else if (TMMail.pageIs('userfolder')) { 
            header = MailScriptResource.EmptyUserFolderHeader; 
            description = MailScriptResource.EmptyUserFolderDescription;
            imgClass = 'inbox'; // TODO: Change to userfolder
            buttons = [];
        }

        showPage("folderEmptyScreen", header, description, imgClass, buttons);
    }

    function showEmptyCrmContacts() {
        var buttons = [{
            text: MailScriptResource.EmptyScrCrmButton,
            cssClass: "addFirstElement",
            handler: null,
            href: "/Products/CRM/"
        }];

        showPage(
            "crmContactsEmptyScreen",
            MailScriptResource.EmptyScrCrmHeader,
            MailScriptResource.EmptyScrCrmDescription,
            'contacts',
            buttons
        );
    }

    function showEmptyMailContacts() {
        var buttons = [{
            text: MailScriptResource.CreateContactButton,
            cssClass: "addFirstElement",
            handler: function () {
                editContactModal.show(null, true);
                return false;
            },
            href: null
        }];

        showPage(
            "mailContactsEmptyScreen",
            MailScriptResource.EmptyScrCrmHeader,
            MailScriptResource.EmptyScrMailContactsDescription,
            'contacts',
            buttons
        );
    }

    function showNoCrmContacts() {
        var buttons = [{
            text: MailScriptResource.ResetFilterButton,
            cssClass: "clearFilterButton",
            handler: function () {
                contactsPage.resetFilter();
                return false;
            },
            href: null
        }];

        showPage(
            "filterNoCrmContactsEmptyScreen",
            MailScriptResource.ResetCrmContactsFilterHeader,
            MailScriptResource.ResetCrmContactsFilterDescription,
            'filter',
            buttons
        );
    }

    function showNoTlContacts() {
        var buttons = [{
            text: MailScriptResource.ResetFilterButton,
            cssClass: "clearFilterButton",
            handler: function () {
                contactsPage.resetFilter();
                return false;
            },
            href: null
        }];

        showPage(
            "filterNoTlContactsEmptyScreen",
            MailScriptResource.ResetTlContactsFilterHeader,
            MailScriptResource.ResetTlContactsFilterDescription,
            'filter',
            buttons
        );
    }

    function showNoMailContacts() {
        var buttons = [{
            text: MailScriptResource.ResetFilterButton,
            cssClass: "clearFilterButton",
            handler: function () {
                contactsPage.resetFilter();
                return false;
            },
            href: null
        }];

        showPage(
            "filterNoMailContactsEmptyScreen",
            MailScriptResource.ResetMailContactsFilterHeader,
            MailScriptResource.ResetMailContactsFilterDescription,
            'filter',
            buttons
        );
    }

    function showNoMailDomains() {
        var buttons = [{
            text: window.MailAdministrationResource.NoDomainSetUpButton,
            cssClass: "addFirstElement",
            handler: function () {
                createDomainModal.show(administrationManager.getServerInfo());
                return false;
            },
            href: null
        }];

        if (ASC.Mail.Constants.MS_MIGRATION_LINK_AVAILABLE) {
            buttons.push(
            {
                text: window.MailAdministrationResource.MigrateFromMSExchangeButton,
                cssClass: "linkMseFaq",
                handler: null,
                href: ASC.Resources.Master.HelpLink + "/server/docker/enterprise/migrate-from-exchange.aspx",
                skipNewLine: true,
                openNewTab: true
            });
        }

        showPage(
            "domainsEmptyScreen",
            window.MailAdministrationResource.NoDomainSetUpHeader,
            window.MailAdministrationResource.NoDomainSetUpDescription,
            'domains',
            buttons
        );
    }

    //buttons = [{ text: "", cssClass: "", handler: function(){}, href: "" }]
    function showPage(id, header, description, imgClass, buttons) {

        var buttonHtml = undefined;

        if (buttons) {
            var tmpl = $.template("emptyScrButtonTmpl");
            buttonHtml = tmpl($, {data : { buttons: buttons }});
        }

        var screen = $.tmpl("emptyScrTmpl",
            {
                ID: id,
                ImgCss: imgClass,
                Header: header,
                Describe: TMMail.htmlEncode(description),
                ButtonHTML: buttonHtml
            });

        page.empty().html(screen);

        var btnArray = page.find("#{0} .emptyScrBttnPnl a".format(id));
        $.each(btnArray, function (index, value) {
            if (buttons[index] && buttons[index].handler) {
                $(value).click(buttons[index].handler);
            }
        });

        mailBoxContainer.hide();
        page.show();
        TMMail.scrollTop();
    }

    function hide() {
        page.hide();
        mailBoxContainer.show();
    }

    return {
        init: init,
        showEmptyAccounts: showEmptyAccounts,
        showNoLettersFilter: showNoLettersFilter,
        showEmptyFolder: showEmptyFolder,
        showEmptyCrmContacts: showEmptyCrmContacts,
        showEmptyMailContacts: showEmptyMailContacts,
        showNoCrmContacts: showNoCrmContacts,
        showNoTlContacts: showNoTlContacts,
        showNoMailContacts: showNoMailContacts,
        showEmptyTags: showEmptyTags,
        showNoMailDomains: showNoMailDomains,
        showEmptyUserFolders: showEmptyUserFolders,
        showEmptyFilters: showEmptyFilters,
        hide: hide
    };
})(jQuery);