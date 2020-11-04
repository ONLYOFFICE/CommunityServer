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

namespace ASC.Notify.Model
{
    [Serializable]
    public class NotifyAction : INotifyAction
    {
        public string ID { get; private set; }

        public string Name { get; private set; }


        public NotifyAction(string id)
            : this(id, null)
        {
        }

        public NotifyAction(string id, string name)
        {
            if (id == null) throw new ArgumentNullException("id");

            ID = id;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            var a = obj as INotifyAction;
            return a != null && a.ID == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("action: {0}", ID);
        }
    }
}