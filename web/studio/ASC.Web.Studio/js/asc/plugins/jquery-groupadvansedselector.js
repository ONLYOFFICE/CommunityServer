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

    var groupadvancedSelector = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.groupadvancedSelector.defaults, options);

        this.init();
    };

    groupadvancedSelector.prototype = $.extend({}, $.fn.advancedSelector.Constructor.prototype, {
        constructor: groupadvancedSelector,
        initCreationBlock: function (data) {
            var that = this,
                opts = {},
                itemsSimpleSelect = [];

            opts.newoptions = [
                { title: resources.SelectorTitle, type: "input", tag: "title" },
                { title: resources.SelectorHead, type: "select", tag: "manager" }
            ];
            opts.newbtn = resources.CreateButton;
            that.displayAddItemBlock.call(that, opts);

            var filter = {
                employeeStatus: 1,
                employeeType: 1
            };

            teamlab.getSimpleProfilesByFilter({}, {
                before: that.showLoaderSimpleSelector.call(that, "manager"),
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
            var that = this;

            that.rewriteObjectItem.call(that, window.GroupManager.getAllGroups());

            //teamlab.getGroups({}, {
            //    before: function () {
            //        that.showLoaderListAdvSelector.call(that, 'items');
            //    },
            //    after: function () {
            //        that.hideLoaderListAdvSelector.call(that, 'items');
            //    },
            //    success: function (params, data) {
            //        that.rewriteObjectItem.call(that, data);
            //    },
            //    error: function (params, errors) {
            //        toastr.error(errors);
            //    }
            //});
             
        },

        rewriteObjectItem: function (data) {
            var that = this;
            that.items = [];

            for (var i = 0, length = data.length; i < length; i++) {
                var newObj = {};
                newObj.title = data[i].name;
                newObj.id = data[i].id;
                that.items.push(newObj);
            }

            that.items = that.items.sort(SortData);

            if (that.options.withadmin) {
                var withAdmin = ASC.Resources.Master.GroupSelector_WithGroupAdmin;
                that.items.unshift({
                    id: withAdmin.Id,
                    title: withAdmin.Name
                });
            }
            if (that.options.witheveryone) {
                var withEveryone = ASC.Resources.Master.GroupSelector_WithGroupEveryone;
                that.items.unshift({
                    id: withEveryone.Id,
                    title: withEveryone.Name
                });
            }
			that.$element.data('items', that.items);
            that.showItemsListAdvSelector.call(that);
        },
        createNewItemFn: function () {
            var that = this,
                $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block"),
                $btn = $addPanel.find(".advanced-selector-btn-add"),
                isError,
                managerId = $addPanel.find(".manager input").attr("data-id"),

                newGroup = {
                groupName: $addPanel.find(".title input").val().trim(),
                groupManager: (managerId && managerId.length) ? managerId : "00000000-0000-0000-0000-000000000000",
                groupId: null,
                members: []
            };
            if (!newGroup.groupName) {
                that.showErrorField.call(that, { field: $addPanel.find(".title"), error: resources.ErrorEmptyGroupTitle });
                isError = true;
            }
            if (newGroup.groupName && newGroup.groupName.length > 64) {
                that.showErrorField.call(that, { field: $addPanel.find(".title"), error: resources.ErrorMesLongField64 });
                isError = true;
            }
            if (newGroup.groupManager == "00000000-0000-0000-0000-000000000000" && $addPanel.find(".manager input").val().trim()) {
                that.showErrorField.call(that, { field: $addPanel.find(".manager"), error: resources.ErrorHeadNotExist });
                isError = true;
            }
            if (isError) {
                $addPanel.find(".error input").first().focus();
                return;
            }
            teamlab.addGroup(null, newGroup, {
                before: function(){
                    that.displayLoadingBtn.call(that, { btn: $btn, text: resources.LoadingProcessing });
                },
                after: function(){
                    that.hideLoadingBtn.call(that, $btn);
                },
                error: function (params, errors) {
                    that.showServerError.call(that, { field: $btn, error: this.__errors[0] });
                },
                success: function (params, group) {
                    group = this.__responses[0];
                    var newgroup = {
                        id: group.id,
                        title: group.name,
                        head: group.manager
                    }
                    toastr.success(resources.GroupSelectorAddSuccess.format("<b>" + Encoder.htmlEncode(newgroup.title) + "</b>"));
                    that.actionsAfterCreateItem.call(that, { newitem: newgroup, response: group });
                }
            })  
        },
        addNewItemObj: function (group) {
            var that = this,
                newgroup = {
                id: group.id,
                title: group.name,
                head: group.manager
            }
            that.actionsAfterCreateItem.call(that, { newitem: newgroup, response: group });
        }

    });



    $.fn.groupadvancedSelector = function (option,val) {
        return this.each(function () {
            var $this = $(this),
                data = $this.data('groupadvancedSelector'),
                options = $.extend({},
                        $.fn.groupadvancedSelector.defaults,
                        $this.data(),
                        typeof option == 'object' && option);
            if (!data) $this.data('groupadvancedSelector', (data = new groupadvancedSelector(this, options)));
            if (typeof option == 'string') data[option](val);
        });
    }
    $.fn.groupadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {
        addtext: resources.GroupSelectorAddText,
        noresults: resources.GroupSelectorNoResults,
        emptylist: resources.GroupSelectorEmptyList,
        witheveryone: false,
        withadmin: false,
        isInitializeItems: true
    });


})(jQuery);