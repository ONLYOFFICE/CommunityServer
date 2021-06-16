﻿/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


#region Import

using System;
using System.Web;

using ASC.Web.CRM.Classes;

using Newtonsoft.Json;

#endregion

namespace ASC.Web.CRM.Controls.Settings
{
    public partial class InvoiceItemActionView : BaseUserControl
    {

        #region Properies

        public static string Location { get { return PathProvider.GetFileStaticRelativePath("Settings/InvoiceSettings/InvoiceItemActionView.ascx"); } }

        public ASC.CRM.Core.Entities.InvoiceItem TargetInvoiceItem { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterClientScript(new Masters.ClientScripts.InvoiceItemActionViewData());

            RegisterScript();
        }

        #endregion

        #region Methods


        private void RegisterScript()
        {
            var script = String.Format(@"
                ASC.CRM.InvoiceItemActionView.init('{0}',{1});",
                        Global.EncodeTo64(JsonConvert.SerializeObject(TargetInvoiceItem)),

                        TargetInvoiceItem != null && !String.IsNullOrEmpty(TargetInvoiceItem.Currency) ?
                                JsonConvert.SerializeObject(CurrencyProvider.Get(TargetInvoiceItem.Currency)) :
                                JsonConvert.SerializeObject(Global.TenantSettings.DefaultCurrency)
                        );

            Page.RegisterInlineScript(script);
        }

        #endregion
    }
}