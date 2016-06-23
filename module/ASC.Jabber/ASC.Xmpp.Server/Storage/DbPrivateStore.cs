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


using ASC.Common.Data.Sql;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Storage.Interface;
using System;
using System.Data;

namespace ASC.Xmpp.Server.Storage
{
    public class DbPrivateStore : DbStoreBase, IPrivateStore
    {
        protected override SqlCreate[] GetCreateSchemaScript()
        {
            var t1 = new SqlCreate.Table("jabber_private", true)
                .AddColumn("jid", DbType.String, 255, true)
                .AddColumn("tag", DbType.String, 255, true)
                .AddColumn("namespace", DbType.String, 255, true)
                .AddColumn("element", DbType.String, UInt16.MaxValue)
                .PrimaryKey("jid", "tag", "namespace");
            return new[] { t1 };
        }

        
        #region IPrivateStore Members

        public Element GetPrivate(Jid jid, Element element)
        {
            CheckArgs(jid, element);

            var elementStr = ExecuteScalar<string>(new SqlQuery("jabber_private").Select("element").Where("jid", jid.Bare).Where("tag", element.TagName).Where("namespace", element.Namespace));
            return !string.IsNullOrEmpty(elementStr) ? ElementSerializer.DeSerializeElement<Element>(elementStr) : null;
        }

        public void SetPrivate(Jid jid, Element element)
        {
            CheckArgs(jid, element);

            ExecuteNonQuery(
                new SqlInsert("jabber_private", true)
                .InColumnValue("jid", jid.Bare)
                .InColumnValue("tag", element.TagName)
                .InColumnValue("namespace", element.Namespace)
                .InColumnValue("element", ElementSerializer.SerializeElement(element)));
        }

        #endregion

        private void CheckArgs(Jid jid, Element element)
        {
            if (jid == null) throw new ArgumentNullException("jid");
            if (element == null) throw new ArgumentNullException("element");
            if (string.IsNullOrEmpty(element.TagName)) throw new ArgumentNullException("element.TagName");
            if (string.IsNullOrEmpty(element.Namespace)) throw new ArgumentNullException("element.Namespace");
        }
    }
}