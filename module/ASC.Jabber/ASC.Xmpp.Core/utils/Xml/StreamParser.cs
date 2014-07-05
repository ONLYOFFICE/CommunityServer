/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="StreamParser.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#region using

using System;
using System.Collections;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Core.utils.Xml.xpnet;
using Encoding = System.Text.Encoding;
using UTF8Encoding = ASC.Xmpp.Core.utils.Xml.xpnet.UTF8Encoding;

#endregion

namespace ASC.Xmpp.Core.utils.Xml
{

    #region usings

    #endregion

    /// <summary>
    /// </summary>
    /// <param name="sender"> </param>
    /// <param name="ex"> </param>
    public delegate void StreamError(object sender, Exception ex);

    /// <summary>
    /// </summary>
    /// <param name="sender"> </param>
    /// <param name="e"> </param>
    public delegate void StreamStartHandler(object sender, Node e, string streamNamespace);

    public delegate void StreamHandler(object sender, Node e);

    /// <summary>
    ///   Stream Parser is a lighweight Streaming XML Parser.
    /// </summary>
    public class StreamParser
    {
        #region Events

        /// <summary>
        ///   Event for general errors
        /// </summary>
        public event StreamError OnError;

        /// <summary>
        /// </summary>
        public event StreamHandler OnStreamElement;

        /// <summary>
        /// </summary>
        public event StreamHandler OnStreamEnd;

        /// <summary>
        ///   Event for XML-Stream errors
        /// </summary>
        public event StreamError OnStreamError;

        /// <summary>
        /// </summary>
        public event StreamStartHandler OnStreamStart;

        #endregion

        #region Members

        /// <summary>
        /// </summary>
        private static readonly Encoding utf = Encoding.UTF8;

        /// <summary>
        /// </summary>
        private readonly xpnet.Encoding m_enc = new UTF8Encoding();

        /// <summary>
        /// </summary>
        private readonly NS m_ns = new NS();

        /// <summary>
        /// </summary>
        private Element current;

        /// <summary>
        /// </summary>
        private int m_Depth;

        /// <summary>
        /// </summary>
        private BufferAggregate m_buf = new BufferAggregate();

        /// <summary>
        /// </summary>
        private bool m_cdata;

        /// <summary>
        /// </summary>
        private Node m_root;

        /// <summary>
        /// </summary>
        private object thisLock = new object();

        #endregion

        #region Properties

