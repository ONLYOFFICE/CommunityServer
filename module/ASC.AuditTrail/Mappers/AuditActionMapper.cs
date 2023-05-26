/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.MessagingSystem;

namespace ASC.AuditTrail.Mappers
{
    public static class AuditActionMapper
    {
        public static List<IProductActionMapper> Mappers { get; }

        static AuditActionMapper()
        {
            Mappers = new List<IProductActionMapper>()
            {
                new CrmActionMapper(),
                new DocumentsActionMapper(),
                new LoginActionsMapper(),
                new OthersActionsMapper(),
                new PeopleActionMapper(),
                new ProjectsActionsMapper(),
                new SettingsActionsMapper()
            };
        }

        public static string GetActionText(this MessageMaps action, AuditEvent evt)
        {
            if (action == null)
            {
                //log.Error(string.Format("There is no action text for \"{0}\" type of event", action));
                return string.Empty;
            }

            try
            {
                var actionText = action.GetActionText();

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

        public static string GetActionText(this MessageMaps action, LoginEvent evt)
        {
            if (action == null)
            {
                //log.Error(string.Format("There is no action text for \"{0}\" type of event", action));
                return string.Empty;
            }

            try
            {
                var actionText = action.GetActionText();

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

        public static string GetActionTypeText(this MessageMaps action)
        {
            return action == null
                       ? string.Empty
                       : action.GetActionTypeText();
        }

        public static string GetProductText(this MessageMaps action)
        {
            return action == null
                       ? string.Empty
                       : action.GetProductText();
        }

        public static string GetModuleText(this MessageMaps action)
        {
            return action == null
                       ? string.Empty
                       : action.GetModuleText();
        }

        private static string ToLimitedText(string text)
        {
            if (text == null) return null;
            return text.Length < 50 ? text : string.Format("{0}...", text.Substring(0, 47));
        }

        public static MessageMaps GetMessageMaps(int actionInt)
        {
            var action = (MessageAction)actionInt;
            var mapper = Mappers.SelectMany(m => m.Mappers).FirstOrDefault(m => m.Actions.ContainsKey(action));
            if (mapper != null)
            {
                return mapper.Actions[action];
            }
            return null;
        }
    }
}