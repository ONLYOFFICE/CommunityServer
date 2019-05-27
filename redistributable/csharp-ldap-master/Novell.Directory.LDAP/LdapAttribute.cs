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
// Novell.Directory.Ldap.LdapAttribute.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     The name and values of one attribute of a directory entry.
    ///     LdapAttribute objects are used when searching for, adding,
    ///     modifying, and deleting attributes from the directory.
    ///     LdapAttributes are often used in conjunction with an
    ///     {@link LdapAttributeSet} when retrieving or adding multiple
    ///     attributes to an entry.
    /// </summary>
    /// <seealso cref="LdapEntry">
    /// </seealso>
    /// <seealso cref="LdapAttributeSet">
    /// </seealso>
    /// <seealso cref="LdapModification">
    /// </seealso>
    public class LdapAttribute : IComparable
    {
        private class URLData
        {
            private void InitBlock(LdapAttribute enclosingInstance)
            {
                this.enclosingInstance = enclosingInstance;
            }

            private LdapAttribute enclosingInstance;

            public LdapAttribute Enclosing_Instance
            {
                get { return enclosingInstance; }
            }

            private readonly int length;
            private readonly sbyte[] data;

            public URLData(LdapAttribute enclosingInstance, sbyte[] data, int length)
            {
                InitBlock(enclosingInstance);
                this.length = length;
                this.data = data;
            }

            public int getLength()
            {
                return length;
            }

            public sbyte[] getData()
            {
                return data;
            }
        }

        /// <summary>
        ///     Returns an enumerator for the values of the attribute in byte format.
        /// </summary>
        /// <returns>
        ///     The values of the attribute in byte format.
        ///     Note: All string values will be UTF-8 encoded. To decode use the
        ///     String constructor. Example: new String( byteArray, "UTF-8" );
        /// </returns>
        public virtual IEnumerator ByteValues
        {
            get { return new ArrayEnumeration(ByteValueArray); }
        }

        /// <summary>
        ///     Returns an enumerator for the string values of an attribute.
        /// </summary>
        /// <returns>
        ///     The string values of an attribute.
        /// </returns>
        public virtual IEnumerator StringValues
        {
            get { return new ArrayEnumeration(StringValueArray); }
        }

        /// <summary>
        ///     Returns the values of the attribute as an array of bytes.
        /// </summary>
        /// <returns>
        ///     The values as an array of bytes or an empty array if there are
        ///     no values.
        /// </returns>
        [CLSCompliant(false)]
        public virtual sbyte[][] ByteValueArray
        {
            get
            {
                if (null == values)
                    return new sbyte[0][];
                var size = values.Length;
                var bva = new sbyte[size][];
                // Deep copy so application cannot change values
                for (int i = 0, u = size; i < u; i++)
                {
                    bva[i] = new sbyte[((sbyte[]) values[i]).Length];
                    Array.Copy((Array) values[i], 0, bva[i], 0, bva[i].Length);
                }
                return bva;
            }
        }

        /// <summary>
        ///     Returns the values of the attribute as an array of strings.
        /// </summary>
        /// <returns>
        ///     The values as an array of strings or an empty array if there are
        ///     no values
        /// </returns>
        public virtual string[] StringValueArray
        {
            get
            {
                if (null == values)
                    return new string[0];
                var size = values.Length;
                var sva = new string[size];
                for (var j = 0; j < size; j++)
                {
                    try
                    {
                        var encoder = Encoding.GetEncoding("utf-8");
                        var dchar = encoder.GetChars(SupportClass.ToByteArray((sbyte[]) values[j]));
//						char[] dchar = encoder.GetChars((byte[])values[j]);
                        sva[j] = new string(dchar);
//						sva[j] = new String((sbyte[]) values[j], "UTF-8");
                    }
                    catch (IOException uee)
                    {
                        // Exception should NEVER get thrown but just in case it does ...
                        throw new Exception(uee.ToString());
                    }
                }
                return sva;
            }
        }

        /// <summary>
        ///     Returns the the first value of the attribute as a <code>String</code>.
        /// </summary>
        /// <returns>
        ///     The UTF-8 encoded<code>String</code> value of the attribute's
        ///     value.  If the value wasn't a UTF-8 encoded <code>String</code>
        ///     to begin with the value of the returned <code>String</code> is
        ///     non deterministic.
        ///     If <code>this</code> attribute has more than one value the
        ///     first value is converted to a UTF-8 encoded <code>String</code>
        ///     and returned. It should be noted, that the directory may
        ///     return attribute values in any order, so that the first
        ///     value may vary from one call to another.
        ///     If the attribute has no values <code>null</code> is returned
        /// </returns>
        public virtual string StringValue
        {
            get
            {
                string rval = null;
                if (values != null)
                {
                    try
                    {
                        var encoder = Encoding.GetEncoding("utf-8");
                        var dchar = encoder.GetChars(SupportClass.ToByteArray((sbyte[]) values[0]));
//						char[] dchar = encoder.GetChars((byte[]) this.values[0]);
                        rval = new string(dchar);
                    }
                    catch (IOException use)
                    {
                        throw new Exception(use.ToString());
                    }
                }
                return rval;
            }
        }

        /// <summary>
        ///     Returns the the first value of the attribute as a byte array.
        /// </summary>
        /// <returns>
        ///     The binary value of <code>this</code> attribute or
        ///     <code>null</code> if <code>this</code> attribute doesn't have a value.
        ///     If the attribute has no values <code>null</code> is returned
        /// </returns>
        [CLSCompliant(false)]
        public virtual sbyte[] ByteValue
        {
            get
            {
                sbyte[] bva = null;
                if (values != null)
                {
                    // Deep copy so app can't change the value
                    bva = new sbyte[((sbyte[]) values[0]).Length];
                    Array.Copy((Array) values[0], 0, bva, 0, bva.Length);
                }
                return bva;
            }
        }

        /// <summary>
        ///     Returns the language subtype of the attribute, if any.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns the string, lang-ja.
        /// </summary>
        /// <returns>
        ///     The language subtype of the attribute or null if the attribute
        ///     has none.
        /// </returns>
        public virtual string LangSubtype
        {
            get
            {
                if (subTypes != null)
                {
                    for (var i = 0; i < subTypes.Length; i++)
                    {
                        if (subTypes[i].StartsWith("lang-"))
                        {
                            return subTypes[i];
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        ///     Returns the name of the attribute.
        /// </summary>
        /// <returns>
        ///     The name of the attribute.
        /// </returns>
        public virtual string Name
        {
            get { return name; }
        }

        /// <summary>
        ///     Replaces all values with the specified value. This protected method is
        ///     used by sub-classes of LdapSchemaElement because the value cannot be set
        ///     with a contructor.
        /// </summary>
        protected internal virtual string Value
        {
            set
            {
                values = null;
                try
                {
                    var encoder = Encoding.GetEncoding("utf-8");
                    var ibytes = encoder.GetBytes(value);
                    var sbytes = SupportClass.ToSByteArray(ibytes);

                    add(sbytes);
                }
                catch (IOException ue)
                {
                    throw new Exception(ue.ToString());
                }
            }
        }

        private readonly string name; // full attribute name
        private readonly string baseName; // cn of cn;lang-ja;phonetic
        private readonly string[] subTypes; // lang-ja of cn;lang-ja
        private object[] values; // Array of byte[] attribute values

        /// <summary>
        ///     Constructs an attribute with copies of all values of the input
        ///     attribute.
        /// </summary>
        /// <param name="attr">
        ///     An LdapAttribute to use as a template.
        ///     @throws IllegalArgumentException if attr is null
        /// </param>
        public LdapAttribute(LdapAttribute attr)
        {
            if (attr == null)
            {
                throw new ArgumentException("LdapAttribute class cannot be null");
            }
            // Do a deep copy of the LdapAttribute template
            name = attr.name;
            baseName = attr.baseName;
            if (null != attr.subTypes)
            {
                subTypes = new string[attr.subTypes.Length];
                Array.Copy(attr.subTypes, 0, subTypes, 0, subTypes.Length);
            }
            // OK to just copy attributes, as the app only sees a deep copy of them
            if (null != attr.values)
            {
                values = new object[attr.values.Length];
                Array.Copy(attr.values, 0, values, 0, values.Length);
            }
        }

        /// <summary>
        ///     Constructs an attribute with no values.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        ///     @throws IllegalArgumentException if attrName is null
        /// </param>
        public LdapAttribute(string attrName)
        {
            if ((object) attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }
            name = attrName;
            baseName = getBaseName(attrName);
            subTypes = getSubtypes(attrName);
        }

        /// <summary>
        ///     Constructs an attribute with a byte-formatted value.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     @throws IllegalArgumentException if attrName or attrBytes is null
        /// </param>
        [CLSCompliant(false)]
        public LdapAttribute(string attrName, sbyte[] attrBytes) : this(attrName)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            // Make our own copy of the byte array to prevent app from changing it
            var tmp = new sbyte[attrBytes.Length];
            Array.Copy(attrBytes, 0, tmp, 0, attrBytes.Length);
            add(tmp);
        }

        /// <summary>
        ///     Constructs an attribute with a single string value.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrString">
        ///     Value of the attribute as a string.
        ///     @throws IllegalArgumentException if attrName or attrString is null
        /// </param>
        public LdapAttribute(string attrName, string attrString) : this(attrName)
        {
            if ((object) attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            try
            {
                var encoder = Encoding.GetEncoding("utf-8");
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = SupportClass.ToSByteArray(ibytes);

                add(sbytes);
            }
            catch (IOException e)
            {
                throw new Exception(e.ToString());
            }
        }

        /// <summary>
        ///     Constructs an attribute with an array of string values.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrStrings">
        ///     Array of values as strings.
        ///     @throws IllegalArgumentException if attrName, attrStrings, or a member
        ///     of attrStrings is null
        /// </param>
        public LdapAttribute(string attrName, string[] attrStrings) : this(attrName)
        {
            if (attrStrings == null)
            {
                throw new ArgumentException("Attribute values array cannot be null");
            }
            for (int i = 0, u = attrStrings.Length; i < u; i++)
            {
                try
                {
                    if ((object) attrStrings[i] == null)
                    {
                        throw new ArgumentException("Attribute value " + "at array index " + i + " cannot be null");
                    }
                    var encoder = Encoding.GetEncoding("utf-8");
                    var ibytes = encoder.GetBytes(attrStrings[i]);
                    var sbytes = SupportClass.ToSByteArray(ibytes);
                    add(sbytes);
//					this.add(attrStrings[i].getBytes("UTF-8"));
                }
                catch (IOException e)
                {
                    throw new Exception(e.ToString());
                }
            }
        }

        /// <summary>
        ///     Returns a clone of this LdapAttribute.
        /// </summary>
        /// <returns>
        ///     clone of this LdapAttribute.
        /// </returns>
        public object Clone()
        {
            try
            {
                var newObj = MemberwiseClone();
                if (values != null)
                {
                    Array.Copy(values, 0, ((LdapAttribute) newObj).values, 0, values.Length);
                }
                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }

        /// <summary>
        ///     Adds a string value to the attribute.
        /// </summary>
        /// <param name="attrString">
        ///     Value of the attribute as a String.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void addValue(string attrString)
        {
            if ((object) attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            try
            {
                var encoder = Encoding.GetEncoding("utf-8");
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = SupportClass.ToSByteArray(ibytes);
                add(sbytes);
//				this.add(attrString.getBytes("UTF-8"));
            }
            catch (IOException ue)
            {
                throw new Exception(ue.ToString());
            }
        }

        /// <summary>
        ///     Adds a byte-formatted value to the attribute.
        /// </summary>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     @throws IllegalArgumentException if attrBytes is null
        /// </param>
        [CLSCompliant(false)]
        public virtual void addValue(sbyte[] attrBytes)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            add(attrBytes);
        }

        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  String
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrString">
        ///     The base64 value of the attribute as a String.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void addBase64Value(string attrString)
        {
            if ((object) attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            add(Base64.decode(attrString));
        }

        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  Character
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrString">
        ///     The base64 value of the attribute as a StringBuffer.
        /// </param>
        /// <param name="start">
        ///     The start index of base64 encoded part, inclusive.
        /// </param>
        /// <param name="end">
        ///     The end index of base encoded part, exclusive.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void addBase64Value(StringBuilder attrString, int start, int end)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            add(Base64.decode(attrString, start, end));
        }

        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  Character
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrChars">
        ///     The base64 value of the attribute as an array of
        ///     characters.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void addBase64Value(char[] attrChars)
        {
            if (attrChars == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            add(Base64.decode(attrChars));
        }

        /// <summary>
        ///     Adds a URL, indicating a file or other resource that contains
        ///     the value of the attribute.
        /// </summary>
        /// <param name="url">
        ///     String value of a URL pointing to the resource containing
        ///     the value of the attribute.
        ///     @throws IllegalArgumentException if url is null
        /// </param>
        public virtual void addURLValue(string url)
        {
            if ((object) url == null)
            {
                throw new ArgumentException("Attribute URL cannot be null");
            }
            addURLValue(new Uri(url));
        }

        /// <summary>
        ///     Adds a URL, indicating a file or other resource that contains
        ///     the value of the attribute.
        /// </summary>
        /// <param name="url">
        ///     A URL class pointing to the resource containing the value
        ///     of the attribute.
        ///     @throws IllegalArgumentException if url is null
        /// </param>
        public virtual void addURLValue(Uri url)
        {
            // Class to encapsulate the data bytes and the length
            if (url == null)
            {
                throw new ArgumentException("Attribute URL cannot be null");
            }
            try
            {
                // Get InputStream from the URL
                var webRequest = WebRequest.Create(url);
                var in_Renamed = webRequest.GetResponseAsync().ResultAndUnwrap().GetResponseStream();
                // Read the bytes into buffers and store the them in an arraylist
                var bufs = new ArrayList();
                var buf = new sbyte[4096];
                int len, totalLength = 0;
                while ((len = SupportClass.ReadInput(in_Renamed, ref buf, 0, 4096)) != -1)
                {
                    bufs.Add(new URLData(this, buf, len));
                    buf = new sbyte[4096];
                    totalLength += len;
                }
                /*
                * Now that the length is known, allocate an array to hold all
                * the bytes of data and copy the data to that array, store
                * it in this LdapAttribute
                */
                var data = new sbyte[totalLength];
                var offset = 0; //
                for (var i = 0; i < bufs.Count; i++)
                {
                    var b = (URLData) bufs[i];
                    len = b.getLength();
                    Array.Copy(b.getData(), 0, data, offset, len);
                    offset += len;
                }
                add(data);
            }
            catch (IOException ue)
            {
                throw new Exception(ue.ToString());
            }
        }

        /// <summary>
        ///     Returns the base name of the attribute.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns cn.
        /// </summary>
        /// <returns>
        ///     The base name of the attribute.
        /// </returns>
        public virtual string getBaseName()
        {
            return baseName;
        }

        /// <summary>
        ///     Returns the base name of the specified attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns cn.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute from which to extract the
        ///     base name.
        /// </param>
        /// <returns>
        ///     The base name of the attribute.
        ///     @throws IllegalArgumentException if attrName is null
        /// </returns>
        public static string getBaseName(string attrName)
        {
            if ((object) attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }
            var idx = attrName.IndexOf(';');
            if (-1 == idx)
            {
                return attrName;
            }
            return attrName.Substring(0, idx - 0);
        }

        /// <summary>
        ///     Extracts the subtypes from the attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns an array containing lang-ja and phonetic.
        /// </summary>
        /// <returns>
        ///     An array subtypes or null if the attribute has none.
        /// </returns>
        public virtual string[] getSubtypes()
        {
            return subTypes;
        }

        /// <summary>
        ///     Extracts the subtypes from the specified attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns an array containing lang-ja and phonetic.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute from which to extract
        ///     the subtypes.
        /// </param>
        /// <returns>
        ///     An array subtypes or null if the attribute has none.
        ///     @throws IllegalArgumentException if attrName is null
        /// </returns>
        public static string[] getSubtypes(string attrName)
        {
            if ((object) attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }
            var st = new SupportClass.Tokenizer(attrName, ";");
            string[] subTypes = null;
            var cnt = st.Count;
            if (cnt > 0)
            {
                st.NextToken(); // skip over basename
                subTypes = new string[cnt - 1];
                var i = 0;
                while (st.HasMoreTokens())
                {
                    subTypes[i++] = st.NextToken();
                }
            }
            return subTypes;
        }

        /// <summary>
        ///     Reports if the attribute name contains the specified subtype.
        ///     For example, if you check for the subtype lang-en and the
        ///     attribute name is cn;lang-en, this method returns true.
        /// </summary>
        /// <param name="subtype">
        ///     The single subtype to check for.
        /// </param>
        /// <returns>
        ///     True, if the attribute has the specified subtype;
        ///     false, if it doesn't.
        ///     @throws IllegalArgumentException if subtype is null
        /// </returns>
        public virtual bool hasSubtype(string subtype)
        {
            if ((object) subtype == null)
            {
                throw new ArgumentException("subtype cannot be null");
            }
            if (null != subTypes)
            {
                for (var i = 0; i < subTypes.Length; i++)
                {
                    if (subTypes[i].ToUpper().Equals(subtype.ToUpper()))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Reports if the attribute name contains all the specified subtypes.
        ///     For example, if you check for the subtypes lang-en and phonetic
        ///     and if the attribute name is cn;lang-en;phonetic, this method
        ///     returns true. If the attribute name is cn;phonetic or cn;lang-en,
        ///     this method returns false.
        /// </summary>
        /// <param name="subtypes">
        ///     An array of subtypes to check for.
        /// </param>
        /// <returns>
        ///     True, if the attribute has all the specified subtypes;
        ///     false, if it doesn't have all the subtypes.
        ///     @throws IllegalArgumentException if subtypes is null or if array member
        ///     is null.
        /// </returns>
        public virtual bool hasSubtypes(string[] subtypes)
        {
            if (subtypes == null)
            {
                throw new ArgumentException("subtypes cannot be null");
            }
            for (var i = 0; i < subtypes.Length; i++)
            {
                for (var j = 0; j < subTypes.Length; j++)
                {
                    if ((object) subTypes[j] == null)
                    {
                        throw new ArgumentException("subtype " + "at array index " + i + " cannot be null");
                    }
                    if (subTypes[j].ToUpper().Equals(subtypes[i].ToUpper()))
                    {
                        goto gotSubType;
                    }
                }
                return false;
                gotSubType:
                ;
            }
            return true;
        }

        /// <summary>
        ///     Removes a string value from the attribute.
        /// </summary>
        /// <param name="attrString">
        ///     Value of the attribute as a string.
        ///     Note: Removing a value which is not present in the attribute has
        ///     no effect.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void removeValue(string attrString)
        {
            if (null == (object) attrString)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            try
            {
                var encoder = Encoding.GetEncoding("utf-8");
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = SupportClass.ToSByteArray(ibytes);
                removeValue(sbytes);
//				this.removeValue(attrString.getBytes("UTF-8"));
            }
            catch (IOException uee)
            {
                // This should NEVER happen but just in case ...
                throw new Exception(uee.ToString());
            }
        }

        /// <summary>
        ///     Removes a byte-formatted value from the attribute.
        /// </summary>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     Example: <code>String.getBytes("UTF-8");</code>
        ///     Note: Removing a value which is not present in the attribute has
        ///     no effect.
        ///     @throws IllegalArgumentException if attrBytes is null
        /// </param>
        [CLSCompliant(false)]
        public virtual void removeValue(sbyte[] attrBytes)
        {
            if (null == attrBytes)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            for (var i = 0; i < values.Length; i++)
            {
                if (equals(attrBytes, (sbyte[]) values[i]))
                {
                    if (0 == i && 1 == values.Length)
                    {
                        // Optimize if first element of a single valued attr
                        values = null;
                        return;
                    }
                    if (values.Length == 1)
                    {
                        values = null;
                    }
                    else
                    {
                        var moved = values.Length - i - 1;
                        var tmp = new object[values.Length - 1];
                        if (i != 0)
                        {
                            Array.Copy(values, 0, tmp, 0, i);
                        }
                        if (moved != 0)
                        {
                            Array.Copy(values, i + 1, tmp, i, moved);
                        }
                        values = tmp;
                        tmp = null;
                    }
                    break;
                }
            }
        }

        /// <summary>
        ///     Returns the number of values in the attribute.
        /// </summary>
        /// <returns>
        ///     The number of values in the attribute.
        /// </returns>
        public virtual int size()
        {
            return null == values ? 0 : values.Length;
        }

        /// <summary>
        ///     Compares this object with the specified object for order.
        ///     Ordering is determined by comparing attribute names (see
        ///     {@link #getName() }) using the method compareTo() of the String class.
        /// </summary>
        /// <param name="attribute">
        ///     The LdapAttribute to be compared to this object.
        /// </param>
        /// <returns>
        ///     Returns a negative integer, zero, or a positive
        ///     integer as this object is less than, equal to, or greater than the
        ///     specified object.
        /// </returns>
        public virtual int CompareTo(object attribute)
        {
            return name.CompareTo(((LdapAttribute) attribute).name);
        }

        /// <summary>
        ///     Adds an object to <code>this</code> object's list of attribute values
        /// </summary>
        /// <param name="bytes">
        ///     Ultimately all of this attribute's values are treated
        ///     as binary data so we simplify the process by requiring
        ///     that all data added to our list is in binary form.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        /// </param>
        private void add(sbyte[] bytes)
        {
            if (null == values)
            {
                values = new object[] {bytes};
            }
            else
            {
                // Duplicate attribute values not allowed
                for (var i = 0; i < values.Length; i++)
                {
                    if (equals(bytes, (sbyte[]) values[i]))
                    {
                        return; // Duplicate, don't add
                    }
                }
                var tmp = new object[values.Length + 1];
                Array.Copy(values, 0, tmp, 0, values.Length);
                tmp[values.Length] = bytes;
                values = tmp;
                tmp = null;
            }
        }

        /// <summary>
        ///     Returns true if the two specified arrays of bytes are equal to each
        ///     another.  Matches the logic of Arrays.equals which is not available
        ///     in jdk 1.1.x.
        /// </summary>
        /// <param name="e1">
        ///     the first array to be tested
        /// </param>
        /// <param name="e2">
        ///     the second array to be tested
        /// </param>
        /// <returns>
        ///     true if the two arrays are equal
        /// </returns>
        private bool equals(sbyte[] e1, sbyte[] e2)
        {
            // If same object, they compare true
            if (e1 == e2)
                return true;

            // If either but not both are null, they compare false
            if (e1 == null || e2 == null)
                return false;

            // If arrays have different length, they compare false
            var length = e1.Length;
            if (e2.Length != length)
                return false;

            // If any of the bytes are different, they compare false
            for (var i = 0; i < length; i++)
            {
                if (e1[i] != e2[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Returns a string representation of this LdapAttribute
        /// </summary>
        /// <returns>
        ///     a string representation of this LdapAttribute
        /// </returns>
        public override string ToString()
        {
            var result = new StringBuilder("LdapAttribute: ");
            try
            {
                result.Append("{type='" + name + "'");
                if (values != null)
                {
                    result.Append(", ");
                    if (values.Length == 1)
                    {
                        result.Append("value='");
                    }
                    else
                    {
                        result.Append("values='");
                    }
                    for (var i = 0; i < values.Length; i++)
                    {
                        if (i != 0)
                        {
                            result.Append("','");
                        }
                        if (((sbyte[]) values[i]).Length == 0)
                        {
                            continue;
                        }
                        var encoder = Encoding.GetEncoding("utf-8");
//						char[] dchar = encoder.GetChars((byte[]) values[i]);
                        var dchar = encoder.GetChars(SupportClass.ToByteArray((sbyte[]) values[i]));
                        var sval = new string(dchar);

//						System.String sval = new String((sbyte[]) values[i], "UTF-8");
                        if (sval.Length == 0)
                        {
                            // didn't decode well, must be binary
                            result.Append("<binary value, length:" + sval.Length);
                            continue;
                        }
                        result.Append(sval);
                    }
                    result.Append("'");
                }
                result.Append("}");
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return result.ToString();
        }
    }
}