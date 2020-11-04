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


(function ($) {
    var resources = ASC.Resources.Master.Resource, teamlab = Teamlab;

    var projectadvancedSelector = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.projectadvancedSelector.defaults, options);

        this.init();
    };

    projectadvancedSelector.prototype = $.extend({}, $.fn.advancedSelector.Constructor.prototype, {
        constructor: projectadvancedSelector,
        initCreationBlock: function (data) {
            var that = this,
                opts = {},
                itemsSimpleSelect = [];

            opts.newoptions = [
                      { title: resources.SelectorTitle, type: "input", tag: "title" },
                      { title: resources.SelectorManager, type: "select", tag: "manager" }
            ];
            opts.newbtn = resources.CreateButton;

            that.displayAddItemBlock.call(that, opts);

            var filter = {
                employeeStatus: 1,
                employeeType: 1
            };

            teamlab.getSimpleProfilesByFilter({}, {
                before: function () {
                    that.showLoaderSimpleSelector.call(that, "manager");
                },
                filter: filter,
                success: function (params, data) {
                    for (var i = 0, length = data.length; i < length; i++) {
                        if (data[i].id == teamlab.profile.id) {
                            itemsSimpleSelect.unshift(
                                {
                                    title: resources.MeLabel,
                                    id: data[i].id
                                }
                            );
                        }
                        else {
                            itemsSimpleSelect.push(
                                {
                                    title: data[i].displayName,
                                    id: data[i].id
                                }
                            );
                        }
                    }
                    that.initDataSimpleSelector.call(that, { tag: "manager", items: itemsSimpleSelect });
                    that.hideLoaderSimpleSelector.call(that, "manager");
                },

                error: function (params, errors) {
                    toastr.error(errors);
                }
            });
        },

        initAdvSelectorData: function () {

        },
        
        rewriteObjectItem: function (data) {
            var that = this;
            
            that.items = data.map(function(item) {
                return { id: item.id, title: item.title, description: item.description, isPrivate: item.isPrivate, responsible: item.responsible, deadline: item.deadline };
            });
            that.items = that.items.sort(that.options.sortMethod || SortData);
            that.showItemsListAdvSelector.call(that);
        },

        createNewItemFn: function () {
            var that = this,
                $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block"),
                $btn = $addPanel.find(".advanced-selector-btn-add"),
                isError;

            var newProject = {
                title: $addPanel.find(".title input").val().trim(),
                responsibleid: $addPanel.find(".manager input").attr("data-id"),
                notify: false,
                description: "",
                tags: "",
                'private': false
            };

            if (!newProject.title) {
                that.showErrorField.call(that, { field: $addPanel.find(".title"), error: resources.ProjectSelectorEmptyTitleError });
                isError = true;
            }
            if (!newProject.responsibleid) {
                that.showErrorField.call(that, { field: $addPanel.find(".manager"), error: resources.ProjectSelectorEmptyManagerError });
                isError = true;
            }
            if (!newProject.responsibleid && $addPanel.find(".manager input").val()) {
                that.showErrorField.call(that, { field: $addPanel.find(".manager"), error: resources.ProjectSelectorNotPersonError });
                isError = true;
            }
            if (isError) {
                $addPanel.find(".error input").first().focus();
                return;
            }
            teamlab.addPrjProject({}, newProject, {
                before: function() {
                    that.displayLoadingBtn.call(that, { btn: $btn, text: resources.LoadingProcessing });
                },
                error: function(params, errors) {
                    that.showServerError.call(that, { field: $btn, error: errors });
                },
                success: function(params, project) {
                    project = this.__responses[0];
                    var newproject = {
                        id: project.id,
                        title: project.title
                    }
                    that.actionsAfterCreateItem.call(that, { newitem: newproject, response: project });
                },
                after: function() {
                    that.hideLoadingBtn.call(that, $btn);
                }
            });
        }

    });


    $.fn.projectadvancedSelector = function (option) {
        var selfargs = Array.prototype.slice.call(arguments, 1);
        return this.each(function() {
            var $this = $(this),
                data = $this.data('projectadvancedSelector'),
                options = $.extend({},
                    $.fn.projectadvancedSelector.defaults,
                    $this.data(),
                    typeof option == 'object' && option);
            if (!data) $this.data('projectadvancedSelector', (data = new projectadvancedSelector(this, options)));
            if (typeof option == 'string') data[option].apply(data, selfargs);
        });
    };
    
    $.fn.projectadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {
        showme: true,
        addtext: resources.ProjectSelectorAddText,
        noresults: resources.ProjectSelectorNoResult,
        noitems: resources.ProjectSelectorNoItems,
        emptylist: resources.ProjectSelectorEmptyList
    });

})(jQuery);