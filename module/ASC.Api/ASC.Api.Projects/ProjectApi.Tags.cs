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
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Projects.Wrappers;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        ///<summary>
        ///Returns the list of all available project tags
        ///</summary>
        ///<short>
        ///Project tags
        ///</short>
        ///<category>Tags</category>
        ///<returns>List of tags</returns>
        [Read(@"tag")]
        public IEnumerable<ObjectWrapperBase> GetAllTags()
        {
            return EngineFactory.TagEngine.GetTags().Select(x => new ObjectWrapperBase {Id = x.Key, Title = x.Value});
        }

        ///<summary>
        ///Creates new tag
        ///</summary>
        ///<short>
        ///Tag
        ///</short>
        ///<category>Tags</category>
        ///<returns>Created tag</returns>
        [Create(@"tag")]
        public ObjectWrapperBase CreateNewTag(string data)
        {
            if (string.IsNullOrEmpty(data)) throw new ArgumentException("data");
            ProjectSecurity.DemandCreate<Project>(null);

            var result =  EngineFactory.TagEngine.Create(data);
            
            return new ObjectWrapperBase {Id = result.Key, Title = result.Value};
        }

        ///<summary>
        ///Returns the detailed list of all projects with the specified tag
        ///</summary>
        ///<short>
        ///Project by tag
        ///</short>
        ///<category>Tags</category>
        ///<param name="tag">Tag name</param>
        ///<returns>List of projects</returns>
        [Read(@"tag/{tag}")]
        public IEnumerable<ProjectWrapper> GetProjectsByTags(string tag)
        {
            var projectsTagged = EngineFactory.TagEngine.GetTagProjects(tag);
            return EngineFactory.ProjectEngine.GetByID(projectsTagged).Select(ProjectWrapperSelector).ToList();
        }


        ///<summary>
        ///Returns the list of all tags like the specified tag name
        ///</summary>
        ///<short>
        ///Tags by tag name
        ///</short>
        ///<category>Tags</category>
        ///<param name="tagName">Tag name</param>
        ///<returns>List of tags</returns>
        [Read(@"tag/search")]
        public string[] GetTagsByName(string tagName)
        {
            return !string.IsNullOrEmpty(tagName) && tagName.Trim() != string.Empty
                       ? EngineFactory.TagEngine.GetTags(tagName.Trim()).Select(r => r.Value).ToArray()
                       : new string[0];
        }
    }
}