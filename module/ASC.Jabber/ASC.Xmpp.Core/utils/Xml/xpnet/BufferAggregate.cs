/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


#region using

using System;
using System.IO;

#endregion

namespace ASC.Xmpp.Core.utils.Xml.xpnet
{

    #region usings

    #endregion

    /// <summary>
    ///   Aggregate byte arrays together, so we can parse across IP packet boundaries
    /// </summary>
    public class BufferAggregate
    {
        #region Members

        /// <summary>
        /// </summary>
        private readonly MemoryStream m_stream = new MemoryStream();

        /// <summary>
        /// </summary>
        private BufNode m_head;

        /// <summary>
        /// </summary>
        private BufNode m_tail;

        #endregion

        #region Methods

        /// <summary>
        ///   Write to the buffer. Please make sure that you won't use this memory any more after you hand it in. It will get mangled.
        /// </summary>
        /// <param name="buf"> </param>
        public void Write(byte[] buf)
        {
            m_stream.Write(buf, 0, buf.Length);
            if (m_tail == null)
            {
                m_head = m_tail = new BufNode();
                m_head.buf = buf;
            }
            else
            {
                var n = new BufNode();
                n.buf = buf;
                m_tail.next = n;
                m_tail = n;
            }
        }

        /// <summary>
        ///   Get the current aggregate contents of the buffer.
        /// </summary>
        /// <returns> </returns>
        public byte[] GetBuffer()
        {
            return m_stream.ToArray();
        }

        /// <summary>
        ///   Clear the first "offset" bytes of the buffer, so they won't be parsed again.
        /// </summary>
        /// <param name="offset"> </param>
        public void Clear(int offset)
        {
            int s = 0;
            int save = -1;

            BufNode bn = null;
            for (bn = m_head; bn != null; bn = bn.next)
            {
                if (s + bn.buf.Length <= offset)
                {
                    if (s + bn.buf.Length == offset)
                    {
                        bn = bn.next;
                        break;
                    }

                    s += bn.buf.Length;
                }
                else
                {
                    save = s + bn.buf.Length - offset;
                    break;
                }
            }

            m_head = bn;
            if (m_head == null)
            {
                m_tail = null;
            }

            if (save > 0)
            {
                var buf = new byte[save];
                Buffer.BlockCopy(m_head.buf, m_head.buf.Length - save, buf, 0, save);
                m_head.buf = buf;
            }

            m_stream.SetLength(0);
            for (bn = m_head; bn != null; bn = bn.next)
            {
                m_stream.Write(bn.buf, 0, bn.buf.Length);
            }
        }

        /// <summary>
        ///   UTF8 encode the current contents of the buffer. Just for prettiness in the debugger.
        /// </summary>
        /// <returns> </returns>
        public override string ToString()
        {
            byte[] b = GetBuffer();
            return System.Text.Encoding.UTF8.GetString(b, 0, b.Length);
        }

        #endregion

        // RingBuffer of the Nieblung

        #region Nested type: BufNode

        /// <summary>
        /// </summary>
        private class BufNode
        {
            #region Members

            /// <summary>
            /// </summary>
            public byte[] buf;

            /// <summary>
            /// </summary>
            public BufNode next;

            #endregion
        }

        #endregion
    }
}