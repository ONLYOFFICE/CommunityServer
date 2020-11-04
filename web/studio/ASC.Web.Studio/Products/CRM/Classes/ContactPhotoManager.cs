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
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Linq;
using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Common.Threading.Workers;
using ASC.Data.Storage;
using ASC.Web.CRM.Configuration;
using ASC.Web.CRM.Resources;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core;

namespace ASC.Web.CRM.Classes
{
    public class ResizeWorkerItem
    {
        public int ContactID { get; set; }

        public bool UploadOnly { get; set; }

        public String TmpDirName { get; set; }

        public Size[] RequireFotoSize { get; set; }

        public byte[] ImageData { get; set; }

        public IDataStore DataStore { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is ResizeWorkerItem)) return false;

            var item = (ResizeWorkerItem)obj;

            return item.ContactID.Equals(ContactID) && RequireFotoSize.Equals(item.RequireFotoSize) && ImageData.Length == item.ImageData.Length;
        }

        public override int GetHashCode()
        {
            return ContactID ^ RequireFotoSize.GetHashCode() ^ ImageData.Length;
        }
    }

    public static class ContactPhotoManager
    {
        #region Members

        private const string PhotosBaseDirName = "photos";
        private const string PhotosDefaultTmpDirName = "temp";

        private static readonly Dictionary<int, IDictionary<Size, string>> _photoCache = new Dictionary<int, IDictionary<Size, string>>();

        private static readonly WorkerQueue<ResizeWorkerItem> ResizeQueue = new WorkerQueue<ResizeWorkerItem>(2, TimeSpan.FromSeconds(30), 1, true);
        private static readonly ICacheNotify cachyNotify; 

        private static readonly Size _oldBigSize = new Size(145, 145);

        private static readonly Size _bigSize = new Size(200, 200);
        private static readonly Size _mediumSize = new Size(82, 82);
        private static readonly Size _smallSize = new Size(40, 40);

        private static readonly object locker = new object();

        #endregion

        #region Cache and DataStore Methods

        static ContactPhotoManager()
        {
            cachyNotify = AscCache.Notify;
            cachyNotify.Subscribe<KeyValuePair<int, KeyValuePair<Size, string>>>(
                (item, action) =>
                {
                    var contactID = item.Key;
                    var sizeUriPair = item.Value;

                    switch (action)
                    {
                        case CacheNotifyAction.InsertOrUpdate:
                            ToPrivateCache(contactID, sizeUriPair.Value, sizeUriPair.Key);
                            break;
                        case CacheNotifyAction.Remove:
                            RemoveFromPrivateCache(contactID);
                            break;
                    }
                });
        }

        private static String FromCache(int contactID, Size photoSize)
        {
            lock (locker)
            {
                if (_photoCache.ContainsKey(contactID))
                {
                    if (_photoCache[contactID].ContainsKey(photoSize))
                    {
                        return _photoCache[contactID][photoSize];
                    }
                    if (photoSize == _bigSize && _photoCache[contactID].ContainsKey(_oldBigSize))
                    {
                        return _photoCache[contactID][_oldBigSize];
                    }
                }
            }
            return String.Empty;
        }

        private static void RemoveFromCache(int contactID)
        {
            cachyNotify.Publish(CreateCacheItem(contactID, "", Size.Empty), CacheNotifyAction.Remove);
        }

        private static void RemoveFromPrivateCache(int contactID)
        {
            lock (locker)
            {
                _photoCache.Remove(contactID);
            }
        }

        private static void ToCache(int contactID, String photoUri, Size photoSize)
        {
            cachyNotify.Publish(CreateCacheItem(contactID, photoUri, photoSize), CacheNotifyAction.InsertOrUpdate);
        }

        private static void ToPrivateCache(int contactID, String photoUri, Size photoSize)
        {
            lock (locker)
            {
                if (_photoCache.ContainsKey(contactID))
                    if (_photoCache[contactID].ContainsKey(photoSize))
                        _photoCache[contactID][photoSize] = photoUri;
                    else
                        _photoCache[contactID].Add(photoSize, photoUri);
                else
                    _photoCache.Add(contactID, new Dictionary<Size, string> {{photoSize, photoUri}});
            }
        }

        private static KeyValuePair<int, KeyValuePair<Size, string>> CreateCacheItem(int contactID, String photoUri, Size photoSize)
        {
            var sizeUriPair = new KeyValuePair<Size, string>(photoSize, photoUri);
            return new KeyValuePair<int, KeyValuePair<Size, string>>(contactID, sizeUriPair);
        }

        private static String FromDataStore(int contactID, Size photoSize)
        {
            return FromDataStore(contactID, photoSize, false, null);
        }

        private static String FromDataStore(int contactID, Size photoSize, Boolean isTmpDir, String tmpDirName)
        {
            var directoryPath = !isTmpDir
                                    ? BuildFileDirectory(contactID)
                                    : (String.IsNullOrEmpty(tmpDirName) ? BuildFileTmpDirectory(contactID) : BuildFileTmpDirectory(tmpDirName));

            var filesURI = Global.GetStore().ListFiles(directoryPath, BuildFileName(contactID, photoSize) + "*", false);

            if (filesURI.Length == 0 && photoSize == _bigSize)
            {
                filesURI = Global.GetStore().ListFiles(directoryPath, BuildFileName(contactID, _oldBigSize) + "*", false);
            }

            if (filesURI.Length == 0)
            {
                return String.Empty;
            }

            return filesURI[0].ToString();
        }

        private static String FromDataStoreRelative(int contactID, Size photoSize, Boolean isTmpDir, String tmpDirName)
        {
            var directoryPath = !isTmpDir
                                    ? BuildFileDirectory(contactID)
                                    : (String.IsNullOrEmpty(tmpDirName) ? BuildFileTmpDirectory(contactID) : BuildFileTmpDirectory(tmpDirName));

            var filesPaths = Global.GetStore().ListFilesRelative("", directoryPath, BuildFileName(contactID, photoSize) + "*", false);

            if (filesPaths.Length == 0 && photoSize == _bigSize)
            {
                filesPaths = Global.GetStore().ListFilesRelative("", directoryPath, BuildFileName(contactID, _oldBigSize) + "*", false);
            }

            if (filesPaths.Length == 0)
            {
                return String.Empty;
            }

            return Path.Combine(directoryPath, filesPaths[0]);
        }

        private static PhotoData FromDataStore(Size photoSize, String tmpDirName)
        {
            var directoryPath = BuildFileTmpDirectory(tmpDirName);

            if (!Global.GetStore().IsDirectory(directoryPath))
                return null;

            var filesURI = Global.GetStore().ListFiles(directoryPath, BuildFileName(0, photoSize) + "*", false);

            if (filesURI.Length == 0) return null;

            return new PhotoData { Url = filesURI[0].ToString(), Path = tmpDirName };
        }

        #endregion

        #region Private Methods

        private static String GetPhotoUri(int contactID, bool isCompany, Size photoSize)
        {
            var photoUri = FromCache(contactID, photoSize);

            if (!String.IsNullOrEmpty(photoUri)) return photoUri;

            photoUri = FromDataStore(contactID, photoSize);

            if (String.IsNullOrEmpty(photoUri))
                photoUri = GetDefaultPhoto(isCompany, photoSize);

            ToCache(contactID, photoUri, photoSize);

            return photoUri;
        }

        private static String BuildFileDirectory(int contactID)
        {
            var s = contactID.ToString("000000");

            return String.Concat(PhotosBaseDirName, "/", s.Substring(0, 2), "/",
                                 s.Substring(2, 2), "/",
                                 s.Substring(4), "/");
        }

        private static String BuildFileTmpDirectory(int contactID)
        {
            return String.Concat(BuildFileDirectory(contactID), PhotosDefaultTmpDirName, "/");
        }

        private static String BuildFileTmpDirectory(string tmpDirName)
        {
            return String.Concat(PhotosBaseDirName, "/", tmpDirName.TrimEnd('/'), "/");
        }

        private static String BuildFileName(int contactID, Size photoSize)
        {
            return String.Format("contact_{0}_{1}_{2}", contactID, photoSize.Width, photoSize.Height);
        }

        private static String BuildFilePath(int contactID, Size photoSize, String imageExtension)
        {
            if (photoSize.IsEmpty || contactID == 0)
                throw new ArgumentException();

            return String.Concat(BuildFileDirectory(contactID), BuildFileName(contactID, photoSize), imageExtension);
        }

        private static String BuildFileTmpPath(int contactID, Size photoSize, String imageExtension, String tmpDirName)
        {
            if (photoSize.IsEmpty || (contactID == 0 && String.IsNullOrEmpty(tmpDirName)))
                throw new ArgumentException();

            return String.Concat(
                String.IsNullOrEmpty(tmpDirName)
                    ? BuildFileTmpDirectory(contactID)
                    : BuildFileTmpDirectory(tmpDirName),
                BuildFileName(contactID, photoSize), imageExtension);
        }

        private static void ExecResizeImage(ResizeWorkerItem resizeWorkerItem)
        {
            foreach (var fotoSize in resizeWorkerItem.RequireFotoSize)
            {
                var data = resizeWorkerItem.ImageData;
                using (var stream = new MemoryStream(data))
                using (var img = new Bitmap(stream))
                {
                    var imgFormat = img.RawFormat;
                    if (fotoSize != img.Size)
                    {
                        using (var img2 = CommonPhotoManager.DoThumbnail(img, fotoSize, false, false, false))
                        {
                            data = CommonPhotoManager.SaveToBytes(img2, Global.GetImgFormatName(imgFormat));
                        }
                    }
                    else
                    {
                        data = Global.SaveToBytes(img);
                    }

                    var fileExtension = String.Concat("." + Global.GetImgFormatName(imgFormat));

                    var photoPath = !resizeWorkerItem.UploadOnly
                                        ? BuildFilePath(resizeWorkerItem.ContactID, fotoSize, fileExtension)
                                        : BuildFileTmpPath(resizeWorkerItem.ContactID, fotoSize, fileExtension, resizeWorkerItem.TmpDirName);

                    using (var fileStream = new MemoryStream(data))
                    {
                        var photoUri = resizeWorkerItem.DataStore.Save(photoPath, fileStream).ToString();
                        photoUri = String.Format("{0}?cd={1}", photoUri, DateTime.UtcNow.Ticks);

                        if (!resizeWorkerItem.UploadOnly)
                        {
                            ToCache(resizeWorkerItem.ContactID, photoUri, fotoSize);
                        }
                    }
                }
            }
        }

        private static String GetDefaultPhoto(bool isCompany, Size photoSize)
        {
            int contactID;

            if (isCompany)
                contactID = -1;
            else
                contactID = -2;

            var defaultPhotoUri = FromCache(contactID, photoSize);

            if (!String.IsNullOrEmpty(defaultPhotoUri)) return defaultPhotoUri;

            if (isCompany)
                defaultPhotoUri = WebImageSupplier.GetAbsoluteWebPath(String.Format("empty_company_logo_{0}_{1}.png", photoSize.Height, photoSize.Width), ProductEntryPoint.ID);
            else
                defaultPhotoUri = WebImageSupplier.GetAbsoluteWebPath(String.Format("empty_people_logo_{0}_{1}.png", photoSize.Height, photoSize.Width), ProductEntryPoint.ID);

            ToCache(contactID, defaultPhotoUri, photoSize);

            return defaultPhotoUri;
        }

        #endregion

        #region Delete Methods

        public static void DeletePhoto(int contactID)
        {
            DeletePhoto(contactID, false, null, true);
        }

        public static void DeletePhoto(int contactID, bool isTmpDir, string tmpDirName, bool recursive)
        {
            if (contactID == 0)
                throw new ArgumentException();

            lock (locker)
            {
                ResizeQueue.GetItems().Where(item => item.ContactID == contactID)
                           .All(item =>
                               {
                                   ResizeQueue.Remove(item);
                                   return true;
                               });

                var photoDirectory = !isTmpDir
                                         ? BuildFileDirectory(contactID)
                                         : (String.IsNullOrEmpty(tmpDirName) ? BuildFileTmpDirectory(contactID) : BuildFileTmpDirectory(tmpDirName));
                var store = Global.GetStore();

                if (store.IsDirectory(photoDirectory))
                {
                    store.DeleteFiles(photoDirectory, "*", recursive);
                    if (recursive)
                    {
                        store.DeleteDirectory(photoDirectory);
                    }
                }

                if (!isTmpDir)
                {
                    RemoveFromCache(contactID);
                }
            }
        }

        public static void DeletePhoto(string tmpDirName)
        {
            lock (locker)
            {

                var photoDirectory = BuildFileTmpDirectory(tmpDirName);
                var store = Global.GetStore();

                if (store.IsDirectory(photoDirectory))
                {
                    store.DeleteFiles(photoDirectory, "*", false);
                }
            }
        }

        #endregion

        public static void TryUploadPhotoFromTmp(int contactID, bool isNewContact, string tmpDirName)
        {
            var directoryPath = BuildFileDirectory(contactID);
            var dataStore = Global.GetStore();

            try
            {
                if (dataStore.IsDirectory(directoryPath))
                {
                    DeletePhoto(contactID, false, null, false);
                }
                foreach (var photoSize in new[] {_bigSize, _mediumSize, _smallSize})
                {
                    var photoTmpPath = FromDataStoreRelative(isNewContact ? 0 : contactID, photoSize, true, tmpDirName);
                    if (string.IsNullOrEmpty(photoTmpPath)) throw new Exception("Temp phono not found");

                    var imageExtension = Path.GetExtension(photoTmpPath);

                    var photoPath = String.Concat(directoryPath, BuildFileName(contactID, photoSize), imageExtension).TrimStart('/');

                    byte[] data;
                    using (var photoTmpStream = dataStore.GetReadStream(photoTmpPath))
                    {
                        data = Global.ToByteArray(photoTmpStream);
                    }
                    using (var fileStream = new MemoryStream(data))
                    {
                        var photoUri = dataStore.Save(photoPath, fileStream).ToString();
                        photoUri = String.Format("{0}?cd={1}", photoUri, DateTime.UtcNow.Ticks);
                        ToCache(contactID, photoUri, photoSize);
                    }
                }
                DeletePhoto(contactID, true, tmpDirName, true);
            }
            catch(Exception ex)
            {
                LogManager.GetLogger("ASC.CRM").ErrorFormat("TryUploadPhotoFromTmp for contactID={0} failed witth error: {1}", contactID, ex);
                return;
            }
        }

        #region Get Photo Methods

        public static String GetSmallSizePhoto(int contactID, bool isCompany)
        {
            if (contactID <= 0)
                return GetDefaultPhoto(isCompany, _smallSize);

            return GetPhotoUri(contactID, isCompany, _smallSize);
        }

        public static String GetMediumSizePhoto(int contactID, bool isCompany)
        {
            if (contactID <= 0)
                return GetDefaultPhoto(isCompany, _mediumSize);

            return GetPhotoUri(contactID, isCompany, _mediumSize);
        }

        public static String GetBigSizePhoto(int contactID, bool isCompany)
        {
            if (contactID <= 0)
                return GetDefaultPhoto(isCompany, _bigSize);

            return GetPhotoUri(contactID, isCompany, _bigSize);
        }

        #endregion

        private static PhotoData ResizeToBigSize(byte[] imageData, string tmpDirName)
        {
            return ResizeToBigSize(imageData, 0, true, tmpDirName);
        }

        private static PhotoData ResizeToBigSize(byte[] imageData, int contactID, bool uploadOnly, string tmpDirName)
        {
            var resizeWorkerItem = new ResizeWorkerItem
                {
                    ContactID = contactID,
                    UploadOnly = uploadOnly,
                    RequireFotoSize = new[] {_bigSize},
                    ImageData = imageData,
                    DataStore = Global.GetStore(),
                    TmpDirName = tmpDirName
                };

            ExecResizeImage(resizeWorkerItem);

            if (!uploadOnly)
            {
                return new PhotoData { Url = FromCache(contactID, _bigSize) };
            }
            else if (String.IsNullOrEmpty(tmpDirName))
            {
                return new PhotoData { Url = FromDataStore(contactID, _bigSize, true, null), Path = PhotosDefaultTmpDirName };
            }
            else
            {
                return FromDataStore(_bigSize, tmpDirName);
            }
        }

        private static void ExecGenerateThumbnail(byte[] imageData, int contactID, bool uploadOnly)
        {
            ExecGenerateThumbnail(imageData, contactID, uploadOnly, null);
        }

        private static void ExecGenerateThumbnail(byte[] imageData, string tmpDirName)
        {
            ExecGenerateThumbnail(imageData, 0, true, tmpDirName);
        }


        private static void ExecGenerateThumbnail(byte[] imageData, int contactID, bool uploadOnly, string tmpDirName)
        {
            var resizeWorkerItem = new ResizeWorkerItem
                {
                    ContactID = contactID,
                    UploadOnly = uploadOnly,
                    RequireFotoSize = new[] {_mediumSize, _smallSize},
                    ImageData = imageData,
                    DataStore = Global.GetStore(),
                    TmpDirName = tmpDirName
                };

            if (!ResizeQueue.GetItems().Contains(resizeWorkerItem))
                ResizeQueue.Add(resizeWorkerItem);

            if (!ResizeQueue.IsStarted) ResizeQueue.Start(ExecResizeImage);
        }

        private static byte[] ToByteArray(Stream inputStream, int streamLength)
        {
            using (var br = new BinaryReader(inputStream))
            {
                return br.ReadBytes(streamLength);
            }
        }

        #region UploadPhoto Methods

        public static PhotoData UploadPhoto(String imageUrl, int contactID, bool uploadOnly, bool checkFormat = true)
        {
            var request = (HttpWebRequest)WebRequest.Create(imageUrl);
            using (var response = request.GetResponse())
            {
                using (var inputStream = response.GetResponseStream())
                {
                    var imageData = ToByteArray(inputStream, (int)response.ContentLength);
                    return UploadPhoto(imageData, contactID, uploadOnly, checkFormat);
                }
            }
        }

        public static PhotoData UploadPhoto(Stream inputStream, int contactID, bool uploadOnly, bool checkFormat = true)
        {
            var imageData = Global.ToByteArray(inputStream);
            return UploadPhoto(imageData, contactID, uploadOnly, checkFormat);
        }

        public static PhotoData UploadPhoto(byte[] imageData, int contactID, bool uploadOnly, bool checkFormat = true)
        {
            if (contactID == 0)
                throw new ArgumentException();

            if (checkFormat)
                CheckImgFormat(imageData);

            DeletePhoto(contactID, uploadOnly, null, false);

            ExecGenerateThumbnail(imageData, contactID, uploadOnly);

            return ResizeToBigSize(imageData, contactID, uploadOnly, null);
        }


        public static PhotoData UploadPhotoToTemp(String imageUrl, String tmpDirName, bool checkFormat = true)
        {
            var request = (HttpWebRequest)WebRequest.Create(imageUrl);
            using (var response = request.GetResponse())
            {
                using (var inputStream = response.GetResponseStream())
                {
                    var imageData = ToByteArray(inputStream, (int)response.ContentLength);
                    if (string.IsNullOrEmpty(tmpDirName))
                    {
                        tmpDirName = Guid.NewGuid().ToString();
                    }
                    return UploadPhotoToTemp(imageData, tmpDirName, checkFormat);
                }
            }
        }

        public static PhotoData UploadPhotoToTemp(Stream inputStream, String tmpDirName, bool checkFormat = true)
        {
            var imageData = Global.ToByteArray(inputStream);
            return UploadPhotoToTemp(imageData, tmpDirName, checkFormat);
        }

        public static PhotoData UploadPhotoToTemp(byte[] imageData, String tmpDirName, bool checkFormat = true)
        {
            if (checkFormat)
                CheckImgFormat(imageData);

            DeletePhoto(tmpDirName);

            ExecGenerateThumbnail(imageData, tmpDirName);

            return ResizeToBigSize(imageData, tmpDirName);
        }

        public static ImageFormat CheckImgFormat(byte[] imageData)
        {
            using (var stream = new MemoryStream(imageData))
            using (var img = new Bitmap(stream))
            {
                var format = img.RawFormat;

                if (!format.Equals(ImageFormat.Png) && !format.Equals(ImageFormat.Jpeg))
                    throw new Exception(CRMJSResource.ErrorMessage_NotImageSupportFormat);

                return format;
            }
        }

        public class PhotoData
        {
            public string Url;
            public string Path;
        }

        #endregion
    }
}