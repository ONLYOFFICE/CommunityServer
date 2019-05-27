/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Interfaces;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Files.Core.Entries;
using ASC.Web.Studio.Core;
using Newtonsoft.Json;
using ASC.Common.Logging;

namespace ASC.Api.Documents
{
    public class BlockchainApi : IApiEntryPoint
    {
        public string Name
        {
            get { return "blockchain"; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <visible>false</visible>
        [Update("data")]
        public object UpdateData(string address, string publicKey)
        {
            SecurityContext.DemandPermissions(new UserSecurityProvider(SecurityContext.CurrentAccount.ID), Core.Users.Constants.Action_EditUser);
            var currentAddressString = BlockchainLoginProvider.GetAddress();
            if (!string.IsNullOrEmpty(currentAddressString))
            {
                var currentAddress = JsonConvert.DeserializeObject<BlockchainAddress>(currentAddressString);
                if (currentAddress.PublicKey.Equals(publicKey)) return new { isset = true };

                LogManager.GetLogger("ASC.Api.Documents").InfoFormat("User {0} updates address", SecurityContext.CurrentAccount.ID);
            }

            var account = new BlockchainAddress { Address = address, PublicKey = publicKey };
            var accountString = JsonConvert.SerializeObject(account);
            BlockchainLoginProvider.UpdateData(accountString);

            return new
                {
                    isset = !string.IsNullOrEmpty(BlockchainLoginProvider.GetAddress())
                };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <visible>false</visible>
        [Read("access/{fileId}")]
        public IEnumerable<BlockchainAddress> GetAccessAddresses(string fileId)
        {
            var accountsString = BlockchainAddress.GetAddress(fileId);

            var accounts = accountsString.Select(JsonConvert.DeserializeObject<BlockchainAddress>);
            return accounts;
        }
    }
}