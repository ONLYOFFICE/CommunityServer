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
using System.Text;
using System.Web;
using ASC.Web.CRM.Configuration;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Classes;
using ASC.CRM.Core;
using ASC.Web.CRM.Resources;
using Newtonsoft.Json.Linq;
using ASC.CRM.Core.Entities;
using System.Collections.Generic;
using ASC.CRM.Core.Dao;

#endregion


namespace ASC.Web.CRM.Controls.Settings
{
    public partial class InvoiceItemsView : BaseUserControl
    {
        #region Members

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Settings/InvoiceSettings/InvoiceItemsView.ascx"); }
        }

        private const string ExportErrorCookieKey = "export_invoice_items_error";

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            //Page.RegisterClientScript(typeof(Masters.ClientScripts.ListInvoiceViewData));

            if (UrlParameters.Action != "export")
            {
                RegisterScript();
            }
            else // export to csv
            {
                var invoiceItems = GetInvoiceItemsByFilter(DaoFactory);

                if (invoiceItems.Count != 0)
                {
                    if (UrlParameters.View != "editor")
                    {
                        Response.Clear();
                        Response.ContentType = "text/csv; charset=utf-8";
                        Response.ContentEncoding = Encoding.UTF8;
                        Response.Charset = Encoding.UTF8.WebName;

                        var fileName = "products_services.csv";

                        Response.AppendHeader("Content-Disposition", String.Format("attachment; filename={0}", fileName));
                        Response.Write(ExportToCSV.ExportInvoiceItemsToCSV(invoiceItems, false));
                        Response.End();
                    }
                    else
                    {
                        var fileUrl = ExportToCSV.ExportInvoiceItemsToCSV(invoiceItems, true);
                        Response.Redirect(fileUrl);
                    }
                }
                else
                {
                    var cookie = HttpContext.Current.Request.Cookies.Get(ExportErrorCookieKey);
                    if (cookie == null)
                    {
                        cookie = new HttpCookie(ExportErrorCookieKey);
                        cookie.Value = CRMSettingResource.ExportInvoiceItemsEmptyError;
                        HttpContext.Current.Response.Cookies.Add(cookie);
                    }
                    Response.Redirect(PathProvider.StartURL() + "settings.aspx?type=invoice_items");
                }
            }
        }

        #endregion

        #region Methods

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.InvoiceItemsView.init('{0}');",
                ExportErrorCookieKey
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        private class FilterObject
        {
            public string SortBy { get; set; }
            public string SortOrder { get; set; }
            public string FilterValue { get; set; }

            public bool? InventoryStock { get; set; }
           
            public FilterObject()
            {
                InventoryStock = null;
            }
        };

        private FilterObject GetFilterObjectFromCookie()
        {
            var result = new FilterObject();

            var cookieKey = GetCookieKeyForFilterForExport();

            var cookie = Request.Cookies[System.Web.HttpUtility.UrlEncode(cookieKey)];

            if (cookie != null && !String.IsNullOrEmpty(cookie.Value))
            {
                var anchor = cookie.Value;
                try
                {
                    var cookieJson = Encoding.UTF8.GetString(Convert.FromBase64String(HttpUtility.UrlDecode(anchor)));

                    var jsonArray = cookieJson.Split(';');

                    foreach (var filterItem in jsonArray)
                    {
                        var filterObj = JObject.Parse(filterItem);

                        var filterParam = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(filterObj.Value<string>("params"))));

                        switch (filterObj.Value<string>("id"))
                        {
                            case "sorter":
                                result.SortBy = filterParam.Value<string>("id");
                                result.SortOrder = filterParam.Value<string>("sortOrder");
                                break;
                            case "text":
                                result.FilterValue = filterParam.Value<string>("value");
                                break;

                            case "withInventoryStock":
                            case "withoutInventoryStock":
                                result.InventoryStock = filterParam.Value<bool>("value");
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                    result.SortBy = "name";
                    result.SortOrder = "ascending";
                }
            }
            else
            {
                result.SortBy = "name";
                result.SortOrder = "ascending";
            }

            return result;
        }

        protected List<InvoiceItem> GetInvoiceItemsByFilter(DaoFactory daoFactory)
        {
            var filterObj = GetFilterObjectFromCookie();

            InvoiceItemSortedByType sortBy;
            if (!Web.CRM.Classes.EnumExtension.TryParse(filterObj.SortBy, true, out sortBy))
            {
                sortBy = InvoiceItemSortedByType.Name;
            }

            var isAsc = !String.IsNullOrEmpty(filterObj.SortOrder) && filterObj.SortOrder != "descending";

            return daoFactory.InvoiceItemDao.GetInvoiceItems(
                                                           filterObj.FilterValue,
                                                           0,
                                                           filterObj.InventoryStock,
                                                           0, 0,
                                                           new OrderBy(sortBy, isAsc));
        }


        #endregion

    }
}