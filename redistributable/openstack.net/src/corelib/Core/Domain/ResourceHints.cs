namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using JSIStudios.SimpleRESTServices.Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using ContentType = System.Net.Mime.ContentType;

    /// <summary>
    /// This class models the Resource Hints of the home document described by
    /// <strong>Home Documents for HTTP APIs</strong>.
    /// </summary>
    /// <remarks>
    /// Resource hints allow clients to find relevant information about
    /// interacting with a resource beforehand, as a means of optimizing
    /// communications, as well as advertising available behaviors (e.g., to
    /// aid in laying out a user interface for consuming the API).
    ///
    /// <para>Hints are just that - they are not a "contract", and are to only
    /// be taken as advisory. The runtime behavior of the resource always
    /// overrides hinted information.</para>
    ///
    /// <para>For example, a resource might hint that the PUT method is allowed
    /// on all "widget" resources. This means that generally, the user has the
    /// ability to PUT to a particular resource, but a specific resource
    /// could reject a PUT based upon access control or other considerations.
    /// More fine-grained information might be gathered by interacting with
    /// the resource (e.g., via a GET), or by another resource "containing"
    /// it (such as a "widgets" collection).</para>
    ///
    /// <para>The current specification defines a set of common hints, based
    /// upon information that's discoverable by directly interacting with
    /// resources. A future draft will explain how to define new hints.</para>
    /// </remarks>
    /// <seealso href="http://tools.ietf.org/html/draft-nottingham-json-home-03#section-4">Resource Hints (Home Documents for HTTP APIs - draft-nottingham-json-home-03)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ResourceHints : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Allow"/> property.
        /// </summary>
        [JsonProperty("allow", DefaultValueHandling = DefaultValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        private HttpMethod[] _allow;

        /// <summary>
        /// This is the backing field for the <see cref="Formats"/> property.
        /// </summary>
        [JsonProperty("formats", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private Dictionary<string, JObject> _formats;

        /// <summary>
        /// This is the backing field for the <see cref="AcceptPatch"/> property.
        /// </summary>
        [JsonProperty("accept-patch", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string[] _acceptPatch;

        /// <summary>
        /// This is the backing field for the <see cref="AcceptPost"/> property.
        /// </summary>
        [JsonProperty("accept-post", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string[] _acceptPost;

        /// <summary>
        /// This is the backing field for the <see cref="AcceptPut"/> property.
        /// </summary>
        [JsonProperty("accept-put", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string[] _acceptPut;

        /// <summary>
        /// This is the backing field for the <see cref="AcceptRanges"/> property.
        /// </summary>
        [JsonProperty("accept-ranges", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string[] _acceptRanges;

        /// <summary>
        /// This is the backing field for the <see cref="Prefer"/> property.
        /// </summary>
        [JsonProperty("prefer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string[] _prefer;

        /// <summary>
        /// This is the backing field for the <see cref="Docs"/> property.
        /// </summary>
        [JsonProperty("docs", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _docs;

        /// <summary>
        /// This is the backing field for the <see cref="Preconditions"/> property.
        /// </summary>
        [JsonProperty("precondition-req", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string[] _preconditionReq;

        /// <summary>
        /// This is the backing field for the <see cref="AuthenticationRequirements"/> property.
        /// </summary>
        [JsonProperty("auth-req", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private AuthenticationRequirement[] _authReq;

        /// <summary>
        /// This is the backing field for the <see cref="Status"/> property.
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _status;
#pragma warning restore 649

        /// <summary>
        /// Gets the HTTP methods that the current client will be able to use to interact
        /// with the resource; equivalent to the <strong>Allow</strong> HTTP response
        /// header.
        /// </summary>
        public ReadOnlyCollection<HttpMethod> Allow
        {
            get
            {
                if (_allow == null)
                    return null;

                return new ReadOnlyCollection<HttpMethod>(_allow);
            }
        }

        /// <summary>
        /// Gets the representation types that the resource produces and consumes, using the
        /// GET and PUT methods respectively, subject to the <see cref="Allow"/> hint. The
        /// keys of this collections are <see cref="ContentType.MediaType">media types</see>,
        /// and the values are objects which have not yet been defined.
        /// </summary>
        public Dictionary<string, JObject> Formats
        {
            get
            {
                return _formats;
            }
        }

        /// <summary>
        /// Gets the PATCH request formats accepted by the resource for this client;
        /// equivalent to the <strong>Accept-Patch</strong> HTTP response header.
        /// </summary>
        /// <seealso href="http://tools.ietf.org/html/rfc5789">RFC5789 (PATCH Method for HTTP)</seealso>
        public ReadOnlyCollection<string> AcceptPatch
        {
            get
            {
                if (_acceptPatch == null)
                    return null;

                return new ReadOnlyCollection<string>(_acceptPatch);
            }
        }

        /// <summary>
        /// Gets the POST request formats accepted by the resource for this client.
        /// </summary>
        public ReadOnlyCollection<string> AcceptPost
        {
            get
            {
                if (_acceptPost == null)
                    return null;

                return new ReadOnlyCollection<string>(_acceptPost);
            }
        }

        /// <summary>
        /// Gets the PUT request formats accepted by the resource for this client.
        /// </summary>
        /// <remarks>
        /// If this value is <see langword="null"/>, a client may assume that any format indicated by
        /// the <see cref="Formats"/> hint is acceptable in a PUT.
        /// </remarks>
        public ReadOnlyCollection<string> AcceptPut
        {
            get
            {
                if (_acceptPut == null)
                    return null;

                return new ReadOnlyCollection<string>(_acceptPut);
            }
        }

        /// <summary>
        /// Gets the range-specifiers available to the client for this resource;
        /// equivalent to the <strong>Accept-Ranges</strong> HTTP response header.
        /// </summary>
        /// <remarks>
        /// The values are HTTP range specifiers.
        /// </remarks>
        /// <seealso href="http://tools.ietf.org/html/draft-ietf-httpbis-p5-range-24#section-2.3">Accept-Ranges (Hypertext Transfer Protocol (HTTP/1.1): Range Requests - draft-ietf-httpbis-p5-range-24)</seealso>
        public ReadOnlyCollection<string> AcceptRanges
        {
            get
            {
                if (_acceptRanges == null)
                    return null;

                return new ReadOnlyCollection<string>(_acceptRanges);
            }
        }

        /// <summary>
        /// Gets the preferences supported by the resource. Note that, as per that
        /// specification, a preference can be ignored by the server.
        /// </summary>
        /// <seealso href="http://tools.ietf.org/html/draft-snell-http-prefer-12">Prefer Header for HTTP (draft-snell-http-prefer-12)</seealso>
        public ReadOnlyCollection<string> Prefer
        {
            get
            {
                if (_prefer == null)
                    return null;

                return new ReadOnlyCollection<string>(_prefer);
            }
        }

        /// <summary>
        /// Gets the location for human-readable documentation for the relation type
        /// of the resource.
        /// </summary>
        public Uri Docs
        {
            get
            {
                if (_docs == null)
                    return null;

                return new Uri(_docs);
            }
        }

        /// <summary>
        /// Gets a collection of preconditions that the resource may require for
        /// state-changing requests (e.g., PUT, PATCH) to include a precondition,
        /// to avoid conflicts due to concurrent updates.
        /// </summary>
        /// <remarks>
        /// This collection may contain the values <literal>"etag"</literal> and
        /// <literal>"last-modified"</literal> indicating the type of precondition
        /// expected.
        /// </remarks>
        /// <seealso href="http://tools.ietf.org/html/draft-ietf-httpbis-p4-conditional-24">Hypertext Transfer Protocol (HTTP/1.1): Conditional Requests (draft-ietf-httpbis-p4-conditional-24)</seealso>
        public ReadOnlyCollection<string> Preconditions
        {
            get
            {
                if (_preconditionReq == null)
                    return null;

                return new ReadOnlyCollection<string>(_preconditionReq);
            }
        }

        /// <summary>
        /// Gets a collection of requirements for authentication using the HTTP Authentication Framework.
        /// </summary>
        /// <seealso cref="AuthenticationRequirement"/>
        /// <seealso href="http://tools.ietf.org/html/draft-ietf-httpbis-p7-auth-24">Hypertext Transfer Protocol (HTTP/1.1): Authentication (draft-ietf-httpbis-p7-auth-24)</seealso>
        public ReadOnlyCollection<AuthenticationRequirement> AuthenticationRequirements
        {
            get
            {
                if (_authReq == null)
                    return null;

                return new ReadOnlyCollection<AuthenticationRequirement>(_authReq);
            }
        }

        /// <summary>
        /// Gets the status of the resource. Possible values for the status are
        /// <literal>"deprecated"</literal> and <literal>"gone"</literal>.
        /// </summary>
        /// <remarks>
        /// Some possible values for this property are:
        ///
        /// <list type="bullet">
        /// <item><c>deprecated</c>: indicates that use of the resource is not recommended, but it is still available.</item>
        /// <item><c>gone</c>: indicates that the resource is no longer available; i.e. it will return a 410 Gone HTTP status code if accessed.</item>
        /// </list>
        /// </remarks>
        public string Status
        {
            get
            {
                return _status;
            }
        }
    }
}
