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
// Novell.Directory.Ldap.Utilclass.ResultCodeMessages.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Collections.Generic;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     This class contains strings corresponding to Ldap Result Codes.
    ///     The resources are accessed by the String representation of the result code.
    /// </summary>
    public class ResultCodeMessages //:System.Resources.ResourceManager
    {
        internal static readonly Dictionary<string, string> errorCodes = new Dictionary<string, string>
        {
            {"0", "Success"},
            {"1", "Operations Error"},
            {"2", "Protocol Error"},
            {"3", "Timelimit Exceeded"},
            {"4", "Sizelimit Exceeded"},
            {"5", "Compare False"},
            {"6", "Compare True"},
            {"7", "Authentication Method Not Supported"},
            {"8", "Strong Authentication Required"},
            {"9", "Partial Results"},
            {"10", "Referral"},
            {"11", "Administrative Limit Exceeded"},
            {"12", "Unavailable Critical Extension"},
            {"13", "Confidentiality Required"},
            {"14", "SASL Bind In Progress"},
            {"16", "No Such Attribute"},
            {"17", "Undefined Attribute Type"},
            {"18", "Inappropriate Matching"},
            {"19", "Constraint Violation"},
            {"20", "Attribute Or Value Exists"},
            {"21", "Invalid Attribute Syntax"},
            {"32", "No Such Object"},
            {"33", "Alias Problem"},
            {"34", "Invalid DN Syntax"},
            {"35", "Is Leaf"},
            {"36", "Alias Dereferencing Problem"},
            {"48", "Inappropriate Authentication"},
            {"49", "Invalid Credentials"},
            {"50", "Insufficient Access Rights"},
            {"51", "Busy"},
            {"52", "Unavailable"},
            {"53", "Unwilling To Perform"},
            {"54", "Loop Detect"},
            {"64", "Naming Violation"},
            {"65", "Object Class Violation"},
            {"66", "Not Allowed On Non-leaf"},
            {"67", "Not Allowed On RDN"},
            {"68", "Entry Already Exists"},
            {"69", "Object Class Modifications Prohibited"},
            {"71", "Affects Multiple DSAs"},
            {"80", "Other"},
            {"81", "Server Down"},
            {"82", "Local Error"},
            {"83", "Encoding Error"},
            {"84", "Decoding Error"},
            {"85", "Ldap Timeout"},
            {"86", "Authentication Unknown"},
            {"87", "Filter Error"},
            {"88", "User Cancelled"},
            {"89", "Parameter Error"},
            {"90", "No Memory"},
            {"91", "Connect Error"},
            {"92", "Ldap Not Supported"},
            {"93", "Control Not Found"},
            {"94", "No Results Returned"},
            {"95", "More Results To Return"},
            {"96", "Client Loop"},
            {"97", "Referral Limit Exceeded"},
            {"112", "TLS not supported"},
            {"113", "SSL handshake failed"},
            {"114", "SSL Provider not found"}
        };

        public static string GetResultCode(string code)
        {
            return errorCodes[code];
        }
    } //End ResultCodeMessages
}