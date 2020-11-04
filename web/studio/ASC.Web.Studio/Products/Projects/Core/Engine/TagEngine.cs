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


using System.Collections.Generic;
using System.Linq;
using ASC.Projects.Core.DataInterfaces;

namespace ASC.Projects.Engine
{
    public class TagEngine
    {
        public IDaoFactory DaoFactory { get; set; }

        public KeyValuePair<int, string> Create(string data)
        {
            return DaoFactory.TagDao.Create(data);
        }

        public Dictionary<int, string> GetTags()
        {
            return DaoFactory.TagDao.GetTags();
        }

        public Dictionary<int, string> GetTags(string prefix)
        {
            return DaoFactory.TagDao.GetTags(prefix);
        }

        public string GetById(int id)
        {
            return DaoFactory.TagDao.GetById(id);
        }

        public int[] GetTagProjects(string tagName)
        {
            return DaoFactory.TagDao.GetTagProjects(tagName);
        }

        public int[] GetTagProjects(int tagID)
        {
            return DaoFactory.TagDao.GetTagProjects(tagID);
        }

        public Dictionary<int, string> GetProjectTags(int projectId)
        {
            return DaoFactory.TagDao.GetProjectTags(projectId);
        }

        public void SetProjectTags(int projectId, string tags)
        {
            DaoFactory.TagDao.SetProjectTags(projectId, FromString(tags));
        }

        public void SetProjectTags(int projectId, IEnumerable<int> tags)
        {
            DaoFactory.TagDao.SetProjectTags(projectId, tags);
        }

        private string[] FromString(string tags)
        {
            return (tags ?? string.Empty)
                .Split(',', ';')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToArray();
        }
    }
}
