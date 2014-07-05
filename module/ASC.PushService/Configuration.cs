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
using System.Configuration;

namespace ASC.PushService
{
    public class PushServiceConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("debug", DefaultValue = false)]
        public bool IsDebug
        {
            get { return (bool)this["debug"]; }
            set { this["debug"] = value; }
        }

        [ConfigurationProperty("restartInterval", DefaultValue = "0:1:0")]
        public TimeSpan RestartInterval
        {
            get { return (TimeSpan)this["restartInterval"]; }
            set { this["restartInterval"] = value; }
        }

        [ConfigurationProperty("feedLifetime", DefaultValue = "14.0:0:0")]
        public TimeSpan FeedLifetime
        {
            get { return (TimeSpan)this["feedLifetime"]; }
            set { this["feedLifetime"] = value; }
        }

        [ConfigurationProperty("apns", IsRequired = false)]
        public ApnsConfiguration Apns
        {
            get { return (ApnsConfiguration)this["apns"]; }
            set { this["apns"] = value; }
        }

        [ConfigurationProperty("gcm", IsRequired = false)]
        public GcmConfiguration Gcm
        {
            get { return (GcmConfiguration)this["gcm"]; }
            set { this["gcm"] = value; }
        }

        public static PushServiceConfiguration GetSection()
        {
            return (PushServiceConfiguration)ConfigurationManager.GetSection("pushService");
        }
    }

    public class ApnsConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("devMode", DefaultValue = true)]
        public bool IsDevelopmentMode
        {
            get { return (bool)this["devMode"]; }
            set { this["devMode"] = value; }
        }

        [ConfigurationProperty("certPath", IsRequired = true)]
        public string CertificatePath
        {
            get { return (string)this["certPath"]; }
            set { this["certPath"] = value; }
        }

        [ConfigurationProperty("certPwd", IsRequired = true)]
        public string CertificatePassword
        {
            get { return (string)this["certPwd"]; }
            set { this["certPwd"] = value; }
        }

        [ConfigurationProperty("maxMessageLength", DefaultValue = 100)]
        public int MaxMessageLength
        {
            get { return (int) this["maxMessageLength"]; }
            set { this["maxMessageLength"] = value; }
        }

        [ConfigurationProperty("feedbackMinutes", DefaultValue = 5)]
        public int FeedbackIntervalMinutes
        {
            get { return (int)this["feedbackMinutes"]; }
            set { this["feedbackMinutes"] = value; }
        }
    }

    public class GcmConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("authKey", IsRequired = true)]
        public string AuthorizationKey
        {
            get { return (string)this["authKey"]; }
            set { this["authKey"] = value; }
        }
    }
}