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
using System.Web;
using System.Web.UI;
using ASC.Web.Studio.Utility;
using AjaxPro;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.ProductsAndInstruments, Location, SortOrder = 200)]
    [AjaxNamespace("PricingPageSettingsController")]
    public partial class PricingPageSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/PricingPageSettings/PricingPageSettings.ascx";

        protected bool Checked { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!TenantExtra.EnableTarrifSettings)
                return;

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts("~/UserControls/Management/PricingPageSettings/js/pricingpagesettings.js");

            Checked = TariffSettings.HidePricingPage;
        }

        [AjaxMethod]
        public object Save(bool hide)
        {
            try
            {
                TariffSettings.HidePricingPage = hide;

                return new
                {
                    Status = 1,
                    Message = Resources.Resource.SuccessfullySaveSettingsMessage
                };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }
    }
}