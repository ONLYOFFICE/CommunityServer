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


using ASC.Xmpp.Core.utils.Xml.Dom;

// JEP-0086: Error Condition Mappings

// <stanza-kind to='sender' type='error'>
// [RECOMMENDED to include sender XML here]
// <error type='error-type'>
// <defined-condition xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
// <text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'
// xml:lang='langcode'>
// OPTIONAL descriptive text
// </text>
// [OPTIONAL application-specific condition element]
// </error>
// </stanza-kind>

// Legacy Error
// <error code="501">Not Implemented</error>

// XMPP Style Error
// <error code='404' type='cancel'>
//		<item-not-found xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
// </error>

namespace ASC.Xmpp.Core.protocol.client
{
    // XMPP error condition  		XMPP error type  	Legacy error code
    // <bad-request/> 				modify 				400
    // <conflict/> 					cancel 				409
    // <feature-not-implemented/> 	cancel 				501
    // <forbidden/> 				auth 				403
    // <gone/> 						modify 				302 (permanent)
    // <internal-server-error/> 	wait 				500
    // <item-not-found/> 			cancel 				404
    // <jid-malformed/> 			modify 				400
    // <not-acceptable/> 			modify 				406
    // <not-allowed/> 				cancel 				405
    // <not-authorized/> 			auth 				401
    // <payment-required/> 			auth 				402
    // <recipient-unavailable/> 	wait 				404
    // <redirect/> 					modify 				302 (temporary)
    // <registration-required/> 	auth 				407
    // <remote-server-not-found/> 	cancel 				404
    // <remote-server-timeout/> 	wait 				504
    // <resource-constraint/> 		wait 				500
    // <service-unavailable/> 		cancel 				503
    // <subscription-required/> 	auth 				407
    // <undefined-condition/> 		[any] 				500
    // <unexpected-request/> 		wait 				400

    public enum ErrorCondition
    {
        BadRequest,
        Conflict,
        FeatureNotImplemented,
        Forbidden,
        Gone,
        InternalServerError,
        ItemNotFound,
        JidMalformed,
        NotAcceptable,
        NotAllowed,
        NotAuthorized,
        PaymentRequired,
        RecipientUnavailable,
        Redirect,
        RegistrationRequired,
        RemoteServerNotFound,
        RemoteServerTimeout,
        ResourceConstraint,
        ServiceUnavailable,
        SubscriptionRequired,
        UndefinedCondition,
        UnexpectedRequest
    }

    // The value of the <error/> element's 'type' attribute MUST be one of the following:
    // * cancel -- do not retry (the error is unrecoverable)
    // * continue -- proceed (the condition was only a warning)
    // * modify -- retry after changing the data sent
    // * auth -- retry after providing credentials
    // * wait -- retry after waiting (the error is temporary)
    public enum ErrorType
    {
        cancel,
        @continue,
        modify,
        auth,
        wait
    }


    /// <summary>
    ///   The legacy Error Code
    /// </summary>
    public enum ErrorCode
    {
        None = -1,

        /// <summary>
        ///   Bad request
        /// </summary>
        BadRequest = 400,

        /// <summary>
        ///   Unauthorized
        /// </summary>
        Unauthorized = 401,

        /// <summary>
        ///   Payment required
        /// </summary>
        PaymentRequired = 402,

        /// <summary>
        ///   Forbidden
        /// </summary>
        Forbidden = 403,

        /// <summary>
        ///   Not found
        /// </summary>
        NotFound = 404,

        /// <summary>
        ///   Not allowed
        /// </summary>
        NotAllowed = 405,

        /// <summary>
        ///   Not acceptable
        /// </summary>
        NotAcceptable = 406,

        /// <summary>
        ///   Registration required
        /// </summary>
        RegistrationRequired = 407,

        /// <summary>
        ///   Request timeout
        /// </summary>
        RequestTimeout = 408,

        /// <summary>
        ///   Conflict
        /// </summary>
        Conflict = 409,

        /// <summary>
        ///   Internal server error
        /// </summary>
        InternalServerError = 500,

        /// <summary>
        ///   Not implemented
        /// </summary>
        NotImplemented = 501,

