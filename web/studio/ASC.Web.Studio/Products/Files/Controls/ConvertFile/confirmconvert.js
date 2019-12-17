/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


window.ASC.Files.ConfrimConvert = (function () {
    var isInit = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            jq(".store-original").change(function () {
                storeOriginalFiles(this);
            });
        }
    };

    var showDialog = function (continueCallback, cancelCallbackString, isForSave) {
        if (jq("#confirmConvert:visible").length > 0) {
            return;
        }

        isForSave = isForSave === true;

        var currentState = jq("#confirmConvert").attr(isForSave ? "data-save" : "data-open");
        if (currentState == "true") {
            if (typeof continueCallback === "function") {
                continueCallback.call();
            }

            return;
        }

        jq("#confirmConvertSave,#confirmConvertSaveDescript").toggle(isForSave);
        jq("#confirmConvertEdit,#confirmConvertEditDescript").toggle(!isForSave);

        jq("#confirmConvertContinue").off("click").on("click", function () {
            jq("#confirmConvertContinue").off("click");
            
            if (jq("#confirmConvertHide").prop("checked")) {
                Teamlab.hideConfirmConvert(isForSave, {
                    success: function () {
                        jq("#confirmConvert").attr(isForSave ? "data-save" : "data-open", true);
                    }
                });
            }

            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();

            if (typeof continueCallback === "function") {
                continueCallback.call();
            }
        });

        PopupKeyUpActionProvider.CloseDialogAction = cancelCallbackString;
        ASC.Files.UI.blockUI("#confirmConvert", 500, 0, -120);
    };

    var storeOriginalFiles = function (target) {
        var value = jq(target).prop("checked");

        Teamlab.filesStoreOriginal(value, {
            success: function (_, data) {
                ASC.Files.Common.storeOriginal = (data === true);

                jq(".store-original").prop("checked", ASC.Files.Common.storeOriginal);
            },
            error: function (params, error) {
                ASC.Files.UI.displayInfoPanel(error[0], true);
            }
        });
    };

    return {
        init: init,

        showDialog: showDialog
    };
})();

(function ($) {
    $(function () {
        ASC.Files.ConfrimConvert.init();
    });
})(jQuery);