/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ASC.Mail.Aggregator.Common.Imap;
using Newtonsoft.Json;

namespace ASC.Mail.Aggregator.Common.Utils
{
    public static class MailUtil
    {
        private static readonly Regex RegexSubjectPrefix = new Regex("([\\[\\(]\\s*)?((?<=(\\A)|\\s|\\^|\\:|\\(|\\[)(RE?S?|FYI|RIF|I|FS|VB|RV|ENC|ODP|PD|YNT|ILT|SV|VS|VL|BLS|TRS?|AW|WG|ΑΠ|ΣΧΕΤ|ΠΡΘ|ANTW|DOORST|НА|תגובה|הועבר|主题|转发|FWD?))(\\[\\d+\\])* *([-:;)\\]][ :;\\])-]*|$)|\\]+ *$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegexSubjectAbbreviationsSplit = new Regex("\\[.*\\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegexSubjectAbbreviations = new Regex("([A-Z]+[\\s\\/]*[A-Z]+)+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<int> GetLabelsFromString(string stringLabel)
        {
            var list = new List<int>();
            if (!string.IsNullOrEmpty(stringLabel))
            {
                var labels = stringLabel.Split(',');
                foreach (var label in labels)
                {
                    int labelIn;
                    if (int.TryParse(label, out labelIn))
                    {
                        list.Add(labelIn);
                    }
                }
            }
            return list;
        }

        public static string GetStringFromLabels(List<int> labels)
        {
            if (labels != null)
            {
                return string.Join(",", labels.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
            }
            return string.Empty;
        }

        public static string GetJsonString(object data)
        {
            string jsonData = null;
            if (data != null)
            {
                using (var stream = new MemoryStream())
                {
                    var serializer = new DataContractJsonSerializer(data.GetType());
                    serializer.WriteObject(stream, data);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        jsonData = reader.ReadToEnd();
                    }
                }
            }
            return jsonData;
        }

        public static double BytesToMegabytes(long bytes)
        {
            return Math.Round(bytes / 1024d / 1024d, 1);
        }

        public static Dictionary<string, ImapFolderUids> ParseImapIntervals(string json)
        {
            var text = json;

            if (json.StartsWith("["))
            {
                text =
                    text.Replace("\"Key\":", "")
                        .Replace(",\"Value\"", "")
                        .Replace("\"__type\":\"ImapFolderUids:#ASC.Mail.Aggregator.Common.Imap\",", "")
                        .Replace("},{", ",");
                text = text
                    .Substring(1, text.Length - 2);
            }

            return (Dictionary<string, ImapFolderUids>)JsonConvert.DeserializeObject(text, typeof(Dictionary<string, ImapFolderUids>));
        }

        public static string NormalizeSubject(string subject)
        {
            subject = RegexSubjectPrefix.Replace(subject, "");

            var matches = RegexSubjectAbbreviationsSplit.Matches(subject);

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i].Value.Trim('[', ']');

                if (RegexSubjectAbbreviations.IsMatch(match))
                {
                    subject = subject.Replace(matches[i].Value, "");
                }
            }

            return subject.Trim();
        }

        /// <summary>
        /// Removes control characters and other non-UTF-8 characters
        /// </summary>
        /// <param name="inString">The string to process</param>
        /// <returns>A string with no control characters or entities above 0x00FD</returns
        public static string NormalizeStringForMySql(string inString)
        {
            if (string.IsNullOrEmpty(inString))
                return inString;

            var newString = new StringBuilder(inString.Length);

            foreach (var ch in inString.Where(XmlConvert.IsXmlChar))
                newString.Append(ch);

            return newString.ToString();
        }
    }
}