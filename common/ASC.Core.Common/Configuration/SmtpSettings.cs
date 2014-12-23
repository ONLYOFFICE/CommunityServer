/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Web;

namespace ASC.Core.Configuration
{
    public class SmtpSettings
    {
        public const int DefaultSmtpPort = 25;
        public const string DefaultSenderDisplayName = "Teamlab Postman";

        public string Host { get; private set; }

        public int Port { get; private set; }

        public string SenderAddress { get; private set; }

        public string SenderDisplayName { get; private set; }

        public string CredentialsDomain { get; private set; }

        public string CredentialsUserName { get; private set; }

        public string CredentialsUserPassword { get; private set; }

        public bool EnableSSL { get; set; }

        public bool IsDefaultSettings { get; internal set; }

        public bool IsRequireAuthentication
        {
            get { return !string.IsNullOrEmpty(CredentialsUserName) && !string.IsNullOrEmpty(CredentialsUserPassword); }
        }

        public SmtpSettings(string host, string senderAddress)
            : this(host, senderAddress, DefaultSenderDisplayName)
        {
            
        }

        public SmtpSettings(string host, string senderAddress, string senderDisplayName)
            : this(host, DefaultSmtpPort, senderAddress, senderDisplayName)
        {

        }

        public SmtpSettings(string host, int port, string senderAddress)
            : this(host, port, senderAddress, DefaultSenderDisplayName)
        {

        }

        public SmtpSettings(string host, int port, string senderAddress, string senderDisplayName)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("Empty smtp host.", "host");
            }
            if (string.IsNullOrEmpty(senderAddress))
            {
                throw new ArgumentException("Empty sender address.", "senderAddress");
            }
            if (senderDisplayName == null)
            {
                throw new ArgumentNullException("senderDisplayName");
            }
            Host = host;
            Port = port;
            SenderAddress = senderAddress;
            SenderDisplayName = senderDisplayName;
        }

        public void SetCredentials(string userName, string password)
        {
            SetCredentials(userName, password, string.Empty);
        }

        public void SetCredentials(string userName, string password, string domain)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Empty user name.", "userName");
            }
            if (string.IsNullOrEmpty("password"))
            {
                throw new ArgumentException("Empty password.", "password");
            }
            CredentialsUserName = userName;
            CredentialsUserPassword = password;
            CredentialsDomain = domain;
        }

        public string Serialize()
        {
            return ToString();
        }

        public static SmtpSettings Deserialize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var props = value.Split(new[] { '#' }, StringSplitOptions.None);
            props = Array.ConvertAll(props, p => !string.IsNullOrEmpty(p) ? p : null);

            var host = HttpUtility.UrlDecode(props[3]);
            var port = !string.IsNullOrEmpty(props[4]) ? Int32.Parse(props[4]) : DefaultSmtpPort;
            var senderAddress = HttpUtility.UrlDecode(props[5]);
            var senderDisplayName = HttpUtility.UrlDecode(props[6]) ?? DefaultSenderDisplayName;

            var settings = new SmtpSettings(host, port, senderAddress, senderDisplayName)
                {
                    EnableSSL = 7 < props.Length && !string.IsNullOrEmpty(props[7]) && Convert.ToBoolean(props[7])
                };

            var credentialsUserName = HttpUtility.UrlDecode(props[1]);
            var credentialsUserPassword = HttpUtility.UrlDecode(props[2]);
            var credentialsDomain = HttpUtility.UrlDecode(props[0]);
            if (!string.IsNullOrEmpty(credentialsUserName) && !string.IsNullOrEmpty(credentialsUserPassword))
            {
                settings.SetCredentials(credentialsUserName, credentialsUserPassword, credentialsDomain);
            }
            
            return settings;
        }

        public override string ToString()
        {
            return string.Join("#",
                               HttpUtility.UrlEncode(CredentialsDomain),
                               HttpUtility.UrlEncode(CredentialsUserName),
                               HttpUtility.UrlEncode(CredentialsUserPassword),
                               HttpUtility.UrlEncode(Host),
                               Port.ToString(),
                               HttpUtility.UrlEncode(SenderAddress),
                               HttpUtility.UrlEncode(SenderDisplayName),
                               EnableSSL.ToString());
        }
    }
}
