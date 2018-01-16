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
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;

namespace ASC.SingleSignOn.Common
{
    [Serializable]
    [DataContract]
    public class SsoSettings : BaseSettings<SsoSettings>
    {
        public override Guid ID
        {
            get
            {
                return new Guid("{108F2408-302D-44CA-A7F9-893B3B59294E}");
            }
        }

        public override ISettings GetDefault()
        {
            return new SsoSettings
            {
                EnableSso = false,
                TokenType = TokenTypes.SAML,
                ValidationType = ValidationTypes.X509,
                PublicKey = string.Empty,
                Issuer = string.Empty,
                SsoEndPoint = string.Empty,
                SloEndPoint = string.Empty
            };
        }

        [DataMember]
        public bool EnableSso
        {
            get;
            set;
        }

        [DataMember]
        public string TokenType
        {
            get;
            set;
        }

        [DataMember]
        public string ValidationType
        {
            get;
            set;
        }

        [DataMember]
        public string PublicKey
        {
            get;
            set;
        }

        [DataMember]
        public string Issuer
        {
            get;
            set;
        }

        [DataMember]
        public string SsoEndPoint
        {
            get;
            set;
        }

        [DataMember]
        public string SloEndPoint
        {
            get;
            set;
        }

        [DataMember]
        public string ClientPassword
        {
            get;
            set;
        }

        [DataMember]
        public string ClientCertificateFileName
        {
            get;
            set;
        }
    }
}