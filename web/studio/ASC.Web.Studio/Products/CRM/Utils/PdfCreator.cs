/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.Core.Files;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Statistics;
using Ionic.Zip;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Xml;

namespace ASC.Web.CRM.Classes
{
    public class PdfCreator
    {
        private static string TemplatePath
        {
            get
            {
                var path = HttpContext.Current.Server.MapPath("~/products/crm/invoicetemplates");
                return Path.Combine(path, "template.docx");
            }
        }
        private const string FormatPdf = ".pdf";
        private const string FormatDocx = ".docx";
        private const string DocumentXml = "word/document.xml";
        private const string DocumentLogoImage = "word/media/logo.jpeg";


        public static void CreateAndSaveFile(int invoiceId)
        {
            var log = log4net.LogManager.GetLogger("ASC.CRM");
            try
            {
                var invoice = Global.DaoFactory.GetInvoiceDao().GetByID(invoiceId);
                if (invoice == null)
                {
                    throw new Exception("Invoice not found " + invoiceId);
                }

                var exist = invoice.GetInvoiceFile() != null;

                string urlToFile;
                using (var docxStream = GetStreamDocx(invoice))
                {
                    urlToFile = GetUrlToFile(docxStream);
                }

                if (string.IsNullOrEmpty(urlToFile))
                {
                    throw new Exception("Empty url to pdf file " + invoiceId);
                }

                var file = new ASC.Files.Core.File
                    {
                        Title = string.Format("{0}{1}", invoice.Number, FormatPdf),
                        FolderID = Global.DaoFactory.GetFileDao().GetRoot(),
                    };

                if (exist)
                {
                    file.ID = invoice.FileID;
                }

                var request = WebRequest.Create(urlToFile);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    file.ContentLength = response.ContentLength;
                    file = Global.DaoFactory.GetFileDao().SaveFile(file, stream);
                }

                if (file != null)
                {
                    invoice.FileID = Int32.Parse(file.ID.ToString());
                    Global.DaoFactory.GetInvoiceDao().UpdateInvoiceFileID(invoice.ID, invoice.FileID);
                    if (!exist)
                    {
                        Global.DaoFactory.GetRelationshipEventDao().AttachFiles(invoice.ContactID, invoice.EntityType, invoice.EntityID, new[] { invoice.FileID });
                    }
                }
                else
                {
                    throw new Exception("file is null");
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        public static ASC.Files.Core.File CreateFile(Invoice data)
        {
            var log = log4net.LogManager.GetLogger("ASC.CRM");
            try
            {
                using (var docxStream = GetStreamDocx(data))
                {
                    var urlToFile = GetUrlToFile(docxStream);
                    return SaveFile(data, urlToFile);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                throw;
            }
        }

        private static string GetUrlToFile(Stream docxStream)
        {
            var documentService = new DocumentService(StudioKeySettings.GetKey(), StudioKeySettings.GetSKey(), TenantStatisticsProvider.GetUsersCount());
            var revisionId = DocumentService.GenerateRevisionId(Guid.NewGuid().ToString());

            var crmStorageUrl = WebConfigurationManager.AppSettings["crm.invoice.url.storage"];
            if (string.IsNullOrEmpty(crmStorageUrl))
            {
                crmStorageUrl = FilesLinkUtility.DocServiceStorageUrl;
            }

            var crmConverterUrl = WebConfigurationManager.AppSettings["crm.invoice.url.converter"];
            if (string.IsNullOrEmpty(crmConverterUrl))
            {
                crmConverterUrl = FilesLinkUtility.DocServiceConverterUrl;
            }

            var externalUri = documentService.GetExternalUri(crmStorageUrl, docxStream, "text/plain", revisionId);

            string urlToFile;
            documentService.GetConvertedUri(crmConverterUrl, externalUri, FormatDocx, FormatPdf, revisionId, false, out urlToFile);

            return urlToFile;
        }

        public static ConverterData StartCreationFileAsync(Invoice data)
        {
            using (var docxStream = GetStreamDocx(data))
            {
                var documentService = new DocumentService(StudioKeySettings.GetKey(), StudioKeySettings.GetSKey(), TenantStatisticsProvider.GetUsersCount());
                var revisionId = DocumentService.GenerateRevisionId(Guid.NewGuid().ToString());

                var crmStorageUrl = WebConfigurationManager.AppSettings["crm.invoice.url.storage"];
                if (string.IsNullOrEmpty(crmStorageUrl))
                {
                    crmStorageUrl = FilesLinkUtility.DocServiceStorageUrl;
                }

                var crmConverterUrl = WebConfigurationManager.AppSettings["crm.invoice.url.converter"];
                if (string.IsNullOrEmpty(crmConverterUrl))
                {
                    crmConverterUrl = FilesLinkUtility.DocServiceConverterUrl;
                }

                var externalUri = documentService.GetExternalUri(crmStorageUrl, docxStream, "text/plain", revisionId);

                string urlToFile;
                documentService.GetConvertedUri(crmConverterUrl, externalUri, FormatDocx, FormatPdf, revisionId, true, out urlToFile);

                return new ConverterData
                    {
                        ConverterUrl = crmConverterUrl,
                        StorageUrl = externalUri,
                        RevisionId = revisionId,
                        InvoiceId = data.ID,
                        UrlToFile = urlToFile
                    };
            }
        }

        public static ASC.Files.Core.File GetConvertedFile(ConverterData data)
        {
            if (string.IsNullOrEmpty(data.ConverterUrl) || string.IsNullOrEmpty(data.StorageUrl) || string.IsNullOrEmpty(data.RevisionId))
            {
                return null;
            }
            
            var documentService = new DocumentService(StudioKeySettings.GetKey(), StudioKeySettings.GetSKey(), TenantStatisticsProvider.GetUsersCount());
            
            string urlToFile;
            documentService.GetConvertedUri(data.ConverterUrl, data.StorageUrl, FormatDocx, FormatPdf, data.RevisionId, true, out urlToFile);

            if (string.IsNullOrEmpty(urlToFile))
            {
                return null;
            }

            var invoice = Global.DaoFactory.GetInvoiceDao().GetByID(data.InvoiceId);

            return SaveFile(invoice, urlToFile);
        }

        private static ASC.Files.Core.File SaveFile(Invoice data, string url)
        {
            ASC.Files.Core.File file = null;

            var request = (HttpWebRequest)WebRequest.Create(url);

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        var document = new ASC.Files.Core.File
                        {
                            Title = string.Format("{0}{1}", data.Number, FormatPdf),
                            FolderID = Global.DaoFactory.GetFileDao().GetRoot(),
                            ContentLength = response.ContentLength
                        };

                        if (data.GetInvoiceFile() != null)
                        {
                            document.ID = data.FileID;
                        }

                        file = Global.DaoFactory.GetFileDao().SaveFile(document, stream);
                    }
                }
            }

            return file;
        }

