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


var CoBrandingManager = new function () {

    var _uploadCoBrandingLogoComplete = function (file, response, logotype) {
        //jq.unblockUI();

        var result = eval("(" + response + ")");
        if (result.Success) {
            jq('#studio_coBrandingSettings .logo_' + logotype).attr('src', result.Message);
            jq('#logoPath_' + logotype).val(result.Message);
        } else {
            toastr.error(result.Message);
        }
        LoadingBanner.hideLoaderBtn("#studio_coBrandingSettings");
    };

    var _updateCoBrandingLogosSrc = function (coBrandingLogos) {
        for (var logo in coBrandingLogos) {
            if (coBrandingLogos.hasOwnProperty(logo)) {
                var now = new Date();
                jq('#studio_coBrandingSettings .logo_' + logo).attr('src', coBrandingLogos[logo] + '?' + now.getTime());
            }
        }
    }

    var _bindAjaxProOnLoading = function () {
        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#studio_coBrandingSettings");
            } else {
                LoadingBanner.hideLoaderBtn("#studio_coBrandingSettings");
            }
        };
    }

    this.SaveCoBrandingOptions = function () {
        var logoTypeList = [],
            logoPathList = [],
            $logoPaths = jq('[id^=logoPath_]'),
            needToSave = false;

        for (var i = 0, n = $logoPaths.length; i < n; i++) {
            var logotype = jq($logoPaths[i]).attr('id').split('_')[1],
                logoPath = jq.trim(jq($logoPaths[i]).val());
            logoTypeList.push(logotype);
            logoPathList.push(logoPath);
            if (logoPath != "") { needToSave = true; }
        }

        if (needToSave) {
            _bindAjaxProOnLoading();

            AjaxPro.CoBranding.SaveCoBrandingSettings(
                CoBrandingManager.IsRetina,
                logoTypeList,
                logoPathList,
                    function (result) {
                        //clean logo path input
                        jq('[id^=logoPath_]').val('');

                        if (result.value.Status == 1) {
                            window.location.reload(true);
                        }
                        LoadingBanner.showMesInfoBtn("#studio_coBrandingSettings", result.value.Message, result.value.Status == 1 ? "success" : "error");
                    });
        }
    };

    this.RestoreCoBrandingOptions = function () {
        _bindAjaxProOnLoading();

        AjaxPro.CoBranding.RestoreCoBrandingOptions(CoBrandingManager.IsRetina, function (result) {
            //clean logo path input
            jq('[id^=logoPath_]').val('');

            if (result.value.Status == 1) {
                //var defaultLogos = jq.parseJSON(result.value.LogoPath);
                //_updateCoBrandingLogosSrc(defaultLogos);
                window.location.reload(true);
            }
            LoadingBanner.showMesInfoBtn("#studio_coBrandingSettings", result.value.Message, result.value.Status == 1 ? "success" : "error");
        });
    };

    this.Init = function () {
        var isRetina = jq.cookies.get("is_retina");

        CoBrandingManager.IsRetina = isRetina != null && isRetina == true;

        jq('#saveCoBrandingSettingsBtn').on("click", function () { CoBrandingManager.SaveCoBrandingOptions(); });
        jq('#restoreCoBrandingSettingsBtn').on("click", function () { CoBrandingManager.RestoreCoBrandingOptions(); });


        var $uploaderBtns = jq('[id^=logoUploader_]');

        for (var i = 0, n = $uploaderBtns.length; i < n; i++) {
            var logotype = jq($uploaderBtns[i]).attr('id').split('_')[1];

            new AjaxUpload($uploaderBtns[i], {
                action: 'ajaxupload.ashx?type=ASC.Web.Studio.UserControls.CoBranding.LogoUploader,ASC.Web.Studio',
                data: { logotype: logotype },
                onChange: function (file, ext) {
                    LoadingBanner.showLoaderBtn("#studio_coBrandingSettings");
                },
                onComplete: function (file, response) {
                    _uploadCoBrandingLogoComplete(file, response, this._settings.data.logotype);
                }
            });
        }
     
    };

};


jq(function () {
    CoBrandingManager.Init();
});