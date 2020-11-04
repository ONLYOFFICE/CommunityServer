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


#region usings

using System;
using System.Collections.Generic;

#endregion

namespace ASC.Notify.Recipients
{
    [Serializable]
    public class DirectRecipient
        : IDirectRecipient
    {
        private readonly List<string> _Addresses = new List<string>();

        public DirectRecipient(string id, string name)
        {
            ID = id;
            Name = name;
        }

        public DirectRecipient(string id, string name, string[] addresses)
            : this(id, name, addresses, true)
        {
        }

        public DirectRecipient(string id, string name, string[] addresses, bool checkActivation)
        {
            ID = id;
            Name = name;
            CheckActivation = checkActivation;
            if (addresses != null)
                _Addresses.AddRange(addresses);
        }

        #region IDirectRecipient

        public string[] Addresses
        {
            get { return _Addresses.ToArray(); }
        }



        #endregion

        #region IRecipient

        public string ID { get; private set; }

        public string Name { get; private set; }
        public bool CheckActivation { get; set; }

        #endregion

        public override bool Equals(object obj)
        {
            var recD = obj as IDirectRecipient;
            if (recD == null) return false;
            return Equals(recD.ID, ID);
        }

        public override int GetHashCode()
        {
            return (ID ?? "").GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{0}({1})", Name, String.Join(";", _Addresses.ToArray()));
        }
    }
}