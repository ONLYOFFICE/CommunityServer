/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using ASC.MessagingSystem;
using System.Linq;
using log4net;

namespace ASC.AuditTrail.Mappers
{
    public class AuditActionMapper
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Messaging");

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
                                     .Select(split => string.Join(", ", split.Select(ToLimitedText))).ToList();


                return string.Format(actionText, description.ToArray());
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
                                     .Select(split => string.Join(", ", split.Select(ToLimitedText))).ToList();

                return string.Format(actionText, description.ToArray());
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