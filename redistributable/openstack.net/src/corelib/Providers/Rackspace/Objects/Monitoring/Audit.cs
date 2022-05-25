using System.Collections.ObjectModel;

namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using net.openstack.Core.Domain;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;
    using HttpMethod = JSIStudios.SimpleRESTServices.Client.HttpMethod;

    /// <summary>
    /// This class models the JSON representation of an Audit resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <remarks>
    /// Every write operation performed against the API (PUT, POST or DELETE) generates an
    /// audit record that is stored for 30 days. Audits record a variety of information
    /// about the request including the method, URL, headers, query string, transaction ID,
    /// the request body and the response code. They also store information about the action
    /// performed including a JSON list of the previous state of any modified objects. For
    /// example, if you perform an update on an entity, this will record the state of the
    /// entity before modification.
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/general-api-info-audits.html">Audits (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Audit : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private AuditId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Timestamp"/> property.
        /// </summary>
        [JsonProperty("timestamp")]
        private long? _timestamp;

        /// <summary>
        /// This is the backing field for the <see cref="Headers"/> property.
        /// </summary>
        [JsonProperty("headers")]
        private Dictionary<string, string> _headers;

        /// <summary>
        /// This is the backing field for the <see cref="Url"/> property.
        /// </summary>
        [JsonProperty("url")]
        private string _url;

        /// <summary>
        /// This is the backing field for the <see cref="App"/> property.
        /// </summary>
        [JsonProperty("app")]
        private string _app;

        /// <summary>
        /// This is the backing field for the <see cref="Query"/> property.
        /// </summary>
        [JsonProperty("query")]
        private Dictionary<string, string> _query;

        /// <summary>
        /// This is the backing field for the <see cref="TransactionId"/> property.
        /// </summary>
        [JsonProperty("txnId")]
        private TransactionId _transactionId;

        /// <summary>
        /// This is the backing field for the <see cref="Payload"/> property.
        /// </summary>
        [JsonProperty("payload")]
        private string _payload;

        /// <summary>
        /// This is the backing field for the <see cref="Method"/> property.
        /// </summary>
        [JsonProperty("method")]
        [JsonConverter(typeof(StringEnumConverter))]
        private HttpMethod _method;

        /// <summary>
        /// This is the backing field for the <see cref="AccountId"/> property.
        /// </summary>
        [JsonProperty("account_id")]
        private ProjectId _accountId;

        /// <summary>
        /// This is the backing field for the <see cref="Who"/> property.
        /// </summary>
        [JsonProperty("who")]
        private string _who;

        /// <summary>
        /// This is the backing field for the <see cref="Why"/> property.
        /// </summary>
        [JsonProperty("why")]
        private string _why;

        /// <summary>
        /// This is the backing field for the <see cref="StatusCode"/> property.
        /// </summary>
        [JsonProperty("statusCode")]
        private int _statusCode;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="Audit"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected Audit()
        {
        }

        /// <summary>
        /// Gets the unique identifier of the audit resource.
        /// </summary>
        public AuditId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets a timestamp indicating when this audit record was created.
        /// </summary>
        public DateTimeOffset? Timestamp
        {
            get
            {
                return DateTimeOffsetExtensions.ToDateTimeOffset(_timestamp);
            }
        }

        /// <summary>
        /// Gets a collection of custom HTTP headers which were sent with the request that created this audit record.
        /// </summary>
        public ReadOnlyDictionary<string, string> Headers
        {
            get
            {
                if (_headers == null)
                    return null;

                return new ReadOnlyDictionary<string, string>(_headers);
            }
        }

        /// <summary>
        /// Gets the target URI of the HTTP request represented by the audit record.
        /// </summary>
        public Uri Url
        {
            get
            {
                if (_url == null)
                    return null;

                return new Uri(_url, UriKind.RelativeOrAbsolute);
            }
        }

        /// <summary>
        /// Gets the name of the monitoring service module that processed the request.
        /// </summary>
        public string App
        {
            get
            {
                return _app;
            }
        }

        /// <summary>
        /// Gets a collection of query string parameters decoded from <see cref="Url"/>.
        /// </summary>
        public ReadOnlyDictionary<string, string> Query
        {
            get
            {
                if (_query == null)
                    return null;

                return new ReadOnlyDictionary<string, string>(_query);
            }
        }

        /// <summary>
        /// Gets the ID of the transaction that created this audit record.
        /// </summary>
        public TransactionId TransactionId
        {
            get
            {
                return _transactionId;
            }
        }

        /// <summary>
        /// Gets the body of the HTTP request that created this audit record.
        /// </summary>
        public string Payload
        {
            get
            {
                return _payload;
            }
        }

        /// <summary>
        /// Gets the HTTP method used for the request represented by the audit record.
        /// </summary>
        public HttpMethod Method
        {
            get
            {
                return _method;
            }
        }

        /// <summary>
        /// Gets the account ID associated with the audit record. The account ID within
        /// the monitoring service is equivalent to the <see cref="Tenant.Id">Tenant.Id</see>
        /// referenced by other services.
        /// </summary>
        /// <value>
        /// The account ID for the audit record, or <see langword="null"/> if the JSON response from
        /// the server did not include the underlying property.
        /// </value>
        public ProjectId AccountId
        {
            get
            {
                return _accountId;
            }
        }

        /// <summary>
        /// Gets a value indicating <em>who</em> made this change to the monitoring account.
        /// </summary>
        /// <remarks>
        /// This is the value of the optional <c>_who</c> query parameter for HTTP requests.
        /// </remarks>
        public string Who
        {
            get
            {
                return _who;
            }
        }

        /// <summary>
        /// Gets a value indicating <em>why</em> this change was made to the monitoring account.
        /// </summary>
        /// <remarks>
        /// This is the value of the optional <c>_why</c> query parameter for HTTP requests.
        /// </remarks>
        public string Why
        {
            get
            {
                return _why;
            }
        }

        /// <summary>
        /// Gets the HTTP status code returned by the HTTP request represented by the audit record.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get
            {
                return (HttpStatusCode)_statusCode;
            }
        }
    }
}
