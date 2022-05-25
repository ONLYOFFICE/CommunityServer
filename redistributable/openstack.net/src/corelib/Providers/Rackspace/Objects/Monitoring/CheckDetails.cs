namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This is the base class for classes modeling the detailed configuration parameters
    /// of various types of checks.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class CheckDetails : ExtensibleJsonObject
    {
        /// <summary>
        /// Provides factory methods for deserializing a <see cref="CheckDetails"/> instance from
        /// a <see cref="JObject"/> according to the <see cref="CheckTypeId"/> of the associated
        /// check.
        /// </summary>
        private static readonly Dictionary<CheckTypeId, Func<JObject, CheckDetails>> DetailsFactories =
            new Dictionary<CheckTypeId, Func<JObject, CheckDetails>>
            {
                // remote checks
                { CheckTypeId.RemoteDns, obj => obj.ToObject<DnsCheckDetails>() },
                { CheckTypeId.RemoteFtpBanner, obj => obj.ToObject<FtpBannerCheckDetails>() },
                { CheckTypeId.RemoteHttp, obj => obj.ToObject<HttpCheckDetails>() },
                { CheckTypeId.RemoteImapBanner, obj => obj.ToObject<ImapBannerCheckDetails>() },
                { CheckTypeId.RemoteMssqlBanner, obj => obj.ToObject<MssqlBannerCheckDetails>() },
                { CheckTypeId.RemoteMysqlBanner, obj => obj.ToObject<MysqlBannerCheckDetails>() },
                { CheckTypeId.RemotePing, obj => obj.ToObject<PingCheckDetails>() },
                { CheckTypeId.RemotePop3Banner, obj => obj.ToObject<Pop3CheckDetails>() },
                { CheckTypeId.RemotePostgresqlBanner, obj => obj.ToObject<PostgresqlBannerCheckDetails>() },
                { CheckTypeId.RemoteSmtpBanner, obj => obj.ToObject<SmtpBannerCheckDetails>() },
                { CheckTypeId.RemoteSmtp, obj => obj.ToObject<SmtpCheckDetails>() },
                { CheckTypeId.RemoteSsh, obj => obj.ToObject<SshCheckDetails>() },
                { CheckTypeId.RemoteTcp, obj => obj.ToObject<TcpCheckDetails>() },
                { CheckTypeId.RemoteTelnetBanner, obj => obj.ToObject<TelnetBannerCheckDetails>() },

                // agent checks
                { CheckTypeId.AgentFilesystem, obj => obj.ToObject<FilesystemCheckDetails>() },
                { CheckTypeId.AgentMemory, obj => obj.ToObject<MemoryCheckDetails>() },
                { CheckTypeId.AgentLoadAverage, obj => obj.ToObject<LoadAverageCheckDetails>() },
                { CheckTypeId.AgentCpu, obj => obj.ToObject<CpuCheckDetails>() },
                { CheckTypeId.AgentDisk, obj => obj.ToObject<DiskCheckDetails>() },
                { CheckTypeId.AgentNetwork, obj => obj.ToObject<NetworkCheckDetails>() },
                { CheckTypeId.AgentPlugin, obj => obj.ToObject<PluginCheckDetails>() },
            };

        /// <summary>
        /// Deserializes a JSON object to a <see cref="CheckDetails"/> instance of the proper type.
        /// </summary>
        /// <param name="checkTypeId">The check type ID.</param>
        /// <param name="obj">The JSON object representing the check details.</param>
        /// <returns>A <see cref="CheckDetails"/> object corresponding to the JSON object.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="checkTypeId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="obj"/> is <see langword="null"/>.</para>
        /// </exception>
        public static CheckDetails FromJObject(CheckTypeId checkTypeId, JObject obj)
        {
            if (checkTypeId == null)
                throw new ArgumentNullException("checkTypeId");
            if (obj == null)
                throw new ArgumentNullException("obj");

            Func<JObject, CheckDetails> factory;
            if (DetailsFactories.TryGetValue(checkTypeId, out factory))
                return factory(obj);

            return obj.ToObject<GenericCheckDetails>();
        }

        /// <summary>
        /// Determines whether the current <see cref="CheckDetails"/> object is compatible
        /// with checks of a particular type.
        /// </summary>
        /// <param name="checkTypeId">The check type ID.</param>
        /// <returns><see langword="true"/> if the current <see cref="CheckDetails"/> object is compatible with <paramref name="checkTypeId"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="checkTypeId"/> is <see langword="null"/>.</exception>
        protected internal abstract bool SupportsCheckType(CheckTypeId checkTypeId);
    }
}
