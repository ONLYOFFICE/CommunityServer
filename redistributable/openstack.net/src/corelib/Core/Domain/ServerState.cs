namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the state of a compute server.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known server states,
    /// with added support for unknown states returned by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverter(typeof(ServerState.Converter))]
    public sealed class ServerState : ExtensibleEnum<ServerState>
    {
        private static readonly ConcurrentDictionary<string, ServerState> _states =
            new ConcurrentDictionary<string, ServerState>(StringComparer.OrdinalIgnoreCase);
        private static readonly ServerState _active = FromName("ACTIVE");
        private static readonly ServerState _build = FromName("BUILD");
        private static readonly ServerState _deleted = FromName("DELETED");
        private static readonly ServerState _error = FromName("ERROR");
        private static readonly ServerState _hardReboot = FromName("HARD_REBOOT");
        private static readonly ServerState _migrating = FromName("MIGRATING");
        private static readonly ServerState _password = FromName("PASSWORD");
        private static readonly ServerState _reboot = FromName("REBOOT");
        private static readonly ServerState _rebuild = FromName("REBUILD");
        private static readonly ServerState _rescue = FromName("RESCUE");
        private static readonly ServerState _resize = FromName("RESIZE");
        private static readonly ServerState _revertResize = FromName("REVERT_RESIZE");
        private static readonly ServerState _suspended = FromName("SUSPENDED");
        private static readonly ServerState _unknown = FromName("UNKNOWN");
        private static readonly ServerState _verifyResize = FromName("VERIFY_RESIZE");
        private static readonly ServerState _prepRescue = FromName("PREP_RESCUE");
        private static readonly ServerState _prepUnrescue = FromName("PREP_UNRESCUE");

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerState"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private ServerState(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="ServerState"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="ServerState"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static ServerState FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _states.GetOrAdd(name, i => new ServerState(i));
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server which is active and ready to use.
        /// </summary>
        public static ServerState Active
        {
            get
            {
                return _active;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server which is currently being built.
        /// </summary>
        public static ServerState Build
        {
            get
            {
                return _build;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server which has been deleted.
        /// </summary>
        /// <remarks>
        /// By default, the <see cref="IComputeProvider.ListServers"/> operation does not return
        /// servers which have been deleted. To list deleted servers, call
        /// <see cref="IComputeProvider.ListServers"/> specifying the <c>changesSince</c>
        /// parameter.
        /// </remarks>
        public static ServerState Deleted
        {
            get
            {
                return _deleted;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server which failed to perform
        /// an operation and is now in an error state.
        /// </summary>
        public static ServerState Error
        {
            get
            {
                return _error;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server currently performing a hard
        /// reboot. When the reboot operation completes, the server will be in the <see cref="Active"/>
        /// state.
        /// </summary>
        /// <seealso cref="ServerBase.HardReboot"/>
        /// <seealso cref="IComputeProvider.RebootServer"/>
        public static ServerState HardReboot
        {
            get
            {
                return _hardReboot;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server which is currently being moved
        /// from one physical node to another.
        /// </summary>
        /// <remarks>
        /// <note>Server migration is a Rackspace-specific extension.</note>
        /// </remarks>
        public static ServerState Migrating
        {
            get
            {
                return _migrating;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing the password for the server is being changed.
        /// </summary>
        /// <seealso cref="IComputeProvider.ChangeAdministratorPassword"/>
        public static ServerState Password
        {
            get
            {
                return _password;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server currently performing a soft
        /// reboot. When the reboot operation completes, the server will be in the <see cref="Active"/>
        /// state.
        /// </summary>
        /// <seealso cref="ServerBase.SoftReboot"/>
        /// <seealso cref="IComputeProvider.RebootServer"/>
        public static ServerState Reboot
        {
            get
            {
                return _reboot;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server currently being rebuilt.
        /// When the rebuild operation completes, the server will be in the <see cref="Active"/>
        /// state if the rebuild was successful; otherwise, it will be in the <see cref="Error"/> state
        /// if the rebuild operation failed.
        /// </summary>
        /// <seealso cref="ServerBase.Rebuild"/>
        /// <seealso cref="IComputeProvider.RebuildServer"/>
        public static ServerState Rebuild
        {
            get
            {
                return _rebuild;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server currently in rescue mode.
        /// </summary>
        /// <seealso cref="ServerBase.Rescue"/>
        /// <seealso cref="IComputeProvider.RescueServer"/>
        public static ServerState Rescue
        {
            get
            {
                return _rescue;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server currently executing a resize action.
        /// When the resize operation completes, the server will be in the <see cref="VerifyResize"/>
        /// state if the resize was successful; otherwise, it will be in the <see cref="Active"/> state
        /// if the resize operation failed.
        /// </summary>
        /// <seealso cref="ServerBase.Resize"/>
        /// <seealso cref="IComputeProvider.ResizeServer"/>
        public static ServerState Resize
        {
            get
            {
                return _resize;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server currently executing a revert resize action.
        /// </summary>
        /// <seealso cref="ServerBase.RevertResize"/>
        /// <seealso cref="IComputeProvider.RevertServerResize"/>
        public static ServerState RevertResize
        {
            get
            {
                return _revertResize;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> for a server that is currently inactive, either by request or necessity.
        /// </summary>
        public static ServerState Suspended
        {
            get
            {
                return _suspended;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> for a server that is currently in an unknown state.
        /// </summary>
        public static ServerState Unknown
        {
            get
            {
                return _unknown;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server which completed a resize operation
        /// and is now waiting for the operation to be confirmed or reverted.
        /// </summary>
        /// <seealso cref="ServerBase.Resize"/>
        /// <seealso cref="IComputeProvider.ResizeServer"/>
        /// <seealso cref="ServerBase.ConfirmResize"/>
        /// <seealso cref="IComputeProvider.ConfirmServerResize"/>
        public static ServerState VerifyResize
        {
            get
            {
                return _verifyResize;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server currently executing a rescue action.
        /// </summary>
        /// <seealso cref="ServerBase.Rescue"/>
        /// <seealso cref="IComputeProvider.RescueServer"/>
        public static ServerState PrepRescue
        {
            get
            {
                return _prepRescue;
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerState"/> representing a server currently executing an un-rescue action.
        /// </summary>
        /// <seealso cref="ServerBase.UnRescue"/>
        /// <seealso cref="IComputeProvider.UnRescueServer"/>
        public static ServerState PrepUnrescue
        {
            get
            {
                return _prepUnrescue;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="ServerState"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override ServerState FromName(string name)
            {
                return ServerState.FromName(name);
            }
        }
    }
}
