using ASC.Common.Logging;
using ASC.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ASC.Web.Files.Services.FFmpegService
{
    public class FFmpegService
    {
        public static List<string> MustConvertable
        {
            get
            {
                if (string.IsNullOrEmpty(FFmpegPath)) return new List<string>();
                return ConvertableMedia.Keys.ToList();
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

            var exts = WebConfigurationManager.AppSettings["files.ffmpeg.exts"];
            ConvertableMedia = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(exts))
            {
                try
                {
                    ConvertableMedia = exts.Split('|').ToDictionary(f => f.Split('-')[0], f => f.Split('-')[1]);
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

                    if (WorkContext.IsMono)
                    {
                        if (File.Exists(Path.Combine(folder, "ffmpeg")))
                        {
                            FFmpegPath = Path.Combine(folder, "ffmpeg");
                            break;
                        }
                        else if (File.Exists(Path.Combine(folder, "avconv")))
                        {
                            FFmpegPath = Path.Combine(folder, "avconv");
                            break;
                        }
                    }
                    else
                    {
                        if (File.Exists(Path.Combine(folder, "ffmpeg.exe")))
                        {
                            FFmpegPath = Path.Combine(folder, "ffmpeg.exe");
                            break;
                        }
                        else if (File.Exists(Path.Combine(folder, "avconv.exe")))
                        {
                            FFmpegPath = Path.Combine(folder, "avconv.exe");
                            break;
                        }
                    }
                }
            }
        }

        private static readonly Dictionary<string, string> ConvertableMedia;
        private static readonly string FFmpegPath = WebConfigurationManager.AppSettings["files.ffmpeg"];
        private static readonly string FFmpegArgs = WebConfigurationManager.AppSettings["files.ffmpeg.args"] ?? "-f {0} -i - -preset ultrafast -movflags frag_keyframe+empty_moov -f {1} -";

        private static readonly ILog logger;

        private static ProcessStartInfo PrepareFFmpeg(string inputFormat)
        {
            if (!ConvertableMedia.ContainsKey(inputFormat.TrimStart('.'))) throw new ArgumentException();
            var internalFormat = ConvertableMedia[inputFormat.TrimStart('.')];

            var startInfo = new ProcessStartInfo();

            if (string.IsNullOrEmpty(FFmpegPath))
            {
                logger.Error("FFmpeg/avconv was not found in PATH or 'files.ffmpeg' setting");
                throw new Exception("no ffmpeg");
            }

            startInfo.FileName = FFmpegPath;
            startInfo.WorkingDirectory = Path.GetDirectoryName(FFmpegPath);
            startInfo.Arguments = string.Format(FFmpegArgs, internalFormat, "mp4");
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