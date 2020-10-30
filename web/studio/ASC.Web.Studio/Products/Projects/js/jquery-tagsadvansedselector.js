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
    var resources = ASC.Projects.Resources.ProjectsJSResource, teamlab = Teamlab, masterResource = ASC.Resources.Master.Resource;

    var tagsadvancedSelector = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.tagsadvancedSelector.defaults, options);

        this.init();
    };

    tagsadvancedSelector.prototype = $.extend({}, $.fn.advancedSelector.Constructor.prototype, {
        constructor: tagsadvancedSelector,
        initCreationBlock: function () {
            var that = this,
                opts = {
                    newoptions: [
                      { title: resources.TagsSelectorTitle, type: "input", tag: "title" }
                    ],
                    newbtn: masterResource.CreateButton
                };

            that.displayAddItemBlock.call(that, opts);
        },

        initAdvSelectorData: function () {
            var that = this;
            that.rewriteObjectItem.call(that, []);
        },

        rewriteObjectItem: function (data) {
            var that = this;

            that.items = data.map(function (item) {
                return { id: item.id, title: item.title, description: item.description, isPrivate: item.isPrivate, responsible: item.responsible, deadline: item.deadline };
            });
            that.items = that.items.sort(that.options.sortMethod || SortData);
            that.showItemsListAdvSelector.call(that);
        },

        createNewItemFn: function () {
            var that = this,
                $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block"),
                $btn = $addPanel.find(".advanced-selector-btn-add");

            var newTag = { data: $addPanel.find(".title input").val().trim() };

            if (!newTag.data) {
                that.showErrorField.call(that, { field: $addPanel.find(".title"), error: resources.TagsSelectorEmptyTitleError });
                $addPanel.find(".error input").first().focus();
                return;
            }
            
            teamlab.addPrjTag({}, newTag, {
                before: function () {
                    that.displayLoadingBtn.call(that, { btn: $btn, text: resources.LoadingProcessing });
                },
                error: function (params, errors) {
                    that.showServerError.call(that, { field: $btn, error: errors });
                },
                success: function (params, project) {
                    project = this.__responses[0];
                    var newproject = {
                        id: project.id,
                        title: project.title
                    }
                    that.actionsAfterCreateItem.call(that, { newitem: newproject, response: project });
                },
                after: function () {
                    that.hideLoadingBtn.call(that, $btn);
                }
            });
        }

    });


    $.fn.tagsadvancedSelector = function (option) {
        var selfargs = Array.prototype.slice.call(arguments, 1);
        return this.each(function () {
            var $this = $(this),
                data = $this.data('tagsadvancedSelector'),
                options = $.extend({},
                    $.fn.tagsadvancedSelector.defaults,
                    $this.data(),
                    typeof option == 'object' && option);
            if (!data) $this.data('tagsadvancedSelector', (data = new tagsadvancedSelector(this, options)));
            if (typeof option == 'string') data[option].apply(data, selfargs);
        });
    };

    $.fn.tagsadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {
        addtext: resources.TagsSelectorAddText,
        noresults: resources.TagsSelectorNoResult,
        noitems: resources.TagsSelectorNoItems,
        emptylist: resources.TagsSelectorEmptyList
    });

})(jQuery);