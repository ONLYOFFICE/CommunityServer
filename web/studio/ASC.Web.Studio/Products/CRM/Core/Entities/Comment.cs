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
using System.Globalization;
using System.Linq;
using System.Text;
using ASC.Core.Users;

#endregion

namespace ASC.CRM.Core.Entities
{
    public class Comment
    {
        public Guid Parent { get; set; }

        public string Content { get; set; }

        public bool Inactive { get; set; }

        public String TargetUniqID { get; set; }

        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public override int GetHashCode()
        {
            return (GetType().FullName + "|" + Content + "|" + CreateBy.GetHashCode() + "|" + Parent.GetHashCode()).GetHashCode();
        }

    }
}