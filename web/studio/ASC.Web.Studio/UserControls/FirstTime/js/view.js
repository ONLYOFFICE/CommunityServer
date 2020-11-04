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


if (typeof (ASC) == 'undefined')
    ASC = {};
if (typeof (ASC.Controls) == 'undefined')
    ASC.Controls = {};

ASC.Controls.FirstTimeView = new function() {

    this.Finish = function() {
        window.onbeforeunload = null;

        try {
            if (window.ga) {
                window.ga('www.send', 'pageview', '/wizard_finish');
                window.ga('www.send', 'event', 'onlyoffice_adwords', 'wizard_finish');
                window.ga('testTracker.send', 'pageview', '/wizard_finish');
                window.ga('testTracker.send', 'event', 'onlyoffice_adwords', 'wizard_finish');
            }
            window.uetq = window.uetq || [];
            window.uetq.push({ 'ec': 'onlyoffice_msn', 'ea': 'clickto', 'el': 'onbutton', 'ev': '1' });
        } catch(err) {
        }

        var url = "Default.aspx";
        if (jq("body").is(".desktop")) {
            url += "?desktop=true&first=true";
        }

        location.href = url;
    };

    this.SaveRequiredStep = function() {

        AjaxPro.onLoading = function(b) {
            if (b)
                LoadingBanner.showLoaderBtn(".wizardContent");
            else
                LoadingBanner.hideLoaderBtn(".wizardContent");
        };

        var FTManager = new ASC.Controls.FirstTimeManager();
        FTManager.SaveRequiredData(this.SaveRequiredStepCallback);
    };

    this.SaveRequiredStepCallback = function(result) {

        if (result.Status == 1) {
            var time = new TimeAndLanguageContentManager();
            time.SaveTimeLangSettings(ASC.Controls.FirstTimeView.SaveTimeLangSettingsCallback);
        } else {
            ASC.Controls.FirstTimeView.ShowOperationInfo(result);
        }
    };

    this.SaveTimeLangSettingsCallback = function(result) {
        if (result.Status == 1) {
            ASC.Controls.FirstTimeView.Finish(); // enter redirect on the dash of modules
        } else {
            ASC.Controls.FirstTimeView.ShowOperationInfo(result);
            window.onbeforeunload = function(e) {
                return ASC.Resources.Master.Resource.WizardCancelConfirmMessage;
            };
        }
    };

    this.ShowOperationInfo = function(result) {
        if (result.Status == 1)
            toastr.success(result.Message);
        else
            toastr.error(result.Message);
    };
};


jq(document).ready(function() {
    jq(document).keyup(function (e) {
    if (e.which == 13) {
        ASC.Controls.FirstTimeView.SaveRequiredStep();
        }
    });
    window.onbeforeunload = function(e) {
    return ASC.Resources.Master.Resource.WizardCancelConfirmMessage;
    };
});
