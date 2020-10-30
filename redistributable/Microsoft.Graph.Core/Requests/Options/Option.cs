// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// A key value pair to be added to the request.
    /// </summary>
    public abstract class Option
    {
        /// <summary>
        /// Create a new option.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="value">The value of the option.</param>
        protected Option(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// The name, or key, of the option.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The value of the option.
        /// </summary>
        public string Value { get; private set; }
    }
}
