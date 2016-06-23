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


using Novell.Directory.Ldap.Events.Edir;
using Novell.Directory.Ldap.Rfc2251;
using System;
using System.Reflection;

namespace Novell.Directory.Ldap.Utilclass
{

    /// <summary> 
    /// Takes an LdapExtendedResponse and returns an object
    /// (that implements the base class ParsedExtendedResponse)
    /// based on the OID.
    /// 
    /// <p>You can then call methods defined in the child
    /// class to parse the contents of the response.  The methods available
    /// depend on the child class. All child classes inherit from the
    /// ParsedExtendedResponse.</p>
    /// 
    /// </summary>
    public class ExtResponseFactory
    {
        /// <summary> Used to Convert an RfcLdapMessage object to the appropriate
        /// LdapExtendedResponse object depending on the operation being performed.
        /// 
        /// </summary>
        /// <param name="inResponse">  The LdapExtendedReponse object as returned by the
        /// extendedOperation method in the LdapConnection object.
        /// </param>
        /// <returns> An object of base class LdapExtendedResponse.  The actual child
        /// class of this returned object depends on the operation being
        /// performed.
        /// </returns>

        static public LdapExtendedResponse convertToExtendedResponse(RfcLdapMessage inResponse)
        {
            LdapExtendedResponse tempResponse = new LdapExtendedResponse(inResponse);
            // Get the oid stored in the Extended response
            string inOID = EventOids.NLDAP_MONITOR_EVENTS_RESPONSE;// tempResponse.ID;

            RespExtensionSet regExtResponses = LdapExtendedResponse.RegisteredResponses;
            try
            {
                System.Type extRespClass = regExtResponses.findResponseExtension(inOID);
                if (extRespClass == null)
                {
                    return tempResponse;
                }
                Type[] argsClass = new Type[] { typeof(RfcLdapMessage) };
                object[] args = new object[] { inResponse };
                Exception ex;
                try
                {
                    ConstructorInfo extConstructor = extRespClass.GetConstructor(argsClass);
                    try
                    {
                        object resp = null;
                        resp = extConstructor.Invoke(args);
                        return (LdapExtendedResponse)resp;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        ex = e;
                    }
                    catch (TargetInvocationException e)
                    {
                        ex = e;
                    }
                    catch (Exception e)
                    {
                        // Could not create the ResponseControl object
                        // All possible exceptions are ignored. We fall through
                        // and create a default LdapControl object
                        ex = e;
                    }
                }
                catch (MethodAccessException e)
                {
                    // bad class was specified, fall through and return a
                    // default  LdapExtendedResponse object
                    ex = e;
                }
            }
            catch (FieldAccessException)
            {
            }
            // If we get here we did not have a registered extendedresponse
            // for this oid.  Return a default LdapExtendedResponse object.
            return tempResponse;
        }
    }
}