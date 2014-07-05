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
window.watchdog = (function($) {
    var popup_queue = [],
        is_init = false;

    function init(timeout_ms){
        if (true === is_init)
            return;

        is_init = true;

        serviceManager.bind(window.Teamlab.events.getMailAccounts, function(params, accounts) {
            checkAccounts(accounts);
        });

        setInterval(function() {
            serviceManager.getMailAccounts();
        }, timeout_ms*10);
    };

    function addPopup(header_text, body_text) {
        var body = "<div class=\"popup\"><p class=\"error\">";
        body += body_text;
        body += "</p>";
        body += "<div class=\"buttons\"><a class=\"button middle gray cancel\">" + window.MailScriptResource.CloseBtnLabel + "</a></div></div>";
        popup_queue.push({ header: header_text, body: body });
    }

    function checkAccounts(accounts) {
        var quota_error_accounts = [];
        $.each(accounts, function(index, value) {
            if (!value.enabled)
                return;
            if (value.quotaError)
                quota_error_accounts.push(value);
        });

        if (quota_error_accounts.length > 0) {
            addPopup(window.MailScriptResource.QuotaPopupHeader, window.MailScriptResource.QuotaPopupBody);
        }

        var item;
        while (item = popup_queue.pop())
            popup.addBig(item.header, item.body, item.onHide);
    }

    return {
        init: init
    };

})(jQuery);