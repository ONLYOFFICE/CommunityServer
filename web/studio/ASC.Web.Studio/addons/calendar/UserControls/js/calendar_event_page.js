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


(function (jq) {

    var instances = [];

    function getInstance(obj) {
        var instance = null;

        jq.each(instances, function (index, item) {
            if (obj.is(item.input)) {
                instance = item;
                return false;
            }
            return true;
        });

        return instance;
    }

    function removeInstance(obj) {
        jq.each(instances, function (index, item) {
            if (obj.is(item.obj)) {
                instances.splice(index, 1);
                return false;
            }
            return true;
        });
    }

    function attendeesSelector(obj, options) {
        var self = this;

        this.input = obj;
        this.settings = options || {};
        this.container = options.container;
        this.organizer = options.organizer;
        this.cache = [];
        this.firstFailTerm = "";
        this.canEdit = true;

        this.Statuses = ["NEEDS-ACTION", "ACCEPTED", "DECLINED", "TENTATIVE"];
        
        this.Roles = ["REQ-PARTICIPANT", "OPT-PARTICIPANT"];

        this.convertAddressToAttendee = function (item) {
            var attendee = new ICAL.Property("attendee");
            
            attendee.setParameter("cutype", "INDIVIDUAL");
            attendee.setParameter("role", item.role || self.Roles[0]);
            attendee.setParameter("partstat", item.partstat || self.Statuses[0]);
            attendee.setParameter("rsvp", "TRUE");
            
            if (item.organizer)
                attendee.setParameter("x-organizer", "TRUE");

            if (item.name)
                attendee.setParameter("cn", item.name);

            attendee.setValue("mailto:" + item.email);

            return attendee.jCal;
        };

        this.parseEmails = function (value) {
            if (!value) return [];

            var attendees = [];

            if (typeof value === "string") {
                var addresses = ASC.Mail.Utility.ParseAddresses(value).addresses;
                jq.each(addresses, function (index, item) {
                    if (item.isValid) {
                        attendees.push(self.convertAddressToAttendee(item));
                    }
                });
            }

            if (jq.isArray(value))
                attendees = value;

            var result = [];

            jq.each(attendees, function (index, item) {
                if (typeof item === "object")
                    result.push(item);

                if (typeof item === "string")
                    result = result.concat(self.parseEmails(item));
            });

            return result;
        };

        this.addItem = function (value, insertOrganizer) {

            var hasValue = jq.isArray(value) ? value.length : value;

            var parsedItems = self.parseEmails(value);

            if (!parsedItems.length) {
                if (hasValue)
                    self.input.css("border-color", "#cc3300");
                else
                    self.input.css("border-color", "");
                return;
            } else {
                self.input.css("border-color", "");
            }

            var selectedEmails = jq.map(self.getItems(), function (selectedItem) {
                return selectedItem[3];
            });

            insertOrganizer = !selectedEmails.length && insertOrganizer;

            if (insertOrganizer) {
                var selectedAccountObj = self.organizer.find("option:selected");
                var selectedAccount = selectedAccountObj.length ? { email: selectedAccountObj.attr("value"), name: Encoder.htmlDecode(selectedAccountObj.attr("data-name")) } : ASC.Mail.DefaultAccount;

                var organizer = self.convertAddressToAttendee({
                    email: selectedAccount.email,
                    name: selectedAccount.name,
                    partstat: self.Statuses[1],
                    organizer: true
                });
                parsedItems.unshift(organizer);
            }

            jq.each(parsedItems, function (index, item) {
                if (!item) return;

                if (jq.inArray(item[3], selectedEmails) < 0) {
                    var data = {
                        canEdit: self.canEdit,
                        status: item[1].partstat,
                        email: item[3].replace(new RegExp("mailto:", "ig"), ""),
                        name: item[1].cn || "",
                        role: item[1].role || self.Roles[0]
                    };
                    jq("#attendeeTemplate").tmpl(data).data("item-data", item).appendTo(self.container);
                    self.container.removeClass("display-none");
                    self.container.find(".attendee-item:last .action select").val(data.role).tlcombobox();
                    selectedEmails.push(item[3]);
                    self.input.val("");
                }
            });
            
            self.resizeList();
        };

        this.getItems = function () {
            var items = [];
            
            jq.each(self.container.find(".attendee-item"), function (index, item) {
                items.push(jq(item).data("item-data"));
            });
            
            return items;
        };

        this.clearItems = function () {
            self.container.find(".attendee-item").remove();
            self.resizeList();
        };

        this.updateItemStatus = function(select) {
            var parent = select.parents(".attendee-item");
            var data = parent.data("item-data");
            data[1].role = select.val();
            parent.data("item-data", data);
        };

        this.removeItem = function (deleteBtn) {
            deleteBtn.parents(".attendee-item").remove();
            self.resizeList();
        };

        this.setBindings = function () {
            self.input.on("keyup", function (e) {
                if (e.keyCode == 13) {
                    self.addItem(jq(this).val().trim(), true);
                }
            });
            
            self.input.parents(".input-container").find(".button").on("click", function () {
                if (!jq(this).hasClass("disable"))
                    self.addItem(self.input.val().trim(), true);
            });

            self.container.on("click", ".removeItem", function () {
                self.removeItem(jq(this));
            });

            self.container.on("change", ".action select", function () {
                self.updateItemStatus(jq(this));
            });

            self.container.on("click", ".combobox-title", function () {
                if (jq(this).closest(".sharingItem").is(".attendees-user-list .sharingItem:gt(-3)")) {
                    self.container.scrollTo(jq(".sharingItem:has(.combobox-container:visible)"));
                }
            });
        };

        this.initAutocomplete = function () {

            self.input.autocomplete({
                minLength: 2,
                delay: 300,

                select: function (event, ui) {
                    self.addItem([ui.item], true);
                    return false;
                },

                search: function () {
                    return self.input.is(":visible");
                },

                source: function (request, response) {
                    if (self.settings.isInPopup) {
                        var ul = this.menu.element;
                        var popupParent = self.input.parents(".blockUI.blockPage:first");

                        if (popupParent.length != 0) {
                            if (popupParent.css("position") == "fixed") {
                                ul.css("position", "fixed");
                            } else {
                                ul.css("position", "absolute");
                            }
                        }
                    }

                    if (request.term in self.cache) {
                        var cachedItems = self.cache[request.term];
                        return response(cachedItems);
                    }

                    if (!request.term) {
                        self.firstFailTerm = "";
                        return response([]);
                    }

                    if (self.firstFailTerm) {
                        if (request.term.indexOf(self.firstFailTerm) === 0) {
                            return response([]);
                        } else
                            self.firstFailTerm = "";
                    }
                    
                    Teamlab.searchEmails({}, { term: request.term }, {
                        before: function() {
                            ASC.CalendarController.Search = true;
                        },
                        success: function (params, contacts) {
                            if (!contacts || contacts.length === 0) {
                                self.firstFailTerm = request.term;
                                return response([]);
                            }
                            
                            var items = self.parseEmails(contacts);
                            self.cache[request.term] = items;
                            return response(items);
                        },
                        error: function (params, error) {
                            console.log(error);
                        },
                        after: function () {
                            ASC.CalendarController.Search = false;
                        },
                    });
                },
                create: function() {
                    jq(window).resize(function () {
                        self.closeSelector();
                    });
                },
                focus: function() {
                    return false;
                }
            });

            self.input.data("ui-autocomplete")._renderMenu = function (ul, items) {
                var target = this;

                jq.each(items, function (index, item) {
                    target._renderItemData(ul, item);
                });

                if (self.settings.isInPopup && jq.browser.mobile && ul.parents(".blockUI.blockPage:first").length == 0) {
                    var popupParent = self.input.parents(".blockUI.blockPage:first");
                    if (popupParent.length != 0) {
                        ul.appendTo(popupParent.children());
                    }
                }
            };

            self.input.data("ui-autocomplete")._renderItem = function (ul, item) {
                var data = { email: item[3].replace(new RegExp("mailto:", "ig"), ""), name: item[1].cn || "" };
                return jq("#attendeeTemplateAutocomplete").tmpl(data).appendTo(ul);
            };

            self.input.data("ui-autocomplete")._resizeMenu = function () {
                this.menu.element.outerWidth(self.input.outerWidth());
            };

            self.input.data("ui-autocomplete")._suggest = function (items) {
                var ul = this.menu.element.empty().zIndex(100500);
                this._renderMenu(ul, items);
                this.menu.refresh();
                ul.show();
                this._resizeMenu();
                ul.position(jq.extend({ of: this.element }, this.options.position));
            };
        };

        this.init = function () {
            self.setBindings();
            self.initAutocomplete();
            self.clearItems();
            self.addItem(self.settings.items);
        };

        this.setItems = function (items, canEdit) {
            self.clearItems();
            self.canEdit = canEdit == undefined ? true : canEdit;
            self.addItem(items);
            
            if (self.canEdit) {
                self.input.removeAttr("disabled").parents(".input-container").removeClass("display-none").find(".button").removeClass("disable");
            } else {
                self.input.attr("disabled", true).parents(".input-container").addClass("display-none").find(".button").addClass("disable");
            }
            
            if (items.length){
                self.container.removeClass("display-none");
            } else {
                self.container.addClass("display-none");
            }
        };

        this.closeSelector = function() {
            var uiAutocomplete = self.input.data("ui-autocomplete");
            if (uiAutocomplete) {
                uiAutocomplete.close();
            }
        };

        this.resizeList = function() {
            var itemsCount = self.container.find(".sharingItem").length;

            if (itemsCount) {
                self.container.removeClass("display-none");
            } else {
                self.container.addClass("display-none");
            }

            if (itemsCount > 7) {
                self.container.addClass("scrollable");
            } else {
                self.container.removeClass("scrollable");
            }
        };
    }

    var methods = {
        init: function (options) {
            var obj = jq(this);
            removeInstance(obj);
            var instance = new attendeesSelector(obj, options);
            instances.push(instance);
            instance.init();
            return this;
        },
        get: function () {
            var obj = jq(this);
            var instance = getInstance(obj);
            return instance ? instance.getItems() : null;
        },
        set: function (data, canEdit) {
            var obj = jq(this);
            var instance = getInstance(obj);
            if (instance) {
                instance.setItems(data, canEdit);
            }
        }
    };

    jq.fn.AttendeesSelector = function (method) {

        if (this.length && methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }

        return this;
    };

})(jQuery);


