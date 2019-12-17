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


window.ASC.Desktop = (function () {
    if (!window["AscDesktopEditor"]) {
        return null;
    }

    var isInit = false;
    var domain = null;
    var isEncryptionSupport = typeof window.AscDesktopEditor.isBlockchainSupport === "function" && window.AscDesktopEditor.isBlockchainSupport();

    var init = function () {
        if (isInit === false) {
            isInit = true;

            domain = new RegExp("^http(s)?:\/\/[^\/]+").exec(location)[0];

            if (jq("#desktopWelcome").is(":visible")) {
                ASC.Files.UI.blockUI("#desktopWelcome", 520, 600);
            }

            regDesktop();

            if (ASC.Desktop.encryptionSupport()) {
                if (typeof StudioManager !== "undefined" && typeof Teamlab !== "undefined") {
                    StudioManager.addPendingRequest(requestEncryptionData);
                }
            }
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
        };

        window.AscDesktopEditor.execCommand("portal:login", JSON.stringify(data));

        window.onSystemMessage = function (e) {
            switch (e.type) {
                case "user":
                    {
                        setEncryptionData(e.account);
                        break;
                    }
                case "share":
                    {
                        if (LoadingBanner) {
                            LoadingBanner.hideLoading();
                        }

                        if (e.result !== "OK") {
                            ASC.Files.UI.displayInfoPanel(e.result, true);
                        }
                        break;
                    }
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

        window.onChangeCryptoMode = function (mode) {
            isEncryptionSupport = (mode > 0);

            requestEncryptionData();

            if (ASC.Files.Folders) {
                ASC.Files.Anchor.navigationSet(ASC.Files.Folders.currentFolder.id, false);
            }
        };
    };

    var encryptionUploadDialog = (typeof window.AscDesktopEditor.CloudCryptUpload !== "function"
        ? null
        : function (callback) {
            window.AscDesktopEditor.CloudCryptUpload(null, function (tmpFilePath, encrypted) {
                encrypted = encrypted !== false;

                window.AscDesktopEditor.loadLocalFile(tmpFilePath, function (bytes) {
                    if (LoadingBanner) {
                        LoadingBanner.hideLoading();
                    }

                    var filename = tmpFilePath.split("\\").pop().split("/").pop();
                    var file = new File([bytes], filename);

                    callback(file, encrypted);
                });
            });
        }
    );

    var encryptionUploadEnd = (typeof window.AscDesktopEditor.CloudCryptUploadEnd !== "function"
        ? null
        : function () {
            window.AscDesktopEditor.CloudCryptUploadEnd();
        }
    );

    var encryptFileByUrl = (typeof window.AscDesktopEditor.CloudCryptFile !== "function"
        ? null
        : function (fileUrl, callback) {
            if (fileUrl.indexOf("http") != 0) {
                fileUrl = domain + fileUrl;
            }
            fileUrl = fileUrl.replace(/action=download/, "action=stream");

            window.AscDesktopEditor.CloudCryptFile(fileUrl, function (tmpFilePath, encrypted) {
                encrypted = encrypted !== false;

                window.AscDesktopEditor.loadLocalFile(tmpFilePath, function (bytes) {
                    if (LoadingBanner) {
                        LoadingBanner.hideLoading();
                    }

                    var filename = tmpFilePath.split("\\").pop().split("/").pop();
                    var file = new File([bytes], filename);

                    callback(file, encrypted);
                });
            });
        }
    );

    //request

    var requestEncryptionData = function () {
        if (!ASC.Desktop.encryptionSupport()) {
            return;
        }

        window.AscDesktopEditor.sendSystemMessage({type: "user"});
    };

    var setEncryptionData = function (account) {
        if (!account.address || !account.publicKey) {
            ASC.Files.UI.displayInfoPanel("Empty encryption address", true);
            return;
        }

        if (typeof Teamlab !== "undefined") {
            Teamlab.updateEncryptionAddress({},
                {
                    address: account.address,
                    publicKey: account.publicKey
                },
                {
                    error: function (params, error) {
                        ASC.Files.UI.displayInfoPanel(error[0], true);
                    }
                });
        }
    };

    var setAccess = function (fileId, callback) {
        if (!ASC.Desktop.encryptionSupport()) {
            return;
        }

        Teamlab.getEncryptionAccess({}, fileId, {
            success: function (params, addresses) {

                var downloadLink = domain + ASC.Files.Utility.GetFileDownloadUrl(fileId);

                window.AscDesktopEditor.GetHash(downloadLink,
                    function (hash, docinfo) {
                        var data =
                        {
                            accounts: { "addresses" : addresses },
                            hash: hash,
                            docinfo: (typeof docinfo === "object" ? docinfo.docinfo : null),
                            type: "share",
                        };

                        if (LoadingBanner) {
                            LoadingBanner.displayLoading();
                        }

                        window.AscDesktopEditor.sendSystemMessage(data);

                        if (typeof callback == "function") {
                            callback();
                        }
                    });

            },
            error: function (params, error) {
                ASC.Files.UI.displayInfoPanel(error[0], true);
            }
        });
    };

    return {
        init: init,

        encryptionSupport: encryptionSupport,

        setAccess: setAccess,

        encryptionUploadDialog: encryptionUploadDialog,
        encryptionUploadEnd: encryptionUploadEnd,
        encryptFileByUrl: encryptFileByUrl,
    };
})();

(function ($) {
    $(function () {
        if (ASC.Desktop) {
            ASC.Desktop.init();
        }
    });
})(jQuery);