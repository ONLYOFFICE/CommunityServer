/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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