        /// <summary>
        ///   Remote server error
        /// </summary>
        RemoteServerError = 502,

        /// <summary>
        ///   Service unavailable
        /// </summary>
        ServiceUnavailable = 503,

        /// <summary>
        ///   Remote server timeout
        /// </summary>
        RemoteServerTimeout = 504,

        /// <summary>
        ///   Disconnected
        /// </summary>
        Disconnected = 510
    }


    /// <summary>
    ///   Summary description for Error.
    /// </summary>
    public class Error : Element
    {
        #region << Constructors >>

        public Error()
        {
            Namespace = Uri.CLIENT;
            TagName = "error";
        }


        public Error(int code) : this()
        {
            SetAttribute("code", code.ToString());
        }

        public Error(ErrorCode code) : this()
        {
            SetAttribute("code", (int) code);
        }

        public Error(ErrorType type) : this()
        {
            Type = type;
        }

        /// <summary>
        ///   Creates an error Element according the the condition The type attrib as added automatically as decribed in the XMPP specs This is the prefered way to create error Elements
        /// </summary>
        /// <param name="condition"> </param>
        public Error(ErrorCondition condition) : this()
        {
            Condition = condition;
        }

        public Error(ErrorType type, ErrorCondition condition) : this(type)
        {
            Condition = condition;
        }

        #endregion

        /// <summary>
        ///   The error Description
        /// </summary>
        public string Message
        {
            get { return Value; }
            set { Value = value; }
        }

        public ErrorCode Code
        {
            get { return (ErrorCode) GetAttributeInt("code"); }
            set { SetAttribute("code", (int) value); }
        }

        public ErrorType Type
        {
            get { return (ErrorType) GetAttributeEnum("type", typeof (ErrorType)); }
            set { SetAttribute("type", value.ToString()); }
        }

