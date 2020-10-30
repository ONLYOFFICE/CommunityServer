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


using ASC.CRM.Core;
using ASC.Web.CRM.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ASC.Web.CRM.Controls.Settings
{
    public partial class CurrencySettingsView : BaseUserControl
    {
        #region Property

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Settings/CurrencySettingsView.ascx"); }
        }

        protected List<CurrencyInfo> BasicCurrencyRates { get; set; }
        protected List<CurrencyInfo> OtherCurrencyRates { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            BasicCurrencyRates = CurrencyProvider.GetBasic();
            OtherCurrencyRates = CurrencyProvider.GetOther();

            RegisterScript();
        }

        private void RegisterScript()
        {
            Page.JsonPublisher(DaoFactory.CurrencyRateDao.GetAll(), "currencyRates");
            Page.JsonPublisher(Global.TenantSettings.DefaultCurrency.Abbreviation, "defaultCurrency");

            Page.RegisterInlineScript("ASC.CRM.CurrencySettingsView.init();");
        }

        public bool IsSelectedBidCurrency(String abbreviation)
        {
            return string.Compare(abbreviation, Global.TenantSettings.DefaultCurrency.Abbreviation, StringComparison.OrdinalIgnoreCase) == 0;
        }

        #endregion
    }
}