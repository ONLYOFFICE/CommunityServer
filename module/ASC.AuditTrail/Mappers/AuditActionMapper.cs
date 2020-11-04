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
using ASC.MessagingSystem;
using System.Linq;

namespace ASC.AuditTrail.Mappers
{
    public class AuditActionMapper
    {
        private static readonly Dictionary<MessageAction, MessageMaps> actions;

        static AuditActionMapper()
        {
            actions = new Dictionary<MessageAction, MessageMaps>();

            actions = actions
                .Union(LoginActionsMapper.GetMaps())
                .Union(ProjectsActionsMapper.GetMaps())
                .Union(CrmActionMapper.GetMaps())
                .Union(PeopleActionMapper.GetMaps())
                .Union(DocumentsActionMapper.GetMaps())
                .Union(SettingsActionsMapper.GetMaps())
                .Union(OthersActionsMapper.GetMaps())
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public static string GetActionText(AuditEvent evt)
        {
            var action = (MessageAction)evt.Action;
            if (!actions.ContainsKey(action))
            {
                //log.Error(string.Format("There is no action text for \"{0}\" type of event", action));
                return string.Empty;
            }

            try
            {
                var actionText = actions[(MessageAction)evt.Action].GetActionText();

                if (evt.Description == null || !evt.Description.Any()) return actionText;

                var description = evt.Description
                                     .Select(t => t.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                                     .Select(split => string.Join(", ", split.Select(ToLimitedText))).ToArray();


                return string.Format(actionText, description);
            }
            catch
            {
                //log.Error(string.Format("Error while building action text for \"{0}\" type of event", action));
                return string.Empty;
            }
        }

        public static string GetActionText(LoginEvent evt)
        {
            var action = (MessageAction)evt.Action;
            if (!actions.ContainsKey(action))
            {
                //log.Error(string.Format("There is no action text for \"{0}\" type of event", action));
                return string.Empty;
            }

            try
            {
                var actionText = actions[(MessageAction)evt.Action].GetActionText();

                if (evt.Description == null || !evt.Description.Any()) return actionText;

                var description = evt.Description
                                     .Select(t => t.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                     .Select(split => string.Join(", ", split.Select(ToLimitedText))).ToArray();

                return string.Format(actionText, description);
            }
            catch
            {
                //log.Error(string.Format("Error while building action text for \"{0}\" type of event", action));
                return string.Empty;
            }
        }

        public static string GetActionTypeText(AuditEvent evt)
        {
            var action = (MessageAction)evt.Action;
            return !actions.ContainsKey(action)
                       ? string.Empty
                       : actions[(MessageAction)evt.Action].GetActionTypeText();
        }

        public static string GetProductText(AuditEvent evt)
        {
            var action = (MessageAction)evt.Action;
            return !actions.ContainsKey(action)
                       ? string.Empty
                       : actions[(MessageAction)evt.Action].GetProduct();
        }

        public static string GetModuleText(AuditEvent evt)
        {
            var action = (MessageAction)evt.Action;
            return !actions.ContainsKey(action)
                       ? string.Empty
                       : actions[(MessageAction)evt.Action].GetModule();
        }

        private static string ToLimitedText(string text)
        {
            if (text == null) return null;
            return text.Length < 50 ? text : string.Format("{0}...", text.Substring(0, 47));
        }
    }
}