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


using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using ASC.Core.Common.Configuration;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;

namespace ASC.FederatedLogin.LoginProviders
{
    public abstract class BaseLoginProvider<T> : Consumer, ILoginProvider where T : Consumer, ILoginProvider, new ()
    {
        public static T Instance
        {
            get
            {
                return ConsumerFactory.Get<T>();
            }
        }

        public abstract string CodeUrl { get; }
        public abstract string AccessTokenUrl { get; }
        public abstract string RedirectUri { get; }
        public abstract string ClientID { get; }
        public abstract string ClientSecret { get; }
        public virtual string Scopes { get { return ""; } }

        public virtual bool IsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(ClientID) &&
                       !string.IsNullOrEmpty(ClientSecret) &&
                       !string.IsNullOrEmpty(RedirectUri);
            }
        }

        protected BaseLoginProvider()
        {
            
        }

        protected BaseLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null): base(name, order, props, additional)
        {
            
        }

        public virtual LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            try
            {
                var token = Auth(context, Scopes);

                return GetLoginProfile(token == null ? null : token.AccessToken);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return LoginProfile.FromError(ex);
            }
        }

        protected virtual OAuth20Token Auth(HttpContext context, string scopes, Dictionary<string, string> additionalArgs = null)
        {
            var error = context.Request["error"];
            if (!string.IsNullOrEmpty(error))
            {
                if (error == "access_denied")
                {
                    error = "Canceled at provider";
                }
                throw new Exception(error);
            }

            var code = context.Request["code"];
            if (string.IsNullOrEmpty(code))
            {
                OAuth20TokenHelper.RequestCode<T>(HttpContext.Current, scopes, additionalArgs);
                return null;
            }

            return OAuth20TokenHelper.GetAccessToken<T>(code);
        }

        public abstract LoginProfile GetLoginProfile(string accessToken);
    }
}
