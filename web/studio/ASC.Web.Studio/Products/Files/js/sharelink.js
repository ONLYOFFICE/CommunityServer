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


window.ASC.Files.ShareLink = (function () {
    var isInit = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            window.resizeTo(650, 530);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SendLinkToEmail, onSendLinkToEmail);

            jq("#shareLinkEmailSelector").AdvancedEmailSelector("init");

            jq("body").on("click", "#shareSendLinkToEmail:not(.disable)", sendLinkToEmail);

            jq("body").on("keypress", function (e) {
                if (e.ctrlKey && e.keyCode === ASC.Files.Common.keyCode.enter) {
                    jq("#shareSendLinkToEmail:not(.disable)").click();
                }
            });

            jq("body").on("keypress", "#shareLink", function (e) {
                return e.ctrlKey && (e.charCode === ASC.Files.Common.keyCode.c || e.charCode === ASC.Files.Common.keyCode.C || e.keyCode === ASC.Files.Common.keyCode.insertKey);
            });

            autosize(jq("#shareMailText"));
        }
    };

    //request

    var sendLinkToEmail = function () {
        var fileId = jq("#shareSendLinkToEmail").attr("data-id");
        var message = jq("#shareMailText").val().trim();

        var stringList = new Array();

        jq(jq("#shareLinkEmailSelector").AdvancedEmailSelector("get")).each(function (i, item) {
            if (item.isValid) {
                stringList.push(item.email);
            }
        });

        if (!stringList.length) {
            jq(".emailselector-input").focus();
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.ErrorMassage_EmptyField, true);
            return;
        }

        jq("#shareSendLinkToEmail").addClass("disable");

        var dataResult =
            {
                address: { entry: stringList },
                message: message
            };

        ASC.Files.ServiceManager.sendLinkToEmail(ASC.Files.ServiceManager.events.SendLinkToEmail,
            { fileId: fileId },
            { message_data: dataResult });
    };

    //event handler

    var onSendLinkToEmail = function (jsonData, params, errorMessage) {
        jq("#shareSendLinkToEmail").removeClass("disable");
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoMailsSended);
        setTimeout(function () { window.close(); }, 2000);
    };

    return {
        init: init,
    };
})();

jq(document).ready(function () {
    (function ($) {
        if (jq("#shareMailText").length == 0)
            return;

        ASC.Files.ShareLink.init();
        $(function () {

        });
    })(jQuery);
});