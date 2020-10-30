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

namespace ASC.ActiveDirectory.Base.Data
{
    /// <summary>
    /// LDAP object class
    /// </summary>
    public abstract class LdapObject
    {
        #region .Public

        public abstract string DistinguishedName { get; }

        public abstract string Sid { get; }

        public abstract string SidAttribute { get; }

        public abstract bool IsDisabled { get; }

        #endregion

        /// <summary>
        /// Get property object
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <returns>value object</returns>
        public abstract object GetValue(string propertyName, bool getBytes = false);

        /// <summary>
        /// Get property values
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <returns>list of values</returns>
        public abstract List<string> GetValues(string propertyName);
    }
}
