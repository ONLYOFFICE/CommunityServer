using System;
using System.IO;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web.Ftp
{
    internal class FtpService : WebRequestService
    {
        /// <summary>
        /// This method creates a directory in the ftp server
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public bool FtpCreateDirectory(string uri, ICredentials credentials)
        {
            return PerformSimpleFtpMethod(uri, WebRequestMethodsEx.Ftp.MakeDirectory, credentials);
        }

        /// <summary>
        /// This method removes a directory
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public bool FtpDeleteEmptyDirectory(string uri, ICredentials credentials)
        {
            return PerformSimpleFtpMethod(uri, WebRequestMethodsEx.Ftp.RemoveDirectory, credentials);
        }

        /// <summary>
        /// This method removes an file
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public bool FtpDeleteFile(string uri, ICredentials credentials)
        {
            return PerformSimpleFtpMethod(uri, WebRequestMethodsEx.Ftp.DeleteFile, credentials);
        }

        public bool FtpRename(string uri, string newNameFullPath, ICredentials credentials)
        {
            // create a webrequest 
            var renameRequest = (FtpWebRequest)CreateWebRequest(uri, WebRequestMethodsEx.Ftp.Rename, credentials, false, null);

            // set the target 
            renameRequest.RenameTo = newNameFullPath;

            // perform the operation
            try
            {
                //get response but ignore it
                GetStringResponse(renameRequest);

                // ok 
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override WebRequest CreateBasicWebRequest(Uri uri, bool bAllowStreamBuffering)
        {
            //create request
            var result = (FtpWebRequest)WebRequest.Create(uri);

            //Do not keep alive (stateless mode)
            result.KeepAlive = false;

            // go ahead
            return result;
        }

        /// <summary>
        /// Obtains a response stream as a string
        /// </summary>
        /// <param name="ftp">current FTP request</param>
        /// <returns>String containing response</returns>
        /// <remarks>FTP servers typically return strings with CR and
        /// not CRLF. Use respons.Replace(vbCR, vbCRLF) to convert
        /// to an MSDOS string</remarks>
        private static string GetStringResponse(WebRequest ftp)
        {
            //Get the result, streaming to a string
            string result;

            using (var response = ftp.GetResponse())
            {
                using (var datastream = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(datastream))
                    {
                        result = sr.ReadToEnd();
                        sr.Close();
                    }
                    datastream.Close();
                }
                response.Close();
            }

            return result;
        }

        private bool PerformSimpleFtpMethod(string uri, string requestMethod, ICredentials credentials)
        {
            // create a webrequest 
            var simpleFTPRequest = CreateWebRequest(uri, requestMethod, credentials, false, null);

            // perform remove            
            try
            {
                //get response but ignore it
                GetStringResponse(simpleFTPRequest);

                // ok 
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override int GetWebResponseStatus(WebResponse response)
        {
            if (!(response is FtpWebResponse))
                throw new InvalidOperationException("Response is not a FTP reponse");

            var resp = (FtpWebResponse)response;
            return (int)resp.StatusCode;
        }
    }
}