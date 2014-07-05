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
if (typeof (ASC) == 'undefined')
    ASC = {};

if (typeof(ASC.Studio) == 'undefined')
    ASC.Studio = {};

ASC.Studio.UserSelector = new function() {

    this.UserGroupItem = function(id, name) {
        this.Name = name;
        this.ID = id;
        this.Users = [];
        this.PreSelected = false;
    };

    this.UserItem = function(id, name, selected, group, defaultSelected, title) {
        this.ID = id;
        this.Name = name;
        this.Selected = selected;
        this.Group = group;
        this.PreSelected = false;
        this.Disable = false;
        this.DefaultSelected = defaultSelected;
        this.Title = title;
    };

    this.UserSelectorPrototype = function(id, objName, mobileVersion) {
        this.ID = id;
        this.ObjName = objName;
        this.Groups = [];

        this.OnCancelButtonClick = null;
        this.OnOkButtonClick = null;

        this.IsFirstVisit = true;
        this.MobileVersion = mobileVersion == true;

        this.RenderItems = function() {
            var userList = "",
                userListM = "",
                selectedUserList = "",
                selectedUserArray = [];

            for (var i = 0; i < this.Groups.length; i++) {
                var group = this.Groups[i],
                    isEmpty = true;

                for (var j = 0; j < group.Users.length; j++) {
                    var user = this.Groups[i].Users[j];
                    if (user.Disable)
                        continue;

                    if (this.MobileVersion) {
                        if (isEmpty && group.Name != '') {
                            userListM += "<optgroup class='tintLight' label='{0}' style='max-width:300px;'>".format(group.Name);
                            isEmpty = false;
                        }


                        userListM += "<option class='tintMedium' style='cursor:pointer; max-width:300px;' value='{0}' {2}>{1}</option>".format(
                            user.ID,
                            user.Name,
                            user.Selected ? "selected = 'selected'" : "");


                    } else {

                        if (user.Selected) {
                            var alreadyAddedFlag = false;
                            for (var k = 0; k < selectedUserArray.length; k++) {
                                var selectedUser = selectedUserArray[k];
                                if (selectedUser.ID == user.ID) {
                                    alreadyAddedFlag = true;
                                    user.alreadySelected = true;
                                    selectedUser.OtherGroupsIDs = selectedUser.OtherGroupsIDs ? selectedUser.OtherGroupsIDs : [];
                                    selectedUser.OtherGroupsIDs.push(group.ID);
                                    break;
                                }
                            }
                            if (!alreadyAddedFlag) {
                                selectedUserArray.push(user);
                            }
                        } else {
                            if (isEmpty && group.Name != '') {

                                userList +=
                                ['<div class="clearFix">',
                                    '<div style="float:left; padding-top:5px; margin-rigth:5px;"><input onclick="',
                                        this.ObjName, '.PreSelectGroup(\'', group.ID, '\');" id="usrdialog_', this.ID, '_dep_', group.ID, '" type="checkbox"/></div>',
                                        '<div class="depBox" style="cursor:pointer;"><label for="usrdialog_',
                                        this.ID, '_dep_', group.ID, '">', group.Name, '</label></div>',
                                    '</div>',
                                    '<div class="depBoxContent">'
                                ].join('');
                                isEmpty = false;
                            }

                            userList += '<div class="usrBox">';

                            var checked = '';
                            if (user.PreSelected) {
                                checked = 'checked="checked"';
                            }

                            userList += ['<input onclick="',
                                            this.ObjName, '.PreSelectUser(\'', user.ID, '\', this);" id="usrdialog_', this.ID, '_usr_', user.ID,
                                            '" type="checkbox" ', checked, ' />',
                                            '<label for="usrdialog_',
                                                this.ID,
                                                '_usr_', user.ID, '">',
                                                user.Name,
                                            '</label>',
                                        '</div>'].join('');
                        }
                    }
                }

                if (isEmpty == false && group.Name != '') {
                    if (this.MobileVersion) {
                        userListM += "</optgroup>";
                    } else {
                        userList += '</div>';
                    }

                }
            }


            if (this.MobileVersion) {
                jq("#selectmobile_" + this.ID).html(userListM);
            } else {
                selectedUserArray.sort(function(a, b) {
                    if (a.Name > b.Name)
                        return 1;
                    if (a.Name < b.Name)
                        return -1;
                    return 0;
                });
                for (var i = 0; i < selectedUserArray.length; i++) {
                    var user = selectedUserArray[i],
                        groups = user.Group.ID;
                    if (user.OtherGroupsIDs) {
                        groups = groups + "," + user.OtherGroupsIDs.join(",");
                    }
                    selectedUserList += ['<div class="usrBox">',
                                            '<input onclick="', this.ObjName,
                                            '.PreSelectUser(\'', selectedUserArray[i].ID, '\', this);" id="usrdialog_',
                                            this.ID, '_usr_', selectedUserArray[i].ID,
                                            '" dep="', groups, '" type="checkbox"/>',
                                            '<label for="usrdialog_', this.ID, '_usr_', selectedUserArray[i].ID, '">',
                                                selectedUserArray[i].Name, '</label>',
                                        '</div>'].join('');
                }

                jq('#usrdialog_leftBox_' + this.ID).html(userList);
                jq('#usrdialog_rightBox_' + this.ID).html(selectedUserList);
            }
        };

        this.ShowNewDialog = function(zIdex) {
            this.ClearSelection();
            this.ShowDialog(zIdex);
        };

        this.ShowDialog = function(zIdex) {

            if (this.IsFirstVisit) {
                this.IsFirstVisit = false;
            } else {
                for (var i = 0, n = this.Groups.length; i < n; i++) {
                    var group = this.Groups[i];
                    for (var j = 0, m = group.Users.length; j < m; j++) {
                        var user = this.Groups[i].Users[j];
                        user.Selected = user.DefaultSelected;
                    }
                }
                this.Unselect();
            }

            this.RenderItems();

            var zi = 66666;
            if (zIdex != undefined && zIdex != null)
                zi = zIdex;

            try {

                var margintop = jq(window).scrollTop() - 100;
                margintop = margintop + 'px';

                jq.blockUI({ message: jq("#usrdialog_" + this.ID),
                    css: {
                        left: '50%',
                        top: '25%',
                        opacity: '1',
                        border: 'none',
                        padding: '0px',
                        width: this.MobileVersion ? "400px" : "750px",
                        cursor: 'default',
                        textAlign: 'left',
                        position: 'absolute',
                        'margin-left': this.MobileVersion ? "-200px" : "-375px",
                        'margin-top': margintop,
                        'background-color': 'White'
                    },
                    overlayCSS: {
                        backgroundColor: '#AAA',
                        cursor: 'default',
                        opacity: '0.3'
                    },

                    focusInput: false,
                    baseZ: zi,

                    fadeIn: 0,
                    fadeOut: 0
                });
            }
            catch (e) { };

            PopupKeyUpActionProvider.CloseDialogAction = this.ObjName + '.CloseAction();';
            PopupKeyUpActionProvider.EnterAction = this.ObjName + '.ApplyAndCloseDialog();';

        };

        this.PreSelectGroup = function(groupID) {
            var state = jq('#usrdialog_' + this.ID + '_dep_' + groupID).is(':checked');
            for (var i = 0; i < this.Groups.length; i++) {
                if (this.Groups[i].ID == groupID) {
                    for (var j = 0; j < this.Groups[i].Users.length; j++) {
                        if (this.Groups[i].Users[j].Selected == false) {
                            this.Groups[i].Users[j].PreSelected = state;

                            var inputs = jq("input[id$='_usr_" + this.Groups[i].Users[j].ID + "']");
                            if (state) {
                                inputs.prop("checked", true);
                            } else {
                                inputs.prop("checked", false);
                            }
                            if (inputs.length > 1) {
                                this.PreSelectUser(this.Groups[i].Users[j].ID, inputs[0]);
                            }
                        }
                    }
                    this.Groups[i].PreSelected = state;
                    return;
                }
            }
        };

        this.PreSelectUser = function(userID, elem) {
            var state = true;
            if (elem) {
                state = jq(elem).is(':checked');
            }
            if (state) {
                jq("input[id$='_usr_" + userID + "']").prop("checked", true);
            } else {
                jq("input[id$='_usr_" + userID + "']").prop("checked", false);
            }
            
            for (var i = 0; i < this.Groups.length; i++) {
                for (var j = 0; j < this.Groups[i].Users.length; j++) {
                    if (this.Groups[i].Users[j].ID == userID) {
                        this.Groups[i].Users[j].PreSelected = state;
                        if (!this.Groups[i].Users[j].OthersGroupIds) {
                            var groupsIds = jq('#usrdialog_' + this.ID + '_usr_' + userID).attr("dep");
                            if (groupsIds) {
                                groupsIds = groupsIds.split(",");
                                if (groupsIds.length > 1) {
                                    groupsIds.splice(0, 1);
                                }
                                this.Groups[i].Users[j].OthersGroupIds = groupsIds;
                            }
                        }
                        if (state == false) {
                            this.Groups[i].PreSelected = false;
                            jq('#usrdialog_' + this.ID + '_dep_' + this.Groups[i].ID).prop("checked", false);
                        } else {
                            var checkedFlag = true;
                            for (var k = 0; k < this.Groups[i].Users.length; k++) {
                                if (this.Groups[i].Users[k].PreSelected == false) {
                                    checkedFlag = false;
                                    break;
                                }
                            }
                            if (checkedFlag)
                                jq('#usrdialog_' + this.ID + '_dep_' + this.Groups[i].ID).prop("checked", true);
                        }
                    }
                }
            }
        };

        this.Select = function() {
            for (var i = 0; i < this.Groups.length; i++) {
                for (var j = 0; j < this.Groups[i].Users.length; j++) {
                    if (!this.Groups[i].Users[j].Selected && this.Groups[i].Users[j].PreSelected) {
                        var currentUserID = this.Groups[i].Users[j].ID,
                            parentEl = jq('input[id*="' + currentUserID + '"]').parent();

                        if (!parentEl.is(':visible')) {
                            continue;
                        }

                        this.Groups[i].Users[j].Selected = true;
                    }
                    this.Groups[i].Users[j].PreSelected = false;
                }
            }

            this.RenderItems();
        };

        this.Unselect = function (removeDefaultSelection) { // removeDefaultSelection - optional parametr
            for (var i = 0, n = this.Groups.length; i < n; i++) {
                var preSelectedFlag = false;
                for (var j = 0; j < this.Groups[i].Users.length; j++) {

                    if (this.Groups[i].Users[j].Selected && this.Groups[i].Users[j].PreSelected) {
                        this.Groups[i].Users[j].Selected = false;
                        this.Groups[i].Users[j].alreadySelected = false;
                        if (removeDefaultSelection) {
                            this.Groups[i].Users[j].DefaultSelected = false;
                        }
                        if (this.Groups[i].Users[j].OtherGroupsIds && this.Groups[i].Users[j].OtherGroupsIds.length) {
                            this.Groups[i].Users[j].OtherGroupsIds.splice(0, 1);
                            if (this.Groups[i].Users[j].OtherGroupsIds.length) preSelectedFlag = true;
                        }
                    }
                    this.Groups[i].Users[j].PreSelected = preSelectedFlag;
                }
            }
            this.RenderItems();
        };

        this.ChangeMobileSelect = function() {
            if (!this.MobileVersion) return;

            var selectId = jq("#selectmobile_" + this.ID).val();

            for (var i = 0, n = this.Groups.length; i < n; i++) {
                for (var j = 0, m = this.Groups[i].Users.length; j < m; j++) {
                    this.Groups[i].Users[j].Selected = jq.inArray(this.Groups[i].Users[j].ID, selectId) != -1;
                }
            }
        };

        this.SelectUser = function(userID) {
            for (var i = 0, n = this.Groups.length; i < n; i++) {
                for (var j = 0, m = this.Groups[i].Users.length; j < m; j++) {
                    if (this.Groups[i].Users[j].ID == userID)
                        this.Groups[i].Users[j].Selected = true;
                }
            }
        };

        this.DisableUser = function(userID) {
            for (var i = 0, n = this.Groups.length; i < n; i++) {
                for (var j = 0, m = this.Groups[i].Users.length; j < m; j++) {
                    if (this.Groups[i].Users[j].ID == userID)
                        this.Groups[i].Users[j].Disable = true;
                }
            }
        };

        this.EnableUser = function(userID) {
            for (var i = 0, n = this.Groups.length; i < n; i++) {
                for (var j = 0, m = this.Groups[i].Users.length; j < m; j++) {
                    if (this.Groups[i].Users[j].ID == userID)
                        this.Groups[i].Users[j].Disable = false;
                }
            }
        };

        this.ClearSelection = function() {
            for (var i = 0, n = this.Groups.length; i < n; i++) {
                for (var j = 0, m = this.Groups[i].Users.length; j < m; j++) {
                    this.Groups[i].Users[j].Selected = false;
                    this.Groups[i].Users[j].Disable = false;
                }
            }
        };

        this.GetSelectedUsers = function() {
            var users = [];
            for (var i = 0, n = this.Groups.length; i < n; i++) {
                for (var j = 0, m = this.Groups[i].Users.length; j < m; j++) {
                    if (this.Groups[i].Users[j].Selected && !this.Groups[i].Users[j].alreadySelected)
                        users.push(this.Groups[i].Users[j]);
                }
            }

            return users;
        };

        this.CloseAction = function() {
            if (this.OnCancelButtonClick != null)
                this.OnCancelButtonClick();
        };

        this.ApplyAndCloseDialog = function() {
            PopupKeyUpActionProvider.ClearActions();
            jq.unblockUI();
            if (this.OnOkButtonClick != null) {
                for (var i = 0, n = this.Groups.length; i < n; i++) {
                    var group = this.Groups[i];
                    for (var j = 0, m = group.Users.length; j < m; j++) {
                        var user = this.Groups[i].Users[j];
                        user.DefaultSelected = user.Selected;
                    }
                }
                this.OnOkButtonClick();
            }
        };
        this.ClearUsrdialogRightBox = function() {
            jq('div[id^="usrdialog_rightBox_"]').empty();
            jq("div.centerBox div:last-child a").click();
        };

        this.SelectAll = function() {
            for (var i = 0, n = this.Groups.length; i < n; i++) {
                for (var j = 0, m = this.Groups[i].Users.length; j < m; j++) {
                    this.Groups[i].Users[j].Selected = true;
                }
            }
        };
        this.ClearUsrdialogLeftBox = function() {
            jq('div[id^="usrdialog_leftBox_"]').empty();
            jq("div.centerBox div:first-child a").click();
        };
    };
};


