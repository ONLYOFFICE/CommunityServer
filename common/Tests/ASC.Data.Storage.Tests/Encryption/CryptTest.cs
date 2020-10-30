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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ASC.Data.Storage.Encryption;

namespace ASC.Data.Storage.Tests.Encryption
{
    [TestClass]
    public class CryptTest
    {
        private readonly CommonMethods CommonMethods;
        private readonly EncryptionSettings Settings;
        private readonly Crypt Crypt;


        public CryptTest()
        {
            CommonMethods = new CommonMethods();

            Settings = new EncryptionSettings()
            {
                Password = EncryptionSettings.GeneratePassword(32, 16),
                Status = EncryprtionStatus.Encrypted,
                NotifyUsers = false
            };

            Crypt = new Crypt("test", Settings);
        }


        [TestMethod]
        public void EncryptDecryptReadonlyFiles()
        {
            var testFile = CommonMethods.CreateTestFile();

            var fileInfo = new FileInfo(testFile.Path);

            Assert.AreEqual(false, fileInfo.IsReadOnly);

            fileInfo.IsReadOnly = true;

            Assert.AreEqual(true, fileInfo.IsReadOnly);

            Encrypt(testFile);

            Decrypt(testFile);

            fileInfo = new FileInfo(testFile.Path);

            Assert.AreEqual(false, fileInfo.IsReadOnly);

            CommonMethods.DeleteTestFile(testFile.Path);
        }

        [TestMethod]
        public void EncryptDecryptFile()
        {
            var testFile = CommonMethods.CreateTestFile();

            Encrypt(testFile);

            Decrypt(testFile);

            CommonMethods.DeleteTestFile(testFile.Path);
        }

        [TestMethod]
        public void EncryptTwiceDecryptTwiceFile()
        {
            var testFile = CommonMethods.CreateTestFile();

            Encrypt(testFile);

            Encrypt(testFile);

            Decrypt(testFile);

            Decrypt(testFile);

            CommonMethods.DeleteTestFile(testFile.Path);
        }

        [TestMethod]
        public void GetReadStream()
        {
            var testFile = CommonMethods.CreateTestFile();

            Encrypt(testFile);

            GetReadStream(testFile);

            Decrypt(testFile);

            CommonMethods.DeleteTestFile(testFile.Path);
        }

        [TestMethod]
        public void GetFileSize()
        {
            var testFile = CommonMethods.CreateTestFile();

            Crypt.EncryptFile(testFile.Path);

            var size = Crypt.GetFileSize(testFile.Path);

            CommonMethods.DeleteTestFile(testFile.Path);

            Assert.AreEqual(testFile.Size, size);
        }


        private void Encrypt(CommonFileInfo fileInfo)
        {
            Crypt.EncryptFile(fileInfo.Path);

            Assert.AreNotEqual(fileInfo.Content, File.ReadAllText(fileInfo.Path));

            Assert.AreEqual(fileInfo.Size, Crypt.GetFileSize(fileInfo.Path));
        }

        private void Decrypt(CommonFileInfo fileInfo)
        {
            Crypt.DecryptFile(fileInfo.Path);

            Assert.AreEqual(fileInfo.Content, File.ReadAllText(fileInfo.Path));

            Assert.AreEqual(fileInfo.Size, new FileInfo(fileInfo.Path).Length);
        }

        private void GetReadStream(CommonFileInfo fileInfo)
        {
            using (var stream = Crypt.GetReadStream(fileInfo.Path))
            {
                stream.Position = 0;
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
                Assert.AreEqual(fileInfo.Content, new StreamReader(stream).ReadToEnd());
                Assert.AreEqual(fileInfo.Size, stream.Length);
            }
        }
    }
}
