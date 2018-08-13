/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


ASC.Projects.Tab = function (moduleName, count, divID, $container, link, isVisibleSelector, emptyScreen) {
    this.title = moduleName;
    this.count = count;
    this.divID = divID;
    this.$container = $container;

    if (!link) {
        this.selected = false;
    } else {
        if (link.indexOf("#") === 0) {
            this.selected = location.href.endsWith(link);
        } else {
            this.selected = location.href.indexOf(link) > 0;
        }
    }

    this.link = link;
    this.isVisibleSelector = isVisibleSelector;
    this.emptyScreen = emptyScreen;
}

ASC.Projects.Tab.prototype.select = function () {
    this.selected = true;

    var self = this,
        $emptyScreenContainer = jq("#emptyScrCtrlPrj");

    if (this.count() === 0 && this.emptyScreen) {
        this.$container.hide();
        $emptyScreenContainer.html(jq.tmpl("projects_emptyScreen", this.emptyScreen)).show();

        var $addFirstElement = $emptyScreenContainer.find(".addFirstElement");

        $addFirstElement.off("click").on("click",
            function () {
                $emptyScreenContainer.hide();
                self.$container.show();
                self.emptyScreen.button.onclick();
            });

        var currentHash = ASC.Controls.AnchorController.getAnchor();

        if (currentHash === this.emptyScreen.button.hash && $addFirstElement.length > 0) {
            $addFirstElement.click();
        }

    } else if (this.$container) {
        $emptyScreenContainer.hide();
        this.$container.show();
    }

    if (this.link.indexOf("#") === 0) {
        location.hash = this.link;
    }

    this.rewrite();
}

ASC.Projects.Tab.prototype.unselect = function () {
    this.selected = false;
    if (this.$container) {
        this.$container.hide();
    }
    this.rewrite();
}

ASC.Projects.Tab.prototype.rewrite = function () {
    this.getDiv().replaceWith(jq.tmpl("projects_tabsNavigatorItem", this));
}

ASC.Projects.Tab.prototype.getDiv = function () {
    return jq("#" + this.divID + "_tab");
}

ASC.Projects.ProjectTab = function (project, moduleName, count, page, divID, onclick, isVisibleSelector) {
    this.href = page ? page + "?prjID=" + project.id : "";
    this.onclick = onclick;

    ASC.Projects.Tab.call(this, moduleName, count, divID, undefined, page, isVisibleSelector);
}

ASC.Projects.ProjectTab.prototype = new ASC.Projects.Tab();

ASC.Projects.DescriptionTab = (function () {
    var description,
        $tab,
        self;

    function init() {
        self = this;
        description = { data: [], status: { items: [], current: {}, canChange: false} };
        $tab = jq(".tab");
        return self;
    }

    function tmpl() {
        $tab.html(jq.tmpl("projects_description", description));
        show();
        var statuses = description.status.items;

        if (statuses.length) {
            var $statusSelector = $tab.find(".status");
            $statusSelector.advancedSelector(
                {
                    height: 26 * statuses.length,
                    itemsChoose: statuses,
                    itemsSelectedIds: [description.status.current.id],
                    showSearch: false,
                    onechosen: true,
                    sortMethod: function() { return 0; }
                }
            );

            $statusSelector.on("showList", function (event, item) {
                var status = statuses.find(function (i) { return i.id == item.id; });
                status.handler();
            });
        }

        return self;
    }

    function show() {
        $tab.show();
    }

    function hide() {
        $tab.hide();
    }

    function pushData(title, value, href, wrapper) {
        description.data.push({ title: title, value: value, href: href, wrapper: wrapper });
        return self;
    }

    function pushStatus(title, id, handler) {
        description.status.items.push({ title: title, handler: handler, id: id.toString() });

        return self;
    }

    function setCurrentStatus(id) {
        description.status.current = description.status.items.find(function (item) { return item.id == id });
        return self;
    }

    function resetStatus(statusId) {
        var $statusSelector = $tab.find(".status");
        $statusSelector.advancedSelector("select", [statusId.toString()]);
        return self;
    }

    function setStatusRight(canChange) {
        description.status.canChange = canChange;
        return self;
    }

    return {
        init: init,
        push: pushData,
        pushStatus: pushStatus,
        resetStatus: resetStatus,
        setStatusRight: setStatusRight,
        setCurrentStatus: setCurrentStatus,
        tmpl: tmpl,
        show: show,
        hide: hide
    };
})();

