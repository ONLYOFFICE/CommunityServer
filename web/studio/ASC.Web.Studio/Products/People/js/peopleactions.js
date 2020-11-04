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


;
window.peopleActions = (function() {
    function getProfile(id) {
        var 
            profiles = ASC.People.model.profiles || [],
            profilesInd;

        var profilesInd = profiles.length;
        while (profilesInd--) {
            if (profiles[profilesInd].id == id) {
                return profiles[profilesInd];
            }
        }
        return null;
    };
    
    var callMethodByClassname = function(classname, thisArg, argsArray) {
        if ((' ' + classname + ' ').indexOf(' disabled ') !== -1) {
            return undefined;
        }

        var cls = '',
            classesInd = 0,
            classes = typeof classname === 'string' ? classname.split(/\s+/) : [],
            classesInd = classes ? classes.length : 0;
        while (classesInd--) {
            cls = classes[classesInd].replace(/-/g, '_');
            if (typeof peopleActions[cls] === 'function') {
                peopleActions[cls].apply(thisArg, argsArray);
            }
        }
    };
	
	var callbackAddUser = function(result) {
        var $o = jq.tmpl("userListTemplate", { users: [result.Data], isAdmin: Teamlab.profile.isAdmin || window.ASC.Resources.Master.IsProductAdmin });

        if (jq("#peopleData tr.profile").length == 0) {
            jq("#emptyContentForPeopleFilter").addClass("display-none");
            jq(".people-content").removeClass("display-none");
        }
        jq("#peopleData").prepend($o);
        ASC.Controls.AnchorController.trigger();
    };

    return {
        callMethodByClassname: callMethodByClassname,

        add_group: function(evt, $btn) {
            DepartmentManagement.AddDepartmentOpenDialog();
        },

        update_group: function(evt, $btn) {
            ASC.People.PeopleController.getEditGroup();
        },

        delete_group: function(evt, $btn) {
            jq("#confirmationDeleteDepartmentPanel .confirmationAction").html(jq.format(ASC.Resources.Master.ConfirmRemoveDepartment, "<b>" + jq(".profile-title:first>.header").html() + "</b>"));
            jq("#confirmationDeleteDepartmentPanel .middle-button-container>.button.blue.middle").unbind("click").bind("click", function() {
                ASC.People.PeopleController.deleteGroup();
            });
            StudioBlockUIManager.blockUI("#confirmationDeleteDepartmentPanel", 500);
            PopupKeyUpActionProvider.ClearActions();
            PopupKeyUpActionProvider.EnterAction = 'ASC.People.PeopleController.deleteGroup();';
        },

        add_profiles: function(evt, $btn) {
            location.href = "/Products/People/Import.aspx";
        },

        send_invites: function(evt, $btn) {
            InvitesResender.Show();
        },

        send_email: function(evt, $btn) {
            var userId = $btn.parents('tr.item.profile:first').attr('data-id');
            if (userId) {
                var email = $btn.parents('tr.item.profile:first').attr('data-email');
                if (email) {
                    window.open('../../addons/mail/#composeto/email=' + email, "_blank");
                }
                //var profile = getProfile(userId);
                //if (profile) {
                //  location.href = 'mailto:' + profile.email;
                //}
            }
        },
		
		open_dialog: function(evt, $btn) {
            var userId = $btn.parents('tr.item.profile:first').attr('data-id');
            if (userId) {
                var userName = $btn.parents('tr.item.profile:first').attr('data-username');
                if (userName) {
                    try {
                        ASC.Controls.JabberClient.open(userName);
                    } catch (err) { }
                }
                //var profile = getProfile(userId);
                //if (profile) {
                //  try { ASC.Controls.JabberClient.open(profile.userName) } catch (err) {console.log(err)}
                //}
            }
        },

        invite_link: function (evt, $btn) {
            PopupKeyUpActionProvider.ClearActions();
            jq("#otherActions").hide();
            StudioBlockUIManager.blockUI("#inviteLinkContainer", 550);
            ASC.InvitePanel.bindClipboardEvent();
        }
    };
})();

jq(function () {
    var pathname = "/Products/People/";
    jq("#groupList .menu-item-label").each(function (index, item) {
        var id = jq(item).parents(".menu-sub-item").attr("data-id");
        if (location.pathname != pathname) {
            jq(item).attr("href", pathname + "#group=" + id);
        }
    });

    if (location.pathname != pathname) {
        jq("#defaultLinkPeople").attr("href", pathname);
    }

    jq(".people-import-banner_img").on("click", function () {
        location.href = "/Products/People/Import.aspx";
    });
})