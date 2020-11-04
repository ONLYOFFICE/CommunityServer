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
    [Serializable]
    public class Action : IAction
    {
        public Guid ID { get; private set; }

        public string Name { get; private set; }

        public bool AdministratorAlwaysAllow { get; private set; }

        public bool Conjunction { get; private set; }


        public Action(Guid id, string name)
            : this(id, name, true, true)
        {
        }

        public Action(Guid id, string name, bool administratorAlwaysAllow, bool conjunction)
        {
            if (id == Guid.Empty) throw new ArgumentNullException("id");

            ID = id;
            Name = name;
            AdministratorAlwaysAllow = administratorAlwaysAllow;
            Conjunction = conjunction;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var a = obj as Action;
            return a != null && a.ID == ID;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
