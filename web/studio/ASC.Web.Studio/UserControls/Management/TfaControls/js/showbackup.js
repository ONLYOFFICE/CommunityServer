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


;
window.ASC.Controls.TfaAppShowBackupCodes = (function () {
    var $content = jq("#showBackupCodesContent"),
        $errorBox,
        $showBackupCodes,
        $showBackupCodesContent,
        $codesremain,
        teamlab = Teamlab,
        displayNone = "display-none",
        codes;

    var init = function () {
        teamlab.tfaappcodes({
            success: function (params, response) {
                initCodes(response);
                $content.html(jq.tmpl("tfaCodesPopup", codes));
                $errorBox = jq("#errorRequestNewCodes");
                $showBackupCodes = jq("#showBackupCodes");
                $showBackupCodesContent = jq("#showBackupCodesContent");
                $codesremain = jq("#codesremain");

                addEvents();

                if (ASC.Controls.AnchorController.getAnchor() === "codes") {
                    openDialog();
                }
            }
        });
    };

    function addEvents () {
        jq("#showBackupCodesPrint").on("click", function () {

            var onloadStr = "javascript:setTimeout(function() {window.print();";
            if (!jq.browser.opera && !jq.browser.safari) {
                onloadStr += " window.close();";
            }
            onloadStr += "}, 100)";

            var windowContent = '<!DOCTYPE html>';
            windowContent += '<html>';
            windowContent += '<head>';
            windowContent += '<title>' + document.title + '</title>';
            windowContent += '<style>@media print{img{height:90%;} @page { size: landscape; }} body { margin: 40px; padding: 0;} #showBackupCodes { margin: 12px 0; font-weight: bold; } div.disabled { text-decoration-line:line-through; }</style>';
            windowContent += '</head>';
            windowContent += '<body onload="' + onloadStr + '">';
            windowContent += jq("<div />").append(jq.tmpl("tfaCodesContentTmpl", codes)).html();
            windowContent += '</body>';
            windowContent += '</html>';

            var content = windowContent;
            var printWin = window.open();

            printWin.document.open();
            printWin.document.write(content);
            printWin.document.close();
            printWin.focus();
        });

        jq("#showBackupCodeRequestNew").on("click", function () {
            $errorBox.addClass(displayNone);

            teamlab.tfaAppRequestNewCodes({
                success: showCodes,
                error: function (params, error) {
                    $errorBox.html(error[0] || "Error").removeClass(displayNone);
                }
            });
        });

        jq("#showBackupCodesClose").on("click", function () {
            PopupKeyUpActionProvider.CloseDialog();
            return false;
        });

        jq("#showBackupCodesButton").on("click", openDialog);
    }

    function showCodes (params, response) {
        initCodes(response);
        jq(".codesremain").text(codes.unused);
        $showBackupCodes.html(jq.tmpl("tfaCodesTmpl", response));
    }

    function openDialog () {
        $content.show();
        StudioBlockUIManager.blockUI("#studio_showBackupCodesDialog", 500, 600);
        ASC.Controls.AnchorController.move();
    }

    function initCodes (response) {
        codes = {items: response, unused: response.filter(function (item) { return !item.isUsed; }).length};
    }

    return {
        init: init
    };
})();

jq(document).ready(function () {
    StudioManager.addPendingRequest(window.ASC.Controls.TfaAppShowBackupCodes.init);
});