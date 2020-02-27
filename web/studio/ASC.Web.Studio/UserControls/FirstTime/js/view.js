/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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

        var url = "default.aspx";
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
