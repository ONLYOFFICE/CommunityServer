/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;

namespace ASC.SingleSignOn.Common
{
    [Serializable]
    [DataContract]
    public class SsoSettingsV2 : ISettings
    {
        public Guid ID
        {
            get { return new Guid("{1500187F-B8AB-406F-97B8-04BFE8261DBE}"); }
        }

        public ISettings GetDefault()
        {
            return new SsoSettingsV2
                {
                    EnableSso = false,

                    IdpSettings = new SsoIdpSettings
                        {
                            EntityId = string.Empty,
                            SsoUrl = string.Empty,
                            SsoBinding = SsoBindingType.Saml20HttpPost,
                            SloUrl = string.Empty,
                            SloBinding = SsoBindingType.Saml20HttpPost,
                            NameIdFormat = SsoNameIdFormatType.Saml20Transient
                        },

                    IdpCertificates = new List<SsoCertificate>(),
                    IdpCertificateAdvanced = new SsoIdpCertificateAdvanced
                        {
                            DecryptAlgorithm = SsoEncryptAlgorithmType.AES_128,
                            DecryptAssertions = false,
                            VerifyAlgorithm = SsoSigningAlgorithmType.RSA_SHA1,
                            VerifyAuthResponsesSign = false,
                            VerifyLogoutRequestsSign = false,
                            VerifyLogoutResponsesSign = false
                        },

                    SpCertificates = new List<SsoCertificate>(),
                    SpCertificateAdvanced = new SsoSpCertificateAdvanced
                        {
                            EncryptAlgorithm = SsoEncryptAlgorithmType.AES_128,
                            EncryptAssertions = false,
                            SigningAlgorithm = SsoSigningAlgorithmType.RSA_SHA1,
                            SignAuthRequests = false,
                            SignLogoutRequests = false,
                            SignLogoutResponses = false
                        },

                    FieldMapping = new SsoFieldMapping
                        {
                            FirstName = "givenName",
                            LastName = "sn",
                            Email = "mail",
                            Title = "title",
                            Location = "l",
                            Phone = "mobile"
                        }
                };
        }

        [DataMember]
        public bool EnableSso { get; set; }

        [DataMember]
        public SsoIdpSettings IdpSettings { get; set; }

        [DataMember]
        public List<SsoCertificate> IdpCertificates { get; set; }

        [DataMember]
        public SsoIdpCertificateAdvanced IdpCertificateAdvanced { get; set; }

        [DataMember]
        public List<SsoCertificate> SpCertificates { get; set; }

        [DataMember]
        public SsoSpCertificateAdvanced SpCertificateAdvanced { get; set; }

        [DataMember]
        public SsoFieldMapping FieldMapping { get; set; }

    }


    #region SpSettings

    [Serializable]
    [DataContract]
    public class SsoIdpSettings
    {
        [DataMember]
        public string EntityId { get; set; }

        [DataMember]
        public string SsoUrl { get; set; }

        [DataMember]
        public string SsoBinding { get; set; }

        [DataMember]
        public string SloUrl { get; set; }

        [DataMember]
        public string SloBinding { get; set; }

        [DataMember]
        public string NameIdFormat { get; set; }
    }

    #endregion


    #region FieldsMapping

    [Serializable]
    [DataContract]
    public class SsoFieldMapping
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Location { get; set; }

        [DataMember]
        public string Phone { get; set; }
    }

    #endregion


    #region Certificates

    [Serializable]
    [DataContract]
    public class SsoCertificate
    {
        [DataMember]
        public bool SelfSigned { get; set; }

        [DataMember]
        public string Crt { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Action { get; set; }

        [DataMember]
        public string DomainName { get; set; }

        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime ExpiredDate { get; set; }
    }

    [Serializable]
    [DataContract]
    public class SsoIdpCertificateAdvanced
    {
        [DataMember]
        public string VerifyAlgorithm { get; set; }

        [DataMember]
        public bool VerifyAuthResponsesSign { get; set; }

        [DataMember]
        public bool VerifyLogoutRequestsSign { get; set; }

        [DataMember]
        public bool VerifyLogoutResponsesSign { get; set; }

        [DataMember]
        public string DecryptAlgorithm { get; set; }

        [DataMember]
        public bool DecryptAssertions { get; set; }
    }

    [Serializable]
    [DataContract]
    public class SsoSpCertificateAdvanced
    {
        [DataMember]
        public string SigningAlgorithm { get; set; }

        [DataMember]
        public bool SignAuthRequests { get; set; }

        [DataMember]
        public bool SignLogoutRequests { get; set; }

        [DataMember]
        public bool SignLogoutResponses { get; set; }

        [DataMember]
        public string EncryptAlgorithm { get; set; }

        [DataMember]
        public bool EncryptAssertions { get; set; }
    }

    #endregion


    #region Types

    [Serializable]
    [DataContract]
    public class SsoNameIdFormatType
    {
        [DataMember]
        public const string Saml11Unspecified = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified";

        [DataMember]
        public const string Saml11EmailAddress = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";

        [DataMember]
        public const string Saml20Entity = "urn:oasis:names:tc:SAML:2.0:nameid-format:entity";

        [DataMember]
        public const string Saml20Transient = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient";

        [DataMember]
        public const string Saml20Persistent = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent";

        [DataMember]
        public const string Saml20Encrypted = "urn:oasis:names:tc:SAML:2.0:nameid-format:encrypted";

        [DataMember]
        public const string Saml20Unspecified = "urn:oasis:names:tc:SAML:2.0:nameid-format:unspecified";

        [DataMember]
        public const string Saml11X509SubjectName = "urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName";

        [DataMember]
        public const string Saml11WindowsDomainQualifiedName = "urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName";

        [DataMember]
        public const string Saml20Kerberos = "urn:oasis:names:tc:SAML:2.0:nameid-format:kerberos";
    }

    [Serializable]
    [DataContract]
    public class SsoBindingType
    {
        [DataMember]
        public const string Saml20HttpPost = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST";

        [DataMember]
        public const string Saml20HttpRedirect = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect";
    }

    [Serializable]
    [DataContract]
    public class SsoSigningAlgorithmType
    {
        [DataMember]
        public const string RSA_SHA1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

        [DataMember]
        public const string RSA_SHA256 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        [DataMember]
        public const string RSA_SHA512 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";
    }

    [Serializable]
    [DataContract]
    public class SsoEncryptAlgorithmType
    {
        [DataMember]
        public const string AES_128 = "http://www.w3.org/2001/04/xmlenc#aes128-cbc";

        [DataMember]
        public const string AES_256 = "http://www.w3.org/2001/04/xmlenc#aes256-cbc";

        [DataMember]
        public const string TRI_DEC = "http://www.w3.org/2001/04/xmlenc#tripledes-cbc";
    }

    [Serializable]
    [DataContract]
    public class SsoSpCertificateActionType
    {
        [DataMember]
        public const string Signing = "signing";

        [DataMember]
        public const string Encrypt = "encrypt";

        [DataMember]
        public const string SigningAndEncrypt = "signing and encrypt";
    }

    [Serializable]
    [DataContract]
    public class SsoIdpCertificateActionType
    {
        [DataMember]
        public const string Verification = "verification";

        [DataMember]
        public const string Decrypt = "decrypt";

        [DataMember]
        public const string VerificationAndDecrypt = "verification and decrypt";
    }

    #endregion
}