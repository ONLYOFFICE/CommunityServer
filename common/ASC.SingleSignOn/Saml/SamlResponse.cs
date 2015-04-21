/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using ASC.SingleSignOn.Common;
using log4net;
using System;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace ASC.SingleSignOn.Saml
{
    public class SamlResponse
    {
        private XmlDocument xmlDoc;
        private SsoSettings _ssoSettings;
        private Certificate _certificate;
        private XmlNamespaceManager manager;
        private readonly static ILog _log = LogManager.GetLogger(typeof(SamlResponse));

        public SamlResponse(SsoSettings ssoSettings)
        {
            _ssoSettings = ssoSettings;
            _certificate = new Certificate();
            _certificate.LoadCertificate(_ssoSettings.PublicKey);
        }

        public void LoadXml(string xml)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.XmlResolver = null;
            xmlDoc.LoadXml(xml);

            manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            _log.DebugFormat("SamlResponse: {0}", xmlDoc.InnerXml);
        }

        public void LoadXmlFromBase64(string response)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            LoadXml(enc.GetString(Convert.FromBase64String(response)));
        }

        public bool IsValid()
        {
            _log.Debug("Checking Saml response.");
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            XmlNodeList nodeList = xmlDoc.SelectNodes("//ds:Signature", manager);

            //SignedXml signedXml = new SignedXml(xmlDoc);
            SamlSignedXml signedXml = new SamlSignedXml(xmlDoc);

            if (nodeList == null)
            {
                _log.Error("Certificate signature not found.");
                return false;
            }
            foreach (XmlNode node in nodeList)
            {
                signedXml.LoadXml((XmlElement)node);
                if (!signedXml.CheckSignature(_certificate.cert.PublicKey.Key))
                {
                    _log.Error("Certificate validaiton failed.");
                    return false;
                }
            }
            return IsValidEmail(GetNameID());
        }

        public string GetNameID()
        {
            XmlNode node = xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:NameID", manager);
            if (node == null)
            {
                node = xmlDoc.SelectSingleNode(
                    "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='NameID']/saml:AttributeValue", manager);
            }
            if (node == null)
            {
                node = xmlDoc.SelectSingleNode(
                    "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='email']/saml:AttributeValue", manager);
            }
            if (node == null)
            {
                node = xmlDoc.SelectSingleNode(
                    "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='" + ClaimTypes.Email + "']/saml:AttributeValue", manager);
            }
            return node != null ? node.InnerText : null;
        }

        public string GetFirstName()
        {
            XmlNode node = xmlDoc.SelectSingleNode(
                "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='first_name']/saml:AttributeValue", manager);
            if (node == null)
            {
                node = xmlDoc.SelectSingleNode(
                    "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='" + ClaimTypes.GivenName + "']/saml:AttributeValue", manager);
            }
            return node != null ? node.InnerText : null;
        }

        public string GetLastName()
        {
            XmlNode node = xmlDoc.SelectSingleNode(
                "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='last_name']/saml:AttributeValue", manager);
            if (node == null)
            {
                node = xmlDoc.SelectSingleNode(
                    "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='" + ClaimTypes.Surname + "']/saml:AttributeValue", manager);
            }
            return node != null ? node.InnerText : null;
        }

        public string GetExtUserId()
        {
            XmlNode node = xmlDoc.SelectSingleNode(
                "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='user_id']/saml:AttributeValue", manager);
            return node != null ? node.InnerText : null;
        }

        public string GetMobilePhone()
        {
            XmlNode node = xmlDoc.SelectSingleNode(
                "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='mobile_phone']/saml:AttributeValue", manager);
            if (node == null)
            {
                node = xmlDoc.SelectSingleNode(
                    "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='" + ClaimTypes.MobilePhone + "']/saml:AttributeValue", manager);
            }
            return node != null ? node.InnerText : null;
        }

        public string GetTitle()
        {
            XmlNode node = xmlDoc.SelectSingleNode(
                "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='title']/saml:AttributeValue", manager);
            return node != null ? node.InnerText : null;
        }

        public string GetStreetAddress()
        {
            XmlNode node = xmlDoc.SelectSingleNode(
                "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='streetaddress']/saml:AttributeValue", manager);
            if (node == null)
            {
                node = xmlDoc.SelectSingleNode(
                    "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='" + ClaimTypes.Locality + "']/saml:AttributeValue", manager);
            }
            return node != null ? node.InnerText : null;
        }

        public string GetBirthDate()
        {
            XmlNode node = xmlDoc.SelectSingleNode(
                "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='birthdate']/saml:AttributeValue", manager);
            if (node == null)
            {
                node = xmlDoc.SelectSingleNode(
                    "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='" + ClaimTypes.DateOfBirth + "']/saml:AttributeValue", manager);
            }
            return node != null ? node.InnerText : null;
        }

        public string GetRemotePhotoUrl()
        {
            XmlNode node = xmlDoc.SelectSingleNode(
                "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='remote_photo_url']/saml:AttributeValue", manager);
            return node != null ? node.InnerText : null;
        }

        public string GetSex()
        {
            XmlNode node = xmlDoc.SelectSingleNode(
                "/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='sex']/saml:AttributeValue", manager);
            return node != null ? node.InnerText : null;
        }

        public string GetIssuer()
        {
            XmlNode node = xmlDoc.SelectSingleNode("/samlp:Response/saml:Issuer", manager);
            return node != null ? node.InnerText : null;
        }

        private bool IsValidEmail(string email)
        {
            if (email == null)
            {
                _log.Error("No mandatory parameter: email");
                return false;
            }
            try
            {
                new MailAddress(email);
                return true;
            }
            catch
            {
                _log.ErrorFormat("Wrong email format: {0}", email);
                return false;
            }
        }
    }
}