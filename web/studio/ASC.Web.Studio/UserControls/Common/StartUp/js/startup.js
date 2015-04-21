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


ProgressStartUpManager = new function () {
    var scripts = [], returnUrl, progress = 0, totalScripts, percentage;

    this.init = function (currentProgress) {
        AjaxPro.StartUp.Start();
        progress = currentProgress;
        showProgress();
        getProgress();
        jQuery.fn.ready = function (fn) { };
        DocsAPI = {};
    };

    function showProgress() {
        var $progressValue = jq(".asc-progress-value"),
            $progressText = jq(".asc-progress_percent");

        $progressValue.animate({ "width": progress + "%" });
        $progressText.text(Math.floor(progress) + "% ");
    };

    function showError(er) {
        jq("#progress-line").hide();
        jq("#progress-error").text(er).show();
    };

    function getProgress() {
        AjaxPro.timeoutPeriod = 120000;
        AjaxPro.onError = function () {
            setTimeout(getProgress, 2000);
        };

        AjaxPro.StartUp.GetStartUpProgress(function (response) {
            if (response.error) {
                //showError(response.error.Message);
                location.reload();
            } else if (response.value) {
                var newProgress = Math.floor(response.value.ProgressPercent);
                if (newProgress > progress) {
                    showProgress();
                    progress = newProgress;
                }

                if (response.value.Completed) {
                    returnUrl = response.value.Link;
                    scripts = response.value.Bundles;
                    totalScripts = scripts.length;
                    percentage = parseInt(response.value.Percentage);
                    getScript();
                } else {
                    setTimeout(getProgress, 2000);
                }
            }
        });
    }

    function getScript() {
        var item = scripts.pop();
        if (!item) {
            window.location.href = returnUrl;
            return;
        }

        jq.ajax({ url: item }).always(function () {
            progress += (100 - percentage) / totalScripts;
            showProgress();
            getScript();
        });
    }
};