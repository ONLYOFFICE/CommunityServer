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
using System.Linq;
using System.Web;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Helpers;
using ASC.Web.CRM.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Core.Users;

namespace ASC.Web.Projects.Masters.ClientScripts
{
    public class ClientLocalizationResources : ClientScriptLocalization
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Projects.Resources"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return new List<KeyValuePair<string, object>>(10)
            {
                RegisterResourceSet("ProjectsJSResource", ProjectsJSResource.ResourceManager),
                RegisterResourceSet("ProjectsFilterResource", ProjectsFilterResource.ResourceManager),
                RegisterResourceSet("ImportResource", ImportResource.ResourceManager),
                RegisterResourceSet("TasksResource", TaskResource.ResourceManager),
                RegisterResourceSet("CommonResource", ProjectsCommonResource.ResourceManager),
                RegisterResourceSet("TimeTrackingResource", TimeTrackingResource.ResourceManager),
                RegisterResourceSet("MessageResource", MessageResource.ResourceManager),
                RegisterResourceSet("ProjectResource", ProjectResource.ResourceManager),
                RegisterResourceSet("MilestoneResource", MilestoneResource.ResourceManager),
                RegisterResourceSet("ProjectTemplatesResource", ProjectTemplatesResource.ResourceManager),
                RegisterResourceSet("ProjectsFileResource", ProjectsFileResource.ResourceManager),
                RegisterResourceSet("ReportResource", ReportResource.ResourceManager),
                RegisterObject(
                new
                    {
                        ViewByDepartments = CustomNamingPeople.Substitute<ReportResource>("ViewByDepartments").HtmlEncode(),
                        ViewByUsers = CustomNamingPeople.Substitute<ReportResource>("ViewByUsers").HtmlEncode(),
                        AllDepartments = CustomNamingPeople.Substitute<ProjectsCommonResource>("AllDepartments").HtmlEncode(),
                        AllUsers = CustomNamingPeople.Substitute<ProjectsCommonResource>("AllUsers").HtmlEncode(),
                        PaymentStatus = new
                        {
                            NotChargeable = ResourceEnumConverter.ConvertToString(PaymentStatus.NotChargeable),
                            NotBilled = ResourceEnumConverter.ConvertToString(PaymentStatus.NotBilled),
                            Billed = ResourceEnumConverter.ConvertToString(PaymentStatus.Billed)
                        },
                        GrammaticalResource.DayGenitiveSingular,
                        GrammaticalResource.MonthNominative,
                        GrammaticalResource.MonthGenitiveSingular,
                        GrammaticalResource.MonthGenitivePlural,
                        ProjectStatus = new[]
                        {
                            new {id = ProjectStatus.Open, title = ProjectsJSResource.StatusOpenProject},
                            new {id = ProjectStatus.Paused, title = ProjectsJSResource.StatusSuspendProject},
                            new {id = ProjectStatus.Closed, title = ProjectsJSResource.StatusClosedProject}
                        },
                        StartModules = new[]
                        {
                            StartModule.TaskModule,
                            StartModule.ProjectsModule,
                            StartModule.DiscussionModule,
                            StartModule.TimeTrackingModule
                        }.Select(r=> new {r.Page, r.StartModuleType, Title = r.Title()}),
                        Statuses = CustomTaskStatus.GetDefaults().Select(r=> new
                        {
                            id = r.Id,
                            image = r.Image,
                            imageType = r.ImageType,
                            title = r.Title,
                            description = r.Description,
                            color = r.Color,
                            statusType = r.StatusType,
                            isDefault = r.IsDefault,
                            available = r.Available,
                            canChangeAvailable = r.CanChangeAvailable
                        })
                    })
            };
        }
    }

    public class CRMDataResources : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return new List<KeyValuePair<string, object>>(1)
                   {
                       RegisterObject(new
                       {
                            SmallSizePhotoCompany = ContactPhotoManager.GetSmallSizePhoto(0, true),
                            SmallSizePhoto = ContactPhotoManager.GetSmallSizePhoto(0, false),
                            MediumSizePhotoCompany = ContactPhotoManager.GetMediumSizePhoto(0, true),
                            MediumSizePhoto = ContactPhotoManager.GetMediumSizePhoto(0, false),
                       })
                   };
        }
    }
}