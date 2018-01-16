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
using System.Collections.Specialized;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace ASC.Api.Batch
{
    public class ApiWorkerRequest:SimpleWorkerRequest
    {
        private readonly TextWriter _output;
        private readonly HttpContextBase _context;
        private readonly ContentType _contentType;

        public string HttpVerb { get; set; }

        public byte[] EntityBody { get; set; }

        public int HttpStatus { get; private set; }
        public string HttpStatusDescription { get;private set; }

        public NameValueCollection ResponseHeaders { get; private set; }

        public ApiWorkerRequest(string lookupDir, string page, string query, TextWriter output, HttpContextBase context, ContentType contentType):base("/",lookupDir, page,query,output)
        {
            if (context == null) throw new ArgumentNullException("context");
            _output = output;
            _context = context;
            _contentType = contentType;
            ResponseHeaders = new NameValueCollection();
        }

        public ApiWorkerRequest(string page, string query, TextWriter output, HttpContextBase context, ContentType contentType):base(page,query,output)
        {
            if (context == null) throw new ArgumentNullException("context");
            _output = output;
            _context = context;
            _contentType = contentType;
            ResponseHeaders = new NameValueCollection();
        }

        public override string GetServerVariable(string name)
        {
            if (!string.IsNullOrEmpty(name))
                return _context.Request.ServerVariables[name]??string.Empty;
            return string.Empty;
        }

        public override string GetHttpVerbName()
        {
            return HttpVerb;
        }


        public override string GetRemoteAddress()
        {
            return _context.Request.UserHostAddress;
        }

        public override string MapPath(string path)
        {
            return _context.Server.MapPath(path);
        }

        public override byte[] GetPreloadedEntityBody()
        {
            return EntityBody;
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            if (EntityBody!=null)
            {
                var resize = size <= EntityBody.Length ? size : EntityBody.Length;
                Buffer.BlockCopy(EntityBody,0,buffer,0,resize);
                return resize;
            }
            return base.ReadEntityBody(buffer, size);
        }

        public override long GetBytesRead()
        {
            if (EntityBody != null)
                return EntityBody.Length;
            return base.GetBytesRead();
        }


        public override int GetPreloadedEntityBodyLength()
        {
            if (EntityBody != null)
                return EntityBody.Length;
            return base.GetPreloadedEntityBodyLength();
        }

        public override string GetKnownRequestHeader(int index)
        {
            if (index==HeaderContentType && _contentType!=null)
            {
                return _contentType.ToString();
            }
            if (index==HeaderContentLength && EntityBody!=null)
            {
                return EntityBody.Length.ToString();
            }
            return _context.Request.Headers[(GetKnownRequestHeaderName(index))];
        }

        public override string GetUnknownRequestHeader(string name)
        {
            return _context.Request.Headers[name];
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            if (value == null)
            {
                ResponseHeaders.Remove(GetKnownResponseHeaderName(index));
            }
            else
            {
                ResponseHeaders.Add(GetKnownResponseHeaderName(index), value);
            }
            base.SendKnownResponseHeader(index, value);
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            if (name != null)
            {
                if (value == null)
                {
                    ResponseHeaders.Remove(name);
                }
                else
                {
                    ResponseHeaders.Add(name, value);
                }
            }
            base.SendUnknownResponseHeader(name, value);
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
            SendKnownResponseHeader(HeaderContentLength,contentLength.ToString());
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            HttpStatus = statusCode;
            HttpStatusDescription = statusDescription;
            base.SendStatus(statusCode, statusDescription);
        }

        public override bool IsEntireEntityBodyIsPreloaded()
        {
            if (EntityBody != null)
                return true;
            return base.IsEntireEntityBodyIsPreloaded();
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            //Get encoding
            Encoding encoding = Encoding.UTF8;
            var contentEncoding = ResponseHeaders[GetKnownResponseHeaderName(HeaderContentEncoding)];
            if (!string.IsNullOrEmpty(contentEncoding))
            {
                encoding = Encoding.GetEncoding(contentEncoding);
            }
            else
            {
                var contentType = ResponseHeaders[GetKnownResponseHeaderName(HeaderContentType)];
                if (!string.IsNullOrEmpty(contentType))
                {
                    var ctypeCharset = new ContentType(contentType).CharSet;
                    if (!string.IsNullOrEmpty(ctypeCharset))
                        encoding = Encoding.GetEncoding(ctypeCharset);
                }
            }
            _output.Write(encoding.GetChars(data, 0, length));
        }
    }


}