using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using RestResponse = JSIStudios.SimpleRESTServices.Client.Response;

namespace net.openstack.Core.Exceptions.Response
{
    /// <summary>
    /// Defines the base class for errors resulting from a call to a REST API.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class ResponseException : Exception
    {
        [NonSerialized]
        private ExceptionData _state;

        /// <summary>
        /// Gets the REST <see cref="RestResponse"/> containing the details
        /// about this error.
        /// </summary>
        public RestResponse Response
        {
            get
            {
                return _state.Response;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseException"/> class with the
        /// specified error message and REST response.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">The REST response.</param>
        public ResponseException(string message, RestResponse response)
            : base(message)
        {
            _state.Response = response;
#if !NET35
            SerializeObjectState += (ex, args) => args.AddSerializedState(_state);
#endif
        }

        /// <inheritdoc/>
        public override string Message
        {
            get
            {
                if (Response != null && Response.HasJsonBody())
                {
                    try
                    {
                        var response = JsonConvert.DeserializeObject<Dictionary<string, ErrorDetails>>(Response.RawBody);
                        var pair = response.FirstOrDefault();
                        if (pair.Value != null && !string.IsNullOrEmpty(pair.Value.Message))
                            return pair.Value.Message;
                    }
                    catch (JsonSerializationException)
                    {
                        // will fall back to base.Message
                    }
                }

                return base.Message;
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class ErrorDetails
        {
#pragma warning disable 649 // Field '{fieldname}' is never assigned to, and will always have its default value {0 | null}
            [JsonProperty("code")]
            public int Code;

            [JsonProperty("message")]
            public string Message;

            [JsonProperty("details")]
            public string Details;
#pragma warning restore 649
        }

        [Serializable]
        private struct ExceptionData : ISafeSerializationData
        {
            public RestResponse Response
            {
                get;
                set;
            }

            void ISafeSerializationData.CompleteDeserialization(object deserialized)
            {
                ((ResponseException)deserialized)._state = this;
            }
        }
    }
}
