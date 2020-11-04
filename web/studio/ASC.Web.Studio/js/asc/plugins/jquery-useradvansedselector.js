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

    var useradvancedSelector = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.useradvancedSelector.defaults, options);

        this.init();
    };

    useradvancedSelector.prototype = $.extend({}, $.fn.advancedSelector.Constructor.prototype, {
        constructor: useradvancedSelector,
        initCreationBlock: function (data) {
            var that = this,
                 opts = {},
                 itemsSimpleSelect = [],
                 groups = data.groups;

            for (var i = 0, length = groups.length; i < length; i++) {
                itemsSimpleSelect.push(
                    {
                        title: groups[i].title,
                        id: groups[i].id
                    }
                );
            }

            opts.newoptions =
                    [
                        { title: resources.SelectorType, type: "choice", tag: "type", items: [
                                        { type: "user", title: resources.SelectorUser },
                                        { type: "visitor", title: resources.SelectorVisitor }
                            ]
                        },
                        { title: resources.SelectorFirstName, type: "input", tag: "first-name" },
                        { title: resources.SelectorLastName, type: "input", tag: "last-name" },
                        { title: resources.SelectorEmail, type: "input", tag: "email" },
                        { title: resources.SelectorGroup, type: "select", tag: "group" }
                    ],
            opts.newbtn = resources.InviteButton;

            that.displayAddItemBlock.call(that, opts);
            that.initDataSimpleSelector.call(that, { tag: "group", items: itemsSimpleSelect });

            var $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block");

            if (that.options.withGuests) {
                teamlab.getQuotas({}, {
                    success: function (params, data) {
                        if (data.availableUsersCount == 0) {
                            $addPanel.find(".type select").val("visitor").attr("disabled", "disabled");
                        }
                    },
                    error: function (params, errors) { }
                });
            } else {
                $addPanel.find(".type").hide();
            }
        },

        initAdvSelectorData: function () {
            var that = this,
                data = [];

            var dataItems = window.UserManager.getAllUsers(!that.options.showDisabled);

            for (var dataItemId in dataItems) {
                if (!dataItems.hasOwnProperty(dataItemId)) continue;

                var dataItem = dataItems[dataItemId];
                
                if(!that.options.withGuests && dataItem.isVisitor)
                    continue;

                var newObj = {
                    title: dataItem.displayName,
                    id: dataItem.id,
                    status: dataItem.isPending || dataItem.isActivated === false ? ASC.Resources.Master.Resource.UserPending : "",
                    groups: window.GroupManager.getGroups(dataItem.groups)
                };

                data.push(newObj);
            }

            that.rewriteObjectItem.call(that, data.sort(SortData));

            // var   filter = {
            //        employeeStatus: 1,
            //        isAdministrator: that.options.isAdmin
            //    };
            //if (!that.options.withGuests) {
            //    filter.employeeType = 1;
            //}
            //teamlab.getProfilesByFilter({}, {
            //    before: function () {
            //        that.showLoaderListAdvSelector.call(that, 'items');
            //    },
            //    after: function () {
            //        that.hideLoaderListAdvSelector.call(that, 'items');
            //    },
            //    filter: filter,
            //    success: function (params, data) {
            //        that.rewriteObjectItem.call(that, data);
            //    },
            //    error: function (params, errors) {
            //        toastr.error(errors);
            //    }
            //});
        },

        initAdvSelectorGroupsData: function () {
            var that = this;

            that.rewriteObjectGroup.call(that, window.GroupManager.getAllGroups());

            if (that.options.isAdmin) {
                var groups = [];

                that.$groupsListSelector.find(".advanced-selector-list li").hide();
                that.items.forEach(function (e) {
                    groups = groups.concat(e.groups).unique();
                });

                groups.forEach(function (elem) {
                    that.$groupsListSelector.find(".advanced-selector-list li[data-id=" + elem.id + "]").show();
                });
            }

            //teamlab.getGroups({}, {
            //    before: function () {
            //        that.showLoaderListAdvSelector.call(that, 'groups');
            //    },
            //    after: function () {
            //        that.hideLoaderListAdvSelector.call(that, 'groups');
            //    },
            //    success: function (params, data) {
            //        that.rewriteObjectGroup.call(that, data);
            //        if (that.options.isAdmin) {
            //            var groups = [],
            //                dataIds = [];
            //            that.$groupsListSelector.find(".advanced-selector-list li").hide();
            //            that.items.forEach(function (e) {
            //                groups = groups.concat(e.groups).unique();
            //            });

            //            groups.forEach(function (elem) {
            //                that.$groupsListSelector.find(".advanced-selector-list li[data-id=" + elem.id + "]").show();
            //            });
            //        }
            //    },
            //    error: function (params, errors) {
            //        toastr.error(errors);
            //    }
            //});
        },

        rewriteObjectItem: function (data) {
            var that = this;

            that.items = data;

            that.$element.data('items', that.items);
            that.showItemsListAdvSelector.call(that);
        },

        createNewItemFn: function () {
            var that = this,
                $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block"),
                $btn = $addPanel.find(".advanced-selector-btn-add"),
                isError,
                isVisitor;

            if (that.options.withGuests) {
                switch ($addPanel.find(".type select").val()) {
                    case "user":
                        isVisitor = false;
                        break;
                    case "visitor":
                        isVisitor = true;
                        break;
                };
            } else {
                isVisitor = false;
            }

            var departmentId =  $addPanel.find(".group input").attr("data-id");
            var newUser = {
                isVisitor: isVisitor,
                firstname: $addPanel.find(".first-name input").val().trim(),
                lastname: $addPanel.find(".last-name input").val().trim(),
                email: $addPanel.find(".email input").val().trim(),
                department: (departmentId && departmentId.length) ? [departmentId] : []
            };

            if (!newUser.firstname) {
                that.showErrorField.call(that, { field: $addPanel.find(".first-name"), error: resources.ErrorEmptyUserFirstName });
                isError = true;
            }
            if (!newUser.lastname) {
                that.showErrorField.call(that, { field: $addPanel.find(".last-name"), error: resources.ErrorEmptyUserLastName });
                isError = true;
            }
            if (newUser.firstname && newUser.firstname.length > 64) {
                that.showErrorField.call(that, { field: $addPanel.find(".first-name"), error: resources.ErrorMesLongField64 });
                isError = true;
            }
            if (newUser.lastname && newUser.lastname.length > 64) {
                that.showErrorField.call(that, { field: $addPanel.find(".last-name"), error: resources.ErrorMesLongField64 });
                isError = true;
            }
            if (!jq.isValidEmail(newUser.email)) {
                that.showErrorField.call(that, { field: $addPanel.find(".email"), error: resources.ErrorNotCorrectEmail });
                isError = true;
            }
            if (!newUser.department.length && $addPanel.find(".group input").val()) {
                that.showErrorField.call(that, { field: $addPanel.find(".group"), error: resources.ErrorGroupNotExist });
                isError = true;
            }

            if (isError) {
                $addPanel.find(".error input").first().focus();
                return;
            }

            teamlab.getQuotas({}, {
                success: function (params, data) {
                    if (data.availableUsersCount == 0 && !newUser.isVisitor) {
                        that.showServerError.call(that, { field: $btn, error: resources.UserSelectorErrorLimitUsers + data.maxUsersCount });
                        return;
                    }

                    teamlab.addProfile({}, newUser, {
                        before: function () {
                            that.displayLoadingBtn.call(that, { btn: $btn, text: resources.LoadingProcessing });
                        },
                        success: function (params, profile) {
                            profile = this.__responses[0];

                            var newuser = {
                                id: profile.id,
                                title: profile.displayName,
                                isVisitor: profile.isVisitor,
                                status: ASC.Resources.Master.Resource.UserPending,
                                groups: []
                            };

                            var copy = Object.assign({}, profile);
                            copy.groups = (profile.groups || []).map(function (group) { return group.id; });
                            UserManager.addNewUser(copy);

                            toastr.success(resources.UserSelectorAddSuccess.format("<b>" + newuser.title + "</b>"));
                            that.actionsAfterCreateItem.call(that, { newitem: newuser, response: profile, nameProperty: "groups" });
                        },
                        error: function () {
                            that.showServerError.call(that, { field: $btn, error: this.__errors[0] });
                        },
                        after: function () {
                            that.hideLoadingBtn.call(that, $btn);
                        }
                    });
                },
                error: function (params, errors) { }
            });
        },

        addNewItemObj: function (item) {
            var newuser = {
                id: item.id,
                title: item.displayName,
                isVisitor: item.isVisitor,
                status: item.isPending || item.isActivated === false ? ASC.Resources.Master.Resource.UserPending : "",
                groups: []
            };
            this.actionsAfterCreateItem.call(this, { newitem: newuser, response: item, nameProperty: "groups" });
        },


    });

    $.fn.useradvancedSelector = function (option, val) {
        var selfargs = Array.prototype.slice.call(arguments, 1);
        return this.each(function () {
            var $this = $(this),
                data = $this.data('useradvancedSelector'),
                options = $.extend({},
                        $.fn.useradvancedSelector.defaults,
                        $this.data(),
                        typeof option == 'object' && option);
            if (!data) $this.data('useradvancedSelector', (data = new useradvancedSelector(this, options)));
            if (typeof option == 'string') data[option].apply(data, selfargs);
        });
  }
    $.fn.useradvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {
        showme: true,
        addtext: resources.UserSelectorAddText,
        noresults: resources.UserSelectorNoResults,
        noitems: resources.UserSelectorNoItems,
        nogroups: resources.UserSelectorNoGroups,
        emptylist: resources.UserSelectorEmptyList,
        isAdmin: false,
        withGuests: true,
        showDisabled: false,
        isInitializeItems: true
    });


})(jQuery);