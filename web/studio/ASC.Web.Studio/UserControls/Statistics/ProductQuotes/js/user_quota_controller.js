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


ASC.UserQuotaController = (function () {

    var isInit = false;
    var isFirstLoad = true;
    var currentAnchor = null;

    var _peopleList = new Array();
    var _selectedItems = new Array();

    var pageNavigator;

    function showLoader() {
        LoadingBanner.displayLoading();
    };

    function hideLoader() {
        LoadingBanner.hideLoading();
    };


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

            data.push(profile);
        }
        return data;
    };

    function getRequestFilter(filter) {
        filter = filter || {};

        filter.fields = 'id,userName,email,avatarSmall,displayName,usedSpace,docsSpace,mailSpace,talkSpace,quotaLimit,doNotUseCache';

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
        $o.filter("tr.profile").each(function () {
            var $this = jq(this),
                id = jq(this).attr("data-id"),
                $saveBtn = $this.find(".save-btn"),
                $startModule = $this.find(".edit-quota-val"),
                $closeBtn = $this.find(".close-btn"),
                $buttons = $this.find("td.info:first [id^='peopleEmailSwitcher'] .dropdown-item");
            $buttons.on("click", onButtonClick);

            $saveBtn.on("click", function () {
                saveQuota(id, $this);
            });
            $closeBtn.on("click", function () {
                closeEditForm($this);
            });

            var size = $startModule.attr('data');
            var sizeNames = ASC.Resources.Master.FileSizePostfix ? ASC.Resources.Master.FileSizePostfix.split(',') : ["bytes", "KB", "MB", "GB", "TB"];
            var sizeNameIndex = sizeNames.indexOf(size);

            $startModule.prop('title', sizeNames[sizeNameIndex]).attr('data-id', sizeNameIndex);
            $startModule.text(sizeNames[sizeNameIndex]);

            $startModule.advancedSelector({
                height: 30 * sizeNames.length,
                itemsSelectedIds: [sizeNameIndex],
                onechosen: true,
                showSearch: false,
                itemsChoose: sizeNames.map(function (item, index) { return { id: index, title: item } }),
                sortMethod: function () { return 0; }
            })
                .on("showList",
                    function (event, item) {
                        $startModule.html(item.title).attr("title", item.title).attr('data-id', item.id);
                    });

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
                    showFunction: function (switcherObj, dropdownItem) {
                        if (dropdownItem.is(":hidden")) {
                            switcherObj.addClass("active");
                        } else {
                            switcherObj.removeClass("active");
                        }
                    },
                    hideFunction: function () {
                        groupsToggleMenu.removeClass("active");
                    }
                });

            }
        });
    };
    function closeEditForm(row) {
        row.find(".user-quota-info").removeClass("display-none");
        row.find(".set-user-quota-form").addClass("display-none");
    }

    function resetQuota(userId, row) {
        closeEditForm(row)

        var quota = -1;
        var data = { userIds: [userId], quota: quota };

        Teamlab.updateUserQuota({}, data,
            {
                success: function (params, data) {
                    var profiles = performProfiles(data);
                    updateProfiles(params, profiles);
                    if (data[0].quotaLimit > 0) {
                        toastr.success(ASC.Resources.Master.ResourceJS.QuotaEnabled);
                    }
                },
                before: LoadingBanner.displayLoading,
                after: LoadingBanner.hideLoading,
                error: function (params, errors) {

                    toastr.error(errors);
                }
            });
    }

    function saveQuota(userId, row) {
        closeEditForm(row)

        var quotaLimit = parseInt(row.find(".set-user-quota-form input").val());
        var quotaVal = row.find(".edit-quota-val").attr("data-id");
        var quota = -1;

        switch (quotaVal) {
            case '0':                                           //bytes
                quota = quotaLimit;
                break;
            case '1':
                quota = quotaLimit * 1024;
                break;
            case '2':
                quota = parseInt(quotaLimit * Math.pow(1024, 2))
                break;
            case '3':
                quota = parseInt(quotaLimit * Math.pow(1024, 3))
                break;
            case '4':
                quota = parseInt(quotaLimit * Math.pow(1024, 4))
                break;
        }
        var filter = {};
        filter.fields = 'id,userName,email,avatarSmall,displayName,usedSpace,docsSpace,mailSpace,talkSpace,quotaLimit';
        var data = { userIds: [userId], quota: quota };
        Teamlab.updateUserQuota({},
            data,
            {
                filter: filter,
                success: function (params, data) {
                    var profiles = performProfiles(data);
                    updateProfiles(params, profiles);
                    if (data[0].quotaLimit > 0) {
                        toastr.success(ASC.Resources.Master.ResourceJS.QuotaEnabled);
                    }
                },
                before: LoadingBanner.displayLoading,
                after: LoadingBanner.hideLoading,
                error: function (params, errors) {

                    toastr.error(errors);
                }
            });
    }

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
            data = data.sort(function (item) { return (-1) * item.isGroupManager; });

        _peopleList = data;

        var $o = jq.tmpl("userListTemplate",
            {
                users: data,
                isAdmin: Teamlab.profile.isAdmin || window.ASC.Resources.Master.IsProductAdmin,
                quotaEnabled: jq("#quota-enabled").is(":checked")
            });


        jq("#peopleData tbody").empty().append($o);
        bindEvents(jq($o));
        jq(window).trigger('people-render-profiles', [params, data]);

        checkFullSelection();
        renderSimplePageNavigator();
    }

    var onButtonClick = function (evt) {
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
        jq("#peopleHeaderMenu .on-top-link").trigger("click");
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
                    newAnchorObj["sortby"] = ASC.Resources.Master.userDisplayFormat == 1 ? "firstname" : "lastname";
                    newAnchorObj["sortorder"] = "ascending";
                    if (currentAnchorObj == null && (Teamlab.profile.isAdmin || window.ASC.Resources.Master.IsProductAdmin)) {
                        //check if active users exist
                        var needActiveFilterAsDefault = false;
                        var users = window.UserManager.getAllUsers(true);
                        for (var userId in users) {
                            if (!users.hasOwnProperty(userId)) continue;
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


    function showUserActionMenu(personId) {
        var $person = jq("#user_" + personId),
            email = $person.attr("data-email"),
            username = $person.attr("data-username"),
            status = $person.attr("data-status"),
            isOwner = $person.attr("data-isOwner"),
            isVisitor = $person.attr("data-isVisitor"),
            $actionMenu = jq("#editQuotaMenu"),
           
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
       
        var $menu = jq.tmpl("userQuotaMenuTemplate",
            { user: profile });
        $actionMenu.html($menu);

        var $buttons = $actionMenu.find(".dropdown-item");
        $buttons.on("click", function (e) {
            $actionMenu.hide();
            var $row = jq("#user_" + personId);
            if (jq(this).hasClass("no-quota")) {
                resetQuota(personId, $row)
            }
            else if (jq(this).hasClass("edit-quota")) {
                $row.find(".user-quota-info").addClass("display-none");
                $row.find(".set-user-quota-form").removeClass("display-none");
            }
            else if (jq(this).hasClass("block-profile")) {
                changeUserStatusAction(personId, 2, isVisitor);
            }
           
        });
    }

    function initPeopleQuotaMenu() {

        jq.dropdownToggle({
            dropdownID: "editQuotaMenu",
            switcherSelector: "#peopleData .quota-action",
            addTop: 2,
            addLeft: -2,
            fixWinSize: true,
            beforeShowFunction: function (switcherObj, dropdownItem) {
                var personId = switcherObj.attr("id").split('_')[1];
                if (!personId) {
                    return;
                }
                showUserActionMenu(personId);
            },
            showFunction: function (switcherObj, dropdownItem) {
            },
            hideFunction: function () {
            }
        });
    };
    var init = function () {

        if (isInit !== false) {
            return undefined;
        }
        isInit = true;

       
        pageNavigator = ASC.UserQuotaController.PageNavigator.pgNavigator;

        initAdvansedFilter();

        initPeopleQuotaMenu();
        initButtonsEvents();

        ASC.Controls.AnchorController.bind(onAnchChange);

    };



    var getAnchorByFilterParams = function (filters) {
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

    var resetAllFilters = function () {
        pageNavigator.CurrentPageNumber = 1;
        setPaginationCookie(1, "pageOfProfilesList");
        deselectAll();
        currentAnchor = jq.removeParam('query', currentAnchor);
        ASC.Controls.AnchorController.move("");
    };

    var moveToPage = function (page) {
        pageNavigator.CurrentPageNumber = page;
        setPaginationCookie(page, "pageOfProfilesList");
        searchQuery();
    };

    var changeCountOfRows = function (newValue) {
        if (isNaN(newValue)) {
            return;
        }
        var newCountOfRows = newValue * 1;
        pageNavigator.EntryCountOnPage = newCountOfRows;
        setPaginationCookie(newCountOfRows, "countOfProfilesList");
        ASC.UserQuotaController.moveToPage("1");
    };

    var setPaginationCookie = function (value, cookieKey) {
        if (cookieKey && cookieKey != "") {
            var cookie = {
                key: value
            };
            jq.cookies.set(cookieKey, cookie, { path: location.pathname });
        }
    };


    var showGroupActionMenu = function () {
        jq("#peopleHeaderMenu").show();
        //call ScrolledGroupMenu.stickMenuToTheTop
        jq(window).trigger("scroll");
    };
    var lockMainActions = function () {
        jq("#peopleHeaderMenu").find(".menuEditQuota, .menuNoQuota").removeClass("unlockAction").off("click");
    };

    var checkForLockMainActions = function () {
        var $changeQuotaButton = jq("#peopleHeaderMenu .menuEditQuota"),
            $resetQuotaButton = jq("#peopleHeaderMenu .menuNoQuota");

        if (_selectedItems.length === 0) {
            closeEditUsersQuotaForm();
            lockMainActions();
            return;
        } else {
            $changeQuotaButton.addClass("unlockAction");
            $resetQuotaButton.addClass("unlockAction");
        }
        
    };

    var selectAll = function (obj) {
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

    var deselectAll = function () {
        _selectedItems = new Array();
        jq("#peopleData input:checkbox").prop("checked", false);
        jq("#mainSelectAll").prop("checked", false);
        jq("#peopleData tr.selected").removeClass("selected");
        renderSelectedCount(_selectedItems.length);
        lockMainActions();
    };

    var checkFullSelection = function () {
        var rowsCount = jq("#peopleData tbody tr").length;
        var selectedRowsCount = jq("#peopleData input[id^=checkUser_]:checked").length;
        jq("#mainSelectAll").prop("checked", rowsCount == selectedRowsCount);
    };

    var renderSelectedCount = function (count) {
        if (count > 0) {
            jq("#peopleHeaderMenu .menu-action-checked-count > span").text(jq.format("{0} selected", count)); //TODO
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

    var editUsersQuota = function () {
        jq("#peopleHeaderMenu").find(".menuEditQuota, .menuNoQuota").removeClass("unlockAction").removeClass("menuAction").addClass("display-none");
        jq("#peopleHeaderMenu").find(".editUsersQuotaForm").removeClass("display-none");
    };
    var setNoUsersQuota = function () {
        var users = jq.extend(true, [], _selectedItems);
        var userIds = users.map(function (user) {
            return user.id;
        });
        saveUsersQuotaLimit(userIds, -1);
    };

    var saveUsersQuotaLimit = function (users, quota) {
       
        var data = { userIds: users, quota: quota };

        Teamlab.updateUserQuota({}, data,
            {
                success: function (params, data) {
                    var profiles = performProfiles(data);
                    updateProfiles(params, profiles);
                    if (data[0].quotaLimit > 0) {
                        toastr.success(ASC.Resources.Master.ResourceJS.QuotaEnabled);
                    }
                },
                before: LoadingBanner.displayLoading,
                after: LoadingBanner.hideLoading,
                error: function (params, errors) {

                    toastr.error(errors);
                }
            });
    }

    //redraw user rows

    var updateProfiles = function (params, data) {
        for (var i = 0, n = data.length; i < n; i++) {
            var profile = data[i];
            profile.isChecked = true;

            var $row = jq.tmpl("userListTemplate",
                {
                    users: [profile],
                    isAdmin: Teamlab.profile.isAdmin || window.ASC.Resources.Master.IsProductAdmin,
                    quotaEnabled: jq("#quota-enabled").is(":checked")
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

    //init functions

    var initAdvansedFilter = function () {
        var filters = new Array();

        filters.push(
            {
                type: "combobox",
                id: "selected-activation-status-active",
                title: ASC.Resources.Master.ResourceJS.LblActive,
                filtertitle: ASC.Resources.Master.ResourceJS.SelectorEmail,
                group: ASC.Resources.Master.ResourceJS.SelectorEmail,
                groupby: "selected-activation-status-value",
                options: [
                    { value: "active", classname: "active", title: ASC.Resources.Master.ResourceJS.LblActive, def: true },
                    { value: "pending", classname: "pending", title: ASC.Resources.Master.ResourceJS.LblPending }
                ],
                hashmask: "type/{0}"
            },
            {
                type: "combobox",
                id: "selected-activation-status-pending",
                title: ASC.Resources.Master.ResourceJS.LblPending,
                filtertitle: ASC.Resources.Master.ResourceJS.SelectorEmail,
                group: ASC.Resources.Master.ResourceJS.SelectorEmail,
                groupby: "selected-activation-status-value",
                options: [
                    { value: "active", classname: "active", title: ASC.Resources.Master.ResourceJS.LblActive },
                    { value: "pending", classname: "pending", title: ASC.Resources.Master.ResourceJS.LblPending, def: true }
                ],
                hashmask: "type/{0}"
            },
            {
                type: "combobox",
                id: "selected-type-admin",
                title: ASC.Resources.Master.Admin,
                filtertitle: ASC.Resources.Master.ResourceJS.SelectorType,
                group: ASC.Resources.Master.ResourceJS.SelectorType,
                groupby: "selected-profile-type-value",
                options: [
                    { value: "admin", classname: "admin", title: ASC.Resources.Master.Admin, def: true },
                    { value: "user", classname: "user", title: ASC.Resources.Master.User },
                    { value: "visitor", classname: "visitor", title: ASC.Resources.Master.Guest }
                ],
                hashmask: "type/{0}"
            },
            {
                type: "combobox",
                id: "selected-type-user",
                title: ASC.Resources.Master.User,
                filtertitle: ASC.Resources.Master.ResourceJS.SelectorType,
                group: ASC.Resources.Master.ResourceJS.SelectorType,
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
                filtertitle: ASC.Resources.Master.ResourceJS.SelectorType,
                group: ASC.Resources.Master.ResourceJS.SelectorType,
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
                group: ASC.Resources.Master.ResourceJS.LblOther,
                hashmask: "group/{0}"
            });

        ASC.UserQuotaController.advansedFilter = jq("#peopleFilter").advansedFilter({
            //zindex : true,
            maxfilters: -1,
            anykey: false,
            store: false,
            hintDefaultDisable: true,
            sorters:
                [
                    { id: "firstname", title: ASC.Resources.Master.ResourceJS.FirstName, dsc: false, def: ASC.Resources.Master.userDisplayFormat == 1 },
                    { id: "lastname", title: ASC.Resources.Master.ResourceJS.LastName, dsc: false, def: ASC.Resources.Master.userDisplayFormat == 2 }
                ],
            filters: filters
        })
            .on("setfilter", ASC.UserQuotaController.setFilter)
            .on("resetfilter", ASC.UserQuotaController.resetFilter)
            .on("resetallfilters", ASC.UserQuotaController.resetAllFilters);

    };

    var initButtonsEvents = function () {

        var $sizeQuotaSelect = jq("#peopleHeaderMenu").find(".edit-quota-val");
        var sizeNames = ASC.Resources.Master.FileSizePostfix ? ASC.Resources.Master.FileSizePostfix.split(',') : ["bytes", "KB", "MB", "GB", "TB"];

        $sizeQuotaSelect.prop('title', sizeNames[0]);
        $sizeQuotaSelect.text(sizeNames[0]);

        $sizeQuotaSelect.advancedSelector({
            height: 30 * sizeNames.length,
            itemsSelectedIds: [0],
            onechosen: true,
            showSearch: false,
            itemsChoose: sizeNames.map(function (item, index) { return { id: index, title: item } }),
            sortMethod: function () { return 0; }
        })
            .on("showList",
                function (event, item) {
                    $sizeQuotaSelect.html(item.title).attr("title", item.title).attr('data-id', item.id);
                });

        jq("#peopleHeaderMenu").on("click", ".menuEditQuota.unlockAction", function () {
            editUsersQuota();
            return false;
        });

        jq("#peopleHeaderMenu").on("click", ".menuNoQuota.unlockAction", function () {
            setNoUsersQuota();
            return false;
        });

        jq("#peopleHeaderMenu").on("click", ".close-btn", function () {
            closeEditUsersQuotaForm();
            return false;
        });

        jq("#peopleHeaderMenu").on("click", ".save-btn", function () {
            var users = jq.extend(true, [], _selectedItems)
            var userIds = users.map(function (user) {
                return user.id;
            });

            var quotaLimit = parseInt(jq("#peopleHeaderMenu").find(".set-user-quota-form input").val());
            var quotaVal = jq("#peopleHeaderMenu").find(".edit-quota-val").attr("data-id");
            var quota = -1;

            switch (quotaVal) {
                case '0':
                    quota = quotaLimit;
                    break;
                case '1':
                    quota = quotaLimit * 1024;
                    break;
                case '2':
                    quota = parseInt(quotaLimit * Math.pow(1024, 2))
                    break;
                case '3':
                    quota = parseInt(quotaLimit * Math.pow(1024, 3))
                    break;
                case '4':
                    quota = parseInt(quotaLimit * Math.pow(1024, 4))
                    break;
            }
            saveUsersQuotaLimit(userIds, quota);
        });
        
        jq("#peopleHeaderMenu").on("click", "#mainDeselectAll", function () {
            deselectAll();
            return false;
        });

        jq("#peopleHeaderMenu").on("click", ".on-top-link", function () {
            window.scrollTo(0, 0);
            document.querySelector('.mainPageContent').scrollTo(0, 0);
            return false;
        });


        //navigation panel
        jq("#tableForPeopleNavigation").on("change", "select", function () {
            changeCountOfRows(this.value);
        });

    };

    function closeEditUsersQuotaForm() {
        jq("#peopleHeaderMenu").find(".menuEditQuota, .menuNoQuota").addClass("unlockAction").addClass("menuAction").removeClass("display-none");
        jq("#peopleHeaderMenu").find(".editUsersQuotaForm").addClass("display-none");
    }

    return {
        init: init,
        setFilter: setFilter,
        resetFilter: resetFilter,
        moveToPage: moveToPage,

        resetAllFilters: resetAllFilters,

        selectAll: selectAll,
        selectRow: selectRow,
        selectCheckbox: selectCheckbox,
        searchQuery: searchQuery
    };
})();

function onRenderProfiles(evt, params, data) {
    jq('#peopleFilter').advansedFilter({
        nonetrigger: true,
        hintDefaultDisable: true,
        sorters: [
            { id: params.sortby, dsc: params.sortorder }
        ],
        filters: [
            {
                id: 'text',
                params: { value: params.query }
            },
            {
                id: 'selected-group',
                params: params.groupId ? { id: params.groupId } : null
            },
            {
                id: 'selected-employee-status-active',
                params: params.employeestatus ? { value: params.employeestatus } : null
            },
            {
                id: 'selected-employee-status-disabled',
                params: params.employeestatus ? { value: params.employeestatus } : null
            },
            {
                id: 'selected-activation-status-active',
                params: params.activationstatus ? { value: params.activationstatus } : null
            },
            {
                id: 'selected-activation-status-pending',
                params: params.activationstatus ? { value: params.activationstatus } : null
            },
            {
                id: 'selected-type-admin',
                params: params.type ? { value: params.type } : null
            },
            {
                id: 'selected-type-user',
                params: params.type ? { value: params.type } : null
            },
            {
                id: 'selected-type-visitor',
                params: params.type ? { value: params.type } : null
            }
        ]
    });
    if (params.groupId || params.employeestatus || params.activationstatus || params.type) {
        jq('#peopleFilter').addClass("has-filters");
    }
}

jq(document).ready(function () {
    var $peopleData = jq("#peopleData");
    if (!$peopleData.length) return;

    ASC.UserQuotaController.PageNavigator.init();
    ASC.UserQuotaController.init();

    jq(window).on('people-render-profiles', onRenderProfiles);
    $peopleData.on("click", ".check-list", ASC.UserQuotaController.selectRow);
    $peopleData.on("click", ".checkbox-user", ASC.UserQuotaController.selectCheckbox);
});
