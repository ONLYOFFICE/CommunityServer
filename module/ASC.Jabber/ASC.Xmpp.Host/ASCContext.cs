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


using ASC.Core;
using ASC.Core.Configuration;
using ASC.Core.Tenants;
using ASC.Xmpp.Server.Configuration;
using System.Configuration;

namespace ASC.Xmpp.Host
{
    static class ASCContext
    {
        public static UserManager UserManager
        {
            get
            {
                return CoreContext.UserManager;
            }
        }

        public static AuthManager Authentication
        {
            get
            {
                return CoreContext.Authentication;
            }
        }

        public static Tenant GetCurrentTenant()
        {
            return CoreContext.TenantManager.GetCurrentTenant(false);
        }

        public static void SetCurrentTenant(string domain)
        {
            SecurityContext.AuthenticateMe(Constants.CoreSystem);
            // for migration from teamlab.com to onlyoffice.com
            if (JabberConfiguration.ReplaceDomain && domain.EndsWith(JabberConfiguration.ReplaceFromDomain))
            {
                int place = domain.LastIndexOf(JabberConfiguration.ReplaceFromDomain);
                if (place >= 0)
                {
                    domain = domain.Remove(place, JabberConfiguration.ReplaceFromDomain.Length).Insert(place, JabberConfiguration.ReplaceToDomain);
                }
            }
            var current = CoreContext.TenantManager.GetCurrentTenant(false);
            if (current == null || string.Compare(current.TenantDomain, domain, true) != 0)
            {
                CoreContext.TenantManager.SetCurrentTenant(domain);
            }
        }
    }
}