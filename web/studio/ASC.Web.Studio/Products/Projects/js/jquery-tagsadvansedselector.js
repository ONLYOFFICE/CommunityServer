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