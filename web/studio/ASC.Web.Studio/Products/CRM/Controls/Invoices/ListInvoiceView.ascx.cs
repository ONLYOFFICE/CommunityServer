/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Web.CRM.Controls.Common;

#endregion

namespace ASC.Web.CRM.Controls.Invoices
{
    public partial class ListInvoiceView : BaseUserControl
    {
        #region Properies

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Invoices/ListInvoiceView.ascx"); }
        }


        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            _phListBase.Controls.Add((ListBaseView)LoadControl(ListBaseView.Location));
        }

        #endregion

    }
}