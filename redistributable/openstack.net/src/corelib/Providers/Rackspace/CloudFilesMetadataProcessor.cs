using System;
using System.Collections.Generic;
using System.Text;
using JSIStudios.SimpleRESTServices.Client;
using net.openstack.Core;

namespace net.openstack.Providers.Rackspace
{
    internal class CloudFilesMetadataProcessor : IObjectStorageMetadataProcessor
    {
        /// <summary>
        /// A default instance of <see cref="CloudFilesMetadataProcessor"/>.
        /// </summary>
        private static readonly CloudFilesMetadataProcessor _default = new CloudFilesMetadataProcessor();

        /// <summary>
        /// Gets a default instance of <see cref="CloudFilesMetadataProcessor"/>.
        /// </summary>
        public static CloudFilesMetadataProcessor Default
        {
            get
            {
                return _default;
            }
        }

        /// <summary>
        /// Extracts metadata information from a collection of HTTP headers.
        /// </summary>
        /// <remarks>
        /// The returned collection has two keys: <see cref="CloudFilesProvider.ProcessedHeadersHeaderKey"/>
        /// and <see cref="CloudFilesProvider.ProcessedHeadersMetadataKey"/>.
        ///
        /// <para>The value for
        /// <see cref="CloudFilesProvider.ProcessedHeadersMetadataKey"/> contains the processed Cloud Files
        /// metadata included in HTTP headers such as <strong>X-Account-Meta-*</strong>,
        /// <strong>X-Container-Meta-*</strong>, and <strong>X-Object-Meta-*</strong>. The metadata prefix
        /// has been removed from the keys stored in this value.</para>
        ///
        /// <para>The value for <see cref="CloudFilesProvider.ProcessedHeadersHeaderKey"/> contains the
        /// HTTP headers which were not in the form of a known Cloud Files metadata prefix.</para>
        /// </remarks>
        /// <inheritdoc/>
        public virtual Dictionary<string, Dictionary<string, string>> ProcessMetadata(IList<HttpHeader> httpHeaders)
        {
            if (httpHeaders == null)
                throw new ArgumentNullException("httpHeaders");

            var pheaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in httpHeaders)
            {
                if (header == null)
                    throw new ArgumentException("httpHeaders cannot contain any null values");
                if (string.IsNullOrEmpty(header.Key))
                    throw new ArgumentException("httpHeaders cannot contain any values with a null or empty key");

                if (header.Key.StartsWith(CloudFilesProvider.AccountMetaDataPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    metadata.Add(header.Key.Substring(CloudFilesProvider.AccountMetaDataPrefix.Length), DecodeUnicodeValue(header.Value));
                }
                else if (header.Key.StartsWith(CloudFilesProvider.ContainerMetaDataPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    metadata.Add(header.Key.Substring(CloudFilesProvider.ContainerMetaDataPrefix.Length), DecodeUnicodeValue(header.Value));
                }
                else if (header.Key.StartsWith(CloudFilesProvider.ObjectMetaDataPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    metadata.Add(header.Key.Substring(CloudFilesProvider.ObjectMetaDataPrefix.Length), DecodeUnicodeValue(header.Value));
                }
                else
                {
                    pheaders.Add(header.Key, header.Value);
                }
            }

            var processedHeaders = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
                {
                    {CloudFilesProvider.ProcessedHeadersHeaderKey, pheaders},
                    {CloudFilesProvider.ProcessedHeadersMetadataKey, metadata}
                };

            return processedHeaders;
        }

        private string DecodeUnicodeValue(string value)
        {
            return Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(value));
        }
    }
}
