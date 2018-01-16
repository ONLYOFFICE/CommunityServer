using System;
using System.Reflection;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     Takes an LdapExtendedResponse and returns an object
    ///     (that implements the base class ParsedExtendedResponse)
    ///     based on the OID.
    ///     <p>
    ///         You can then call methods defined in the child
    ///         class to parse the contents of the response.  The methods available
    ///         depend on the child class. All child classes inherit from the
    ///         ParsedExtendedResponse.
    ///     </p>
    /// </summary>
    public class ExtResponseFactory
    {
        /// <summary>
        ///     Used to Convert an RfcLdapMessage object to the appropriate
        ///     LdapExtendedResponse object depending on the operation being performed.
        /// </summary>
        /// <param name="inResponse">
        ///     The LdapExtendedReponse object as returned by the
        ///     extendedOperation method in the LdapConnection object.
        /// </param>
        /// <returns>
        ///     An object of base class LdapExtendedResponse.  The actual child
        ///     class of this returned object depends on the operation being
        ///     performed.
        /// </returns>
        public static LdapExtendedResponse convertToExtendedResponse(RfcLdapMessage inResponse)
        {
            var tempResponse = new LdapExtendedResponse(inResponse);
            // Get the oid stored in the Extended response
            var inOID = tempResponse.ID;
            if(inOID == null) return tempResponse;

            var regExtResponses = LdapExtendedResponse.RegisteredResponses;
            try
            {
                var extRespClass = regExtResponses.findResponseExtension(inOID);
                if (extRespClass == null)
                {
                    return tempResponse;
                }
                Type[] argsClass = {typeof(RfcLdapMessage)};
                object[] args = {inResponse};
                Exception ex;
                try
                {
                    var extConstructor = extRespClass.GetConstructor(argsClass);
                    try
                    {
                        object resp = null;
                        resp = extConstructor.Invoke(args);
                        return (LdapExtendedResponse) resp;
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
            catch (FieldAccessException ex)
            {
                Logger.Log.LogWarning("Exception swallowed", ex);
            }
            // If we get here we did not have a registered extendedresponse
            // for this oid.  Return a default LdapExtendedResponse object.
            return tempResponse;
        }
    }
}