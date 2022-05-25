using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Extensions;
using System.Net.Http.Headers;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using OpenStack.Authentication;
using OpenStack.Serialization;

namespace OpenStack
{
    /// <summary>
    /// A static container for global configuration settings affecting OpenStack.NET behavior.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public static class OpenStackNet
    {
        /// <summary>
        /// Global configuration which affects OpenStack.NET's behavior.
        /// <para>Customize using the <see cref="Configuring"/> event.</para>
        /// </summary>
        public static OpenStackNetConfigurationOptions Configuration
        {
            get
            {
                if (!_isConfigured)
                    Configure();
                return _config;
            }
        }

        private static OpenStackNetConfigurationOptions _config;
        private static readonly object ConfigureLock = new object();
        private static bool _isConfigured;

        /// <summary>
        /// Occurs when initializing the global configuration for OpenStack.NET.
        /// </summary>
        public static event Action<OpenStackNetConfigurationOptions> Configuring;

        /// <summary>
        /// <para>DEPRECATED, use the <see cref="Configuring"/> event instead.</para>
        /// <para>Provides thread-safe accesss to OpenStack.NET's global configuration options.</para>
        /// <para>Can only be called once at application start-up, before instantiating any OpenStack.NET objects.</para>
        /// </summary>
        /// <param name="configureFlurl">Addtional configuration of the OpenStack.NET Flurl client settings <seealso cref="Flurl.Http.FlurlHttp.Configure" />.</param>
        /// <param name="configure">Additional configuration of OpenStack.NET's global settings.</param>
        [Obsolete("This will be removed in v2.0. Use the OpenStackNet.Configuring event instead.")]
        public static void Configure(Action<FlurlHttpSettings> configureFlurl = null, Action<OpenStackNetConfigurationOptions> configure = null)
        {
            lock (ConfigureLock)
            {
                if (_isConfigured)
                {
                    // Check if a user is attempting to apply custom configuration after the default config has been applied
                    if(configureFlurl != null || configure != null)
                        Trace.TraceError("Ignoring additional call to OpenStackNet.Configure. It can only be called once at application start-up, before instantiating any OpenStack.NET objects.");

                    return;
                }

                // Give the application an opportunity to tweak the default config
                _config = OpenStackNetConfigurationOptions.Create();
                Configuring?.Invoke(_config);
                
                // Apply legacy custom configuration, removed in 2.0 as it's replaced by the Configuring event
                configureFlurl?.Invoke(_config.FlurlHttpSettings);

                // Finish configuration and lock it
                _config.CompleteInitialization();

                _isConfigured = true;
            }
        }

        /// <summary>
        /// <par>Resets all configuration (OpenStack.NET, Flurl and Json.NET).</par>
        /// <para>After this is called, you must re-register any <see cref="Configuring"/> event handlers.</para>
        /// </summary>
        public static void ResetDefaults()
        {
            lock (ConfigureLock)
            {
                _config = null;
                Configuring = null;
                _isConfigured = false;
            }
        }

        /// <summary>
        /// Deserializes an object from a json string representation.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="json">The json string.</param>
        public static T Deserialize<T>(string json)
        {
            return Configuration.FlurlHttpSettings.JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Serializes an object to a json string representation
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The json string representation of the object.</returns>
        public static string Serialize(object obj)
        {
            return Configuration.FlurlHttpSettings.JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// Provides global point for programmatically configuraing tracing
        /// </summary>
        public static class Tracing
        {
            /// <summary>
            /// Trace source for all HTTP requests. Default level is Error.
            /// <para>
            /// In your app or web.config the trace soruce name is "Flurl.Http".
            /// </para>
            /// </summary>
            public static readonly TraceSource Http = new TraceSource("Flurl.Http", SourceLevels.Error);

            /// <summary>
            /// Traces a failed HTTP request
            /// </summary>
            /// <param name="httpCall">The Flurl HTTP call instance, containing information about the request and response.</param>
            public static void TraceFailedHttpCall(FlurlCall httpCall)
            {
                Http.TraceData(TraceEventType.Error, 0, SerializeHttpCall(httpCall));
                Http.Flush();
            }

            /// <summary>
            /// Traces an HTTP request
            /// </summary>
            /// <param name="httpCall">The Flurl HTTP call instance, containing information about the request and response.</param>
            public static void TraceHttpCall(FlurlCall httpCall)
            {
                Http.TraceData(TraceEventType.Information, 0, SerializeHttpCall(httpCall));
            }

            private static string SerializeHttpCall(FlurlCall httpCall)
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                return JsonConvert.SerializeObject(httpCall, Formatting.Indented, settings);
            }
        }
    }

    /// <summary>
    /// A readonly set of properties that affect OpenStack.NET's behavior.
    /// <para>To customize, register an event handler for <see cref="OpenStackNet.Configuring"/>.</para>
    /// </summary>
    public class OpenStackNetConfigurationOptions
    {
        private bool _isInitialized;
        private readonly ClientFlurlHttpSettings _flurlHttpSettings;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly List<ProductInfoHeaderValue> _userAgents;

        /// <summary/>
        protected OpenStackNetConfigurationOptions()
        {
            _flurlHttpSettings = new ClientFlurlHttpSettings();
            _jsonSerializerSettings = new JsonSerializerSettings();
            _userAgents = new List<ProductInfoHeaderValue>();
        }

        /// <summary />
        public static event Action<CreateEvent> Creating;

        /// <summary />
        internal static OpenStackNetConfigurationOptions Create()
        {
            var createEvent = new CreateEvent();
            Creating?.Invoke(createEvent);
            return createEvent.Result;
        }

        /// <summary />
        public void CompleteInitialization()
        {
            OnCompleteInitialization();
            ApplyDefaults();
            _isInitialized = true;
        }

        /// <summary />
        protected virtual void OnCompleteInitialization()
        {}

        /// <summary>
        /// Custom Flurl.Http configuration settings which are specific to requests made by this SDK.
        /// </summary>
        public ClientFlurlHttpSettings FlurlHttpSettings
        {
            get
            {
                if(_isInitialized)
                    return _flurlHttpSettings.Clone();

                return _flurlHttpSettings;
            }
        }

        /// <summary>
        /// Custom Json.NET configuration settings which are specific to requests made by this SDK.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                if (_isInitialized)
                    return _jsonSerializerSettings.Clone();

                return _jsonSerializerSettings;
            }
        }

