/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

ASC.VoipNavigationItem = (function () {
    var isInit = false;

    function init() {
        if (!ASC.SocketIO || ASC.SocketIO.disabled()) {
            return;
        }

        jq(".studio-top-panel .voip").removeClass("display-none");
        isInit = true;

        jq(".voipActiveBox").click(function () {
            open();
        });
    }

    function open(contactId) {
        var hWnd = null,
            isExist,
            path = "/voipclient.aspx",
            winName = 'ASCVoipClient' + location.hostname,
            params = 'ontouchend' in document ? '' : 'width=350,height=500,status=no,toolbar=no,menubar=no,resizable=yes,scrollbars=no';

        try {
            hWnd = window.open('', winName, params);
        } catch (err) {
        }

        try {
            isExist = !hWnd || typeof hWnd.ASC === 'undefined' ? false : true;
        } catch (err) {
            isExist = true;
        }

        if (!isExist) {
            hWnd = window.open(path, winName, params);
        }

        if (contactId) {
            setTimeout(function () {
                hWnd.ASC.CRM.Voip.PhoneView.makeCallToContact(contactId);
            }, !isExist ? 3000 : 0);
        }

        try {
            if (hWnd) {
                hWnd.focus();
            }
        } catch (err) {
        }

        return hWnd;
    }

    function call(contactId) {
        if (!isInit) return;

        open(contactId);
    }

    return {
        init: init,
        call: call,
        get isInit() {
            return isInit;
        }
    }
})();

jq(document).ready(function () {
    ASC.VoipNavigationItem.init();
});