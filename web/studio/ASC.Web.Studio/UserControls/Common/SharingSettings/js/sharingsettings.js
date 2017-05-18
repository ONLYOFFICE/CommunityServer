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

    var sharingUserProfileInfo = new PopupBox("pb_SharingUserProfileInfo", 320, 140, "tintLight", "borderBaseShadow", "",
        {
            apiMethodName: "Teamlab.getProfile",
            tmplName: "userProfileCardTmpl"
        });

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
    this.OnCopyLink = null;
    this.OnChange = null;

    var _data = sharingData;
    var _workData = clone(sharingData);
    var _changed = false;
    var _clipLink = null;

    var _manager = this;

    jq(function () {
        if (elementId != undefined) {
            jq("#" + elementId).click(function () {
                _manager.ShowDialog();
            });
        }

        jq("#studio_sharingSettingsDialog").on("click", ".sharing-save-button", _manager.SaveAndCloseDialog);
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

        if (!ASC.Clipboard.enable) {
            jq("#shareGetLink").remove();
        }

        jq.dropdownToggle(
            {
                switcherSelector: "#shareUserDefAction",
                dropdownID: "shareUserDefSelect",
                inPopup: true,
                addLeft: -5,
                addTop: 4,
            });

        jq("#shareUserDefSelect").on("click", ".dropdown-item", function () {
            jq("#shareUserDefSelect .dropdown-item.active").removeClass("active");
            jq(this).addClass("active");
            jq("#shareUserDefSelect").hide();

            var actionId = jq(this).attr("data-id");
            for (var i = 0; i < _workData.actions.length; i++) {
                _workData.actions[i].defaultUserAction = (_workData.actions[i].id == actionId);
            }

            setDefActions();
        });

        jq.dropdownToggle(
            {
                switcherSelector: "#shareGroupDefAction",
                dropdownID: "shareGroupDefSelect",
                inPopup: true,
                addLeft: -5,
                addTop: 4,
            });

        jq("#shareGroupDefSelect").on("click", ".dropdown-item", function () {
            jq("#shareGroupDefSelect .dropdown-item.active").removeClass("active");
            jq(this).addClass("active");
            jq("#shareGroupDefSelect").hide();

            var actionId = jq(this).attr("data-id");
            for (var i = 0; i < _workData.actions.length; i++) {
                _workData.actions[i].defaultGroupAction = (_workData.actions[i].id == actionId);
            }

            setDefActions();
        });
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
        changeStatus(true);
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
        changeStatus(true);
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
        jq(".sharing-empty").toggle(_workData.items.length <= 1);
        return true;
    };

    var addUsers = function (event, users) {
        changeStatus(true);
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
        jq(".sharing-empty").toggle(_workData.items.length <= 1);
    };

    var addUserItem = function (userId, userName) {
        changeStatus(true);
        var defAct = null;
        for (var i = 0; i < _workData.actions.length; i++) {
            if (_workData.actions[i].defaultUserAction) {
                defAct = _workData.actions[i];
                break;
            }
            if (_workData.actions[i].defaultAction) {
                defAct = _workData.actions[i];
            }
        }
        var newItem = { id: userId, name: userName, selectedAction: defAct, isGroup: false, canEdit: true };
        _workData.items.push(newItem);

        jq("#sharingSettingsItems").append(jq.tmpl("sharingListTemplate", { items: [newItem], actions: _workData.actions }));
        jq("#studio_sharingSettingsDialog .action select:last").tlcombobox();

        var latUserLink = jq("#studio_sharingSettingsDialog .userLink:last");
        var id = latUserLink.attr("id");
        if (id != null && id != "") {
            sharingUserProfileInfo.RegistryElement(id, "\"" + latUserLink.attr("data-uid") + "\"");
        }

        jq("#sharingSettingsItems div.sharingItem.tintMedium").removeClass("tintMedium");
        jq("#sharingSettingsItems div.sharingItem:even").addClass("tintMedium");
    };

    var addGroups = function (event, groups) {
        changeStatus(true);
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
        jq(".sharing-empty").toggle(_workData.items.length <= 1);
    };

    var addGroupItem = function (groupId, groupName) {
        changeStatus(true);
        var defAct = null;
        for (var i = 0; i < _workData.actions.length; i++) {
            if (_workData.actions[i].defaultGroupAction) {
                defAct = _workData.actions[i];
                break;
            }
            if (_workData.actions[i].defaultAction) {
                defAct = _workData.actions[i];
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

        jq(".sharing-empty").toggle(_workData.items.length <= 1);

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

    var reDrawDefActions = function () {
        jq("#shareUserDefSelect").html(jq.tmpl("sharingActionsTemplate", _workData));
        jq("#shareGroupDefSelect").html(jq.tmpl("sharingActionsTemplate", _workData));
        setDefActions();
    };

    var setDefActions = function () {
        var setUser = true;
        var setGroup = true;
        var defStyle = "";
        for (var i = 0; i < _workData.actions.length; i++) {
            if (_workData.actions[i].defaultUserAction && setUser) {
                jq("#shareUserDefAction").addClass("share-def-action-" + _workData.actions[i].defaultStyle);
                setUser = false;
            } else {
                jq("#shareUserDefAction").removeClass("share-def-action-" + _workData.actions[i].defaultStyle);
            }
            if (_workData.actions[i].defaultGroupAction && setGroup) {
                jq("#shareGroupDefAction").addClass("share-def-action-" + _workData.actions[i].defaultStyle);
                setGroup = false;
            } else {
                jq("#shareGroupDefAction").removeClass("share-def-action-" + _workData.actions[i].defaultStyle);
            }
            if (_workData.actions[i].defaultAction) {
                defStyle = _workData.actions[i].defaultStyle;
            }
        }
        if (setUser) {
            jq("#shareUserDefAction").addClass("share-def-action-" + defStyle);
        }
        if (setGroup) {
            jq("#shareGroupDefAction").addClass("share-def-action-" + defStyle);
        }
    };

    var changeStatus = function (value) {
        _changed = value;

        jq(".sharing-notchanged-buttons").toggleClass("display-none", value);
        jq(".sharing-changed-buttons").toggleClass("display-none", !value);

        if (_manager.OnChange != null) {
            _manager.OnChange(_changed);
        }
    };

    this.WhereChanges = function (ch) {
        if (ch) {
            changeStatus(true);
        }
        return _changed;
    };

    this.UpdateSharingData = function (data, link) {
        changeStatus(false);
        _data = data;
        _workData = clone(data);

        if (link && jq("#shareGetLink").length) {
            jq("#shareGetLink").show();
            ASC.Clipboard.destroy(_clipLink);
            _clipLink = ASC.Clipboard.create(link, "shareGetLink", {
                onComplete: _manager.OnCopyLink
            });
        } else {
            jq("#shareGetLink").hide();
        }
    };

    this.GetSharingData = function () {
        return _data;
    };

    this.ShowDialog = function (width, height, asFlat) {
        reDrawItems();
        reDrawDefActions();
        hideShareMessage();

        jq("#studio_sharingSettingsDialog .userLink").each(function () {
            var id = jq(this).attr("id");
            if (id != null && id != "") {
                sharingUserProfileInfo.RegistryElement(id, "\"" + jq(this).attr("data-uid") + "\"");
            }
        });

        jq("#shareMessage").val("");
        jq("#shareMessageSend").prop("checked", true);

        width = width || 600;
        height = height || 470;

        if (!asFlat) {
            StudioBlockUIManager.blockUI("#studio_sharingSettingsDialog", width, 0, -height/2, "absolute");
        } else {
            jq("#studio_sharingSettingsDialog").show()
                .css({
                    "display": "block",
                    "width": width - 48
                });
        }

        PopupKeyUpActionProvider.EnterAction = "jq(\".sharing-save-button:visible\").click();";
    };

    this.SaveAndCloseDialog = function () {
        _data = _workData;

        if (_manager.OnSave != null) {
            _manager.OnSave(_data);
        }

        return _manager.CloseDialog();
    };

    this.CloseDialog = function () {
        ASC.Clipboard.destroy(_clipLink);
        return PopupKeyUpActionProvider.CloseDialog();
    };
};