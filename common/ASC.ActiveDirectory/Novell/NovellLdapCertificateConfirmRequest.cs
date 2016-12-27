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
using System.Security.Cryptography.X509Certificates;

namespace ASC.ActiveDirectory.Novell
{
    [Serializable]
    [DataContract]
    public class NovellLdapCertificateConfirmRequest
    {
        private volatile bool approved;
        private volatile bool requested;
        private volatile string serialNumber;
        private volatile string issuerName;
        private volatile string  subjectName;
        private volatile string hash;
        private volatile int[] certificateErrors;

        [DataMember]
        public bool Approved { get { return approved; } set { approved = value; } }

        [DataMember]
        public bool Requested { get { return requested; } set { requested = value; } }

        [DataMember]
        public string SerialNumber { get { return serialNumber; } set { serialNumber = value; } }

        [DataMember]
        public string IssuerName { get { return issuerName; } set { issuerName = value; } }

        [DataMember]
        public string SubjectName { get { return subjectName; } set { subjectName = value; } }

        [DataMember]
        public DateTime ValidFrom { get; set; }

        [DataMember]
        public DateTime ValidUntil { get; set; }

        [DataMember]
        public string Hash { get { return hash; } set { hash = value; } }

        [DataMember]
        public int[] CertificateErrors { get { return certificateErrors; } set { certificateErrors = value; } }
    }
}
