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
using System.Collections.Generic;

namespace ASC.ActiveDirectory.ComplexOperations.Data
{
    public enum LdapChangeType
    {
        User,
        Group
    }

    public enum LdapItemChangeKey
    {
        Sid,
        Name,
        FirstName,
        LastName,
        Mail,
        Phone,
        Title,
        Location,
        Member
    }

    public enum LdapChangeAction
    {
        None,
        Skip,
        Add,
        AddMember,
        Merge,
        Update,
        Remove,
        RemoveMember,
        SaveAsPortal
    }

    public static class LdapUserMapping
    {
        public static readonly List<string> Fields = new List<string>
        {
            Enum.GetName(typeof(LdapItemChangeKey), LdapItemChangeKey.FirstName),
            Enum.GetName(typeof(LdapItemChangeKey), LdapItemChangeKey.LastName),
            Enum.GetName(typeof(LdapItemChangeKey), LdapItemChangeKey.Mail),
            Enum.GetName(typeof(LdapItemChangeKey), LdapItemChangeKey.Phone),
            Enum.GetName(typeof(LdapItemChangeKey), LdapItemChangeKey.Title),
            Enum.GetName(typeof(LdapItemChangeKey), LdapItemChangeKey.Location),
            Enum.GetName(typeof(LdapItemChangeKey), LdapItemChangeKey.Sid),
            Enum.GetName(typeof(LdapItemChangeKey), LdapItemChangeKey.Member)
        };
    }
}