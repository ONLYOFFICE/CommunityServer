/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using System.Collections.Generic;
using System.IO;
using System.Text;

using ASC.Common;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace ASC.Data.Backup
{
    public class ZipWriteOperator : IDataWriteOperator
    {
        private readonly GZipOutputStream gZipOutputStream;
        private readonly TarOutputStream tarOutputStream;
        private readonly Stream file;


        public ZipWriteOperator(string targetFile)
        {
            file = new FileStream(targetFile, FileMode.Create);
            gZipOutputStream = new GZipOutputStream(file);
            tarOutputStream = new TarOutputStream(gZipOutputStream, Encoding.UTF8);
        }

        public void WriteEntry(string key, Stream stream)
        {
            using (var buffered = stream.GetBuffered())
            {
                var entry = TarEntry.CreateTarEntry(key);
                entry.Size = buffered.Length;
                tarOutputStream.PutNextEntry(entry);
                buffered.Position = 0;
                buffered.CopyTo(tarOutputStream);
                tarOutputStream.CloseEntry();
            }
        }

        public void Dispose()
        {
            tarOutputStream.Close();
            tarOutputStream.Dispose();
        }
    }

    public class ZipReadOperator : IDataReadOperator
    {
        private readonly string tmpdir;

        public ZipReadOperator(string targetFile)
        {
            tmpdir = Path.Combine(Path.GetDirectoryName(targetFile), Path.GetFileNameWithoutExtension(targetFile).Replace('>', '_').Replace(':', '_').Replace('?', '_'));

            using (var stream = File.OpenRead(targetFile))
            using (var reader = new GZipInputStream(stream))
            using (var tarOutputStream = TarArchive.CreateInputTarArchive(reader, Encoding.UTF8))
            {
                tarOutputStream.ExtractContents(tmpdir);
            }

            File.Delete(targetFile);
        }

        public Stream GetEntry(string key)
        {
            var filePath = Path.Combine(tmpdir, key);
            return File.Exists(filePath) ? File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read) : null;
        }

        public IEnumerable<string> GetEntries(string key)
        {
            var path = Path.Combine(tmpdir, key);
            var files = Directory.EnumerateFiles(path);
            return files;
        }

        public void Dispose()
        {
            if (Directory.Exists(tmpdir))
            {
                Directory.Delete(tmpdir, true);
            }
        }
    }
}