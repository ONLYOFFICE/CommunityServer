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


var TariffSaas = new function () {
    var isInit = false;
    var changeTariffTimeout = null;

    var $tariffPage = jq(".saas-tariff-page");
    var $mainHeader = $tariffPage.find(".main-header");
    var $currentTariff = $tariffPage.find(".current-tariff");
    var $tariffPlans = $tariffPage.find(".available-tariff-plans");
    var $continueStartupButton = $tariffPlans.find("#continueStartupBtn");
    var $buyBusinessButton = $tariffPlans.find("#buyBusinessBtn");
    var $backButton = $tariffPage.find("#backBtn");
    var $currencyPanel = $tariffPage.find("#currencyPanel");
    var $subscription = $tariffPage.find(".subscription");
    var $subscriptionCalc = $subscription.find(".subscription-calc");
    var $subscriptionPeriodItems = $subscription.find(".subscription-period-item");
    var $tariffSelector = $subscription.find("#tariffSelector");
    var $tariffUserPrice = $subscription.find("#tariffUserPrice");
    var $tariffUsersCount = $subscription.find("#tariffUsersCount");
    var $tariffLength = $subscription.find("#tariffLength");
    var $tariffSale = $subscription.find("#tariffSale");
    var $tariffTotalPrice = $subscription.find("#tariffTotalPrice");
    var $tariffErrorMsg = $subscription.find("#tariffErrorMsg");
    var $minMaxErrorMsg = $subscription.find("#minMaxErrorMsg");
    var $buyNowButton = $subscription.find("#buyNow");
    var $subscriptionInfo = $subscription.find(".subscription-customer-info");
    var $blocker = $subscriptionCalc.find(".blocker");

    var usersCount = parseInt($tariffPage.find("#usersCount").val());
    var usersCountMin = parseInt($tariffSelector.attr("min"));
    var usersCountMax = parseInt($tariffSelector.attr("max"));
    var regionName = $tariffPage.find("#regionName").val();

    var bindEvents = function () {

        jq.switcherAction("#switcherPayments", "#paymentsContainer");

        jq.dropdownToggle({
            switcherSelector: "#currencySelector",
            dropdownID: "currencyList",
            rightPos: true,
        });

        jq("#currencyHelpSwitcher").on("click", function () {
            jq(this).helper({ BlockHelperID: "currencyHelp" })
        });

        $buyBusinessButton.on("click", function () {
            changeView(false);
        });

        $backButton.on("click", function () {
            changeView(true);
        });

        $subscriptionPeriodItems.on("click", changeSubscriptionPeriod);

        $tariffSelector.on("input", fixInputValue);

        $tariffSelector.on("change", throttledChangeTariff);

        $continueStartupButton.on("click", continueStartup);

        $buyNowButton.on("click", buyNow);

        AjaxPro.onLoading = function (block) {
            if (block) {
                LoadingBanner.displayLoading()
                $blocker.removeClass("display-none");
            } else {
                LoadingBanner.hideLoading()
                $blocker.addClass("display-none");
            }
        };
    }

    var changeView = function (showTariffPlans) {
        $backButton.toggleClass("display-none", showTariffPlans);
        $currencyPanel.toggleClass("display-none", showTariffPlans);
        $subscription.toggleClass("display-none", showTariffPlans);
        $mainHeader.toggleClass("display-none", !showTariffPlans);
        $currentTariff.toggleClass("display-none", !showTariffPlans);
        $tariffPlans.toggleClass("display-none", !showTariffPlans);
    };

    var changeSubscriptionPeriod = function () {
        $subscriptionPeriodItems.removeClass("active");
        jq(this).addClass("active");
        changeTariff();
    };

    var fixInputValue = function () {
        $buyNowButton.attr("href", "");
        var strValue = $tariffSelector.val().replace(/[^0-9]/g, "");
        var intValue = parseInt(strValue) || 0;
        $tariffSelector.val(intValue);
    };

    var getRightUsersCount = function (value) {
        return Math.min(Math.max(value, usersCountMin), usersCountMax);
    };

    var changeTariff = function () {
        $buyNowButton.attr("href", "");
        var selectedTariff = getSelectedTariff();
        var selectedUsersCount = getSelectedUsersCount();

        if (selectedUsersCount < usersCountMin || selectedUsersCount > usersCountMax) {
            $tariffErrorMsg.addClass("display-none");
            $minMaxErrorMsg.removeClass("display-none");
            $subscriptionCalc.addClass("requiredFieldError");
            return;
        }

        if (usersCount > selectedUsersCount) {
            $tariffErrorMsg.removeClass("display-none");
            $minMaxErrorMsg.addClass("display-none");
            $subscriptionCalc.addClass("requiredFieldError");
        } else {
            $subscriptionCalc.removeClass("requiredFieldError");
        }

        TariffSaasController.GetTariffDetails(selectedTariff, selectedUsersCount, regionName,
            function (result) {
                $tariffPage.removeClass("display-none");

                if (result.error != null) {
                    toastr.error(result.error.Message);
                    return;
                }

                $tariffUserPrice.text(result.value.Price);
                $tariffUsersCount.text(result.value.UsersCount);
                $tariffLength.text(result.value.Period);
                $tariffSale.text(result.value.Sale);
                $tariffTotalPrice.text(result.value.TotalPrice);

                $buyNowButton.attr("href", result.value.ShoppingUrl).toggleClass("disable", !result.value.ShoppingUrl);
                $subscriptionInfo.text(result.value.InfoText).toggleClass("display-none", !result.value.InfoText);

                if (result.value.ButtonText) {
                    $buyNowButton.text(result.value.ButtonText);
                }
            });
    };

    var throttledChangeTariff = function () {
        clearTimeout(changeTariffTimeout);
        changeTariffTimeout = setTimeout(changeTariff, 250);
    }

    var getSelectedTariff = function () {
        return $subscriptionPeriodItems.filter(".active:first").data("quotaid");
    }

    var getSelectedUsersCount = function () {
        return parseInt($tariffSelector.val()) || 0;
    }

    var continueStartup = function () {
        TariffSaasController.ContinueStartup(
            function (result) {
                if (result.error != null) {
                    toastr.error(result.error.Message);
                    return;
                }
                toastr.success(ASC.Resources.Master.ResourceJS.SaasTariffPaymentAccepted);
                setTimeout(window.location.reload.bind(window.location), 3000);
            });
    };

    var buyNow = function () {
        if ($buyNowButton.hasClass("disable")) return false;
        if (!$buyNowButton.attr("href")) return false;
        return true;
    };

    var init = function () {
        if (isInit) return;

        isInit = true;

        bindEvents();

        if ($tariffPlans.length) {
            $tariffPlans.removeClass("display-none");
        } else {
            $currencyPanel.removeClass("display-none");
            $subscription.removeClass("display-none");
        }

        $tariffSelector.val(getRightUsersCount(usersCount));

        changeTariff();
    };

    return {
        init: init
    };
};

jq(function () {
    TariffSaas.init();
});