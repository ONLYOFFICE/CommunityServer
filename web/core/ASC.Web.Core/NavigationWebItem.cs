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

using ASC.Web.Core.WebZones;

namespace ASC.Web.Core
{
    [WebZoneAttribute(WebZoneType.Nowhere)]
    public class NavigationWebItem : IWebItem
    {
        public virtual Guid ID { get; set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public virtual string StartURL { get; set; }

        public virtual string HelpURL { get; set; }

        public virtual string WarmupURL { get; set; }

        public virtual string ProductClassName { get; set; }

        public bool Visible { get { return true; } }

        public virtual WebItemContext Context { get; set; }


        public override bool Equals(object obj)
        {
            var m = obj as IWebItem;
            return m != null && ID == m.ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
