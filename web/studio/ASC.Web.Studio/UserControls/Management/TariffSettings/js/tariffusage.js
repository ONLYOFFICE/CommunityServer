/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


var TariffSettings = new function () {
    var isInit = false;
    var selectedTariffId = null;
    var selectActiveUsers = "0-0";
    var selectStorage = "0 byte";

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        jq.switcherAction("#switcherPayments", "#paymentsContainer");

        jq(".tariffs-selected input:radio").prop("checked", true);
        TariffSettings.selectTariff();

        if (jq("#annualRecomendationDialog").length) {
            StudioBlockUIManager.blockUI("#annualRecomendationDialog", 600, 300, 0);
            PopupKeyUpActionProvider.EnterAction = "TariffSettings.redirectToBuy(jq('#buttonYearSubscribe').attr('data-id'));";
        }
    };

    var clickOnBuy = function (object) {
        if (!jq("#buyRecommendationDialog").length) {
            return true;
        }
        if (typeof object == "undefined") {
            return true;
        }

        StudioBlockUIManager.blockUI("#buyRecommendationDialog", 550, 300, 0);
        PopupKeyUpActionProvider.EnterAction = "TariffSettings.redirectToBuy(TariffSettings.selectedTariffId);";
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
        var tariffId = tariffLabel.attr("data-id");

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
            TariffSettings.selectedTariffId = null;
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
        TariffSettings.selectedTariffId = tariffId;
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

    var annualRecomendationCheck = function (obj) {
        var dontDisplay = jq(obj).is(":checked");
        TariffUsageController.SaveAnnualRecomendation(dontDisplay,
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

    var redirectToBuy = function (tariffId) {
        var tariffLabel = jq(".tariff-price-block[data-id='" + tariffId + "']");
        var tariffHidden = tariffLabel.find(".tariff-hidden-link");
        var tariffLink = tariffHidden.val();
        var price = tariffLabel.find(".tariff-hidden-price").val();
        var ownerId = jq(".tariffs-panel").attr("data-id");

        //window.ga('send', 'buy', ownerId, price);
        location.href = tariffLink;
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
        return jq(".tariffs-selected").attr("data-id");
    };

    return {
        init: init,

        selectedTariffId: selectedTariffId,
        selectActiveUsers: selectActiveUsers,
        selectStorage: selectStorage,

        clickOnBuy: clickOnBuy,
        selectTariff: selectTariff,

        showDowngradeDialog: showDowngradeDialog,
        hideBuyRecommendation: hideBuyRecommendation,
        dialogRecommendationClose: dialogRecommendationClose,
        redirectToBuy: redirectToBuy,
        annualRecomendationCheck: annualRecomendationCheck,

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
        if (TariffSettings.clickOnBuy(this)) {
            TariffSettings.redirectToBuy(TariffSettings.selectedTariffId);
        }

        return false;
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
        TariffSettings.redirectToBuy(TariffSettings.selectedTariffId);
        return false;
    });

    jq("#annualRecomendationDisplay").click(function () {
        TariffSettings.annualRecomendationCheck(this);
        return true;
    });

    jq("#buttonYearSubscribe").click(function () {
        var tariffId = jq("#buttonYearSubscribe").attr("data-id");
        TariffSettings.redirectToBuy(tariffId);
        return true;
    });

    jq(".tariff-buy-try").click(function () {
        TariffSettings.getTrial();
        return false;
    });

    jq(".tariff-buy-free").click(function () {
        TariffSettings.getFree();
        return false;
    });
});