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

namespace ASC.Projects.Core.Domain
{
    public abstract class DomainObject<TID> where TID : struct
    {
        public abstract EntityType EntityType { get; }

        public TID ID { get; set; }

        public virtual string UniqID
        {
            get { return DoUniqId(GetType(), ID); }
        }

        public virtual string ItemPath
        {
            get { return ""; }
        }

        internal static string DoUniqId(Type type, TID id)
        {
            return string.Format("{0}_{1}", type.Name, id);
        }

        public override int GetHashCode()
        {
            return (GetType().FullName + "|" + ID.GetHashCode()).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as DomainObject<TID>;
            return compareTo != null && (
                (!IsTransient() && !compareTo.IsTransient() && ID.Equals(compareTo.ID)) ||
                ((IsTransient() || compareTo.IsTransient()) && GetHashCode().Equals(compareTo.GetHashCode())));
        }

        private bool IsTransient()
        {
            return ID.Equals(default(TID));
        }
    }
}
