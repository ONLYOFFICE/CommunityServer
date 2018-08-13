/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
                jq('#studio_newSubscriptionButton').html('<div class="errorBox">' + res.rs2 + '</div>');
            }
        });
    };

    this.SubscribeToTipsAndTrics = function () {

        AjaxPro.SubscriptionManager.SubscribeToTipsAndTrics(function (result) {
            var res = result.value;
            if (res.rs1 == '1') {
                jq('#studio_tipsSubscriptionButton').html(res.rs2);
            } else {
                jq('#studio_tipsSubscriptionButton').html('<div class="errorBox">' + res.rs2 + '</div>');
            }
        });
    };

    this.SubscribeToAdminNotify = function() {
        AjaxPro.SubscriptionManager.SubscribeToAdminNotify(function(result) {
            var res = result.value;
            if (res.rs1 == '1') {
                jq('#studio_adminSubscriptionButton').html(res.rs2);
            } else {
                jq('#studio_adminSubscriptionButton').html('<div class="errorBox">' + res.rs2 + '</div>');
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
        jq('select[id^="NotifyByCombobox_"]').each(
            function() {
                jq(this).tlcombobox();
            }
		);
    };

    this.InitListTabsComboboxes = function() {
        jq('select[id^="ListTabsCombobox_"]').each(
            function() {
                jq(this).tlcombobox();
            }
		);
    };

    this.SetNotifyByMethod = function(productID, notifyBy) {
        AjaxPro.SubscriptionManager.SetNotifyByMethod(productID, notifyBy, function(result) { });
    };

    this.SetWhatsNewNotifyByMethod = function(notifyBy) {
        AjaxPro.SubscriptionManager.SetWhatsNewNotifyByMethod(notifyBy, function(result) { });
    };
    this.SetAdminNotifyNotifyByMethod = function(notifyBy) {

        AjaxPro.SubscriptionManager.SetAdminNotifyNotifyByMethod(notifyBy, function(result) { });
    };
};