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

using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Security;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Management;
using log4net;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web;

namespace ASC.Web.Studio.UserControls.FirstTime
{
    [AjaxNamespace("EmailAndPasswordController")]
    public partial class EmailAndPassword : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/FirstTime/EmailAndPassword.ascx"; } }

        protected Tenant _curTenant;

        protected bool IsVisiblePromocode {
            get { return (string.IsNullOrEmpty(CoreContext.TenantManager.GetCurrentTenant().PartnerId) && !CoreContext.Configuration.Standalone); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            InitScript();

            _curTenant = CoreContext.TenantManager.GetCurrentTenant();

            var timeAndLanguage = (TimeAndLanguage)LoadControl(TimeAndLanguage.Location);
            timeAndLanguage.WithoutButton = true;
            _dateandtimeHolder.Controls.Add(timeAndLanguage);
        }

        private void InitScript()
        {
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/firsttime/js/manager.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/firsttime/css/EmailAndPassword.less"));

            var script = new StringBuilder();

            script.AppendFormat(@"ASC.Controls.EmailAndPasswordManager.init('{0}','{1}','{2}','{3}','{4}');",
                Resources.Resource.EmailAndPasswordTypeChangeIt.ReplaceSingleQuote(),
                Resources.Resource.EmailAndPasswordOK.ReplaceSingleQuote(),
                Resources.Resource.EmailAndPasswordWrongPassword.ReplaceSingleQuote(),
                Resources.Resource.EmailAndPasswordEmptyPassword.ReplaceSingleQuote(),
                Resources.Resource.EmailAndPasswordIncorrectEmail.ReplaceSingleQuote()
            );

            Page.RegisterInlineScript(script.ToString());
        }

        [AjaxMethod]
        [SecurityPassthrough]
        public object SaveData(string email, string pwd, string lng, string promocode)
        {
            try
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var settings = SettingsManager.Instance.LoadSettings<WizardSettings>(tenant.TenantId);
                if (settings.Completed)
                {
                    return new { Status = 0, Message = "Wizard passed." };
                }

                if (tenant.OwnerId == Guid.Empty)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(6)); // wait cache interval
                    tenant = CoreContext.TenantManager.GetTenant(tenant.TenantId);
                    if (tenant.OwnerId == Guid.Empty)
                    {
                        LogManager.GetLogger("ASC.Web.FirstTime").Error(tenant.TenantId + ": owner id is empty.");
                    }
                }
                var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                if (CoreContext.Configuration.Standalone)
                {
                    currentUser = CoreContext.UserManager.GetUsers(CoreContext.TenantManager.GetCurrentTenant().OwnerId);
                    var cookie = SecurityContext.AuthenticateMe(currentUser.ID);
                    CookiesManager.SetCookies(CookiesType.AuthKey, cookie);
                }

                if (!currentUser.IsOwner())
                {
                    return new { Status = 0, Message = Resources.Resource.EmailAndPasswordNotOwner };
                }
                if (!UserManagerWrapper.ValidateEmail(email))
                {
                    return new { Status = 0, Message = Resources.Resource.EmailAndPasswordIncorrectEmail };
                }

                UserManagerWrapper.SetUserPassword(currentUser.ID, pwd);

                email = email.Trim();
                if (currentUser.Email != email)
                {
                    currentUser.Email = email;
                    currentUser.ActivationStatus = EmployeeActivationStatus.NotActivated;
                }
                CoreContext.UserManager.SaveUserInfo(currentUser);

                if (!string.IsNullOrWhiteSpace(promocode))
                {
                    try
                    {
                        CoreContext.PaymentManager.ActivateKey(promocode);
                    }
                    catch (Exception err)
                    {
                        LogManager.GetLogger("ASC.Web.FirstTime").ErrorFormat("Incorrect Promo: {0}\r\n{1}", promocode, err);
                        return new { Status = 0, Message = Resources.Resource.EmailAndPasswordIncorrectPromocode };
                    }
                }

                settings.Completed = true;
                SettingsManager.Instance.SaveSettings(settings, tenant.TenantId);

                TrySetLanguage(tenant, lng);
                FirstTimeTenantSettings.SetDefaultTenantSettings();
                FirstTimeTenantSettings.SendInstallInfo(currentUser);

                return new { Status = 1, Message = Resources.Resource.EmailAndPasswordSaved };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Message = ex.Message };
            }
        }

        public string GetEmail()
        {
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            return currentUser.Email;
        }

        private void TrySetLanguage(Tenant tenant, string lng)
        {
            if (!string.IsNullOrEmpty(lng))
            {
                try
                {
                    var culture = CultureInfo.GetCultureInfo(lng);
                    tenant.Language = culture.Name;
                }
                catch (Exception err)
                {
                    LogManager.GetLogger("ASC.Web.FirstTime").Error(err);
                }
            }
        }
    }
}