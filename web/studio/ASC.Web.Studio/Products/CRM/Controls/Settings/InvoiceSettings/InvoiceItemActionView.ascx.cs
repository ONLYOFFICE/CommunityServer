/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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