/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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

        StudioBlockUIManager.blockUI('#studio_welcomeCollaboratorContainer', 500, 400, 0);
    }

    return {
        init: init,
        closeWelcomePopup: closeWelcomePopup
    };
};
jq(document).ready(function () {
    WelcomeCollaboratorsManager.init();
});