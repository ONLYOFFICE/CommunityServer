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


var GreetingLogoSettingsManager = new function () {

    var _bindAjaxProOnLoading = function () {
        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#studio_greetingLogoSettings");
            } else {
                LoadingBanner.hideLoaderBtn("#studio_greetingLogoSettings");
            }
        };
    };

    var _saveGreetingLogoOptions = function () {
        _bindAjaxProOnLoading();
        GreetingLogoSettingsController.SaveGreetingLogoSettings(jq('#studio_greetingLogoPath').val(),
                                                function (result) {
                                                    //clean logo path input
                                                    jq('#studio_greetingLogoPath').val('');
                                                    LoadingBanner.showMesInfoBtn("#studio_greetingLogoSettings", result.value.Message, result.value.Status == 1 ? "success" : "error");
                                                });
      };

    var _restoreGreetingLogoOptions = function () {
        _bindAjaxProOnLoading();
        GreetingLogoSettingsController.RestoreGreetingLogoSettings(function (result) {
            //clean logo path input
            jq('#studio_greetingLogoPath').val('');

            if (result.value.Status == 1) {
                jq('#studio_greetingLogo').attr('src', result.value.LogoPath);
            }
            LoadingBanner.showMesInfoBtn("#studio_greetingLogoSettings", result.value.Message, result.value.Status == 1 ? "success" : "error");
        });
    };

    var _uploadCompleteGreetingLogo = function (file, response) {
        LoadingBanner.hideLoaderBtn("#studio_greetingLogoSettings");

        var result = eval("(" + response + ")");

        if (result.Success) {
            jq('#studio_greetingLogo').attr('src', result.Message);
            jq('#studio_greetingLogoPath').val(result.Message);
        } else {
            LoadingBanner.showMesInfoBtn("#studio_greetingLogoSettings", result.Message, "error");
        }
    };

    this.Init = function () {
        jq('#saveGreetLogoSettingsBtn').on("click", _saveGreetingLogoOptions);
        jq('#restoreGreetLogoSettingsBtn').on("click", _restoreGreetingLogoOptions);

        if (jq('#studio_logoUploader').length > 0) {
            new AjaxUpload('studio_logoUploader', {
                action: 'ajaxupload.ashx?type=ASC.Web.Studio.UserControls.Management.LogoUploader,ASC.Web.Studio',
                onChange: function (file, ext) {
                    LoadingBanner.showLoaderBtn("#studio_greetingLogoSettings");
                },
                onComplete: _uploadCompleteGreetingLogo
            });
        }

    }
};


jq(function () {
    GreetingLogoSettingsManager.Init();
});