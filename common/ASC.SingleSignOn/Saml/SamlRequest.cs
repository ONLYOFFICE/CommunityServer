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