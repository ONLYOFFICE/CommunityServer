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


(function ($, win, doc, body) {

    // add new method for jQuery - contains case insensitive
    jQuery.expr[':'].icontains = function (a, i, m) {
        return jQuery(a).text().toUpperCase()
            .indexOf(m[3].toUpperCase()) >= 0;
    };

    Array.prototype.unique = function () {
        var a = this.concat();
        for (var i = 0; i < a.length; ++i) {
            for (var j = i + 1; j < a.length; ++j) {
                if (a[i] === a[j])
                    a.splice(j--, 1);
            }
        }

        return a;
    };


    function setEvents() {
        var that = this,
            $elem = that.$element,
            opts = that.options;

        if (opts.isInitializeItems) {
            if ("initAdvSelectorData" in that) {
                that.initAdvSelectorData.call(that);
                if (that.options.showGroups) {
                    if (that.options.groupsChoose.length) {
                        that.rewriteObjectGroup.call(that, that.options.groupsChoose);
                    } else if ("initAdvSelectorGroupsData" in that) {
                        that.initAdvSelectorGroupsData.call(that)
                    }
                }
            }
        }
        $elem.on('click', $.proxy(showSelectorContainer, that));
        if (opts.canadd) {
            that.$advancedSelector.find(".advanced-selector-add-new-link").on('click', $.proxy(showAddItemBlock, that));
        }
        $(document.body).on('keyup', $.proxy(listItemsNavigation, that));
        that.$advancedSelector.find(".advanced-selector-block-list .advanced-selector-all-select input").on('click', $.proxy(selectedAll, that));
        that.$advancedSelector.find(".advanced-selector-block-list .advanced-selector-all-select").on('click', $.proxy(showAllGroups, that));

        that.$advancedSelector.find(".advanced-selector-search-field").on('keyup', $.proxy(onSearchInputKeyup, that));
        that.$advancedSelector.find(".advanced-selector-search-btn").on('click', $.proxy(onSearchItems, that));
        that.$advancedSelector.find(".advanced-selector-reset-btn").on('click', $.proxy(onSearchReset, that));

        that.$advancedSelector.on("click", opts.onechosen ? ".advanced-selector-list-items li" : ".advanced-selector-btn-action", $.proxy(onClickSaveSelectedItems, that));
        that.$advancedSelector.on("click", ".advanced-selector-list-block .advanced-selector-btn-cancel", $.proxy(onClickCancelSelector, that));
        $(document.body).on('click', $.proxy(onBodyClick, that));

        var res;
        window.onresize = function () {
            if (res){clearTimeout(res)};
            res = setTimeout(function () {
                setPositionSelectorContainer.call(that);
            }, 500);
        };
        that.$advancedSelector.on("click", ".advanced-selector-btn-cnt button", function (e) {
            e.preventDefault();
        });

        $(document).keyup(function (event) {
            if (!that.$advancedSelector.is(":visible"))
                return;

            var code;
            if (!e)
                var e = event;
            if (e.keyCode)
                code = e.keyCode;
            else if (e.which)
                code = e.which;

            if (code == 27) {
                hideSelectorContainer.call(that);
            }
        });

    }

    function onBodyClick(e) {
        var that = this,
            $target = e && typeof e === 'object' ? jQuery(e.target) : null;
        if ($target.closest(that.$advancedSelector).length === 0 && (that.$advancedSelector.is(":visible") || that.$advancedSelector.css("display") != "none")
            && $target.closest(that.$element).siblings(".advanced-selector-container").length === 0) {
            hideSelectorContainer.call(that);
        }
    }
    // TODO navigation up and down
    function listItemsNavigation(event) {
        var that = this,
            list = that.$itemsListSelector.find("li:visible");

        if (!that.$advancedSelector.is(":visible") || that.$advancedSelector.find(".advanced-selector-add-new-block").is(":visible")) { return; }

        listNavigation(event, list);
    }

    function listNavigation(event, list) {
        var event = event || window.event,
            key = event.keyCode || event.charCode,
            numActiveItem = list.index(list.filter(".active"));

        function move(dir) {
            list.parent().find("li.active").removeClass("active");
            switch (dir) {
                case "up":
                    numActiveItem = (numActiveItem <= 0) ? list.length - 1 : --numActiveItem;
                    break;
                case "down":
                    numActiveItem = (numActiveItem == list.length - 1) ? 0 : ++numActiveItem;
                    break;
            }
            $(list[numActiveItem]).addClass("active");
            var offset = $(list[numActiveItem]).position().top;
            list.parent().scrollTop(offset - list.position().top);
        }

        if (list.length != 0 && (key == 40 || key == 38 || key == 13)) {

            event.preventDefault();
            event.stopPropagation()

            switch (key) {
                case 38:
                    move("up");
                    break;
                case 40:
                    move("down");
                    break;
                case 13:
                    $(list[numActiveItem]).trigger("click");
                    break;
            }
        }

    }

    function onSearchInputKeyup(event) {
        var that = this,
            $this = $(event.target),
            $search = $this.siblings(".advanced-selector-search-btn"),
            $reset = $this.siblings(".advanced-selector-reset-btn"),
            itemList = that.$itemsListSelector.find("li:not(.disabled)"),
            noResult = that.$advancedSelector.find(".advanced-selector-no-results"),
            allSelect = that.$advancedSelector.find(".advanced-selector-all-select");

        if (!that.options.showGroups) {
            allSelect = that.$itemsListSelector.siblings(".advanced-selector-all-select");
        } else {
            if (!allSelect.hasClass("chosen")) allSelect.find("label").trigger("click");
        }

        if ($this.val() !== "") {
            $search.trigger("click");
            $search.hide();
            $reset.show();
            if (!that.options.showGroups) allSelect.hide();
        } else {
            noResult.hide();
            itemList.show();
            $search.show();
            $reset.hide();
            if (!that.options.showGroups) allSelect.show();
        }
    }

    function onSearchItems() {
        var that = this,
            $searchFld = that.$advancedSelector.find(".advanced-selector-search-field"),
            searchQuery = ($searchFld.length !== 0) ? $.trim($searchFld.val()) : "",
            itemList = that.$itemsListSelector.find("li:not(.disabled)"),
            foundItemsList = itemList.filter(':icontains("' + searchQuery + '")'),
            $noResult = that.$advancedSelector.find(".advanced-selector-no-results");

        itemList.hide();
        if (foundItemsList.length !== 0) {
            $noResult.hide();
            foundItemsList.show();
        } else {
            $noResult.show();
        }
    }

    function onSearchReset() {
        var that = this,
            $resetBtn = that.$advancedSelector.find(".advanced-selector-search .advanced-selector-reset-btn"),
            $searchBtn = $resetBtn.siblings(".advanced-selector-search-btn"),
            $itemList = that.$itemsListSelector.find("li:not(.disabled)"),
            $noResult = that.$advancedSelector.find(".advanced-selector-no-results"),
            allSelect = that.$advancedSelector.find(".advanced-selector-all-select"),
            noItems = that.$advancedSelector.find(".advanced-selector-no-items");

        if (that.options.onechosen && that.options.showGroups) {
            that.$groupsListSelector.find("ul li").removeClass("chosen");
            that.$groupsListSelector.siblings(".advanced-selector-all-select").addClass("chosen");
        }
        if (that.options.showGroups ) {
            if (!allSelect.hasClass("chosen"))  allSelect.find("label").trigger("click");
        }
        else {
            allSelect.show();
        }

        $resetBtn.siblings("input").val("");
        $noResult.hide();
        noItems.hide();
        $itemList.show();
        $resetBtn.hide();
        $searchBtn.show();
    }

    function showSelectorContainer() {
        var that = this,
            $itemsListCnt = that.$itemsListSelector.find(".advanced-selector-list li");

        if (that.$element.hasClass("disabled")) return;

        that.$element.trigger("additionalClickEvent");

        if (that.$advancedSelector.is(":visible")) {
            hideSelectorContainer.call(that);
        } else {
            setPositionSelectorContainer.call(that);
            that.$advancedSelector.show();
            setFocusOnSearch.call(that);
        }
        if ($itemsListCnt.length === 0) {
            if (that.options.itemsChoose.length) {
                that.rewriteObjectItem.call(that, that.options.itemsChoose);
            } else if (!that.options.isInitializeItems && "initAdvSelectorData" in that) {
                that.initAdvSelectorData.call(that);
            }
            if (that.options.showGroups) {
                if (that.options.groupsChoose.length) {
                    that.rewriteObjectGroup.call(that, that.options.groupsChoose);
                } else if ("initAdvSelectorGroupsData" in that) {
                    that.initAdvSelectorGroupsData.call(that)
                }
            }
        }
    }

    function setPositionSelectorContainer() {

        if (!this.$element.length) {
            return;
        }
        var that = this,
            $elem = that.$element,
            elemPos = that.options.inPopup ? $elem.position() : $elem.offset(),
            elemPosLeft = elemPos.left,
            elemPosTop = elemPos.top + $elem.outerHeight(),
            $w = $(window),
            docWidth = $(document).width(),
            scrHeight = $w.height(),
            topPadding = $w.scrollTop(),
            leftPadding = $w.scrollLeft(),
            $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block")


        if ((elemPosLeft + that.$advancedSelector.outerWidth() - (!$addPanel.hasClass("right-position") ? $addPanel.outerWidth() : 0) ) > (leftPadding + docWidth)) {
            elemPosLeft = Math.max(0, elemPosLeft + that.$element.outerWidth() - that.$advancedSelector.outerWidth());
        }

        if ($addPanel.is(":visible")) {
            elemPosLeft -= that.widthAddBlock;
        }

        if (elemPosTop + that.$advancedSelector.outerHeight() > scrHeight + topPadding) {
            elemPosTop = elemPos.top - that.$advancedSelector.outerHeight();
        }

        that.$advancedSelector.css(
            {
                top: elemPosTop,
                left: elemPosLeft
            });
    }

    function setFocusOnSearch() {
        var that = this;
        that.$advancedSelector.find(".advanced-selector-search-field").focus();
    }

    function onClickCancelSelector() {
        hideSelectorContainer.call(this);
        onResetToDefaultSelector.call(this);
    }

    function hideSelectorContainer() {
        var that = this;
        onSearchReset.call(that);
        that.$advancedSelector.hide();
        hideAddItemBlock.call(that);
        that.$itemsListSelector.find("ul li").removeClass("active");
        that.$itemsListSelector.find("ul").scrollTop(0);
    }


    function onResetToDefaultSelector() {
        var that = this;

        that.$advancedSelector.find(".advanced-selector-list-items li").removeClass("selected selected-before");
        that.$advancedSelector.find(".advanced-selector-list li input, .advanced-selector-all-select input").prop("checked", false).prop("indeterminate", false);

        if (that.options.showGroups) {
            that.$advancedSelector.find(".advanced-selector-list-groups li").removeClass("selected");
        }
        
        onSearchReset.call(that);
        onCheckItemsById.call(that, that.options.itemsSelectedIds);
    }

    function disableDefaultItemsIds(disabledItemsIds) {
        var that = this;
        for (var i = 0, len = disabledItemsIds.length; i < len; i++) {
            var disabledItem = that.$itemsListSelector.find("ul li[data-id=" + disabledItemsIds[i] + "]");
            if ($(disabledItem).length) {
                disabledItem.addClass("disabled");
            }
        }

    }

    function checkAlreadySelectedItemsIds(selectedItemsIds) {
        var that = this;
        for (var i = 0, len = selectedItemsIds.length; i < len; i++) {
            var checkedItem = that.$itemsListSelector.find("ul li[data-id=" + selectedItemsIds[i] + "]").filter(":not(.disabled)");
            if ($(checkedItem).length) {
                checkedItem.addClass("selected-before");
            }
        }
        onCheckItemsById.call(that, selectedItemsIds);
    }

    function pushItemsForGroup() {
        var that = this,
            itemGroups = [];
        for (var i = 0, n = that.items.length; i < n; i++) {
            itemGroups = that.items[i].groups || [];
            for (var k = 0, l = itemGroups.length; k < l; k++) {
                for (var j = 0, m = that.groups.length; j < m; j++) {
                    if (itemGroups[k].id == that.groups[j].id) {
                        that.groups[j].items.push(that.items[i].id);
                    }
                }
            }
        }
    }

    function getItemsForGroup(groupId) {
        var that = this,
            itemIDs = [];
        for (var i = 0, n = that.groups.length; i < n; i++) {
            if (groupId == that.groups[i].id) {
                itemIDs = that.groups[i].items;
                break;
            }
        }
        return itemIDs;
    }

    function getGroupsForItem(itemId) {
        var that = this,
            groupList = [],
            groupIDs = [];
        for (var i = 0, n = that.items.length; i < n; i++) {
            if (itemId == that.items[i].id) {
                groupList = (that.items[i].hasOwnProperty("groups")) ? that.items[i].groups : [];
                for (var j = 0, m = groupList.length; j < m; j++) {
                    groupIDs.push(groupList[j].id);
                }
            }
        }
        return groupIDs;
    }

    function getItemById(id, itemList) {
        for (var i = 0, len = itemList.length; i < len; i++) {
            if (id == itemList[i].id) {
                return itemList[i];
                break;
            }
        }
    }

    function onMatchGroupInAddItemBlock(tagGroup) {
        var that= this;
        if (!tagGroup) { return; }
        var chosenGroup = that.$groupsListSelector.find("li.chosen"),
            isChosenAll = that.$advancedSelector.find(".advanced-selector-all-select").hasClass("chosen");
        if (chosenGroup && chosenGroup.length) {
            that.$advancedSelector.find(".advanced-selector-add-new-block .advanced-selector-field-wrapper." + tagGroup + " .advanced-selector-field-list li[data-id=" + chosenGroup.attr("data-id") + "]").trigger("click");
        } else if (isChosenAll) {
            that.$advancedSelector.find(".advanced-selector-add-new-block .advanced-selector-field-wrapper." + tagGroup + " .advanced-selector-reset-btn").trigger("click");
        }
        
    }

    function showAddItemBlock(event) {
        var that = this,
            itemsSimpleSelect = [],
            $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block"),
            leftPos = that.options.inPopup ? that.$advancedSelector.position().left : that.$advancedSelector.offset().left;

        if (!$addPanel.length) {
            if ("initCreationBlock" in that){
                that.initCreationBlock.call(that, { items: that.items, groups: that.groups });
            }
            setEventsAddItemBlock.call(that);
            setFocusOnAddField.call(that);
        } else if (!$addPanel.is(":visible")) {
            $addPanel.show();
            if (!$addPanel.hasClass("right-position")) {
                that.$advancedSelector.css({ left: leftPos - that.widthAddBlock });
            }
            that.$advancedSelector.css({ width: that.widthAddBlock + (that.options.showGroups ? that.widthSelector * 2 : that.widthSelector + 5) + "px" });
            setFocusOnAddField.call(that);
        } else {
            hideAddItemBlock.call(that);
        }
    }

    function hideAddItemBlock() {
        var that = this;
        that.$advancedSelector.find(".advanced-selector-add-new-block").hide();
        that.$advancedSelector.css({ width: (that.options.showGroups ? that.widthSelector * 2 : that.widthSelector + 5) + "px" });
        setPositionSelectorContainer.call(that);
    }

    function setEventsAddItemBlock() {
        var that = this,
            addItemBlock = that.$advancedSelector.find(".advanced-selector-add-new-block");
        addItemBlock.find(".advanced-selector-btn-cancel").on('click', $.proxy(hideAddItemBlock, that));
        addItemBlock.find(".advanced-selector-btn-add").on('click', $.proxy(createNewItem, that));

        $(document).keyup(function (event) {
            if (!addItemBlock.is(":visible"))
                return;

            var code;
            if (!e)
                var e = event;
            if (e.keyCode)
                code = e.keyCode;
            else if (e.which)
                code = e.which;

            if (code == 13) {
                addItemBlock.find(".advanced-selector-btn-add").trigger("click");
            }
        });
    }

    function setFocusOnAddField() {
        var that = this;
        that.$advancedSelector.find(".advanced-selector-add-new-block input[type=text]").first().focus();
    }

    function createNewItem(event) {
        var that = this,
            $this = $(event.target),
            $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block");
        $addPanel.find(".advanced-selector-field-wrapper").removeClass("error");
        that.hideServerError($this);
        if ("createNewItemFn" in that) {
            that.createNewItemFn.call(that);
        }
    }

    function showNewListItemsAfterCreateItem(groupId) {
        // TODO select by ID and third status for group
        if (groupId) {
            var $chosenGroup = this.$groupsListSelector.find(".advanced-selector-list li[data-id=" + groupId + "]");
            $chosenGroup.find("input").prop("indeterminate", true).prop("checked", false);
            if ($chosenGroup.length && !$chosenGroup.hasClass("chosen")) {
                $chosenGroup.find("label").trigger("click");
            }
        }
    }

    function selectedAll(event) {
        var that = this,
            $this = $(event.target).closest(".advanced-selector-all-select"),
            allCheckBox = $this.find("input[type=checkbox]"),
            $itemList = $this.siblings("[class^=advanced-selector-list]").find("li:not(.disabled) input[type=checkbox]"),
            flag = allCheckBox.is(":checked") ? false : true,
            itemsList;

            if (that.options.onechosen) {
                $this.trigger("click");
                return;
            }
            if (!$(event.target).is("input")) {
                flag = allCheckBox.is(":checked") ? true : false;
            }
            allCheckBox.prop("checked", !flag).prop("indeterminate", false);
            $itemList.prop("checked", !flag).prop("indeterminate", false);

            if (!flag) {
                $itemList.closest("li").addClass("selected").removeClass("chosen");
                $this.addClass("chosen");
            } else {
                $itemList.closest("li").removeClass("selected");
            }
            itemsList = that.$itemsListSelector.find("li:not(.disabled)");
            that.$advancedSelector.find(".advanced-selector-no-items").hide();
            itemsList.show();
            if (that.options.showGroups && that.$groupsListSelector.length) {
                !flag ? itemsList.addClass("selected") : itemsList.removeClass("selected");
                itemsList.find("input[type=checkbox]").prop('checked', !flag);
            }
            onSearchReset.call(that);
            countSelectedItems(itemsList);
    }

    function showAllGroups(event) {
        var that = this,
            $this = $(event.target).closest(".advanced-selector-all-select"),
            itemList = that.$itemsListSelector.find("li:not(.disabled)");

        if (!that.options.showGroups) {
            selectedAll.call(that, event);
            return;
        }

        if (!$this.hasClass("chosen")) {
            that.$advancedSelector.find(".advanced-selector-no-items").hide();
            itemList.show();
            if (that.$groupsListSelector) {
                that.$groupsListSelector.find("li").removeClass("chosen");
            }
            $this.addClass("chosen");
            onMatchGroupInAddItemBlock.call(that, that.nameSimpleSelectorGroup);
        }
    }

    function countSelectedItems($itemList) {
        $itemList = $itemList.filter(":not(.disabled)");
        var selectedCount = $itemList.find("input[type=checkbox]:checked").length,
            $countBox = $itemList.parents(".advanced-selector-block").find(".advanced-selector-selected-count");
        if (selectedCount > 0) {
            $countBox.text(selectedCount + " " + ASC.Resources.Master.Resource.SelectorSelectedItems).show();
        } else {
            $countBox.text("").hide();
        }
    }

    function arrayContainsAnotherArray(needle, haystack) {
        for (var i = 0; i < needle.length; i++) {
            if (haystack.indexOf(needle[i]) === -1)
                return false;
        }
        return true;
    }

    function onCheckItem(event) {
        var that = this,
            $this = $(event.target).closest("li"),
            $checkBox = $this.find("input[type=checkbox]"),
            flag = $checkBox.is(":checked") ? true : false;

        if ($this.hasClass("disabled")) { return; }

        if (that.options.onechosen) {
            that.$itemsListSelector.find("li.selected").removeClass("selected");
            flag ? $this.removeClass("selected") : $this.addClass("selected");
        } else {
            if ($(event.target).is($this.find("input"))) {
                flag = $checkBox.is(":checked") ? false : true;
            }
            $checkBox.prop("checked", !flag);
            flag ? $this.removeClass("selected") : $this.addClass("selected");

            onCheckSelectedAll.call(that, that.$itemsListSelector);
            countSelectedItems(that.$itemsListSelector);
        }
        if (that.$groupsListSelector && that.$groupsListSelector.length) {
            onSelectGroupsByItem.call(that, [$this.attr("data-id")], !flag);
        }
    }

    function onCheckItemsById(itemsIds) {
        var that = this,
            $itemsList = that.$itemsListSelector.find("li:not(.disabled)"),
            checkedItems = [],
            checkedGroups = [],
            checkedItemsIds = [];

        for (var i = 0, ln = itemsIds.length; i < ln; i++) {
            var checkedItem = $itemsList.filter("[data-id =" + itemsIds[i] + "]");
            if ($(checkedItem).length && !checkedItem.find("input").is(":checked")) {
                checkedItem.addClass("selected");
                checkedItem.find("input").prop("checked", true);
            }
        }

        if (that.$groupsListSelector && that.$groupsListSelector.length) {
            checkedItems = $.grep(that.items, function (el) {
                return ($.inArray(el.id, itemsIds) != -1);
            });

            checkedItems.forEach(function (e) {
                checkedGroups = checkedGroups.concat(e.groups).unique();
            });

            checkedItems.forEach(function (checkedItem) {
                checkedItemsIds.push(checkedItem.id);
            });

            for (var j = 0, l = checkedGroups.length; j < l; j++) {
                var checkedGroup = that.$groupsListSelector.find("li[data-id=" + checkedGroups[j].id + "]"),
                    itemsForCheckedGroup = getItemsForGroup.call(that, checkedGroups[j].id);

                itemsForCheckedGroup = $.grep(itemsForCheckedGroup, function (elem) {
                    return $.inArray(elem, that.options.itemsDisabledIds) == -1;
                })
                if ($(checkedGroup).length) {
                    var isCheck = arrayContainsAnotherArray(itemsForCheckedGroup, checkedItemsIds) ? true : false;
                    checkedGroup.find("input").prop("indeterminate", !isCheck).prop("checked", isCheck);
                }
            }
        }

        onCheckSelectedAll.call(that, that.$itemsListSelector);
        countSelectedItems(that.$itemsListSelector);
    }

    function onCheckSelectedAll(itemList) {
        var selectedCount = itemList.find("li:not(.disabled) input[type=checkbox]:checked").length,
                allSelect = this.$advancedSelector.find(".advanced-selector-all-select"),
                isAllCheck,
                isAllDeterm;

            if (selectedCount == 0) {
                isAllCheck = false;
                isAllDeterm = false;
            } else {
                isAllDeterm = true;
                if (selectedCount == itemList.find("li:not(.disabled)").length) {
                    isAllCheck = true;
                    isAllDeterm = false;
                }
            }

            allSelect.find("input").prop("checked", isAllCheck).prop("indeterminate", isAllDeterm);
    }

    // select groups in which there are the selected item

    function onSelectGroupsByItem(itemIds, check) {
        var that = this,
            itemSelectedGroups = [],
            itemsSelected = that.$itemsListSelector.find("li.selected"),
            itemsSelectedIds = [],
            groupSelectedItems,
            groupCurrent;

        for (var k = 0, ln = itemIds.length; k < ln; k++) {
            itemSelectedGroups = itemSelectedGroups.concat(getGroupsForItem.call(that, itemIds[k])).unique();
        }

        for (var j = 0, n = itemsSelected.length; j < n; j++) {
            itemsSelectedIds.push($(itemsSelected[j]).attr("data-id"));
        }
        for (var i = 0, length = itemSelectedGroups.length; i < length; i++) {
            groupCurrent = that.$groupsListSelector.find("li[data-id=" + itemSelectedGroups[i] + "]").find("input[type=checkbox]");
            groupSelectedItems = getItemsForGroup.call(that, itemSelectedGroups[i]);
            groupSelectedItems = $.grep(groupSelectedItems, function (el) {
                return $.inArray(el, that.options.itemsDisabledIds) == -1;
            });

            if (check) {
                if (arrayContainsAnotherArray(groupSelectedItems, itemsSelectedIds)) {
                    groupCurrent.prop("indeterminate", false).prop("checked", true);
                    groupCurrent.closest("li").not(".selected").addClass("selected");
                } else {
                    groupCurrent.prop("indeterminate", true);
                }
            } else {
                if (!arrayContainsAnotherArray(groupSelectedItems, itemsSelectedIds)) {
                    var isContainInGroup = itemsSelectedIds.some(
                        function(elem){
                            return $.inArray(elem, groupSelectedItems) !== -1;
                    });
                    groupCurrent.prop("checked", false).prop("indeterminate", false);
                    groupCurrent.closest("li").removeClass("selected");
                    if (isContainInGroup) {
                        groupCurrent.prop("indeterminate", true);
                    }
                }
            }
        }
    }

    //select all items from group

    function onCheckGroup(event) {
        var that = this,
            $this = $(event.target),
            $curGroup = $this.closest("li"),
            flag = $this.is(":checked") ? false : true,
            groupSelectedItems = [],
            groupsSelected,
            itemGroup,
            itemSelectedGroups = [],
            isSelect;

        if (that.options.onechosen) {
            $curGroup.trigger("click");
            return;
        }
        that.$advancedSelector.find(".advanced-selector-search-field").val("");

        if (!flag) {
            that.$advancedSelector.find(".advanced-selector-all-select").removeClass("chosen");
            $curGroup.addClass("selected");
        } else {
            $curGroup.removeClass("selected");
        }
        
        $this.prop("indeterminate", false).prop("checked", !flag);
        groupsSelected = that.$groupsListSelector.find("li.selected");
        that.$itemsListSelector.find("li").hide();

        groupSelectedItems = getItemsForGroup.call(that, $curGroup.attr("data-id"));
        for (var i = 0, ln = groupSelectedItems.length; i < ln; i++){
            var el = groupSelectedItems[i],
                itEl = that.$itemsListSelector.find("li[data-id=" + el + "]").filter(":not(.disabled)");
            flag ? itEl.removeClass("selected") : itEl.addClass("selected");
            itEl.find("input").prop("checked", !flag);
            itEl.show();
        };
        onSelectGroupsByItem.call(that, groupSelectedItems, !flag, that.$advancedSelector);
        onCheckSelectedAll.call(that, that.$itemsListSelector);
        countSelectedItems(that.$itemsListSelector);
    }


    //choose group and display items from this group

    function onChooseGroup(event) {
        var that = this,
            $groupChosen = $(event.target).closest("li"),
            $this = $groupChosen.find("label"),
            groupId = $groupChosen.attr("data-id"),
            groupsList = that.$groupsListSelector.find("li"),
            itemsList = that.$itemsListSelector.find("li:not(.disabled)"),
            allGroups = that.$groupsListSelector.siblings(".advanced-selector-all-select"),
            noItems = that.$advancedSelector.find(".advanced-selector-no-items"),
            noResults = that.$advancedSelector.find(".advanced-selector-no-results"),
            itemsGroupList,
            $item;

        allGroups.removeClass("chosen");
        groupsList.removeClass("chosen");
        $groupChosen.addClass("chosen");
        itemsGroupList = findItemsByGroup(that.items, groupId);
        itemsList.hide();
        noItems.hide();
        noResults.hide();

        if (itemsGroupList.length == 0) {
            noItems.show();
        }

        for (var i = 0, length = itemsList.length; i < length; i++) {
            $item = $(itemsList[i]);
            for (var j = 0, m = itemsGroupList.length; j < m; j++) {
                if ($item.attr("data-id") == itemsGroupList[j]) {
                    $item.show();
                }
            }
        }

        if (!itemsList.filter(":visible").length) {
            noItems.show();
        }

        onMatchGroupInAddItemBlock.call(that, that.nameSimpleSelectorGroup);
    }

    function findItemsByGroup(Items, groupId) {
        var itemsGroup = [],
            itemGroups,
            itemId;
        for (var i = 0, length = Items.length; i < length; i++) {
            itemGroups = Items[i].groups;
            itemId = Items[i].id;
            if (itemGroups && itemGroups.length) {
                for (var j = 0, m = itemGroups.length; j < m; j++) {
                    if (!(itemId in itemsGroup) && itemGroups[j].id == groupId) {
                        itemsGroup.push(itemId);
                    }
                }
            }
        }
        return itemsGroup; // list of Item IDs
    }

    function onClickSaveSelectedItems(event) {
        var that = this,
            $this = $(event.target),
            selectedItemsList = that.$itemsListSelector.find("li.selected"),
            selectedItems = [],
            result;
        for (var i = 0, len = selectedItemsList.length; i < len; i++) {
            selectedItems.push(getItemById($(selectedItemsList[i]).attr("data-id"), that.items));
        }
        if (that.options.onechosen && $this.hasClass("selected-before")) {
            $this.removeClass("selected-before");
            return;
        }
        hideSelectorContainer.call(that);
        result = that.options.onechosen ? selectedItems : [selectedItems];
        that.$element.trigger("showList", result);
    }

    function initGroupEvents() {
        var that = this;
        that.$groupsListSelector
            .on('click', 'ul li input', $.proxy(onCheckGroup, that))
            .on('click', 'ul li', $.proxy(onChooseGroup, that));
    }

    // Methods for group selector 

    var initSimpleSelector = function ($field) {
        var that = this,
            $selector = $field.find(".advanced-selector-field-search:first"),
            $listValue = $selector.find(".advanced-selector-field-list"),
            $reset = $field.find(".advanced-selector-reset-btn"),
            $search = $field.find(".advanced-selector-search-btn");

        $selector.find("input").on("keyup", onSearchSimpleSelector).on("focus", function () {
            if ($(this).hasClass("disabled")) return;

            if ($listValue.find("li.chosen").length || $selector.find("input").val() == "") {
                $listValue.show();
            }
        });
        $selector.find("li").on("click", $.proxy(onSelectSimpleSelector, that));
        that.$advancedSelector.on('click', onClickContainerSimpleSelector);
        $reset.on("click", onResetSimpleSelector);
        $(document.body).on('keyup', $.proxy(listSimpleItemsNavigation, that));

        function onClickContainerSimpleSelector(evt) {
            var $target = evt && typeof evt === 'object' ? jQuery(evt.target) : null;
            if ($target.closest(".advanced-selector-field-search").length === 0) {
                $listValue.hide();
                $listValue.find("li.active").removeClass("active");
                $listValue.scrollTop(0);
            }
        }

        function listSimpleItemsNavigation(event) {
            var list = $selector.find("li:visible");
            if ($(list).parent().is(":visible")) {
                listNavigation(event, list);
            }
        }

        function onSearchSimpleSelector(event) {
            var event = event || window.event,
                key = event.keyCode || event.charCode;

            if (key == 13) return;
            
            var searchQuery = $selector.find("input").val(),
                foundList = $listValue.find('li:icontains("' + searchQuery + '")'),
                $wrapSelector = $selector.parents(".advanced-selector-field-wrapper");
            $listValue.hide();
            $listValue.find("li.selected").removeClass("selected");
            $selector.find("input").attr("data-id", "");
            $listValue.find("li").removeClass("chosen").hide();
            if (foundList.length) {
                $listValue.show();
                foundList.addClass("chosen").show();
                $wrapSelector.removeClass("error");
            } else {
                that.showErrorField.call(that, { field: $wrapSelector, error: "" });
            }
            if (searchQuery.length) {
                $reset.show();
                $search.hide();
            }
            else {
                $reset.hide();
                $search.show();
            }
        }

        function onSelectSimpleSelector(event) {
            var $this = $(event.target),
                $field = $this.parent().siblings(".advanced-selector-field");
            $listValue.find("li.selected").removeClass("selected");
            $this.addClass("selected");
            $field.val($this.text());
            $field.attr("data-id", $this.attr("data-id"));
            $listValue.hide();
            $reset.show();
            $search.hide();
            if ("onChooseItemSimpleSelector" in that) {
                that.onChooseItemSimpleSelector.call(this, { id: $this.attr("data-id") });
            }
        }

        function onResetSimpleSelector() {
            var $selector = $field.find(".advanced-selector-field-search:first"),
            $listValue = $selector.find(".advanced-selector-field-list");
            $selector.parents(".advanced-selector-field-wrapper").removeClass("error");
            $selector.find("input").val("").removeAttr("data-id");
            $listValue.find("li").removeClass("selected").show();
            $listValue.hide();
            $reset.hide();
            $search.show();
        }
    };

    var advancedSelector = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.advancedSelector.defaults, options);

        this.init();
    };

    advancedSelector.prototype = {
        constructor: advancedSelector,

        init: function () {
            var that = this;

            that.heightListWithoutCreate = 225;
            that.heightListChooseOne = 200;
            that.widthSelector = 211;
            that.widthAddBlock = 216,
            that.items = [];
            that.groups = [];
            that.nameSimpleSelectorGroup = "";


            var $o = $.tmpl(that.options.templates.selectorContainer, { opts: that.options });
            if (that.options) {
                that.$element.after($o);
            }
            that.$advancedSelector = $o;

            that.$itemsListSelector = that.$advancedSelector.find(".advanced-selector-list-items");

            if (that.options.showGroups) {
                that.$groupsListSelector = that.$advancedSelector.find(".advanced-selector-list-groups");
            }

            that.$advancedSelector.css({ width: (that.options.showGroups ? that.widthSelector * 2 : that.widthSelector + 5) + "px" });
            setEvents.call(that);
        },

        displayAddItemBlock: function (data) {
            var that = this,
            $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block"),
            leftPadding = $(window).scrollLeft(),
            leftPos = that.options.inPopup ? that.$advancedSelector.position().left : that.$advancedSelector.offset().left;

            var $o = $.tmpl(that.options.templates.addNewPanel, { fields: data.newoptions, btnTitle: data.newbtn });

            if ((leftPadding + that.$advancedSelector.offset().left) < that.widthAddBlock) {
                that.$advancedSelector.append($o);
                that.$advancedSelector.find(".advanced-selector-add-new-block").addClass("right-position");
            } else {
                that.$advancedSelector.prepend($o);
                that.$advancedSelector.css({ left: leftPos - that.widthAddBlock });
            }           
            that.$advancedSelector.css({ width: that.widthAddBlock + (that.options.showGroups ? that.widthSelector * 2 : that.widthSelector + 5) + "px" });

        },
        displayLoadingBtn: function (data) {
            var $btnContainer = data.btn.parents(".advanced-selector-btn-cnt"),
                loader = "<div class=\"advanced-selector-loader\">" + data.text + "</div>";
            $btnContainer.find("button").addClass("disable");
            $btnContainer.append(loader);
            this.$advancedSelector.find(".advanced-selector-add-new-block input").attr("disabled", "disabled");
        },

        hideLoadingBtn: function (btn) {
            var $btnContainer = $(btn).parents(".advanced-selector-btn-cnt");
            $btnContainer.find("button").removeClass("disable");
            $btnContainer.find(".advanced-selector-loader").remove();
            this.$advancedSelector.find(".advanced-selector-add-new-block input").removeAttr("disabled");
        },
        showErrorField: function (data) {
            $(data.field).addClass("error");
            $(data.field).find(".advanced-selector-field-error").text(data.error);
        },

        showServerError: function (data) {
            var error = "<div class=\"advanced-selector-server-error\" title=\"{0}\">{0}</div>".format(Encoder.htmlEncode(data.error));
            $(data.field).parents(".advanced-selector-btn-cnt").append(error);
        },

        hideServerError: function ($btn) {
            $btn.parents(".advanced-selector-btn-cnt").find(".advanced-selector-server-error").remove();
        },
        actionsAfterCreateItem: function (data) {
            var that = this,
                newObj = data.newitem,
                profile = data.response,
                tagName = data.nameProperty,
                ID;

            if (profile.hasOwnProperty(tagName)) {

                if ($.isArray(profile[tagName])) {
                    ID = profile[tagName][0].id;
                }
                else {
                    ID = profile[tagName];
                }
                for (var i = 0, length = that.groups.length; i < length; i++) {
                    if (ID == that.groups[i].id) {
                        that.groups[i].items.push(profile.id);
                    }
                }
                newObj.groups = profile.groups;

            }
            that.items.push(newObj);
            var $o = $.tmpl(that.options.templates.itemList, { Items: [newObj], isJustList: that.options.onechosen });
            if ((that.options.showme || that.options.withadmin) && that.$itemsListSelector.find("li").not(".disabled").length) {
                that.$itemsListSelector.find(".advanced-selector-list li:not(.disabled)").first().after($o);
            } else {
                that.$itemsListSelector.find(".advanced-selector-list").prepend($o);
            }
            that.select.call(that, [newObj.id]);
            that.$advancedSelector.find(".advanced-selector-add-new-block input").val("");
            that.$advancedSelector.find(".advanced-selector-no-results, .advanced-selector-no-items").hide();
            if (that.options.showGroups) {
                showNewListItemsAfterCreateItem.call(that, ID);
            }
            that.$element.trigger("afterCreate", newObj);
            hideAddItemBlock.call(that);
        },

        initDataSimpleSelector: function (data) {
            var that = this,
                objType = data.tag,
                list = $.tmpl(that.options.templates.addNewItems, { Items: data.items }),
                $field = this.$advancedSelector.find(".advanced-selector-field-wrapper." + objType);
            that.nameSimpleSelectorGroup = objType;
            $field.find(".advanced-selector-field-list").html(list);
            initSimpleSelector.call(that, $field);
            if (that.options.showGroups) { onMatchGroupInAddItemBlock.call(that, objType); }
        },
        showLoaderSimpleSelector: function (tag) {
            var $field = this.$advancedSelector.find(".advanced-selector-field-wrapper." + tag + " .advanced-selector-field-search");
            $field.append("<div class=\"advanced-selector-field-loader\"></div>");
        },
        hideLoaderSimpleSelector: function (tag) {
            var $field = this.$advancedSelector.find(".advanced-selector-field-wrapper." + tag + " .advanced-selector-field-search");
            $field.find(".advanced-selector-field-loader").remove();
        },
        rewriteObjectItem: function (data) {
            var that = this;
            that.items = [];

            for (var i = 0, length = data.length; i < length; i++) {
                var newObj = {};
                newObj.title = data[i].displayName || data[i].title || data[i].name || data[i].Name;
                newObj.id = (data[i].id && data[i].id.toString()) || data[i].Id.toString();


                if (data[i].hasOwnProperty("isPending")) {
                    newObj.status = data[i].isPending ? "pending" : "";
                }
                if (data[i].hasOwnProperty("groups") || data[i].hasOwnProperty("projectId")) {
                    newObj.groups = data[i].groups || [{ id: data[i].projectId.toString() }];
                    if (data[i].groups && data[i].groups.length && !data[i].groups[0].id) {
                        newObj.groups.map(function (el) {
                            el.id = el.ID;
                        })
                    }
                }
                if (data[i].hasOwnProperty("contactclass")) {
                    newObj.type = data[i].contactclass;
                }

                if (data[i].hasOwnProperty("isPrivate")) {
                    newObj.data = data[i].isPrivate;
                }

                that.items.push(newObj);
            }

            that.items = that.items.sort(SortData);
            that.$element.data('items', that.items);
            that.showItemsListAdvSelector.call(that);
        },

        rewriteObjectGroup: function (data) {
            var that = this;
            that.groups = [];
            for (var i = 0, length = data.length; i < length; i++) {
                var newObj = {};
                newObj.title = data[i].name || data[i].title;
                newObj.id = data[i].id;
                newObj.items = data[i].items || [];
                that.groups.push(newObj);
            }
            that.groups = that.groups.sort(SortData);
            if (that.options.showGroups) {
                that.showGroupsListAdvSelector.call(that);
            }
        },

        showItemsListAdvSelector: function () {
            var that = this,
                itemsDisplay = that.items;

            if (that.options.showme) {
                var user = {};

                for (var i = 0, length = itemsDisplay.length; i < length; i++) {
                    if (itemsDisplay[i].id == Teamlab.profile.id) {
                        user = itemsDisplay[i];
                        user.title = ASC.Resources.Master.Resource.MeLabel;
                        itemsDisplay.splice(i, 1);
                        itemsDisplay.unshift(user);
                    }
                }
            }

            var $items = $.tmpl(that.options.templates.itemList, { Items: itemsDisplay, isJustList: that.options.onechosen });

            if (!that.options.canadd && that.options.showGroups) {
                that.$itemsListSelector.find(".advanced-selector-list").height(177); //height for the items container without the creation of the new item
            }
            if (that.options.onechosen) {
                var height = that.options.canadd ? that.heightListChooseOne : that.heightListWithoutCreate;
                that.$itemsListSelector.height(height);
                that.$itemsListSelector.find(".advanced-selector-list").height(height);
            }

            if (!that.options.onechosen && that.options.canadd && !that.options.showGroups) {
                var height = 131;
                that.$itemsListSelector.height(height);
                that.$itemsListSelector.find(".advanced-selector-list").height(height);
            }

            that.$itemsListSelector.find(".advanced-selector-list").html($items);
            that.$itemsListSelector.find("ul").on('click', 'li', $.proxy(onCheckItem, that));
            disableDefaultItemsIds.call(that, that.options.itemsDisabledIds);

            if (!that.$itemsListSelector.find("li:not(.disabled)").length && !that.options.canadd) {
                that.$advancedSelector.find(".advanced-selector-empty-list").show();
                that.$advancedSelector.find(".advanced-selector-all-select").hide();
            } else {
                that.$advancedSelector.find(".advanced-selector-empty-list").hide();
                that.$advancedSelector.find(".advanced-selector-all-select").show();
            }

            if (that.options.showGroups) {
                initGroupEvents.call(that);
            } else {
                checkAlreadySelectedItemsIds.call(that, that.options.itemsSelectedIds);
            }
            setFocusOnSearch.call(that);
        },

        showGroupsListAdvSelector: function () {
            var that = this,
                $groups = $.tmpl(that.options.templates.groupList, { Items: that.groups, isJustList: that.options.onechosen });
            if (that.groups.length) {
                that.$groupsListSelector.find(".advanced-selector-list").html($groups);
            } else {
                that.$groupsListSelector.find(".advanced-selector-no-groups").show();
            }

            if (that.options.onechosen) {
                var height = that.options.canadd ? that.heightListChooseOne : that.heightListWithoutCreate - 28; // 28 - height of the field "Select All"
                that.$groupsListSelector.find(".advanced-selector-list").height(height);
            }
            pushItemsForGroup.call(that);
            checkAlreadySelectedItemsIds.call(that, that.options.itemsSelectedIds);
        },
        showLoaderListAdvSelector: function (listname) {
            var that= this,
                $listblock;
            switch (listname) {
                case "items":
                    $listblock = that.$itemsListSelector;
                    break;
                case "groups":
                    if (!that.$groupsListSelector) return;
                    $listblock = that.$groupsListSelector;
                    break;
            }
            that.$advancedSelector.find(".advanced-selector-add-new, .advanced-selector-all-select").hide();
            $listblock.append("<div class=\"advanced-selector-loader-list\"></div>");
        },

        hideLoaderListAdvSelector: function (listname) {
            this.$advancedSelector.find(".advanced-selector-add-new, .advanced-selector-all-select").show();
            this.$advancedSelector.find(".advanced-selector-loader-list").hide();
        },

        reset: function (onlySelected) {
            var that = this,
                flag = onlySelected || false,
                list = that.$itemsListSelector.find("ul li");

            list.find(".selected").removeClass("selected selected-before");
            if (!flag) {
                list.removeClass("disabled");
            }
            that.$advancedSelector.find(".advanced-selector-list li input, .advanced-selector-all-select input").prop("checked", false).prop("indeterminate", false);

            if (that.options.showGroups) {
                that.$advancedSelector.find(".advanced-selector-list-groups li").removeClass("selected");
            }

            onSearchReset.call(that);
            countSelectedItems(that.$itemsListSelector);
        },
        select: function (selectedItemsIds) {
            var that = this;
            that.options.itemsSelectedIds = that.options.itemsSelectedIds.concat(selectedItemsIds).unique();

            for (var j = 0, len = selectedItemsIds.length; j < len; j++) {
                var selectItem = that.$itemsListSelector.find("ul li[data-id=" + selectedItemsIds[j] + "]");
                if ($(selectItem).length && !selectItem.find("input").is(":checked")) {
                    selectItem.trigger("click");
                }
            }
        },
        unselect: function (itemsIds) {
            var that = this;
            that.options.itemsSelectedIds = $.grep(that.options.itemsSelectedIds, function (n, i) {
                return $.inArray(n, itemsIds) == -1;
            });

            for (var j = 0, len = itemsIds.length; j < len; j++) {
                var unselectItem = that.$itemsListSelector.find("ul li[data-id=" + itemsIds[j] + "]");
                if ($(unselectItem).length && unselectItem.find("input").is(":checked")) {
                    unselectItem.trigger("click");
                }
            }
        },

        disable: function (itemsIds) {
            var that = this;
            that.unselect.call(that, itemsIds);
            that.options.itemsDisabledIds = that.options.itemsDisabledIds.concat(itemsIds).unique();

            for (var j = 0, len = itemsIds.length; j < len; j++) {
                var disabledItem = that.$itemsListSelector.find("ul li[data-id=" + itemsIds[j] + "]");
                if ($(disabledItem).length) {
                    that.$itemsListSelector.find("ul li[data-id=" + itemsIds[j] + "]").addClass("disabled").hide();
                }
            }
        },

        undisable: function (itemsIds) {
            var that = this;

            that.options.itemsDisabledIds = $.grep(that.options.itemsDisabledIds, function (n, i) {
                return $.inArray(n, itemsIds) == -1;
            });

            for (var j = 0, len = itemsIds.length; j < len; j++) {
                var undisabledItem = that.$itemsListSelector.find("ul li[data-id=" + itemsIds[j] + "]");
                if ($(undisabledItem).length) {
                    that.$itemsListSelector.find("ul li[data-id=" + itemsIds[j] + "]").removeClass("disabled").show();
                }
            }
        },

        add: function (item) {
            var that = this,
                isExist = $.grep(that.items, function (el) { return el.id == item.id }).length;
            if (isExist) {
                var elem = that.$itemsListSelector.find("ul li[data-id=" + item.id + "]");
                if (elem.hasClass("disabled")) {
               /*     that.options.itemsDisabledIds.map(function (e) {
                        return (that.options.itemsDisabledIds !== item.id);
                    })*/
                    elem.removeClass("disabled");
                }
            }
            else if("addNewItemObj" in that  && that.$itemsListSelector.find("ul li").length ) {
                that.addNewItemObj.call(that, item);
            }
        },
        rewriteItemList: function (data, dataIdsSelected) {
            var that = this;
            data = data || [];
            dataIdsSelected = dataIdsSelected || [];
            if (that.$itemsListSelector.find("ul li").length) {
                that.rewriteObjectItem.call(that, data);
                that.select.call(that, dataIdsSelected);
            } else {
                that.options.itemsChoose = data;
                that.options.itemsSelectedIds = dataIdsSelected;
            }
        } 
    }


    $.fn.advancedSelector = function (option, val) {
        var selfargs = Array.prototype.slice.call(arguments, 1);
        return this.each(function () {
            var $this = $(this),
                data = $this.data('advancedSelector'),
                options = $.extend({},
                        $.fn.advancedSelector.defaults,
                        $this.data(),
                        typeof option == 'object' && option);
            if (!data) {
                $this.data('advancedSelector', (data = new advancedSelector(this, options)));
            }
            if (typeof option == 'string') data[option].apply(data, selfargs);
        });
    };
    $.fn.advancedSelector.defaults = {
        itemsChoose: [],
        groupsChoose: [],
        itemsSelectedIds: [],
        itemsDisabledIds: [],
        showGroups: false,
        onechosen: false,
        canadd: false,
        showme: false,
        showSearch: true,
        inPopup: false,
        isInitializeItems: false,

        templates : {
            selectorContainer: "template-selector-container",
            itemList: "template-selector-items-list",
            groupList: "template-selector-groups-list",
            addNewPanel: "template-selector-add-new-item",
            selectedItemsList: "template-selector-selected-items",
            addNewItems: "template-selector-add-new-list-items",
            addNewItemsSelect: "template-selector-add-new-select-items"
        }
    };
    $.fn.advancedSelector.Constructor = advancedSelector;

})(jQuery, window, document, document.body);