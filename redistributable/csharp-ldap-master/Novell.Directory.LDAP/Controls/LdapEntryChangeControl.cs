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
// Novell.Directory.Ldap.Controls.LdapEntryChangeControl.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Controls
{
    /// <summary>
    ///     LdapEntryChangeControl is a Server Control returned at the request
    ///     of the client in response to a persistent search request. It
    ///     contains additional information about a change such as what type of
    ///     change occurred.
    /// </summary>
    public class LdapEntryChangeControl : LdapControl
    {
        /// <summary>
        ///     returns the record number of the change in the servers change log.
        /// </summary>
        /// <returns>
        ///     the record number of the change in the server's change log.
        ///     The server may not return a change number. In this case the return
        ///     value is -1
        /// </returns>
        public virtual bool HasChangeNumber
        {
            get { return m_hasChangeNumber; }
        }

        /// <summary>
        ///     returns the record number of the change in the servers change log.
        /// </summary>
        /// <returns>
        ///     the record number of the change in the server's change log.
        ///     The server may not return a change number. In this case the return
        ///     value is -1
        /// </returns>
        public virtual int ChangeNumber
        {
            get { return m_changeNumber; }
        }

        /// <summary>
        ///     Returns the type of change that occured
        /// </summary>
        /// <returns>
        ///     returns one of the following values indicating the type of
        ///     change that occurred:
        ///     LdapPersistSearchControl.ADD
        ///     LdapPersistSearchControl.DELETE
        ///     LdapPersistSearchControl.MODIFY
        ///     LdapPersistSearchControl.MODDN.
        /// </returns>
        public virtual int ChangeType
        {
            get { return m_changeType; }
        }

        /// <summary>
        ///     Returns the previous DN of the entry, if it was renamed.
        /// </summary>
        /// <returns>
        ///     the previous DN of the entry if the entry was renamed (ie. if the
        ///     change type is LdapersistSearchControl.MODDN.
        /// </returns>
        public virtual string PreviousDN
        {
            get { return m_previousDN; }
        }

        private readonly int m_changeType;
        private readonly string m_previousDN;
        private readonly bool m_hasChangeNumber;
        private readonly int m_changeNumber;

        /// <summary>
        ///     This constructor is called by the SDK to create an
        ///     LdapEntryChangeControl. This constructor should NOT be called by
        ///     application developers. It must be public since it resides in a
        ///     different package than the classes that call it.
        ///     The Entry Change Control is defined as follows:
        ///     EntryChangeNotification ::= SEQUENCE {
        ///     changeType ENUMERATED {
        ///     add             (1),
        ///     delete          (2),
        ///     modify          (4),
        ///     modDN           (8)
        ///     },
        ///     previousDN   LdapDN OPTIONAL,     -- modifyDN ops. only
        ///     changeNumber INTEGER OPTIONAL     -- if supported
        ///     }
        /// </summary>
        /// <param name="oid">
        ///     The OID of the control, as a dotted string.
        /// </param>
        /// <param name="critical">
        ///     True if the Ldap operation should be discarded if
        ///     the control is not supported. False if
        ///     the operation can be processed without the control.
        /// </param>
        /// <param name="value">
        ///     The control-specific data.
        /// </param>
        [CLSCompliant(false)]
        public LdapEntryChangeControl(string oid, bool critical, sbyte[] value_Renamed)
            : base(oid, critical, value_Renamed)
        {
            // Create a decoder objet
            var decoder = new LBERDecoder();
            if (decoder == null)
                throw new IOException("Decoding error.");

            // We should get a sequence initially
            var asnObj = decoder.decode(value_Renamed);

            if (asnObj == null || !(asnObj is Asn1Sequence))
                throw new IOException("Decoding error.");

            var sequence = (Asn1Sequence) asnObj;


            // The first element in the sequence should be an enumerated type
            var asn1Obj = sequence.get_Renamed(0);
            if (asn1Obj == null || !(asn1Obj is Asn1Enumerated))
                throw new IOException("Decoding error.");

            m_changeType = ((Asn1Enumerated) asn1Obj).intValue();

            //check for optional elements
            if (sequence.size() > 1 && m_changeType == 8)
                //8 means modifyDN
            {
                // get the previous DN - it is encoded as an octet string
                asn1Obj = sequence.get_Renamed(1);
                if (asn1Obj == null || !(asn1Obj is Asn1OctetString))
                    throw new IOException("Decoding error get previous DN");

                m_previousDN = ((Asn1OctetString) asn1Obj).stringValue();
            }
            else
            {
                m_previousDN = "";
            }

            //check for change number
            if (sequence.size() == 3)
            {
                asn1Obj = sequence.get_Renamed(2);
                if (asn1Obj == null || !(asn1Obj is Asn1Integer))
                    throw new IOException("Decoding error getting change number");

                m_changeNumber = ((Asn1Integer) asn1Obj).intValue();
                m_hasChangeNumber = true;
            }
            else
                m_hasChangeNumber = false;
        }

        /// <summary>  Returns a string representation of the control for debugging.</summary>
        public override string ToString()
        {
            return base.ToString();
        }
    } //end class LdapEntryChangeControl
}