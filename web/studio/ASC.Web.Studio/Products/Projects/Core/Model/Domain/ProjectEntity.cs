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

namespace ASC.Projects.Core.Domain
{
    ///<inherited>ASC.Projects.Core.Domain.DomainObject`1, ASC.Web.Projects</inherited>
    public abstract class ProjectEntity : DomainObject<int>
    {
        ///<example>Title</example>
        public string Title { get; set; }

        ///<example>Description</example>
        public string Description { get; set; }

        ///<example>HtmlTitle</example>
        public string HtmlTitle
        {
            get
            {
                if (Title == null) return string.Empty;
                return Title.Length <= 40 ? Title : Title.Remove(37) + "...";
            }
        }

        ///<type>ASC.Projects.Core.Domain.Project, ASC.Web.Projects</type>
        public Project Project { get; set; }

        ///<example>2fdfe577-3c26-4736-9df9-b5a683bb8520</example>
        public Guid CreateBy { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public DateTime CreateOn { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public DateTime LastModifiedOn { get; set; }

        ///<example>2fdfe577-3c26-4736-9df9-b5a683bb8520</example>
        public Guid LastModifiedBy { get; set; }

        ///<example>NotifyId</example>
        public string NotifyId
        {
            get { return string.Format("{0}_{1}", UniqID, Project.ID); }
        }

        public static string BuildUniqId<T>(int id)
        {
            return DoUniqId(typeof(T), id);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}|{1}|{2}", GetType().FullName, Title, Project.GetHashCode()).GetHashCode();
        }

        public abstract bool CanEdit();
    }
}