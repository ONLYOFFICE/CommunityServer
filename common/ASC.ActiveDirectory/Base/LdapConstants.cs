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

namespace ASC.ActiveDirectory.Base
{
    /// <summary>
    /// Constants of Active Directory
    /// </summary>
    public sealed class LdapConstants
    {
        public const int STANDART_LDAP_PORT = 389;
        public const int SSL_LDAP_PORT = 636;
        public const int LDAP_ERROR_INVALID_CREDENTIALS = 0x31;
        public const int LDAP_V3 = 3;

        public const string OBJECT_FILTER = "(ObjectClass=*)";

        /// <summary>
        /// User Account type
        /// </summary>
        [Flags]
        public enum AccountType : uint
        {
            // ReSharper disable InconsistentNaming

            /// <summary></summary>
            SAM_DOMAIN_OBJECT = 0x00000000,

            /// <summary></summary>
            SAM_GROUP_OBJECT = 0x10000000,

            /// <summary></summary>
            SAM_NON_SECURITY_GROUP_OBJECT = 0x10000001,

            /// <summary></summary>
            SAM_ALIAS_OBJECT = 0x20000000,

            /// <summary></summary>
            SAM_NON_SECURITY_ALIAS_OBJECT = 0x20000001,

            /// <summary></summary>
            SAM_USER_OBJECT = 0x30000000,

            //SAM_NORMAL_USER_ACCOUNT = 0x30000000,

            /// <summary></summary>
            SAM_MACHINE_ACCOUNT = 0x30000001,

            /// <summary></summary>
            SAM_TRUST_ACCOUNT = 0x30000002,

            /// <summary></summary>
            SAM_APP_BASIC_GROUP = 0x40000000,

            /// <summary></summary>
            SAM_APP_QUERY_GROUP = 0x40000001

            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// User Account Control
        /// </summary>
        [Flags]
        public enum UserAccountControl : uint
        {
            // ReSharper disable InconsistentNaming

            /// <summary>Zero flag</summary>
            EMPTY = 0x00000000,

            /// <summary>The logon script is executed.</summary>
            ADS_UF_SCRIPT = 0x00000001,

            /// <summary>The user account is disabled.</summary>
            ADS_UF_ACCOUNTDISABLE = 0x00000002,

            /// <summary>The home directory is required.</summary>
            ADS_UF_HOMEDIR_REQUIRED = 0x00000008,

            /// <summary>The account is currently locked out.</summary>
            ADS_UF_LOCKOUT = 0x00000010,

            /// <summary>No password is required.</summary>
            ADS_UF_PASSWD_NOTREQD = 0x00000020,

            /// <summary>The user cannot change the password</summary>
            ADS_UF_PASSWD_CANT_CHANGE = 0x00000040,

            /// <summary>The user can send an encrypted password.</summary>
            ADS_UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0x00000080,

            /// <summary>This is an account for users whose primary account is in another domain. 
            /// This account provides user access to this domain, but not to any domain that trusts 
            /// this domain. Also known as a local user account.</summary>
            ADS_UF_TEMP_DUPLICATE_ACCOUNT = 0x00000100,

            /// <summary>This is a default account type that represents a typical user.</summary>
            ADS_UF_NORMAL_ACCOUNT = 0x00000200,

            /// <summary>This is a computer account for a computer that is a member of this domain.</summary>
            ADS_UF_WORKSTATION_TRUST_ACCOUNT = 0x00001000,

            /// <summary>This is a computer account for a system backup domain controller 
            /// that is a member of this domain.</summary>
            ADS_UF_SERVER_TRUST_ACCOUNT = 0x00002000,

            /// <summary>The password for this account will never expire.</summary>
            ADS_UF_DONT_EXPIRE_PASSWD = 0x00010000,

            /// <summary>The user must log on using a smart card.</summary>
            ADS_UF_SMARTCARD_REQUIRED = 0x00040000,

            /// <summary>The service account (user or computer account), under which a service runs, 
            /// is trusted for Kerberos delegation. Any such service can impersonate a client 
            /// requesting the service.</summary>
            ADS_UF_TRUSTED_FOR_DELEGATION = 0x00080000,

            /// <summary>The security context of the user will not be delegated to a service even 
            /// if the service account is set as trusted for Kerberos delegation.</summary>
            ADS_UF_NOT_DELEGATED = 0x00100000,

