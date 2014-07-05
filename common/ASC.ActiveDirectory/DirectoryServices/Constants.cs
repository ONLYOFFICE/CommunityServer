/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;

namespace ASC.ActiveDirectory
{
    /// <summary>
    
    /// </summary>
    public sealed class Constants
    {
        public const int STANDART_LDAP_PORT = 389;
        public const int SSL_LDAP_PORT = 636;
        public const int LDAP_ERROR_INVALID_CREDENTIALS = 0x31;
        /// <summary>
        
        /// </summary>
        //[Flags]
        public enum AccountType : int
        {
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
//            SAM_NORMAL_USER_ACCOUNT = 0x30000000,
            /// <summary></summary>
            SAM_MACHINE_ACCOUNT = 0x30000001,
            /// <summary></summary>
            SAM_TRUST_ACCOUNT = 0x30000002,
            /// <summary></summary>
            SAM_APP_BASIC_GROUP = 0x40000000,
            /// <summary></summary>
            SAM_APP_QUERY_GROUP = 0x40000001 
        }

        /// <summary>
        
        /// </summary>
        [Flags]
        public enum UserAccauntControl : int
        {
            /// <summary>Zero flag</summary>
            Empty = 0x00000000,
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
            /// <summary>This is an account for users whose primary account is in another domain. This account provides user access to this domain, but not to any domain that trusts this domain. Also known as a local user account.</summary>
            ADS_UF_TEMP_DUPLICATE_ACCOUNT = 0x00000100,
            /// <summary>This is a default account type that represents a typical user.</summary>
            ADS_UF_NORMAL_ACCOUNT = 0x00000200,
            /// <summary>This is a computer account for a computer that is a member of this domain.</summary>
            ADS_UF_WORKSTATION_TRUST_ACCOUNT = 0x00001000,
            /// <summary>This is a computer account for a system backup domain controller that is a member of this domain.</summary>
            ADS_UF_SERVER_TRUST_ACCOUNT = 0x00002000,
            /// <summary>The password for this account will never expire.</summary>
            ADS_UF_DONT_EXPIRE_PASSWD = 0x00010000,
            /// <summary>The user must log on using a smart card.</summary>
            ADS_UF_SMARTCARD_REQUIRED = 0x00040000,
            /// <summary>The service account (user or computer account), under which a service runs, is trusted for Kerberos delegation. Any such service can impersonate a client requesting the service.</summary>
            ADS_UF_TRUSTED_FOR_DELEGATION = 0x00080000,
            /// <summary>The security context of the user will not be delegated to a service even if the service account is set as trusted for Kerberos delegation.</summary>
            ADS_UF_NOT_DELEGATED = 0x00100000,
            /// <summary>Restrict this principal to use only Data Encryption Standard (DES) encryption types for keys.</summary>
            ADS_UF_USE_DES_KEY_ONLY = 0x00200000,
            /// <summary>This account does not require Kerberos pre-authentication for logon.</summary>
            ADS_UF_DONT_REQUIRE_PREAUTH = 0x00400000,
            /// <summary>The user password has expired. This flag is created by the system using data from the Pwd-Last-Set attribute and the domain policy.</summary>
            ADS_UF_PASSWORD_EXPIRED = 0x00800000,
            /// <summary>The account is enabled for delegation. This is a security-sensitive setting; accounts with this option enabled should be strictly controlled. This setting enables a service running under the account to assume a client identity and authenticate as that user to other remote servers on the network.</summary>
            ADS_UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0x01000000
        }

        /// <summary>
        
        /// </summary>
        [Flags]
        public enum GroupType : uint
        {
            
            System = 0x00000001,
            
            GlobalScope = 0x00000002,
            
            LocalDomainScope  = 0x00000004,
            
            UniversalScope  = 0x00000008,
            /// <summary>Specifies an APP_BASIC group for Windows Server Authorization Manager.</summary>
            APP_BASIC = 0x000000010,
            /// <summary>Specifies an APP_QUERY group for Windows Server Authorization Manager.</summary>
            APP_QUERY = 0x000000020,
            
            SecurityGroup = 0x80000000

        }
        /// <summary>
        
        /// </summary>
        public static class ADSchemaAttributes
        {
            
            public const string Name = "name";    //RDN - Relative Distinguished Name 
            
            public const string CommonName = "cn"; //Common-Name
            
            public const string DisplayName = "displayName";    //Display-Name
            /// <summary>The list of classes from which this class is derived.</summary>
            public const string ObjectClass = "objectClass";    //Object-Class
            
            public const string ObjectCategory = "objectCategory";    //Object-Category
            /// <summary>The unique identifier for an object.</summary>
            public const string ObjectGUID = "objectGUID"; //Object-Guid
            
