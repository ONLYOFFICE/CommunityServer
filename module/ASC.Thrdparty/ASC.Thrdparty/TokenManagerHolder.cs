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


using ASC.Common.Caching;
using ASC.Thrdparty.Configuration;
using ASC.Thrdparty.TokenManagers;
using System;
using System.Configuration;

namespace ASC.Thrdparty
{
    public class TokenManagerHolder
    {
        public static IAssociatedTokenManager Get(string providerKey)
        {
            String consumerKey;
            String consumerSecret;

            switch (providerKey)
            {
                case  ProviderConstants.Twitter:
                    consumerKey = "twitterKey";
                    consumerSecret = "twitterSecret";
                    break;
                case ProviderConstants.Facebook:
                    consumerKey = "facebookAppID";
                    consumerSecret = "facebookAppSecret";
                    break;
                case ProviderConstants.LinkedIn:
                    consumerKey = "linkedInKey";
                    consumerSecret = "linkedInSecret";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return Get(providerKey, consumerKey, consumerSecret);

        }

        public static IAssociatedTokenManager Get(string providerKey, string consumerKey, string consumerSecret)
        {
            var tokenManager = AscCache.Default.Get<IAssociatedTokenManager>(providerKey);
            if (tokenManager == null)
            {
                if (!string.IsNullOrEmpty(consumerKey))
                {
                    tokenManager = GetTokenManager(KeyStorage.Get(consumerKey), KeyStorage.Get(consumerSecret));
                    AscCache.Default.Insert(providerKey, tokenManager, DateTime.MaxValue);
                }
            }
            return tokenManager;
        }

        private static IAssociatedTokenManager GetTokenManager(string consumerKey, string consumerSecret)
        {
            IAssociatedTokenManager tokenManager = null;
            var section = ConsumerConfigurationSection.GetSection();
            if (section!=null && !string.IsNullOrEmpty(section.ConnectionString))
            {
                tokenManager = new DbTokenManager(KeyStorage.Get(consumerKey), KeyStorage.Get(consumerSecret),
                                                  "auth_tokens",
                                                  ConfigurationManager.ConnectionStrings[section.ConnectionString]);
            }
            else
            {
                //For testing return the inmemorytokenmanager
                tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);    
            }
            
            return tokenManager;
        }
    }
}