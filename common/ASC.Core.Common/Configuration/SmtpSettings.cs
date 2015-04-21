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
