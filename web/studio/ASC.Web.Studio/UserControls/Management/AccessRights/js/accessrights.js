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


if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.Settings === "undefined")
    ASC.Settings = {};

ASC.Settings.AccessRights = new function() {

    var pNameList = [];

    var getSelectedUsers = function (pName) {
        return window["SelectedUsers_" + pName];
    };

    var getSelectedGroups = function (pName) {
        return window["SelectedGroups_" + pName];
    };

    var setSelectedUsers = function (pName, value) {
        window["SelectedUsers_" + pName] = value;
        return getSelectedUsers(pName);
    };

    var setSelectedGroups = function (pName, value) {
        window["SelectedGroups_" + pName] = value;
        return getSelectedGroups(pName);
    };

    return {
        init: function (products) {
            pNameList = products;

            jq("#changeOwnerBtn").click(ASC.Settings.AccessRights.changeOwner);
            jq("#adminTable tbody tr").remove();
            jq("#adminTmpl").tmpl(window.adminList, { isRetina: jq.cookies.get("is_retina") }).prependTo("#adminTable tbody");

            if (window.adminList.length) {
                jq("#adminTable").removeClass("display-none");
            }

            var items = jq("[id^=switcherAccessRights]"), 
                pName;

            for (var i = 0; i < items.length; i++) {
                pName = jq(items[i]).attr("data-id");
                jq.switcherAction("#switcherAccessRights_" + pName, "#accessRightsContainer_" + pName);
            }

            jq("#ownerSelector").on("click", initOwnerSelector);
            jq("#adminAdvancedSelector").on("click", initAdminSelector);

            function initOwnerSelector() {
                var os = jq(this);
                os.off("click", initOwnerSelector);
                os.useradvancedSelector({
                    itemsChoose: [],
                    itemsDisabledIds: [window.ownerId],
                    canadd: false,       // enable to create the new item
                    showGroups: true, // show the group list
                    onechosen: true,
                    withGuests: false
                });
                os.on("showList", function (event, item) {
                    os.html(item.title).attr("data-id", item.id);
                    jq("#changeOwnerBtn").removeClass("disable");
                });
                os.click();
            }

            function initAdminSelector() {
                var as = jq(this);
                var adminIds = [window.ownerId];
                window.adminList.forEach(function (admin) {
                    if (admin.id) adminIds.push(admin.id);
                });
                as.off("click", initAdminSelector);
                as.useradvancedSelector({
                    itemsDisabledIds: adminIds,
                    canadd: true,       // enable to create the new item
                    isAdmin: false, // show Admin only
                    showGroups: true,
                    withGuests: false
                });
                as.on("showList", function (event, admins) {
                    admins.forEach(function (admin) {
                        ASC.Settings.AccessRights.addAdmin(admin.id);
                    });
                });
                as.click();
            }
        },

        changeOwner: function() {
            var ownerId = jq("#ownerSelector").attr("data-id");
            var self = jq(this);
            if (ownerId == null || self.hasClass("disable")) return false;

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
                    self.addClass("disable");
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
                    return;
                }
                window.adminList.push(res.value);
                jq("#adminTmpl").tmpl(res.value, { isRetina: jq.cookies.get("is_retina") }).appendTo("#adminTable tbody");
                jq("#adminTable").removeClass("display-none");

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

        selectedItem_mouseOver: function() {
            var $obj = jq(this);
            $obj.find("img:first").hide();
            $obj.find("img:last").show();
        },

        selectedItem_mouseOut: function() {
            var $obj = jq(this);
            $obj.find("img:first").show();
            $obj.find("img:last").hide();
        },
        
        initProduct: function (productItem) {
            var pItem = jq.parseJSON(jq.base64.decode(productItem)),
                pId = pItem.ID,
                pName = pItem.ItemName,
                pIsPuplic = pItem.SelectedUsers.length == 0 && pItem.SelectedGroups.length == 0,
                su = setSelectedUsers(pName, {}),
                sg = setSelectedGroups(pName, {});

            var len = pItem.SelectedUsers.length;
            while (len--) {
                su[pItem.SelectedUsers[len].ID] = pItem.SelectedUsers[len].Name;
            }

            len = pItem.SelectedGroups.length;
            while (len--) {
                sg[pItem.SelectedGroups[len].ID] = pItem.SelectedGroups[len].Name;
            }

            jq.tmpl("template-productItem", pItem).appendTo("#studioPageContent .mainPageContent:first");

            var $container = jq("#accessRightsContainer_" + pName),
                $allRadio = $container.find("#all_" + pId),
                $fromListRadio = $container.find("#fromList_" + pId),
                $content = $container.find("#selectorContent_" + pName),
                $emptyLabel = $content.find("#emptyUserListLabel_" + pName),
                $selectedUsers = $content.find("#selectedUsers_" + pName),
                $selectedGroups = $content.find("#selectedGroups_" + pName),
                $userSelector = $content.find("#userSelector_" + pName),
                $groupSelector = $content.find("#groupSelector_" + pName);

            if (pIsPuplic) {
                $allRadio.prop("checked", true);
                $emptyLabel.show();
                $content.hide();
            } else {
                $fromListRadio.prop("checked", true);
                $emptyLabel.hide();
                $content.show();
            }

            $content.on("mouseover", ".accessRights-selectedItem", ASC.Settings.AccessRights.selectedItem_mouseOver);
            $content.on("mouseout", ".accessRights-selectedItem", ASC.Settings.AccessRights.selectedItem_mouseOut);
            $selectedUsers.on("click", "img[id^=deleteSelectedUserImg_]", ASC.Settings.AccessRights.deleteUserFromList);
            $selectedGroups.on("click", "img[id^=deleteSelectedGroupImg_]", ASC.Settings.AccessRights.deleteGroupFromList);
            $userSelector.on("click", initUserSelector);
            $groupSelector.on("click", initGroupSelector);

            function initUserSelector() {
                var us = jq(this);
                us.off("click", initUserSelector);
                us.useradvancedSelector({
                    showGroups: true,
                    withGuests: (pName !== "crm" && pName !== "people")
                }).on("showList", ASC.Settings.AccessRights.pushUserIntoList);
                us.useradvancedSelector("disable", Object.keys(su));
                us.click();
            }

            function initGroupSelector() {
                var gs = jq(this);
                gs.off("click", initGroupSelector);
                gs.groupadvancedSelector().on("showList", ASC.Settings.AccessRights.pushGroupIntoList);
                gs.groupadvancedSelector("disable", Object.keys(sg));
                gs.click();
            }
        },

        pushUserIntoList: function (event, users) {
            var pName = jq(this).attr("id").split('_')[1],
                pId = jq("#accessRightsContainer_" + pName).attr("data-id");

            var us = jq("#userSelector_" + pName);
            var su = getSelectedUsers(pName);

            users.forEach(function (el) {
                su[el.id] = el.title;

                var item = jq("<div></div>")
                    .attr("id", "selectedUser_" + pName + "_" + el.id)
                    .addClass("accessRights-selectedItem");

                var peopleImg = jq("<img>")
                    .attr("src", window.imageHelper.PeopleImgSrc);

                var deleteImg = jq("<img>")
                    .attr("src", window.imageHelper.TrashImgSrc)
                    .css("display", "none")
                    .attr("id", "deleteSelectedUserImg_" + pName + "_" + el.id)
                    .attr("title", window.imageHelper.TrashImgTitle);

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
                    us.useradvancedSelector("disable", Object.keys(su));
                }
            });
        },

        pushGroupIntoList: function (event, groups) {
            var pName = jq(this).attr("id").split('_')[1],
                pId = jq("#accessRightsContainer_" + pName).attr("data-id");

            var gs = jq("#groupSelector_" + pName);
            var sg = getSelectedGroups(pName);

            groups.forEach(function (group) {
                sg[group.id] = group.title;

                var item = jq("<div></div>")
                    .attr("id", "selectedGroup_" + pName + "_" + group.id)
                    .addClass("accessRights-selectedItem");

                var groupImg = jq("<img>")
                    .attr("src", window.imageHelper.GroupImgSrc);

                var deleteImg = jq("<img>")
                    .attr("src", window.imageHelper.TrashImgSrc)
                    .css("display", "none")
                    .attr("id", "deleteSelectedGroupImg_" + pName + "_" + group.id)
                    .attr("title", window.imageHelper.TrashImgTitle);

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
                    gs.groupadvancedSelector("disable", Object.keys(sg));
                }
            });
        },

        deleteUserFromList: function () {
            var $obj = jq(this);
            var idComponent = $obj.attr("id").split("_");
            var pName = idComponent[1];
            var uId = idComponent[2];
            var pId = jq("#accessRightsContainer_" + pName).attr("data-id");
            
            $obj.parent().remove();

            var sg = getSelectedGroups(pName);
            var su = getSelectedUsers(pName);

            var us = jq("#userSelector_" + pName);

            delete su[uId];

            if (!Object.keys(su).length && !Object.keys(sg).length)
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
                    if (us.data("useradvancedSelector"))
                        us.useradvancedSelector("undisable", [uId]);
                }
            });

        },

        deleteGroupFromList: function () {
            var $obj = jq(this);
            var idComponent = $obj.attr("id").split("_");
            var pName = idComponent[1];
            var gId = idComponent[2];
            var pId = jq("#accessRightsContainer_" + pName).attr("data-id");

            $obj.parent().remove();

            var sg = getSelectedGroups(pName);
            var su = getSelectedUsers(pName);

            var gs = jq("#groupSelector_" + pName);

            delete sg[gId];

            if (!Object.keys(su).length && !Object.keys(sg).length)
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
                    if (gs.data("groupadvancedSelector"))
                        gs.groupadvancedSelector("undisable", [gId]);
                }
            });
        },
        
        getSubjects: function(pName) {
            var su = getSelectedUsers(pName);
            var sg = getSelectedGroups(pName);
            return Object.keys(su).concat(Object.keys(sg));
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
                    if (us.data("useradvancedSelector"))
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
                        jq("#adminItem_" + data.userid + " input[type=checkbox]").prop("checked", false).attr("disabled", false);
                    }
                    ASC.Settings.AccessRights.hideUserFromAll(data.userid, data.administrator);
                }
            });
        },
        
        hideUserFromAll: function (uId) {
            for (var i = 0; i < pNameList.length; i++){
                var us = jq("#userSelector_" + pNameList[i]);
                if (us.data("useradvancedSelector"))
                    us.useradvancedSelector("disable", [uId]);
            }
        }
    };
    
};
