/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Drawing;
using System.Collections.Generic;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Files.Classes;


namespace ASC.Mail.Aggregator.Common
{
    public static class ContactPhotoManager
    {
        #region Members

        private const string PHOTOS_BASE_DIR_NAME = "photos";
        private static readonly Dictionary<int, IDictionary<Size, string>> PhotoCache = new Dictionary<int, IDictionary<Size, string>>();

        private static readonly Size BigSize = new Size(200, 200);
        private static readonly Size MediumSize = new Size(82, 82);
        private static readonly Size SmallSize = new Size(40, 40);

        private static readonly object Locker = new object();

        #endregion

        #region Get Photo Methods

        public static String GetSmallSizePhoto(int contacId)
        {
            return contacId <= 0 ? GetDefaultPhoto(SmallSize) : GetPhotoUri(contacId, SmallSize);
        }

        public static String GetMediumSizePhoto(int contacId)
        {
            return contacId <= 0 ? GetDefaultPhoto(MediumSize) : GetPhotoUri(contacId, MediumSize);
        }

        public static String GetBigSizePhoto(int contacId)
        {
            return contacId <= 0 ? GetDefaultPhoto(BigSize) : GetPhotoUri(contacId, BigSize);
        }

        #endregion

        #region Private Methods

        private static String GetDefaultPhoto(Size photoSize)
        {
            const int ContacId = -1;

            var defaultPhotoUri = FromCache(ContacId, photoSize);

            if (!String.IsNullOrEmpty(defaultPhotoUri)) return defaultPhotoUri;

            defaultPhotoUri = WebImageSupplier.GetAbsoluteWebPath(String.Format("empty_people_logo_{0}_{1}.png", photoSize.Height, photoSize.Width), WebItemManager.MailProductID);

            ToCache(ContacId, defaultPhotoUri, photoSize);

            return defaultPhotoUri;
        }

        private static String GetPhotoUri(int contactId, Size photoSize)
        {
            var photoUri = FromCache(contactId, photoSize);

            if (!String.IsNullOrEmpty(photoUri)) return photoUri;

            photoUri = FromDataStore(contactId, photoSize);

            if (String.IsNullOrEmpty(photoUri))
                photoUri = GetDefaultPhoto(photoSize);

            ToCache(contactId, photoUri, photoSize);

            return photoUri;
        }

        private static String BuildFileDirectory(int contactId)
        {
            var s = contactId.ToString("000000");

            return String.Concat(PHOTOS_BASE_DIR_NAME, "/", s.Substring(0, 2), "/",
                                 s.Substring(2, 2), "/",
                                 s.Substring(4), "/");
        }

        private static String BuildFileName(int contactId, Size photoSize)
        {
            return String.Format("contact_{0}_{1}_{2}", contactId, photoSize.Width, photoSize.Height);
        }

        #endregion

        #region Cache and DataStore Methods

        private static String FromCache(int contactId, Size photoSize)
        {
            lock (Locker)
            {
                if (PhotoCache.ContainsKey(contactId))
                {
                    if (PhotoCache[contactId].ContainsKey(photoSize))
                    {
                        return PhotoCache[contactId][photoSize];
                    }
                }
            }
            return String.Empty;
        }

        private static void ToCache(int contactId, String photoUri, Size photoSize)
        {
            lock (Locker)
            {
                if (PhotoCache.ContainsKey(contactId))
                    if (PhotoCache[contactId].ContainsKey(photoSize))
                        PhotoCache[contactId][photoSize] = photoUri;
                    else
                        PhotoCache[contactId].Add(photoSize, photoUri);
                else
                    PhotoCache.Add(contactId, new Dictionary<Size, string> { { photoSize, photoUri } });
            }
        }

        private static String FromDataStore(int contactId, Size photoSize)
        {
            var directoryPath = BuildFileDirectory(contactId);

            var filesUri = Global.GetStore().ListFiles(directoryPath, BuildFileName(contactId, photoSize) + "*", false);

            return filesUri.Length == 0 ? String.Empty : filesUri[0].ToString();
        }

        #endregion
    }
}
