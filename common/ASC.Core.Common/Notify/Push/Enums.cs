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

namespace ASC.Core.Common.Notify.Push
{
    [DataContract]
    public enum PushAction
    {
        [EnumMember]
        Unknown,

        [EnumMember]
        Created,

        [EnumMember]
        Assigned,

        [EnumMember]
        InvitedTo,

        [EnumMember]
        Closed,

        [EnumMember]
        Resumed,

        [EnumMember]
        Deleted
    }

    [DataContract]
    public enum PushItemType
    {
        [EnumMember]
        Unknown,

        [EnumMember]
        Task,

        [EnumMember]
        Subtask,

        [EnumMember]
        Milestone,

        [EnumMember]
        Project,

        [EnumMember]
        Message
    }

    [DataContract]
    public enum PushModule
    {
        [EnumMember]
        Unknown,

        [EnumMember]
        Projects
    }

    [DataContract]
    public enum MobileAppType
    {
        [EnumMember]
        IosProjects = 0,
        
        [EnumMember]
        AndroidProjects = 1,

        [EnumMember]
        IosDocuments = 2,

        [EnumMember]
        AndroidDocuments = 3,

        [EnumMember]
        DesktopEditor = 4
    }
}
