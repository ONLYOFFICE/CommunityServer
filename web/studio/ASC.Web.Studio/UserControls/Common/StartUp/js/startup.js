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
    var progressTimeout;

    this.init = function (currentProgress) {
        progress = currentProgress.ProgressPercent;
        showProgress();
        getProgress();
        jQuery.fn.ready = function (fn) { };
        DocsAPI = {};
    };

    function showProgress() {
        var $progressValue = jq(".asc-progress-value"),
            $progressText = jq(".asc-progress_percent");
        
        $progressValue.animate({ "width": Math.floor(progress) + "%"}, Math.round(progress) == 100 ? 100 : 300 );
        $progressText.text(Math.floor(progress) + "% ");
    };

    function getProgress() {
        AjaxPro.timeoutPeriod = 120000;
        AjaxPro.onError = getProgressDelay;

        AjaxPro.StartUp.GetStartUpProgress(getProgressResponseHandler);
    }

    function getProgressDelay() {
        setTimeout(getProgress, 2000);
    }

    function getProgressResponseHandler(response) {
        if (response.error || !response.value) {
            location.reload();
            return;
        }

        var responseProgress = JSON.parse(response.value);
        var newProgress = Math.floor(responseProgress.ProgressPercent);
        if (newProgress > progress) {
            progress = newProgress;
            showProgress();

            if (progressTimeout) {
                clearTimeout(progressTimeout);
                progressTimeout = 0;
            }

        } else if (!progressTimeout) {
            progressTimeout = setTimeout(function() { location.reload(); }, 60000);
        }

        if (responseProgress.Completed) {
            onComplete(responseProgress);
        } else {
            getProgressDelay();
        }
    }

    function onComplete(responseProgress) {
        returnUrl = responseProgress.Link;
        scripts = responseProgress.Bundles;
        totalScripts = scripts.length;
        percentage = parseInt(responseProgress.Percentage);
        getScript();
    }

    function getScript() {
        var item = scripts.pop();
        if (!item) {
            window.location.href = returnUrl;
            return;
        }

        jq.ajax({ url: item, dataType: "text" }).always(function () {
            progress += (100 - percentage) / totalScripts;
            if (Math.floor(progress) % 2 == 0) {
                showProgress();
            }

            getScript();
        });
    }
};