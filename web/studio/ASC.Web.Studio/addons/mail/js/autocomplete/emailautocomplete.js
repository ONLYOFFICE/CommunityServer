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

    function emailSelector(obj, options) {
        var self = this;

        this.input = obj;
        this.settings = options || {};
        this.container = options.container || null;
        this.cache = [];
        this.firstFailTerm = "";
        this.canEdit = true;

        this.parseEmails = function (value) {
            var result = [];
            if (!value)
                return result;

            if (typeof value === "string") {
                return ASC.Mail.Utility.ParseAddresses(value).addresses;
            }

            if (jq.isArray(value)) {
                jq.each(value, function (index, item) {
                    if (typeof item === "object")
                        result.push(item);

                    if (typeof item === "string")
                        result = result.concat(self.parseEmails(item));
                });
            }

            return result;
        };

        this.closeSelector = function() {
            var uiAutocomplete = self.input.data("ui-autocomplete");
            if (uiAutocomplete) {
                uiAutocomplete.close();
            }
        };

        this.addItem = function (value) {
            var parsedItems = self.parseEmails(value);

            if (!parsedItems.length) {
                TMMail.setRequiredHint('mail_CreateTag_Email', MailScriptResource.ErrorEmptyField);
                TMMail.setRequiredError('mail_CreateTag_Email', true);
                self.closeSelector();
                return;
            }

            var selectedEmails = jq.map(self.getItems(), function (selectedItem) {
                return selectedItem;
            });
            var hasErrors = false;
            jq.each(parsedItems, function (ind, item) {
                if (!item.isValid) {
                    TMMail.setRequiredHint('mail_CreateTag_Email', MailScriptResource.ErrorIncorrectEmail);
                    TMMail.setRequiredError('mail_CreateTag_Email', true);
                    hasErrors = true;
                    return;
                }

                for (var i = 0, len = selectedEmails.length; i < len; i++) {
                    if (item.EqualsByEmail(selectedEmails[i])) {
                        TMMail.setRequiredHint('mail_CreateTag_Email', MailResource.ErrorEmailExist);
                        TMMail.setRequiredError('mail_CreateTag_Email', true);
                        hasErrors = true;
                        return;
                    }
                }
                if (self.container) {
                    jq.tmpl('tagEmailInEditPopupTmpl', { address: item.email })
                        .data("item-data", item)
                        .appendTo(self.container);
                    self.container.show();
                }

                selectedEmails.push(item);
            });

            if (!hasErrors) {
                TMMail.setRequiredError('mail_CreateTag_Email', false);
                if (self.container) {
                    self.input.val("");
                } else {
                    self.input.val(parsedItems[0].email);
                }
            }

            self.closeSelector();
        };

        this.getItems = function () {
            var items = [];

            if (self.container) {
                jq.each(self.container.find(".linkedTagAddress"),
                    function(index, item) {
                        var stored = jq(item).data("item-data");
                        items.push(new ASC.Mail.Address(stored.name, stored.email, stored.isValid));
                    });
            } else {
                var value = self.input.val();

                if (value) {
                    items.push(value);
                }
            }

            return items;
        };

        this.clearItems = function () {
            if (self.container) {
                self.container.find(".linkedTagAddress").remove();
            } else {
                self.input.val("");
            }
        };

        this.removeItem = function (deleteBtn) {
            deleteBtn.closest(".linkedTagAddress").remove();
            if (!self.getItems().length) {
                if (self.container) {
                    self.container.hide();
                }
            }
        };

        this.setBindings = function () {
            self.input.off("keyup").on("keyup", function (e) {
                if (e.keyCode === 13) {
                    var item = jq(this).val().trim();
                    if(item)
                        self.addItem(item, true);
                } else {
                    TMMail.setRequiredError('mail_CreateTag_Email', false);
                }
            });

            self.input.next(".plusmail").off("click").on("click", function () {
                self.addItem(self.input.val().trim(), true);
            });

            if (self.container) {
                self.container.off("click")
                    .on("click",
                        ".removeTagAddress",
                        function() {
                            self.removeItem(jq(this));
                        });
            }
        };

        this.initAutocomplete = function () {
            self.input.autocomplete({
                minLength: 2,
                delay: 300,

                select: function (event, ui) {
                    if (ui.item) {
                        self.input.val(ui.item.email);
                        self.addItem([ui.item], true);
                    }
                    return false;
                },

                search: function () {
                    return self.input.is(":visible");
                },

                source: function (request, response) {
                    if (self.settings.isInPopup) {
                        var ul = this.menu.element;
                        var popupParent = self.input.parents(".blockUI.blockPage:first");

                        if (popupParent.length !== 0) {
                            if (popupParent.css("position") === "fixed") {
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
                        success: function(params, contacts) {
                            if (!contacts || contacts.length === 0) {
                                self.firstFailTerm = request.term;
                                return response([]);
                            }

                            var items = self.parseEmails(contacts);
                            self.cache[request.term] = items;
                            return response(items);
                        },
                        error: function(params, error) {
                            console.log(error);
                        }
                    });
                },
                focus: function () {
                    return false;
                },
                create: function() {
                    jq(window).resize(function () {
                        self.closeSelector();
                    });
                },
                open: function () {
                    var e = jq.Event("keydown", { keyCode: 40 });
                    jq(self.input).trigger(e);
                    return false;
                }
            });

            self.input.data("ui-autocomplete")._renderMenu = function (ul, items) {
                var target = this;

                jq.each(items, function (index, item) {
                    target._renderItemData(ul, item);
                });

                if (self.settings.isInPopup && jq.browser.mobile && ul.parents(".blockUI.blockPage:first").length === 0) {
                    var popupParent = self.input.parents(".blockUI.blockPage:first");
                    if (popupParent.length !== 0) {
                        ul.appendTo(popupParent.children());
                    }
                }
            };

           self.input.data("ui-autocomplete")._renderItem = function (ul, item) {
                return jq.tmpl("template-emailselector-autocomplete", item).appendTo(ul);
            };

            self.input.data("ui-autocomplete")._resizeMenu = function () {
                this.menu.element.outerWidth(self.input.outerWidth());
            };

            self.input.data("ui-autocomplete")._suggest = function (items) {
                var ul = this.menu.element.empty().zIndex(this.element.zIndex() + 1);
                this._renderMenu(ul, items);
                this.menu.refresh();
                ul.show();
                this._resizeMenu();
                ul.position(jq.extend({ of: jq(this.element).parent() }, this.options.position));
            };
        };

        this.init = function () {
            self.setBindings();
            self.initAutocomplete();
            self.clearItems();
            self.setItems(self.settings.items || []);
            self.input.val(self.input.attr("value") || "");
        };

        this.setItems = function (items, canEdit) {
            self.clearItems();
            self.canEdit = canEdit == undefined ? true : canEdit;
            jq.each(items, function (i, item) {
                self.addItem(item);
            });

            if (self.canEdit) {
                self.input.removeAttr("disabled").show();
            } else {
                self.input.attr("disabled", true).hide();
            }

            if (self.container) {
                if (items.length) {
                    self.container.show();
                } else {
                    self.container.hide();
                }
            }
        };
    }

    var methods = {
        init: function (options) {
            var obj = jq(this);
            removeInstance(obj);
            var instance = new emailSelector(obj, options);
            instances.push(instance);
            instance.init();
            return this;
        },
        get: function () {
            var obj = jq(this);
            var instance = getInstance(obj);
            return instance ? instance.getItems() : null;
        },
        set: function(data, canEdit) {
            var obj = jq(this);
            var instance = getInstance(obj);
            if (instance) {
                instance.setItems(data, canEdit);
            }
        },
        turnOn: function () {
            var obj = jq(this);
            var instance = getInstance(obj);
            instance.input.autocomplete({
                disabled: false
            });
        },
        turnOff: function () {
            var obj = jq(this);
            var instance = getInstance(obj);
            instance.input.autocomplete({
                disabled: true
            });
        }
    };

    jq.fn.EmailsSelector = function (method) {
        if (this.length && methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }

        return this;
    };

})(jQuery);


