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


namespace ASC.Mail.Net.IMAP.Server
{
    #region usings

    using System;
    using System.IO;
    using System.Text;
    using Mime;

    #endregion

    /// <summary>
    /// FETCH command helper methods.
    /// </summary>
    internal class FetchHelper
    {
        #region Methods

        /// <summary>
        /// Returns requested header fields lines.
        /// Note: Header terminator blank line is included.
        /// </summary>
        /// <param name="fieldsStr">Header fields to get.</param>
        /// <param name="entity">Entity which header field lines to get.</param>
        /// <returns></returns>
        public static byte[] ParseHeaderFields(string fieldsStr, MimeEntity entity)
        {
            return ParseHeaderFields(fieldsStr, Encoding.Default.GetBytes(entity.HeaderString));
        }

        /// <summary>
        /// Returns requested header fields lines.
        /// Note: Header terminator blank line is included.
        /// </summary>
        /// <param name="fieldsStr">Header fields to get.</param>
        /// <param name="data">Message data.</param>
        /// <returns></returns>
        public static byte[] ParseHeaderFields(string fieldsStr, byte[] data)
        {
            fieldsStr = fieldsStr.Trim();
            if (fieldsStr.StartsWith("("))
            {
                fieldsStr = fieldsStr.Substring(1, fieldsStr.Length - 1);
            }
            if (fieldsStr.EndsWith(")"))
            {
                fieldsStr = fieldsStr.Substring(0, fieldsStr.Length - 1);
            }

            string retVal = "";

            string[] fields = fieldsStr.Split(' ');
            using (MemoryStream mStrm = new MemoryStream(data))
            {
                TextReader r = new StreamReader(mStrm);
                string line = r.ReadLine();

                bool fieldFound = false;
                // Loop all header lines
                while (line != null)
                {
                    // End of header
                    if (line.Length == 0)
                    {
                        break;
                    }

                    // Field continues
                    if (fieldFound && line.StartsWith("\t"))
                    {
                        retVal += line + "\r\n";
                    }
                    else
                    {
                        fieldFound = false;

                        // Check if wanted field
                        foreach (string field in fields)
                        {
                            if (line.Trim().ToLower().StartsWith(field.Trim().ToLower()))
                            {
                                retVal += line + "\r\n";
                                fieldFound = true;
                            }
                        }
                    }

                    line = r.ReadLine();
                }
            }

            // Add header terminating blank line
            retVal += "\r\n";

            return Encoding.ASCII.GetBytes(retVal);
        }

        /// <summary>
        /// Returns header fields lines except requested.
        /// Note: Header terminator blank line is included.
        /// </summary>
        /// <param name="fieldsStr">Header fields to skip.</param>
        /// <param name="entity">Entity which header field lines to get.</param>
        /// <returns></returns>
        public static byte[] ParseHeaderFieldsNot(string fieldsStr, MimeEntity entity)
        {
            return ParseHeaderFieldsNot(fieldsStr, Encoding.Default.GetBytes(entity.HeaderString));
        }

        /// <summary>
        /// Returns header fields lines except requested.
        /// Note: Header terminator blank line is included.
        /// </summary>
        /// <param name="fieldsStr">Header fields to skip.</param>
        /// <param name="data">Message data.</param>
        /// <returns></returns>
        public static byte[] ParseHeaderFieldsNot(string fieldsStr, byte[] data)
        {
            fieldsStr = fieldsStr.Trim();
            if (fieldsStr.StartsWith("("))
            {
                fieldsStr = fieldsStr.Substring(1, fieldsStr.Length - 1);
            }
            if (fieldsStr.EndsWith(")"))
            {
                fieldsStr = fieldsStr.Substring(0, fieldsStr.Length - 1);
            }

            string retVal = "";

            string[] fields = fieldsStr.Split(' ');
            using (MemoryStream mStrm = new MemoryStream(data))
            {
                TextReader r = new StreamReader(mStrm);
                string line = r.ReadLine();

                bool fieldFound = false;
                // Loop all header lines
                while (line != null)
                {
                    // End of header
                    if (line.Length == 0)
                    {
                        break;
                    }

                    // Filed continues
                    if (fieldFound && line.StartsWith("\t"))
                    {
                        retVal += line + "\r\n";
                    }
                    else
                    {
                        fieldFound = false;

                        // Check if wanted field
                        foreach (string field in fields)
                        {
                            if (line.Trim().ToLower().StartsWith(field.Trim().ToLower()))
                            {
                                fieldFound = true;
                            }
                        }

                        if (!fieldFound)
                        {
                            retVal += line + "\r\n";
                        }
                    }

                    line = r.ReadLine();
                }
            }

            return Encoding.ASCII.GetBytes(retVal);
        }

