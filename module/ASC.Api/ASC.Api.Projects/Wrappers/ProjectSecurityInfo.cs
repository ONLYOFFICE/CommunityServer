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


using System.Runtime.Serialization;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "project_security", Namespace = "")]
    public class ProjectSecurityInfo : CommonSecurityInfo
    {
        [DataMember]
        public bool CanCreateMilestone { get; set; }

        [DataMember]
        public bool CanCreateMessage { get; set; }

        [DataMember]
        public bool CanCreateTask { get; set; }

        [DataMember]
        public bool CanCreateTimeSpend { get; set; }

        [DataMember]
        public bool CanEditTeam { get; set; }

        [DataMember]
        public bool CanReadFiles { get; set; }

        [DataMember]
        public bool CanReadMilestones { get; set; }

        [DataMember]
        public bool CanReadMessages { get; set; }

        [DataMember]
        public bool CanReadTasks { get; set; }

        [DataMember]
        public bool CanLinkContact { get; set; }

        [DataMember]
        public bool CanReadContacts { get; set; }

        [DataMember]
        public bool IsInTeam { get; set; }

        private ProjectSecurityInfo()
        {
        }

        public ProjectSecurityInfo(Project project)
        {
            CanCreateMilestone = ProjectSecurity.CanCreateMilestone(project);
            CanCreateMessage = ProjectSecurity.CanCreateMessage(project);
            CanCreateTask = ProjectSecurity.CanCreateTask(project);
            CanCreateTimeSpend = ProjectSecurity.CanCreateTimeSpend(project);

            CanEditTeam = ProjectSecurity.CanEditTeam(project);
            CanReadFiles = ProjectSecurity.CanReadFiles(project);
            CanReadMilestones = ProjectSecurity.CanReadMilestones(project);
            CanReadMessages = ProjectSecurity.CanReadMessages(project);
            CanReadTasks = ProjectSecurity.CanReadTasks(project);
            IsInTeam = ProjectSecurity.IsInTeam(project, SecurityContext.CurrentAccount.ID, false);
            CanLinkContact = ProjectSecurity.CanLinkContact(project);
            CanReadContacts = ProjectSecurity.CanReadContacts(project);
        }


        public static ProjectSecurityInfo GetSample()
        {
            return new ProjectSecurityInfo();
        }
    }
}