/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Utilclass.ExceptionMessages.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Resources;

namespace Novell.Directory.Ldap.Utilclass
{

    /// <summary> This class contains strings that may be associated with Exceptions generated
    /// by the Ldap API libraries.
    /// Two entries are made for each message, a String identifier, and the
    /// actual error string.  Parameters are identified as {0}, {1}, etc.
    /// </summary>
    public class ExceptionMessages : ResourceManager
    {
        public object[][] getContents()
        {
            return contents;
        }

        //static strings to aide lookup and guarantee accuracy:
        //DO NOT include these strings in other Locales
        [CLSCompliantAttribute(false)]
        public const string TOSTRING = "TOSTRING";
        public const string SERVER_MSG = "SERVER_MSG";
        public const string MATCHED_DN = "MATCHED_DN";
        public const string FAILED_REFERRAL = "FAILED_REFERRAL";
        public const string REFERRAL_ITEM = "REFERRAL_ITEM";
        public const string CONNECTION_ERROR = "CONNECTION_ERROR";
        public const string CONNECTION_IMPOSSIBLE = "CONNECTION_IMPOSSIBLE";
        public const string CONNECTION_WAIT = "CONNECTION_WAIT";
        public const string CONNECTION_FINALIZED = "CONNECTION_FINALIZED";
        public const string CONNECTION_CLOSED = "CONNECTION_CLOSED";
        public const string CONNECTION_READER = "CONNECTION_READER";
        public const string DUP_ERROR = "DUP_ERROR";
        public const string REFERRAL_ERROR = "REFERRAL_ERROR";
        public const string REFERRAL_LOCAL = "REFERRAL_LOCAL";
        public const string REFERENCE_ERROR = "REFERENCE_ERROR";
        public const string REFERRAL_SEND = "REFERRAL_SEND";
        public const string REFERENCE_NOFOLLOW = "REFERENCE_NOFOLLOW";
        public const string REFERRAL_BIND = "REFERRAL_BIND";
        public const string REFERRAL_BIND_MATCH = "REFERRAL_BIND_MATCH";
        public const string NO_DUP_REQUEST = "NO_DUP_REQUEST";
        public const string SERVER_CONNECT_ERROR = "SERVER_CONNECT_ERROR";
        public const string NO_SUP_PROPERTY = "NO_SUP_PROPERTY";
        public const string ENTRY_PARAM_ERROR = "ENTRY_PARAM_ERROR";
        public const string DN_PARAM_ERROR = "DN_PARAM_ERROR";
        public const string RDN_PARAM_ERROR = "RDN_PARAM_ERROR";
        public const string OP_PARAM_ERROR = "OP_PARAM_ERROR";
        public const string PARAM_ERROR = "PARAM_ERROR";
        public const string DECODING_ERROR = "DECODING_ERROR";
        public const string ENCODING_ERROR = "ENCODING_ERROR";
        public const string IO_EXCEPTION = "IO_EXCEPTION";
        public const string INVALID_ESCAPE = "INVALID_ESCAPE";
        public const string SHORT_ESCAPE = "SHORT_ESCAPE";
        public const string INVALID_CHAR_IN_FILTER = "INVALID_CHAR_IN_FILTER";
        public const string INVALID_CHAR_IN_DESCR = "INVALID_CHAR_IN_DESCR";
        public const string INVALID_ESC_IN_DESCR = "INVALID_ESC_IN_DESCR";
        public const string UNEXPECTED_END = "UNEXPECTED_END";
        public const string MISSING_LEFT_PAREN = "MISSING_LEFT_PAREN";
        public const string MISSING_RIGHT_PAREN = "MISSING_RIGHT_PAREN";
        public const string EXPECTING_RIGHT_PAREN = "EXPECTING_RIGHT_PAREN";
        public const string EXPECTING_LEFT_PAREN = "EXPECTING_LEFT_PAREN";
        public const string NO_OPTION = "NO_OPTION";
        public const string INVALID_FILTER_COMPARISON = "INVALID_FILTER_COMPARISON";
        public const string NO_MATCHING_RULE = "NO_MATCHING_RULE";
        public const string NO_ATTRIBUTE_NAME = "NO_ATTRIBUTE_NAME";
        public const string NO_DN_NOR_MATCHING_RULE = "NO_DN_NOR_MATCHING_RULE";
        public const string NOT_AN_ATTRIBUTE = "NOT_AN_ATTRIBUTE";
        public const string UNEQUAL_LENGTHS = "UNEQUAL_LENGTHS";
        public const string IMPROPER_REFERRAL = "IMPROPER_REFERRAL";
        public const string NOT_IMPLEMENTED = "NOT_IMPLEMENTED";
        public const string NO_MEMORY = "NO_MEMORY";
        public const string SERVER_SHUTDOWN_REQ = "SERVER_SHUTDOWN_REQ";
        public const string INVALID_ADDRESS = "INVALID_ADDRESS";
        public const string UNKNOWN_RESULT = "UNKNOWN_RESULT";
        public const string OUTSTANDING_OPERATIONS = "OUTSTANDING_OPERATIONS";
        public const string WRONG_FACTORY = "WRONG_FACTORY";
        public const string NO_TLS_FACTORY = "NO_TLS_FACTORY";
        public const string NO_STARTTLS = "NO_STARTTLS";
        public const string STOPTLS_ERROR = "STOPTLS_ERROR";
        public const string MULTIPLE_SCHEMA = "MULTIPLE_SCHEMA";
        public const string NO_SCHEMA = "NO_SCHEMA";
        public const string READ_MULTIPLE = "READ_MULTIPLE";
        public const string CANNOT_BIND = "CANNOT_BIND";
        public const string SSL_PROVIDER_MISSING = "SSL_PROVIDER_MISSING";

