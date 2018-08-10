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


using System;
using System.Collections.Generic;
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
                        }
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