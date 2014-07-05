/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/


(function ($, win, doc, body) {

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
                        { title: ASC.Resources.Master.Resource.SelectorType, type: "choice", tag: "type", items: [
                                        { type: "user", title: ASC.Resources.Master.Resource.SelectorUser },
                                        { type: "visitor", title: ASC.Resources.Master.Resource.SelectorVisitor }
                            ]
                        },
                        { title: ASC.Resources.Master.Resource.SelectorFirstName, type: "input", tag: "first-name" },
                        { title: ASC.Resources.Master.Resource.SelectorLastName, type: "input", tag: "last-name" },
                        { title: ASC.Resources.Master.Resource.SelectorEmail, type: "input", tag: "email" },
                        { title: ASC.Resources.Master.Resource.SelectorGroup, type: "select", tag: "group" }
                    ],
            opts.newbtn = ASC.Resources.Master.Resource.InviteButton;

            that.displayAddItemBlock.call(that, opts);
            that.initDataSimpleSelector.call(that, { tag: "group", items: itemsSimpleSelect });

            var $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block");

            if (that.options.withGuests) {
                Teamlab.getQuotas({}, {
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
                data = ASC.Resources.Master.ApiResponses_Profiles.response;
            if (!that.options.withGuests) {
                data = $.grep(data, function (el) { return el.isVisitor == false });
            }
            that.rewriteObjectItem.call(that, data);

            // var   filter = {
            //        employeeStatus: 1,
            //        isAdministrator: that.options.isAdmin
            //    };
            //if (!that.options.withGuests) {
            //    filter.employeeType = 1;
            //}
            //Teamlab.getProfilesByFilter({}, {
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
            var that = this,
                data = ASC.Resources.Master.ApiResponses_Groups.response;

            that.rewriteObjectGroup.call(that, data);
            if (that.options.isAdmin) {
                var groups = [],
                    dataIds = [];
                that.$groupsListSelector.find(".advanced-selector-list li").hide();
                that.items.forEach(function (e) {
                    groups = groups.concat(e.groups).unique();
                });

                groups.forEach(function (elem) {
                    that.$groupsListSelector.find(".advanced-selector-list li[data-id=" + elem.id + "]").show();
                });
            }

            //Teamlab.getGroups({}, {
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
            that.items = [];

            for (var i = 0, length = data.length; i < length; i++) {
                var newObj = {};
                newObj.title = data[i].displayName || data[i].title;
                newObj.id = data[i].id;
                newObj.isVisitor = data[i].isVisitor;
                if (data[i].hasOwnProperty("isPending")) {
                    newObj.status = data[i].isPending ? "pending" : "";
                }
                if (data[i].hasOwnProperty("groups")) {
                    newObj.groups = data[i].groups;
                    if (data[i].groups && data[i].groups.length && !data[i].groups[0].id) {
                        newObj.groups.map(function (el) {
                            el.id = el.ID;
                        })
                    }
                }
                that.items.push(newObj);
            }

            that.items = that.items.sort(SortData);
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
                        break
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
                that.showErrorField.call(that, { field: $addPanel.find(".first-name"), error: ASC.Resources.Master.Resource.ErrorEmptyUserFirstName });
                isError = true;
            }
            if (!newUser.lastname) {
                that.showErrorField.call(that, { field: $addPanel.find(".last-name"), error: ASC.Resources.Master.Resource.ErrorEmptyUserLastName });
                isError = true;
            }
            if (newUser.firstname && newUser.firstname.length > 64) {
                that.showErrorField.call(that, { field: $addPanel.find(".first-name"), error: ASC.Resources.Master.Resource.ErrorMesLongField64 });
                isError = true;
            }
            if (newUser.lastname && newUser.lastname.length > 64) {
                that.showErrorField.call(that, { field: $addPanel.find(".last-name"), error: ASC.Resources.Master.Resource.ErrorMesLongField64 });
                isError = true;
            }
            if (!jq.isValidEmail(newUser.email)) {
                that.showErrorField.call(that, { field: $addPanel.find(".email"), error: ASC.Resources.Master.Resource.ErrorNotCorrectEmail });
                isError = true;
            }
            if (!newUser.department.length && $addPanel.find(".group input").val()) {
                that.showErrorField.call(that, { field: $addPanel.find(".group"), error: ASC.Resources.Master.Resource.ErrorGroupNotExist });
                isError = true;
            }

            if (isError) {
                $addPanel.find(".error input").first().focus();
                return;
            }

            Teamlab.getQuotas({}, {
                success: function (params, data) {
                    if (data.availableUsersCount == 0 && !newUser.isVisitor) {
                        that.showServerError.call(that, { field: $btn, error: ASC.Resources.Master.Resource.UserSelectorErrorLimitUsers + data.maxUsersCount });
                        return;
                    }

                    Teamlab.addProfile({}, newUser, {
                        before: function () {
                            that.displayLoadingBtn.call(that, { btn: $btn, text: ASC.Resources.Master.Resource.LoadingProcessing });
                        },
                        success: function (params, profile) {
                            profile = this.__responses[0];

                            var newuser = {
                                id: profile.id,
                                title: profile.displayName,
                                isVisitor: profile.isVisitor,
                                status: "pending",
                                groups: []
                            };

                            toastr.success(ASC.Resources.Master.Resource.UserSelectorAddSuccess.format("<b>" + newuser.title + "</b>"));
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
                status: item.isPending ? "pending" : "",
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
        addtext: ASC.Resources.Master.Resource.UserSelectorAddText,
        noresults: ASC.Resources.Master.Resource.UserSelectorNoResults,
        noitems: ASC.Resources.Master.Resource.UserSelectorNoItems,
        nogroups: ASC.Resources.Master.Resource.UserSelectorNoGroups,
        emptylist: ASC.Resources.Master.Resource.UserSelectorEmptyList,
        isAdmin: false,
        withGuests: true,
        isInitializeItems: true
    });


})(jQuery, window, document, document.body);