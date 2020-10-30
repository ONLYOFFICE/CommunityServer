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


if (typeof ASC === "undefined") {
    ASC = {};
}

ASC.InvitePanel = (function () {
    var isInit = false;
    var isShortenSelected = false;
    var clip = null;

    var init = function () {
        if (isInit) return;
        isInit = true;

        jq("#invitePanelContainer").replaceWith(
            jq.tmpl("template-blockUIPanel", {
                id: "inviteLinkContainer",
                headerTest: jq("#invitePanelContainer").attr("data-header"),
                innerHtmlText: jq("#invitePanelContainer").html(),

                OKBtn: jq("#invitePanelContainer").attr("data-ok")
            })
        );

        jq("#inviteLinkContainer").on("click", ".button.blue.middle, .cancelButton", function () {
            var $target = jq(this),
                defaultLink = "";

            clip = ASC.Clipboard.destroy(clip);

            if ($target.is(".button.blue")) { PopupKeyUpActionProvider.CloseDialog(); }

            jq("#getShortenInviteLink").show();
            isShortenSelected = false;

            if (jq("#hiddenUserLink").length != 0) {
                jq("#chkVisitor").prop("checked", false);
                defaultLink = jq("#hiddenUserLink").val();
            } else {
                jq("#chkVisitor").prop("checked", true);
                defaultLink = jq("#hiddenVisitorLink").val();
            }

            jq("#shareInviteUserLink").val(defaultLink);

            ASC.InvitePanel.bindClipboardEvent();

            updateSocialLink(defaultLink);
            if ($target.is(".button.blue")) { return false; }
        });

        if (jq("#hiddenUserLink").length != 0 && jq("#hiddenVisitorLink").length != 0) {
            jq("#chkVisitor").on("click", function() {
                changeEmployeeType(this);
            });
        }

        if (jq("#getShortenInviteLink").length != 0) {
            jq("#getShortenInviteLink").on("click", function () {
                getShortenLink(jq("#shareInviteUserLink").val());
            });
        }

        updateSocialLink(jq("#shareInviteUserLink").val());

        jq("#shareInviteLinkViaSocPanel").on("click", "a", function () {
            window.open(jq(this).attr("href"), "new", "height=600,width=1020,fullscreen=0,resizable=0,status=0,toolbar=0,menubar=0,location=1");
            return false;
        });
    };

    var bindClipboardEvent = function () {
        if (!isInit) return;

        if (!ASC.Clipboard.enable) {
            jq("#shareInviteUserLinkCopy").remove();
        } else {

            clip = ASC.Clipboard.destroy(clip);

            var url = jq("#shareInviteUserLink").val();
            clip = ASC.Clipboard.create(url, "shareInviteUserLinkCopy", {
                onComplete: function () {
                    if (typeof(window.toastr) !== "undefined") {
                        toastr.success(ASC.Resources.Master.Resource.LinkCopySuccess);
                    } else {
                        jq("#shareInviteUserLink, #shareInviteUserLinkCopy").yellowFade();
                    }
                }
            });
        }
    };

    function changeEmployeeType(obj) {
        var newLink = jq(obj).is(":checked") ? jq("#hiddenVisitorLink").val() : jq("#hiddenUserLink").val();

        if (isShortenSelected) {
            getShortenLink(newLink);
        } else {
            jq("#shareInviteUserLink").val(newLink);

            ASC.InvitePanel.bindClipboardEvent();

            updateSocialLink(newLink);
        }
    };

    function getShortenLink(link) {

        Teamlab.getShortenLink({}, link,
             {
                 before: function () {
                     LoadingBanner.showLoaderBtn("#inviteLinkContainer");
                 },
                 after: function () {
                     LoadingBanner.hideLoaderBtn("#inviteLinkContainer");
                 },
                 error: function (params, errors) {
                     toastr.error(errors[0]);
                     jq("#getShortenInviteLink").remove();
                 },
                 success: function (params, response) {
                    jq("#getShortenInviteLink").hide();
                    jq("#shareInviteUserLink").val(response);

                    ASC.InvitePanel.bindClipboardEvent();

                    updateSocialLink(jq("#shareInviteUserLink").val());
                    isShortenSelected = true;
                 }
             });
    };

    function updateSocialLink(url) {


        var linkPanel = jq("#shareInviteLinkViaSocPanel"),
            link = encodeURIComponent(url),
            text = encodeURIComponent(jq.format(ASC.Resources.Master.Resource.ShareInviteLinkDscr, url));

        if (!!ASC.Resources.Master.UrlShareFacebook) {
            linkPanel.find(".facebook").attr("href", ASC.Resources.Master.UrlShareFacebook.format(link, "", "", ""));
        } else {
            linkPanel.find(".facebook").remove();
        }
        if (!!ASC.Resources.Master.UrlShareTwitter) {
            linkPanel.find(".twitter").attr("href", ASC.Resources.Master.UrlShareTwitter.format(text));
        } else {
            linkPanel.find(".twitter").remove();
        }
    };


    return {
        init: init,
        bindClipboardEvent: bindClipboardEvent
    };
})();

jq(document).ready(function () {
    try {
        ASC.InvitePanel.init();
    } catch (e) { }
});