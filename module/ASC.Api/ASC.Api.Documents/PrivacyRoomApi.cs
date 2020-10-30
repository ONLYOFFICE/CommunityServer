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


using System.Collections.Generic;
using System.Web;
using ASC.Api.Attributes;
using ASC.Api.Interfaces;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Files.Core.Entries;
using ASC.Web.Studio.Core;
using Resources;

namespace ASC.Api.Documents
{
    public class PrivacyRoomApi : IApiEntryPoint
    {
        public string Name
        {
            get { return "privacyroom"; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <visible>false</visible>
        [Update("keys")]
        public object SetKeys(string publicKey, string privateKeyEnc)
        {
            SecurityContext.DemandPermissions(new UserSecurityProvider(SecurityContext.CurrentAccount.ID), Core.Users.Constants.Action_EditUser);

            if (!PrivacyRoomSettings.Enabled) throw new System.Security.SecurityException();

            var keyPair = EncryptionKeyPair.GetKeyPair();
            if (keyPair != null)
            {
                if (!string.IsNullOrEmpty(keyPair.PublicKey))
                {
                    return new { isset = true };
                }

                LogManager.GetLogger("ASC.Api.Documents").InfoFormat("User {0} updates address", SecurityContext.CurrentAccount.ID);
            }

            EncryptionKeyPair.SetKeyPair(publicKey, privateKeyEnc);

            return new
                {
                    isset = true
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <visible>false</visible>
        [Read("access/{fileId}")]
        public IEnumerable<EncryptionKeyPair> GetPublicKeysWithAccess(string fileId)
        {
            if (!PrivacyRoomSettings.Enabled) throw new System.Security.SecurityException();

            var fileKeyPair = EncryptionKeyPair.GetKeyPair(fileId);
            return fileKeyPair;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <visible>false</visible>
        [Read("")]
        public bool PrivacyRoom()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            return PrivacyRoomSettings.Enabled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        /// <visible>false</visible>
        [Update("")]
        public bool SetPrivacyRoom(bool enable)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (enable)
            {
                if (!PrivacyRoomSettings.Available)
                {
                    throw new BillingException(Resource.ErrorNotAllowedOption, "PrivacyRoom");
                }
            }

            PrivacyRoomSettings.Enabled = enable;

            MessageService.Send(HttpContext.Current.Request, enable ? MessageAction.PrivacyRoomEnable : MessageAction.PrivacyRoomDisable);

            return enable;
        }
    }
}