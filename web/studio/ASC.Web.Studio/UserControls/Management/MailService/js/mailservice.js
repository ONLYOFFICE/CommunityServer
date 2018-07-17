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
    var sqlip;
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

        var serverIp = jq("#mailServiceIp").val().trim();
        var sqlIp = jq("#mailServiceSqlIp").val().trim();
        var usr = jq("#mailServiceUser").val().trim();
        var pwd = jq("#mailServicePassword").val().trim();
        var valid = true;

        if (!serverIp) {
            jq("#mailServiceIp").parent().addClass("requiredFieldError");
            valid = false;
        } else {
            jq("#mailServiceIp").parent().removeClass("requiredFieldError");
        }

        if (!usr) {
            jq("#mailServiceUser").parent().addClass("requiredFieldError");
            valid = false;
        } else {
            jq("#mailServiceUser").parent().removeClass("requiredFieldError");
        }

        if (!pwd) {
            jq("#mailServicePassword").parent().addClass("requiredFieldError");
            valid = false;
        } else {
            jq("#mailServicePassword").parent().removeClass("requiredFieldError");
        }

        if (!valid) return;

        Teamlab.connectMailServerInfo(null, serverIp, sqlIp || serverIp, usr, pwd, {
            before: function() {
                jq("#mailServiceBlock input").prop("disabled", true);
                LoadingBanner.showLoaderBtn("#mailServiceBlock");
            },
            success: function (params, res) {
                LoadingBanner.showMesInfoBtn("#mailServiceBlock", res.message, res.status);
                if (res.status == "success") {
                    ip = res.ip;
                    sqlip = res.sqlip;
                    user = res.user;
                    password = res.password;
                    token = res.token;
                    host = res.host;
                    jq("#mailServiceSaveBtn").removeClass("disable");
                } else {
                    ip = null;
                    sqlip = null;
                    user = null;
                    password = null;
                    token = null;
                    host = null;
                    jq("#mailServiceSaveBtn").addClass("disable");
                }
            },
            error: function (params, error) {
                LoadingBanner.showMesInfoBtn("#mailServiceBlock", error[0], "error");
            },
            after: function() {
                jq("#mailServiceBlock input").prop("disabled", false);
                LoadingBanner.hideLoaderBtn("#mailServiceBlock");
            }
        });
    };

    var save = function () {

        if (!ip || !user || !password || !token || !host) return;

        Teamlab.saveMailServerInfo(null, ip, sqlip || ip, user, password, token, host, {
            before: function () {
                jq("#mailServiceBlock input").prop("disabled", true);
                LoadingBanner.showLoaderBtn("#mailServiceBlock");
            },
            success: function (params, res) {
                LoadingBanner.showMesInfoBtn("#mailServiceBlock", res.message, res.status);
                if (res.status == "success") {
                    jq("#mailServiceLink").removeClass("display-none");
                } else {
                    jq("#mailServiceLink").addClass("display-none");
                }
            },
            error: function (params, error) {
                LoadingBanner.showMesInfoBtn("#mailServiceBlock", error[0], "error");
            },
            after: function () {
                jq("#mailServiceBlock input").prop("disabled", false);
                LoadingBanner.hideLoaderBtn("#mailServiceBlock");
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