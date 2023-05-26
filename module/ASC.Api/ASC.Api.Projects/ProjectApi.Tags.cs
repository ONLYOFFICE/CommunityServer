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

using ASC.Api.Attributes;
using ASC.Api.Projects.Wrappers;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        /// <summary>
        /// Returns a list of all the available project tags.
        /// </summary>
        /// <short>
        /// Get project tags
        /// </short>
        /// <category>Tags</category>
        /// <returns type="ASC.Api.Projects.Wrappers.ObjectWrapperBase, ASC.Api.Projects">List of tags</returns>
        /// <path>api/2.0/project/tag</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"tag")]
        public IEnumerable<ObjectWrapperBase> GetAllTags()
        {
            return EngineFactory.TagEngine.GetTags().Select(x => new ObjectWrapperBase { Id = x.Key, Title = x.Value });
        }

        /// <summary>
        /// Creates a new tag with the data specified in the request.
        /// </summary>
        /// <short>
        /// Create a tag
        /// </short>
        /// <category>Tags</category>
        /// <returns type="ASC.Api.Projects.Wrappers.ObjectWrapperBase, ASC.Api.Projects">Created tag</returns>
        /// <path>api/2.0/project/tag</path>
        /// <httpMethod>POST</httpMethod>
        /// <param type="System.String, System" name="data">Tag data</param>
        [Create(@"tag")]
        public ObjectWrapperBase CreateNewTag(string data)
        {
            if (string.IsNullOrEmpty(data)) throw new ArgumentException("data");
            ProjectSecurity.DemandCreate<Project>(null);

            var result = EngineFactory.TagEngine.Create(data);

            return new ObjectWrapperBase { Id = result.Key, Title = result.Value };
        }

        /// <summary>
        /// Returns the detailed list of all the projects with a tag specified in the request.
        /// </summary>
        /// <short>
        /// Get projects by a tag
        /// </short>
        /// <category>Tags</category>
        /// <param type="System.String, System" method="url" name="tag">Tag name</param>
        /// <returns>List of projects</returns>
        /// <path type="ASC.Api.Projects.Wrappers.ProjectWrapper, ASC.Api.Projects">api/2.0/project/tag/{tag}</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"tag/{tag}")]
        public IEnumerable<ProjectWrapper> GetProjectsByTags(string tag)
        {
            var projectsTagged = EngineFactory.TagEngine.GetTagProjects(tag);
            return EngineFactory.ProjectEngine.GetByID(projectsTagged.ToList()).Select(ProjectWrapperSelector).ToList();
        }


        /// <summary>
        /// Returns a list of all the tags by the tag name specified in the request.
        /// </summary>
        /// <short>
        /// Get tags by a tag name
        /// </short>
        /// <category>Tags</category>
        /// <param type="System.String, System" name="tagName">Tag name</param>
        /// <returns>List of tags</returns>
        /// <path>api/2.0/project/tag/search</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"tag/search")]
        public IEnumerable<ObjectWrapperBase> GetTagsByName(string tagName)
        {
            tagName = (tagName ?? "").Trim();

            if (string.IsNullOrEmpty(tagName)) return new List<ObjectWrapperBase>();

            return EngineFactory.TagEngine.GetTags(tagName).Select(x => new ObjectWrapperBase { Id = x.Key, Title = x.Value });
        }
    }
}