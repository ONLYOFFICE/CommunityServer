/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.Core.Billing;
using ASC.Web.Studio.UserControls.Statistics;
using AjaxPro;
using ASC.Core;
using ASC.Web.Studio.Utility;
using System;
using System.Web;
using System.Web.UI;
using Resources;

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
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/tariffsettings/js/tariffpartner.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/tariffsettings/css/tariffpartner.less"));

            PartnerPayKeyContainer.Options.IsPopup = true;
            PartnerApplyContainer.Options.IsPopup = true;
            PartnerRequestContainer.Options.IsPopup = true;
            PartnerPayExceptionContainer.Options.IsPopup = true;
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            if (HttpRuntime.Cache.Get(PartnerCache) == null)
            {
                HttpRuntime.Cache.Remove(PartnerCache);
                HttpRuntime.Cache.Insert(PartnerCache, DateTime.UtcNow);
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
                if (!HttpRuntime.Cache.Get(PartnerCache).Equals(DateTime.UtcNow))
                {
                    HttpRuntime.Cache.Remove(PartnerCache);
                    HttpRuntime.Cache.Insert(PartnerCache, DateTime.UtcNow);
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
                    throw new Exception(Resource.PartnerRequestLimitInfo);
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