        //End constants

        internal static readonly object[][] contents = {new object[]{"TOSTRING", "{0}: {1} ({2}) {3}"}, new object[]{"SERVER_MSG", "{0}: Server Message: {1}"}, new object[]{"MATCHED_DN", "{0}: Matched DN: {1}"}, new object[]{"FAILED_REFERRAL", "{0}: Failed Referral: {1}"}, new object[]{"REFERRAL_ITEM", "{0}: Referral: {1}"}, new object[]{"CONNECTION_ERROR", "Unable to connect to server {0}:{1}"}, new object[]{"CONNECTION_IMPOSSIBLE", "Unable to reconnect to server, application has never called connect()"}, new object[]{"CONNECTION_WAIT", "Connection lost waiting for results from {0}:{1}"}, new object[]{"CONNECTION_FINALIZED", "Connection closed by the application finalizing the object"}, new object[]{"CONNECTION_CLOSED", "Connection closed by the application disconnecting"}, new object[]{"CONNECTION_READER", "Reader thread terminated"}, new object[]{"DUP_ERROR", "RfcLdapMessage: Cannot duplicate message built from the input stream"}, new object[]{"REFERENCE_ERROR", "Error attempting to follow a search continuation reference"}, new object[]{"REFERRAL_ERROR", "Error attempting to follow a referral"}, new object[]{"REFERRAL_LOCAL", "LdapSearchResults.{0}(): No entry found & request is not complete"}, new object[]{"REFERRAL_SEND", "Error sending request to referred server"}, new object[]{"REFERENCE_NOFOLLOW", "Search result reference received, and referral following is off"}, new object[]{"REFERRAL_BIND", "LdapBind.bind() function returned null"}, new object[]{"REFERRAL_BIND_MATCH", "Could not match LdapBind.bind() connection with Server Referral URL list"}, new object[]{"NO_DUP_REQUEST", "Cannot duplicate message to follow referral for {0} request, not allowed"}, new object[]{"SERVER_CONNECT_ERROR", "Error connecting to server {0} while attempting to follow a referral"}, new object[]{"NO_SUP_PROPERTY", "Requested property is not supported."}, new 
			object[]{"ENTRY_PARAM_ERROR", "Invalid Entry parameter"}, new object[]{"DN_PARAM_ERROR", "Invalid DN parameter"}, new object[]{"RDN_PARAM_ERROR", "Invalid DN or RDN parameter"}, new object[]{"OP_PARAM_ERROR", "Invalid extended operation parameter, no OID specified"}, new object[]{"PARAM_ERROR", "Invalid parameter"}, new object[]{"DECODING_ERROR", "Error Decoding responseValue"}, new object[]{"ENCODING_ERROR", "Encoding Error"}, new object[]{"IO_EXCEPTION", "I/O Exception on host {0}, port {1}"}, new object[]{"INVALID_ESCAPE", "Invalid value in escape sequence \"{0}\""}, new object[]{"SHORT_ESCAPE", "Incomplete escape sequence"}, new object[]{"UNEXPECTED_END", "Unexpected end of filter"}, new object[]{"MISSING_LEFT_PAREN", "Unmatched parentheses, left parenthesis missing"}, new object[]{"NO_OPTION", "Semicolon present, but no option specified"}, new object[]{"MISSING_RIGHT_PAREN", "Unmatched parentheses, right parenthesis missing"}, new object[]{"EXPECTING_RIGHT_PAREN", "Expecting right parenthesis, found \"{0}\""}, new object[]{"EXPECTING_LEFT_PAREN", "Expecting left parenthesis, found \"{0}\""}, new object[]{"NO_ATTRIBUTE_NAME", "Missing attribute description"}, new object[]{"NO_DN_NOR_MATCHING_RULE", "DN and matching rule not specified"}, new object[]{"NO_MATCHING_RULE", "Missing matching rule"}, new object[]{"INVALID_FILTER_COMPARISON", "Invalid comparison operator"}, new object[]{"INVALID_CHAR_IN_FILTER", "The invalid character \"{0}\" needs to be escaped as \"{1}\""}, new object[]{"INVALID_ESC_IN_DESCR", "Escape sequence not allowed in attribute description"}, new object[]{"INVALID_CHAR_IN_DESCR", "Invalid character \"{0}\" in attribute description"}, new object[]{"NOT_AN_ATTRIBUTE", "Schema element is not an LdapAttributeSchema object"}, new object[]{"UNEQUAL_LENGTHS", 
			"Length of attribute Name array does not equal length of Flags array"}, new object[]{"IMPROPER_REFERRAL", "Referral not supported for command {0}"}, new object[]{"NOT_IMPLEMENTED", "Method LdapConnection.startTLS not implemented"}, new object[]{"NO_MEMORY", "All results could not be stored in memory, sort failed"}, new object[]{"SERVER_SHUTDOWN_REQ", "Received unsolicited notification from server {0}:{1} to shutdown"}, new object[]{"INVALID_ADDRESS", "Invalid syntax for address with port; {0}"}, new object[]{"UNKNOWN_RESULT", "Unknown Ldap result code {0}"}, new object[]{"OUTSTANDING_OPERATIONS", "Cannot start or stop TLS because outstanding Ldap operations exist on this connection"}, new object[]{"WRONG_FACTORY", "StartTLS cannot use the set socket factory because it does not implement LdapTLSSocketFactory"}, new object[]{"NO_TLS_FACTORY", "StartTLS failed because no LdapTLSSocketFactory has been set for this Connection"}, new object[]{"NO_STARTTLS", "An attempt to stopTLS on a connection where startTLS had not been called"}, new object[]{"STOPTLS_ERROR", "Error stopping TLS: Error getting input & output streams from the original socket"}, new object[]{"MULTIPLE_SCHEMA", "Multiple schema found when reading the subschemaSubentry for {0}"}, new object[]{"NO_SCHEMA", "No schema found when reading the subschemaSubentry for {0}"}, new object[]{"READ_MULTIPLE", "Read response is ambiguous, multiple entries returned"}, new object[]{"CANNOT_BIND", "Cannot bind. Use PoolManager.getBoundConnection()"}};
    } //End ExceptionMessages
}
