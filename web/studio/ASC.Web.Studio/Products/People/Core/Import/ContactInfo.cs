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
using System.Linq;
using System.Reflection;
using ASC.Web.People.Resources;

namespace ASC.Web.People.Core.Import
{
    public class ContactInfo
    {
        private string firstName;
        private string lastName;
        private string email;

        [Resource("FirstName")]
        public string FirstName
        {
            get { return (firstName ?? "").Trim(); }
            set { firstName = value; }
        }

        [Resource("LastName")]
        public string LastName
        {
            get { return (lastName ?? "").Trim(); }
            set { lastName = value; }
        }

        [Resource("Email")]
        public string Email
        {
            get { return (email ?? "").Trim(); }
            set { email = value; }
        }

        public static List<KeyValuePair<string, string>> GetColumns()
        {
            return typeof(ContactInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(r =>
                {
                    var attr = r.GetCustomAttribute<ResourceAttribute>();
                    return new KeyValuePair<string, string>(attr.Title,
                        PeopleResource.ResourceManager.GetString(attr.Title));
                }).ToList();
        }

        public override bool Equals(object obj)
        {
            try
            {
                if (obj is ContactInfo)
                {
                    var o = obj as ContactInfo;
                    return Email.Equals(o.Email);
                }
            }
            catch
            {
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Email.GetHashCode();
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ResourceAttribute : Attribute
    {
        public string Title { get; private set; }

        public ResourceAttribute(string title)
        {
            Title = title;
        }
    }
}