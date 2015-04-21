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


/*sharing data sample
    {
        actions : [{name: "actionName",
                    id  : "id",
                    defaultAction : "true/false"
                  }],
                  
        items : [{
            id : "itemId",
            data : "customData",
            isGroup : true,
            name : "name",
            canEdit : true/false,
            hideRemove : true/false,
            selectedAction : {
                                name: "actionName",
                                id  : "id"
                             }
            
        }]
    }
*/

var SharingSettingsManager = function (elementId, sharingData) {
    var shareUserSelector = jq("#shareUserSelector");
    var shareGroupSelector = jq("#shareGroupSelector");

    var clone = function (o) {
        if (!o || "object" !== typeof o) {
            return o;
        }

        var c = "function" === typeof o.pop ? [] : {};
        var p, v;
        for (p in o) {
            if (o.hasOwnProperty(p)) {
                v = o[p];
                if (v && "object" === typeof v) {
                    c[p] = clone(v);
                } else {
                    c[p] = v;
                }
            }
        }
        return c;
    };

    this.OnSave = null;

    var _data = sharingData;
    var _workData = clone(sharingData);

    var _manager = this;

    jq(function () {
        if (elementId != undefined) {
            jq("#" + elementId).click(function () {
                _manager.ShowDialog();
            });
        }

        jq("#sharingSettingsSaveButton").click(_manager.SaveAndCloseDialog);
        jq("#studio_sharingSettingsDialog").on("click", ".sharing-cancel-button", _manager.CloseDialog);

        jq("#studio_sharingSettingsDialog").on("click", ".removeItem", function () {
            removeItem(jq(this).attr("data"));
        });
        jq("#sharingSettingsItems").on("change", ".action select", function () {
            changeItemAction(jq(this).attr("data"), jq(this).val());
        });

        jq("#sharingSettingsItems").on("click", ".combobox-title", function () {
            if (jq(this).closest(".sharingItem").is("#sharingSettingsItems .sharingItem:gt(-3)")) {
                jq("#sharingSettingsItems").scrollTo(jq(".sharingItem:has(.combobox-container:visible)"));
            }
        });

        jq("#shareAddMessage").on("click", function () {
            showShareMessage();
            return false;
        });

        jq("#shareRemoveMessage").on("click", function () {
            hideShareMessage();
            jq("#shareMessage").val("");
            return false;
        });

        jq("#shareMessageSend").on("change", function () {
            if (!jq("#shareMessageSend").is(":checked")) {
                hideShareMessage();
            }
        });

        shareUserSelector
            .useradvancedSelector(
                {
                    showGroups: true,
                    inPopup: true
                })
            .on("showList", addUsers);

        shareGroupSelector
            .groupadvancedSelector(
                {
                    inPopup: true,
                    witheveryone: true,
                    withadmin: true
                })
            .on("showList", addGroups);

        hideShareMessage();
    });

    var hideShareMessage = function () {
        jq("#shareRemoveMessage").hide();
        jq("#sharingSettingsDialogBody").removeClass("with-message");
        jq("#shareAddMessage").show();
    };

    var showShareMessage = function () {
        jq("#shareAddMessage").hide();
        jq("#shareRemoveMessage").show();
        jq("#sharingSettingsDialogBody").addClass("with-message");
        jq("#shareMessageSend").prop("checked", true);
    };

    var changeItemAction = function (itemId, actionId) {
        var act = null;

        for (var i = 0; i < _workData.actions.length; i++) {
            if (_workData.actions[i].id == actionId) {
                act = _workData.actions[i];
                break;
            }
        }

        for (i = 0; i < _workData.items.length; i++) {
            if (_workData.items[i].id == itemId) {

                _workData.items[i].selectedAction = act;
                break;
            }
        }
    };

    var removeItem = function (itemId) {
        for (var i = 0; i < _workData.items.length; i++) {
            if (_workData.items[i].id == itemId) {
                if (_workData.items[i].canEdit === false || _workData.items[i].hideRemove) {
                    return false;
                }
                if (_workData.items[i].isGroup) {
                    shareGroupSelector.groupadvancedSelector("unselect", [itemId]);
                } else {
                    shareUserSelector.useradvancedSelector("unselect", [itemId]);
                }
                _workData.items.splice(i, 1);
                break;
            }
        }

        jq("#sharing_item_" + itemId).remove();
        jq("#sharingSettingsItems div.sharingItem.tintMedium").removeClass("tintMedium");
        jq("#sharingSettingsItems div.sharingItem:even").addClass("tintMedium");
        return true;
    };

    var addUsers = function (event, users) { 
        var selectedIds = jq(users).map(function (j, user) {
            return user.id;
        }).toArray();

        for (var i = 0; i < _workData.items.length; i++) {
            var item = _workData.items[i];
            if (!item.isGroup) {
                var num = jq.inArray(item.id, selectedIds);
                if (num == -1) {
                    if (removeItem(item.id)) {
                        i--;
                    }
                } else {
                    selectedIds.splice(num, 1);
                }
            }
        }

        jq(users).each(function (j, user) {
            if (jq.inArray(user.id, selectedIds) != -1) {
                addUserItem(user.id, user.title);
            }
        });
    };

    var addUserItem = function (userId, userName) {
        var defAct = null;
        for (var i = 0; i < _workData.actions.length; i++) {
            if (_workData.actions[i].defaultAction) {
                defAct = _workData.actions[i];
                break;
            }
        }
        var newItem = { id: userId, name: userName, selectedAction: defAct, isGroup: false, canEdit: true };
        _workData.items.push(newItem);

        jq("#sharingSettingsItems").append(jq.tmpl("sharingListTemplate", { items: [newItem], actions: _workData.actions }));
        jq("#studio_sharingSettingsDialog .action select:last").tlcombobox();

        jq("#sharingSettingsItems div.sharingItem.tintMedium").removeClass("tintMedium");
        jq("#sharingSettingsItems div.sharingItem:even").addClass("tintMedium");
    };

    var addGroups = function (event, groups) {
        var selectedIds = jq(groups).map(function (j, group) {
            return group.id;
        }).toArray();

        for (var i = 0; i < _workData.items.length; i++) {
            var item = _workData.items[i];
            if (item.isGroup) {
                var num = jq.inArray(item.id, selectedIds);
                if (num == -1) {
                    if (removeItem(item.id)) {
                        i--;
                    }
                } else {
                    selectedIds.splice(num, 1);
                }
            }
        }

        jq(groups).each(function (j, group) {
            if (jq.inArray(group.id, selectedIds) != -1) {
                addGroupItem(group.id, group.title);
            }
        });
    };

    var addGroupItem = function (groupId, groupName) {
        var defAct = null;
        for (var i = 0; i < _workData.actions.length; i++) {
            if (_workData.actions[i].defaultAction) {
                defAct = _workData.actions[i];
                break;
            }
        }
        var newItem = { id: groupId, name: groupName, selectedAction: defAct, isGroup: true, canEdit: true };
        _workData.items.push(newItem);

        jq("#sharingSettingsItems").append(jq.tmpl("sharingListTemplate", { items: [newItem], actions: _workData.actions }));
        jq("#studio_sharingSettingsDialog .action select:last").tlcombobox();

        jq("#sharingSettingsItems div.sharingItem.tintMedium").removeClass("tintMedium");
        jq("#sharingSettingsItems div.sharingItem:even").addClass("tintMedium");
    };

    var reDrawItems = function () {
        jq("#sharingSettingsItems").html(jq.tmpl("sharingListTemplate", _workData));

        if (jq.browser.mobile) {
            jq("#sharingSettingsItems").addClass("isMobileAgent");
        }

        jq("#studio_sharingSettingsDialog .action select").each(function () {
            jq(this).tlcombobox();
        });

        shareUserSelector.useradvancedSelector("reset");
        shareGroupSelector.groupadvancedSelector("reset");

        var userIds = new Array();
        var groupIds = new Array();
        var disableUserIds = new Array();
        var disableGroupIds = new Array();
        for (var i = 0; i < _workData.items.length; i++) {
            var item = _workData.items[i];
            if (item.isGroup) {
                groupIds.push(item.id);
                if (item.hideRemove || !item.canEdit) {
                    disableGroupIds.push(item.id);
                }
            } else {
                userIds.push(item.id);
                if (item.hideRemove || !item.canEdit) {
                    disableUserIds.push(item.id);
                }
            }
        }
        shareUserSelector.useradvancedSelector("select", userIds);
        shareUserSelector.useradvancedSelector("disable", disableUserIds);

        shareGroupSelector.groupadvancedSelector("select", groupIds);
        shareGroupSelector.groupadvancedSelector("disable", disableGroupIds);
    };

    this.UpdateSharingData = function (data) {
        _data = data;
        _workData = clone(data);
    };

    this.GetSharingData = function () {
        return _data;
    };

    this.ShowDialog = function (width, asFlat) {
        reDrawItems();
        hideShareMessage();
        jq("#shareMessage").val("");
        jq("#shareMessageSend").prop("checked", true);

        width = width || 600;

        if (!asFlat) {
            StudioBlockUIManager.blockUI("#studio_sharingSettingsDialog", width, 0, -300, "absolute");
        } else {
            jq("#studio_sharingSettingsDialog").show()
                .css({ "display": "block" });
        }

        PopupKeyUpActionProvider.EnterAction = "jq(\"#sharingSettingsSaveButton\").click();";
    };

    this.SaveAndCloseDialog = function () {
        _data = _workData;

        if (_manager.OnSave != null) {
            _manager.OnSave(_data);
        }

        return _manager.CloseDialog();
    };

    this.CloseDialog = function () {
        return PopupKeyUpActionProvider.CloseDialog();
    };
};