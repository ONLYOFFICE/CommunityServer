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

            StudioBlockUIManager.blockUI("#portalRenameConfirmation", 420);
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
