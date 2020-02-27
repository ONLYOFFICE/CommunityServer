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


namespace ASC.Xmpp.Core.protocol.extensions.ibb
{
    /*
         <data xmlns='http://jabber.org/protocol/ibb' sid='mySID' seq='0'>
            qANQR1DBwU4DX7jmYZnncmUQB/9KuKBddzQH+tZ1ZywKK0yHKnq57kWq+RFtQdCJ
            WpdWpR0uQsuJe7+vh3NWn59/gTc5MDlX8dS9p0ovStmNcyLhxVgmqS8ZKhsblVeu
            IpQ0JgavABqibJolc3BKrVtVV1igKiX/N7Pi8RtY1K18toaMDhdEfhBRzO/XB0+P
            AQhYlRjNacGcslkhXqNjK5Va4tuOAPy2n1Q8UUrHbUd0g+xJ9Bm0G0LZXyvCWyKH
            kuNEHFQiLuCY6Iv0myq6iX6tjuHehZlFSh80b5BVV9tNLwNR5Eqz1klxMhoghJOA
         </data>
      
         <xs:element name='data'>
             <xs:complexType>
              <xs:simpleContent>
                <xs:extension base='xs:string'>
                  <xs:attribute name='sid' type='xs:string' use='required'/>
                  <xs:attribute name='seq' type='xs:string' use='required'/>
                </xs:extension>
              </xs:simpleContent>
             </xs:complexType>
           </xs:element>
    */

    /// <summary>
    /// </summary>
    public class Data : Base
    {
        /// <summary>
        /// </summary>
        public Data()
        {
            TagName = "data";
        }

        /// <summary>
        /// </summary>
        /// <param name="sid"> </param>
        /// <param name="seq"> </param>
        public Data(string sid, int seq) : this()
        {
            Sid = sid;
            Sequence = seq;
        }

        /// <summary>
        ///   the sequence
        /// </summary>
        public int Sequence
        {
            get { return GetAttributeInt("seq"); }
            set { SetAttribute("seq", value); }
        }
    }
}