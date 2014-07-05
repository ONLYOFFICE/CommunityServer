/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
window.blankPages = (function($) {
    var page;

    var init = function() {
        page = $('#blankPage');
    };

    function showEmptyAccounts() {
        showPage(MailScriptResource.EmptyScrAccountsHeader,
            MailScriptResource.EmptyScrAccountsDescription,
            'accounts',
            MailScriptResource.EmptyScrAccountsButton,
            'addFirstElement',
            function() {
                ASC.Controls.AnchorController.move('#accounts');
                accountsModal.addBox(); return false;
            }

        );
    }

    function showEmptyTags() {
        showPage(MailScriptResource.EmptyScrTagsHeader,
            MailScriptResource.EmptyScrTagsDescription,
            'tags',
            MailScriptResource.EmptyScrTagsButton,
            'addFirstElement',
            function() {
                tagsModal.showCreate();
                return false;
            }
        );
    }

    function showNoLettersFilter() {
        showPage(MailScriptResource.NoLettersFilterHeader,
            MailScriptResource.NoLettersFilterDescription,
            'filter',
            MailScriptResource.ResetFilterButton,
            'clearFilterButton',
            function() { folderFilter.reset(); return false; }
        );
    }

    function showEmptyFolder() {
        var header = undefined;
        var description = undefined;
        var img_class = undefined;
        var btn_text = undefined;
        var btn_class = 'addFirstElement';
        var btn_handler = function() {
            messagePage.compose();
            return false;
        };

        if (TMMail.pageIs('inbox')) {
            header = MailScriptResource.EmptyInboxHeader;
            description = MailScriptResource.EmptyInboxDescription;
            img_class = 'inbox';
            btn_text = MailScriptResource.EmptyInboxButton;
        } else if (TMMail.pageIs('sent')) {
            header = MailScriptResource.EmptySentHeader;
            description = MailScriptResource.EmptySentDescription;
            img_class = 'sent';
            btn_text = MailScriptResource.EmptySentButton;
        } else if (TMMail.pageIs('drafts')) {
            header = MailScriptResource.EmptyDraftsHeader;
            description = MailScriptResource.EmptyDraftsDescription;
            img_class = 'drafts';
            btn_text = MailScriptResource.EmptyDraftsButton;
        } else if (TMMail.pageIs('trash')) {
            header = MailScriptResource.EmptyTrashHeader;
            description = MailScriptResource.EmptyTrashDescription;
            img_class = 'trash';
            btn_handler = undefined;
        } else if (TMMail.pageIs('spam')) {
            header = MailScriptResource.EmptySpamHeader;
            description = MailScriptResource.EmptySpamDescription;
            img_class = 'spam';
            btn_handler = undefined;
        }
        showPage(header, description, img_class, btn_text, btn_class, btn_handler);
    }

    function showEmptyCrmContacts() {
        showPage(MailScriptResource.EmptyScrCrmHeader,
            MailScriptResource.EmptyScrCrmDescription,
            'contacts',
            MailScriptResource.EmptyScrCrmButton,
            'addFirstElement',
            null,
            '/products/crm/'
        );
    }

    function showNoCrmContacts() {
        showPage(MailScriptResource.ResetCrmContactsFilterHeader,
            MailScriptResource.ResetCrmContactsFilterDescription,
            'filter',
            MailScriptResource.ResetFilterButton,
            'clearFilterButton',
            function() { contactsPage.resetFilter(); return false; }
        );
    }

    function showNoTlContacts() {
        showPage(MailScriptResource.ResetTlContactsFilterHeader,
            MailScriptResource.ResetTlContactsFilterDescription,
            'filter',
            MailScriptResource.ResetFilterButton,
            'clearFilterButton',
            function() { contactsPage.resetFilter(); return false; }
        );
    }

    function showPage(header, description, imgClass, btnText, btnClass, btnHandler, btnHref) {
        var button_html = undefined;
        if (btnText != undefined) {
            var tmpl = $.template("emptyScrButtonTmpl"); // template function
            var strings = tmpl($, { data: { ButtonHref: btnHref, ButtonClass: btnClass, ButtonText: btnText } }); // array of strings
            button_html = strings.join(''); // single string
        }

        var screen = $.tmpl("emptyScrTmpl",
            {
                ImgCss: imgClass,
                Header: header,
                Describe: description,
                ButtonHTML: button_html
            });

        page.empty().html(screen);
        
        if ($.isFunction(btnHandler)) {
            page.find('.emptyScrBttnPnl a').click(btnHandler);
        }

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
        hide: hide
    };
})(jQuery);