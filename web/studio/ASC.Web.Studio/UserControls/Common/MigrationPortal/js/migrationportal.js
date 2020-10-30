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


window.MigrationPortal = new function () {
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
        Teamlab.getStorageProgress({},
        {
            success: function (params, progress) {
                if (progress === -1) {
                    showProgress(100);
                    completeReload();
                    return;
                }

                var percentage = Math.round(Number(progress));
                showProgress(percentage);
                if (percentage === 100) {
                    completeReload();
                }

                setTimeout(function () {
                    getProgress();
                }, 1000);
            },
            error: function (params, errors) {
                showError(errors[0]);
            }
        });
    };

    function showError(er) {
        $("#progress-line").hide();
        $("#progress-error").text(er).show();
    };

    function completeReload() {
        jQuery("body").css("cursor", "wait");
        setTimeout(function () {
            window.location.href = "/";
        }, 5000);
    }
}

$(function () {
    ServiceHelper.init("/api/2.0/");
    MigrationPortal.init();
});