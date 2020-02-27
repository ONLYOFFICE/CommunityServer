/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


jq(function () {
    PortalRename.init();
});

var PortalRename = new function () {

    this.init = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "portalRenameConfirmation",
            headerTest: ASC.Resources.Master.Resource.PortalRenameConfirmationTitle,
            questionText: '',
            innerHtmlText: jq.format(ASC.Resources.Master.Resource.PortalRenameConfirmationPopup, "<p>", "</p>"),
            OKBtn: ASC.Resources.Master.Resource.ContinueButton,
            CancelBtn: ASC.Resources.Master.Resource.CancelButton
        }).insertAfter("#studio_portalRename");

        jq("#portalRenameConfirmation").find(".button.blue").on("click", function () {
            PopupKeyUpActionProvider.CloseDialog();
            PortalRename.saveNewTenantAlias();
        });

        jq("#studio_portalRename .button.blue").on("click", function (event) {
            if (jq("#studio_portalRename .button.blue").hasClass("disable")) return;

            var alias = jq.trim(jq("#studio_tenantAlias").val());
            if (alias === "") {
                jq("#studio_tenantAlias").addClass("with-error");
                return;
            } else if (alias == jq("#studio_tenantAlias").attr("data-actualvalue")) {
                jq("#studio_tenantAlias").addClass("with-error");
                LoadingBanner.showMesInfoBtn("#studio_portalRename", ASC.Resources.Master.Resource.ErrorPortalNameWasNotChanged, "error");
                return;
            }  else {
                jq("#studio_tenantAlias").removeClass("with-error");
            }

            StudioBlockUIManager.blockUI("#portalRenameConfirmation", 420, 300, 0);
        });
    };

    this.saveNewTenantAlias = function () {

        var alias = jq.trim(jq("#studio_tenantAlias").val());

        Teamlab.updatePortalName({}, alias, {

            before: function () {
                jq("#studio_tenantAlias").prop("disabled", true);
                LoadingBanner.showLoaderBtn("#studio_portalRename");
            },
            after: function () {
                LoadingBanner.hideLoaderBtn("#studio_portalRename");
            },
            success: function (params, response) {
                LoadingBanner.showMesInfoBtn("#studio_portalRename", response.message, "success");
                window.location.replace(response.reference);
            },
            error: function (params, errors) {
                var errTest = errors[0];
                try {
                    var error = jq.parseJSON(Encoder.htmlDecode(errTest)).errors[0];

                    switch (error) {
                        case "portalNameExist": errTest = ASC.Resources.Master.Resource.ErrorPortalNameExist; break;
                        case "tooShortError": errTest = ASC.Resources.Master.Resource.ErrorPortalNameTooShort; break;
                        case "portalNameIncorrect": errTest = ASC.Resources.Master.Resource.ErrorPortalNameIncorrect; break;
                    }
                }
                catch (e) { }
                LoadingBanner.showMesInfoBtn("#studio_portalRename", errTest, "error");
                jq("#studio_tenantAlias").prop("disabled", false);
            }

        });
    };
}
