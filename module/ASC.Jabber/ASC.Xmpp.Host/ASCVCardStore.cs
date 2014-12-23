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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Caching;
using ASC.Core.Users;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.vcard;
using ASC.Xmpp.Server;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Storage.Interface;

namespace ASC.Xmpp.Host
{
    class ASCVCardStore : DbVCardStore, IVCardStore
    {
        private static readonly int IMAGE_SIZE = 64;

        private TimeSpan CACHE_TIMEOUT = TimeSpan.FromHours(1);

        private readonly ICache cache = AscCache.Default;


        public override void Configure(IDictionary<string, string> properties)
        {
            base.Configure(properties);
            if (properties.ContainsKey("cacheTimeout"))
            {
                var timeout = int.Parse(properties["cacheTimeout"]);
                CACHE_TIMEOUT = 0 < timeout ? TimeSpan.FromMinutes(timeout) : TimeSpan.MaxValue;
            }
        }

        public override void SetVCard(Jid jid, Vcard vcard)
        {
            ASCContext.SetCurrentTenant(jid.Server);
            if (ASCContext.UserManager.IsUserNameExists(jid.User)) throw new JabberException(ErrorCode.Forbidden);
            base.SetVCard(jid, vcard);
        }

        public override Vcard GetVCard(Jid jid, string id = "")
        {
            ASCContext.SetCurrentTenant(jid.Server);

            jid = new Jid(jid.Bare.ToLowerInvariant());
            var ui = ASCContext.UserManager.GetUserByUserName(jid.User);

            if (ui != null)
            {

                var vcard = (Vcard)cache.Get(jid.ToString());
                if (vcard != null)
                {
                    return vcard;
                }

                vcard = new Vcard();
                vcard.Name = new Name(ui.LastName, ui.FirstName, null);
                vcard.Fullname = UserFormatter.GetUserName(ui);
                vcard.Nickname = ui.UserName;
                vcard.Description = ui.Notes;
                if (ui.BirthDate != null) vcard.Birthday = ui.BirthDate.Value;
                vcard.JabberId = jid;
                if (ui.Sex.HasValue)
                {
                    vcard.Gender = ui.Sex.Value ? Gender.MALE : Gender.FEMALE;
                }

                var index = ui.Contacts.FindIndex(c => string.Compare(c, "phone", true) == 0) + 1;
                if (0 < index && index < ui.Contacts.Count)
                {
                    vcard.AddTelephoneNumber(new Telephone(TelephoneLocation.WORK, TelephoneType.NUMBER, ui.Contacts[index]));
                }
                vcard.AddEmailAddress(new Email(EmailType.INTERNET, ui.Email, true));

                var tenant = ASCContext.GetCurrentTenant();
                var departments = string.Join(", ", CoreContext.UserManager.GetUserGroups(ui.ID).Select(d => HttpUtility.HtmlEncode(d.Name)).ToArray());
                if (tenant != null) vcard.Organization = new Organization(tenant.Name, departments);
                vcard.Title = ui.Title;
                if (id == null || id.IndexOf("tmtalk", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    var image = PreparePhoto(ASCContext.UserManager.GetUserPhoto(ui.ID, Guid.Empty));
                    if (image != null)
                    {
                        vcard.Photo = new Photo(image, ImageFormat.Png);
                        image.Dispose();
                    }
                }
                cache.Insert(jid.ToString(), vcard, CACHE_TIMEOUT);
                return vcard;
            }
            else
            {
                return base.GetVCard(jid);
            }
        }

        public override ICollection<Vcard> Search(Vcard pattern)
        {
            throw new NotImplementedException();
        }


        private Image PreparePhoto(byte[] photo)
        {
            if (photo == null || photo.Length == 0) return null;

            using (var stream = new MemoryStream(photo))
            using (var image = Image.FromStream(stream))
            {
                var imageMinSize = Math.Min(image.Width, image.Height);
                var size = IMAGE_SIZE;
                if (imageMinSize < 96) size = 64;
                if (imageMinSize < 64) size = 32;

                using (var bitmap = new Bitmap(size, size))
                using (var g = Graphics.FromImage(bitmap))
                {
                    var delta = (image.Width - image.Height)/2;
                    var srcRect = new RectangleF(0f, 0f, imageMinSize, imageMinSize);
                    if (image.Width < image.Height) srcRect.Y = -delta;
                    else srcRect.X = delta;

                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;

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