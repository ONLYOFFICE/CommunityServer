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
                            setTimeout(function() {
                                    window.location.href = "./";
                                }, 5000);

                        }
                    } else {
                        showProgress(100);
                        setTimeout(function () {
                            window.location.href = "./";
                        }, 5000);
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