namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON response to an Extract Archive operation.
    /// </summary>
    /// <seealso cref="CloudFilesProvider.ExtractArchive"/>
    /// <seealso cref="CloudFilesProvider.ExtractArchiveFromFile"/>
    /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Extract_Archive-d1e2338.html">Extracting Archive Files (Rackspace Cloud Files Developer Guide - API v1)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ExtractArchiveResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="CreatedFiles"/> property.
        /// </summary>
        [JsonProperty("Number Files Created")]
        private int? _createdFiles;

        /// <summary>
        /// This is the backing field for the <see cref="ResponseStatus"/> property.
        /// </summary>
        [JsonProperty("Response Status")]
        private string _responseStatus;

        /// <summary>
        /// This is the backing field for the <see cref="ResponseBody"/> property.
        /// </summary>
        [JsonProperty("Response Body")]
        private string _responseBody;

        /// <summary>
        /// This is the backing field for the <see cref="CreatedFiles"/> property.
        /// </summary>
        [JsonProperty("Errors")]
        private string[][] _errors;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractArchiveResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ExtractArchiveResponse()
        {
        }

        /// <summary>
        /// Gets the number of files created by the Extract Archive operation.
        /// </summary>
        /// <value>
        /// The number of files created by the Extract Archive operation, or <see langword="null"/> if
        /// the JSON response from the server did not include the underlying property.
        /// </value>
        public int? CreatedFiles
        {
            get
            {
                return _createdFiles;
            }
        }

        /// <summary>
        /// Gets the response status for the Extract Archive operation.
        /// </summary>
        /// <value>
        /// The response status for the Extract Archive operation, or <see langword="null"/> if
        /// the JSON response from the server did not include the underlying property.
        /// </value>
        public string ResponseStatus
        {
            get
            {
                return _responseStatus;
            }
        }

        /// <summary>
        /// Gets the response body for the Extract Archive operation.
        /// </summary>
        /// <value>
        /// The response body for the Extract Archive operation, or <see langword="null"/> if
        /// the JSON response from the server did not include the underlying property.
        /// </value>
        public string ResponseBody
        {
            get
            {
                return _responseBody;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="ExtractArchiveError"/> objects describing errors
        /// which occurred while processing specific files within the archive.
        /// </summary>
        /// <value>
        /// A collection of errors, if any, which occurred for specific files during the
        /// Extract Archive operation, or <see langword="null"/> if the JSON response from the server
        /// did not include the underlying property.
        /// </value>
        public ReadOnlyCollection<ExtractArchiveError> Errors
        {
            get
            {
                if (_errors == null)
                    return null;

                List<ExtractArchiveError> errors = new List<ExtractArchiveError>();
                foreach (string[] error in _errors)
                {
                    if (error == null)
                        continue;

                    if (error.Length >= 2)
                        errors.Add(new ExtractArchiveError(error[0], error[1]));
                    else if (error.Length == 1)
                        errors.Add(new ExtractArchiveError(error[0], "Unknown Error"));
                    else
                        errors.Add(new ExtractArchiveError("Unknown File", "Unknown Error"));
                }

                return errors.AsReadOnly();
            }
        }
    }
}
