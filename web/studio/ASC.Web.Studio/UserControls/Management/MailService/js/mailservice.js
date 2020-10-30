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


if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.Settings === "undefined")
    ASC.Settings = {};

ASC.Settings.MailService = (function () {

    var ip;
    var sqlip;
    var database;
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
        var db = jq("#mailServiceDatabase").val().trim();
        var usr = jq("#mailServiceUser").val().trim();
        var pwd = jq("#mailServicePassword").val().trim();
        var valid = true;

        if (!serverIp) {
            jq("#mailServiceIp").parent().addClass("requiredFieldError");
            valid = false;
        } else {
            jq("#mailServiceIp").parent().removeClass("requiredFieldError");
        }

        if (!db) {
            jq("#mailServiceDatabase").parent().addClass("requiredFieldError");
            valid = false;
        } else {
            jq("#mailServiceDatabase").parent().removeClass("requiredFieldError");
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

        Teamlab.connectMailServerInfo(null, serverIp, sqlIp || serverIp, db, usr, pwd, {
            before: function() {
                jq("#mailServiceBlock input").prop("disabled", true);
                LoadingBanner.showLoaderBtn("#mailServiceBlock");
            },
            success: function (params, res) {
                LoadingBanner.showMesInfoBtn("#mailServiceBlock", res.message, res.status);
                if (res.status == "success") {
                    ip = res.ip;
                    sqlip = res.sqlip;
                    database = res.database;
                    user = res.user;
                    password = res.password;
                    token = res.token;
                    host = res.host;
                    jq("#mailServiceSaveBtn").removeClass("disable");
                } else {
                    ip = null;
                    sqlip = null;
                    database = null;
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

        if (!ip || !database || !user || !password || !token || !host) return;

        Teamlab.saveMailServerInfo(null, ip, sqlip || ip, database, user, password, token, host, {
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