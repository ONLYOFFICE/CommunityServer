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
            return StorageFactory.GetStorage(null, tennant.ToString(), module, null, null);
        }


        private Stream GetDataStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes("unit test generated file"));
        }

        [TestMethod]
        public void SslLinkGeneration()
        {
            var uri = StorageFactory.GetStorage(null, 23.ToString(), "photo", null, null).Save("", defaultfile, GetDataStream());
        }

        [TestMethod]
        public void TestFile()
        {
            var stream = GetDataStream();
            var uri = store.Save(defauldomain, defaultfile, stream);
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
            stream.Position = 0;
        }


        [TestMethod]
        public void Test2()
        {
            var storage = StorageFactory.GetStorage("0", "fckuploaders");
            var list = storage.ListFiles("forum", "40105221-fb0c-4943-bccd-baa635a016f7/", "*.*", true);
            var listRel = storage.ListFilesRelative("forum", "40105221-fb0c-4943-bccd-baa635a016f7/", "*.*", true);
            storage.DeleteFiles("forum", "40105221-fb0c-4943-bccd-baa635a016f7/", "*.*", true);
        }

        [TestMethod]
        public void TestListrelative()
        {
            var listing = GetStorageWithoutQuota(0, "fckuploaders").ListFilesRelative("blogs", "", "*.*", true);
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
            }
        }
    }
}
#endif