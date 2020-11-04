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


using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.Client.HttpHandlers;

namespace ASC.Web.CRM.Masters.ClientScripts
{
    public class ClientTemplateResources : ClientScriptTemplate
    {
        protected override string[] Links
        {
            get
            {
                return new[] {
                "~/Products/CRM/Templates/CasesTemplates.html",
                "~/Products/CRM/Templates/CommonCustomFieldsTemplates.html",
                "~/Products/CRM/Templates/CommonTemplates.html",
                "~/Products/CRM/Templates/ContactsTemplates.html",
                "~/Products/CRM/Templates/DealsTemplates.html",
                "~/Products/CRM/Templates/DealsSelectorTemplates.html",
                "~/Products/CRM/Templates/SimpleContactListTemplate.html",
                "~/Products/CRM/Templates/TasksTemplates.html",
                "~/Products/CRM/Templates/ContactSelectorTemplates.html",
                "~/Products/CRM/Templates/ContactInfoCardTemplate.html",
                "~/Products/CRM/Templates/SettingsTemplates.html",
                "~/Products/CRM/Templates/InvoicesTemplates.html",
                "~/Products/CRM/Templates/VoipTemplates.html",
                "~/Products/Projects/ProjectsTemplates/ListProjectsTemplates.html"
                };
            }
        }
    }
}