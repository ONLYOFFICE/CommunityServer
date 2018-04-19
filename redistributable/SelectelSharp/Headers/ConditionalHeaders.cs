using System;
using System.Collections.Generic;

namespace SelectelSharp.Headers
{
    public class ConditionalHeaders
    {
        /// <summary>
        /// Succeeds if the ETag of the distant resource is equal to one listed in this header.
        /// </summary>
        public string IfMatch { get; set; }

        /// <summary>
        /// Succeeds if the ETag of the distant resource is different to each listed in this header.
        /// </summary>
        public string IfNoneMatch { get; set; }

        /// <summary>
        /// Succeeds if the Last-Modified date of the distant resource is more recent than the one given in this header.
        /// </summary>
        public DateTime? IfModifiedSince { get; set; }

        /// <summary>
        /// Succeeds if the Last-Modified date of the distant resource is older or the same than the one given in this header.
        /// </summary>
        public DateTime? IfUnmodifiedSince { get; set; }        

        public ConditionalHeaders(
            string ifMatch = null,
            string ifNoneMatch = null,
            DateTime? ifModifiedSince = null,
            DateTime? ifUnmodifiedSince = null)
        {
            this.IfMatch = ifMatch;
            this.IfNoneMatch = ifNoneMatch;
            this.IfModifiedSince = ifModifiedSince;
            this.IfUnmodifiedSince = ifUnmodifiedSince;
        }

        public IDictionary<string, object> GetHeaders()
        {
            var result = new Dictionary<string, object>();

            TryAddHeader(result, HeaderKeys.IfMatch, IfMatch);
            TryAddHeader(result, HeaderKeys.IfNoneMatch, IfNoneMatch);

            if (IfModifiedSince.HasValue)
            {
                TryAddHeader(result, HeaderKeys.IfModifiedSince, IfModifiedSince.Value.ToString(HeaderKeys.DateFormat));
            }

            if (IfUnmodifiedSince.HasValue)
            {
                TryAddHeader(result, HeaderKeys.IfUnmodifiedSince, IfUnmodifiedSince.Value.ToString(HeaderKeys.DateFormat));
            }

            return result;
        }

        private void TryAddHeader(Dictionary<string, object> headers, string header, object value)
        {
            if (value != null)
            {
                headers.Add(header, value.ToString());
            }
        }
    }
}
