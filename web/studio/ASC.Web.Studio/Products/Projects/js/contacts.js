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


ASC.Projects.Contacts = (function($) {
    var baseObject = ASC.Projects,
        currentProjectId,
        resources = baseObject.Resources.ProjectsJSResource,
        displayNoneClass = "display-none";

    var teamlab;
    var $contactsForProjectPanel = $("#contactsForProjectPanel"),
        $escNoContacts = $("#escNoContacts");

    var init = function () {
        teamlab = Teamlab;
        currentProjectId = jq.getURLParam("prjID");

        if (!currentProjectId)
            return;
        
        var dataForContactSelectorInit = {
            SelectorType: -1,
            EntityType: 0,
            EntityID: 0,

            ShowOnlySelectorContent: true,
            DescriptionText: resources.CRMDescrForSelector,
            DeleteContactText: "",
            AddContactText: "",
            IsInPopup: false,
            NewCompanyTitleWatermark: resources.CRMCompanyName,
            NewContactFirstNameWatermark: resources.CRMFirstName,
            NewContactLastNameWatermark: resources.CRMLastName,

            ShowChangeButton: false,
            ShowAddButton: false,
            ShowDeleteButton: false,
            ShowContactImg: false,
            ShowNewCompanyContent: true,
            ShowNewContactContent: true,

            ExcludedContacts: [],
            HTMLParent: "#projContactSelectorParent"
        };

        if (typeof (ASC.Projects.Master.Team) == 'undefined') {
            teamlab.getPrjTeam({}, currentProjectId, {
                success: function(param, team) {
                    ASC.Projects.Master.Team = team;
                }
            });
        }

        teamlab.bind(teamlab.events.getCrmContactsForProject,
            function(param, data) {
                if (data.length !== 0) {
                    jq.extend(dataForContactSelectorInit, { presetSelectedContactsJson: jq.toJSON(data) });
                }
                window["projContactSelector"] = new ASC.CRM.ContactSelector
                    .ContactSelector("projContactSelector", dataForContactSelectorInit);

                if (data.length === 0) {
                    $escNoContacts.removeClass(displayNoneClass).show();
                } else {
                    $contactsForProjectPanel.show();
                }

                window.projContactSelector.SelectItemEvent = addContactToProject;
                ASC.CRM.ListContactView.removeMember = removeContactFromProject;

                ASC.CRM.ListContactView.CallbackMethods.render_simple_content(param, data);
            });
        //remove success
        setTimeout(function() {
            teamlab.getCrmContactsForProject(makeParams(), currentProjectId);
        }, 0);

        jq("#escNoContacts .emptyScrBttnPnl>a").bind("click", function() {
            $escNoContacts.addClass(displayNoneClass);
            $contactsForProjectPanel.show();
        });
    };

    function addContactToProject (obj, params) {
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
                    managerList: new Array(teamlab.profile.id)
                };

            teamlab.updateCrmContactRights({}, dataRights, {});
        }

        teamlab.addCrmEntityMember(makeParams(), "project", data.projectid, data.contactid, data, {
            success: function(par, contact) {
                ASC.CRM.ListContactView.CallbackMethods.addMember(par, contact);
                window.projContactSelector.SelectedContacts.push(contact.id);
            }
        });

        return true;
    };

    function removeContactFromProject (id) {
        teamlab.removeCrmEntityMember({ contactID: id }, "project", currentProjectId, id, {
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
                    if (window.projContactSelector.SelectedContacts.length === 0) {
                        $contactsForProjectPanel.hide();
                        $escNoContacts.removeClass(displayNoneClass).show();
                    }
                }, 500);

            }
        });
    };

    function makeParams() {
        var param = {
            showCompanyLink: true,
            showUnlinkBtn: false
        };

        if ($contactsForProjectPanel.length) {
            param.showUnlinkBtn = true;
        }
        return param;
    }

    return {
        init: init
    };
})(jQuery);