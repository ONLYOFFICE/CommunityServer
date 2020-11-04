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


#region Import

using System;
using System.Web;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using System.Text;

#endregion

namespace ASC.Web.CRM.Controls.Invoices
{
    public partial class InvoiceDetailsView : BaseUserControl
    {
        #region Property

        public static String Location { get { return PathProvider.GetFileStaticRelativePath("Invoices/InvoiceDetailsView.ascx"); } }

        public Invoice TargetInvoice { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (TargetInvoice == null || !CRMSecurity.CanAccessTo(TargetInvoice))
                Response.Redirect(PathProvider.StartURL() + "Invoices.aspx");

            RegisterClientScriptHelper.DataInvoicesDetailsView(Page, TargetInvoice);

            RegisterScript();
        }

        #endregion

        #region Methods

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.Append(@"ASC.CRM.InvoiceDetailsView.init();");

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}