        /// <summary> 
        /// Additional application specific user agents which should be set in the UserAgent header on all requests made by this SDK.
        /// </summary>
        public IList<ProductInfoHeaderValue> UserAgents
        {
            get
            {
                if(_isInitialized)
                    return _userAgents.AsReadOnly();

                return _userAgents;
            }
        }

        private void ApplyDefaults()
        {
            //
            // Apply our default settings on top of user customizations, hopefully without clobbering anything
            //
            UserAgents.Add(new ProductInfoHeaderValue("openstack.net", GetType().GetAssemblyFileVersion()));

            _jsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
            _jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            _jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            if(!(_jsonSerializerSettings.ContractResolver is OpenStackContractResolver))
                _jsonSerializerSettings.ContractResolver = new OpenStackContractResolver();
            
            _flurlHttpSettings.JsonSerializer = new NewtonsoftJsonSerializer(_jsonSerializerSettings);

            // When in test mode (set via HttpTest), this will be a custom class (TestHttpClientFactory)
            if (_flurlHttpSettings.HttpClientFactory?.GetType() == typeof(DefaultHttpClientFactory))
                _flurlHttpSettings.HttpClientFactory = new AuthenticatedHttpClientFactory();

            // Apply our event handling and optionally include any custom application handlers
            var applicationBeforeCall = _flurlHttpSettings.BeforeCall;
            _flurlHttpSettings.BeforeCall = call =>
            {
                SetUserAgentHeader(call);
                applicationBeforeCall?.Invoke(call);
            };

            var applicationAfterCall = _flurlHttpSettings.AfterCall;
            _flurlHttpSettings.AfterCall = call =>
            {
                OpenStackNet.Tracing.TraceHttpCall(call);
                applicationAfterCall?.Invoke(call);
            };

            var applicationOnError = _flurlHttpSettings.OnError;
            _flurlHttpSettings.OnError = call =>
            {
                OpenStackNet.Tracing.TraceFailedHttpCall(call);
                applicationOnError?.Invoke(call);
            };
        }

        private void SetUserAgentHeader(FlurlCall call)
        {
            foreach (ProductInfoHeaderValue userAgent in UserAgents)
            {
                call.Request.Headers.Add("userAgent", userAgent.ToString());
            }
        }

        /// <summary>
        /// Raised when creating the SDK configuration options class.
        /// <para>Intended for vendors to override.</para>
        /// </summary>
        /// <exclude />
        public class CreateEvent
        {
            /// <summary>
            /// An instance of the configuration class to use when configuring the SDK.
            /// </summary>
            public OpenStackNetConfigurationOptions Result = new OpenStackNetConfigurationOptions();
        }
    }
}
