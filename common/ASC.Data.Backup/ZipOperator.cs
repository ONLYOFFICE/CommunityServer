/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System.IO;
using SharpCompress.Common;
using SharpCompress.Reader;
using SharpCompress.Writer;

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


        public ZipReadOperator(string targetFile)
        {
            tmpdir = Path.Combine(Path.GetDirectoryName(targetFile), Path.GetFileNameWithoutExtension(targetFile));

            using (var stream = File.OpenRead(targetFile))
            {
                var reader = ReaderFactory.Open(stream, Options.LookForHeader | Options.KeepStreamsOpen);
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
                            entryStream.CopyTo(fileStream);
                        }
                    }
                    else
                    {
                        reader.WriteEntryToDirectory(tmpdir, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                    }
                }
            }
        }

        public Stream GetEntry(string key)
        {
            var filePath = Path.Combine(tmpdir, key);
            return File.Exists(filePath) ? File.Open(filePath, FileMode.Open) : null;
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