ASC.Projects.TabCollection = (function () {
    var currentTabs, selectedTab, $projectTabs;

    function init(tabs) {
        currentTabs = tabs;
        
        var filteredTabs = currentTabs.filter(function(item) {
            return typeof item.isVisibleSelector === "function" ? item.isVisibleSelector() : true;
        });
        $projectTabs = jq("#projectTabs");
        $projectTabs.html(jq.tmpl("projects_tabsNavigator", { tabs: filteredTabs })).show();
        $projectTabs.off("click").on("click", ".tabsNavigationLink", function (event) {
            var id = jq(this).attr("id");
            var tab = filteredTabs.find(function (item) { return item.divID + "_tab" === id; });
            if (!tab) return;

            selectedTab.unselect();
            tab.select();
            selectedTab = tab;

            if (typeof selectedTab.onclick === "function") {
                return selectedTab.onclick.call(this, { id: selectedTab.getDiv(), href: selectedTab.getDiv().attr("href") }, event);
            }

            return true;
        });

        var containItemSelected = filteredTabs.some(function(item) {
            return item.selected;
        });

        if (!containItemSelected) {
            filteredTabs[0].selected = true;
        }

        filteredTabs.forEach(function(item) {
            if (item.selected) {
                item.select();
            } else {
                item.unselect();
            }
        });

        selectedTab = filteredTabs.find(function (item) { return item.selected; });
    };

    return {
        init: init
    }
})();

ASC.Projects.InfoContainer = (function () {
    var projectInfoContainerClass = ".project-info-container", $projectInfoContainer;

    var init = function (data, showEntityMenu, tabs) {
        jq(projectInfoContainerClass).replaceWith(jq.tmpl("projects_navPanel", data));
        $projectInfoContainer = jq(projectInfoContainerClass);
        ASC.Projects.Base.initActionPanel(showEntityMenu && showEntityMenu().menuItems.length > 0 ? showEntityMenu : undefined, $projectInfoContainer);

        if (tabs) {
            ASC.Projects.TabCollection.init(tabs);
        }
        $projectInfoContainer.show();
    }
    var updateTitle = function(title) {
        $projectInfoContainer.find("#essenceTitle").attr("title", title).text(title);
    }
    return { init: init, updateTitle: updateTitle };
})();

ASC.Projects.ActionMenuItem = function (id, text, handler) {
    this.id = id;
    this.text = text;
    this.handler = handler;
}

