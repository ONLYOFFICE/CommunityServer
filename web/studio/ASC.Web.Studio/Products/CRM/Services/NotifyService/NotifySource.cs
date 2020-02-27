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


#region Import

using System;
using System.Reflection;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using NotifySourceBase = ASC.Core.Notify.NotifySource;

#endregion

namespace ASC.Web.CRM.Services.NotifyService
{

    public class NotifySource : NotifySourceBase
    {
        public static NotifySource Instance
        {
            get;
            private set;
        }

        static NotifySource()
        {
            Instance = new NotifySource();
        }

        public NotifySource()
            : base(new Guid("{13FF36FB-0272-4887-B416-74F52B0D0B02}"))
        {

        }

        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(
                NotifyConstants.Event_ResponsibleForTask,
                NotifyConstants.Event_ResponsibleForOpportunity,
                NotifyConstants.Event_AddRelationshipEvent,
                NotifyConstants.Event_TaskReminder,
                NotifyConstants.Event_SetAccess,
                NotifyConstants.Event_ExportCompleted,
                NotifyConstants.Event_ExportCompletedCustomMode,
                NotifyConstants.Event_ImportCompleted,
                NotifyConstants.Event_ImportCompletedCustomMode,
                NotifyConstants.Event_CreateNewContact);
        }

        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider2(CRMPatternResource.patterns);
        }
    }
}