            /// <summary>Restrict this principal to use only Data Encryption Standard 
            /// (DES) encryption types for keys.</summary>
            ADS_UF_USE_DES_KEY_ONLY = 0x00200000,

            /// <summary>This account does not require Kerberos pre-authentication for logon.</summary>
            ADS_UF_DONT_REQUIRE_PREAUTH = 0x00400000,

            /// <summary>The user password has expired. This flag is created by the system 
            /// using data from the Pwd-Last-Set attribute and the domain policy.</summary>
            ADS_UF_PASSWORD_EXPIRED = 0x00800000,

            /// <summary>The account is enabled for delegation. This is a security-sensitive 
            /// setting; accounts with this option enabled should be strictly controlled. 
            /// This setting enables a service running under the account to assume a client 
            /// identity and authenticate as that user to other remote servers on the network.</summary>
            ADS_UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0x01000000

            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// Type of Group
        /// </summary>
        [Flags]
        public enum GroupType : uint
        {
            // ReSharper disable InconsistentNaming

            /// <summary>System group</summary>
            SYSTEM = 0x00000001,

            /// <summary>Global scope group</summary>
            GLOBAL_SCOPE = 0x00000002,

            /// <summary>Local domain scope group</summary>
            LOCAL_DOMAIN_SCOPE = 0x00000004,

            /// <summary>Universal scope group</summary>
            UNIVERSAL_SCOPE = 0x00000008,

            /// <summary>Specifies an APP_BASIC group for Windows Server Authorization Manager.</summary>
            APP_BASIC = 0x000000010,

            /// <summary>Specifies an APP_QUERY group for Windows Server Authorization Manager.</summary>
            APP_QUERY = 0x000000020,

            /// <summary>Security group</summary>
            SECURITY_GROUP = 0x80000000

            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// Schema attributes of Active Directory
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static class ADSchemaAttributes
        {
            /// <summary>Relative Distinguished Name </summary>
            public const string NAME = "name";

            /// <summary>Common-Name</summary>
            public const string COMMON_NAME = "cn";

            /// <summary>Display-Name</summary>
            public const string DISPLAY_NAME = "displayName";

            /// <summary>The list of classes from which this class is derived.</summary>
            public const string OBJECT_CLASS = "objectClass";

            /// <summary>DN category</summary>
            public const string OBJECT_CATEGORY = "objectCategory";

            /// <summary>The unique identifier for an object.</summary>
            public const string OBJECT_GUID = "objectGUID";

            /// <summary>Show-In-Advanced-View-Only</summary>
            public const string SHOW_IN_ADVANCED_VIEW_ONLY = "showInAdvancedViewOnly";

            /// <summary>Obj-Dist-Name</summary>
            public const string DISTINGUISHED_NAME = "distinguishedName";

            /// <summary>Is-Critical-System-Object</summary>
            public const string IS_CRITICAL_SYSTEM_OBJECT = "isCriticalSystemObject";

            /// <summary>NT-Security-Descriptor in format SDDL</summary>
            public const string NT_SECURITY_DESCRIPTOR = "nTSecurityDescriptor";

            /// <summary>Is-Member-Of-DL</summary>
            public const string MEMBER_OF = "memberOf";

            /// <summary>Users which are members of this object</summary>
            public const string MEMBER = "member";

            /// <summary>Organizational-Unit-Name</summary>
            public const string ORGANIZATIONAL_UNIT_NAME = "ou";

            /// <summary>Organization-Name</summary>
            public const string ORGANIZATION_NAME = "o";

            /// <summary>SAM-Account-Name</summary>
            public const string ACCOUNT_NAME = "sAMAccountName";

            /// <summary>SAM-Account-Type</summary>
            public const string ACCOUNT_TYPE = "sAMAccountType";

            /// <summary>A binary value that specifies the security identifier (SID) of the user. 
            /// The SID is a unique value used to identify the user as a security principal.</summary>
            public const string OBJECT_SID = "objectSid"; //Object-Sid

            /// <summary>Flags that control the behavior of the user account.</summary>
            public const string USER_ACCOUNT_CONTROL = "userAccountControl";

            /// <summary>This attribute contains the UPN that is an Internet-style login name 
            /// for a user based on the Internet standard RFC 822. The UPN is shorter than 
            /// the distinguished name and easier to remember. By convention, this should map 
            /// to the user e-mail name. The value set for this attribute is equal to the length 
            /// of the user's ID and the domain name. For more information about this attribute, 
            /// see the Naming Properties topic in the Active Directory guide.</summary>
            public const string USER_PRINCIPAL_NAME = "userPrincipalName";

            /// <summary>Contains the given name (first name) of the user.</summary>
            public const string FIRST_NAME = "givenName";

            /// <summary>This attribute contains the family or last name for a user.</summary>
            public const string SURNAME = "sn";

            /// <summary>Primary-Group-ID</summary>
            public const string PRIMARY_GROUP_ID = "primaryGroupID";

            /// <summary>Name of computer as registered in DNS</summary>
            public const string DNS_HOST_NAME = "dNSHostName";

            /// <summary>The Operating System Version string </summary>
            public const string OPERATING_SYSTEM_VERSION = "operatingSystemVersion";

            /// <summary>The Operating System Service Pack ID String </summary>
            public const string OPERATING_SYSTEM_SERVICE_PACK = "operatingSystemServicePack";

            /// <summary>The hotfix level of the operating system.</summary>
            public const string OPERATING_SYSTEM_HOTFIX = "operatingSystemHotfix";

            /// <summary>The Operating System name .</summary>
            public const string OPERATING_SYSTEM = "operatingSystem";

            /// <summary>The TCP/IP address for a network segment. Also called the subnet address.</summary>
            public const string NETWORK_ADDRESS = "networkAddress";

            /// <summary>Mobile phone</summary>
            public const string MOBILE = "mobile";

            /// <summary>Email address</summary>
            public const string MAIL = "mail";

            /// <summary>Telephone number</summary>
            public const string TELEPHONE_NUMBER = "telephoneNumber";

            /// <summary>Title</summary>
            public const string TITLE = "title";

            /// <summary>Street Address</summary>
            public const string STREET = "street";

            /// <summary>Postal code</summary>
            public const string POSTAL_CODE = "postalCode";

            /// <summary>Home phone</summary>
            public const string HOME_PHONE = "homePhone";

            /// <summary>Initials</summary>
            public const string INITIALS = "initials";

            /// <summary>Department</summary>
            public const string DIVISION = "division";

            /// <summary>Company</summary>
            public const string COMPANY = "company";
        }