(function (jq) {

    var instances = [];

    function getInstance(obj) {
        var instance = null;

        jq.each(instances, function (index, item) {
            if (obj.is(item.selector)) {
                instance = item;
                return false;
            }
            return true;
        });

        return instance;
    }

    function removeInstance(obj) {
        jq.each(instances, function (index, item) {
            if (obj.is(item.obj)) {
                instances.splice(index, 1);
                return false;
            }
            return true;
        });
    }

    function shareUsersSelector(obj, options) {
        var self = this;

        this.selector = obj;
        this.permissions = options.permissions;
        this.container = options.container;
        this.canEdit = true;

        this.removeItem = function (itemId) {
            for (var i = 0; i < self.permissions.data.items.length; i++) {
                if (self.permissions.data.items[i].id == itemId) {
                    if (self.permissions.data.items[i].canEdit === false || self.permissions.data.items[i].hideRemove) {
                        return false;
                    }
                    
                    if (!self.permissions.data.items[i].isGroup) {
                        self.selector.useradvancedSelector("unselect", [itemId]);
                    }
                    
                    self.permissions.data.items.splice(i, 1);
                    break;
                }
            }

            self.container.find("#sharing_event_item_" + itemId).remove();

            self.resizeList();

            return true;
        };

        this.addUsers = function (e, users) { 
            var selectedIds = jq(users).map(function (index, user) {
                return user.id;
            }).toArray();

            for (var i = 0; i < self.permissions.data.items.length; i++) {
                var item = self.permissions.data.items[i];
                if (!item.isGroup) {
                    var num = jq.inArray(item.id, selectedIds);
                    if (num == -1) {
                        if (self.removeItem(item.id)) {
                            i--;
                        }
                    } else {
                        selectedIds.splice(num, 1);
                    }
                }
            }

            jq(users).each(function (j, user) {
                if (jq.inArray(user.id, selectedIds) != -1) {
                    self.addUserItem(user.id, user.title);
                }
            });
        };

        this.addUserItem = function (userId, userName) {
            var defAct = null;

            for (var i = 0; i < self.permissions.data.actions.length; i++) {
                if (self.permissions.data.actions[i].defaultAction) {
                    defAct = self.permissions.data.actions[i];
                    break;
                }
            }
            
            var newItem = { id: userId, name: userName, selectedAction: defAct, isGroup: false, canEdit: self.canEdit };

            self.permissions.data.items.push(newItem);

            jq("#sharingUserTemplate").tmpl({ items: [newItem], actions: self.permissions.data.actions }).appendTo(self.container);

            self.container.find(".action:last select").tlcombobox();

            self.resizeList();
        };

        this.getItems = function () {
            var permissions = new Array();
            for (var i = 0; i < self.permissions.data.items.length; i++) {
                var item = self.permissions.data.items[i];
                if (item.selectedAction.id == 'owner')
                    continue;

                permissions.push({ objectId: item.id, name: item.name });
            }
            
            return { users: permissions, data: self.permissions.data };
        };

        this.renderContainer = function () {
            self.container.empty();

            jq("#sharingUserTemplate").tmpl(self.permissions.data).appendTo(self.container);

            self.container.find(".action select").each(function () {
                jq(this).tlcombobox();
            });

            self.resizeList();

            self.selector.useradvancedSelector("reset");

            var userIds = new Array();
            var disableUserIds = new Array();
            
            for (var i = 0; i < self.permissions.data.items.length; i++) {
                var item = self.permissions.data.items[i];
                if (!item.isGroup) {
                    userIds.push(item.id);
                    if (item.hideRemove || !item.canEdit) {
                        disableUserIds.push(item.id);
                    }
                }
            }
            
            self.selector.useradvancedSelector("select", userIds);
            self.selector.useradvancedSelector("disable", disableUserIds);
        };

        this.changeItemAction = function (itemId, actionId) {
            var act = null;

            for (var i = 0; i < self.permissions.data.actions.length; i++) {
                if (self.permissions.data.actions[i].id == actionId) {
                    act = self.permissions.data.actions[i];
                    break;
                }
            }

            for (i = 0; i < self.permissions.data.items.length; i++) {
                if (self.permissions.data.items[i].id == itemId) {
                    self.permissions.data.items[i].selectedAction = act;
                    break;
                }
            }
        };

        this.setBindings = function () {

            self.container.on("click", ".removeItem", function () {
                self.removeItem(jq(this).attr("data"));
            });

            self.container.on("change", ".action select", function () {
                self.changeItemAction(jq(this).attr("data"), jq(this).val());
            });

            self.container.on("click", ".combobox-title", function () {
                if (jq(this).closest(".sharingItem").is(".shared-user-list .sharingItem:gt(-3)")) {
                    self.container.scrollTo(jq(".sharingItem:has(.combobox-container:visible)"));
                }
            });

            self.selector
                .useradvancedSelector(
                    {
                        showGroups: true
                    })
                .on("showList", self.addUsers);
        };

        this.init = function () {
            self.setBindings();
            self.renderContainer();
        };

        this.resizeList = function () {
            var itemsCount = self.container.find(".sharingItem").length;

            if (itemsCount) {
                self.container.removeClass("display-none");
            } else {
                self.container.addClass("display-none");
            }

            if (itemsCount > 7) {
                self.container.addClass("scrollable");
            } else {
                self.container.removeClass("scrollable");
            }
        };
    }

    var methods = {
        init: function (options) {
            var obj = jq(this);
            removeInstance(obj);
            var instance = new shareUsersSelector(obj, options);
            instances.push(instance);
            instance.init();
            return this;
        },
        get: function () {
            var obj = jq(this);
            var instance = getInstance(obj);
            return instance ? instance.getItems() : null;
        },
        set: function (data, canEdit) {
            var obj = jq(this);
            var instance = getInstance(obj);
            if (instance) {
                instance.permissions = data;
                instance.canEdit = canEdit == undefined ? true : canEdit;
                instance.renderContainer();
            }
        }
    };

    jq.fn.ShareUsersSelector = function (method) {

        if (this.length && methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }

        return this;
    };

})(jQuery);