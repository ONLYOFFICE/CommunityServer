namespace net.openstack.Core.Domain
{
    using System;
    using System.Text;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes a file to inject into the file system while creating or
    /// rebuilding a server.
    /// </summary>
    /// <remarks>
    /// You can customize the personality of a server instance by injecting data into
    /// its file system. For example, you might want to insert SSH keys, set configuration
    /// files, or store data that you want to retrieve from inside the instance. This
    /// feature provides a minimal amount of launch-time personalization. If you require
    /// significant customization, create a custom image.
    /// </remarks>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Server_Personality-d1e2543.html">Server Personality (OpenStack Compute API V2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Personality : ExtensibleJsonObject
    {
        /// <summary>
        /// The path of the file to create on the target file system.
        /// </summary>
        /// <remarks>
        /// The behavior of the related <see cref="IComputeProvider"/> methods is undefined
        /// if the UTF-8 encoded value is longer than 255 bytes.
        /// </remarks>
        [JsonProperty("path")]
        public string Path { get; private set; }

        /// <summary>
        /// The contents of the file to create on the target file system.
        /// </summary>
        /// <remarks>
        /// The maximum size of the file contents is determined by the compute provider
        /// and may vary based on the image that is used to create the server. The provider
        /// may provide a <c>maxPersonalitySize</c> absolute limit, which is a byte limit
        /// that is guaranteed to apply to all images in the deployment. Providers can set
        /// additional per-image personality limits.
        ///
        /// <note type="warning">
        /// The behavior of the related <see cref="IComputeProvider"/> methods is undefined
        /// if the value is not a UTF-8 encoded text file.
        /// </note>
        /// </remarks>
        [JsonProperty("contents")]
        public byte[] Content { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Personality"/> class during
        /// JSON deserialization.
        /// </summary>
        [JsonConstructor]
        private Personality()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Personality"/> class with the specified
        /// path and contents.
        /// </summary>
        /// <param name="path">The path of the file to create on the target file system.</param>
        /// <param name="content">The contents of the file to create on the target file system.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="path"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="content"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="path"/> is empty.</exception>
        public Personality(string path, byte[] content)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (content == null)
                throw new ArgumentNullException("content");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path cannot be empty");

            Path = path;
            Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Personality"/> class with the specified
        /// path and text content, using <see cref="Encoding.UTF8"/> for the content encoding.
        /// </summary>
        /// <param name="path">The path of the text file to create on the target file system.</param>
        /// <param name="content">The contents of the text file to create on the target file system.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="path"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="content"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="path"/> is empty.</exception>
        public Personality(string path, string content)
            : this(path, content, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Personality"/> class with the specified
        /// path, text content, and context encoding.
        /// </summary>
        /// <param name="path">The path of the text file to create on the target file system.</param>
        /// <param name="content">The contents of the text file to create on the target file system.</param>
        /// <param name="encoding">The encoding to use for the text file.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="path"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="content"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="encoding"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="path"/> is empty.</exception>
        public Personality(string path, string content, Encoding encoding)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (content == null)
                throw new ArgumentNullException("content");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path cannot be empty");

            Path = path;
            Content = encoding.GetBytes(content);
        }
    }
}
