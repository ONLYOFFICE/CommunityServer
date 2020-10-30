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


#if DEBUG
namespace ASC.Data.Storage.Tests
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ASC.Data.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DataStorageTest
    {
        private readonly IDataStore store;
        private const string defaultModule = "forum";
        private const string defaultDomain = "";
        private const string defaultDirectory = "directory";
        private const string defaultFile = "test.txt";


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
            return GetStorageWithoutQuota(tennant, defaultModule);
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
            Assert.IsNotNull(store);

            var uri = store.Save(defaultDomain, defaultFile, GetDataStream());
            Assert.AreEqual(true, store.IsFile(defaultDomain, defaultFile));
            Assert.IsNotNull(uri);

            store.Delete(defaultDomain, defaultFile);
            Assert.AreEqual(false, store.IsFile(defaultDomain, defaultFile));
        }

        [TestMethod]
        public void MoveTest()
        {
            
            Assert.IsNotNull(store);

            var srcFilePath = Path.Combine(defaultDirectory, defaultFile);
            var srcUri = store.Save(defaultDomain, srcFilePath, GetDataStream());
            Assert.IsNotNull(srcUri);

            var files = store.ListFiles(defaultDomain, defaultDirectory, defaultFile, true);
            Assert.IsNotNull(files);
            Assert.AreEqual(true, files.Any());

            var size = store.GetFileSize(defaultDomain, srcFilePath);
            Assert.AreEqual(size, GetDataStream().Length);

            var movedFileName = "moved-" + defaultFile;
            var destFilePath = Path.Combine(defaultDirectory, movedFileName);
            var movedUri = store.Move(defaultDomain, srcFilePath, defaultDomain, destFilePath);
            Assert.IsNotNull(movedUri);

            files = store.ListFiles(defaultDomain, defaultDirectory, movedFileName, true);
            Assert.IsNotNull(files);
            Assert.AreEqual(true, files.Any());

            store.Delete(defaultDomain, destFilePath);

            files = store.ListFiles(defaultDomain, defaultDirectory, "*.*", true);
            Assert.IsNotNull(files);
            Assert.AreEqual(false, files.Any());

            store.DeleteDirectory(defaultDomain, defaultDirectory);
            Assert.AreEqual(false, store.IsDirectory(defaultDomain, defaultDirectory));
        }

        [TestMethod]
        public void DisposeStreamTest()
        {
            Assert.IsNotNull(store);

            var stream = GetDataStream();
            Assert.IsNotNull(stream);

            var uri = store.Save(defaultDomain, defaultFile, stream);
            Assert.AreEqual(true, store.IsFile(defaultDomain, defaultFile));
            Assert.IsNotNull(uri);

            Assert.AreEqual(false, stream.Position == 0);

            store.Delete(defaultDomain, defaultFile);
            Assert.AreEqual(false, store.IsFile(defaultDomain, defaultFile));
        }


        [TestMethod]
        public void ListFilesTest()
        {
            Assert.IsNotNull(store);

            var path = Path.Combine(defaultDirectory, defaultFile);
            var uri = store.Save(defaultDomain, path, GetDataStream());
            Assert.AreEqual(true, store.IsFile(defaultDomain, path));
            Assert.IsNotNull(uri);

            var list = store.ListFiles(defaultDomain, defaultDirectory, "*.*", true);
            Assert.IsNotNull(list);
            Assert.AreEqual(true, list.Any());

            store.DeleteFiles(defaultDomain, defaultDirectory, "*.*", true);

            list = store.ListFiles(defaultDomain, defaultDirectory, "*.*", true);
            Assert.IsNotNull(list);
            Assert.AreEqual(false, list.Any());

            store.DeleteDirectory(defaultDomain, defaultDirectory);
            Assert.AreEqual(false, store.IsDirectory(defaultDomain, defaultDirectory));
        }

        [TestMethod]
        public void ListFilesRelativeTest()
        {
            Assert.IsNotNull(store);

            var path = Path.Combine(defaultDirectory, defaultFile);
            var uri = store.Save(defaultDomain, path, GetDataStream());
            Assert.AreEqual(true, store.IsFile(defaultDomain, path));
            Assert.IsNotNull(uri);

            var listRel = store.ListFilesRelative(defaultDomain, defaultDirectory, "*.*", true);
            Assert.IsNotNull(listRel);
            Assert.AreEqual(true, listRel.Any());

            store.DeleteFiles(defaultDomain, defaultDirectory, "*.*", true);

            listRel = store.ListFilesRelative(defaultDomain, defaultDirectory, " *.*", true);
            Assert.IsNotNull(listRel);
            Assert.AreEqual(false, listRel.Any());

            store.DeleteDirectory(defaultDomain, defaultDirectory);
            Assert.AreEqual(false, store.IsDirectory(defaultDomain, defaultDirectory));
        }

        [TestMethod]
        public void GetReadStreamTest()
        {
            Assert.IsNotNull(store);

            var path = Path.Combine(defaultDirectory, defaultFile);
            var uri = store.Save(path, GetDataStream());
            Assert.IsNotNull(uri);
            Assert.AreEqual(true, store.IsFile(defaultDomain, path));

            var list = store.ListFilesRelative(defaultDomain, defaultDirectory, "*", false);
            Assert.IsNotNull(list);
            Assert.AreEqual(true, list.Any());

            foreach (var file in list)
            {
                var stream = store.GetReadStream(Path.Combine(defaultDirectory, file));
                Assert.IsNotNull(stream);
                stream.Close();
            }

            store.Delete(path);
            Assert.AreEqual(false, store.IsFile(defaultDomain, path));

            store.DeleteDirectory(defaultDirectory);
            Assert.AreEqual(false, store.IsDirectory(defaultDomain, defaultDirectory));
        }

        private void ReadFile(IDataStore dataStore, string directory, string file)
        {
            using (var stream = dataStore.GetReadStream(Path.Combine(directory, file)))
            {
                byte[] array = new byte[81920];
                while (stream.Read(array, 0, array.Length) != 0)
                {
                }
            }
        }

        [TestMethod]
        public void ParallelGetReadStreamTest()
        {
            var path = Path.Combine(defaultDirectory, defaultFile);
            var uri = store.Save(path, GetDataStream());

            var array = new int[10];

            Parallel.ForEach(array, (item) =>
            {
                ReadFile(store, defaultDirectory, defaultFile);
            });

            store.Delete(path);
            store.DeleteDirectory(defaultDirectory);
        }
    }
}
#endif