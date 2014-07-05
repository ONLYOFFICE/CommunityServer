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


TransferManager = new function() {

    this.transferID = "";
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

    this.InitProcess = function () {
        jq(".asc-progress-value").animate({ "width": "1%" });
        jq("#transfer_percent").text("1% ");
    };

    this.SetProcess = function (percent) {
        jq(".asc-progress-value").animate({ "width": percent + "%" });
        jq("#transfer_percent").text(percent + "% ");
    };

    this.StartTransfer = function() {
        if (TransferManager.transferID != '') {
            return;
        }

        jq("#transfer_error").addClass("display-none");

        //     at the process of migration the portal is not available
        //     may be later we will show the loading of this process        

        //        TransferManager.InitProcess();

        //        jq("#transfer_button").addClass("disable");
        //        jq("#transfer_button").blur();

        //        jq("#transfer_progress").removeClass("display-none");

        var notify = jq("#notifyAboutMigration").is(":checked"),
            withMail = jq("#migrationMail").is(":checked"),
            region = jq("#transfer_region").val();

        TransferPortal.StartTransfer(region, notify, withMail, TransferManager.startTransferCallback);
    };

    this.CheckTransferProgress = function() {
        if (TransferManager.transferID == "") {
            return;
        }
        TransferPortal.CheckTransferProgress(this.transferID, TransferManager.startTransferCallback);
    };

    this.startTransferCallback = function(result) {
        if (result == null) {
            return;
        }

        if (result.error != null) {
            //            jq("#transfer_progress").addClass("display-none");
            jq("#transfer_error").html(result.error.Message).removeClass("display-none");
        }
        if (result.value != null) {
            //            clearInterval(TransferManager.transferInterval);

            var serviceAnswer = result.value!= ""? jq.parseJSON(result.value) : "";
            if (serviceAnswer.success) {
                //                TransferManager.SetProcess(serviceAnswer.data.Percent);
                //                jq("#transfer_progress").removeClass("display-none");

                TransferManager.transferID = serviceAnswer.data.Id;

                if (!serviceAnswer.data.Completed) {
                    location.href = "migration-portal.htm";
                    //                    TransferManager.transferInterval = setInterval("TransferManager.CheckTransferProgress()", 10000);
                }
                //                else {
                //                    var link = location.protocol + "//" + jq("#regionDomain").text().trim() + jq("#regionUrl").text().trim();
                //                    var msg = ASC.Resources.Master.Resource.TransferPortalCompleted + " <a href=\"" + link + "\">" + link + "</a>";
                //                    jq("#transfer_ready").html(msg).removeClass("display-none");
                //                    location.href = link;
                //                }
            } else {
                //                jq("#transfer_progress").addClass("display-none");
                jq("#transfer_error").html(serviceAnswer.data).removeClass("display-none");
            }
        }
    };

};
jq(function() {
    TransferManager.init();
})