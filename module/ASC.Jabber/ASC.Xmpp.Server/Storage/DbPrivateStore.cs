/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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