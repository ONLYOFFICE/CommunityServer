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


namespace ASC.ActiveDirectory.Base.Settings
{
    public enum LdapSettingsStatus
    {
        Ok = 0,
        WrongServerOrPort = 1,
        WrongUserDn = 2,
        IncorrectLDAPFilter = 3,
        UsersNotFound = 4,
        WrongLoginAttribute = 5,
        WrongGroupDn = 6,
        IncorrectGroupLDAPFilter = 7,
        GroupsNotFound = 8,
        WrongGroupAttribute = 9,
        WrongUserAttribute = 10,
        WrongGroupNameAttribute = 11,
        CredentialsNotValid = 12,
        ConnectError = 13,
        StrongAuthRequired = 14,
        WrongSidAttribute = 15,
        CertificateRequest = 16,
        TlsNotSupported = 17,
        DomainNotFound = 18
    }
}
