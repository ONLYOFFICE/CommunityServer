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


DeactivationPortalManager = new function () {
    this.Init = function () {
        jq("#sendDeactivateInstructionsBtn").on("click", DeactivationPortalManager.SendDeactivateInstructions);
        jq("#showDeleteDialogBtn").on("click", DeactivationPortalManager.showDeleteDialog);
        jq("#sendDeleteInstructionsBtn").on("click", DeactivationPortalManager.SendDeleteInstructions);
    };

    this.showDeleteDialog = function() {
        if (jq("#deleteDialog").length)
            StudioBlockUIManager.blockUI("#deleteDialog", 450);
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
