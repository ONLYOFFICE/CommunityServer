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


using ASC.Core;
using ASC.SingleSignOn.Common;
using log4net;
using System;
using System.IdentityModel.Tokens;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml;

namespace ASC.SingleSignOn.Saml
{
    public class SamlRequest
    {
        private string _id;
        private string _issueInstant;
        private SsoSettings _ssoSettings;
        private readonly static ILog _log = LogManager.GetLogger(typeof(SamlResponse));

        public SamlRequest(SsoSettings ssoSettings)
        {
            _ssoSettings = ssoSettings;
            _id = "_" + Guid.NewGuid().ToString();
            _issueInstant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        public string GetRequest(SamlRequestFormat format, string assertionConsumerServiceUrl, string certPath, string certPassword)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;

                using (XmlWriter xw = XmlWriter.Create(sw, xws))
                {
                    xw.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("ID", _id);
                    xw.WriteAttributeString("Version", "2.0");
                    xw.WriteAttributeString("IssueInstant", _issueInstant);
                    xw.WriteAttributeString("Destination", _ssoSettings.SsoEndPoint);
                    xw.WriteAttributeString("AssertionConsumerServiceURL", assertionConsumerServiceUrl);

                    // for ADFS
                    xw.WriteAttributeString("Consent", "urn:oasis:names:tc:SAML:2.0:consent:unspecified");
                    xw.WriteStartElement("Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString("www.sp.com");
                    xw.WriteEndElement();

                    // for ADFS
                    xw.WriteStartElement("Conditions", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteStartElement("AudienceRestriction");
                    xw.WriteStartElement("Audience");
                    xw.WriteString(assertionConsumerServiceUrl);
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();

                    xw.WriteEndElement();
                }

                _log.DebugFormat("Format: {0}, SAML Request: {1}", format, sw.ToString());
                if (format == SamlRequestFormat.Base64)
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(sw.ToString());
                    AsymmetricAlgorithm privateKey;
                    try
                    {
                        _log.DebugFormat("Using certificate for signing saml requests. certPath={0}", certPath);
                        X509Certificate2 cert = new X509Certificate2(certPath, certPassword);
                        privateKey = cert.PrivateKey;
                    }
                    catch (CryptographicException ex)
                    {
                        _log.DebugFormat("Using SAML requests without certificate. {0}", ex);
                        privateKey = null;
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorFormat("Certification error. {0}", ex);
                        throw new Exception("Certification error");
                    }
                    return CreateQueryString(xdoc.DocumentElement, privateKey);
                }
                _log.ErrorFormat("Unknown format: {0}", format);
                return null;
            }
        }

        private string CreateQueryString(XmlElement samlMessage, AsymmetricAlgorithm key)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("SAMLRequest={0}", HttpUtility.UrlEncode(DeflateRequest(samlMessage.OuterXml)));
            if (key != null)
            {
                stringBuilder.AppendFormat("&SigAlg={0}", HttpUtility.UrlEncode(SecurityAlgorithms.RsaSha1Signature));
                byte[] signature = ((RSACryptoServiceProvider)key).SignData(
                        Encoding.UTF8.GetBytes(stringBuilder.ToString()), new SHA1CryptoServiceProvider());
                stringBuilder.AppendFormat("&Signature={0}", HttpUtility.UrlEncode(Convert.ToBase64String(signature)));
            }
            return stringBuilder.ToString();
        }

        private string DeflateRequest(string samlRequest)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(samlRequest);
            using (var memoryStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(messageBytes, 0, messageBytes.Length);
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}