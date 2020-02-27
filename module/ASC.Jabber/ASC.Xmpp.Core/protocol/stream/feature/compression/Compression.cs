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


using ASC.Xmpp.Core.protocol.extensions.compression;
using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.stream.feature.compression
{
    public class Compression : Element
    {
        /*
         *  <compression xmlns='http://jabber.org/features/compress'>
         *      <method>zlib</method>
         *  </compression>
         * 
         * <stream:features>
         *      <starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>
         *      <compression xmlns='http://jabber.org/features/compress'>
         *          <method>zlib</method>
         *          <method>lzw</method>
         *      </compression>
         * </stream:features>
         */

        public Compression()
        {
            TagName = "compression";
            Namespace = Uri.FEATURE_COMPRESS;
        }

        /// <summary>
        ///   method/algorithm used to compressing the stream
        /// </summary>
        public CompressionMethod Method
        {
            set
            {
                if (value != CompressionMethod.Unknown)
                    SetTag("method", value.ToString());
            }
            get { return (CompressionMethod) GetTagEnum("method", typeof (CompressionMethod)); }
        }

        /// <summary>
        ///   Add a compression method/algorithm
        /// </summary>
        /// <param name="method"> </param>
        public void AddMethod(CompressionMethod method)
        {
            if (!SupportsMethod(method))
                AddChild(new Method(method));
        }

        /// <summary>
        ///   Is the given compression method/algrithm supported?
        /// </summary>
        /// <param name="method"> </param>
        /// <returns> </returns>
        public bool SupportsMethod(CompressionMethod method)
        {
            ElementList nList = SelectElements(typeof (Method));
            foreach (Method m in nList)
            {
                if (m.CompressionMethod == method)
                    return true;
            }
            return false;
        }

        public Method[] GetMethods()
        {
            ElementList methods = SelectElements(typeof (Method));

            var items = new Method[methods.Count];
            int i = 0;
            foreach (Method m in methods)
            {
                items[i] = m;
                i++;
            }
            return items;
        }
    }
}