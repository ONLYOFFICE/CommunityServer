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

namespace ASC.IPSecurity
{
    [Serializable]
    public class IPRestriction : IPRestrictionBase
    {
        ///<example type="int">123</example>
        public int Id { get; set; }

        ///<example type="int">1</example>
        public int TenantId { get; set; }
    }

    [Serializable]
    public class IPRestrictionBase
    {
        ///<example>Ip</example>
        public string Ip { get; set; }

        ///<example>false</example>
        public bool ForAdmin { get; set; }
    }
}