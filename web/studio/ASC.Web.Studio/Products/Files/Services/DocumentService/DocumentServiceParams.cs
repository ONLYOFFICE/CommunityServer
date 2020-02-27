/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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