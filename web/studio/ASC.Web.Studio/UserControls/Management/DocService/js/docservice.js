/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


jq(function () {
    var saveUrls = function () {
        var docServiceUrl = jq("#docServiceUrl").val();
        var docServiceUrlInternal = jq("#docServiceUrlInternal").val();
        var docServiceUrlPortal = jq("#docServiceUrlPortal").val();

        Teamlab.saveDocServiceUrl(docServiceUrl, docServiceUrlInternal, docServiceUrlPortal, {
            success: function (_, data) {
                jq("#docServiceUrl").val(data[0]);
                jq("#docServiceUrlInternal").val(data[1]);
                jq("#docServiceUrlPortal").val(data[2]);

                LoadingBanner.showMesInfoBtn("#docServiceBlock", ASC.Resources.Master.Resource.SuccessfullySaveSettingsMessage, "success");
                jq("#docServiceBlock").unblock();
            },
            error: function (params, error) {
                LoadingBanner.showMesInfoBtn("#docServiceBlock", error, "error");
                jq("#docServiceBlock").unblock();
            }
        });
    };

    var testDocServiceApi = function () {
        var testApiResult = function () {
            var result = typeof DocsAPI != "undefined";

            if (result || !jq("#docServiceUrl").val().length) {
                saveUrls();
            } else {
                LoadingBanner.showMesInfoBtn("#docServiceBlock", "Api url: Service is not defined", "error");
                jq("#docServiceBlock").unblock();
            }
        };

        delete DocsAPI;

        jq("#scripDocServiceAddress").remove();

        var js = document.createElement("script");
        js.setAttribute("type", "text/javascript");
        js.setAttribute("id", "scripDocServiceAddress");
        document.getElementsByTagName("head")[0].appendChild(js);

        var scriptAddress = jq("#scripDocServiceAddress");

        scriptAddress.on("load", testApiResult).on("error", testApiResult);

        var docServiceUrlApi = jq("#docServiceUrl").val();
        if (docServiceUrlApi) {
            if (docServiceUrlApi.indexOf("/") == 0) {
                docServiceUrlApi = docServiceUrlApi.substring(1);
            } else {
                docServiceUrlApi += "/";
                if (!new RegExp('(^https?:\/\/)|^\/', 'i').test(docServiceUrlApi)) {
                    docServiceUrlApi = "http://" + docServiceUrlApi;
                }
            }
            docServiceUrlApi += "web-apps/apps/api/documents/api.js";
        }

        scriptAddress.attr("src", docServiceUrlApi);
    };

    jq("#docServiceButtonSave").click(function () {
        jq("#docServiceBlock").block();
        testDocServiceApi();

        return false;
    });

    jq(".doc-service-value").bind(jq.browser.msie ? "keydown" : "keypress", function (e) {
        if ((e.keyCode || e.which) == 13) {
            jq("#docServiceButtonSave").click();
        }
    });
});