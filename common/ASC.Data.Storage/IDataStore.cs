/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.IO;
using ASC.Data.Storage.Configuration;

namespace ASC.Data.Storage
{
    ///<summary>
    /// Interface for working with files
    ///</summary>
    public interface IDataStore
    {
        IQuotaController QuotaController { get; set; }

        TimeSpan GetExpire(string domain);

        ///<summary>
        /// Get absolute Uri for html links to handler
        ///</summary>
        ///<param name="path"></param>
        ///<returns></returns>
        Uri GetUri(string path);

        ///<summary>
        /// Get absolute Uri for html links to handler
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="path"></param>
        ///<returns></returns>
        Uri GetUri(string domain, string path);

        /// <summary>
        /// Get absolute Uri for html links to handler
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="expire"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        Uri GetPreSignedUri(string domain, string path, TimeSpan expire, IEnumerable<string> headers);

        ///<summary>
        /// Supporting generate uri to the file
        ///</summary>
        ///<returns></returns>
        bool IsSupportInternalUri { get; }

        /// <summary>
        /// Get absolute Uri for html links
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="expire"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        Uri GetInternalUri(string domain, string path, TimeSpan expire, IEnumerable<string> headers);

        ///<summary>
        /// A stream of read-only. In the case of the C3 stream NetworkStream general, and with him we have to work
        /// Very carefully as a Jedi cutter groin lightsaber.
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="path"></param>
        ///<returns></returns>
        Stream GetReadStream(string domain, string path);

        ///<summary>
        /// A stream of read-only. In the case of the C3 stream NetworkStream general, and with him we have to work
        /// Very carefully as a Jedi cutter groin lightsaber.
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="path"></param>
        ///<returns></returns>
        Stream GetReadStream(string domain, string path, int offset);

        ///<summary>
        /// Saves the contents of the stream in the repository.
        ///</ Summary>
        /// <param Name="domain"> </param>
        /// <param Name="path"> </param>
        /// <param Name="stream"> flow. Is read from the current position! Desirable to set to 0 when the transmission MemoryStream instance </param>
        /// <returns> </Returns>
        Uri Save(string domain, string path, Stream stream);

        /// <summary>
        /// Saves the contents of the stream in the repository.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        /// <param name="acl"></param>
        /// <returns></returns>
        Uri Save(string domain, string path, Stream stream, ACL acl);

        /// <summary>
        /// Saves the contents of the stream in the repository.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        /// <param name="attachmentFileName"></param>
        /// <returns></returns>
        Uri Save(string domain, string path, Stream stream, string attachmentFileName);

        /// <summary>
        /// Saves the contents of the stream in the repository.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        /// <param name="contentType"></param>
        /// <param name="contentDisposition"></param>
        /// <returns></returns>
        Uri Save(string domain, string path, Stream stream, string contentType, string contentDisposition);

        /// <summary>
        /// Saves the contents of the stream in the repository.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        /// <param name="contentEncoding"></param>
        /// <param name="cacheDays"></param>
        /// <returns></returns>
         Uri Save(string domain, string path, Stream stream, string contentEncoding, int cacheDays);

        string InitiateChunkedUpload(string domain, string path);

        string UploadChunk(string domain, string path, string uploadId, Stream stream, long defaultChunkSize, int chunkNumber, long chunkLength);

        Uri FinalizeChunkedUpload(string domain, string path, string uploadId, Dictionary<int, string> eTags);

        void AbortChunkedUpload(string domain, string path, string uploadId);

        bool IsSupportChunking { get; }

        bool IsSupportedPreSignedUri { get; }

        ///<summary>
        /// Deletes file
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="path"></param>
        void Delete(string domain, string path);

        ///<summary>
        /// Deletes file by mask
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="folderPath"></param>
        ///<param name="pattern">Wildcard mask (*.png)</param>
        ///<param name="recursive"></param>
        void DeleteFiles(string domain, string folderPath, string pattern, bool recursive);

        ///<summary>
        /// Deletes files
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="listPaths"></param>
        void DeleteFiles(string domain, List<string> paths);

        ///<summary>
        /// Deletes file by last modified date
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="folderPath"></param>
        ///<param name="fromDate"></param>
        ///<param name="toDate"></param>
        void DeleteFiles(string domain, string folderPath, DateTime fromDate, DateTime toDate);

