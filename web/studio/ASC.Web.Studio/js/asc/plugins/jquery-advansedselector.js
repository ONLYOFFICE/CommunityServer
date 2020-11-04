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

    Array.prototype.chunkArray = function (chunkSize) {
        var res = [];

        for (var index = 0; index < this.length; index += chunkSize) {
            var chunk = this.slice(index, index + chunkSize);
            res.push(chunk);
        }

        return res;
    };

    function mapItems(item) {
        var id = item.getAttribute("data-id");
        return {
            item: jq(item),
            id: id
        };
    }

    function getList(filter, $selector) {
        $selector = $selector || this.$itemsListSelector;

        return Array.from($selector.find("li" + (filter || ""))).map(mapItems);
    }

    function actionWithListItem(itemsIds, action, filter) {
        var lis = Array.from(this.$itemsListSelector.find("li" + (filter || "")));
        var newList = {};

        for (var j = 0; j < lis.length; j++) {
            var newObj = mapItems(lis[j]);
            newList[newObj.id] = newObj;
        }

        for (var j = 0, len = itemsIds.length; j < len; j++) {
            var $item = newList[itemsIds[j]];
            if ($item && $item.item.length) {
                action($item.item);
            }
        }
    }

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
                        that.initAdvSelectorGroupsData.call(that);
                    }
                }
            }
        }
        $elem.on('click', showSelectorContainer.bind(that));
        if (opts.canadd) {
            that.$advancedSelector.find(".advanced-selector-add-new-link").on('click', showAddItemBlock.bind(that));
        }
        $(document.body).on('keyup', listItemsNavigation.bind(that));
        that.$advancedSelector.find(".advanced-selector-block-list .advanced-selector-all-select div").on('click', selectedAll.bind(that));
        that.$advancedSelector.find(".advanced-selector-block-list .advanced-selector-all-select").on('click', showAllGroups.bind(that));

        that.$advancedSelector.find(".advanced-selector-search-field").on('keyup', onSearchInputKeyup.bind(that));
        that.$advancedSelector.find(".advanced-selector-search-btn").on('click', that.options.isTempLoad? that.onSearchItemsTempLoad.bind(that) : that.onSearchItems.bind(that));
        that.$advancedSelector.find(".advanced-selector-reset-btn").on('click', onSearchReset.bind(that));

        that.$advancedSelector.on("click", opts.onechosen ? ".advanced-selector-list-items li" : ".advanced-selector-btn-action", onClickSaveSelectedItems.bind(that));
        that.$advancedSelector.on("click", ".advanced-selector-list-block .advanced-selector-btn-cancel", onClickCancelSelector.bind(that));
        $(document.body).on('click', onBodyClick.bind(that));


        jq(window).bind("resizeWinTimerWithMaxDelay", function (event) {
            setPositionSelectorContainer.call(that);
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

        if (!that.$advancedSelector.is(":visible"))
            that.$advancedSelector.hide();

        if ($target.closest(that.$advancedSelector).length === 0 && that.$advancedSelector.is(":visible")
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
            if (that.options.isTempLoad) {
                that.initAdvSelectorDataTempLoad.call(that);
            } else {
                $search.trigger("click");
            }
            if (!that.options.showGroups) allSelect.show();
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

        if (that.options.isTempLoad) {
            that.initAdvSelectorDataTempLoad.call(that);
        } else {
            that.options.isTempLoad ? that.onSearchItemsTempLoad.call(that) : that.onSearchItems.call(that);
        }
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
                    that.initAdvSelectorGroupsData.call(that);
                }
            }
        }
        !that.$itemsListSelector.find(".advanced-selector-list li").filter(":visible").length ?
            that.$advancedSelector.find(".advanced-selector-empty-list").show() :
            that.$advancedSelector.find(".advanced-selector-empty-list").hide();
    }

    function setPositionSelectorContainer() {
        if (!this.$element.length) {
            return;
        }
        var that = this,
            $elem = that.$element,
            elemPos = that.options.inPopup ? $elem.position() : $elem.offset(),
            elemPosLeft = elemPos.left,
            elemPosTop = elemPos.top + $elem.outerHeight() + 4, // 4 - the top padding
            $w = that.options.$parent || $(window),
            windowWidth = $w.width(),
            scrHeight = $w.height(),
            topPadding = $w.scrollTop(),
            leftPadding = $w.scrollLeft(),
            $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block");

        var selectorOuterWidth = that.$advancedSelector.widthCounted + parseInt(that.$advancedSelector.css("border-left-width"), 10) + parseInt(that.$advancedSelector.css("border-right-width"), 10);
        var selectorOuterHeight = that.$advancedSelector.heightCounted + parseInt(that.$advancedSelector.css("border-top-width"), 10) + parseInt(that.$advancedSelector.css("border-bottom-width"), 10);

        if (($elem.offset().left + selectorOuterWidth - ($addPanel.length && !$addPanel.hasClass("right-position") ? $addPanel.outerWidth() : 0)) > (leftPadding + windowWidth)) {
            elemPosLeft = Math.max(0, elemPosLeft + that.$element.outerWidth() - selectorOuterWidth);
        }

        if ($addPanel.is(":visible")) {
            elemPosLeft -= that.widthAddBlock;
        }

        if (!jq.browser.mobile && elemPosTop + selectorOuterHeight > scrHeight + topPadding) {
            elemPosTop = elemPos.top - selectorOuterHeight;
        }

        if (that.options.$parent && $elem.offset().top + selectorOuterHeight > that.options.$parent.offset().top + that.options.$parent.height()) {
            elemPosTop = elemPos.top - selectorOuterHeight - 4;
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
        that.$advancedSelector.find(".advanced-selector-all-select div").removeClass("checked").removeClass("indeterminate");

        var lis = getList.call(that);

        for (var i = 0, ln = lis.length; i < ln; i++) {
            lis[i].item[0].getElementsByTagName("div")[0].classList.remove("checked");
        }

        if (that.options.showGroups) {
            that.$advancedSelector.find(".advanced-selector-list-groups li").removeClass("selected");
            that.$advancedSelector.find(".advanced-selector-list-groups div").removeClass("checked").removeClass("indeterminate");
        }
        that.itemsSelectedIds = that.options.itemsSelectedIds;
        onSearchReset.call(that);
        onCheckItemsById.call(that,  Object.keys(that.options.itemsSelectedIds));
    }


    function checkAlreadySelectedItemsIds(selectedItemsIds) {
        var that = this;

        actionWithListItem.call(that, selectedItemsIds, function(item) { item.addClass("selected-before"); },  ":not(.disabled)");

        onCheckItemsById.call(that, selectedItemsIds);
    }

    function pushItemsForGroup() {
        var that = this;
        var itemIds = {};
        for (var i = 0, j = that.items.length; i < j; i++) {
            itemIds[that.items[i].id] = undefined;
        }

        for (var i = 0, n = that.groups.length; i < n; i++) {
            var groupItems = window.GroupManager.getGroupItems(that.groups[i].id);
            that.groups[i].items = [];

            for (var k = 0, j = groupItems.length; k < j; k++) {
                var gi = groupItems[k];
                if (itemIds.hasOwnProperty(gi)) {
                    that.groups[i].items.push(gi);
                }
            }
        }
    }

    function getItemsForGroup(groupId) {
        var that = this;
        for (var i = 0, n = that.groups.length; i < n; i++) {
            if (groupId == that.groups[i].id) {
                return that.groups[i].items;
            }
        }
        return [];
    }

    function getItemById(id, itemList) {
        for (var i = 0, len = itemList.length; i < len; i++) {
            if (id == itemList[i].id) {
                return itemList[i];
            }
        }
        return null;
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
        addItemBlock.find(".advanced-selector-btn-cancel").on('click', hideAddItemBlock.bind(that));
        addItemBlock.find(".advanced-selector-btn-add").on('click', createNewItem.bind(that));

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
            $chosenGroup.find("div").addClass("indeterminate").removeClass("checked");
            if ($chosenGroup.length && !$chosenGroup.hasClass("chosen")) {
                $chosenGroup.find("label").trigger("click");
            }
        }
    }

    function selectedAll(event) {
        var that = this,
            $this = $(event.target).closest(".advanced-selector-all-select"),
            allCheckBox = $this.find("div"),
            flag = allCheckBox.hasClass("checked");

        if (that.options.onechosen) {
            $this.trigger("click");
            return;
        }

        if (!$(event.target).is("div")) {
            flag = allCheckBox.hasClass("checked");
        }

        allCheckBox.removeClass("indeterminate");
        that.itemsSelectedIds = {};

        var items = that.items;

        for (var i = 0, j = items.length; i < j; i++) {
            var id = items[i].id;
            if (that.options.itemsDisabledIds.find(function (item) { return item === id })) continue;
            if (!flag) {
                that.itemsSelectedIds[id] = undefined;
            } else {
                delete that.itemsSelectedIds[id];
            }
        }

        fullRedraw.call(that, items);

        var lis = getList.call(that, null, that.$groupsListSelector);

        for (var i = 0, j = lis.length; i < j; i++) {
            var lisI = lis[i].item[0];
            var lisIdiv = lisI.getElementsByTagName("div")[0];
            lisIdiv.classList.remove("indeterminate");
            if (flag) {
                lisI.classList.remove("selected");
                lisIdiv.classList.remove("checked");
            } else {
                lisI.classList.remove("chosen");
                lisI.classList.add("selected");
                lisI.getElementsByTagName("div")[0].classList.add("checked");
            }
        }

        if (!flag) {
            allCheckBox.addClass("checked");
        } else {
            allCheckBox.removeClass("checked");
        }

        $this.toggleClass("chosen", !flag);

        that.$advancedSelector.find(".advanced-selector-no-items").hide();
        onSearchReset.call(that);
        countSelectedItems.call(that, that.$itemsListSelector.find("li:not(.disabled)"));
        event.stopPropagation();
    }

    function showAllGroups(event) {
        var that = this,
            $this = $(event.target).closest(".advanced-selector-all-select"),
            $checkBox = $(event.target).closest("div");

        if (!that.options.showGroups && !event.target.isEqualNode($checkBox[0])) {
            selectedAll.call(that, event);
            return;
        }

        if (!$this.hasClass("chosen")) {
            that.$advancedSelector.find(".advanced-selector-no-items").hide();

            fullRedraw.call(that, that.items);

            if (that.$groupsListSelector) {
                that.$groupsListSelector.find("li").removeClass("chosen");
            }
            $this.addClass("chosen");
            onMatchGroupInAddItemBlock.call(that, that.nameSimpleSelectorGroup);
        }
    }

    function countSelectedItems($itemList) {
        var selectedCount = Object.keys(this.itemsSelectedIds).length,
            $countBox = $itemList.parents(".advanced-selector-block").find(".advanced-selector-selected-count");

        if (selectedCount > 0) {
            $countBox.text(selectedCount + " " + ASC.Resources.Master.Resource.SelectorSelectedItems).show();
        } else {
            $countBox.text("").hide();
        }
    }

    function arrayContainsAnotherArray(needle, haystack) {
        if (needle.length > haystack.length) return false;

        for (var i = 0; i < needle.length; i++) {
            if (haystack.indexOf(needle[i]) === -1)
                return false;
        }
        return true;
    }

    function onCheckItem(event, groupClicked) {
        var that = this,
            $target,
            $this,
            $checkBox,
            flag,
            id;

        if (groupClicked) {
            $target = event.item;
            $this = $target;
            $checkBox = $target[0].getElementsByTagName("div");
            id = event.id;
        } else {
            $target = $(event.target);
            $this = $(event.target).closest("li");
            $checkBox = $this[0].getElementsByTagName("div");
            id = $this[0].getAttribute("data-id");
        }

        flag = $checkBox[0] && $checkBox[0].classList.contains("checked");

        if ($this.hasClass("disabled")) { return; }

        if (that.options.onechosen) {
            that.$itemsListSelector.find("li.selected").removeClass("selected");
            $this.addClass("selected");
            that.itemsSelectedIds = {};
            that.itemsSelectedIds[id] = undefined;
        } else {
            if (flag) {
                $checkBox[0].classList.remove("checked");
                delete that.itemsSelectedIds[id];
                $this[0].classList.remove("selected");
            }
            else {
                $checkBox[0].classList.add("checked");
                $this[0].classList.add("selected");
                that.itemsSelectedIds[id] = undefined;
            }
            if (!groupClicked) {
                onCheckSelectedAll.call(that, that.$itemsListSelector);
                countSelectedItems.call(that, that.$itemsListSelector);
            }
        }

        if (!flag) {
            that.itemsSelectedIds[id] = undefined;
        } else {
            delete that.itemsSelectedIds[id];
        }
        if (!groupClicked) {
            if (that.$groupsListSelector && that.$groupsListSelector.length) {
                onSelectGroupsByItem.call(that, [id], !flag);
            }
        }
        return id;
    }

    function onCheckItems(items) {
        var ids = [];
        for (var i = 0, j = items.length; i < j; i++) {
            ids.push(onCheckItem.call(this, items[i], true));
        }

        var that = this;

        if (!that.options.onechosen) {
            onCheckSelectedAll.call(that, that.$itemsListSelector);
            countSelectedItems.call(that, that.$itemsListSelector);
        }

        if (that.$groupsListSelector && that.$groupsListSelector.length) {
            onSelectGroupsByItem.call(that, ids, true);
        }
    }

    function onCheckItemsById(itemsIds) {
        var that = this,
            checkedItems = [],
            checkedGroups = [],
            checkedItemsIds = [];

        actionWithListItem.call(that,
            itemsIds,
            function (item) {
                item[0].classList.add("selected");
                var div = item[0].getElementsByTagName("div")[0];
                if (div && !div.classList.contains("checked")) {
                    div.classList.add("checked");
                    //that.itemsSelectedIds[id] = undefined;
                }
            },
            ":not(.disabled)");

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

                itemsForCheckedGroup = $.grep(itemsForCheckedGroup,
                    function(elem) {
                        return $.inArray(elem, that.options.itemsDisabledIds) == -1;
                    });

                if ($(checkedGroup).length) {
                    var isCheck = arrayContainsAnotherArray(itemsForCheckedGroup, checkedItemsIds) ? true : false;

                    var div = checkedGroup.find("div")[0];
                    if (div) {
                        if (isCheck) {
                            div.classList.add("checked");
                            div.classList.remove("indeterminate");
                        } else {
                            div.classList.add("indeterminate");
                            div.classList.remove("checked");
                        }
                    }
                }
            }
        }

        onCheckSelectedAll.call(that, that.$itemsListSelector);
        countSelectedItems.call(that, that.$itemsListSelector);
    }

    function onCheckSelectedAll() {
        var selectedCount = Object.keys(this.itemsSelectedIds).length,
                allSelect = this.$advancedSelector.find(".advanced-selector-all-select"),
                isAllCheck,
                isAllDeterm;

        if (selectedCount === 0) {
            isAllCheck = false;
            isAllDeterm = false;
        } else {
            isAllDeterm = true;
            if (selectedCount === (this.items.length - this.options.itemsDisabledIds.length)) {
                isAllCheck = true;
                isAllDeterm = false;
            }
        }

        if (isAllCheck) {
            allSelect.find("div").addClass("checked");
        } else {
            allSelect.find("div").removeClass("checked");
        }

        if (isAllDeterm) {
            allSelect.find("div").addClass("indeterminate");
        } else {
            allSelect.find("div").removeClass("indeterminate");
        }

        if (!isAllCheck && !isAllDeterm) {
            allSelect.removeClass("chosen");
        }
    }

    // select groups in which there are the selected item

    function onSelectGroupsByItem(itemIds, check) {
        var that = this,
            itemSelectedGroups = {},
            groupSelectedItems,
            groupCurrent;

        for (var k = 0, ln = itemIds.length; k < ln; k++) {
            var user = UserManager.getUser(itemIds[k]);
            if (!user || !user.groups) continue;

            for (var l = 0; l < user.groups.length; l++) {
                itemSelectedGroups[user.groups[l]] = undefined;
            }
        }

        function findGroupById(itemSelectedGroup) {
            return function(item) { return item.id === itemSelectedGroup };
        }

        var groups = Array.from(that.$groupsListSelector.find("li")).map(mapItems);

        for (var itemSelectedGroup in itemSelectedGroups) {
            if (!itemSelectedGroups.hasOwnProperty(itemSelectedGroup)) continue;

            groupCurrent = groups.find(findGroupById(itemSelectedGroup));
            
            if(!groupCurrent) continue;

            groupCurrent = groupCurrent.item.find("div");

            groupSelectedItems = getItemsForGroup.call(that, itemSelectedGroup);
            //groupSelectedItems = $.grep(groupSelectedItems, function (el) {
            //    return $.inArray(el, that.options.itemsDisabledIds) == -1;
            //});

            if (check) {
                if (selectedAllForGroup.call(that, groupSelectedItems)) {
                    groupCurrent.removeClass("indeterminate").addClass("checked");
                    groupCurrent.closest("li").not(".selected").addClass("selected");
                } else {
                    groupCurrent.addClass("indeterminate");
                }
            } else {
                if (!selectedAllForGroup.call(that, groupSelectedItems)) {
                    var isContainInGroup = Object.keys(that.itemsSelectedIds).some(
                        function(elem){
                            return $.inArray(elem, groupSelectedItems) !== -1;
                    });
                    groupCurrent.removeClass("checked").removeClass("indeterminate");
                    groupCurrent.closest("li").removeClass("selected");
                    if (isContainInGroup) {
                        groupCurrent.addClass("indeterminate");
                    }
                }
            }
        }
    }

    function selectedAllForGroup(arrayToSearch) {
        if (!arrayToSearch.length) return false;
        for (var i = 0, j = arrayToSearch.length; i < j; i++) {
            var arrayToSearchI = arrayToSearch[i];
            if (this.options.itemsDisabledIds.indexOf(arrayToSearchI) > -1) continue;

            if (!this.itemsSelectedIds.hasOwnProperty(arrayToSearchI)) {
                return false;
            }
        }
        return true;
    }

    //select all items from group

    function onCheckGroup(event) {
        var that = this,
            $this = $(event.target),
            $curGroup = $this.closest("li"),
            flag = $this[0].classList.contains("checked"),
            groupSelectedItems = [];

        onChooseGroup.call(this, event);

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

        $this.removeClass("indeterminate");

        if (flag) {
            $this.removeClass("checked");
        } else {
            $this.addClass("checked");
        }

        var currgroupId = $curGroup.attr("data-id");
        groupSelectedItems = getItemsForGroup.call(that, currgroupId);

        function findGroup(item) {
            return item === currgroupId;
        }

        var lis = getList.call(that, ":not(.disabled)");

        for (var i = 0, ln = lis.length; i < ln; i++) {
            var lisI = lis[i];
            var user = UserManager.getUser(lisI.id);
            if (!user || !user.groups || !user.groups.some(findGroup)) continue;
            if (that.options.itemsDisabledIds.hasOwnProperty(lisI.id)) continue;

            var itEl = lisI.item;
            if (flag) {
                delete that.itemsSelectedIds[lisI.id];
            } else {
                that.itemsSelectedIds[lisI.id] = undefined;
            }

            flag ? itEl[0].classList.remove("selected") : itEl[0].classList.add("selected");
            if (flag) {
                itEl[0].getElementsByTagName("div")[0].classList.remove("checked");
            } else {
                itEl[0].getElementsByTagName("div")[0].classList.add("checked");
            }
        };

        onSelectGroupsByItem.call(that, groupSelectedItems, !flag, that.$advancedSelector);
        onCheckSelectedAll.call(that, that.$itemsListSelector);
        countSelectedItems.call(that, that.$itemsListSelector);
        event.stopPropagation();
    }


    //choose group and display items from this group

    function onChooseGroup(event) {
        var that = this,
            $groupChosen = $(event.target).closest("li"),
            groupId = $groupChosen.attr("data-id"),
            groupsList = that.$groupsListSelector.find("li"),
            allGroups = that.$groupsListSelector.siblings(".advanced-selector-all-select"),
            noItems = that.$advancedSelector.find(".advanced-selector-no-items"),
            noResults = that.$advancedSelector.find(".advanced-selector-no-results");

        allGroups.removeClass("chosen");
        groupsList.removeClass("chosen");
        $groupChosen.addClass("chosen");

        noItems.hide();
        noResults.hide();

        function findGroup(item) {
            return item === groupId;
        }

        var items = that.items.filter(function (item) {
            var user = UserManager.getUser(item.id);
            return user && user.groups && user.groups.some(findGroup);
        });

        fullRedraw.call(that, items);

        if (items.length === 0) {
            noItems.show();
        }

        onMatchGroupInAddItemBlock.call(that, that.nameSimpleSelectorGroup);
    }

    function onClickSaveSelectedItems(event) {
        var that = this,
            $this = $(event.target),
            selectedItemsList = that.itemsSelectedIds,
            selectedItems = [],
            result;

        that.options.itemsSelectedIds = jq.extend({}, that.itemsSelectedIds);
        for (var sil in selectedItemsList) {
            if (selectedItemsList.hasOwnProperty(sil)) {
                var item = getItemById(sil, that.items);
                if (item !== null) {
                    selectedItems.push(item);
                }
            }
        }

        if (that.options.onechosen && $this.hasClass("selected-before")) {
            $this.removeClass("selected-before");
            hideSelectorContainer.call(that);
            return;
        }
        that.selectedItems = selectedItems;
        hideSelectorContainer.call(that);
        result = that.options.onechosen ? selectedItems : [selectedItems];
        that.$element.trigger("showList", result);
        delete that.selectedItems;
    }

    function initGroupEvents() {
        var that = this;
        that.$groupsListSelector
            .off('click')
            .on('click', 'ul li div', onCheckGroup.bind(that))
            .on('click', 'ul li', onChooseGroup.bind(that));
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
        $selector.find("li").on("click", onSelectSimpleSelector.bind(that));
        that.$advancedSelector.on('click', onClickContainerSimpleSelector);
        $reset.on("click", onResetSimpleSelector);
        $(document.body).on('keyup', listSimpleItemsNavigation.bind(that));

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

    var redraw = function (itemsDisplay) {
        var that = this;

        if (that.options.showme) {
            var user = {};

            for (var i = 0, length = itemsDisplay.length; i < length; i++) {
                if (itemsDisplay[i].id == Teamlab.profile.id) {
                    user = itemsDisplay[i];
                    user.title = ASC.Resources.Master.Resource.MeLabel;
                    itemsDisplay.splice(i, 1);
                    itemsDisplay.unshift(user);
                    break;
                }
            }
        }

        var fragment = document.createDocumentFragment();

        var li1 = document.createElement("li");
        var div1 = document.createElement("div");
        var label1 = document.createElement("label");
        var multiplyChosen = !that.options.onechosen;

        for (var i = 0; i < itemsDisplay.length; i++) {
            var item = itemsDisplay[i];
            var title = typeof item.title === "string" ? Encoder.htmlDecode(item.title) : item.title;
            var li = li1.cloneNode(false);
            var className = "";
            li.title = title;

            var dataId = document.createAttribute("data-id");
            dataId.value = item.id;
            li.setAttributeNode(dataId);

            //data-id, data-cnt
            if (item.status) {
                li.title += jq.format(" ({0})", item.status);
                if (item.status === ASC.Resources.Master.Resource.UserPending) {
                    className += " pending";
                }
            }
            if (item.type) {
                className += " " + item.type;
            }

            if (that.itemsSelectedIds.hasOwnProperty(item.id)) {
                className += " selected";
            }

            if (that.options.itemsDisabledIds.indexOf(item.id) > -1) {
                className += " disabled";
            }

            if (className) {
                li.className = className;
            }

            if (multiplyChosen) {
                var input = div1.cloneNode(false);
                if (that.itemsSelectedIds.hasOwnProperty(item.id)) {
                    input.className = "checked";
                }
                li.appendChild(input);

                var label = label1.cloneNode(false);
                label.innerText = title;
                li.appendChild(label);
            } else {
                li.innerText = title;
            }

            fragment.appendChild(li);
        }

        return fragment;
    };

    var fullRedraw = function (items) {
        var that = this;

        var height;
        if ((!that.options.canadd && that.options.showGroups) || that.options.isTempLoad) {
            height = 177;//height for the items container without the creation of the new item
        }

        if (that.options.onechosen) {
            height = that.options.canadd ? that.heightListChooseOne : that.heightListWithoutCreate;
        }

        if (!that.options.onechosen && that.options.canadd && !that.options.showGroups) {
            height = 131;
        }

        if (that.options.height) {
            height = that.options.height;
        }

        var list = that.$itemsListSelector.find(".advanced-selector-list")[0];
        list.style.display = "none";

        var ul = document.createElement("ul");
        ul.className = "advanced-selector-list";
        ul.appendChild(redraw.call(that, items));

        if (height) {
            that.$itemsListSelector.height(height);
            ul.style.height = height + "px";
        }

        jq(list).replaceWith(ul);

        list.style.display = "";
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
            that.widthSelector = that.options.width ? that.options.width : 211;
            that.widthAddBlock = 216,
            that.items = [];
            that.groups = [];
            that.nameSimpleSelectorGroup = "";
            that.cache = {};
            that.itemsSelectedIds = {};

            if (Array.isArray(that.options.itemsSelectedIds)) {
                var itemsSelectedIds = that.options.itemsSelectedIds.concat([]);
                that.options.itemsSelectedIds = {};

                for (var i = 0, j = itemsSelectedIds.length; i < j; i++) {
                    that.options.itemsSelectedIds[itemsSelectedIds[i]] = undefined;
                    that.itemsSelectedIds[itemsSelectedIds[i]] = undefined;
                }
            }
            var advancedSelectorWidthAppender = 5;


            var $o = $.tmpl(that.options.templates.selectorContainer, { opts: that.options });
            if (that.options) {
                that.$element.after($o);
            }
            that.$advancedSelector = $o;

            that.$itemsListSelector = that.$advancedSelector.find(".advanced-selector-list-items");

            if (that.options.showGroups) {
                that.$groupsListSelector = that.$advancedSelector.find(".advanced-selector-list-groups");
            }
            that.$advancedSelector.widthCounted = that.options.showGroups
                ? that.widthSelector * 2
                : that.widthSelector + advancedSelectorWidthAppender;

            that.$advancedSelector.css({
                width: that.$advancedSelector.widthCounted
            });

            if (that.options.height || that.options.width) {
                var $listBlock = that.$advancedSelector.find(".advanced-selector-list-block");

                if (that.options.height) {
                    var listBlockHeightPadding = $listBlock.innerHeight() - $listBlock.height();

                    that.$advancedSelector.css({
                        height: that.options.height + listBlockHeightPadding
                    });
                    that.$advancedSelector.heightCounted = that.options.height + listBlockHeightPadding;
                }

                if (that.options.width) {
                    var listWidthPadding = $listBlock.innerWidth() - $listBlock.width();

                    var widthCounted = ((that.options.showGroups ? that.widthSelector : that.widthSelector + advancedSelectorWidthAppender) - listWidthPadding);

                    that.$advancedSelector.find(".advanced-selector-block-list").css({
                        width: widthCounted + "px"
                    });
                }
            }

            if (!that.options.height) {
                that.$advancedSelector.heightCounted = that.$advancedSelector.outerHeight();
            }

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
            that.$advancedSelector.find(".advanced-selector-no-results, .advanced-selector-no-items, .advanced-selector-empty-list").hide();
            if (!that.options.onechosen) {
                that.$advancedSelector.find(".advanced-selector-all-select").show();
            }
            if (that.options.showSearch) {
                that.$advancedSelector.find(".advanced-selector-search").show();
            }
            if (that.options.showGroups) {
                showNewListItemsAfterCreateItem.call(that, ID);
            }
            that.$element.trigger("afterCreate", newObj);
            hideAddItemBlock.call(that);
        },

        disableDefaultItemsIds: function (disabledItemsIds) {
            var that = this;

            actionWithListItem.call(that, disabledItemsIds,
                function (item) {
                    item.addClass("disabled");
                });
        },

        onSearchItems: function () {
            var that = this;

            if (that.$advancedSelector.find(".advanced-selector-loader-list").is(":visible")) {
                setTimeout(function () { that.onSearchItems.call(that); }, 100);
                return;
            }

            var $searchFld = that.$advancedSelector.find(".advanced-selector-search-field"),
                searchQuery = ($searchFld.length !== 0) ? $.trim($searchFld.val()) : "",
                $noResult = that.$advancedSelector.find(".advanced-selector-no-results");

            $noResult.hide();

            searchQuery = searchQuery.toLowerCase();

            var items = that.items;

            if (searchQuery) {
                items = items
                    .filter(function (item) {
                        return Encoder.htmlDecode(item.title).toLowerCase().indexOf(searchQuery) > -1;
                    });
            }

            fullRedraw.call(that, items);

            if (searchQuery && !items.length) {
                $noResult.show();
            }
        },
        initDataSimpleSelector: function (data) {
            var that = this,
                objType = data.tag,
                list = $.tmpl(that.options.templates.addNewItems, { Items: data.items }),
                $field = that.$advancedSelector.find(".advanced-selector-field-wrapper." + objType);
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
                if (data[i].hasOwnProperty("id")) {
                    newObj.id = data[i].id;
                } else if (data[i].hasOwnProperty("Id")) {
                    newObj.id = data[i].Id;
                }


                if (data[i].hasOwnProperty("isPending")) {
                    newObj.status = data[i].isPending || data[i].isActivated === false ? ASC.Resources.Master.Resource.UserPending : "";
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

            that.items = that.items.sort(that.options.sortMethod || SortData);
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
            var that = this;

            fullRedraw.call(that, that.items);

            that.$itemsListSelector.off('click').on('click', 'li', onCheckItem.bind(that));

            if (!that.$itemsListSelector.find("li:not(.disabled)").length ) {
                that.$advancedSelector.find(".advanced-selector-empty-list").show();
                that.$advancedSelector.find(".advanced-selector-search").hide();
                that.$advancedSelector.find(".advanced-selector-all-select").hide();
            } else {
                that.$advancedSelector.find(".advanced-selector-empty-list").hide();
                if (that.options.showSearch) {
                    that.$advancedSelector.find(".advanced-selector-search").show();
                }
                that.$advancedSelector.find(".advanced-selector-all-select").show();
            }

            if (that.options.showGroups) {
                initGroupEvents.call(that);
            } else {
                checkAlreadySelectedItemsIds.call(that, Object.keys(that.options.itemsSelectedIds));
            }
            setFocusOnSearch.call(that);
        },

        showGroupsListAdvSelector: function () {
            var that = this;

            if (that.groups.length) {
                var $groups = [];

                that.groups.chunkArray(1000).forEach(function (chunck) {
                    $groups.push($.tmpl(that.options.templates.groupList, { Items: chunck, isJustList: that.options.onechosen }));
                });

                that.$groupsListSelector.find(".advanced-selector-list").html($groups);
            } else {
                that.$groupsListSelector.find(".advanced-selector-no-groups").show();
            }

            if (that.options.onechosen) {
                var height = that.options.canadd ? that.heightListChooseOne : that.heightListWithoutCreate - 28; // 28 - height of the field "Select All"
                that.$groupsListSelector.find(".advanced-selector-list").height(height);
            }
            pushItemsForGroup.call(that);
            checkAlreadySelectedItemsIds.call(that, Object.keys(that.itemsSelectedIds));
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

            list.filter(".selected,.selected-before").removeClass("selected selected-before");
            if (!flag) {
                list.removeClass("disabled");
            }
            that.$advancedSelector.find(".advanced-selector-list li div, .advanced-selector-all-select div").removeClass("checked").removeClass("indeterminate");

            if (that.options.showGroups) {
                that.$advancedSelector.find(".advanced-selector-list-groups li").removeClass("selected");
                that.$advancedSelector.find(".advanced-selector-list-groups div").removeClass("checked").removeClass("indeterminate");
            }
            that.itemsSelectedIds = {};
            that.options.itemsSelectedIds = {};
            onSearchReset.call(that);
            countSelectedItems.call(that, that.$itemsListSelector);
        },
        select: function (selectedItemsIds) {
            var that = this;

            var itemsSelectedIds = that.options.itemsSelectedIds;
            for (var i = 0, j = selectedItemsIds.length; i < j; i++) {
                itemsSelectedIds[selectedItemsIds[i]] = undefined;
            }

            var lis = getList.call(that);

            if (lis.length) {
                var filtered = [];

                for (var i = 0, j = lis.length; i < j; i++) {
                    var lisI = lis[i];
                    if (that.itemsSelectedIds.hasOwnProperty(lisI.id)) continue;
                    if (that.options.itemsDisabledIds.hasOwnProperty(lisI.id)) continue;
                    if (itemsSelectedIds.hasOwnProperty(lisI.id)) {
                        filtered.push(lisI);
                    }
                }

                onCheckItems.call(this, filtered);

                if (that.options.onechosen) {
                    for (var i = 0, j = filtered.length; i < j; i++) {
                        onClickSaveSelectedItems.call(that, { target: filtered[i].item });
                    }
                }
            }
        },
        unselect: function (itemsIds) {
            var that = this;

            var itemsSelectedIds = that.options.itemsSelectedIds;
            for (var i = 0, j = itemsIds.length; i < j; i++) {
                delete itemsSelectedIds[itemsIds[i]];
            }

            actionWithListItem.call(that, itemsIds, function(item) {
                if (that.options.onechosen) {
                    item.removeClass("selected");
                }
                var div = item[0].getElementsByTagName("div");
                if (div && div.length && div[0].classList.contains("checked")) {
                    item.trigger("click");
                }
            });
        },

        disable: function (itemsIds) {
            var that = this;
            that.unselect.call(that, itemsIds);
            that.options.itemsDisabledIds = that.options.itemsDisabledIds.concat(itemsIds).unique();

            actionWithListItem.call(that, itemsIds, function (item) {
                item.addClass("disabled").hide();
            });
        },

        undisable: function (itemsIds) {
            var that = this;

            that.options.itemsDisabledIds = $.grep(that.options.itemsDisabledIds, function (n, i) {
                return $.inArray(n, itemsIds) == -1;
            });

            actionWithListItem.call(that, itemsIds, function (item) {
                item.removeClass("disabled").show();
            });
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
                that.itemsSelectedIds = {};
                that.options.itemsSelectedIds = {};
                for (var i = 0; i < dataIdsSelected.length; i++) {
                    that.itemsSelectedIds[dataIdsSelected[i]] = undefined;
                    that.options.itemsSelectedIds[dataIdsSelected[i]] = undefined;
                }
            }
        },
        selectBeforeShow: function (item) {
            this.$element.trigger("showList", arguments);
            this.select([item.id]);
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
        isTempLoad: false,
        $parent: null,

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