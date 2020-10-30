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
using System.IO;
using System.Linq;

namespace ASC.Data.Storage.Tests.Encryption
{
    public class CommonMethods
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_-+=[{]};:>|./?";

        private static readonly Random random = new Random();

        public CommonFileInfo CreateTestFile()
        {
            var filePath = Path.GetTempFileName();

            File.WriteAllText(filePath, GetRandomString());

            return new CommonFileInfo(filePath);
        }

        public string CreateTestFile(long size)
        {
            var filePath = Path.GetTempFileName();

            var fileStream = new FileStream(filePath, FileMode.Open);

            fileStream.Seek(size, SeekOrigin.Begin);
            fileStream.WriteByte(0);
            fileStream.Close();

            return filePath;
        }

        public string CreateTestFile(string filePath, long size)
        {
            DeleteTestFile(filePath);

            var fileStream = new FileStream(filePath, FileMode.Create);

            fileStream.Seek(size, SeekOrigin.Begin);
            fileStream.WriteByte(0);
            fileStream.Close();

            return filePath;
        }

        public void DeleteTestFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static string GetRandomString()
        {
            var length = random.Next(0, chars.Length);

            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
