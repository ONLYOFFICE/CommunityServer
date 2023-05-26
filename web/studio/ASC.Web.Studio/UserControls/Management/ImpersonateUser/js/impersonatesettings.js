/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


window.ASC.Controls.ImpersonateSettings = new function () {

    var mainTag = jq('#studio_impersonateSettings'),
        settingsBlock = mainTag.find('.settings-block'),
        adminSelectorTag = mainTag.find("#adminSelector"),
        userSelectorTag = mainTag.find("#userSelector"),
        groupSelectorTag = mainTag.find("#groupSelector"),
        selectedAdminsTag = mainTag.find('#selectedAdmins'),
        selectedUsersTag = mainTag.find('#selectedUsers'),
        selectedGroupsTag = mainTag.find('#selectedGroups'),
        settingsOn = mainTag.find('#impersonateSettingsOn'),
        settingsOnWithLimits = mainTag.find('#impersonateSettingsOnWithLimits'),
        settingsOff = mainTag.find('#impersonateSettingsOff'),
        onlyForOwnGroupsChb = mainTag.find('#onlyForOwnGroups'),
        restrictionsAccess = mainTag.find('#restrictions-access'),
        adminSelector = mainTag.find('#impersonate-adminSelctorContent'),
        userSelector = mainTag.find('#impersonate-userSelectorContent'),
        groupSelector = mainTag.find('#impersonate-groupSelectorContent'),
        userRestrictionsChb = mainTag.find('#userRestrictions'),
        impersonateSettingsChb = mainTag.find('#impersonateSettingsCheckbox'),
        saveBtn = mainTag.find("#impersonateSettingSaveBtn"),
        settingsSwitcher = mainTag.find(".switcher")


    var enableType = {
        disableForAdmins: 0,
        enableForAll: 1,
        enableWithLimits: 2
    }

    function clickUserRestrictionsChb() {
        if (userRestrictionsChb.is(':checked')) {
            userSelector.show();
            groupSelector.show();
        }
        else {
            userSelector.hide();
            groupSelector.hide();
        }
    }

    return {

        Init: function () {

            bindEvents();

            getSettings(function (params, data) {

                if (!data.enabled) {
                    settingsOff.trigger("click");
                    initSelectors();
                    impersonateSettingsChb.trigger("click");
                    return;
                }

                if (data.enableType == enableType.disableForAdmins) {
                    initSelectors();
                    settingsOff.trigger("click");
                    return;
                }
                else if (data.enableType == enableType.enableForAll) {
                    initSelectors([], data.restrictionUsers, data.restrictionGroups);
                    settingsOn.trigger("click");
                }
                else {
                    initSelectors(data.allowedAdmins, data.restrictionUsers, data.restrictionGroups);
                    settingsOnWithLimits.trigger("click");
                }

                if (data.onlyForOwnGroups == true) {
                    onlyForOwnGroupsChb.prop('checked', true);
                }

                if (data.restrictionUsers && data.restrictionUsers.length || data.restrictionGroups && data.restrictionGroups.length) {
                    userRestrictionsChb.trigger("click");
                }
            })

            function initSelectors(adminIds, userIds, groupIds) {
                initAdminSelector(adminIds);
                initUserSelector(userIds);
                initGroupSelector(groupIds);
            }

            function initGroupSelector(groupIds = []) {

                groupSelectorTag.groupadvancedSelector()
                    .on("showList", function (event, items) {
                        pushGroupsIntoList(items)
                    });

                if (groupIds.length) {
                    groupSelectorTag.groupadvancedSelector("disable", groupIds);

                    var groups = GroupManager.getGroups(groupIds);
                    pushGroupsIntoList(groups);
                }
            }

            function initUserSelector(userIds = []) {

                var adminsIds = window.adminList.map(e => e.id);
                adminsIds.push(window.ownerId);

                userSelectorTag.useradvancedSelector({
                    itemsDisabledIds: adminsIds,
                    showGroups: false,
                }).on("showList", function (event, items) {
                    pushUsersIntoList(items)
                });

                if (userIds.length) {
                    userSelectorTag.useradvancedSelector("disable", userIds);

                    var users = UserManager.getUsers(userIds);
                    pushUsersIntoList(users);
                }
            }

            function initAdminSelector(adminIds = []) {

                var fullAdmins = [];

                window.adminList.forEach(e => {
                    if (e.accessList[0].pAccess) {
                        fullAdmins.push({ id: e.id, title: e.displayName });
                    }
                })

                adminSelectorTag.useradvancedSelector("rewriteItemList", fullAdmins, [])
                    .on("showList", function (event, items) {
                        pushAdminsIntoList(items)
                    });

                if (adminIds.length) {
                    adminSelectorTag.useradvancedSelector("disable", adminIds);

                    var admins = UserManager.getUsers(adminIds);
                    pushAdminsIntoList(admins);
                }
            }

            function pushGroupsIntoList(groups) {
                pushItemsIntoList(groups, "group", selectedGroupsTag, groupSelectorTag);
            }

            function pushUsersIntoList(users) {
                pushItemsIntoList(users, "user", selectedUsersTag, userSelectorTag);
            }

            function pushAdminsIntoList(admins) {
                pushItemsIntoList(admins, "admin", selectedAdminsTag, adminSelectorTag);
                enableSaveBtn();
            }

            function pushItemsIntoList(items, type, container, selector) {
                var itemsForDisable = [];

                items.forEach(function (item) {

                    itemsForDisable.push(item.id);

                    var div = jq("<div></div>")
                        .attr("id", item.id)
                        .attr("class", "selecteditem " + type + "-item");

                    var typeImg = jq("<div></div>")
                        .attr("class", "image " + type + "-image");

                    var deleteImg = jq("<div></div>")
                        .attr("class", "image trash-image")
                        .css("display", "none");

                    div.append(typeImg).append(deleteImg).append(Encoder.htmlEncode(item.title || item.displayName || item.name));

                    container.append(div);
                });

                selector.useradvancedSelector("disable", itemsForDisable);
            }

            function deleteGroupFromList() {
                var id = deleteItemFromList(this);
                groupSelectorTag.groupadvancedSelector("undisable", [id]);
            }

            function deleteUserFromList() {
                var id = deleteItemFromList(this);
                userSelectorTag.useradvancedSelector("undisable", [id]);
            }

            function deleteAdminFromList() {
                var id = deleteItemFromList(this);
                var selectedAdmins = mainTag.find(".admin-item");

                if (!selectedAdmins.length) {
                    disableSaveBtn();
                }

                adminSelectorTag.useradvancedSelector("undisable", [id]);
            }

            function deleteItemFromList(element) {
                var parent = jq(element).parent();
                var id = parent.attr("id");
                parent.remove();
                return id;
            }

            function selectedItem_mouseOver() {
                var obj = jq(this);
                obj.find(".admin-image,.user-image,.group-image").hide();
                obj.find(".trash-image").show();
            }

            function selectedItem_mouseOut() {
                var obj = jq(this);
                obj.find(".admin-image,.user-image,.group-image").show();
                obj.find(".trash-image").hide();
            }

            function bindEvents() {

                settingsOff.on("click", disableSettings);
                settingsOn.on("click", enableSettingsForAllAdmins);
                settingsOnWithLimits.on("click", enableSettingsWithLimits);

                userRestrictionsChb.on("click", clickUserRestrictionsChb);

                settingsBlock.on("mouseover", ".selecteditem", selectedItem_mouseOver);
                settingsBlock.on("mouseout", ".selecteditem", selectedItem_mouseOut);

                selectedUsersTag.on("click", ".trash-image", deleteUserFromList);
                selectedAdminsTag.on("click", ".trash-image", deleteAdminFromList);
                selectedGroupsTag.on("click", ".trash-image", deleteGroupFromList);

                settingsSwitcher.on("click", refreshSaveBtnStatus);

                impersonateSettingsChb.on("click", onEnableImpersonateSettings);

                jq.switcherAction("#impersonateSettingsSpoiler", "#impersonateSettingsSpoilerBlock");
            }

            function onEnableImpersonateSettings() {

                var on = jq(this).hasClass("off");

                impersonateSettingsChb.toggleClass("off", !on).toggleClass("on", on);

                settingsBlock.toggleClass("disabled");

                if (settingsBlock.hasClass("disabled")) {
                    settingsBlock.find("input").prop("disabled", true);
                    adminSelector.hide();
                    restrictionsAccess.hide();
                }
                else {
                    settingsBlock.find("input").prop("disabled", false);

                    if (settingsOff.is(":checked")) return;

                    if (settingsOnWithLimits.is(":checked")) {
                        adminSelector.show();
                    }

                    restrictionsAccess.show();
                }
            }

            function getSettings(callback) {

                showLoader();

                Teamlab.getImpersonateSettings({
                    success: function (params, data) {
                        hideLoader();
                        callback(params, data);
                    },
                    error: function () {
                        hideLoader();
                        showErrorMessage();
                    }
                });

                function showLoader() {
                    LoadingBanner.showLoaderBtn(mainTag);
                }

                function hideLoader() {
                    LoadingBanner.hideLoaderBtn(mainTag);
                }

                function showErrorMessage() {
                    LoadingBanner.showMesInfoBtn(mainTag, ASC.Resources.Master.ResourceJS.CommonJSErrorMsg, 'error');
                }
            }

            function disableSettings() {
                restrictionsAccess.hide();
                adminSelector.hide();
            }

            function enableSettingsForAllAdmins() {
                restrictionsAccess.show();
                adminSelector.hide();
            }

            function enableSettingsWithLimits() {
                restrictionsAccess.show();
                adminSelector.show();
            }

            function refreshSaveBtnStatus() {

                var withAdminsLimit = settingsOnWithLimits.is(":checked");

                if (withAdminsLimit) {
                    var selectedAdmins = mainTag.find(".admin-item");

                    if (selectedAdmins.length) {
                        enableSaveBtn();
                    }
                    else
                    {
                        disableSaveBtn();
                    }
                }
                else {
                    enableSaveBtn();
                }
            }

            function disableSaveBtn() {

                var isDisabled = saveBtn.hasClass('disable');

                if (!isDisabled) {
                    saveBtn.addClass('disable');
                    saveBtn.off('click');
                }
            }

            function enableSaveBtn() {

                var isDisabled = saveBtn.hasClass('disable');

                if (isDisabled) {
                    saveBtn.removeClass('disable');
                    saveBtn.on("click", ASC.Controls.ImpersonateSettings.SaveSettings);
                }
            }
        },

        SaveSettings: function () {

            var callback = {
                before: function () {
                    LoadingBanner.showLoaderBtn(mainTag);
                },
                success: function () {
                    LoadingBanner.showMesInfoBtn(mainTag, ASC.Resources.Master.ResourceJS.SuccessfullySaveSettingsMessage, "success");
                },
                error: function (_, error) {
                    LoadingBanner.showMesInfoBtn(mainTag, error[0], "error");
                },
                after: function () {
                    LoadingBanner.hideLoaderBtn(mainTag);
                }
            };

            var disabled = impersonateSettingsChb.hasClass("off");

            if (disabled) {
                Teamlab.updateImpersonateSettings(false, enableType.disableForAdmins, false, [], [], [], callback);
                return;
            }

            var disabledForAdmins = settingsOff.is(":checked");

            if (disabledForAdmins) {
                Teamlab.updateImpersonateSettings(true, enableType.disableForAdmins, false, [], [], [], callback);
                return;
            }

            var adminsForSave = [];
            var withAdminsLimit = settingsOnWithLimits.is(":checked");

            if (withAdminsLimit) {
                var selectedAdmins = mainTag.find(".admin-item");

                selectedAdmins.each(function (idx, el) {
                    adminsForSave.push(jq(el).attr("id"));
                });
            }

            var usersForSave = [];
            var groupsForSave = [];
            var withUsersLimit = userRestrictionsChb.is(":checked");

            if (withUsersLimit) {
                var selectedUsers = mainTag.find(".user-item");

                selectedUsers.each(function (idx, el) {
                    usersForSave.push(jq(el).attr("id"));
                });

                var selectedGroups = mainTag.find(".group-item");

                selectedGroups.each(function (idx, el) {
                    groupsForSave.push(jq(el).attr("id"));
                });

                if (!selectedUsers.length && !selectedGroups.length) {
                    userRestrictionsChb.trigger("click");
                }
            }

            var onlyForOwnGroups = onlyForOwnGroupsChb.is(":checked");

            if (withAdminsLimit) {
                Teamlab.updateImpersonateSettings(true, enableType.enableWithLimits, onlyForOwnGroups, adminsForSave, usersForSave, groupsForSave, callback);
            } else {
                Teamlab.updateImpersonateSettings(true, enableType.enableForAll, onlyForOwnGroups, adminsForSave, usersForSave, groupsForSave, callback);
            }
        }
    };
};

(function () {
    jq(function () {
        ASC.Controls.ImpersonateSettings.Init();

        jq("#impersonateSettingSaveBtn").on("click", ASC.Controls.ImpersonateSettings.SaveSettings);
    });
})();