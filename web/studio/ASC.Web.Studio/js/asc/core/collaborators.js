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


/*
 * Welcome Collaborators popup Manager
 */
window.WelcomeCollaboratorsManager = new function () {

    function closeWelcomePopup () {
        Teamlab.closeWelcomePopup();
        javascript: jq.unblockUI();
    }

    function init () {
        var regPath = /(^\/$)|(\/feed\.aspx)|(\/products\/community\/)|(\/products\/projects\/)|(\/products\/files\/)/g;
        if (location.pathname.match(regPath) == null) {
            return;
        }

        jq.tmpl("template-blockUIPanel", {
            id: "studio_welcomeCollaboratorContainer",
            headerTest: ASC.Resources.Master.Resource.WelcomeCollaboratorPopupHeader,

            innerHtmlText: ["<div class=\"welcome-to-teamlab-with-logo\">",
                "<p class=\"welcome\">",
                Encoder.XSSEncode(ASC.Resources.Master.Resource.WelcomeToTeamlab),
                "</p>",
                "<p>",
                ASC.Resources.Master.Resource.WelcomeCollaboratorRole,
                "</p>",
                jq.format(ASC.Resources.Master.Resource.WelcomeCollaboratorCan,
                    "<p>", "</p><ul class='welcome-collaborator-can'><li>", "</li><li>", "</li><li>", "</li></ul>"),
                "<p>",
                ASC.Resources.Master.Resource.WelcomeCollaboratorOtherActions,
                "</p>",
                "</div>"].join(''),
            OKBtn: ASC.Resources.Master.Resource.WelcomeCollaboratorStartWork
        }).appendTo("#studioPageContent .mainPageContent:first");

        jq("#studio_welcomeCollaboratorContainer").on("click", ".button.blue, .cancelButton", function () {
            window.WelcomeCollaboratorsManager.closeWelcomePopup();
        });

        StudioBlockUIManager.blockUI('#studio_welcomeCollaboratorContainer', 500);
    }

    return {
        init: init,
        closeWelcomePopup: closeWelcomePopup
    };
};
jq(document).ready(function () {
    WelcomeCollaboratorsManager.init();
});