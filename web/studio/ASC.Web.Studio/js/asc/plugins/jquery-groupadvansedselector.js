/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/


(function ($, win, doc, body) {
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
                { title: ASC.Resources.Master.Resource.SelectorTitle, type: "input", tag: "title" },
                { title: ASC.Resources.Master.Resource.SelectorHead, type: "select", tag: "manager" }
            ];
            opts.newbtn = ASC.Resources.Master.Resource.CreateButton;
            that.displayAddItemBlock.call(that, opts);

            var filter = {
                employeeStatus: 1,
                employeeType: 1
            };

            Teamlab.getProfilesByFilter({}, {
                before: that.showLoaderSimpleSelector.call(that, "manager"),
                filter: filter,
                success: function (params, data) {
                    for (var i = 0, length = data.length; i < length; i++) {
                        if (data[i].id == Teamlab.profile.id) {
                            itemsSimpleSelect.unshift(
                                {
                                    title: ASC.Resources.Master.Resource.MeLabel,
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
            var that = this,
                data = ASC.Resources.Master.ApiResponses_Groups.response;

            that.rewriteObjectItem.call(that, data);

            //Teamlab.getGroups({}, {
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
                that.showErrorField.call(that, { field: $addPanel.find(".title"), error: ASC.Resources.Master.Resource.ErrorEmptyGroupTitle });
                isError = true;
            }
            if (newGroup.groupName && newGroup.groupName.length > 64) {
                that.showErrorField.call(that, { field: $addPanel.find(".title"), error: ASC.Resources.Master.Resource.ErrorMesLongField64 });
                isError = true;
            }
            if (newGroup.groupManager == "00000000-0000-0000-0000-000000000000" && $addPanel.find(".manager input").val().trim()) {
                that.showErrorField.call(that, { field: $addPanel.find(".manager"), error: ASC.Resources.Master.Resource.ErrorHeadNotExist });
                isError = true;
            }
            if (isError) {
                $addPanel.find(".error input").first().focus();
                return;
            }
            Teamlab.addGroup(null, newGroup, {
                before: function(){
                    that.displayLoadingBtn.call(that, { btn: $btn, text: ASC.Resources.Master.Resource.LoadingProcessing });
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
                    toastr.success(ASC.Resources.Master.Resource.GroupSelectorAddSuccess.format("<b>" + newgroup.title + "</b>"));
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
        addtext: ASC.Resources.Master.Resource.GroupSelectorAddText,
        noresults: ASC.Resources.Master.Resource.GroupSelectorNoResults,
        emptylist: ASC.Resources.Master.Resource.GroupSelectorEmptyList,
        witheveryone: false,
        withadmin: false,
        isInitializeItems: true
    });


})(jQuery, window, document, document.body);