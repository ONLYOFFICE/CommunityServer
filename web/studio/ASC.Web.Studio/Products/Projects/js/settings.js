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


ASC.Projects.SettingsManager = (function () {
    var teamlab = Teamlab,
        projects = ASC.Projects,
        common,
        defResources = projects.Resources.Statuses,
        masterResource = ASC.Resources.Master.Resource,
        fileSelector,
        folder,
        statuses,
        currentModule,
        modules,
        baseFolder = ASC.Files.Constants.FOLDER_ID_MY_FILES,
        resources = projects.Resources,
        commonResource = resources.CommonResource,
        projectsFilterResource = resources.ProjectsFilterResource;

    var $settings,
        $folderSelectorBox,
        $box,
        $button;
    var disabled = "disabled", checked = "checked";

    function init() {
        $settings = jq("#settings");
        common = projects.Common;
        if (location.hash === "#status") {
            if (!projects.Master.IsModuleAdmin) return;

            teamlab.getPrjStatuses({
                success: function (params, data) {
                    statuses = data.concat([]);
                    data = [
                        getCustomStatus(1, projectsFilterResource.StatusOpenTask),
                        getCustomStatus(2, projectsFilterResource.StatusClosedTask)
                    ];

                    jq("#settings").html(jq.tmpl("projects_settings_custom_statuses_box", { customStatuses: data }));

                    initStatusSettings();
                }
            });
        } else {
            teamlab.getPrjSettings({
                success: function (params, data) {
                    modules = projects.Resources.StartModules;
                    currentModule = modules.find(function (item) {
                        return item.StartModuleType === data.startModuleType;
                    }) || modules[0];
                    data.startModuleTitle = currentModule.Title;

                    jq("#settings").html(jq.tmpl("projects_settings", data));

                    initCommonSettings();
                    initPersonalSettings(data);
                }
            });
        }
    }

    function getCustomStatus(id, title) {
        if (!statuses.find(function (item) { return item.isDefault; })) {
            statuses.push(defResources.find(filterByStatus(id)));
        }

        return {
            id: id,
            title: title,
            state: getState(id),
            sub: getSub(id)
        };
    }

    function getSub(id) {
        return statuses
            .filter(filterByStatus(id))
            .sort(function (a, b) {
                if (a.order < b.order) {
                    return -1;
                } else if (a.order > b.order) {
                    return 1;
                }
                return 0;
            });
    }

    function getState(id) {
        var sub = getSub(id);

        function red(prop) {
            return function (accumulator, currentValue) {
                return accumulator && currentValue[prop];
            };
        }

        var on = !sub.filter(function (item) {
            return item.id > 0;
        }).reduce(red("available"), true);
        var can = sub.reduce(red("canChangeAvailable"), true);
        var enabled = can && sub.some(function (item) {
            return item.id > 0;
        });

        return {
            can: can,
            enabled: enabled,
            on: on
        };
    }

    function updateState(statusType) {
        var state = getState(statusType);

        if (!state.can) return;

        var $input = jq(".custom-status[data-type=" + statusType + "] input");

        if (state.enabled) {
            $input.removeAttr(disabled);
        } else {
            $input.attr(disabled, disabled);
            $input[0].checked = false;
        }
        if (state.on) {
            $input.attr(checked, checked);
        } else {
            $input.removeAttr(checked);
        }
    }

    function initCommonSettings() {
        var $cbxEnableSettings = jq("#cbxEnableSettings"),
            $cbxHideSettings = jq("#cbxHideSettings");

        $cbxEnableSettings.on("change", function () {
            updateSettings({ everebodyCanCreate: $cbxEnableSettings.is(":checked") });
        });

        $cbxHideSettings.on("change", function () {
            updateSettings({ hideEntitiesInPausedProjects: $cbxHideSettings.is(":checked") });
        });
    }

    function updateSettings(data, callback) {
        teamlab.updatePrjSettings(data,
        {
            success: function() {
                common.displayInfoPanel(masterResource.ChangesSuccessfullyAppliedMsg);

                if (typeof callback === "function") {
                    callback();
                }
            },
            error: function() {
                common.displayInfoPanel(masterResource.CommonJSErrorMsg, true);
            }
        });
    }

    function initStatusSettings() {
        jq("#CommonListContainer").show();
        ASC.Projects.Base.initActionPanel(showEntityMenu, jq(".custom-status"));

        var $newStatus = $settings.find("#newStatus");
        $newStatus.on("click", function () {
            var newStatus = Object.assign({}, defResources[0]);
            newStatus.id = 0;
            newStatus.isDefault = false;
            newStatus.statusType = 1;
            saEditHandler(newStatus);
        });

        var $checkAvailable = $settings.find(".on-off-checkbox");
        $checkAvailable.on("change", function () {
            var $self = jq(this);
            var statusType = +$self.parents(".custom-status").attr("data-type");
            var subStatuses = statuses.filter(filterByStatus(statusType));

            for (var i = 0; i < subStatuses.length; i++) {
                subStatuses[i].available = !$self.is(":checked");
            }

            teamlab.updatePrjStatuses(subStatuses,
            {
                success: function (params, data) {
                    for (var i = 0; i < data.length; i++) {
                        var dataI = data[i];
                        onUpdate(subStatuses[i].id, null, dataI);
                    }
                    common.displayInfoPanel(masterResource.OperationSuccededMsg);
                }
            });
        });

        jq(".custom-status").sortable({
            cursor: "move",
            handle: '.sort_drag_handle',
            items: '.grid-page',
            update: function () {
                var $self = jq(this);
                var $items = $self.find(".grid-page");
                var count = $items.length;
                var statusType;
                for (var i = 0; i < count; i++) {
                    var $item = jq($items[i]);
                    var status = statuses.find(filterById(+$item.attr("data-id")));
                    if (status) {
                        status.order = i + 1;
                        statusType = status.statusType;
                    }
                }
                var forUpdate = statuses.filter(filterByStatus(statusType));
                teamlab.updatePrjStatuses(forUpdate,
                {
                    success: function(params, data) {
                        for (var i = 0; i < data.length; i++) {
                            var dataI = data[i];
                            onUpdate(forUpdate[i].id, null, dataI);
                        }
                    }
                });
            }
        });
    }

    function initPersonalSettings(data) {
        var $startModule = jq("#startModule");

        $startModule.advancedSelector({
            height: 26 * 4, //magic: itemsCount*itemHeight
            itemsSelectedIds: [currentModule.StartModuleType],
            onechosen: true,
            showSearch: false,
            itemsChoose: modules.map(function (item) { return { id: item.StartModuleType, title: item.Title } }),
            sortMethod: function () { return 0; }
        })
        .on("showList",
            function (event, item) {
                $startModule.html(item.title).attr("title", item.title);
                updateSettings({ startModule: item.id });
            });

        folder = data.folderId;

        initFolderSelector();
    }

    function initFolderSelector() {
        $folderSelectorBox = jq(".folderSelectorBox");
        $box = $folderSelectorBox.find("input");
        $button = $folderSelectorBox.find(".button");

        $button.on("click", showFolderPop);

        jq('#fileSelectorTree > ul > li.tree-node:not([data-id=\'' + baseFolder + '\'])').remove();

        fileSelector = ASC.Files.FileSelector;
        fileSelector.onInit = function () {
            fileSelector.toggleThirdParty(true);

            fileSelector.onThirdPartyTreeCreated = function () {
                $('*:not(.disabled) > .settings-block .thirdPartyStorageSelectorBox')
                    .removeAttr('title')
                    .removeClass('disabled')
                    .find(':radio')
                    .attr('disabled', false);
            };
            fileSelector.createThirdPartyTree();
        };

        fileSelector.onSubmit = function (folderId) {
            displayFolderPath(folderId);
            updateSettings({ folderId: folderId });
        };

        fileSelector.setTitle(masterResource.SelectFolder);

        displayFolderPath();
    }
    
    function displayFolderPath(folderId) {
        if (typeof folderId != "undefined") {
            folder = folderId;
        }

        teamlab.getFolderPath(folder,
        {
            success: function (params, data) {
                var path = data.map(function (item) { return item.title; }),
                    pathTitle = path.join(' > ');
                $box.val(pathTitle);
            }
        });
    }

    function showFolderPop() {
        if ($button.is('.disable')) {
            return;
        }

        fileSelector.openDialog(folder, true, false);
    }

    function showEntityMenu(selectedActionCombobox) {
        var id = +selectedActionCombobox.parent().attr("data-id");
        var status = statuses.find(filterById(id));

        var ActionMenuItem = projects.ActionMenuItem;
        var menuItems = [
            new ActionMenuItem("status_edit", commonResource.Edit, saEditHandler.bind(null, status), "edit")
        ];

        if (status.id > 0) {
            menuItems.push(new ActionMenuItem("status_delete", commonResource.Delete, saDeleteHandler.bind(null, status), "delete"));
        }

        return { menuItems: menuItems };
    }

    function saEditHandler(status) {
        var newStatus = Object.assign({}, status);
        newStatus.category = defResources.find(function (item) { return item.statusType === newStatus.statusType }).title;

        var $commonPopupContainer = jq("#commonPopupContainer");
        $commonPopupContainer.addClass("editStatus");
        $commonPopupContainer.html(jq.tmpl("common_containerTmpl",
        {
            options: {
                PopupContainerCssClass: "popupContainerClass",
                OnCancelButtonClick: "PopupKeyUpActionProvider.CloseDialog();",
                IsPopup: true
            },
            header: {
                data: { title: status.id === 0 ? commonResource.CustomStatusNew : commonResource.Edit },
                title: "projects_common_popup_header"
            }
            ,
            body: {
                data: newStatus,
                title: "projects_settings_status_edit"
            }
        }));

        PopupKeyUpActionProvider.EnterAction = "jq('.commonPopupContent .blue').click();";

        StudioBlockUIManager.blockUI($commonPopupContainer, 400);

        var clickEvent = "click", background = "background-color", src = "src";

        $commonPopupContainer.off(clickEvent)
            .on(clickEvent, ".blue", function () { jq.unblockUI(); })
            .on(clickEvent, ".gray", function () { jq.unblockUI(); });

        var $colorPanel = $commonPopupContainer.find("#popup_colorPanel"),
            $currentColor = $commonPopupContainer.find("#changeColor div"),
            $iconPanel = $commonPopupContainer.find("#popup_iconPanel"),
            $currentIcon = $commonPopupContainer.find("#changeIcon img"),
            $uploadButton = $commonPopupContainer.find("#uploadButton"),
            $uploadButtonInput = $uploadButton.siblings(),
            $categoryPanel = $commonPopupContainer.find("#popup_categoryPanel"),
            $changeCategory = $commonPopupContainer.find("#changeCategory");

        jq.dropdownToggle({
            dropdownID: "popup_iconPanel",
            switcherSelector: "#changeIcon",
            position: "fixed",
            addTop: 2,
            addLeft: $currentIcon.width()
        });

        jq.dropdownToggle({
            dropdownID: "popup_colorPanel",
            switcherSelector: "#changeColor",
            position: "fixed",
            addTop: 2,
            addLeft: $currentColor.width()
        });

        jq.dropdownToggle({
            dropdownID: "popup_categoryPanel",
            switcherSelector: "#changeCategory",
            position: "fixed",
            addTop: 2
        });


        $colorPanel.on(clickEvent, "div > div", function () {
            var color = hexc(jq(this).css(background));
            newStatus.color = color;
            applyImage();
            $currentColor.css(background, color);
            $colorPanel.hide();
        });

        $iconPanel.on(clickEvent, "img", function () {
            var data = jq(this).attr(src);
            newStatus.image = data.split(',')[1];
            applyImage();
            $iconPanel.hide();
        });

        $categoryPanel.on(clickEvent, "div > div", function () {
            var $self = jq(this);
            var newStatusTitle = $self.text();
            newStatus.statusType = +$self.attr("data-id");
            $changeCategory.text(newStatusTitle);
            $categoryPanel.hide();
        });

        function applyImage() {
            var re = /fill([=|:])(\"#\w+\")/g;
            var image = window.atob(newStatus.image);
            image = image.replace(re, jq.format('fill$1"{0}"', newStatus.color));
            newStatus.image = window.btoa(image);

            $currentIcon.attr(src, jq.format("data:{0};base64,{1}", newStatus.imageType, newStatus.image));
        }

        $uploadButton.on(clickEvent, function (e) {
            $uploadButtonInput.click();
            e.stopPropagation();
        });

        $uploadButtonInput.on("change", function (evt) {
            var files = evt.target.files;
            var f = files[0];
            var reader = new FileReader();

            reader.onload = function (e) {
                newStatus.image = window.btoa(e.target.result);
                applyImage();
                $iconPanel.hide();
            };

            reader.readAsText(f);
        });

        var $buttonContainer = $commonPopupContainer.find(".middle-button-container");
        $buttonContainer.on(clickEvent, ".blue", function (e) {
            var newTitle = $commonPopupContainer.find(".textEdit").val();
            var $titlePanel = $commonPopupContainer.find(".requiredField");
            var requiredFieldError = "requiredFieldError";

            if (!newTitle) {
                $titlePanel.addClass(requiredFieldError);
                e.stopPropagation();
                return;
            } else {
                $titlePanel.removeClass(requiredFieldError);
            }

            newStatus.title = newTitle;
            newStatus.description = $commonPopupContainer.find("textarea").val();

            if (newStatus.id === 0) {
                teamlab.addPrjStatus(newStatus, { success: onAdd });
            } else {
                teamlab.updatePrjStatus(newStatus, { success: onUpdate.bind(null, newStatus.id) });
            }
        });
    }

    function saDeleteHandler(forDelete) {
        statuses = statuses.filter(function(item) {
            return item.id !== forDelete.id;
        });

        teamlab.removePrjStatus(forDelete.id, { success: onRemove.bind(null, forDelete) });
    }

    function onAdd(params, data) {
        var htmlData = jq.tmpl("projects_settings_custom_status", data);
        jq(".custom-status[data-type=" + data.statusType + "] .settings-box:last").append(htmlData);
        statuses.push(data);

        updateState(data.statusType);
    }

    function onUpdate(oldId, params, data) {
        var oldStatus = statuses.find(filterById(oldId));
        if (oldStatus.statusType !== data.statusType) {
            onRemove(oldStatus);
            onAdd(null, data);
        }
        else {
            var htmlData = jq.tmpl("projects_settings_custom_status", data);
            jq(".grid-page[data-id=" + oldId + "]").replaceWith(htmlData);
            if (oldId < 0) {
                statuses = statuses.filter(filterNotById(oldId));
                statuses.push(data);
            } else {
                Object.assign(oldStatus, data);
            }

            updateState(data.statusType);
        }
        
    }

    function onRemove(forDelete) {
        statuses = statuses.filter(filterNotById(forDelete.id));
        
        if (forDelete.isDefault) {
            var newDefresource = defResources.find(filterByStatus(forDelete.statusType));
            statuses.push(newDefresource);
            var htmlData = jq.tmpl("projects_settings_custom_status", newDefresource);
            jq(".grid-page[data-id=" + forDelete.id + "]").replaceWith(htmlData);
        } else {
            jq(".grid-page[data-id=" + forDelete.id + "]").remove();
        }

        updateState(forDelete.statusType);
    }

    function filterById(id) {
        return function (item) { return item.id === id; };
    }

    function filterNotById(id) {
        return function (item) { return item.id !== id; };
    }

    function filterByStatus(statusType) {
        return function (item) { return item.statusType === statusType; };
    }

    function hexc(colorval) {
        var parts = colorval.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/);
        delete (parts[0]);
        for (var i = 1; i <= 3; ++i) {
            parts[i] = parseInt(parts[i]).toString(16);
            if (parts[i].length === 1) parts[i] = '0' + parts[i];
        }
        return '#' + parts.join('');
    }

    return {
        init: init
    };
})(jQuery);