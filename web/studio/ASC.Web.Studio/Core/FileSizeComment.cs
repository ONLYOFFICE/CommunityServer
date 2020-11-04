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
using ASC.Core.Tenants;
using Resources;

namespace ASC.Web.Studio.Core
{
    public static class FileSizeComment
    {
        public static string FileSizeExceptionString
        {
            get { return GetFileSizeExceptionString(SetupInfo.MaxUploadSize); }
        }

        public static string FileImageSizeExceptionString
        {
            get { return GetFileSizeExceptionString(SetupInfo.MaxImageUploadSize); }
        }

        public static string GetFileSizeExceptionString(long size)
        {
            return string.Format("{0} ({1}).", Resource.FileSizeMaxExceed, FilesSizeToString(size));
        }

        public static string GetPersonalFreeSpaceExceptionString(long size)
        {
            return string.Format("{0} ({1}).", Resource.PersonalFreeSpaceException, FilesSizeToString(size));
        }

        /// <summary>
        /// The maximum file size is exceeded (25 MB).
        /// </summary>
        public static Exception FileSizeException
        {
            get { return new TenantQuotaException(FileSizeExceptionString); }
        }

        /// <summary>
        /// The maximum file size is exceeded (1 MB).
        /// </summary>
        public static Exception FileImageSizeException
        {
            get { return new TenantQuotaException(FileImageSizeExceptionString); }
        }

        public static Exception GetFileSizeException(long size)
        {
            return new TenantQuotaException(GetFileSizeExceptionString(size));
        }

        public static Exception GetPersonalFreeSpaceException(long size)
        {
            return new TenantQuotaException(GetPersonalFreeSpaceExceptionString(size));
        }

        /// <summary>
        /// Get note about maximum file size
        /// </summary>
        /// <returns>Note: the file size cannot exceed 25 MB</returns>
        public static string GetFileSizeNote()
        {
            return GetFileSizeNote(true);
        }

        /// <summary>
        /// Get note about maximum file size
        /// </summary>
        /// <param name="withHtmlStrong">Highlight a word about size</param>
        /// <returns>Note: the file size cannot exceed 25 MB</returns>
        public static string GetFileSizeNote(bool withHtmlStrong)
        {
            return GetFileSizeNote(Resource.FileSizeNote, withHtmlStrong);
        }

        /// <summary>
        /// Get note about maximum file size
        /// </summary>
        /// <param name="note">Resource fromat of note</param>
        /// <param name="withHtmlStrong">Highlight a word about size</param>
        /// <returns>Note: the file size cannot exceed 25 MB</returns>
        public static string GetFileSizeNote(string note, bool withHtmlStrong)
        {
            return
                String.Format(note,
                              FilesSizeToString(SetupInfo.MaxUploadSize),
                              withHtmlStrong ? "<strong>" : string.Empty,
                              withHtmlStrong ? "</strong>" : string.Empty);
        }

        /// <summary>
        /// Get note about maximum file size of image
        /// </summary>
        /// <param name="note">Resource fromat of note</param>
        /// <param name="withHtmlStrong">Highlight a word about size</param>
        /// <returns>Note: the file size cannot exceed 1 MB</returns>
        public static string GetFileImageSizeNote(string note, bool withHtmlStrong)
        {
            return
                String.Format(note,
                              FilesSizeToString(SetupInfo.MaxImageUploadSize),
                              withHtmlStrong ? "<strong>" : string.Empty,
                              withHtmlStrong ? "</strong>" : string.Empty);
        }

        /// <summary>
        /// Generates a string the file size
        /// </summary>
        /// <param name="size">Size in bytes</param>
        /// <returns>10 b, 100 Kb, 25 Mb, 1 Gb</returns>
        public static string FilesSizeToString(long size)
        {
            var sizeNames = !string.IsNullOrEmpty(Resource.FileSizePostfix) ? Resource.FileSizePostfix.Split(',') : new[] {"bytes", "KB", "MB", "GB", "TB"};
            var power = 0;

            double resultSize = size;
            if (1024 <= resultSize)
            {
                power = (int) Math.Log(resultSize, 1024);
                power = power < sizeNames.Length ? power : sizeNames.Length - 1;
                resultSize = resultSize/Math.Pow(1024d, power);
            }
            return string.Format("{0:#,0.##} {1}", resultSize, sizeNames[power]);
        }
    }
}