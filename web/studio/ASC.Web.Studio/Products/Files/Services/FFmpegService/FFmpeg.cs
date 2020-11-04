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
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ASC.Common.Logging;
using ASC.Core;

namespace ASC.Web.Files.Services.FFmpegService
{
    public class FFmpegService
    {
        public static List<string> MustConvertable
        {
            get
            {
                if (string.IsNullOrEmpty(FFmpegPath)) return new List<string>();
                return ConvertableMedia.ToList();
            }
        }

        public static bool IsConvertable(string extension)
        {
            return MustConvertable.Contains(extension.TrimStart('.'));
        }

        public static Stream Convert(Stream inputStream, string inputFormat)
        {
            if (inputStream == null) throw new ArgumentException();
            if (string.IsNullOrEmpty(inputFormat)) throw new ArgumentException();

            var startInfo = PrepareFFmpeg(inputFormat);

            Process process;
            using (process = new Process { StartInfo = startInfo })
            {
                process.Start();

                var _ = StreamCopyToAsync(inputStream, process.StandardInput.BaseStream, closeDst: true);

                ProcessLog(process.StandardError.BaseStream);

                return process.StandardOutput.BaseStream;
            }
        }

        static FFmpegService()
        {
            logger = LogManager.GetLogger("ASC.Files");

            var exts = ConfigurationManagerExtension.AppSettings["files.ffmpeg.exts"];
            ConvertableMedia = new List<string>();

            if (!string.IsNullOrEmpty(exts))
            {
                try
                {
                    ConvertableMedia = exts.Split('|').ToList();
                }
                catch (Exception e)
                {
                    logger.Error("Couldn't parse 'files.ffmpeg.exts' setting.", e);
                }
            }

            if (string.IsNullOrEmpty(FFmpegPath))
            {
                var pathvar = Environment.GetEnvironmentVariable("PATH");
                var folders = pathvar.Split(WorkContext.IsMono ? ':' : ';').Distinct();
                foreach (var folder in folders)
                {
                    if (!Directory.Exists(folder)) continue;

                    foreach (var name in FFmpegExecutables)
                    {
                        var path = Path.Combine(folder, WorkContext.IsMono ? name : name + ".exe");
                        if (File.Exists(path))
                        {
                            FFmpegPath = path;
                            logger.InfoFormat("FFmpeg found in {0}", path);
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(FFmpegPath)) break;
                }
            }
        }

        private static readonly List<string> ConvertableMedia;
        private static readonly List<string> FFmpegExecutables = new List<string>() { "ffmpeg", "avconv" };
        private static readonly string FFmpegPath = ConfigurationManagerExtension.AppSettings["files.ffmpeg"];
        private static readonly string FFmpegArgs = ConfigurationManagerExtension.AppSettings["files.ffmpeg.args"] ?? "-i - -preset ultrafast -movflags frag_keyframe+empty_moov -f {0} -";

        private static readonly ILog logger;

        private static ProcessStartInfo PrepareFFmpeg(string inputFormat)
        {
            if (!ConvertableMedia.Contains(inputFormat.TrimStart('.'))) throw new ArgumentException();

            var startInfo = new ProcessStartInfo();

            if (string.IsNullOrEmpty(FFmpegPath))
            {
                logger.Error("FFmpeg/avconv was not found in PATH or 'files.ffmpeg' setting");
                throw new Exception("no ffmpeg");
            }

            startInfo.FileName = FFmpegPath;
            startInfo.WorkingDirectory = Path.GetDirectoryName(FFmpegPath);
            startInfo.Arguments = string.Format(FFmpegArgs, "mp4");
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            return startInfo;
        }

        private static async Task<int> StreamCopyToAsync(Stream srcStream, Stream dstStream, bool closeSrc = false, bool closeDst = false)
        {
            const int bufs = 2048 * 4;

            if (srcStream == null) throw new ArgumentNullException("srcStream");
            if (dstStream == null) throw new ArgumentNullException("dstStream");

            var buffer = new byte[bufs];
            int readed;
            var total = 0;
            while ((readed = await srcStream.ReadAsync(buffer, 0, bufs)) > 0)
            {
                await dstStream.WriteAsync(buffer, 0, readed);
                await dstStream.FlushAsync();
                total += readed;
            }

            if (closeSrc)
            {
                srcStream.Dispose();
                srcStream.Close();
            }

            if (closeDst)
            {
                await dstStream.FlushAsync();
                dstStream.Dispose();
                dstStream.Close();
            }

            return total;
        }

        private static async void ProcessLog(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    logger.Info(line);
                }
            }
        }
    }
}