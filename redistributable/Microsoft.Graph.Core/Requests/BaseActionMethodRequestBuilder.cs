// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;

    /// <summary>
    /// The base method request builder class used for POST actions.
    /// </summary>
    public abstract class BaseActionMethodRequestBuilder<T> : BaseRequestBuilder where T : IBaseRequest
    {
        private Dictionary<string, object> _parameters = new Dictionary<string, object>();

        /// <summary>
        /// Constructs a new BaseActionMethodRequestBuilder.
        /// </summary>
        /// <param name="requestUrl">The URL for the request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        public BaseActionMethodRequestBuilder(
            string requestUrl,
            IBaseClient client)
            : base(requestUrl, client)
        {
        }

        /// <summary>
        /// Derived classes implement this function to construct the specific request class instance
        /// when a request object is required.
        /// </summary>
        /// <param name="functionUrl">The URL to use for the request.</param>
        /// <param name="options">The query and header options for the request.</param>
        /// <returns>An instance of the request class.</returns>
        protected abstract T CreateRequest(string functionUrl, IEnumerable<Option> options);

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <param name="options">The query and header options for the request.</param>
        /// <returns>The built request.</returns>
        public T Request(IEnumerable<Option> options = null)
        {
            return CreateRequest(this.RequestUrl, options);
        }

        /// <summary>
        /// A helper method for injecting a parameter string for a given name and value
        /// pair. This method handles the nullable case and properly wrapped and escaping
        /// string values.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="nullable">A flag specifying whether the parameter is allowed to be null.</param>
        /// <typeparam name="U">The type of the value parameter.</typeparam>
        /// <returns>A string representing the parameter for an OData method call.</returns>
        protected void SetParameter<U>(string name, U value, bool nullable)
        {
            if (value == null && !nullable)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = "invalidRequest",
                        Message = string.Format("{0} is a required parameter for this method request.", name),
                    });
            }

            _parameters.Add(name, value);
        }

        /// <summary>
        /// Check if the parameter list contains a given name.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>True if the parameter list contains the given name.</returns>
        protected bool HasParameter(string name)
        {
            return _parameters.ContainsKey(name);
        }

        /// <summary>
        /// Get a parameter string for a given name.
        /// </summary>
        /// <typeparam name="U">The type of the value parameter.</typeparam>
        /// <param name="name">The name key.</param>
        /// <returns>The value associated with the given name.</returns>
        protected U GetParameter<U>(string name)
        {
            return (U)_parameters[name];
        }
    }
}
