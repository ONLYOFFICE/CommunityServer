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


using System;
using System.IO;
using System.Xml.Linq;
using ASC.Core;
using ASC.ElasticSearch;
using ASC.ElasticSearch.Core;
using ASC.Web.Talk.Resources;

namespace ASC.Web.Talk
{
    public class JabberWrapper : WrapperWithDoc
    {
        protected override string Table
        {
            get { return "jabber_archive"; }
        }

        [ColumnLastModified("stamp")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnMeta("jid", 1)]
        public string Jid { get; set; }

        [ColumnTenantId("")]
        public override int TenantId
        {
            get
            {
                try
                {
                    var alias = Jid.Split('|')[0].Split('@')[1];
                    var index = alias.IndexOf(CoreContext.Configuration.BaseDomain, StringComparison.Ordinal);
                    if (CoreContext.Configuration.BaseDomain != alias && index > 0)
                    {
                        alias = alias.Substring(0, index);
                    }

                    var tenant = CoreContext.TenantManager.GetTenant(alias);
                    return tenant.TenantId;
                }
                catch (Exception)
                {
                    return -1;
                }
            }
        }

        private string message;

        [Column("message", 1)]
        public string Message { get { return ""; } set { message = value; } }

        protected override Stream GetDocumentStream()
        {
            if (!string.IsNullOrEmpty(message))
            {
                var doc = XDocument.Parse(message);
                if (doc.Declaration == null)
                {
                    doc.Declaration = new XDeclaration("1.0", "utf-8", "yes");
                }

                var result = new MemoryStream();
                doc.Save(result);
                result.Position = 0;
                return result;
            }

            return new MemoryStream(new byte[0]);
        }

        public override string SettingsTitle
        {
            get { return TalkResource.IndexTitle; }
        }
    }
}
