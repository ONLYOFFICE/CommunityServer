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

ProgressStartUpManager = new function() {
    var scripts = [], returnUrl, progress = 0, totalScripts;
    
    this.init = function() {
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
        AjaxPro.onError = function() {
            setTimeout(getProgress, 2000);
        };

        AjaxPro.StartUp.GetStartUpProgress(function (response) {
            console.log(response);
            if (response.error) {
                showError(response.error.Message);
                return;
            } else if (response.value) {
                progress = response.value.ProgressPercent;
                showProgress();
                
                if (response.value.IsCompleted) {
                    returnUrl = response.value.Link;
                    scripts = response.value.Bundles;
                    totalScripts = scripts.length;
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
            progress += 25 / totalScripts;
            showProgress();
            getScript();
        });
    }
};