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
            return MailEngineFactory.DisplayImagesAddressEngine.Get();
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
            MailEngineFactory.DisplayImagesAddressEngine.Add(address);

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
            MailEngineFactory.DisplayImagesAddressEngine.Remove(address);

            return address;
        }
    }
}
