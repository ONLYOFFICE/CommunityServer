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


/*
Copyright (c) Ascensio System SIA 2013. All rights reserved.
http://www.teamlab.com
*/
window.TariffPartner = (function () {
    var isInit = false;

    var init = function () {
        if (isInit) {
            return;
        }
        isInit = true;

        jq("#registrationKeyValue").on("keyup change paste", function (e) {
            setTimeout(changeCodeValue, 0);
            return e.type == "paste";
        });

        jq("#partnerPayKeyDialog").on("change", "input:radio", function () {
            var isKeyEnter = jq("#registrationKeyOption").prop("checked");
            selectOption(isKeyEnter);
        });

        jq("#partnerPayKeyDialog").on("click", ".tariff-key-aplly:not(.disable)", function () {
            activateKey();
            return false;
        });

        jq("#partnerPayKeyDialog").on("click", ".tariff-key-request:not(.disable)", function () {
            requestKey();
            return false;
        });

        jq(".tariffs-button-block").on("click", ".tariff-pay-pal:not(.disable)", function () {
            payPal();
            return false;
        });
        
        jq("#partnerPayKeyDialog").on("click", ".tariff-key-cancel:not(.disable)", function () {
            PopupKeyUpActionProvider.CloseDialog();
            return false;
        });

        jq(".tariffs-button-block").on("click", ".tariff-pay-key:not(.disable)", function () {
            TariffPartner.showPayKeyDialog();
            return false;
        });
    };

    var validKey = function (key) {
        return key && key.length > 0 && key.length < 256;
    };

    var showPayKeyDialog = function () {
        jq("#registrationKeyValue").val("");
        jq(".tariff-key-aplly, .tariff-key-request, .tariff-key-cancel").removeClass("disable");
        jq("#partnerPayKeyDialog input").prop("disabled", false);
        LoadingBanner.hideLoaderBtn("#partnerPayKeyDialog");
        jq("#partnerPayKeyDialog .error-popup").hide();
        changeCodeValue();
        TariffSettings.selectTariff();

        StudioBlockUIManager.blockUI("#partnerPayKeyDialog", 350, 300, 0);

        selectOption(true);
    };

    var selectOption = function (isKeyEnter) {
        isKeyEnter = isKeyEnter === true;
        jq("#registrationKeyOption").prop("checked", isKeyEnter);
        jq("#registrationRequestOption").prop("checked", !isKeyEnter);

        jq(".tariff-key-aplly").toggle(isKeyEnter);
        jq(".tariff-key-request").toggle(!isKeyEnter);
        
        if (isKeyEnter) {
            jq("#registrationKeyValue").focus();
        }
    };

    var changeCodeValue = function () {
        var key = jq("#registrationKeyValue").val().trim();

        jq(".tariff-key-aplly").toggleClass("disable", !validKey(key));
        selectOption(true);
    };

    var activateKey = function () {
        var key = jq("#registrationKeyValue").val().trim();
        if (!validKey(key)) {
            return;
        }

        jq(".tariff-key-aplly, .tariff-key-request, .tariff-key-cancel").addClass("disable");
        jq("#partnerPayKeyDialog input").prop("disabled", true);
        LoadingBanner.showLoaderBtn("#partnerPayKeyDialog");
        jq("#partnerPayKeyDialog .error-popup").hide();
        
        var timeout = AjaxPro.timeoutPeriod;
        AjaxPro.timeoutPeriod = 60 * 1000;
        TariffPartnerController.ActivateKey(key,
            function (result) {
                jq(".tariff-key-aplly, .tariff-key-request, .tariff-key-cancel").removeClass("disable");
                jq("#partnerPayKeyDialog input").prop("disabled", false);
                LoadingBanner.hideLoaderBtn("#partnerPayKeyDialog");
                jq("#partnerPayKeyDialog .error-popup").hide();
                if (result.error != null) {
                    jq("#partnerPayKeyDialog .error-popup").show().text(ASC.Resources.Master.Resource.ErrorKeyActivation);
                    return;
                }

                PopupKeyUpActionProvider.EnterAction = "PopupKeyUpActionProvider.CloseDialog();";
                PopupKeyUpActionProvider.CloseDialogAction = "location.reload();";
                StudioBlockUIManager.blockUI("#partnerApplyDialog", 350, 300, 0);
                setTimeout("location.reload();", 5000);
                AjaxPro.timeoutPeriod = timeout;
            });
    };

    var requestKey = function () {
        jq(".tariff-key-aplly, .tariff-key-request, .tariff-key-cancel").addClass("disable");
        jq("#partnerPayKeyDialog input").prop("disabled", true);
        LoadingBanner.showLoaderBtn("#partnerPayKeyDialog");
        jq("#partnerPayKeyDialog .error-popup").hide();
        
        var quotaId = TariffSettings.selectedQuotaId();
        TariffPartnerController.RequestKey(quotaId,
            function (result) {
                jq(".tariff-key-aplly, .tariff-key-request, .tariff-key-cancel").removeClass("disable");
                jq("#partnerPayKeyDialog input").prop("disabled", false);
                jq("#partnerPayKeyDialog .error-popup").hide();
                LoadingBanner.hideLoaderBtn("#partnerPayKeyDialog");
                if (result.error != null) {
                    jq("#partnerPayKeyDialog .error-popup").show().text(result.error.Message);
                    return;
                }

                PopupKeyUpActionProvider.EnterAction = "PopupKeyUpActionProvider.CloseDialog();";
                StudioBlockUIManager.blockUI("#partnerRequestDialog", 350, 300, 0);
            });
    };

    var payPal = function () {
        var quotaId = TariffSettings.selectedQuotaId();
        TariffPartnerController.RequestPayPal(quotaId,
            function (result) {
                var res = result.error || result.value;
                if (res.Message || res.message) {
                    jq("#partnerPayExceptionText").html(res.Message || res.message);
                    PopupKeyUpActionProvider.EnterAction = "PopupKeyUpActionProvider.CloseDialog();";
                    StudioBlockUIManager.blockUI("#partnerPayExceptionDialog", 350);
                    return;
                }

                if (result.value.rs2 == "quotaexceed") {
                    TariffSettings.showDowngradeDialog();
                    return;
                }

                location.href = result.value.rs1;
            });
    };

    return {
        init: init,

        showPayKeyDialog: showPayKeyDialog
    };
})();

jq(function () {
    TariffPartner.init();
});
