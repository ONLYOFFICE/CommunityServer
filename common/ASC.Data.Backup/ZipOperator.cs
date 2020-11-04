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
using System.Collections.Generic;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;

namespace ASC.Data.Backup
{
    public class ZipWriteOperator : IDataWriteOperator
    {
        private readonly IWriter writer;
        private readonly Stream file;


        public ZipWriteOperator(string targetFile)
        {
            file = new FileStream(targetFile, FileMode.Create);
            writer = WriterFactory.Open(file, ArchiveType.Tar, CompressionType.GZip);
        }

        public void WriteEntry(string key, string source)
        {
            writer.Write(key, source);
        }

        public void Dispose()
        {
            writer.Dispose();
            file.Close();
        }
    }

    public class ZipReadOperator : IDataReadOperator
    {
        private readonly string tmpdir;
        public List<string> Entries { get; private set; }

        public ZipReadOperator(string targetFile)
        {
            tmpdir = Path.Combine(Path.GetDirectoryName(targetFile), Path.GetFileNameWithoutExtension(targetFile).Replace('>', '_').Replace(':', '_').Replace('?', '_'));
            Entries = new List<string>();

            using (var stream = File.OpenRead(targetFile))
            {
                var reader = ReaderFactory.Open(stream, new ReaderOptions { LookForHeader = true, LeaveStreamOpen = true });
                while (reader.MoveToNextEntry())
                {
                    if (reader.Entry.IsDirectory) continue;

                    if (reader.Entry.Key == "././@LongLink")
                    {
                        string fullPath;
                        using (var streamReader = new StreamReader(reader.OpenEntryStream()))
                        {
                            fullPath = streamReader.ReadToEnd().TrimEnd(char.MinValue);
                        }

                        fullPath = Path.Combine(tmpdir, fullPath);

                        if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                        }

                        reader.MoveToNextEntry();

                        using (var fileStream = File.Create(fullPath))
                        using (var entryStream = reader.OpenEntryStream())
                        {
                            entryStream.StreamCopyTo(fileStream);
                        }
                    }
                    else
                    {
                        try
                        {
                            reader.WriteEntryToDirectory(tmpdir, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                    Entries.Add(reader.Entry.Key);
                }
            }
        }

        public Stream GetEntry(string key)
        {
            var filePath = Path.Combine(tmpdir, key);
            return File.Exists(filePath) ? File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read) : null;
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