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

module.exports = function (app, config, logger) {
  const saml = require("samlify");
  const { SamlLib: libsaml } = saml;
  const urlResolver = require("./utils/resolver")(logger);
  const coder = require("./utils/coder");
  const urn = require("samlify/build/src/urn");
  const fetch = require("node-fetch");
  const fileManager = require('./fileManager');
  const forge = require('node-forge');
  const path = require('path');
  const uuid = require('uuid');
  const formidable = require('formidable');
  const UserModel = require("./model/user");
  const LogoutModel = require("./model/logout");
  const fs = require('fs');
  let uploadDir = "";
  const selfSignedDomain = "myselfsigned.crt";

  function verifySetting(req) {
    if (!req.providersInfo.settings.EnableSso) {
      logger.error("Sso settings is disabled");
      return false;
    }

    return true;
  }

  function getError(e) {
    if (!e) return "EMPTY_ERROR";

    if (typeof e === "object" && e.message) {
      return e.message;
    } else {
      return e;
    }
  }

  function getSpMetadata(req, res) {
    const sp = req.providersInfo.sp;

    res.type("application/xml");

    const xml = sp.getMetadata();

    if (config.app.logSamlData) {
      logger.debug(xml);
    }

    return res.send(xml);
  }

  function onGenerateCert(req, res){
    try {
        res.status(200).send(generateCertificate());
    }
    catch (error) {
        res.status(500).send("Cannot generate certificate");
    }
  }
  function onValidateCerts(req, res){
    try {
        res.status(200).send(validateCertificate(req.body.certs));
    }
    catch (error) {
        res.status(500).send("Invalid certificate");
    }
  }
  function onUploadMetadata(req, res) {
    function formParse(err, fields, files) {
      if (err) {
        res.status(500).send(err).end();
        return;
      }

      if (!files || !files.metadata) {
        res.status(500).send("Metadata file not transferred").end();
        return;
      }

      if (!files.metadata.name.toLowerCase().endsWith(".xml")) {
        fileManager.deleteFile(files.metadata.path);
        res.status(500).send("Incorrect metadata file type. A .xml file is required.").end();
        return;
      }

      const idp = saml.IdentityProvider({
        metadata: fs.readFileSync(files.metadata.path)
      });
      const idpMetadata = idp.entityMeta;

      if (!idpMetadata ||
        !idpMetadata.meta ||
        !idpMetadata.meta.entityDescriptor ||
        !idpMetadata.meta.singleSignOnService) {
        fileManager.deleteFile(files.metadata.path);
        res.status(500).send("Invalid metadata xml file").end();
        return;
      }

      fileManager.deleteFile(files.metadata.path);
      res.status(200).send(idpMetadata).end();
    }

    const form = new formidable.IncomingForm();
    form.uploadDir = uploadDir;
    form.parse(req, formParse);
  }

  function onLoadMetadata(req, res) {
    try {
      const filePath = path.join(uploadDir, uuid.v1() + ".xml");

      fileManager.downloadFile(req.body.url, filePath)
        .then((result) => {
          const idp = saml.IdentityProvider({
            metadata: fs.readFileSync(result)
          });
          const idpMetadata = idp.entityMeta;

          if (!idpMetadata ||
            !idpMetadata.meta ||
            !idpMetadata.meta.entityDescriptor ||
            !idpMetadata.meta.singleSignOnService) {
            fileManager.deleteFile(result);
            res.status(500).send("Invalid metadata xml file").end();
            return;
          }

          fileManager.deleteFile(result);
          res.status(200).send(idpMetadata).end();
        })
        .catch((error) => {
          fileManager.deleteFile(filePath);
          res.status(500).send("Metadata file not transferred");
        });
    }
    catch (error) {
      res.status(500).send(error);
    }
  }
  function generateCertificate() {
    const pki = forge.pki;

    let keys = pki.rsa.generateKeyPair(2048);
    let cert = pki.createCertificate();

    cert.publicKey = keys.publicKey;
    cert.serialNumber = "01";
    cert.validity.notBefore = new Date();
    cert.validity.notAfter = new Date();
    cert.validity.notAfter.setFullYear(cert.validity.notBefore.getFullYear() + 1);

    let attr = [{
        name: "commonName",
        value: selfSignedDomain
    }];

    cert.setSubject(attr);
    cert.setIssuer(attr);

    cert.sign(keys.privateKey);

    let crt = pki.certificateToPem(cert);
    let key = pki.privateKeyToPem(keys.privateKey);

    return {
        crt: crt,
        key: key
    };
  }
  function validateCertificate(certs){
    const result = [];
    const pki = forge.pki;

    certs.forEach(function (data) {
        if (!data.crt)
            throw "Empty public certificate";

        if (data.crt[0] !== "-")
            data.crt = "-----BEGIN CERTIFICATE-----\n" + data.crt + "\n-----END CERTIFICATE-----";

        const cert = pki.certificateFromPem(data.crt);

        const publicKey = cert.publicKey;
        if (!publicKey)
            throw "Invalid public cert";

        if (data.key) {
            const privateKey = pki.privateKeyFromPem(data.key);
            if (!privateKey)
                throw "Invalid private key";

            const md = forge.md.sha1.create();
            md.update('sign this', 'utf8');
            const signature = privateKey.sign(md);

            // verify data with a public key
            // (defaults to RSASSA PKCS#1 v1.5)
            const verified = publicKey.verify(md.digest().bytes(), signature);

            if (!verified)
                throw "Invalid key-pair (unverified signed data test)";
        }

        const domainName = cert.subject.getField("CN").value || cert.issuer.getField("CN").value;
        const startDate = cert.validity.notBefore.toISOString().split(".")[0] + "Z";
        const expiredDate = cert.validity.notAfter.toISOString().split(".")[0] + "Z";

        result.push({
            selfSigned: domainName === selfSignedDomain,
            crt: data.crt,
            key: data.key,
            action: data.action,
            domainName: domainName,
            startDate: startDate,
            expiredDate: expiredDate
        });
    });

    return result;
  }

  function sendToIDP(res, request, isPost) {
    logger.info(`SEND ${isPost ? "POST" : "REDIRECT"} to IDP`);
    if (isPost) {
      return res.render("actions", request); // see more in "actions.handlebars"
    } else {
      return res.redirect(request.context);
    }
  }

  const createAuthnTemplateCallback = (_idp, _sp, method) => (template) => {
    const metadata = { idp: _idp.entityMeta, sp: _sp.entityMeta };
    const spSetting = _sp.entitySetting;
    const entityEndpoint = metadata.idp.getSingleSignOnService(method);
    const nameIDFormat = spSetting.nameIDFormat;
    const selectedNameIDFormat = Array.isArray(nameIDFormat)
      ? nameIDFormat[0]
      : nameIDFormat;
    const id = spSetting.generateID();
    const assertionConsumerServiceURL = metadata.sp.getAssertionConsumerService(
      method
    );

    const data = {
      ID: id,
      Destination: entityEndpoint,
      Issuer: metadata.sp.getEntityID(),
      IssueInstant: new Date().toISOString(),
      NameIDFormat: selectedNameIDFormat,
      AssertionConsumerServiceURL: assertionConsumerServiceURL,
      EntityID: metadata.sp.getEntityID(),
      AllowCreate: spSetting.allowCreate,
    };

    const t = template.context || template;

    const rawSamlRequest = libsaml.replaceTagsByValue(t, data);
    return {
      id: id,
      context: rawSamlRequest,
    };
  };

  const sendLoginRequest = (req, res) => {
    try {
      if (!verifySetting(req)) {
        return res.redirect(urlResolver.getPortal500Url(req));
      }

      const sp = req.providersInfo.sp;
      const idp = req.providersInfo.idp;

      const isPost =
        req.providersInfo.settings.IdpSettings.SsoBinding ===
        urn.namespace.binding.post;

      const method = isPost
        ? urn.wording.binding.post
        : urn.wording.binding.redirect;

      const data = sp.createLoginRequest(
        idp,
        method,
        createAuthnTemplateCallback(idp, sp, method)
      );

      return sendToIDP(res, data, isPost);
    } catch (e) {
      logger.error(`sendLoginRequest ${getError(e)}`);
      return res.redirect(
        urlResolver.getPortalAuthErrorUrl(
          req,
          urlResolver.ErrorMessageKey.SsoError
        )
      );
    }
  };

  const parseLoginResponse = (sp, idp, method, req) => {
    return sp.parseLoginResponse(idp, method, req).catch((e) => {
      const message = getError(e);
      if (message == "ERR_FAILED_TO_VERIFY_SIGNATURE") {
        if (
          idp.entitySetting.messageSigningOrder == urn.MessageSignatureOrder.ETS
        ) {
          idp.entitySetting.messageSigningOrder = urn.MessageSignatureOrder.STE;
        } else {
          idp.entitySetting.messageSigningOrder = urn.MessageSignatureOrder.ETS;
        }

        logger.info(
          `parseLoginResponse -> Changing urn.MessageSignatureOrder to ${idp.entitySetting.messageSigningOrder}`
        );

        return sp.parseLoginResponse(idp, method, req);
      } else {
        logger.error(`parseLoginResponse failed ${message}`);
      }

      return Promise.reject(e);
    });
  };

  const onLoginResponse = async (req, res) => {
    try {
      if (!verifySetting(req)) {
        return res.redirect(urlResolver.getPortal500Url(req));
      }

      const sp = req.providersInfo.sp;
      const idp = req.providersInfo.idp;

      const method =
        req.method === "POST"
          ? urn.wording.binding.post
          : urn.wording.binding.redirect;

      const requestInfo = await parseLoginResponse(sp, idp, method, req);

      if (config.app.logSamlData) {
        logger.debug(`parseLoginResponse ${JSON.stringify(requestInfo)}`);
      }

      if (!requestInfo.extract.attributes) {
        return res.redirect(
          urlResolver.getPortalAuthErrorUrl(
            req,
            urlResolver.ErrorMessageKey.SsoAttributesNotFound
          )
        );
      }

      if (config.app.logSamlData) {
        logger.debug(`parseLoginResponse nameID=${requestInfo.extract.nameID}`);
        logger.debug(
          `parseLoginResponse sessionIndex=${JSON.stringify(
            requestInfo.extract.sessionIndex
          )}`
        );
        logger.debug(
          `parseLoginResponse attributes=${JSON.stringify(
            requestInfo.extract.attributes
          )}`
        );
        logger.debug(
          `parseLoginResponse mapping=${JSON.stringify(
            req.providersInfo.mapping
          )}`
        );
      }

      const user = new UserModel(
        requestInfo.extract.nameID,
        requestInfo.extract.sessionIndex.sessionIndex,
        requestInfo.extract.attributes,
        req.providersInfo.mapping
      );

      logger.info(`SSO User ${JSON.stringify(user)}`);

      // Use the parseResult can do customized action
      const data = coder.encodeData(user);

      if (!data) {
        logger.error("coder.encodeData", user);
        return res.redirect(
          urlResolver.getPortalAuthErrorUrl(
            req,
            urlResolver.ErrorMessageKey.SsoError
          )
        );
      } else {
        return res.redirect(urlResolver.getPortalSsoLoginUrl(req, data));
      }
    } catch (e) {
      logger.error(`onLoginResponse ${getError(e)}`);
      return res.redirect(
        urlResolver.getPortalAuthErrorUrl(
          req,
          urlResolver.ErrorMessageKey.SsoAuthFailed
        )
      );
    }
  };

  const createLogoutTemplateCallback = (_idp, _sp, user, method) => (
    template
  ) => {
    const _target = _idp;
    const _init = _sp;
    const metadata = { init: _init.entityMeta, target: _target.entityMeta };
    const initSetting = _init.entitySetting;
    const nameIDFormat = initSetting.nameIDFormat;
    const selectedNameIDFormat = Array.isArray(nameIDFormat)
      ? nameIDFormat[0]
      : nameIDFormat;
    const id = initSetting.generateID();
    const entityEndpoint = metadata.target.getSingleLogoutService(method);

    const data = {
      ID: id,
      Destination: entityEndpoint,
      Issuer: metadata.init.getEntityID(),
      IssueInstant: new Date().toISOString(),
      EntityID: metadata.init.getEntityID(),
      NameQualifier: metadata.target.getEntityID(),
      NameIDFormat: selectedNameIDFormat,
      NameID: user.logoutNameID,
      SessionIndex: user.sessionIndex,
    };

    const t = template.context || template;

    const rawSamlRequest = libsaml.replaceTagsByValue(t, data);
    return {
      id: id,
      context: rawSamlRequest,
    };
  };

  const createLogoutResponseTemplateCallback = (
    _idp,
    _sp,
    method,
    inResponseTo,
    relayState,
    statusCode
  ) => (template) => {
    const _target = _idp;
    const _init = _sp;
    const metadata = { init: _init.entityMeta, target: _target.entityMeta };
    const initSetting = _init.entitySetting;
    const id = initSetting.generateID();
    const entityEndpoint = metadata.target.getSingleLogoutService(method);

    const data = {
      ID: id,
      Destination: entityEndpoint,
      Issuer: metadata.init.getEntityID(),
      IssueInstant: new Date().toISOString(),
      InResponseTo: inResponseTo,
      StatusCode: statusCode,
    };

    const t = template.context || template;

    const rawSamlResponse = libsaml.replaceTagsByValue(t, data);
    return {
      id: id,
      context: rawSamlResponse,
      relayState,
    };
  };

  const sendLogoutRequest = async (req, res) => {
    try {
      if (!verifySetting(req)) {
        return res.redirect(urlResolver.getPortal500Url(req));
      }

      const sp = req.providersInfo.sp;
      const idp = req.providersInfo.idp;

      const isPost =
        req.providersInfo.settings.IdpSettings.SloBinding ===
        urn.namespace.binding.post;

      const method = isPost
        ? urn.wording.binding.post
        : urn.wording.binding.redirect;

      const relayState = urlResolver.getPortalAuthUrl(req);

      const userData = coder.decodeData(req.query["data"]);

      if (!userData) {
        logger.error(`coder.decodeData ${req.query["data"]}`);
        return res.redirect(urlResolver.getPortal500Url(req));
      }

      //const logoutUser = new LogoutModel(userData.NameId, userData.SessionId);
      const user = {
        logoutNameID: userData.NameId,
        sessionIndex: userData.SessionId,
      };

      const data = sp.createLogoutRequest(
        idp,
        method,
        user,
        relayState,
        createLogoutTemplateCallback(idp, sp, user, method)
      );

      return sendToIDP(res, data, isPost);
    } catch (e) {
      logger.error(`sendLogoutRequest ${getError(e)}`);
      return res.redirect(
        urlResolver.getPortalAuthErrorUrl(
          req,
          urlResolver.ErrorMessageKey.SsoError
        )
      );
    }
  };

  function getRelayState(requestInfo, req) {
    try {
      let { relayState } = requestInfo.extract;

      if (!relayState) {
        return req.body.RelayState || req.query.RelayState || null;
      }

      return relayState;
    } catch (e) {
      logger.error(`getRelayState failed ${getError(e)}`);
      return null;
    }
  }

  function getSessionIndex(requestInfo) {
    try {
      let { sessionIndex } = requestInfo.extract;

      if (!sessionIndex) return null;

      if (typeof sessionIndex === "object") {
        sessionIndex = requestInfo.extract.sessionIndex.sessionIndex;
      }
      return sessionIndex;
    } catch (e) {
      logger.error(`getSessionIndex failed ${getError(e)}`);
      return null;
    }
  }

  function getNameId(requestInfo) {
    try {
      const { nameID } = requestInfo.extract;

      return nameID;
    } catch (e) {
      logger.error(`getNameId failed ${getError(e)}`);
      return null;
    }
  }

  function getInResponseTo(requestInfo) {
    try {
      const { request } = requestInfo.extract;

      return (request && request.id) || null;
    } catch (e) {
      logger.error(`getInResponseTo failed ${getError(e)}`);
      return null;
    }
  }

  const sendPortalLogout = async (user, req) => {
    try {
      const data = coder.encodeData(user);

      if (!data) {
        const errorMessage = `EncodeData is EMPTY`;
        throw new Error(errorMessage);
        //return res.redirect(urlResolver.getPortal500Url(req));
      }

      const url = urlResolver.getPortalSsoLogoutUrl(req, data);

      logger.info(`SEND PORTAL LOGOUT`);

      const response = await fetch(url);

      logger.info(
        `PORTAL LOGOUT ${
          response.ok ? "success" : `fail (status=${response.statusText})`
        }`
      );

      return response;
    } catch (error) {
      return Promise.reject(error);
    }
  };

  const onLogout = async (req, res) => {
    try {
      if (!verifySetting(req)) {
        return res.redirect(urlResolver.getPortal500Url(req));
      }

      const sp = req.providersInfo.sp;
      const idp = req.providersInfo.idp;

      let isPost = req.method === "POST";
      let method = isPost
        ? urn.wording.binding.post
        : urn.wording.binding.redirect;
      const isResponse = req.query.SAMLResponse || req.body.SAMLResponse;

      if (isResponse) {
        const responseInfo = await sp.parseLogoutResponse(idp, method, req);

        if (config.app.logSamlData) {
          logger.debug(`onLogout->response ${JSON.stringify(responseInfo)}`);
        }

        return res.redirect(urlResolver.getPortalAuthUrl(req));
      } else {
        const requestInfo = await sp.parseLogoutRequest(idp, method, req);

        if (config.app.logSamlData) {
          logger.debug(`onLogout->request ${JSON.stringify(requestInfo)}`);
        }

        const nameID = getNameId(requestInfo);
        const sessionIndex = getSessionIndex(requestInfo);

        const logoutUser = new LogoutModel(nameID, sessionIndex);

        const response = await sendPortalLogout(logoutUser, req);

        const relayState = getRelayState(requestInfo, req);
        const inResponseTo = getInResponseTo(requestInfo);
        const statusCode = response.ok
          ? urn.namespace.statusCode.success
          : urn.namespace.statusCode.partialLogout;

        method = urn.wording.binding.redirect;
        isPost = false;

        const data = sp.createLogoutResponse(
          idp,
          requestInfo,
          method,
          relayState,
          createLogoutResponseTemplateCallback(
            idp,
            sp,
            method,
            inResponseTo,
            relayState,
            statusCode
          )
        );

        return sendToIDP(res, data, isPost);
      }
    } catch (e) {
      logger.error(`onLogout ${getError(e)}`);
      return res.redirect(
        urlResolver.getPortalAuthErrorUrl(
          req,
          urlResolver.ErrorMessageKey.SsoError
        )
      );
    }
  };

  /**
   * @desc Route to get Sp metadata
   * @param {object} req - request
   * @param {object} res - response
   */
  app.get(config.routes.metadata, getSpMetadata);

  /**
   * @desc Route to send login request from Sp to Idp
   * @param {object} req - request
   * @param {object} res - response
   */
  app.get(config.routes.login, sendLoginRequest);

  /**
   * @desc Route to send login request from Sp to Idp
   * @param {object} req - request
   * @param {object} res - response
   */
  app.post(config.routes.login, sendLoginRequest);

  /**
   * @desc Route to read login response from Idp to Sp
   * @param {object} req - request with assertion info
   * @param {object} res - response
   */
  app.get(config.routes.login_callback, onLoginResponse);

  /**
   * @desc Route to read login response from Idp to Sp
   * @param {object} req - request with assertion info
   * @param {object} res - response
   */
  app.post(config.routes.login_callback, onLoginResponse);

  /**
   * @desc Route to send logout request from Sp to Idp
   * @param {object} req - request with data parameter (NameID required)
   * @param {object} res - response
   */
  app.get(config.routes.logout, sendLogoutRequest);

  /**
   * @desc Route to read logout response from Idp to Sp
   * @param {object} req - request with logout info
   * @param {object} res - response
   */
  app.get(config.routes.logout_callback, onLogout);

  /**
   * @desc Route to read logout response from Idp to Sp
   * @param {object} req - request with logout info
   * @param {object} res - response
   */
  app.post(config.routes.logout_callback, onLogout);

  /**
    * @desc Route to generate a certificate
    */
  app.get(config.routes.generatecert, onGenerateCert);

  /**
    * @desc Route to validate the certificate
    */
  app.post(config.routes.validatecerts, onValidateCerts);

  /**
    * @desc Route to upload metadata
    */
  app.post(config.routes.uploadmetadata, onUploadMetadata);

  /**
    * @desc Route to load metadata
    */
  app.post(config.routes.loadmetadata, onLoadMetadata);
    
  /**
   * @desc Catch any untracked routes
   * @param {object} req - request with data parameter (NameID required)
   * @param {object} res - response
   */
  app.use(function (req, res, next) {
    next(res.redirect(urlResolver.getPortal404Url(req)));
  });
};