ASC.Projects.DescriptionPanel = (function() {
    var $panel,
        over,
        timeout,
        clickEvent = "click";

    function init($commonListContainer, filter, settings) {
        if (!settings) return;

        tmpl($commonListContainer, filter);

        $commonListContainer.on('mouseenter', settings.selector, function (event) {
            var $targetObject = jq(event.target);
            var item = settings.getItem(event.target);
            show(item, $targetObject, settings.getLink ? settings.getLink(item) : undefined);
        });

        $commonListContainer.on('mouseleave', settings.selector, function () {
            hide(false);
        });
    }

    function tmpl($commonListContainer, filter) {
        if ($panel) return;
        $commonListContainer.append(jq.tmpl("projects_panelFrame", { panelId: "descrPanel" })); // description panel
        $panel = jq("#descrPanel");

        $panel.on("mouseenter", function () {
            over = true;
        });

        $panel.on("mouseleave", function () {
            over = false;
            hide();
        });

        $panel.on(clickEvent, '.project .value', function () {
            hide(false);
            filter.add('project', jq(this).attr("projectId"), ['tag', 'milestone', 'myprojects']);
        });

        $panel.on(clickEvent, '.milestone .value', function () {
            filter.add('milestone', jq(this).attr('milestone'));
        });
    }

    function show(obj, $targetObject, readmore) {
        if (typeof obj === "undefined") return;

        if (timeout > 0) return;
        timeout = setTimeout(function() {
                var panelContent = $panel.find(".panel-content"),
                    descriptionObj = {};

                var created = obj.displayDateCrtdate || obj.created,
                    description = obj.description,
                    createdBy = "",
                    projectId = jq.getURLParam("prjID") ? undefined : obj.projectId;

                if (typeof obj.createdBy != "undefined") {
                    createdBy = typeof obj.createdBy == "string" ? obj.createdBy : obj.createdBy.displayName;
                }

                if (checkProperty(created)) {
                    descriptionObj.creationDate = created;
                }

                if (checkProperty(description)) {
                    descriptionObj.description = description;
                    if (readmore && (description.indexOf("\n") > 2 || description.length > 80)) {
                        descriptionObj.readMore = readmore;
                    }
                }

                if (checkProperty(projectId)) {
                    descriptionObj.project = jq.htmlEncodeLight(obj.projectTitle);
                    descriptionObj.projectId = obj.projectId;
                }

                if (checkProperty(createdBy)) {
                    descriptionObj.createdBy = Encoder.htmlEncode(createdBy);
                }

                if (checkProperty(obj.displayDateUptdate) && (obj.statusname === "closed" || obj.status === "closed" || obj.status === 2)) {
                    descriptionObj.closedDate = obj.displayDateUptdate.substr(0, 10);
                    if (checkProperty(obj.updatedBy)) {
                        descriptionObj.closedBy = Encoder.htmlEncode(obj.updatedBy.displayName);
                    }
                }

                if (checkProperty(obj.displayDateStart)) {
                    descriptionObj.startDate = obj.displayDateStart;
                }

                if (checkProperty(obj.milestone)) {
                    descriptionObj.milestone = Encoder.htmlEncode("[" + obj.milestone.displayDateDeadline + "] " + obj.milestone.title);
                    descriptionObj.milestoneId = obj.milestone.id;
                }

                panelContent.html(jq.tmpl("projects_descriptionPanelContent", descriptionObj));
                showDescPanelByObject($targetObject);
                over = true;

            },
            500,
            this);

        return;
    };

    function hide(newOverValue) {
        if (timeout <= 0 || typeof $panel == "undefined") return;
        over = newOverValue;

        setTimeout(function() {
                if (over) return;
                $panel.hide();
                clearTimeout(timeout);
                timeout = -1;
            },
            50);
    };

    function checkProperty(prop) {
        return typeof prop != 'undefined' && jq.trim(prop) !== "" && prop != null;
    }

    function showDescPanelByObject($targetObject) {
        var offset = $targetObject.offset();
        if (!offset.top && !offset.left) return;
        var x = offset.left,
            y = calculateTopPosition(offset.top + $targetObject.outerHeight(), $targetObject);

        $panel.css({ left: x, top: y });
        $panel.show();
    };

    function calculateTopPosition(y, self) {
        var panelHeight = $panel.innerHeight(),
            w = jq(window),
            scrScrollTop = w.scrollTop(),
            scrHeight = w.height();

        if (panelHeight < y && scrHeight + scrScrollTop - panelHeight <= y) {
            y = y - $panel.outerHeight();

            if (self) {
                y = y - self.outerHeight();
            }
        }

        return y;
    }

    return {
        init: init,
        hide: hide
    };
})();

ASC.Projects.Event = function(event, handler) {
    return { event: event, handler: handler };
}

ASC.Projects.EventBinder = (function () {
    var teamlab = Teamlab;
    var handlers = [];

    function bind(events) {
        if (events && Array.isArray(events)) {
            handlers = events.map(function(item) { return teamlab.bind(item.event, item.handler); });
        }
    }

    function unbind() {
        while (handlers.length) {
            var handler = handlers.shift();
            teamlab.unbind(handler);
        }
    }
    return {
        bind: bind,
        unbind: unbind
    }
})();

