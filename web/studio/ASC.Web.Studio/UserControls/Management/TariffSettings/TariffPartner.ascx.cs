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


using AjaxPro;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("TariffPartnerController")]
    public partial class TariffPartner : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/TariffSettings/TariffPartner.ascx"; }
        }

        public Partner CurPartner;
        public bool TariffNotPaid = false;
        public bool TariffProlongable = true;
        public static string PartnerCache = "PartnerCache";


        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/usercontrols/management/tariffsettings/js/tariffpartner.js");
            Page.RegisterStyle("~/usercontrols/management/tariffsettings/css/tariffpartner.less");
            PartnerPayKeyContainer.Options.IsPopup = true;
            PartnerApplyContainer.Options.IsPopup = true;
            PartnerRequestContainer.Options.IsPopup = true;
            PartnerPayExceptionContainer.Options.IsPopup = true;
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            if (AscCache.Memory.Get<object>(PartnerCache) == null)
            {
                AscCache.Memory.Insert(PartnerCache, DateTime.UtcNow, DateTime.MaxValue);
            }
        }

        [AjaxMethod]
        public void ActivateKey(string code)
        {
            CoreContext.PaymentManager.ActivateKey(code);
        }

        [AjaxMethod]
        public void RequestKey(int qoutaId)
        {
            var partnerId = CoreContext.TenantManager.GetCurrentTenant().PartnerId;
            CoreContext.PaymentManager.RequestClientPayment(partnerId, qoutaId, true);
        }

        [AjaxMethod]
        public AjaxResponse RequestPayPal(int qoutaId)
        {
            var res = new AjaxResponse();

            try
            {
                if (!((DateTime)AscCache.Memory.Get<object>(PartnerCache)).Equals(DateTime.UtcNow))
                {
                    AscCache.Memory.Insert(PartnerCache, DateTime.UtcNow, DateTime.MaxValue);
                }
                var partner = CoreContext.PaymentManager.GetApprovedPartner();
                if (partner == null || partner.PaymentMethod != PartnerPaymentMethod.PayPal)
                {
                    throw new MethodAccessException(Resource.PartnerPayPalExc);
                }

                var tenantQuota = TenantExtra.GetTenantQuota(qoutaId);

                var curruntQuota = TenantExtra.GetTenantQuota();
                if (TenantExtra.GetCurrentTariff().State == TariffState.Paid
                    && tenantQuota.ActiveUsers < curruntQuota.ActiveUsers
                    && tenantQuota.Year == curruntQuota.Year)
                {
                    throw new MethodAccessException(Resource.PartnerPayPalDowngrade);
                }

                if (tenantQuota.Price > partner.AvailableCredit)
                {
                    CoreContext.PaymentManager.RequestClientPayment(partner.Id, qoutaId, false);
                    throw new Exception(Resource.PartnerRequestLimitInfo.HtmlEncode());
                }

                var usersCount = TenantStatisticsProvider.GetUsersCount();
                var usedSize = TenantStatisticsProvider.GetUsedSize();

                if (tenantQuota.ActiveUsers < usersCount || tenantQuota.MaxTotalSize < usedSize)
                {
                    res.rs2 = "quotaexceed";
                    return res;
                }

                res.rs1 = CoreContext.PaymentManager.GetButton(partner.Id, qoutaId);
            }
            catch (Exception e)
            {
                res.message = e.Message;
            }
            return res;
        }
    }
}