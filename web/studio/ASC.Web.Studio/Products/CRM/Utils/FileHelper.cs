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


using System.IO;
using System.Reflection;

namespace ASC.Web.CRM.Classes
{
    public class FileHelper
    {
        public static void WriteTextToFile(string path, string text)
        {
            File.WriteAllText(path, text);
        }

        public static string ReadTextFromFile(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public static string ReadTextFromEmbeddedResource(string resourceName)
        {
            var result = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var sr = new StreamReader(stream))
                    {
                        result = sr.ReadToEnd();
                    }
                }
            }

            return result;
        }

        public static byte[] ReadBytesFromFile(string path)
        {
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public static byte[] ReadBytesFromEmbeddedResource(string resourceName)
        {
            byte[] result = null;
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        result = memoryStream.ToArray();
                    }
                }
            }

            return result;
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}