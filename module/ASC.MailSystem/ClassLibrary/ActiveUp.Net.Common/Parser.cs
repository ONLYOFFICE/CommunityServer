// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using ActiveUp.Net.Security;
using HtmlAgilityPack;
#if !PocketPC
using System.Security.Cryptography.Pkcs;
#endif

// ReSharper disable CheckNamespace
namespace ActiveUp.Net.Mail
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Base class for all Parser objects.
    /// </summary>
#if !PocketPC
    [Serializable]
#endif
    public class Parser
    {
        private static readonly Regex _regxTraceFrom = new Regex(@"from.+?(?=(from|by|via|with|for|id|;|\r?\n))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxTraceBy = new Regex(@"(?<=by ).+?(?= ?(from|by|via|with|for|id|;|\r?\n))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxTraceVia = new Regex(@"(?<=via ).+?(?= ?(from|by|via|with|for|id|;|\r?\n))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxTraceWith = new Regex(@"(?<=with ).+?(?= ?(from|by|via|with|for|id|;|\r?\n))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxTraceFor = new Regex(@"(?<=for ).+?(?= ?(from|by|via|with|for|id|;|\r?\n))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxTraceId = new Regex(@"(?<=id ).+?(?= ?(from|by|via|with|for|id|;|\r?\n))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        
        private static readonly Regex _regxClean = new Regex(@"(\(((\\\))|[^)])*\))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxPlus = new Regex(@" +", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxPlusEx = new Regex(@"( +: +)|(: +)|( +:)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex _regxCharset = new Regex(@"(?<=charset=)(([^;,\r\n]))*", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex _regxMimeType = new Regex(@"(?<=: ?)\S+?(?=([;\s]|\Z))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxMimeTypeParams = new Regex(@"(?<=;\s+)[^;\s]*=[^;]*(?=(;|\Z))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxMimeDisposition = new Regex(@"(?<=: ?)\S+?(?=([;\s]|\Z))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxMimeDispositionParams = new Regex(@"(?<=;[ \t]?)[^;]*=[^;]*(?=(;|\Z))", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex _regxWhiteSpaces = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxUnfold = new Regex(@"\r?\n(?=[ \t])", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex _regxHeaderEnd = new Regex(@".(?=\r?\n\r?\n)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxBodyStart = new Regex(@"(?<=\r?\n\r?\n).", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxHeaderFieldsParams = new Regex(@"(?<=((\r?\n)|\n)|\A)\S+:(.|(\r?\n[\t ]))+(?=((\r?\n)\S)|\Z)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxHeader = new Regex(@"[\s\S]+?((?=\r?\n\r?\n)|\Z)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxHeaderLines = new Regex(@"(?<=((\r?\n)|\n)|\A)\S+:(.|(\r?\n[\t ]))+(?=((\r?\n)\S)|\Z)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxEmail = new Regex("<(.|[.])*?>", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        //private static Regex regx_base64 = new Regex("([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regxNewlines = new Regex(@"(?:\r\n|[\r\n])", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        #region Methods

        #region Private and internal methods

        /// <summary>
        /// Gets the month.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <returns></returns>
        internal static int GetMonth(string month)
        {
            switch (month)
            {
                case "Jan": return 1;
                case "Feb": return 2;
                case "Mar": return 3;
                case "Apr": return 4;
                case "May": return 5;
                case "Jun": return 6;
                case "Jul": return 7;
                case "Aug": return 8;
                case "Sep": return 9;
                case "Oct": return 10;
                case "Nov": return 11;
                case "Dec": return 12;
                default: return -1;
            }
        }

        /// <summary>
        /// Invs the get month.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <returns></returns>
        internal static string InvGetMonth(int month)
        {
            switch (month)
            {
                case 1: return "Jan";
                case 2: return "Feb";
                case 3: return "Mar";
                case 4: return "Apr";
                case 5: return "May";
                case 6: return "Jun";
                case 7: return "Jul";
                case 8: return "Aug";
                case 9: return "Sep";
                case 10: return "Oct";
                case 11: return "Nov";
                case 12: return "Dec";
                default: return "???";
            }
        }

        /// <summary>
        /// Gets the charset of the content.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static string GetCharset(string input)
        {
            input = input.Replace(" ", "");
            var charset = _regxCharset.Match(input);
            if (charset.Success)
            {
                var char_set = charset.Value.Replace("\"", "").Trim();

                try
                {
// ReSharper disable UnusedVariable
                    var enc = Encoding.GetEncoding(char_set);
// ReSharper restore UnusedVariable
                    return char_set;
                }
                catch {
                    var enc = EncodingTools.GetEncodingByCodepageName(char_set.ToLower());

                    if (enc != null)
                        return enc.HeaderName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static ContentType GetContentType(string input)
        {
            var field = new ContentType {MimeType = _regxMimeType.Match(input).Value};
            var parammatch = _regxMimeTypeParams.Match(input);

            while (parammatch.Success)
            {
                var name = FormatFieldName(parammatch.Value.Substring(0, parammatch.Value.IndexOf('='))).ToLower();
                var value = parammatch.Value.Substring(parammatch.Value.IndexOf('=') + 1).Trim('\r', '\n').Trim();

                if (!string.IsNullOrEmpty(value))
                {
                    if (value[0] == '\"')
                    {
                        var i = value.LastIndexOf('\"');

                        if (i > 1)
                            value = value.Substring(1, i - 1);
                    }

                    value = value.Replace("\"", "");
                }

                field.Parameters.Add(name, value);

                parammatch = parammatch.NextMatch();
            }
            return field;
        }

        /// <summary>
        /// Gets the content disposition.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static ContentDisposition GetContentDisposition(string input)
        {
            var field = new ContentDisposition();
            
            input = input.Replace("\t", "");

            field.Disposition = _regxMimeDisposition.Match(input).Value;

            var parammatch = _regxMimeDispositionParams.Match(input);

            for (; parammatch.Success; parammatch = parammatch.NextMatch())
            {
                var name = parammatch.Value.Substring(0, parammatch.Value.IndexOf('='));
                var value = parammatch.Value.Substring(parammatch.Value.IndexOf('=') + 1)
                    .Replace("\"", "")
                    .Trim('\r', '\n');

                if (name.EndsWith("*")) // RFC 5987 Extended notation
                {
                    value = Codec.RFC5987Decode(value);
                    name = name.Remove(name.Length - 1);
                    value = Codec.RFC2047Encode(value);
                }

                field.Parameters.Add(FormatFieldName(name), value);
            }

            return field;
        }

        /// <summary>
        /// Parses the sub parts.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="original_content"></param>
        /// <param name="message">The message.</param>
        private static void ParseSubParts(ref MimePart part, ref string original_content, int start, int length)
        {
            var boundary = part.ContentType.Parameters["boundary"];
            Logger.AddEntry(string.Format("boundary : {0}", boundary));
            
            var regex = new Regex(string.Format("\r?\n?--{0}", Regex.Escape(boundary)));

            var partIndexes = new Dictionary<int, int>();

            var m = regex.Match(original_content, start, length);

            while (m.Success)
            {
                partIndexes.Add(m.Index, m.Length);
                m = m.NextMatch();
            }

            var list = partIndexes.ToList();

            for (var i = 0; i < list.Count; i++)
            {
                var partStart = list[i].Key + list[i].Value;

                int partLength;

                if (i + 1 != list.Count)
                {
                    partLength = list[i + 1].Key - partStart;
                }
                else
                {
                    partLength = start + length - partStart;
                }

                if (partLength == 0 || original_content.Substring(partStart, 2) == "--")
                    continue;

                var newpart = ParseMimePart(ref original_content, partStart, partLength);
                newpart.Container = part;
                part.SubParts.Add(newpart);
            }
        }

        /// <summary>
        /// Dispatches the parts.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="message">The message.</param>
        private static void DispatchParts(MimePart root, ref Message message)
        {
            foreach (MimePart entity in root.SubParts)
            {
                DispatchPart(entity, ref message);
            }
        }

        /// <summary>
        /// Dispatches the parts.
        /// </summary>
        /// <param name="message">The message.</param>
        internal static void DispatchParts(ref Message message)
        {
            DispatchPart(message.PartTreeRoot, ref message);
        }

        private static ContentType _parentContentType;

        private static void DispatchPart(MimePart part, ref Message message)
        {
            if (part.SubParts.Count > 0) // This is a container part.
            {
                _parentContentType = part.ContentType; // saveing right content-type of message
                DispatchParts(part, ref message);
            }
            else // This is a leaf part.
            {
                // We will consider the highest-level text parts that are not attachments to be the intended for display.
                // We know the highest-level parts will be set, because the parser first goes to the deepest level and returns top-level parts last.
                if (part.ContentType.Type.ToLower().Equals("text") && !part.ContentDisposition.Disposition.ToLower().Equals("attachment"))
                {
                    if (part.ContentType.SubType.ToLower().Equals("plain"))
                    {
                        message.BodyText.Charset = part.Charset;
                        message.BodyText.Text = part.TextContent;
                    }
                    else if (part.ContentType.SubType.ToLower().Equals("html"))
                    {
                        message.IsHtml = true;
                        message.BodyHtml.Charset = part.Charset;
                        message.BodyHtml.Text = part.TextContent;
                    }
                    else if (part.ContentType.SubType.ToLower().Equals("calendar"))
                    {
                        message.BodyCalendar.Charset = part.Charset;
                        message.BodyCalendar.Text = part.TextContent;
                    }

                    if (_parentContentType != null)
                        message.ContentType = _parentContentType;
                }
                // If this part has to be displayed has an attachment, add it to the appropriate collection.
                else if (part.ContentDisposition.Disposition.ToLower().Equals("attachment"))
                {
                    message.Attachments.Add(part);
                }
                // If this part has to be displayed at the same time as the main body, add it to the appropriate collection.
                else if (part.ContentDisposition.Disposition.ToLower().Equals("inline"))
                {
                    message.EmbeddedObjects.Add(part);
                }
                // If we have image isn't marked as "Content-Disposition: inline", we will add it to EmbededObjects
                else if (part.ContentType.Type.ToLower().Equals("image"))
                {
                    message.EmbeddedObjects.Add(part);
                }

                // Parse message/rfc822 parts as Message objects and place them in the appropriate collection.
                else if (part.ContentType.MimeType.ToLower().Equals("message/rfc822"))
                {
                    var msg = part.TextContent;
                    var embeddedMessage = ParseMessage(ref msg);
                    message.SubMessages.Add(embeddedMessage);
                    var filename = string.IsNullOrEmpty(part.Filename)
                        ? embeddedMessage.Subject + ".eml"
                        : part.Filename;
                    message.AddAttachmentFromString(filename, msg);
                }

                else if (part.ContentType.MimeType.ToLower().Equals("application/pkcs7-signature")
                    || part.ContentType.MimeType.ToLower().Equals("application/x-pkcs7-signature"))
                {
                    var to_digest = part.Container.TextContent;
                    to_digest = Regex.Split(to_digest, string.Format("\r\n--{0}", part.Container.ContentType.Parameters["boundary"]))[1];
                    to_digest = to_digest.TrimStart('\r', '\n');
                    //Match endDelimiter = Regex.Match(toDigest, "(?<=[^\r\n]\r\n)\r\n", RegexOptions.RightToLeft);
                    //int lastNonNewLine = Regex.Match(toDigest, "[^\r\n]", RegexOptions.RightToLeft).Index;
                    //if (endDelimiter.Index != -1 && endDelimiter.Index > lastNonNewLine) toDigest = toDigest.Remove(endDelimiter.Index);

                    //TODO: What should be done in PPC ?
#if !PocketPC
                    message.Signatures.Smime = new SignedCms(new ContentInfo(Encoding.ASCII.GetBytes(to_digest)), true);
                    message.Signatures.Smime.Decode(part.BinaryContent);
#endif
                }
                else if (message.IsMultipartReport && part.ContentType.MimeType.ToLower().Equals("message/delivery-status"))
                {
                    message.BodyText.Text += String.Format("\r\nDelivery status:\r\n{0}", part.TextContent);
                }
                else
                {
                    message.UnknownDispositionMimeParts.Add(part);
                }

                // Anyway, this is a leaf part of the message.
                message.LeafMimeParts.Add(part);
            }
        }

        private static bool IsAttachment(ref MimePart part)
        {
            var isAttach = false;

            if (part.ContentDisposition.Disposition.ToLower().Equals("attachment"))
            {
                isAttach = true;
            }
            // If this part has to be displayed at the same time as the main body, add it to the appropriate collection.
            else if (!part.ContentType.Type.ToLower().Equals("text") && part.ContentDisposition.Disposition.ToLower().Equals("inline"))
            {
                isAttach = true;
            }
            // If we have image isn't marked as "Content-Disposition: inline", we will add it to EmbededObjects
            else if (part.ContentType.Type.ToLower().Equals("image"))
            {
                isAttach = true;
            }

            return isAttach;
        }

        private static Encoding DetectPartEncodingAndCharset(ref MimePart part, byte[] bytes)
        {
            Encoding encoding;
            var charset = (!string.IsNullOrEmpty(part.Charset) ? part.Charset : "iso-8859-1");

            var detectedCharset = Codec.DetectCharset(bytes) ?? charset;

            if (string.IsNullOrEmpty(part.Charset) || part.Charset != detectedCharset)
            {
                try
                {
                    encoding = Encoding.GetEncoding(detectedCharset);
                    part.Charset = detectedCharset;
                }
                catch (NotSupportedException)
                {
                    encoding = Codec.GetEncoding(charset);
                    part.Charset = encoding.EncodingName;
                }
            }
            else
            {
                encoding = Codec.GetEncoding(charset);
            }

            return encoding;
        }

        /// <summary>
        /// Decodes the part body.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        private static void DecodePartBody(ref MimePart part, ref string data, int start, int length)
        {
            if (part.ContentType.Type.ToLower().Equals("multipart") && part.Charset == null)
                return;

            // Let's see if a charset is specified. Otherwise we default to "iso-8859-1".
            var charset = (!string.IsNullOrEmpty(part.Charset) ? part.Charset : "iso-8859-1");

            var skipDecode = false;

            byte[] bytes;

            var rawText = data.Substring(start, length);
            Encoding encoding;

            switch (part.ContentTransferEncoding)
            {
                case ContentTransferEncoding.Base64:
                    try
                    {
                        bytes = Converter.FromBase64String(data, start, length);
                    }
                    catch (FormatException)
                    {
                        bytes = Encoding.GetEncoding(charset).GetBytes(rawText);
                        skipDecode = true;
                    }

                    if (IsAttachment(ref part))
                        part.BinaryContent = bytes;
                    else
                    {
                        encoding = DetectPartEncodingAndCharset(ref part, bytes);

                        part.TextContent = !skipDecode
                            ? encoding.GetString(bytes, 0, bytes.Length)
                            : rawText;
                    }
                    break;

                case ContentTransferEncoding.QuotedPrintable:
                    try
                    {
                        bytes = Converter.FromQuotedPrintableString(rawText);
                    }
                    catch (Exception)
                    {
                        bytes = Encoding.GetEncoding(charset).GetBytes(rawText);
                        skipDecode = true;
                    }

                    if (IsAttachment(ref part))
                    {
                        part.BinaryContent = bytes;
                    }
                    else
                    {
                        encoding = DetectPartEncodingAndCharset(ref part, bytes);

                        part.TextContent = !skipDecode
                            ? encoding.GetString(bytes, 0, bytes.Length).TrimEnd('=')
                            : rawText;
                    }

                    break;
                default:
                    bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(rawText);

                    if (IsAttachment(ref part))
                        part.BinaryContent = bytes;
                    else
                    {
                        encoding = DetectPartEncodingAndCharset(ref part, bytes);
                        part.TextContent = encoding.GetString(bytes);
                    }
                    break;
            }

        }

        /// <summary>
        /// Replaces the time zone.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static string ReplaceTimeZone(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            input = input.Replace("EDT", "-0400");
            input = input.Replace("EST", "-0500");
            input = input.Replace("CDT", "-0500");
            input = input.Replace("CST", "-0600");
            input = input.Replace("MDT", "-0600");
            input = input.Replace("MST", "-0700");
            input = input.Replace("PDT", "-0700");
            input = input.Replace("PST", "-0800");
            input = input.Replace("UT", "+0000");
            input = input.Replace("GMT", "+0000");
            return input;
        }

        /// <summary>
        /// Removes the white spaces.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        internal static string RemoveWhiteSpaces(string input)
        {
            return _regxWhiteSpaces.Replace(input, "");
        }

        /// <summary>
        /// Removes new lines.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        internal static string RemoveNewLines(string input)
        {
            return _regxNewlines.Replace(input, "");
        }
        /// <summary>
        /// Formats the name of the field.
        /// </summary>
        /// <param name="field_name">Name of the field.</param>
        /// <returns></returns>
        internal static string FormatFieldName(string field_name)
        {
            return field_name.ToLower().Trim();
        }

        /// <summary>
        /// Cleans the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        internal static string Clean(string input)
        {
            return _regxClean.Replace(input, "").Trim(' '); 
        }

        #endregion

        #region Public methods

        #region Header folding

        /// <summary>
        /// Folds the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Fold(string input)
        {
            var sb = new StringBuilder();
            var separated = input.Split(' ');
            var templine = new StringBuilder();
            foreach (var t in separated)
            {
                if (templine.Length + t.Length < 77) 
                    templine.AppendFormat("{0} ", t);
                else
                {
                    sb.Append(templine).Append("\r\n ");
                    templine.Clear();
                }
            }
            sb.Append(templine);
            return sb.ToString();
        }

        /// <summary>
        /// Unfolds the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Unfold(string input)
        {
            return _regxUnfold.Replace(input, "");
        }

        #endregion
        
        #region Mime part parsing

        /// <summary>
        /// Parses the MIME part.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="length"></param>
        /// <param name="message">The message.</param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static MimePart ParseMimePart(ref string data, int start, int length)
        {
            var part = new MimePart();
            //part.ParentMessage = message;
            //part.OriginalContent = data;

            try
            {
                // Separate header and body.
                var mH = _regxHeaderEnd.Match(data, start, length);

                if (!mH.Success)
                    throw new ParsingException("MimePart header not found.");

                var headerEnd = mH.Index + 1;

                var mB = _regxBodyStart.Match(data, start, length);

                if (start + length >= headerEnd)
                {
                    var header = data.Substring(start, headerEnd - start);

                    header = Unfold(header);

                    // Parse header fields and their parameters.
                    var m = _regxHeaderFieldsParams.Match(header);
                    while (m.Success)
                    {
                        if (m.Value.ToLower().StartsWith("content-type:"))
                        {
                            part.ContentType = GetContentType(m.Value);

                            if (m.Value.ToLower().IndexOf("charset", StringComparison.Ordinal) != -1)
                                part.Charset = GetCharset(m.Value);
                        }
                        else if (m.Value.ToLower().StartsWith("content-disposition:"))
                        {
                            part.ContentDisposition = GetContentDisposition(m.Value);
                        }

                        var commaIndex = m.Value.IndexOf(':');
                        var name = FormatFieldName(m.Value.Substring(0, commaIndex));
                        var value =
                            Codec.RFC2047Decode(m.Value.Substring(commaIndex + 1).Trim(' ', '\r', '\n')).Trim('\n');

                        part.HeaderFields.Add(name, value);
                        part.HeaderFieldNames.Add(name, value);

                        m = m.NextMatch();
                    }

                    var isMultipart = part.ContentType.Type.ToLower().Equals("multipart");

                    if (!mB.Success)
                        return part;

                    var bodyStart = mB.Index;

                    // This is a container part.
                    if (isMultipart)
                    {
                        ParseSubParts(ref part, ref data, bodyStart, start + length - bodyStart);
                    }
                    // This is a nested message.
                    else if (part.ContentType.Type.ToLower().Equals("message"))
                    {
                        // TODO
                    }

                    if (!isMultipart)
                        DecodePartBody(ref part, ref data, bodyStart, start + length - bodyStart);

                }

            }
            catch (ParsingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ParsingException(ex.Message);
            }

            return part;
        }

        #endregion

        #region Header parsing

        /// <summary>
        /// Parses a Header from a file to a Header object.
        /// </summary>
        /// <param name="file_path">The path of the file to be parsed.</param>
        /// <returns>The parsed file as a Header object.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Header Header = Parser.ParseHeader("C:\\My headers\\header.txt");
        /// //Expose the subject
        /// string subject = header.Subject;
        /// 
        /// VB.NET
        /// 
        /// Dim Header As Header = Parser.ParseHeader("C:\My headers\header.txt")
        /// 'Expose the subject
        /// Dim subject As String = header.Subject
        /// 
        /// JScript.NET
        /// 
        /// var header:Header = Parser.ParseHeader("C:\\My headers\\header.txt");
        /// //Expose the subject
        /// var subject:string = header.Subject;
        /// </code>
        /// </example> 
        public static Header ParseHeader(string file_path)
        {
            var fs = File.OpenRead(file_path);
            var data = new byte[fs.Length];
            fs.Read(data, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            var hdr = new Header {OriginalData = data};
            ParseHeader(ref hdr);
            return hdr;
        }
        
        /// <summary>
        /// Parses a MemoryStream's content to a Header object.
        /// </summary>
        /// <param name="input_stream">The MemoryStream containing the Header data to be parsed.</param>
        /// <returns>The parsed Header as a Header object.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Header Header = Parser.ParseHeader(someStream);
        /// //Expose the subject
        /// string subject = header.Subject;
        /// 
        /// VB.NET
        /// 
        /// Dim Header As Header = Parser.ParseHeader(someStream)
        /// 'Expose the subject
        /// Dim subject As String = header.Subject
        /// 
        /// JScript.NET
        /// 
        /// var header:Header = Parser.ParseHeader(someStream);
        /// //Expose the subject
        /// var subject:string = header.Subject;
        /// </code>
        /// </example> 
        public static Header ParseHeader(MemoryStream input_stream)
        {
            var buf = new byte[input_stream.Length];
            input_stream.Read(buf, 0, buf.Length);
            var hdr = new Header {OriginalData = buf};
            ParseHeader(ref hdr);
            return hdr;
        }

        /// <summary>
        /// Parses a Header object from a byte array.
        /// </summary>
        /// <returns>The parsed Header as a Header object.</returns>
        /// <example>
        ///     <code lang="CS">
        /// Header Header = Parser.ParseHeader(someBuffer);
        /// //Expose the subject
        /// string subject = header.Subject;
        ///     </code>
        ///     <code lang="VB">
        /// Dim Header As Header = Parser.ParseHeader(someBuffer)
        /// 'Expose the subject
        /// Dim subject As String = header.Subject
        ///     </code>
        ///     <code lang="J#">
        /// var header:Header = Parser.ParseHeader(someBuffer);
        /// //Expose the subject
        /// var subject:string = header.Subject;
        ///     </code>
        /// </example>
        /// <param name="data">The byte array containing the Header data to be parsed.</param>
        public static Header ParseHeader(byte[] data)
        {
            var hdr = new Header {OriginalData = data};
            ParseHeader(ref hdr);
            return hdr;
        }

        /// <summary>
        /// Delegate for header field parsing event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="header">The header object.</param>
        public delegate void OnHeaderFieldParsingEvent(Object sender, Header header);

        /// <summary>
        /// Event handler for header field parsing.
        /// </summary>
        public static event OnHeaderFieldParsingEvent HeaderFieldParsing;

        /// <summary>
        /// Parses the header.
        /// </summary>
        /// <param name="header">The header.</param>
        public static void ParseHeader(ref Header header)
        {
#if !PocketPC
            var hdr = Encoding.GetEncoding("iso-8859-1").GetString(header.OriginalData,0,header.OriginalData.Length);
#else
            string hdr = Pop3Client.PPCEncode.GetString(header.OriginalData, 0, header.OriginalData.Length);
#endif
            hdr = _regxHeader.Match(hdr).Value; 
            hdr = Unfold(hdr);

            var m = _regxHeaderLines.Match(hdr);
            while (m.Success)
            {
                var name = FormatFieldName(m.Value.Substring(0, m.Value.IndexOf(':')));
                var value = Codec.RFC2047Decode(m.Value.Substring(m.Value.IndexOf(":", StringComparison.Ordinal) + 1)).Trim('\r', '\n').TrimStart(' ');
                if (name.Equals("received")) header.Trace.Add(ParseTrace(m.Value.Trim(' ')));
                else if (name.Equals("to")) header.To = ParseAddresses(value);
                else if (name.Equals("cc")) header.Cc = ParseAddresses(value);
                else if (name.Equals("bcc")) header.Bcc = ParseAddresses(value);
                else if (name.Equals("reply-to")) header.ReplyTo = ParseAddress(value);
                else if (name.Equals("from")) header.From = ParseAddress(value);
                else if (name.Equals("sender")) header.Sender = ParseAddress(value);
                else if (name.Equals("content-type")) header.ContentType = GetContentType(m.Value);
                else if (name.Equals("content-disposition")) header.ContentDisposition = GetContentDisposition(m.Value);
                //else
                //{
                    header.HeaderFields.Add(name, value);
                    header.HeaderFieldNames.Add(name, m.Value.Substring(0, m.Value.IndexOf(':')));
                //}
                m = m.NextMatch();

                if (HeaderFieldParsing != null)
                    HeaderFieldParsing(null, header);
            }
        }

        private static readonly Regex RegxHeader = new Regex(@"[\s\S]+?((?=\r?\n\r?\n)|\Z)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RegxUnfold = new Regex(@"\r?\n(?=[ \t])", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RegxHeaderLines = new Regex(@"(?<=((\r?\n)|\n)|\A)\S+:(.|(\r?\n[\t ]))+(?=((\r?\n)\S)|\Z)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool TryParseDefectiveHeader(string origanal_header_data, out Header header)
        {
            if (string.IsNullOrEmpty(origanal_header_data))
            {
                header = null;
                return false;
            }

            var header_bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(origanal_header_data);

            header = new Header { OriginalData = header_bytes };

            var hdr = origanal_header_data;

            hdr = RegxHeader.Match(hdr).Value;
            hdr = RegxUnfold.Replace(hdr, "");

            var m = RegxHeaderLines.Match(hdr);
            while (m.Success)
            {
                try
                {
                    var name = FormatFieldName(m.Value.Substring(0, m.Value.IndexOf(':')));
                    var value =
                        Codec.RFC2047Decode(m.Value.Substring(m.Value.IndexOf(":", StringComparison.Ordinal) + 1))
                             .Trim('\r', '\n')
                             .TrimStart(' ');

                    if (name.Equals("received"))
                    {
                        TraceInfo trace;
                        if (TryParseTrace(m.Value.Trim(' '), out trace))
                        {
                            header.Trace.Add(trace);
                        }
                    }
                    else if (name.Equals("to"))
                    {
                        var address_collection = new AddressCollection();
                        TryParseAddresses(value, out address_collection);
                        header.To = address_collection;
                    }
                    else if (name.Equals("cc"))
                    {
                        AddressCollection address_collection;
                        if (TryParseAddresses(value, out address_collection))
                        {
                            header.Cc = address_collection;
                        }
                    }
                    else if (name.Equals("bcc"))
                    {
                        AddressCollection address_collection;
                        if (TryParseAddresses(value, out address_collection))
                        {
                            header.Bcc = address_collection;
                        }
                    }
                    else if (name.Equals("reply-to"))
                    {
                        Address address;
                        if (TryParseAddress(value, out address))
                        {
                            header.ReplyTo = address;
                        }
                    }
                    else if (name.Equals("from"))
                    {
                        Address address;
                        if (TryParseAddress(value, out address))
                        {
                            header.From = address;
                        }
                    }
                    else if (name.Equals("sender"))
                    {
                        Address address;
                        if (TryParseAddress(value, out address))
                        {
                            header.Sender = address;
                        }
                    }
                    else if (name.Equals("content-type"))
                    {
                        header.ContentType = GetContentType(m.Value);
                    }
                    else if (name.Equals("content-disposition"))
                    {
                        header.ContentDisposition = GetContentDisposition(m.Value);
                    }

                    header.HeaderFields.Add(name, value);
                    header.HeaderFieldNames.Add(name, m.Value.Substring(0, m.Value.IndexOf(':')));
                }
                catch { }

                m = m.NextMatch();
            }

            return true;
        }

        /// <summary>
        /// Parses the header.
        /// </summary>
        /// <param name="origanal_data">response header string</param>
        /// <param name="header_fields">NameValueCollection of fields</param>
        public static void ParseHeader(string origanal_data, ref NameValueCollection header_fields)
        {
            var hdr = _regxHeader.Match(origanal_data).Value;

            var m = _regxHeaderLines.Match(hdr);

            while (m.Success)
            {
                var index_colon = m.Value.IndexOf(':');
                
                var name = FormatFieldName(m.Value
                    .Substring(0, index_colon));
                
                var value = Codec.RFC2047Decode(m.Value
                    .Substring(index_colon + 1))
                    .Trim('\r', '\n')
                    .TrimStart(' ');

                header_fields.Add(name, value);

                m = m.NextMatch();
            }
        }

        /// <summary>
        /// Parses a Header from a string formatted accordingly to the RFC822.
        /// </summary>
        /// <param name="data">The string containing the Header data to be parsed.</param>
        /// <returns>The parsed message as a Header object.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Header Header = Parser.ParseHeaderString(rfc822string);
        /// //Expose the subject
        /// string subject = header.Subject;
        /// 
        /// VB.NET
        /// 
        /// Dim Header As Header = Parser.ParseHeaderString(rfc822string)
        /// 'Expose the subject
        /// Dim subject As String = header.Subject
        /// 
        /// JScript.NET
        /// 
        /// var header:Header = Parser.ParseHeaderString(rfc822string);
        /// //Expose the subject
        /// var subject:string = header.Subject;
        /// </code>
        /// </example>
        public static Header ParseHeaderString(string data)
        {

#if !PocketPC
            return ParseHeader(Encoding.GetEncoding("iso-8859-1").GetBytes(data));
#else
            return Parser.ParseHeader(Pop3Client.PPCEncode.GetBytes(data));
#endif
        }

        #endregion

        #region Message parsing

        /// <summary>
        /// Delegate for OnErrorParsingEvent.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="ex">The exception object.</param>
        public delegate void OnErrorParsingEvent(Object sender, Exception ex);

        /// <summary>
        /// Event handler for error parsing.
        /// </summary>
        public static event OnErrorParsingEvent ErrorParsing;
        /// <summary>
        /// Parses the message.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static Message ParseMessage(byte[] data)
        {
            var msg = Encoding.GetEncoding("iso-8859-1").GetString(data, 0, data.Length);

            return ParseMessage(ref msg);
        }

        /// <summary>
        /// Parses a Message from a string formatted accordingly to the RFC822.
        /// </summary>
        /// <param name="msg">The string containing the message data to be parsed.</param>
        /// <param name="loadOriginalMessage">Need load original eml in Message</param>
        /// <returns>The parsed message as a Message object.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = Parser.ParseMessageString(rfc822string);
        /// //Expose the subject
        /// string subject = message.Subject;
        /// 
        /// VB.NET
        /// 
        /// Dim message As Message = Parser.ParseMessageString(rfc822string)
        /// 'Expose the subject
        /// Dim subject As String = message.Subject
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = Parser.ParseMessageString(rfc822string);
        /// //Expose the subject
        /// var subject:string = message.Subject;
        /// </code>
        /// </example>
        public static Message ParseMessage(ref string msg, bool loadOriginalMessage = false)
        {
            var message = new Message();

            try
            {
                // Build a part tree and get all headers. 
                var part = ParseMimePart(ref msg, 0, msg.Length);
                // Fill a new message object with the new information.
                //message.OriginalData = data;
                message.HeaderFields = part.HeaderFields;
                message.HeaderFieldNames = part.HeaderFieldNames;

                // Dispatch header fields to corresponding object.
                foreach (var key in message.HeaderFields.AllKeys)
                {
                    var name = key;
                    var value = message.HeaderFields[key];
                    // TODO : Fix trace
                    if (name.Equals("received")) message.Trace.Add(ParseTrace(string.Format("{0}:{1}", key, value)));
                    else if (name.Equals("to")) message.To = ParseAddresses(value);
                    else if (name.Equals("cc")) message.Cc = ParseAddresses(value);
                    else if (name.Equals("bcc")) message.Bcc = ParseAddresses(value);
                    else if (name.Equals("reply-to")) message.ReplyTo = ParseAddress(value);
                    else if (name.Equals("from")) message.From = ParseAddress(value);
                    else if (name.Equals("sender")) message.Sender = ParseAddress(value);
                    else if (name.Equals("content-type")) message.ContentType = GetContentType(string.Format("{0}:{1}", key, value));
                    else if (name.Equals("content-disposition"))
                        message.ContentDisposition = GetContentDisposition(string.Format("{0}:{1}", key, value));
                    else if (name.Equals("domainkey-signature"))
                        message.Signatures.DomainKeys = Signature.Parse(string.Format("{0}:{1}", key, value), message);
                }

                if (message.ContentType.MimeType.Equals("application/pkcs7-mime")
                    || message.ContentType.MimeType.Equals("application/x-pkcs7-mime"))
                {
                    if (message.ContentType.Parameters["smime-type"] != null
                        && message.ContentType.Parameters["smime-type"].Equals("enveloped-data"))
                    {
                        message.IsSmimeEncrypted = true;
                    }

                    if (message.ContentType.Parameters["smime-type"] != null
                        && message.ContentType.Parameters["smime-type"].Equals("signed-data"))
                    {
                        message.HasSmimeSignature = true;
                    }
                }

                if (message.ContentType.MimeType.Equals("multipart/signed"))
                {
                    message.HasSmimeDetachedSignature = true;
                }

                if (message.ContentType.MimeType.Equals("multipart/report"))
                {
                    message.IsMultipartReport = true;
                }

                DispatchPart(part, ref message);

                // Check message for text limit
                const int message_limit = 500000; // 500kb

                if (!string.IsNullOrEmpty(message.BodyHtml.Text))
                {
                    if (message.BodyHtml.Text.Length > message_limit)
                    {
                        var charset = (!string.IsNullOrEmpty(message.BodyHtml.Charset) ? message.BodyHtml.Charset : "iso-8859-1");

                        message.AddAttachmentFromString("original_message.html",
                                                        string.Format(
                                                            "<!DOCTYPE html><html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset={0}\" /></head><body>{1}</body><html>",
                                                            charset,
                                                            message.BodyHtml.Text),
                                                        Encoding.GetEncoding(charset));
                    

                        // To long message's html body.
                        message.BodyHtml.Text =
                            "<div>The received email size is too big. The complete text was saved into a separate file and attached to this email.</div>";

                    }
                }
                else if (message.BodyText.Text.Length > message_limit)
                {
                    var charset = (!string.IsNullOrEmpty(message.BodyHtml.Charset) ? message.BodyHtml.Charset : "iso-8859-1");

                    message.AddAttachmentFromString("original_message.txt", message.BodyText.Text, Encoding.GetEncoding(charset));

                    // To long message's text body.

                    message.BodyText.Text =
                        string.Format(
                            "{0}\r\n...\r\n[The received email shown incompletely. The complete text was saved into a separate file and attached to this email.]",
                            message.BodyText.Text.Substring(0, message_limit));
                }

                message.OriginalData = loadOriginalMessage ? Encoding.GetEncoding("iso-8859-1").GetBytes(msg) : null;
            }
            catch (Exception ex)
            {
                if (ErrorParsing != null)
                    ErrorParsing(null, ex);
                else
                    throw;
            }
            return message;
        }

        private static bool IsHtmlTooComplex(string html)
        {
            if (string.IsNullOrEmpty(html)) return false;
            
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
            }
            catch (RecursionDepthException) { return true; }
            catch { }

            return false;
        }

        /// <summary>
        /// Parses a MemoryStream's content to a Message object.
        /// </summary>
        /// <param name="input_stream">The MemoryStream containing the Header data to be parsed.</param>
        /// <returns>The parsed Header as a Message object.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = Parser.ParseMessage(someStream);
        /// //Expose the subject
        /// string subject = message.Subject;
        /// 
        /// VB.NET
        /// 
        /// Dim message As Message = Parser.ParseMessage(someStream)
        /// 'Expose the subject
        /// Dim subject As String = message.Subject
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = Parser.ParseMessage(someStream);
        /// //Expose the subject
        /// var subject:string = message.Subject;
        /// </code>
        /// </example>
        public static Message ParseMessage(MemoryStream input_stream)
        {
            var buf = new byte[input_stream.Length];
            input_stream.Read(buf, 0, buf.Length);
            var msg = new Message {OriginalData = buf};
            ParseMessage(buf);//ref msg);
            return msg;
        }
        /// <summary>
        /// Parses a message from a file to a Message object.
        /// </summary>
        /// <param name="file_path">The path of the file to be parsed.</param>
        /// <returns>The parsed message as a Message object.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = Parser.ParseMessage("C:\\My messages\\message.txt");
        /// //Expose the subject
        /// string subject = message.Subject;
        /// 
        /// VB.NET
        /// 
        /// Dim message As Message = Parser.ParseMessage("C:\My messages\message.txt")
        /// 'Expose the subject
        /// Dim subject As String = message.Subject
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = Parser.ParseMessage("C:\\My messages\\message.txt");
        /// //Expose the subject
        /// var subject:string = message.Subject;
        /// </code>
        /// </example> 
        public static Message ParseMessageFromFile(string file_path)
        {
            string msg;

            using (var fs = new FileStream(file_path, FileMode.Open))
            {
                var data = new byte[fs.Length];
                fs.Read(data, 0, Convert.ToInt32(fs.Length));
                fs.Close();

                msg = Encoding.GetEncoding("iso-8859-1").GetString(data, 0, data.Length);
            }

            var message = ParseMessage(ref msg);

            return message;
        }

        #endregion

        #region Address parsing

        /// <summary>
        /// Parses a string containing addresses in the following formats :
        /// <list type="circle">
        /// <item>"John Doe" &lt;jdoe@myhost.com>,"Mike Johns" &lt;mjohns@otherhost.com></item>
        /// <item>"John Doe" &lt;jdoe@myhost.com>;"Mike Johns" &lt;mjohns@otherhost.com></item>
        /// <item>&lt;jdoe@myhost.com></item>
        /// <item>jdoe@myhost.com</item>
        /// </list>
        /// </summary>
        /// <param name="input">A string containing addresses in the formats desribed above.</param>
        /// <returns>An AddressCollection object containing the parsed addresses.</returns>
        public static AddressCollection ParseAddresses(string input)
        {
            //TODO: enforce parser to use regex
            var addresses = new AddressCollection();

            var comma_separated = input.Split(',');
            for (var i = 0; i < comma_separated.Length; i++)
                if (comma_separated[i].IndexOf("@", StringComparison.Ordinal) == -1 && comma_separated.Length > (i + 1))
                    comma_separated[i + 1] = comma_separated[i] + comma_separated[i + 1];

            foreach (var t in comma_separated.Where(t => t.IndexOf("@", StringComparison.Ordinal)!=-1))
                addresses.Add(
                    ParseAddress((
                    t.IndexOf("<", StringComparison.Ordinal) != -1 && 
                    t.IndexOf(":", StringComparison.Ordinal) != -1 && 
                    t.IndexOf(":", StringComparison.Ordinal) < t.IndexOf("<", StringComparison.Ordinal)) ? 
                    ((t.Split(':')[0].IndexOf("\"", StringComparison.Ordinal) == -1) ? t.Split(':')[1] : t) : t));

            return addresses;
        }

        /// <summary>
        /// Parses the address.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Address ParseAddress(string input)
        {
            var address = new Address();
            input = input.TrimEnd(';');
            try
            {
                if (input.IndexOf("<", StringComparison.Ordinal) == -1) address.Email = RemoveWhiteSpaces(input);
                else
                {
                    foreach (Match match in _regxEmail.Matches(input))
                    {
                        //This needed because only last match is email. SAmple name: name<teamlab>endname<teamlab@teamlab.com>.
                        //Two matches: <teamlab>, <teamlab@teamlab.com> - only last match - email.
                        // if format like name<teamlab@teamlab.com>endname<teamlab>.Its incorrect address.
                        address.Email = match.Value.TrimStart('<').TrimEnd('>');
                    }

                    address.Name = input.Replace(string.Format("<{0}>", address.Email), "");
                    address.Email = Clean(RemoveWhiteSpaces(address.Email));
                    if (address.Name.IndexOf("\"", StringComparison.Ordinal) == -1) address.Name = Clean(address.Name);
                    address.Name = address.Name.Trim(new[] {' ', '\"'});
                }
                return address;
            }
            catch
            {
                var addr = new MailAddress(input);
                return new Address(addr.Address, addr.DisplayName);
            }
        }

        public static bool TryParseTrace(string input, out TraceInfo trace_info)
        {
            trace_info = null;
            try
            {
                trace_info = ParseTrace(input);
                return true;
            }
            catch { }

            return false;
        }

        public static bool TryParseAddress(string input, out Address address)
        {
            address = null;
            try
            {
                address = ParseAddress(input);
                return true;
            }
            catch { }

            return false;
        }

        public static bool TryParseAddresses(string input, out AddressCollection addresses)
        {
            addresses = new AddressCollection();
            try
            {
                var comma_separated = input.Split(',');
                for (var i = 0; i < comma_separated.Length; i++)
                    if (comma_separated[i].IndexOf("@", StringComparison.Ordinal) == -1 && comma_separated.Length > (i + 1))
                        comma_separated[i + 1] = comma_separated[i] + comma_separated[i + 1];

                foreach (var t in comma_separated.Where(t => t.IndexOf("@", StringComparison.Ordinal) != -1))
                {
                    var address_string = (
                                         t.IndexOf("<", StringComparison.Ordinal) != -1 &&
                                         t.IndexOf(":", StringComparison.Ordinal) != -1 &&
                                         t.IndexOf(":", StringComparison.Ordinal) <
                                         t.IndexOf("<", StringComparison.Ordinal)
                                         ? ((t.Split(':')[0].IndexOf("\"", StringComparison.Ordinal) == -1)
                                                ? t.Split(':')[1]
                                                : t)
                                         : t);

                    Address address;
                    if (TryParseAddress(address_string, out address))
                    {
                        addresses.Add(address);
                    }
                }

                return true;
            }
            catch { }

            return false;
        }

        #endregion

        #region Date parsing

        /// <summary>
        /// Parses as universal date time.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static DateTime ParseAsUniversalDateTime(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    throw new NullReferenceException("input is empty");
                
                input = ReplaceTimeZone(input);
                input = Clean(input);
                input = _regxPlus.Replace(input, " ");
                input = _regxPlusEx.Replace(input, ":");

                if (input.IndexOf(",", StringComparison.Ordinal) != -1)
                {
                    input = input.Replace(string.Format("{0}, ", input.Split(',')[0]), "");
                }

                DateTime date;
                if (!DateTime.TryParse(input, out date))
                {
                    var parts = input.Replace("\t", string.Empty).Split(' ');
                    var year = Convert.ToInt32(parts[2]);
                    if (year < 100)
                    {
                        if (year > 49) year += 1900;
                        else year += 2000;
                    }
                    var month = GetMonth(parts[1]);
                    var day = Convert.ToInt32(parts[0]);
                    var date_parts = parts[3].Split(':');
                    var hour = Convert.ToInt32(date_parts[0]);
                    var minute = Convert.ToInt32(date_parts[1]);
                    var second = 0;
                    if (date_parts.Length > 2) second = Convert.ToInt32(date_parts[2]);
                    var offset_hours = Convert.ToInt32(parts[4].Substring(0, 3));
                    var offset_minutes = Convert.ToInt32(parts[4].Substring(3, 2));
                    date = new DateTime(year, month, day, hour, minute, second);
                    date = date.AddHours(-offset_hours);
                    date = date.AddMinutes(-offset_minutes);
                }

                return date;
            }
            catch (Exception)
            {
                return DateTime.UtcNow;
            }
        }

        #endregion

        #region Trace parsing

        /// <summary>
        /// Parses the trace.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static TraceInfo ParseTrace(string input)
        {
            var trace_info = new TraceInfo();
            
            var m = _regxTraceFrom.Match(input);
            if (m.Success) trace_info.From = m.Value.Trim(' ','\t');

            m = _regxTraceBy.Match(input);
            if (m.Success) trace_info.By = m.Value.Trim(' ', '\t');

            m = _regxTraceVia.Match(input);
            if (m.Success) trace_info.Via = m.Value.Trim(' ', '\t');

            m = _regxTraceWith.Match(input);
            if (m.Success) trace_info.With = m.Value.Trim(' ', '\t');

            m = _regxTraceFor.Match(input);
            if (m.Success) trace_info.For = m.Value.Trim(' ', '\t');

            m = _regxTraceId.Match(input);
            if (m.Success) trace_info.Id = m.Value.Trim(' ', '\t');
            
            trace_info.Date = ParseAsUniversalDateTime(input.Substring(input.LastIndexOf(';') + 1));
            
            return trace_info;
        }

        /// <summary>
        /// Parses the traces.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static TraceInfoCollection ParseTraces(string[] input)
        {
            var traceinfos = new TraceInfoCollection();
            try
            {
                foreach (var item in input)
                {
                    var traceinfo = new TraceInfo();
                    var itemlow = item.ToLower();

                    var found_index = itemlow.IndexOf(" from ", StringComparison.Ordinal);
                    if (found_index != -1) 
                        traceinfo.From = item.Substring(found_index + 6, item.IndexOf(" ", found_index + 6, StringComparison.Ordinal) - (found_index + 6)).TrimEnd(';');
                    
                    found_index = itemlow.IndexOf(" by ", StringComparison.Ordinal);
                    if (found_index != -1) 
                        traceinfo.By = item.Substring(found_index + 4, item.IndexOf(" ", found_index + 4, StringComparison.Ordinal) - (found_index + 4)).TrimEnd(';');
                    
                    found_index = itemlow.IndexOf(" for ", StringComparison.Ordinal);
                    if (found_index != -1) 
                        traceinfo.For = item.Substring(found_index + 5, item.IndexOf(" ", found_index + 5, StringComparison.Ordinal) - (found_index + 5)).TrimEnd(';');
                    
                    found_index = itemlow.IndexOf(" id ", StringComparison.Ordinal);
                    if (found_index != -1) 
                        traceinfo.Id = item.Substring(found_index + 4, item.IndexOf(" ", found_index + 4, StringComparison.Ordinal) - (found_index + 4)).TrimEnd(';');
                    
                    found_index = itemlow.IndexOf(" via ", StringComparison.Ordinal);
                    if (found_index != -1) 
                        traceinfo.Via = item.Substring(found_index + 5, item.IndexOf(" ", found_index + 5, StringComparison.Ordinal) - (found_index + 5)).TrimEnd(';');
                    
                    found_index = itemlow.IndexOf(" with ", StringComparison.Ordinal);
                    if (found_index != -1) 
                        traceinfo.With = item.Substring(found_index + 6, item.IndexOf(" ", found_index + 6, StringComparison.Ordinal) - (found_index + 6)).TrimEnd(';');
                    
                    traceinfo.Date = ParseAsUniversalDateTime(item.Split(';')[item.Split(';').Length - 1].Trim(' '));
                    traceinfos.Add(traceinfo);
                }
            }
            catch { }
            return traceinfos;
        }

        #endregion

        #endregion

        #endregion

    }
}