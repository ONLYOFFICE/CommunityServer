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


using System;
using System.Collections.Generic;

using ASC.Api.Attributes;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Returns a list of all the trusted addresses for image displaying.
        /// </summary>
        /// <returns>List of addresses. Email adresses are represented as strings in the name@domain format</returns>
        /// <short>Get trusted image addresses</short> 
        /// <category>Images</category>
        /// <path>api/2.0/mail/display_images/addresses</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"display_images/addresses")]
        public IEnumerable<string> GetDisplayImagesAddresses()
        {
            return MailEngineFactory.DisplayImagesAddressEngine.Get();
        }

        ///  <summary>
        ///  Adds an image address specified in the request to the list of trusted image addresses.
        ///  </summary>
        /// <param type="System.String, System" name="address">Image address</param>
        /// <returns>Added image address</returns>
        /// <short>Add the trusted image address</short> 
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        /// <category>Images</category>
        ///  <path>api/2.0/mail/display_images/address</path>
        ///  <httpMethod>POST</httpMethod>
        [Create(@"display_images/address")]
        public string AddDisplayImagesAddress(string address)
        {
            MailEngineFactory.DisplayImagesAddressEngine.Add(address);

            return address;
        }

        ///  <summary>
        ///  Removes an image address specified in the request from the list of trusted image addresses.
        ///  </summary>
        /// <param type="System.String, System" name="address">Image address</param>
        /// <returns>Removed image address</returns>
        ///  <short>Remove the trusted image address</short> 
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        ///  <category>Images</category>
        ///  <path>api/2.0/mail/display_images/address</path>
        ///  <httpMethod>DELETE</httpMethod>
        [Delete(@"display_images/address")]
        public string RemovevDisplayImagesAddress(string address)
        {
            MailEngineFactory.DisplayImagesAddressEngine.Remove(address);

            return address;
        }
    }
}
