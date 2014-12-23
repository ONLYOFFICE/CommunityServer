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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using ASC.Xmpp.Core.protocol;

#endregion

#if NET_2
#endif

//using System.Linq;
//using System.Linq.Expressions;

namespace ASC.Xmpp.Core.utils.Xml.Dom
{

    #region usings

    #endregion

    /// <summary>
    /// </summary>
    public class Element : Node
    {
        // Member Variables
        /// <summary>
        /// </summary>
        private string m_TagName;

        /// <summary>
        /// </summary>
        private ListDictionary m_Attributes;

        /// <summary>
        /// </summary>
        private readonly Text m_Value = new Text();

        /// <summary>
        /// </summary>
        public Element()
        {
            NodeType = NodeType.Element;
            AddChild(m_Value);

            m_Attributes = new ListDictionary();

            m_TagName = string.Empty;
            Value = string.Empty;
        }

        /* 
         * don't think we need this 2 constructors anymore
         * 
        public Element(NodeType type)
        {
            this.NodeType = NodeType.Element;
            this.AddChild(m_Value);

            m_Attributes = new ListDictionary();

            NodeType = type;
        }

        public Element(NodeType type, string text)
        {
            this.NodeType = NodeType.Element;
            this.AddChild(m_Value);

            m_Attributes = new ListDictionary();

            NodeType = type;
            Value = text;
        }
         */

        /// <summary>
        /// </summary>
        /// <param name="tagName"> </param>
        public Element(string tagName) : this()
        {
            m_TagName = tagName;
        }

        /// <summary>
        /// </summary>
        /// <param name="tagName"> </param>
        /// <param name="tagText"> </param>
        public Element(string tagName, string tagText) : this(tagName)
        {
            Value = tagText;
        }

        /// <summary>
        /// </summary>
        /// <param name="tagName"> </param>
        /// <param name="tagText"> </param>
        public Element(string tagName, bool tagText) : this(tagName, tagText ? "true" : "false")
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="tagName"> </param>
        /// <param name="tagText"> </param>
        /// <param name="ns"> </param>
        public Element(string tagName, string tagText, string ns) : this(tagName, tagText)
        {
            Namespace = ns;
        }

        /// <summary>
        ///   Is this Element a Rootnode?
        /// </summary>
        public bool IsRootElement
        {
            get { return Parent != null ? false : true; }
        }

        /// <summary>
        /// </summary>
        public override string Value
        {
            get { return m_Value.Value; }

            set { m_Value.Value = value; }
        }

        /// <summary>
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        ///   The Full Qualified Name
        /// </summary>
        public string TagName
        {
            get { return m_TagName; }

            set { m_TagName = value; }
        }

        /// <summary>
        /// </summary>
        public string TextBase64
        {
            get
            {
                byte[] b = Convert.FromBase64String(Value);
                return Encoding.UTF8.GetString(b);
            }

            set
            {
                byte[] b = Encoding.UTF8.GetBytes(value);
                Value = Convert.ToBase64String(b);
            }
        }

