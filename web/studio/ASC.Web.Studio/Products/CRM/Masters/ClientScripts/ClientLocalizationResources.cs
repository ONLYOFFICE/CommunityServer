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