/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
    var ResourceJS = ASC.Resources.Master.ResourceJS, teamlab = Teamlab;

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
                        { title: ResourceJS.SelectorType, type: "choice", tag: "type", items: [
                                        { type: "user", title: ResourceJS.SelectorUser },
                                        { type: "visitor", title: ResourceJS.SelectorVisitor }
                            ]
                        },
                        { title: ResourceJS.SelectorFirstName, type: "input", tag: "first-name" },
                        { title: ResourceJS.SelectorLastName, type: "input", tag: "last-name" },
                        { title: ResourceJS.SelectorEmail, type: "input", tag: "email" },
                        { title: ResourceJS.SelectorGroup, type: "select", tag: "group" }
                    ],
            opts.newbtn = ResourceJS.InviteButton;

            that.displayAddItemBlock.call(that, opts);
            that.initDataSimpleSelector.call(that, { tag: "group", items: itemsSimpleSelect });

            var $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block");

            if (that.options.withGuests) {
                teamlab.getQuotas({}, {
                    success: function (params, data) {
                        if (data.availableUsersCount == 0) {
                            $addPanel.find(".type select").val("visitor").prop("disabled", true);
                        }
                    },
                    error: function (params, errors) { }
                });
            } else {
                $addPanel.find(".type").hide();
            }
        },

        getTooltip: function (profile) {
            var tooltip = Encoder.htmlDecode(profile.displayName);
            if (profile.email) {
                tooltip += "\n" + profile.email;
            }
            if (profile.isPending || profile.isActivated === false) {
                tooltip += " (" + ASC.Resources.Master.ResourceJS.UserPending + ")";
            }
            return tooltip;
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
                    isVisitor: dataItem.isVisitor,
                    status: dataItem.isPending || dataItem.isActivated === false ? "pending" : "",
                    groups: window.GroupManager.getGroups(dataItem.groups),
                    avatarSmall: dataItem.avatarSmall,
                    tooltip: that.getTooltip(dataItem)
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

            that.rewriteObjectGroup.call(that, window.GroupManager.getGroupsArray());

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
                that.showErrorField.call(that, { field: $addPanel.find(".first-name"), error: ResourceJS.ErrorEmptyUserFirstName });
                isError = true;
            }
            if (!newUser.lastname) {
                that.showErrorField.call(that, { field: $addPanel.find(".last-name"), error: ResourceJS.ErrorEmptyUserLastName });
                isError = true;
            }
            if (newUser.firstname && newUser.firstname.length > 64) {
                that.showErrorField.call(that, { field: $addPanel.find(".first-name"), error: ResourceJS.ErrorMesLongField64 });
                isError = true;
            }
            if (newUser.lastname && newUser.lastname.length > 64) {
                that.showErrorField.call(that, { field: $addPanel.find(".last-name"), error: ResourceJS.ErrorMesLongField64 });
                isError = true;
            }
            if (!jq.isValidEmail(newUser.email)) {
                that.showErrorField.call(that, { field: $addPanel.find(".email"), error: ResourceJS.ErrorNotCorrectEmail });
                isError = true;
            }
            if (!newUser.department.length && $addPanel.find(".group input").val()) {
                that.showErrorField.call(that, { field: $addPanel.find(".group"), error: ResourceJS.ErrorGroupNotExist });
                isError = true;
            }

            if (isError) {
                $addPanel.find(".error input").first().trigger("focus");
                return;
            }

            teamlab.getQuotas({}, {
                success: function (params, data) {
                    if (data.availableUsersCount == 0 && !newUser.isVisitor) {
                        that.showServerError.call(that, { field: $btn, error: ResourceJS.UserSelectorErrorLimitUsers + " " + data.maxUsersCount });
                        return;
                    }

                    teamlab.addProfile({}, newUser, {
                        before: function () {
                            that.displayLoadingBtn.call(that, { btn: $btn, text: ResourceJS.LoadingProcessing });
                        },
                        success: function (params, profile) {
                            profile = this.__responses[0];

                            var newuser = {
                                id: profile.id,
                                title: profile.displayName,
                                isVisitor: profile.isVisitor,
                                isPending: true,
                                status: "pending",
                                groups: [],
                                avatarSmall: profile.avatarSmall,
                                tooltip: that.getTooltip(profile)
                            };

                            var copy = Object.assign({}, profile);
                            copy.groups = (profile.groups || []).map(function (group) { return group.id; });
                            UserManager.addNewUser(copy);

                            toastr.success(ResourceJS.UserSelectorAddSuccess.format("<b>" + newuser.title + "</b>"));
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
                status: item.isPending || item.isActivated === false ? "pending" : "",
                groups: [],
                avatarSmall: profile.avatarSmall,
                tooltip: that.getTooltip(profile)
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
        addtext: ResourceJS.UserSelectorAddText,
        noresults: ResourceJS.UserSelectorNoResults,
        noitems: ResourceJS.UserSelectorNoItems,
        nogroups: ResourceJS.UserSelectorNoGroups,
        emptylist: ResourceJS.UserSelectorEmptyList,
        isAdmin: false,
        withGuests: true,
        showDisabled: false,
        isInitializeItems: true
    });


})(jQuery);