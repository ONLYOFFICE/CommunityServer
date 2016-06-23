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

// <failure xmlns='urn:ietf:params:xml:ns:xmpp-sasl'>
//		<incorrect-encoding/>
// </failure>

namespace ASC.Xmpp.Core.protocol.sasl
{
    /// <summary>
    ///   Summary description for Failure.
    /// </summary>
    public class Failure : Element
    {
        public Failure()
        {
            TagName = "failure";
            Namespace = Uri.SASL;
        }

        public Failure(FailureCondition cond) : this()
        {
            Condition = cond;
        }

        public FailureCondition Condition
        {
            get
            {
                if (HasTag("aborted"))
                    return FailureCondition.aborted;
                else if (HasTag("incorrect-encoding"))
                    return FailureCondition.incorrect_encoding;
                else if (HasTag("invalid-authzid"))
                    return FailureCondition.invalid_authzid;
                else if (HasTag("invalid-mechanism"))
                    return FailureCondition.invalid_mechanism;
                else if (HasTag("mechanism-too-weak"))
                    return FailureCondition.mechanism_too_weak;
                else if (HasTag("not-authorized"))
                    return FailureCondition.not_authorized;
                else if (HasTag("temporary-auth-failure"))
                    return FailureCondition.temporary_auth_failure;
                else
                    return FailureCondition.UnknownCondition;
            }
            set
            {
                if (value == FailureCondition.aborted)
                    SetTag("aborted");
                else if (value == FailureCondition.incorrect_encoding)
                    SetTag("incorrect-encoding");
                else if (value == FailureCondition.invalid_authzid)
                    SetTag("invalid-authzid");
                else if (value == FailureCondition.invalid_mechanism)
                    SetTag("invalid-mechanism");
                else if (value == FailureCondition.mechanism_too_weak)
                    SetTag("mechanism-too-weak");
                else if (value == FailureCondition.not_authorized)
                    SetTag("not-authorized");
                else if (value == FailureCondition.temporary_auth_failure)
                    SetTag("temporary-auth-failure");
            }
        }
    }
}