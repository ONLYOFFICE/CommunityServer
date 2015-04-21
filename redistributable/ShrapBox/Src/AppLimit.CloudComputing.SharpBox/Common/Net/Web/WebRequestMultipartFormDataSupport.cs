using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web
{
    class WebRequestMultipartFormDataSupport
    {
        private const string FormBoundary = "-----------------------------28947758029299";

        public string GetMultipartFormContentType()
        {
            return string.Format("multipart/form-data; boundary={0}", FormBoundary);
        }        

        public long GetHeaderFooterSize(String fileName)
        {
            Byte[] header = GetHeaderData(fileName);
            String footer = GetFooterString();

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
            Byte[] header = GetHeaderData(fileName);

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
            string footer = GetFooterString();
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
                else
                    return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }


        private Byte[] GetHeaderData (String fileName)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] data = utf8.GetBytes(string.Format("--{0}{4}Content-Disposition: form-data; name=\"{2}\"; filename=\"{1}\";{4}Content-Type: {3}{4}{4}",
                                    FormBoundary,
                                    fileName,
                                    "file",
                                    "application/octet-stream",
                                    Environment.NewLine));

            return data;

        }

        private String GetFooterString()
        {
            return String.Format("{1}--{0}--{1}", FormBoundary, Environment.NewLine); 
        }        
    }
}
