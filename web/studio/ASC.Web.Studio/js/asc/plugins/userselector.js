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


function UserSelector(options) {
    var ResourceJS = ASC.Resources.Master.ResourceJS;
    var GroupEveryone = ASC.Resources.Master.GroupSelector_WithGroupEveryone;
    var GroupAdmin = ASC.Resources.Master.GroupSelector_WithGroupAdmin;

    var that = this;

    init(options);

    that.changeData = function(selected, accessRights, defaultAccessRight) {
        that.selected = selected;

        if (!that.singleChoice && accessRights) {
            that.accessRights = accessRights;
            that.defaultAccessRight = defaultAccessRight;

            that.accessRightsDialogElement.firstElementChild.replaceWith(that.userDomGenerator.createAccessRightsDialogContent(accessRights));
            that.accessRightsLinkElement.firstElementChild.replaceWith(that.userDomGenerator.createAccessRightsLinkText(accessRights, defaultAccessRight));
        }
    }

    that.resetTempData = function () {
        that.tmpSelected = Object.assign({}, that.selected);
        that.searchText = "";
        that.searchResult = null;
        that.userSearcher.clear();
        that.pagging.searchItemsNumber = 0;
        that.pagging.groupsItemsNumber = 0;
        that.pagging.usersItemsNumber = 0;
    }

    that.render = function () {
        openGroup(renderGroups());
        renderCount();
    }


    function init(options) {
        initProperties(options);
        initDomElements(options);
        setBindings();
    }

    //todo: use private properties
    function initProperties(options) {
        that.userCache = options.userCache;
        that.groupCache = options.groupCache;

        that.selected = options.selected;

        that.accessRights = options.accessRights;
        that.defaultAccessRight = options.defaultAccessRight;

        that.onSave = options.onSave;
        that.onCancel = options.onCancel;

        that.singleChoice = options.singleChoice;

        that.groupSelection = options.groupSelection;

        that.tmpSelected = Object.assign({}, that.selected);

        that.searchText = "";
        that.searchResult = null;

        that.pagging = {
            renderBy: 100,
            searchItemsNumber: 0,
            groupsItemsNumber: 0,
            usersItemsNumber: 0
        };
    }

    function initDomElements(options) {
        that.userDomGenerator = options.userDomGenerator;

        that.dialogElement = that.userDomGenerator.createUserSelectorDialog(that.singleChoice, that.accessRights, that.defaultAccessRight);

        that.searchContainerElement = that.dialogElement.querySelector(".us-search");

        that.contentListParentElement = that.dialogElement.querySelector(".us-list-parent");

        that.accessRightsLinkElement = that.dialogElement.querySelector(".access-rights-container .access-rights-link");

        that.accessRightsDialogElement = that.dialogElement.querySelector(".access-rights-container .access-rights-dialog");

        that.saveBtnElement = that.dialogElement.querySelector(".small-button-container .button.blue");

        that.cancelBtnElement = that.dialogElement.querySelector(".small-button-container .button.gray");

        that.countElement = that.dialogElement.querySelector(".small-button-container .gray-text");

        that.openedGroupElement = null;
    }

    function setBindings() {
        that.userSearcher = new UserSearcher(that.groupCache, that.userCache, that.searchContainerElement, onSearchChange);

        that.contentListParentElement.addEventListener("click", onContentElementClick);

        if (that.singleChoice) {
            return;
        }

        that.accessRightsDialogHelper = new DialogHelper(that.accessRightsLinkElement, that.accessRightsDialogElement, onBeforeShowAccessRightsDialog);

        that.accessRightsDialogElement.addEventListener("click", onAccessRightsElementClick);

        that.saveBtnElement.addEventListener("click", onSaveBtnClick);

        that.cancelBtnElement.addEventListener("click", onCancelBtnClick);
    }



    function onSearchChange(text, result) {
        if (that.searchText == text) {
            return;
        }

        that.searchText = text;
        that.searchResult = result;

        if (text) {
            renderSearch();
        } else {
            openGroup(renderGroups());
        }
    };

    function onContentElementClick(event) {
        if (event.target.classList.contains("us-show-more")) {
            clickOnShowMoreElement(event.target.closest(".us-show-more-row"), event);
            return;
        }

        var element = event.target.closest(".us-name");

        if (!element || element.classList.contains("disabled")) {
            if (element && element.id == GroupEveryone.Id) {
                openGroup(element);
            }
            return;
        }

        if (element.classList.contains("us-groupname")) {
            clickOnGroupElement(element, event);
        } else {
            if (element.classList.contains("us-checkbox")) {
                clickOnCheckboxElement(element, event);
            } else {
                clickOnGroupItemElement(element, event);
            }
        }

        renderCount();
    }

    function onBeforeShowAccessRightsDialog() {
        that.accessRightsDialogElement.style.bottom = (that.accessRightsLinkElement.offsetHeight + 4) + "px";
        that.accessRightsDialogElement.style.left = that.accessRightsLinkElement.offsetLeft + "px";
        return true;
    }

    function onAccessRightsElementClick(event) {
        var element = event.target.closest(".dropdown-item");

        if (!element) {
            return;
        }

        that.defaultAccessRight = +element.getAttribute("data-access");
        that.accessRightsLinkElement.firstElementChild.textContent = element.textContent

        that.accessRightsDialogHelper.hide();
    }

    function onSaveBtnClick() {
        if (!that.singleChoice && this.classList.contains("disable")) {
            return;
        }

        that.onSave(that.tmpSelected, that.defaultAccessRight);
        that.onCancel();
    }

    function onCancelBtnClick() {
        that.onCancel();
    }



    function renderGroups() {
        var data = getGroupsFragment(0, Math.max(that.pagging.groupsItemsNumber, that.pagging.renderBy) - 1);

        that.openedGroupElement = null;
        that.pagging.groupsItemsNumber = data.count;

        if (!that.singleChoice) {
            var adminGroup = that.tmpSelected[GroupAdmin.Id];
            var adminSelected = adminGroup !== undefined;
            var adminDisabled = adminSelected && !adminGroup.canRemove;
            var adminGroupElement = that.userDomGenerator.createGroup({ id: GroupAdmin.Id, name: GroupAdmin.Name, isGroup: true, disabled: adminDisabled, selected: adminSelected, canSelect: that.groupSelection, canOpen: false });
            data.fragment.prepend(adminGroupElement);

            var everyoneGroup = that.tmpSelected[GroupEveryone.Id];
            var everyoneSelected = everyoneGroup !== undefined;
            var everyoneDisabled = everyoneGroup && !everyoneGroup.canRemove;
            var everyoneGroupElement = that.userDomGenerator.createGroup({ id: GroupEveryone.Id, name: GroupEveryone.Name, isGroup: true, disabled: everyoneDisabled, selected: everyoneSelected, canSelect: that.groupSelection });
            data.fragment.prepend(everyoneGroupElement);
        }

        var newContentListElement = that.userDomGenerator.createContentList();
        newContentListElement.append(data.fragment);
        that.contentListParentElement.firstElementChild.replaceWith(newContentListElement);

        return that.contentListParentElement.firstElementChild.querySelector(".us-name");
    }

    function getGroupsFragment(from, to) {
        var result = {
            fragment: document.createDocumentFragment(),
            count: -1
        }

        for (var groupId in that.groupCache) {
            result.count++;

            if (result.count < from) {
                continue;
            }

            if (result.count > to) {
                break;
            }

            var group = that.groupCache[groupId];
            var selectedGroup = that.tmpSelected[groupId];
            var selected = selectedGroup !== undefined;
            var disabled = selected && !selectedGroup.canRemove;
            var groupElement = that.userDomGenerator.createGroup({ id: groupId, name: group.name, isGroup: true, disabled: disabled, selected: selected, canSelect: that.groupSelection })

            result.fragment.append(groupElement);
        }

        if (result.count > to) {
            result.fragment.append(that.userDomGenerator.createShowMoreRow({ name: ResourceJS.UserSelectorShowMore }));
            return result;
        }

        result.fragment.append(that.userDomGenerator.createGroup({ id: "withouthGroup", name: ResourceJS.UserSelectorUsersWithouthGroup, isGroup: true, canSelect: false }));
        result.count++;

        return result;
    }


    function renderGroupItems(groupItemsElement, groupId, onlyGroupItemsBlock) {
        if (!onlyGroupItemsBlock) {
            that.pagging.usersItemsNumber = 0;
        }

        var newGroupItemsElement = that.userDomGenerator.createGroupItems();

        if (groupId == GroupEveryone.Id) {

            if (onlyGroupItemsBlock) {
                return;
            }

            var allGroupsDisabled = true;
            var allGroupsSelected = true;
            for (var id in that.groupCache) {
                allGroupsDisabled = false;
                if (that.tmpSelected[id] === undefined) {
                    allGroupsSelected = false;
                    break;
                }
            }

            if (allGroupsDisabled) {
                allGroupsSelected = false;
            }

            var allUsersSelected = true;
            for (var id in that.userCache) {
                if (that.tmpSelected[id] === undefined) {
                    allUsersSelected = false;
                    break;
                }
            }

            newGroupItemsElement.append(that.userDomGenerator.createGroupItem({ id: "allGroups", group: groupId, name: ResourceJS.UserSelectorCheckAllGroups, isCheckbox: true, checked: allGroupsSelected, disabled: allGroupsDisabled }));
            newGroupItemsElement.append(that.userDomGenerator.createGroupItem({ id: "allUsers", group: groupId, name: ResourceJS.UserSelectorCheckAllUsers, isCheckbox: true, checked: allUsersSelected }));

        } else if (groupId == "withouthGroup") {

            var data = getWithoutGroupItemsFragment(groupId, 0, Math.max(that.pagging.usersItemsNumber, that.pagging.renderBy) - 1);

            that.pagging.usersItemsNumber = data.count;

            var groupItemsBlockElement = that.userDomGenerator.createGroupItemsBlock();
            groupItemsBlockElement.append(data.fragment);

            if (onlyGroupItemsBlock) {
                groupItemsElement.lastElementChild.replaceWith(groupItemsBlockElement);
                return;
            }

            if (data.count > 0) {
                if (!that.singleChoice) {
                    newGroupItemsElement.append(that.userDomGenerator.createGroupItem({ id: "allUsersWithouthGroup", group: groupId, name: ResourceJS.UserSelectorCheckCurrentUsers, isCheckbox: true, checked: data.allSelected }));
                }
                newGroupItemsElement.append(groupItemsBlockElement);
            }

        } else {

            var data = getGroupItemsFragment(groupId, 0, Math.max(that.pagging.usersItemsNumber, that.pagging.renderBy) - 1);

            that.pagging.usersItemsNumber = data.count;

            var groupItemsBlockElement = that.userDomGenerator.createGroupItemsBlock();
            groupItemsBlockElement.append(data.fragment);

            if (onlyGroupItemsBlock) {
                groupItemsElement.lastElementChild.replaceWith(groupItemsBlockElement);
                return;
            }

            if (data.count > 0) {
                if (!that.singleChoice) {
                    newGroupItemsElement.append(that.userDomGenerator.createGroupItem({ id: "allUsersInGroup" + groupId, group: groupId, name: ResourceJS.UserSelectorCheckCurrentUsers, isCheckbox: true, checked: data.allSelected }));
                }
                newGroupItemsElement.append(groupItemsBlockElement);
            }
        }

        groupItemsElement.replaceWith(newGroupItemsElement);
    }

    function getWithoutGroupItemsFragment(groupId, from, to) {
        var result = {
            fragment: document.createDocumentFragment(),
            count: -1,
            totalCount: -1,
            allSelected: true
        }

        for (var userId in that.userCache) {
            var user = that.userCache[userId];
            if (user.groups.length > 0) {
                continue;
            }

            result.totalCount++;

            var selectedUser = that.tmpSelected[userId];
            var selected = selectedUser !== undefined;
            var disabled = selected && !selectedUser.canRemove;

            result.allSelected = result.allSelected && selected;

            if (result.totalCount < from) {
                result.count++;
                continue;
            }

            if (result.totalCount > to) {
                continue;
            }

            result.count++;
            result.fragment.append(that.userDomGenerator.createGroupItem({ id: userId, group: groupId, name: user.displayName, avatar: user.avatarSmall, disabled: disabled, selected: selected }));
        }

        if (result.totalCount > to) {
            result.fragment.append(that.userDomGenerator.createShowMoreRow({ name: ResourceJS.UserSelectorShowMore, group: groupId }));
        }

        result.count++;
        result.totalCount++;
        return result;
    }

    function getGroupItemsFragment(groupId, from, to) {
        var usersInGroup = that.groupCache[groupId].users;
        var groupSelected = that.tmpSelected[groupId] !== undefined;

        var result = {
            fragment: document.createDocumentFragment(),
            count: -1,
            totalCount: -1,
            allSelected: !groupSelected
        }

        for (var userId of usersInGroup) {
            var user = that.userCache[userId];
            if (!user) {
                continue; //terminated
            }

            result.totalCount++;

            var selectedUser = that.tmpSelected[userId];
            var disabled = groupSelected || (selectedUser && !selectedUser.canRemove);
            var selected = disabled || selectedUser !== undefined;

            result.allSelected = result.allSelected && selected;

            if (result.totalCount < from) {
                result.count++;
                continue;
            }

            if (result.totalCount > to) {
                continue;
            }

            result.count++;
            result.fragment.append(that.userDomGenerator.createGroupItem({ id: userId, group: groupId, name: user.displayName, avatar: user.avatarSmall, disabled: disabled, selected: selected }));
        }

        if (result.totalCount > to) {
            result.fragment.append(that.userDomGenerator.createShowMoreRow({ name: ResourceJS.UserSelectorShowMore, group: groupId }));
        }

        result.count++;
        result.totalCount++;
        return result;
    }


    function renderSearch() {
        var data = getSearchFragment(0, that.pagging.renderBy - 1);

        that.pagging.searchItemsNumber = data.count;

        var newContentListElement = that.userDomGenerator.createContentList();

        var groupsContentBlockElement = that.userDomGenerator.createGroupItemsBlock();
        var usersContentBlockElement = that.userDomGenerator.createGroupItemsBlock();

        groupsContentBlockElement.append(data.groupsFragment);
        usersContentBlockElement.append(data.usersFragment);

        newContentListElement.append(groupsContentBlockElement);
        newContentListElement.append(usersContentBlockElement);

        that.contentListParentElement.firstElementChild.replaceWith(newContentListElement);

        that.openedGroupElement = null;
    }

    function renderUsersSearchBlock() {

        var data = getSearchFragment(0, that.pagging.searchItemsNumber - 1);

        var usersContentBlockElement = that.userDomGenerator.createGroupItemsBlock();
        usersContentBlockElement.append(data.usersFragment);

        that.contentListParentElement.firstElementChild.lastElementChild.replaceWith(usersContentBlockElement);
    }

    function getSearchFragment(from, to) {
        var result = {
            groupsFragment: document.createDocumentFragment(),
            usersFragment: document.createDocumentFragment(),
            count: -1
        }

        for (var groupId in that.searchResult.groups) {

            result.count++;

            if (result.count < from) {
                continue;
            }

            if (result.count > to) {
                break;
            }

            var group = that.searchResult.groups[groupId];
            var selectedGroup = that.tmpSelected[groupId];
            group.selected = selectedGroup !== undefined;
            group.disabled = selectedGroup && !selectedGroup.canRemove;
            group.canSelect = that.groupSelection;

            var groupElement = that.userDomGenerator.createGroup(group);
            result.groupsFragment.append(groupElement);
        }

        if (result.count > to) {
            result.groupsFragment.append(that.userDomGenerator.createShowMoreRow({ name: ResourceJS.UserSelectorShowMore }));
            return result;
        }

        for (var userId in that.searchResult.users) {

            result.count++;

            if (result.count < from) {
                continue;
            }

            if (result.count > to) {
                break;
            }

            var user = that.searchResult.users[userId];
            var selectedUser = that.tmpSelected[userId];
            user.selected = selectedUser !== undefined;
            user.disabled = selectedUser && !selectedUser.canRemove;

            var groupItemElement = that.userDomGenerator.createGroupItem(user);
            result.usersFragment.append(groupItemElement);
        }

        if (result.count > to) {
            result.usersFragment.append(that.userDomGenerator.createShowMoreRow({ name: ResourceJS.UserSelectorShowMore }));
            return result;
        }

        result.count++;

        if (result.count == 0) {
            result.usersFragment.append(that.userDomGenerator.createEmptySearchRow());
        }

        return result;
    }


    function renderCount() {
        if (that.singleChoice) {
            return;
        }

        var count = Object.keys(that.tmpSelected).length;
        that.countElement.textContent = ResourceJS.UserSelectorSelectedCount.format(count);

        that.saveBtnElement.classList.toggle("disable", count == 0);
    }



    function openGroup(element, rerender) {
        if (!element) {
            that.openedGroupElement = null;
            return;
        }

        if (element.classList.contains("unopenable")) {
            return;
        }

        if (element == that.openedGroupElement && !rerender) {
            element.parentElement.classList.toggle("opened");
            return;
        }

        if (that.openedGroupElement && element != that.openedGroupElement) {
            that.openedGroupElement.parentElement.classList.remove("opened");
        }

        renderGroupItems(element.nextElementSibling, element.id, false);
        element.parentElement.classList.add("opened");
        that.openedGroupElement = element;
    }


    function markItemSelected(itemId, isGroup, selected) {
        var item = that.tmpSelected[itemId];
        if (selected) {
            if (item !== undefined) {
                return;
            }
            if (isGroup) {
                if (itemId == GroupEveryone.Id) {
                    that.tmpSelected[itemId] = { id: itemId, name: GroupEveryone.Name, isGroup: true, canOpen: false, canRemove: true };
                } else if (itemId == GroupAdmin.Id) {
                    that.tmpSelected[itemId] = { id: itemId, name: GroupAdmin.Name, isGroup: true, canOpen: false, canRemove: true };
                } else {
                    var group = that.groupCache[itemId];
                    that.tmpSelected[itemId] = { id: itemId, name: group.name, isGroup: true, canRemove: true };
                }
            } else {
                var user = that.userCache[itemId];
                that.tmpSelected[itemId] = { id: itemId, name: user.displayName, avatar: user.avatarSmall, canRemove: true };
            }
        } else {
            if (item === undefined) {
                return;
            }
            if (item.canRemove === false) {
                return;
            }
            delete that.tmpSelected[itemId];
        }
    }


    function clickOnGroupElement(element, event) {
        if ((event.target.classList.contains("us-avatar") || element.classList.contains("unopenable")) && !element.classList.contains("unselectable")) {
            var selected = element.classList.toggle("selected");
            markItemSelected(element.id, true, selected);

            if (that.singleChoice) {
                onSaveBtnClick();
                return;
            }

            if (element == that.openedGroupElement) {
                var opened = element.parentElement.classList.contains("opened");
                openGroup(element, true);
                element.parentElement.classList.toggle("opened", opened)
            }
        } else {
            openGroup(element);
        }
    }

    function clickOnCheckboxElement(element, event) {
        var cbx = element.querySelector("input[type=checkbox]");

        if (event.target != cbx) {
            cbx.checked = !cbx.checked;
        }

        var selected = cbx.checked;

        var groupId = element.getAttribute("data-group");

        if (groupId == GroupEveryone.Id) {

            if (element.id == "allGroups") {

                for (var groupId in that.groupCache) {
                    markItemSelected(groupId, true, selected);
                }

                openGroup(renderGroups());

            } else if (element.id == "allUsers") {

                for (var userId in that.userCache) {
                    markItemSelected(userId, false, selected);
                }

            }

        } else if (groupId == "withouthGroup") {

            for (var userId in that.userCache) {
                var user = that.userCache[userId];
                if (user.groups.length > 0) {
                    continue;
                }
                markItemSelected(userId, false, selected);
            }

            renderGroupItems(element.parentElement, groupId, true);

        } else {

            if (that.tmpSelected[groupId] !== undefined) {
                markItemSelected(groupId, true, false);
                element.parentElement.previousElementSibling.classList.remove("selected");
            }

            var needRenderUsersSearchBlock = false;
            var usersInGroup = that.groupCache[groupId].users;
            for (var userId of usersInGroup) {
                if (!that.userCache[userId]) {
                    continue; //terminated
                }
                markItemSelected(userId, false, selected);
                if (that.searchText && that.searchResult.users[userId]) {
                    needRenderUsersSearchBlock = true;
                }
            }

            renderGroupItems(element.parentElement, groupId, true);

            if (needRenderUsersSearchBlock) {
                renderUsersSearchBlock();
            }
        }
    }

    function clickOnGroupItemElement(element, event) {
        var selected = element.classList.toggle("selected");
        markItemSelected(element.id, false, selected);

        if (that.singleChoice) {
            onSaveBtnClick();
            return;
        }

        var groupId = element.getAttribute("data-group");

        if (that.searchText) {

            if (groupId) {
                if (that.searchResult.users[element.id] !== undefined) {
                    var itemElement = that.contentListParentElement.firstElementChild.lastElementChild.children[element.id];
                    if (itemElement && itemElement.classList.contains("us-name")) {
                        itemElement.classList.toggle("selected", selected);
                    }
                }
            } else {
                if (that.openedGroupElement && that.tmpSelected[that.openedGroupElement.id] === undefined) {
                    var index = that.groupCache[that.openedGroupElement.id].users.indexOf(element.id);
                    if (index != -1) {
                        var itemElement = that.openedGroupElement.nextElementSibling.lastElementChild.children[element.id];
                        if (itemElement && itemElement.classList.contains("us-name")) {
                            itemElement.classList.toggle("selected", selected);
                        }
                        groupId = that.openedGroupElement.id;
                    }
                }
            }

        }

        if (groupId == "withouthGroup") {

            var allChecked = true;
            for (var userId in that.userCache) {
                var user = that.userCache[userId];
                if (user.groups.length > 0) {
                    continue;
                }
                if (that.tmpSelected[userId] === undefined) {
                    allChecked = false;
                    break;
                }
            }
            element.parentElement.previousElementSibling.querySelector("input").checked = allChecked;

        } else if (groupId) {

            var allChecked = true;
            var usersInGroup = that.groupCache[groupId].users;
            for (var userId of usersInGroup) {
                if (!that.userCache[userId]) {
                    continue; //terminated
                }
                if (that.tmpSelected[userId] === undefined) {
                    allChecked = false;
                    break;
                }
            }
            element.parentElement.previousElementSibling.querySelector("input").checked = allChecked;

        }
    }

    function clickOnShowMoreElement(element, event) {
        var groupId = element.getAttribute("data-group");

        if (groupId == null) {
            if (that.searchText) {
                var data = getSearchFragment(that.pagging.searchItemsNumber, that.pagging.searchItemsNumber + that.pagging.renderBy - 1);
                that.pagging.searchItemsNumber = data.count;
                that.contentListParentElement.firstElementChild.firstElementChild.append(data.groupsFragment);
                that.contentListParentElement.firstElementChild.lastElementChild.append(data.usersFragment);
            } else {
                var data = getGroupsFragment(that.pagging.groupsItemsNumber, that.pagging.groupsItemsNumber + that.pagging.renderBy - 1);
                that.pagging.groupsItemsNumber = data.count;
                that.contentListParentElement.firstElementChild.append(data.fragment);
            }
        } else {
            var data = (groupId == "withouthGroup")
                ? getWithoutGroupItemsFragment(groupId, that.pagging.usersItemsNumber, that.pagging.usersItemsNumber + that.pagging.renderBy - 1)
                : getGroupItemsFragment(groupId, that.pagging.usersItemsNumber, that.pagging.usersItemsNumber + that.pagging.renderBy - 1);
            that.pagging.usersItemsNumber = data.count;
            element.parentElement.append(data.fragment);
        }

        element.remove();
    }
};


