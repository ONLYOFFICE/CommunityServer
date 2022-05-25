namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a <placeholder>item placeholder</placeholder> in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="CheckType.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(CheckTypeId.Converter))]
    public sealed class CheckTypeId : ResourceIdentifier<CheckTypeId>
    {
        private static readonly CheckTypeId _remoteDns = new CheckTypeId("remote.dns");
        private static readonly CheckTypeId _remoteFtpBanner = new CheckTypeId("remote.ftp-banner");
        private static readonly CheckTypeId _remoteHttp = new CheckTypeId("remote.http");
        private static readonly CheckTypeId _remoteImapBanner = new CheckTypeId("remote.imap-banner");
        private static readonly CheckTypeId _remoteMssqlBanner = new CheckTypeId("remote.mssql-banner");
        private static readonly CheckTypeId _remoteMysqlBanner = new CheckTypeId("remote.mysql-banner");
        private static readonly CheckTypeId _remotePing = new CheckTypeId("remote.ping");
        private static readonly CheckTypeId _remotePop3Banner = new CheckTypeId("remote.pop3-banner");
        private static readonly CheckTypeId _remotePostgresqlBanner = new CheckTypeId("remote.postgresql-banner");
        private static readonly CheckTypeId _remoteSmtpBanner = new CheckTypeId("remote.smtp-banner");
        private static readonly CheckTypeId _remoteSmtp = new CheckTypeId("remote.smtp");
        private static readonly CheckTypeId _remoteSsh = new CheckTypeId("remote.ssh");
        private static readonly CheckTypeId _remoteTcp = new CheckTypeId("remote.tcp");
        private static readonly CheckTypeId _remoteTelnetBanner = new CheckTypeId("remote.telnet-banner");

        private static readonly CheckTypeId _agentFilesystem = new CheckTypeId("agent.filesystem");
        private static readonly CheckTypeId _agentMemory = new CheckTypeId("agent.memory");
        private static readonly CheckTypeId _agentLoadAverage = new CheckTypeId("agent.load_average");
        private static readonly CheckTypeId _agentCpu = new CheckTypeId("agent.cpu");
        private static readonly CheckTypeId _agentDisk = new CheckTypeId("agent.disk");
        private static readonly CheckTypeId _agentNetwork = new CheckTypeId("agent.network");
        private static readonly CheckTypeId _agentPlugin = new CheckTypeId("agent.plugin");

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTypeId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public CheckTypeId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote DNS check.
        /// </summary>
        /// <seealso cref="DnsCheckDetails"/>
        public static CheckTypeId RemoteDns
        {
            get
            {
                return _remoteDns;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote FTP banner check.
        /// </summary>
        /// <seealso cref="FtpBannerCheckDetails"/>
        public static CheckTypeId RemoteFtpBanner
        {
            get
            {
                return _remoteFtpBanner;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote HTTP check.
        /// </summary>
        /// <seealso cref="HttpCheckDetails"/>
        public static CheckTypeId RemoteHttp
        {
            get
            {
                return _remoteHttp;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote IMAP check.
        /// </summary>
        /// <seealso cref="ImapBannerCheckDetails"/>
        public static CheckTypeId RemoteImapBanner
        {
            get
            {
                return _remoteImapBanner;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote SQL Server check.
        /// </summary>
        /// <seealso cref="MssqlBannerCheckDetails"/>
        public static CheckTypeId RemoteMssqlBanner
        {
            get
            {
                return _remoteMssqlBanner;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote MySQL check.
        /// </summary>
        /// <seealso cref="MysqlBannerCheckDetails"/>
        public static CheckTypeId RemoteMysqlBanner
        {
            get
            {
                return _remoteMysqlBanner;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote PING check.
        /// </summary>
        /// <seealso cref="PingCheckDetails"/>
        public static CheckTypeId RemotePing
        {
            get
            {
                return _remotePing;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote POP3 check.
        /// </summary>
        /// <seealso cref="Pop3CheckDetails"/>
        public static CheckTypeId RemotePop3Banner
        {
            get
            {
                return _remotePop3Banner;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote PostgreSQL check.
        /// </summary>
        /// <seealso cref="PostgresqlBannerCheckDetails"/>
        public static CheckTypeId RemotePostgresqlBanner
        {
            get
            {
                return _remotePostgresqlBanner;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote SMTP banner check.
        /// </summary>
        /// <seealso cref="SmtpBannerCheckDetails"/>
        public static CheckTypeId RemoteSmtpBanner
        {
            get
            {
                return _remoteSmtpBanner;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote SMTP check.
        /// </summary>
        /// <seealso cref="SmtpCheckDetails"/>
        public static CheckTypeId RemoteSmtp
        {
            get
            {
                return _remoteSmtp;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote SSH check.
        /// </summary>
        /// <seealso cref="SshCheckDetails"/>
        public static CheckTypeId RemoteSsh
        {
            get
            {
                return _remoteSsh;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote TCP check.
        /// </summary>
        /// <seealso cref="TcpCheckDetails"/>
        public static CheckTypeId RemoteTcp
        {
            get
            {
                return _remoteTcp;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing a remote telnet banner check.
        /// </summary>
        /// <seealso cref="TelnetBannerCheckDetails"/>
        public static CheckTypeId RemoteTelnetBanner
        {
            get
            {
                return _remoteTelnetBanner;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing an agent filesystem check.
        /// </summary>
        /// <seealso cref="FilesystemCheckDetails"/>
        public static CheckTypeId AgentFilesystem
        {
            get
            {
                return _agentFilesystem;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing an agent memory check.
        /// </summary>
        /// <seealso cref="MemoryCheckDetails"/>
        public static CheckTypeId AgentMemory
        {
            get
            {
                return _agentMemory;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing an agent load average check.
        /// </summary>
        /// <seealso cref="LoadAverageCheckDetails"/>
        public static CheckTypeId AgentLoadAverage
        {
            get
            {
                return _agentLoadAverage;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing an agent CPU check.
        /// </summary>
        /// <seealso cref="CpuCheckDetails"/>
        public static CheckTypeId AgentCpu
        {
            get
            {
                return _agentCpu;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing an agent disk check.
        /// </summary>
        /// <seealso cref="DiskCheckDetails"/>
        public static CheckTypeId AgentDisk
        {
            get
            {
                return _agentDisk;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing an agent network check.
        /// </summary>
        /// <seealso cref="NetworkCheckDetails"/>
        public static CheckTypeId AgentNetwork
        {
            get
            {
                return _agentNetwork;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeId"/> representing an agent plug-in check.
        /// </summary>
        /// <seealso cref="PluginCheckDetails"/>
        public static CheckTypeId AgentPlugin
        {
            get
            {
                return _agentPlugin;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this check type identifies an agent check.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this check type identifies an agent check type; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsAgent
        {
            get
            {
                return Value.StartsWith("agent.", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this check type identifies a remote check.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this check type identifies a remote check type; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsRemote
        {
            get
            {
                return Value.StartsWith("remote.", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="CheckTypeId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override CheckTypeId FromValue(string id)
            {
                return new CheckTypeId(id);
            }
        }
    }
}