        ///<summary>
        /// Moves the contents of one directory to another. s3 for a very expensive procedure.
        ///</summary>
        ///<param name="srcdomain"></param>
        ///<param name="srcdir"></param>
        ///<param name="newdomain"></param>
        ///<param name="newdir"></param>
        void MoveDirectory(string srcdomain, string srcdir, string newdomain, string newdir);

        ///<summary>
        /// Moves file
        ///</summary>
        ///<param name="srcdomain"></param>
        ///<param name="srcpath"></param>
        ///<param name="newdomain"></param>
        ///<param name="newpath"></param>
        ///<returns></returns>
        Uri Move(string srcdomain, string srcpath, string newdomain, string newpath);

        ///<summary>
        /// Saves the file in the temp. In fact, almost no different from the usual Save except that generates the file name itself. An inconvenient thing.
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="assignedPath"></param>
        ///<param name="stream"></param>
        ///<returns></returns>
        Uri SaveTemp(string domain, out string assignedPath, Stream stream);

        /// <summary>
        ///  Returns a list of links to all subfolders
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="recursive">iterate subdirectories or not</param>
        /// <returns></returns>
        string[] ListDirectoriesRelative(string domain, string path, bool recursive);

        ///<summary>
        /// Returns a list of links to all files
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="path"></param>
        ///<param name="pattern">Wildcard mask (*. jpg for example)</param>
        ///<param name="recursive">iterate subdirectories or not</param>
        ///<returns></returns>
        Uri[] ListFiles(string domain, string path, string pattern, bool recursive);

        ///<summary>
        /// Returns a list of relative paths for all files
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="path"></param>
        ///<param name="pattern">Wildcard mask (*. jpg for example)</param>
        ///<param name="recursive">iterate subdirectories or not</param>
        ///<returns></returns>
        string[] ListFilesRelative(string domain, string path, string pattern, bool recursive);

        ///<summary>
        /// Checks whether a file exists. On s3 it took long time.
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="path"></param>
        ///<returns></returns>
        bool IsFile(string domain, string path);

        ///<summary>
        /// Checks whether a directory exists. On s3 it took long time.
        ///</summary>
        ///<param name="domain"></param>
        ///<param name="path"></param>
        ///<returns></returns>
        bool IsDirectory(string domain, string path);

        void DeleteDirectory(string domain, string path);

        long GetFileSize(string domain, string path);

        long GetDirectorySize(string domain, string path);

        long ResetQuota(string domain);

        long GetUsedQuota(string domain);

        Uri Copy(string srcdomain, string path, string newdomain, string newpath);
        void CopyDirectory(string srcdomain, string dir, string newdomain, string newdir);

        //Then there are restarted methods without domain. functionally identical to the top

#pragma warning disable 1591
        Stream GetReadStream(string path);
        Uri Save(string path, Stream stream, string attachmentFileName);
        Uri Save(string path, Stream stream);
        void Delete(string path);
        void DeleteFiles(string folderPath, string pattern, bool recursive);
        Uri Move(string srcpath, string newdomain, string newpath);
        Uri SaveTemp(out string assignedPath, Stream stream);
        string[] ListDirectoriesRelative(string path, bool recursive);
        Uri[] ListFiles(string path, string pattern, bool recursive);
        bool IsFile(string path);
        bool IsDirectory(string path);
        void DeleteDirectory(string path);
        long GetFileSize(string path);
        long GetDirectorySize(string path);
        Uri Copy(string path, string newdomain, string newpath);
        void CopyDirectory(string dir, string newdomain, string newdir);
#pragma warning restore 1591


        IDataStore Configure(IDictionary<string, string> props);
        IDataStore SetQuotaController(IQuotaController controller);

        string SavePrivate(string domain, string path, Stream stream, DateTime expires);
        void DeleteExpired(string domain, string path, TimeSpan oldThreshold);

        string GetUploadForm(string domain, string directoryPath, string redirectTo, long maxUploadSize,
                             string contentType, string contentDisposition, string submitLabel);

        string GetUploadedUrl(string domain, string directoryPath);
        string GetUploadUrl();

        string GetPostParams(string domain, string directoryPath, long maxUploadSize, string contentType,
                             string contentDisposition);
    }
}