ASC.Projects.GroupActionPanel = (function () {
    var $groupActionContainer,
        $groupActionMenu,
        $selectAll,
        $container,
        countChecked = 0;

   var tmpl = "projects_groupActionMenu",
        $counterSelectedItems;
    var checkedAttr = "checked",
        checkedRowClass = "checked-row",
        menuActionOnTopClass = ".menu-action-on-top",
        withCheckbox = ".with-checkbox",
        changeEvent = "change",
        clickEvent = "click",
        unlockAction = "unlockAction",
        indeterminate = "indeterminate",
        currentActions,
        currentGetItemByCheckbox;

    function init(settings, $c) {
        $container = $c;
        currentActions = settings.actions;
        countChecked = 0;
        currentGetItemByCheckbox = settings.getItemByCheckbox;

        if ($groupActionMenu) {
            $groupActionMenu.remove();
        }

        $groupActionContainer = jq("#groupActionContainer");
        $groupActionContainer.append(jq.tmpl(tmpl, settings));
        $groupActionMenu = jq("#groupActionMenu");

        for (var i = currentActions.length - 1; i >= 0; i--) {
            var actionsHandler = function(actionsItem) {
                return function () {
                    if (!jq(this).hasClass(unlockAction)) return;
                    actionsItem.handler(getCheckedItems().map(function (item) {return item.id; }));
                };
            }(currentActions[i]);

            jq("#" + currentActions[i].id).on("click", actionsHandler);
        }

        $counterSelectedItems = $groupActionMenu ? $groupActionMenu.find(".menu-action-checked-count") : [];

        if ($groupActionMenu) {
            var options = {
                menuSelector: "#groupActionMenu",
                menuAnchorSelector: "#selectAll",
                menuSpacerSelector: "#CommonListContainer .header-menu-spacer",
                userFuncInTop: function () { $groupActionMenu.find(menuActionOnTopClass).hide(); },
                userFuncNotInTop: function () { $groupActionMenu.find(menuActionOnTopClass).show(); }
            };
            ScrolledGroupMenu.init(options);

            $selectAll = jq("#selectAll");

            $selectAll.off(changeEvent).on(changeEvent, function () {
                jq("#groupActionMenuSelector").hide();
                var $checkboxes = $container.find(".checkbox input");
                var $rows = $container.find("tr");

                if ($selectAll.is(":" + checkedAttr) && countChecked === 0) {
                    countChecked = $checkboxes.length;
                    $checkboxes.each(function (id, item) { item.checked = true; });
                    $rows.addClass(checkedRowClass);
                    unlockActionButtons();
                    changeSelectedItemsCounter();
                } else {
                    deselectAll();
                }

                return false;
            });

            jq("#deselectAll").off(clickEvent).on(clickEvent, function () {
                deselectAll();
            });
        }

        $container.off(changeEvent + ".ga").on(changeEvent + ".ga", ".checkbox input", onCheck);

        var multiSelector = settings.multiSelector;
        if (multiSelector) {
            jq.dropdownToggle(
                {
                    switcherSelector: "#groupActionContainer .menuActionSelectAll",
                    dropdownID: "groupActionMenuSelector",
                    anchorSelector: ".menuActionSelectAll",
                    position: "fixed",
                    simpleToggle: true
                });

            for (var j = multiSelector.length - 1; j >= 0; j--) {
                var groupSelectorActionsHandler = function (actionsItem) {
                    return function () {
                        deselectAll();
                        var line = settings.getLineByCondition(actionsItem.condition);
                        line.forEach(function ($line) {
                            var $checkBox = $line.find(".checkbox input[type='checkbox']");
                            $checkBox.prop("checked", "checked");
                            onCheck.call($checkBox);
                        });
                    };
                }(multiSelector[j]);

                jq("#" + multiSelector[j].id).on("click", groupSelectorActionsHandler);
            }
        }
    };

    function onCheck() {
        var $input = jq(this);
        var allCounts = $container.find(".checkbox input").length;

        if ($input.is(":" + checkedAttr)) {
            countChecked++;
            $input.parents(withCheckbox).addClass(checkedRowClass);

            if (countChecked === allCounts) {
                $selectAll.prop(checkedAttr, true);
            }

        } else {
            countChecked--;
            $input.parents(withCheckbox).removeClass(checkedRowClass);
            $selectAll.prop(checkedAttr, false);
        }

        if (countChecked > 0) {
            unlockActionButtons();
        } else {
            lockActionButtons();
        }

        $selectAll.prop(indeterminate, countChecked > 0 && !$selectAll.prop(checkedAttr));

        changeSelectedItemsCounter();
    }

    function deselectAll() {
        var $checkboxes = $container.find(".checkbox input"),
            $rows = $container.find("tr");

        countChecked = 0;
        $checkboxes.each(function (id, item) { item.checked = false; });
        $rows.removeClass(checkedRowClass);
        lockActionButtons();
        changeSelectedItemsCounter();
        $selectAll.prop(indeterminate, false);
        $selectAll.prop(checkedAttr, false);
    }

    function changeSelectedItemsCounter() {
        if (countChecked > 0) {
            $counterSelectedItems.show();
            $counterSelectedItems.find("span").text(countChecked + " " + ASC.Projects.Resources.ProjectsJSResource.GroupMenuSelectedItems);
        } else {
            $counterSelectedItems.hide();
        }
    };

    function unlockActionButtons() {
        var items = getCheckedItems();

        for (var j = currentActions.length; j--;) {
            var action = currentActions[j];
            if (items.every(action.checker)) {
                jq("#" + action.id).addClass(unlockAction);
            } else {
                jq("#" + action.id).removeClass(unlockAction);
            }
        }
    };

    function getCheckedItems() {
        var $checkboxes = $container.find(".checkbox input:checked").toArray();
        return $checkboxes.map(currentGetItemByCheckbox);
    }

    function lockActionButtons() {
        for (var j = currentActions.length; j --;) {
            var action = currentActions[j];
            jq("#" + action.id).removeClass(unlockAction);
        }
    };

    function hide() {
        if ($groupActionContainer) {
            $groupActionContainer.hide();
        }
    }

    return {
        init: init,
        hide: hide,
        deselectAll: deselectAll
    };
})();

