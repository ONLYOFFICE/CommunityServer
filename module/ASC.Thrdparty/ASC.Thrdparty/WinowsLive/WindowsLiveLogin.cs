/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


/*
 * FILE:        WindowsLiveLogin.cs
 *                                                                      
 * DESCRIPTION: Sample implementation of Web Authentication and Delegated 
 *              Authentication protocol in C#. Also includes trusted 
 *              sign-in and application verification sample 
 *              implementations.
 *
 * VERSION:     1.1
 *
 * Copyright (c) 2008 Microsoft Corporation.  All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections;
using System.Web;
using System.Web.Configuration;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;

namespace WindowsLive
{
    /// <summary>
    /// Sample implementation of Web Authentication and Delegated Authentication 
    /// protocol. Also includes trusted sign-in and application 
    /// verification sample implementations.
    /// </summary>
    public class WindowsLiveLogin
    {
        /// <summary>
        /// Stub implementation for logging debug output. You can run
        /// a tool such as 'dbmon' to see the output.
        /// </summary>
        static void debug(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
            System.Diagnostics.Debug.Flush();
        }

        /// <summary>
        /// Initialize the WindowsLiveLogin module with the
        /// application ID and secret key.
        ///
        /// We recommend that you employ strong measures to protect
        /// the secret key. The secret key should never be
        /// exposed to the Web or other users.
        /// </summary>
        public WindowsLiveLogin(string appId, string secret) : 
            this(appId, secret, null){}

        /// <summary>
        /// Initialize the WindowsLiveLogin module with the
        /// application ID, secret key, and security algorithm.
        ///
        /// We recommend that you employ strong measures to protect
        /// the secret key. The secret key should never be
        /// exposed to the Web or other users.
        /// </summary>
        public WindowsLiveLogin(string appId, string secret, string securityAlgorithm) :
            this(appId, secret, securityAlgorithm, false){}

        /// <summary>
        /// Initialize the WindowsLiveLogin module with the
        /// forceDelAuthNonProvisioned flag, policy URL, and return URL.
        /// 
        /// The 'force_delauth_nonprovisioned' flag indicates whether
        /// your application is registered for Delegated Authentication 
        /// (that is, whether it uses an application ID and secret key). We 
        /// recommend that your Delegated Authentication application always 
        /// be registered for enhanced security and functionality. 
        /// </summary>
        public WindowsLiveLogin(bool forceDelAuthNonProvisioned, string policyUrl, string returnUrl)
        {
            ForceDelAuthNonProvisioned = forceDelAuthNonProvisioned;
            PolicyUrl = policyUrl;
            ReturnUrl = returnUrl;
        }

        /// <summary>
        /// Initialize the WindowsLiveLogin module with the
        /// application ID, secret key, security algorithm and 
        /// forceDelAuthNonProvisioned flag.
        ///
        /// We recommend that you employ strong measures to protect
        /// the secret key. The secret key should never be
        /// exposed to the Web or other users.
        /// 
        /// The 'force_delauth_nonprovisioned' flag indicates whether
        /// your application is registered for Delegated Authentication 
        /// (that is, whether it uses an application ID and secret key). We 
        /// recommend that your Delegated Authentication application always 
        /// be registered for enhanced security and functionality. 
        /// </summary>
        public WindowsLiveLogin(string appId, string secret, string securityAlgorithm, bool forceDelAuthNonProvisioned) :
            this(appId, secret, securityAlgorithm, forceDelAuthNonProvisioned, null){}

        /// <summary>
        /// Initialize the WindowsLiveLogin module with the
        /// application ID, secret key, security algorithm,    
        /// forceDelAuthNonProvisioned and policy URL use.
        ///
        /// We recommend that you employ strong measures to protect
        /// the secret key. The secret key should never be
        /// exposed to the Web or other users.
        /// 
        /// The 'force_delauth_nonprovisioned' flag indicates whether
        /// your application is registered for Delegated Authentication 
        /// (that is, whether it uses an application ID and secret key). We 
        /// recommend that your Delegated Authentication application always 
        /// be registered for enhanced security and functionality. 
        /// </summary>
        public WindowsLiveLogin(string appId, string secret, string securityAlgorithm, bool forceDelAuthNonProvisioned, string policyUrl) :
            this(appId, secret, securityAlgorithm, forceDelAuthNonProvisioned, policyUrl, null){}

        /// <summary>
        /// Initialize the WindowsLiveLogin module with the
        /// application ID, secret key, security algorithm,    
        /// forceDelAuthNonProvisioned, policy URL and return URL.
        ///
        /// We recommend that you employ strong measures to protect
        /// the secret key. The secret key should never be
        /// exposed to the Web or other users.
        /// 
        /// The 'force_delauth_nonprovisioned' flag indicates whether
        /// your application is registered for Delegated Authentication 
        /// (that is, whether it uses an application ID and secret key). We 
        /// recommend that your Delegated Authentication application always 
        /// be registered for enhanced security and functionality.
        public WindowsLiveLogin(string appId, string secret, string securityAlgorithm, bool forceDelAuthNonProvisioned, string policyUrl, string returnUrl)
        {
            ForceDelAuthNonProvisioned = forceDelAuthNonProvisioned;
            AppId = appId;
            Secret = secret;
            SecurityAlgorithm = securityAlgorithm;
            PolicyUrl = policyUrl;
            ReturnUrl = returnUrl;
        }  

        /// <summary>
        /// Initialize the WindowsLiveLogin module from the
        /// web.config file if loadAppSettings is true. Otherwise,
        /// you will have to manually set the AppId, Secret and
        /// SecurityAlgorithm properties.
        /// 
        /// In a Delegated Authentication scenario, you may also specify
        /// the return and privacy policy URLs to use, as shown in the 
        /// Delegated Authentication samples.     
        /// </summary>
        public WindowsLiveLogin(bool loadAppSettings)
        {
            if (!loadAppSettings) { return; }

            NameValueCollection appSettings = WebConfigurationManager.AppSettings;
            if (appSettings == null) 
            {
                throw new IOException("Error: WindowsLiveLogin: Failed to load the Web application settings.");
            }

            string forceDelAuthNonProvisioned = appSettings["wll_force_delauth_nonprovisioned"];

            if (!string.IsNullOrEmpty(forceDelAuthNonProvisioned) && 
                (forceDelAuthNonProvisioned.ToLower() == "true"))
            {
                ForceDelAuthNonProvisioned = true;
            }
            else
            {
                ForceDelAuthNonProvisioned = false;
            }

            AppId = appSettings["wll_appid"];
            Secret = appSettings["wll_secret"];
            OldSecret = appSettings["wll_oldsecret"];
            OldSecretExpiry = appSettings["wll_oldsecretexpiry"];
            SecurityAlgorithm = appSettings["wll_securityalgorithm"];
            PolicyUrl = appSettings["wll_policyurl"];
            ReturnUrl = appSettings["wll_returnurl"];
            BaseUrl = appSettings["wll_baseurl"];
            SecureUrl = appSettings["wll_secureurl"];
            ConsentUrl = appSettings["wll_consenturl"];
        }

        /// <summary><![CDATA[
        /// Initialize the WindowsLiveLogin module from a settings file. 
        /// 
        /// 'settingsFile' specifies the location of the XML settings
        /// file containing the application ID, secret key, an optional
        /// security algorithm and a privacy policy URL (required for
        /// Delegated Auth).  The file is of the following format:
        /// 
        /// <windowslivelogin>
        ///   <appid>APPID</appid>
        ///   <secret>SECRET</secret>
        ///   <securityalgorithm>wsignin1.0</securityalgorithm>
        ///   <policyurl>http://[your domain]/[your privacy policy]</policyurl>
        ///   <returnurl>http://[your domain]/[your return url]</policyurl>
        /// </windowslivelogin>
        /// 
        /// In a Delegated Authentication scenario, you may also specify
        /// 'returnurl' and 'policyurl' in the settings file. 
        ///  
        /// We recommend that you store the Windows Live Login settings file
        /// in an area on your server that cannot be accessed through
        /// the Internet. This file contains important confidential
        /// information.      
        /// ]]></summary>
        public WindowsLiveLogin(string settingsFile)
        {
            NameValueCollection settings = parseSettings(settingsFile);

            string forceDelAuthNonProvisioned = settings["force_delauth_nonprovisioned"];

            if (!string.IsNullOrEmpty(forceDelAuthNonProvisioned) && 
                (forceDelAuthNonProvisioned.ToLower() == "true"))
            {
                ForceDelAuthNonProvisioned = true;
            }
            else
            {
                ForceDelAuthNonProvisioned = false;
            }

            AppId = settings["appid"];
            Secret = settings["secret"];
            OldSecret = settings["oldsecret"];
            OldSecretExpiry = settings["oldsecretexpiry"];
            SecurityAlgorithm = settings["securityalgorithm"];
            PolicyUrl = settings["policyurl"];
            ReturnUrl = settings["returnurl"];
            BaseUrl = settings["baseurl"];
            SecureUrl = settings["secureurl"];
            ConsentUrl = settings["consenturl"];
        }

        string appId;

        /// <summary>
        /// Gets or sets the application ID.
        /// </summary>
        public string AppId
        {
            set 
            {
                if (string.IsNullOrEmpty(value)) 
                {
                    if (ForceDelAuthNonProvisioned)
                    {
                        return;
                    }

                    throw new ArgumentNullException("value");
                }

                Regex re = new Regex(@"^\w+$");
                if (!re.IsMatch(value))
                {
                    throw new ArgumentException("Error: AppId: Application ID must be alphanumeric: " + value);
                }

                appId = value; 
            }

            get 
            { 
                if (string.IsNullOrEmpty(appId)) 
                {
                    throw new InvalidOperationException("Error: AppId: Application ID was not set. Aborting.");
                }
                
                return appId; 
            }
        }

        byte[] cryptKey;
        byte[] signKey;

        /// <summary>
        /// Sets your secret key. Use this method if you did not specify 
        /// a secret key at initialization.
        /// </summary>
        public string Secret
        {
            set 
            {
                if (string.IsNullOrEmpty(value)) 
                {
                    if (ForceDelAuthNonProvisioned)
                    {
                        return;
                    }

                    throw new ArgumentNullException("value");
                }

                if (value.Length < 16)
                {
                    throw new ArgumentException("Error: Secret: Secret key is expected to be longer than 16 characters: " + value.Length);
                }
                    
                cryptKey = derive(value, "ENCRYPTION");
                signKey = derive(value, "SIGNATURE");
            }

            get { return null; }
        }

        byte[] oldCryptKey;
        byte[] oldSignKey;

        /// <summary>
        /// Sets your old secret key.
        /// 
        /// Use this property to set your old secret key if you are in the
        /// process of transitioning to a new secret key. You may need this 
        /// property because the Windows Live ID servers can take up to 
        /// 24 hours to propagate a new secret key after you have updated 
        /// your application settings.
        /// 
        /// If an old secret key is specified here and has not expired
        /// (as determined by the OldSecretExpiry setting), it will be used
        /// as a fallback if token decryption fails with the new secret 
        /// key.
        /// </summary>
        public string OldSecret
        {
            set 
            {
                if (string.IsNullOrEmpty(value)) 
                {
                    return;
                }
                    
                if (value.Length < 16)
                {
                    throw new ArgumentException("Error: OldSecret: Secret key is expected to be longer than 16 characters: " + value.Length);
                }
                
                oldCryptKey = derive(value, "ENCRYPTION");
                oldSignKey = derive(value, "SIGNATURE");
            }

            get { return null; }
        }

        string oldSecretExpiryString;
        DateTime oldSecretExpiry;

        /// <summary>
        /// Sets or gets the expiry time for your old secret key.
        /// 
        /// After this time has passed, the old secret key will no longer be
        /// used even if token decryption fails with the new secret key.
        ///
        /// The old secret expiry time is represented as the number of seconds
        /// elapsed since January 1, 1970. 
        /// </summary>
        public string OldSecretExpiry
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                oldSecretExpiryString = value;
                int timestampInt;

                try 
                {
                    timestampInt = Convert.ToInt32(value);
                } 
                catch (Exception) 
                {
                    throw new ArgumentException("Error: OldSecretExpiry: Invalid timestamp: " 
                                                + value);
                }
            
                DateTime refTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                oldSecretExpiry = refTime.AddSeconds(timestampInt);
            }

            get { return oldSecretExpiryString; }
        }
        
        string securityAlgorithm;

        /// <summary>
        /// Sets or gets the version of the security algorithm being used.
        /// </summary>
        public string SecurityAlgorithm
        {
            set { securityAlgorithm = value; }

            get 
            { 
                if (string.IsNullOrEmpty(securityAlgorithm))
                {
                    return "wsignin1.0";
                }

                return securityAlgorithm; 
            }
        }

        bool forceDelAuthNonProvisioned = false;

        /// <summary>
        /// Sets or gets a flag that indicates whether Delegated Authentication
        /// is non-provisioned (i.e. does not use an application ID or secret
        /// key).
        /// </summary>
        public bool ForceDelAuthNonProvisioned
        {
            set { forceDelAuthNonProvisioned = value; }

            get { return forceDelAuthNonProvisioned; }
        }

        string policyUrl;
        
        /// <summary>
        /// Sets or gets the privacy policy URL.
        /// 
        /// Set the property for Delegated Authentication, if you did 
        /// not provide one at initialization time.
        /// </summary>
        public string PolicyUrl
        {
            set 
            { 
                if (string.IsNullOrEmpty(value) && ForceDelAuthNonProvisioned)
                {
                    throw new ArgumentNullException("value");
                }
                    
                policyUrl = value; 
            }

            get 
            { 
                if (string.IsNullOrEmpty(policyUrl))
                {
                    debug("Warning: In the initial release of Delegated Auth, a Policy URL must be configured in the SDK for both provisioned and non-provisioned scenarios.");

                    if (ForceDelAuthNonProvisioned)
                    {
                        throw new InvalidOperationException("Error: PolicyUrl: Policy URL must be set in a Delegated Auth non-provisioned scenario. Aborting.");
                    }
                }

                return policyUrl; 
            }
        }

        string returnUrl;

        /// <summary>
        /// Sets or gets the return URL--the URL on your site to which the consent 
        /// service redirects users (along with the action, consent token, 
        /// and application context) after they have successfully provided 
        /// consent information for Delegated Authentication. 
        /// 
        /// This value will override the return URL specified during 
        /// registration.
        /// </summary>
        public string ReturnUrl
        {
            set 
            { 
                if (string.IsNullOrEmpty(value) && ForceDelAuthNonProvisioned)
                {
                    throw new ArgumentNullException("value");
                }

                returnUrl = value; 
            }

            get 
            { 
                if (string.IsNullOrEmpty(returnUrl) && ForceDelAuthNonProvisioned)
                {
                    throw new InvalidOperationException("Error: ReturnUrl: Return URL must be specified in a delegated auth non-provisioned scenario. Aborting.");
                }

                return returnUrl; 
            }
        }

        string baseUrl;
        
        /// <summary>
        /// Sets or gets the URL to use for the Windows Live Login server. 
        /// You should not have to use or change this. Furthermore, we
        /// recommend that you use the Sign In control instead of
        /// the URL methods provided here.
        /// </summary>
        public string BaseUrl
        {
            set { baseUrl = value; }

            get 
            { 
                if (string.IsNullOrEmpty(baseUrl))
                {
                    return "http://login.live.com/";
                }

                return baseUrl; 
            }
        }

        string secureUrl;
        
        /// <summary>
        /// Sets or gets the secure (HTTPS) URL to use for the Windows Live
        /// Login server.  You should not have to use or change this
        /// directly.  
        // </summary>
        public string SecureUrl
        {
            set { secureUrl = value; }
            
            get 
            { 
                if (string.IsNullOrEmpty(secureUrl))
                {
                    return "https://login.live.com/";
                }

                return secureUrl; 
            }
        }

        string consentUrl;
        
        /// <summary>
        /// Sets or gets the URL to use for the Windows Live Consent server. You
        /// should not have to use or change this directly.
        /// </summary>
        public string ConsentUrl
        {
            set { consentUrl = value; }
            
            get 
            { 
                if (string.IsNullOrEmpty(consentUrl))
                {
                    return "https://consent.live.com/";
                }

                return consentUrl;
            }
        }

        /* Methods for Web Authentication support. */

        /// <summary>
        /// Returns the sign-in URL to use for the Windows Live Login server.
        /// We recommend that you use the Sign In control instead.
        /// </summary>
        /// <returns>Sign-in URL</returns>
        public string GetLoginUrl()
        {
            return GetLoginUrl(null);
        }

        /// <summary>
        /// Returns the sign-in URL to use for the Windows Live Login server.
        /// We recommend that you use the Sign In control instead.
        /// </summary>
        /// <param name="context">If you specify it, <paramref
        /// name="context"/> will be returned as-is in the sign-in
        /// response for site-specific use.</param>
        /// <returns>Sign-in URL</returns>
        public string GetLoginUrl(string context)
        {
            return GetLoginUrl(context, null);
        }

        /// <summary>
        /// Returns the sign-in URL to use for the Windows Live Login server.
        /// We recommend that you use the Sign In control instead.
        /// </summary>
        /// <param name="context">If you specify it, <paramref
        /// name="context"/> will be returned as-is in the sign-in
        /// response for site-specific use.</param>
        /// <param name="market">The language in which the sign-in page is 
        /// displayed is configured by culture ID (For example, 'fr-fr' or 
        /// 'en-us') specified in the 'market' parameter.</param>
        /// <returns>Sign-in URL</returns>
        public string GetLoginUrl(string context, string market)
        {
            string alg = "&alg=" + SecurityAlgorithm;

            context = string.IsNullOrEmpty(context) ? 
              string.Empty : "&appctx=" + HttpUtility.UrlEncode(context);

            market = string.IsNullOrEmpty(market) ? 
              string.Empty : "&mkt=" + HttpUtility.UrlEncode(market);

            return BaseUrl + "wlogin.srf?appid=" + AppId + 
              alg + context + market;
        }

        /// <summary>
        /// Returns the sign-out URL to use for the Windows Live Login server.
        /// We recommend that you use the Sign In control instead.
        /// </summary>
        /// <returns>Sign-out URL</returns>
        public string GetLogoutUrl()
        {
            return GetLogoutUrl(null);
        }

        /// <summary>
        /// Returns the sign-out URL to use for the Windows Live Login server.
        /// We recommend that you use the Sign In control instead.
        /// </summary>
        /// <param name="market">The language in which the sign-in page is 
        /// displayed is configured by culture ID (For example, 'fr-fr' or 
        /// 'en-us') specified in the 'market' parameter.</param>
        /// <returns>Sign-out URL</returns>
        public string GetLogoutUrl(string market)
        {
            market = string.IsNullOrEmpty(market) ? 
              string.Empty : "&mkt=" + HttpUtility.UrlEncode(market);

            return BaseUrl + "logout.srf?appid=" + AppId + market;
        }

        /// <summary>
        /// Holds the user information after a successful sign-in.
        /// </summary>
        public class User
        {
            public User(string timestamp, string id, string flags, string context, string token)
            {
                setTimestamp(timestamp);
                setId(id);
                setFlags(flags);
                setContext(context);
                setToken(token);
            }

            DateTime timestamp;

            /// <summary>
            ///  Returns the timestamp as obtained from the SSO token.
            /// </summary>
            public DateTime Timestamp { get { return timestamp; } }

            /// <summary>
            /// Sets the Unix timestamp.
            /// </summary>
            /// <param name="timestamp"></param>
            private void setTimestamp(string timestamp)
            {
                if (string.IsNullOrEmpty(timestamp))
                {
                    throw new ArgumentException("Error: User: Null timestamp in token.");
                }

                int timestampInt;

                try 
                {
                    timestampInt = Convert.ToInt32(timestamp);
                } 
                catch (Exception) 
                {
                    throw new ArgumentException("Error: User: Invalid timestamp: " 
                                                + timestamp);
                }

                DateTime refTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                this.timestamp = refTime.AddSeconds(timestampInt);
            }

            string id;

            /// <summary>
            /// Returns the pairwise unique ID for the user.
            /// </summary>
            public string Id { get { return id; } }

            /// <summary>
            /// Sets the pairwise unique ID for the user.
            /// </summary>
            /// <param name="id">User id</param>
            private void setId(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Error: User: Null id in token.");
                }

                Regex re = new Regex(@"^\w+$");
                if (!re.IsMatch(id))
                {
                    throw new ArgumentException("Error: User: Invalid id: " + id);
                }

                this.id = id;
            }

            bool usePersistentCookie;

            /// <summary>
            /// Indicates whether the application
            /// is expected to store the user token in a session or
            /// persistent cookie.
            /// </summary>
            public bool UsePersistentCookie { get { return usePersistentCookie; } }

            /// <summary>
            /// Sets the usePersistentCookie flag for the user.
            /// </summary>
            /// <param name="flags"></param>
            private void setFlags(string flags)
            {
                this.usePersistentCookie = false;

                if (!string.IsNullOrEmpty(flags))
                {
                    try 
                    {
                        int flagsInt = Convert.ToInt32(flags);
                        this.usePersistentCookie = ((flagsInt % 2) == 1);
                    }
                    catch (Exception) 
                    { 
                        throw new ArgumentException("Error: User: Invalid flags: " 
                                                    + flags);
                    }
                }
            }

            string context;

            /// <summary>
            /// Returns the application context that was originally passed
            /// to the sign-in request, if any.
            /// </summary>
            public string Context { get { return context; } }

            /// <summary>
            /// Sets the the Application context.
            /// </summary>
            /// <param name="context"></param>
            private void setContext(string context)
            {
                this.context = context;
            }

            string token;

            /// <summary>
            /// Returns the encrypted Web Authentication token containing 
            /// the UID. This can be cached in a cookie and the UID can be
            /// retrieved by calling the ProcessToken method.
            /// </summary>
            public string Token { get { return token; } }

            /// <summary>
            /// Sets the the User token.
            /// </summary>
            /// <param name="token"></param>
            private void setToken(string token)
            {
                this.token = token;
            }
        }

        /// <summary>
        /// Processes the sign-in response from the Windows Live Login server.
        /// </summary>
        ///
        /// <param name="query">Contains the preprocessed POST query
        /// such as that returned by HttpRequest.Form</param>
        /// 
        /// <returns>The method returns a User object on successful
        /// sign-in; otherwise null.</returns>
        public User ProcessLogin(NameValueCollection query)
        {
            if (query == null)
            {
                debug("Error: ProcessLogin: Invalid query.");
                return null;
            }

            string action = query["action"];

            if (action != "login")
            {
                debug("Warning: ProcessLogin: query action ignored: " + action);
                return null;
            }

            string token = query["stoken"];
            string context = query["appctx"];

            if (context != null)
            {
                context = HttpUtility.UrlDecode(context);
            }

            return ProcessToken(token, context);
        }

        /// <summary>
        /// Decodes and validates a Web Authentication token. Returns a User
        /// object on success.
        /// </summary>
        public User ProcessToken(string token)
        {
            return ProcessToken(token, null);
        }

        /// <summary>
        /// Decodes and validates a Web Authentication token. Returns a User
        /// object on success. If a context is passed in, it will be
        /// returned as the context field in the User object.
        /// </summary>
        /// <param name="token">Web Authentication token</param>
        /// <param name="context">If you specify it, <paramref
        /// name="context"/> will be returned as-is in the sign-in
        /// response for site-specific use.</param>        
        /// <returns>User object</returns>
        public User ProcessToken(string token, string context)
        {
            if (string.IsNullOrEmpty(token))
            {
                debug("Error: ProcessToken: Invalid token.");
                return null;
            }

            string stoken = DecodeAndValidateToken(token);

            if (string.IsNullOrEmpty(stoken))
            {
                debug("Error: ProcessToken: Failed to decode/validate token: " +
                      token);
                return null;
            }

            NameValueCollection parsedToken = parse(stoken);
            if (parsedToken == null || parsedToken.Count < 3)
            {
                debug("Error: ProcessToken: Failed to parse token after decoding: " + 
                      token);
                return null;
            }

            string appId = parsedToken["appid"];
            if (appId != AppId)
            {
                debug("Error: ProcessToken: Application ID in token did not match ours: " + 
                      appId +  ", " + AppId);
                return null;
            }

            User user = null;
            try 
            {
                user = new User(parsedToken["ts"], 
                                parsedToken["uid"], 
                                parsedToken["flags"],
                                context, token);
            } 
            catch (Exception e)
            {
                debug("Error: ProcessToken: Contents of token considered invalid: " + e);
            }
            return user;
        }

        /// <summary>
        /// Returns an appropriate content type and body
        /// response that the application handler can return to
        /// signify a successful sign-out from the application.
        /// 
        /// When a user signs out of Windows Live or a Windows Live
        /// application, a best-effort attempt is made to sign the user out
        /// from all other Windows Live applications the user might be signed
        /// in to. This is done by calling the handler page for each
        /// application with 'action' parameter set to 'clearcookie' in the query
        /// string. The application handler is then responsible for clearing
        /// any cookies or data associated with the sign-in. After successfully
        /// signing the user out, the handler should return a GIF (any
        /// GIF) as response to the action=clearcookie query.
        /// </summary>
        public void GetClearCookieResponse(out string type, out byte[] content)
        {
            const string gif = 
              "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAEALAAAAAABAAEAAAIBTAA7";
            type = "image/gif";
            content = Convert.FromBase64String(gif);
        }

        /* Methods for Delegated Authentication support. */
        
        /// <summary>
        /// Returns the consent URL to use for Delegated Authentication for
        /// the given comma-delimited list of offers.
        /// </summary>
        /// <param name="offers">Comma-delimited list of offers.</param>
        /// <returns>Consent URL</returns>
        public string GetConsentUrl(string offers)
        {
            return GetConsentUrl(offers, null);
        }

        /// <summary>
        /// Returns the consent URL to use for Delegated Authentication for
        /// the given comma-delimited list of offers.
        /// </summary>
        /// <param name="offers">Comma-delimited list of offers.</param>
        /// <param name="context">If you specify it, <paramref
        /// name="context"/> will be returned as-is in the consent 
        /// response for site-specific use.</param>
        /// <returns>Consent URL</returns>
        public string GetConsentUrl(string offers, string context)
        {
            return GetConsentUrl(offers, context, null);
        }

        /// <summary>
        /// Returns the consent URL to use for Delegated Authentication for
        /// the given comma-delimited list of offers.
        /// </summary>
        /// <param name="offers">Comma-delimited list of offers.</param>
        /// <param name="context">If you specify it, <paramref
        /// name="context"/> will be returned as-is in the consent 
        /// response for site-specific use.</param>
        /// <param name="ru">The registered/configured return URL will be 
        /// overridden by 'ru' specified here.</param>
        /// <returns>Consent URL</returns>
        public string GetConsentUrl(string offers, string context, string ru)
        {
            return GetConsentUrl(offers,  context, ru, null);
        }

        /// <summary>
        /// Returns the consent URL to use for Delegated Authentication for
        /// the given comma-delimited list of offers.
        /// </summary>
        /// <param name="offers">Comma-delimited list of offers.</param>
        /// <param name="context">If you specify it, <paramref
        /// name="context"/> will be returned as-is in the sign-in
        /// response for site-specific use.</param>
        /// <param name="ru">The registered/configured return URL will be 
        /// overridden by 'ru' specified here.</param>
        /// <param name="market">The language in which the consent page is 
        /// displayed is configured by culture ID (For example, 'fr-fr' or 
        /// 'en-us') specified in the 'market' parameter.</param>
        /// <returns>Consent URL</returns>
        public string GetConsentUrl(string offers, string context, string ru, string market)
        {
            if (string.IsNullOrEmpty(offers))
            {
                throw new ArgumentException("Error: GetConsentUrl: Invalid offers list.");
            }

            offers = "?ps=" + HttpUtility.UrlEncode(offers);

            context = string.IsNullOrEmpty(context) ? 
              string.Empty : "&appctx=" + HttpUtility.UrlEncode(context);

            if (string.IsNullOrEmpty(ru))
            {
                ru = ReturnUrl;
            }
            
            ru = string.IsNullOrEmpty(ru) ? 
              string.Empty : "&ru=" + HttpUtility.UrlEncode(ru);

            market = string.IsNullOrEmpty(market) ? 
              string.Empty : "&mkt=" + HttpUtility.UrlEncode(market);

            string pu = string.Empty;

            if (!string.IsNullOrEmpty(PolicyUrl))
            {
                pu = "&pl=" + HttpUtility.UrlEncode(PolicyUrl);
            }

            string app = string.Empty;
            
            if (!ForceDelAuthNonProvisioned)
            {
                app = "&app=" + GetAppVerifier();
            }

            return (ConsentUrl + "Delegation.aspx" + offers + context + ru + pu + market + app);
        }

        /// <summary>
        /// Returns the URL to use to download a new consent token, given the 
        /// offers and refresh token.
        /// </summary>
        /// <param name="offers">Comma-delimited list of offers.</param>
        /// <param name="refreshToken">Refresh token.</param>
        /// <returns>Refresh consent token URL</returns>
        public string GetRefreshConsentTokenUrl(string offers, string refreshToken)
        {
            return GetRefreshConsentTokenUrl(offers, refreshToken, null);
        }

        /// <summary>
        /// Returns the URL to use to download a new consent token, given the 
        /// offers and refresh token.
        /// </summary>
        /// <param name="offers">Comma-delimited list of offers.</param>
        /// <param name="refreshToken">Refresh token.</param>
        /// <returns>Refresh consent token URL</returns>
        /// <param name="ru">The registered/configured return URL will be 
        /// overridden by 'ru' specified here.</param>
        /// <returns>Refresh consent token URL</returns>
        public string GetRefreshConsentTokenUrl(string offers, string refreshToken, string ru)
        {
            if (string.IsNullOrEmpty(offers))
            {
                throw new ArgumentException("Error: GetRefreshConsentTokenUrl: Invalid offers list.");
            }

            offers = "?ps=" + HttpUtility.UrlEncode(offers);

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("Error: GetRefreshConsentTokenUrl: Invalid refresh token.");
            }

            refreshToken = "&reft=" + refreshToken;

            if (string.IsNullOrEmpty(ru))
            {
                ru = ReturnUrl;
            }

            ru = string.IsNullOrEmpty(ru) ? 
              string.Empty : "&ru=" + HttpUtility.UrlEncode(ru);

            string app = string.Empty;
            
            if (!ForceDelAuthNonProvisioned)
            {
                app = "&app=" + GetAppVerifier();
            }

            return ConsentUrl + "RefreshToken.aspx" + offers + refreshToken + ru + app;
        }
        
        /// <summary>
        /// Returns the URL for the consent-management user interface.
        /// </summary>
        /// <returns>Manage consent URL</returns>
        public string GetManageConsentUrl()
        {
            return GetManageConsentUrl(null);
        }

        /// <summary>
        /// Returns the URL for the consent-management user interface.
        /// </summary>
        /// <param name="market">The language in which the consent page is 
        /// displayed is configured by culture ID (For example, 'fr-fr' or 
        /// 'en-us') specified in the 'market' parameter.</param>
        /// <returns>Manage consent URL</returns>
        public string GetManageConsentUrl(string market)
        {
            market = string.IsNullOrEmpty(market) ? 
              string.Empty : "?mkt=" + HttpUtility.UrlEncode(market);

            return ConsentUrl + "ManageConsent.aspx" + market;
        }

        /// <summary>
        /// Holds the Consent Token object corresponding to consent granted. 
        /// </summary>
        public class ConsentToken
        {
            WindowsLiveLogin wll;

            /// <summary>
            /// Initialize the ConsentToken.
            /// </summary>
            /// <param name="wll">WindowsLiveLogin</param>
            /// <param name="delegationToken">Delegation token</param>
            /// <param name="refreshToken">Refresh token</param>
            /// <param name="sessionKey">Session key</param>
            /// <param name="expiry">Expiry</param>
            /// <param name="offers">Offers</param>
            /// <param name="locationID">Location ID</param>
            /// <param name="context">Application context</param>
            /// <param name="decodedToken">Decoded token</param>
            /// <param name="token">Raw token</param>
            public ConsentToken(WindowsLiveLogin wll, string delegationToken, string refreshToken, string sessionKey, string expiry, string offers, string locationID, string context, string decodedToken, string token)
            {
                this.wll = wll;
                setDelegationToken(delegationToken);
                setRefreshToken(refreshToken);
                setSessionKey(sessionKey);
                setExpiry(expiry);
                setOffers(offers);
                setLocationID(locationID);
                setContext(context);
                setDecodedToken(decodedToken);
                setToken(token);
            }

            string delegationToken;
            
            /// <summary>
            /// Gets the Delegation token.
            /// </summary>
            public string DelegationToken { get { return delegationToken; } }

            /// <summary>
            /// Sets the Delegation token.
            /// </summary>
            /// <param name="delegationToken">Delegation token</param>
            private void setDelegationToken(string delegationToken)
            {
                if (string.IsNullOrEmpty(delegationToken))
                {
                    throw new ArgumentException("Error: ConsentToken: Null delegation token.");
                }

                this.delegationToken = delegationToken;
            }

            string refreshToken;
            
            /// <summary>
            /// Gets the refresh token.
            /// </summary>
            public string RefreshToken { get { return refreshToken; } }

            /// <summary>
            /// Sets the refresh token.
            /// </summary>
            /// <param name="refreshToken">Refresh token</param>
            private void setRefreshToken(string refreshToken)
            {
                this.refreshToken = refreshToken;
            }

            byte[] sessionKey;
            
            /// <summary>
            /// Gets the session key.
            /// </summary>
            public byte[] SessionKey { get { return sessionKey; } }

            /// <summary>
            /// Sets the session key.
            /// </summary>
            /// <param name="sessionKey">Session key</param>
            private void setSessionKey(string sessionKey)
            {
                if (string.IsNullOrEmpty(sessionKey))
                {
                    throw new ArgumentException("Error: ConsentToken: Null session key.");
                }

                this.sessionKey = WindowsLiveLogin.u64(sessionKey);
            }

            DateTime expiry;

            /// <summary>
            /// Gets the expiry time of delegation token.
            /// </summary>
            public DateTime Expiry { get { return expiry; } }

            /// <summary>
            /// Sets the expiry time of delegation token.
            /// </summary>
            /// <param name="expiry">Expiry time</param>
            private void setExpiry(string expiry)
            {
                if (string.IsNullOrEmpty(expiry))
                {
                    throw new ArgumentException("Error: ConsentToken: Null expiry time.");
                }

                int expiryInt;

                try 
                {
                    expiryInt = Convert.ToInt32(expiry);
                } 
                catch (Exception) 
                {
                    throw new ArgumentException("Error: Consent: Invalid expiry time: " 
                                                + expiry);
                }

                DateTime refTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                this.expiry = refTime.AddSeconds(expiryInt);
            }
            
            IList offers;

            /// <summary>
            /// Gets the list of offers/actions for which the user granted consent.
            /// </summary>
            public IList Offers { get { return offers; } }

            string offersString;

            /// <summary>
            /// Gets the string representation of all the offers/actions for which 
            /// the user granted consent.
            /// </summary>
            public String OffersString { get { return offersString; } }

            /// <summary>
            /// Sets the offers/actions for which user granted consent.
            /// </summary>
            /// <param name="offers">Comma-delimited list of offers</param>
            private void setOffers(string offers)
            {
                if (string.IsNullOrEmpty(offers))
                {
                    throw new ArgumentException("Error: ConsentToken: Null offers.");
                }

                offers = HttpUtility.UrlDecode(offers);

                this.offersString = string.Empty;
                this.offers = new ArrayList();

                string[] offersList = offers.Split(new Char[]{';'});

                foreach (string offer in offersList)
                {
                    if (!(this.offersString == string.Empty))
                    {
                        this.offersString += ",";
                    }
                    
                    int separator = offer.IndexOf(':');
                    if (separator == -1)
                    {
                        debug("Warning: ConsentToken: offer may be invalid: " + offer);
                        this.offers.Add(offer);
                        this.offersString += offer;
                    }
                    else
                    {
                        string o = offer.Substring(0, separator);
                        this.offers.Add(o);
                        this.offersString += o;
                    }
                }
            }

            string locationID;

            /// <summary>
            /// Gets the location ID.
            /// </summary>
            public string LocationID { get { return locationID; } }

            /// <summary>
            /// Sets the location ID.
            /// </summary>
            /// <param name="locationID">Location ID</param>
            private void setLocationID(string locationID)
            {
                this.locationID = locationID;
            }

            string context;

            /// <summary>
            /// Returns the application context that was originally passed 
            /// to the consent request, if any.
            /// </summary>
            public string Context { get { return context; } }

            /// <summary>
            /// Sets the application context.
            /// </summary>
            /// <param name="context">Application context</param>
            private void setContext(string context)
            {
                this.context = context;
            }

            string decodedToken;

            /// <summary>
            /// Gets the decoded token.
            /// </summary>
            public string DecodedToken { get { return decodedToken; } }

            /// <summary>
            /// Sets the decoded token.
            /// </summary>
            /// <param name="decodedToken">Decoded token</param>
            private void setDecodedToken(string decodedToken)
            {
                this.decodedToken = decodedToken;
            }

            string token;

            /// <summary>
            /// Gets the raw token.
            /// </summary>
            public string Token { get { return token; } }

            /// <summary>
            /// Sets the raw token.
            /// </summary>
            /// <param name="token">Raw token</param>
            private void setToken(string token)
            {
                this.token = token;
            }

            /// <summary>
            /// Indicates whether the delegation token is set and has not expired.
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                if (string.IsNullOrEmpty(DelegationToken))
                {
                    return false;
                }

                if (DateTime.UtcNow.AddSeconds(-300) > Expiry)
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Attempt to refresh the current token and replace it. If operation succeeds 
            /// true is returned to signify success.
            /// </summary>
            /// <returns></returns>
            public bool Refresh()
            {
                ConsentToken ct = wll.RefreshConsentToken(this);

                if (ct == null) 
                {
                    return false;
                }
                
                copy(ct);

                return true;
            }

            /// <summary>
            /// Makes a copy of the ConsentToken object.
            /// </summary>
            /// <param name="consentToken"></param>
            void copy(ConsentToken consentToken)
            {
                this.delegationToken = consentToken.delegationToken;
                this.refreshToken = consentToken.refreshToken;
                this.sessionKey = consentToken.sessionKey;
                this.expiry = consentToken.expiry;
                this.offers = consentToken.offers;
                this.locationID = consentToken.locationID;
                this.offersString = consentToken.offersString;
                this.decodedToken = consentToken.decodedToken;
                this.token = consentToken.token;
            }
        }

        /// <summary>
        /// Processes the POST response from the Delegated Authentication 
        /// service after a user has granted consent. The processConsent
        /// function extracts the consent token string and returns the result 
        /// of invoking the processConsentToken method. 
        /// </summary>
        /// <param name="query">Response from the Delegated Authentication service.</param>
        /// <returns>ConsentToken</returns>
        public ConsentToken ProcessConsent(NameValueCollection query)
        {
            if (query == null)
            {
                debug("Error: ProcessConsent: Invalid query.");
                return null;
            }

            string action = query["action"];

            if (action != "delauth")
            {
                debug("Warning: ProcessConsent: query action ignored: " + action);
                return null;
            }

            if (query["ResponseCode"] != "RequestApproved")
            {
                debug("Error: ProcessConsent: Consent was not successfully granted: " 
                      + query["ResponseCode"]);
                return null;
            }

            string token = query["ConsentToken"];
            string context = query["appctx"];

            if (!string.IsNullOrEmpty(context))
            {
                context = HttpUtility.UrlDecode(context);
            }

            return ProcessConsentToken(token, context);
        }

        /// <summary>
        /// Processes the consent token string that is returned in the POST 
        /// response by the Delegated Authentication service after a 
        /// user has granted consent.
        /// </summary>
        /// <param name="token">Raw token.</param>
        /// <returns>ConsentToken</returns>
        public ConsentToken ProcessConsentToken(string token)
        {
            return ProcessConsentToken(token, null);
        }

        /// <summary>
        /// Processes the consent token string that is returned in the POST 
        /// response by the Delegated Authentication service after a 
        /// user has granted consent.
        /// </summary>
        /// <param name="token">Raw token.</param>
        /// <param name="context">If you specify it, <paramref
        /// name="context"/> will be returned as-is in the sign-in
        /// response for site-specific use.</param>
        /// <returns></returns>
        public ConsentToken ProcessConsentToken(string token, string context)
        {
            string decodedToken = token;

            if (string.IsNullOrEmpty(token))
            {
                debug("Error: ProcessConsentToken: Null token.");
                return null;
            }
            
            NameValueCollection parsedToken = 
              parse(HttpUtility.UrlDecode(token));
            
            if (!string.IsNullOrEmpty(parsedToken["eact"]))
            {
                decodedToken = DecodeAndValidateToken(parsedToken["eact"]);
                if (string.IsNullOrEmpty(decodedToken))
                {
                    debug("Error: ProcessConsentToken: Failed to decode/validate token: " +
                          token);
                    return null;
                }

                parsedToken = parse(decodedToken);
                decodedToken = HttpUtility.UrlEncode(decodedToken);
            }
            
            ConsentToken consentToken = null;
            try
            {
                consentToken = new ConsentToken(this, 
                                                parsedToken["delt"], 
                                                parsedToken["reft"],
                                                parsedToken["skey"],
                                                parsedToken["exp"],
                                                parsedToken["offer"],
                                                parsedToken["lid"],
                                                context, decodedToken,
                                                token);
            }
            catch (Exception e)
            {
                debug("Error: ProcessConsentToken: Contents of token considered invalid: " + e);
            }

            return consentToken;
        }

        /// <summary>
        /// Attempts to obtain a new, refreshed token and return it. The 
        /// original token is not modified.
        /// </summary>
        /// <param name="token">ConsentToken object.</param>
        /// <returns>Refreshed ConsentToken object.</returns>
        public ConsentToken RefreshConsentToken(ConsentToken token)
        {
            return RefreshConsentToken(token, null);
        }

        /// <summary>
        /// Attempts to obtain a new, refreshed token and return it. The 
        /// original token is not modified.
        /// </summary>
        /// <param name="token">ConsentToken object.</param>
        /// <param name="ru">The registered/configured return URL will be 
        /// overridden by 'ru' specified here.</param>
        /// <returns>Refreshed ConsentToken object.</returns>
        public ConsentToken RefreshConsentToken(ConsentToken token, string ru)
        {
            if (token == null)
            {
                debug("Error: RefreshConsentToken: Null consent token.");
                return null;
            }

            return RefreshConsentToken(token.OffersString, token.RefreshToken, ru);
        }
        
        /// <summary>
        /// Attempts to obtain a new, refreshed token and return it using 
        /// the offers and refresh token. The original token is not modified.
        /// </summary>
        /// <param name="offers">Comma-delimited list of offers.</param>
        /// <param name="refreshToken">Refresh token.</param>
        /// <returns>Refreshed ConsentToken object.</returns>
        public ConsentToken RefreshConsentToken(string offers, string refreshToken)
        {
            return RefreshConsentToken(offers, refreshToken, null);
        }

        /// <summary>
        /// Attempts to obtain a new, refreshed token and return it using 
        /// the offers and refresh token. The original token is not modified.
        /// </summary>
        /// <param name="offers">Comma-delimited list of offers.</param>
        /// <param name="refreshToken">Refresh token.</param>
        /// <param name="ru">The registered/configured return URL will be 
        /// overridden by 'ru' specified here.</param>
        /// <returns>Refreshed ConsentToken object.</returns>
        public ConsentToken RefreshConsentToken(string offers, string refreshToken, string ru)
        {
            string url = null;
            
            try
            {
                url = GetRefreshConsentTokenUrl(offers, refreshToken, ru);
            }
            catch (Exception e)
            {
                debug("Error: Failed to construct refresh consent token URL: " + e);
                return null;
            }

            if (string.IsNullOrEmpty(url))
            {
                debug("Error: Failed to construct refresh consent token URL.");
                return null;
            }
            
            string body = fetch(url);

            if (string.IsNullOrEmpty(body))
            {
                debug("Error: RefreshConsentToken: Failed to download token.");
                return null;
            }   
            
            Regex re = new Regex("{\"ConsentToken\":\"(.*)\"}");
            GroupCollection gc = re.Match(body).Groups;

            if (gc.Count != 2)
            {
                debug("Error: RefreshConsentToken: Failed to extract token: " + body);
                return null;
            }

            CaptureCollection cc = gc[1].Captures;

            if (cc.Count != 1)
            {
                debug("Error: RefreshConsentToken: Failed to extract token: " + body);
                return null;
            }
            
            return ProcessConsentToken(cc[0].ToString());
        }

        /* Common methods. */

        /// <summary>
        /// Decodes and validates the raw token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string DecodeAndValidateToken(string token)
        {
            bool haveOldSecret = false;

            if (DateTime.UtcNow < oldSecretExpiry)
            {
                if ((oldCryptKey != null) && (oldSignKey != null))
                {
                    haveOldSecret = true;
                }
            }

            string stoken = DecodeAndValidateToken(token, cryptKey, signKey);

            if (string.IsNullOrEmpty(stoken))
            {
                if (haveOldSecret)
                {
                    debug("Warning: Failed to validate token with current secret, attempting old secret.");
                    return DecodeAndValidateToken(token, oldCryptKey, oldSignKey);
                }
            }

            return stoken;
        }

        /// <summary>
        /// Decodes and validates the raw token with appropriate crypt key 
        /// and sign key.
        /// </summary>
        /// <param name="token">Raw token.</param>
        /// <param name="cryptKey">Crypt key.</param>
        /// <param name="signKey">Sign key.</param>
        /// <returns></returns>
        public string DecodeAndValidateToken(string token, byte[] cryptKey, byte[] signKey)
        {
            string stoken = DecodeToken(token, cryptKey);

            if (!string.IsNullOrEmpty(stoken))
            {
                stoken = ValidateToken(stoken, signKey);
            }

            return stoken;
        }

        /// <summary>
        /// Decode the given token. Returns null on failure.
        /// </summary>
        ///
        /// <list type="number">
        /// <item>First, the string is URL unescaped and base64
        /// decoded.</item>
        /// <item>Second, the IV is extracted from the first 16 bytes
        /// of the string.</item>
        /// <item>Finally, the string is decrypted by using the
        /// encryption key.</item> 
        /// </list>
        /// <param name="token">Raw token.</param>
        /// <returns>Decoded token.</returns>
        public string DecodeToken(string token)
        {
            return DecodeToken(token, cryptKey);
        }

        /// <summary>
        /// Decode the given token. Returns null on failure.
        /// </summary>
        ///
        /// <list type="number">
        /// <item>First, the string is URL unescaped and base64
        /// decoded.</item>
        /// <item>Second, the IV is extracted from the first 16 bytes
        /// of the string.</item>
        /// <item>Finally, the string is decrypted by using the
        /// encryption key.</item> 
        /// </list>
        /// <param name="token">Raw token.</param>
        /// <param name="cryptKey">Crypt key.</param>
        /// <returns>Decoded token.</returns>
        public string DecodeToken(string token, byte[] cryptKey)
        {
            if (cryptKey == null || cryptKey.Length == 0)
            {
                throw new InvalidOperationException("Error: DecodeToken: Secret key was not set. Aborting.");
            }

            if (string.IsNullOrEmpty(token))
            {
                debug("Error: DecodeToken: Null token input.");
                return null;
            }

            const int ivLength = 16;
            byte[] ivAndEncryptedValue = u64(token);

            if ((ivAndEncryptedValue == null) || 
                (ivAndEncryptedValue.Length <= ivLength) || 
                ((ivAndEncryptedValue.Length % ivLength) != 0))
            {
                debug("Error: DecodeToken: Attempted to decode invalid token.");
                return null;
            }

            Rijndael aesAlg = null;
            MemoryStream memStream = null;
            CryptoStream cStream = null;
            StreamReader sReader = null;
            string decodedValue = null;

            try 
            {
                aesAlg = new RijndaelManaged();
                aesAlg.KeySize = 128;
                aesAlg.Key = cryptKey;
                aesAlg.Padding = PaddingMode.PKCS7;
                memStream = new MemoryStream(ivAndEncryptedValue);
                byte[] iv = new byte[ivLength];
                memStream.Read(iv, 0, ivLength);
                aesAlg.IV = iv;
                cStream = new CryptoStream(memStream, aesAlg.CreateDecryptor(), CryptoStreamMode.Read);
                sReader = new StreamReader(cStream, Encoding.ASCII);
                decodedValue = sReader.ReadToEnd();
            } 
            catch (Exception e) 
            {
                debug("Error: DecodeToken: Decryption failed: " + e);
                return null;
            } 
            finally 
            {
                try 
                {
                    if (sReader != null) { sReader.Close(); }
                    if (cStream != null) { cStream.Close(); }
                    if (memStream != null) { memStream.Close(); }
                    if (aesAlg != null) { aesAlg.Clear(); }
                } 
                catch (Exception e) 
                {
                    debug("Error: DecodeToken: Failure during resource cleanup: " + e);
                }
            }
            
            return decodedValue;
        }

        /// <summary>
        /// Creates a signature for the given string.
        /// </summary>
        public byte[] SignToken(string token)
        {
            return SignToken(token, signKey);
        }

        /// <summary>
        /// Creates a signature for the given string by using the
        /// signature key.
        /// </summary>
        public byte[] SignToken(string token, byte[] signKey)
        {
            if (signKey == null || signKey.Length == 0)
            {
                throw new InvalidOperationException("Error: SignToken: Secret key was not set. Aborting.");
            }

            if (string.IsNullOrEmpty(token))
            {
                debug("Attempted to sign null token.");
                return null;
            }
            
            using (HashAlgorithm hashAlg = new HMACSHA256(signKey)) 
            {
                byte[] data = Encoding.Default.GetBytes(token);
                byte[] hash = hashAlg.ComputeHash(data);
                return hash;
            }
        }

        /// <summary>
        /// Extracts the signature from the token and validates it.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string ValidateToken(string token)
        {
            return ValidateToken(token, signKey);
        }

        /// <summary>
        /// Extracts the signature from the token and validates it by using the 
        /// signature key.
        /// </summary>
        public string ValidateToken(string token, byte[] signKey)
        {
            if (string.IsNullOrEmpty(token))
            {
                debug("Error: ValidateToken: Null token.");
                return null;
            }

            string[] s = { "&sig=" };
            string[] bodyAndSig = token.Split(s, StringSplitOptions.None);

            if (bodyAndSig.Length != 2)
            {
                debug("Error: ValidateToken: Invalid token: " + token);
                return null;
            }
            
            byte[] sig = u64(bodyAndSig[1]);

            if (sig == null)
            {
                debug("Error: ValidateToken: Could not extract the signature from the token.");
                return null;
            }

            byte[] sig2 = SignToken(bodyAndSig[0], signKey);

            if (sig2 == null)
            {
                debug("Error: ValidateToken: Could not generate a signature for the token.");
                return null;
            }
                
            if (sig.Length == sig2.Length) 
            {
                for (int i = 0; i < sig.Length; i++) 
                {
                    if (sig[i] != sig2[i]) { goto badSig; }
                }

                return token;
            }

        badSig:
            debug("Error: ValidateToken: Signature did not match.");
            return null;
        }

        /* Implementation of the methods needed to perform Windows Live
           application verification as well as trusted sign-in. */

        /// <summary>
        /// Generates an Application Verifier token.
        /// </summary>
        public string GetAppVerifier()
        {
            return GetAppVerifier(null);
        }

        /// <summary>
        /// Generates an Application Verifier token. An IP address
        /// can be included in the token.
        /// </summary>
        public string GetAppVerifier(string ip)
        {
            ip = string.IsNullOrEmpty(ip) ? string.Empty : ("&ip=" + ip);
            string token = "appid=" + AppId + "&ts=" + getTimestamp() + ip;
            string sig = e64(SignToken(token));

            if (string.IsNullOrEmpty(sig))
            {
                debug("Error: GetAppVerifier: Failed to sign the token.");
                return null;
            }

            token += "&sig=" + sig;
            return HttpUtility.UrlEncode(token);
        }        

        /// <summary>
        /// Returns the URL needed to retrieve the application
        /// security token. The application security token
        /// will be generated for the Windows Live site.
        ///
        /// JavaScript Output Notation (JSON) output is returned:
        ///
        /// {"token":"&lt;value&gt;"}
        /// </summary>
        public string GetAppLoginUrl()
        {
            return GetAppLoginUrl(null, null, false);
        }

        /// <summary>
        /// Returns the URL needed to retrieve the application
        /// security token.
        ///
        /// By default, the application security token will be
        /// generated for the Windows Live site; a specific Site ID
        /// can optionally be specified in 'siteId'.
        ///
        /// JSON output is returned:
        ///
        /// {"token":"&lt;value&gt;"}
        /// </summary>
        public string GetAppLoginUrl(string siteId)
        {
            return GetAppLoginUrl(siteId, null, false);
        }

        /// <summary>
        /// Returns the URL needed to retrieve the application
        /// security token.
        ///
        /// By default, the application security token will be
        /// generated for the Windows Live site; a specific Site ID
        /// can optionally be specified in 'siteId'. The IP address
        /// can also optionally be included in 'ip'.
        ///
        /// JSON output is returned:
        ///
        /// {"token":"&lt;value&gt;"}
        /// </summary>
        public string GetAppLoginUrl(string siteId, string ip)
        {
            return GetAppLoginUrl(siteId, ip, false);
        }

        /// <summary>
        /// Returns the URL needed to retrieve the application
        /// security token.
        ///
        /// By default, the application security token will be
        /// generated for the Windows Live site; a specific Site ID
        /// can optionally be specified in 'siteId'. The IP address
        /// can also optionally be included in 'ip'.
        ///
        /// If 'js' is false, then JSON output is returned: 
        ///
        /// {"token":"&lt;value&gt;"}
        ///
        /// Otherwise, a JavaScript response is returned. It is assumed
        /// that WLIDResultCallback is a custom function implemented to
        /// handle the token value:
        /// 
        /// WLIDResultCallback("&lt;tokenvalue&gt;");
        /// </summary>
        public string GetAppLoginUrl(string siteId, string ip, bool js)
        {
            string algPart = "&alg=" + SecurityAlgorithm;
            string sitePart = string.IsNullOrEmpty(siteId) ? 
              string.Empty : "&id=" + siteId;
            string jsPart = (!js) ? string.Empty : "&js=1";
            string url = SecureUrl + "wapplogin.srf?app=" + 
              GetAppVerifier(ip) + algPart + sitePart + jsPart;
            return url;            
        }

        /// <summary>
        /// Retrieves the application security token for application
        /// verification from the application sign-in URL. The
        /// application security token will be generated for the
        /// Windows Live site.
        /// </summary>
        public string GetAppSecurityToken()
        {
            return GetAppSecurityToken(null, null);
        }

        /// <summary>
        /// Retrieves the application security token for application
        /// verification from the application sign-in URL.
        ///
        /// By default, the application security token will be
        /// generated for the Windows Live site; a specific Site ID
        /// can optionally be specified in 'siteId'.
        /// </summary>
        public string GetAppSecurityToken(string siteId)
        {
            return GetAppSecurityToken(siteId, null);
        }

        /// <summary>
        /// Retrieves the application security token for application
        /// verification from the application sign-in URL.
        ///
        /// By default, the application security token will be
        /// generated for the Windows Live site; a specific Site ID
        /// can optionally be specified in 'siteId'. The IP address
        /// can also optionally be included in 'ip'.
        ///
        /// Implementation note: The application security token is
        /// downloaded from the application sign-in URL in JSON format
        /// {"token":"&lt;value&gt;"}, so we need to extract
        /// &lt;value&gt; from the string and return it as seen here.
        /// </summary>
        public string GetAppSecurityToken(string siteId, string ip)
        {
            string url = GetAppLoginUrl(siteId, ip);
            string body = fetch(url);
            if (string.IsNullOrEmpty(body))
            {
                debug("Error: GetAppSecurityToken: Failed to download token.");
                return null;
            }   

            Regex re = new Regex("{\"token\":\"(.*)\"}");
            GroupCollection gc = re.Match(body).Groups;

            if (gc.Count != 2)
            {
                debug("Error: GetAppSecurityToken: Failed to extract token: " + body);
                return null;
            }

            CaptureCollection cc = gc[1].Captures;

            if (cc.Count != 1)
            {
                debug("Error: GetAppSecurityToken: Failed to extract token: " + body);
                return null;
            }

            return cc[0].ToString();
        }

        /// <summary>
        /// Returns a string that can be passed to the GetTrustedParams
        /// function as the 'retcode' parameter. If this is specified as
        /// the 'retcode', then the app will be used as return URL
        /// after it finishes trusted sign-in.  
        /// </summary>
        public string GetAppRetCode()
        {
            return "appid=" + AppId;
        }

        /// <summary>
        /// Returns a table of key-value pairs that must be posted to
        /// the sign-in URL for trusted sign-in. Use HTTP POST to do
        /// this. Be aware that the values in the table are neither
        /// URL nor HTML escaped and may have to be escaped if you are
        /// inserting them in code such as an HTML form.
        /// 
        /// The user to be trusted on the local site is passed in as
        /// string 'user'.
        /// </summary>
        public NameValueCollection GetTrustedParams(string user)
        {
            return GetTrustedParams(user, null);
        }

        /// <summary>
        /// Returns a table of key-value pairs that must be posted to
        /// the sign-in URL for trusted sign-in. Use HTTP POST to do
        /// this. Be aware that the values in the table are neither
        /// URL nor HTML escaped and may have to be escaped if you are
        /// inserting them in code such as an HTML form.
        /// 
        /// The user to be trusted on the local site is passed in as
        /// string 'user'.
        /// 
        /// Optionally, 'retcode' specifies the resource to which
        /// successful sign-in is redirected, such as Windows Live Mail,
        /// and is typically a string in the format 'id=2000'. If you
        /// pass in the value from GetAppRetCode instead, sign-in will
        /// be redirected to the application. Otherwise, an HTTP 200
        /// response is returned.
        /// </summary>
        public NameValueCollection GetTrustedParams(string user, string retcode)
        {
            string token = GetTrustedToken(user);

            if (string.IsNullOrEmpty(token)) { return null; }

            token = "<wst:RequestSecurityTokenResponse xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\"><wst:RequestedSecurityToken><wsse:BinarySecurityToken xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">" + token + "</wsse:BinarySecurityToken></wst:RequestedSecurityToken><wsp:AppliesTo xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\"><wsa:EndpointReference xmlns:wsa=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\"><wsa:Address>uri:WindowsLiveID</wsa:Address></wsa:EndpointReference></wsp:AppliesTo></wst:RequestSecurityTokenResponse>";

            NameValueCollection nvc = new NameValueCollection(3);
            nvc["wa"] = SecurityAlgorithm;
            nvc["wresult"] = token;

            if (retcode != null) 
            {
                nvc["wctx"] = retcode;
            }

            return nvc;
        }

        /// <summary>
        /// Returns the trusted sign-in token in the format needed by the
        /// trusted sign-in gadget.
        ///
        /// User to be trusted on the local site is passed in as string
        /// 'user'.
        /// </summary>
        public string GetTrustedToken(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                debug("Error: GetTrustedToken: Invalid user specified.");
                return null;
            }

            string token = "appid=" + AppId + "&uid=" + 
              HttpUtility.UrlEncode(user) + "&ts=" + getTimestamp();
            string sig = e64(SignToken(token));

            if (string.IsNullOrEmpty(sig))
            {
                debug("Error: GetTrustedToken: Failed to sign the token.");
                return null;
            }

            token += "&sig=" + sig;
            return HttpUtility.UrlEncode(token);
        }

        /// <summary>
        /// Returns the trusted sign-in URL to use for the Windows Live
        /// Login server. 
        /// </summary>
        public string GetTrustedLoginUrl()
        {
            return SecureUrl + "wlogin.srf";
        }

        /// <summary>
        /// Returns the trusted sign-out URL to use for the Windows Live
        /// Login server. 
        /// </summary>
        public string GetTrustedLogoutUrl()
        {
            return SecureUrl + "logout.srf?appid=" + AppId;
        }

        /* Helper methods */

        /// <summary>
        /// Function to parse the settings file.
        /// </summary>
        /// <param name="settingsFile"></param>
        /// <returns></returns>
        static NameValueCollection parseSettings(string settingsFile)
        {
            if (string.IsNullOrEmpty(settingsFile))
            {
                throw new ArgumentNullException("settingsFile");
            }
            
            // Throws an exception on any failure.
            XmlDocument xd = new XmlDocument();
            xd.Load(settingsFile);

            XmlNode topNode = xd.SelectSingleNode("//windowslivelogin");

            if (topNode == null)
            {
                throw new XmlException("Error: parseSettings: Failed to parse settings file: " + settingsFile);
            }   

            NameValueCollection settings = new NameValueCollection();
            IEnumerator children = topNode.GetEnumerator();
            
            while (children.MoveNext())
            {
                XmlNode child = (XmlNode) children.Current;
                settings[child.Name] = child.InnerText;
            }

            return settings;
        }

        /// <summary>
        /// Derives the key, given the secret key and prefix as described in the 
        /// Web Authentication SDK documentation.
        /// </summary>
        static byte[] derive(string secret, string prefix)
        {
            using (HashAlgorithm hashAlg = HashAlgorithm.Create("SHA256"))
            {
                const int keyLength = 16;
                byte[] data = Encoding.Default.GetBytes(prefix+secret);
                byte[] hashOutput = hashAlg.ComputeHash(data);
                byte[] byteKey = new byte[keyLength];
                Array.Copy(hashOutput, byteKey, keyLength);
                return byteKey;
            }
        }

        /// <summary>
        /// Parses query string and return a table representation of 
        /// the key and value pairs.  Similar to 
        /// HttpUtility.ParseQueryString, except that no URL decoding
        /// is done and only the last value is considered in the case
        /// of multiple values with one key.
        /// </summary>
        static NameValueCollection parse(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                debug("Error: parse: Null input.");
                return null;
            }

            NameValueCollection pairs = new NameValueCollection();
            
            string[] kvs = input.Split(new Char[]{'&'});
            foreach (string kv in kvs)
            {
                int separator = kv.IndexOf('=');
                
                if ((separator == -1) || (separator == kv.Length))
                {
                    debug("Warning: parse: Ignoring pair: " + kv);
                    continue;
                }

                pairs[kv.Substring(0, separator)] = kv.Substring(separator+1);
            }

            return pairs;
        }
        
        /// <summary>
        /// Generates a timestamp suitable for the application
        /// verifier token.
        /// </summary>
        static string getTimestamp()
        {
            DateTime refTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan ts = DateTime.UtcNow - refTime;
            return ((uint)ts.TotalSeconds).ToString();
        }
        
        /// <summary>
        /// Base64-encodes and URL-escapes a byte array.
        /// </summary>
        static string e64(byte[] b)
        {
            string s = null;
            if (b == null) { return s; }

            try 
            {
                s = Convert.ToBase64String(b);
                s = HttpUtility.UrlEncode(s);
            } 
            catch(Exception e) 
            {
                debug("Error: e64: Base64 conversion error: " + e);
            }

            return s;
        }

        /// <summary>
        /// URL-unescapes and Base64-decodes a string.
        /// </summary>
        static byte[] u64(string s)
        {
            byte[] b = null;
            if (s == null) { return b; }
            s = HttpUtility.UrlDecode(s);

            try 
            {
                b = Convert.FromBase64String(s);
            } 
            catch (Exception e) 
            {
                debug("Error: u64: Base64 conversion error: " + s + ", " + e);
            }
            return b;
        }

        /// <summary>
        /// Fetches the contents given a URL.
        /// </summary>
        static string fetch(string url)
        {
            string body = null;
            try 
            {
                WebRequest req = HttpWebRequest.Create(url);
                req.Method = "GET";
                WebResponse res = req.GetResponse();
                using (StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    body = sr.ReadToEnd();
                }
            } 
            catch (Exception e) 
            {
                debug("Error: fetch: Failed to get the document: " + url + 
                      ", " + e);
            }
            return body;
        }
    }
}
