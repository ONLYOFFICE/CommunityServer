
(function ($, win, doc, body) {
    var contactadvancedSelector = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.contactadvancedSelector.defaults, options);

        this.init();
    };

    contactadvancedSelector.prototype = $.extend({}, $.fn.advancedSelector.Constructor.prototype, {
        constructor: contactadvancedSelector,
        initCreationBlock: function (data) {
            var that = this,
                opts = {},
                itemsSimpleSelect = [],
                items = data.items;

            opts.newoptions = [
                         {
                             title: ASC.Resources.Master.Resource.SelectorContactType, type: "choice", tag: "type", items: [
                                      { type: "person", title: ASC.Resources.Master.Resource.SelectorPerson },
                                      { type: "company", title: ASC.Resources.Master.Resource.SelectorCompany }
                             ]
                         },
                         { title: ASC.Resources.Master.Resource.SelectorFirstName, type: "input", tag: "first-name" },
                         { title: ASC.Resources.Master.Resource.SelectorLastName, type: "input", tag: "last-name" },
                         { title: ASC.Resources.Master.Resource.SelectorCompany, type: "select", tag: "company" },
                         { title: ASC.Resources.Master.Resource.SelectorCompanyName, type: "input", tag: "title" }
            ];
            opts.newbtn = ASC.Resources.Master.Resource.CreateButton;

            that.displayAddItemBlock.call(that, opts);

            var $select = that.$advancedSelector.find(".advanced-selector-field-wrapper.type .advanced-selector-field");

            $select.on("change", function () {
                var typeContact = $select.val();
                switch (typeContact) {
                    case "person":
                        that.$advancedSelector.find(".advanced-selector-field-wrapper.first-name, .advanced-selector-field-wrapper.last-name, .advanced-selector-field-wrapper.company").show();
                        that.$advancedSelector.find(".advanced-selector-field-wrapper.title").hide();
                        break;
                    case "company":
                        that.$advancedSelector.find(".advanced-selector-field-wrapper.first-name, .advanced-selector-field-wrapper.last-name, .advanced-selector-field-wrapper.company").hide();
                        that.$advancedSelector.find(".advanced-selector-field-wrapper.title").show();
                        break;
                }
            });
            $select.trigger("change");

            for (var i = 0, length = items.length; i < length; i++) {
                if (items[i].type == "company") {
                    itemsSimpleSelect.push(
                        {
                            title: items[i].title,
                            id: items[i].id
                        }
                    );
                }
            }
            that.initDataSimpleSelector.call(that, { tag: "company", items: itemsSimpleSelect });
            that.hideLoaderSimpleSelector.call(that, "company");

        },

        initAdvSelectorDataTempLoad: function () {
            var that = this,
                startIndex = 0;

            if (!that.cache[""]) {

                Teamlab.getCrmContacts({}, {
                    filter: {
                        startIndex: startIndex,
                        Count: 15
                    },
                    before: function() {
                        that.showLoaderListAdvSelector.call(that, 'items');
                    },
                    after: function() {
                        that.hideLoaderListAdvSelector.call(that, 'items');
                    },
                    success: successCallback,
                    error: errorCallback
                });

            } else {
                successCallback(null, that.cache[""]);
            }

            that.$advancedSelector.find(".advanced-selector-list").off("scroll").on("scroll", function () {
                var $this = $(this);
                if ($this.innerHeight() + $this.scrollTop() >= $this.prop("scrollHeight")
                    && that.options.showSearch && that.$advancedSelector.find(".advanced-selector-search-field")) {
                    startIndex += 15;
                    Teamlab.getCrmContacts({}, {
                        filter: {
                            startIndex: startIndex,
                            Count: 15
                        },
                        success: function (params, data) {
                            if (data.length) {
                                data.forEach(function (dataItem) {
                                    that.cache[""].push(dataItem);
                                });
                            }

                            that.displayPartialList.call(that, params, data);
                            for (var i = 0, ln = that.selectedItems.length; i < ln; i++) {
                                var $list = that.$itemsListSelector.find(".advanced-selector-list");
                                data.forEach(function(el) {
                                    if (el.id == $(that.selectedItems[i]).attr("data-id")) {
                                        $list.find("li[data-id=" + el.id + "]").not(".selected").remove();
                                    }
                                });
                            };
                            that.pushNewItemsInList.call(that, data);
                        },
                        error: function (params, errors) {
                            toastr.error(errors);
                        }
                    });
                }
            });
            
            function successCallback(params, data) {
                if (that.options.withPhoneOnly) {
                    data = data.filter(function (el) {
                        return el.commonData.some(
                            function (elem) {
                                return elem.infoType == 0;
                            });
                    });
                }
                
                that.cache[""] = data;

                that.rewriteObjectItem.call(that, data);
                for (var i = 0, ln = that.selectedItems.length; i < ln; i++) {
                    var $list = that.$itemsListSelector.find(".advanced-selector-list");
                    $list.prepend(that.selectedItems[i]);
                    data.forEach(function (el) {
                        if (el.id == $(that.selectedItems[i]).attr("data-id")) {
                            $list.find("li[data-id=" + el.id + "]").not(".selected").remove();
                        }
                    });
                }
            }

            function errorCallback(params, errors) {
                toastr.error(errors);
            }
        },

        initAdvSelectorData: function () {

            var that = this;
            if (that.options.isTempLoad) {
                that.initAdvSelectorDataTempLoad.call(that);
            } else {
                Teamlab.getCrmContacts({}, {
                    before: function () {
                        that.showLoaderListAdvSelector.call(that, 'items');
                    },
                    after: function () {
                        that.hideLoaderListAdvSelector.call(that, 'items');
                    },
                    success: function (params, data) {
                        that.rewriteObjectItem.call(that, data);
                    },
                    error: function (params, errors) {
                        toastr.error(errors);
                    }
                });
            }
        },

        onSearchItemsTempLoad: function () {
            var that = this,
                startIndex = 0,
                cachedItem = null,
                filter = {
                    prefix: that.$advancedSelector.find(".advanced-selector-search-field").val(),
                    searchType: -1,
                    entityID: 0,
                    startIndex: startIndex,
                    Count: 15
                };
            
            if (filter.prefix in that.cache) {
                cachedItem = that.cache[filter.prefix];
            } else {
                for (var cacheterm in that.cache) {
                    if (that.cache[cacheterm].length == 0 && filter.prefix.indexOf(cacheterm) == 0) {
                        cachedItem = [];
                    }
                }
            }

            if (cachedItem == null) {
                Teamlab.getCrmContactsByPrefix({}, {
                    filter: filter,
                    success: successCallback,
                    error: errorCallback
                });
            } else {
                successCallback(null, cachedItem);
            }

            if (cachedItem == null || cachedItem.scrolled == false) {
                that.$advancedSelector.find(".advanced-selector-list").off("scroll").on("scroll", function() {
                    var $this = $(this);
                    if ($this.innerHeight() + $this.scrollTop() >= $this.prop("scrollHeight")
                        && that.options.showSearch && that.$advancedSelector.find(".advanced-selector-search-field")) {
                        startIndex += 15;
                        Teamlab.getCrmContactsByPrefix({}, {
                            filter: {
                                startIndex: startIndex,
                                Count: 15,
                                prefix: that.$advancedSelector.find(".advanced-selector-search-field").val(),
                                searchType: -1,
                                entityID: 0
                            },
                            success: function(params, data) {
                                if (data.length) {
                                    data.forEach(function (dataItem) {
                                        that.cache[params.__filter.prefix].push(dataItem);
                                    });
                                }

                                that.displayPartialList.call(that, params, data);
                            },
                            error: errorCallback
                        });
                    }
                });
            }

            function successCallback (params, data) {

                if (params)
                    that.cache[params.__filter.prefix] = data;

                var $noResult = that.$advancedSelector.find(".advanced-selector-no-results");
                that.$itemsListSelector.find(".advanced-selector-list").html("");
                if (data.length == 0) {
                    $noResult.show();
                } else {
                    $noResult.hide();
                    that.displayPartialList.call(that, params, data);

                    var selectedItemsIds = [];
                    that.selectedItems.forEach(function (item) {
                        selectedItemsIds.push($(item).attr("data-id"));
                    });
                    
                    that.$itemsListSelector.find(".advanced-selector-list li").each(function () {
                        var $this = $(this);
                        if ($.inArray($this.attr("data-id"), selectedItemsIds) != -1) {
                            $this.addClass("selected").find("input").prop("checked", true);
                        }
                    });
                    
                    that.disableDefaultItemsIds.call(that, that.options.itemsDisabledIds);
                }
            }
            
            function errorCallback (params, errors) {
                toastr.error(errors);
            }
        },
        
        displayPartialList: function (params, data) {
            var that = this,
                newDisplayItems = [],
                itemIds = [];
            for (var i = 0, length = data.length; i < length; i++) {
                var newObj = {};
                newObj.title = data[i].displayName || data[i].title || data[i].name || data[i].Name;
                newObj.id = data[i].id && data[i].id.toString();
                newObj.phone = data[i].phone || (data[i].commonData && data[i].commonData.filter(function (el) {
                    return el.infoType == 0;
                }));

                if (data[i].hasOwnProperty("contactclass")) {
                    newObj.type = data[i].contactclass;
                }
                newDisplayItems.push(newObj);

                that.items.forEach(function (item) {
                    itemIds.push(item.id);
                });
                if ($.inArray(newObj.id, itemIds) == -1) {
                    that.items.push(newObj);
                }
            }
            var $o = $.tmpl(that.options.templates.itemList, { Items: newDisplayItems, isJustList: that.options.onechosen });
            that.$itemsListSelector.find(".advanced-selector-list").append($o);
            that.disableDefaultItemsIds.call(that, that.options.itemsDisabledIds);
        },

        createNewItemFn: function () {
            var that = this,
                $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block"),
                $btn = $addPanel.find(".advanced-selector-btn-add"),
                isError,

            isCompany = $addPanel.find(".type select").val() == "company" ? true : false,
            newContact = {};

            if (isCompany) {
                newContact.companyName = $addPanel.find(".title input").val().trim();
            } else {
                newContact.firstName = $addPanel.find(".first-name input").val().trim();
                newContact.lastName = $addPanel.find(".last-name input").val().trim();
                newContact.companyId = $addPanel.find(".company input").attr("data-id");
            }
            if (isCompany && !newContact.companyName) {
                that.showErrorField.call(that, { field: $addPanel.find(".title"), error: ASC.Resources.Master.Resource.ContactSelectorEmptyNameError });
                isError = true;
            }
            if (!isCompany && !newContact.firstName) {
                that.showErrorField.call(that, { field: $addPanel.find(".first-name"), error: ASC.Resources.Master.Resource.ErrorEmptyUserFirstName });
                isError = true;
            }
            if (!isCompany && !newContact.lastName) {
                that.showErrorField.call(that, { field: $addPanel.find(".last-name"), error: ASC.Resources.Master.Resource.ErrorEmptyUserLastName });
                isError = true;
            }
            if (!isCompany && !newContact.companyId && $addPanel.find(".company input").val()) {
                that.showErrorField.call(that, { field: $addPanel.find(".company"), error: ASC.Resources.Master.Resource.ContactSelectorNotFoundError });
                isError = true;
            }
            if (isError) {
                $addPanel.find(".error input").first().focus();
                return;
            }
            Teamlab.addCrmContact({}, isCompany, newContact, {
                before:function(){
                    that.displayLoadingBtn.call(that, { btn: $btn, text: ASC.Resources.Master.Resource.LoadingProcessing });
                },
                error: function (params, errors) {
                    that.showServerError.call(that, { field: $btn, error: errors });
                },
                success: function (params, contact) {
                    var newcontact = {
                        id: contact.id,
                        title: contact.displayName,
                        phone: contact.commonData && contact.commonData.filter(function (el) {
                            return el.infoType == 0;
                        })
                    };
                    that.actionsAfterCreateItem.call(that, { newitem: newcontact, response: contact });
                },
                after: function () { that.hideLoadingBtn.call(that, $btn); }
            })

        },

        rewriteObjectItem: function (data) {
            var that = this;
            that.items = [];

            that.pushNewItemsInList.call(that, data);
            if (!that.options.isTempLoad) {
                that.items = that.items.sort(SortData);
            }
            that.showItemsListAdvSelector.call(that);
        },

        pushNewItemsInList: function (data) {
            var that = this;
            for (var i = 0, length = data.length; i < length; i++) {
                var newObj = {};
                newObj.title = data[i].displayName || data[i].title || data[i].name || data[i].Name;
                newObj.id = (data[i].id && data[i].id.toString());
                newObj.phone = data[i].commonData && data[i].commonData.filter(function (el) {
                    return el.infoType == 0;
                });

                if (data[i].hasOwnProperty("contactclass")) {
                    newObj.type = data[i].contactclass;
                }
                that.items.push(newObj);
            }
        }

    });

   

    $.fn.contactadvancedSelector = function (option, val) {
        return this.each(function () {
            var $this = $(this),
                data = $this.data('contactadvancedSelector'),
                options = $.extend({},
                        $.fn.contactadvancedSelector.defaults,
                        $this.data(),
                        typeof option == 'object' && option);
            if (!data) $this.data('contactadvancedSelector', (data = new contactadvancedSelector(this, options)));
            if (typeof option == 'string') data[option](val);
        });
    }
    $.fn.contactadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {
        showme: true,
        addtext: ASC.Resources.Master.Resource.ContactSelectorAddText,
        noresults: ASC.Resources.Master.Resource.ContactSelectorNoResult,
        noitems: ASC.Resources.Master.Resource.ContactSelectorNoItems,
        emptylist: ASC.Resources.Master.Resource.ContactSelectorEmptyList,
        withPhoneOnly: false
        
    });

})(jQuery, window, document, document.body);