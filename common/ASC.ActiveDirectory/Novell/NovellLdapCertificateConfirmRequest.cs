/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System;
using System.Runtime.Serialization;

namespace ASC.ActiveDirectory.Novell
{
    [Serializable]
    [DataContract]
    public class NovellLdapCertificateConfirmRequest
    {
        private volatile bool _approved;
        private volatile bool _requested;
        private volatile string _serialNumber;
        private volatile string _issuerName;
        private volatile string  _subjectName;
        private volatile string _hash;
        private volatile int[] _certificateErrors;

        [DataMember]
        public bool Approved { get { return _approved; } set { _approved = value; } }

        [DataMember]
        public bool Requested { get { return _requested; } set { _requested = value; } }

        [DataMember]
        public string SerialNumber { get { return _serialNumber; } set { _serialNumber = value; } }

        [DataMember]
        public string IssuerName { get { return _issuerName; } set { _issuerName = value; } }

        [DataMember]
        public string SubjectName { get { return _subjectName; } set { _subjectName = value; } }

        [DataMember]
        public DateTime ValidFrom { get; set; }

        [DataMember]
        public DateTime ValidUntil { get; set; }

        [DataMember]
        public string Hash { get { return _hash; } set { _hash = value; } }

        [DataMember]
        public int[] CertificateErrors { get { return _certificateErrors; } set { _certificateErrors = value; } }
    }
}
