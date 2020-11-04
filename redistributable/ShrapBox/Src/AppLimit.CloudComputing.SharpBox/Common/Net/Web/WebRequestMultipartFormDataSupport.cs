using System;
using System.IO;
using System.Net;
using System.Text;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web
{
    internal class WebRequestMultipartFormDataSupport
    {
        private const string FormBoundary = "-----------------------------28947758029299";

        public string GetMultipartFormContentType()
        {
            return string.Format("multipart/form-data; boundary={0}", FormBoundary);
        }

        public long GetHeaderFooterSize(String fileName)
        {
            var header = GetHeaderData(fileName);
            var footer = GetFooterString();

            return header.Length + footer.Length;
        }

        /// <summary>
        /// This method prepares a webrequest to be a multpart data requets
        /// </summary>
        /// <param name="request"></param>
        public void PrepareWebRequest(WebRequest request)
        {
            request.Method = "POST";
            request.ContentType = GetMultipartFormContentType();
        }

        /// <summary>
        /// This method opens a network file data stream which means the http(s)
        /// will be established to the server
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public void PrepareRequestStream(Stream requestStream, String fileName)
        {
            // Add just the first part of this param, since we will write the file data directly to the Stream
            var header = GetHeaderData(fileName);

            // write the header into stream            
            requestStream.Write(header, 0, header.Length);
        }

        /// <summary>
        /// This method writes the footer into the data stream 
        /// </summary>
        /// <param name="networkStream"></param>
        public void FinalizeNetworkFileDataStream(Stream networkStream)
        {
            var encoding = Encoding.UTF8;
            var footer = GetFooterString();
            networkStream.Write(encoding.GetBytes(footer), 0, footer.Length);
        }

        /// <summary>
        /// Check if the current request us a multipart upload file
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool IsRequestMultiPartUpload(WebRequest request)
        {
            try
            {
                if (request.ContentType == null)
                    return false;
                if (request.ContentType.Equals(GetMultipartFormContentType()))
                    return true;
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        private static Byte[] GetHeaderData(String fileName)
        {
            var utf8 = new UTF8Encoding();
            var data = utf8.GetBytes(string.Format("--{0}{4}Content-Disposition: form-data; name=\"{2}\"; filename=\"{1}\";{4}Content-Type: {3}{4}{4}",
                                                   FormBoundary,
                                                   fileName,
                                                   "file",
                                                   "application/octet-stream",
                                                   Environment.NewLine));
            return data;
        }

        private static String GetFooterString()
        {
            return String.Format("{1}--{0}--{1}", FormBoundary, Environment.NewLine);
        }
    }
}