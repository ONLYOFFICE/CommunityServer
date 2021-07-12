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

module.exports = (app, config, logger) => {
  const urlResolver = require("../utils/resolver")(logger);
  const coder = require("../utils/coder");
  const converter = require("../utils/converter")(logger);
  const _ = require("lodash");
  const fetch = require("node-fetch");
  const routes = _.values(config.routes);

  const fetchConfig = async (req, res, next) => {
    const foundRoutes =
      req.url && req.url.length > 0
        ? routes.filter(function (route) {
            return 0 === req.url.indexOf(route);
          })
        : [];

    if (!foundRoutes.length) {
      logger.error(`invalid route ${req.originalUrl}`);
      return res.redirect(urlResolver.getPortal404Url(req));
    }

    const baseUrl = urlResolver.getBaseUrl(req);

    const promise = new Promise(async (resolve) => {
      var url = urlResolver.getPortalSsoConfigUrl(req);

      const response = await fetch(url);

      if (!response || response.status === 404) {
        if (response) {
          logger.error(response.statusText);
        }
        return resolve(res.redirect(urlResolver.getPortal404Url(req)));
      } else if (response.status !== 200) {
        throw new Error(`Invalid response status ${response.status}`);
      } else if (!response.body) {
        throw new Error("Empty config response");
      }

      const text = await response.text();

      const ssoConfig = coder.decodeData(text);

      const idp = converter.toIdp(ssoConfig);

      const sp = converter.toSp(ssoConfig, baseUrl);

      const providersInfo = {
        sp: sp,
        idp: idp,
        mapping: ssoConfig.FieldMapping,
        settings: ssoConfig,
      };

      req.providersInfo = providersInfo;

      return resolve(next());
    }).catch((error) => {
      logger.error(error);
      return res.redirect(
        urlResolver.getPortalAuthErrorUrl(
          req,
          urlResolver.ErrorMessageKey.SsoError
        )
      );
    });

    return promise;
  };

  app.use(fetchConfig);
};
