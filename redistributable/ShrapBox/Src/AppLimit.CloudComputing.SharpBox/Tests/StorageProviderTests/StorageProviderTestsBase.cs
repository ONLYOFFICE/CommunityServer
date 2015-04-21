#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppLimit.CloudComputing.SharpBox.Tests.StorageProviderTests
{
    [TestClass]
    public abstract class StorageProviderTestsBase
    {
        protected abstract CloudStorage CreateStorage();
        protected abstract ICloudStorageAccessToken GetAccessToken();

        private readonly List<ICloudFileSystemEntry> _toDelete = new List<ICloudFileSystemEntry>();
        private ICloudDirectoryEntry _root;
        private ICloudFileSystemEntry _uploadedFile;
        private CloudStorage _storage;

        protected const String DownloadPath = @"%USERPROFILE%\Desktop\";

        [TestCleanup]
        public virtual void TestTearDown()
        {
            foreach (var entry in _toDelete.ToArray())
            {
                GetStorage().DeleteFileSystemEntry(entry);
            }

            if(_uploadedFile != null)
            {
                GetStorage().DeleteFileSystemEntry(_uploadedFile);
            }

            if (File.Exists(GetDownloadedFilePath()))
            {
                File.Delete(GetDownloadedFilePath());   
            }
        }

        [TestMethod]
        public virtual void DeleteResourceTest()
        {
            var folder = GetStorage().CreateFolder("testfolder", GetRoot());
            Assert.IsTrue(GetStorage().DeleteFileSystemEntry(folder));
        }

        [TestMethod]
        public virtual void DeleteUploadedFileTest()
        {
            var file = UploadTestFileInternal();
            Assert.IsTrue(GetStorage().DeleteFileSystemEntry(file));
            _uploadedFile = null;
        }

        [TestMethod]
        public virtual void UploadFileTest()
        {
            var file = UploadTestFileInternal();
            Assert.AreNotEqual(0, file.Length);
        }

        [TestMethod]
        public virtual void DownloadFileTest()
        {
            var file = UploadTestFileInternal();
            GetStorage().DownloadFile(file.Parent, file.Id, DownloadPath);
            Assert.IsTrue(File.Exists(GetDownloadedFilePath()));
        }

        [TestMethod]
        public virtual void GetFileInfoTest()
        {
            var file1 = UploadTestFileInternal();
            var file2 = GetStorage().GetFile('/' + file1.Id, file1.Parent);
            Assert.AreEqual(file1.Id, file2.Id);
        }

        [TestMethod]
        public virtual void GetFolderInfoTest()
        {
            var folder1 = GetStorage().CreateFolder("testfolder", GetRoot());
            var folder2 = GetStorage().GetFolder("/" + folder1.Id);
            Assert.AreEqual(folder1.Id, folder2.Id);
        }

        [TestMethod]
        public virtual void MoveTest()
        {
            var folder1 = GetStorage().CreateFolder("testfolder1", GetRoot());
            var folder2 = GetStorage().CreateFolder("testfolder2", GetRoot());
            _toDelete.Add(folder1);
            _toDelete.Add(folder2);
            Assert.IsTrue(GetStorage().MoveFileSystemEntry(folder1, folder2));
            Assert.IsNull(GetRoot().GetChild(folder1.Id, false));
            Assert.IsNotNull(folder2.GetChild(folder1.Id, false));
        }

        [TestMethod]
        public virtual void CopyFolderTest()
        {
            var folder1 = GetStorage().CreateFolder("testfolder_copy_1", GetRoot());
            var folder2 = GetStorage().CreateFolder("testfolder_copy_2", GetRoot());
            _toDelete.Add(folder1);
            _toDelete.Add(folder2);
            Assert.IsTrue(GetStorage().CopyFileSystemEntry(folder1, folder2));
            var folder3 = GetStorage().GetFolder("/" + folder2.Id + "/" + folder1.Name);
            Assert.IsNotNull(folder3);
        }

        [TestMethod]
        public virtual void CreateFolderTest()
        {
            var folder = GetStorage().CreateFolder("testfolder", GetRoot());
            _toDelete.Add(folder);
            Assert.IsNotNull(GetRoot().GetChild(folder.Id));
        }

        [TestMethod]
        public virtual void CreateFolderWithCyrillicNameTest()
        {
            var folder = GetStorage().CreateFolder("новая папка", GetRoot());
            _toDelete.Add(folder);
            var folder1 = GetStorage().GetFolder("/" + folder.Id, GetRoot());
            Assert.AreEqual("новая папка", folder1.Name);
        }

        [TestMethod]
        public virtual void RenameResourceTest()
        {
            var folder = GetStorage().CreateFolder("testfolder", GetRoot());
            _toDelete.Add(folder);
            Assert.IsTrue(GetStorage().RenameFileSystemEntry(folder, "testfolder_renamed"));
            Assert.AreEqual("testfolder_renamed", folder.Name);
        }

        protected virtual String GetTestFilePath()
        {
            return @"Tests\StorageProviderTests\" + GetTestFileName();
        }

        protected virtual String GetTestFileName()
        {
            return "testtesttesttest.txt";
        }

        protected CloudStorage GetStorage()
        {
            return _storage ?? (_storage = CreateStorage());
        }

        protected ICloudDirectoryEntry GetRoot()
        {
            return _root = GetStorage().GetRoot();
        }

        protected ICloudFileSystemEntry UploadTestFileInternal()
        {
            if (_uploadedFile == null)
            {
                using (var fs = new FileStream(GetTestFilePath(), FileMode.Open, FileAccess.Read))
                {
                    var file = GetStorage().CreateFile(GetRoot(), "testtesttesttest.txt");
                    file.GetDataTransferAccessor().Transfer(fs, nTransferDirection.nUpload);
                    _uploadedFile = file;
                }
            }
            return _uploadedFile;
        }

        private String GetDownloadedFilePath()
        {
            return Environment.ExpandEnvironmentVariables(DownloadPath + GetTestFileName());
        }
    }
}
#endif