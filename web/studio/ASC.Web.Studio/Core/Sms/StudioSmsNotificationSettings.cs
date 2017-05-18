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


using System;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Core.Tenants;
using ASC.Web.Studio.Utility;
using log4net;

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
                try
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
                    return paid + firstSmsCount*int.Parse(ConfigurationManager.AppSettings["web.sms-count"] ?? "2");
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC.Web.Billing").Error(e);
                }
                return 0;
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