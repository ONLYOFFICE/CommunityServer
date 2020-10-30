// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Handles resolving interfaces to the correct concrete class during serialization/deserialization.
    /// </summary>
    /// <typeparam name="T">The concrete instance type.</typeparam>
    public class InterfaceConverter<T> : CustomCreationConverter<T>
        where T : new()
    {

        /// <summary>
        /// Create a new object of type T.
        /// </summary>
        /// <param name="objectType">The type of object to create.</param>
        /// <returns></returns>
        public override T Create(Type objectType)
        {
            return new T();
        }
    }
}
