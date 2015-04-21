/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Dal;

namespace ASC.Mail.Aggregator
{
    [Serializable]
    [DataContract(Namespace = "", Name="Account")]
    public class MailAccount
    {
        [DataMember(IsRequired = true)]
        public string Address
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        public bool Enabled
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        public bool QuotaError { get; set; }

        [DataMember(IsRequired = true)]
        public bool AuthError { get; set; }

        [DataMember(IsRequired = true)]
        public string Name
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        public int Id
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        public bool OAuthConnection { get; set; }

        [DataMember(IsRequired = true)]
        public SignatureDto Signature { get; set; }

        [DataMember(IsRequired = true)]
        public string EMailInFolder { get; set; }

        public override string ToString()
        {
            return Name + " <"+Address+">";
        }

        public MailAccount(string address, string name, bool enabled, 
            bool quota_error, MailBox.AuthProblemType auth_error, int id,
            bool oauth_connection, SignatureDto signature, string email_in_folder)
        {
            Address = address;
            Name = name;
            Enabled = enabled;
            QuotaError = quota_error;
            AuthError = auth_error == MailBox.AuthProblemType.TooManyErrors;
            Id = id;
            OAuthConnection = oauth_connection;
            Signature = signature;
            EMailInFolder = email_in_folder;
        }
    }
}
