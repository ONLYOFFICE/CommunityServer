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
