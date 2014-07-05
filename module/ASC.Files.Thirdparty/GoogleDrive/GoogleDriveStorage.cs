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

using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    internal class GoogleDriveStorage
    {

        private GoogleDocsStorageProvider _provider;

        public bool IsOpened { get; private set; }

        public long MaxUploadFileSize { get; internal set; }

        public long MaxChunkedUploadFileSize { get; internal set; }

        public GoogleDriveStorage()
        {
            MaxUploadFileSize = MaxChunkedUploadFileSize = 2L*1024L*1024L*1024L;
        }

        public void Open(string authToken)
        {
            if (IsOpened)
                return;

            if (_provider == null)
                _provider = new GoogleDocsStorageProvider();

            var token = new CloudStorage().DeserializeSecurityTokenFromBase64(authToken);
            _provider.Open(new GoogleDocsConfiguration(), token);

            IsOpened = true;
        }

        public void Close()
        {
            if (_provider != null)
                _provider.Close();

            IsOpened = false;
        }


        public ICloudDirectoryEntry GetRoot()
        {
            return _provider.GetRoot();
        }

        public string GetFileSystemObjectPath(ICloudFileSystemEntry fsObject)
        {
            return _provider.GetFileSystemObjectPath(fsObject);
        }

        public ICloudDirectoryEntry GetFolder(string path)
        {
            var ph = new PathHelper(path);
            if (!ph.IsPathRooted())
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);

            return GetFolder(path, null);
        }

        public ICloudDirectoryEntry GetFolder(string path, ICloudDirectoryEntry parent)
        {
            var dir = GetFileSystemObject(path, parent) as ICloudDirectoryEntry;
            if (dir == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);

            return dir;
        }

        public ICloudDirectoryEntry GetFolder(string path, bool throwException)
        {
            try
            {
                return GetFolder(path);
            }
            catch (SharpBoxException)
            {
                if (throwException)
                    throw;

                return null;
            }
        }

        public ICloudDirectoryEntry GetFolder(string path, ICloudDirectoryEntry startFolder, bool throwException)
        {
            try
            {
                return GetFolder(path, startFolder);
            }
            catch (SharpBoxException)
            {
                if (throwException)
                    throw;

                return null;
            }
        }

        public ICloudFileSystemEntry GetFile(string path, ICloudDirectoryEntry startFolder)
        {
            var fsEntry = GetFileSystemObject(path, startFolder);
            if (fsEntry is ICloudDirectoryEntry)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);

            return fsEntry;
        }

        public ICloudFileSystemEntry GetFileSystemObject(string name, ICloudDirectoryEntry parent)
        {
            return _provider.GetFileSystemObject(name, parent);
        }

        public ICloudFileSystemEntry CreateFile(ICloudDirectoryEntry parent, string name)
        {
            return _provider.CreateFile(parent, name);
        }

        public bool DeleteFileSystemEntry(ICloudFileSystemEntry fsentry)
        {
            return _provider.DeleteFileSystemEntry(fsentry);
        }

        public bool MoveFileSystemEntry(string filePath, string newParentPath)
        {
            if ((filePath == null) || (newParentPath == null))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);


            var ph = new PathHelper(filePath);
            var dir = ph.GetDirectoryName();
            var file = ph.GetFileName();

            if (dir.Length == 0)
                dir = "/";

            var container = GetFolder(dir);

            var fsEntry = GetFileSystemObject(file, container);

            var newParent = GetFolder(newParentPath);

            return MoveFileSystemEntry(fsEntry, newParent);
        }

        private bool MoveFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            return _provider.MoveFileSystemEntry(fsentry, newParent);
        }

        public bool CopyFileSystemEntry(string filePath, string newParentPath)
        {
            if ((filePath == null) || (newParentPath == null))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            var ph = new PathHelper(filePath);
            var dir = ph.GetDirectoryName();
            var file = ph.GetFileName();

            if (dir.Length == 0)
                dir = "/";

            var container = GetFolder(dir);

            var fsEntry = GetFileSystemObject(file, container);

            var newParent = GetFolder(newParentPath);

            return CopyFileSystemEntry(fsEntry, newParent);
        }

        private bool CopyFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            return _provider.CopyFileSystemEntry(fsentry, newParent);
        }

        public bool RenameFileSystemEntry(ICloudFileSystemEntry fsentry, string newName)
        {
            return _provider.RenameFileSystemEntry(fsentry, newName);
        }

        public ICloudDirectoryEntry CreateFolder(string path)
        {
            var ph = new PathHelper(path);

            return ph.IsPathRooted()
                       ? CreateFolderEx(path, GetRoot())
                       : null;
        }

        public ICloudDirectoryEntry CreateFolder(string name, ICloudDirectoryEntry parent)
        {
            return _provider.CreateFolder(name, parent);
        }

        private ICloudDirectoryEntry CreateFolderEx(string path, ICloudDirectoryEntry entry)
        {
            var ph = new PathHelper(path);

            var pes = ph.GetPathElements();

            foreach (var el in pes)
            {
                var cur = GetFolder(el, entry, false);

                if (cur == null)
                {
                    var newFolder = CreateFolder(el, entry);
                    if (newFolder == null)
                        throw new SharpBoxException(SharpBoxErrorCodes.ErrorCreateOperationFailed);

                    cur = newFolder;
                }

                entry = cur;
            }

            return entry;
        }
    }
}