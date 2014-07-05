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

using ActiveUp.Net.Mail;
using ActiveUp.Net.Mail;
using ActiveUp.Net.Mail;
using ActiveUp.Net.Mail;
using ActiveUp.Net.Nntp;
using ActiveUp.Net.Mail;
using ActiveUp.Net.Security;
using ActiveUp.Net.Security;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System;

namespace ActiveUp.Net.Mail 
{
    /// <summary>
    /// Base class for all Parser objects.
    /// </summary>
    [System.Serializable]
    public class Parser
    {
        /*const string atext_pattern = "([A-Za-z0-9!#$%&'*+-/=?^_`{|}~]*)";
        const string dot_atom_text_pattern = "(1*([A-Za-z0-9!#$%&'*+-/=?^_`{|}~]*)([.]1*([A-Za-z0-9!#$%&'*+-/=?^_`{|}~]))*)";
        const string local_part_pattern = dot_atom_text_pattern/*+"|"+qtext_pattern;
        const string domain_pattern = */
        internal static int GetMonth(string month)
        {
            switch(month)
            {
                case "Jan" : return 1;
                case "Feb" : return 2;
                case "Mar" : return 3;
                case "Apr" : return 4;
                case "May" : return 5;
                case "Jun" : return 6;
                case "Jul" : return 7;
                case "Aug" : return 8;
                case "Sep" : return 9;
                case "Oct" : return 10;
                case "Nov" : return 11;
                case "Dec" : return 12;
                default : return -1;
            }
        }
        internal static string InvGetMonth(int month)
        {
            switch(month)
            {
                case 1 : return "Jan";
                case 2 : return "Feb";
                case 3 : return "Mar";
                case 4 : return "Apr";
                case 5 : return "May";
                case 6 : return "Jun";
                case 7 : return "Jul";
                case 8 : return "Aug";
                case 9 : return "Sep";
                case 10 : return "Oct";
                case 11 : return "Nov";
                case 12 : return "Dec";
                default : return "???";
            }
        }
        public static void ParseHeader(ref Header header)
        {
            string hdr = System.Text.Encoding.ASCII.GetString(header.OriginalData);
            hdr = System.Text.RegularExpressions.Regex.Match(hdr, @"[\s\S]+?((?=\r?\n\r?\n)|\Z)").Value;
            hdr = Parser.Unfold(hdr);
            hdr = Codec.RFC2047Decode(hdr);
            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(hdr, @"(?<=((\r?\n)|\n)|\A)\S+:(.|(\r?\n[\t ]))+(?=((\r?\n)\S)|\Z)");
            while(m.Success)
            {
                string name = FormatFieldName(m.Value.Substring(0, m.Value.IndexOf(':')));
                string value = m.Value.Substring(m.Value.IndexOf(":") + 1);
                if (name.Equals("received")) header.Trace.Add(Parser.ParseTrace(m.Value.Trim(' ')));
                else if (name.Equals("to")) header.To = Parser.ParseAddresses(value);
                else if (name.Equals("cc")) header.Cc = Parser.ParseAddresses(value);
                else if (name.Equals("bcc")) header.Bcc = Parser.ParseAddresses(value);
                else if (name.Equals("reply-to")) header.ReplyTo = Parser.ParseAddress(value);
                else if (name.Equals("from")) header.From = Parser.ParseAddress(value);
                else if (name.Equals("sender")) header.Sender = Parser.ParseAddress(value);
                else if (name.Equals("content-type")) header.ContentType = Parser.GetContentType(m.Value);
                else if (name.Equals("content-disposition")) header.ContentDisposition = Parser.GetContentDisposition(m.Value);

                header.HeaderFields.Add(name, value);
                header.HeaderFieldNames.Add(name, m.Value.Substring(0, m.Value.IndexOf(':')));
                m = m.NextMatch();
            }
        }
        private static ContentType GetContentType(string input)
        {
            ContentType field = new ContentType();
            field.MimeType = Regex.Match(input, @"(?<=: ?)\S+?(?=([;\s]|\Z))").Value;
            Match parammatch = Regex.Match(input, @"(?<=;\s+)[^;\s]*=[^;]*(?=(;|\Z))");
            while (parammatch.Success)
            {
                field.Parameters.Add(FormatFieldName(parammatch.Value.Substring(0, parammatch.Value.IndexOf('='))).ToLower(), parammatch.Value.Substring(parammatch.Value.IndexOf('=') + 1).Replace("\"","").Trim('\r','\n'));
                parammatch = parammatch.NextMatch();
            }
            return field;
        }
        private static ContentDisposition GetContentDisposition(string input)
        {
            ContentDisposition field = new ContentDisposition();
            field.Disposition = Regex.Match(input, @"(?<=: ?)\S+?(?=([;\s]|\Z))").Value;
            System.Text.RegularExpressions.Match parammatch = System.Text.RegularExpressions.Regex.Match(input, @"(?<=;[ \t]?)[^;]*=[^;]*(?=(;|\Z))");
            for (; parammatch.Success; parammatch = parammatch.NextMatch()) field.Parameters.Add(FormatFieldName(parammatch.Value.Substring(0, parammatch.Value.IndexOf('='))), parammatch.Value.Substring(parammatch.Value.IndexOf('=') + 1).Replace("\"", "").Trim('\r', '\n'));
            return field;
        }
        private static void ParseHeaderv1(ref Headerv1 header)
        {
            try
            {
                string hdr = System.Text.Encoding.ASCII.GetString(header._data);
                hdr = hdr.Replace("\n\n","\r\n\r\n");
                hdr = hdr.Replace("\r\r","\r\n\r\n");
                hdr = hdr.Replace("\r\n\t","²didju²");
                hdr = hdr.Replace("\n\t","\r\n\t");
                hdr = hdr.Replace("²didju²","\r\n\t");
                hdr = hdr.Replace("\r\n\t","²didju²");
                hdr = hdr.Replace("\r\t","\r\n\t");
                hdr = hdr.Replace("²didju²","\r\n\t");
                if(hdr.LastIndexOf("\r\n\r\n")!=-1) header.Lines = Codec.RFC2047Decode(hdr.Substring(0,hdr.IndexOf("\r\n\r\n")).Replace("\r\n\t"," ").Replace("\r\n "," "))+"\r\n";
                else header.Lines = Codec.RFC2047Decode(hdr.Replace("\r\n\t"," ").Replace("\r\n "," "));
            }
            catch(System.Exception) {  }
        }
        private static void DecodeMessage(ref Message message)
        {

        }
        /*private static void ParseMessage(ref Message message)
        {
            ParseMessage(ref message, new X509Certificate2Collection());
        }*/
        private static Message ParseMessage(byte[] data)
        {
            string msg = System.Text.Encoding.ASCII.GetString(data);
            Message message = (Message)Parser.ParseMimeTypedAndEncodedContent(msg);

            foreach (string key in message.HeaderFields.AllKeys)
            {
                string name = key;
                string value = message.HeaderFields[key];
                if (name.Equals("received")) message.Trace.Add(Parser.ParseTrace(key + ": " + value));
                else if (name.Equals("to")) message.To = Parser.ParseAddresses(value);
                else if (name.Equals("cc")) message.Cc = Parser.ParseAddresses(value);
                else if (name.Equals("bcc")) message.Bcc = Parser.ParseAddresses(value);
                else if (name.Equals("reply-to")) message.ReplyTo = Parser.ParseAddress(value);
                else if (name.Equals("from")) message.From = Parser.ParseAddress(value);
                else if (name.Equals("sender")) message.Sender = Parser.ParseAddress(value);
                else if (name.Equals("content-type")) message.ContentType = Parser.GetContentType(key + ": " + value);
                else if (name.Equals("content-disposition")) message.ContentDisposition = Parser.GetContentDisposition(key + ": " + value);
                else if (name.Equals("domainkey-signature")) message.Signatures.DomainKeys = Signature.Parse(key + ": " + value, message);
            }

            return message;
            //if (message.ContentType.MimeType != null && message.ContentType.MimeType.IndexOf("multipart/signed") != -1) message.IsSigned = true;
            //if (message.ContentType.MimeType != null && message.ContentType.MimeType.IndexOf("multipart/encrypted") != -1) message.IsEncrypted = true;

        }
        private static void ParseMessage(ref Message message, X509Certificate2Collection certificates)
        {   
            // Header parsing work
        
            string msg = System.Text.Encoding.ASCII.GetString(message.OriginalData);
            string header = msg.Substring(0,Regex.Match(msg, @"\r?\n\r?\n").Index);
            header = Parser.Unfold(header);
            header = Codec.RFC2047Decode(header);
            Match m = Regex.Match(header, @"(?<=((\r?\n)|\n)|\A)\S+:(.|(\r?\n[\t ]))+(?=((\r?\n)\S)|\Z)");
            while (m.Success)
            {
                string name = FormatFieldName(m.Value.Substring(0, m.Value.IndexOf(':')));
                string value = m.Value.Substring(m.Value.IndexOf(":") + 1);
                if (name.Equals("received")) message.Trace.Add(Parser.ParseTrace(m.Value.Trim(' ')));
                else if (name.Equals("to")) message.To = Parser.ParseAddresses(value);
                else if (name.Equals("cc")) message.Cc = Parser.ParseAddresses(value);
                else if (name.Equals("bcc")) message.Bcc = Parser.ParseAddresses(value);
                else if (name.Equals("reply-to")) message.ReplyTo = Parser.ParseAddress(value);
                else if (name.Equals("from")) message.From = Parser.ParseAddress(value);
                else if (name.Equals("sender")) message.Sender = Parser.ParseAddress(value);
                else if (name.Equals("content-type")) message.ContentType = Parser.GetContentType(m.Value);
                else if (name.Equals("content-disposition")) message.ContentDisposition = Parser.GetContentDisposition(m.Value);
                else if (name.Equals("domainkey-signature")) message.Signatures.DomainKeys = Signature.Parse(m.Value,message);

                message.HeaderFields.Add(name,value);
                message.HeaderFieldNames.Add(name, m.Value.Substring(0, m.Value.IndexOf(':')));
                m = m.NextMatch();
            }

            // Body parsing work

            if (message.ContentType.MimeType != null && message.ContentType.MimeType.IndexOf("multipart/signed") != -1) message.IsSigned = true;
            if (message.ContentType.MimeType != null && message.ContentType.MimeType.IndexOf("multipart/encrypted") != -1) message.IsEncrypted = true;

            //MimeTypedAndEncodedContent part = Parser.ParseMimeTypedAndEncodedContent(msg);//, certificates);
            
            /*if (part.ContentType.MimeType!=null && part.ContentType.MimeType.IndexOf("multipart/") != -1)
            {
                Parser.ParseMultipart(ref message, part, certificates);
                Parser.DistributeParts(ref message);
                Parser.SetBodies(ref message);
            }
            else if (part.ContentDisposition.Disposition.Equals("attachment"))
            {
                message.AllMimeParts.Add(part);
                message.Attachments.Add(part);
            }
            else
            {
                message.AllMimeParts.Add(part);
                if (part.ContentType.MimeType!=null && part.ContentType.MimeType.ToLower().IndexOf("text/html") != -1)
                {
                    if (part.ContentTransferEncoding == ContentTransferEncoding.Base64) message.BodyHtml.Text = System.Text.Encoding.GetEncoding(part.Charset).GetString(System.Convert.FromBase64String(part.TextContent.Substring(0, part.TextContent.IndexOf("\r\n\r\n"))));
                    else message.BodyHtml.Text = part.TextContent;
                }
                else if (part.ContentType.MimeType != null && part.ContentType.MimeType.ToLower().IndexOf("text/plain") != -1)
                {
                    if (part.ContentTransferEncoding == ContentTransferEncoding.Base64) message.BodyText.Text = System.Text.Encoding.GetEncoding(part.Charset).GetString(System.Convert.FromBase64String(part.TextContent.Substring(0, part.TextContent.IndexOf("\r\n\r\n"))));
                    else message.BodyText.Text = part.TextContent;
                }
                else
                {
                    message.BodyText.Text = System.Text.Encoding.ASCII.GetString(message.BodyBinary);
                }
            }

            if (!ActiveUp.Base.InternalLicense.Status.IsRegistered || ActiveUp.Base.InternalLicense.IsLite())
            {

                if (message.BodyHtml.Text.Length > 0
                    && message.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredHtmlSent) == -1
                    && message.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorHtmlSent) == -1
                    && message.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredHtmlReceived) == -1)
                    message.BodyHtml.Text = ActiveUp.Base.InternalLicense.UnRegisteredHtmlReceived + "<br><br>" +  message.BodyHtml.Text;
                
                if ((message.BodyText.Text.Length > 0 || message.BodyHtml.Text.Length == 0)
                    && message.BodyText.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredTextSent) == -1
                    && message.BodyText.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorTextSent) == -1
                    && message.BodyText.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredTextReceived) == -1)
                    message.BodyText.Text = ActiveUp.Base.InternalLicense.UnRegisteredTextReceived + "\n\n" + message.BodyText.Text; 
            }
            
            else if (ActiveUp.Base.InternalLicense.IsSponsored())
            {
                if (message.BodyHtml.Text.Length > 0
                    && message.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorHtmlSent) == -1
                    && message.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorHtmlReceived) == -1
                    && message.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredHtmlSent) == -1)
                    message.BodyHtml.Text += "<br><br>" + ActiveUp.Base.InternalLicense.SponsorHtmlReceived;

                if ((message.BodyText.Text.Length > 0 || message.BodyHtml.Text.Length == 0)
                    && message.BodyText.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorTextSent) == -1
                    && message.BodyText.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorTextReceived) == -1
                    && message.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredTextSent) == -1)
                    message.BodyText.Text += "\n\n" + ActiveUp.Base.InternalLicense.SponsorTextReceived;
            }*/
        }
        public static MimePart ParsePart(string data, X509Certificate2Collection certificates)
        {
            return ParsePart(data, certificates, new SignedCms());
        }
        public static MimeTypedAndEncodedContent ParseMimeTypedAndEncodedContent(string data)
        {
            MimeTypedAndEncodedContent part = new MimeTypedAndEncodedContent();

            // Separate header and body.
            int headerEnd = Regex.Match(data, @".(?=\r?\n\r?\n)").Index + 1;
            int bodyStart = Regex.Match(data, @"(?<=\r?\n\r?\n).").Index - 1;
            
            string header = data.Substring(0,headerEnd);
            header = Parser.Unfold(header);
            header = Codec.RFC2047Decode(header);

            string body = data.Substring(bodyStart);

            // Parse header fields and their parameters.
            Match m = Regex.Match(header, @"(?<=((\r?\n)|\n)|\A)\S+:(.|(\r?\n[\t ]))+(?=((\r?\n)\S)|\Z)");
            while (m.Success)
            {
                if (m.Value.ToLower().StartsWith("content-type:")) part.ContentType = Parser.GetContentType(m.Value);
                else if (m.Value.ToLower().StartsWith("content-disposition:")) part.ContentDisposition = Parser.GetContentDisposition(m.Value);
                part.HeaderFields.Add(FormatFieldName(m.Value.Substring(0, m.Value.IndexOf(':'))), m.Value.Substring(m.Value.IndexOf(':') + 1));
                m = m.NextMatch();
            }

            // Is it QP encoded text ?
            if (part.ContentTransferEncoding.Equals(ContentTransferEncoding.QuotedPrintable))
            {
                // Get the destination charset, or default to us-ascii.
                string charset = "us-ascii";
                if (part.Charset != null && part.Charset.Length > 0) charset = part.Charset;

                // Decode
                part.TextContent = Codec.FromQuotedPrintable(body, charset);
                //part.BinaryContent = System.Text.Encoding.GetEncoding(charset).GetBytes(part.TextContent);
            }
            // Is it a Base64 encoded content ?
            else if (part.ContentTransferEncoding.Equals(ContentTransferEncoding.Base64))
            {
                part.TextContent = body;
                //part.BinaryContent = Convert.FromBase64String(part.TextContent);
            }
            // Is it plain text or binary data ?
            else //if (part.ContentTransferEncoding.Equals(ContentTransferEncoding.SevenBits) || part.ContentTransferEncoding.Equals(ContentTransferEncoding.SevenBits))
            {
                // Get the destination charset, or default to us-ascii.
                string charset = "us-ascii";
                if (part.Charset != null && part.Charset.Length > 0) charset = part.Charset;

                // Extract
                part.TextContent = body;
                //part.BinaryContent = System.Text.Encoding.GetEncoding(charset).GetBytes(part.TextContent);
            }

            // Now we have the decoded content and it's type. Let's take appropriate action.
            if (part.ContentType.Type.Equals("multipart"))
            {
                MultipartContainer multipart = Parser.ParseMultipartContainer(part);
                multipart.Track = DispatchTrack.MultipartContainer;
                return multipart;
            }
            else if (part.ContentType.Type.Equals("message"))
            {
                // TODO
            }
            //else if (part.ContentType.Type.Equals("multipart"))
            
            return part;

        }
        /*public static MimePart ParsePart(string data, X509Certificate2Collection certificates, SignedCms signature)
        {
            MimePart part = new MimePart();
            if (signature.Certificates.Count != 0) part.IsSigned = true;
            part.Signature = signature;
            
            string header = data.Substring(0,Regex.Match(data, @"\r?\n\r?\n").Index);
            Match m = Regex.Match(header, @"(?<=((\r?\n)|\n)|\A)\S+:(.|(\r?\n[\t ]))+(?=((\r?\n)\S)|\Z)");
            while (m.Success)
            {
                if (m.Value.ToLower().StartsWith("content-type:")) part.ContentType = Parser.GetContentType(m.Value);
                else if (m.Value.ToLower().StartsWith("content-disposition:")) part.ContentDisposition = Parser.GetContentDisposition(m.Value);
                part.HeaderFields.Add(FormatFieldName(m.Value.Substring(0, m.Value.IndexOf(':'))), m.Value.Substring(m.Value.IndexOf(':') + 1));
                m = m.NextMatch();
            }
            
            // S/MIME support
            // TODO : Differenciate by ASN.1 decoding
            if (part.ContentType.MimeType!=null && (part.ContentType.MimeType.IndexOf("application/x-pkcs7-mime") != -1 || part.ContentType.MimeType.IndexOf("application/pkcs7-mime") != -1))
            {
                if (part.ContentType.Parameters["smime-type"]!=null && part.ContentType.Parameters["smime-type"].ToLower().IndexOf("signed-data") != -1)
                {
                    part.IsSigned = true;
                    string contentBase64 = data.Substring(Regex.Match(data, @"\r?\n\r?\n").Index);
                    byte[] signedData = System.Convert.FromBase64String(contentBase64);
                    SignedCms sig = new SignedCms();
                    sig.Decode(signedData);
                    
                    return ParsePart(System.Text.Encoding.ASCII.GetString(sig.ContentInfo.Content), certificates, sig);
                }
                else
                {
                    part.IsEncrypted = true;
                    string contentBase64 = data.Substring(Regex.Match(data, @"\r?\n\r?\n").Index);
                    EnvelopedCms env = new EnvelopedCms();
                    env.Decode(System.Convert.FromBase64String(contentBase64));
                    env.Decrypt(certificates);
                    byte[] decryptedContent = env.ContentInfo.Content;
                    
                    return ParsePart(System.Text.Encoding.ASCII.GetString(decryptedContent), certificates);
                }
            }
            if (part.Charset==null || part.Charset.Length < 1) part.Charset = "us-ascii";
            byte[] binData = System.Text.Encoding.ASCII.GetBytes(data);
            try { part.TextContent = System.Text.Encoding.GetEncoding(part.Charset).GetString(binData); }
            catch { part.TextContent = data; }
            m = Regex.Match(part.TextContent,@"\r?\n\r?\n");
            part.TextContent = part.TextContent.Substring(m.Index+m.Length);
            
            if (part.ContentType.MimeType != null && part.ContentType.MimeType.IndexOf("multipart/signed") != -1) part.IsSigned = true;
            if (part.ContentTransferEncoding==ContentTransferEncoding.QuotedPrintable) part.TextContent = Codec.FromQuotedPrintable(part.TextContent,part.Charset);
            if (part.ContentTransferEncoding == ContentTransferEncoding.Base64)
            {
                part.TextContent = part.TextContent.Trim('\r', '\n');
                part.BinaryContent = System.Convert.FromBase64String(part.TextContent);
            }
            else part.BinaryContent = System.Text.Encoding.GetEncoding(part.Charset).GetBytes(part.TextContent);
            
            return part;
        }*/

