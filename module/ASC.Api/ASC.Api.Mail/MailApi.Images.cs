/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASC.Api.Attributes;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        ///    Returns list of all trusted addresses for image displaying.
        /// </summary>
        /// <returns>Addresses list. Email adresses represented as string name@domain.</returns>
        /// <short>Get trusted addresses</short> 
        /// <category>Images</category>
        [Read(@"display_images/addresses")]
        public IEnumerable<string> GetDisplayImagesAddresses()
        {
            return MailBoxManager.GetDisplayImagesAddresses(TenantId, Username);
        }

        ///  <summary>
        ///     Add the address to trusted addresses.
        ///  </summary>
        /// <param name="address">Address for adding. </param>
        /// <returns>Added address</returns>
        ///  <short>Add trusted address</short> 
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        ///  <category>Images</category>
        [Create(@"display_images/address")]
        public string AddDisplayImagesAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Invalid address. Address can't be empty.", "address");

            MailBoxManager.AddDisplayImagesAddress(TenantId, Username, address);
            return address;
        }

        ///  <summary>
        ///     Remove the address from trusted addresses.
        ///  </summary>
        /// <param name="address">Address for removing</param>
        /// <returns>Removed address</returns>
        ///  <short>Remove from trusted addresses</short> 
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        ///  <category>Images</category>
        [Delete(@"display_images/address")]
        public string RemovevDisplayImagesAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Invalid address. Address can't be empty.", "address");

            MailBoxManager.RemovevDisplayImagesAddress(TenantId, Username, address);
            return address;
        }

    }
}
