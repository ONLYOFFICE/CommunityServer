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


using System.Runtime.Serialization;

using ASC.Forum;
using ASC.Specific;

namespace ASC.Api.Forums
{
    [DataContract(Name = "post", Namespace = "")]
    public class ForumTopicPostAttachmentWrapper : IApiSortableDate
    {
        ///<example>2020-12-07T13:56:02.3269198Z</example>
        ///<order>3</order>
        [DataMember(Order = 3)]
        public ApiDateTime Created { get; set; }

        ///<example>2020-12-07T13:56:02.3269198Z</example>
        ///<order>3</order>
        [DataMember(Order = 3)]
        public ApiDateTime Updated { get; set; }

        ///<example>picture.jpg</example>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public string Name { get; set; }

        ///<example>image/jpeg</example>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public string ContentType { get; set; }

        ///<example type="int">122345</example>
        ///<order>3</order>
        [DataMember(Order = 3)]
        public int Size { get; set; }

        ///<example>url to file</example>
        ///<order>5</order>
        [DataMember(Order = 5)]
        public string Path { get; set; }

        public ForumTopicPostAttachmentWrapper(Attachment attachment)
        {
            ContentType = attachment.ContentType.ToString();
            Updated = Created = (ApiDateTime)attachment.CreateDate;

            Name = attachment.Name;
            Size = attachment.Size;
            Path = attachment.OffsetPhysicalPath;//TODO: add through datastorage
        }

        private ForumTopicPostAttachmentWrapper()
        {
        }

        public static ForumTopicPostAttachmentWrapper GetSample()
        {
            return new ForumTopicPostAttachmentWrapper()
            {
                ContentType = "image/jpeg",
                Created = ApiDateTime.GetSample(),
                Updated = ApiDateTime.GetSample(),
                Name = "picture.jpg",
                Path = "url to file",
                Size = 122345
            };
        }
    }
}