function UserSearcher(groupCache, userCache, searchElement, onChange) {
    var that = this;

    var inputElement = searchElement.querySelector(".textEdit");
    var clearElement = searchElement.querySelector(".clear-search");

    var timerId = null;

    that.debounceSearch = function () {
        clearTimeout(timerId);
        timerId = setTimeout(that.search, 500);
    }

    that.search = function () {
        var text = inputElement.value;
        if (text) {
            var result = that.findItems(text);
            searchElement.classList.add("active");
            onChange(text, result);
        } else {
            that.clear();
            onChange("", null);
        }
    }

    that.findItems = function(text) {
        text = text.toLowerCase();

        var result = {
            groups: {},
            users: {}
        };

        for (var groupId in groupCache) {
            var group = groupCache[groupId];

            if (group.name.toLowerCase().indexOf(text) === -1) {
                continue;
            }

            result.groups[groupId] = { id: groupId, name: group.name, isGroup: true };
        }

        for (var userId in userCache) {
            var user = userCache[userId];

            if (user.displayName.toLowerCase().indexOf(text) === -1 && user.email.toLowerCase().indexOf(text) === -1) {
                continue;
            }

            result.users[userId] = { id: userId, name: user.displayName, avatar: user.avatarSmall };
        }

        return result;
    }

    that.clear = function () {
        searchElement.classList.remove("active");
        inputElement.value = "";
    }

    inputElement.addEventListener("keyup", function (event) {
        var text = this.value;

        if (event.key === "Escape" && text) {
            event.stopPropagation();
            that.clear();
            onChange("", null);
            return;
        }

        if (event.key !== "Enter") {
            return;
        }

        clearTimeout(timerId);
        that.search();
    })

    inputElement.addEventListener("input", that.debounceSearch)

    clearElement.addEventListener("click", function () {
        that.clear();
        onChange("", null);
    })
}


