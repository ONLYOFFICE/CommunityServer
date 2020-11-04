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

namespace ASC.Blogs.Core.Domain
{
	public class Settings
    {
        public virtual Guid ID { get; set; }
        public virtual Guid UserID { get; set; }
        public virtual byte[] Data { get; set; }

        public override bool Equals(object obj)
        {
            Settings settings = obj as Settings;

            if (settings != null
                && settings.ID.Equals(this.ID)
                && settings.UserID.Equals(this.UserID))
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode() | this.UserID.GetHashCode();
        }
    }
}
