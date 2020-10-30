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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using Newtonsoft.Json.Linq;

namespace ASC.Files.Core
{
    [DataContract(Name = "editHistory", Namespace = "")]
    [DebuggerDisplay("{ID} v{Version}")]
    public class EditHistory
    {
        public int ID;
        [DataMember(Name = "key")] public string Key;
        [DataMember(Name = "version")] public int Version;
        [DataMember(Name = "versionGroup")] public int VersionGroup;

        [DataMember(Name = "user")] public EditHistoryAuthor ModifiedBy;

        [DataMember(Name = "changeshistory", EmitDefaultValue = false)] public string ChangesString;

        [DataMember(Name = "changes", EmitDefaultValue = false)]
        public List<EditHistoryChanges> Changes
        {
            get
            {
                var changes = new List<EditHistoryChanges>();
                if (string.IsNullOrEmpty(ChangesString)) return changes;

                try
                {
                    var jObject = JObject.Parse(ChangesString);
                    ServerVersion = jObject.Value<string>("serverVersion");

                    if (string.IsNullOrEmpty(ServerVersion))
                        return changes;

                    var jChanges = jObject.Value<JArray>("changes");

                    changes = jChanges.Children()
                                      .Select(jChange =>
                                          {
                                              var jUser = jChange.Value<JObject>("user");
                                              return new EditHistoryChanges
                                                  {
                                                      Date = jChange.Value<string>("created"),
                                                      Author = new EditHistoryAuthor
                                                          {
                                                              Id = new Guid(jUser.Value<string>("id") ?? Guid.Empty.ToString()),
                                                              Name = jUser.Value<string>("name"),
                                                          },
                                                  };
                                          })
                                      .ToList();
                    return changes;
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("DeSerialize old scheme exception", ex);
                }

                return changes;
            }
            set { throw new NotImplementedException(); }
        }

        public DateTime ModifiedOn;

        [DataMember(Name = "created")]
        public string ModifiedOnString
        {
            get { return ModifiedOn.Equals(default(DateTime)) ? null : ModifiedOn.ToString("g"); }
            set { throw new NotImplementedException(); }
        }

        [DataMember(Name = "serverVersion", EmitDefaultValue = false)] public string ServerVersion;
    }

    [DataContract(Name = "user", Namespace = "")]
    [DebuggerDisplay("{Id} {Name}")]
    public class EditHistoryAuthor
    {
        [DataMember(Name = "id")] public Guid Id;

        private string _name;

        [DataMember(Name = "name")]
        public string Name
        {
            get
            {
                UserInfo user;
                return
                    Id.Equals(Guid.Empty)
                    || Id.Equals(ASC.Core.Configuration.Constants.Guest.ID)
                    || (user = CoreContext.UserManager.GetUsers(Id)).Equals(Constants.LostUser)
                        ? string.IsNullOrEmpty(_name)
                              ? FilesCommonResource.Guest
                              : _name
                        : user.DisplayUserName(false);
            }
            set { _name = value; }
        }
    }

    [DataContract(Name = "change", Namespace = "")]
    [DebuggerDisplay("{Author.Name}")]
    public class EditHistoryChanges
    {
        [DataMember(Name = "user")] public EditHistoryAuthor Author;

        private DateTime _date;

        [DataMember(Name = "created")]
        public string Date
        {
            get { return _date.Equals(default(DateTime)) ? null : _date.ToString("g"); }
            set
            {
                if (DateTime.TryParse(value, out _date))
                {
                    _date = TenantUtil.DateTimeFromUtc(_date);
                }
            }
        }
    }

    [DataContract(Name = "data")]
    [DebuggerDisplay("{Version}")]
    public class EditHistoryData
    {
        [DataMember(Name = "changesUrl", EmitDefaultValue = false)] public string ChangesUrl;

        [DataMember(Name = "key")] public string Key;

        [DataMember(Name = "previous", EmitDefaultValue = false)] public EditHistoryUrl Previous;

        [DataMember(Name = "token", EmitDefaultValue = false)] public string Token;

        [DataMember(Name = "url")] public string Url;

        [DataMember(Name = "version")] public int Version;
    }

    [DataContract(Name = "url")]
    [DebuggerDisplay("{Key} - {Url}")]
    public class EditHistoryUrl
    {
        [DataMember(Name = "key")] public string Key;

        [DataMember(Name = "url")] public string Url;
    }
}