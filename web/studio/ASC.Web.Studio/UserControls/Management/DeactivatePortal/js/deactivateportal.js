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


DeactivationPortalManager = new function () {
    this.Init = function () {
        jq("#sendDeactivateInstructionsBtn").on("click", DeactivationPortalManager.SendDeactivateInstructions);
        jq("#showDeleteDialogBtn").on("click", DeactivationPortalManager.showDeleteDialog);
        jq("#sendDeleteInstructionsBtn").on("click", DeactivationPortalManager.SendDeleteInstructions);
    };

    this.showDeleteDialog = function() {
        if (jq("#deleteDialog").length)
            StudioBlockUIManager.blockUI("#deleteDialog", 450, 400);
        else
            DeactivationPortalManager.SendDeleteInstructions();
    };

    this.SendDeactivateInstructions = function () {
        if (jq("#sendDeactivateInstructionsBtn").hasClass("disable")) {
            return;
        }

        LoadingBanner.showLoaderBtn("#accountDeactivationBlock");
        AjaxPro.DeactivatePortal.SendDeactivateInstructions(function (response) {
            if (response.value) {
                var $status = jq('#deativate_sent');
                $status.html(response.value);
                $status.show();
            }
            LoadingBanner.hideLoaderBtn("#accountDeactivationBlock");
        });
    };

    this.SendDeleteInstructions = function () {
        if (jq("#sendDeleteInstructionsBtn").hasClass("disable")) {
            return;
        }

        LoadingBanner.showLoaderBtn("#deleteDialog");
        AjaxPro.DeactivatePortal.SendDeleteInstructions(function (response) {
            if (response.value) {
                var $status = jq('#delete_sent');
                $status.html(response.value);
                $status.show();
            }
            PopupKeyUpActionProvider.CloseDialog();
            LoadingBanner.hideLoaderBtn("#deleteDialog");
        });
    };


};

(function ($) {
    $(function () {
        DeactivationPortalManager.Init();
    });
})(jQuery);
