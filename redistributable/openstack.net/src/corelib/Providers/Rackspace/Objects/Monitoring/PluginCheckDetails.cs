namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.AgentPlugin"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.AgentPlugin"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class PluginCheckDetails : CheckDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="File"/> property.
        /// </summary>
        [JsonProperty("file")]
        private string _file;

        /// <summary>
        /// This is the backing field for the <see cref="Arguments"/> property.
        /// </summary>
        [JsonProperty("args", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string[] _arguments;

        /// <summary>
        /// This is the backing field for the <see cref="Timeout"/> property.
        /// </summary>
        [JsonProperty("timeout", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected PluginCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginCheckDetails"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="file">The name of the plugin file.</param>
        /// <param name="arguments">A collection of command line arguments which are passed to the plugin. If this argument is <see langword="null"/>, no command line arguments are passed to the plugin.</param>
        /// <param name="timeout">The plugin execution timeout. If this value is <see langword="null"/>, the plugin execution timeout is unspecified.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="file"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="file"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="arguments"/> contains any <see langword="null"/> or empty values.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="timeout"/> is less than <see cref="TimeSpan.Zero"/>.</exception>
        public PluginCheckDetails(string file, string[] arguments, TimeSpan? timeout)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (arguments != null && arguments.Any(string.IsNullOrEmpty))
                throw new ArgumentException("arguments cannot contain any null or empty values", "arguments");
            if (timeout < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("timeout cannot be negative");

            _file = file;
            _arguments = (string[])arguments.Clone();
            _timeout = timeout != null ? (int?)timeout.Value.TotalMilliseconds : null;
        }

        /// <summary>
        /// Gets the name of the plugin file.
        /// </summary>
        public string File
        {
            get
            {
                return _file;
            }
        }

        /// <summary>
        /// Gets a collection of command line arguments passed to the plugin.
        /// </summary>
        public ReadOnlyCollection<string> Arguments
        {
            get
            {
                if (_arguments == null)
                    return null;

                return new ReadOnlyCollection<string>(_arguments);
            }
        }

        /// <summary>
        /// Gets the plugin execution timeout.
        /// </summary>
        public TimeSpan? Timeout
        {
            get
            {
                if (_timeout == null)
                    return null;

                return TimeSpan.FromMilliseconds(_timeout.Value);
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.AgentPlugin"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.AgentPlugin;
        }
    }
}