        public static class RfcLDAPAttributes
        {
            public const string ENTRY_DN = "entryDN";
            public const string GUID = "GUID";
            public const string ENTRY_UUID = "entryUUID";
            public const string NS_UNIQUE_ID = "nsuniqueid";
            public const string UID = "uid";
            public const string MEMBER_UID = "memberUid";
            public const string DN = "dn";
        }

        /// <summary>
        /// Standart attributes of ObjectClass
        /// </summary>
        public static class ObjectClassKnowedValues
        {
            /// <summary>
            /// top value
            /// </summary>
            public const string TOP = "top";

            /// <summary>
            /// Domain name
            /// </summary>
            public const string DOMAIN = "domain";

            /// <summary>
            /// Domain DNS
            /// </summary>
            public const string DOMAIN_DNS = "domainDNS";

            /// <summary>
            /// Group name
            /// </summary>
            public const string GROUP = "group";

            /// <summary>
            /// posix-group
            /// </summary>
            public const string POSIX_GROUP = "posixGroup";

            /// <summary>
            /// Person
            /// </summary>
            public const string PERSON = "person";

            /// <summary>
            /// Container
            /// </summary>
            public const string CONTAINER = "container";

            /// <summary>
            /// Org unit
            /// </summary>
            public const string ORGANIZATIONAL_UNIT = "organizationalUnit";

            /// <summary>
            /// Org name
            /// </summary>
            public const string ORGANIZATION = "organization";

            /// <summary>
            /// posix-account
            /// </summary>
            public const string POSIX_ACCOUNT = "posixAccount";

            /// <summary>
            /// Org person
            /// </summary>
            public const string ORGANIZATIONAL_PERSON = "organizationalPerson";

            /// <summary>
            /// User
            /// </summary>
            public const string USER = "user";

            /// <summary>
            /// Computer
            /// </summary>
            public const string COMPUTER = "computer";

            /// <summary>
            /// RPC container
            /// </summary>
            public const string RPC_CONTAINER = "rpcContainer";

            /// <summary>
            /// Built in domain flag
            /// </summary>
            public const string BUILD_IN_DOMAIN = "builtinDomain";
        }
    }
}
