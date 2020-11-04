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


window.ASC.Desktop = (function () {
    if (!window["AscDesktopEditor"]) {
        return null;
    }

    var isInit = false;
    var domain = null;
    var isEncryptionSupport = typeof window.AscDesktopEditor.cloudCryptoCommand === "function";

    var init = function () {
        if (isInit === false) {
            isInit = true;

            domain = new RegExp("^http(s)?:\/\/[^\/]+").exec(location)[0];

            if (jq("#desktopWelcome").is(":visible")) {
                ASC.Files.UI.blockUI("#desktopWelcome", 520);
            }

            regDesktop();
        }
    };

    var encryptionSupport = function () {
        return isEncryptionSupport;
    };

    var regDesktop = function () {
        var data = {
            displayName: ASC.Files.Editor ? ASC.Files.Editor.docServiceParams.displayName : Teamlab.profile.displayName,
            domain: domain,
            email: ASC.Files.Editor ? ASC.Files.Editor.docServiceParams.email : Teamlab.profile.email,
            provider: "onlyoffice",
            userId: ASC.Files.Editor ? ASC.Files.Editor.configurationParams.editorConfig.user.id : Teamlab.profile.id,
        };

        if (isEncryptionSupport) {
            if (jq("#encryptionKeyPair").length) {
                var keys = jq("#encryptionKeyPair").val();
                data.encryptionKeys = keys ? JSON.parse(keys) : {};
                data.encryptionKeys.cryptoEngineId = "{FFF0E1EB-13DB-4678-B67D-FF0A41DBBCEF}";
            }
            else {
                isEncryptionSupport = ASC.Files.Editor && ASC.Files.Editor.configurationParams.editorConfig.encryptionKeys;
            }
        }

        window.AscDesktopEditor.execCommand("portal:login", JSON.stringify(data));

        window.onSystemMessage = function (e) {
            switch (e.type) {
                case "operation":
                    {
                        if (LoadingBanner) {
                            LoadingBanner.displayLoading();
                        }
                        var message = e.opMessage;
                        if (!message) {
                            switch (e.opType) {
                                case 0:
                                    message = ASC.Files.FilesJSResources.DesktopMessageDownloading;
                                    break;
                                case 1:
                                    message = ASC.Files.FilesJSResources.DesktopMessageEncrypting;
                                    break;
                                default:
                                    message = ASC.Resources.Master.Resource.LoadingProcessing;
                            }
                        }
                        //var modal = e.block === true;
                        ASC.Files.UI.displayInfoPanel(message);
                    }
                default:
                    break;
            }
        };

        window.DesktopUpdateFile = function () {
            if (ASC.Files.UI) {
                ASC.Files.UI.checkEditing();
            }
        };

        if (isEncryptionSupport) {
            window.cloudCryptoCommand = function (type, params, callback) {
                switch (type) {
                    case "encryptionKeys":
                        {
                            setEncryptionKeys(params);
                            break;
                        }
                    case "relogin":
                        {
                            var message = "Encryption keys must be re-entered";
                            if (ASC.Files.Editor) {
                                ASC.Files.Editor.docEditor.showMessage(message);
                            } else {
                                ASC.Files.UI.displayInfoPanel(message, true);
                            }

                            relogin();
                            break;
                        }
                    case "getsharingkeys":
                        {
                            if (!ASC.Files.Editor) {
                                callback({});
                                return;
                            }

                            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetEncryptionAccess,
                                function (keys, params, errorMessage) {
                                    var data = {
                                        "keys": keys,
                                    };

                                    if (typeof errorMessage != "undefined") {
                                        if (ASC.Files.Editor) {
                                            ASC.Files.Editor.docEditor.showMessage(message);
                                        } else {
                                            ASC.Files.UI.displayInfoPanel(message, true);
                                        }
                                        data.error = message;
                                    }

                                    callback(data);
                                },
                                { once: true });

                            ASC.Files.ServiceManager.getEncryptionAccess(ASC.Files.ServiceManager.events.GetEncryptionAccess,
                                {
                                    fileId: ASC.Files.Editor.docServiceParams.fileId
                                });
                        }
                }
            };
        }
    };

    var relogin = function () {
        setTimeout(function () {
            var data = {
                domain: domain,
                onsuccess: "reload"
            };

            window.AscDesktopEditor.execCommand("portal:logout", JSON.stringify(data));
        }, 1000);
    };

    var encryptionUploadDialog = !isEncryptionSupport
        ? null
        : function (callback) {

            var filter = jq(ASC.Files.Utility.Resource.ExtsWebEncrypt).map(function (i, format) {
                return "*" + format;
            }).toArray().join(" ");

            window.AscDesktopEditor.cloudCryptoCommand("upload",
                {
                    "cryptoEngineId": "{FFF0E1EB-13DB-4678-B67D-FF0A41DBBCEF}",
                    "filter": filter,
                },
                function (obj) {
                    if (LoadingBanner) {
                        LoadingBanner.hideLoading();
                    }

                    var bytes = obj.bytes;
                    var filename = obj.name;
                    var file = new File([bytes], filename);

                    if (typeof callback == "function") {
                        callback(file, obj.isCrypto !== false);
                    }
                });
        };

    //request

    var setEncryptionKeys = function (encryptionKeys) {
        if (!encryptionKeys.publicKey || !encryptionKeys.privateKeyEnc) {
            ASC.Files.UI.displayInfoPanel("Empty encryption keys", true);
            return;
        }

        if (typeof Teamlab !== "undefined") {
            Teamlab.setEncryptionKeys({},
                {
                    publicKey: encryptionKeys.publicKey,
                    privateKeyEnc: encryptionKeys.privateKeyEnc
                },
                {
                    error: function (params, error) {
                        ASC.Files.UI.displayInfoPanel(error[0], true);
                    }
                });
        }
    };

    var setAccess = !isEncryptionSupport
        ? null
        : function (fileId, callback) {
            //todo: think about share page in editor

            Teamlab.getEncryptionAccess({}, fileId, {
                success: function (params, keys) {

                    var fileUrl = domain + ASC.Files.Utility.GetFileDownloadUrl(fileId);
                    fileUrl = fileUrl.replace(/action=download/, "action=stream");

                    window.AscDesktopEditor.cloudCryptoCommand("share",
                        {
                            "cryptoEngineId": "{FFF0E1EB-13DB-4678-B67D-FF0A41DBBCEF}",
                            "file": [fileUrl],
                            "keys": keys
                        },
                        function (obj) {
                            var file = null;
                            if (obj.isCrypto !== false) {
                                var bytes = obj.bytes;
                                var filename = "temp_name";
                                file = new File([bytes], filename);
                            }

                            if (typeof callback == "function") {
                                callback(file);
                            }
                        });

                },
                error: function (params, error) {
                    ASC.Files.UI.displayInfoPanel(error[0], true);
                    callback();
                }
            });
        };

    return {
        init: init,

        encryptionSupport: encryptionSupport,

        setAccess: setAccess,

        encryptionUploadDialog: encryptionUploadDialog
    };
})();

(function ($) {
    $(function () {
        if (ASC.Desktop) {
            ASC.Desktop.init();
        }
    });
})(jQuery);