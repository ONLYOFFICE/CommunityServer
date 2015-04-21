/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.VoipService.Dao;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Voip
{
    [Serializable]
    [DataContract]
    public class VoipPaymentSettings : ISettings
    {
        public Guid ID
        {
            get { return new Guid("{42CAFEB3-0DEE-4360-9F41-93F6E07E99A6}"); }
        }

        public ISettings GetDefault()
        {
            return new VoipPaymentSettings();
        }

        [DataMember(Name = "SentWarning")]
        public bool SentWarning { get; set; }

        [DataMember(Name = "Enabled")]
        public bool Enabled { get; set; }

        public static bool IsSentWarning
        {
            get
            {
                return SettingsManager.Instance.LoadSettings<VoipPaymentSettings>(TenantProvider.CurrentTenantID).SentWarning;
            }
            set
            {
                var settings = SettingsManager.Instance.LoadSettings<VoipPaymentSettings>(TenantProvider.CurrentTenantID);
                settings.SentWarning = value;
                SettingsManager.Instance.SaveSettings(settings, TenantProvider.CurrentTenantID);
            }
        }


        public static bool IsEnabled
        {
            get
            {
                var settings = SettingsManager.Instance.LoadSettings<VoipPaymentSettings>(TenantProvider.CurrentTenantID);
                var isEnabled = settings.Enabled;

                if (!isEnabled && Left > 1.0)
                {
                    settings.Enabled = isEnabled = true;
                    SettingsManager.Instance.SaveSettings(settings, TenantProvider.CurrentTenantID);
                }

                return isEnabled && IsVisibleSettings;
            }
            set
            {
                var settings = SettingsManager.Instance.LoadSettings<VoipPaymentSettings>(TenantProvider.CurrentTenantID);
                settings.Enabled = value;
                SettingsManager.Instance.SaveSettings(settings, TenantProvider.CurrentTenantID);
            }
        }
        public static double Left
        {
            get
            {
                var q = new TenantQuotaRowQuery(TenantProvider.CurrentTenantID) { Path = "/voip", };
                var row = CoreContext.TenantManager.FindTenantQuotaRows(q).FirstOrDefault();
                return Paid - (row != null ? (int) row.Counter : 0)/1000.0;
            }
        }

        public static double Paid
        {
            get
            {
                var products = Quotas.ToDictionary(q => q.AvangateId, q => q);
                var paid = 0;
                foreach (var payment in CoreContext.PaymentManager.GetTariffPayments(TenantProvider.CurrentTenantID))
                {
                    TenantQuota product;
                    if (!products.TryGetValue(payment.ProductId, out product)) continue;

                    paid += product.ActiveUsers;
                }

                return paid/1000.0;
            }
        }

        public static IEnumerable<TenantQuota> Quotas
        {
            get
            {
                return CoreContext.TenantManager.GetTenantQuotas(true)
                    .Where(q => !string.IsNullOrEmpty(q.AvangateId) && q.GetFeature("voip-product"));
            }
        }

        public static bool IsVisibleSettings
        {
            get
            {
                return ConfigurationManager.AppSettings["voip.enabled"] == "true" &&
                    !CoreContext.Configuration.Personal &&
                    TenantExtra.GetTenantQuota().Voip;
            }
        }

        public static void Increment(long val)
        {
            CoreContext.TenantManager.SetTenantQuotaRow(new TenantQuotaRow { Tenant = TenantProvider.CurrentTenantID, Path = "/voip", Counter = val }, true);

            var left = Left;
            if (left > 1.0 && left <= 10.0 && !IsSentWarning)
            {
                StudioNotifyService.Instance.SendToAdminVoipWarning(left);

                IsSentWarning = true;
            }

            if (left <= 1.0)
            {
                StudioNotifyService.Instance.SendToAdminVoipBlocked();
                var provider = VoipDao.GetVoipProvider();
                var numbers = new CachedVoipDao(CoreContext.TenantManager.GetCurrentTenant().TenantId, "crm").GetNumbers();

                foreach (var voipPhone in numbers)
                {
                    provider.DisablePhone(voipPhone);
                }

                IsEnabled = false;
            }
        }
    }
}