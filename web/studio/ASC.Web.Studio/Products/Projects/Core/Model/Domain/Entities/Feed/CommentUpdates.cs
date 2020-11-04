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

namespace ASC.Projects.Core.Domain.Entities.Feed
{
    [DataContract(Name = "comment")]
    public class ProjectComment
    {
        public Comment Comment { get; set; }
        
        public int CommentedId { get; set; }
        
        public string CommentedTitle { get; set; }
        
        public int ProjectId { get; set; }

        public Message Discussion { get; set; }
        
        public Task Task { get; set; }

        public EntityType CommentedType { get; set; }
    }
}
