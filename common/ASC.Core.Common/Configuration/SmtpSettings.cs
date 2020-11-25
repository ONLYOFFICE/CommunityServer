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
using System.Web;

namespace ASC.Core.Configuration
{
    public class SmtpSettings
    {
        public const int DefaultSmtpPort = 25;
        public const string DefaultSenderDisplayName = "ONLYOFFICE Postman";

        public string Host { get; private set; }

        public int Port { get; private set; }

        public string SenderAddress { get; private set; }

        public string SenderDisplayName { get; private set; }

        public string CredentialsDomain { get; private set; }

        public string CredentialsUserName { get; private set; }

        public string CredentialsUserPassword { get; private set; }

        public bool EnableSSL { get; set; }

        public bool EnableAuth { get; set; }

        public bool IsDefaultSettings { get; internal set; }

        public static SmtpSettings Empty = new SmtpSettings();

        private SmtpSettings()
        {
            
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
            if (string.IsNullOrEmpty(password))
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
                return Empty;
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
                settings.EnableAuth = true;
            }
            else
            {
                settings.EnableAuth = false;
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
