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

using System;
using System.Collections.Generic;
using System.Configuration;
using ASC.Xmpp.protocol.iq.vcard;
using ASC.Xmpp.Server.storage.Interface;

namespace ASC.Xmpp.Server.storage
{
	class VCardStore : DbStoreBase, IVCardStore
	{
		private IDictionary<string, Vcard> vcardsCache = new Dictionary<string, Vcard>();

		protected override string[] CreateSchemaScript
		{
			get
			{
				return new[]{
					"create table VCard(UserName TEXT NOT NULL primary key, VCard TEXT)"
				};
			}
		}

		protected override string[] DropSchemaScript
		{
			get
			{
				return new[]{
					"drop table VCard"
				};
			}
		}

		public VCardStore(ConnectionStringSettings connectionSettings)
			: base(connectionSettings)
		{
			InitializeDbSchema(false);
		}

		public VCardStore(string provider, string connectionString)
			: base(provider, connectionString)
		{
			InitializeDbSchema(false);
		}

		#region IVCardStore Members

		public void SetVCard(string user, Vcard vcard)
		{
			if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("user");
			if (vcard == null) throw new ArgumentNullException("vcard");

			lock (vcardsCache)
			{
				using (var connect = GetDbConnection())
				using (var command = connect.CreateCommand())
				{
					command.CommandText = "insert or replace into VCard(UserName, VCard) values (@userName, @vCard)";
					AddCommandParameter(command, "userName", user);
					AddCommandParameter(command, "vCard", ElementSerializer.SerializeElement(vcard));
					command.ExecuteNonQuery();
				}
				vcardsCache[user] = vcard;
			}
		}

		public Vcard GetVCard(string user)
		{
			if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("user");

			lock (vcardsCache)
			{
				if (!vcardsCache.ContainsKey(user))
				{
					using (var connect = GetDbConnection())
					using (var command = connect.CreateCommand())
					{
						command.CommandText = "select VCard from VCard where UserName = @userName";
						AddCommandParameter(command, "userName", user);
						var vcardStr = command.ExecuteScalar() as string;
						vcardsCache[user] = !string.IsNullOrEmpty(vcardStr) ? ElementSerializer.DeSerializeElement<Vcard>(vcardStr) : null;
					}
				}
				return vcardsCache[user];
			}
		}

		#endregion
	}
}
