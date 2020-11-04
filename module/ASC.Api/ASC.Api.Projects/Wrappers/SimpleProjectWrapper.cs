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


using System.Runtime.Serialization;
using ASC.Projects.Core.Domain;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "project", Namespace = "")]
    public class SimpleProjectWrapper
    {
        [DataMember(Order = 60)]
        public int Id { get; set; }

        [DataMember(Order = 61)]
        public string Title { get; set; }

        [DataMember(Order = 62)]
        public int Status { get; set; }

        [DataMember(Order = 63)]
        public bool IsPrivate { get; set; }

        public SimpleProjectWrapper()
        {
        }

        public SimpleProjectWrapper(Project project)
        {
            Id = project.ID;
            Title = project.Title;
            Status = (int)project.Status;
            IsPrivate = project.Private;
        }

        public SimpleProjectWrapper(int projectId, string projectTitle)
        {
            Id = projectId;
            Title = projectTitle;
        }

        public static SimpleProjectWrapper GetSample()
        {
            return new SimpleProjectWrapper(123, "Sample project");
        }
    }
}