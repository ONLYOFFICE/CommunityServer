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


using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.filetransfer
{
    /// <summary>
    ///   When range is sent in the offer, it should have no attributes. This signifies that the sender can do ranged transfers. When no range element is sent in the Stream Initiation result, the Sender MUST send the complete file starting at offset 0. More generally, data is sent over the stream byte for byte starting at the offset position for the length specified.
    /// </summary>
    public class Range : Element
    {
        /*
		<range offset='252' length='179'/>		    	
		*/

        public Range()
        {
            TagName = "range";
            Namespace = Uri.SI_FILE_TRANSFER;
        }

        public Range(long offset, long length) : this()
        {
            Offset = offset;
            Length = length;
        }

        /// <summary>
        ///   Specifies the position, in bytes, to start transferring the file data from. This defaults to zero (0) if not specified.
        /// </summary>
        public long Offset
        {
            get { return GetAttributeLong("offset"); }
            set { SetAttribute("offset", value.ToString()); }
        }

        /// <summary>
        ///   Specifies the number of bytes to retrieve starting at offset. This defaults to the length of the file from offset to the end.
        /// </summary>
        public long Length
        {
            get { return GetAttributeLong("length"); }
            set { SetAttribute("length", value.ToString()); }
        }
    }
}