            public const string ShowInAdvancedViewOnly = "showInAdvancedViewOnly"; //Show-In-Advanced-View-Only
            
            public const string DistinguishedName = "distinguishedName"; //Obj-Dist-Name
            
            public const string IsCriticalSystemObject = "isCriticalSystemObject"; //Is-Critical-System-Object
            
            public const string NTSecurityDescriptor = "nTSecurityDescriptor"; //NT-Security-Descriptor
            
            public const string MemberOf = "memberOf"; //Is-Member-Of-DL
            
            public const string Member = "member";


            
            public const string OrganizationalUnitName = "ou"; //Organizational-Unit-Name
            
            
            public const string AccountName = "sAMAccountName"; //SAM-Account-Name
            
            public const string AccountType = "sAMAccountType"; //SAM-Account-Type
            /// <summary>A binary value that specifies the security identifier (SID) of the user. The SID is a unique value used to identify the user as a security principal.</summary>
            public const string ObjectSid = "objectSid"; //Object-Sid

            /// <summary>Flags that control the behavior of the user account.</summary>
            public const string UserAccountControl = "userAccountControl"; //User-Account-Control
            /// <summary>This attribute contains the UPN that is an Internet-style login name for a user based on the Internet standard RFC 822. The UPN is shorter than the distinguished name and easier to remember. By convention, this should map to the user e-mail name. The value set for this attribute is equal to the length of the user's ID and the domain name. For more information about this attribute, see the Naming Properties topic in the Active Directory guide.</summary>
            public const string UserPrincipalName = "userPrincipalName"; //User-Principal-Name
            /// <summary>Contains the given name (first name) of the user.</summary>
            public const string FirstName = "givenName";        //Given-Name
            /// <summary>This attribute contains the family or last name for a user.</summary>
            public const string Surname = "sn"; //Surname
            
            public const string PrimaryGroupID = "PrimaryGroupID"; //Primary-Group-ID

            /// <summary>Name of computer as registered in DNS</summary>
            public const string DNSHostName = "dNSHostName"; //DNS-Host-Name
            /// <summary>The Operating System Version string </summary>
            public const string OperatingSystemVersion = "operatingSystemVersion"; //Operating-System-Version
            /// <summary>The Operating System Service Pack ID String </summary>
            public const string OperatingSystemServicePack = "operatingSystemServicePack"; //Operating-System-Service-Pack
            /// <summary>The hotfix level of the operating system.</summary>
            public const string OperatingSystemHotfix = "operatingSystemHotfix"; //Operating-System-Hotfix
            /// <summary>The Operating System name .</summary>
            public const string OperatingSystem = "operatingSystem"; //Operating-System
            /// <summary>The TCP/IP address for a network segment. Also called the subnet address.</summary>
            public const string NetworkAddress = "networkAddress"; //Network-Address

            /// <summary>Mobile phone</summary>
            public const string Mobile = "mobile";
            /// <summary>Email address</summary>
            public const string Mail = "mail";
            /// <summary>Telephone number</summary>
            public const string TelephoneNumber = "telephoneNumber";
            /// <summary>Title</summary>
            public const string Title = "title";
            /// <summary>Street Address</summary>
            public const string Street = "street";
            /// <summary>Postal code</summary>
            public const string PostalCode = "postalCode";
            /// <summary>Home phone</summary>
            public const string HomePhone = "homePhone";
            /// <summary>Initials</summary>
            public const string Initials = "initials";
            /// <summary>Department</summary>
            public const string Division = "division";
            /// <summary>Company</summary>
            public const string Company = "company";
        }

        /// <summary>
        
        /// </summary>
        public static class ObjectClassKnowedValues
        {
            /// <summary>
            /// 
            /// </summary>
            public const string Top = "top";
            /// <summary>
            
            /// </summary>
            public const string Domain = "domain";
            /// <summary>
            
            /// </summary>
            public const string DomainDNS = "domainDNS";
            /// <summary>
            
            /// </summary>
            public const string Group = "group";
            /// <summary>
            
            /// </summary>
            public const string Person = "person";
            /// <summary>
            
            /// </summary>
            public const string Container = "container";
            /// <summary>
            /// 
            /// </summary>
            public const string OrganizationalUnit = "organizationalUnit";
            /// <summary>
            /// 
            /// </summary>
            public const string OrganizationalPerson = "organizationalPerson";
            /// <summary>
            
            /// </summary>
            public const string User = "user";
            /// <summary>
            
            /// </summary>
            public const string Computer = "computer";
            /// <summary>
            
            /// </summary>
            public const string RpcContainer = "rpcContainer";
            /// <summary>
            
            /// </summary>
            public const string BuildInDomain = "builtinDomain";
        }
    }
}
