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