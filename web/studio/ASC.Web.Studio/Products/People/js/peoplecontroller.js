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


; if (typeof (ASC) === 'undefined')
    ASC = {};
if (typeof ASC.People === "undefined")
    ASC.People = (function() { return {} })();

ASC.People.PeopleController = (function() {

    var isInit = false;
    var isFirstLoad = true;
    var currentAnchor = null;
    var advansedFilter = null;

    var _peopleList = new Array();
    var _selectedItems = new Array();
    var _selectedType = 1;
    var _selectedStatus = 1;
    var _tenantQuota = {};

    var pageNavigator;
    var isRetina;

    function showLoader() {
        LoadingBanner.displayLoading();
    };

    function hideLoader() {
        LoadingBanner.hideLoading();
    };

    function showFirstLoader() {
        jq(".mainPageContent").children(".loader-page").css({
            top: jq(window).height() / 2 + "px"
        });
    }

    function hideFirstLoader() {
        isFirstLoad = false;
        jq(".mainPageContent").children(".loader-page").remove();
        jq(".profile-title, #filterContainer, #tableForPeopleNavigation").removeClass("display-none");
        jq('#peopleFilter').advansedFilter("resize");
    };

    function performProfiles(profiles) {
        var data = [];

        for (var i = 0, n = profiles.length; i < n; i++) {
            var profile = profiles[i];

            profile.groups = window.GroupManager.getGroups(window.UserManager.getUser(profile.id).groups);
            profile.link = "Profile.aspx?user=" + encodeURIComponent(profile.userName);

            var daysLeft = getBithdayDaysLeft(profile.birthdayApiString);

            if ((daysLeft >= 0) && (daysLeft < 4)) {
                profile.bithdayDaysLeft = daysLeft;
            }

            data.push(profile);
        }
        return data;
    };

    function getBithdayDaysLeft(birthdayApiString) {

        function stringToDate(dateString) {
            var offset = 0;

            if (dateString.indexOf('Z') === -1) {
                offset = dateString.substring(dateString.length - 5).split(':');
                offset = (+offset[0] * 60 + +offset[1]) * (dateString.charAt(dateString.length - 6, 1) === '+' ? 1 : -1);
            }

            var parts = dateString.split('.')[0].split('T');
            parts[0] = parts[0].split('-');
            parts[1] = parts[1].split(':');

            var date = new Date(parts[0][0], parts[0][1] - 1, parts[0][2], parts[1][0], parts[1][1], parts[1][2], 0);
            date = new Date(date.getTime() - (offset * 60 * 1000));

            return date;
        }

        function isLeapYear(year) {
            if (year < 1 || year > 9999) {
                throw new Error("ArgumentOutOfRange_Year");
            }

            if (year % 4 == 0) {
                if (year % 100 == 0) {
                    return year % 400 == 0;
                }

                return true;
            }

            return false;
        }

        function getDaysLeft(birthday) {
            var oneDay = 24 * 60 * 60 * 1000;

            var now = new Date();

            var today = new Date(now.getTime() + ((now.getTimezoneOffset() + ASC.Resources.Master.CurrentTenantTimeZone.UtcOffset) * 60 * 1000));

            var checkYear = (isLeapYear(today.getFullYear()) || birthday.getMonth() == 1 && birthday.getDate() == 29) ? 2000 : 2001;

            var checkToday = new Date(checkYear, today.getMonth(), today.getDate());
            var checkBirthday = new Date(checkYear, birthday.getMonth(), birthday.getDate());

            var days = Math.round((checkBirthday - checkToday) / oneDay);

            if (days < 0) days += 365;

            return days;
        }

        if (!birthdayApiString) return -1;

        var birthdayDate = stringToDate(birthdayApiString);

        return getDaysLeft(birthdayDate);
    };

    function getRequestFilter(filter) {
        filter = filter || {};

        filter.fields = 'id,status,isAdmin,isOwner,isVisitor,activationStatus,userName,email,displayName,avatarSmall,listAdminModules,birthday,title,location,isLDAP,isSSO';

        if (jq.cookies.get("is_retina")) {
            filter.fields += ",avatar";
        }

        var anchor = jq.anchorToObject(ASC.Controls.AnchorController.getAnchor());

        for (var fld in anchor) {
            if (anchor.hasOwnProperty(fld)) {
                switch (fld) {
                    case "sortby":
                        filter.sortby = anchor[fld];
                        break;
                    case "sortorder":
                        filter.sortorder = anchor[fld];
                        break;
                    case "query":
                        filter.filtervalue = decodeURIComponent(anchor[fld]);
                        break;
                    case "group":
                        filter.groupId = anchor[fld];
                        break;
                    case "employeestatus":
                        filter.employeestatus = anchor[fld];
                        break;
                    case "activationstatus":
                        filter.activationstatus = anchor[fld];
                        break;
                    case "type":
                        filter.type = anchor[fld];
                        break;
                }
            }
        }

        if (filter.hasOwnProperty("employeestatus")) {
            switch (filter.employeestatus) {
                case "active":
                    //EmployeeStatus.Active
                    filter.employeestatus = 1;
                    break;
                case "disabled":
                    //EmployeeStatus.Terminated
                    filter.employeestatus = 2;
                    break;
                default:
                    delete filter.employeestatus;
                    break;
            }
        }

        if (filter.hasOwnProperty("activationstatus")) {
            switch (filter.activationstatus) {
                case "active":
                    //EmployeeActivationStatus.Activated
                    filter.activationstatus = 1;
                    break;
                case "pending":
                    //EmployeeActivationStatus.Pending
                    filter.activationstatus = 2;
                    break;
                default:
                    delete filter.activationstatus;
                    break;
            }
        }

        if (filter.hasOwnProperty("type")) {
            switch (filter.type) {
                case "user":
                    //EmployeeType.User
                    filter.employeeType = 1;
                    break;
                case "visitor":
                    //EmployeeType.Visitor
                    filter.employeeType = 2;
                    break;
                case "admin":
                    filter.isadministrator = true;
                    break;
            }
            delete filter.type;
        }

        return filter;
    };


    function bindEvents($o) {
        $o.filter("tr.profile").each(function() {
            var $this = jq(this),
                id = jq(this).attr("data-id"),
                $buttons = $this.find("td.info:first [id^='peopleEmailSwitcher'] .dropdown-item");
            $buttons.bind("click", onButtonClick);

            var emailToggleMenu = $this.find(".btn.email:first");
            if (emailToggleMenu.length == 1) {
                jq.dropdownToggle({
                    dropdownID: "peopleEmailSwitcher_" + id,
                    switcherSelector: "#peopleEmail_" + id,
                    addTop: 4,
                    addLeft: 17,
                    rightPos: true,
                    showFunction: function (switcherObj, dropdownItem) {
                        jq(this).parents("tr.profile:first").addClass("hover");
                    },
                    hideFunction: function () {
                        jq(this).parents("tr.profile:first").removeClass("hover");
                    }
                });

            }


            var groupsToggleMenu = $this.find("td.group:first .withHoverArrowDown");
            if (groupsToggleMenu.length == 1) {
                jq.dropdownToggle({
                    dropdownID: "peopleGroups_" + id,
                    switcherSelector: "#peopleGroupsSwitcher_" + id,
                    addTop: 4,
                    addLeft: 17,
                    rightPos: true,
                    showFunction: function(switcherObj, dropdownItem) {
                        if (dropdownItem.is(":hidden")) {
                            switcherObj.addClass("active");
                        } else {
                            switcherObj.removeClass("active");
                        }
                    },
                    hideFunction: function() {
                        groupsToggleMenu.removeClass("active");
                    }
                });

            }
        });
    };

    function renderProfiles(params, data) {
        var selectedIDs = new Array();
        for (var i = 0, n = _selectedItems.length; i < n; i++) {
            selectedIDs.push(_selectedItems[i].id);
        }
        
        var existsGroupManager = false;
        var filteredByGroup = jq.getAnchorParam('group', ASC.Controls.AnchorController.getAnchor());
        for (var i = 0, n = data.length; i < n; i++) {
            var index = jq.inArray(data[i].id, selectedIDs);
            data[i].isChecked = index != -1;
            data[i].isGroupManager = data[i].groups.some(function (group) {
                if (filteredByGroup) {
                    return filteredByGroup == group.id && group.manager == data[i].userName;
                }

                return group.manager == data[i].userName;
            });
            
            existsGroupManager = existsGroupManager || data[i].isGroupManager;
        }

        if (filteredByGroup && existsGroupManager)
            data = data.sort(function(item) { return (-1) * item.isGroupManager; });

        _peopleList = data;

        var $o = jq.tmpl("userListTemplate",
        {
            users: data,
            isAdmin: Teamlab.profile.isAdmin || window.ASC.Resources.Master.IsProductAdmin,
            isRetina: isRetina
        });
        jq("#peopleData tbody").empty().append($o);
        bindEvents(jq($o));
        jq(window).trigger('people-render-profiles', [params, data]);

        checkFullSelection();
        renderSimplePageNavigator();
    }

    var onButtonClick = function(evt) {
        var $this = jq(this);

        peopleActions.callMethodByClassname($this.attr('class'), this, [evt, $this]);
        jq(document.body).trigger('click');
    };

    function onGetProfiles(params, profiles) {
        profiles = performProfiles(profiles);
        if (profiles != null && profiles.length > 0) {
            renderProfiles(params, profiles);
            jq("#emptyContentForPeopleFilter").addClass("display-none");
            jq(".people-content").removeClass("display-none");
            showGroupActionMenu();
        } else {
            jq(window).trigger('people-render-profiles', [params, profiles]);
            jq(".people-content").addClass("display-none");
            jq("#emptyContentForPeopleFilter").removeClass("display-none");
        }
        //scroll on top
        jq("#peopleHeaderMenu .on-top-link").click();
    };

    function onDeleteGroup(params, data) {
        window.location.href = jq.clearUrl();
    }

    function onShowEditGroup(params, groupData) {
        DepartmentManagement.EditDepartmentOpenDialog(groupData);
    };

    function thisSection(cur, ne) {
        if (cur == null || ne == null)
            return false;

        cur = jq.removeParam('p', cur);
        cur = jq.removeParam('c', cur);
        cur = jq.removeParam('sortby', cur);
        cur = jq.removeParam('sortorder', cur);
        cur = jq.removeParam('query', cur);

        ne = jq.removeParam('p', ne);
        ne = jq.removeParam('c', ne);
        ne = jq.removeParam('sortby', ne);
        ne = jq.removeParam('sortorder', ne);
        ne = jq.removeParam('query', ne);

        return jq.isEqualAnchors(jq.anchorToObject(ne), jq.anchorToObject(cur));
    };

    function onAnchChange() {
        var newAnchor = ASC.Controls.AnchorController.getAnchor();

        //console.log("onAnchChange", currentAnchor, newAnchor);
        if (currentAnchor === newAnchor) {
            return undefined;
        } else {
            var newAnchorObj = jq.anchorToObject(newAnchor);
            var currentAnchorObj = currentAnchor != null ? jq.anchorToObject(currentAnchor) : null;
            if (!newAnchorObj.hasOwnProperty("sortorder")) {
                if (currentAnchorObj != null && currentAnchorObj.hasOwnProperty("sortby") && currentAnchorObj.hasOwnProperty("sortorder")) {
                    newAnchorObj["sortby"] = currentAnchorObj["sortby"];
                    newAnchorObj["sortorder"] = currentAnchorObj["sortorder"];
                } else {
                    newAnchorObj["sortby"] = ASC.People.Data.userDisplayFormat == 1 ? "firstname" : "lastname";
                    newAnchorObj["sortorder"] = "ascending";
                    if (currentAnchorObj == null && (Teamlab.profile.isAdmin || window.ASC.Resources.Master.IsProductAdmin)) {
                        //check if active users exist
                        var needActiveFilterAsDefault = false;
                        var users = window.UserManager.getAllUsers(true);
                        for (var userId in users) {
                            if (!users.hasOwnProperty(users)) continue;
                            var user = users[userId];
                            if (user.isActivated === true && user.isOwner === false) {
                                needActiveFilterAsDefault = true;
                                break;
                            }
                        }

                        if (needActiveFilterAsDefault) {
                            newAnchorObj["employeestatus"] = "active";
                        }
                    }
                }
            }

            newAnchor = jq.objectToAnchor(newAnchorObj);
            if (!thisSection(currentAnchor, newAnchor)) {
                //console.log("!thisSection", currentAnchor, newAnchor)
                //jq('input.advansed-filter-input').val('');
                newAnchor = jq.removeParam('p', newAnchor);
                newAnchor = jq.removeParam('c', newAnchor);
            }
            ASC.Controls.AnchorController.safemove(newAnchor);
            currentAnchor = newAnchor;
        }

        pageNavigator.CurrentPageNumber = 1;
        setPaginationCookie(1, "pageOfProfilesList");
        searchQuery();
        jq(window).trigger('change-group', [jq.getAnchorParam("group") || null]);
    };


    function getEditGroup() {
        var groupId = jq.getAnchorParam('group') || null;
        if (groupId == null) {
            return;
        }
        
        Teamlab.getGroup({}, groupId, {
            filter: null,
            before: showLoader,
            after: hideLoader,
            success: onShowEditGroup
        });
    };
    function changeUserStatusAction(userID, status, isVisitor) {
        jq("#peopleActionMenu").hide();
        jq("#peopleMenu_" + userID).removeClass("active");

        if (status == 1 && _tenantQuota.availableUsersCount == 0 && isVisitor == "false") {
            if (jq("#tariffLimitExceedUsersPanel").length) {
                TariffLimitExceed.showLimitExceedUsers();
            }
            return;
        }

        var user = new Array();
        user.push(userID);

        var data = {userIds: user };

        Teamlab.updateUserStatus({}, status, data, {
            success: function (params, data) {
                var profiles = performProfiles(data);
                updateProfiles(params, profiles);
                for (var i = 0, length = profiles.length; i < length; i++) {

                    if (jq.inArray(profiles[i], _selectedItems) == -1) {
                        _selectedItems.push(profiles[i]);
                    }
                }
                checkForLockMainActions();
                toastr.success(ASC.People.Resources.PeopleJSResource.SuccessChangeUserStatus);

                initTenantQuota();
            },
            before: showLoader,
            after: hideLoader,
            error: function (params, errors) {
                toastr.error(errors);
            }
        });
    }

    function showUserActionMenu(personId) {
        var $person = jq("#user_" + personId),
            email = $person.attr("data-email"),
            username = $person.attr("data-username"),
            displayname = $person.attr("data-displayname"),
            status = $person.attr("data-status"),
            isOwner = $person.attr("data-isOwner"),
            isVisitor = $person.attr("data-isVisitor"),
            $actionMenu = jq("#peopleActionMenu"),
            canEdit = $actionMenu.attr("data-canedit").toLowerCase(),
            canDel = $actionMenu.attr("data-candel").toLowerCase(),
            isLDAP = $person.attr("data-isLDAP"),
            isSSO = $person.attr("data-isSSO");

        var profile = {
            id: personId,
            email: email,
            username: username,
            status: status,
            isOwner: isOwner,
            isLDAP: isLDAP,
            isSSO: isSSO
        };
        var $menu = jq.tmpl("userActionMenuTemplate",
            { user: profile, isAdmin: Teamlab.profile.isAdmin || window.ASC.Resources.Master.IsProductAdmin, isMe: (profile.id === Teamlab.profile.id), canEdit: canEdit, canDel: canDel });
        $actionMenu.html($menu);

        var $buttons = $actionMenu.find(".dropdown-item");

        $buttons.on("click", function (e) {
            var $this = jq(e.target);
            $actionMenu.hide();
            jq("#peopleMenu_" + personId).removeClass("active");

            if (jq(this).hasClass("edit-profile")) {
                window.location.replace("ProfileAction.aspx?action=edit&user=" + encodeURIComponent(username));
            }
            else if (jq(this).hasClass("change-password")) {
                PasswordTool.ShowPwdReminderDialog("1", email);
            }
            else if (jq(this).hasClass("change-email")) {
                ASC.EmailOperationManager.showEmailChangeWindow(email, personId);
            }
            else if (jq(this).hasClass("email-activation")) {
                ASC.EmailOperationManager.sendEmailActivationInstructions(email, personId, function (response) {
                    $person.attr("data-email", response.request.args.email);
                });
            }
            else if (jq(this).hasClass("block-profile")) {
                changeUserStatusAction(personId, 2, isVisitor);
            }
            else if (jq(this).hasClass("enable-profile")) {
                changeUserStatusAction(personId, 1, isVisitor);
            }
            else if (jq(this).hasClass("reassign-data")) {
                window.location.replace("Reassigns.aspx?user=" + encodeURIComponent(username));
            }
            else if (jq(this).hasClass("remove-data")) {
                window.location.replace("Reassigns.aspx?remove=true&user=" + encodeURIComponent(username));
            }
            else if (jq(this).hasClass("delete-profile")) {
                if (personId == Teamlab.profile.id) {
                    jq("#studio_deleteProfileDialog").find(".email").attr("href", "../../addons/mail/#composeto/email=" + email).html(email);
                    StudioBlockUIManager.blockUI("#studio_deleteProfileDialog", 400);
                } else {
                    ProfileManager.RemoveUser(personId, displayname, username);
                }
            }
        });
    }

    function initPeopleActionMenu() {
        var $dropdownItem = jq("#peopleActionMenu");
        if ($dropdownItem.length == 1) {
            var leftForFix = -200;
            jq.dropdownToggle({
                dropdownID: "peopleActionMenu",
                switcherSelector: "#peopleData .entity-menu",
                addTop: 0,
                addLeft: leftForFix,
                fixWinSize: true,
                showFunction: function(switcherObj, dropdownItem) {
                    var personId = switcherObj.attr("id").split('_')[1];
                    if (!personId) {
                        return;
                    }
                    showUserActionMenu(personId);
                    switcherObj.addClass("active");
                    var left = parseInt(dropdownItem.css("left")) - dropdownItem.innerWidth() + 29 - leftForFix;
                    dropdownItem.css("left", left);
                    if (jq("#peopleData .entity-menu.active").length > 1) {
                        jq("#peopleData .entity-menu.active").not(switcherObj).removeClass("active");
                        dropdownItem.hide();
                    } else if (!dropdownItem.is(":hidden")) {
                        switcherObj.removeClass("active");
                    }
                },
                hideFunction: function() {
                    jq("#peopleData .entity-menu.active").removeClass("active");
                }
            });
        }
    };

    function renderPopups() {
        jq.tmpl("template-blockUIPanel", {
            id: "confirmationDeleteDepartmentPanel",
            headerTest: ASC.People.Resources.PeopleResource.Confirmation,
            innerHtmlText: "<div class=\"confirmationAction\"></div>",
            OKBtn: ASC.People.Resources.PeopleResource.LblOKButton,
            CancelBtn: ASC.People.Resources.PeopleResource.LblCancelButton
        }).insertAfter("#peopleActionMenu");



        jq.tmpl("template-blockUIPanel", {
            id: "studio_deleteProfileDialog",
            headerTest: jq("#studio_deleteProfileDialogBody").attr("data-header")
        }).insertAfter("#studio_deleteProfileDialogBody");
        jq("#studio_deleteProfileDialog .containerBodyBlock:first")
            .replaceWith(jq("#studio_deleteProfileDialogBody").removeClass("display-none").addClass("containerBodyBlock"));



        if (Teamlab.profile.isAdmin || window.ASC.Resources.Master.IsProductAdmin) {

            if (jq("#changeTypeDialogBody").length == 1) {
                jq.tmpl("template-blockUIPanel", {
                    id: "changeTypeDialog",
                    headerTest: ASC.People.Resources.PeopleResource.ChangeTypeDialogHeader
                })
                .insertAfter("#changeTypeDialogBody");
                jq("#changeTypeDialog .containerBodyBlock:first")
                    .replaceWith(jq("#changeTypeDialogBody").removeClass("display-none").addClass("containerBodyBlock"));
            }

            if (jq("#changeStatusDialogBody").length == 1) {
                jq.tmpl("template-blockUIPanel", {
                    id: "changeStatusDialog",
                    headerTest: ASC.People.Resources.PeopleResource.ChangeStatusDialogHeader
                })
                .insertAfter("#changeStatusDialogBody");
                jq("#changeStatusDialog .containerBodyBlock:first")
                    .replaceWith(jq("#changeStatusDialogBody").removeClass("display-none").addClass("containerBodyBlock"));
            }

            jq.tmpl("template-blockUIPanel", {
                id: "deleteUsersDialog",
                headerTest: ASC.People.Resources.PeopleResource.DeleteUserProfiles,
                innerHtmlText: [
                                "<div class=\"confirmationAction\">",
                                    ASC.People.Resources.PeopleResource.DeleteUsersDescriptionText,
                                "</div>",
                                "<div>",
                                    ASC.People.Resources.NotBeUndone,
                                "</div>",
                                "<div class=\"warning-header red-text\">",
                                    ASC.People.Resources.Warning,
                                "</div>",
                                "<div class=\"confirmationAction\">",
                                    ASC.People.Resources.PeopleResource.DeleteUsersDataConfirmation,
                                "</div>",
                                "<a class=\"link dotline showBtn\">",
                                    ASC.People.Resources.PeopleResource.ShowSelectedUserList,
                                "</a>",
                                "<a class=\"link dotline hideBtn display-none\">",
                                    ASC.People.Resources.PeopleResource.HideSelectedUserList,
                                "</a>",
                                "<div class=\"user-list-for-group-operation display-none\"></div>",
                                "<div class=\"error-popup display-none\"></div>"
                                ].join(''),
                OKBtn: ASC.People.Resources.PeopleResource.LblOKButton,
                CancelBtn: ASC.People.Resources.PeopleResource.LblCancelButton
            })
            .insertAfter("#changeTypePanel");


            jq.tmpl("template-blockUIPanel", {
                id: "resendInviteDialog",
                headerTest: ASC.People.Resources.PeopleResource.ResendInviteDialogHeader,
                innerHtmlText: ["<div>",
                                    ASC.People.Resources.PeopleResource.ResendInviteDialogTargetUsers,
                                "</div>",
                                "<div>",
                                    ASC.People.Resources.PeopleResource.ResendInviteDialogAfterActivation,
                            "</div>",
                            "<a class=\"link dotline showBtn\">",
                               ASC.People.Resources.PeopleResource.ShowSelectedUserList,
                            "</a>",
                            "<a class=\"link dotline hideBtn display-none\">",
                                ASC.People.Resources.PeopleResource.HideSelectedUserList,
                            "</a>",
                            "<div class=\"user-list-for-group-operation display-none\"></div>",
                            "<div class=\"error-popup display-none\"></div>"
                ].join(''),
                OKBtn: ASC.People.Resources.PeopleResource.LblOKButton,
                OKBtnID: "resentInviteOkBtn",
                CancelBtn: ASC.People.Resources.PeopleResource.LblCancelButton,
                CancelBtnID:"resentInviteCancelBtn"
            })
           .insertAfter("#changeTypePanel");
        }
    };
    
    function renderEmptyScreen() {
        //init emptyScreen for filter
        jq.tmpl("template-emptyScreen",
            {
                ID: "emptyContentForPeopleFilter",
                ImgSrc: ASC.People.Data.emptyScreenPeopleFilter,
                Header: ASC.People.Resources.PeopleResource.NotFoundTitle,
                Describe: ASC.People.Resources.PeopleResource.NotFoundDescription,
                ButtonHTML: ["<a class='clearFilterButton link dotline' href='javascript:void(0);'",
                    "onclick='ASC.People.PeopleController.resetAllFilters();'>",
                    ASC.People.Resources.PeopleResource.ClearButton,
                    "</a>"].join(''),
                CssClass: "display-none"
            }).insertAfter("#peopleContent");
    };

    var init = function () {
        
        if (isInit !== false) {
            return undefined;
        }
        isInit = true;

        isRetina = jq.cookies.get("is_retina");
        pageNavigator = ASC.People.PageNavigator.pgNavigator;

        showFirstLoader();
        renderEmptyScreen();
        initAdvansedFilter();
        renderPopups();  
        initTenantQuota();
        initScrolledGroupMenu();
        initButtonsEvents();
        initPeopleActionMenu();

        ASC.Controls.AnchorController.bind(onAnchChange);

    };

    

    var getAnchorByFilterParams = function(filters) {
        var newAnchor = {},
            filter = null;
        for (var i = 0, n = filters.length; i < n; i++) {
            filter = filters[i];
            switch (filter.id) {
                case "sorter":
                    newAnchor.sortby = filter.params.id;
                    newAnchor.sortorder = filter.params.sortOrder;
                    break;
                case "text":
                    newAnchor.query = encodeURIComponent(filter.params.value);
                    break;
                case "selected-employee-status-active":
                case "selected-employee-status-disabled":
                    newAnchor.employeestatus = filter.params.value;
                    break;
                case "selected-activation-status-active":
                case "selected-activation-status-pending":
                    newAnchor.activationstatus = filter.params.value;
                    break;
                case "selected-type-admin":
                case "selected-type-user":
                case "selected-type-visitor":
                    newAnchor.type = filter.params.value;
                    break;
                case "selected-group":
                    newAnchor.group = filter.params.id;
                    break;
            }
        }
        return newAnchor;
    };

    var setFilter = function (evt, $container, filter, filterparams, filters) {
        pageNavigator.CurrentPageNumber = 1;
        setPaginationCookie(1, "pageOfProfilesList");

        deselectAll();
        var 
            oldAnchor = ASC.Controls.AnchorController.getAnchor(),
            newAnchor = getAnchorByFilterParams(filters);

        if (filter.id === "text") {
            oldAnchor = jq.removeParam('p', oldAnchor);
            oldAnchor = jq.removeParam('c', oldAnchor);
        }

        oldAnchor = jq.anchorToObject(oldAnchor);
        newAnchor = jq.mergeAnchors(oldAnchor, newAnchor);

        if (!jq.isEqualAnchors(newAnchor, oldAnchor)) {
            ASC.Controls.AnchorController.move(jq.objectToAnchor(newAnchor));
        }
    };

    var resetFilter = function (evt, $container, filter, filters) {
        pageNavigator.CurrentPageNumber = 1;
        setPaginationCookie(1, "pageOfProfilesList");
        deselectAll();
        var 
            oldAnchor = jq.anchorToObject(ASC.Controls.AnchorController.getAnchor()),
            newAnchor = getAnchorByFilterParams(filters);

        if (filter.id === "text") {
            currentAnchor = jq.removeParam('query', currentAnchor);
        }
        if (!jq.isEqualAnchors(newAnchor, oldAnchor)) {
            ASC.Controls.AnchorController.move(jq.objectToAnchor(newAnchor));
        }
        if (filter.id === "text") {
            return searchQuery();
        }
    };

    function searchQuery() {
        var cookieCount = jq.cookies.get("countOfProfilesList");
        if (cookieCount) {
            pageNavigator.EntryCountOnPage = cookieCount.key;
        }
        cookieCount = jq.cookies.get("pageOfProfilesList");
        if (cookieCount) {
            pageNavigator.CurrentPageNumber = cookieCount.key;
        }
        var 
            pageCount = pageNavigator.EntryCountOnPage,
            page = pageNavigator.CurrentPageNumber,
            type = jq.getAnchorParam("type") || null,
            employeestatus = jq.getAnchorParam("employeestatus") || null,
            activationstatus = jq.getAnchorParam("activationstatus") || null,
            groupId = jq.getAnchorParam("group") || null,
            sortby = jq.getAnchorParam("sortby") || ASC.People.Data.userDisplayFormat == 1 ? "firstname" : "lastname",
            sortorder = jq.getAnchorParam("sortorder") == "descending" || false,
            search = decodeURIComponent(jq.getAnchorParam("query"));

        page = isFinite(+page) ? + page : 0;
        jq("#countOfRows").val(pageCount).tlCombobox();

        var params = {
            sortby: sortby,
            sortorder: sortorder,
            page: page,
            type: type,
            employeestatus: employeestatus,
            activationstatus: activationstatus,
            groupId: groupId,
            query: search
        };

        var filter = getRequestFilter({
            StartIndex: (page > 0 ? page - 1 : page) * pageCount,
            Count: pageCount,
            sortby: sortby,
            sortorder: sortorder,
            employeestatus: employeestatus,
            activationstatus: activationstatus
        });

        Teamlab.getProfilesByFilter(params, {
            filter: filter,
            before: function () {
                if (!isFirstLoad) {
                    showLoader();
                };
            },
            after: function () {
                isFirstLoad ? hideFirstLoader() : hideLoader();
            },
            success: onGetProfiles
        });

        if (!type && !status && !groupId && !search.length) {
            jq('#peopleFilter').advansedFilter(null);
        }
    };

    var deleteGroup = function () {
        var groupId = jq.getAnchorParam('group') || null;
        if (groupId == null) {
            return;
        }
        Teamlab.deleteGroup({}, groupId, {
            before: function () { LoadingBanner.showLoaderBtn("#confirmationDeleteDepartmentPanel") },
            after: function () { LoadingBanner.hideLoaderBtn("#confirmationDeleteDepartmentPanel") },
            success: onDeleteGroup
        });
    };


    var resetAllFilters = function () {
        pageNavigator.CurrentPageNumber = 1;
        setPaginationCookie(1, "pageOfProfilesList");
        deselectAll();
        currentAnchor = jq.removeParam('query', currentAnchor);
        ASC.Controls.AnchorController.move("");
    };

    var moveToPage = function(page) {
        pageNavigator.CurrentPageNumber = page;
        setPaginationCookie(page, "pageOfProfilesList");
        searchQuery();
    };

    var changeCountOfRows = function(newValue) {
        if (isNaN(newValue)) {
            return;
        }
        var newCountOfRows = newValue * 1;
        pageNavigator.EntryCountOnPage = newCountOfRows;
        setPaginationCookie(newCountOfRows, "countOfProfilesList");
        ASC.People.PeopleController.moveToPage("1");
    };

    var setPaginationCookie = function(value, cookieKey) {
        if (cookieKey && cookieKey != "") {
            var cookie = {
                key: value
            };
            jq.cookies.set(cookieKey, cookie, { path: location.pathname });
        }
    };

    var resize = function() {

        var mainContentWidth = 911;
        if (jq(window).width() > 1200) {
            mainContentWidth = jq(window).width() - 330;
        }
        var difference = mainContentWidth - defaultSize.mainContent;
        difference = difference > 0 ? difference : 0;

        jq("#SubTasksBody .taskList .taskPlace .taskName").each(
                function() {
                    var dflt = defaultSize.taskRowEntryTitleName;
                    jq(this).css("max-width", dflt + difference + "px");
                }
            );
    };
    //group actions

    var showGroupActionMenu = function() {
        jq("#peopleHeaderMenu").show();
        //call ScrolledGroupMenu.stickMenuToTheTop
        jq(window).scroll();
    };

    var lockMainActions = function() {
        jq("#peopleHeaderMenu").find(".menuChangeType, .menuChangeStatus, .menuSendInvite, .menuRemoveUsers, .menuWriteLetter").removeClass("unlockAction").unbind("click");
        jq("#changeTypePanel, #changeStatusPanel").hide();
    };

    var checkForLockMainActions = function() {
        var enableSendInvite = 0,
            enableChangeType = 0,
            enableChangeStatus = 0,
            enableRemoveUsers = 0,
            onlyGuestsFlag = 1,
            onlyUsersFlag = 1,
            onlyActiveFlag = 1,
            onlyTerminatedFlag = 1;
        var $changeTypeButton = jq("#peopleHeaderMenu .menuChangeType"),
            $changeStatusButton = jq("#peopleHeaderMenu .menuChangeStatus"),
            $changeTypeSelect = jq("#changeTypePanel"),
            $changeStatusSelect = jq("#changeStatusPanel"),
            $changeInviteButton = jq("#peopleHeaderMenu .menuSendInvite"),
            $changeRemoveButton = jq("#peopleHeaderMenu .menuRemoveUsers"),
            $writeLetterButton = jq("#peopleHeaderMenu .menuWriteLetter");


        if (_selectedItems.length === 0) {
            lockMainActions();
            return;
        } else {
            for (var i = 0, n = _selectedItems.length; i < n; i++) {
                var user = _selectedItems[i];
                enableChangeStatus++;
                if (!user.isTerminated) {
                    if (!(user.isAdmin || user.listAdminModules.length || user.isPortalOwner)) {
                        enableChangeType++;
                    }
                    if (!user.isActivated && !user.isSSO) {
                        enableSendInvite++;
                    }
                }
                else if (!user.isLDAP) {
                    enableRemoveUsers++;
                }
                
                if (user.isVisitor) {
                    onlyUsersFlag--;
                } else {
                    onlyGuestsFlag--;
                }
                
                if (user.isTerminated) {
                    onlyTerminatedFlag--;
                } else {
                    onlyActiveFlag--;
                }

                if (user.isPortalOwner || user.isLDAP) {
                    enableChangeStatus--;
                }
            }
            $writeLetterButton.addClass("unlockAction");
        }
        
        if (!$changeStatusButton.hasClass("unlockAction")) {
            $changeStatusButton.addClass("unlockAction");
        }
        if (enableChangeStatus > 0) {
            $changeStatusSelect.find(".dropdown-item").removeClass("disable");
            if (onlyActiveFlag > 0) {
                jq("#changeStatusPanel ul li:last-child a").addClass("disable");
            }
            if (onlyTerminatedFlag > 0) {
                jq("#changeStatusPanel ul li:first-child a").addClass("disable");
            }
        } else {
            $changeStatusButton.removeClass("unlockAction");
        }

        if (!$changeTypeButton.hasClass("unlockAction")) {
            $changeTypeButton.addClass("unlockAction");
        }
        
        if (enableChangeType>0){
            $changeTypeSelect.find(".dropdown-item").removeClass("disable");
            if (onlyGuestsFlag>0) {
                jq("#changeTypePanel ul li:last-child a").addClass("disable");
            }
            if (onlyUsersFlag>0) {
                jq("#changeTypePanel ul li:first-child a").addClass("disable");
            }
        } else {
            $changeTypeButton.removeClass("unlockAction");
        }

        if (enableSendInvite>0) {
            if (!$changeInviteButton.hasClass("unlockAction")) {
                $changeInviteButton.addClass("unlockAction");
            }
        }
        else {
            $changeInviteButton.removeClass("unlockAction");
        }

        if (enableRemoveUsers>0) {
            if (!$changeRemoveButton.hasClass("unlockAction")) {
                $changeRemoveButton.addClass("unlockAction");
            }
        }
        else {
            $changeRemoveButton.removeClass("unlockAction");
        }
    };

    var selectAll = function(obj) {
        var isChecked = jq(obj).is(":checked");

        var selectedIDs = new Array();
        for (var i = 0, n = _selectedItems.length; i < n; i++) {
            selectedIDs.push(_selectedItems[i].id);
        }

        for (var i = 0, len = _peopleList.length; i < len; i++) {
            var peopleItem = _peopleList[i];
            var index = jq.inArray(peopleItem.id, selectedIDs);
            if (isChecked && index == -1) {
                _selectedItems.push(peopleItem);
                selectedIDs.push(peopleItem.id);
                jq("#user_" + peopleItem.id).addClass("selected");
                jq("#checkUser_" + peopleItem.id).prop("checked", true);
            }
            if (!isChecked && index != -1) {
                _selectedItems.splice(index, 1);
                selectedIDs.splice(index, 1);
                jq("#user_" + peopleItem.id).removeClass("selected");
                jq("#checkUser_" + peopleItem.id).prop("checked", false);
            }
        }

        renderSelectedCount(_selectedItems.length);
        checkForLockMainActions();
    };

    var selectRow = function () {
        var id = jq(this).attr("id").split("_")[1].trim();
        selectItem(id, !jq("#checkUser_" + id).is(":checked"));
    };

    var selectCheckbox = function (e) {
        var id = jq(this).attr("id").split("_")[1].trim();
        selectItem(id, this.checked);
        if (!e) {
            e = window.event;
        }
        e.cancelBubble = true;
        if (e.stopPropagation) {
            e.stopPropagation();
        }
    };

    var selectItem = function (id, value) {
        var selectedUser = null;
        for (var i = 0, n = _peopleList.length; i < n; i++) {
            if (id == _peopleList[i].id) {
                selectedUser = _peopleList[i];
            }
        }
        var selectedIDs = new Array();
        for (i = 0, n = _selectedItems.length; i < n; i++) {
            selectedIDs.push(_selectedItems[i].id);
        }

        var index = jq.inArray(id, selectedIDs);

        jq("#checkUser_" + id).prop("checked", value === true);
        if (value) {
            jq("#user_" + id).addClass("selected");
            if (index == -1 && selectedUser != null) {
                _selectedItems.push(selectedUser);
            }
            checkFullSelection();
        } else {
            jq("#mainSelectAll").prop("checked", false);
            jq("#user_" + id).removeClass("selected");
            if (index != -1) {
                _selectedItems.splice(index, 1);
            }
        }

        renderSelectedCount(_selectedItems.length);
        checkForLockMainActions();
    };

    var deselectAll = function() {
        _selectedItems = new Array();
        jq("#peopleData input:checkbox").prop("checked", false);
        jq("#mainSelectAll").prop("checked", false);
        jq("#peopleData tr.selected").removeClass("selected");
        renderSelectedCount(_selectedItems.length);
        lockMainActions();
    };

    var checkFullSelection = function() {
        var rowsCount = jq("#peopleData tbody tr").length;
        var selectedRowsCount = jq("#peopleData input[id^=checkUser_]:checked").length;
        jq("#mainSelectAll").prop("checked", rowsCount == selectedRowsCount);
    };

    var renderSelectedCount = function(count) {
        if (count > 0) {
            jq("#peopleHeaderMenu .menu-action-checked-count > span").text(jq.format(PeopleManager.SelectedCount, count));
            jq("#peopleHeaderMenu .menu-action-checked-count").show();
        } else {
            jq("#peopleHeaderMenu .menu-action-checked-count > span").text("");
            jq("#peopleHeaderMenu .menu-action-checked-count").hide();
        }
    };

    var renderSimplePageNavigator = function () {
        jq("#peopleHeaderMenu .menu-action-simple-pagenav").html("");
        var $simplePN = jq("<div></div>");
        var lengthOfLinks = 0,
            $prevBtn = jq("#tableForPeopleNavigation .pagerPrevButtonCSSClass"),
            $nextBtn = jq("#tableForPeopleNavigation .pagerNextButtonCSSClass");
        if ($prevBtn.length != 0) {
            lengthOfLinks++;
            $prevBtn.clone().appendTo($simplePN);
        }
        if ($nextBtn.length != 0) {
            lengthOfLinks++;
            if (lengthOfLinks === 2) {
                jq("<span style='padding: 0 8px;'>&nbsp;</span>").clone().appendTo($simplePN);
            }
            $nextBtn.clone().appendTo($simplePN);
        }
        if ($simplePN.children().length != 0) {
            $simplePN.appendTo("#peopleHeaderMenu .menu-action-simple-pagenav");
            jq("#peopleHeaderMenu .menu-action-simple-pagenav").show();
        } else {
            jq("#peopleHeaderMenu .menu-action-simple-pagenav").hide();
        }
    };

    //ChangeTypeDialog

    var showChangeTypeDialog = function(obj) {
        jq("#changeTypePanel").hide();
        var type = parseInt(jq(obj).attr("data-type"));
        initChangeTypeDialog(type);
        StudioBlockUIManager.blockUI("#changeTypeDialog", 500);
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = "jq(\"#changeTypeDialogOk\").click();";
    };

    var initChangeTypeDialog = function(type) {
        _selectedType = type;
        var dialog = jq("#changeTypeDialog");
        var container = jq("#changeTypeDialog .user-list-for-group-operation");
        var users = getUsersToChangeType();

        unlockDialog(dialog);
        hideError(dialog);
        renderSelectedUserList(users, container);

        jq("#changeTypeDialogTariff").hide();
        jq("#changeTypeDialogOk").removeClass("gray").addClass("blue");
        jq("#userTypeInfo .action-info").removeClass("display-none");
        jq("#changeTypeDialog .selected-users-info").removeClass("display-none");

        if (_selectedType == 1) {
            jq("#userTypeInfo").removeClass("display-none");
            jq("#visitorTypeInfo").addClass("display-none");

            if (jq("#changeTypeDialogTariff").length) {
                jq("#changeTypeDialogTariff").show();
                if (jq("#changeTypeDialogTariff~#changeTypeDialogOk").length) {
                    jq("#changeTypeDialogOk").removeClass("blue").addClass("gray");
                }
            }

            //GET QUOTA & SET TO INTERFACE
            var quota = _tenantQuota.availableUsersCount;
            jq("#userTypeInfo .tariff-limit").html(jq.format(PeopleManager.UserLimit, "<b>", quota, "</b>"));
            updateUserListToChangeTypeByQuota(container, quota);

            if (quota == 0) {
                jq("#userTypeInfo .action-info").addClass("display-none");
                jq("#changeTypeDialog .selected-users-info").addClass("display-none");
            }
        } else if (_selectedType == 2) {
            jq("#userTypeInfo").addClass("display-none");
            jq("#visitorTypeInfo").removeClass("display-none");
        }

        if (dialog.find("input[disabled]").length == dialog.find("input").length) {
            jq("#changeTypeDialogOk").addClass("disable");
        } else {
            jq("#changeTypeDialogOk").removeClass("disable");
        }
    };

    var getUsersToChangeType = function() {
        var users = jq.extend(true, [], _selectedItems);
        for (var i = 0, n = users.length; i < n; i++) {
            var item = users[i];
            if (item.isPortalOwner || item.isAdmin || item.isMe) {
                item.locked = true;
            }
            if (item.listAdminModules.length) {
                item.locked = true;
            }
            if (item.isTerminated) {
                item.locked = true;
            }
            if (_selectedType == 1 && !item.isVisitor) {
                item.locked = true;
            }
            if (_selectedType == 2 && item.isVisitor) {
                item.locked = true;
            }
        }
        return users;
    };

    var updateUserListToChangeTypeByQuota = function (container, quota) {
        if (typeof (quota) == "undefined" || quota == null)
            return;

        var $containerBox = jq(container);
        var count = 0;

        $containerBox.find("input[type=checkbox]").each(function (i, item) {
            var $checkbox = jq(item);
            if ($checkbox.attr("locked")) {
                $checkbox.prop("checked", false).attr("disabled", true);
            } else {
                if (quota > 0 && count < quota) {
                    $checkbox.prop("checked", true).attr("disabled", false);
                    count++;
                } else {
                    $checkbox.prop("checked", false).attr("disabled", true);
                }
            }
        });

        $containerBox.on("click", "input[type=checkbox]", function () {
            selectItemToChangeTypeInUserList(this, container, quota);
        });
    };

    var selectItemToChangeTypeInUserList = function (obj, container, quota) {
        var $checkbox = jq(obj);
        var $containerBox = jq(container);
        var $inputs = $containerBox.find("input[type=checkbox]");

        if ($checkbox.is(":checked")) {
            var selectedCount = $inputs.filter(":checked").length;

            if (selectedCount > quota) {
                $checkbox.prop("checked", false);
                return;
            }

            if (selectedCount == quota) {
                $inputs.each(function (i, item) {
                    var $item = jq(item);
                    if (!$item.is(":checked"))
                        $item.attr("disabled", true);
                });
            }
        } else {
            $inputs.each(function (i, item) {
                var $item = jq(item);
                if ($item.attr("locked")) {
                    $item.prop("checked", false).attr("disabled", true);
                } else {
                    $item.attr("disabled", false);
                }
            });
        }
    };

    var changeUserType = function() {
        var dialog = jq("#changeTypeDialog");
        var container = jq("#changeTypeDialog .user-list-for-group-operation");
        var users = getSelectedUserList(container);
        var data = { userIds: users };

        if (users.length == 0) {
            jq.unblockUI();
            return false;
        }

        Teamlab.updateUserType({}, _selectedType, data, {
            success: function(params, data) {
                var profiles = performProfiles(data);                    
                updateProfiles(params, profiles);
                jq("#changeTypePanel .dropdown-item").removeClass("disable")
                jq("#changeTypePanel .dropdown-item[data-type=" + _selectedType + "]").addClass("disable");
                unlockDialog(dialog);
                jq.unblockUI();
                toastr.success(ASC.People.Resources.PeopleJSResource.SuccessChangeUserType);
                
                //SET QUOTA
                initTenantQuota();
            },
            error: function(params, errors) {
                unlockDialog(dialog);
                showError(dialog, errors);
            },
            after: function(params) {
                unlockDialog(dialog);
            },
            before: function(params) {
                lockDialog(dialog);
            }
        });
    };

    //ChangeStatusDialog

    var showChangeStatusDialog = function(obj) {
        jq("#changeStatusPanel").hide();
        var status = parseInt(jq(obj).attr("data-status"));
        initChangeStatusDialog(status);
        StudioBlockUIManager.blockUI("#changeStatusDialog", 650);
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = "jq(\"#changeStatusOkBtn\").click();";
    };

    var initChangeStatusDialog = function(status) {
        _selectedStatus = status;
        var dialog = jq("#changeStatusDialog");
        var container = jq("#changeStatusDialog .user-list-for-group-operation");
        var users = getUsersToChangeStatus();

        unlockDialog(dialog);
        hideError(dialog);
        renderSelectedUserList(users, container);

        jq("#changeStatusTariff").hide();
        jq("#changeStatusOkBtn").removeClass("gray").addClass("blue");
        jq("#activeStatusInfo .action-info").removeClass("display-none");
        jq("#changeStatusDialog .selected-users-info").removeClass("display-none");

        if (_selectedStatus == 1) {
            jq("#activeStatusInfo").removeClass("display-none");
            jq("#terminateStatusInfo").addClass("display-none");

            if (jq("#changeStatusTariff").length) {
                jq("#changeStatusTariff").show();
                if (jq("#changeStatusTariff~#changeStatusOkBtn").length) {
                    jq("#changeStatusOkBtn").removeClass("blue").addClass("gray");
                }
            }

            //GET QUOTA & SET TO INTERFACE
            var quota = _tenantQuota.availableUsersCount;
            jq("#activeStatusInfo .tariff-limit").html(jq.format(PeopleManager.UserLimitExcludingGuests, "<b>", quota, "</b>"));
            updateUserListToChangeStatusByQuota(container, quota);

        } else if (_selectedStatus == 2) {
            jq("#activeStatusInfo").addClass("display-none");
            jq("#terminateStatusInfo").removeClass("display-none");
        }

        if (dialog.find("input[disabled]").length == dialog.find("input").length) {
            jq("#changeStatusOkBtn").addClass("disable");
        } else {
            jq("#changeStatusOkBtn").removeClass("disable");
        }
    };

    var getUsersToChangeStatus = function() {
        var users = jq.extend(true, [], _selectedItems);
        for (var i = 0, n = users.length; i < n; i++) {
            var item = users[i];
            if (item.isPortalOwner || item.isMe || item.isLDAP) {
                item.locked = true;
            }
            if (_selectedStatus == 1 && !item.isTerminated) {
                item.locked = true;
            }
            if (_selectedStatus == 2 && item.isTerminated) {
                item.locked = true;
            }
        }
        return users;
    };

    var updateUserListToChangeStatusByQuota = function (container, quota) {
        if (typeof (quota) == "undefined" || quota == null)
            return;

        var $containerBox = jq(container);
        var count = 0;

        $containerBox.find("input[type=checkbox]").each(function (i, item) {
            var $checkbox = jq(item);
            if ($checkbox.attr("locked")) {
                $checkbox.prop("checked", false).attr("disabled", true);
            } else {
                if ($checkbox.attr("user-isvisitor") == "true") {
                    $checkbox.prop("checked", true).attr("disabled", false);
                } else if (quota > 0 && count < quota) {
                    $checkbox.prop("checked", true).attr("disabled", false);
                    count++;
                } else {
                    $checkbox.prop("checked", false).attr("disabled", true);
                }
            }
        });

        $containerBox.on("click", "input[type=checkbox]", function () {
            selectItemToChangeStatusInUserList(this, container, quota);
        });
    };

    var selectItemToChangeStatusInUserList = function (obj, container, quota) {
        var $checkbox = jq(obj);
        var $containerBox = jq(container);
        var $inputs = $containerBox.find("input[type=checkbox]");

        var isCheckboxVisitor = $checkbox.attr("user-isvisitor") == "true";

        if (isCheckboxVisitor) return;

        if ($checkbox.is(":checked")) {

            var selectedCount = $inputs.filter(function () {
                var cbx = jq(this);
                return cbx.is(":checked") && cbx.attr("user-isvisitor") != "true";
            }).length;

            if (selectedCount > quota) {
                $checkbox.prop("checked", false);
                return;
            }

            if (selectedCount == quota) {
                $inputs.each(function (i, item) {
                    var $item = jq(item);
                    if (!$item.is(":checked") && $item.attr("user-isvisitor") != "true")
                        $item.attr("disabled", true);
                });
            }
        } else {
            $inputs.each(function (i, item) {
                var $item = jq(item);
                if ($item.attr("locked")) {
                    $item.prop("checked", false).attr("disabled", true);
                } else {
                    $item.attr("disabled", false);
                }
            });
        }
    };

    var changeUserStatus = function() {
        var dialog = jq("#changeStatusDialog");
        var container = jq("#changeStatusDialog .user-list-for-group-operation");
        var users = getSelectedUserList(container);
        var data = { userIds: users };

        if (users.length == 0) {
            jq.unblockUI();
            return false;
        }

        Teamlab.updateUserStatus({}, _selectedStatus, data, {
            success: function(params, data) {
                var profiles = performProfiles(data);
                updateProfiles(params, profiles);
                unlockDialog(dialog);
                jq.unblockUI();
                checkForLockMainActions();
                toastr.success(ASC.People.Resources.PeopleJSResource.SuccessChangeUserStatus);

                //SET QUOTA
                initTenantQuota();
            },
            error: function(params, errors) {
                unlockDialog(dialog);
                showError(dialog, errors);
            },
            after: function(params) {
                unlockDialog(dialog);
            },
            before: function(params) {
                lockDialog(dialog);
            }
        });
    };

    //ResendInviteDialog

    var showResendInviteDialog = function () {
        jq("#changeTypePanel, #changeStatusPanel, #otherFunctionCnt").hide();
        initResendInviteDialog();
        StudioBlockUIManager.blockUI("#resendInviteDialog", 500);
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = "jq(\"#resendInviteDialog .button.blue\").click();";
    };

    var initResendInviteDialog = function() {
        var dialog = jq("#resendInviteDialog");
        var container = jq("#resendInviteDialog .user-list-for-group-operation");
        var users = getUsersToResendInvites();

        unlockDialog(dialog);
        hideError(dialog);
        renderSelectedUserList(users, container);
    };

    var getUsersToResendInvites = function() {
        var users = jq.extend(true, [], _selectedItems);
        for (var i = 0, n = users.length; i < n; i++) {
            var item = users[i];
            if (item.isActivated || item.isTerminated || item.isLDAP || item.isSSO) {
                item.locked = true;
            }
        }
        return users;
    };

    var resendInvites = function() {
        var dialog = jq("#resendInviteDialog");
        var container = jq("#resendInviteDialog .user-list-for-group-operation");
        var users = getSelectedUserList(container);
        var data = { userIds: users };

        if (users.length == 0) {
            jq.unblockUI();
            return false;
        }

        Teamlab.sendInvite({}, data, {
            success: function(params, data) {
                unlockDialog(dialog);
                jq.unblockUI();
                toastr.success(ASC.People.Resources.PeopleJSResource.SuccessSendInvitation);
            },
            error: function(params, errors) {
                unlockDialog(dialog);
                showError(dialog, errors);
            },
            after: function(params) {
                unlockDialog(dialog);
            },
            before: function(params) {
                lockDialog(dialog);
            }
        });
    };

    //RemoveUsersDialog

    var showRemoveUsersDialog = function () {
        jq("#changeTypePanel, #changeStatusPanel, #otherFunctionCnt").hide();
        initRemoveUsersDialog();
        StudioBlockUIManager.blockUI("#deleteUsersDialog", 500);
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = "jq(\"#deleteUsersDialog .button.blue\").click();";
    };

    var initRemoveUsersDialog = function () {
        var dialog = jq("#deleteUsersDialog");
        var container = jq("#deleteUsersDialog .user-list-for-group-operation");
        var users = getUsersToRemove();

        unlockDialog(dialog);
        hideError(dialog);
        renderSelectedUserList(users, container);
    };

    var getUsersToRemove = function () {
        var users = jq.extend(true, [], _selectedItems);
        for (var i = 0, n = users.length; i < n; i++) {
            var item = users[i];
            if (!item.isTerminated || item.isLDAP) {
                item.locked = true;
            }
        }
        return users;
    };
    
    var removeUsers = function () {
        var dialog = jq("#deleteUsersDialog"),
            container = jq("#deleteUsersDialog .user-list-for-group-operation"),
            users = getSelectedUserList(container),
            data = { userIds: users };

        if (users.length == 0) {
            jq.unblockUI();
            return false;
        }

        Teamlab.removeUsers({}, data, {
            success: function (params, data) {
                unlockDialog(dialog);
                jq.unblockUI();
                window.location.reload(true);
            },
            error: function (params, errors) {
                unlockDialog(dialog);
                showError(dialog, errors);
            },
            after: function (params) {
                unlockDialog(dialog);
            },
            before: function (params) {
                lockDialog(dialog);
            }
        });
    };

    // write letter for users

    var writeLetterUsers = function() {
        jq("#changeTypePanel, #changeStatusPanel, #otherFunctionCnt").hide();
        var users = jq.extend(true, [], _selectedItems),
            emails = [];
        for (var i = 0, n = users.length; i < n; i++) {
            emails.push(users[i].email);
        }
        window.open('../../addons/mail/#composeto/email=' + emails.join(), "_blank");
    };

    //redraw user rows

    var updateProfiles = function(params, data) {
        for (var i = 0, n = data.length; i < n; i++) {
            var profile = data[i];
            profile.isChecked = true;

            var $row = jq.tmpl("userListTemplate",
            {
                users: [profile],
                isAdmin: Teamlab.profile.isAdmin || window.ASC.Resources.Master.IsProductAdmin,
                isRetina: isRetina
            });
            jq("#user_" + profile.id).replaceWith($row);

            for (var j = 0, m = _peopleList.length; j < m; j++) {
                if (profile.id == _peopleList[j].id) {
                    _peopleList[j] = profile;
                }
            }

            for (var k = 0, l = _selectedItems.length; k < l; k++) {
                if (profile.id == _selectedItems[k].id) {
                    _selectedItems[k] = profile;
                }
            }
            bindEvents(jq($row));
        }
    };

    //selected user list in dialogs

    var renderSelectedUserList = function(users, container) {
        var $containerBox = jq(container);
        $containerBox.html("");
        $containerBox.off("click", "input[type=checkbox]");
        jq(users).each(function(i, item) {
            var $div = jq("<div/>");
            var $label = jq("<label/>");
            var $checkbox = jq("<input/>");
            $checkbox.attr("type", "checkbox");
            $checkbox.attr("user-id", item.id);
            $checkbox.attr("user-isVisitor", item.isVisitor);
            if (item.locked) {
                $checkbox.attr("locked", item.id).prop("checked", false).attr("disabled", true);
                $label.addClass("gray-text");
            } else {
                $checkbox.prop("checked", true).attr("disabled", false);
            }
            $label.html(item.displayName);
            $checkbox.prependTo($label);
            $label.appendTo($div);
            $div.appendTo($containerBox);
        });
    };

    var toggleSelectedUserList = function(obj) {
        var $parent = jq(obj).parents("div.containerBodyBlock:first");
        var $showBtn = $parent.find("a.showBtn");
        var $hideBtn = $parent.find("a.hideBtn");
        var $userList = $parent.find("div.user-list-for-group-operation");
        if ($userList.hasClass("display-none")) {
            $showBtn.addClass("display-none");
            $hideBtn.removeClass("display-none");
            $userList.removeClass("display-none");
        } else {
            $showBtn.removeClass("display-none");
            $hideBtn.addClass("display-none");
            $userList.addClass("display-none");
        }
    };

    var getSelectedUserList = function(obj) {
        var selectedInputs = jq(obj).find("input[type=checkbox]:checked");
        var userIds = new Array();
        selectedInputs.each(function(i, item) {
            var id = jq(item).attr("user-id").trim();
            userIds.push(id);
        });
        return userIds;
    };

    var lockDialog = function(obj) {
        LoadingBanner.showLoaderBtn(obj);
        jq(obj).find("input[type=checkbox]").attr("disabled", true);
    };

    var unlockDialog = function(obj) {
        LoadingBanner.hideLoaderBtn(obj);
        jq(obj).find("input[type=checkbox]").each(function() {
            if (!jq(this).attr("locked")) {
                jq(this).attr("disabled", false);
            }
        });
    };

    var showError = function(obj, errors) {
        jq(obj).find(".error-popup").html(errors).removeClass("display-none");
    };

    var hideError = function(obj) {
        jq(obj).find(".error-popup").html("").addClass("display-none");
    };

    //init functions

    var initAdvansedFilter = function() {
        var filters = new Array();

        if (Teamlab.profile.isAdmin || window.ASC.Resources.Master.IsProductAdmin) {
            filters.push({
                    type: "combobox",
                    id: "selected-employee-status-active",
                    title: ASC.People.Resources.PeopleJSResource.LblActive,
                    filtertitle: ASC.People.Resources.PeopleJSResource.LblStatus,
                    group: ASC.People.Resources.PeopleJSResource.LblStatus,
                    groupby: "selected-employee-status-value",
                    options: [
                        { value: "active", classname: "active", title: ASC.People.Resources.PeopleJSResource.LblActive, def: true },
                        { value: "disabled", classname: "disabled", title: ASC.People.Resources.PeopleJSResource.LblTerminated }
                    ],
                    hashmask: "type/{0}"
                },
                {
                    type: "combobox",
                    id: "selected-employee-status-disabled",
                    title: ASC.People.Resources.PeopleJSResource.LblTerminated,
                    filtertitle: ASC.People.Resources.PeopleJSResource.LblStatus,
                    group: ASC.People.Resources.PeopleJSResource.LblStatus,
                    groupby: "selected-employee-status-value",
                    options: [
                        { value: "active", classname: "active", title: ASC.People.Resources.PeopleJSResource.LblActive },
                        { value: "disabled", classname: "disabled", title: ASC.People.Resources.PeopleJSResource.LblTerminated, def: true }
                    ],
                    hashmask: "type/{0}"
                }
            );
        } 

        filters.push(
            {
                type: "combobox",
                id: "selected-activation-status-active",
                title: ASC.People.Resources.PeopleJSResource.LblActive,
                filtertitle: ASC.People.Resources.PeopleResource.Email,
                group: ASC.People.Resources.PeopleResource.Email,
                groupby: "selected-activation-status-value",
                options: [
                    { value: "active", classname: "active", title: ASC.People.Resources.PeopleJSResource.LblActive, def: true },
                    { value: "pending", classname: "pending", title: ASC.People.Resources.PeopleJSResource.LblPending }
                ],
                hashmask: "type/{0}"
            },
            {
                type: "combobox",
                id: "selected-activation-status-pending",
                title: ASC.People.Resources.PeopleJSResource.LblPending,
                filtertitle: ASC.People.Resources.PeopleResource.Email,
                group: ASC.People.Resources.PeopleResource.Email,
                groupby: "selected-activation-status-value",
                options: [
                    { value: "active", classname: "active", title: ASC.People.Resources.PeopleJSResource.LblActive },
                    { value: "pending", classname: "pending", title: ASC.People.Resources.PeopleJSResource.LblPending, def: true }
                ],
                hashmask: "type/{0}"
            },
            {
                type: "combobox",
                id: "selected-type-admin",
                title: ASC.Resources.Master.Admin,
                filtertitle: ASC.People.Resources.PeopleJSResource.LblByType,
                group: ASC.People.Resources.PeopleJSResource.LblByType,
                groupby: "selected-profile-type-value",
                options: [
                    { value: "admin", classname: "admin", title: ASC.Resources.Master.Admin, def: true },
                    { value: "user", classname: "user", title: ASC.Resources.Master.User},
                    { value: "visitor", classname: "visitor", title: ASC.Resources.Master.Guest }
                ],
                hashmask: "type/{0}"
            },
            {
                type: "combobox",
                id: "selected-type-user",
                title: ASC.Resources.Master.User,
                filtertitle: ASC.People.Resources.PeopleJSResource.LblByType,
                group: ASC.People.Resources.PeopleJSResource.LblByType,
                groupby: "selected-profile-type-value",
                options: [
                    { value: "admin", classname: "admin", title: ASC.Resources.Master.Admin },
                    { value: "user", classname: "user", title: ASC.Resources.Master.User, def: true },
                    { value: "visitor", classname: "visitor", title: ASC.Resources.Master.Guest }
                ],
                hashmask: "type/{0}"
            },
            {
                type: "combobox",
                id: "selected-type-visitor",
                title: ASC.Resources.Master.Guest,
                filtertitle: ASC.People.Resources.PeopleJSResource.LblByType,
                group: ASC.People.Resources.PeopleJSResource.LblByType,
                groupby: "selected-profile-type-value",
                options: [
                    { value: "admin", classname: "admin", title: ASC.Resources.Master.Admin },
                    { value: "user", classname: "user", title: ASC.Resources.Master.User },
                    { value: "visitor", classname: "visitor", title: ASC.Resources.Master.Guest, def: true }
                ],
                hashmask: "type/{0}"
            },
            {
                type: "group",
                id: "selected-group",
                title: ASC.Resources.Master.Department,
                group: ASC.People.Resources.PeopleJSResource.LblOther,
                hashmask: "group/{0}"
            });
            
        ASC.People.PeopleController.advansedFilter = jq("#peopleFilter").advansedFilter({
            //zindex : true,
            maxfilters: -1,
            anykey: false,
            store: false,
            hintDefaultDisable: true,
            sorters:
            [
                { id: "firstname", title: ASC.Resources.Master.Resource.FirstName, dsc: false, def: ASC.People.Data.userDisplayFormat == 1 },
                { id: "lastname", title: ASC.Resources.Master.Resource.LastName, dsc: false, def: ASC.People.Data.userDisplayFormat == 2 }
            ],
            filters: filters
        })
            .bind("setfilter", ASC.People.PeopleController.setFilter)
            .bind("resetfilter", ASC.People.PeopleController.resetFilter)
            .bind("resetallfilters", ASC.People.PeopleController.resetAllFilters);

        PeopleManager.SelectedCount = ASC.People.Resources.PeopleJSResource.SelectedCount;
        PeopleManager.UserLimit = ASC.People.Resources.PeopleJSResource.TariffActiveUserLimit;
        PeopleManager.UserLimitExcludingGuests = ASC.People.Resources.PeopleJSResource.TariffActiveUserLimitExcludingGuests;

        ASC.People.PeopleController.advansedFilter.one("adv-ready", function() {
            var peopleAdvansedFilterContainer = jq("#peopleFilter .advansed-filter-list");
            peopleAdvansedFilterContainer.find("li[data-id='selected-status-active'] .inner-text").trackEvent(ga_Categories.people, ga_Actions.filterClick, 'status_active');
            peopleAdvansedFilterContainer.find("li[data-id='selected-status-disabled'] .inner-text").trackEvent(ga_Categories.people, ga_Actions.filterClick, 'status_disabled');
            peopleAdvansedFilterContainer.find("li[data-id='selected-status-pending'] .inner-text").trackEvent(ga_Categories.people, ga_Actions.filterClick, 'status_pending');
            peopleAdvansedFilterContainer.find("li[data-id='selected-type-admin'] .inner-text").trackEvent(ga_Categories.people, ga_Actions.filterClick, 'type_admin');
            peopleAdvansedFilterContainer.find("li[data-id='selected-type-user'] .inner-text").trackEvent(ga_Categories.people, ga_Actions.filterClick, 'type_user');
            peopleAdvansedFilterContainer.find("li[data-id='selected-type-visitor'] .inner-text").trackEvent(ga_Categories.people, ga_Actions.filterClick, 'type_visitor');
            peopleAdvansedFilterContainer.find("li[data-id='selected-group'] .inner-text").trackEvent(ga_Categories.people, ga_Actions.filterClick, 'group');
        });

        jq(".people-import-banner_img").trackEvent(ga_Categories.people, ga_Actions.bannerClick, "import-people");
        jq("#peopleFilter .btn-toggle-sorter").trackEvent(ga_Categories.people, ga_Actions.filterClick, "sort");
        jq("#peopleFilter .advansed-filter-input").trackEvent(ga_Categories.people, ga_Actions.filterClick, "search_text", "enter");

    };

    var initTenantQuota = function() {
        Teamlab.getQuotas({}, {
            success: function(params, data) {
                _tenantQuota = data;
            },
            error: function(params, errors) { }
        });
    };

    var initScrolledGroupMenu = function() {
        ScrolledGroupMenu.init({
            menuSelector: "#peopleHeaderMenu",
            menuAnchorSelector: "#mainSelectAll",
            menuSpacerSelector: ".people-content .header-menu-spacer",
            userFuncInTop: function() { jq("#peopleHeaderMenu .menu-action-on-top").hide(); },
            userFuncNotInTop: function() { jq("#peopleHeaderMenu .menu-action-on-top").show(); }
        });
    };

    var initButtonsEvents = function() {
        //header menu
        jq.dropdownToggle({
            dropdownID: "changeTypePanel",
            switcherSelector: "#peopleHeaderMenu .menuChangeType.unlockAction",
            addTop: 4,
            addLeft: 0
        });

        jq.dropdownToggle({
            dropdownID: "changeStatusPanel",
            switcherSelector: "#peopleHeaderMenu .menuChangeStatus.unlockAction",
            addTop: 4,
            addLeft: 0
        });

        jq("#peopleHeaderMenu").on("click", ".menuSendInvite.unlockAction", function() {
            showResendInviteDialog();
            return false;
        });

        jq("#peopleHeaderMenu").on("click", ".menuRemoveUsers.unlockAction", function () {
            showRemoveUsersDialog();
            return false;
        });
        jq("#peopleHeaderMenu").on("click", ".menuWriteLetter.unlockAction", function () {
            writeLetterUsers();
            return false;
        });

        jq("#peopleHeaderMenu").on("click", "#mainDeselectAll", function() {
            deselectAll();
            return false;
        });

        jq("#peopleHeaderMenu").on("click", ".on-top-link", function() {
            window.scrollTo(0, 0);
            return false;
        });

        //dropdown panels
        jq("#changeTypePanel").on("click", "a.dropdown-item:not(.disable)", function () {
            showChangeTypeDialog(this);
            return false;
        });

        jq("#changeStatusPanel").on("click", "a.dropdown-item:not(.disable)", function() {
            showChangeStatusDialog(this);
            return false;
        });

        //changeTypeDialog
        jq("#changeTypeDialog").on("click", "a.showBtn, a.hideBtn", function() {
            toggleSelectedUserList(this);
            return false;
        });

        jq("#changeTypeDialog").on("click", "#changeTypeDialogOk:not(.disable)", function () {
            changeUserType();
            return false;
        });

        jq("#changeTypeDialog").on("click", "#changeTypeDialogCancel:not(.disable)", function () {
            jq.unblockUI();
            return false;
        });

        //changeStatusDialog
        jq("#changeStatusDialog").on("click", "a.showBtn, a.hideBtn", function() {
            toggleSelectedUserList(this);
            return false;
        });

        jq("#changeStatusDialog").on("click", "#changeStatusOkBtn:not(.disable)", function () {
            changeUserStatus();
            return false;
        });

        jq("#changeStatusDialog").on("click", "#changeStatusCancelBtn:not(.disable)", function () {
            jq.unblockUI();
            return false;
        });

        //resendInviteDialog
        jq("#resendInviteDialog").on("click", "a.showBtn, a.hideBtn", function() {
            toggleSelectedUserList(this);
            return false;
        });

        jq("#resendInviteDialog").on("click", "a.button.blue:not(.disable)", function() {
            resendInvites();
            return false;
        });

        jq("#resendInviteDialog").on("click", "a.button.gray:not(.disable)", function() {
            jq.unblockUI();
            return false;
        });

        //deleteUsersDialog
        jq("#deleteUsersDialog").on("click", "a.showBtn, a.hideBtn", function () {
            toggleSelectedUserList(this);
            return false;
        });

        jq("#deleteUsersDialog").on("click", "a.button.blue:not(.disable)", function () {
            removeUsers();
            return false;
        });

        jq("#deleteUsersDialog").on("click", "a.button.gray:not(.disable)", function () {
            jq.unblockUI();
            return false;
        });


        //navigation panel
        jq("#tableForPeopleNavigation").on("change", "select", function() {
            changeCountOfRows(this.value);
        });        

        // right mouse button click
        jq("body").unbind("contextmenu").bind("contextmenu", function (event) {
            var e = jq.fixEvent(event);

            if (typeof e == "undefined" || !e) {
                return true;
            }

            var target = jq(e.srcElement || e.target);
            var $dropdownItem = jq("#peopleActionMenu");

            if (!target.parents("#peopleData").length) {
                $dropdownItem.hide();
                return true;
            }

            var userField = target.closest("tr.with-entity-menu");
            if (userField.length) {
                var userId = userField.attr("id").split('_')[1];
                showUserActionMenu(userId);
                jq("#peopleData .entity-menu.active").removeClass("active");

                $dropdownItem.show();
                $dropdownItem.hide();

                jq.showDropDownByContext(e, target, $dropdownItem);

                return false;
            }
            return true;
        });

         jq.dropdownToggle({
             dropdownID: "otherFunctionCnt",
             switcherSelector: "#peopleHeaderMenu .otherFunctions",
             position: "fixed",
             addTop: 4,
             addLeft: 0,
             rightPos: true
         });

    };

    return {
        init: init,
        setFilter: setFilter,
        resetFilter: resetFilter,
        moveToPage: moveToPage,

        resetAllFilters: resetAllFilters,
        getEditGroup: getEditGroup,
        deleteGroup: deleteGroup,

        selectAll: selectAll,
        selectRow: selectRow,
        selectCheckbox: selectCheckbox
    };
})();

jq(document).ready(function() {
    var $peopleData = jq("#peopleData");
    if (!$peopleData.length) return;

    ASC.People.PeopleController.init();

    $peopleData.on("click", ".check-list", ASC.People.PeopleController.selectRow);
    $peopleData.on("click", ".checkbox-user", ASC.People.PeopleController.selectCheckbox);
});
