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


ASC.Projects.Contacts = (function() {
    var projectTeam, currentProjectId;
    var init = function () {
        currentProjectId = jq.getURLParam("prjID");

        if (!currentProjectId)
            return;
        
        var dataForContactSelectorInit = {
            SelectorType: -1,
            EntityType: 0,
            EntityID: 0,

            ShowOnlySelectorContent: true,
            DescriptionText: ASC.Projects.Resources.ProjectsJSResource.CRMDescrForSelector,
            DeleteContactText: "",
            AddContactText: "",
            IsInPopup: false,
            NewCompanyTitleWatermark: ASC.Projects.Resources.ProjectsJSResource.CRMCompanyName,
            NewContactFirstNameWatermark: ASC.Projects.Resources.ProjectsJSResource.CRMFirstName,
            NewContactLastNameWatermark: ASC.Projects.Resources.ProjectsJSResource.CRMLastName,

            ShowChangeButton: false,
            ShowAddButton: false,
            ShowDeleteButton: false,
            ShowContactImg: false,
            ShowNewCompanyContent: true,
            ShowNewContactContent: true,

            ExcludedContacts: [],
            HTMLParent: "#projContactSelectorParent"
        };

        if (typeof (ASC.Projects.Master.Team) != 'undefined') {
            projectTeam = ASC.Projects.Master.Team;
        } else {
            Teamlab.getPrjTeam({}, currentProjectId, {
                success: function(param, team) {
                    ASC.Projects.Master.Team = team;
                    projectTeam = team;
                }
            });
        }

        var params = {};
        params.showCompanyLink = true;
        if (jq("#contactsForProjectPanel").length) {
            params.showUnlinkBtn = true;
        } else {
            params.showUnlinkBtn = false;
        }
        Teamlab.getCrmContactsForProject(params, currentProjectId, {
            success: function(param, data) {
                if (data.length != 0) {
                    jq.extend(dataForContactSelectorInit, { presetSelectedContactsJson: jq.toJSON(data) });
                }
                window["projContactSelector"] = new ASC.CRM.ContactSelector.ContactSelector("projContactSelector", dataForContactSelectorInit);

                if (data.length == 0) {
                    jq("#escNoContacts.display-none").removeClass("display-none").show();
                } else {
                    ASC.Projects.projectNavPanel.changeModuleItemsCount(ASC.Projects.projectNavPanel.projectModulesNames.contacts, data.length);
                    jq("#contactsForProjectPanel").show();
                }

                window.projContactSelector.SelectItemEvent = addContactToProject;
                ASC.CRM.ListContactView.removeMember = removeContactFromProject;

                ASC.CRM.ListContactView.CallbackMethods.render_simple_content(param, data);
            }
        });

        jq("#escNoContacts .emptyScrBttnPnl>a").bind("click", function() {
            jq("#escNoContacts").addClass("display-none");
            jq("#contactsForProjectPanel").show();
        });
    };

    var addContactToProject = function(obj, params) {
        if (jq("#contactItem_" + obj.id).length > 0) {
            return false;
        }
        var data = {
            contactid: obj.id,
            projectid: currentProjectId
        };

        if (params.newContact === true) {
            var dataRights = {
                    contactid: [obj.id],
                    isShared: false,
                    managerList: new Array(Teamlab.profile.id)
                };

            Teamlab.updateCrmContactRights({}, dataRights, {});
        }
        var param = {};
        param.showCompanyLink = true;
        if (jq("#contactsForProjectPanel").length) {
            param.showUnlinkBtn = true;
        } else {
            param.showUnlinkBtn = false;
        }
        Teamlab.addCrmEntityMember(param, "project", data.projectid, data.contactid, data, {
            success: function(par, contact) {
                ASC.CRM.ListContactView.CallbackMethods.addMember(par, contact);
                window.projContactSelector.SelectedContacts.push(contact.id);
                ASC.Projects.projectNavPanel.changeModuleItemsCount(ASC.Projects.projectNavPanel.projectModulesNames.contacts, "add");
            }
        });

    };

    var removeContactFromProject = function(id) {
        Teamlab.removeCrmEntityMember({ contactID: id }, "project", currentProjectId, id, {
            before: function(params) {
                jq("#trashImg_" + params.contactID).hide();
                jq("#loaderImg_" + params.contactID).show();
            },
            after: function(params) {

                var index = jq.inArray(params.contactID, window.projContactSelector.SelectedContacts);
                window.projContactSelector.SelectedContacts.splice(index, 1);

                jq("#contactItem_" + params.contactID).animate({ opacity: "hide" }, 500);

                setTimeout(function() {
                    jq("#contactItem_" + params.contactID).remove();
                    ASC.Projects.projectNavPanel.changeModuleItemsCount(ASC.Projects.projectNavPanel.projectModulesNames.contacts, "delete");
                    if (window.projContactSelector.SelectedContacts.length == 0) {
                        jq("#contactsForProjectPanel").hide();
                        jq("#escNoContacts.display-none").removeClass("display-none").show();
                    }
                }, 500);

            }
        });
    };

    return {
        init: init
    };
})(jQuery);