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

#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.CRM.Core;
using ASC.Web.CRM.Controls.Common;
using ASC.Core;
using ASC.Web.Studio.Core.Users;
using Newtonsoft.Json.Linq;
using System.Web;
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
            Page.RegisterClientScript(typeof(Masters.ClientScripts.InvoiceItemActionViewData));

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
                                JsonConvert.SerializeObject(CurrencyProvider.Get(TargetInvoiceItem.Currency)):
                                JsonConvert.SerializeObject(Global.TenantSettings.DefaultCurrency)
                        );

            Page.RegisterInlineScript(script);
        }

        #endregion
    }
}