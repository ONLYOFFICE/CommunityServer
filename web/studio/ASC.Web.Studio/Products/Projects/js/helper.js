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

        if (currentHash == this.emptyScreen.button.hash && $addFirstElement.length > 0) {
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
        $projectInfoContainer.find("#essenceTitle").attr("title", title).val(title);
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

    function init($commonListContainer, filter) {
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
        return typeof prop != 'undefined' && jq.trim(prop) != "" && prop != null;
    }

    function showDescPanelByObject($targetObject) {
        var offset = $targetObject.offset();
        if (!offset.top && !offset.left) return;
        $panel.css({ left: offset.left, top: offset.top + $targetObject.parent().height() / 2 });
        $panel.show();
    };

    return {
        init: init,
        show: show,
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