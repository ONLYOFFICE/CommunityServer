/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

CookieSettingsManager = new function () {
    this.Init = function () {
        jq("#saveCookieSettingsBtn").click(CookieSettingsManager.Save);
        
        jq("#lifeTimeTxt").keydown(function (e) {
            // Allow: backspace, delete, tab, escape, enter and .
            if (jq.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
                // Allow: Ctrl+A, Command+A
                (e.keyCode === 65 && (e.ctrlKey === true || e.metaKey === true)) || 
                // Allow: home, end, left, right, down, up
                (e.keyCode >= 35 && e.keyCode <= 40)) {
                // let it happen, don't do anything
                return;
            }
            // Ensure that it is a number and stop the keypress
            if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                e.preventDefault();
            }
        });
        
        jq("#cookieSettingsOff").on("click", CookieSettingsManager.HideLifeTimeSettings);
        
        jq("#cookieSettingsOn").on("click", CookieSettingsManager.ShowLifeTimeSettings);
    };

    this.Save = function () {
        AjaxPro.onLoading = function (b) {
            if (b)
                LoadingBanner.showLoaderBtn("#studio_cookieSettings");
            else
                LoadingBanner.hideLoaderBtn("#studio_cookieSettings");
        };

        var callback = function (result) {
            var res = result.value;
            if (res.Status == 1) {
                LoadingBanner.showMesInfoBtn("#studio_cookieSettings", res.Message, "success");
            } else {
                LoadingBanner.showMesInfoBtn("#studio_cookieSettings", res.Message, "error");
            }
        };

        if (jq("#cookieSettingsOn").is(":checked")) {
            var lifeTime = parseInt(jq("#lifeTimeTxt").val().trim());
            if (lifeTime > 0) {
                window.CookieSettingsController.Save(lifeTime, callback);
            }
        } else {
            window.CookieSettingsController.Restore(callback);
        } 
    };

    this.HideLifeTimeSettings = function () {
        jq("#lifeTimeSettings").addClass("display-none");
    };
    
    this.ShowLifeTimeSettings = function () {
        jq("#lifeTimeSettings").removeClass("display-none");
    };
};

jq(document).ready(function () {
    CookieSettingsManager.Init();
});