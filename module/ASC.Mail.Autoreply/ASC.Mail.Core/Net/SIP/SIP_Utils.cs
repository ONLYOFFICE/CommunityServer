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


namespace ASC.Mail.Net.SIP
{
    #region usings

    using System;
    using AUTH;
    using Message;
    using MIME;
    using Stack;

    #endregion

    /// <summary>
    /// SIP helper methods.
    /// </summary>
    public class SIP_Utils
    {
        #region Methods

        /// <summary>
        /// Parses address from SIP To: header field.
        /// </summary>
        /// <param name="to">SIP header To: value.</param>
        /// <returns></returns>
        public static string ParseAddress(string to)
        {
            try
            {
                string retVal = to;
                if (to.IndexOf('<') > -1 && to.IndexOf('<') < to.IndexOf('>'))
                {
                    retVal = to.Substring(to.IndexOf('<') + 1, to.IndexOf('>') - to.IndexOf('<') - 1);
                }
                // Remove sip:
                if (retVal.IndexOf(':') > -1)
                {
                    retVal = retVal.Substring(retVal.IndexOf(':') + 1).Split(':')[0];
                }
                return retVal;
            }
            catch
            {
                throw new ArgumentException("Invalid SIP header To: '" + to + "' value !");
            }
        }

        /// <summary>
        /// Converts URI to Request-URI by removing all not allowed Request-URI parameters from URI.
        /// </summary>
        /// <param name="uri">URI value.</param>
        /// <returns>Returns valid Request-URI value.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>uri</b> is null reference.</exception>
        public static AbsoluteUri UriToRequestUri(AbsoluteUri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri is SIP_Uri)
            {
                // RFC 3261 19.1.2.(Table)
                // We need to strip off "method-param" and "header" URI parameters".
                // Currently we do it for sip or sips uri, do we need todo it for others too ?

                SIP_Uri sUri = (SIP_Uri) uri;
                sUri.Parameters.Remove("method");
                sUri.Header = null;

                return sUri;
            }
            else
            {
                return uri;
            }
        }

        /// <summary>
        /// Gets if specified value is SIP or SIPS URI.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>Returns true if specified value is SIP or SIPS URI, otherwise false.</returns>
        public static bool IsSipOrSipsUri(string value)
        {
            try
            {
                SIP_Uri.Parse(value);
                return true;
            }
            catch {}
            return false;
        }

        /// <summary>
        /// Gets if specified URI is tel: or sip tel URI. There is special case when SIP URI can be tel:, 
        /// sip:+xxxx and sip:xxx;user=phone.
        /// </summary>
        /// <param name="uri">URI to check.</param>
        /// <returns>Returns true if specified URI is tel: URI.</returns>
        public static bool IsTelUri(string uri)
        {
            uri = uri.ToLower();

            try
            {
                if (uri.StartsWith("tel:"))
                {
                    return true;
                }
                else if (IsSipOrSipsUri(uri))
                {
                    SIP_Uri sipUri = SIP_Uri.Parse(uri);
                    // RFC 3398 12. If user part starts with +, it's tel: URI.
                    if (sipUri.User.StartsWith("+"))
                    {
                        return true;
                    }
                        // RFC 3398 12.
                    else if (sipUri.Param_User != null && sipUri.Param_User.ToLower() == "phone")
                    {
                        return true;
                    }
                }
            }
            catch {}

            return false;
        }

        /// <summary>
        /// Gets specified realm SIP proxy credentials. Returns null if none exists for specified realm.
        /// </summary>
        /// <param name="request">SIP reques.</param>
        /// <param name="realm">Realm(domain).</param>
        /// <returns>Returns specified realm credentials or null if none.</returns>
        public static SIP_t_Credentials GetCredentials(SIP_Request request, string realm)
        {
            foreach (
                SIP_SingleValueHF<SIP_t_Credentials> authorization in request.ProxyAuthorization.HeaderFields)
            {
                if (authorization.ValueX.Method.ToLower() == "digest")
                {
                    Auth_HttpDigest authDigest = new Auth_HttpDigest(authorization.ValueX.AuthData,
                                                                     request.RequestLine.Method);
                    if (authDigest.Realm.ToLower() == realm.ToLower())
                    {
                        return authorization.ValueX;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets is specified option tags constains specified option tag.
        /// </summary>
        /// <param name="tags">Option tags.</param>
        /// <param name="tag">Option tag to check.</param>
        /// <returns>Returns true if specified option tag exists.</returns>
        public static bool ContainsOptionTag(SIP_t_OptionTag[] tags, string tag)
        {
            foreach (SIP_t_OptionTag t in tags)
            {
                if (t.OptionTag.ToLower() == tag)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets if specified method can establish dialog.
        /// </summary>
        /// <param name="method">SIP method.</param>
        /// <returns>Returns true if specified SIP method can establish dialog, otherwise false.</returns>
        /// <exception cref="ArgumentException">Is raised when invalid value is passed.</exception>
        public static bool MethodCanEstablishDialog(string method)
        {
            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException("Argument 'method' value can't be null or empty !");
            }
            method = method.ToUpper();

            if (method == SIP_Methods.INVITE)
            {
                return true;
            }
            else if (method == SIP_Methods.SUBSCRIBE)
            {
                return true;
            }
            else if (method == SIP_Methods.REFER)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates tag for tag header filed. For example From:/To: tag value.
        /// </summary>
        /// <returns>Returns tag string.</returns>
        public static string CreateTag()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(8);
        }

        /// <summary>
        /// Gets if the specified transport is reliable transport.
        /// </summary>
        /// <param name="transport">SIP transport.</param>
        /// <returns>Returns if specified transport is reliable.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>transport</b> is null reference.</exception>
        public static bool IsReliableTransport(string transport)
        {
            if (transport == null)
            {
                throw new ArgumentNullException("transport");
            }

            if (transport.ToUpper() == SIP_Transport.TCP)
            {
                return true;
            }
            else if (transport.ToUpper() == SIP_Transport.TLS)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets if the specified value is "token".
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>Returns true if specified valu is token, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public static bool IsToken(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return MIME_Reader.IsToken(value);
        }

        #endregion
    }
}