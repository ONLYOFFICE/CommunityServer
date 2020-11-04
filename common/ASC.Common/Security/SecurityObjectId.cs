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
using ASC.Common.Security.Authorizing;

namespace ASC.Common.Security
{
    [DebuggerDisplay("ObjectType: {ObjectType.Name}, SecurityId: {SecurityId}")]
    public class SecurityObjectId : ISecurityObjectId
    {
        public object SecurityId { get; private set; }

        public Type ObjectType { get; private set; }


        public SecurityObjectId(object id, Type objType)
        {
            if (objType == null) throw new ArgumentNullException("objType");

            SecurityId = id;
            ObjectType = objType;
        }

        public override int GetHashCode()
        {
            return AzObjectIdHelper.GetFullObjectId(this).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as SecurityObjectId;
            return other != null &&
                   Equals(AzObjectIdHelper.GetFullObjectId(other), AzObjectIdHelper.GetFullObjectId(this));
        }
    }
}