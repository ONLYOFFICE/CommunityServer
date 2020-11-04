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
        StudioBlockUIManager.blockUI("#studio_showBackupCodesDialog", 500);
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