        private static Stream GetStreamDocx(Invoice data)
        {
            var invoiceData = InvoiceFormattedData.GetData(data, 0, 0);
            var logo = new byte[] {};

            if (!string.IsNullOrEmpty(invoiceData.LogoBase64))
            {
                logo = Convert.FromBase64String(invoiceData.LogoBase64);
            }

            using (var zip = ZipFile.Read(TemplatePath))
            {
                var documentXmlStream = new MemoryStream();
                foreach (var entry in zip.Entries.Where(entry => entry.FileName == DocumentXml))
                {
                    entry.Extract(documentXmlStream);
                }
                documentXmlStream.Position = 0;
                zip.RemoveEntry(DocumentXml);
                var document = new XmlDocument();
                document.Load(documentXmlStream);
                var documentStr = GenerateDocumentXml(document, invoiceData, logo);
                zip.AddEntry(DocumentXml, documentStr, Encoding.UTF8);

                if (logo.Length > 0)
                {
                    zip.UpdateEntry(DocumentLogoImage, logo);
                }

                var streamZip = new MemoryStream();
                zip.Save(streamZip);
                streamZip.Seek(0, SeekOrigin.Begin);
                streamZip.Flush();
                return streamZip;
            }

        }

        private static string GenerateDocumentXml(XmlDocument xDocument, InvoiceFormattedData data, byte[] logo)
        {
            XmlNodeList nodeList;
            XmlNode parent;
            XmlNode child;


            #region Seller

            nodeList = xDocument.SelectNodes("//*[@ascid='seller']");
            parent = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
            if (parent != null)
            {
                if (data.Seller == null)
                {
                    parent.RemoveAll();
                }
                else
                {
                    var newText = parent.CloneNode(true).OuterXml;
                    newText = newText
                        .Replace("${label}", EncodeAndReplaceLineBreaks(data.Seller.Item1))
                        .Replace("${value}", EncodeAndReplaceLineBreaks(data.Seller.Item2));
                    var newEl = new XmlDocument();
                    newEl.LoadXml(newText);
                    if (parent.ParentNode != null)
                    {
                        if (newEl.DocumentElement != null)
                        {
                            parent.ParentNode.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), parent);
                        }
                        parent.ParentNode.RemoveChild(parent);
                    }
                }
            }

