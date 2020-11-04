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


namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.IO;
    using IO;

    #endregion

    /// <summary>
    /// Represents a MIME message. Defined in RFC 2045 2.3.
    /// </summary>
    public class MIME_Message : MIME_Entity
    {
        #region Members

        private bool m_IsDisposed = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets all MIME entities as list.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is disposed and this property is accessed.</exception>
        public MIME_Entity[] AllEntities
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                List<MIME_Entity> retVal = new List<MIME_Entity>();
                Queue<MIME_Entity> entitiesQueue = new Queue<MIME_Entity>();
                entitiesQueue.Enqueue(this);

                while (entitiesQueue.Count > 0)
                {
                    MIME_Entity currentEntity = entitiesQueue.Dequeue();

                    // Current entity is multipart entity, add it's body-parts for processing.
                    if (Body != null && currentEntity.Body.GetType().IsSubclassOf(typeof (MIME_b_Multipart)))
                    {
                        foreach (MIME_Entity bodypart in ((MIME_b_Multipart) currentEntity.Body).BodyParts)
                        {
                            entitiesQueue.Enqueue(bodypart);
                        }
                    }

                    retVal.Add(currentEntity);
                }

                return retVal.ToArray();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses MIME message from the specified file.
        /// </summary>
        /// <param name="file">File name with path from where to parse MIME message.</param>
        /// <returns>Returns parsed MIME message.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>file</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static MIME_Message ParseFromFile(string file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            if (file == "")
            {
                throw new ArgumentException("Argument 'file' value must be specified.");
            }

            return ParseFromStream(File.OpenRead(file));
        }

        /// <summary>
        /// Parses MIME message from the specified stream.
        /// </summary>
        /// <param name="stream">Stream from where to parse MIME message. Parsing starts from current stream position.</param>
        /// <returns>Returns parsed MIME message.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
        public static MIME_Message ParseFromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            MIME_Message retVal = new MIME_Message();
            retVal.Parse(new SmartStream(stream, false), new MIME_h_ContentType("text/plain"));

            return retVal;
        }

        /// <summary>
        /// Creates attachment entity.
        /// </summary>
        /// <param name="file">File name with optional path.</param>
        /// <returns>Returns created attachment entity.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>file</b> is null reference.</exception>
        public static MIME_Entity CreateAttachment(string file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            MIME_Entity retVal = new MIME_Entity();
            MIME_b_Application body = new MIME_b_Application(MIME_MediaTypes.Application.octet_stream);
            retVal.Body = body;
            body.SetBodyDataFromFile(file, MIME_TransferEncodings.Base64);
            retVal.ContentType.Param_Name = Path.GetFileName(file);

            FileInfo fileInfo = new FileInfo(file);
            MIME_h_ContentDisposition disposition =
                new MIME_h_ContentDisposition(MIME_DispositionTypes.Attachment);
            disposition.Param_FileName = Path.GetFileName(file);
            disposition.Param_Size = fileInfo.Length;
            disposition.Param_CreationDate = fileInfo.CreationTime;
            disposition.Param_ModificationDate = fileInfo.LastWriteTime;
            disposition.Param_ReadDate = fileInfo.LastAccessTime;
            retVal.ContentDisposition = disposition;

            return retVal;
        }

        /// <summary>
        /// Gets MIME entity with the specified Content-ID. Returns null if no such entity.
        /// </summary>
        /// <param name="cid">Content ID.</param>
        /// <returns>Returns MIME entity with the specified Content-ID or null if no such entity.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this class is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>cid</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public MIME_Entity GetEntityByCID(string cid)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (cid == null)
            {
                throw new ArgumentNullException("cid");
            }
            if (cid == "")
            {
                throw new ArgumentException("Argument 'cid' value must be specified.", "cid");
            }

            foreach (MIME_Entity entity in AllEntities)
            {
                if (entity.ContentID == cid)
                {
                    return entity;
                }
            }

            return null;
        }

        #endregion

        // TODO:
        //public MIME_Entity GetEntityByPartsSpecifier(string partsSpecifier)
    }
}