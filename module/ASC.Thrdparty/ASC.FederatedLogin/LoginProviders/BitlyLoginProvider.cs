/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Globalization;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.Core.Common.Configuration;

namespace ASC.FederatedLogin.LoginProviders
{
    public class BitlyLoginProvider : Consumer, IValidateKeysProvider
    {
        private static string BitlyClientId
        {
            get { return Instance["bitlyClientId"]; }
        }

        private static string BitlyClientSecret
        {
            get { return Instance["bitlyClientSecret"]; }
        }

        private static string BitlyUrl
        {
            get { return Instance["bitlyUrl"]; }
        }

        private static BitlyLoginProvider Instance
        {
            get { return ConsumerFactory.Get<BitlyLoginProvider>(); }
        }

        public BitlyLoginProvider() { }

        public BitlyLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }

        public bool ValidateKeys()
        {
            try
            {
                return !string.IsNullOrEmpty(GetShortenLink("https://www.onlyoffice.com"));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Enabled
        {
            get
            {
                return !String.IsNullOrEmpty(BitlyClientId) &&
                       !String.IsNullOrEmpty(BitlyClientSecret) &&
                       !String.IsNullOrEmpty(BitlyUrl);
            }
        }

        public static String GetShortenLink(String shareLink)
        {
            var uri = new Uri(shareLink);

            var bitly = string.Format(BitlyUrl, BitlyClientId, BitlyClientSecret, Uri.EscapeDataString(uri.ToString()));
            XDocument response;
            try
            {
                response = XDocument.Load(bitly);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(e.Message, e);
            }

            var status = response.XPathSelectElement("/response/status_code").Value;
            if (status != ((int)HttpStatusCode.OK).ToString(CultureInfo.InvariantCulture))
            {
                throw new InvalidOperationException(status);
            }

            var data = response.XPathSelectElement("/response/data/url");

            return data.Value;
        }
    }
}
