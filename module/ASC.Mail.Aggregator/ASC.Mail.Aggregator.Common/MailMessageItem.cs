/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ActiveUp.Net.Mail;
using ASC.Specific;
using HtmlAgilityPack;

namespace ASC.Mail.Aggregator.Common
{
    [DataContract(Namespace = "")]
    public class MailMessageItem
    {
        static readonly Regex UrlReg = new Regex(@"(?:(?:(?:http|ftp|gopher|telnet|news)://)(?:w{3}\.)?(?:[a-zA-Z0-9/;\?&=:\-_\$\+!\*'\(\|\\~\[\]#%\.])+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex EmailReg = new Regex(@"\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        static readonly Regex SurrogateCodePointReg = new Regex(@"\&#\d+;", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private static readonly DateTime DefaultMysqlMinimalDate = new DateTime(1975, 01, 01, 0, 0, 0); // Common decision of TLMail developers 

        public MailMessageItem()
        {
        }

        private bool IsDateCorrect(DateTime date)
        {
            return date >= DefaultMysqlMinimalDate && date <= DateTime.Now;
        }

        public MailMessageItem(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");

            Date = IsDateCorrect(message.Date)
                       ? message.Date
                       : (IsDateCorrect(message.ReceivedDate) ? message.ReceivedDate : DateTime.Now);
            MimeMessageId = message.MessageId;
            MimeReplyToId = message.InReplyTo;
            ChainId = message.MessageId;
            ReplyTo = message.ReplyTo.ToString();
            From = message.From.ToString();
            FromEmail = message.From.Email;
            To = string.Join(", ", message.To.Select(s => s.ToString()));
            Cc = string.Join(", ", message.Cc.Select(s => s.ToString()));
            Bcc = string.Join(", ", message.Bcc.Select(s => s.ToString()));
            Subject = message.Subject ?? "";
            bool is_message_has_high_flag = String.Compare(message.Flag, "high", StringComparison.OrdinalIgnoreCase) == 0;
            Important = is_message_has_high_flag || message.Priority == MessagePriority.High;
            TextBodyOnly = false;
            SetHtmlBodyAndIntroduction(message);
            Size = HtmlBody.Length;
            LoadAttachments(message.Attachments.Cast<MimePart>(), false);
            LoadAttachments(message.EmbeddedObjects.Cast<MimePart>(), true);
            LoadAttachments(message.UnknownDispositionMimeParts.Cast<MimePart>(), true);
        }

        private void SetHtmlBodyAndIntroduction(Message message)
        {
            HtmlBody = "<br/>";
            Introduction = "";

            var html_body_builder = new StringBuilder().Append(message.BodyHtml.Text);
            if (message.ContentType.MimeType.Equals("multipart/mixed", StringComparison.InvariantCultureIgnoreCase)
                && !string.IsNullOrEmpty(message.BodyHtml.Text)
                && !string.IsNullOrEmpty(message.BodyText.Text))
            {
                html_body_builder.AppendFormat("<p>{0}</p>", MakeHtmlFromText(message.BodyText.Text));
            }

            if (html_body_builder.Length != 0)
            {
                HtmlBody = html_body_builder.ToString();
                Introduction = GetIntroduction(HtmlBody);
                return;
            }

            if (!string.IsNullOrEmpty(message.BodyText.Text))
            {
                message.BodyText.Text = MakeHtmlFromText(message.BodyText.Text);
                HtmlBody = string.Format("<body><pre>{0}</pre></body>", message.BodyText.Text);
                Introduction = GetIntroduction(message.BodyText.Text);
                TextBodyOnly = true;
                return;
            }

            if (message.SubMessages.Count > 0)
            {
                BuildBodyFromSubmessages(message, html_body_builder);
                HtmlBody = html_body_builder.ToString();
                Introduction = GetIntroduction(message.SubMessages[0].BodyHtml.Text);
            }
        }

        private static void BuildBodyFromSubmessages(Message message, StringBuilder html_body_builder)
        {
            foreach (Message sub_message in message.SubMessages)
            {
                var to_string = string.Join(", ", sub_message.To.Select(s => s.ToString()));
                html_body_builder.Append("<hr /><br/>")
                                 .Append("<div style=\"padding-left:15px;\">")
                                 .AppendFormat("<span><b>Subject:</b> {0}</span><br/>", sub_message.Subject)
                                 .AppendFormat("<span><b>From:</b> {0}</span><br/>", sub_message.From)
                                 .AppendFormat("<span><b>Date:</b> {0}</span><br/>", sub_message.ReceivedDate)
                                 .AppendFormat("<span><b>To:</b> {0}</span><br/></div>", to_string)
                                 .AppendFormat("<br/>{0}", sub_message.BodyHtml.Text);
            }
        }

        public static string GetIntroduction(string html_body)
        {
            var introduction = string.Empty;
            
            if (!string.IsNullOrEmpty(html_body))
            {
                try
                {
                    introduction = ExtractTextFromHtml(html_body, 200);
                }
                catch (RecursionDepthException ex)
                {
                    throw new Exception(ex.Message);
                }
                catch
                {
                    introduction = (html_body.Length > 200 ? html_body.Substring(0, 200) : html_body);
                }

                introduction = introduction.Replace("\n", " ").Replace("\r", " ");
            }

            return introduction;
        }

        //if limit_length < 1 then text will be unlimited
        private static string ExtractTextFromHtml(string html, int limit_length)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var out_text = limit_length < 1 ? new StringBuilder() : new StringBuilder(limit_length);
            ConvertTo(doc.DocumentNode, out_text, limit_length);
            return out_text.ToString();
        }

        private static void ConvertTo(HtmlNode node, StringBuilder out_text, int limit_length)
        {
            if (out_text.Length >= limit_length) return;
            
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // Comments will not ouput.
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, out_text, limit_length);
                    break;

                case HtmlNodeType.Text:
                    // Scripts and styles will not ouput.
                    var parent_name = node.ParentNode.Name;
                    if ((parent_name == "script") || (parent_name == "style"))
                        break;

                    var html = ((HtmlTextNode)node).Text;

                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    html = html.Replace("&nbsp;", "");

                    if (html.Trim().Length > 0)
                    {
                        html = SurrogateCodePointReg.Replace(html, "");
                        var text = HtmlEntity.DeEntitize(html);
                        var new_length = (out_text + text).Length;
                        if (limit_length > 0 && new_length >= limit_length)
                        {
                            if (new_length > limit_length)
                            {
                                text = text.Substring(0, limit_length - out_text.Length);
                                out_text.Append(text);
                            }
                            return;
                        }
                        out_text.Append(text);
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            out_text.Append("\r\n");
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, out_text, limit_length);
                    }
                    break;
            }
        }

        private static void ConvertContentTo(HtmlNode node, StringBuilder out_text, int limit_length)
        {
            foreach (var subnode in node.ChildNodes)
            {
                ConvertTo(subnode, out_text, limit_length);
            }
        }

        private static string MakeHtmlFromText(string text_body)
        {
           var list_text = text_body.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();

            var builder = new StringBuilder(text_body.Length);

            list_text.ForEach(line =>
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var modified_line = line;

                    var found_urls = new List<string>();

                    for (var m = UrlReg.Match(modified_line); m.Success; m = m.NextMatch())
                    {
                        var found_url = m.Groups[0].Value;

                        found_urls.Add(string.Format("<a href=\"{0}\" target=\"_blank\">{0}</a>", found_url));

                        modified_line = modified_line.Replace(found_url, "{" + found_urls.Count.ToString(CultureInfo.InvariantCulture) + "}");
                    }

                    for (var m = EmailReg.Match(modified_line); m.Success; m = m.NextMatch())
                    {
                        var found_mail_address = m.Groups[0].Value;

                        found_urls.Add(string.Format("<a href=\"mailto:{0}\">{1}</a>", 
                            System.Web.HttpUtility.UrlEncode(found_mail_address),
                            found_mail_address));

                        modified_line = modified_line.Replace(found_mail_address, "{" + found_urls.Count.ToString(CultureInfo.InvariantCulture) + "}");
                    }

                    modified_line = System.Web.HttpUtility.HtmlEncode(modified_line);

                    if (found_urls.Count > 0)
                    {
                        for (int i = 0; i < found_urls.Count; i++)
                        {
                            modified_line = modified_line.Replace("{" + (i + 1).ToString(CultureInfo.InvariantCulture) + "}", found_urls.ElementAt(i));
                        }
                    }

                    builder.Append(modified_line);
                }
                builder.Append("<br/>");

                System.Threading.Thread.Sleep(1);

            });

            return builder.ToString();
        }

        public void LoadAttachments(IEnumerable<MimePart> mime_parts, bool skip_text)
        {
            if (Attachments == null)
            {
                Attachments = new List<MailAttachment>();
            }

            foreach (var mime_part in mime_parts)
            {
                if (skip_text && mime_part.ContentType.Type.Contains("text"))
                    continue;

                var ext = Path.GetExtension(mime_part.Filename);

                if (string.IsNullOrEmpty(ext))
                {
                    // If the file extension is not specified, there will be issues with saving on s3
                    var new_ext = ".ext";

                    if (mime_part.ContentType.Type.ToLower().IndexOf("image", StringComparison.Ordinal) != -1)
                    {
                        var sub_type = mime_part.ContentType.SubType;

                        new_ext = ".png";

                        if (!string.IsNullOrEmpty(sub_type))
                        {
                            // List was get from http://en.wikipedia.org/wiki/Internet_media_type
                            var known_image_types = new List<string> { "gif", "jpeg", "pjpeg", "png", "svg", "tiff", "ico", "bmp" };

                            var found_ext = known_image_types
                                .Find(s => sub_type.IndexOf(s, StringComparison.Ordinal) != -1);

                            if (!string.IsNullOrEmpty(found_ext))
                                new_ext = "." + found_ext;
                        }
                    }
                    mime_part.Filename = Path.ChangeExtension(mime_part.Filename, new_ext);
                }

                Attachments.Add(new MailAttachment
                    {
                        contentId = mime_part.EmbeddedObjectContentId,
                        size = mime_part.Size,
                        fileName = mime_part.Filename,
                        contentType = mime_part.ContentType.Type,
                        data = mime_part.BinaryContent,
                        contentLocation = mime_part.ContentLocation
                    });
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public List<MailAttachment> Attachments { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Introduction { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HtmlBody { get; set; }

        [DataMember]
        public bool ContentIsBlocked { get; set; }

        [DataMember]
        public bool Important { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public bool HasAttachments { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Bcc { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Cc { get; set; }

        [DataMember]
        public string To { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string From { get; set; }

        public string FromEmail { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ReplyTo { get; set; }

        [DataMember]
        public long Id { get; set; }

        /*[DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string InReplyTo { get; set; }*/

        [DataMember(EmitDefaultValue = false)]
        public string ChainId { get; set; }

        public DateTime ChainDate { get; set; }

        [DataMember(Name = "ChainDate")]
        public string ChainDateString
        {
            get { return ChainDate.ToString("G", CultureInfo.InvariantCulture); }
            set
            {
                DateTime date;
                if (DateTime.TryParseExact(value, "g", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
                {
                    ChainDate = date;
                }
            }
        }

        public DateTime Date { get; set; }

        [DataMember(Name = "Date")]
        public string DateString
        {
            get { return Date.ToString("G", CultureInfo.InvariantCulture);}
            set { 
                DateTime date;
                if (DateTime.TryParseExact(value,"g",CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal,out date))
                {
                    Date = date;
                }
            }
        }

        [DataMember(Name = "DateDisplay")]
        public string DateDisplay
        {
            get { return Date.ToVerbString(); }
        }

        [DataMember(EmitDefaultValue = false)]
        public ItemList<int> TagIds { get; set; }

        public string LabelsString
        {
            set
            {
                TagIds = new ItemList<int>(MailUtil.GetLabelsFromString(value));
            }
        }

        [DataMember(Name = "LabelsInString")]
        public string LabelsInString
        {
            get { return MailUtil.GetStringFromLabels(TagIds); }
        }


        [DataMember]
        public bool IsNew { get; set; }

        [DataMember]
        public bool IsAnswered { get; set; }

        [DataMember]
        public bool IsForwarded { get; set; }

        [DataMember]
        public bool IsFromCRM { get; set; }

        [DataMember]
        public bool IsFromTL { get; set; }

        [DataMember]
        public bool TextBodyOnly { get; set; }

        [DataMember]
        public long Size { get; set; }

        [DataMember]
        public string EMLLink { get; set; }

        [DataMember]
        public string StreamId { get; set; }

        [DataMember]
        public int RestoreFolderId { get; set; }

        [DataMember]
        public int Folder { get; set; }

        [DataMember]
        public int ChainLength { get; set; }

        [DataMember]
        public bool WasNew { get; set; }

        [DataMember]
        public bool IsToday { get; set; }

        [DataMember]
        public bool IsYesterday { get; set; }

        [DataMember]
        public ApiDateTime ReceivedDate
        {
            get { return (ApiDateTime)Date; }
        }

        public int MailboxId { get; set; }

        public List<CrmContactEntity> LinkedCrmEntityIds { get; set; }

        public List<int> ParticipantsCrmContactsId { get; set; }

        [DataMember]
        public bool IsBodyCorrupted { get; set; }

        [DataMember]
        public bool HasParseError { get; set; }

        [DataMember]
        public string MimeMessageId { get; set; }

        [DataMember]
        public string MimeReplyToId { get; set; }
    }
}