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

using System.Collections.Generic;
using ASC.Common.Data.Sql;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region db defines

        // ReSharper disable InconsistentNaming
        public const string MAIL_DISPLAY_IMAGES = "mail_display_images";
        // ReSharper restore InconsistentNaming

        public struct DisplayImagesFields
        {
            public static string id_user = "id_user";
            public static string id_tenant = "tenant";
            public static string address = "address";
        };

        #endregion

        #region public methods

        public void AddDisplayImagesAddress(int tenant, string user, string address)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                    new SqlInsert(MAIL_DISPLAY_IMAGES)
                        .InColumnValue(DisplayImagesFields.id_tenant, tenant)
                        .InColumnValue(DisplayImagesFields.id_user, user)
                        .InColumnValue(DisplayImagesFields.address, address));
            }
        }

        public void RemovevDisplayImagesAddress(int tenant, string user, string address)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                    new SqlDelete(MAIL_DISPLAY_IMAGES)
                        .Where(GetUserWhere(user, tenant))
                        .Where(DisplayImagesFields.address, address));
            }
        }

        public List<string> GetDisplayImagesAddresses(int tenant, string user)
        {
            List<string> addresses;

            using (var db = GetDb())
            {
                addresses = db.ExecuteList(
                    new SqlQuery(MAIL_DISPLAY_IMAGES)
                        .Select(DisplayImagesFields.address)
                        .Where(GetUserWhere(user, tenant)))
                        .ConvertAll(fields => fields[0].ToString());
            }

            return addresses;
        }

        #endregion

    }
}
