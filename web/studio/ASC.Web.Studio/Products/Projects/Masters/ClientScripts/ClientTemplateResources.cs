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


using ASC.Web.Core.Client.HttpHandlers;

namespace ASC.Web.Projects.Masters.ClientScripts
{
    public class ClientTemplateResources : ClientScriptTemplate
    {
        protected override string[] Links
        {
            get
            {
                return new[]
                {
                    "~/Products/Projects/ProjectsTemplates/ListProjectsTemplates.html",
                    "~/Products/Projects/ProjectsTemplates/ListMilestonesTemplates.html",
                    "~/Products/Projects/ProjectsTemplates/TimeTrackingTemplates.html",
                    "~/Products/Projects/ProjectsTemplates/ProjectsTmplTemplates.html",
                    "~/Products/Projects/ProjectsTemplates/ListTasksTemplates.html",
                    "~/Products/Projects/ProjectsTemplates/TaskDescriptionTemplates.html",
                    "~/Products/Projects/ProjectsTemplates/SubtaskTemplates.html",
                    "~/Products/Projects/ProjectsTemplates/ListDiscussionsTemplates.html",
                    "~/Products/Projects/ProjectsTemplates/ActionPanelsTemplates.html",
                    "~/Products/Projects/ProjectsTemplates/PopupContentTemplates.html",
                    "~/Products/Projects/ProjectsTemplates/ReportTemplates.html",
                    "~/Products/Projects/ProjectsTemplates/ProjectAction.html",
                    "~/Products/Projects/ProjectsTemplates/SettingsTemplates.html",
                    "~/Products/CRM/Templates/SimpleContactListTemplate.html",
                    "~/Products/CRM/Templates/ContactSelectorTemplates.html",
                    "~/Products/CRM/Templates/ContactInfoCardTemplate.html"
                };
            }
        }
    }
}