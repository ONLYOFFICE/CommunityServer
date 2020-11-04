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
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using ASC.Core.Billing;

namespace ASC.Web.Studio.UserControls.Management
{
    public partial class TariffHistory : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/TariffSettings/TariffHistory.ascx"; }
        }

        public IEnumerable<PaymentInfo> Payments;

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStyle("~/UserControls/Management/TariffSettings/css/tariffhistory.less");

            PaymentsRepeater.DataSource = Payments;
            PaymentsRepeater.DataBind();
        }
    }
}