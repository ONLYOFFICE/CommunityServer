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
    ASC = { };
if (typeof (ASC.Controls) == 'undefined')
    ASC.Controls = { };


function jqHtmlEncode(html) {
    if (html == undefined)
        return '';

    return jq('<div/>').text(html).html();
}

ASC.Controls.AdvancedUserSelector = new function() {

    this.EmptyId = "00000000-0000-0000-0000-000000000000";
    this.DefaultGroupName = '';
    this.Group = function(id, name) {
        this.Name = name;
        this.ID = id;
        this.Users = new Array();

        this.Clone = function () {
            var gr = {
                ID: this.ID,
                Name: this.Name,
                Users: new Array()
            };
            for (var i = 0; i < this.Users.length; i++) {
                gr.Users.push(this.Users[i].Clone());
            }
            return gr;
        };
    };

    this.User = function(id, name, group, Title, PhotoUrl, isVisitor) {
        this.ID = id;
        this.Name = name;
        this.Group = group;
        this.Title = Title;
        this.PhotoUrl = PhotoUrl;
        this.Hidden = false;
        this.IsVisitor = (isVisitor === true);

        this.Clone = function () {
            return {
                ID: this.ID,
                Name: this.Name,
                Group: this.Group,
                Title: this.Title,
                PhotoUrl: this.PhotoUrl,
                Hidden: this.Hidden,
                IsVisitor: this.IsVisitor
            };
        };
    };

    this.InitCommonData = function () {
        ASC.Controls.AdvancedUserSelector.Groups = new Array();
        var data = ASC.Controls.AdvancedUserSelector.Groups,
            groups = jQuery.extend(true, {}, ASC.Resources.Master.ApiResponses_Groups),
            profiles = jQuery.extend(true, {}, ASC.Resources.Master.ApiResponses_Profiles);

        groups = groups ? groups.response : [];
        profiles = profiles ? profiles.response : [];

        var commonGroup = new ASC.Controls.AdvancedUserSelector.Group(ASC.Controls.AdvancedUserSelector.EmptyId, ASC.Controls.AdvancedUserSelector.DefaultGroupName || 'All Departments');
        data.push(commonGroup);

        if (groups.length == 0) {
            groups.push({ id: ASC.Controls.AdvancedUserSelector.EmptyId, name: ASC.Controls.AdvancedUserSelector.DefaultGroupName || 'All Departments' });
        }

        for (var i = 0, iN = groups.length; i < iN; i++) {

            var gr = groups[i],
                curGroup = null;
            for (var k = 0, kN = data.length; k < kN; k++) {
                if (data[i].ID == gr.id) {
                    curGroup = data[i];
                    break;
                }
            }
            if (curGroup == null) {
                curGroup = new ASC.Controls.AdvancedUserSelector.Group(gr.id, jqHtmlEncode(gr.name));
                data.push(curGroup);
            }

            //users
            for (var j = 0, m = profiles.length; j < m; j++) {

                var user = profiles[j],
                    group = null;
                if (user.groups == null || user.groups.length == 0) {
                    var group = commonGroup;
                } else {
                    for (var k = 0, t = user.groups.length; k < t; k++) {
                        if (user.groups[k].id == curGroup.ID) {
                            group = curGroup;
                            break;
                        }
                    }
                }

                if (group != null) {
                    var exists = false;
                    for (var k = 0, t = group.Users.length; k < t; k++) {
                        if (group.Users[k].ID == user.id) {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists) {
                        group.Users.push(new ASC.Controls.AdvancedUserSelector.User(user.id,
                        user.displayName,
                        group,
                        jqHtmlEncode(user.title),
                        user.avatarSmall,
                        user.isVisitor));
                    }
                }
            }
        }
    };

    this.RenderControlPrototype = function (jsObjName, InputWidth, SelectedUser, IsLinkView, LinkText, parentHtmlSelector) {
        jq.tmpl("advUserSelectorTemplate", {
            jsObjName: jsObjName,
            InputWidth: InputWidth,
            SelectedUser: SelectedUser,//SecurityContext.CurrentAccount.ID
            IsLinkView: IsLinkView,
            LinkText: LinkText
        }).appendTo(parentHtmlSelector);
    };

    this.UserSelectorPrototype = function (id, objName, EmptyUserListMsg, clearLinkTitle, isLinkView, linkText, defaultGroupText, employeeType) {
        this.AllDepartmentsGroupName = defaultGroupText;
        this.ID = id;
        this.ObjName = objName;
        this.Groups = new Array();
        this.EmptyUserListMsg = EmptyUserListMsg;
        this.ClearLinkTitle = clearLinkTitle;
        this.SelectedDepartmentId = ASC.Controls.AdvancedUserSelector.EmptyId;
        this.SelectedUserId = null;
        this.LastUserNameBegin = "";
        this.SelectedUserName = "",
        this.IsLinkView = isLinkView;
        this.AdditionalFunction = function() { };
        this.LinkText = linkText;
        this.EmployeeType = employeeType;
        this.Me = function() {
            return jq("#" + this.ObjName);
        };

        var selector = this;
        ASC.Controls.AdvancedUserSelector.DefaultGroupName = selector.AllDepartmentsGroupName;

        this.Init = function () {
            if (typeof (ASC.Controls.AdvancedUserSelector.Groups) == "undefined") {
                ASC.Controls.AdvancedUserSelector.InitCommonData();
            }
            if (ASC.Controls.AdvancedUserSelector.DefaultGroupName != null && ASC.Controls.AdvancedUserSelector.DefaultGroupName != '') {
                ASC.Controls.AdvancedUserSelector.Groups[0].Name = ASC.Controls.AdvancedUserSelector.DefaultGroupName;
            }
            commonGroup = ASC.Controls.AdvancedUserSelector.Groups[0].Clone();
            commonGroup.Users = new Array();
            for (var i = 0, iN = ASC.Controls.AdvancedUserSelector.Groups.length; i < iN; i++) {

                if (typeof (selector.UserIDs) != 'undefined' && typeof (selector.UserIDs) != undefined && selector.UserIDs != null){// && selector.UserIDs.length > 0) {

                    //custom user list
                    var gr = ASC.Controls.AdvancedUserSelector.Groups[i];
                    for (var k = 0, kN = gr.Users.length; k < kN; k++) {
                        for (var j = 0, jN = selector.UserIDs.length; j < jN; j++) {
                            if (gr.Users[k].ID == selector.UserIDs[j]) {
                                commonGroup.Users.push(gr.Users[k].Clone());
                                break;
                            }
                        }
                    }

                    if (i == 0) {
                        selector.Groups.push(commonGroup);
                    }

                } else {

                    //filter by disabled users
                    var gr = ASC.Controls.AdvancedUserSelector.Groups[i].Clone();
                    if (gr.ID == ASC.Controls.AdvancedUserSelector.EmptyID)
                        gr.Name = ASC.Controls.AdvancedUserSelector.DefaultGroupName;

                    if (typeof (selector.DisabledUserIDs) != undefined && typeof (selector.DisabledUserIDs) != 'undefined') {
                        for (var j = 0, jN = selector.DisabledUserIDs.length; j < jN; j++) {
                            for (var k = 0, kN = gr.Users.length; k < kN; k++) {
                                if (gr.Users[k].ID == selector.DisabledUserIDs[j]) {
                                    gr.Users.splice(k, 1);
                                    k--;
                                    kN--;
                                }
                            }
                        }
                    }
                    selector.Groups.push(gr);
                }
            }
            if (jq.browser.mobile == true) {
                selector.RenderMobileControl();
            }
            jq(document).click(function(event){
                selector.dropdownRegAutoHide(event);
            }); 
        };

        this.GetAllDepartments = function() {
            if (jq.browser.mobile === true || this.Me().find('.adv-userselector-deps').html() !== "")
                return;

            var groupList = new String();
            for (var i = 0; i < this.Groups.length; i++)
                groupList += "<div id='Department_" + this.Groups[i].ID + "' class='adv-userselector-dep" + (i == 0 ? " adv-userselector-userHover' " : "' ") +
                    "onclick='javascript:" + this.ObjName + ".ChangeDepartment(\"" + this.Groups[i].ID + "\");'>" + this.Groups[i].Name + "</div>";
            this.Me().find('.adv-userselector-deps').html(groupList);
        };

        this.GetAllUsers = function() {
            if (jq.browser.mobile === true)
                return;

            var userArray = [],
                hashUserArray = {},
                tempDiv = jq(document.createElement('div'));

            for (var i = 0, iN = this.Groups.length; i < iN; i++) {
                var curGroup = this.Groups[i];
                if (this.SelectedDepartmentId == ASC.Controls.AdvancedUserSelector.EmptyId || curGroup.ID === this.SelectedDepartmentId) {
                    var userName = '';
                    for (var j = 0, jN = curGroup.Users.length; j < jN; j++) {
                        userName = curGroup.Users[j].Name.toLowerCase();
                        userName = tempDiv.html(userName).text();
                        if (userName.indexOf(this.LastUserNameBegin) != -1 && curGroup.Users[j].Hidden == false) {
                            if (!hashUserArray.hasOwnProperty(curGroup.Users[j].ID)) {
                                //EmployeeType undefined
                                if(!this.EmployeeType) {
                                    userArray.push(curGroup.Users[j]);
                                    hashUserArray[curGroup.Users[j].ID] = true;
                                } else {
                                    switch (this.EmployeeType) {
                                        //EmployeeType.All
                                        case 0:
                                            userArray.push(curGroup.Users[j]);
                                            hashUserArray[curGroup.Users[j].ID] = true;
                                            break;
                                        //EmployeeType.User
                                        case 1:
                                            if(!curGroup.Users[j].IsVisitor)
                                            {
                                                userArray.push(curGroup.Users[j]);
                                                hashUserArray[curGroup.Users[j].ID] = true;
                                            }
                                            break;
                                        //EmployeeType.Visitor
                                        case 2:
                                            if(curGroup.Users[j].IsVisitor)
                                            {
                                                userArray.push(curGroup.Users[j]);
                                                hashUserArray[curGroup.Users[j].ID] = true;
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            userArray.sort(function(a, b) {
                return (a.Name.toLowerCase() > b.Name.toLowerCase() ? 1 : -1);
            });

            var $userListHtml = {};
            if (userArray.length == 0) {
                $userListHtml = jq("<div style='padding:4px;'></div>").html(this.EmptyUserListMsg);
            } else {
                $userListHtml = jq.tmpl("advUSUserListTemplate",
                    {
                        jsObjName: this.ObjName,
                        users: userArray
                    });
            }

            this.Me().find(".adv-userselector-users").empty();
            $userListHtml.appendTo(this.Me().find(".adv-userselector-users"));

            this.SelectCurUserInDept();
        };

        this.RenderItems = function() {
            if (jq.browser.mobile === true)
                return;

            this.SelectedUserName = this.Me().find('.inputUserName').val().trim();
            this.LastUserNameBegin = this.SelectedUserName.toLowerCase();
            this.GetAllDepartments();
            this.ChangeDepartment();
            this.Me().find(".adv-userselector-DepsAndUsersContainer").toggle();
        };

        this.ChangeDepartment = function(DepId) {
            if (jq.browser.mobile === true)
                return;

            this.Me().find('.adv-userselector-deps .adv-userselector-userHover').removeClass('adv-userselector-userHover');
            if (!DepId) {
                DepId = ASC.Controls.AdvancedUserSelector.EmptyId;
            }
            var selectedDept = this.Me().find('#Department_' + DepId);
            selectedDept.addClass('adv-userselector-userHover');

            this.SetFocusToSelectedElement('.adv-userselector-deps');
            this.SelectedDepartmentId = DepId;
            this.GetAllUsers();
        };

        this.SelectUser = function(obj) {
            if (jq.browser.mobile === true) {
                this.SelectedUserId = jq(obj).val();
                if (this.SelectedUserId == "" || this.SelectedUserId == "-1") {
                    this.SelectedUserId = null;
                    jq(window).trigger("userSelectorSelectUserComplete", [this.ObjName, this.SelectedUserId]);
                    return;
                }
                var UserName = jq(obj).find("option:selected").text().trim();
                this.SelectedUserName = UserName;
                this.LastUserNameBegin = this.SelectedUserName.toLowerCase();
                this.AdditionalFunction(this.SelectedUserId, UserName);
                jq(window).trigger("userSelectorSelectUserComplete", [this.ObjName, this.SelectedUserId]);
                return;
            }

            this.SelectedUserId = obj.id.replace('User_', '');
            var UserName = jq.trim(jq(obj).find('.userName').text());
            this.Me().find('.inputUserName').val(UserName);
            this.Me().find('.adv-userselector-DepsAndUsersContainer').hide();
            this.SelectedUserName = UserName;
            this.LastUserNameBegin = UserName.toLowerCase();

            this.Me().find("#peopleImg").show();
            this.Me().find("#searchImg").hide();

            this.AdditionalFunction(this.SelectedUserId, UserName);
            jq(window).trigger("userSelectorSelectUserComplete", [this.ObjName, this.SelectedUserId]);
        };

        this.ClearFilter = function() {
            if (jq.browser.mobile === true)
                return;

            this.Me().find('.inputUserName').val("");
            var BegOfUserName = "";
            if ((this.LastUserNameBegin === BegOfUserName) && (this.LastUserNameBegin != ""))
                return;
            this.SelectedUserName = "";
            this.LastUserNameBegin = BegOfUserName;
            this.GetAllDepartments();
            this.GetAllUsers();

            if (this.Me().find('.adv-userselector-DepsAndUsersContainer:visible').length < 1)
                this.Me().find('.adv-userselector-DepsAndUsersContainer').show();

            this.SelectedUserId = null;
        };

        this.SuggestUser = function(event) {
            if (jq.browser.mobile === true)
                return;

            this.GetAllDepartments();
            if (event.keyCode != 9 && event.keyCode != 13)
                this.SelectedUserId = null;
            var UserName = this.Me().find('.inputUserName').val().trim(),
                BegOfUserName = UserName.toLowerCase();
            if (this.LastUserNameBegin === BegOfUserName)
                return;
            this.SelectedUserName = UserName;
            this.LastUserNameBegin = BegOfUserName;
            this.GetAllUsers();
            if (this.Me().find('.adv-userselector-DepsAndUsersContainer:visible').length < 1)
                this.Me().find('.adv-userselector-DepsAndUsersContainer').show();

            this.SelectCurUserInDept(true);

        };


        this.HideUser = function(userID, hide) {
            if (hide == undefined)
                hide = true;

            for (var i = 0, n = this.Groups.length; i < n; i++) {
                for (var j = 0, m = this.Groups[i].Users.length; j < m; j++) {
                    if (this.Groups[i].Users[j].ID == userID)
                        this.Groups[i].Users[j].Hidden = (hide == true);
                }
            }
            this.SelectedUserId = null;

            if (jq.browser.mobile === true) {
                if (hide) {
                    var elem = this.Me().find("option[value='" + userID + "']");

                    if (elem.parent().is("optgroup") && elem.parent().children().length == 1) {
                        elem.parent().remove();
                    } else {
                        elem.remove();
                    }

                    this.Me().find('option:not(:disabled):first').attr('selected', 'selected');
                    this.SelectedUserId = this.Me().find("select").val();
                } else {
                    this.RenderMobileControl();
                }
            }
        };

        this.DisplayAll = function() {
            for (var i = 0, n = this.Groups.length; i < n; i++) {
                for (var j = 0, m = this.Groups[i].Users.length; j < m; j++) {
                    this.Groups[i].Users[j].Hidden = false;
                }
            }

            if (jq.browser.mobile === true) {
                this.RenderMobileControl();
            }
        };

        this.RenderMobileControl = function() {
            if (jq.browser.mobile === false) return;

            var selectBox = this.Me().find("select").empty(),
                sb = "<option style='max-width:300px;' value='{0}' {2}>{1}</option>".format(
                        -1,
                        this.LinkText,
                        this.SelectedUserId === null ? "selected = 'selected'" : "");
            for (var i = 0; i < this.Groups.length; i++) {
                var sbTmpDep = "",
                    sbTmpUser = "";
                if (this.Groups[i].ID != ASC.Controls.AdvancedUserSelector.EmptyId)
                    sbTmpDep = "<optgroup label='{0}' style='max-width:300px;'>".format(this.Groups[i].Name);

                for (var j = 0, n = this.Groups[i].Users.length; j < n; j++) {
                    if (this.Groups[i].Users[j].Hidden == false) {
                        sbTmpUser += "<option style='max-width:300px;' value='{0}' {2}>{1}</option>".format(
                            this.Groups[i].Users[j].ID,
                            this.Groups[i].Users[j].Name,
                            this.Groups[i].Users[j].ID == this.SelectedUserId ? "selected = 'selected'" : "");
                    }
                }

                if (sbTmpUser != "") {
                    sb += sbTmpDep + sbTmpUser;
                    if (this.Groups[i].ID != ASC.Controls.AdvancedUserSelector.EmptyId)
                        sb += "</optgroup>";
                }
            }

            selectBox.html(sb);
            this.SelectedUserId = selectBox.val();
        };

        this.dropdownRegAutoHide = function(event) {
            if (jq.browser.mobile === true)
                return;

            var Obj = jq((event.target) ? event.target : event.srcElement).parents().addBack(),
                hide = true;
            for (var i = 0; i < Obj.length; i++) {
                if (Obj[i] === this.Me().find(".addUserLink")[0] || Obj[i] === this.Me().find(".adv-userselector-DepsAndUsersContainer")[0]) {
                    hide = false;
                    break;
                }
            }
            if (hide)
                this.Me().find(".adv-userselector-DepsAndUsersContainer").hide();
        };

        this.OnInputClick = function (el, event) {
            jq(".adv-userselector-DepsAndUsersContainer").not(this.Me().find('.adv-userselector-DepsAndUsersContainer')).hide();

            this.ClearFilter();
            this.Me().find("#peopleImg").hide();
            this.Me().find("#searchImg").show();
            this.SetFocusToSelectedElement();
            if (this.Me().find('.adv-userselector-DepsAndUsersContainer').css("display") == "none") {
                jq("body").click();
                this.GetAllDepartments();
                this.GetAllUsers();
            }
            this.SetFocusToSelectedElement('.adv-userselector-users');
            seletorFocus = '.adv-userselector-users'
            this.Me().find('.adv-userselector-DepsAndUsersContainer').css("display", "block");

            this.Me().find('.inputUserName').focus();

            el.CancelBubble(event);
        };

        this.CancelBubble = function(event) {
            try {
                if (event) {
                    if (event.cancelBubble) {
                        event.cancelBubble = true;
                    }
                    if (event.returnValue) {
                        event.returnValue = false;
                    }
                    if (event.stopPropagation) {
                        event.stopPropagation();
                    }
                }
            } catch (err) { }
        };

        this.ChangeSelection = function(e) {
            if (this.ChangeSelectedElement(e)) {
                var code = this.GetKeyCode(e);
                if (code == 13) {
                    var userID = this.Me().find(".loginhidden").val();
                    if (userID) {
                        this.SelectUser(document.getElementById('User_' + userID));
                    }
                    this.CancelBubble(e);
                    return 13;
                }
                return;
            }
        };

        this.ChangeSelectedElement = function(e) {
            if (e == null) {
                return false;
            }
            var code = this.GetKeyCode(e);

            if (!window.seletorFocus) {
                seletorFocus = '.adv-userselector-users';
            }
            var changedElement = false;

            if (code == 37)//left
            {
                seletorFocus = '.adv-userselector-users';
                this.SelectCurUserInDept();
            }
            if (code == 39) { //right
                seletorFocus = '.adv-userselector-deps';
            }

            if (code == 38)//up
            {
                this.GetPrevChild(seletorFocus);
                changedElement = true;
            }
            if (code == 40) { //down
                this.GetNextChild(seletorFocus);
                changedElement = true;
            }

            this.SetFocusToSelectedElement(seletorFocus);

            if (code == 13) {
                changedElement = true;
            }
            return changedElement;
        };

        this.GetKeyCode = function(e) {
            var code;
            if (!e) var e = window.event;
            if (e.keyCode) code = e.keyCode;
            else if (e.which) code = e.which;

            return code;
        };

        this.SelectCurUserInDept = function(setFocus) {
            if (setFocus) {
                seletorFocus = '.adv-userselector-users'
                this.SetFocusToSelectedElement(seletorFocus);
            }

            this.GetPrevChild('.adv-userselector-users');
            this.GetNextChild('.adv-userselector-users');
        };

        this.GetNextChild = function(seletorFocus) {
            var selectedEl = this.Me().find(seletorFocus + ' .adv-userselector-userHover'),
                next = selectedEl.size() == 1 ? selectedEl.next() : null;
            if (!next || next == null || next.size() == 0) {
                next = jq(this.Me().find(seletorFocus).children()[1]);
            }
            this.SetSelectedElement(next, seletorFocus);
        };

        this.GetPrevChild = function(seletorFocus) {
            var selectedEl = this.Me().find(seletorFocus + ' .adv-userselector-userHover'),
                next = selectedEl.size() == 1 ? selectedEl.prev() : null;
            if (!next || next == null || next.size() == 0) {
                var children = this.Me().find(seletorFocus).children();
                next = jq(children[children.length - 1]);
            }
            this.SetSelectedElement(next, seletorFocus);
        };

        this.SetSelectedElement = function(next, seletorFocus) {
            this.Me().find(seletorFocus + ' .adv-userselector-userHover').removeClass('adv-userselector-userHover');
            next.addClass('adv-userselector-userHover');
            this.ScrollDivToPosition(next, seletorFocus);
            if (seletorFocus == '.adv-userselector-users' && next.length > 0) {
                var elID = next.attr('id');
                if (elID != undefined && elID != '') {
                    elID = elID.replace('User_', '');
                    this.Me().find(".loginhidden").val(elID);
                }
            }
            if (seletorFocus == '.adv-userselector-deps') {
                next.click();
            }
        };

        this.ScrollDivToPosition = function(el, seletorFocus) {
            var offsetTop = el.attr('offsetTop') - 87;
            if (offsetTop < 0) {
                offsetTop = 0;
            }
            this.Me().find(seletorFocus).scrollTo(offsetTop);
        };

        this.SetFocusToSelectedElement = function(seletorFocus) {
            this.Me().find('.tintMedium').removeClass('tintMedium');
            if (id) {
                this.Me().find(seletorFocus + ' .adv-userselector-userHover').addClass('tintMedium');
            }
        };
    };
};