        /// <summary>
        /// </summary>
        public ListDictionary Attributes
        {
            get { return m_Attributes; }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="enumType"> </param>
        /// <returns> </returns>
        public object GetAttributeEnum(string name, Type enumType)
        {
            string att = GetAttribute(name);
            if (att == null)
            {
                return -1;
            }

            try
            {
#if CF
				return util.Enum.Parse(enumType, att, true);
#else
                return System.Enum.Parse(enumType, att, true);
#endif
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public string GetAttribute(string name)
        {
            if (HasAttribute(name))
            {
                return (string) m_Attributes[name];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public int GetAttributeInt(string name)
        {
            if (HasAttribute(name))
            {
                return int.Parse((string) m_Attributes[name]);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public long GetAttributeLong(string name)
        {
            if (HasAttribute(name))
            {
                return long.Parse((string) m_Attributes[name]);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        ///   Reads a boolean Attribute, if the attrib is absent it returns also false.
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public bool GetAttributeBool(string name)
        {
            if (HasAttribute(name))
            {
                var tmp = (string) m_Attributes[name];
                if (tmp.ToLower() == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public Jid GetAttributeJid(string name)
        {
            if (HasAttribute(name))
            {
                return new Jid(GetAttribute(name));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="ifp"> </param>
        /// <returns> </returns>
        public double GetAttributeDouble(string name, IFormatProvider ifp)
        {
            if (HasAttribute(name))
            {
                try
                {
                    return double.Parse((string) m_Attributes[name], ifp);
                }
                catch
                {
                    return double.NaN;
                }
            }
            else
            {
                return double.NaN;
            }
        }

        /// <summary>
        ///   Get a Attribute of type double (Decimal seperator = ".")
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public double GetAttributeDouble(string name)
        {
            // Parse the double always in english format ==> "." = Decimal seperator
            var nfi = new NumberFormatInfo();
            nfi.NumberGroupSeparator = ".";
            return GetAttributeDouble(name, nfi);
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public bool HasAttribute(string name)
        {
            return Attributes.Contains(name);
        }

        /// <summary>
        ///   Return the Text of the first Tag with a specified Name. It doesnt traverse the while tree and checks only the unerlying childnodes
        /// </summary>
        /// <param name="TagName"> Name of Tag to find as string </param>
        /// <returns> </returns>
        public string GetTag(string TagName)
        {
            Element tag = _SelectElement(this, TagName);
            if (tag != null)
            {
                return tag.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="TagName"> </param>
        /// <param name="traverseChildren"> </param>
        /// <returns> </returns>
        public string GetTag(string TagName, bool traverseChildren)
        {
            Element tag = _SelectElement(this, TagName, traverseChildren);
            if (tag != null)
            {
                return tag.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="type"> </param>
        /// <returns> </returns>
        public string GetTag(Type type)
        {
            Element tag = _SelectElement(this, type);
            if (tag != null)
            {
                return tag.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="TagName"> </param>
        /// <returns> </returns>
        public string GetTagBase64(string TagName)
        {
            byte[] b = Convert.FromBase64String(GetTag(TagName));
            return Encoding.UTF8.GetString(b);
        }

        /// <summary>
        ///   Adds a Tag and encodes the Data to BASE64
        /// </summary>
        /// <param name="argTagname"> </param>
        /// <param name="argText"> </param>
        public void SetTagBase64(string argTagname, string argText)
        {
            byte[] b = Encoding.UTF8.GetBytes(argText);
            SetTag(argTagname, Convert.ToBase64String(b));
        }

        /// <summary>
        ///   Adds a Tag end decodes the byte buffer to BASE64
        /// </summary>
        /// <param name="argTagname"> </param>
        /// <param name="buffer"> </param>
        public void SetTagBase64(string argTagname, byte[] buffer)
        {
            SetTag(argTagname, Convert.ToBase64String(buffer));
        }

        /// <summary>
        /// </summary>
        /// <param name="argTagname"> </param>
        /// <param name="argText"> </param>
        public void SetTag(string argTagname, string argText)
        {
            if (HasTag(argTagname) == false)
            {
                AddChild(new Element(argTagname, argText));
            }
            else
            {
                SelectSingleElement(argTagname).Value = argText;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="argText"> </param>
        public void SetTag(Type type, string argText)
        {
            if (HasTag(type) == false)
            {
                Element newel;
                newel = (Element) Activator.CreateInstance(type);
                newel.Value = argText;
                AddChild(newel);
            }
            else
            {
                SelectSingleElement(type).Value = argText;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="type"> </param>
        public void SetTag(Type type)
        {
            if (HasTag(type))
            {
                RemoveTag(type);
            }

            AddChild((Element) Activator.CreateInstance(type));
        }

        /// <summary>
        /// </summary>
        /// <param name="argTagname"> </param>
        public void SetTag(string argTagname)
        {
            SetTag(argTagname, string.Empty);
        }

        /// <summary>
        /// </summary>
        /// <param name="argTagname"> </param>
        /// <param name="argText"> </param>
        /// <param name="argNS"> </param>
        public void SetTag(string argTagname, string argText, string argNS)
        {
            if (HasTag(argTagname) == false)
            {
                AddChild(new Element(argTagname, argText, argNS));
            }
            else
            {
                Element e = SelectSingleElement(argTagname);
                e.Value = argText;
                e.Namespace = argNS;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="argTagname"> </param>
        /// <param name="dbl"> </param>
        /// <param name="ifp"> </param>
        public void SetTag(string argTagname, double dbl, IFormatProvider ifp)
        {
            SetTag(argTagname, dbl.ToString(ifp));
        }

        /// <summary>
        /// </summary>
        /// <param name="argTagname"> </param>
        /// <param name="dbl"> </param>
        public void SetTag(string argTagname, double dbl)
        {
            var nfi = new NumberFormatInfo();
            nfi.NumberGroupSeparator = ".";
            SetTag(argTagname, dbl, nfi);
        }

        /// <summary>
        /// </summary>
        /// <param name="argTagname"> </param>
        /// <param name="val"> </param>
        public void SetTag(string argTagname, bool val)
        {
            SetTag(argTagname, val ? "true" : "false");
        }

        /// <summary>
        /// </summary>
        /// <param name="argTagname"> </param>
        /// <param name="val"> </param>
        public void SetTag(string argTagname, int val)
        {
            SetTag(argTagname, val.ToString());
        }

        /// <summary>
        /// </summary>
        /// <param name="argTagname"> </param>
        /// <param name="jid"> </param>
        public void SetTag(string argTagname, Jid jid)
        {
            SetTag(argTagname, jid.ToString());
        }

        /// <summary>
        /// </summary>
        /// <param name="argTagname"> </param>
        /// <param name="argText"> </param>
        public void AddTag(string argTagname, string argText)
        {
            AddChild(new Element(argTagname, argText));
        }

        /// <summary>
        /// </summary>
        /// <param name="argTagname"> </param>
        public void AddTag(string argTagname)
        {
            AddChild(new Element(argTagname));
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="enumType"> </param>
        /// <returns> </returns>
        public object GetTagEnum(string name, Type enumType)
        {
            string tag = GetTag(name);
            if ((tag == null) || (tag.Length == 0))
            {
                return -1;
            }

            try
            {
#if CF
				return util.Enum.Parse(enumType, tag, true);
#else
                return System.Enum.Parse(enumType, tag, true);
#endif
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        ///   Return the Text of the first Tag with a specified Name in all childnodes as boolean
        /// </summary>
        /// <param name="TagName"> name of Tag to findas string </param>
        /// <returns> </returns>
        public bool GetTagBool(string TagName)
        {
            Element tag = _SelectElement(this, TagName);
            if (tag != null)
            {
                if (tag.Value.ToLower() == "false" || tag.Value.ToLower() == "0")
                {
                    return false;
                }
                else if (tag.Value.ToLower() == "true" || tag.Value.ToLower() == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="TagName"> </param>
        /// <returns> </returns>
        public int GetTagInt(string TagName)
        {
            Element tag = _SelectElement(this, TagName);
            if (tag != null)
            {
                return int.Parse(tag.Value);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="TagName"> </param>
        /// <returns> </returns>
        public Jid GetTagJid(string TagName)
        {
            string jid = GetTag(TagName);

            if (jid != null)
            {
                return new Jid(jid);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///   Get a Tag of type double (Decimal seperator = ".")
        /// </summary>
        /// <param name="argTagName"> </param>
        /// <returns> </returns>
        public double GetTagDouble(string argTagName)
        {
            // Parse the double always in english format ==> "." = Decimal seperator
            var nfi = new NumberFormatInfo();
            nfi.NumberGroupSeparator = ".";

            return GetTagDouble(argTagName, nfi);
        }

        /// <summary>
        ///   Get a Tag of type double with the given iFormatProvider
        /// </summary>
        /// <param name="argTagName"> </param>
        /// <param name="ifp"> </param>
        /// <returns> </returns>
        public double GetTagDouble(string argTagName, IFormatProvider ifp)
        {
            string val = GetTag(argTagName);
            if (val != null)
            {
                return Double.Parse(val, ifp);
            }
            else
            {
                return Double.NaN;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public bool HasTag(string name)
        {
            Element tag = _SelectElement(this, name);
            if (tag != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="traverseChildren"> </param>
        /// <returns> </returns>
        public bool HasTag(string name, bool traverseChildren)
        {
            Element tag = _SelectElement(this, name, traverseChildren);
            if (tag != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="type"> </param>
        /// <returns> </returns>
        public bool HasTag(Type type)
        {
            Element tag = _SelectElement(this, type);
            if (tag != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="traverseChildren"> </param>
        /// <returns> </returns>
        public bool HasTag(Type type, bool traverseChildren)
        {
            Element tag = _SelectElement(this, type, traverseChildren);
            if (tag != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="enumType"> </param>
        /// <returns> </returns>
        public object HasTagEnum(Type enumType)
        {
#if CF || CF_2            
			string[] members = util.Enum.GetNames(enumType);
#else
            string[] members = System.Enum.GetNames(enumType);
#endif
            foreach (string member in members)
            {
                if (HasTag(member))
                {
#if CF
					return util.Enum.Parse(enumType, member, false);
#else
                    return System.Enum.Parse(enumType, member, false);
                }

#endif
            }

            return -1;
        }

        /// <summary>
        ///   Remove a Tag when it exists
        /// </summary>
        /// <param name="TagName"> Tagname to remove </param>
        /// <returns> true when existing and removed, false when not existing </returns>
        public bool RemoveTag(string TagName)
        {
            Element tag = _SelectElement(this, TagName);
            if (tag != null)
            {
                tag.Remove();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///   Remove a Tag when it exists
        /// </summary>
        /// <param name="type"> Type of the tag that should be removed </param>
        /// <returns> true when existing and removed, false when not existing </returns>
        public bool RemoveTag(Type type)
        {
            Element tag = _SelectElement(this, type);
            if (tag != null)
            {
                tag.Remove();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///   Removes all Tags of the given type. Doesnt traverse the tree
        /// </summary>
        /// <param name="type"> Type of the tags that should be removed </param>
        /// <returns> true when tags were removed, false when no tags were found and removed </returns>
        public bool RemoveTags(Type type)
        {
            bool ret = false;

            ElementList list = SelectElements(type);

            if (list.Count > 0)
            {
                ret = true;
            }

            foreach (Element e in list)
            {
                e.Remove();
            }

            return ret;
        }

        /// <summary>
        ///   Same as AddChild, but Replaces the childelement when it exists
        /// </summary>
        /// <param name="e"> </param>
        public void ReplaceChild(Element e)
        {
            if (HasTag(e.TagName))
            {
                RemoveTag(e.TagName);
            }

            AddChild(e);
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public string Attribute(string name)
        {
            return (string) m_Attributes[name];
        }

        /// <summary>
        ///   Removes a Attribute
        /// </summary>
        /// <param name="name"> Attribute as string to remove </param>
        public void RemoveAttribute(string name)
        {
            if (HasAttribute(name))
            {
                Attributes.Remove(name);
                return;
            }
        }

        /// <summary>
        ///   Adds a new Attribue or changes a Attriv when already exists
        /// </summary>
        /// <param name="name"> name of Attribute to add/change </param>
        /// <param name="val"> </param>
        public void SetAttribute(string name, string val)
        {
            // When the attrib already exists then we overweite it
            // So we must remove it first and add it again then
            if (HasAttribute(name))
            {
                Attributes.Remove(name);
            }

            m_Attributes.Add(name, val);
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="value"> </param>
        public void SetAttribute(string name, int value)
        {
            SetAttribute(name, value.ToString());
        }

        /// <summary>
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="value"> </param>
        public void SetAttribute(string name, long value)
        {
            SetAttribute(name, value.ToString());
        }

        /// <summary>
        ///   Writes a boolean attribute, the value is either 'true' or 'false'
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="val"> </param>
        public void SetAttribute(string name, bool val)
        {
            // When the attrib already exists then we overweite it
            // So we must remove it first and add it again then
            if (HasAttribute(name))
            {
                Attributes.Remove(name);
            }

            m_Attributes.Add(name, val ? "true" : "false");
        }

        /// <summary>
        ///   Set a attribute of type Jid
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="value"> </param>
        public void SetAttribute(string name, Jid value)
        {
            if (value != null)
            {
                SetAttribute(name, value.ToString());
            }
            else
            {
                RemoveAttribute(name);
            }
        }

        /// <summary>
        ///   Set a attribute from a double in english number format
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="value"> </param>
        public void SetAttribute(string name, double value)
        {
            var nfi = new NumberFormatInfo();
            nfi.NumberGroupSeparator = ".";
            SetAttribute(name, value, nfi);
        }

        /// <summary>
        ///   Set a attribute from a double with the given Format provider
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="value"> </param>
        /// <param name="ifp"> </param>
        public void SetAttribute(string name, double value, IFormatProvider ifp)
        {
            SetAttribute(name, value.ToString(ifp));
        }

        /// <summary>
        /// </summary>
        /// <param name="value"> </param>
        public void SetNamespace(string value)
        {
            SetAttribute("xmlns", value);
        }

        /// <summary>
        /// </summary>
        public string InnerXml
        {
            get
            {
                if (ChildNodes.Count > 0)
                {
                    string xml = string.Empty;
                    try
                    {
                        for (int i = 0; i < ChildNodes.Count; i++)
                        {
                            if (ChildNodes.Item(i).NodeType == NodeType.Element)
                            {
                                xml += ChildNodes.Item(i).ToString();
                            }
                            else if (ChildNodes.Item(i).NodeType == NodeType.Text)
                            {
                                xml += ChildNodes.Item(i).Value;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    return xml;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                var doc = new Document();
                doc.LoadXml(value);
                Element root = doc.RootElement;
                if (root != null)
                {
                    ChildNodes.Clear();
                    AddChild(root);
                }
            }
        }

        /// <summary>
        ///   returns whether the current element has child elements or not. cares only about element, not text nodes etc...
        /// </summary>
        public bool HasChildElements
        {
            get
            {
                foreach (Node e in ChildNodes)
                {
                    if (e.NodeType == NodeType.Element)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        ///   returns the first child element (no textNodes)
        /// </summary>
        public Element FirstChild
        {
            get
            {
                if (ChildNodes.Count > 0)
                {
                    foreach (Node e in ChildNodes)
                    {
                        if (e.NodeType == NodeType.Element)
                        {
                            return e as Element;
                        }
                    }

                    return null;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///   Returns the first ChildNode, doesnt matter of which type it is
        /// </summary>
        public Node FirstNode
        {
            get
            {
                if (ChildNodes.Count > 0)
                {
                    return ChildNodes.Item(0);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///   Returns the last ChildNode, doesnt matter of which type it is
        /// </summary>
        public Node LastNode
        {
            get
            {
                if (ChildNodes.Count > 0)
                {
                    return ChildNodes.Item(ChildNodes.Count - 1);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        internal string StartTag()
        {
            var sw = new StringWriter();
            var tw = new XmlTextWriter(sw);
            tw.Formatting = Formatting.None;

            if (Prefix == null)
            {
                tw.WriteStartElement(TagName);
            }
            else
            {
                tw.WriteStartElement(Prefix + ":" + TagName);
            }

            // Write Namespace
            if (Namespace != null && Namespace.Length != 0)
            {
                if (Prefix == null)
                {
                    tw.WriteAttributeString("xmlns", Namespace);
                }
                else
                {
                    tw.WriteAttributeString("xmlns:" + Prefix, Namespace);
                }
            }

            foreach (string attName in Attributes.Keys)
            {
                tw.WriteAttributeString(attName, Attribute(attName));
            }

            tw.Flush();
            tw.Close();

            return sw.ToString().Replace("/>", ">");
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        internal string EndTag()
        {
            if (Prefix == null)
            {
                return "</" + TagName + ">";
            }
            else
            {
                return "</" + Prefix + ":" + TagName + ">";
            }
        }

        #region << Xml Select Functions >>

        /// <summary>
        ///   Find a Element by type
        /// </summary>
        /// <param name="type"> </param>
        /// <returns> </returns>
        public Element SelectSingleElement(Type type)
        {
            return _SelectElement(this, type);
        }

        /// <summary>
        ///   find a Element by type and loop thru all children
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="loopChildren"> </param>
        /// <returns> </returns>
        public Element SelectSingleElement(Type type, bool loopChildren)
        {
            return _SelectElement(this, type, true);
        }

        /// <summary>
        /// </summary>
        /// <param name="TagName"> </param>
        /// <returns> </returns>
        public Element SelectSingleElement(string TagName)
        {
            return _SelectElement(this, TagName);
        }

        /// <summary>
        /// </summary>
        /// <param name="TagName"> </param>
        /// <param name="traverseChildren"> </param>
        /// <returns> </returns>
        public Element SelectSingleElement(string TagName, bool traverseChildren)
        {
            return _SelectElement(this, TagName, true);
        }

        /// <summary>
        /// </summary>
        /// <param name="TagName"> </param>
        /// <param name="AttribName"> </param>
        /// <param name="AttribValue"> </param>
        /// <returns> </returns>
        public Element SelectSingleElement(string TagName, string AttribName, string AttribValue)
        {
            return _SelectElement(this, TagName, AttribName, AttribValue);
        }

        /// <summary>
        /// </summary>
        /// <param name="TagName"> </param>
        /// <param name="ns"> </param>
        /// <returns> </returns>
        public Element SelectSingleElement(string TagName, string ns)
        {
            // return this._SelectElement(this, TagName, "xmlns", ns);
            return _SelectElement(this, TagName, ns, true);
        }

        /// <summary>
        /// </summary>
        /// <param name="TagName"> </param>
        /// <param name="ns"> </param>
        /// <param name="traverseChildren"> </param>
        /// <returns> </returns>
        public Element SelectSingleElement(string TagName, string ns, bool traverseChildren)
        {
            return _SelectElement(this, TagName, ns, traverseChildren);
        }

#if NET_2

        /// <summary>
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <returns> </returns>
        public T SelectSingleElement<T>() where T : Element
        {
            return (T) _SelectElement(this, typeof (T));
        }

        /// <summary>
        /// </summary>
        /// <param name="traverseChildren"> </param>
        /// <typeparam name="T"> </typeparam>
        /// <returns> </returns>
        public T SelectSingleElement<T>(bool traverseChildren) where T : Element
        {
            return (T) _SelectElement(this, typeof (T), traverseChildren);
        }

#endif

        // public Element Element(string name)
        // {
        // return SelectSingleElement(name);
        // }

        // public T Element<T>() where T : Element
        // {
        // return (T)this._SelectElement(this, typeof(T));
        // }

        /// <summary>
        ///   Returns all childNodes with the given Tagname, this function doesn't traverse the whole tree!!!
        /// </summary>
        /// <param name="TagName"> </param>
        /// <returns> </returns>
        public ElementList SelectElements(string TagName)
        {
            var es = new ElementList();

            // return this._SelectElements(this, TagName, es);
            return _SelectElements(this, TagName, es, false);
        }

        /// <summary>
        /// </summary>
        /// <param name="TagName"> </param>
        /// <param name="traverseChildren"> </param>
        /// <returns> </returns>
        public ElementList SelectElements(string TagName, bool traverseChildren)
        {
            var es = new ElementList();

            // return this._SelectElements(this, TagName, es);
            return _SelectElements(this, TagName, es, traverseChildren);
        }

        /// <summary>
        /// </summary>
        /// <param name="type"> </param>
        /// <returns> </returns>
        public ElementList SelectElements(Type type)
        {
            var es = new ElementList();
            return _SelectElements(this, type, es);
        }

        /// <summary>
        ///   returns a nodelist of all found nodes of the given Type
        /// </summary>
        /// <param name="e"> </param>
        /// <param name="type"> </param>
        /// <param name="es"> </param>
        /// <returns> </returns>
        private ElementList _SelectElements(Element e, Type type, ElementList es)
        {
            return _SelectElements(e, type, es, false);
        }

        /// <summary>
        /// </summary>
        /// <param name="e"> </param>
        /// <param name="type"> </param>
        /// <param name="es"> </param>
        /// <param name="traverseChildren"> </param>
        /// <returns> </returns>
        private ElementList _SelectElements(Element e, Type type, ElementList es, bool traverseChildren)
        {
            if (e.ChildNodes.Count > 0)
            {
                foreach (Node n in e.ChildNodes)
                {
                    if (n.NodeType == NodeType.Element)
                    {
                        if (n.GetType() == type)
                        {
                            es.Add(n);
                        }

                        if (traverseChildren)
                        {
                            _SelectElements((Element) n, type, es, true);
                        }
                    }
                }
            }

            return es;
        }

        /// <summary>
        ///   Select a single Element. This function doesnt traverse the whole tree and checks only the underlying childnodes
        /// </summary>
        /// <param name="se"> </param>
        /// <param name="tagname"> </param>
        /// <returns> </returns>
        private Element _SelectElement(Node se, string tagname)
        {
            return _SelectElement(se, tagname, false);
        }

        /// <summary>
        ///   Select a single Elemnt
        /// </summary>
        /// <param name="se"> </param>
        /// <param name="tagname"> </param>
        /// <param name="traverseChildren"> when set to true then the function traverses the whole tree </param>
        /// <returns> </returns>
        private Element _SelectElement(Node se, string tagname, bool traverseChildren)
        {
            Element rElement = null;

            if (se.ChildNodes.Count > 0)
            {
                foreach (Node ch in se.ChildNodes)
                {
                    if (ch.NodeType == NodeType.Element)
                    {
                        if (((Element) ch).TagName == tagname)
                        {
                            rElement = (Element) ch;
                            return rElement;
                        }
                        else
                        {
                            if (traverseChildren)
                            {
                                rElement = _SelectElement(ch, tagname, true);
                                if (rElement != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return rElement;
        }

        /// <summary>
        /// </summary>
        /// <param name="se"> </param>
        /// <param name="type"> </param>
        /// <returns> </returns>
        private Element _SelectElement(Node se, Type type)
        {
            return _SelectElement(se, type, false);
        }

        /// <summary>
        /// </summary>
        /// <param name="se"> </param>
        /// <param name="type"> </param>
        /// <param name="traverseChildren"> </param>
        /// <returns> </returns>
        private Element _SelectElement(Node se, Type type, bool traverseChildren)
        {
            Element rElement = null;

            if (se.ChildNodes.Count > 0)
            {
                foreach (Node ch in se.ChildNodes)
                {
                    if (ch.NodeType == NodeType.Element)
                    {
                        if (ch.GetType() == type)
                        {
                            rElement = (Element) ch;
                            return rElement;
                        }
                        else
                        {
                            if (traverseChildren)
                            {
                                rElement = _SelectElement(ch, type, true);
                                if (rElement != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return rElement;
        }

        /// <summary>
        /// </summary>
        /// <param name="se"> </param>
        /// <param name="tagname"> </param>
        /// <param name="AttribName"> </param>
        /// <param name="AttribValue"> </param>
        /// <returns> </returns>
        private Element _SelectElement(Node se, string tagname, string AttribName, string AttribValue)
        {
            Element rElement = null;

            if (se.NodeType == NodeType.Element)
            {
                var e = se as Element;
                if (e.m_TagName == tagname)
                {
                    if (e.HasAttribute(AttribName))
                    {
                        if (e.GetAttribute(AttribName) == AttribValue)
                        {
                            rElement = e;
                            return rElement;
                        }
                    }
                }
            }

            if (se.ChildNodes.Count > 0)
            {
                foreach (Node ch in se.ChildNodes)
                {
                    rElement = _SelectElement(ch, tagname, AttribName, AttribValue);
                    if (rElement != null)
                    {
                        break;
                    }
                }
            }

            return rElement;
        }

        /// <summary>
        ///   Find Element by Namespace
        /// </summary>
        /// <param name="se"> </param>
        /// <param name="tagname"> </param>
        /// <param name="nameSpace"> </param>
        /// <param name="traverseChildren"> </param>
        /// <returns> </returns>
        private Element _SelectElement(Node se, string tagname, string nameSpace, bool traverseChildren)
        {
            Element rElement = null;

            if (se.ChildNodes.Count > 0)
            {
                foreach (Node ch in se.ChildNodes)
                {
                    if (ch.NodeType == NodeType.Element)
                    {
                        var e = ch as Element;
                        if (e.TagName == tagname && e.Namespace == nameSpace)
                        {
                            rElement = (Element) ch;
                            return rElement;
                        }
                        else
                        {
                            if (traverseChildren)
                            {
                                rElement = _SelectElement(ch, tagname, nameSpace, traverseChildren);
                                if (rElement != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return rElement;
        }

        /// <summary>
        /// </summary>
        /// <param name="e"> </param>
        /// <param name="tagname"> </param>
        /// <param name="es"> </param>
        /// <param name="traverseChildren"> </param>
        /// <returns> </returns>
        private ElementList _SelectElements(Element e, string tagname, ElementList es, bool traverseChildren)
        {
            if (e.ChildNodes.Count > 0)
            {
                foreach (Node n in e.ChildNodes)
                {
                    if (n.NodeType == NodeType.Element)
                    {
                        if (((Element) n).m_TagName == tagname)
                        {
                            es.Add(n);
                        }

                        if (traverseChildren)
                        {
                            _SelectElements((Element) n, tagname, es, true);
                        }
                    }
                }
            }

            return es;
        }

#if NET_2

        /// <summary>
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <returns> </returns>
        public List<T> SelectElements<T>() where T : Element
        {
            return SelectElements<T>(false);
        }

        /// <summary>
        /// </summary>
        /// <param name="traverseChildren"> </param>
        /// <typeparam name="T"> </typeparam>
        /// <returns> </returns>
        public List<T> SelectElements<T>(bool traverseChildren) where T : Element
        {
            var list = new List<T>();
            return _SelectElements(this, list, traverseChildren);
        }

        /// <summary>
        /// </summary>
        /// <param name="e"> </param>
        /// <param name="list"> </param>
        /// <param name="traverseChildren"> </param>
        /// <typeparam name="T"> </typeparam>
        /// <returns> </returns>
        private List<T> _SelectElements<T>(Element e, List<T> list, bool traverseChildren) where T : Element
        {
            if (e.ChildNodes.Count > 0)
            {
                foreach (Node n in e.ChildNodes)
                {
                    if (n.NodeType == NodeType.Element)
                    {
                        if (n.GetType() == typeof (T))
                        {
                            list.Add(n as T);
                        }

                        if (traverseChildren)
                        {
                            _SelectElements((Element) n, list, true);
                        }
                    }
                }
            }

            return list;
        }

#endif

        #endregion

        public override object Clone()
        {
            var clone = (Element) base.Clone();
            clone.m_Attributes = new ListDictionary();
            foreach (DictionaryEntry a in m_Attributes) clone.m_Attributes.Add(a.Key, a.Value);
            return clone;
        }
    }
}