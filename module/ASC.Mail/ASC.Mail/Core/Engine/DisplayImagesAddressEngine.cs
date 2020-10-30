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

namespace ASC.Mail.Core.Engine
{
    public class DisplayImagesAddressEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public DisplayImagesAddressEngine(int tenant, string user)
        {
            Tenant = tenant;
            User = user;
        }

        public IEnumerable<string> Get()
        {
            using (var daoFactory = new DaoFactory())
            {
                return daoFactory
                    .CreateDisplayImagesAddressDao(Tenant, User)
                    .GetDisplayImagesAddresses();
            }
        }

        public void Add(string address)
        {
            using (var daoFactory = new DaoFactory())
            {
                daoFactory
                    .CreateDisplayImagesAddressDao(Tenant, User)
                    .AddDisplayImagesAddress(address);
            }
        }

        public void Remove(string address)
        {
            using (var daoFactory = new DaoFactory())
            {
                daoFactory
                    .CreateDisplayImagesAddressDao(Tenant, User)
                    .RemovevDisplayImagesAddress(address);
            }
        }
    }
}
