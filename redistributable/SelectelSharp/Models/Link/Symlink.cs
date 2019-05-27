using SelectelSharp.Common;
using SelectelSharp.Headers;
using System;
using System.Collections.Generic;

namespace SelectelSharp.Models.Link
{
    public class Symlink
    {
        public string ContentType { get; private set; }
        public long? DeleteAt { get; private set; }
        public string Key { get; private set; }
        public string ObjectLocation { get; private set; }
        public string Link { get; private set; }
        public string ContentDisposition { get; private set; }

        public Symlink(string link, SymlinkType type, string objectLocaton, DateTime? deleteAt = null, string password = null, string contentDisposition = null)
        {
            if (string.IsNullOrEmpty(link))
                throw new ArgumentNullException("Link is null");

            if (objectLocaton == null)
                throw new ArgumentNullException("Object location is empty");

            if (string.IsNullOrWhiteSpace(objectLocaton ))
                throw new ArgumentOutOfRangeException("Object location is empty");

            this.Link = link;
            this.ContentType = typeToString[type];
            this.ContentDisposition = contentDisposition;
            this.ObjectLocation = Uri.EscapeUriString(objectLocaton);

            if (!String.IsNullOrEmpty(password))
            {
                this.Key = Helpers.CalculateSHA1(string.Concat(password, this.ObjectLocation));
            }

            if (deleteAt.HasValue)
            {
                this.DeleteAt = Helpers.DateToUnixTimestamp(deleteAt.Value);
            }            
        }

        public IDictionary<string, object> GetHeaders()
        {
            var result = new Dictionary<string, object>();

            TryAddHeader(result, HeaderKeys.ContentType, this.ContentType);
            TryAddHeader(result, HeaderKeys.ContentLenght, 0);
            TryAddHeader(result, HeaderKeys.ContentDisposition, this.ContentDisposition);
            TryAddHeader(result, HeaderKeys.XObjectMetaLocation, this.ObjectLocation);
            TryAddHeader(result, HeaderKeys.XObjectMetaDeleteAt, this.DeleteAt);
            TryAddHeader(result, HeaderKeys.XObjectMetaLinkKey, this.Key);

            return result;
        }
        
        private void TryAddHeader(Dictionary<string, object> headers, string header, object value)
        {
            if (value != null)
            {
                headers.Add(header, value.ToString());
            }
        }

        private static Dictionary<SymlinkType, string> typeToString = new Dictionary<SymlinkType, string>
        {
            { SymlinkType.Symlink, "x-storage/symlink" },
            { SymlinkType.OnetimeSymlink, "x-storage/onetime-symlink" },
            { SymlinkType.SymlinkSecure, "x-storage/symlink+secure" },
            { SymlinkType.OnetimeSymlinkSecure, "x-storage/onetime-symlink+secure" },
        };

        public enum SymlinkType
        {
            /// <summary>
            /// обычная ссылка
            /// </summary>
            Symlink,

            /// <summary>
            /// Одноразовая ссылка
            /// </summary>
            OnetimeSymlink,

            /// <summary>
            /// Обычная запароленная ссылка
            /// </summary>
            SymlinkSecure,

            /// <summary>
            /// Одноразовая запароленная ссылка
            /// </summary>
            OnetimeSymlinkSecure
        }
    }
}