ASC.Projects.StatusList = (function () {
    var clickEvent = "click",
        statusComboboxClass = ".changeStatusCombobox.canEdit",
        currentListStatusObjid,
        activeClass = "active";

    var currentSettings;

    var $statusListContainer, $commonListContainer;

    function init(settings, $clc) {
        if (!settings) return;

        currentSettings = settings;
        var panelId = "#statusChangePanel";

        $commonListContainer = $clc;

        jq(panelId).remove();
        $commonListContainer.append(jq.tmpl("projects_statusChangePanel", settings));
        $statusListContainer = jq(panelId);

        $commonListContainer.on(clickEvent, statusComboboxClass, show);

        for (var i = 0; i < settings.statuses.length; i++) {

            var handler = function(statusItemId) {
                return function() {
                    currentSettings.handler(currentListStatusObjid, statusItemId);
                };
            }(settings.statuses[i].id);

            jq("#statusItem_" + settings.statuses[i].id).on("click", handler);
        }

        jq('body')
            .off("click.body3")
            .on("click.body3", function() {
                 setTimeout(function() {
                      hide();
                 }, 1);
            });
    };


    function show() {
        var item = currentSettings.getItem(this);
        var isVisible = $statusListContainer.is(":visible");
        var $obj = jq(this);

        var objid = item.id,
            status = typeof item.status !== "undefined" ? item.status : item.paymentStatus,
            offset = $obj.offset();

        hide();

        if (currentListStatusObjid === objid && isVisible) {
            return undefined;
        }

        currentListStatusObjid = objid;

        $obj.addClass('selected');

        $statusListContainer.css(
        {
            left: offset.left,
            top: offset.top + 28
        });


        $statusListContainer.find('li').show().removeClass(activeClass);
        $statusListContainer.find('#statusItem_' + status).addClass(activeClass);
        if (status === 1) {
            $statusListContainer.find('.paused').hide();
        }

        $statusListContainer.show();
        return false;
    };

    function hide() {
        if ($statusListContainer) {
            $statusListContainer.hide();
        }
        $commonListContainer.find(statusComboboxClass).removeClass("selected");
    }

    function getById(id) {
        return currentSettings.statuses.find(function(item) {
            return id === item.id;
        });
    }

    return { init: init, getById: getById };
})();