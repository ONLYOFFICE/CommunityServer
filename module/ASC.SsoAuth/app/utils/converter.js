/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


"use strict";

const config = require("../../config").get();
const saml = require("samlify");
const validator = require("@authenio/samlify-node-xmllint");
const urn = require("samlify/build/src/urn");
const _ = require("lodash");
const fs = require("fs");
const path = require("path");
const minifyXML = require("minify-xml").minify;

saml.setSchemaValidator(validator);

const ServiceProvider = saml.ServiceProvider;
const IdentityProvider = saml.IdentityProvider;

const templDir = path.join(process.cwd(),"../ASC.SsoAuth", "/app/templates/");

module.exports = function (logger) {
  function removeCertHead(cert) {
    var newCert = cert;
    if (cert && cert[0] === "-") {
      newCert = cert.replace(/-----.*-----/g, "");
    }
    return newCert;
  }

  function loadTemplate(conf, templName) {
    try {
      const tmplPath = path.join(templDir, `${templName}.xml`);

      const template = minifyXML(
        fs.readFileSync(tmplPath, { encoding: "utf-8" })
      );

      conf[`${templName}`] = {
        context: template,
      };
    } catch (e) {
      logger.error(`loadTemplate ${e.message}`);
    }
  }

  return {
    toIdp: function (ssoConfig) {
      if (!ssoConfig && typeof ssoConfig !== "string") return undefined;

      const idpSetting = {
        entityID: ssoConfig.IdpSettings.EntityId,
        nameIDFormat: [ssoConfig.IdpSettings.NameIdFormat],

        requestSignatureAlgorithm:
          ssoConfig.IdpCertificateAdvanced.VerifyAlgorithm,
        dataEncryptionAlgorithm:
          ssoConfig.IdpCertificateAdvanced.DecryptAlgorithm,

        singleSignOnService: [
          {
            Binding: ssoConfig.IdpSettings.SsoBinding,
            Location: ssoConfig.IdpSettings.SsoUrl,
          },
        ],
        singleLogoutService: [
          {
            Binding: ssoConfig.IdpSettings.SloBinding,
            Location: ssoConfig.IdpSettings.SloUrl,
          },
        ],
        wantAuthnRequestsSigned:
          ssoConfig.SpCertificateAdvanced.SignAuthRequests,
        wantLogoutResponseSigned:
          ssoConfig.SpCertificateAdvanced.SignLogoutResponses,
        wantLogoutRequestSigned:
          ssoConfig.SpCertificateAdvanced.SignLogoutRequests,
        isAssertionEncrypted: ssoConfig.SpCertificateAdvanced.EncryptAssertions,
      };

      if (
        Array.isArray(ssoConfig.IdpCertificates) &&
        ssoConfig.IdpCertificates.length > 0
      ) {
        idpSetting.signingCert = removeCertHead(
          _.result(
            _.find(ssoConfig.IdpCertificates, function (obj) {
              return (
                obj.Action === "verification" ||
                obj.Action === "verification and decrypt"
              );
            }),
            "Crt"
          )
        );

        idpSetting.encryptCert = removeCertHead(
          _.result(
            _.find(ssoConfig.IdpCertificates, function (obj) {
              return (
                obj.Action === "decrypt" ||
                obj.Action === "verification and decrypt"
              );
            }),
            "Crt"
          )
        );
      }

      const idp = new IdentityProvider(idpSetting);

      return idp;
    },

    toSp: function (ssoConfig, baseUrl) {
      if (!ssoConfig && typeof ssoConfig !== "string")
        throw "Invalid ssoConfig";

      const metaUrl = baseUrl + "/sso" + config.routes.metadata,
        acsUrl = baseUrl + "/sso" + config.routes.login_callback,
        sloUrl = baseUrl + "/sso" + config.routes.logout_callback;

      const spSetting = {
        entityID: metaUrl,

        nameIDFormat: [ssoConfig.IdpSettings.NameIdFormat],

        requestSignatureAlgorithm:
          ssoConfig.SpCertificateAdvanced.SigningAlgorithm,
        dataEncryptionAlgorithm:
          ssoConfig.SpCertificateAdvanced.EncryptAlgorithm,

        assertionConsumerService: [
          {
            Binding: urn.namespace.binding.post,
            Location: acsUrl,
          },
          {
            Binding: urn.namespace.binding.redirect,
            Location: acsUrl,
          },
        ],

        singleLogoutService: [
          {
            Binding: urn.namespace.binding.post,
            Location: sloUrl,
          },
          {
            Binding: urn.namespace.binding.redirect,
            Location: sloUrl,
          },
        ],

        /*requestedAttributes: [
                    {
                        FriendlyName: "mail",
                        Name: "urn:oid:0.9.2342.19200300.100.1.3",
                        NameFormat: "urn:oasis:names:tc:SAML:2.0:attrname-format:uri"
                    },
                    {
                        FriendlyName: "givenName",
                        Name: "urn:oid:2.5.4.42",
                        NameFormat: "urn:oasis:names:tc:SAML:2.0:attrname-format:uri"
                    },
                    {
                        FriendlyName: "sn",
                        Name: "urn:oid:2.5.4.4",
                        NameFormat: "urn:oasis:names:tc:SAML:2.0:attrname-format:uri"
                    },
                    {
                        FriendlyName: "mobile",
                        Name: "urn:oid:0.9.2342.19200300.100.1.41",
                        NameFormat: "urn:oasis:names:tc:SAML:2.0:attrname-format:uri"
                    },
                    {
                        FriendlyName: "title",
                        Name: "urn:oid:2.5.4.12",
                        NameFormat: "urn:oasis:names:tc:SAML:2.0:attrname-format:uri"
                    },
                    {
                        FriendlyName: "l",
                        Name: "urn:oid:2.5.4.7",
                        NameFormat: "urn:oasis:names:tc:SAML:2.0:attrname-format:uri"
                    }
                ],*/
        authnRequestsSigned: ssoConfig.SpCertificateAdvanced.SignAuthRequests,
        wantAssertionsSigned:
          ssoConfig.IdpCertificateAdvanced.VerifyAuthResponsesSign,
        wantLogoutResponseSigned:
          ssoConfig.IdpCertificateAdvanced.VerifyLogoutResponsesSign,
        wantLogoutRequestSigned:
          ssoConfig.IdpCertificateAdvanced.VerifyLogoutRequestsSign,
        elementsOrder: [
          "KeyDescriptor",
          "SingleLogoutService",
          "NameIDFormat",
          "AssertionConsumerService",
          "AttributeConsumingService",
        ],
        //clockDrifts: [-60000, 60000],
      };

      if (
        Array.isArray(ssoConfig.SpCertificates) &&
        ssoConfig.SpCertificates.length > 0
      ) {
        spSetting.privateKey = _.result(
          _.find(ssoConfig.SpCertificates, function (obj) {
            return (
              obj.Action === "signing" || obj.Action === "signing and encrypt"
            );
          }),
          "Key"
        );

        spSetting.privateKeyPass = "";

        spSetting.encPrivateKey = _.result(
          _.find(ssoConfig.SpCertificates, function (obj) {
            return (
              obj.Action === "encrypt" || obj.Action === "signing and encrypt"
            );
          }),
          "Key"
        );

        spSetting.encPrivateKeyPass = "";

        spSetting.signingCert = _.result(
          _.find(ssoConfig.SpCertificates, function (obj) {
            return (
              obj.Action === "signing" || obj.Action === "signing and encrypt"
            );
          }),
          "Crt"
        );
        spSetting.encryptCert = _.result(
          _.find(ssoConfig.SpCertificates, function (obj) {
            return (
              obj.Action === "encrypt" || obj.Action === "signing and encrypt"
            );
          }),
          "Crt"
        );

        // must have if assertion signature fails validation
        spSetting.transformationAlgorithms = [
          "http://www.w3.org/2000/09/xmldsig#enveloped-signature",
          "http://www.w3.org/2001/10/xml-exc-c14n#",
        ];
      }

      loadTemplate(spSetting, "loginRequestTemplate");
      loadTemplate(spSetting, "logoutRequestTemplate");
      loadTemplate(spSetting, "logoutResponseTemplate");

      //if (config.app.organization) {
      //    spSetting.organization = config.app.organization;
      //}

      //if (config.app.contact) {
      //    spSetting.contact = config.app.contact;
      //}

      const sp = new ServiceProvider(spSetting);

      return sp;
    },
  };
};

//module.exports = Converter(logger);
