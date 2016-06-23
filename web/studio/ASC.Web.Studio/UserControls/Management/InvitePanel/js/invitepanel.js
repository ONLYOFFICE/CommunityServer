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

            ASC.Clipboard.destroy(clip);

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

            ASC.Clipboard.destroy(clip);

            var url = jq("#shareInviteUserLink").val();
            clip = ASC.Clipboard.create(url, "shareInviteUserLinkCopy", {
                panelId: "shareInviteUserLinkPanel",
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

        linkPanel.find(".google").attr("href", ASC.Resources.Master.UrlShareGooglePlus.format(link));
        linkPanel.find(".facebook").attr("href", ASC.Resources.Master.UrlShareFacebook.format(link, "", "", ""));
        linkPanel.find(".twitter").attr("href", ASC.Resources.Master.UrlShareTwitter.format(text));
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