            #endregion


            #region Logo

            nodeList = xDocument.SelectNodes("//*[@ascid='logo']");
            parent = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
            if (parent != null)
            {
                if (logo.Length <= 0)
                {
                    parent.RemoveAll();
                }
                else
                {
                    var stream = new MemoryStream(logo);
                    var img = System.Drawing.Image.FromStream(stream);
                    var cx = img.Width * 9525;//1px =  9525emu
                    var cy = img.Height * 9525;//1px =  9525emu
                    
                    var newText = parent.CloneNode(true).OuterXml;
                    newText = newText
                        .Replace("${width}", cx.ToString(CultureInfo.InvariantCulture))
                        .Replace("${height}", cy.ToString(CultureInfo.InvariantCulture));
                    var newEl = new XmlDocument();
                    newEl.LoadXml(newText);
                    if (parent.ParentNode != null)
                    {
                        if (newEl.DocumentElement != null)
                        {
                            parent.ParentNode.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), parent);
                        }
                        parent.ParentNode.RemoveChild(parent);
                    }
                }
            }

            #endregion


            #region Number

            nodeList = xDocument.SelectNodes("//*[@ascid='number']");
            parent = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
            if (parent != null)
            {
                if (data.Number == null)
                {
                    parent.RemoveAll();
                }
                else
                {
                    var newText = parent.CloneNode(true).OuterXml;
                    newText = newText
                        .Replace("${label}", EncodeAndReplaceLineBreaks(data.Number.Item1))
                        .Replace("${value}", EncodeAndReplaceLineBreaks(data.Number.Item2));
                    var newEl = new XmlDocument();
                    newEl.LoadXml(newText);
                    if (parent.ParentNode != null)
                    {
                        if (newEl.DocumentElement != null)
                        {
                            parent.ParentNode.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), parent);
                        }
                        parent.ParentNode.RemoveChild(parent);
                    }
                }
            }

            #endregion


            #region Invoice

            nodeList = xDocument.SelectNodes("//*[@ascid='invoice']");
            parent = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
            if (parent != null)
            {
                nodeList = xDocument.SelectNodes("//*[@ascid='invoice']//*[@ascid='row']");
                child = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
                if (child != null)
                {
                    if (data.Invoice == null || data.Invoice.Count <= 0)
                    {
                        if (parent.ParentNode != null)
                        {
                            parent.ParentNode.RemoveChild(parent);
                        }
                    }
                    else
                    {
                        foreach (var line in data.Invoice)
                        {
                            var newText = child.CloneNode(true).OuterXml;
                            newText = newText
                                .Replace("${label}", EncodeAndReplaceLineBreaks(line.Item1))
                                .Replace("${value}", EncodeAndReplaceLineBreaks(line.Item2));
                            var newEl = new XmlDocument();
                            newEl.LoadXml(newText);
                            if (newEl.DocumentElement != null)
                            {
                                parent.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), child);
                            }
                        }
                        parent.RemoveChild(child);
                    }
                }
            }

            #endregion


            #region Customer

            nodeList = xDocument.SelectNodes("//*[@ascid='customer']");
            parent = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
            if (parent != null)
            {
                if (data.Customer == null)
                {
                    if (parent.ParentNode != null)
                    {
                        parent.ParentNode.RemoveChild(parent);
                    }
                }
                else
                {
                    var newText = parent.CloneNode(true).OuterXml;
                    newText = newText
                        .Replace("${label}", EncodeAndReplaceLineBreaks(data.Customer.Item1))
                        .Replace("${value}", EncodeAndReplaceLineBreaks(data.Customer.Item2));
                    var newEl = new XmlDocument();
                    newEl.LoadXml(newText);
                    if (parent.ParentNode != null)
                    {
                        if (newEl.DocumentElement != null)
                        {
                            parent.ParentNode.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), parent);
                        }
                        parent.ParentNode.RemoveChild(parent);
                    }
                }
            }

            #endregion


            nodeList = xDocument.SelectNodes("//*[@ascid='table']");
            parent = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
            if (parent != null)
            {
                #region TableHeaderRow

                nodeList = xDocument.SelectNodes("//*[@ascid='table']//*[@ascid='headerRow']");
                child = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
                if (child != null)
                {
                    if (data.TableHeaderRow == null || data.TableHeaderRow.Count <= 0)
                    {
                        if (parent.ParentNode != null)
                            parent.ParentNode.RemoveChild(parent);
                    }
                    else
                    {
                        var newText = child.CloneNode(true).OuterXml;
                        for (var i = 0; i < data.TableHeaderRow.Count; i++)
                        {
                            newText = newText
                                .Replace("${label" + i + "}", EncodeAndReplaceLineBreaks(data.TableHeaderRow[i]));
                        }
                        var newEl = new XmlDocument();
                        newEl.LoadXml(newText);
                        if (newEl.DocumentElement != null)
                        {
                            parent.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), child);
                        }
                        parent.RemoveChild(child);
                    }
                }

                #endregion


                #region TableBodyRows

                nodeList = xDocument.SelectNodes("//*[@ascid='table']//*[@ascid='bodyRow']");
                child = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
                if (child != null)
                {
                    if (data.TableBodyRows == null || data.TableBodyRows.Count <= 0)
                    {
                        if (parent.ParentNode != null)
                            parent.ParentNode.RemoveChild(parent);
                    }
                    else
                    {
                        foreach (var line in data.TableBodyRows)
                        {
                            var newText = child.CloneNode(true).OuterXml;
                            for (var i = 0; i < line.Count; i++)
                            {
                                newText = newText
                                    .Replace("${value" + i + "}", EncodeAndReplaceLineBreaks(line[i]));
                            }
                            var newEl = new XmlDocument();
                            newEl.LoadXml(newText);
                            if (newEl.DocumentElement != null)
                            {
                                parent.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), child);
                            }
                        }
                        parent.RemoveChild(child);
                    }
                }

                #endregion


                #region TableFooterRows

                nodeList = xDocument.SelectNodes("//*[@ascid='table']//*[@ascid='footerRow']");
                child = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
                if (child != null)
                {
                    if (data.TableFooterRows == null || data.TableFooterRows.Count <= 0)
                    {
                        if (parent.ParentNode != null)
                            parent.ParentNode.RemoveChild(parent);
                    }
                    else
                    {
                        foreach (var line in data.TableFooterRows)
                        {
                            var newText = child.CloneNode(true).OuterXml;
                            newText = newText
                                .Replace("${label}", EncodeAndReplaceLineBreaks(line.Item1))
                                .Replace("${value}", EncodeAndReplaceLineBreaks(line.Item2));
                            var newEl = new XmlDocument();
                            newEl.LoadXml(newText);
                            if (newEl.DocumentElement != null)
                            {
                                parent.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), child);
                            }
                        }
                        parent.RemoveChild(child);
                    }
                }

                #endregion


                #region TableTotalRow

                nodeList = xDocument.SelectNodes("//*[@ascid='table']//*[@ascid='totalRow']");
                child = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
                if (child != null)
                {
                    if (data.TableTotalRow == null)
                    {
                        if (parent.ParentNode != null)
                            parent.ParentNode.RemoveChild(parent);
                    }
                    else
                    {
                        var newText = child.CloneNode(true).OuterXml;
                        newText = newText
                            .Replace("${label}", EncodeAndReplaceLineBreaks(data.TableTotalRow.Item1))
                            .Replace("${value}", EncodeAndReplaceLineBreaks(data.TableTotalRow.Item2));
                        var newEl = new XmlDocument();
                        newEl.LoadXml(newText);
                        if (newEl.DocumentElement != null)
                        {
                            parent.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), child);
                        }
                        parent.RemoveChild(child);
                    }
                }

                #endregion
            }


            #region Terms

            nodeList = xDocument.SelectNodes("//*[@ascid='terms']");
            parent = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
            if (parent != null)
            {
                if (data.Terms == null)
                {
                    if (parent.ParentNode != null)
                        parent.ParentNode.RemoveChild(parent);
                }
                else
                {
                    var newText = parent.CloneNode(true).OuterXml;
                    newText = newText
                        .Replace("${label}", EncodeAndReplaceLineBreaks(data.Terms.Item1))
                        .Replace("${value}", EncodeAndReplaceLineBreaks(data.Terms.Item2));
                    var newEl = new XmlDocument();
                    newEl.LoadXml(newText);
                    if (parent.ParentNode != null)
                    {
                        if (newEl.DocumentElement != null)
                        {
                            parent.ParentNode.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), parent);
                        }
                        parent.ParentNode.RemoveChild(parent);
                    }
                }
            }

            #endregion


            #region Notes

            nodeList = xDocument.SelectNodes("//*[@ascid='notes']");
            parent = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
            if (parent != null)
            {
                if (data.Notes == null)
                {
                    if (parent.ParentNode != null)
                        parent.ParentNode.RemoveChild(parent);
                }
                else
                {
                    var newText = parent.CloneNode(true).OuterXml;
                    newText = newText
                        .Replace("${label}", EncodeAndReplaceLineBreaks(data.Notes.Item1))
                        .Replace("${value}", EncodeAndReplaceLineBreaks(data.Notes.Item2));
                    var newEl = new XmlDocument();
                    newEl.LoadXml(newText);
                    if (parent.ParentNode != null)
                    {
                        if (newEl.DocumentElement != null)
                        {
                            parent.ParentNode.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), parent);
                        }
                        parent.ParentNode.RemoveChild(parent);
                    }
                }
            }

            #endregion


            #region Consignee

            nodeList = xDocument.SelectNodes("//*[@ascid='consignee']");
            parent = nodeList != null && nodeList.Count > 0 ? nodeList[0] : null;
            if (parent != null)
            {
                if (data.Consignee == null)
                {
                    if (parent.ParentNode != null)
                    {
                        parent.ParentNode.RemoveChild(parent);
                    }
                }
                else
                {
                    var newText = parent.CloneNode(true).OuterXml;
                    newText = newText
                        .Replace("${label}", EncodeAndReplaceLineBreaks(data.Consignee.Item1))
                        .Replace("${value}", EncodeAndReplaceLineBreaks(data.Consignee.Item2));
                    var newEl = new XmlDocument();
                    newEl.LoadXml(newText);
                    if (parent.ParentNode != null)
                    {
                        if (newEl.DocumentElement != null)
                        {
                            parent.ParentNode.InsertBefore(xDocument.ImportNode(newEl.DocumentElement, true), parent);
                        }
                        parent.ParentNode.RemoveChild(parent);
                    }
                }
            }

            #endregion


            return xDocument.InnerXml;
        }

        private static string EncodeAndReplaceLineBreaks(string str)
        {
            return str
                .Replace("&", "&amp;")
                .Replace("'", "&apos;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace(Environment.NewLine, "</w:t><w:br/><w:t xml:space=\"preserve\">");
        }
    }

    public class InvoiceFormattedData
    {
        public int TemplateType { get; set; }
        public Tuple<string, string> Seller { get; set; }
        public string LogoBase64 { get; set; }
        public string LogoSrcFormat { get; set; }
        public Tuple<string, string> Number { get; set; }
        public List<Tuple<string, string>> Invoice { get; set; }
        public Tuple<string, string> Customer { get; set; }
        public List<string> TableHeaderRow { get; set; }
        public List<List<string>> TableBodyRows { get; set; }
        public List<Tuple<string, string>> TableFooterRows { get; set; }
        public Tuple<string, string> TableTotalRow { get; set; }
        public Tuple<string, string> Terms { get; set; }
        public Tuple<string, string> Notes { get; set; }
        public Tuple<string, string> Consignee { get; set; }

        public int DeliveryAddressID { get; set; }
        public int BillingAddressID { get; set; }

        public static InvoiceFormattedData GetData(Invoice invoice, int billingAddressID, int deliveryAddressID)
        {
            return invoice.JsonData != null ? ReadData(invoice.JsonData) : CreateData(invoice, billingAddressID, deliveryAddressID);
        }

        private static InvoiceFormattedData CreateData(Invoice invoice, int billingAddressID, int deliveryAddressID)
        {
            var data = new InvoiceFormattedData();
            var sb = new StringBuilder();
            var list = new List<string>();
            var cultureInfo = string.IsNullOrEmpty(invoice.Language) ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(invoice.Language);
            

            #region TemplateType

            data.TemplateType = (int) invoice.TemplateType;

            #endregion


            #region Seller, LogoBase64, LogoSrcFormat

            var invoiceSettings = Global.DaoFactory.GetInvoiceDao().GetSettings();

            if (!string.IsNullOrEmpty(invoiceSettings.CompanyName))
            {
                sb.Append(invoiceSettings.CompanyName);
            }

            if (!string.IsNullOrEmpty(invoiceSettings.CompanyAddress))
            {
                var obj = JObject.Parse(invoiceSettings.CompanyAddress);

                var str = obj.Value<string>("country");
                if (!string.IsNullOrEmpty(str))
                    list.Add(str);

                str = obj.Value<string>("state");
                if (!string.IsNullOrEmpty(str))
                    list.Add(str);

                str = obj.Value<string>("city");
                if (!string.IsNullOrEmpty(str))
                    list.Add(str);

                str = obj.Value<string>("street");
                if (!string.IsNullOrEmpty(str))
                    list.Add(str);

                str = obj.Value<string>("zip");
                if (!string.IsNullOrEmpty(str))
                    list.Add(str);

                if (list.Count > 0)
                {
                    sb.AppendLine();
                    sb.Append(string.Join(", ", list));
                }
            }

            data.Seller = new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Seller", cultureInfo), sb.ToString());

            if (invoiceSettings.CompanyLogoID != 0)
            {
                data.LogoBase64 = OrganisationLogoManager.GetOrganisationLogoBase64(invoiceSettings.CompanyLogoID);
                data.LogoSrcFormat = OrganisationLogoManager.OrganisationLogoSrcFormat;
            }

            #endregion


            #region Number

            data.Number = new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Invoice", cultureInfo), invoice.Number);

            #endregion


            #region Invoice

            data.Invoice = new List<Tuple<string, string>>();
            data.Invoice.Add(new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("IssueDate", cultureInfo), invoice.IssueDate.ToShortDateString()));
            if (!string.IsNullOrEmpty(invoice.PurchaseOrderNumber))
            {
                data.Invoice.Add(new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("PONumber", cultureInfo), invoice.PurchaseOrderNumber));
            }
            data.Invoice.Add(new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("DueDate", cultureInfo), invoice.DueDate.ToShortDateString()));
            
            #endregion


            #region Customer

            var customer = Global.DaoFactory.GetContactDao().GetByID(invoice.ContactID);

            if (customer != null)
            {
                sb = new StringBuilder();

                sb.Append(customer.GetTitle());

                var billingAddress = billingAddressID != 0 ? Global.DaoFactory.GetContactInfoDao().GetByID(billingAddressID) : null;
                if (billingAddress != null && billingAddress.InfoType == ContactInfoType.Address && billingAddress.Category == (int)AddressCategory.Billing )
                {
                    list = new List<string>();

                    var obj = JObject.Parse(billingAddress.Data);

                    var str = obj.Value<string>("country");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("state");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("city");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("street");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("zip");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    if (list.Count > 0)
                    {
                        sb.AppendLine();
                        sb.Append(string.Join(", ", list));
                    }
                }

                data.Customer = new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("BillTo", cultureInfo), sb.ToString());
            }

            #endregion


            #region TableHeaderRow, TableBodyRows, TableFooterRows, TableTotalRow

            data.TableHeaderRow = new List<string>
                {
                    CRMInvoiceResource.ResourceManager.GetString("ItemCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("QuantityCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("PriceCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("DiscountCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("TaxCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("TaxCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("AmountCol", cultureInfo)
                };

            data.TableBodyRows = new List<List<string>>();

            var invoiceLines = invoice.GetInvoiceLines();
            var invoiceTaxes = new Dictionary<int, decimal>();

            decimal subtotal = 0;
            decimal discount = 0;
            decimal amount = 0;

            foreach (var line in invoiceLines)
            {
                var item = Global.DaoFactory.GetInvoiceItemDao().GetByID(line.InvoiceItemID);
                var tax1 = line.InvoiceTax1ID > 0 ? Global.DaoFactory.GetInvoiceTaxDao().GetByID(line.InvoiceTax1ID) : null;               
                var tax2 = line.InvoiceTax2ID > 0 ? Global.DaoFactory.GetInvoiceTaxDao().GetByID(line.InvoiceTax2ID) : null;

                var subtotalValue = Math.Round(line.Quantity * line.Price, 2);
                var discountValue = Math.Round(subtotalValue * line.Discount / 100, 2);

                var rate = 0;
                if (tax1 != null)
                {
                    rate += tax1.Rate;
                    if (invoiceTaxes.ContainsKey(tax1.ID))
                    {
                        invoiceTaxes[tax1.ID] = invoiceTaxes[tax1.ID] + Math.Round((subtotalValue - discountValue)*tax1.Rate/100, 2);
                    }
                    else
                    {
                        invoiceTaxes.Add(tax1.ID, Math.Round((subtotalValue - discountValue)*tax1.Rate/100, 2));
                    }
                }
                if (tax2 != null)
                {
                    rate += tax2.Rate;
                    if (invoiceTaxes.ContainsKey(tax2.ID))
                    {
                        invoiceTaxes[tax2.ID] = invoiceTaxes[tax2.ID] + Math.Round((subtotalValue - discountValue)*tax2.Rate/100, 2);
                    }
                    else
                    {
                        invoiceTaxes.Add(tax2.ID, Math.Round((subtotalValue - discountValue)*tax2.Rate/100, 2));
                    }
                }

                decimal taxValue = Math.Round((subtotalValue - discountValue)*rate/100, 2);
                decimal amountValue = Math.Round(subtotalValue - discountValue + taxValue, 2);

                subtotal += subtotalValue;
                discount += discountValue;
                amount += amountValue;

                data.TableBodyRows.Add(new List<string>
                    {
                        item.Title + (string.IsNullOrEmpty(line.Description) ? string.Empty : ": " +line.Description),
                        line.Quantity.ToString(CultureInfo.InvariantCulture),
                        line.Price.ToString(CultureInfo.InvariantCulture),
                        line.Discount.ToString(CultureInfo.InvariantCulture),
                        tax1 != null ? tax1.Name : string.Empty,
                        tax2 != null ? tax2.Name : string.Empty,
                        (subtotalValue-discountValue).ToString(CultureInfo.InvariantCulture)
                    });
            }

            data.TableFooterRows = new List<Tuple<string, string>>();
            data.TableFooterRows.Add(new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Subtotal", cultureInfo), (subtotal - discount).ToString(CultureInfo.InvariantCulture)));

            foreach (var invoiceTax in invoiceTaxes)
            {
                var iTax = Global.DaoFactory.GetInvoiceTaxDao().GetByID(invoiceTax.Key);
                data.TableFooterRows.Add(new Tuple<string, string>(string.Format("{0} ({1}%)", iTax.Name, iTax.Rate), invoiceTax.Value.ToString(CultureInfo.InvariantCulture)));
            }

            //data.TableFooterRows.Add(new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Discount", cultureInfo), "-" + discount.ToString(CultureInfo.InvariantCulture)));

            data.TableTotalRow = new Tuple<string, string>(string.Format("{0} ({1})", CRMInvoiceResource.ResourceManager.GetString("Total", cultureInfo), invoice.Currency), amount.ToString(CultureInfo.InvariantCulture));


            #endregion


            #region Terms

            data.Terms = new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Terms", cultureInfo), invoice.Terms);

            #endregion


            #region Notes

            if (!string.IsNullOrEmpty(invoice.Description))
            {
                data.Notes = new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("ClientNotes", cultureInfo), invoice.Description);
            }

            #endregion


            #region Consignee

            var consignee = Global.DaoFactory.GetContactDao().GetByID(invoice.ConsigneeID);

            if (consignee != null)
            {
                sb = new StringBuilder();

                sb.Append(consignee.GetTitle());

                var deliveryAddress = deliveryAddressID != 0 ? Global.DaoFactory.GetContactInfoDao().GetByID(deliveryAddressID) : null;
                if (deliveryAddress != null && deliveryAddress.InfoType == ContactInfoType.Address && deliveryAddress.Category == (int)AddressCategory.Postal )
                {
                    list = new List<string>();
                    
                    var obj = JObject.Parse(deliveryAddress.Data);

                    var str = obj.Value<string>("country");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("state");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("city");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("street");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("zip");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    if (list.Count > 0)
                    {
                        sb.AppendLine();
                        sb.Append(string.Join(", ", list));
                    }
                }

                data.Consignee = new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("ShipTo", cultureInfo), sb.ToString());
            }

            #endregion

            #region Addresses

            data.BillingAddressID = billingAddressID;
            data.DeliveryAddressID = deliveryAddressID;

            #endregion

            return data;
        }

        private static InvoiceFormattedData ReadData(string jsonData)
        {
            var data = new InvoiceFormattedData();
            var jsonObj = JObject.Parse(jsonData);


            #region TemplateType

            data.TemplateType = jsonObj.Value<int>("TemplateType");

            #endregion


            #region Seller, LogoBase64, LogoSrcFormat

            var seller = jsonObj.Value<JObject>("Seller");
            if (seller != null)
            {
                data.Seller = seller.ToObject<Tuple<string, string>>();
            }

            data.LogoBase64 = jsonObj.Value<string>("LogoBase64");
            data.LogoSrcFormat = jsonObj.Value<string>("LogoSrcFormat");

            #endregion


            #region Number

            var number = jsonObj.Value<JObject>("Number");
            if (number != null)
            {
                data.Number = number.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region Invoice

            var invoice = jsonObj.Value<JArray>("Invoice");
            if (invoice != null)
            {
                data.Invoice = invoice.ToObject<List<Tuple<string, string>>>();
            }

            #endregion


            #region Customer

            var customer = jsonObj.Value<JObject>("Customer");
            if (customer != null)
            {
                data.Customer = customer.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region TableHeaderRow, TableBodyRows, TableFooterRows, Total

            var tableHeaderRow = jsonObj.Value<JArray>("TableHeaderRow");
            if (tableHeaderRow != null)
            {
                data.TableHeaderRow = tableHeaderRow.ToObject<List<string>>();
            }

            var tableBodyRows = jsonObj.Value<JArray>("TableBodyRows");
            if (tableBodyRows != null)
            {
                data.TableBodyRows = tableBodyRows.ToObject<List<List<string>>>();
            }

            var tableFooterRows = jsonObj.Value<JArray>("TableFooterRows");
            if (tableFooterRows != null)
            {
                data.TableFooterRows = tableFooterRows.ToObject<List<Tuple<string, string>>>();
            }

            var tableTotalRow = jsonObj.Value<JObject>("TableTotalRow");
            if (tableTotalRow != null)
            {
                data.TableTotalRow = tableTotalRow.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region Terms

            var terms = jsonObj.Value<JObject>("Terms");
            if (terms != null)
            {
                data.Terms = terms.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region Notes

            var notes = jsonObj.Value<JObject>("Notes");
            if (notes != null)
            {
                data.Notes = notes.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region Consignee

            var consignee = jsonObj.Value<JObject>("Consignee");
            if (consignee != null)
            {
                data.Consignee = consignee.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region Addresses

            data.DeliveryAddressID = !String.IsNullOrEmpty(jsonObj.Value<string>("DeliveryAddressID")) ? jsonObj.Value<int>("DeliveryAddressID") : 0;
            data.BillingAddressID = !String.IsNullOrEmpty(jsonObj.Value<string>("BillingAddressID")) ? jsonObj.Value<int>("BillingAddressID") : 0;

            #endregion

            return data;
        }
    }

    public class ConverterData
    {
        public string ConverterUrl { get; set; }
        public string StorageUrl { get; set; }
        public string RevisionId { get; set; }
        public string UrlToFile { get; set; }
        public int InvoiceId { get; set; }
        public int FileId { get; set; }
    }
}