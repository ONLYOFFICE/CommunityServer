/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;
    using System.IO;
    using System.Text;
    using IO;

    #endregion

    /// <summary>
    /// This class is base class for singlepart media bodies like: text,video,audio,image.
    /// </summary>
    public abstract class MIME_b_SinglepartBase : MIME_b, IDisposable
    {
        #region Members

        private readonly Stream m_pEncodedDataStream;
        private bool m_IsModified;
        private string m_MediaType = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets if body has modified.
        /// </summary>
        public override bool IsModified
        {
            get { return m_IsModified; }
        }

        /// <summary>
        /// Gets encoded body data size in bytes.
        /// </summary>
        public int EncodedDataSize
        {
            get { return (int) m_pEncodedDataStream.Length; }
        }

        /// <summary>
        /// Gets body encoded data. 
        /// </summary>
        /// <remarks>NOTE: Use this property with care, because body data may be very big and you may run out of memory.
        /// For bigger data use <see cref="GetEncodedDataStream"/> method instead.</remarks>
        public byte[] EncodedData
        {
            get
            {
                MemoryStream ms = new MemoryStream();
                Net_Utils.StreamCopy(GetEncodedDataStream(), ms, Workaround.Definitions.MaxStreamLineLength);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Gets body decoded data.
        /// </summary>
        /// <remarks>NOTE: Use this property with care, because body data may be very big and you may run out of memory.
        /// For bigger data use <see cref="GetDataStream"/> method instead.</remarks>
        /// <exception cref="NotSupportedException">Is raised when body contains not supported Content-Transfer-Encoding.</exception>
        /// fucking idiot!
        /// 
        private byte[] _dataCached = null;

        public byte[] Data
        {
            get
            {
                if (_dataCached == null)
                {
                    using (var ms = new MemoryStream())
                    {
                        Net_Utils.StreamCopy(GetDataStream(), ms, Workaround.Definitions.MaxStreamLineLength);
                        _dataCached = ms.ToArray();
                    }
                }
                return _dataCached;
            }
        }

        /// <summary>
        /// Gets encoded data stream.
        /// </summary>
        protected Stream EncodedStream
        {
            get { return m_pEncodedDataStream; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when <b>mediaType</b> is null reference.</exception>
        public MIME_b_SinglepartBase(string mediaType) : base(new MIME_h_ContentType(mediaType))
        {
            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            m_MediaType = mediaType;

            /*m_pEncodedDataStream = new FileStream(Path.GetTempFileName(),
                                                  FileMode.Create,
                                                  FileAccess.ReadWrite,
                                                  FileShare.None,
                                                  Workaround.Definitions.MaxStreamLineLength,
                                                  FileOptions.DeleteOnClose);*/
            m_pEncodedDataStream = new MemoryStream();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets body encoded data stream.
        /// </summary>
        /// <returns>Returns body encoded data stream.</returns>
        public Stream GetEncodedDataStream()
        {
            m_pEncodedDataStream.Position = 0;

            return m_pEncodedDataStream;
        }

        /// <summary>
        /// Sets body encoded data from specified stream.
        /// </summary>
        /// <param name="contentTransferEncoding">Content-Transfer-Encoding in what encoding <b>stream</b> data is.</param>
        /// <param name="stream">Stream data to add.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>contentTransferEncoding</b> or <b>stream</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the argumennts has invalid value.</exception>
        public void SetEncodedData(string contentTransferEncoding, Stream stream)
        {
            if (contentTransferEncoding == null)
            {
                throw new ArgumentNullException("contentTransferEncoding");
            }
            if (contentTransferEncoding == string.Empty)
            {
                throw new ArgumentException("Argument 'contentTransferEncoding' value must be specified.");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            m_pEncodedDataStream.SetLength(0);
            Net_Utils.StreamCopy(stream, m_pEncodedDataStream, Workaround.Definitions.MaxStreamLineLength);

            // If body won't end with CRLF, add CRLF.
            if (m_pEncodedDataStream.Length >= 2)
            {
                m_pEncodedDataStream.Position = m_pEncodedDataStream.Length - 2;
            }
            if (m_pEncodedDataStream.ReadByte() != '\r' && m_pEncodedDataStream.ReadByte() != '\n')
            {
                m_pEncodedDataStream.Write(new[] {(byte) '\r', (byte) '\n'}, 0, 2);
            }
            Entity.ContentTransferEncoding = contentTransferEncoding;

            m_IsModified = true;
        }

        /// <summary>
        /// Gets body decoded data stream.
        /// </summary>
        /// <returns>Returns body decoded data stream.</returns>
        /// <exception cref="NotSupportedException">Is raised when body contains not supported Content-Transfer-Encoding.</exception>
        /// <remarks>The returned stream should be clossed/disposed as soon as it's not needed any more.</remarks>
        public Stream GetDataStream()
        {
            /* RFC 2045 6.1.
                This is the default value -- that is, "Content-Transfer-Encoding: 7BIT" is assumed if the
                Content-Transfer-Encoding header field is not present.
            */
            string transferEncoding = MIME_TransferEncodings.SevenBit;
            if (Entity.ContentTransferEncoding != null)
            {
                transferEncoding = Entity.ContentTransferEncoding.ToLowerInvariant();
            }

            m_pEncodedDataStream.Position = 0;
            m_pEncodedDataStream.Seek(0, SeekOrigin.Begin);
            if (transferEncoding == MIME_TransferEncodings.QuotedPrintable)
            {
                return new QuotedPrintableStream(new SmartStream(m_pEncodedDataStream, false), FileAccess.Read);
            }
            else if (transferEncoding == MIME_TransferEncodings.Base64)
            {
                return new Base64Stream(m_pEncodedDataStream, false, true, FileAccess.Read);
            }
            else if (transferEncoding == MIME_TransferEncodings.Binary)
            {
                return new ReadWriteControlledStream(m_pEncodedDataStream, FileAccess.Read);
            }
            else if (transferEncoding == MIME_TransferEncodings.EightBit)
            {
                return new ReadWriteControlledStream(m_pEncodedDataStream, FileAccess.Read);
            }
            else if (transferEncoding == MIME_TransferEncodings.SevenBit)
            {
                return new ReadWriteControlledStream(m_pEncodedDataStream, FileAccess.Read);
            }
            else
            {
                throw new NotSupportedException("Not supported Content-Transfer-Encoding '" +
                                                Entity.ContentTransferEncoding + "'.");
            }
        }

        /// <summary>
        /// Sets body data from the specified stream.
        /// </summary>
        /// <param name="stream">Source stream.</param>
        /// <param name="transferEncoding">Specifies content-transfer-encoding to use to encode data.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> or <b>transferEncoding</b> is null reference.</exception>
        public void SetData(Stream stream, string transferEncoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (transferEncoding == null)
            {
                throw new ArgumentNullException("transferEncoding");
            }

            if (transferEncoding == MIME_TransferEncodings.QuotedPrintable)
            {
                using (MemoryStream mem_stream = new MemoryStream())
                {
                    QuotedPrintableStream encoder = new QuotedPrintableStream(new SmartStream(mem_stream, false),
                                                                              FileAccess.ReadWrite);
                    Net_Utils.StreamCopy(stream, encoder, Workaround.Definitions.MaxStreamLineLength);
                    encoder.Flush();
                    mem_stream.Position = 0;
                    SetEncodedData(transferEncoding, mem_stream);
                }
            }
            else if (transferEncoding == MIME_TransferEncodings.Base64)
            {
                using (MemoryStream mem_stream = new MemoryStream())
                {
                    Base64Stream encoder = new Base64Stream(mem_stream, false, true, FileAccess.ReadWrite);
                    Net_Utils.StreamCopy(stream, encoder, Workaround.Definitions.MaxStreamLineLength);
                    encoder.Finish();
                    mem_stream.Position = 0;
                    SetEncodedData(transferEncoding, mem_stream);
                }
            }
            else if (transferEncoding == MIME_TransferEncodings.Binary)
            {
                SetEncodedData(transferEncoding, stream);
            }
            else if (transferEncoding == MIME_TransferEncodings.EightBit)
            {
                SetEncodedData(transferEncoding, stream);
            }
            else if (transferEncoding == MIME_TransferEncodings.SevenBit)
            {
                SetEncodedData(transferEncoding, stream);
            }
            else
            {
                throw new NotSupportedException("Not supported Content-Transfer-Encoding '" + transferEncoding +
                                                "'.");
            }
        }

        /// <summary>
        /// Sets body data from the specified file.
        /// </summary>
        /// <param name="file">File name with optional path.</param>
        /// <param name="transferEncoding">Specifies content-transfer-encoding to use to encode data.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>file</b> is null reference.</exception>
        public void SetBodyDataFromFile(string file, string transferEncoding)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            using (FileStream fs = File.OpenRead(file))
            {
                SetData(fs, transferEncoding);
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Stores MIME entity body to the specified stream.
        /// </summary>
        /// <param name="stream">Stream where to store body data.</param>
        /// <param name="headerWordEncoder">Header 8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="headerParmetersCharset">Charset to use to encode 8-bit header parameters. Value null means parameters not encoded.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        protected internal override void ToStream(Stream stream,
                                                  MIME_Encoding_EncodedWord headerWordEncoder,
                                                  Encoding headerParmetersCharset)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            Net_Utils.StreamCopy(GetEncodedDataStream(), stream, Workaround.Definitions.MaxStreamLineLength);
        }

        #endregion

        public void Dispose()
        {
            if (m_pEncodedDataStream != null)
            {
                m_pEncodedDataStream.Dispose();
            }
        }
    }
}