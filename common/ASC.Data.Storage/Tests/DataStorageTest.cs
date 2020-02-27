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


#if DEBUG
namespace ASC.Data.Storage.Tests
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using ASC.Data.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DataStorageTest
    {
        private readonly IDataStore store;
        private const string defaultmodule = "forum";
        private const string defauldomain = "forum";
        private const string defaultfile = "test.txt";


        public DataStorageTest()
        {
            store = GetStorageWithoutQuota();
        }

        private static IDataStore GetStorageWithoutQuota()
        {
            return GetStorageWithoutQuota(0);
        }

        private static IDataStore GetStorageWithoutQuota(int tennant)
        {
            return GetStorageWithoutQuota(tennant, defaultmodule);
        }

        private static IDataStore GetStorageWithoutQuota(int tennant, string module)
        {
            return StorageFactory.GetStorage(null, tennant.ToString(), module, null);
        }


        private Stream GetDataStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes("unit test generated file"));
        }

        [TestMethod]
        public void SslLinkGeneration()
        {
            var uri = StorageFactory.GetStorage(null, 23.ToString(), "userPhotos", null).Save("", defaultfile, GetDataStream());
            Assert.IsNotNull(uri);
        }

        [TestMethod]
        public void TestFile()
        {
            var stream = GetDataStream();
            var uri = store.Save(defauldomain, defaultfile, stream);
            Assert.IsNotNull(uri);
            var files = store.ListFiles(defauldomain, "", "*.*", true);
            Assert.IsNotNull(files);
            Assert.IsNotNull(files.Where(x => x.ToString().Equals(uri.ToString())).SingleOrDefault());
            var size = store.GetFileSize(defauldomain, defaultfile);
            Assert.AreEqual(size, GetDataStream().Length);
            var moved = store.Move(defauldomain, defaultfile, "", "testmoved.txt");
            files = store.ListFiles("", "testmoved.txt", "*.*", true);
            Assert.IsNotNull(files);
            Assert.IsNotNull(files.Where(x => x.ToString().Equals(moved.ToString())).SingleOrDefault());

            store.Delete("", "testmoved.txt");
            files = store.ListFiles(defauldomain, "", "*.*", true);
            Assert.IsNotNull(files);
            Assert.IsNull(files.Where(x => x.ToString().Equals(uri.ToString())).SingleOrDefault());
        }

        [TestMethod]
        public void TestDisposeStream()
        {
            var stream = GetDataStream();
            var uri = store.Save(defauldomain, defaultfile, stream);
            Assert.IsNotNull(uri);
            stream.Position = 0;
        }


        [TestMethod]
        public void Test2()
        {
            var storage = StorageFactory.GetStorage("0", "fckuploaders");
            var list = storage.ListFiles("forum", "40105221-fb0c-4943-bccd-baa635a016f7/", "*.*", true);
            var listRel = storage.ListFilesRelative("forum", "40105221-fb0c-4943-bccd-baa635a016f7/", "*.*", true);
            storage.DeleteFiles("forum", "40105221-fb0c-4943-bccd-baa635a016f7/", "*.*", true);

            Assert.IsNotNull(list);
            Assert.IsNotNull(listRel);
        }

        [TestMethod]
        public void TestListrelative()
        {
            var listing = GetStorageWithoutQuota(0, "fckuploaders").ListFilesRelative("blogs", "", "*.*", true);
            Assert.IsNotNull(listing);
        }

        [TestMethod]
        public void GetFilesTest()
        {
            var store = StorageFactory.GetStorage("0", "crm");
            store.Save("path/file.jpg", new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6 }));
            var list = store.ListFilesRelative("", "path", "*", false);
            foreach (var f in list)
            {
                var stream = store.GetReadStream(f.ToString());
                Assert.IsNotNull(stream);
            }
        }
    }
}
#endif