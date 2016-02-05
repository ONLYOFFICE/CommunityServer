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


window.blankPages = (function($) {
    var page;

    var init = function() {
        page = $('#blankPage');
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

        showPage(MailScriptResource.EmptyScrAccountsHeader,
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

        showPage(MailScriptResource.EmptyScrTagsHeader,
            MailScriptResource.EmptyScrTagsDescription,
            'tags',
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

        showPage(MailScriptResource.NoLettersFilterHeader,
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
        }

        showPage(header, description, imgClass, buttons);
    }

    function showEmptyCrmContacts() {
        var buttons = [{
            text: MailScriptResource.EmptyScrCrmButton,
            cssClass: "addFirstElement",
            handler: null,
            href: "/products/crm/"
        }];

        showPage(MailScriptResource.EmptyScrCrmHeader,
            MailScriptResource.EmptyScrCrmDescription,
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

        showPage(MailScriptResource.ResetCrmContactsFilterHeader,
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

        showPage(MailScriptResource.ResetTlContactsFilterHeader,
            MailScriptResource.ResetTlContactsFilterDescription,
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

        showPage(window.MailAdministrationResource.NoDomainSetUpHeader,
            window.MailAdministrationResource.NoDomainSetUpDescription,
            'domains',
            buttons
        );
    }

    //buttons = [{ text: "", cssClass: "", handler: function(){}, href: "" }]
    function showPage(header, description, imgClass, buttons) {

        var buttonHtml = undefined;

        if (buttons) {
            var tmpl = $.template("emptyScrButtonTmpl");
            buttonHtml = tmpl($, {data : { buttons: buttons }});
        }

        var screen = $.tmpl("emptyScrTmpl",
            {
                ImgCss: imgClass,
                Header: header,
                Describe: TMMail.htmlEncode(description),
                ButtonHTML: buttonHtml
            });

        page.empty().html(screen);

        var btnArray = page.find('.emptyScrBttnPnl a');
        $.each(btnArray, function (index, value) {
            if (buttons[index] && buttons[index].handler) {
                $(value).click(buttons[index].handler);
            }
        });

        page.show();
    }

    function hide() {
        page.hide();
    }

    return {
        init: init,
        showEmptyAccounts: showEmptyAccounts,
        showNoLettersFilter: showNoLettersFilter,
        showEmptyFolder: showEmptyFolder,
        showEmptyCrmContacts: showEmptyCrmContacts,
        showNoCrmContacts: showNoCrmContacts,
        showNoTlContacts: showNoTlContacts,
        showEmptyTags: showEmptyTags,
        showNoMailDomains: showNoMailDomains,
        hide: hide
    };
})(jQuery);