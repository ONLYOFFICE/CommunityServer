/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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