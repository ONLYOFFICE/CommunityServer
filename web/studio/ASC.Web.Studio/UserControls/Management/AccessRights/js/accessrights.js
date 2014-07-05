/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.Settings === "undefined")
    ASC.Settings = {};

ASC.Settings.AccessRights = new function() {

    var pNameList = [];
    var gsLinkText = "";
    

    var getSelectedUsers = function (pName) {
        return window["SelectedUsers_" + pName];
    };
    var getSelectedGroups = function (pName) {
        return window["SelectedGroups_" + pName];
    };

    return {
        init: function (products, linkText) {
            pNameList = products;
            gsLinkText = linkText;

            jq("#changeOwnerBtn").click(ASC.Settings.AccessRights.changeOwner);
            jq("#adminTable tbody tr").remove();
            jq("#adminTmpl").tmpl(window.adminList).prependTo("#adminTable tbody");


            var items = jq("[id^=switcherAccessRights]"), 
                pName;

            for (var i = 0; i < items.length; i++) {
                pName = jq(items[i]).attr("data-id");
                jq.switcherAction("#switcherAccessRights_" + pName, "#accessRightsContainer_" + pName);
            }

            var adminIds = [],
                adminsSelector = [];

            window.adminList.forEach(function (admin) {
                if (admin.id) adminIds.push(admin.id);
            });

            var $ownerSelector = jq("#ownerSelector");
            $ownerSelector.useradvancedSelector({
                itemsChoose: [],
                itemsDisabledIds: [window.ownerId],
                canadd: false,       // enable to create the new item
                showGroups: true, // show the group list
                onechosen: true,
                withGuests: false
            });

            $ownerSelector.on("showList", function (event, item) {
                jq(this).html(item.title).attr("data-id", item.id);
                jq("#changeOwnerBtn").removeClass("disable");
            });

            var $adminAdvancedSelector = jq("#adminAdvancedSelector");
            $adminAdvancedSelector.useradvancedSelector({
                itemsDisabledIds: adminIds,
                canadd: true,       // enable to create the new item
                isAdmin: false, // show Admin only
                showGroups: true,
                withGuests: false
            });

            $adminAdvancedSelector.on("showList", function (event, items) {
                var adminIds = [],
                    itemsIds = [];
                window.adminList.forEach(function (admin) {
                    if (admin.id) adminIds.push(admin.id);
                });
                items.forEach(function (item) {
                    itemsIds.push(item.id);
                });

                for (var j = 0, l = itemsIds.length; j < l; j++) {
                    ASC.Settings.AccessRights.addAdmin(items[j].id);
                }

            })
        },

        changeOwner: function() {
            var ownerId = jq("#ownerSelector").attr("data-id");

            if (ownerId == null) return false;

            window.AjaxPro.onLoading = function(b) {
                if (b)
                    LoadingBanner.displayLoading();
                else
                    LoadingBanner.hideLoading();
            };

            window.AccessRightsController.ChangeOwner(ownerId, function(result) {
                var res = result.value;
                if (res.Status == 1) {
                    toastr.success(res.Message);
                }
                else {
                    toastr.error(res.Message);
                }
            });
            return false;
        },

        addAdmin: function(uId) {
            window.AccessRightsController.AddAdmin(uId, function(res) {
                if (res.error != null) {
                    toastr.error(res.error.Message);
                    return false;
                }
                window.adminList.push(res.value);
                jq("#adminTmpl").tmpl(res.value).appendTo("#adminTable tbody");

                jq("#adminAdvancedSelector").useradvancedSelector("disable", [uId]);
                ASC.Settings.AccessRights.hideUserFromAll(uId, true);
            });
        },

        changeAccessType: function(obj, pName) {

            var subjects = ASC.Settings.AccessRights.getSubjects(pName);

            var type = jq(obj).attr("id").split("_")[0];
            var id = jq(obj).attr("id").split("_")[1];
            var params = {};
            var data = {
                id: id,
                enabled: true
            };

            if (type == "all") {
                Teamlab.setWebItemSecurity(params, data, {
                    success: function() {
                        jq("#selectorContent_" + pName).hide();
                    }
                });
            } else {
                if (subjects.length == 0)
                    data.enabled = false;
                if (subjects.length > 0)
                    data.subjects = subjects;
                Teamlab.setWebItemSecurity(params, data, {
                    success: function() {
                        jq("#selectorContent_" + pName).show();
                    }
                });
            }
        },

        selectedItem_mouseOver: function(obj) {
            jq(obj).find("img:first").hide();
            jq(obj).find("img:last").show();
        },

        selectedItem_mouseOut: function(obj) {
            jq(obj).find("img:first").show();
            jq(obj).find("img:last").hide();
        },
        
        initProduct: function (pId, pName, pIsPuplic) {

            var us = jq("#userSelector_" + pName);
            var gs = jq("#groupSelector_" + pName);

            var su = getSelectedUsers(pName);
            var sg = getSelectedGroups(pName);

            us.useradvancedSelector({
                showGroups: true,
                withGuests: (pId !== ("6743007c-6f95-4d20-8c88-a8601ce5e76d" || "f4d98afd-d336-4332-8778-3c6945c81ea0"))
            }).on("showList", ASC.Settings.AccessRights.pushUserIntoList)

            gs.groupadvancedSelector({
                witheveryone: true
            }).on("showList", ASC.Settings.AccessRights.pushGroupIntoList)

            us.useradvancedSelector("disable", su.IDs);
            gs.groupadvancedSelector("disable", sg.IDs);

            if (pIsPuplic) {
                jq("#all_" + pId).prop("checked", true);
                jq("#emptyUserListLabel_" + pName).show();
                jq("#selectorContent_" + pName).hide();
            } else {
                jq("#fromList_" + pId).prop("checked", true);
                jq("#emptyUserListLabel_" + pName).hide();
                jq("#selectorContent_" + pName).show();
            }
            
            jq("#selectorContent_" + pName).on("mouseover", ".accessRights-selectedItem", function () {
                ASC.Settings.AccessRights.selectedItem_mouseOver(jq(this));
                return false;
            });
            jq("#selectorContent_" + pName).on("mouseout", ".accessRights-selectedItem", function () {
                ASC.Settings.AccessRights.selectedItem_mouseOut(jq(this));
                return false;
            });
            jq("#selectedUsers_" + pName).on("click", "img[id^=deleteSelectedUserImg_]", function () {
                ASC.Settings.AccessRights.deleteUserFromList(jq(this));
                return false;
            });
            jq("#selectedGroups_" + pName).on("click", "img[id^=deleteSelectedGroupImg_]", function () {
                ASC.Settings.AccessRights.deleteGroupFromList(jq(this));
                return false;
            });
        },
        
        pushUserIntoList: function (event, users) {
            var pName = jq(this).attr("id").split('_')[1],
                pId = jq("#accessRightsContainer_" + pName).attr("data-id");

            var us = jq("#userSelector_" + pName);
            var su = getSelectedUsers(pName);

            users.forEach(function (el) {
                su.IDs.push(el.id);
                su.Names.push(el.title);

                var item = jq("<div></div>")
                    .attr("id", "selectedUser_" + pName + "_" + el.id)
                    .addClass("accessRights-selectedItem");

                var peopleImg = jq("<img>")
                    .attr("src", su.PeopleImgSrc);

                var deleteImg = jq("<img>")
                        .attr("src", su.TrashImgSrc)
                        .css("display", "none")
                        .attr("id", "deleteSelectedUserImg_" + pName + "_" + el.id)
                        .attr("title", su.TrashImgTitle);

                item.append(peopleImg).append(deleteImg).append(Encoder.htmlEncode(el.title));

                jq("#selectedUsers_" + pName).append(item);
            });

            jq("#emptyUserListLabel_" + pName).hide();
            jq("#selectedUsers_" + pName).parent().find("div.adv-userselector-DepsAndUsersContainer").hide();

            var data = {
                id: pId,
                enabled: true
            };

            var subjects = ASC.Settings.AccessRights.getSubjects(pName);

            if (subjects.length > 0)
                data.subjects = subjects;

            Teamlab.setWebItemSecurity({}, data, {
                success: function () {
                    us.useradvancedSelector("disable", su.IDs);
                }
            });
        },

        pushGroupIntoList: function (event, groups) {
            var pName = jq(this).attr("id").split('_')[1],
                pId = jq("#accessRightsContainer_" + pName).attr("data-id");

            var gs = jq("#groupSelector_" + pName);
            var sg = getSelectedGroups(pName);

            groups.forEach(function (group) {
                sg.IDs.push(group.id);
                sg.Names.push(group.title);

                var item = jq("<div></div>")
                    .attr("id", "selectedGroup_" + pName + "_" + group.id)
                    .addClass("accessRights-selectedItem");

                var groupImg = jq("<img>")
                    .attr("src", sg.GroupImgSrc);

                var deleteImg = jq("<img>")
                    .attr("src", sg.TrashImgSrc)
                    .css("display", "none")
                    .attr("id", "deleteSelectedGroupImg_" + pName + "_" + group.id)
                    .attr("title", sg.TrashImgTitle);

                item.append(groupImg).append(deleteImg).append(Encoder.htmlEncode(group.title));

                jq("#selectedGroups_" + pName).append(item);
            });

            jq("#emptyUserListLabel_" + pName).hide();
            jq("#selectedUsers_" + pName).parent().find("div[id^=groupSelectorContainer_]").hide();

            var data = {
                id: pId,
                enabled: true
            };

            var subjects = ASC.Settings.AccessRights.getSubjects(pName);

            if (subjects.length > 0)
                data.subjects = subjects;

            Teamlab.setWebItemSecurity({}, data, {
                success: function () {
                    gs.groupadvancedSelector("disable", sg.IDs);
                }
            });
        },
        
        deleteUserFromList: function (obj) {
            var idComponent = jq(obj).attr("id").split("_");
            var pName = idComponent[1];
            var uId = idComponent[2];
            var pId = jq("#accessRightsContainer_" + pName).attr("data-id");
            
            jq(obj).parent().remove();

            var sg = getSelectedGroups(pName);
            var su = getSelectedUsers(pName);

            var us = jq("#userSelector_" + pName);

            for (var i = 0; i < su.IDs.length; i++) {
                if (su.IDs[i] == uId) {
                    su.IDs.splice(i, 1);
                    su.Names.splice(i, 1);
                    break;
                }
            }

            if (su.IDs.length == 0 && sg.IDs.length == 0)
                jq("#emptyUserListLabel_" + pName).show();

            var data = {
                id: pId,
                enabled: true
            };

            var subjects = ASC.Settings.AccessRights.getSubjects(pName);

            if (subjects.length > 0) {
                data.subjects = subjects;
            } else {
                data.enabled = false;
            }

            Teamlab.setWebItemSecurity({}, data, {
                success: function () {
                    us.useradvancedSelector("undisable", [uId]);
                }
            });

        },
        
        deleteGroupFromList: function (obj) {
            var idComponent = jq(obj).attr("id").split("_");
            var pName = idComponent[1];
            var gId = idComponent[2];
            var pId = jq("#accessRightsContainer_" + pName).attr("data-id");

            jq(obj).parent().remove();

            var sg = getSelectedGroups(pName);
            var su = getSelectedUsers(pName);

            var gs = jq("#groupSelector_" + pName);

            for (var i = 0; i < sg.IDs.length; i++) {
                if (sg.IDs[i] == gId) {
                    sg.IDs.splice(i, 1);
                    sg.Names.splice(i, 1);
                    break;
                }
            }

            if (su.IDs.length == 0 && sg.IDs.length == 0)
                jq("#emptyUserListLabel_" + pName).show();

            var data = {
                id: pId,
                enabled: true
            };

            var subjects = ASC.Settings.AccessRights.getSubjects(pName);

            if (subjects.length > 0) {
                data.subjects = subjects;
            } else {
                data.enabled = false;
            }

            Teamlab.setWebItemSecurity({}, data, {
                success: function () {
                    gs.groupadvancedSelector("undisable", [gId]);
                }
            });
        },
        
        getSubjects: function(pName) {
            var su = getSelectedUsers(pName);
            var sg = getSelectedGroups(pName);
            return su.IDs.concat(sg.IDs);
        },
        
        setAdmin: function (obj, pId) {
            var idComponent = jq(obj).attr("id").split("_"),
                pName = idComponent[1],
                uId = idComponent[2],
                isChecked = jq(obj).is(":checked"),
                data = {
                   productid: pId,
                   userid: uId,
                   administrator: isChecked
                };

            if (pName == "full") {
                ASC.Settings.AccessRights.setFullAdmin(obj, data);
            } else {
                ASC.Settings.AccessRights.setProductAdmin(pName, data);
            }
        },
        
        setProductAdmin: function (pName, data) {
            Teamlab.setProductAdministrator({}, data, {
                success: function () {
                    var us = jq("#userSelector_" + pName);
                    us.useradvancedSelector("disable", [data.userid]);
                }
            });
        },

        setFullAdmin: function (obj, data) {
            Teamlab.setProductAdministrator({}, data, {
                success: function() {
                    if (data.administrator) {
                        jq("#adminItem_" + data.userid + " input[type=checkbox]").prop("checked", true).attr("disabled", true);
                        jq(obj).removeAttr("disabled");
                    } else {
                        jq("#adminItem_" + data.userid + " input[type=checkbox]").removeAttr("checked").removeAttr("disabled");
                    }
                    ASC.Settings.AccessRights.hideUserFromAll(data.userid, data.administrator);
                }
            });
        },
        
        hideUserFromAll: function (uId, hide) {
            for (var i = 0; i < pNameList.length; i++){
                var us = jq("#userSelector_" + pNameList[i]); 
                us.useradvancedSelector("disable", [uId]);
            }
        }
    };
    
};
