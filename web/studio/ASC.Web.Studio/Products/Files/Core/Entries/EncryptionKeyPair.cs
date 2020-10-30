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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Studio.Core;
using Newtonsoft.Json;

namespace ASC.Web.Files.Core.Entries
{
    [DataContract(Name = "account", Namespace = "")]
    public class EncryptionKeyPair
    {
        [DataMember(Name = "privateKeyEnc", EmitDefaultValue = false)]
        public string PrivateKeyEnc;

        [DataMember(Name = "publicKey")]
        public string PublicKey;

        [DataMember(Name = "userId")]
        public Guid UserId;


        public static void SetKeyPair(string publicKey, string privateKeyEnc)
        {
            if (string.IsNullOrEmpty(publicKey)) throw new ArgumentNullException("publicKey");
            if (string.IsNullOrEmpty(privateKeyEnc)) throw new ArgumentNullException("privateKeyEnc");

            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            if (!SecurityContext.IsAuthenticated || user.IsVisitor()) throw new System.Security.SecurityException();

            var keyPair = new EncryptionKeyPair {
                PrivateKeyEnc = privateKeyEnc,
                PublicKey = publicKey,
                UserId = user.ID,
            };

            var keyPairString = JsonConvert.SerializeObject(keyPair);
            EncryptionLoginProvider.SetKeys(user.ID, keyPairString);
        }

        public static EncryptionKeyPair GetKeyPair()
        {
            var currentAddressString = EncryptionLoginProvider.GetKeys();
            if (string.IsNullOrEmpty(currentAddressString)) return null;

            var keyPair = JsonConvert.DeserializeObject<EncryptionKeyPair>(currentAddressString);
            if (keyPair.UserId != SecurityContext.CurrentAccount.ID) return null;
            return keyPair;
        }

        public static IEnumerable<EncryptionKeyPair> GetKeyPair(string fileId)
        {
            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                fileDao.InvalidateCache(fileId);

                var file = fileDao.GetFile(fileId);
                if (file == null) throw new System.IO.FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
                if (!Global.GetFilesSecurity().CanEdit(file)) throw new System.Security.SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
                if (file.RootFolderType != FolderType.Privacy) throw new NotSupportedException();
            }

            var fileShares = Global.FileStorageService.GetSharedInfo(new ItemList<string> { String.Format("file_{0}", fileId) }).ToList();
            fileShares = fileShares.Where(share => !share.SubjectGroup
                                            && !share.SubjectId.Equals(FileConstant.ShareLinkId)
                                            && share.Share == FileShare.ReadWrite).ToList();

            var fileKeysPair = fileShares.Select(share =>
                {
                    var fileKeyPairString = EncryptionLoginProvider.GetKeys(share.SubjectId);
                    if (string.IsNullOrEmpty(fileKeyPairString)) return null;
                    
                    var fileKeyPair = JsonConvert.DeserializeObject<EncryptionKeyPair>(fileKeyPairString);
                    if (fileKeyPair.UserId != share.SubjectId) return null;

                    fileKeyPair.PrivateKeyEnc = null;

                    return fileKeyPair;
                })
                .Where(keyPair => keyPair != null);

            return fileKeysPair;
        }
    }
}