/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

    var progressStatus = {
        Queued: 0,
        Started: 1,
        Done: 2,
        Failed: 3
    };

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

        Teamlab.checkCrmReport({}, data,
            {
                success: function (params, response) {
                    if (response && response.hasData) {
                        if (response.missingRates && response.missingRates.length) {
                            closeProgressDialod();
                            showCurrencyDialod(response.missingRates);
                        } else {
                            generateReport(data);
                        }
                    } else {
                        displayLoading(false);
                        toastr.warning(ASC.CRM.Resources.CRMReportResource.NoData);
                    }
                },
                error: generateReportErrorCallback
            });
    }

    function generateReport(data) {

        trackGoogleAnalitics(data.type);

        Teamlab.generateCrmReport({}, data,
            {
                success: generateReportSuccessCallback,
                error: generateReportErrorCallback
            });
    }

    function getCrmReportStatus() {

        displayLoading(true);

        Teamlab.getCrmReportStatus({},
            {
                success: generateReportSuccessCallback,
                error: generateReportErrorCallback
            });
    }

    function terminateCrmReport() {

        Teamlab.terminateCrmReport({},
            {
                success: function() {
                    clearTimeout(timeoutId);
                    displayLoading(false);
                    closeProgressDialod();
                },
                error: generateReportErrorCallback
            });
    }

    function generateReportSuccessCallback(params, response) {
        if (!response || jq.isEmptyObject(response)) {
            displayLoading(false);
            return;
        }

        renderRow(response);

        if (response.status == progressStatus.Queued) {
            showProgressDialod();
            changeHeaderText(ASC.CRM.Resources.CRMReportResource.ReportBuilding);
            setRowProgress(response.id, response.percentage);
            showRowStartedStatus(response.id);
            timeoutId = setTimeout(getCrmReportStatus, 1000);
            return;
        }

        if (response.status == progressStatus.Started) {
            showProgressDialod();
            changeHeaderText(ASC.CRM.Resources.CRMReportResource.ReportBuildingProgress.format(response.percentage));
            setRowProgress(response.id, response.percentage);
            showRowStartedStatus(response.id);
            timeoutId = setTimeout(getCrmReportStatus, 1000);
            return;
        }

        if (response.status == progressStatus.Failed) {
            displayLoading(false);
            showProgressDialod();
            changeHeaderText(ASC.CRM.Resources.CRMReportResource.ReportBuilding);
            setRowProgress(response.id, response.percentage);
            showRowFailedStatus(response.id, response.errorText);
            toastr.error(response.errorText);
            return;
        }

        if (response.status == progressStatus.Done) {
            displayLoading(false);
            showProgressDialod();
            changeHeaderText(ASC.CRM.Resources.CRMReportResource.ReportBuilding);
            setRowProgress(response.id, response.percentage);
            showRowDoneStatus(response.id, ASC.Files.Utility.GetFileWebEditorUrl(response.fileId));
        }
    }

    function generateReportErrorCallback(params, errors) {
        displayLoading(false);
        toastr.error(errors[0]);
        console.log(errors);
    }

    function displayLoading(display) {
        if (display) {
            LoadingBanner.showLoaderBtn(".reports-content-container");
        } else {
            LoadingBanner.hideLoaderBtn(".reports-content-container");
        }
    }

    function trackGoogleAnalitics(reportType) {
        var reportTypeName = "";

        for (var name in ASC.CRM.Data.ReportType) {
            if (reportType == ASC.CRM.Data.ReportType[name])
                reportTypeName = name;
        }

        trackingGoogleAnalitics(ga_Categories.reports, ga_Actions.generateNew, reportTypeName);
    }

    function setBindings(reportType, viewFiles) {

        jq("#menuCreateNewTask").bind("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });

        jq("#bottomLoaderPanel").draggable(
            {
                axis: "x",
                handle: ".progress-dialog-header",
                containment: "body"
            }
        );

        jq("#reportProgressDialog .actions-container.close").click(terminateCrmReport);

        jq("#reportProgressDialog").on("click", ".progress-error", showErrorText);

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

        jq("#timePeriod_Reports").tlCombobox();

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

    function renderRow (data) {

        function replaceSpecCharacter(str) {
            var characterRegExp = new RegExp("[\t*\+:\"<>?|\\\\/]", "gim");
            return (str || "").trim().replace(characterRegExp, "_");
        }

        if (jq("#" + data.id).length) return;

        data.fileName = replaceSpecCharacter(data.fileName);
        data.fileTypeCssClass = ASC.Files.Utility.getCssClassByFileTitle(data.fileName, true);

        jq("#reportsTable tbody").append(jq.tmpl("reportProgressRowTmpl", data));
        jq("#reportsTable").parent().scrollTo("#" + data.id);
    }

    function setRowProgress(id, percentage)
    {
        function setProgressValue(progressBar, value) {
            value = value | 0;
            progressBar = jq(progressBar);
            if (!progressBar.is("progress")) {
                progressBar = progressBar.find("progress");
            }

            var dt = 50;
            var timer = progressBar.data("timer");
            clearInterval(timer);

            var curValue = progressBar.val();
            if (!value || curValue > value) {
                progressBar.val(value);
            } else {
                var nextProgressValue = function (dValue, maxValue) {
                    var v = Math.min(maxValue, progressBar.val() + dValue);
                    progressBar.val(v);
                    if (v == maxValue) {
                        clearInterval(timer);
                    }
                };

                var dV = Math.max(1, (value - curValue) / dt);
                timer = setInterval(function () {
                    nextProgressValue(dV, value);
                }, 1);
                progressBar.data("timer", timer);
            }

            var prValue = progressBar.find(".asc-progress-value");

            if (!value) {
                prValue.css("width", value + "%");
            } else {
                prValue.animate({ "width": value + "%" });
            }

            progressBar.next().text(value + "%");
        }

        setProgressValue("#" + id + " progress", percentage);
    }

    function showRowStartedStatus (id) {
        jq("#" + id)
            .removeClass("done")
            .removeClass("error")
            .addClass("started");
    }

    function showRowFailedStatus(id, errorText) {
        jq("#" + id)
            .removeClass("started")
            .removeClass("done")
            .addClass("error")
            .removeAttr("id")
            .find(".popup_helper").text(errorText);
    }

    function showRowDoneStatus(id, url) {
        jq("#" + id)
            .removeClass("started")
            .removeClass("error")
            .addClass("done")
            .removeAttr("id")
            .find(".linkMedium").attr("href", url);
    }

    function showProgressDialod () {
        jq("#reportProgressDialog").show();
    }

    function closeProgressDialod () {
        jq("#reportProgressDialog").hide();
        jq("#reportsTable tbody").empty();
    }

    function changeHeaderText (text) {
        jq("#reportProgressDialogHeader").text(text);
    };

    function showErrorText () {
        var rowIndex = jq(this).closest(".rp-row").index() + 1;

        jq(this).helper({
            BlockHelperID: "reportsTable tr:nth-child(" + rowIndex + ") .popup_helper",
            position: "fixed"
        });
    }

    function init(reportType, viewFiles) {

        window.currencyRates = JSON.parse(window.currencyRates);

        setBindings(reportType, viewFiles);

        getCrmReportStatus();

        jq(".reports-menu-container, .reports-content-container").removeClass("display-none");

        jq(".loader-page").hide();
    }

    return {
        init: init
    };
})();