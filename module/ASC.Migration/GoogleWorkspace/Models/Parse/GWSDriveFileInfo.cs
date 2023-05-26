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

using Newtonsoft.Json;

namespace ASC.Migration.GoogleWorkspace.Models.Parse
{
    public class GwsDriveFileInfo
    {
        [JsonProperty("starred")]
        public bool Starred { get; set; }

        [JsonProperty("viewers_can_download")]
        public bool ViewersCanDownload { get; set; }

        [JsonProperty("editors_can_edit_access")]
        public bool EditorsCanEditAccess { get; set; }

        [JsonProperty("last_modified_by_any_user")]
        public DateTimeOffset LastModifiedByAnyUser { get; set; }

        [JsonProperty("last_modified_by_me")]
        public DateTimeOffset LastModifiedByMe { get; set; }

        [JsonProperty("content_last_modified")]
        public DateTimeOffset ContentLastModified { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        [JsonProperty("permissions")]
        public List<GwsDriveFilePermission> Permissions { get; set; }
    }

    public class GwsDriveFilePermission
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("additional_roles")]
        public List<string> AdditionalRoles { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("self_link")]
        public Uri SelfLink { get; set; }

        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }
    }
}
