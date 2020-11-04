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

    function removeInstance(obj) {
        jq.each(instances, function (index, item) {
            if (obj.is(item.obj)) {
                instances.splice(index, 1);
                return false;
            }
            return true;
        });
    }

    function crmSelector(obj, options) {
        var self = this;

        this.input = obj;
        this.settings = options || {};
        this.container = options.container || null;
        this.getEntityType = options.getEntityType || null;
        this.onSelectItem = options.onSelectItem || null;
        this.isExists = options.isExists || null;
        this.cache = [];
        this.firstFailTerm = "";

        this.closeSelector = function() {
            var uiAutocomplete = self.input.data("ui-autocomplete");
            if (uiAutocomplete) {
                uiAutocomplete.close();
            }
        };

        this.addItem = function (item) {
            self.onSelectItem(item);
            self.closeSelector();
        };

        this.initAutocomplete = function () {
            self.input.autocomplete({
                minLength: 1,
                delay: 500,

                position: {my: "left top", at: "left bottom+2", collision: "none"},

                select: function (event, ui) {
                    if (ui.item) {
                        self.input.val("");
                        self.addItem(ui.item);
                    }
                    return false;
                },

                search: function () {
                    return self.input.is(":visible");
                },

                source: function (request, response) {
                    var entityType = +self.getEntityType();
                    var params = { searchText: request.term, entityType: entityType };

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

                    var cacheKey = request.term + '|' + entityType;

                    if (cacheKey in self.cache) {
                        var cachedItems = self.cache[cacheKey];
                        params.skipCache = true;
                        return onGetCrmContactsByPrefix(params, cachedItems);
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

                    if (!self.getEntityType) {
                        console.error("No set getEntityType function");
                        return response([]);
                    }

                    function onGetCrmContactsByPrefix(params, contacts) {
                        if (!contacts || contacts.length === 0) {
                            self.firstFailTerm = request.term;
                            return response([]);
                        }

                        var items = contacts.map(function (contact) {
                            if (!self.isExists(params.entityType, contact.id)) {

                                var contactInfo = {
                                    id: contact.id,
                                    entityType: params.entityType,
                                    displayName: contact.displayName || contact.title
                                };

                                if (contact.smallFotoUrl) {
                                    contactInfo.smallFotoUrl = contact.smallFotoUrl;
                                }

                                return contactInfo;
                            }
                            return undefined;
                        }).filter(function (val) { return val != undefined; });

                        if (!params.skipCache) {
                            var cacheKey = request.term + '|' + params.entityType;
                            self.cache[cacheKey] = items;
                        }

                        return response(items);
                    }

                    switch (entityType) {
                        case 1: // contactType
                            window.Teamlab.getCrmContactsByPrefix(params, {
                                filter: { prefix: request.term, searchType: -1 },
                                success: onGetCrmContactsByPrefix,
                                error: function (params, error) {
                                    console.log(error);
                                }
                            });
                            break;
                        case 2: // caseType
                            window.Teamlab.getCrmCasesByPrefix(params, {
                                filter: { prefix: request.term, searchType: -1 },
                                success: onGetCrmContactsByPrefix,
                                error: function (params, error) {
                                    console.log(error);
                                }
                            });
                            break;
                        case 3: // opportunityType
                            window.Teamlab.getCrmOpportunitiesByPrefix(params, {
                                filter: { prefix: request.term },
                                success: onGetCrmContactsByPrefix,
                                error: function (params, error) {
                                    console.log(error);
                                }
                            });
                            break;
                        default:
                            return response([]);
                    }
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
               return jq.tmpl("template-emailselector-autocomplete", { name: null, email: item.displayName }).appendTo(ul);
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
                var position = jq.extend({ of: jq(this.element).parent() }, this.options.position);
                ul.position(position);
            };
        };

        this.init = function () {
            self.initAutocomplete();
            self.input.val("");
        };

    }

    var methods = {
        init: function (options) {
            var obj = jq(this);
            removeInstance(obj);
            var instance = new crmSelector(obj, options);
            instances.push(instance);
            instance.init();
            return this;
        }
    };

    jq.fn.CrmSelector = function (method) {
        if (this.length && methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }

        return this;
    };

})(jQuery);