function UserDomGenerator() {
    var ResourceJS = ASC.Resources.Master.ResourceJS;

    this.createCbx = function (item) {
        var cbx = document.createElement("input");
        cbx.setAttribute("type", "checkbox");
        cbx.checked = item.checked;
        cbx.disabled = item.disabled;
        return cbx;
    }

    this.createAvatar = function(item) {
        var avatar = document.createElement("span");
        avatar.setAttribute("class", "us-avatar");

        if (item.avatar && !item.avatar.endsWith("default_user_photo_size_32-32.png") && !item.avatar.endsWith("default_user_photo_dark_size_32-32.png")) { 
            avatar.setAttribute("style", "background-image: url(" + item.avatar + ")");
        }

        return avatar;
    }

    this.createLinkSettings = function (item) {
        var settings = document.createElement("span");
        settings.setAttribute("class", "us-settings");

        var passwordSettings = document.createElement("span");
        passwordSettings.setAttribute("class", "us-settings-item us-settings-password" + (!item.linkSettings.password ? "" : " enabled"));
        passwordSettings.setAttribute("title", ResourceJS.PasswordProtection);
        settings.append(passwordSettings);

        var expirationSettings = document.createElement("span");
        expirationSettings.setAttribute("class", "us-settings-item us-settings-lifetime" + (!item.linkSettings.expirationDate ? "" : item.linkSettings.expired ? " warning" : " enabled"));
        expirationSettings.setAttribute("title", ResourceJS.Lifetime);
        settings.append(expirationSettings);

        return settings;
    }

    this.createAccess = function (item, static) {
        var access = document.createElement("span");
        access.setAttribute("class", "us-access" + (item.canEdit === false ? " uneditable" : "") + (item.isOwner ? " owner" : " access-" + item.access) + (static ? " static" : ""));
        access.setAttribute("title", item.accessName || "");
        return access;
    }

    this.createName = function (item) {
        var name = document.createElement("div");
        name.setAttribute("id", item.id);
        name.setAttribute("class", "us-name" +
            (item.isGroup ? " us-groupname" : "") +
            (item.isLink ? item.entryType === ASC.Files.Constants.EntryType.Folder ? " us-folder-linkname" : " us-linkname" : "") +
            (item.isCheckbox ? " us-checkbox" : "") +
            (item.selected ? " selected" : "") +
            (item.disabled ? " disabled" : "") +
            (item.infoText ? " info" : "") +
            (item.canSelect === false ? " unselectable" : "") +
            (item.canOpen === false ? " unopenable" : "") +
            (item.canEdit === false ? " uneditable" : "") +
            (item.canChange === true ? " changeable" : ""));

        if (item.group) {
            name.setAttribute("data-group", item.group);
        }

        var text = document.createElement("span");
        text.setAttribute("class", "us-name-text");

        var inner = document.createElement("span");
        var displayName = Encoder.htmlDecode(item.name);
        if (!item.withoutTitle) {
            inner.setAttribute("title", displayName);
        }
        inner.append(document.createTextNode(displayName))

        text.append(inner);

        if (item.infoText) {
            text.append(this.createNameInfoText(item.infoText));
        }

        name.append(text);

        return name;
    }

    this.createNameInfoText = function (text) {
        var info = document.createElement("i");
        info.setAttribute("class", "gray-text");
        info.append(document.createTextNode(text))
        return info;
    }

    this.createGroupItems = function() {
        var groupItems = document.createElement("div");
        groupItems.setAttribute("class", "us-group-items");
        return groupItems;
    }

    this.createGroupItemsBlock = function () {
        var groupItems = document.createElement("div");
        groupItems.setAttribute("class", "us-group-items-block");
        return groupItems;
    }

    this.createGroup = function(item) {
        var avatar = this.createAvatar(item);

        var name = this.createName(item);
        name.prepend(avatar);

        if (item.access != undefined) {
            var access = this.createAccess(item);
            name.append(access);
        }

        var items = this.createGroupItems();

        var group = document.createElement("div");
        group.setAttribute("class", "us-group");
        group.append(name);
        group.append(items);

        return group;
    }

    this.createGroupItem = function(item) {
        var name = this.createName(item);

        if (item.isCheckbox) {
            var cbx = this.createCbx(item);
            name.prepend(cbx);
        } else {
            var avatar = this.createAvatar(item);
            name.prepend(avatar);
        }

        if (item.access != undefined) {
            var access = this.createAccess(item);
            name.append(access);
        }

        return name;
    }

    this.createShowMoreRow = function (item) {
        var row = document.createElement("div");
        row.setAttribute("class", "us-show-more-row");

        if (item.group) {
            row.setAttribute("data-group", item.group);
        }

        var link = document.createElement("span");
        link.setAttribute("class", "link dotline us-show-more");
        link.textContent = item.name;

        row.append(link);

        return row;
    }

    this.createEmptySearchRow = function () {
        var row = document.createElement("div");
        row.setAttribute("class", "us-empty-row gray-text");
        row.textContent = ASC.Resources.Master.ResourceJS.UserSelectorNoResults;
        return row;
    }

    this.createLink = function (item) {
        var avatar = this.createAvatar(item);

        var name = this.createName(item);
        name.prepend(avatar);

        var hasSettings = item.linkSettings != undefined && !item.inherited;
        if (hasSettings) {
            var linkSettings = this.createLinkSettings(item);
            name.append(linkSettings);
        }

        if (item.access != undefined) {
            var access = this.createAccess(item, hasSettings);
            name.append(access);
        }

        return name;
    }

    this.createLinkRenameBlock = function () {
        var block = document.createElement("div");
        block.setAttribute("class", "us-rename");

        var input = document.createElement("input");
        input.setAttribute("class", "textEdit");
        input.setAttribute("type", "text");
        block.append(input);

        var apply = document.createElement("span");
        apply.setAttribute("class", "button gray btn-action __apply");
        apply.setAttribute("title", ResourceJS.SaveButton);
        block.append(apply);

        var reset = document.createElement("span");
        reset.setAttribute("class", "button gray btn-action __reset");
        reset.setAttribute("title", ResourceJS.CancelButton);
        block.append(reset);

        return block;
    }

    this.createAccessRightsDialogContent = function (accessRights) {
        var content = document.createElement("ul");
        content.setAttribute("class", "dropdown-content");

        for (var item of accessRights) {
            if (item.disabled) {
                continue;
            }

            var contentItem = document.createElement("li");

            var contentItemLink = document.createElement("a");
            contentItemLink.setAttribute("class", "dropdown-item");
            contentItemLink.setAttribute("data-access", item.id);
            contentItemLink.append(document.createTextNode(item.name))

            contentItem.append(contentItemLink);
            content.append(contentItem);
        }

        return content;
    }

    this.createAccessRightsImgDialogContent = function (accessRights) {
        var fragment = document.createDocumentFragment();

        for (var item of accessRights) {
            if (item.disabled) {
                continue;
            }

            var accessRightsItem = document.createElement("div");
            accessRightsItem.setAttribute("class", "access-rights-item");
            accessRightsItem.setAttribute("data-id", item.id);

            var accessRightsItemText = document.createElement("div");
            accessRightsItemText.setAttribute("class", "us-access access-" + item.id);
            accessRightsItemText.append(document.createTextNode(item.name))

            accessRightsItem.append(accessRightsItemText);
            fragment.append(accessRightsItem);
        }

        var separator = document.createElement("div");
        separator.setAttribute("class", "dropdown-item-seporator");
        fragment.append(separator);

        var removeItem = document.createElement("div");
        removeItem.setAttribute("class", "access-rights-item");
        removeItem.setAttribute("data-id", "remove");

        var removeItemText = document.createElement("div");
        removeItemText.setAttribute("class", "us-access access-remove");
        removeItemText.append(document.createTextNode(ResourceJS.RemoveButton))

        removeItem.append(removeItemText);
        fragment.append(removeItem);

        return fragment;
    }

    this.createAccessRightsLinkText = function (accessRights, defaultAccessRight) {
        var linkText = document.createElement("span");
        linkText.setAttribute("class", "access-rights-linktext");

        for (var item of accessRights) {
            if (item.id == defaultAccessRight) {
                linkText.append(document.createTextNode(item.name))
                break;
            }
        }

        return linkText;
    }

    this.createAccessRightsContainer = function (accessRights, defaultAccessRight) {
        var accessRightsContainer = document.createElement("div");
        accessRightsContainer.setAttribute("class", "access-rights-container");
        accessRightsContainer.append(document.createTextNode(ResourceJS.UserSelectorAddWithAccessRights + ":"));

        var accessRightsLink = document.createElement("span");
        accessRightsLink.setAttribute("class", "access-rights-link");
        accessRightsLink.append(this.createAccessRightsLinkText(accessRights, defaultAccessRight));

        var accessRightsDialog = document.createElement("div");
        accessRightsDialog.setAttribute("class", "access-rights-dialog studio-action-panel");
        accessRightsDialog.append(this.createAccessRightsDialogContent(accessRights));

        accessRightsContainer.append(accessRightsLink);
        accessRightsContainer.append(accessRightsDialog);

        return accessRightsContainer;
    }

    this.createContentList = function () {
        var contentList = document.createElement("div");
        contentList.setAttribute("class", "us-list");
        return contentList;
    }

    this.createUserSelectorDialog = function (singleChoice, accessRights, defaultAccessRight) {
        var dialog = document.createElement("div");
        dialog.setAttribute("class", "us-dialog us-popup-dialog");

        var searchBlock = document.createElement("div");
        searchBlock.setAttribute("class", "us-search");
        var searchInput = document.createElement("input");
        searchInput.setAttribute("class", "textEdit");
        searchInput.setAttribute("placeholder", ResourceJS.UserSelectorSearchPlaceholder);
        searchInput.setAttribute("autocomplete", "new-password");
        var searchClear = document.createElement("span");
        searchClear.setAttribute("class", "clear-search");
        searchBlock.append(searchInput);
        searchBlock.append(searchClear);
        dialog.append(searchBlock);

        var contentList = this.createContentList();
        var contentListParent = document.createElement("div");
        contentListParent.setAttribute("class", "us-list-parent");
        contentListParent.append(contentList);
        dialog.append(contentListParent);

        if (singleChoice || !accessRights) {
            return dialog;
        }

        var accessRightsContainer = this.createAccessRightsContainer(accessRights, defaultAccessRight);
        dialog.append(accessRightsContainer);

        var btnContainer = document.createElement("div");
        btnContainer.setAttribute("class", "small-button-container");
        var saveBtn = document.createElement("a");
        saveBtn.setAttribute("class", "button blue");
        saveBtn.append(document.createTextNode(ResourceJS.SaveButton));
        var btnSplitter = document.createElement("span");
        btnSplitter.setAttribute("class", "splitter-buttons");
        var cancelBtn = document.createElement("a");
        cancelBtn.setAttribute("class", "button gray");
        cancelBtn.append(document.createTextNode(ResourceJS.CancelButton));
        var selectedCount = document.createElement("span");
        selectedCount.setAttribute("class", "gray-text");
        btnContainer.append(saveBtn);
        btnContainer.append(btnSplitter);
        btnContainer.append(cancelBtn);
        btnContainer.append(selectedCount);
        dialog.append(btnContainer);

        return dialog;
    }

    this.clearElement = function(element) {
        while (element.lastChild) {
            element.removeChild(element.lastChild);
        }
    }
}


