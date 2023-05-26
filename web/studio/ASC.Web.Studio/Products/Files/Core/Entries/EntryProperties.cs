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


using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Core.Users;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ASC.Files.Core
{
    [Serializable]
    [DataContract(Name = "file_properties", Namespace = "")]
    [DebuggerDisplay("")]
    public class EntryProperties
    {
        #region Property

        [DataMember(EmitDefaultValue = false)]
        public FormFillingProperties FormFilling { get; set; }

        #endregion

        #region Nested Classes

        [DataContract(Name = "document", Namespace = "")]
        public class FormFillingProperties
        {
            [DataMember]
            public bool CollectFillForm { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string ToFolderId { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string ToFolderPath { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string CreateFolderTitle { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string CreateFileMask { get; set; }

            public void FixFileMask()
            {
                if (string.IsNullOrEmpty(CreateFileMask)) return;

                var indFileName = CreateFileMask.IndexOf("{0}");
                CreateFileMask = CreateFileMask.Replace("{0}", "");

                var indUserName = CreateFileMask.IndexOf("{1}");
                CreateFileMask = CreateFileMask.Replace("{1}", "");

                var indDate = CreateFileMask.IndexOf("{2}");
                CreateFileMask = CreateFileMask.Replace("{2}", "");

                CreateFileMask = "_" + CreateFileMask + "_";
                CreateFileMask = Global.ReplaceInvalidCharsAndTruncate(CreateFileMask);
                CreateFileMask = CreateFileMask.Substring(1, CreateFileMask.Length - 2);

                if (indDate >= 0) CreateFileMask = CreateFileMask.Insert(indDate, "{2}");
                if (indUserName >= 0) CreateFileMask = CreateFileMask.Insert(indUserName, "{1}");
                if (indFileName >= 0) CreateFileMask = CreateFileMask.Insert(indFileName, "{0}");
            }

            public static readonly string DefaultTitleMask = "{0} - {1} ({2})";

            public string GetTitleByMask(string sourceFileName)
            {
                FixFileMask();

                var mask = CreateFileMask;
                if (string.IsNullOrEmpty(mask))
                {
                    mask = DefaultTitleMask;
                }

                string userName;
                var userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                if (userInfo.Equals(Constants.LostUser))
                {
                    userName = CustomNamingPeople.Substitute<FilesCommonResource>("ProfileRemoved");
                }
                else
                {
                    userName = userInfo.DisplayUserName(false);
                }

                var title = mask
                    .Replace("{0}", Path.GetFileNameWithoutExtension(sourceFileName))
                    .Replace("{1}", userName)
                    .Replace("{2}", TenantUtil.DateTimeNow().ToString("g"));

                if (FileUtility.GetFileExtension(title) != "docx")
                {
                    title += ".docx";
                }

                return title;
            }
        }

        #endregion

        public static EntryProperties Parse(string data)
        {
            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                {
                    var properties = new DataContractJsonSerializer(typeof(EntryProperties)).ReadObject(stream);
                    return (EntryProperties)properties;
                }
            }
            catch (Exception e)
            {
                Global.Logger.Error("Error parse EntryProperties: " + data, e);
                return null;
            }
        }

        public static string Serialize(EntryProperties entryProperties)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    var serializer = new DataContractJsonSerializer(typeof(EntryProperties));
                    serializer.WriteObject(ms, entryProperties);
                    ms.Seek(0, SeekOrigin.Begin);
                    return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                }
            }
            catch (Exception e)
            {
                Global.Logger.Error("Error serialize EntryProperties", e);
                return null;
            }
        }
    }
}