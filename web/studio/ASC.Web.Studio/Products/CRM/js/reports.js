/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.CRM === "undefined")
    ASC.CRM = (function () { return {}; })();

ASC.CRM.Reports = (function() {

    var timeoutId = null;

    function changeCurrencyRate() {
        var obj = jq(this);
        var rate = Number(obj.val());

        if (rate <= 0) {
            obj.val("1.00");
        } else {
            obj.val(rate.toFixed(2));
        }
    }

    function addCurrencyRates() {
        var newCurrencyRates = [];

        jq("#currencyRatesDialog .currency-rate-item").each(function (index, item) {
            var obj = jq(item);

            newCurrencyRates.push({
                fromCurrency: obj.find("span:first-child").text().trim(),
                rate: obj.find("input").val().trim(),
                toCurrency: window.defaultCurrency
            });
        });

        Teamlab.addCrmCurrencyRates(null, newCurrencyRates,
            {
                before: function () {
                    jq("#currencyRatesDialog .button").addClass("disable");
                    LoadingBanner.displayLoading();
                },
                after: function () {
                    jq("#currencyRatesDialog .button").removeClass("disable");
                    LoadingBanner.hideLoading();
                },
                success: function (params, response) {
                    jq.unblockUI();
                    window.currencyRates = response;
                    toastr.success(ASC.CRM.Resources.CRMJSResource.SettingsUpdated);
                    jq("#generateBtn_Reports").click();
                },
                error: function (params, errors) {
                    console.log(errors);
                    toastr.error(errors[0]);
                }
            });
    }

    function initCurrencyRatesDialog(missingRates) {
        if (jq("#currencyRatesDialog").length) {
             jq("#currencyRatesDialog .content").empty();
        } else {
            jq.tmpl("template-blockUIPanel", {
                id: "currencyRatesDialog",
                headerTest: ASC.CRM.Resources.CRMReportResource.CurrencySettingsDialogHdr,
                questionText: "",
                innerHtmlText: ASC.CRM.Resources.CRMReportResource.CurrencySettingsDialogBody + "<br/><div class='content'></div>",
                OKBtn: ASC.CRM.Resources.CRMCommonResource.Save,
                OKBtnClass: "saveButton",
                CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel
            }).insertAfter(".reports-content-container");

            jq("#currencyRatesDialog").on("click", ".saveButton", addCurrencyRates);

            jq("#currencyRatesDialog").on("change", ".textEdit", changeCurrencyRate);
        }

        var data = [];

        jq(missingRates).each(function (index, item) {
            data.push({
                fromCurrency: item,
                rate: "1.00",
                toCurrency: window.defaultCurrency
            });
        });

        jq.tmpl("currencyRateItemTmpl", data).appendTo("#currencyRatesDialog .content");

        jq("#currencyRatesDialog .content .crm-deleteLink").remove();

        jq(data).each(function (index, item) {
            jq.forceNumber({
                parent: "#currencyRatesDialog",
                input: "#currencyRate_" + item.fromCurrency,
                integerOnly: false,
                positiveOnly: true
            });
        });
    };

    function showCurrencyDialod(missingRates) {
        displayLoading(false);

        initCurrencyRatesDialog(missingRates);

        StudioBlockUIManager.blockUI("#currencyRatesDialog", 400, 400, 0);
    }

    function initAttachments () {
        window.Attachments.init();

        window.Attachments.bind("deleteFile", function(ev, fileId) {
            Teamlab.removeCrmReportFile({ fileId: fileId }, fileId, {
                success: function(params) {
                    window.Attachments.deleteFileFromLayout(params.fileId);
                }
            });
        });

        window.Attachments.loadFiles();
    }

    function checkReport(reportType) {
        displayLoading(true);

        var data = {
            type: reportType,
            timePeriod: jq("#timePeriod_Reports").val(),
            managers: window.SelectedUsers_Reports.IDs
        };

        Teamlab.checkCrmReport(data,
            {
                success: function (params, response) {
                    if (response && response.hasData) {
                        if (response.missingRates && response.missingRates.length) {
                            ProgressDialog.close();
                            showCurrencyDialod(response.missingRates);
                        } else {
                            trackGoogleAnalytics(data.type);
                            ProgressDialog.generate(data);
                        }
                    } else {
                        displayLoading(false);
                        toastr.warning(ASC.CRM.Resources.CRMReportResource.NoData);
                    }
                }
            });
    }


    function displayLoading(display) {
        if (display) {
            LoadingBanner.showLoaderBtn(".reports-content-container");
        } else {
            LoadingBanner.hideLoaderBtn(".reports-content-container");
        }
    }

    function trackGoogleAnalytics(reportType) {
        var reportTypeName = "";

        for (var name in ASC.CRM.Data.ReportType) {
            if (reportType == ASC.CRM.Data.ReportType[name])
                reportTypeName = name;
        }

        trackingGoogleAnalytics(ga_Categories.reports, ga_Actions.generateNew, reportTypeName);
    }

    function setBindings(reportType, viewFiles) {
        jq("#menuCreateNewTask").bind("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });

        ProgressDialog.init(
            {
                header: ASC.CRM.Resources.CRMReportResource.ReportBuilding,
                footer: ASC.CRM.Resources.CRMReportResource.ReportBuildingInfo.format("<a class='link underline' href='reports.aspx'>", "</a>"),
                progress: ASC.CRM.Resources.CRMReportResource.ReportBuildingProgress
            },
            jq("#studioPageContent .mainPageContent .containerBodyBlock:first"),
            {
                terminate: Teamlab.terminateCrmReport,
                status: Teamlab.getCrmReportStatus,
                generate: Teamlab.generateCrmReport
            });


        if (viewFiles) {
            initAttachments();
            return;
        }

        if (!jq("#generateBtn_Reports").length)
            return;

        jq("#generateBtn_Reports").on("click", function() {
            if (jq(this).hasClass("disable")) return;

            checkReport(reportType);
        });

        jq("#timePeriod_Reports").tlCombobox({ align: 'left' });

        jq("#timePeriod_Reports").prev().find(".combobox-options").css("max-height", "none");

        ASC.CRM.UserSelectorListView.Init(
            "_Reports",
            "UserSelector_Reports",
            false,
            "",
            [],
            null,
            [],
            "#userSelectorContainer_Reports",
            false);

        jq("#selectedUsers_Reports").attr("style", "");
    }


    function init(reportType, viewFiles) {
        window.currencyRates = JSON.parse(window.currencyRates);

        setBindings(reportType, viewFiles);

        jq(".reports-menu-container, .reports-content-container").removeClass("display-none");

        jq(".loader-page").hide();
    }

    return {
        init: init
    };
})();