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

namespace ASC.Common.Security.Authorizing
{
    public sealed class Constants
    {
        public static readonly Role Admin = new Role(new Guid("cd84e66b-b803-40fc-99f9-b2969a54a1de"), "Admin");

        public static readonly Role Everyone = new Role(new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), "Everyone");


        public static readonly Role User = new Role(new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), "User");

        public static readonly Role Visitor = new Role(new Guid("aced04fa-dd96-4b35-af3e-346bf1eb972d"), "Visitor");


        public static readonly Role Member = new Role(new Guid("ba74ca02-873f-43dc-8470-8620c156bc67"), "Member");

        public static readonly Role Owner = new Role(new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), "Owner");

        public static readonly Role Self = new Role(new Guid("5d5b7260-f7f7-49f1-a1c9-95fbb6a12604"), "Self");
    }
}