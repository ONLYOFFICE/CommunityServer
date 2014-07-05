/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.IO;
using System.Web;
using ASC.Core.Tenants;
using log4net.Core;
using log4net.Layout;
using log4net.Util;

namespace ASC.Core.Common.Logging
{
    public class AdminLogLayout : LayoutSkeleton
    {
        public string Property
        {
            get;
            set;
        }

        public override void ActivateOptions()
        {
        }

        public override void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            var value = string.Empty;
            try
            {
                if (Property == "tenant")
                {
                    var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
                    value = (tenant != null ? tenant.TenantId : Tenant.DEFAULT_TENANT).ToString();
                }
                else if (Property == "ip" && HttpContext.Current != null)
                {
                    value = HttpContext.Current.Request.UserHostAddress;
                }
                else if (Property == "user")
                {
                    value = SecurityContext.CurrentAccount.ID.ToString();
                }
                else if (Property == "email")
                {
                    value = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).Email;
                }
            }
            catch (Exception error)
            {
                if (IgnoresException)
                {
                    LogLog.Error("Can not format property " + Property, error);
                }
                else
                {
                    throw;
                }
            }
            writer.Write(value);
        }
    }
}
