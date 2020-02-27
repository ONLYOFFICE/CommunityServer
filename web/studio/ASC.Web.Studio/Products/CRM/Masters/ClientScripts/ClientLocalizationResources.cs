/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Web.CRM.Classes;
using ASC.Web.Studio.Core.Users;
using Resources;

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
            return new List<KeyValuePair<string, object>>(11)
            {
                RegisterResourceSet("CRMJSResource", CRMJSResource.ResourceManager),
                RegisterResourceSet("CRMCommonResource", CRMCommonResource.ResourceManager),
                RegisterResourceSet("CRMContactResource", CRMContactResource.ResourceManager),
                RegisterResourceSet("CRMDealResource", CRMDealResource.ResourceManager),
                RegisterResourceSet("CRMInvoiceResource", CRMInvoiceResource.ResourceManager),
                RegisterResourceSet("CRMTaskResource", CRMTaskResource.ResourceManager),
                RegisterResourceSet("CRMCasesResource", CRMCasesResource.ResourceManager),
                RegisterResourceSet("CRMEnumResource", CRMEnumResource.ResourceManager),
                RegisterResourceSet("CRMSettingResource", CRMSettingResource.ResourceManager),
                RegisterResourceSet("CRMSocialMediaResource", CRMSocialMediaResource.ResourceManager),
                RegisterResourceSet("CRMVoipResource", CRMVoipResource.ResourceManager),
                RegisterResourceSet("CRMReportResource", CRMReportResource.ResourceManager),
                RegisterObject(new
                {
                    DealMilestoneStatus = new
                    {
                        Open = new
                        {
                            num = (int)ASC.CRM.Core.DealMilestoneStatus.Open,
                            str = ASC.CRM.Core.DealMilestoneStatus.Open.ToLocalizedString()
                        },
                        ClosedAndWon = new
                        {
                            num = (int)ASC.CRM.Core.DealMilestoneStatus.ClosedAndWon,
                            str = ASC.CRM.Core.DealMilestoneStatus.ClosedAndWon.ToLocalizedString()
                        },
                        ClosedAndLost = new
                        {
                            num = (int)ASC.CRM.Core.DealMilestoneStatus.ClosedAndLost,
                            str = ASC.CRM.Core.DealMilestoneStatus.ClosedAndLost.ToLocalizedString()
                        }
                    },
                    smtpQuotas = string.Format(CRMSettingResource.InternalSMTP, MailSender.GetQuotas())
                })
            };
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
                           AddUser = CustomNamingPeople.Substitute<CRMCommonResource>("AddUser").HtmlEncode(),
                           AddGroup = CustomNamingPeople.Substitute<CRMCommonResource>("AddGroup").HtmlEncode(),
                           CurrentUser = CustomNamingPeople.Substitute<CRMCommonResource>("CurrentUser").HtmlEncode(),
                           PrivatePanelAccessListLable = CustomNamingPeople.Substitute<CRMCommonResource>("PrivatePanelAccessListLable").HtmlEncode(),
                           PrivatePanelDescription = CustomNamingPeople.Substitute<CRMCommonResource>("PrivatePanelDescription").HtmlEncode(),
                           SocialMediaAccountNotFoundTwitter = CRMSocialMediaResource.SocialMediaAccountNotFoundTwitter.HtmlEncode(),
                           DisabledEmployeeTitle = Resource.DisabledEmployeeTitle.HtmlEncode()
                       })
                   };
        }
    }
}