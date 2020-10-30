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
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ASC.Data.Storage.Encryption;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASC.Data.Storage.Tests.Encryption
{
    [TestClass]
    public class SpeedTest
    {
        private readonly CommonMethods CommonMethods;
        private readonly EncryptionSettings Settings;
        private readonly Crypt Crypt;


        public SpeedTest()
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
        public void GetFileSizeSpeedTest()
        {
            var iterations = 100000;
            var fileInfo = CommonMethods.CreateTestFile();


            long fileInfoSize = 0;
            var fileInfoWatch = Stopwatch.StartNew();
            //Parallel.For(0, iterations, (a) => fileInfoSize = new FileInfo(fileInfo.Path).Length);
            for (var i = 0; i < iterations; i++)
            {
                fileInfoSize = new FileInfo(fileInfo.Path).Length;
            }
            fileInfoWatch.Stop();
            var fileInfoSizeSpeed = (float)fileInfoWatch.ElapsedMilliseconds;


            Crypt.EncryptFile(fileInfo.Path);


            long discDataTransformSize = 0;
            var discDataTransformWatch = Stopwatch.StartNew();
            //Parallel.For(0, iterations, (a) => discDataTransformSize = Crypt.GetFileSize(fileInfo.Path));
            for (var i = 0; i < iterations; i++)
            {
                discDataTransformSize = Crypt.GetFileSize(fileInfo.Path);
            }
            discDataTransformWatch.Stop();
            var discDataTransformSpeed = (float)discDataTransformWatch.ElapsedMilliseconds;


            CommonMethods.DeleteTestFile(fileInfo.Path);


            Assert.AreEqual(fileInfoSize, discDataTransformSize);
            Assert.AreEqual(true, discDataTransformSpeed / fileInfoSizeSpeed < 2); // ~1.6 times slower
        }

        [TestMethod]
        public void EncryptDecryptManyFilesSpeedTest()
        {
            var directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            while (Directory.Exists(directoryPath))
            {
                directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            }

            var directoryInfo = Directory.CreateDirectory(directoryPath);
            var size = 2L * 1024 * 1024;
            var index = 0;

            while (index < 100)
            {
                var filePath = Path.Combine(directoryPath, string.Format("{0}.txt", Guid.NewGuid()));
                CommonMethods.CreateTestFile(filePath, size);
                index++;
            }

            var files = directoryInfo.GetFiles();


            var encryptWatch = Stopwatch.StartNew();
            foreach (var file in files)
            {
                Crypt.EncryptFile(file.FullName);
            }
            encryptWatch.Stop();
            var encryptSpeed = (float)encryptWatch.ElapsedMilliseconds;


            var decryptWatch = Stopwatch.StartNew();
            foreach (var file in files)
            {
                Crypt.DecryptFile(file.FullName);
            }
            decryptWatch.Stop();
            var decryptSpeed = (float)decryptWatch.ElapsedMilliseconds;


            foreach (var file in files)
            {
                File.Delete(file.FullName);
            }

            directoryInfo.Delete();

            Assert.AreEqual(true, encryptSpeed > 0);
            Assert.AreEqual(true, decryptSpeed > 0);
        }

        [TestMethod]
        public void EncryptDecryptHugeFileSpeedTest()
        {
            var size = 200L * 1024 * 1024;

            var filePath = CommonMethods.CreateTestFile(size);

            var fileSize = new FileInfo(filePath).Length;

            var fileEncryptWatch = Stopwatch.StartNew();

            Crypt.EncryptFile(filePath);

            fileEncryptWatch.Stop();

            var fileEncryptSpeed = (float)fileEncryptWatch.ElapsedMilliseconds;

            Assert.AreEqual(fileSize, Crypt.GetFileSize(filePath));

            var fileDecryptWatch = Stopwatch.StartNew();

            Crypt.DecryptFile(filePath);

            fileDecryptWatch.Stop();

            var fileDecryptSpeed = (float)fileDecryptWatch.ElapsedMilliseconds;

            Assert.AreEqual(fileSize, new FileInfo(filePath).Length);

            CommonMethods.DeleteTestFile(filePath);

            Assert.AreEqual(true, fileEncryptSpeed > 0);
            Assert.AreEqual(true, fileDecryptSpeed > 0);
        }

        private void ReadFile(string filePath)
        {
            using (var stream = Crypt.GetReadStream(filePath))
            {
                byte[] array = new byte[81920];
                while (stream.Read(array, 0, array.Length) != 0)
                {
                }
            }
        }

        [TestMethod]
        public void EncryptReadHugeFileMemoryTest()
        {
            var size = 1L * 1024 * 1024 * 1024;

            var filePath = CommonMethods.CreateTestFile(size);

            Crypt.EncryptFile(filePath);

            try
            {
                ReadFile(filePath);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                CommonMethods.DeleteTestFile(filePath);
            }
        }

        [TestMethod]
        public void ParallelReadHugeFileMemoryTest()
        {
            var size = 1L * 1024 * 1024 * 1024;

            var filePath = CommonMethods.CreateTestFile(size);

            Crypt.EncryptFile(filePath);

            var array = new int[3]; // System.OutOfMemoryException when use MemoryStream

            try
            {
                Parallel.ForEach(array, (item) =>
                {
                    ReadFile(filePath);
                });
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                CommonMethods.DeleteTestFile(filePath);
            }
        }

        [TestMethod]
        public void ParallelReadFileTest()
        {
            var size = 2L * 1024 * 1024;

            var filePath = CommonMethods.CreateTestFile(size);

            Crypt.EncryptFile(filePath);

            var array = new int[10];

            try
            {
                Parallel.ForEach(array, (item) =>
                {
                    ReadFile(filePath);
                });
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                CommonMethods.DeleteTestFile(filePath);
            }
        }

        [TestMethod]
        public void IterationsSpeedTest()
        {
            var password  = "password";
            var salt = new byte[32];
            var iterations = 10000;

            const int keySize = 256;
            const int blockSize = 128;

            byte[] key;
            byte[] iv;

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                key = deriveBytes.GetBytes(keySize / 8);
                iv = deriveBytes.GetBytes(blockSize / 8);
            }
        }

        [TestMethod]
        public void EncryptDecryptWithCryptoStream()
        {
            var srcFile = Path.Combine(Path.GetTempPath(), "test.src");

            if (File.Exists(srcFile)) File.Delete(srcFile);

            File.WriteAllText(srcFile, "test");

            var encFile = Path.Combine(Path.GetTempPath(), "test.enc");

            if (File.Exists(encFile)) File.Delete(encFile);

            var decFile = Path.Combine(Path.GetTempPath(), "test.dec");

            if (File.Exists(decFile)) File.Delete(decFile);

            var algorithm = new RijndaelManaged();

            using (var destination = new FileStream(encFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (var cryptoStream = new CryptoStream(destination, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (var source = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        source.CopyTo(cryptoStream);
                    }
                }
            }

            using (var destination = new FileStream(decFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (var cryptoStream = new CryptoStream(destination, algorithm.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    using (var source = new FileStream(encFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        source.CopyTo(cryptoStream);
                    }
                }
            }

            Assert.AreNotEqual(File.ReadAllText(srcFile), File.ReadAllText(encFile));
            Assert.AreEqual(File.ReadAllText(srcFile), File.ReadAllText(decFile));

            File.Delete(srcFile);
            File.Delete(encFile);
            File.Delete(decFile);
        }
    }
}
