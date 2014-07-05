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

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Microsoft.Web.Administration;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    internal class CertificateUploader : IFileUploadHandler
    {
        public FileUploadResult ProcessUpload(HttpContext context)
        {
            var result = new FileUploadResult();
            try
            {
                if (context.Request.Files.Count != 0)
                {
                    var certificate = context.Request.Files[0];
                    var path = Path.Combine(Path.GetTempPath(), certificate.FileName);

                    if (!File.Exists(path))
                    {
                        certificate.SaveAs(path);
                    }

                    result.Success = true;
                    result.Message = path;
                }
                else
                {
                    result.Success = false;
                    result.Message = Resource.ErrorEmptyUploadFileSelected;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.HtmlEncode();
            }

            return result;
        }

    }

    [AjaxNamespace("UploadHttps")]
    public partial class UploadHttps : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/UploadHttps/UploadHttps.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _checkAttachmentContainer.Options.IsPopup = true;
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/uploader/ajaxupload.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/uploadhttps/js/uploadhttps.js"));
        }

        [AjaxMethod]
        public AjaxResponse CheckAttachment()
        {
            var response = new AjaxResponse();

            try
            {
                using (var serverManager = new ServerManager())
                {
                    if (serverManager.Sites.SelectMany(r => r.Bindings).Any(r => r.BindingInformation.Contains("*:443:")))
                    {
                        response.status = "error";
                        return response;
                    }
                }

                response.status = "success";
            }
            catch (Exception e)
            {
                response.status = "error";
                response.message = e.Message;
            }

            return response;
        }

        [AjaxMethod]
        public AjaxResponse UploadCertificate(string fileName, string password)
        {
            var response = new AjaxResponse();

            try
            {
                const string newbindinginformation = "*:443:";

                var store2 = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                var sp = new StorePermission(PermissionState.Unrestricted) {Flags = StorePermissionFlags.AllFlags};
                sp.Assert();
                store2.Open(OpenFlags.MaxAllowed);

                var cert = new X509Certificate2(fileName, password) {FriendlyName = fileName.Split('\\').Last()};
                store2.Add(cert);
                store2.Close();

                using (var serverManager = new ServerManager())
                {
                    foreach (var s in serverManager.Sites)
                    {
                        var bindingIndex = -1;
                        foreach (var b in s.Bindings.Where(r => r.BindingInformation.Contains(newbindinginformation)))
                        {
                            bindingIndex = s.Bindings.IndexOf(b);
                        }

                        if (bindingIndex != -1)
                        {
                            s.Bindings.RemoveAt(bindingIndex);
                        }
                    }

                    var site = serverManager.Sites[HostingEnvironment.ApplicationHost.GetSiteName()];
                    var binding = site.Bindings.Add(newbindinginformation, cert.GetCertHash(), store2.Name);
                    binding.Protocol = "https";

                    AddRewriteRules(serverManager);

                    serverManager.CommitChanges();
                }

                response.status = "success";
                response.message = Resource.UploadHttpsSettingsSuccess;
            }
            catch (Exception e)
            {
                response.status = "error";
                response.message = e.Message;
            }

            return response;
        }

        private static void AddRewriteRules(ServerManager serverManager)
        {
            var config = serverManager.GetWebConfiguration("asc");

            var rulesSection = config.GetSection("system.webServer/rewrite/rules");

            var rulesCollection = rulesSection.GetCollection();
            //HTTP to HTTPS
            var ruleElement = rulesCollection.CreateElement("rule");
            ruleElement["name"] = @"HTTP to HTTPS";
            ruleElement["stopProcessing"] = true;

            var matchElement = ruleElement.GetChildElement("match");
            matchElement["url"] = @".*";

            var conditionsElement = ruleElement.GetChildElement("conditions");

            var conditionsCollection = conditionsElement.GetCollection();

            var addElement = conditionsCollection.CreateElement("add");
            addElement["input"] = @"{HTTPS}";
            addElement["pattern"] = @"off";
            conditionsCollection.Add(addElement);

            addElement = conditionsCollection.CreateElement("add");
            addElement["input"] = @"{REQUEST_URI}";
            addElement["pattern"] = @"products/files/services/wcfservice/service.svc.*";
            addElement["negate"] = @"true";
            conditionsCollection.Add(addElement);

            var actionElement = ruleElement.GetChildElement("action");
            actionElement["type"] = @"Redirect";
            actionElement["url"] = @"https://{HTTP_HOST}/{R:0}";
            actionElement["appendQueryString"] = "true";
            actionElement["redirectType"] = @"Temporary";

            if (rulesCollection.Count(r => "HTTP to HTTPS".Equals(r.GetAttributeValue("name"))) <= 0)
            {
                rulesCollection.Add(ruleElement);
            }

            //WCF files HTTPS to HTTP
            ruleElement = rulesCollection.CreateElement("rule");
            ruleElement["name"] = @"WCF files HTTPS to HTTP";
            ruleElement["stopProcessing"] = true;

            matchElement = ruleElement.GetChildElement("match");
            matchElement["url"] = @"products/files/services/wcfservice/service.svc.*";

            conditionsElement = ruleElement.GetChildElement("conditions");

            conditionsCollection = conditionsElement.GetCollection();

            addElement = conditionsCollection.CreateElement("add");
            addElement["input"] = @"{HTTPS}";
            addElement["pattern"] = @"on";
            conditionsCollection.Add(addElement);

            actionElement = ruleElement.GetChildElement("action");
            actionElement["type"] = @"Rewrite";
            actionElement["url"] = @"http://{HTTP_HOST}/{R:0}";
            actionElement["appendQueryString"] = "true";

            if (rulesCollection.Count(r => "WCF files HTTPS to HTTP".Equals(r.GetAttributeValue("name"))) <= 0)
            {
                rulesCollection.Add(ruleElement);
            }
        }
    }
}