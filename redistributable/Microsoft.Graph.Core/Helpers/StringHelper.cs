// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Linq;

    /// <summary>
    /// Helper class for string casing.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Converts the type string to title case.
        /// </summary>
        /// <param name="typeString">The type string.</param>
        /// <returns>The converted string.</returns>
        public static string ConvertTypeToTitleCase(string typeString)
        {
            if (!string.IsNullOrEmpty(typeString))
            {
                var stringSegments = typeString.Split('.').Select(
                    segment => string.Concat(segment.Substring(0, 1).ToUpperInvariant(), segment.Substring(1)));
                return string.Join(".", stringSegments);
            }

            return typeString;
        }

        /// <summary>
        /// Converts the type string to lower camel case.
        /// </summary>
        /// <param name="typeString">The type string.</param>
        /// <returns>The converted string.</returns>
        public static string ConvertTypeToLowerCamelCase(string typeString)
        {
            if (!string.IsNullOrEmpty(typeString))
            {
                var stringSegments = typeString.Split('.').Select(
                    segment => string.Concat(segment.Substring(0, 1).ToLowerInvariant(), segment.Substring(1)));
                return string.Join(".", stringSegments);
            }

            return typeString;
        }

        /// <summary>
        /// Converts the identifier string to lower camel case.
        /// </summary>
        /// <param name="identifierString">The identifier string.</param>
        /// <returns>The converted string.</returns>
        public static string ConvertIdentifierToLowerCamelCase(string identifierString)
        {
            if (!string.IsNullOrEmpty(identifierString))
            {
                return string.Concat(identifierString.Substring(0, 1).ToLowerInvariant(), identifierString.Substring(1));
            }
            return identifierString;
        }
    }
}
