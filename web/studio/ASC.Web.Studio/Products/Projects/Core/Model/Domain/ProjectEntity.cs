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

namespace ASC.Projects.Core.Domain
{
    public abstract class ProjectEntity : DomainObject<int>
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string HtmlTitle
        {
            get
            {
                if (Title == null) return string.Empty;
                return Title.Length <= 40 ? Title : Title.Remove(37) + "...";
            }
        }

        public Project Project { get; set; }

        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public DateTime LastModifiedOn { get; set; }

        public Guid LastModifiedBy { get; set; }

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