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


//TODO: READ ME

//<div id="emailSelector" class="emailselector">
//    <input type="text" class="emailselector-input" autocomplete="off">
//    <pre class="emailSelector-input-buffer"></pre>
//</div>

//jq(document).ready(function () {
//    var itemsString = '"test" <test@test.ru>, NOT VALID, test@test.com, <test@test.org>';
//    var itemsArray = [
//        '"test.ru" <test@test.ru>',
//        '"BROKEN" <net@net.>',
//        '<test@test.org>',
//        'NOT VALID',
//        { name: "test.com", email: "test@test.com", isValid: true }
//    ];
//    jq("#emailSelector").AdvancedEmailSelector("init", {
//        isInPopup: false,
//        items: itemsArray || itemsString,
//        maxCount: 20,
//        onChangeCallback: function () {
//            console.log("changed");
//        }
//    });
//    var selectedJson = jq("#emailSelector").AdvancedEmailSelector("get");
//    var selectedString = jq("#emailSelector").AdvancedEmailSelector("getString");
//    jq("#emailSelector").AdvancedEmailSelector("clear");
//});

(function (jq) {

    var instances = [];

    function getInstance(obj) {
        var instance = null;

        jq.each(instances, function (index, item) {
            if (obj.is(item.obj)) {
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

        this.obj = obj;
        this.input = this.obj.find(".emailselector-input");
        this.buffer = this.obj.find(".emailSelector-input-buffer");
        this.hiddenInput = jq("<input>").attr("type", "text").addClass("emailselector-hidden-input").insertBefore(this.input);

        this.settings = options || {};
        this.cache = [];
        this.firstFailTerm = "";
        this.history = [];
        this.editedValue = "";
        this.historyStep = 0;
        this.selectionMode = null;
        this.ctrlKeyPressed = false;


        this.parseEmails = function(value) {
            if (!value) return [];

            var parsedItems = [];

            if (typeof value === "string")
                parsedItems = ASC.Mail.Utility.ParseAddresses(value).addresses;

            if (jq.isArray(value))
                parsedItems = value;

            var result = [];

            jq.each(parsedItems, function (index, item) {
                if (typeof item === "object")
                    result.push(item);

                if (typeof item === "string")
                    result = result.concat(ASC.Mail.Utility.ParseAddresses(item).addresses);
            });

            return result;
        };

        this.addItem = function(value, fireCallback) {
            var parsedItems = self.parseEmails(value);

            if (!parsedItems.length) return;

            var changed = self.drawItems(parsedItems, self.input);

            self.input.val("").css("width", "");
            self.input.insertBefore(self.buffer);
            self.closeSelector();

            if (changed) {
                self.updateHistoryStep(true, true);

                if (self.settings.onChangeCallback && fireCallback)
                    self.settings.onChangeCallback();
            }

            self.input.focus();
        };

        this.drawItems = function(items, nextObj) {
            if (!items.length) return false;

            var selectedEmails = jq.map(self.getItems(), function (selectedItem) {
                return selectedItem.email;
            });

            var changed = false;

            jq.each(items, function(index, item) {
                if (!item) return;

                if (jq.inArray(item.email, selectedEmails) < 0) {
                    jq.tmpl("template-emailselector-item", item).data("item-data", item).insertBefore(nextObj);
                    selectedEmails.push(item.email);
                    changed = true;

                    if (self.settings.maxCount) {
                        if (selectedEmails.length >= self.settings.maxCount) {
                            self.obj.find(".emailselector-input").prop("disabled", true);
                            return;
                        } else {
                            self.obj.find(".emailselector-input").prop("disabled", false);
                        }
                    }
                }
            });

            return changed;
        };

        this.getItems = function (selectedOnly) {
            return jq.map(self.obj.find(".emailselector-item" + (selectedOnly ? ".selected" : "")), function (item) {
                var obj = jq(item).data("item-data");
                return new ASC.Mail.Address(obj.name, obj.email, obj.isValid);
            });
        };

        this.getString = function (selectedOnly) {
            var items = jq.map(self.getItems(selectedOnly), function (item) {
                return item.ToString();
            });

            return items.join(", ");
        };

        this.closeSelector = function () {
            var uiAutocomplete = self.input.data("ui-autocomplete");
            if (uiAutocomplete) {
                uiAutocomplete.close();
            }
        };

        this.clearItems = function() {
            self.obj.find(".emailselector-item").remove();
            self.obj.find(".emailselector-input").prop("disabled", false);
        };

        this.updateHistoryStep = function (stepForward, setData) {
            var data = self.getItems();

            if (stepForward) {
                if (setData) {
                    self.historyStep++;
                    self.history = jq.grep(self.history, function(item) {
                        return item.step < self.historyStep;
                    });
                    self.history.push({
                        data: data,
                        step: self.historyStep
                    });
                } else {
                    if (self.historyStep < self.history.length)
                        self.historyStep++;
                }
            } else {
                if (self.historyStep > 0)
                    self.historyStep--;
            }

            if (self.settings.maxCount) {
                if (data.length >= self.settings.maxCount) {
                    self.obj.find(".emailselector-input").prop("disabled", true);
                } else {
                    self.obj.find(".emailselector-input").prop("disabled", false);
                }
            }
        };

        this.showHistoryStep = function () {
            var stepData = jq.grep(self.history, function (item) {
                return item.step === self.historyStep;
            });

            self.clearItems();

            if (stepData.length) {
                self.drawItems(stepData[0].data, self.input);
            }
        };

        this.setBindings = function() {

            var keyCode = {
                A: 65,
                C: 67,
                X: 88,
                V: 86,
                Z: 90,
                Y: 89,
                enter: 13,
                backspace: 8,
                del: 46,
                left: 37,
                right: 39,
                ctrl: 17
            };

            self.obj.on("click", function () {
                self.input.focus();
            });

            self.obj.scroll(function () {
                self.hiddenInput.css("top", self.obj.scrollTop() + "px");
            });

            self.obj.on("click", ".emailselector-item-close", function (e) {
                jq(e.target).parent().remove();

                self.updateHistoryStep(true, true);

                if (self.settings.onChangeCallback)
                    self.settings.onChangeCallback();

                self.input.focus();

                e.stopPropagation();
            });

            self.obj.on("click", ".emailselector-item", function (e) {
                var target = jq(e.target);

                if (target.is(".emailselector-item-text"))
                    target = target.parent();

                if (e.ctrlKey) {
                    target.toggleClass("selected");
                    self.ctrlKeyPressed = true;
                } else if(e.shiftKey) {
                    target.toggleClass("selected");
                } else {
                    self.obj.find(".emailselector-item").removeClass("selected");
                    target.addClass("selected");
                }

                self.hiddenInput.val(self.getString(true)).focus().select();

                e.stopPropagation();
            });

            self.obj.on("mousedown", ".emailselector-item", function (e) {
                if (e.preventDefault)
                    e.preventDefault();
                else
                    e.returnValue = false;
            });

            self.obj.on("dblclick", ".emailselector-item", function () {
                var target = jq(this);
                self.editedValue = target.data("item-data").ToString();
                var width = self.buffer.text(self.editedValue).width();
                target.replaceWith(self.input);
                self.input.val(self.editedValue).width(width).focus();
            });

            self.obj.on("input", "input[type=text]", function () {
                var target = jq(this);
                self.buffer.text(target.val());
                target.width(self.buffer.width() + 5 || 1); // (+ 5) -> ie measurement error, (|| 1) -> chrome don't copy text from input width = 0
            });

            self.obj.on("keydown", ".emailselector-hidden-input", function (e) {
                switch (e.keyCode) {
                    case keyCode.enter:
                        if (self.obj.find(".emailselector-item.selected").length === 1)
                            self.obj.find(".emailselector-item.selected").trigger("dblclick");
                        else
                            self.input.focus();
                        break;
                    case keyCode.A:
                        if (e.ctrlKey) {
                            self.obj.find(".emailselector-item").addClass("selected");
                            self.hiddenInput.val(self.getString(true)).select();
                        }
                        break;
                    case keyCode.Z:
                        if (e.ctrlKey) {
                            self.updateHistoryStep(false, false);
                            self.showHistoryStep();

                            if (self.settings.onChangeCallback)
                                self.settings.onChangeCallback();
                        }
                        break;
                    case keyCode.Y:
                        if (e.ctrlKey) {
                            self.updateHistoryStep(true, false);
                            self.showHistoryStep();

                            if (self.settings.onChangeCallback)
                                self.settings.onChangeCallback();
                        }
                        break;
                    case keyCode.backspace:
                    case keyCode.del:
                        self.obj.find(".emailselector-item.selected").remove();
                        self.hiddenInput.val(self.getString(true));

                        self.updateHistoryStep(true, true);

                        if (self.settings.onChangeCallback)
                            self.settings.onChangeCallback();

                        self.input.focus();
                        break;
                    case keyCode.left:
                        var leftSelected = self.obj.find(".emailselector-item.selected:" + (self.selectionMode === "right" ? "last" : "first"));
                        if (leftSelected.length) {
                            var prev = leftSelected.prev();
                            if (prev.length && prev.hasClass("emailselector-item")) {
                                if (e.shiftKey) {
                                    if (!self.selectionMode)
                                        self.selectionMode = "left";

                                    if (prev.hasClass("selected")) {
                                        leftSelected.removeClass("selected");

                                        if (self.obj.find(".emailselector-item.selected").length === 1)
                                            self.selectionMode = null;
                                    } else {
                                        prev.addClass("selected");
                                    }
                                } else {
                                    self.obj.find(".emailselector-item").removeClass("selected");
                                    prev.addClass("selected");
                                    self.selectionMode = null;
                                }
                                self.hiddenInput.val(self.getString(true)).select();
                            }
                        }
                        break;
                    case keyCode.right:
                        var rightSelected = self.obj.find(".emailselector-item.selected:" + (self.selectionMode === "left" ? "first" : "last"));
                        if (rightSelected.length) {
                            var next = rightSelected.next();
                            if (next.length && next.hasClass("emailselector-item")) {
                                if (e.shiftKey) {
                                    if (!self.selectionMode)
                                        self.selectionMode = "right";

                                    if (next.hasClass("selected")) {
                                        rightSelected.removeClass("selected");

                                        if (self.obj.find(".emailselector-item.selected").length === 1)
                                            self.selectionMode = null;
                                    } else {
                                        next.addClass("selected");
                                    }
                                } else {
                                    self.obj.find(".emailselector-item").removeClass("selected");
                                    next.addClass("selected");
                                    self.selectionMode = null;
                                }
                                self.hiddenInput.val(self.getString(true)).select();
                            } else {
                                self.obj.find(".emailselector-item").removeClass("selected");
                                self.input.focus();
                            }
                        }
                        break;
                    default:
                        break;
                }

                return (e.ctrlKey && (e.keyCode === keyCode.X || e.keyCode === keyCode.C));
            });

            self.obj.on("cut", ".emailselector-hidden-input", function () {
                self.obj.find(".emailselector-item.selected").remove();

                self.updateHistoryStep(true, true);

                if (self.settings.onChangeCallback)
                    self.settings.onChangeCallback();

                setTimeout(function () {
                    self.input.focus();
                }, 0);
            });

            self.obj.on("keydown", ".emailselector-input", function (e) {
                var target = jq(this);
                var value = target.val();

                if (e.keyCode === keyCode.enter) {
                    self.addItem(value.trim(), true);
                    self.input.focus();
                }

                if (!value) {
                    if (e.ctrlKey && e.keyCode === keyCode.A) {
                        self.hiddenInput.focus();
                        self.obj.find(".emailselector-item").addClass("selected");
                        self.hiddenInput.val(self.getString(true)).select();
                    }

                    if (e.ctrlKey && e.keyCode === keyCode.Z) {
                        self.updateHistoryStep(false, false);
                        self.showHistoryStep();

                        if (self.settings.onChangeCallback)
                            self.settings.onChangeCallback();

                        return false;
                    }

                    if (e.ctrlKey && e.keyCode === keyCode.Y) {
                        self.updateHistoryStep(true, false);
                        self.showHistoryStep();

                        if (self.settings.onChangeCallback)
                            self.settings.onChangeCallback();

                        return false;
                    }

                    if (e.keyCode === keyCode.backspace) {
                        self.obj.find(".emailselector-item:last").remove();

                        self.updateHistoryStep(true, true);

                        if (self.settings.onChangeCallback)
                            self.settings.onChangeCallback();

                        self.input.focus();
                    }

                    if (e.keyCode === keyCode.left) {
                        self.hiddenInput.focus();
                        self.obj.find(".emailselector-item:last").addClass("selected");
                        self.hiddenInput.val(self.getString(true)).select();
                    }
                }
            });

            self.obj.on("keyup", ".emailselector-hidden-input", function (e) {
                if (e.keyCode === keyCode.ctrl) {
                    self.ctrlKeyPressed = false;
                }
            });

            self.obj.on("blur", ".emailselector-hidden-input", function (e) {
                jq(this).val("");
                if (!self.ctrlKeyPressed) {
                    self.obj.find(".emailselector-item").removeClass("selected");
                }
            });

            self.obj.on("blur", ".emailselector-input", function () {
                var target = jq(this);
                var value = target.val().trim();
                self.addItem(value, true);
                self.selectionMode = null;
            });
        };

        this.initAutocomplete = function() {
            self.input.autocomplete({
                minLength: 2,
                delay: 300,

                select: function (event, ui) {
                    if (ui.item.name)
                        ui.item.name = Encoder.htmlDecode(ui.item.name);

                    self.addItem([ui.item], true);
                    return false;
                },

                search: function() {
                    return self.obj.is(":visible");
                },

                source: function(request, response) {
                    if (self.settings.isInPopup) {
                        var ul = this.menu.element;
                        var popupParent = self.obj.parents(".blockUI.blockPage:first");

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
                create: function () {
                    jq(window).resize(function () {
                        self.closeSelector();
                    });
                },
                open: function () {
                    var e = jq.Event("keydown", { keyCode: 40 });
                    jq(self.input).trigger(e);
                    jq(this).autocomplete('widget').css('z-index', 100);
                    return false;
                }
            });

            self.input.data("ui-autocomplete")._renderMenu = function (ul, items) {
                var target = this;

                jq.each(items, function(index, item) {
                    target._renderItemData(ul, item);
                });

                if (self.settings.isInPopup && jq.browser.mobile && ul.parents(".blockUI.blockPage:first").length === 0) {
                    var popupParent = self.obj.parents(".blockUI.blockPage:first");
                    if (popupParent.length !== 0) {
                        ul.appendTo(popupParent.children());
                    }
                }
            };

            self.input.data("ui-autocomplete")._renderItem = function (ul, item) {
                return jq.tmpl("template-emailselector-autocomplete", item).appendTo(ul);
            };

            self.input.data("ui-autocomplete")._resizeMenu = function () {
                this.menu.element.outerWidth(self.obj.width());
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

        this.init = function() {
            self.setBindings();
            self.initAutocomplete();
            self.clearItems();
            self.addItem(self.settings.items, false);
        };
    }

    var methods = {
        init: function(options) {
            var obj = jq(this);
            removeInstance(obj);
            var instance = new emailSelector(obj, options);
            instances.push(instance);
            instance.init();
            return this;
        },
        get: function() {
            var obj = jq(this);
            var instance = getInstance(obj);
            return instance ? instance.getItems() : null;
        },
        getString: function () {
            var obj = jq(this);
            var instance = getInstance(obj);
            return instance ? instance.getString() : null;
        },
        clear: function() {
            var obj = jq(this);
            var instance = getInstance(obj);
            if (instance) {
                instance.clearItems();
                instance.history = [];
                instance.historyStep = 0;
            }
            return this;
        }
    };

    jq.fn.AdvancedEmailSelector = function (method) {

        if (this.length && methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }

        return this;
    };

})(jQuery);