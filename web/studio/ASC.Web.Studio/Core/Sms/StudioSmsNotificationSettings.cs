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

using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;

namespace ASC.Web.Studio.Core.SMS
{
    [Serializable]
    [DataContract]
    public class StudioSmsNotificationSettings : ISettings
    {
        public Guid ID
        {
            get { return new Guid("{2802df61-af0d-40d4-abc5-a8506a5352ff}"); }
        }

        public ISettings GetDefault()
        {
            return new StudioSmsNotificationSettings { EnableSetting = false, };
        }

        [DataMember(Name = "Enable")]
        public bool EnableSetting { get; set; }


        public static bool Enable
        {
            get
            {
                return SettingsManager.Instance.LoadSettings<StudioSmsNotificationSettings>(TenantProvider.CurrentTenantID).EnableSetting && LeftSms > 0;
            }
            set
            {
                var settings = SettingsManager.Instance.LoadSettings<StudioSmsNotificationSettings>(TenantProvider.CurrentTenantID);
                settings.EnableSetting = value;
                SettingsManager.Instance.SaveSettings(settings, TenantProvider.CurrentTenantID);
            }
        }

        public static int LeftSms
        {
            get
            {
                var q = new TenantQuotaRowQuery(TenantProvider.CurrentTenantID) { Path = "/sms", };
                var row = CoreContext.TenantManager.FindTenantQuotaRows(q).FirstOrDefault();
                return PaidSms - (row != null ? (int)row.Counter : 0);
            }
        }

        public static int PaidSms
        {
            get
            {
                var products = CoreContext.TenantManager.GetTenantQuotas(true).Where(q => !string.IsNullOrEmpty(q.AvangateId)).ToDictionary(q => q.AvangateId, q => q);
                var paid = 0;
                var firstSmsCount = 0;
                foreach (var payment in CoreContext.PaymentManager.GetTariffPayments(TenantProvider.CurrentTenantID))
                {
                    TenantQuota product;
                    if (products.TryGetValue(payment.ProductId, out product))
                    {
                        if (product.GetFeature("sms-product"))
                        {
                            paid += product.ActiveUsers;
                        }
                        else if (0m < product.Price)
                        {
                            firstSmsCount = product.ActiveUsers;
                        }
                    }
                }
                if (firstSmsCount == 0m)
                {
                    firstSmsCount = TenantExtra.GetTenantQuota().ActiveUsers;
                }
                return paid + firstSmsCount * int.Parse(ConfigurationManager.AppSettings["web.sms-count"] ?? "2");
            }
        }

        public static bool IsVisibleSettings
        {
            get
            {
                return SetupInfo.IsVisibleSettings<StudioSmsNotificationSettings>() && TenantExtra.GetTenantQuota().Sms;
            }
        }

        public static void IncrementSentSms()
        {
            CoreContext.TenantManager.SetTenantQuotaRow(new TenantQuotaRow { Tenant = TenantProvider.CurrentTenantID, Path = "/sms", Counter = 1 }, true);
        }
    }
}