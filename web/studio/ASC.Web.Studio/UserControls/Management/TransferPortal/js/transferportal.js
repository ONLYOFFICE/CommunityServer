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
            StudioBlockUIManager.blockUI(popup, 500, 500, 0);
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