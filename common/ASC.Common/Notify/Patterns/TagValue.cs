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
using System.Diagnostics;

namespace ASC.Notify.Patterns
{
    [DebuggerDisplay("{Tag}: {Value}")]
    public class TagValue : ITagValue
    {
        public string Tag
        {
            get;
            private set;
        }

        public object Value
        {
            get;
            private set;
        }

        public TagValue(string tag, object value)
        {
            if (string.IsNullOrEmpty(tag)) throw new ArgumentNullException("tag");

            Tag = tag;
            Value = value;
        }
    }

    public class AdditionalSenderTag : TagValue
    {
        public AdditionalSenderTag(string senderName)
            : base("__AdditionalSender", senderName)
        {
        }
    }

    public class TagActionValue : ITagValue
    {
        private readonly Func<string> action;

        public string Tag
        {
            get;
            private set;
        }

        public object Value
        {
            get { return action(); }
        }

        public TagActionValue(string name, Func<string> action)
        {
            Tag = name;
            this.action = action;
        }
    }
}