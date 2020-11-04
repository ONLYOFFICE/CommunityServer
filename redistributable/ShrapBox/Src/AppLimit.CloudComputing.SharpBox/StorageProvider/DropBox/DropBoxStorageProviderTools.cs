using System;
using System.Collections.Generic;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox.Logic;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox
{
    /// <summary>
    /// This class contains a couple a tools which will be helpful
    /// when working with dropbox only
    /// </summary>
    public static class DropBoxStorageProviderTools
    {
        private const String DropBoxMobileLogin = DropBoxStorageProviderService.DropBoxBaseUrl + "/0/token";

        /// <summary>
        /// This method retrieves a new request token from the dropbox server
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <returns></returns>
        public static DropBoxRequestToken GetDropBoxRequestToken(DropBoxConfiguration configuration, String consumerKey, String consumerSecret)
        {
            // build the consumer context
            var consumerContext = new OAuthConsumerContext(consumerKey, consumerSecret);

            // build up the oauth session
            var serviceContext = new OAuthServiceContext(configuration.RequestTokenUrl.ToString(),
                                                         configuration.AuthorizationTokenUrl.ToString(), configuration.AuthorizationCallBack.ToString(),
                                                         configuration.AccessTokenUrl.ToString());

            // get a request token from the provider      
            var svc = new OAuthService();
            var oauthToken = svc.GetRequestToken(serviceContext, consumerContext);
            return oauthToken != null ? new DropBoxRequestToken(oauthToken) : null;
        }

        /// <summary>
        /// This method builds derived from the request token a valid authorization url which can be used
        /// for web applications
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="dropBoxRequestToken"></param>
        /// <returns></returns>
        public static String GetDropBoxAuthorizationUrl(DropBoxConfiguration configuration, DropBoxRequestToken dropBoxRequestToken)
        {
            // build the auth url
            return OAuthUrlGenerator.GenerateAuthorizationUrl(configuration.AuthorizationTokenUrl.ToString(),
                                                              configuration.AuthorizationCallBack.ToString(),
                                                              dropBoxRequestToken.RealToken);
        }

        /// <summary>
        /// This method is able to exchange the request token into an access token which can be used in 
        /// sharpbox. It is necessary that the user validated the request via authorization url otherwise 
        /// this call wil results in an unauthorized exception!
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="consumerKey"></param>
        /// <param name="ConsumerSecret"></param>
        /// <param name="dropBoxRequestToken"></param>
        /// <returns></returns>
        public static ICloudStorageAccessToken ExchangeDropBoxRequestTokenIntoAccessToken(DropBoxConfiguration configuration, String consumerKey, String ConsumerSecret, DropBoxRequestToken dropBoxRequestToken)
        {
            // build the consumer context
            var consumerContext = new OAuthConsumerContext(consumerKey, ConsumerSecret);

            // build up the oauth session
            var serviceContext = new OAuthServiceContext(configuration.RequestTokenUrl.ToString(),
                                                         configuration.AuthorizationTokenUrl.ToString(), configuration.AuthorizationCallBack.ToString(),
                                                         configuration.AccessTokenUrl.ToString());

            // build the access token
            var svc = new OAuthService();
            var accessToken = svc.GetAccessToken(serviceContext, consumerContext, dropBoxRequestToken.RealToken);
            if (accessToken == null)
                throw new UnauthorizedAccessException();

            // create the access token 
            return new DropBoxToken(accessToken,
                                    new DropBoxBaseTokenInformation
                                        {
                                            ConsumerKey = consumerKey,
                                            ConsumerSecret = ConsumerSecret
                                        });
        }

        /// <summary>
        /// This method returns the account information of a dropbox account
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static DropBoxAccountInfo GetAccountInformation(ICloudStorageAccessToken token)
        {
            // generate the dropbox service
            var service = new DropBoxStorageProviderService();

            // generate a session
            var session = service.CreateSession(token, DropBoxConfiguration.GetStandardConfiguration());
            if (session == null)
                return null;

            // receive acc info
            var accInfo = service.GetAccountInfo(session);

            // close the session
            service.CloseSession(session);

            // go ahead
            return accInfo;
        }

        /// <summary>
        /// This method returns the public URL of a DropBox file or folder
        /// </summary>
        /// <param name="token"></param>
        /// <param name="fsEntry"></param>
        /// <returns></returns>
        public static Uri GetPublicObjectUrl(ICloudStorageAccessToken token, ICloudFileSystemEntry fsEntry)
        {
            // check parameters
            if (fsEntry == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            // get the resource path
            var resourcePath = GenericHelper.GetResourcePath(fsEntry);

            // check if it is a public dir
            if (!resourcePath.ToLower().Contains("/public/"))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            // get accoutn inf
            var accInfo = GetAccountInformation(token);

            // http;//dl.dropbox.com/u/" + userid / + folder + filename
            var uri = "http://dl.dropbox.com/u/" + accInfo.UserId.ToString() + resourcePath.Replace("/Public", "");

            // go ahead
            return new Uri(uri);
        }

        /// <summary>
        /// This method offers the mobile login api of dropbox for users who are migrating from version 0 of the 
        /// dropbox API because version 1 supports token based logins only
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="appkey"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        public static ICloudStorageAccessToken LoginWithMobileAPI(String username, String password, String appkey, String appsecret)
        {
            // get the configuration
            var configuration = DropBoxConfiguration.GetStandardConfiguration();

            // build the consumer context
            var consumerContext = new OAuthConsumerContext(appkey, appsecret);

            // build up the oauth session
            var serviceContext = new OAuthServiceContext(configuration.RequestTokenUrl.ToString(),
                                                         configuration.AuthorizationTokenUrl.ToString(), configuration.AuthorizationCallBack.ToString(),
                                                         configuration.AccessTokenUrl.ToString());

            // get a request token from the provider      
            var svc = new OAuthService();
            var oAuthRequestToken = svc.GetRequestToken(serviceContext, consumerContext);
            if (oAuthRequestToken == null)
            {
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidConsumerKeySecret);
            }
            var dropBoxRequestToken = new DropBoxToken(oAuthRequestToken, new DropBoxBaseTokenInformation { ConsumerKey = appkey, ConsumerSecret = appsecret });

            // generate the dropbox service
            var service = new DropBoxStorageProviderService();

            // build up a request Token Session
            var requestSession = new DropBoxStorageProviderSession(dropBoxRequestToken, configuration, consumerContext, service);

            // build up the parameters
            var param = new Dictionary<String, String>
                {
                    { "email", username },
                    { "password", password }
                };

            // call the mobile login api 
            string result;

            try
            {
                int code;
                result = DropBoxRequestParser.RequestResourceByUrl(DropBoxMobileLogin, param, service, requestSession, out code);
                if (result.Length == 0)
                    throw new UnauthorizedAccessException();
            }
            catch (System.Web.HttpException netex)
            {
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorCouldNotContactStorageService, netex);
            }

            // exchange a request token for an access token
            var accessToken = new DropBoxToken(result);

            // adjust the token 
            if (accessToken.BaseTokenInformation == null)
            {
                accessToken.BaseTokenInformation = new DropBoxBaseTokenInformation
                    {
                        ConsumerKey = appkey,
                        ConsumerSecret = appsecret
                    };
            }

            // go ahead
            return accessToken;
        }
    }
}