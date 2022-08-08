using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace IpGeolocationConverter
{
    class Program
    {
        const char Separator = ',';

        static string _sourceFilePath;

        static readonly List<string> ColumnNames = new List<string> {
             "First IP address",
             "Last IP address",
             "Country ISO-3166-alpha2 code",
             "State or Province name",
             "District name",
             "City name",
             "ZIP or Postal code",
             "Latitude",
             "Longitude",
             "Unique geoname ID",
             "Time Zone offset",
             "Time Zone name"
        };

        static readonly List<int> ColumnMap = new List<int>();

        static void Main()
        {
            try
            {
                ReadSourceFilePath();
                ReadColumnMap();
                Convert();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }

            Console.ReadLine();
        }

        private static void ReadSourceFilePath()
        {
            do
            {
                Console.WriteLine("Enter the source .scv file path:");
                _sourceFilePath = Console.ReadLine();
            }
            while (string.IsNullOrEmpty(_sourceFilePath));

            if (!File.Exists(_sourceFilePath))
            {
                throw new ArgumentException("file not exist");
            }

            if (Path.GetExtension(_sourceFilePath).ToLowerInvariant() != ".csv")
            {
                throw new ArgumentException("wrong file extension");
            }
        }

        private static void ReadColumnMap()
        {
            Console.WriteLine();
            Console.WriteLine("Enter the column indexes (starting from 0) corresponding to the following data, or leave the field blank if there is no data");
            Console.WriteLine();

            foreach (var columnName in ColumnNames)
            {
                Console.Write("{0}: ", columnName);

                var value = Console.ReadLine();

                int index;
                if (int.TryParse(value, out index))
                {
                    ColumnMap.Add(index);
                }
                else
                {
                    ColumnMap.Add(-1);
                }
            }
        }

        private static void Convert()
        {
            Console.WriteLine();
            Console.WriteLine("The conversion process has been started. Please wait...");

            var left = Console.CursorLeft;
            var top = Console.CursorTop;

            var outputFilePath = Path.Combine(Path.GetDirectoryName(_sourceFilePath), "converted" + Path.GetFileName(_sourceFilePath));

            using (var streamReader = File.OpenText(_sourceFilePath))
            using (var streamWriter = new StreamWriter(outputFilePath))
            {
                uint processedCount = 0;
                uint ignoredCount = 0;

                while (streamReader.Peek() >= 0)
                {
                    processedCount++;
                    var sourceLine = streamReader.ReadLine();
                    string fixedLine;
                    if (!FixLine(sourceLine, out fixedLine))
                    {
                        ignoredCount++;
                        continue;
                    }

                    streamWriter.WriteLine(fixedLine);

                    if (processedCount % 1000000 == 0)
                    {
                        Console.SetCursorPosition(left, top);
                        Console.WriteLine("Info: processed {0} lines, ignored {1} lines", processedCount, ignoredCount);
                    }
                }

                Console.SetCursorPosition(left, top);
                Console.WriteLine("Info: processed {0} lines, ignored {1} lines", processedCount, ignoredCount);

                Console.WriteLine("The conversion process has been completed.");
            }

            Console.WriteLine();
            Console.WriteLine("Output file path: " + outputFilePath);
        }

        private static bool FixLine(string line, out string result)
        {
            result = null;

            var cells = line.Split(Separator);
            var newCells = new List<string>();
            var numericСolumns = new List<string>() { "Latitude", "Longitude", "Unique geoname ID", "Time Zone offset" };

            for (var i = 0; i < ColumnNames.Count; i++)
            {
                var columnName = ColumnNames[i];
                var index = ColumnMap[i];
                var value = index > -1 && index < cells.Length ? cells[index] : string.Empty;

                if (columnName == "First IP address")
                {
                    newCells.Add(value.Contains(".") ? "ipv4" : "ipv6");
                    value = FormatIp(value);
                }

                if (columnName == "Last IP address")
                {
                    value = FormatIp(value);
                }

                if (columnName == "Country ISO-3166-alpha2 code" && value == "ZZ")
                {
                    return false;
                }

                if (string.IsNullOrEmpty(value) && numericСolumns.Contains(columnName))
                {
                    value = "0";
                }

                newCells.Add(value);
            }

            result = string.Join(Separator.ToString(CultureInfo.InvariantCulture), newCells);

            return true;
        }

        private static string FormatIp(string ip)
        {
            ip = (ip ?? "").Trim();

            if (ip.Contains('.'))
            {
                //ip v4
                if (ip.Length == 15)
                {
                    return ip;
                }

                return string.Join(".", ip.Split(':')[0].Split('.').Select(s => ("00" + s).Substring(s.Length - 1)).ToArray());
            }

            if (ip.Contains(':'))
            {
                //ip v6
                if (ip.Length == 39)
                {
                    return ip;
                }

                var index = ip.IndexOf("::", StringComparison.Ordinal);
                if (0 <= index)
                {
                    ip = ip.Insert(index + 2, new string(':', 8 - ip.Split(':').Length));
                }

                return string.Join(":", ip.Split(':').Select(s => ("0000" + s).Substring(s.Length)).ToArray());
            }

            throw new ArgumentException("Unknown ip " + ip);
        }
    }
}
