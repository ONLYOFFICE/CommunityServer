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


using System.Linq;
using System.Net.Mail;

namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using IO;

    #endregion

    /// <summary>
    /// This class represents MIME header fields collection. Defined in RFC 2045.
    /// </summary>
    public class MIME_h_Collection : IEnumerable
    {
        #region Members

        private readonly List<MIME_h> m_pFields;
        private readonly MIME_h_Provider m_pProvider;
        private bool m_IsModified;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if header has modified since it was loaded.
        /// </summary>
        public bool IsModified
        {
            get
            {
                if (m_IsModified)
                {
                    return true;
                }

                foreach (MIME_h field in m_pFields)
                {
                    if (field.IsModified)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pFields.Count; }
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>Returns the element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when <b>index</b> is out of range.</exception>
        public MIME_h this[int index]
        {
            get
            {
                if (index < 0 || index >= m_pFields.Count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                return m_pFields[index];
            }
        }

        /// <summary>
        /// Gets header fields with the specified name.
        /// </summary>
        /// <param name="name">Header field name.</param>
        /// <returns>Returns header fields with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>name</b> is null reference.</exception>
        public MIME_h[] this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }

                List<MIME_h> retVal = new List<MIME_h>();
                foreach (MIME_h field in m_pFields.ToArray())
                {
                    if (string.Compare(name, field.Name, true) == 0)
                    {
                        retVal.Add(field);
                    }
                }

                return retVal.ToArray();
            }
        }

        /// <summary>
        /// Gets header fields provider.
        /// </summary>
        public MIME_h_Provider FieldsProvider
        {
            get { return m_pProvider; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="provider">Header fields provider.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>provider</b> is null reference.</exception>
        public MIME_h_Collection(MIME_h_Provider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            m_pProvider = provider;

            m_pFields = new List<MIME_h>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts a new header field into the collection at the specified location.
        /// </summary>
        /// <param name="index">The location in the collection where you want to add the item.</param>
        /// <param name="field">Header field to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when <b>index</b> is out of range.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>field</b> is null reference.</exception>
        public void Insert(int index, MIME_h field)
        {
            if (index < 0 || index > m_pFields.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            m_pFields.Insert(index, field);
            m_IsModified = true;
        }

        /// <summary>
        /// Parses and adds specified header field to the end of the collection.
        /// </summary>
        /// <param name="field">Header field string (Name: value).</param>
        /// <returns>Retunrs added header field.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>field</b> is null reference.</exception>
        public MIME_h Add(string field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            MIME_h h = m_pProvider.Parse(field);
            m_pFields.Add(h);
            m_IsModified = true;

            return h;
        }

        /// <summary>
        /// Adds specified header field to the end of the collection.
        /// </summary>
        /// <param name="field">Header field to add.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>field</b> is null reference value.</exception>
        public void Add(MIME_h field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            m_pFields.Add(field);
            m_IsModified = true;
        }

        /// <summary>
        /// Removes specified header field from the collection.
        /// </summary>
        /// <param name="field">Header field to remove.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>field</b> is null reference value.</exception>
        public void Remove(MIME_h field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            m_pFields.Remove(field);
            m_IsModified = true;
        }

        /// <summary>
        /// Removes all header fields with the specified name.
        /// </summary>
        /// <param name="name">Header field name.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>name</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void RemoveAll(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == string.Empty)
            {
                throw new ArgumentException("Argument 'name' value must be specified.", "name");
            }

            foreach (MIME_h field in m_pFields.ToArray())
            {
                if (string.Compare(name, field.Name, true) == 0)
                {
                    m_pFields.Remove(field);
                }
            }
            m_IsModified = true;
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            m_pFields.Clear();
            m_IsModified = true;
        }

        /// <summary>
        /// Gets if collection has item with the specified name.
        /// </summary>
        /// <param name="name">Header field name.</param>
        /// <returns>Returns true if specified item exists in the collection, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>name</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public bool Contains(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == string.Empty)
            {
                throw new ArgumentException("Argument 'name' value must be specified.", "name");
            }

            foreach (MIME_h field in m_pFields.ToArray())
            {
                if (string.Compare(name, field.Name, true) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets if collection contains the specified item.
        /// </summary>
        /// <param name="field">Header field.</param>
        /// <returns>Returns true if specified item exists in the collection, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>field</b> is null reference.</exception>
        public bool Contains(MIME_h field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            return m_pFields.Contains(field);
        }

        /// <summary>
        /// Gets first header field with the specified name. returns null if specified header field doesn't exist.
        /// </summary>
        /// <param name="name">Header field name.</param>
        /// <returns>Returns first header field with the specified name. returns null if specified header field doesn't exist.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>name</b> is null reference.</exception>
        public MIME_h GetFirst(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            foreach (MIME_h field in m_pFields.ToArray())
            {
                if (string.Compare(name, field.Name, true) == 0)
                {
                    return field;
                }
            }

            return null;
        }

        /// <summary>
        /// Replaces first header field with specified name with specified value.
        /// </summary>
        /// <param name="field">Hedaer field.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>field</b> is null reference.</exception>
        public void ReplaceFirst(MIME_h field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            for (int i = 0; i < m_pFields.Count; i++)
            {
                if (string.Equals(field.Name, m_pFields[i].Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    m_pFields.RemoveAt(i);
                    m_pFields.Insert(i, field);

                    return;
                }
            }
        }

        /// <summary>
        /// Copies header fields to new array.
        /// </summary>
        /// <returns>Returns header fields array.</returns>
        public MIME_h[] ToArray()
        {
            return m_pFields.ToArray();
        }

        /// <summary>
        /// Stores header to the specified file.
        /// </summary>
        /// <param name="fileName">File name with optional path.</param>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="parmetersCharset">Charset to use to encode 8-bit header parameters. Value null means parameters not encoded.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>fileName</b> is null reference.</exception>
        public void ToFile(string fileName, MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            using (FileStream fs = File.Create(fileName))
            {
                ToStream(fs, wordEncoder, parmetersCharset);
            }
        }

        /// <summary>
        /// Stores header to the specified stream.
        /// </summary>
        /// <param name="stream">Stream where to store header.</param>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="parmetersCharset">Charset to use to encode 8-bit header parameters. Value null means parameters not encoded.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        public void ToStream(Stream stream, MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            byte[] header = Encoding.UTF8.GetBytes(ToString(wordEncoder, parmetersCharset));
            stream.Write(header, 0, header.Length);
        }

        /// <summary>
        /// Returns MIME header as string.
        /// </summary>
        /// <returns>Returns MIME header as string.</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>
        /// Returns MIME header as string.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="parmetersCharset">Charset to use to encode 8-bit header parameters. Value null means parameters not encoded.</param>
        /// <returns>Returns MIME header as string.</returns>
        public string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
        {
            StringBuilder retVal = new StringBuilder();
            foreach (MIME_h field in m_pFields)
            {
                retVal.Append(field.ToString(wordEncoder, parmetersCharset));
            }

            return retVal.ToString();
        }

        /// <summary>
        /// Parses MIME header from the specified value.
        /// </summary>
        /// <param name="value">MIME header string.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public void Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Parse(new SmartStream(new MemoryStream(Encoding.UTF8.GetBytes(value)), true));
        }

        /// <summary>
        /// Parses MIME header from the specified stream.
        /// </summary>
        /// <param name="stream">MIME header stream.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
        public void Parse(SmartStream stream)
        {
            //TODO: ���� ��������� �������! �������� ����! �� ��� ���� �������� � utf8 �����

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var headers = new List<KeyValuePair<string, byte[]>>();
            var currentMemStream = new MemoryStream();
            SmartStream.ReadLineAsyncOP readLineOP = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                                     SizeExceededAction.
                                                                                         ThrowException);
            while (true)
            {
                stream.ReadLine(readLineOP, false);
                if (readLineOP.Error != null)
                {
                    throw readLineOP.Error;
                }
                // We reached end of stream.
                if (readLineOP.BytesInBuffer == 0)
                {
                    if (currentMemStream.Length > 0)
                    {
                        AddToBinaryDict(headers, currentMemStream);
                    }
                    m_IsModified = false;

                    break;
                }
                // We got blank header terminator line.
                if (readLineOP.LineBytesInBuffer == 0)
                {
                    if (currentMemStream.Length > 0)
                    {
                        AddToBinaryDict(headers, currentMemStream);
                    }
                    m_IsModified = false;

                    break;
                }

                string line = Encoding.UTF8.GetString(readLineOP.Buffer, 0, readLineOP.BytesInBuffer);
                var realBuffer = new List<byte>();

                if ((line.StartsWith("From: \"") || line.StartsWith("To: \"")) && !line.EndsWith(">\r\n"))
                {
                    var tmpArr = new byte[readLineOP.BytesInBuffer];
                    Array.Copy(readLineOP.Buffer, 0, tmpArr, 0, readLineOP.BytesInBuffer);
                    realBuffer.AddRange(tmpArr);
                    do
                    {
                        stream.ReadLine(readLineOP, false);

                        if (readLineOP.LineBytesInBuffer == 0)
                            break;

                        line = Encoding.UTF8.GetString(readLineOP.Buffer, 0, readLineOP.BytesInBuffer);

                        tmpArr = new byte[readLineOP.BytesInBuffer];
                        Array.Copy(readLineOP.Buffer, 0, tmpArr, 0, readLineOP.BytesInBuffer);
                        realBuffer.AddRange(tmpArr);

                    } while (!line.EndsWith(">\r\n"));

                    if (realBuffer.Count > 0)
                    {
                        line = Encoding.UTF8.GetString(realBuffer.ToArray());
                    }
                }
                

                // New header field starts.
                if (currentMemStream.Length == 0)
                {
                    currentMemStream.Write(readLineOP.Buffer, 0, readLineOP.BytesInBuffer);
                }
                // Header field continues.
                else if (char.IsWhiteSpace(line[0]))
                {
                    currentMemStream.Write(readLineOP.Buffer, 0, readLineOP.BytesInBuffer);
                }
                // Current header field closed, new starts.
                else
                {
                    AddToBinaryDict(headers, currentMemStream);

                    currentMemStream = new MemoryStream();
                    if (realBuffer.Count > 0)
                        currentMemStream.Write(realBuffer.ToArray(), 0, realBuffer.Count);
                    else
                        currentMemStream.Write(readLineOP.Buffer, 0, readLineOP.BytesInBuffer);
                }
            }
            //Process dictionary
            //Find content type
            var contentTypeHeader = 
                headers
                    .Where(x => x.Value != null)
                    .Where(x => "content-type".Equals(x.Key, StringComparison.OrdinalIgnoreCase))
                    .Select(x => Encoding.UTF8.GetString(x.Value))
                    .SingleOrDefault();
            var encoding = Encoding.UTF8;
            if (contentTypeHeader != null)
            {
                var mime = MIME_h_ContentType.Parse(contentTypeHeader);
                if (!string.IsNullOrEmpty(mime.Param_Charset))
                {
                    encoding = EncodingTools.GetEncodingByCodepageName(mime.Param_Charset) ?? Encoding.UTF8;
                }
                else
                {
                    //Join headers
                    var subjectRaw =
                        headers
                            .Where(x => x.Value != null)
                            .Where(x => "subject".Equals(x.Key, StringComparison.OrdinalIgnoreCase))
                            .Select(x => x.Value)
                            .SingleOrDefault();
                    //Try to detect hueristic
                    encoding = subjectRaw != null ? EncodingTools.DetectInputCodepage(subjectRaw) : Encoding.UTF8;
                }
            }

            foreach (var keyValuePair in headers)
            {
                Add(encoding.GetString(keyValuePair.Value));
            }
        }

        private void AddToBinaryDict(List<KeyValuePair<string, byte[]>> dictionary, MemoryStream ms)
        {
            var buffer = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buffer, 0, buffer.Length);
            var headerString = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            //Parse
            var headerNames = headerString.Split(':');
            if (headerNames.Length > 0)
            {
                dictionary.Add(new KeyValuePair<string, byte[]>(headerNames[0].Trim().ToLowerInvariant(), buffer));
            }
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pFields.GetEnumerator();
        }

        #endregion
    }
}