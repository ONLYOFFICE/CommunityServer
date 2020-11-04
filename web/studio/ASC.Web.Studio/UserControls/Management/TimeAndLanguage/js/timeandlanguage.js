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


var TimeAndLanguage = new function () {
    this.SaveLanguageTimeSettings = function () {
        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#studio_lngTimeSettings");
            } else {
                LoadingBanner.hideLoaderBtn("#studio_lngTimeSettings");
            }
        };
        var timeManager = new TimeAndLanguageContentManager();
        timeManager.SaveTimeLangSettings(function (res) {
            if (res.Status == "1") {
                LoadingBanner.showMesInfoBtn("#studio_lngTimeSettings", res.Message, "success");
                window.location.reload(true);
            } else {
                LoadingBanner.showMesInfoBtn("#studio_lngTimeSettings", res.Message, "error");
            }
        });
    };
};

TimeAndLanguageContentManager = function () {
    this.SaveTimeLangSettings = function (parentCallback) {
        Teamlab.setTimaAndLanguage(jq("#studio_lng").val() || jq('#studio_lng').data('default'), jq("#studio_timezone").val(), {
            success: function (params, response) {
                if (parentCallback != null)
                    parentCallback({Status: 1, Message: response});
            },
            error: function (params, response) {
                if (parentCallback != null)
                    parentCallback({Status: 0, Message: response[0]});
            }
        });
    };
};

jq(function () {
    var previous;

    if (jq("#NotFoundLanguage").length) {
        jq("#studio_lng").on('focus', function () {
            previous = this.value;
        }).change(function () {
            if (!this.value) {
                setTimeout(function() {
                    jq(".langTimeZoneBlock .HelpCenterSwitcher").helper({ BlockHelperID: 'NotFoundLanguage' });
                }, 0);
                this.value = previous;
                return false;
            } else {
                previous = this.value;
            }
        });
    }
});