/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

var TariffSettings = new function () {
    var isInit = false;
    var selectBuyLink = "";
    var selectActiveUsers = "0-0";
    var selectStorage = "0 byte";

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        jq.switcherAction("#switcherPayments", "#paymentsContainer");

        jq(".tariffs-selected input:radio").prop("checked", true);
        TariffSettings.selectTariff();
    };

    var clickOnBuy = function (object) {
        if (!jq("#buyRecommendationDialog").length) {
            return true;
        }
        if (typeof object == "undefined") {
            return true;
        }

        StudioBlockUIManager.blockUI("#buyRecommendationDialog", 550, 300, 0);
        PopupKeyUpActionProvider.EnterAction = "TariffSettings.redirectToBuy();";
        PopupKeyUpActionProvider.CloseDialogAction = "TariffSettings.dialogRecommendationClose();";

        return false;
    };

    var selectTariff = function (tariffLabel) {
        if (!tariffLabel) {
            tariffLabel = jq(".tariffs-selected");
        }

        jq(".tariffs-selected").removeClass("tariffs-selected");
        tariffLabel.addClass("tariffs-selected");

        var tariffHidden = tariffLabel.find(".tariff-hidden-link");
        var tariffLink = tariffHidden.val();

        jq(".tariff-buy-action").hide();
        jq(".tariff-pay-key-prolongable").removeClass("disable");

        var button = jq();
        if (tariffHidden.hasClass("tariff-hidden-pay")) {
            button = jq(".tariff-buy-pay, .tariff-pay-pal");
            jq(".tariff-pay-key-prolongable").addClass("disable").removeAttr("href");
        } else if (tariffHidden.hasClass("tariff-hidden-free")) {
            button = jq(".tariff-buy-free");
            jq(".tariff-pay-key-prolongable").addClass("disable").removeAttr("href");
        } else if (tariffHidden.hasClass("tariff-hidden-stopfree")) {
        } else if (tariffHidden.hasClass("tariff-hidden-limit")) {
            button = jq(".tariff-buy-limit");
            TariffSettings.selectBuyLink = "";
            TariffSettings.selectActiveUsers = tariffLabel.find(".tariff-hidden-users").val();
            TariffSettings.selectStorage = tariffLabel.find(".tariff-hidden-storage").val();
        } else {
            if (tariffHidden.length) {
                button = jq(".tariff-buy-change, .tariff-pay-pal");
            }
        }

        button.css({ "display": "inline-block" });
        if (!button.hasClass("disable"))
            button.attr("href", tariffLink);
        TariffSettings.selectBuyLink = tariffLink;
    };

    var showDowngradeDialog = function () {
        var quotaActiveUsers = TariffSettings.selectActiveUsers;
        var quotaStorageSize = TariffSettings.selectStorage;
        jq("#downgradeUsers").html(quotaActiveUsers);
        jq("#downgradeStorage").html(quotaStorageSize);

        StudioBlockUIManager.blockUI("#tafirrDowngradeDialog", 450, 300, 0);
    };

    var hideBuyRecommendation = function (obj) {
        var dontDisplay = jq(obj).is(":checked");
        TariffUsageController.SaveHideRecommendation(dontDisplay,
            function (result) {
                if (result.error != null) {
                    toastr.error(result.error.Message);
                    return;
                }
            });
    };

    var dialogRecommendationClose = function () {
        if (jq("#buyRecommendationDisplay").is(":checked")) {
            jq("#buyRecommendationDialog").remove();
        }
    };

    var redirectToBuy = function () {
        location.href = TariffSettings.selectBuyLink;
        PopupKeyUpActionProvider.CloseDialog();
    };

    var getTrial = function () {
        TariffUsageController.GetTrial(
            function (result) {
                if (result.error != null) {
                    toastr.error(result.error.Message);
                }
                location.reload();
            });
    };

    var getFree = function () {
        TariffUsageController.GetFree(
            function (result) {
                if (result.error != null) {
                    toastr.error(result.error.Message);
                }
                location.reload();
            });
    };

    var selectedQuotaId = function () {
        return jq(".tariffs-selected").attr("data");
    };

    return {
        init: init,

        selectBuyLink: selectBuyLink,
        selectActiveUsers: selectActiveUsers,
        selectStorage: selectStorage,

        clickOnBuy: clickOnBuy,
        selectTariff: selectTariff,

        showDowngradeDialog: showDowngradeDialog,
        hideBuyRecommendation: hideBuyRecommendation,
        dialogRecommendationClose: dialogRecommendationClose,
        redirectToBuy: redirectToBuy,

        selectedQuotaId: selectedQuotaId,

        getTrial: getTrial,
        getFree: getFree
    };
};

jq(function () {

    TariffSettings.init();

    jq(".tariffs-panel").on("change", "input:radio", function () {
        TariffSettings.selectTariff(jq(this).closest(".tariff-price-block"));
    });

    jq(".tariffs-button-block").on("click", ".tariff-buy-pay:not(.disable), .tariff-buy-change", function () {
        return TariffSettings.clickOnBuy(this);
    });

    jq(".tariffs-button-block").on("click", ".tariff-buy-limit", function () {
        TariffSettings.showDowngradeDialog();
        return false;
    });

    jq("#buyRecommendationDisplay").click(function () {
        TariffSettings.hideBuyRecommendation(this);
        return true;
    });

    jq("#buyRecommendationOk").click(function () {
        TariffSettings.redirectToBuy();
        return false;
    });

    jq(".tariff-buy-try").click(function () {
        TariffSettings.getFree();
        return false;
    });

    jq(".tariff-buy-free").click(function () {
        TariffSettings.getFree();
        return false;
    });
});