        private static MultipartContainer ParseMultipartContainer(MimePart part)
        {
            MultipartContainer container = new MultipartContainer();
            string boundary = part.ContentType.Parameters["boundary"];
            string[] arrpart = Regex.Split(part.TextContent, @"\r?\n?" + Regex.Escape("--" + boundary));
            for (int i = 0; i < arrpart.Length; i++)
            {
                string strpart = arrpart[i];
                if (!strpart.StartsWith("--"))
                {
                    container.PartTree.Add(Parser.ParseMimeTypedAndEncodedContent(strpart));
                }
            }
            return container;
        }
        /*private static void ParseMultipart(ref Message message, MimePart part, X509Certificate2Collection certificates)
        {
            string boundary = part.ContentType.Parameters["boundary"];
            ActiveUp.Net.Mail.Logger.AddEntry("bound : " + boundary); 
            string[] arrpart = Regex.Split(part.TextContent,@"\r?\n?"+Regex.Escape("--"+boundary));
            int start = 1;
            int end = arrpart.Length;
            if(part.ContentType.MimeType.IndexOf("multipart/signed")!=-1) 
            {
                start = end-1;
                end = 0;
            }
            
            for(int i = start;((start>end) ? i>end : i<end);i = ((start<end) ? i+1 : i-1))
            {
                string strpart = arrpart[i];
                if (!strpart.StartsWith("--"))
                {
                    MimePart newpart = Parser.ParsePart(strpart, certificates);
                    
                    // S/MIME support (detached signatures)
                    if (part.IsSigned
                        && (newpart.ContentType.MimeType.IndexOf("application/pkcs7-signature") != -1
                            || newpart.ContentType.MimeType.IndexOf("application/x-pkcs7-signature") != -1))
                    {
                        Match startDelimiter = Regex.Match(part.TextContent, "--" + Regex.Escape(boundary) + "\r?\n");
                        Match endDelimiter = Regex.Match(part.TextContent, "\r?\n--" + Regex.Escape(boundary) + "\r?\n", RegexOptions.RightToLeft);
                        int s = startDelimiter.Index+startDelimiter.Length;
                        int e = endDelimiter.Index;
                        byte[] content = System.Text.Encoding.ASCII.GetBytes(part.TextContent.Substring(s,e-s));
                        SignedCms signature = new SignedCms(new ContentInfo(content),true);
                        signature.Decode(System.Convert.FromBase64String(newpart.TextContent));
                        part.Signature = signature;
                        continue;
                    }
                    if (part.IsSigned)
                    {
                        newpart.IsSigned = true;
                        newpart.Signature = part.Signature;
                    }
                    if (part.IsEncrypted) newpart.IsEncrypted = true;
                    if (newpart.ContentType.MimeType!=null && newpart.ContentType.MimeType.ToLower().IndexOf("multipart/") != -1) Parser.ParseMultipart(ref message, newpart, certificates);
                    else if (newpart.ContentType.MimeType != null && newpart.ContentType.MimeType.ToLower().IndexOf("message/rfc822") != -1) message.SubMessages.Add(Parser.ParseMessage(newpart.BinaryContent));
                    else if (newpart.TextContent != "\r\n" && newpart.TextContent != "\r" && newpart.TextContent != "\n" && newpart.TextContent != "" && newpart.TextContent != "--") message.AllMimeParts.Add(newpart);
                }
            }
        }*/
        private static void DistributeParts(ref Message message)
        {
            //try
            //{
                foreach(MimePart part in message.AllMimeParts)
                {
                    if(part.ContentDisposition.Disposition.Equals("attachment")) message.Attachments.Add(part);
                    if(part.ContentDisposition.Disposition.Equals("inline")) message.EmbeddedObjects.Add(part);
                }
            //}
            //catch(System.Exception) {  }
        }
        private static void SetBodies(ref Message message)
        {
            //try
            //{
                foreach(MimePart part in message.AllMimeParts)
                {
                    if ((part.ContentDisposition.Disposition.Equals("") || part.ContentDisposition.Disposition.Equals("inline")) && part.ContentType.MimeType.ToLower().IndexOf("text/plain") != -1) 
                    {
                        message.BodyText.Charset = part.Charset;
                        message.BodyText.Text = part.TextContent;
                    }
                }
                foreach(MimePart part in message.AllMimeParts)
                {
                    if ((part.ContentDisposition.Disposition.Equals("") || part.ContentDisposition.Disposition.Equals("inline")) && part.ContentType.MimeType.ToLower().IndexOf("text/html") != -1) 
                    {
                        message.BodyHtml.Charset = part.Charset;
                        message.BodyHtml.Text = part.TextContent;
                    }
                }
            //}
            //catch(System.Exception) {  }
        }
        public static string Fold(string input)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string[] separated = input.Split(' ');
            string templine = string.Empty;
            for(int i=0;i<separated.Length;i++)
            {
                if(templine.Length+separated[i].Length<77) templine += separated[i]+" ";
                else
                {
                    sb.Append(templine+"\r\n ");
                    templine = string.Empty;
                }
            }
            sb.Append(templine);
            return sb.ToString();
        }
        public static string Unfold(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input,@"\r?\n(?=[ \t])","");
        }
        /// <summary>
        /// Parses a Header from a file to a Header object.
        /// </summary>
        /// <param name="filePath">The path of the file to be parsed.</param>
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
        public static Header ParseHeader(string filePath)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(filePath);
            byte[] data = new byte[fs.Length];
            fs.Read(data,0,System.Convert.ToInt32(fs.Length));
            fs.Close();
            Header hdr = new Header();
            hdr.OriginalData = data;
            Parser.ParseHeader(ref hdr);
            return hdr;
        }
        public static Headerv1 ParseHeaderv1(string filePath)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(filePath);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, System.Convert.ToInt32(fs.Length));
            fs.Close();
            Headerv1 hdr = new Headerv1();
            hdr._data = data;
            Parser.ParseHeaderv1(ref hdr);
            return hdr;
        }
        /// <summary>
        /// Parses a MemoryStream's content to a Header object.
        /// </summary>
        /// <param name="inputStream">The MemoryStream containing the Header data to be parsed.</param>
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
        public static Header ParseHeader(System.IO.MemoryStream inputStream)
        {
            byte[] buf = new byte[inputStream.Length];
            inputStream.Read(buf,0,buf.Length);
            Header hdr = new Header();
            hdr.OriginalData = buf;
            Parser.ParseHeader(ref hdr);
            return hdr;
        }
        /// <summary>
        /// Parses a Header object from a byte array.
        /// </summary>
        /// <param name="data">The byte array containing the Header data to be parsed.</param>
        /// <returns>The parsed Header as a Header object.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Header Header = Parser.ParseHeader(someBuffer);
        /// //Expose the subject
        /// string subject = header.Subject;
        /// 
        /// VB.NET
        /// 
        /// Dim Header As Header = Parser.ParseHeader(someBuffer)
        /// 'Expose the subject
        /// Dim subject As String = header.Subject
        /// 
        /// JScript.NET
        /// 
        /// var header:Header = Parser.ParseHeader(someBuffer);
        /// //Expose the subject
        /// var subject:string = header.Subject;
        /// </code>
        /// </example> 
        public static Header ParseHeader(byte[] data)
        {
            Header hdr = new Header();
            hdr.OriginalData = data;
            Parser.ParseHeader(ref hdr);
            return hdr;
        }
        /// <summary>
        /// Parses a message from a file to a Message object.
        /// </summary>
        /// <param name="filePath">The path of the file to be parsed.</param>
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
        public static Message ParseMessageFromFile(string filePath, X509Certificate2Collection extraCertificates)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(filePath);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, System.Convert.ToInt32(fs.Length));
            fs.Close();
            Message msg = new Message();
            msg.OriginalData = data;
            Parser.ParseMessage(ref msg, extraCertificates);
            return msg;
        }
        public static Message ParseMessageFromFile(string filePath)
        {
            return ParseMessageFromFile(filePath, new X509Certificate2Collection());
        }
        /*public static Message ParseMessageFromFile(string filePath, CertificateSelectionCallback certificateSelectionCallback)
        {
            Message msg = ParseMessageFromFile(filePath);
            if(msg.ContentType.Equals("application/pkcs7-mime") || msg.ContentType.Equals("application/x-pkcs7-mime") 
                if(msg.SmimeType.Equals(SmimeType.EnvelopedData)) return Parser.ParseMessage(.certificateSelectionCallback.Invoke(msg.Recipients);
        }
        public delegate System.Security.Cryptography.X509Certificates.X509Certificate2 CertificateSelectionCallback(AddressCollection recipients);
        /*public static Message ParseMessageFromFile(string filePath, System.Security.Cryptography.X509Certificates.X509Certificate2Collection certificates)
        {

        }*/
        /// <summary>
        /// Parses a MemoryStream's content to a Message object.
        /// </summary>
        /// <param name="inputStream">The MemoryStream containing the Header data to be parsed.</param>
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
        public static Message ParseMessage(System.IO.MemoryStream inputStream)
        {
            byte[] buf = new byte[inputStream.Length];
            inputStream.Read(buf,0,buf.Length);
            Message msg = new Message();
            msg.OriginalData = buf;
            Parser.ParseMessage(buf);//ref msg);
            return msg;
        }
        
        /*/// <summary>
        /// Parses a Message from a byte array.
        /// </summary>
        /// <param name="data">The byte array containing the message data to be parsed.</param>
        /// <returns>The parsed message as a Message object.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = Parser.ParseMessage(someBuffer);
        /// //Expose the subject
        /// string subject = message.Subject;
        /// 
        /// VB.NET
        /// 
        /// Dim message As Message = Parser.ParseMessage(someBuffer)
        /// 'Expose the subject
        /// Dim subject As String = message.Subject
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = Parser.ParseMessage(someBuffer);
        /// //Expose the subject
        /// var subject:string = message.Subject;
        /// </code>
        /// </example>
        public static Message ParseMessage(byte[] data)
        {
            Message msg = new Message();
            msg.OriginalData = data;
            Parser.ParseMessage(ref msg);
            return msg;
        }*/
        
        /// <summary>
        /// Parses a Message from a string formatted accordingly to the RFC822.
        /// </summary>
        /// <param name="data">The string containing the message data to be parsed.</param>
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
        public static Message ParseMessage(string data)
        {
            return Parser.ParseMessage(System.Text.Encoding.ASCII.GetBytes(data));
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
            return Parser.ParseHeader(System.Text.Encoding.ASCII.GetBytes(data));
        }
        //Address parsing conformant to RFC2822's addr-spec.
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
            AddressCollection addresses = new AddressCollection();
            string[] comma_separated = input.Split(',');
            for(int i=0;i<comma_separated.Length;i++) 
                if(comma_separated[i].IndexOf("@")==-1 && comma_separated.Length>(i+1)) 
                    comma_separated[i+1] = comma_separated[i]+comma_separated[i+1];

            for(int i=0;i<comma_separated.Length;i++) /*if(comma_separated[i].IndexOf("@")!=-1)*/ 
                addresses.Add(Parser.ParseAddress((comma_separated[i].IndexOf("<")!=-1 && comma_separated[i].IndexOf(":")!=-1 && comma_separated[i].IndexOf(":")<comma_separated[i].IndexOf("<")) ? ((comma_separated[i].Split(':')[0].IndexOf("\"")==-1) ? comma_separated[i].Split(':')[1] : comma_separated[i]) : comma_separated[i]));

            return addresses;
        }
        public static Address ParseAddress(string input)
        {
            Address address = new Address();
            input = input.TrimEnd(';');
            try
            {
                if(input.IndexOf("<")==-1) address.Email = Parser.RemoveWhiteSpaces(input);
                else
                {
                    address.Email = System.Text.RegularExpressions.Regex.Match(input,"<(.|[.])*>").Value.TrimStart('<').TrimEnd('>');
                    address.Name = input.Replace("<"+address.Email+">","");
                    address.Email = Parser.Clean(Parser.RemoveWhiteSpaces(address.Email));
                    if(address.Name.IndexOf("\"")==-1) address.Name = Parser.Clean(address.Name);
                    address.Name = address.Name.Trim(new char[] {' ','\"'});
                }
                return address;
            }
            catch { return new Address(input); }
        }
        //End of address parsing.
        //Date parsing conformant to RFC2822 and accepting RFC822 dates.
        public static System.DateTime ParseAsUniversalDateTime(string input)
        {
            input = Parser.ReplaceTimeZone(input);
            input = Parser.Clean(input);
            input = System.Text.RegularExpressions.Regex.Replace(input,@" +"," ");
            input = System.Text.RegularExpressions.Regex.Replace(input,@"( +: +)|(: +)|( +:)",":");
            if(input.IndexOf(",")!=-1) 
            {
                input = input.Replace(input.Split(',')[0]+", ","");
            }
            string[] parts = input.Split(' ');
            int year = System.Convert.ToInt32(parts[2]);
            if(year<100)
            {
                if(year>49) year += 1900;
                else year += 2000;
            }
            int month = Parser.GetMonth(parts[1]);
            int day = System.Convert.ToInt32(parts[0]);
            string[] dateParts = parts[3].Split(':');
            int hour = System.Convert.ToInt32(dateParts[0]);
            int minute = System.Convert.ToInt32(dateParts[1]);
            int second = 0;
            if(dateParts.Length>2) second = System.Convert.ToInt32(dateParts[2]);
            int offset_hours = System.Convert.ToInt32(parts[4].Substring(0,3));
            int offset_minutes = System.Convert.ToInt32(parts[4].Substring(3,2));
            System.DateTime date = new System.DateTime(year,month,day,hour,minute,second);
            date = date.AddHours(-offset_hours);
            date = date.AddMinutes(-offset_minutes);
            return date;
        }
        private static string ReplaceTimeZone(string input)
        {
            input = input.Replace("EDT","-0400");
            input = input.Replace("EST","-0500");
            input = input.Replace("CDT","-0500");
            input = input.Replace("CST","-0600");
            input = input.Replace("MDT","-0600");
            input = input.Replace("MST","-0700");
            input = input.Replace("PDT","-0700");
            input = input.Replace("PST","-0800");
            input = input.Replace("UT","+0000");
            input = input.Replace("GMT","+0000");
            return input;
        }
        internal static string RemoveWhiteSpaces(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input,@"\s+","");
        }
        internal static string FormatFieldName(string fieldName)
        {
            return fieldName.ToLower();
        }
        public static TraceInfo ParseTrace(string input)
        {
            TraceInfo traceInfo = new TraceInfo();
            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(input, @"from.+?(?=(from|by|via|with|for|id|;|\r?\n))");
            if (m.Success) traceInfo.From = m.Value;
            m = System.Text.RegularExpressions.Regex.Match(input, @"(?<=by ).+?(?= ?(from|by|via|with|for|id|;|\r?\n))");
            if (m.Success) traceInfo.By = m.Value;
            m = System.Text.RegularExpressions.Regex.Match(input, @"(?<=via ).+?(?= ?(from|by|via|with|for|id|;|\r?\n))");
            if (m.Success) traceInfo.Via = m.Value;
            m = System.Text.RegularExpressions.Regex.Match(input, @"(?<=with ).+?(?= ?(from|by|via|with|for|id|;|\r?\n))");
            if (m.Success) traceInfo.With = m.Value;
            m = System.Text.RegularExpressions.Regex.Match(input, @"(?<=for ).+?(?= ?(from|by|via|with|for|id|;|\r?\n))");
            if (m.Success) traceInfo.For = m.Value;
            m = System.Text.RegularExpressions.Regex.Match(input, @"(?<=id ).+?(?= ?(from|by|via|with|for|id|;|\r?\n))");
            if (m.Success) traceInfo.Id = m.Value;
            traceInfo.Date = Parser.ParseAsUniversalDateTime(input.Substring(input.LastIndexOf(';')+1));
            return traceInfo;
        }
        public static TraceInfoCollection ParseTraces(string[] input)
        {
            string itemlow;
            TraceInfoCollection traceinfos = new TraceInfoCollection();
            TraceInfo traceinfo = new TraceInfo();
            try
            {
                foreach (string item in input)
                {
                    traceinfo = new TraceInfo();
                    //item = " " + Parser.Clean(item1);
                    itemlow = item.ToLower();
                    if (itemlow.IndexOf(" from ") != -1) traceinfo.From = item.Substring(itemlow.IndexOf(" from ") + 6, item.IndexOf(" ", itemlow.IndexOf(" from ") + 6) - (itemlow.IndexOf(" from ") + 6)).TrimEnd(';');
                    if (itemlow.IndexOf(" by ") != -1) traceinfo.By = item.Substring(itemlow.IndexOf(" by ") + 4, item.IndexOf(" ", itemlow.IndexOf(" by ") + 4) - (itemlow.IndexOf(" by ") + 4)).TrimEnd(';');
                    if (itemlow.IndexOf(" for ") != -1) traceinfo.For = item.Substring(itemlow.IndexOf(" for ") + 5, item.IndexOf(" ", itemlow.IndexOf(" for ") + 5) - (itemlow.IndexOf(" for ") + 5)).TrimEnd(';');
                    if (itemlow.IndexOf(" id ") != -1) traceinfo.Id = item.Substring(itemlow.IndexOf(" id ") + 4, item.IndexOf(" ", itemlow.IndexOf(" id ") + 4) - (itemlow.IndexOf(" id ") + 4)).TrimEnd(';');
                    if (itemlow.IndexOf(" via ") != -1) traceinfo.Via = item.Substring(itemlow.IndexOf(" via ") + 5, item.IndexOf(" ", itemlow.IndexOf(" via ") + 5) - (itemlow.IndexOf(" via ") + 5)).TrimEnd(';');
                    if (itemlow.IndexOf(" with ") != -1) traceinfo.With = item.Substring(itemlow.IndexOf(" with ") + 6, item.IndexOf(" ", itemlow.IndexOf(" with ") + 6) - (itemlow.IndexOf(" with ") + 6)).TrimEnd(';');
                    traceinfo.Date = Parser.ParseAsUniversalDateTime(item.Split(';')[item.Split(';').Length - 1].Trim(' '));
                    traceinfos.Add(traceinfo);
                }
            }
            catch { };
            return traceinfos;
        }
        public static string Clean(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input,@"(\(((\\\))|[^)])*\))","").Trim(' ');
        }
    }
}