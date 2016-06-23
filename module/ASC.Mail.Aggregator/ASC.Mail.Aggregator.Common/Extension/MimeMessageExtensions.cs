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


using System;
using System.IO;
using System.Linq;
using System.Text;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;
using MimeKit;

namespace ASC.Mail.Aggregator.Common.Extension
{
    public static class MimeMessageExtensions
    {
        public static void FixEncodingIssues(this MimeMessage mimeMessage, ILogger logger = null)
        {
            if (logger == null)
                logger = new NullLogger();

            try
            {
                foreach (var mimeEntity in mimeMessage.BodyParts)
                {
                    var textPart = mimeEntity as TextPart;

                    if (textPart == null ||
                        textPart.ContentObject == null ||
                        textPart.ContentObject.Encoding != ContentEncoding.Default)
                    {
                        continue;
                    }

                    try
                    {
                        string charset;
                        using (var stream = new MemoryStream())
                        {
                            textPart.ContentObject.DecodeTo(stream);
                            var bytes = stream.ToArray();
                            charset = EncodingTools.DetectCharset(bytes);
                        }

                        if (!string.IsNullOrEmpty(charset) &&
                            (textPart.ContentType == null ||
                             string.IsNullOrEmpty(textPart.ContentType.Charset) ||
                             textPart.ContentType.Charset != charset))
                        {
                            var encoding = EncodingTools.GetEncodingByCodepageName(charset);

                            if(encoding == null)
                                continue;

                            var newText = textPart.GetText(charset);

                            textPart.SetText(encoding, newText);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("MimeMessage.FixEncodingIssues->ImproveBodyEncoding: {0}", ex.Message);
                    }
                }

                if (mimeMessage.Headers.Contains(HeaderId.From))
                {
                    var fromParsed = mimeMessage.From.FirstOrDefault();
                    if (fromParsed != null && !string.IsNullOrEmpty(fromParsed.Name))
                    {
                        var fromHeader = mimeMessage.Headers.FirstOrDefault(h => h.Id == HeaderId.From);
                        fromHeader.FixEncodingIssues(logger);
                    }
                }

                if (!mimeMessage.Headers.Contains(HeaderId.Subject))
                    return;

                var subjectHeader = mimeMessage.Headers.FirstOrDefault(h => h.Id == HeaderId.Subject);
                subjectHeader.FixEncodingIssues(logger);

            }
            catch (Exception ex)
            {
                logger.Warn("MimeMessage.FixEncodingIssues: {0}", ex.Message);
            }
        }

        public static void FixEncodingIssues(this Header header, ILogger logger = null)
        {
            if (logger == null)
                logger = new NullLogger();

            try
            {
                var rawValueString = Encoding.UTF8.GetString(header.RawValue).Trim();
                if (rawValueString.IndexOf("?q?", StringComparison.InvariantCultureIgnoreCase) > -1 ||
                    rawValueString.IndexOf("?b?", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    return;
                }

                var charset = EncodingTools.DetectCharset(header.RawValue);

                if(string.IsNullOrEmpty(charset))
                    return;

                var newValue = header.GetValue(charset);

                if (header.Value.Equals(newValue, StringComparison.InvariantCultureIgnoreCase))
                    return;

                var encoding = EncodingTools.GetEncodingByCodepageName(charset);
                header.SetValue(encoding, newValue);
            }
            catch (Exception ex)
            {
                logger.Warn("Header.FixEncodingIssues: {0}", ex.Message);
            }
        }

        public static void FixDateIssues(this MimeMessage mimeMessage, DateTimeOffset? internalDate = null, ILogger logger = null)
        {
            if (logger == null)
                logger = new NullLogger();

            try
            {
                if (!mimeMessage.Headers.Contains(HeaderId.Date) || mimeMessage.Date > DateTimeOffset.UtcNow)
                {
                    mimeMessage.Date = internalDate ?? DateTimeOffset.UtcNow;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("MimeMessage.FixEncodingIssues: {0}", ex.Message);
            }
        }
    }
}