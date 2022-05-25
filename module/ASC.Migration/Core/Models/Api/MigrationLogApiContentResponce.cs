using System.IO;
using System.Net.Mime;
using System.Text;

using ASC.Api.Interfaces.ResponseTypes;

namespace ASC.Migration.Core.Models.Api
{
    public class MigrationLogApiContentResponce : IApiContentResponce
    {
        private readonly Stream _stream;
        private readonly string _fileName;


        public MigrationLogApiContentResponce(Stream stream, string fileName)
        {
            _stream = stream;
            _fileName = fileName;
        }

        public Encoding ContentEncoding
        {
            get { return Encoding.UTF8; }
        }

        public Stream ContentStream
        {
            get { return _stream; }
        }

        public ContentType ContentType
        {
            get { return new ContentType("text/plain; charset=UTF-8"); }
        }

        public ContentDisposition ContentDisposition
        {
            get { return new ContentDisposition { Inline = false, FileName = _fileName }; }
        }
    }
}
