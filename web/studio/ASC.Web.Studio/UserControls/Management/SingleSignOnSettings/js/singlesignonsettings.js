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


//TODO: Remove this or re-write like in Control Panel? 
var SsoSettings = (function () {
    var already = false,
        canSave = false,
        SAML = "SAML",
        JWT = "JWT",
        HMAC_SHA256 = "HMAC SHA-256",
        RSA_SHA256 = "RSA SHA-256",
        X509 = "X.509",
        uploadData = null,
        isStandalone,
        $ssoCertFile = jq("#ssoCertFile"),
        $ssoCertStatusContainer = jq("#ssoCertStatusContainer"),
        $ssoCertUploader = jq("#ssoCertUploader"),
        $ssoSettingsContainer = jq(".sso-settings-container"),
        $ssoSettingsMainContainer = jq(".sso-settings-main-container")
        $fileInput = jq("#ssoCertFile .name"),
        $ssoSettingsDownloadPublicKey = jq(".sso-settings-download-public-key"),
        $ssoSettingsTokenType = jq(".sso-settings-token-type"),
        $ssoSettingsCertPwd = jq(".sso-settings-cert-pwd"),
        $ssoSettingsSave = jq(".sso-settings-save"),
        $ssoSettingsValidationType = jq(".sso-settings-validation-type"),
        $ssoSettingsClientCrtBlock = jq(".sso-settings-client-crt-block");

    function canSaveSettings() {
        canSave = true;
        $ssoSettingsSave.removeClass("disable");
    }

    function createFileuploadInput(buttonObj) {
        var inputObj = jq("<input/>")
            .attr("id", "ssoFileUpload")
            .attr("type", "file")
            .css("width", "0")
            .css("height", "0")
            .hide();

        inputObj.appendTo(buttonObj.parent());

        buttonObj.on("click", function (e) {
            e.preventDefault();
            jq("#ssoFileUpload").click();
        });
    }

    function getFileExtension(fileTitle) {
        if (typeof fileTitle == "undefined" || fileTitle == null) {
            return "";
        }
        fileTitle = fileTitle.trim();
        var posExt = fileTitle.lastIndexOf(".");
        return 0 <= posExt ? fileTitle.substring(posExt).trim().toLowerCase() : "";
    }

    function createUploader() {
        var $ssoFileUpload = jq("#ssoFileUpload");

        if ($ssoFileUpload.length) {
            var uploader = $ssoFileUpload.fileupload({
                url: "ajaxupload.ashx?type=ASC.Web.Studio.UserControls.Management.SingleSignOnSettings.SingleSignOnSettings,ASC.Web.Studio",
                autoUpload: false,
                singleFileUploads: true,
                sequentialUploads: true
            });

            uploader.bind("fileuploadadd", function (e, data) {
                if (getFileExtension(data.files[0].name) != ".pfx") {
                    uploadData = null;
                    $ssoCertStatusContainer.addClass("error-popup").
                        removeClass("display-none").text(ASC.Resources.Master.Resource.UploadHttpsFileTypeError);
                } else {
                    uploadData = data;
                    $fileInput.text(data.files[0].name);
                    $ssoCertFile.show();
                    $ssoFileUpload.prop("disabled", true);
                    jq("#ssoFileUpload").prop("disabled", true);
                    $ssoCertUploader.addClass("gray");
                    $ssoCertStatusContainer.addClass("display-none");
                    $ssoSettingsCertPwd.removeAttr("disabled");
                    canSaveSettings();
                }
            });

            uploader.bind("fileuploadfail", function (e, data) {
                uploadData = null;
                var msg = data.errorThrown || data.textStatus;
                if (data.jqXHR && data.jqXHR.responseText) {
                    msg = jq.parseJSON(data.jqXHR.responseText).Message;
                }
                LoadingBanner.showMesInfoBtn(".sso-settings-main-container", Encoder.htmlEncode(msg), "error");
            });

            uploader.bind("fileuploaddone", function (e, data) {
                var res = JSON.parse(data.result);
                if (res.Success) {
                    uploadData = null;
                    jq("#ssoFileUpload").prop("disabled", false);
                    $ssoCertUploader.removeClass("gray");
                    $ssoCertStatusContainer.addClass("display-none");
                    saveSettings();
                } else {
                    LoadingBanner.showMesInfoBtn(".sso-settings-main-container", Encoder.htmlEncode(res.Message), "error");
                }
            });
        }
    }
    function ratioButtonChange() {
        canSaveSettings();
        if (jq(this).is(":checked") && jq(this).is("#ssoSettingsEnable")) {
            $ssoSettingsContainer.removeClass("sso-settings-disabled");
            $ssoSettingsContainer.
                find("input, textarea.sso-settings-public-key-area, select:not(.sso-settings-validation-type)").
                    removeAttr("disabled");
            if ($ssoSettingsTokenType.val() == JWT) {
                $ssoSettingsValidationType.removeAttr("disabled");
            }
            $ssoCertUploader.removeClass("gray");
            if (jq(".sso-settings-client-public-key-area").val()) {
                $ssoSettingsDownloadPublicKey.removeClass("sso-link-not-active");
            }
        } else {
            $ssoSettingsContainer.addClass("sso-settings-disabled");
            $ssoSettingsContainer.find("input, textarea.sso-settings-public-key-area, select").attr("disabled", "");
            $ssoCertUploader.addClass("gray");
            $ssoSettingsDownloadPublicKey.addClass("sso-link-not-active");
        }
    }

    function saveSettings() {
        if (already || !canSave) {
            return;
        }
        already = true;
        HideRequiredError();
        var result = false,
            publicKey = jq(".sso-settings-public-key-area").val(),
            issuer = jq(".sso-settings-issuer-url").val(),
            ssoEndPoint = jq(".sso-settings-endpoint-url").val(),
            enableSso = jq("#ssoSettingsEnable").is(":checked");
        if (enableSso) {
            if (publicKey == "") {
                result = true;
                ShowRequiredError(jq("#ssoSettingsPublicKeyError"));
            }
            if (issuer == "") {
                result = true;
                ShowRequiredError(jq("#ssoSettingsIssuerUrlError"));
            }
            if (ssoEndPoint == "") {
                result = true;
                ShowRequiredError(jq("#ssoSettingsEndpointUrlError"));
            }
            if (result) {
                already = false;
                return;
            }
        }
        var $ssoSettingsStatus = jq(".sso-settings-status"),
            $ssoSettingsLoader = jq(".sso-settings-loader");

        $ssoSettingsMainContainer.addClass("sso-settings-disabled");
        $ssoSettingsMainContainer.find("input, textarea, select").attr("disabled", "");
        $ssoSettingsStatus.text(ASC.Resources.Master.Resource.LoadingProcessing);
        $ssoSettingsStatus.removeClass("display-none");
        $ssoSettingsLoader.removeClass("display-none");
        
        /*SsoSettingsController.SaveSettings(JSON.stringify({
            EnableSso: enableSso,
            TokenType: $ssoSettingsTokenType.val(),
            ValidationType: $ssoSettingsValidationType.val(),
            PublicKey: publicKey,
            Issuer: issuer,
            SsoEndPoint: ssoEndPoint,
            SloEndPoint: jq(".slo-settings-endpoint-url").val(),
            ClientPassword: $ssoSettingsCertPwd.val(),
            ClientCertificateFileName: $ssoSettingsClientCrtBlock.find(".name").text()
        }), function (result) {

            jq("#ssoSettingsEnable").removeAttr("disabled");
            jq("#ssoSettingsDisable").removeAttr("disabled");

            $ssoSettingsMainContainer.removeClass("sso-settings-disabled");

            var status = result.value.Status;
            if (!status) {
                if (isStandalone) {
                    jq(".sso-settings-client-public-key-area").val(result.value.PublicKey);
                }
                status = ASC.Resources.Master.Resource.SavedTitle;
                LoadingBanner.showMesInfoBtn(".sso-settings-main-container", Encoder.htmlEncode(status), "success");
            } else {
                LoadingBanner.showMesInfoBtn(".sso-settings-main-container", Encoder.htmlEncode(status), "error");
            }

            if (jq("#ssoSettingsEnable").is(":checked")) {
                $ssoSettingsContainer.
                    find("input, textarea.sso-settings-public-key-area, select:not(.sso-settings-validation-type)").removeAttr("disabled");
                if (jq(".sso-settings-client-public-key-area").val()) {
                    $ssoSettingsDownloadPublicKey.removeClass("sso-link-not-active");
                }
                if ($ssoSettingsTokenType.val() == JWT) {
                    $ssoSettingsValidationType.removeAttr("disable");
                }
            }

            $ssoSettingsLoader.addClass("display-none");
            $ssoSettingsStatus.addClass("display-none");
            canSave = false;
            $ssoSettingsSave.addClass("disable");
            already = false;
        });*/
    }

    jq(window).on("load", function () {
        $ssoSettingsMainContainer.on("click", ".sso-settings-save",
            function () {
                /*SsoSettingsController.IsStandalone($ssoSettingsCertPwd.val(),
                    function (result) {
                        isStandalone = result.value;
                        if (result.value && uploadData != null) {
                            uploadData.submit();
                        } else {
                            saveSettings();
                        }
                    });*/
            }
        );

        jq("#ssoCertFile .trash").on("click", function () {
            if (jq("#ssoSettingsEnable").is(":checked")) {
                uploadData = null;
                jq("#ssoFileUpload").prop("disabled", false);
                $ssoCertUploader.removeClass("gray");
                $ssoCertFile.hide();
                $ssoCertFile.find(".name").text("");
                $ssoCertStatusContainer.addClass("display-none");
                jq(".sso-settings-client-public-key-area").val("");
                $ssoSettingsDownloadPublicKey.addClass("sso-link-not-active");
                jq("#ssoFileUpload").prop("disabled", false);
                canSaveSettings();
            }
        });

        $ssoSettingsTokenType.change(function (e) {
            canSaveSettings();
            if (jq(e.currentTarget).val() == JWT) {
                $ssoSettingsValidationType.removeAttr("disabled");
                $ssoSettingsClientCrtBlock.addClass("display-none");
            } else if (jq(e.currentTarget).val() == SAML) {
                $ssoSettingsValidationType.find("option[value='X.509']").attr("selected", "");
                $ssoSettingsValidationType.attr("disabled", "");
                $ssoSettingsClientCrtBlock.removeClass("display-none");
            }
        });
        createFileuploadInput($ssoCertUploader);
        createUploader();
        if (!jq("#ssoSettingsEnable").is(":checked")) {
            jq("#ssoFileUpload").prop("disabled", true);
        }
        jq("#ssoSettingsEnable").change(ratioButtonChange);
        jq("#ssoSettingsDisable").change(ratioButtonChange);
        $ssoSettingsValidationType.change(canSaveSettings);
        $ssoSettingsMainContainer.on("keydown", ".sso-settings-public-key-area", canSaveSettings);
        $ssoSettingsMainContainer.on("keydown", ".sso-settings-issuer-url", canSaveSettings);
        $ssoSettingsMainContainer.on("keydown", ".sso-settings-endpoint-url", canSaveSettings);
        $ssoSettingsMainContainer.on("keydown", ".slo-settings-endpoint-url", canSaveSettings);
        $ssoSettingsMainContainer.on("keydown", ".sso-settings-cert-pwd", canSaveSettings);
    });
})();