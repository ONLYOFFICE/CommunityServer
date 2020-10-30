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


using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ASC.Web.Files.Services.DocumentService
{
    [DataContract(Name = "docServiceParams", Namespace = "")]
    public class DocumentServiceParams
    {
        [DataMember(Name = "displayName")]
        public string DisplayName;

        [DataMember(Name = "docKeyForTrack")]
        public string DocKeyForTrack;

        [DataMember(Name = "editByUrl")]
        public bool EditByUrl;

        [DataMember(Name = "email")]
        public string Email;

        [DataMember(Name = "fileId", EmitDefaultValue = false)]
        public string FileId;

        [DataMember(Name = "fileProviderKey", EmitDefaultValue = false)]
        public string FileProviderKey;

        [DataMember(Name = "fileVersion", EmitDefaultValue = false)]
        public int FileVersion;

        [DataMember(Name = "linkToEdit")]
        public string LinkToEdit;

        [DataMember(Name = "openHistory", EmitDefaultValue = false)]
        public bool OpenHistory;

        [DataMember(Name = "openinigDate")]
        public string OpeninigDate;

        [DataMember(Name = "serverErrorMessage")]
        public string ServerErrorMessage;

        [DataMember(Name = "shareLinkParam")]
        public string ShareLinkParam;

        [DataMember(Name = "tabId")]
        public string TabId;

        [DataMember(Name = "thirdPartyApp")]
        public bool ThirdPartyApp;

        [DataMember(Name = "canGetUsers")]
        public bool CanGetUsers;

        [DataMember(Name = "pageTitlePostfix")]
        public string PageTitlePostfix;


        public static string Serialize(DocumentServiceParams docServiceParams)
        {
            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof (DocumentServiceParams));
                serializer.WriteObject(ms, docServiceParams);
                ms.Seek(0, SeekOrigin.Begin);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }
    }
}