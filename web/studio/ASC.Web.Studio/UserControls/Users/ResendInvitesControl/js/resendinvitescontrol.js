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


jq(function() {
    jq('#resendBtn').click(function() {
        InvitesResender.Resend();
    });
 
    jq('#resendCancelBtn, #resendInvitesCloseBtn').click(function() {
        InvitesResender.Hide();
    });
});
var InvitesResender = new function() {
    this.Resend = function() {
        AjaxPro.onLoading = function(b) {
            if (b)
                LoadingBanner.showLoaderBtn("#inviteResender");
            else
                LoadingBanner.hideLoaderBtn("#inviteResender");
        };
        InviteResender.Resend(function(result) {
            var res = result.value;
            if (res.status == 1) {
                jq.unblockUI();
                toastr.success(res.message);
            }
            else {
                toastr.error(res.message);
            }
            PopupKeyUpActionProvider.EnterAction = "PopupKeyUpActionProvider.CloseDialog();";

        })
    }

    this.Show = function() {
        jq('#resendInvitesResult').addClass("display-none");
        jq('#resendInvitesContent').removeClass("display-none");

        StudioBlockUIManager.blockUI("#inviteResender", 330);
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = "jq(\"#resendBtn\").click();";
    }
    
    this.Hide = function() {
        jq.unblockUI();
    }
}