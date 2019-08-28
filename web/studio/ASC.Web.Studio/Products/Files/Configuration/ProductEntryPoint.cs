/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

            var adminOpportunities = CoreContext.Configuration.CustomMode
                                         ? CustomModeResource.ProductAdminOpportunitiesCustomMode
                                         : FilesCommonResource.ProductAdminOpportunities;

            var userOpportunities = CoreContext.Configuration.CustomMode
                                         ? CustomModeResource.ProductUserOpportunitiesCustomMode
                                         : FilesCommonResource.ProductUserOpportunities;

            _productContext =
                new ProductContext
                    {
                        MasterPageFile = FilesLinkUtility.FilesBaseVirtualPath + "Masters/BasicTemplate.master",
                        DisabledIconFileName = "product_disabled_logo.png",
                        IconFileName = "product_logo.png",
                        LargeIconFileName = "product_logolarge.png",
                        DefaultSortOrder = 10,
                        SubscriptionManager = new SubscriptionManager(),
                        SpaceUsageStatManager = new FilesSpaceUsageStatManager(),
                        AdminOpportunities = () => adminOpportunities.Split('|').ToList(),
                        UserOpportunities = () => userOpportunities.Split('|').ToList(),
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


        public override string ExtendedDescription
        {
            get { return FilesCommonResource.ProductDescriptionEx; }
        }

        public override string Description
        {
            get { return FilesCommonResource.ProductDescription; }
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