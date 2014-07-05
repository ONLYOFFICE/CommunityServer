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

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="CleintResources.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

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
            yield return RegisterObject("AddUser", Studio.Core.Users.CustomNamingPeople.Substitute<CRMCommonResource>("AddUser").HtmlEncode());
            yield return RegisterObject("AddGroup", Studio.Core.Users.CustomNamingPeople.Substitute<CRMCommonResource>("AddGroup").HtmlEncode());
            yield return RegisterObject("CurrentUser", Studio.Core.Users.CustomNamingPeople.Substitute<CRMCommonResource>("CurrentUser").HtmlEncode());
            yield return RegisterObject("PrivatePanelAccessListLable", Studio.Core.Users.CustomNamingPeople.Substitute<CRMCommonResource>("PrivatePanelAccessListLable").HtmlEncode());
            yield return RegisterObject("PrivatePanelDescription", Studio.Core.Users.CustomNamingPeople.Substitute<CRMCommonResource>("PrivatePanelDescription").HtmlEncode());
        }
    }
}