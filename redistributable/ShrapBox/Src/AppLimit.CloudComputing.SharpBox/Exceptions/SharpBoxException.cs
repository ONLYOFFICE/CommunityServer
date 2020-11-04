using System;
using System.Net;
using System.Runtime.Serialization;

namespace AppLimit.CloudComputing.SharpBox.Exceptions
{
    /// <summary>
    /// The SharpBoxException class implements the error code and error
    /// message of a specific sharp box error. Catch this kind of exceptions
    /// to get all sharpbox related error in the application
    /// </summary>
    [Serializable]
    public class SharpBoxException : Exception
    {
        /// <summary>
        /// This property contains the errorcode of the specific sharpbox error
        /// </summary>
        public SharpBoxErrorCodes ErrorCode { get; private set; }

        /// <summary>
        /// This property contains the used webrequest which throws the exception
        /// </summary>
        public WebRequest PostedRequet { get; private set; }

        /// <summary>
        /// This property contains a disposed webresponse which received during 
        /// network operation which throws this exception
        /// </summary>
        public WebResponse DisposedReceivedResponse { get; private set; }

        /// <summary>
        /// The constructure if the SharpBoxException class. The error code wil
        /// be resolved into an text message automatically
        /// </summary>
        /// <param name="errorCode"></param>
        public SharpBoxException(SharpBoxErrorCodes errorCode)
            : this(errorCode, null)
        {
        }

        /// <summary>
        /// The constructure if the SharpBoxException class. The error code wil
        /// be resolved into an text message automatically
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="innerException"></param>
        internal SharpBoxException(SharpBoxErrorCodes errorCode, Exception innerException)
            : this(errorCode, innerException, null, null)
        {
        }

        internal SharpBoxException(SharpBoxErrorCodes errorCode, Exception innerException, WebRequest request, WebResponse response)
            : base(GetErrorMessage(errorCode), innerException)
        {
            ErrorCode = errorCode;
            PostedRequet = request;
            DisposedReceivedResponse = response;
        }

        protected SharpBoxException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static String GetErrorMessage(SharpBoxErrorCodes errorCode)
        {
            // get the key
            var key = errorCode.ToString();

            // Load the value of string value for Client
            try
            {
                return ErrorMessages.ResourceManager.GetString(key);
            }
            catch (Exception)
            {
                return "n/a";
            }
        }

        internal static void ThrowSharpBoxExceptionBasedOnNetworkErrorCode(WebRequest uploadRequest, int code, WebException e)
        {
            if (Convert.ToInt32(code) == 507)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInsufficientDiskSpace, e, uploadRequest, null);

            throw new SharpBoxException(SharpBoxErrorCodes.ErrorCreateOperationFailed, e, uploadRequest, null);
        }

        internal static void ThrowSharpBoxExceptionBasedOnHttpErrorCode(HttpWebRequest uploadRequest, HttpStatusCode code, WebException e)
        {
            ThrowSharpBoxExceptionBasedOnNetworkErrorCode(uploadRequest, (int)code, e);
        }
    }
}