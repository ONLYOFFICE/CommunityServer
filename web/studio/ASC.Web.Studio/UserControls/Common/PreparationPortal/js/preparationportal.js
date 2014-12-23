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

ProgressManager = new function () {

    this.init = function () {
        showProgress(0);
        getProgress();
    };
    function showProgress(progress) {
        var $progressValue = $(".asc-progress-value"),
            $progressText = $(".asc-progress_percent");

        $progressValue.animate({ "width": progress + "%" });
        $progressText.text(progress + "% ");
    };

    function getProgress() {
        var type = getURLParam("type");

        switch (type) {
            case "0":
                AjaxPro.Backup.GetTransferProgress(function (response) {
                    if (response.error) {
                        showError(response.error.Message);
                        return;
                    } else if (response.value) {
                        showProgress(response.value.Progress);
                        if (response.value.IsCompleted) {
                            window.location.href = response.value.Link;
                        }
                    }
                    setTimeout(function () {
                        AjaxPro.Backup.GetTransferProgress(getProgress);
                    }, 1000);
                    
                });
                break;
            case "1":
                AjaxPro.Backup.GetRestoreProgress(function (response) {
                    if (response.error) {
                        showError(response.error.Message);
                        return;
                    }
                    if (response.value) {
                        showProgress(response.value.Progress);
                        if (response.value.IsCompleted) {
                            window.location.href = "./";
                        }
                    } else {
                        showProgress(100);
                        window.location.href = "./";
                    }
                    setTimeout(function () {
                        AjaxPro.Backup.GetRestoreProgress(getProgress);
                    }, 1000);
                });
                break;
                
        }
    };

    function showError(er) {
        $("#progress-line").hide();
        $("#progress-error").text(er).show();
    };

    function getURLParam (strParamName) {
        strParamName = strParamName.toLowerCase();

        var strReturn = "";
        var strHref = window.location.href.toLowerCase();
        var bFound = false;

        var cmpstring = strParamName + "=";
        var cmplen = cmpstring.length;

        if (strHref.indexOf("?") > -1) {
            var strQueryString = strHref.substr(strHref.indexOf("?") + 1);
            var aQueryString = strQueryString.split("&");
            for (var iParam = 0; iParam < aQueryString.length; iParam++) {
                if (aQueryString[iParam].substr(0, cmplen) == cmpstring) {
                    var aParam = aQueryString[iParam].split("=");
                    strReturn = aParam[1];
                    bFound = true;
                    break;
                }
            }
        }
        if (bFound == false) {
            return null;
        }

        if (strReturn.indexOf("#") > -1) {
            return strReturn.split("#")[0];
        }

        return strReturn;
    };
}

$(function () {
    ProgressManager.init();
});