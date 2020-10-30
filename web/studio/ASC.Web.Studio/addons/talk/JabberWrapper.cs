/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
