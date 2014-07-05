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
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Statistics
{
    public static class TenantStatisticsProvider
    {
        public static bool IsNotPaid()
        {
            return TenantExtra.EnableTarrifSettings && TenantExtra.GetCurrentTariff().State >= TariffState.NotPaid;
        }

        public static int GetUsersCount()
        {
            return CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.User).Length;
        }

        public static long GetUsedSize()
        {
            return GetUsedSize(TenantProvider.CurrentTenantID);
        }

        public static long GetUsedSize(int tenant)
        {
            return GetQuotaRows(tenant).Sum(r => r.Counter);
        }

        public static long GetUsedSize(Guid moduleId)
        {
            return GetQuotaRows(TenantProvider.CurrentTenantID).Where(r => new Guid(r.Tag).Equals(moduleId)).Sum(r => r.Counter);
        }

        private static IEnumerable<TenantQuotaRow> GetQuotaRows(int tenant)
        {
            return CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(tenant))
                .Where(r => !string.IsNullOrEmpty(r.Tag) && new Guid(r.Tag) != Guid.Empty);
        }
    }
}