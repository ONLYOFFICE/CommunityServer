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

        StudioBlockUIManager.blockUI("#currencyRatesDialog", 400);
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
                footer: ASC.CRM.Resources.CRMReportResource.ReportBuildingInfo.format("<a class='link underline' href='Reports.aspx'>", "</a>"),
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