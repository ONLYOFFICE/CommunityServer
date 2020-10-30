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
        ASC.Files.UI.blockUI("#confirmConvert", 510);
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