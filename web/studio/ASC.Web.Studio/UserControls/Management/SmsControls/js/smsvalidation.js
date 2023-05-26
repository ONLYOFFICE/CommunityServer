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


;
window.ASC.Controls.SmsValidationSettings = new function () {

    var commonIpRegex = /^(\s*(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\s*(\-\s*(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\s*)?)$/;
    var CIDRIpRegex = /^(\s*(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\s*(\/(3[012]|[12]?[0-9]))\s*)$/;

    var mainTag = jq('#studio_smsValidationSettings'),
        smsAuthCbx = mainTag.find('#chk2FactorAuthEnable'),
        appAuthCbx = mainTag.find('#chk2FactorAppAuthEnable'),
        restrictionsList = mainTag.find('#restrictions-list'),
        restrictionTmpl = mainTag.find('#restriction-tmpl'),
        addRestrictionBtn = mainTag.find('#add-restriction-btn'),
        emptyList = mainTag.find("#emptyUserListLabel"),
        showView = mainTag.find('#show-view'),
        hideView = mainTag.find('#hide-view'),
        userSelectorTag = mainTag.find("#userSelector"),
        groupSelectorTag = mainTag.find("#groupSelector"),
        selectedUsersTag = mainTag.find('#selectedUsers'),
        selectedGroupsTag = mainTag.find('#selectedGroups');

    var tfaType = {
        none: 0,
        sms: 1,
        app: 2
    }

    var isOpen = false;

    return {

        ShowAdvancedSettings : function () {
            showView.show();
            hideView.hide();
            isOpen = true;
        },

        ShowAdvancedSettingsButton: function () {
            showView.hide();
            hideView.show();
            isOpen = false;
        },

        HideAdvancedSettings: function () {
            showView.hide();
            hideView.hide();
        },

        RefreshAdvancedSettingsView: function () {
            if (smsAuthCbx.is(':checked') || appAuthCbx.is(':checked')) {
                if (isOpen) {
                    ASC.Controls.SmsValidationSettings.ShowAdvancedSettings();
                } else {
                    ASC.Controls.SmsValidationSettings.ShowAdvancedSettingsButton();
                }
            } else {
                ASC.Controls.SmsValidationSettings.HideAdvancedSettings();
            }
        },

        InitMandatoryBlock: function (users = [], groups = []) {

            initUserSelector(users);
            initGroupSelector(groups);

            showView.on("mouseover", ".SelectedItem", selectedItem_mouseOver);
            showView.on("mouseout", ".SelectedItem", selectedItem_mouseOut);

            selectedUsersTag.on("click", "div[id^=deleteSelectedUserImg_]", deleteUserFromList);
            selectedGroupsTag.on("click", "div[id^=deleteSelectedGroupImg_]", deleteGroupFromList);

            function initUserSelector(userIds) {

                userSelectorTag.useradvancedSelector({
                    showGroups: true,
                }).on("showList", function (event, items) {
                    pushUsersIntoList(items)
                });

                if (userIds.length) {
                    userSelectorTag.useradvancedSelector("disable", userIds);

                    var users = UserManager.getUsers(userIds);
                    pushUsersIntoList(users);
                }
            }

            function initGroupSelector(groupIds) {

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

            function pushUsersIntoList(users) {

                users.forEach(function (user) {

                    var item = jq("<div></div>")
                        .attr("id", user.id)
                        .addClass("SelectedItem")
                        .addClass("user-item");

                    var peopleImg = jq("<div></div>")
                        .attr("class", "people-image")
                        .addClass("image");

                    var deleteImg = jq("<div></div>")
                        .attr("class", "trash-image")
                        .addClass("image")
                        .css("display", "none")
                        .attr("id", "deleteSelectedUserImg_" + user.id);

                    item.append(peopleImg).append(deleteImg).append(Encoder.htmlEncode(user.title || user.displayName));

                    selectedUsersTag.append(item);
                });

                emptyList.hide();

                var usersForDisable = users.map(a => a.id);
                userSelectorTag.useradvancedSelector("disable", usersForDisable);
            }

            function pushGroupsIntoList(groups) {

                groups.forEach(function (group) {

                    var item = jq("<div></div>")
                        .attr("id", group.id)
                        .addClass("SelectedItem")
                        .addClass("group-item");

                    var groupImg = jq("<div></div>")
                        .attr("class", "group-image")
                        .addClass("image");

                    var deleteImg = jq("<div></div>")
                        .attr("class", "trash-image")
                        .css("display", "none")
                        .attr("id", "deleteSelectedGroupImg_" + group.id)
                        .addClass("image");

                    item.append(groupImg).append(deleteImg).append(Encoder.htmlEncode(group.title || group.name));

                    selectedGroupsTag.append(item);
                });

                emptyList.hide();

                var groupForDisable = groups.map(a => a.id);
                groupSelectorTag.groupadvancedSelector("disable", groupForDisable);
            }

            function deleteUserFromList() {
                var obj = jq(this);
                var idComponent = obj.attr("id").split("_");
                var uId = idComponent[1];

                obj.parent().remove();

                if (!getSelectedUsers().length && !getSelectedGroups().length)
                    emptyList.show();

                userSelectorTag.useradvancedSelector("undisable", [uId]);

            }

            function deleteGroupFromList() {
                var obj = jq(this);
                var idComponent = obj.attr("id").split("_");
                var gId = idComponent[1];

                obj.parent().remove();

                if (!getSelectedGroups().length && !getSelectedUsers().length)
                    emptyList.show();

                groupSelectorTag.groupadvancedSelector("undisable", [gId]);
            }

            function getSelectedUsers() {
                return selectedUsersTag.find(".user-item");
            }

            function getSelectedGroups() {
                return selectedGroupsTag.find(".group-item");
            }

            function selectedItem_mouseOver() {
                var obj = jq(this);
                obj.find(".group-image,.people-image").hide();
                obj.find(".trash-image").show();
            }

            function selectedItem_mouseOut() {
                var obj = jq(this);
                obj.find(".group-image,.people-image").show();
                obj.find(".trash-image").hide();
            }
        },

        InitTrustedIpBlock: function(data = []) {

            var userRestrictions = data.map(it => new Object({ ip: it }));

            bindEvents();
            renderView();

            function bindEvents() {
                addRestrictionBtn.on('click', addRestriction);
                restrictionsList.on('click', '.restriction .delete-btn', deleteRestriction);
            }

            function addRestriction() {
                var newRestriction = restrictionTmpl.tmpl();
                addRestrictionBtn.before(newRestriction);
            }

            function renderView() {
                var restrictions = restrictionTmpl.tmpl(userRestrictions);
                addRestrictionBtn.before(restrictions);
            }

            function deleteRestriction() {
                jq(this).closest('.restriction').remove();
            }
        },

        InitAdvancedSettings: function () {

            getSettings(function (params, data) {
                if (data.tfaSettings) {
                    window.ASC.Controls.SmsValidationSettings.InitMandatoryBlock(data.tfaSettings.mandatoryUsers, data.tfaSettings.mandatoryGroups);
                    window.ASC.Controls.SmsValidationSettings.InitTrustedIpBlock(data.tfaSettings.trustedIps);
                }
                else {
                    window.ASC.Controls.SmsValidationSettings.InitMandatoryBlock();
                    window.ASC.Controls.SmsValidationSettings.InitTrustedIpBlock();
                }
            })

            ASC.Controls.SmsValidationSettings.RefreshAdvancedSettingsView();

            function getSettings(callback) {

                var smsDisabled = smsAuthCbx.is(':disabled');
                var appDisabled = appAuthCbx.is(':disabled');

                if (smsDisabled && appDisabled) {
                    return;
                }

                var smsChecked = smsAuthCbx.is(':checked');
                var appChecked = appAuthCbx.is(':checked');

                if (smsChecked || appChecked) {
                    showLoader();
                }

                Teamlab.getTfaSettings({
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
        },

        SaveSettings: function () {

            var callback = {
                before: function showLoader() {
                    LoadingBanner.showLoaderBtn(mainTag);
                },
                success: function (_, data) {
                    LoadingBanner.showMesInfoBtn(mainTag, ASC.Resources.Master.ResourceJS.SuccessfullySaveSettingsMessage, "success");
                    if (data) {
                        window.location.reload(true);
                    }
                    else {
                        LoadingBanner.hideLoaderBtn(mainTag);
                    }
                },
                error: function (_, error) {
                    LoadingBanner.hideLoaderBtn(mainTag);
                    LoadingBanner.showMesInfoBtn(mainTag, error[0], "error");
                }
            };

            var smsEnable = smsAuthCbx.is(":checked");
            var tfaAppEnable = appAuthCbx.is(":checked");

            if (!smsEnable && !tfaAppEnable) {
                Teamlab.tfaAppAuthSettings(tfaType.none, null, [], [], callback);
                return;
            }

            var formTrustedIP = [];
            restrictionsList.find('.restriction .ip').each(function (idx, el) {
                formTrustedIP.push(jq(el).val());
            });

            var trustedIPForSave = [];
            for (var i = 0; i < formTrustedIP.length; i++) {
                var r = formTrustedIP[i].replace(/\s/g, '');
                if (r == '') {
                    continue;
                }

                if (!commonIpRegex.test(r) && !CIDRIpRegex.test(r)) {
                    LoadingBanner.showMesInfoBtn(mainTag, ASC.Resources.Master.ResourceJS.IncorrectIPAddressFormatError, 'error');
                    return;
                }

                if (~trustedIPForSave.indexOf(r)) {
                    LoadingBanner.showMesInfoBtn(mainTag, ASC.Resources.Master.ResourceJS.SameIPRestrictionError, 'error');
                    return;
                } else {
                    trustedIPForSave.push(r);
                }
            }

            var mandatoryUsersForSave = [],
                mandatoryGroupsForSave = [];

            var selectedUsers = mainTag.find(".user-item"),
                selectedGroups = mainTag.find(".group-item");

            selectedUsers.each(function (idx, el) {
                mandatoryUsersForSave.push(jq(el).attr("id"));
            });

            selectedGroups.each(function (idx, el) {
                mandatoryGroupsForSave.push(jq(el).attr("id"));
            });

            if (smsEnable) {
                Teamlab.tfaAppAuthSettings(tfaType.sms, trustedIPForSave, mandatoryUsersForSave, mandatoryGroupsForSave, callback);
            } else {
                Teamlab.tfaAppAuthSettings(tfaType.app, trustedIPForSave, mandatoryUsersForSave, mandatoryGroupsForSave, callback);
            }
        }
    };
};

(function () {
    jq(function () {

        ASC.Controls.SmsValidationSettings.InitAdvancedSettings();

        jq("#chk2FactorAuthSave").on("click", ASC.Controls.SmsValidationSettings.SaveSettings);

        jq('#studio_smsValidationSettings input[type="radio"]').on("change", ASC.Controls.SmsValidationSettings.RefreshAdvancedSettingsView);
    });
})();