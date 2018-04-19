using SelectelSharp.Headers;
using SelectelSharp.Models.File;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace SelectelSharp.Requests.File
{
    public class UploadArchiveRequest : BaseRequest<UploadArchiveResult>
    {
        private string Path { get; set; }

        public UploadArchiveRequest(
            bool AutoCloseStream = true,
            bool AutoResetStreamPosition = true,
            Stream inputStream = null,     
            FileArchiveFormat archiveFormat = FileArchiveFormat.TarGz,
            string path = null) : base()
        {
            SetArchiveFormat(archiveFormat);
            TryAddHeader(HeaderKeys.Accept, HeaderKeys.AcceptJson);

            this.ContentStream = inputStream;
            this.AutoCloseStream = AutoCloseStream;
            this.AutoResetStreamPosition = AutoResetStreamPosition;            
        }

        private void SetArchiveFormat(FileArchiveFormat archiveFormat)
        {
            string format;
            switch (archiveFormat)
            {
                case FileArchiveFormat.Tar:
                    format = "tar";
                    break;
                case FileArchiveFormat.TarGz:
                    format = "tar.gz";
                    break;
                case FileArchiveFormat.TarBz2:
                    format = "tar.bz2";
                    break;
                default:
                    throw new ArgumentException("Not supported archive format.");
            }

            TryAddQueryParam("extract-archive", format);
        }

        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.PUT;
            }
        }        

        internal override void Parse(NameValueCollection headers, object data, HttpStatusCode status)
        {
            if (status == HttpStatusCode.Created)
            {
                base.Parse(headers, data, status);
            }
            else
            {
                ParseError(null, status);
            }
        }

        protected override string GetUrl(string storageUrl)
        {
            var url = storageUrl;
            if (this.Path != null) {
                url = string.Concat(url, this.Path);
            }

            return url;
        }
    }
}