function DialogHelper(switcherElement, dialogElement, onBeforeShow, onBeforeHide, doNotHideWhenDisplayed, includeDisconnectedElements, trustedElement) {
    var that = this;

    that.selection = false;

    that.addSelection = function () {
        that.selection = true;
    }

    that.removeSelection = function () {
        that.selection = false;
    }

    that.show = function () {

        if (onBeforeShow && !onBeforeShow()) {
            return;
        }

        if (!doNotHideWhenDisplayed && dialogElement.style.display == "block") {
            that.hide();
            return;
        }

        dialogElement.style.display = "block";

        setTimeout(function () {
            dialogElement.addEventListener("mousedown", that.addSelection);
            document.addEventListener("mouseup", that.autoHide);
        }, 0);
    }

    that.hide = function() {
        if (that.selection) {
            that.removeSelection();
            return;
        }

        if (onBeforeHide && !onBeforeHide()) {
            return;
        }

        dialogElement.style.display = "none";

        dialogElement.removeEventListener("mousedown", that.addSelection);
        document.removeEventListener("mouseup", that.autoHide);
    }

    that.autoHide = function(event) {
        //event.stopPropagation();

        //var rect = that.$userSelectorDialog.get(0).getBoundingClientRect();
        //if (event.clientX < rect.left || event.clientX > rect.right || event.clientY < rect.top || event.clientY > rect.bottom ) {
        //    hideDialog()
        //}

        //if (!that.$userSelectorDialog.is(event.target) && !that.$userSelectorDialog.has(event.target).length) {
        //    hideDialog()
        //}

        var element = event.target;

        if (includeDisconnectedElements && !element.isConnected) {
            that.removeSelection();
            return;
        }

        while (element != null) {
            if (element == dialogElement || element == trustedElement) {
                that.removeSelection();
                return;
            }
            element = element.parentNode;
        }

        setTimeout(that.hide,0);
    }

    switcherElement && switcherElement.addEventListener("click", that.show);
}
