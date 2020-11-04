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
                LoadingBanner.showMesInfoBtn("#docServiceBlock", error[0], "error");
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

    jq("#docServiceButtonReset").click(function () {
        jq("#docServiceUrl,#docServiceUrlInternal,#docServiceUrlPortal").val("");
        jq("#docServiceButtonSave").click();
    });

    jq(".doc-service-value").bind(jq.browser.msie ? "keydown" : "keypress", function (e) {
        if ((e.keyCode || e.which) == 13) {
            jq("#docServiceButtonSave").click();
        }
    });
});