function filterEmployees() {
    var el = jq('div[id^="usrdialog_leftBox_"]'),
        labels = el.find("label");
    labels.each(
    		function() {
    		    jq(this).parent().show();
    		    jq(this).parent().parent().show();
    		}
    	);

    jq('#employeeFilterInputCloseImage').attr('class', 'employeeFilterInputCloseImageGrey float-right');

    var userName = jq('#employeeFilterInput').val();
    if (userName == null || userName == '' || userName == jq('#employeeFilterInput').attr('title')) {
        return;
    }

    jq('#employeeFilterInputCloseImage').attr('class', 'employeeFilterInputCloseImageBlue float-right');

    userName = userName.toLowerCase();
    var userNameArray = userName.split(' '),
        hideHeaderFlag = true,
        userCount = 0,
        lastSelectedUser,
        header;

    labels.each(function() {
        var child = jq(this);

        if (child.parent().attr('class').indexOf("depBox") != -1) {
            if (hideHeaderFlag && header != null) {
                header.hide();
            }
            header = child.parent().parent();
            hideHeaderFlag = true;
        } else {
            if (child.parent().attr('class').indexOf("usrBox") != -1) {
                var currentUserName = child.text().toLowerCase(),
                    i,
                    j = 0;
                for (i = 0; i < userNameArray.length; i++) {
                    if (currentUserName.indexOf(userNameArray[i]) != -1 || currentUserName.indexOf(' ' + userNameArray[i]) != -1) {
                        j++;
                    } else {
                        child.parent().hide();
                        j = -1;
                        break;
                    }
                }
            }

            if (i == j) {
                hideHeaderFlag = false;

                userCount++;
                lastSelectedUser = child;
            }
        }
    });

    if (hideHeaderFlag && header != null) {
        header.hide();
    }
};

function onEmployeeFilterInputFocus() {
    jq('#employeeFilterInput').attr('class', 'employeeFilterInputFocus');
    var title = jq('#employeeFilterInput').attr('title');
    if (jq('#employeeFilterInput').val() == title) {
        jq('#employeeFilterInput').val('');
    }
};

function onEmployeeFilterInputFocusLost() {
    if (jq('#employeeFilterInput').val() == '') {
        jq('#employeeFilterInput').attr('class', 'employeeFilterInputGreyed');
        setTimeout("jq('#employeeFilterInput').val(jq('#employeeFilterInput').attr('title'));", 200);
    }
};

function employeeFilterInputCloseImageClick() {
    var currentUserName = jq('#employeeFilterInput').val(),
        title = jq('#employeeFilterInput').attr('title');
    if (currentUserName == null || currentUserName == '' || currentUserName == title) {
        return;
    }
    jq('#employeeFilterInput').attr('class', 'employeeFilterInputGreyed');
    jq('#employeeFilterInput').val('');
    filterEmployees();

    setTimeout("jq('#employeeFilterInput').val(jq('#employeeFilterInput').attr('title'));", 100);
};