/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


var SsoSettings = (function () {

    var isInit = false,

        currentSettings = null,
        defaultSettings = null,
        ssoConstants = null,
        spMetadata = null,

        tmpIdpCertificates = [],
        tmpSPCertificates = [],
        tmpCertificate = null,
        tmpCertificateIndex = null,

        $ssoMainContainer = jq(".sso-main-container"),

        $ssoEnableBtn = jq("#ssoEnableBtn"),

        $ssoSPSettingsSpoilerLink = jq("#ssoSPSettingsSpoilerLink"),
        $ssoSPSettingsSpoiler = jq("#ssoSPSettingsSpoiler"),

        $ssoUploadMetadataInput = jq("#ssoUploadMetadataInput"),
        $ssoUploadMetadataBtn = jq("#ssoUploadMetadataBtn"),
        $ssoSelectMetadataBtn = jq("#ssoSelectMetadataBtn"),
        $ssoSelectMetadataInput = jq("#ssoSelectMetadataInput"),

        $ssoSpLoginLabel = jq("#ssoSpLoginLabel"),
        $ssoEntityId = jq("#ssoEntityId"),

        $ssoSignPostRbx = jq("#ssoSignPostRbx"),
        $ssoSignPostUrl = jq("#ssoSignPostUrl"),
        $ssoSignRedirectRbx = jq("#ssoSignRedirectRbx"),
        $ssoSignRedirectUrl = jq("#ssoSignRedirectUrl"),

        $ssoLogoutPostRbx = jq("#ssoLogoutPostRbx"),
        $ssoLogoutPostUrl = jq("#ssoLogoutPostUrl"),
        $ssoLogoutRedirectRbx = jq("#ssoLogoutRedirectRbx"),
        $ssoLogoutRedirectUrl = jq("#ssoLogoutRedirectUrl"),

        $ssoNameIdFormat = jq("#ssoNameIdFormat"),

        $ssoIdPCertificateContainer = jq("#ssoIdPCertificateContainer"),
        $ssoAddIdPCertificateBtn = jq("#ssoAddIdPCertificateBtn"),
        $ssoIdpCertificateSpoilerLink = jq("#ssoIdpCertificateSpoilerLink"),
        $ssoIdpCertificateSpoiler = jq("#ssoIdpCertificateSpoiler"),

        $ssoVerifyAuthResponsesSignCbx = jq("#ssoVerifyAuthResponsesSignCbx"),
        $ssoVerifyLogoutRequestsSignCbx = jq("#ssoVerifyLogoutRequestsSignCbx"),
        $ssoVerifyLogoutResponsesSignCbx = jq("#ssoVerifyLogoutResponsesSignCbx"),
        $ssoDecryptAssertionsCbx = jq("#ssoDecryptAssertionsCbx"),
        $ssoDefaultSignVerifyingAlgorithm = jq("#ssoDefaultSignVerifyingAlgorithm"),
        $ssoDefaultDecryptAlgorithm = jq("#ssoDefaultDecryptAlgorithm"),

        $ssoSPCertificateContainer = jq("#ssoSPCertificateContainer"),
        $ssoAddSPCertificateBtn = jq("#ssoAddSPCertificateBtn"),
        $ssoSpCertificateSpoilerLink = jq("#ssoSpCertificateSpoilerLink"),
        $ssoSpCertificateSpoiler = jq("#ssoSpCertificateSpoiler"),

        $ssoSignAuthRequestsCbx = jq("#ssoSignAuthRequestsCbx"),
        $ssoSignLogoutRequestsCbx = jq("#ssoSignLogoutRequestsCbx"),
        $ssoSignLogoutResponsesCbx = jq("#ssoSignLogoutResponsesCbx"),
        $ssoEncryptAssertionsCbx = jq("#ssoEncryptAssertionsCbx"),
        $ssoSigningAlgorithm = jq("#ssoSigningAlgorithm"),
        $ssoEncryptAlgorithm = jq("#ssoEncryptAlgorithm"),

        $ssoFirstName = jq("#ssoFirstName"),
        $ssoLastName = jq("#ssoLastName"),
        $ssoEmail = jq("#ssoEmail"),
        $ssoLocation = jq("#ssoLocation"),
        $ssoTitle = jq("#ssoTitle"),
        $ssoPhone = jq("#ssoPhone"),

        $ssoSaveBtn = jq("#ssoSaveBtn"),
        $ssoResetBtn = jq("#ssoResetBtn"),

        $ssoSPEntityId = jq("#ssoSPEntityId"),
        $ssoSPConsumerUrl = jq("#ssoSPConsumerUrl"),
        $ssoSPLogoutUrl = jq("#ssoSPLogoutUrl"),

        $ssoDownloadSPMetadataBtn = jq("#ssoDownloadSPMetadataBtn"),

        $ssoIdpCertificateDialog = jq("#ssoIdpCertificateDialog"),
        $ssoIdpPublicCertificate = jq("#ssoIdpPublicCertificate"),
        $ssoIdpCertificateActionType = jq("#ssoIdpCertificateActionType"),
        $ssoIdpCertificateOkBtn = jq("#ssoIdpCertificateOkBtn"),

        $ssoSpCertificateDialog = jq("#ssoSpCertificateDialog"),
        $ssoSpCertificateGenerateBtn = jq("#ssoSpCertificateGenerateBtn"),
        $ssoSpPublicCertificate = jq("#ssoSpPublicCertificate"),
        $ssoSpPrivateKey = jq("#ssoSpPrivateKey"),
        $ssoSpCertificateActionType = jq("#ssoSpCertificateActionType"),
        $ssoSpCertificateOkBtn = jq("#ssoSpCertificateOkBtn"),

        $certificatesTmpl = jq("#certificatesTmpl"),
        $certificateItemTmpl = jq("#certificateItemTmpl"),

        $ssoSettingsInviteDialog = jq("#ssoSettingsInviteDialog"),
        $ssoSettingsTurnOffDialog = jq("#ssoSettingsTurnOffDialog");
        $ssoHideAuthPageCbx = jq("#ssoHideAuthPage");

    function getSettings() {
        var isSignPost = $ssoSignPostRbx.is(":checked");
        var isLogoutPost = $ssoLogoutPostRbx.is(":checked");
        return {
            enableSso: $ssoEnableBtn.hasClass("on"),
            spLoginLabel: $ssoSpLoginLabel.val().trim(),
            idpSettings: {
                entityId: $ssoEntityId.val().trim(),
                ssoUrl: isSignPost ? $ssoSignPostUrl.val().trim() : $ssoSignRedirectUrl.val().trim(),
                ssoBinding: isSignPost ? ssoConstants.SsoBindingType.Saml20HttpPost : ssoConstants.SsoBindingType.Saml20HttpRedirect,
                sloUrl: isLogoutPost ? $ssoLogoutPostUrl.val().trim() : $ssoLogoutRedirectUrl.val().trim(),
                sloBinding: isLogoutPost ? ssoConstants.SsoBindingType.Saml20HttpPost : ssoConstants.SsoBindingType.Saml20HttpRedirect,
                nameIdFormat: $ssoNameIdFormat.val(),
            },
            idpCertificates: tmpIdpCertificates,
            idpCertificateAdvanced: {
                verifyAlgorithm: $ssoDefaultSignVerifyingAlgorithm.val(),
                verifyAuthResponsesSign: $ssoVerifyAuthResponsesSignCbx.is(":checked"),
                verifyLogoutRequestsSign: $ssoVerifyLogoutRequestsSignCbx.is(":checked"),
                verifyLogoutResponsesSign: $ssoVerifyLogoutResponsesSignCbx.is(":checked"),
                decryptAlgorithm: $ssoDefaultDecryptAlgorithm.val(),
                decryptAssertions: $ssoDecryptAssertionsCbx.is(":checked")
            },
            spCertificates: tmpSPCertificates,
            spCertificateAdvanced: {
                decryptAlgorithm: null,
                signingAlgorithm: $ssoSigningAlgorithm.val(),
                signAuthRequests: $ssoSignAuthRequestsCbx.is(":checked"),
                signLogoutRequests: $ssoSignLogoutRequestsCbx.is(":checked"),
                signLogoutResponses: $ssoSignLogoutResponsesCbx.is(":checked"),
                encryptAlgorithm: $ssoEncryptAlgorithm.val(),
                encryptAssertions: $ssoEncryptAssertionsCbx.is(":checked")
            },
            fieldMapping: {
                firstName: $ssoFirstName.val().trim(),
                lastName: $ssoLastName.val().trim(),
                email: $ssoEmail.val().trim(),
                title: $ssoTitle.val().trim(),
                location: $ssoLocation.val().trim(),
                phone: $ssoPhone.val().trim()
            },
            hideAuthPage: $ssoHideAuthPageCbx.is(":checked")
        };
    }

    function init(current, defaults, constants, metadata, error) {
        if (error) {
            toastr.error(error);
            isInit = true;
            jq(window).trigger("rightSideReady", null);
            return;
        }

        if (isInit) return;

        currentSettings = current;
        defaultSettings = defaults;
        ssoConstants = constants;
        spMetadata = metadata;

        tmpIdpCertificates = current.IdpCertificates.slice();
        tmpSPCertificates = current.SpCertificates.slice();


        if (currentSettings.EnableSso) {
            onSettingsChanged();
        }

        $ssoSPSettingsSpoilerLink.on('click', function () {
            spoiler.toggle('#ssoSPSettingsSpoiler', jq(this));
        });
        $ssoIdpCertificateSpoilerLink.on('click', function () {
            spoiler.toggle('#ssoIdpCertificateSpoiler', jq(this), null, ASC.Resources.Master.ResourceJS.SsoHideAdvancedSettings, ASC.Resources.Master.ResourceJS.SsoShowAdvancedSettings);
        });

        $ssoSpCertificateSpoilerLink.on('click', function () {
            spoiler.toggle('#ssoSpCertificateSpoiler', jq(this), null, ASC.Resources.Master.ResourceJS.SsoHideAdvancedSettings, ASC.Resources.Master.ResourceJS.SsoShowAdvancedSettings);
        });

        jq("#ssoSPMetadataSpoilerLink").on('click', function () {
            spoiler.toggle('#ssoSPMetadataSpoiler', jq(this));
        });

        

        $ssoEnableBtn.on("click", onSsoEnabled);

        $ssoSaveBtn.on("click", saveSettings);

        $ssoResetBtn.on("click", resetSettings);

        $ssoUploadMetadataBtn.on("click", loadMetadata);

        $ssoMainContainer.find("input, textarea").on("keyup", onSettingsChanged);

        $ssoMainContainer.find("input[type=radio]").on("click", onRadioBoxChanged);

        $ssoMainContainer.find(".checkBox").on("click", onCheckBoxChanged);

        $ssoNameIdFormat.on("change", onSettingsChanged);

        $ssoDefaultSignVerifyingAlgorithm.on("change", onSettingsChanged);

        $ssoDefaultDecryptAlgorithm.on("change", onSettingsChanged);

        $ssoSigningAlgorithm.on("change", onSettingsChanged);

        $ssoEncryptAlgorithm.on("change", onSettingsChanged);

        $ssoDownloadSPMetadataBtn.on("click", onDownloadSPMetadata);

        $ssoAddIdPCertificateBtn.on("click", function () {
            if (jq(this).hasClass("disable"))
                return;

            showIdpCertificateDialog(null, null);
        });

        $ssoIdpCertificateOkBtn.on("click", addIdpCertificate);

        $ssoIdPCertificateContainer.on("click", ".delete", deleteIdpCertificate);

        $ssoIdPCertificateContainer.on("click", ".edit", editIdpCertificate);

        $ssoAddSPCertificateBtn.on("click", function () {
            if (jq(this).hasClass("disable"))
                return;

            showSpCertificateDialog(null);
        });

        $ssoSpCertificateGenerateBtn.on("click", generateSpCertificate);

        $ssoSpCertificateOkBtn.on("click", addSpCertificate);

        $ssoSPCertificateContainer.on("click", ".delete", deleteSpCertificate);

        $ssoSPCertificateContainer.on("click", ".edit", editSpCertificate);

        $ssoSettingsInviteDialog.on("click", ".sso-settings-ok", function () {
            if (jq(this).hasClass("disable"))
                return;
            LoadingBanner.displayLoading()
            Teamlab.deleteSsoSettings(
                {
                    success: function (res, data) {
                        setDefaultSettings(false);
                        onSsoEnabled();
                        showSuccess(data);
                        onSettingsChanged();
                        closeDialog();
                        LoadingBanner.hideLoading();
                    },
                    error: function (error) {
                        showError(error)
                        onSettingsChanged();
                        closeDialog();
                        LoadingBanner.hideLoading();
                    }
                });
        });
        $ssoSpCertificateDialog.on("click", ".sso-settings-cancel", closeDialog);
        $ssoIdpCertificateDialog.on("click", ".sso-settings-cancel", closeDialog);
        $ssoSettingsInviteDialog.on("click", ".sso-settings-cancel", closeDialog);

        $ssoUploadMetadataInput.on("input propertychange paste change", function () {
            jq(this).toggleClass("error", false);
        });

        $ssoSettingsTurnOffDialog.on("click", ".sso-settings-ok", function () {
            if (jq(this).hasClass("disable"))
                return;

            enableSso(false);
            saveSettings();
            closeDialog();
        });
        $ssoSettingsTurnOffDialog.on("click", ".sso-settings-cancel", closeDialog);

        bindUploader();

        renderIdpCertificates(false);
        renderSpCertificates(false);

        renderSpMetadata();

        bindCopyToClipboard();

        isInit = true;

    }

    function setDefaultSettings(partial) {
        $ssoUploadMetadataInput.val("");

        enableSso(defaultSettings.EnableSso);

        $ssoSpLoginLabel.val(defaultSettings.SpLoginLabel);
        $ssoEntityId.val(defaultSettings.IdpSettings.EntityId);

        if (defaultSettings.IdpSettings.SsoBinding === ssoConstants.SsoBindingType.Saml20HttpPost) {
            $ssoSignPostRbx.trigger("click");
            $ssoSignPostUrl.val(defaultSettings.IdpSettings.SsoUrl);
            $ssoSignRedirectUrl.val("");
        } else {
            $ssoSignRedirectRbx.trigger("click");
            $ssoSignPostUrl.val("");
            $ssoSignRedirectUrl.val(defaultSettings.IdpSettings.SsoUrl);
        }

        if (defaultSettings.IdpSettings.SloBinding === ssoConstants.SsoBindingType.Saml20HttpPost) {
            $ssoLogoutPostRbx.trigger("click");
            $ssoLogoutPostUrl.val(defaultSettings.SloUrl);
            $ssoLogoutRedirectUrl.val("");
        } else {
            $ssoLogoutRedirectRbx.trigger("click");
            $ssoLogoutPostUrl.val("");
            $ssoLogoutRedirectUrl.val(defaultSettings.SloUrl);
        }

        $ssoNameIdFormat.val(defaultSettings.IdpSettings.NameIdForma)

        tmpIdpCertificates = defaultSettings.IdpCertificates;

        renderIdpCertificates(false);

        $ssoDefaultSignVerifyingAlgorithm.val(defaultSettings.IdpCertificateAdvanced.VerifyAlgorithm)

        if (defaultSettings.IdpCertificateAdvanced.VerifyAuthResponsesSign)
            $ssoVerifyAuthResponsesSignCbx.prop('checked', true);
        else
            $ssoVerifyAuthResponsesSignCbx.prop('checked', false);

        if (defaultSettings.IdpCertificateAdvanced.VerifyLogoutRequestsSign)
            $ssoVerifyLogoutRequestsSignCbx.prop('checked', true);
        else
            $ssoVerifyLogoutRequestsSignCbx.prop('checked', false);

        if (defaultSettings.IdpCertificateAdvanced.VerifyLogoutResponsesSign)
            $ssoVerifyLogoutResponsesSignCbx.prop('checked', true);
        else
            $ssoVerifyLogoutResponsesSignCbx.prop('checked', false);

        $ssoDefaultDecryptAlgorithm.val(defaultSettings.IdpCertificateAdvanced.DecryptAlgorithm)

        if (defaultSettings.IdpCertificateAdvanced.DecryptAssertions)
            $ssoDecryptAssertionsCbx.prop('checked', true);
        else
            $ssoDecryptAssertionsCbx.prop('checked', false);

        if (partial) return;
        tmpSPCertificates = defaultSettings.SpCertificates;

        renderSpCertificates(false);
        $ssoSigningAlgorithm.val(defaultSettings.SpCertificateAdvanced.SigningAlgorithm)

        if (defaultSettings.SpCertificateAdvanced.SignAuthRequests)
            $ssoSignAuthRequestsCbx.prop('checked', true);
        else
            $ssoSignAuthRequestsCbx.prop('checked', false);

        if (defaultSettings.SpCertificateAdvanced.SignLogoutRequests)
            $ssoSignLogoutRequestsCbx.prop('checked', true);
        else
            $ssoSignLogoutRequestsCbx.prop('checked', false);

        if (defaultSettings.SpCertificateAdvanced.SignLogoutResponses)
            $ssoSignLogoutResponsesCbx.prop('checked', true);
        else
            $ssoSignLogoutResponsesCbx.prop('checked', false);

        $ssoEncryptAlgorithm.val(defaultSettings.SpCertificateAdvanced.EncryptAlgorithm)

        if (defaultSettings.SpCertificateAdvanced.EncryptAssertions)
            $ssoEncryptAssertionsCbx.prop('checked', true);
        else
            $ssoEncryptAssertionsCbx.prop('checked', false);

        $ssoFirstName.val(defaultSettings.FieldMapping.FirstName);
        $ssoLastName.val(defaultSettings.FieldMapping.LastName);
        $ssoEmail.val(defaultSettings.FieldMapping.Email);
        $ssoTitle.val(defaultSettings.FieldMapping.Title);
        $ssoLocation.val(defaultSettings.FieldMapping.Location);
        $ssoPhone.val(defaultSettings.FieldMapping.Phone);

        if (defaultSettings.HideAuthPage)
            $ssoHideAuthPageCbx.prop('checked', true);
        else
            $ssoHideAuthPageCbx.prop('checked', false);
    }

    function bindCopyToClipboard() {
        var clipboard = new Clipboard('.copyBtn');
        clipboard.on('success', function (e) {
            toastr.success(window.Resource.Copied);
            e.clearSelection();
        });
    }

    function renderSpMetadata() {
        var baseUrl = spMetadata.baseUrl || window.location.origin;

        $ssoSPEntityId.val(baseUrl + spMetadata.EntityId).next(".copyBtn").attr("data-clipboard-text", baseUrl + spMetadata.EntityId);

        $ssoSPConsumerUrl.val(baseUrl + spMetadata.ConsumerUrl).next(".copyBtn").attr("data-clipboard-text", baseUrl + spMetadata.ConsumerUrl);

        $ssoSPLogoutUrl.val(baseUrl + spMetadata.LogoutUrl).next(".copyBtn").attr("data-clipboard-text", baseUrl + spMetadata.LogoutUrl);

        $ssoDownloadSPMetadataBtn.attr("data-href", baseUrl + spMetadata.MetadataUrl);
    }

    function getPropValue(obj, propName) {
        var value = "";

        if (!obj) return value;

        if (obj.hasOwnProperty(propName))
            return obj[propName];

        if (obj.hasOwnProperty("binding") && obj.hasOwnProperty("location") && obj["binding"] == propName)
            return obj["location"];

        if (Array.isArray(obj)) {
            obj.forEach(function (item) {
                if (item.hasOwnProperty(propName)) {
                    value = item[propName];
                    return;
                }

                if (item.hasOwnProperty("binding") && item.hasOwnProperty("location") && item["binding"] == propName) {
                    value = item["location"]
                    return;
                }
            });
        }

        return value;
    }

    function fillFieldsByMetadata(res) {

        setDefaultSettings(true);
        enableSso(true);

        var metadata = res.meta ? res.meta : res.responseJSON.meta;

        var value;

        if (metadata.entityID)
            $ssoEntityId.val(metadata.entityID || "");

        if (metadata.singleSignOnService) {
            value = getPropValue(metadata.singleSignOnService, ssoConstants.SsoBindingType.Saml20HttpPost);
            if (value) {
                $ssoSignPostUrl.val(value);
                $ssoSignPostRbx.trigger("click");
            }

            value = getPropValue(metadata.singleSignOnService, ssoConstants.SsoBindingType.Saml20HttpRedirect);
            if (value) {
                $ssoSignRedirectUrl.val(value);
                $ssoSignRedirectRbx.trigger("click");
            }
        }

        if (metadata.singleLogoutService) {
            value = getPropValue(metadata.singleLogoutService, ssoConstants.SsoBindingType.Saml20HttpPost);

            if (value) {
                $ssoLogoutPostUrl.val(value);
                $ssoLogoutPostRbx.trigger("click");
            }

            value = getPropValue(metadata.singleLogoutService, ssoConstants.SsoBindingType.Saml20HttpRedirect);
            if (value) {
                $ssoLogoutRedirectUrl.val(value);
                $ssoLogoutRedirectRbx.trigger("click");
            }
        }

        if (metadata.nameIDFormat) {
            if (Array.isArray(metadata.nameIDFormat)) {
                var formats = metadata.nameIDFormat.filter(function (format) {
                    return includePropertyValue(ssoConstants.SsoNameIdFormatType, format);
                });
                if (formats.length) {
                    $ssoNameIdFormat.val(formats[0])
                    //window.Common.selectorListener.set($ssoNameIdFormat, formats[0]);
                }
            } else {
                if (includePropertyValue(ssoConstants.SsoNameIdFormatType, metadata.nameIDFormat)) {
                    $ssoNameIdFormat.val(metadata.nameIDFormat)
                    //window.Common.selectorListener.set($ssoNameIdFormat, metadata.nameIDFormat);
                }
            }
        }

        if (metadata.certificate) {
            var data = [];

            if (metadata.certificate.signing) {
                if (Array.isArray(metadata.certificate.signing)) {
                    metadata.certificate.signing = getUniqueItems(metadata.certificate.signing).reverse();
                    metadata.certificate.signing.forEach(function (signingCrt) {
                        data.push({
                            crt: signingCrt.trim(),
                            key: null,
                            action: ssoConstants.SsoIdpCertificateActionType.Verification
                        });
                    });

                    if (data.length > 1) {
                        window.toastr.warning(ASC.Resources.Master.ResourceJS.SsoCertificateMultipleVerificationError);
                    }
                } else {
                    data.push({
                        crt: metadata.certificate.signing.trim(),
                        key: null,
                        action: ssoConstants.SsoIdpCertificateActionType.Verification
                    });
                }
            }

            Teamlab.ssoValidateCerts({ certs: data },
                function (res) {
                    tmpIdpCertificates = res.responseJSON;
                    renderIdpCertificates(true);
                    onSettingsChanged();
                },
                function (error) {

                    var errorMessage = (error ? error.responseText || error.statusText : null) || ASC.Resources.Master.ResourceJS.OperationFailedError;
                    errorMessage = errorMessage == "Invalid certificate" ? ASC.Resources.Master.ResourceJS.InvalidCertificate : errorMessage

                    window.toastr.error(errorMessage);

                },
                null
            );
        }

        HideRequiredError();
        onSettingsChanged();
    }

    function getUniqueItems(array) {
        return array.filter(function (item, index, array) {
            return array.indexOf(item) == index
        });
    }

    function bindUploader() {

        $ssoSelectMetadataInput.fileupload({
            url: "/" + "sso/uploadmetadata",
            dataType: "json",
            autoUpload: true,
            singleFileUploads: true,
            sequentialUploads: true,
            progress: function () {
                LoadingBanner.displayLoading()
            },
            add: function (e, data) {
                var fileName = data.files[0].name;
                if (getFileExtension(fileName) !== ".xml") {
                    toastr.error(ASC.Resources.Master.ResourceJS.SsoMetadataFileTypeError);
                } else {
                    data.submit().then(fillFieldsByMetadata, function (error) {
                        var errorMessage = (error ? error.responseText || error.statusText : null) || ASC.Resources.Master.ResourceJS.OperationFailedError;

                        if (errorMessage == "Metadata file not transferred") {
                            errorMessage = ASC.Resources.Master.ResourceJS.MetadataFileNotTransferred;
                        } else if (errorMessage == "Incorrect metadata file type. A .xml file is required.") {
                            errorMessage = ASC.Resources.Master.ResourceJS.IncorrectMetadataFileType;
                        } else if (errorMessage == "Invalid metadata xml file") {
                            errorMessage = ASC.Resources.Master.ResourceJS.InvalidMetadataXmlFile;
                        }
                        if (error.status !== 500) {
		                    errorMessage = error.statusText;
		                }

                        window.toastr.error(errorMessage);
                    });
                }
            },
            always: function () {
                LoadingBanner.hideLoading();
            }
        });
    }

    function closeDialog() {
        PopupKeyUpActionProvider.CloseDialog();
    }

    function editSpCertificate() {
        if ($ssoSPCertificateContainer.hasClass("disable")) return;
        var actionType = jq(this).parent().find(".action").text().trim();

        var certificate = jq.grep(tmpSPCertificates, function (_item) {
            var item = setKeysToLowerCase(_item);
            return item.action == actionType;
        })[0];

        showSpCertificateDialog(certificate);
    }

    function prepareCertificateData(data) {
        
        var _data = setKeysToLowerCase(data);
        var res = jq.extend({}, _data);
        var now = new Date();

        if (typeof res.startDate !== "object")
            res.startDate = new Date(res.startDate);

        if (typeof res.expiredDate !== "object")
            res.expiredDate = new Date(res.expiredDate);

        res.valid = (res.startDate < now && res.expiredDate > now);
        res.startDateStr = res.startDate.toLocaleDateString();
        res.expiredDateStr = res.expiredDate.toLocaleDateString();

        return res;
    }

    function renderSpCertificates(changeSpAdvancedSettings) {
        var settings = getSettings();
        enableSpAdvancedSettings(settings.enableSso);
        var array = jq.map(tmpSPCertificates, function (item) {
            return prepareCertificateData(item);
        });

        html = $certificatesTmpl.tmpl({ items: array });
        html.appendTo($ssoSPCertificateContainer.empty());

        if (changeSpAdvancedSettings)
            $ssoSpCertificateSpoiler.find("input[type=checkbox]").prop('checked', false);

        if (!array.length) return;

        if ($ssoSpCertificateSpoiler.hasClass("display-none"))
            $ssoSpCertificateSpoilerLink.trigger("click");

        if (!changeSpAdvancedSettings) return;

        array.forEach(function (item) {
            if (item.action == ssoConstants.SsoSpCertificateActionType.Signing) {
                $ssoSignAuthRequestsCbx.prop('checked', true);
                $ssoSignLogoutRequestsCbx.prop('checked', true);
            }
            if (item.action == ssoConstants.SsoSpCertificateActionType.Encrypt) {
                $ssoEncryptAssertionsCbx.prop('checked', true);
            }
            if (item.action == ssoConstants.SsoSpCertificateActionType.SigningAndEncrypt) {
                $ssoSignAuthRequestsCbx.prop('checked', true);
                $ssoSignLogoutRequestsCbx.prop('checked', true);
                $ssoEncryptAssertionsCbx.prop('checked', true);
            }
        });
    }

    function deleteSpCertificate() {

        if ($ssoSPCertificateContainer.hasClass("disable")) return;
        var actionType = jq(this).parent().find(".action").text().trim();

        tmpSPCertificates = jq.grep(tmpSPCertificates, function (_item) {
            var item = setKeysToLowerCase(_item);
            return item.action != actionType;
        });

        renderSpCertificates(true);
        onSettingsChanged();
    }

    function checkSpCertificateExist(actionType) {

        var certificates = jq.grep(tmpSPCertificates, function (_item) {
            var item = setKeysToLowerCase(_item);
            return tmpCertificate ? item.action != tmpCertificate.action : true;
        });

        var exists = jq.grep(certificates, function (_item) {
            var item = setKeysToLowerCase(_item);
            return item.action == ssoConstants.SsoSpCertificateActionType.SigningAndEncrypt;
        });

        if (exists.length) {
            return true;
        }

        if (actionType == ssoConstants.SsoSpCertificateActionType.SigningAndEncrypt && certificates.length) {
            return true;
        }

        exists = jq.grep(certificates, function (item) {
            return item.action == actionType;
        });

        if (exists.length) {
            return true;
        }

        return false;
    }

    function addSpCertificate() {

        HideRequiredError();

        var isValid = true;

        var data = {
            crt: $ssoSpPublicCertificate.val().trim(),
            key: $ssoSpPrivateKey.val().trim(),
            action: $ssoSpCertificateActionType.val()
        };

        if (!data.crt) {
            isValid = false;
            ShowRequiredError($ssoSpPublicCertificate, !isValid, !isValid);
        }

        if (!data.key) {
            isValid = false;
            ShowRequiredError($ssoSpPrivateKey, !isValid, !isValid);
        }

        if (checkSpCertificateExist(data.action)) {
            isValid = false;
            
            window.toastr.error(ASC.Resources.Master.ResourceJS.SsoCertificateActionTypeError);
        }

        if (!isValid) return;

        LoadingBanner.displayLoading()

        Teamlab.ssoValidateCerts({ certs: [data] },
            function (res) {
                if (tmpCertificate) {
                    tmpSPCertificates = jq.grep(tmpSPCertificates, function (_item) {
                        var item = setKeysToLowerCase(_item);
                        return item.action != tmpCertificate.action;
                    });
                }
                
                tmpSPCertificates = tmpSPCertificates.concat(res.responseJSON[0]);
                renderSpCertificates(true);
                onSettingsChanged();
                PopupKeyUpActionProvider.CloseDialog();
            },
            function (error) {

                var errorMessage = (error ? error.responseText || error.statusText : null) || ASC.Resources.Master.ResourceJS.OperationFailedError;
                errorMessage = errorMessage == "Invalid certificate" ? ASC.Resources.Master.ResourceJS.InvalidCertificate : errorMessage

                window.toastr.error(errorMessage);

            },
            function () {
                LoadingBanner.hideLoading();
            }
        );

    }

    function generateSpCertificate() {
        LoadingBanner.displayLoading()
        enableInputs(false, $ssoSpCertificateDialog);

        Teamlab.ssoGenerateCert(
            function (res) {
                if (res) {
                    $ssoSpPublicCertificate.val(res.responseJSON.crt);
                    $ssoSpPrivateKey.val(res.responseJSON.key);
                }

            },
            function (error) {

                var errorMessage = (error ? error.responseText || error.statusText : null) || ASC.Resources.Master.ResourceJS.OperationFailedError;
                errorMessage = errorMessage == "Cannot generate certificate" ? ASC.Resources.Master.ResourceJS.CannotGenerateCertificate : errorMessage

                if (error.status !== 500) {
                    errorMessage = error.statusText;
                }

                window.toastr.error(errorMessage);

            },
            function () {
                LoadingBanner.hideLoading()
                enableInputs(true, $ssoSpCertificateDialog);
            }
        );
    }

    function showSpCertificateDialog(_certificate) {
        if (jq(this).hasClass("disable"))
            return;

        var certificate = _certificate != null && _certificate != undefined ? setKeysToLowerCase(_certificate) : _certificate;

        tmpCertificate = certificate;

        HideRequiredError();

        $ssoSpCertificateDialog.find("input, textarea").prop("disabled", false).val("");
        $ssoSpCertificateDialog.find(".selectBox, .radioBox, .checkBox, .button").removeClass("disable");

        if (certificate) {
            $ssoSpPublicCertificate.val(certificate.crt);
            $ssoSpPrivateKey.val(certificate.key);


            $ssoSpCertificateActionType.val(certificate.action);
            //window.Common.selectorListener.set($ssoSpCertificateActionType, certificate.action); //TODO check

            $ssoSpCertificateDialog.find(".create-caption").addClass("display-none");
            $ssoSpCertificateDialog.find(".edit-caption").removeClass("display-none");
        } else {
            $ssoSpCertificateDialog.find(".create-caption").removeClass("display-none");
            $ssoSpCertificateDialog.find(".edit-caption").addClass("display-none");
        }
        StudioBlockUIManager.blockUI("#ssoSpCertificateDialog", 500);

    }

    function editIdpCertificate() {

        if ($ssoIdPCertificateContainer.hasClass("disable")) return;
        var index = jq(this).parent().index();

        var certificate = jq.grep(tmpIdpCertificates, function (item, i) {
            return i == index;
        })[0];

        showIdpCertificateDialog(certificate, index);
    }

    function onDownloadSPMetadata() {
        if (jq(this).hasClass("disable"))
            return;

        window.open(jq(this).attr("data-href"), "_blank");
    }

    function renderIdpCertificates(changeIdpAdvancedSettings) {
        var settings = getSettings();
        enableIdpAdvancedSettings(settings.enableSso);
        var array = jq.map(tmpIdpCertificates, function (item) {
            return prepareCertificateData(item);
        });

        var html = $certificatesTmpl.tmpl({ items: array });
        html.appendTo($ssoIdPCertificateContainer.empty());

        if (changeIdpAdvancedSettings)
            $ssoIdpCertificateSpoiler.find("input[type=checkbox]").prop('checked', false);

        if (!array.length) return;

        if ($ssoIdpCertificateSpoiler.hasClass("display-none"))
            $ssoIdpCertificateSpoilerLink.trigger("click");

        if (!changeIdpAdvancedSettings) return;

        array.forEach(function (item) {
            if (item.action == ssoConstants.SsoIdpCertificateActionType.Verification) {
                $ssoVerifyAuthResponsesSignCbx.prop('checked', true);
                $ssoVerifyLogoutRequestsSignCbx.prop('checked', true);
            }
            if (item.action == ssoConstants.SsoIdpCertificateActionType.Decrypt) {
                $ssoDecryptAssertionsCbx.prop('checked', true);
            }
            if (item.action == ssoConstants.SsoIdpCertificateActionType.VerificationAndDecrypt) {
                $ssoVerifyAuthResponsesSignCbx.prop('checked', true);
                $ssoVerifyLogoutRequestsSignCbx.prop('checked', true);
                $ssoDecryptAssertionsCbx.prop('checked', true);
            }
        });
    }

    function deleteIdpCertificate() {

        if ($ssoIdPCertificateContainer.hasClass("disable")) return;
        var index = jq(this).parent().index();

        tmpIdpCertificates = jq.grep(tmpIdpCertificates, function (item, i) {
            return i != index;
        });

        renderIdpCertificates(true);
        onSettingsChanged();
    }

    function addIdpCertificate() {

        HideRequiredError();
        var isValid = true;

        var data = {
            crt: $ssoIdpPublicCertificate.val().trim(),
            key: null,
            action: ssoConstants.SsoIdpCertificateActionType.Verification 
        };
        if (!data.crt) {
            isValid = false;
            ShowRequiredError($ssoIdpPublicCertificate, !isValid, !isValid);
        }

        if (checkIdpCertificateExist(data.action)) {
            isValid = false;

            window.toastr.error(ASC.Resources.Master.ResourceJS.SsoCertificateActionTypeError);

        }

        if (!isValid) return;

        LoadingBanner.displayLoading()
        Teamlab.ssoValidateCerts({ certs: [data] },
            function (res) {
                if (tmpCertificate) {
                    tmpIdpCertificates[tmpCertificateIndex] = res.responseJSON[0];
                } else {
                    tmpIdpCertificates = tmpIdpCertificates.concat(res.responseJSON[0]);
                }

                renderIdpCertificates(true);
                onSettingsChanged();
                PopupKeyUpActionProvider.CloseDialog();
            },
            function (error) {

                var errorMessage = (error ? error.responseText || error.statusText : null) || ASC.Resources.Master.ResourceJS.OperationFailedError;
                errorMessage = errorMessage == "Invalid certificate" ? ASC.Resources.Master.ResourceJS.InvalidCertificate : errorMessage

                if (error.status !== 500) {
                    errorMessage = error.statusText;
                }

                window.toastr.error(errorMessage);

            },
            function () {
                LoadingBanner.hideLoading();
            }
        );
    }

    function showError(error) {
        window.toastr.error((error ? error.responseText || error.statusText : null) || ASC.Resources.Master.ResourceJS.OperationFailedError);
    }

    function checkIdpCertificateExist(actionType) {

        var certificates = jq.grep(tmpIdpCertificates, function (item) {
            return tmpCertificate ? item.action != tmpCertificate.action : true;
        });

        var exists = jq.grep(certificates, function (item) {
            return item.action == ssoConstants.SsoIdpCertificateActionType.VerificationAndDecrypt;
        });

        if (exists.length) {
            return true;
        }

        if (actionType == ssoConstants.SsoIdpCertificateActionType.VerificationAndDecrypt && certificates.length) {
            return true;
        }

        exists = jq.grep(certificates, function (item) {
            return item.action == actionType;
        });

        if (exists.length) {
            return true;
        }
        if (tmpCertificate) {
            var exists =jq.grep(tmpIdpCertificates, function (item, i) {
                return i != tmpCertificateIndex && item.action == data.action && item.crt == data.crt;
            });

            if (exists.length) {
                return true;
            }
        }

        return false;
    }

    function showIdpCertificateDialog(_certificate, index) {
        if (jq(this).hasClass("disable"))
            return;

        var certificate = _certificate != null && _certificate != undefined ? setKeysToLowerCase(_certificate) : _certificate;

        tmpCertificate = certificate;
        tmpCertificateIndex = index;

        HideRequiredError();
        $ssoIdpCertificateDialog.find("input, textarea").prop("disabled", false).val("");
        $ssoIdpCertificateDialog.find(".selectBox, .radioBox, .checkBox, .button").removeClass("disable");

        if (certificate) {
            $ssoIdpPublicCertificate.val(certificate.crt);
            
            $ssoIdpCertificateDialog.find(".create-caption").addClass("display-none");
            $ssoIdpCertificateDialog.find(".edit-caption").removeClass("display-none");
        } else {
            $ssoIdpCertificateDialog.find(".create-caption").removeClass("display-none");
            $ssoIdpCertificateDialog.find(".edit-caption").addClass("display-none");
        }
        StudioBlockUIManager.blockUI("#ssoIdpCertificateDialog", 500);
    }

    function onCheckBoxChanged() {
        if (jq(this).hasClass("disable")) return;

        onSettingsChanged();
    }

    function onRadioBoxChanged() {
        var $this = jq(this);

        if ($this.hasClass("disable")) return;

        $this.closest(".sso-settings-block").find(".textEdit").addClass("display-none");
        jq("#" + $this.attr("data-value")).removeClass("display-none");

        onSettingsChanged();
    }

    var isValidUrl = function (url) {
        return /^(?:(?:https?|ftp):\/\/)(?:\S+(?::\S*)?@)?(?:(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,}))\.?)(?::\d{2,5})?(?:[/?#]\S*)?$/i.test(url);
    }

    function loadMetadata() {
        if (jq(this).hasClass("disable"))
            return;

        var url = $ssoUploadMetadataInput.val().trim();

        if (!url || !isValidUrl(url)) {
            $ssoUploadMetadataInput.toggleClass("error", true);
            return;
        }

        $ssoUploadMetadataInput.toggleClass("error", false);
        LoadingBanner.displayLoading()

        Teamlab.ssoLoadMetadata({ url: url },
            function (res) {
                fillFieldsByMetadata(res);
            },
            function (error) {
                var errorMessage = (error ? error.responseText || error.statusText : null) || ASC.Resources.Master.ResourceJS.OperationFailedError;

                if (error.status !== 500) {
                    errorMessage = error.statusText;
                }
                if (errorMessage == "Metadata file not transferred") {
                    errorMessage = ASC.Resources.Master.ResourceJS.MetadataFileNotTransferred;
                }  else if (errorMessage == "Invalid metadata xml file") {
                    errorMessage = ASC.Resources.Master.ResourceJS.InvalidMetadataXmlFile;
                }

                window.toastr.error(errorMessage);
            },
            function () {
                LoadingBanner.hideLoading();
            }
        );
    }

    function resetSettings() {
        if (jq(this).hasClass("disable"))
            return;

        StudioBlockUIManager.blockUI("#ssoSettingsInviteDialog", 500);
    }

    function saveSettings() {
        if (jq(this).hasClass("disable"))
            return;

        var validSettings = getValidSettings();

        if (!validSettings) {
            enableSaveBtn(false);
            return;
        }
        LoadingBanner.displayLoading()
        Teamlab.saveSsoSettings({},
            { serializeSettings: JSON.stringify(validSettings) },
            {
                success: function (res, data) {
                    showSuccess(data);
                    onSettingsChanged();
                    LoadingBanner.hideLoading();
                },
                error: function (error) {
                    showError(error)
                    onSettingsChanged();
                    LoadingBanner.hideLoading();
                },
            });
    }

    function getValidSettings() {
        HideRequiredError();

        var isValid = true;
        var settings = getSettings();

        if (!settings.spLoginLabel) {
            isValid = false;
            ShowRequiredError($ssoSpLoginLabel, !isValid, !isValid);
        }

        if (!settings.idpSettings.entityId) {
            isValid = false;
            ShowRequiredError($ssoEntityId, !isValid, !isValid);
        }

        if (!settings.idpSettings.ssoUrl) {
            isValid = false;
            ShowRequiredError($ssoSignPostRbx.hasClass("checked") ? $ssoSignPostUrl : $ssoSignRedirectUrl, !isValid, !isValid);
        }

        if (!settings.fieldMapping.firstName) {
            isValid = false;
            ShowRequiredError($ssoFirstName, !isValid, !isValid);
        }

        if (!settings.fieldMapping.lastName) {
            isValid = false;
            ShowRequiredError($ssoLastName, !isValid, !isValid);
        }

        if (!settings.fieldMapping.email) {
            isValid = false;
            ShowRequiredError($ssoEmail, !isValid, !isValid);
        }
        var verificationCertificates = jq.grep(settings.idpCertificates, function (item) {
            return item.action == ssoConstants.SsoIdpCertificateActionType.Verification ||
                item.action == ssoConstants.SsoIdpCertificateActionType.VerificationAndDecrypt;
        });

        if (verificationCertificates.length > 1) {
            isValid = false;
            window.toastr.error(ASC.Resources.Master.ResourceJS.SsoCertificateMultipleVerificationError);
        }

        return isValid ? settings : null;
    }

    function ShowRequiredError(item, withouthScroll, withouthFocus) {
        
        jq("div[class='infoPanel alert']").hide();
        jq("div[class='infoPanel alert']").empty();
        var parentBlock = jq(item).parents(".requiredField");
        jq(parentBlock).addClass("requiredFieldError");

        if (typeof (withouthScroll) == "undefined" || withouthScroll == false) {
            jq.scrollTo(jq(parentBlock).position().top - 50, { speed: 500 });
        }

        if (typeof (withouthFocus) == "undefined" || withouthFocus == false) {
            jq(item).trigger("focus");
        }
    }

    function showSuccess(data) {
        currentSettings = data;
        toastr.success(ASC.Resources.Master.ResourceJS.LdapSettingsSuccess);

        if (!currentSettings.enableSso)
            spoiler.toggle("#ssoSPSettingsSpoiler", "#ssoSPSettingsSpoilerLink", false);

        spoiler.toggle("#ssoSPMetadataSpoiler", "#ssoSPMetadataSpoilerLink", currentSettings.enableSso ? true : false);

        // Scroll page to full bottom
        jq("html, body").animate({ scrollTop: jq(document).height() }, 1000);
    }

    function HideRequiredError() {
        jq(".requiredField").removeClass("requiredFieldError");
    }

    function setKeysToLowerCase(object) {

        var _object = new Object;
        Object.keys(object).map(key => {
            _object[key.charAt(0).toLowerCase() + key.slice(1)] = object[key]
        });

        return _object;

    }

    function enableSso(on) {
        $ssoEnableBtn.toggleClass("off", !on).toggleClass("on", on);

        if (on && $ssoSPSettingsSpoiler.hasClass("display-none")) {
            $ssoSPSettingsSpoilerLink.trigger("click");
        }

        enableInputs(on);

        HideRequiredError();

        onSettingsChanged();
        enableIdpAdvancedSettings(on);
        enableSpAdvancedSettings(on);
    }

    function enableIdpAdvancedSettings(on) {

        $ssoIdpCertificateSpoiler.find("input[type=checkbox], select").prop("disabled", true);
        $ssoIdPCertificateContainer.addClass("disable");

        if (!on) return;

        $ssoIdPCertificateContainer.removeClass("disable");
        tmpIdpCertificates.forEach(function (item) {

            var _item = setKeysToLowerCase(item);

            if (_item.action == ssoConstants.SsoIdpCertificateActionType.Verification || _item.action == ssoConstants.SsoIdpCertificateActionType.VerificationAndDecrypt) {
                $ssoVerifyAuthResponsesSignCbx.prop("disabled", false);
                $ssoVerifyLogoutRequestsSignCbx.prop("disabled", false);
                $ssoVerifyLogoutResponsesSignCbx.prop("disabled", false);
                $ssoDefaultSignVerifyingAlgorithm.prop("disabled", false);
            }
            if (_item.action == ssoConstants.SsoIdpCertificateActionType.Decrypt || _item.action == ssoConstants.SsoIdpCertificateActionType.VerificationAndDecrypt) {
                $ssoDecryptAssertionsCbx.prop("disabled", false);
                $ssoDefaultDecryptAlgorithm.prop("disabled", false);
            }
        });
    }

    function enableSpAdvancedSettings(on) {
        $ssoSpCertificateSpoiler.find("input[type=checkbox], select").prop("disabled", true);
        $ssoSPCertificateContainer.addClass("disable");
        if (!on) return;

        $ssoSPCertificateContainer.removeClass("disable");
        tmpSPCertificates.forEach(function (item) {

            var _item = setKeysToLowerCase(item);

            if (_item.action == ssoConstants.SsoSpCertificateActionType.Signing || _item.action == ssoConstants.SsoSpCertificateActionType.SigningAndEncrypt) {
                $ssoSignAuthRequestsCbx.prop("disabled", false);
                $ssoSignLogoutRequestsCbx.prop("disabled", false);
                $ssoSignLogoutResponsesCbx.prop("disabled", false);
                $ssoSigningAlgorithm.prop("disabled", false);
            }
            if (_item.action == ssoConstants.SsoSpCertificateActionType.Encrypt || _item.action == ssoConstants.SsoSpCertificateActionType.SigningAndEncrypt) {
                $ssoEncryptAssertionsCbx.prop("disabled", false);
                $ssoEncryptAlgorithm.prop("disabled", false);
            }
        });
    }

    function onSsoEnabled() {
        var $this = jq(this);
        var on = $this.hasClass("off");
        var settings = setKeysToLowerCase(currentSettings);
        if (settings.enableSso) {
            StudioBlockUIManager.blockUI("#ssoSettingsTurnOffDialog", 500);
        }
        else {
            enableSso(on);
        }
    }

    function enableInputs(enabled, parent) {
        parent = parent || $ssoMainContainer;
        parent.find("input:not(.blocked), textarea:not(.blocked), select").prop("disabled", !enabled);
        parent.find(".radioBox, .checkBox, .button:not(.on-off-button)").toggleClass("disable", !enabled);
    }

    function onSettingsChanged() {
        var hasUserChanges = hasChanges();
        var isDefaultSettings = isDefault();
        var settings = getSettings();

        enableSaveBtn(hasUserChanges);
        enableResetBtn(!isDefaultSettings);
        enableDownloadBtn(settings.enableSso && !hasUserChanges && !isDefaultSettings);
    }

    function enableSaveBtn(enabled) {
        $ssoSaveBtn.toggleClass("disable", !enabled);
    }

    function enableResetBtn(enabled) {
        $ssoResetBtn.toggleClass("disable", !enabled);
    }

    function enableDownloadBtn(enabled) {
        $ssoDownloadSPMetadataBtn.toggleClass("disable", !enabled);
    }

    function hasChanges() {
        return !isEqual(currentSettings, getSettings());
    }

    function isDefault() {
        return isEqual(currentSettings, defaultSettings);
    }

    function getFileExtension(fileName) {
        if (!fileName) return "";

        var posExt = fileName.lastIndexOf(".");
        return posExt >= 0 ? fileName.substring(posExt).trim().toLowerCase() : "";
    }

    function isEqual(_a, _b) {
        var a = setKeysToLowerCase(_a);
        var b = setKeysToLowerCase(_b);

        if (a === null || b === null)
            return a === b;

        var aProps = Object.getOwnPropertyNames(a);
        var bProps = Object.getOwnPropertyNames(b);

        if (aProps.length != bProps.length) {
            return false;
        }

        for (var i = 0; i < aProps.length; i++) {
            var propName = aProps[i].charAt(0).toLowerCase() + aProps[i].slice(1);
            if (typeof a[propName] == "object" && a[propName] != null) {
                if (!isEqual(a[propName], b[propName])) {
                    return false;
                }
            } else {
                if (a[propName] !== b[propName]) {
                    return false;
                }
            }
        }

        function showSuccess(data) {
            currentSettings = data;
            
            toastr.success(ASC.Resources.Master.ResourceJS.SsoSettingsSuccess);

            if (!currentSettings.EnableSso)
                window.Common.spoiler.toggle("#ssoSPSettingsSpoiler", "#ssoSPSettingsSpoilerLink", false); //currentSettings.enableSso ? false : true);

            window.Common.spoiler.toggle("#ssoSPMetadataSpoiler", "#ssoSPMetadataSpoilerLink", currentSettings.EnableSso ? true : false);

            // Scroll page to full bottom
            jq("html, body").animate({ scrollTop: jq(document).height() }, 1000);
        }

        return true;
    }

    function includePropertyValue(obj, value) {
        var props = Object.getOwnPropertyNames(obj);

        for (var i = 0; i < props.length; i++) {
            if (obj[props[i]] === value)
                return true;
        }

        return false;
    }

    return {
        init: init
    };

})();

var spoiler = function () {
    var toggle = function (toggleEl, spoilerEl, force, hideLinkText, showLinkText) {
        var el = jq(toggleEl);
        var $this = jq(spoilerEl);
        if (!el)
            return;

        var enabled = typeof force === "boolean" ? force : el.hasClass('display-none');
        var linkText = "";

        if (enabled) {
            el.toggleClass('display-none', false);
            if ($this) {
                linkText = hideLinkText || ASC.Resources.Master.ResourceJS.HideLink;
                $this.text(linkText).prop('title', linkText);
            }
        } else {
            el.toggleClass('display-none', true);
            if ($this) {
                linkText = showLinkText || ASC.Resources.Master.ResourceJS.ShowLink;
                $this.text(linkText).prop('title', linkText);
            }
        }
    };

    return {
        toggle: toggle
    };
}();