        /// <summary>
        ///   Reset the XML Stream
        /// </summary>
        /// <param name="sr"> new Stream that is used for parsing </param>
        public long Depth
        {
            get { return m_Depth; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Reset the XML Stream
        /// </summary>
        public void Reset()
        {
            m_Depth = 0;
            m_root = null;
            current = null;
            m_cdata = false;

            m_buf = null;
            m_buf = new BufferAggregate();

            // m_buf.Clear(0);
            m_ns.Clear();
        }

        /// <summary>
        ///   Put bytes into the parser.
        /// </summary>
        /// <param name="buf"> The bytes to put into the parse stream </param>
        /// <param name="offset"> Offset into buf to start at </param>
        /// <param name="length"> Number of bytes to write </param>
        public void Push(byte[] buf, int offset, int length)
        {
            // or assert, really, but this is a little nicer.
            if (length == 0)
            {
                return;
            }

            // No locking is required.  Read() won't get called again
            // until this method returns.

            // TODO: only do this copy if we have a partial token at the
            // end of parsing.
            var copy = new byte[length];
            Buffer.BlockCopy(buf, offset, copy, 0, length);
            m_buf.Write(copy);

            byte[] b = m_buf.GetBuffer();
            int off = 0;
            TOK tok = TOK.END_TAG;
            var ct = new ContentToken();
            try
            {
                while (off < b.Length)
                {
                    if (m_cdata)
                    {
                        tok = m_enc.tokenizeCdataSection(b, off, b.Length, ct);
                    }
                    else
                    {
                        tok = m_enc.tokenizeContent(b, off, b.Length, ct);
                    }

                    switch (tok)
                    {
                        case TOK.EMPTY_ELEMENT_NO_ATTS:
                        case TOK.EMPTY_ELEMENT_WITH_ATTS:
                            StartTag(b, off, ct, tok);
                            EndTag(b, off, ct, tok);
                            break;
                        case TOK.START_TAG_NO_ATTS:
                        case TOK.START_TAG_WITH_ATTS:
                            StartTag(b, off, ct, tok);
                            break;
                        case TOK.END_TAG:
                            EndTag(b, off, ct, tok);
                            break;
                        case TOK.DATA_CHARS:
                        case TOK.DATA_NEWLINE:
                            AddText(utf.GetString(b, off, ct.TokenEnd - off));
                            break;
                        case TOK.CHAR_REF:
                        case TOK.MAGIC_ENTITY_REF:
                            AddText(new string(new[] {ct.RefChar1}));
                            break;
                        case TOK.CHAR_PAIR_REF:
                            AddText(new string(new[] {ct.RefChar1, ct.RefChar2}));
                            break;
                        case TOK.COMMENT:
                            if (current != null)
                            {
                                // <!-- 4
                                // --> 3
                                int start = off + 4*m_enc.MinBytesPerChar;
                                int end = ct.TokenEnd - off - 7*m_enc.MinBytesPerChar;
                                string text = utf.GetString(b, start, end);
                                current.AddChild(new Comment(text));
                            }

                            break;
                        case TOK.CDATA_SECT_OPEN:
                            m_cdata = true;
                            break;
                        case TOK.CDATA_SECT_CLOSE:
                            m_cdata = false;
                            break;
                        case TOK.XML_DECL:

                            // thou shalt use UTF8, and XML version 1.
                            // i shall ignore evidence to the contrary...

                            // TODO: Throw an exception if these assuptions are
                            // wrong
                            break;
                        case TOK.ENTITY_REF:
                        case TOK.PI:
#if CF
					    throw new util.NotImplementedException("Token type not implemented: " + tok);
#else
                            throw new NotImplementedException("Token type not implemented: " + tok);
#endif
                    }

                    off = ct.TokenEnd;
                }
            }
            catch (PartialTokenException)
            {
                // ignored;
            }
            catch (ExtensibleTokenException)
            {
                // ignored;
            }
            catch (Exception ex)
            {
                if (OnStreamError != null)
                {
                    OnStreamError(this, ex);
                }
            }
            finally
            {
                m_buf.Clear(off);
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        ///   If users didnt use the library correctly and had no local error handles it always crashed here and disconencted the socket. Catch this errors here now and foreward them.
        /// </summary>
        /// <param name="el"> </param>
        internal void DoRaiseOnStreamElement(Element el)
        {
            try
            {
                if (OnStreamElement != null)
                {
                    OnStreamElement(this, current);
                }
            }
            catch (Exception ex)
            {
                if (OnError != null)
                {
                    OnError(this, ex);
                }
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="offset"> </param>
        /// <param name="ct"> </param>
        /// <param name="tok"> </param>
        private void StartTag(byte[] buf, int offset, ContentToken ct, TOK tok)
        {
            m_Depth++;
            int colon;
            string name;
            string prefix;
            var ht = new Hashtable();

            m_ns.PushScope();

            // if i have attributes
            if ((tok == TOK.START_TAG_WITH_ATTS) || (tok == TOK.EMPTY_ELEMENT_WITH_ATTS))
            {
                int start;
                int end;
                string val;
                for (int i = 0; i < ct.getAttributeSpecifiedCount(); i++)
                {
                    start = ct.getAttributeNameStart(i);
                    end = ct.getAttributeNameEnd(i);
                    name = utf.GetString(buf, start, end - start);

                    start = ct.getAttributeValueStart(i);
                    end = ct.getAttributeValueEnd(i);

                    // val = utf.GetString(buf, start, end - start);
                    val = NormalizeAttributeValue(buf, start, end - start);

                    // <foo b='&amp;'/>
                    // <foo b='&amp;amp;'
                    // TODO: if val includes &amp;, it gets double-escaped
                    if (name.StartsWith("xmlns:"))
                    {
                        colon = name.IndexOf(':');
                        prefix = name.Substring(colon + 1);
                        m_ns.AddNamespace(prefix, val);
                    }
                    else if (name == "xmlns")
                    {
                        m_ns.AddNamespace(string.Empty, val);
                    }
                    else
                    {
                        ht.Add(name, val);
                    }
                }
            }

            name = utf.GetString(buf,
                                 offset + m_enc.MinBytesPerChar,
                                 ct.NameEnd - offset - m_enc.MinBytesPerChar);

            colon = name.IndexOf(':');
            string ns = string.Empty;
            prefix = null;
            if (colon > 0)
            {
                prefix = name.Substring(0, colon);
                name = name.Substring(colon + 1);
                ns = m_ns.LookupNamespace(prefix);
            }
            else
            {
                ns = m_ns.DefaultNamespace;
            }

            Element newel = ElementFactory.GetElement(prefix, name, ns);

            foreach (string attrname in ht.Keys)
            {
                newel.SetAttribute(attrname, (string) ht[attrname]);
            }

            if (m_root == null)
            {
                m_root = newel;

                // FireOnDocumentStart(m_root);
                if (OnStreamStart != null)
                {
                    OnStreamStart(this, m_root, m_ns.DefaultNamespace ?? "");
                }
            }
            else
            {
                if (current != null)
                {
                    current.AddChild(newel);
                }

                current = newel;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="offset"> </param>
        /// <param name="ct"> </param>
        /// <param name="tok"> </param>
        private void EndTag(byte[] buf, int offset, ContentToken ct, TOK tok)
        {
            m_Depth--;
            m_ns.PopScope();

            if (current == null)
            {
                // end of doc
                if (OnStreamEnd != null)
                {
                    OnStreamEnd(this, m_root);
                }

                // 				FireOnDocumentEnd();
                return;
            }

            string name = null;

            if ((tok == TOK.EMPTY_ELEMENT_WITH_ATTS) || (tok == TOK.EMPTY_ELEMENT_NO_ATTS))
            {
                name = utf.GetString(buf,
                                     offset + m_enc.MinBytesPerChar,
                                     ct.NameEnd - offset - m_enc.MinBytesPerChar);
            }
            else
            {
                name = utf.GetString(buf,
                                     offset + m_enc.MinBytesPerChar*2,
                                     ct.NameEnd - offset - m_enc.MinBytesPerChar*2);
            }

            // 			if (current.Name != name)
            // 				throw new Exception("Invalid end tag: " + name +
            // 					" != " + current.Name);
            var parent = (Element) current.Parent;
            if (parent == null)
            {
                DoRaiseOnStreamElement(current);

                // if (OnStreamElement!=null)
                // OnStreamElement(this, current);
                // FireOnElement(current);
            }

            current = parent;
        }

        /// <summary>
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="offset"> </param>
        /// <param name="length"> </param>
        /// <returns> </returns>
        /// <exception cref="NotImplementedException"></exception>
        private string NormalizeAttributeValue(byte[] buf, int offset, int length)
        {
            if (length == 0)
            {
                return null;
            }

            string val = null;
            var buffer = new BufferAggregate();
            var copy = new byte[length];
            Buffer.BlockCopy(buf, offset, copy, 0, length);
            buffer.Write(copy);
            byte[] b = buffer.GetBuffer();
            int off = 0;
            TOK tok = TOK.END_TAG;
            var ct = new ContentToken();
            try
            {
                while (off < b.Length)
                {
                    // tok = m_enc.tokenizeContent(b, off, b.Length, ct);
                    tok = m_enc.tokenizeAttributeValue(b, off, b.Length, ct);

                    switch (tok)
                    {
                        case TOK.ATTRIBUTE_VALUE_S:
                        case TOK.DATA_CHARS:
                        case TOK.DATA_NEWLINE:
                            val += utf.GetString(b, off, ct.TokenEnd - off);
                            break;
                        case TOK.CHAR_REF:
                        case TOK.MAGIC_ENTITY_REF:
                            val += new string(new[] {ct.RefChar1});
                            break;
                        case TOK.CHAR_PAIR_REF:
                            val += new string(new[] {ct.RefChar1, ct.RefChar2});
                            break;
                        case TOK.ENTITY_REF:
#if CF
						    throw new util.NotImplementedException("Token type not implemented: " + tok);
#else
                            throw new NotImplementedException("Token type not implemented: " + tok);
#endif
                    }

                    off = ct.TokenEnd;
                }
            }
            catch (PartialTokenException)
            {
                // ignored;
            }
            catch (ExtensibleTokenException)
            {
                // ignored;
            }
            catch (Exception ex)
            {
                if (OnStreamError != null)
                {
                    OnStreamError(this, ex);
                }
            }
            finally
            {
                buffer.Clear(off);
            }

            return val;
        }

        /// <summary>
        /// </summary>
        /// <param name="text"> </param>
        private void AddText(string text)
        {
            if (text == string.Empty)
            {
                return;
            }

            // Console.WriteLine("AddText:" + text);
            // Console.WriteLine(lastTOK);
            if (current != null)
            {
                Node last = current.LastNode;
                if (last != null && last.NodeType == NodeType.Text)
                {
                    last.Value = last.Value + text;
                }
                else
                {
                    current.AddChild(new Text(text));
                }
            }
        }

        #endregion

        // Stream Event Handlers
    }
}