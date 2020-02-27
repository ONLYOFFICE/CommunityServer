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


namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements SIP "Subscription-State" value. Defined in RFC 3265.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3265 Syntax:
    ///     Subscription-State     = substate-value *( SEMI subexp-params )
    ///     substate-value         = "active" / "pending" / "terminated" / extension-substate
    ///     extension-substate     = token
    ///     subexp-params          =   ("reason" EQUAL event-reason-value)
    ///                              / ("expires" EQUAL delta-seconds)
    ///                              / ("retry-after" EQUAL delta-seconds)
    ///                              / generic-param
    ///     event-reason-value     = "deactivated" / "probation" / "rejected" / "timeout" / "giveup"
    ///                               / "noresource" / event-reason-extension
    ///     event-reason-extension = token
    /// </code>
    /// </remarks>
    public class SIP_t_SubscriptionState : SIP_t_ValueWithParams
    {
        #region Nested type: EventReason

        /// <summary>
        /// This class holds 'event-reason-value' values.
        /// </summary>
        public class EventReason
        {
            #region Members

            /// <summary>
            /// The subscription has been terminated, but the subscriber SHOULD retry immediately 
            /// with a new subscription.  One primary use of such a status code is to allow migration of 
            /// subscriptions between nodes.  The "retry-after" parameter has no semantics for "deactivated".
            /// </summary>
            public const string deactivated = "deactivated";

            /// <summary>
            /// The subscription has been terminated because the notifier could not obtain authorization in a 
            /// timely fashion.  If a "retry-after" parameter is also present, the client SHOULD wait at least
            /// the number of seconds specified by that parameter before attempting to re-subscribe; otherwise, 
            /// the client MAY retry immediately, but will likely get put back into pending state.
            /// </summary>
            public const string giveup = "giveup";

            /// <summary>
            /// The subscription has been terminated because the resource state which was being monitored 
            /// no longer exists. Clients SHOULD NOT attempt to re-subscribe. The "retry-after" parameter 
            /// has no semantics for "noresource".
            /// </summary>
            public const string noresource = "noresource";

            /// <summary>
            /// The subscription has been terminated, but the client SHOULD retry at some later time. 
            /// If a "retry-after" parameter is also present, the client SHOULD wait at least the number of
            /// seconds specified by that parameter before attempting to re-subscribe.
            /// </summary>
            public const string probation = "probation";

            /// <summary>
            /// The subscription has been terminated due to change in authorization policy. 
            /// Clients SHOULD NOT attempt to re-subscribe. The "retry-after" parameter has no 
            /// semantics for "rejected".
            /// </summary>
            public const string rejected = "rejected";

            /// <summary>
            /// The subscription has been terminated because it was not refreshed before it expired. 
            /// Clients MAY re-subscribe immediately. The "retry-after" parameter has no semantics for "timeout".
            /// </summary>
            public const string timeout = "timeout";

            #endregion
        }

        #endregion

        #region Nested type: SubscriptionState

        /// <summary>
        /// This class holds 'substate-value' values.
        /// </summary>
        public class SubscriptionState
        {
            #region Members

            /// <summary>
            /// The subscription has been accepted and (in general) has been authorized.
            /// </summary>
            public const string active = "active";

            /// <summary>
            /// The subscription has been received by the notifier, but there is insufficient policy
            /// information to grant or deny the subscription yet.
            /// </summary>
            public const string pending = "pending";

            /// <summary>
            /// The subscriber should consider the subscription terminated.
            /// </summary>
            public const string terminated = "terminated";

            #endregion
        }

        #endregion

        #region Members

        private string m_Value = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets subscription state value. Known values are defined in SubscriptionState class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value passed.</exception>
        /// <exception cref="ArgumentException">Is raised when empty string is passed or value is not token.</exception>
        public string Value
        {
            get { return m_Value; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Value");
                }
                if (value == "")
                {
                    throw new ArgumentException("Property 'Value' value may not be '' !");
                }
                if (!TextUtils.IsToken(value))
                {
                    throw new ArgumentException("Property 'Value' value must be 'token' !");
                }

                m_Value = value;
            }
        }

        /// <summary>
        /// Gets or sets 'reason' parameter value. Known reason values are defined in EventReason class.
        /// Value null means not specified. 
        /// </summary>
        public string Reason
        {
            get
            {
                SIP_Parameter parameter = Parameters["reason"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Parameters.Remove("reason");
                }
                else
                {
                    Parameters.Set("reason", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'expires' parameter value. Value -1 means not specified.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when negative value(except -1) is passed.</exception>
        public int Expires
        {
            get
            {
                SIP_Parameter parameter = Parameters["expires"];
                if (parameter != null)
                {
                    return Convert.ToInt32(parameter.Value);
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (value == -1)
                {
                    Parameters.Remove("expires");
                }
                else
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("Property 'Expires' value must >= 0 !");
                    }

                    Parameters.Set("expires", value.ToString());
                }
            }
        }

        /// <summary>
        /// Gets or sets 'expires' parameter value. Value -1 means not specified.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when negative value(except -1) is passed.</exception>
        public int RetryAfter
        {
            get
            {
                SIP_Parameter parameter = Parameters["retry-after"];
                if (parameter != null)
                {
                    return Convert.ToInt32(parameter.Value);
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (value == -1)
                {
                    Parameters.Remove("retry-after");
                }
                else
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("Property 'RetryAfter' value must >= 0 !");
                    }

                    Parameters.Set("retry-after", value.ToString());
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value"></param>
        public SIP_t_SubscriptionState(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Subscription-State" from specified value.
        /// </summary>
        /// <param name="value">SIP "Subscription-State" value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>value</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Parse(new StringReader(value));
        }

        /// <summary>
        /// Parses "Subscription-State" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Subscription-State     = substate-value *( SEMI subexp-params )
                substate-value         = "active" / "pending" / "terminated" / extension-substate
                extension-substate     = token
                subexp-params          =   ("reason" EQUAL event-reason-value)
                                          / ("expires" EQUAL delta-seconds)
                                          / ("retry-after" EQUAL delta-seconds)
                                          / generic-param
                event-reason-value     = "deactivated" / "probation" / "rejected" / "timeout" / "giveup"
                                          / "noresource" / event-reason-extension
                event-reason-extension = token
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // substate-value
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("SIP Event 'substate-value' value is missing !");
            }
            m_Value = word;

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "Subscription-State" value.
        /// </summary>
        /// <returns>Returns "Subscription-State" value.</returns>
        public override string ToStringValue()
        {
            /*
                Subscription-State     = substate-value *( SEMI subexp-params )
                substate-value         = "active" / "pending" / "terminated" / extension-substate
                extension-substate     = token
                subexp-params          =   ("reason" EQUAL event-reason-value)
                                          / ("expires" EQUAL delta-seconds)
                                          / ("retry-after" EQUAL delta-seconds)
                                          / generic-param
                event-reason-value     = "deactivated" / "probation" / "rejected" / "timeout" / "giveup"
                                          / "noresource" / event-reason-extension
                event-reason-extension = token
            */

            StringBuilder retVal = new StringBuilder();

            // substate-value
            retVal.Append(m_Value);

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}