        /// <summary>
        /// Gets specified mime entity. Returns null if specified mime entity doesn't exist.
        /// </summary>
        /// <param name="parser">Reference to mime parser.</param>
        /// <param name="mimeEntitySpecifier">Mime entity specifier. Nested mime entities are pointed by '.'. 
        /// For example: 1,1.1,2.1, ... .</param>
        /// <returns></returns>
        public static MimeEntity GetMimeEntity(Mime parser, string mimeEntitySpecifier)
        {
            // TODO: nested rfc 822 message

            // For single part message there is only one entity with value 1.
            // Example:
            //		header
            //		entity -> 1

            // For multipart message, entity counting starts from MainEntity.ChildEntities
            // Example:
            //		header
            //		multipart/mixed
            //			entity1  -> 1
            //			entity2  -> 2
            //          ...

            // Single part
            if ((parser.MainEntity.ContentType & MediaType_enum.Multipart) == 0)
            {
                if (mimeEntitySpecifier.Length == 1 && Convert.ToInt32(mimeEntitySpecifier) == 1)
                {
                    return parser.MainEntity;
                }
                else
                {
                    return null;
                }
            }
                // multipart
            else
            {
                MimeEntity entity = parser.MainEntity;
                string[] parts = mimeEntitySpecifier.Split('.');
                foreach (string part in parts)
                {
                    int mEntryNo = Convert.ToInt32(part) - 1;
                        // Enitites are zero base, mimeEntitySpecifier is 1 based.
                    if (mEntryNo > -1 && mEntryNo < entity.ChildEntities.Count)
                    {
                        entity = entity.ChildEntities[mEntryNo];
                    }
                    else
                    {
                        return null;
                    }
                }

                return entity;
            }
        }

        /// <summary>
        /// Gets specified mime entity header.
        /// Note: Header terminator blank line is included.
        /// </summary>
        /// <param name="entity">Mime entity.</param>
        /// <returns></returns>
        public static byte[] GetMimeEntityHeader(MimeEntity entity)
        {
            return Encoding.ASCII.GetBytes(entity.HeaderString + "\r\n");
        }

        /// <summary>
        /// Gets requested mime entity header. Returns null if specified mime entity doesn't exist.
        /// Note: Header terminator blank line is included.
        /// </summary>
        /// <param name="parser">Reference to mime parser.</param>
        /// <param name="mimeEntitySpecifier">Mime entity specifier. Nested mime entities are pointed by '.'. 
        /// For example: 1,1.1,2.1, ... .</param>
        /// <returns>Returns requested mime entity data or NULL if requested entry doesn't exist.</returns>
        public static byte[] GetMimeEntityHeader(Mime parser, string mimeEntitySpecifier)
        {
            MimeEntity mEntry = GetMimeEntity(parser, mimeEntitySpecifier);
            if (mEntry != null)
            {
                return GetMimeEntityHeader(mEntry);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets requested mime entity data. Returns null if specified mime entity doesn't exist.
        /// </summary>
        /// <param name="parser">Reference to mime parser.</param>
        /// <param name="mimeEntitySpecifier">Mime entity specifier. Nested mime entities are pointed by '.'. 
        /// For example: 1,1.1,2.1, ... .</param>
        /// <returns>Returns requested mime entity data or NULL if requested entry doesn't exist.</returns>
        public static byte[] GetMimeEntityData(Mime parser, string mimeEntitySpecifier)
        {
            MimeEntity entity = GetMimeEntity(parser, mimeEntitySpecifier);
            if (entity != null)
            {
                return entity.DataEncoded;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Utility methods

        private static string Escape(string text)
        {
            text = text.Replace("\\", "\\\\");
            text = text.Replace("\"", "\\\"");

            return text;
        }

        #endregion
    }
}