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


using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.CRM.Resources;
using ASC.Web.Core.Client.HttpHandlers;

namespace ASC.Web.CRM.Masters.ClientScripts
{
    public class ClientLocalizationResources : ClientScriptLocalization
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Resources"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            yield return RegisterResourceSet("CRMJSResource", CRMJSResource.ResourceManager);
            yield return RegisterResourceSet("CRMCommonResource", CRMCommonResource.ResourceManager);
            yield return RegisterResourceSet("CRMContactResource", CRMContactResource.ResourceManager);
            yield return RegisterResourceSet("CRMDealResource", CRMDealResource.ResourceManager);
            yield return RegisterResourceSet("CRMInvoiceResource", CRMInvoiceResource.ResourceManager);
            yield return RegisterResourceSet("CRMTaskResource", CRMTaskResource.ResourceManager);
            yield return RegisterResourceSet("CRMCasesResource", CRMCasesResource.ResourceManager);
            yield return RegisterResourceSet("CRMEnumResource", CRMEnumResource.ResourceManager);
            yield return RegisterResourceSet("CRMSettingResource", CRMSettingResource.ResourceManager);
            yield return RegisterResourceSet("CRMSocialMediaResource", CRMSocialMediaResource.ResourceManager);

        }
    }

    public class ClientCustomResources : ClientScriptCustom
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Resources"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return new List<KeyValuePair<string, object>>(1)
                   {
                       RegisterObject(
                       new
                       {
                           AddUser = Studio.Core.Users.CustomNamingPeople.Substitute<CRMCommonResource>("AddUser").HtmlEncode(),
                           AddGroup = Studio.Core.Users.CustomNamingPeople.Substitute<CRMCommonResource>("AddGroup").HtmlEncode(),
                           CurrentUser = Studio.Core.Users.CustomNamingPeople.Substitute<CRMCommonResource>("CurrentUser").HtmlEncode(),
                           PrivatePanelAccessListLable = Studio.Core.Users.CustomNamingPeople.Substitute<CRMCommonResource>("PrivatePanelAccessListLable").HtmlEncode(),
                           PrivatePanelDescription = Studio.Core.Users.CustomNamingPeople.Substitute<CRMCommonResource>("PrivatePanelDescription").HtmlEncode(),
                           SocialMediaAccountNotFoundTwitter = ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.SocialMediaAccountNotFoundTwitter.HtmlEncode()
                       })
                   };
        }
    }
}