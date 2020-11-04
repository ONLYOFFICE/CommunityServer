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


namespace ASC.Web.Studio.ThirdParty.ImportContacts
{
    public class ContactInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

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
}