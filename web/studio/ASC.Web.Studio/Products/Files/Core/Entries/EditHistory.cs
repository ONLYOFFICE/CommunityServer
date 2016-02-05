/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
        //todo: delete
        [DataMember(Name = "version_group")] public int VersionGroupOld { get { return VersionGroup; } set { VersionGroup = value; } }

        [DataMember(Name = "user")] public EditHistoryAuthor ModifiedBy;

        public string ChangesString;

        [DataMember(Name = "changes")]
        public List<EditHistoryChanges> Changes
        {
            get
            {
                var changes = new List<EditHistoryChanges>();
                if (string.IsNullOrEmpty(ChangesString)) return changes;
                try
                {
                    var jChanges = JArray.Parse(ChangesString);

                    changes = jChanges.Children<JObject>()
                                      .Select(jChange =>
                                              new EditHistoryChanges
                                                  {
                                                      Date = jChange.Value<string>("date"),
                                                      Author = new EditHistoryAuthor
                                                          {
                                                              Id = new Guid(jChange.Value<string>("userid") ?? Guid.Empty.ToString()),
                                                              Name = jChange.Value<string>("username")
                                                          }
                                                  })
                                      .ToList();
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("DeSerialize exception", ex);
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
                return string.IsNullOrEmpty(_name)
                           ? Id.Equals(Guid.Empty) || Id.Equals(ASC.Core.Configuration.Constants.Guest.ID)
                                 ? FilesCommonResource.Guest
                                 : CoreContext.UserManager.GetUsers(Id).DisplayUserName(false)
                           : _name;
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
}