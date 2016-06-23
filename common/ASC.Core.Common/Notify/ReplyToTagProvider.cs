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


using System;
using System.Text.RegularExpressions;
using ASC.Notify.Patterns;

namespace ASC.Core.Common.Notify
{
    /// <summary>
    /// Class that generates 'mail to' addresses to create new TeamLab entities from post client 
    /// </summary>
    public static class ReplyToTagProvider
    {
        private static readonly Regex EntityType = new Regex(@"blog|forum.topic|event|photo|file|wiki|bookmark|project\.milestone|project\.task|project\.message");

        private const string TagName = "replyto";

        /// <summary>
        /// Creates 'replyto' tag that can be used to comment some TeamLab entity
        /// </summary>
        /// <param name="entity">Name of entity e.g. 'blog', 'project.task', etc.</param>
        /// <param name="entityId">Uniq id of the entity</param>
        /// <returns>New TeamLab tag</returns>
        public static TagValue Comment(string entity, string entityId)
        {
            return Comment(entity, entityId, null);
        }

        /// <summary>
        /// Creates 'replyto' tag that can be used to comment some TeamLab entity
        /// </summary>
        /// <param name="entity">Name of entity e.g. 'blog', 'project.task', etc.</param>
        /// <param name="entityId">Uniq id of the entity</param>
        /// <param name="parentId">Comment's parent comment id</param>
        /// <returns>New TeamLab tag</returns>
        public static TagValue Comment(string entity, string entityId, string parentId)
        {
            if (String.IsNullOrEmpty(entity) || !EntityType.Match(entity).Success) throw new ArgumentException(@"Not supported entity type", entity);
            if (String.IsNullOrEmpty(entityId)) throw new ArgumentException(@"Entity Id is null or empty", entityId);

            string pId = parentId != Guid.Empty.ToString() && parentId != null ? parentId : String.Empty;
            return new TagValue(TagName, String.Format("reply_{0}_{1}_{2}@{3}", entity, entityId, pId, AutoreplyDomain));
        }

        /// <summary>
        /// Creates 'replyto' tag that can be used to create TeamLab project message
        /// </summary>
        /// <param name="projectId">Id of the project to create message</param>
        /// <returns>New TeamLab tag</returns>
        public static TagValue Message(int projectId)
        {
            return new TagValue(TagName, String.Format("message_{0}@{1}", projectId, AutoreplyDomain));
        }

        private static string AutoreplyDomain
        {
            get
            {
                // we use mapped domains for standalone portals because it is the only way to reach autoreply service
                // mapped domains are no allowed for SAAS because of http(s) problem
                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                return tenant.GetTenantDomain(CoreContext.Configuration.Standalone);
            }
        }
    }
}