        public ErrorCondition Condition
        {
            get
            {
                if (HasTag("bad-request")) // <bad-request/> 
                    return ErrorCondition.BadRequest;
                else if (HasTag("conflict")) // <conflict/> 
                    return ErrorCondition.Conflict;
                else if (HasTag("feature-not-implemented")) // <feature-not-implemented/>
                    return ErrorCondition.FeatureNotImplemented;
                else if (HasTag("forbidden")) // <forbidden/> 
                    return ErrorCondition.Forbidden;
                else if (HasTag("gone")) // <gone/>
                    return ErrorCondition.Gone;
                else if (HasTag("internal-server-error")) // <internal-server-error/>
                    return ErrorCondition.InternalServerError;
                else if (HasTag("item-not-found")) // <item-not-found/> 
                    return ErrorCondition.ItemNotFound;
                else if (HasTag("jid-malformed")) // <jid-malformed/>
                    return ErrorCondition.JidMalformed;
                else if (HasTag("not-acceptable")) // <not-acceptable/> 
                    return ErrorCondition.NotAcceptable;
                else if (HasTag("not-authorized")) // <not-authorized/>
                    return ErrorCondition.NotAuthorized;
                else if (HasTag("payment-required")) // <payment-required/>
                    return ErrorCondition.PaymentRequired;
                else if (HasTag("recipient-unavailable")) // <recipient-unavailable/>
                    return ErrorCondition.RecipientUnavailable;
                else if (HasTag("redirect")) // <redirect/>
                    return ErrorCondition.Redirect;
                else if (HasTag("registration-required")) // <registration-required/>
                    return ErrorCondition.RegistrationRequired;
                else if (HasTag("remote-server-not-found")) // <remote-server-not-found/> 
                    return ErrorCondition.RemoteServerNotFound;
                else if (HasTag("remote-server-timeout")) // <remote-server-timeout/> 
                    return ErrorCondition.RemoteServerTimeout;
                else if (HasTag("resource-constraint")) // <resource-constraint/>
                    return ErrorCondition.ResourceConstraint;
                else if (HasTag("service-unavailable")) // <service-unavailable/> 
                    return ErrorCondition.ServiceUnavailable;
                else if (HasTag("subscription-required")) // <subscription-required/> 
                    return ErrorCondition.SubscriptionRequired;
                else if (HasTag("undefined-condition")) // <undefined-condition/> 
                    return ErrorCondition.UndefinedCondition;
                else if (HasTag("unexpected-request")) // <unexpected-request/> 
                    return ErrorCondition.UnexpectedRequest;
                else
                    return ErrorCondition.UndefinedCondition;
            }
            set
            {
                switch (value)
                {
                    case ErrorCondition.BadRequest:
                        SetTag("bad-request", "", Uri.STANZAS);
                        Type = ErrorType.modify;
                        break;
                    case ErrorCondition.Conflict:
                        SetTag("conflict", "", Uri.STANZAS);
                        Type = ErrorType.cancel;
                        break;
                    case ErrorCondition.FeatureNotImplemented:
                        SetTag("feature-not-implemented", "", Uri.STANZAS);
                        Type = ErrorType.cancel;
                        break;
                    case ErrorCondition.Forbidden:
                        SetTag("forbidden", "", Uri.STANZAS);
                        Type = ErrorType.auth;
                        break;
                    case ErrorCondition.Gone:
                        SetTag("gone", "", Uri.STANZAS);
                        Type = ErrorType.modify;
                        break;
                    case ErrorCondition.InternalServerError:
                        SetTag("internal-server-error", "", Uri.STANZAS);
                        Type = ErrorType.wait;
                        break;
                    case ErrorCondition.ItemNotFound:
                        SetTag("item-not-found", "", Uri.STANZAS);
                        Type = ErrorType.cancel;
                        break;
                    case ErrorCondition.JidMalformed:
                        SetTag("jid-malformed", "", Uri.STANZAS);
                        Type = ErrorType.modify;
                        break;
                    case ErrorCondition.NotAcceptable:
                        SetTag("not-acceptable", "", Uri.STANZAS);
                        Type = ErrorType.modify;
                        break;
                    case ErrorCondition.NotAllowed:
                        SetTag("not-allowed", "", Uri.STANZAS);
                        Type = ErrorType.cancel;
                        break;
                    case ErrorCondition.NotAuthorized:
                        SetTag("not-authorized", "", Uri.STANZAS);
                        Type = ErrorType.auth;
                        break;
                    case ErrorCondition.PaymentRequired:
                        SetTag("payment-required", "", Uri.STANZAS);
                        Type = ErrorType.auth;
                        break;
                    case ErrorCondition.RecipientUnavailable:
                        SetTag("recipient-unavailable", "", Uri.STANZAS);
                        Type = ErrorType.wait;
                        break;
                    case ErrorCondition.Redirect:
                        SetTag("redirect", "", Uri.STANZAS);
                        Type = ErrorType.modify;
                        break;
                    case ErrorCondition.RegistrationRequired:
                        SetTag("registration-required", "", Uri.STANZAS);
                        Type = ErrorType.auth;
                        break;
                    case ErrorCondition.RemoteServerNotFound:
                        SetTag("remote-server-not-found", "", Uri.STANZAS);
                        Type = ErrorType.cancel;
                        break;
                    case ErrorCondition.RemoteServerTimeout:
                        SetTag("remote-server-timeout", "", Uri.STANZAS);
                        Type = ErrorType.wait;
                        break;
                    case ErrorCondition.ResourceConstraint:
                        SetTag("resource-constraint", "", Uri.STANZAS);
                        Type = ErrorType.wait;
                        break;
                    case ErrorCondition.ServiceUnavailable:
                        SetTag("service-unavailable", "", Uri.STANZAS);
                        Type = ErrorType.cancel;
                        break;
                    case ErrorCondition.SubscriptionRequired:
                        SetTag("subscription-required", "", Uri.STANZAS);
                        Type = ErrorType.auth;
                        break;
                    case ErrorCondition.UndefinedCondition:
                        SetTag("undefined-condition", "", Uri.STANZAS);
                        // could be any
                        break;
                    case ErrorCondition.UnexpectedRequest:
                        SetTag("unexpected-request", "", Uri.STANZAS);
                        Type = ErrorType.wait;
                        break;
                }
            }
        }
    }
}