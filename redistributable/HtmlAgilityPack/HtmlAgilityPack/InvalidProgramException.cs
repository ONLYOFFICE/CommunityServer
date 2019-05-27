// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// Website & Documentation: http://html-agility-pack.net
// Forum & Issues: https://github.com/zzzprojects/html-agility-pack
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// More projects: http://www.zzzprojects.com/
// Copyright © ZZZ Projects Inc. 2014 - 2017. All rights reserved.

#if METRO

using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// The exception that is thrown when a program contains invalid Microsoft intermediate language (MSIL) or metadata. Generally this indicates a bug in the compiler that generated the program.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    [ComVisible(true)]
    public sealed class InvalidProgramException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.InvalidProgramException"/> class with default properties.
        /// </summary>
        public InvalidProgramException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.InvalidProgramException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        public InvalidProgramException(string message) : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.InvalidProgramException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param><param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception. </param>
        public InvalidProgramException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
#endif