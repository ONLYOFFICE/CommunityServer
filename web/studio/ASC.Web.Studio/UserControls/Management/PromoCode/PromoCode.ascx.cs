/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Web.UI;
using System.Web;

using ASC.Core;
using Resources;

using AjaxPro;
using ASC.Common.Logging;


namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("PromoCodeController")]
    public partial class PromoCode : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/PromoCode/PromoCode.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts("~/UserControls/Management/PromoCode/promocode.js");
        }

        [AjaxMethod]
        public object ActivateKey(string promocode)
        {
            if (!string.IsNullOrEmpty(promocode))
            {
                try
                {
                    CoreContext.PaymentManager.ActivateKey(promocode);
                    return new { Status = 1, Message = Resource.SuccessfullySaveSettingsMessage };
                }
                catch (Exception err)
                {
                    LogManager.GetLogger("ASC.Web.FirstTime").Error(err);
                }
            }
            return new { Status = 0, Message = Resource.EmailAndPasswordIncorrectPromocode };
        }
    }
}