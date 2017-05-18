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
            PopupKeyUpActionProvider.ClearActions();
            jq("#otherActions").hide();
            StudioBlockUIManager.blockUI("#inviteLinkContainer", 550, 350, 0);
            ASC.InvitePanel.bindClipboardEvent();
        }
    };
})();

jq(function () {
    var pathname = "/products/people/";
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
        ImportUsersManager.ShowImportControl('ASC.Controls.AnchorController.trigger()');
    });
})