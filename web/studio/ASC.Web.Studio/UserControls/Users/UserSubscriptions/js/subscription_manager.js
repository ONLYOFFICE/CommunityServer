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


var CommonSubscriptionManager = new function() {
    this.currentModuleSubsTab;
    this.currentModuleSubsSubtabContents;

    this.LoadSubscriptions = function() {
        AjaxPro.onLoading = function(b) {
            if (b) {
                LoadingBanner.displayLoading();
            } else {
                LoadingBanner.hideLoading();
            }
        };

        var timeoutPeriod = AjaxPro.timeoutPeriod;
        AjaxPro.timeoutPeriod = 5 * 60 * 1000;
        AjaxPro.SubscriptionManager.GetAllSubscriptions(function(result) {
            jq('#modules_notifySenders').html(jq("#headerSubscriptionsTemplate").tmpl(result.value));
            jq('#contents_notifySenders').html(jq("#contentSubscriptionsTemplate").tmpl(result.value));
            CommonSubscriptionManager.InitNotifyByComboboxes();
            CommonSubscriptionManager.InitListTabsComboboxes();
            AjaxPro.timeoutPeriod = timeoutPeriod;
        });

        jq('.subs-tabs .subs-module').on('click', function() {
            CommonSubscriptionManager.ClickProductTag(jq(this).attr("data-id"));
        });

    };

    this.SubscribeToWhatsNew = function() {
        AjaxPro.SubscriptionManager.SubscribeToWhatsNew(function(result) {
            var res = result.value;
            if (res.rs1 == '1') {
                jq('#studio_newSubscriptionButton').html(res.rs2);
            } else {
                toastr.error(res.rs2);
            }
        });
    };

    this.SubscribeToTipsAndTricks = function () {
        AjaxPro.SubscriptionManager.SubscribeToTipsAndTricks(function (result) {
            var res = result.value;
            if (res.rs1 == '1') {
                jq('#studio_tipsSubscriptionButton').html(res.rs2);
            } else {
                toastr.error(res.rs2);
            }
        });
    };

    this.SubscribeToSpam = function () {
        AjaxPro.SubscriptionManager.SubscribeToSpam(function (result) {
            var res = result.value;
            if (res.rs1 == '1') {
                jq('#studio_spamSubscriptionButton').html(res.rs2);
            } else {
                toastr.error(res.rs2);
            }
        });
    };

    this.SubscribeToAdminNotify = function() {
        AjaxPro.SubscriptionManager.SubscribeToAdminNotify(function (result) {
            var res = result.value;
            if (res.rs1 == '1') {
                jq('#studio_adminSubscriptionButton').html(res.rs2);
            } else {
                toastr.error(res.rs2);
            }
        });
    };

    var UpdateTypeSubscriptionCallback = function(result) {
        var res = result.value;
        if (res.Status == 1) {
            var but = jq('#studio_subscribeType_' + res.Data.Id + '_' + res.SubItemId + '_' + res.SubscriptionTypeID).find("a.on_off_button");
            if (but.hasClass("on")) {
                but.removeClass("on").addClass("off");
            } else {
                but.removeClass("off").addClass("on");
            }
        } 
    };

    var UpdateProductSubscriptionCallback = function(result) {
        var res = result.value;
        if (res.Status == 1) {
            if (jq('#content_product_subscribeBox_' + res.Data.Id).find(".subs-subtab").length) {
                jq(".subs-tabs .subs-module.active").removeClass("active");
                jq(".subs-subtab .module.active").removeClass("active");
                jq("[id^='studio_subscriptions_" + res.Data.Id + "']").remove();
                CommonSubscriptionManager.ClickProductTag(res.Data.Id);
                return;
            }
            
            jq('#content_product_subscribeBox_' + res.Data.Id).replaceWith(jq('#contentSubscriptionsTemplate').tmpl({ Items: [res.Data] }));
            var subscribeBox = jq('#content_product_subscribeBox_' + res.Data.Id);
            subscribeBox.addClass("active");
            CommonSubscriptionManager.InitNotifyByComboboxes();
            CommonSubscriptionManager.InitListTabsComboboxes();
        } else {
            jq('#content_product_subscribeBox_' + res.ItemId).html('<div class="errorBox">' + res.Message + '</div>');
        }
    };

    this.RememberCurrentModuleSubtab = function(productID) {
        var subscribeBox = jq('#content_product_subscribeBox_' + productID);
        if (subscribeBox.find(".subs-subtab").length) {
            CommonSubscriptionManager.currentModuleSubsTab = subscribeBox.find(".subs-subtab .active").attr("id");
            CommonSubscriptionManager.currentModuleSubsSubtabContents = subscribeBox.find(".subs-subtab-contents .active").attr("id");
        }
    };

    this.UnsubscribeProduct = function (productID, productName) {
        var mes = (ASC.Resources.Master.Resource.UnsubscribeProductMessage).format(productName);
        if (!confirm(mes))
            return;

        AjaxPro.SubscriptionManager.UnsubscribeProduct(productID, UpdateProductSubscriptionCallback);
    };

    this.SubscribeType = function (productID, moduleID, subscribeType) {
        var subscribe = jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + subscribeType).find("a.on_off_button").hasClass("on");
        
        if (subscribe && !confirm(ASC.Resources.Master.Resource.ConfirmMessage))
            return;

        CommonSubscriptionManager.RememberCurrentModuleSubtab(productID);

        if (subscribe)
            AjaxPro.SubscriptionManager.UnsubscribeType(productID, moduleID, subscribeType, UpdateTypeSubscriptionCallback);
        else
            AjaxPro.SubscriptionManager.SubscribeType(productID, moduleID, subscribeType, UpdateTypeSubscriptionCallback);
        
    };

    this.SubscribeObject = function(productID, moduleID, subscribeType, obj) {
        var item = jq(obj).attr("data-value");
        var subscribe = jq(obj).hasClass("off");
        AjaxPro.SubscriptionManager.SubscribeObject(productID, moduleID, subscribeType, item, subscribe, function (result) {
            var res = result.value;
            if (res.rs1 == '1') {
                var but = jq('#studio_subscribeItem_' + productID + '_' + moduleID + '_' + subscribeType + '_' + item).find('a');
                if (subscribe) {
                    but.removeClass('off').addClass('on');
                } else {
                    but.removeClass('on').addClass('off');
                }
            } else {
                jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + subscribeType).html(res.rs6);
            }

        });
    };
    
    this.ClickProductTag = function(productID) {

        var id = "product_subscribeBox_" + productID;
        var module;

        if (id != jq(".subs-tabs .subs-module.active").attr("id")) {
            jq(".subs-tabs .subs-module").removeClass("active");
            jq("#" + id).addClass("active");
            jq(".subs-contents div").removeClass("active");
            jq("#content_" + id).addClass("active");

            var left = jq("#" + id).position().left + (jq("#" + id).width() - jq("#productSelector").width()) / 2;
            jq("#productSelector").css("left", left);

            if (jq("#content_" + id + " .subs-subtab select").length != 0) {
                var option = jq("#content_" + id + " .subs-subtab option");
                var num = jq("#content_" + id + " .selected-item").attr("data-value");
                module = jq(option[num]).attr("data-id");

                jq("#content_" + id + " .subs-subtab select").change(function() {
                    var elem = jq("#content_" + id + " .subs-subtab option")[jq(this).val()];
                    var selected = jq(elem).attr("data-id");
                    CommonSubscriptionManager.ClickModuleTag(productID, selected);
                });
            }
            else {
                module = jq("#content_" + id + " .subs-subtab span").first().attr("data-id");
            }
            CommonSubscriptionManager.ClickModuleTag(productID, module);
        }
    }
    this.ClickModuleTag = function(productID, moduleID) {
        var id = "module_subscribeBox_" + productID + "_" + moduleID;

        if (id != jq(".subs-subtab .module.active").attr("id")) {
            jq(".subs-subtab .module").removeClass("active");
            jq("#" + id).addClass("active");
            jq(".subs-subtab-contents div").removeClass("active");
            jq("#content_" + id).addClass("active");
        }
        var sections = jq("#content_" + id).children(".subs-sections");
        jq(sections).each(function() {
            var typeID = jq(this).attr("data-type");
            var subscriptionElementID = 'studio_subscriptions_' + productID + '_' + moduleID + '_' + typeID;
            var subscriptionElement = jq('#' + subscriptionElementID);

            if (subscriptionElement == null || subscriptionElement.attr('id') == null) {

                AjaxPro.SubscriptionManager.RenderGroupItemSubscriptions(productID, moduleID, typeID, function(res) {
                    var el = jq('#studio_types_' + productID + '_' + moduleID + '_' + typeID);
                    var resultHTML = '';
                    if (res.value.Status = 1) {

                        resultHTML = jq('<div></div>').html(jq("#subscribtionObjectsTemplate").tmpl(res.value)).html();
                    }

                    if (resultHTML == null || '' == resultHTML) {
                        resultHTML = "<div id='" + subscriptionElementID + "' style='height: 0px;'>&nbsp</div>";
                    }
                    el.html(resultHTML);
                });
            };
        });
    }

    this.InitNotifyByComboboxes = function() {
        jq(".subsSelector").each(function () {
            var el = jq(this);

            var notify = parseInt(el.attr("data-notify"));
            if (!notify) return;

            var subId = el.attr("data-id");
            var fun = el.attr("data-function");
            el.removeAttr("data-id").removeAttr("data-notify").removeAttr("data-function");

            var f = subId ? function () { AjaxPro.SubscriptionManager[fun](subId, parseCheckBoxes(el)); } : function () { AjaxPro.SubscriptionManager[fun](parseCheckBoxes(el)); };

            el.html(jq("#subsSelectorTemplate").tmpl({ type: notify }));

            el.find("input[type=\"checkbox\"]").change(f);

            var connector = el.find(".baseLinkAction");
            if (connector.hasClass("tgConnector")) {
                telegramConnect.init(connector);
            }
        });
    };

    this.InitListTabsComboboxes = function() {
        jq('select[id^="ListTabsCombobox_"]').each(
            function() {
                jq(this).tlcombobox();
            }
		);
    };

    var telegramConnect = (function () {
        var isInit = false;
        var elements = [];

        var helper = jq("#telegramConnectTemplate").tmpl();
        var body = jq("body");

        var tgConnect = helper.children("#tgConnect");
        var tgConnected = helper.children("#tgConnected");
        var tgLink = helper.children("#tgLink");
        var tgCopy = helper.children("#tgCopy");
        var tgDisconnect = helper.children("#tgDisconnect");

        var clipboard;
        var timeout;

        function onApiFail(params, error) {
            LoadingBanner.hideLoading();
            toastr.error(error[0]);
        };

        function hideAll() {
            tgConnect.addClass("display-none");
            tgConnected.addClass("display-none");
            tgLink.addClass("display-none");
            tgCopy.addClass("display-none");
            tgDisconnect.addClass("display-none");
        };

        var lastRender;
        function render(isConnected) {
            lastRender = isConnected;
            for (var i = 0; i < elements.length; i++) {
                elements[i].children("span").toggleClass("display-none", isConnected);
                elements[i].parent().children("input").toggleClass("display-none", !isConnected);
            }

            tgConnect.toggleClass("display-none", isConnected);
            tgConnected.toggleClass("display-none", !isConnected);
            tgLink.toggleClass("display-none", isConnected);
            tgCopy.toggleClass("display-none", isConnected && ASC.Clipboard.enable);
            tgDisconnect.toggleClass("display-none", !isConnected);
        };

        function renderSingle(element) {
            if (lastRender == undefined) return;
            element.children("span").toggleClass("display-none", lastRender);
            element.parent().children("input").toggleClass("display-none", !lastRender);
        }

        function generateLink() {
            Teamlab.telegramLink({ success: linkCallback, error: onApiFail });
        };

        function isConnected(stealth) {
            Teamlab.telegramIsConnected({ success: stealth ? function (params, response) { render(response == 1); } : isConnectedCallback, error: onApiFail });
        };

        function disconnect() {
            LoadingBanner.displayLoading();
            Teamlab.telegramDisconnect({ success: function () { tgDisconnect.off("click"); isConnectedCallback(null, 0); }, error: onApiFail });
        };

        function linkCallback(params, response) {
            if (clipboard) ASC.Clipboard.destroy(clipboard);

            tgLink.on("click", linkUsed);
            if (ASC.Clipboard.enable) {
                tgCopy.on("click", linkUsed);

                clipboard = ASC.Clipboard.create(response, "tgCopy", {
                    onComplete: function () {
                        toastr.success(ASC.Resources.Master.Resource.LinkCopySuccess);
                    }
                });
            }

            tgLink.children("a").attr("href", "https://" + response).text(response.slice(0, 30) + "...");
        };

        function isConnectedCallback(params, response) {
            render(response == 1);
            LoadingBanner.hideLoading();

            if (response != 1) {
                generateLink();

                if (response == 2) {
                    timeout = timeOutFunction()
                }
            } else {
                if (clipboard) ASC.Clipboard.destroy(clipboard);
                tgDisconnect.on("click", disconnect);
            }
        };

        function timeOutFunction() {
            return setTimeout(Teamlab.telegramIsConnected, 1500, { success: timeOutCallback });
        };

        function timeOutCallback(params, response) {
            if (response != 2) {
                isConnectedCallback(params, response);
            } else {
                timeout = timeOutFunction();
            }
        };

        function linkUsed() {
            tgLink.off("click");
            tgCopy.off("click");

            if (!timeout) {
                timeout = timeOutFunction();
            }
        };

        function onClose() {
            body.unbind("click.tgConnect");
            tgDisconnect.off("click");

            if (timeout) {
                clearTimeout(timeout);
            }
        };

        function init(element) {
            elements.push(element);
            element.removeAttr("data-value");
            element.click(function (e) {
                e.stopPropagation(true);

                var height = jq(window).height();
                var top = e.pageY + jq(e.target).height();
                var diff = height - (top - jq(window).scrollTop() + helper.height() + parseInt(helper.css("padding-top")) * 2);
                top = diff >= 0 ? top : top + diff;

                helper.css({
                    position: "absolute",
                    left: e.pageX,
                    top: top - 10,
                }).toggleClass("display-none");

                if (!helper.hasClass("display-none")) {
                    hideAll();
                    LoadingBanner.displayLoading();
                    isConnected();

                    body.bind("click.tgConnect", function (e) {
                        if (jq(e.target).parents("#telegramConnect").length) return;

                        helper.addClass("display-none");
                        onClose();
                    });
                } else {
                    onClose();
                }
            });
            renderSingle(element);

            if (isInit) return;
            isInit = true;

            hideAll();
            helper.appendTo(body);
            isConnected(true);
        };

        return {
            init: init
        };
    })();

    var parseCheckBoxes = function (el) {
        var notifyBy = 0;

        el.find("input[type=\"checkbox\"]").each(function () {
            var checkBox = jq(this);
            if (checkBox.prop("checked")) {
                var num = parseInt(jq(this).attr("data-value"));
                notifyBy |= num;
            }
        });

        return notifyBy;
    };
};