/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
        var $o = jq.tmpl("userListTemplate", { users: [result.Data], isAdmin: jq.profile.isAdmin });

        if (jq("#peopleData tr.profile").length == 0) {
            jq("#emptyContentForPeopleFilter").addClass("display-none");
            jq("#peopleContent").removeClass("display-none");
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
            StudioBlockUIManager.blockUI("#confirmationDeleteDepartmentPanel", 500, 200, 0);
            PopupKeyUpActionProvider.ClearActions();
            PopupKeyUpActionProvider.EnterAction = 'ASC.People.PeopleController.deleteGroup();';
        },

        add_profiles: function(evt, $btn) {
            ImportUsersManager.ShowImportControl('ASC.Controls.AnchorController.trigger()');
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
            StudioBlockUIManager.blockUI("#inviteLinkContainer", 550, 350, 0);
            PopupKeyUpActionProvider.ClearActions();
            ASC.People.InviteLink.init();
        }
    };
})();

jq(function () {
    jq("#defaultLinkPeople").on("click", function () {
        var pathname = "/products/people/";
        if (location.pathname == pathname) {
            var oldAnchor = jq.anchorToObject(ASC.Controls.AnchorController.getAnchor());
            if (oldAnchor.hasOwnProperty("group")) {
                delete oldAnchor.group;
                ASC.Controls.AnchorController.move(jq.objectToAnchor(oldAnchor));
            }
            ASC.People.PeopleController.moveToPage("1");
        }
        else {
            location.href = pathname;
        }
    });

    jq("#groupList").on("click", ".menu-item-label", function () {
        var pathname = "/products/people/",
            id = jq(this).parents(".menu-sub-item").attr("data-id"),
            oldAnchor = jq.anchorToObject(ASC.Controls.AnchorController.getAnchor()),
            newAnchor = jq.mergeAnchors(oldAnchor, jq.anchorToObject("group=" + id));
        if (location.pathname == pathname) {
            if (!jq.isEqualAnchors(newAnchor, oldAnchor)) {
                ASC.Controls.AnchorController.move(jq.objectToAnchor(newAnchor));
            }
        }
        else {
            location.href = pathname + "#group=" + id;
        }
    });

    jq(".people-import-banner_img").on("click", function () {
        ImportUsersManager.ShowImportControl('ASC.Controls.AnchorController.trigger()');
    });
})