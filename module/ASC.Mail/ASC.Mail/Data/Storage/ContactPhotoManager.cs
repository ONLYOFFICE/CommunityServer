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
using System.Collections.Generic;
using System.Drawing;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Files.Classes;

namespace ASC.Mail.Data.Storage
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
            const int contacе_id = -1;

            var defaultPhotoUri = FromCache(contacе_id, photoSize);

            if (!String.IsNullOrEmpty(defaultPhotoUri)) return defaultPhotoUri;

            defaultPhotoUri = WebImageSupplier.GetAbsoluteWebPath(String.Format("empty_people_logo_{0}_{1}.png", photoSize.Height, photoSize.Width), WebItemManager.MailProductID);

            ToCache(contacе_id, defaultPhotoUri, photoSize);

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
