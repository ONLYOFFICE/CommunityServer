/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Asn1.Asn1Structured.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using System.Text;

namespace Novell.Directory.Ldap.Asn1
{
    /// <summary>
    ///     This class serves as the base type for all ASN.1
    ///     structured types.
    /// </summary>
    [CLSCompliant(true)]
    public abstract class Asn1Structured : Asn1Object
    {
        private Asn1Object[] content;

        private int contentIndex;

        /*
        * Create a an Asn1 structured type with default size of 10
        *
        * @param the Asn1Identifier containing the tag for this structured type
        */

        protected internal Asn1Structured(Asn1Identifier id) : this(id, 10)
        {
        }

        /*
        * Create a an Asn1 structured type with the designated size
        *
        * @param id the Asn1Identifier containing the tag for this structured type
        *
        * @param size the size to allocate
        */

        protected internal Asn1Structured(Asn1Identifier id, int size) : base(id)
        {
            content = new Asn1Object[size];
        }

        /*
        * Create a an Asn1 structured type with default size of 10
        *
        * @param id the Asn1Identifier containing the tag for this structured type
        *
        * @param content an array containing the content
        *
        * @param size the number of items of content in the array
        */

        protected internal Asn1Structured(Asn1Identifier id, Asn1Object[] newContent, int size) : base(id)
        {
            content = newContent;
            contentIndex = size;
        }

        /// <summary>
        ///     Encodes the contents of this Asn1Structured directly to an output
        ///     stream.
        /// </summary>
        public override void encode(Asn1Encoder enc, Stream out_Renamed)
        {
            enc.encode(this, out_Renamed);
        }

        /// <summary> Decode an Asn1Structured type from an InputStream.</summary>
        [CLSCompliant(false)]
        protected internal void decodeStructured(Asn1Decoder dec, Stream in_Renamed, int len)
        {
            var componentLen = new int[1]; // collects length of component

            while (len > 0)
            {
                add(dec.decode(in_Renamed, componentLen));
                len -= componentLen[0];
            }
        }

        /// <summary>
        ///     Returns an array containing the individual ASN.1 elements
        ///     of this Asn1Structed object.
        /// </summary>
        /// <returns>
        ///     an array of Asn1Objects
        /// </returns>
        public Asn1Object[] toArray()
        {
            var cloneArray = new Asn1Object[contentIndex];
            Array.Copy(content, 0, cloneArray, 0, contentIndex);
            return cloneArray;
        }

        /// <summary>
        ///     Adds a new Asn1Object to the end of this Asn1Structured
        ///     object.
        /// </summary>
        /// <param name="value">
        ///     The Asn1Object to add to this Asn1Structured
        ///     object.
        /// </param>
        public void add(Asn1Object value_Renamed)
        {
            if (contentIndex == content.Length)
            {
                // Array too small, need to expand it, double length
                var newSize = contentIndex + contentIndex;
                var newArray = new Asn1Object[newSize];
                Array.Copy(content, 0, newArray, 0, contentIndex);
                content = newArray;
            }
            content[contentIndex++] = value_Renamed;
        }

        /// <summary>
        ///     Replaces the Asn1Object in the specified index position of
        ///     this Asn1Structured object.
        /// </summary>
        /// <param name="index">
        ///     The index into the Asn1Structured object where
        ///     this new ANS1Object will be placed.
        /// </param>
        /// <param name="value">
        ///     The Asn1Object to set in this Asn1Structured
        ///     object.
        /// </param>
        public void set_Renamed(int index, Asn1Object value_Renamed)
        {
            if (index >= contentIndex || index < 0)
            {
                throw new IndexOutOfRangeException("Asn1Structured: get: index " + index + ", size " + contentIndex);
            }
            content[index] = value_Renamed;
        }

        /// <summary>
        ///     Gets a specific Asn1Object in this structred object.
        /// </summary>
        /// <param name="index">
        ///     The index of the Asn1Object to get from
        ///     this Asn1Structured object.
        /// </param>
        public Asn1Object get_Renamed(int index)
        {
            if (index >= contentIndex || index < 0)
            {
                throw new IndexOutOfRangeException("Asn1Structured: set: index " + index + ", size " + contentIndex);
            }
            return content[index];
        }

        /// <summary>
        ///     Returns the number of Asn1Obejcts that have been encoded
        ///     into this Asn1Structured class.
        /// </summary>
        public int size()
        {
            return contentIndex;
        }

        /// <summary>
        ///     Creates a String representation of this Asn1Structured.
        ///     object.
        /// </summary>
        /// <param name="type">
        ///     the Type to put in the String representing this structured object
        /// </param>
        /// <returns>
        ///     the String representation of this object.
        /// </returns>
        [CLSCompliant(false)]
        public string ToString(string type)
        {
            var sb = new StringBuilder();

            sb.Append(type);

            for (var i = 0; i < contentIndex; i++)
            {
                sb.Append(content[i]);
                if (i != contentIndex - 1)
                    sb.Append(", ");
            }
            sb.Append(" }");

            return base.ToString() + sb;
        }
    }
}