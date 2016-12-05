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

if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.Settings === "undefined")
    ASC.Settings = {};

ASC.Settings.MailService = (function () {

    var ip;
    var user;
    var password;
    var token;
    var host;

    var init = function () {
        jq(".settings-switcher-btn").click(function () {
            jq(".settings-switcher-btn, .settings-switcher-content").toggleClass("display-none");
        });

        jq("#mailServiceConnectBtn").click(function () {
            if (jq(this).hasClass("disable")) return;
            connect();
        });
        
        jq("#mailServiceSaveBtn").click(function () {
            if (jq(this).hasClass("disable")) return;
            save();
        });
    };

    var connect = function () {

        AjaxPro.onLoading = function (b) {
            if (b) {
                jq("#mailServiceBlock input").prop("disabled", true);
                LoadingBanner.showLoaderBtn("#mailServiceBlock");
            } else {
                jq("#mailServiceBlock input").prop("disabled", false);
                LoadingBanner.hideLoaderBtn("#mailServiceBlock");
            }
        };

        var serverIp = jq("#mailServiceIp").val().trim();
        var usr = jq("#mailServiceUser").val().trim();
        var pwd = jq("#mailServicePassword").val().trim();

        if (!serverIp || !usr || !pwd) return;

        window.MailService.Connect(serverIp, usr, pwd, function (result) {
            var res = result.value;
            LoadingBanner.showMesInfoBtn("#mailServiceBlock", res.Message, res.Status);
            if (res.Status == "success") {
                ip = res.Ip;
                user = res.User;
                password = res.Password;
                token = res.Token;
                host = res.Host;
                jq("#mailServiceSaveBtn").removeClass("disable");
            } else {
                ip = null;
                user = null;
                password = null;
                token = null;
                host = null;
                jq("#mailServiceSaveBtn").addClass("disable");
            }
        });
    };

    var save = function () {

        AjaxPro.onLoading = function (b) {
            if (b) {
                jq("#mailServiceBlock input").prop("disabled", true);
                LoadingBanner.showLoaderBtn("#mailServiceBlock");
            } else {
                jq("#mailServiceBlock input").prop("disabled", false);
                LoadingBanner.hideLoaderBtn("#mailServiceBlock");
            }
        };

        if (!ip || !user || !password || !token || !host) return;

        window.MailService.Save(ip, user, password, token, host, function (result) {
            var res = result.value;
            LoadingBanner.showMesInfoBtn("#mailServiceBlock", res.Message, res.Status);
            if (res.Status == "success") {
                jq("#mailServiceLink").removeClass("display-none");
            } else {
                jq("#mailServiceLink").addClass("display-none");
            }
        });
    };

    return {

        init: init
        
    };
})();

jq(function () {
    ASC.Settings.MailService.init();
});