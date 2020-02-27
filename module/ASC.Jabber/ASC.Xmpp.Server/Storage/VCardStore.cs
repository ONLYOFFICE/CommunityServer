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
