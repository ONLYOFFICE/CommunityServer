using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web
{    
    internal static class WebRequestMethodsEx
    {
        public static class WebDAV
        {          
            /// <summary>
            /// MOVE Method.
            /// </summary>
            public const string Move = "MOVE";

            /// <summary>
            /// PROPFIND Method.
            /// </summary>
            public const string PropFind = "PROPFIND";     
            /// <summary>
            /// DELETE Method.
            /// </summary>
            public const string Delete = "DELETE";
            /// <summary>
            /// COPY Method.
            /// </summary>
            public const string Copy = "COPY";
            /// <summary>
            /// OPTIONS Method.
            /// </summary>
            public const string Options = "OPTIONS";
        }

        // Summary:
        //     Represents the types of file protocol methods that can be used with a FILE
        //     request. This class cannot be inherited.
        public static class File
        {
            // Summary:
            //     Represents the FILE GET protocol method that is used to retrieve a file from
            //     a specified location.
            public const string DownloadFile = "GET";
            //
            // Summary:
            //     Represents the FILE PUT protocol method that is used to copy a file to a
            //     specified location.
            public const string UploadFile = "PUT";
        }

        // Summary:
        //     Represents the types of FTP protocol methods that can be used with an FTP
        //     request. This class cannot be inherited.
        public static class Ftp
        {
            // Summary:
            //     Represents the FTP APPE protocol method that is used to append a file to
            //     an existing file on an FTP server.
            public const string AppendFile = "APPE";
            //
            // Summary:
            //     Represents the FTP DELE protocol method that is used to delete a file on
            //     an FTP server.
            public const string DeleteFile = "DELE";
            //
            // Summary:
            //     Represents the FTP RETR protocol method that is used to download a file from
            //     an FTP server.
            public const string DownloadFile = "RETR";
            public const string GetDateTimestamp = "MDTM";
            //
            // Summary:
            //     Represents the FTP SIZE protocol method that is used to retrieve the size
            //     of a file on an FTP server.
            public const string GetFileSize = "SIZE";
            //
            // Summary:
            //     Represents the FTP NLIST protocol method that gets a short listing of the
            //     files on an FTP server.
            public const string ListDirectory = "NLST";
            //
            // Summary:
            //     Represents the FTP LIST protocol method that gets a detailed listing of the
            //     files on an FTP server.
            public const string ListDirectoryDetails = "LIST";
            //
            // Summary:
            //     Represents the FTP MKD protocol method creates a directory on an FTP server.
            public const string MakeDirectory = "MKD";
            //
            // Summary:
            //     Represents the FTP PWD protocol method that prints the name of the current
            //     working directory.
            public const string PrintWorkingDirectory = "PWD";
            //
            // Summary:
            //     Represents the FTP RMD protocol method that removes a directory.
            public const string RemoveDirectory = "RMD";
            //
            // Summary:
            //     Represents the FTP RENAME protocol method that renames a directory.
            public const string Rename = "RENAME";
            //
            // Summary:
            //     Represents the FTP STOR protocol method that uploads a file to an FTP server.
            public const string UploadFile = "STOR";
            //
            // Summary:
            //     Represents the FTP STOU protocol that uploads a file with a unique name to
            //     an FTP server.
            public const string UploadFileWithUniqueName = "STOU";
        }

        // Summary:
        //     Represents the types of HTTP protocol methods that can be used with an HTTP
        //     request.
        public static class Http
        {
            // Summary:
            //     Represents the HTTP CONNECT protocol method that is used with a proxy that
            //     can dynamically switch to tunneling, as in the case of SSL tunneling.
            public const string Connect = "CONNECT";
            //
            // Summary:
            //     Represents an HTTP GET protocol method.
            public const string Get = "GET";
            //
            // Summary:
            //     Represents an HTTP HEAD protocol method. The HEAD method is identical to
            //     GET except that the server only returns message-headers in the response,
            //     without a message-body.
            public const string Head = "HEAD";
            //
            // Summary:
            //     Represents an HTTP MKCOL request that creates a new collection (such as a
            //     collection of pages) at the location specified by the request-Uniform Resource
            //     Identifier (URI).
            public const string MkCol = "MKCOL";
            //
            // Summary:
            //     Represents an HTTP POST protocol method that is used to post a new entity
            //     as an addition to a URI.
            public const string Post = "POST";
            //
            // Summary:
            //     Represents an HTTP PUT protocol method that is used to replace an entity
            //     identified by a URI.
            public const string Put = "PUT";
        }

    }        
}
