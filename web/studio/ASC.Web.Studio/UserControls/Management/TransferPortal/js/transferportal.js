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


TransferManager = new function() {

    this.transferInterval;

    this.init = function() {

        var $mainBlock = jq("#migrationPortal");
        if ($mainBlock.hasClass("disable")) {
            $mainBlock.find("select, input").attr("disabled", "disabled");
            $mainBlock.find(".button").addClass("disable");
        }

        var $regionList = jq("#transfer_region").find("option"),
            names = [];

        for (var i = 0; i < $regionList.length; i++) {
            names.push(jq($regionList[i]).val());
        }

        jq("#transfer_region").on("change", function() {
            var $select = jq(this).find("option:selected");
            jq("#regionUrl").text($select.attr("data-url"));

            if (jq(this).val() != jq(this).attr("data-value")
               && jq.inArray(jq(this).attr("data-value"), names) != -1) {
                jq("#transfer_button").removeClass("disable");
            } else {
                jq("#transfer_button").addClass("disable");
            }
        });
        jq("#transfer_button").on("click", function() {
            if (jq(this).hasClass("disable")) {
                return;
            }
            var popup = "#popupTransferStart";
            StudioBlockUIManager.blockUI(popup, 500);
            jq(popup).find(".button.blue").on("click", function () {
                PopupKeyUpActionProvider.CloseDialog();
                TransferManager.StartTransfer();
            });
        });
    };

    this.StartTransfer = function() {

        jq("#transfer_error").addClass("display-none");

        var notify = jq("#notifyAboutMigration").is(":checked"),
            withMail = jq("#migrationMail").is(":checked"),
            region = jq("#transfer_region").val();

        AjaxPro.Backup.StartTransfer(region, notify, withMail, TransferManager.startTransferCallback);
    };

    this.startTransferCallback = function (result) {
        if (!result) {
            return;
        }

        if (result.error) {
            jq("#transfer_error").html(result.error.Message).removeClass("display-none");
        }
        if (result.value) {
            window.location.href = "./PreparationPortal.aspx?type=0";
        }

    };

};
jq(function() {
    TransferManager.init();
})