/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using ASC.Core;
using ASC.Xmpp.protocol.iq.vcard;
using ASC.Xmpp.Server.Exceptions;
using ASC.Xmpp.Server.storage.Interface;

namespace ASC.Xmpp.Server.storage
{
	class ASCVCardStore : DbStoreBase, IVCardStore
	{
		private static readonly int imageSize = 64;

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

		public ASCVCardStore(ConnectionStringSettings connectionSettings)
			: base(connectionSettings)
		{
			InitializeDbSchema(false);
		}

		public ASCVCardStore(string provider, string connectionString)
			: base(provider, connectionString)
		{
			InitializeDbSchema(false);
		}

		#region IVCardStore Members

		public void SetVCard(string user, Vcard vcard)
		{
			if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("user");
			if (vcard == null) throw new ArgumentNullException("vcard");
			if (CoreContext.UserManager.IsUserNameExists(user)) throw new JabberForbiddenException();

			try
			{
				lock (vcardsCache)
				{
					ExecuteNonQuery("insert or replace into VCard(UserName, VCard) values (@userName, @vCard)",
						new[] { "userName", "vCard" }, new object[] { user, ElementSerializer.SerializeElement(vcard) }
					);
					vcardsCache[user] = vcard;
				}
			}
			catch (Exception e)
			{
				throw new JabberServiceUnavailableException("Could not set vcard", e);
			}
		}

		public Vcard GetVCard(string user)
		{
			try
			{
				if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("user");
				if (CoreContext.UserManager.IsUserNameExists(user))
				{
					var ui = CoreContext.UserManager.GetUserByUserName(user);
					var vcard = new Vcard();
					
					vcard.Fullname = vcard.Nickname = string.Format("{0} {1}", ui.LastName, ui.FirstName);
					if (ui.BirthDate != null) vcard.Birthday = ui.BirthDate.Value;
					vcard.JabberId = new Jid(ui.UserName);
					vcard.AddTelephoneNumber(new Telephone(TelephoneLocation.WORK, TelephoneType.NUMBER, ui.PhoneOffice));
					vcard.AddTelephoneNumber(new Telephone(TelephoneLocation.WORK, TelephoneType.FAX, ui.Fax));
					vcard.AddTelephoneNumber(new Telephone(TelephoneLocation.HOME, TelephoneType.NUMBER, ui.PhoneHome));
					vcard.AddTelephoneNumber(new Telephone(TelephoneLocation.HOME, TelephoneType.CELL, ui.PhoneMobile));
					vcard.AddEmailAddress(new Email(EmailType.INTERNET, ui.Email, true));
					
					vcard.Organization = new Organization(CoreContext.UserManager.GetCompanyName(), ui.Department);
					vcard.Title = ui.Title;
					
					vcard.AddAddress(new Address(AddressLocation.HOME, null, ui.PrimaryAddress, ui.City, ui.State, ui.PostalCode, ui.Country, true));
					
					vcard.Description = ui.Notes;
					
					var image = PreparePhoto(CoreContext.UserManager.GetUserPhoto(ui.ID, Guid.Empty));
					if (image != null)
					{
						vcard.Photo = new Photo(image, image.RawFormat);
						//image.Dispose();
					}

					return vcard;
				}
				else
				{
					lock (vcardsCache)
					{
						if (!vcardsCache.ContainsKey(user))
						{
							var vcardStr = ExecuteScalar("select VCard from VCard where UserName = @userName", "userName", user) as string;
							vcardsCache[user] = !string.IsNullOrEmpty(vcardStr) ? ElementSerializer.DeSerializeElement<Vcard>(vcardStr) : null;
						}
						return vcardsCache[user];
					}
				}
			}
			catch (Exception e)
			{
				throw new JabberServiceUnavailableException("Could not get vcard", e);
			}
		}

		#endregion

		private Image PreparePhoto(byte[] photo)
		{
			if (photo == null || photo.Length == 0) return null;

			using (var stream = new MemoryStream(photo))
			using (var image = Image.FromStream(stream))
			{
				var imageMinSize = Math.Min(image.Width, image.Height);
				var size = imageSize;
				if (imageMinSize < 96) size = 64;
				if (imageMinSize < 64) size = 32;

				using (var bitmap = new Bitmap(size, size))
				using (var g = Graphics.FromImage(bitmap))
				{
					var delta = (image.Width - image.Height) / 2;
					var srcRect = new RectangleF(0f, 0f, imageMinSize, imageMinSize);
					if (image.Width < image.Height) srcRect.Y = -delta;
					else srcRect.X = delta;

					g.SmoothingMode = SmoothingMode.HighQuality;
					g.PixelOffsetMode = PixelOffsetMode.HighQuality;
					g.CompositingQuality = CompositingQuality.HighQuality;

					var gu = GraphicsUnit.Pixel;
					var destRect = bitmap.GetBounds(ref gu);
					g.DrawImage(image, destRect, srcRect, gu);

					var saveStream = new MemoryStream();
					bitmap.Save(saveStream, ImageFormat.Png);
					return Image.FromStream(saveStream);
				}
			}
		}
	}
}