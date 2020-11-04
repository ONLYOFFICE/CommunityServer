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


using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using ASC.Web.Core.WebZones;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Xml;
using ASC.Web.Studio.PublicResources;
using SubscriptionManager = ASC.Web.Files.Classes.SubscriptionManager;

namespace ASC.Web.Files.Configuration
{
    [WebZoneAttribute(WebZoneType.CustomProductList | WebZoneType.StartProductList | WebZoneType.TopNavigationProductList)]
    public class ProductEntryPoint : Product
    {
        #region Members

        public static readonly Guid ID = WebItemManager.DocumentsProductID;

        private ProductContext _productContext;

        #endregion

        public override bool Visible { get { return true; } }

        public override void Init()
        {
            Global.Init();

            Func<List<string>> adminOpportunities = () => (CoreContext.Configuration.CustomMode
                                                               ? CustomModeResource.ProductAdminOpportunitiesCustomMode
                                                               : FilesCommonResource.ProductAdminOpportunities).Split('|').ToList();

            Func<List<string>> userOpportunities = () => (CoreContext.Configuration.CustomMode
                                         ? CustomModeResource.ProductUserOpportunitiesCustomMode
                                         : FilesCommonResource.ProductUserOpportunities).Split('|').ToList();

            _productContext =
                new ProductContext
                    {
                        MasterPageFile = FilesLinkUtility.FilesBaseVirtualPath + "Masters/BasicTemplate.master",
                        DisabledIconFileName = "product_disabled_logo.png",
                        IconFileName = "product_logo.png",
                        LargeIconFileName = "product_logolarge.svg",
                        DefaultSortOrder = 10,
                        SubscriptionManager = new SubscriptionManager(),
                        SpaceUsageStatManager = new FilesSpaceUsageStatManager(),
                        AdminOpportunities = adminOpportunities,
                        UserOpportunities = userOpportunities,
                        CanNotBeDisabled = true,
                    };
            SearchHandlerManager.Registry(new SearchHandler());

            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "FileStorageService",
                routeTemplate: "products/files/services/wcfservice/service.svc/{action}",
                defaults: new { controller = "FileStorageService" });
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.UseDataContractJsonSerializer = true;
        }

        public String GetModuleResource(String ResourceClassTypeName, String ResourseKey)
        {
            if (string.IsNullOrEmpty(ResourseKey)) return string.Empty;
            try
            {
                return (String)Type.GetType(ResourceClassTypeName).GetProperty(ResourseKey, BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        private static Dictionary<String, XmlDocument> _xslTemplates;

        public void ProcessRequest(HttpContext context)
        {
            if (_xslTemplates == null)
                _xslTemplates = new Dictionary<String, XmlDocument>();

            if (String.IsNullOrEmpty(context.Request["id"]) || String.IsNullOrEmpty(context.Request["name"]))
                return;

            var TemplateName = context.Request["name"];
            var TemplatePath = context.Request["id"];
            var Template = new XmlDocument();
            try
            {
                Template.Load(context.Server.MapPath(String.Format("~{0}{1}.xsl", TemplatePath, TemplateName)));
            }
            catch (Exception)
            {
                return;
            }
            if (Template.GetElementsByTagName("xsl:stylesheet").Count == 0)
                return;

            var Aliases = new Dictionary<String, String>();

            var RegisterAliases = Template.GetElementsByTagName("register");
            while ((RegisterAliases = Template.GetElementsByTagName("register")).Count > 0)
            {
                var RegisterAlias = RegisterAliases.Item(0);
                if (!String.IsNullOrEmpty(RegisterAlias.Attributes["alias"].Value) &&
                    !String.IsNullOrEmpty(RegisterAlias.Attributes["type"].Value))
                    Aliases.Add(RegisterAlias.Attributes["alias"].Value, RegisterAlias.Attributes["type"].Value);
                RegisterAlias.ParentNode.RemoveChild(RegisterAlias);
            }

            var CurrentResources = Template.GetElementsByTagName("resource");

            while ((CurrentResources = Template.GetElementsByTagName("resource")).Count > 0)
            {
                var CurrentResource = CurrentResources.Item(0);
                if (!String.IsNullOrEmpty(CurrentResource.Attributes["name"].Value))
                {
                    var FullName = CurrentResource.Attributes["name"].Value.Split('.');
                    if (FullName.Length == 2 && Aliases.ContainsKey(FullName[0]))
                    {
                        var ResourceValue =
                            Template.CreateTextNode(GetModuleResource(Aliases[FullName[0]], FullName[1]));
                        CurrentResource.ParentNode.InsertBefore(ResourceValue, CurrentResource);
                    }
                }
                CurrentResource.ParentNode.RemoveChild(CurrentResource);
            }

            context.Response.ContentType = "text/xml";
            context.Response.Write(Template.InnerXml);
        }


        public override Guid ProductID
        {
            get { return ID; }
        }

        public override string Name
        {
            get { return FilesCommonResource.ProductName; }
        }

        public override string Description
        {
            get
            {
                var id = SecurityContext.CurrentAccount.ID;

                if (CoreContext.UserManager.IsUserInGroup(id, ASC.Core.Users.Constants.GroupVisitor.ID))
                    return FilesCommonResource.ProductDescriptionShort;

                if (CoreContext.UserManager.IsUserInGroup(id, ASC.Core.Users.Constants.GroupAdmin.ID) || CoreContext.UserManager.IsUserInGroup(id, ID))
                    return FilesCommonResource.ProductDescriptionEx;

                return FilesCommonResource.ProductDescription;
            }
        }

        public override string StartURL
        {
            get { return PathProvider.StartURL; }
        }

        public override string HelpURL
        {
            get { return PathProvider.StartURL; }
        }

        public override string ProductClassName
        {
            get { return "documents"; }
        }

        public override ProductContext Context
        {
            get { return _productContext; }
        }
    }
}