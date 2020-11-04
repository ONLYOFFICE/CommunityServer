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


#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

#endregion

namespace ASC.CRM.Core
{
    [DataContract]
    [Serializable]
    public class DomainObject
    {
     
        [DataMember(Name = "id")]
        public virtual int ID
        {
            get;
            set;
        }

        public override int GetHashCode()
        {
            return (GetType().FullName + "|" + ID.GetHashCode()).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as DomainObject;
            return compareTo != null && (
                (!IsTransient() && !compareTo.IsTransient() && ID.Equals(compareTo.ID)) ||
                ((IsTransient() || compareTo.IsTransient()) && GetHashCode().Equals(compareTo.GetHashCode())));
        }

        private bool IsTransient()
        {
            return ID.Equals(default(int));
        }
        
    }
}
