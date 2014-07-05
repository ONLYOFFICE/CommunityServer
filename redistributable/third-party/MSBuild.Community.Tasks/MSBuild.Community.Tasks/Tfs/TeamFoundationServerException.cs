
using System;

namespace MSBuild.Community.Tasks.Tfs
{
    /// <summary>
    /// Exceptions returned by the Team Foundation Server
    /// </summary>
    /// <exclude />
    public class TeamFoundationServerException : Exception
    {
        /// <summary>
        /// Creates a new instance of the exception
        /// </summary>
        public TeamFoundationServerException() : base(){}
        /// <summary>
        /// Creates a new instance of the exception
        /// </summary>
        /// <param name="message">A description of the exception</param>
        public TeamFoundationServerException(string message) : base(message) { }
    }
}
