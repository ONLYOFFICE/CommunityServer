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


ASC.Projects.Tab = function (moduleName, count, divID, $container, link, isVisibleSelector, emptyScreen, sublink) {
    this.title = moduleName;
    this.count = count;
    this.divID = divID;
    this.$container = $container;

    if (!link) {
        this.selected = false;
    } else {
        if (link.indexOf("#") === 0) {
            this.selected = location.href.toLowerCase().endsWith(link.toLowerCase()) || (sublink && location.hash.indexOf(sublink) == 0);
        } else {
            this.selected = location.href.toLowerCase().indexOf(link.toLowerCase()) > 0;
        }
    }

    this.link = link;
    this.sublink = sublink;
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

        var currentHash = location.hash.substring(1);

        if (currentHash === this.emptyScreen.button.hash && $addFirstElement.length > 0) {
            $addFirstElement.click();
        }

    } else if (this.$container) {
        $emptyScreenContainer.hide();
        this.$container.show();
    }

    if (this.link.indexOf("#") === 0) {
        if (!this.sublink || location.hash.indexOf(this.sublink) != 0) {
            ASC.Projects.Common.setHash(this.link);
        }
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

    function setStatuses(statuses) {
        description.status.items = statuses;

        return self;
    }

    function setCurrentStatus(currentStatus) {
        description.status.current = currentStatus;
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
        setStatuses: setStatuses,
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

ASC.Projects.ActionMenuItem = function(id, text, handler, classname, seporator) {
    this.id = id;
    this.text = text;
    this.handler = handler;
    this.classname = classname || null;
    this.seporator = !!seporator;
};

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

        $panel.on(clickEvent, '.project .value .link', function () {
            hide(false);
            filter.add('project', jq(this).attr("projectId"), ['tag', 'milestone', 'myprojects']);
        });

        $panel.on(clickEvent, '.milestone .value .link', function () {
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
                    var p = ASC.Projects.Common.getProjectById(projectId);
                    descriptionObj.projectStatus = typeof p !== "undefined" ? p.status : undefined;
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
                    var m = ASC.Projects.Common.getMilestoneById(obj.milestone.id);
                    descriptionObj.milestoneStatus = typeof m !== "undefined" ? m.status : undefined;
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
        return typeof prop !== 'undefined' && jq.trim(prop) !== "" && prop != null;
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
        disable = "disable",
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
            var action = currentActions[i];
            var $action = jq("#" + action.id);

            if (action.multi) {
                $action.on(clickEvent, function (event) {
                    if (!$action.hasClass(unlockAction)) {
                        event.stopPropagation();
                    }
                });

                jq.dropdownToggle(
                    {
                        switcherSelector: "#" + action.id,
                        dropdownID: action.id + "Selector",
                        position: "fixed",
                        simpleToggle: true
                    });

                for (var k = 0; k < action.multi.length; k++) {
                    addActionHandler(action.multi[k], function () {
                        return !$action.hasClass(unlockAction) && jq(this).find("a").hasClass(disable);
                    });
                }

            } else {
                addActionHandler(action, function() {
                    return !this.classList.contains(unlockAction);
                });
            }
        }

        $counterSelectedItems = $groupActionMenu ? $groupActionMenu.find(".menu-action-checked-count") : [];

        if ($groupActionMenu) {
            var options = {
                menuSelector: "#groupActionMenu",
                menuAnchorSelector: "#selectAll",
                menuSpacerSelector: "#groupActionContainer .header-menu-spacer",
                userFuncInTop: function () { $groupActionMenu.find(menuActionOnTopClass).hide(); },
                userFuncNotInTop: function () { $groupActionMenu.find(menuActionOnTopClass).show(); }
            };
            ScrolledGroupMenu.init(options);

            $selectAll = jq("#selectAll");

            $selectAll.off(changeEvent).on(changeEvent, function () {
                jq("#groupActionMenuSelector").hide();
                var $checkboxes = $container.find(".checkbox input");
                var $rows = $container.find("tr");

                if ($selectAll.is(":" + checkedAttr) && !$selectAll.is(":" + indeterminate) && countChecked === 0) {
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

    function addActionHandler(action, isDisabled) {
        var actionsHandler = function (actionsItem) {
            return function () {
                if (isDisabled.call(this)) return;
                actionsItem.handler(getCheckedItems().map(function (item) { return item.id; }));
            };
        }(action);

        jq("#" + action.id).on("click", actionsHandler);
    }

    function onCheck() {
        var $input = jq(this);
        var allCounts = $container.find(".checkbox input").length;

        if ($input.is(":" + checkedAttr)) {
            countChecked++;
            $input.parents(withCheckbox).addClass(checkedRowClass);

        } else {
            countChecked--;
            $input.parents(withCheckbox).removeClass(checkedRowClass);
        }

        if (countChecked > 0) {
            unlockActionButtons();
        } else {
            lockActionButtons();
        }

        $selectAll.prop(checkedAttr, countChecked > 0);
        $selectAll.prop(indeterminate, countChecked > 0 && countChecked < allCounts);

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
            var unlock;

            if (action.multi) {
                jq("#" + action.id + "Selector a").removeClass(disable);

                for(var k = 0; k < action.multi.length; k++) {
                    var multiAction = action.multi[k];
                    var multiActionChecked = items.every(multiAction.checker);
                    var $item = jq("#" + multiAction.id + " a");
                    if(multiActionChecked) {
                        unlock = true;
                    } else {
                        $item.addClass(disable);
                    }
                }

            } else {
                unlock = items.every(action.checker);
            }

            if (unlock) {
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

    var currentSettings, profileId;

    var $statusListContainer, $commonListContainer;

    function init(settings, $clc) {
        if (!settings) return;
        
        profileId = Teamlab.profile.id;

        currentSettings = settings;
        var panelId = "#statusChangePanel";

        $commonListContainer = $clc;

        jq(panelId).remove();
        $commonListContainer.append(jq.tmpl("projects_statusChangePanel", settings));
        $statusListContainer = jq(panelId);

        $commonListContainer.on(clickEvent, statusComboboxClass, show);

        for (var i = 0; i < settings.statuses.length; i++) {

            var handler = function (statusItemId, statusId) {
                return function () {
                    if (jq(this).hasClass(activeClass)) return;
                    currentSettings.handler(currentListStatusObjid, statusItemId, statusId);
                };
            }(settings.statuses[i].id, settings.statuses[i].statusType);

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

        if (status < 0) {
            status = -status;
        }

        $statusListContainer.find('#statusItem_' + getByData(item).id).addClass(activeClass);
        if (status === 1) {
            $statusListContainer.find('.paused').hide();
        }

        var project = typeof (item.projectId) !== "undefined" ? ASC.Projects.Master.Projects.find(function (p) { return p.id === item.projectId; }) : item;

        if (!project) {
            project = ASC.Projects.Common.getProjectByIdFromCache(item.projectId);
        }

        if (item.createdBy.id !== profileId && !ASC.Projects.Master.IsModuleAdmin && project.responsibleId !== profileId) {
            var statuses = currentSettings.statuses;
            for(var j = 0; j < statuses.length; j++) {
                var statusJ = statuses[j];
                if(statusJ.available === false) {
                    $statusListContainer.find("." + statusJ.cssClass).hide();
                }
            }
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

    function getDefaultById(id) {
        return currentSettings.statuses.find(function(item) {
            return item.statusType === id && item.isDefault;
        });
    }

    function getByData(data) {
        var id = typeof data.customTaskStatus !== 'undefined' ? data.customTaskStatus : data.status;
        var result = getById(id);

        return result || getDefaultById(data.status);
    }

    return { init: init, getById: getById, getByData: getByData };
})();