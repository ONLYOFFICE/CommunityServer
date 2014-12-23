using System;
using System.Collections.Generic;
using System.Text;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the base class of all the javascript errors.
    /// </summary>
    [Serializable]
    public class ErrorInstance : ObjectInstance
    {

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new Error instance with the given name, message and optionally a stack trace.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="name"> The initial value of the name property.  Pass <c>null</c> to avoid
        /// creating this property. </param>
        /// <param name="message"> The initial value of the message property.  Pass <c>null</c> to
        /// avoid creating this property. </param>
        internal ErrorInstance(ObjectInstance prototype, string name, string message)
            : base(prototype)
        {
            if (name != null)
                this.FastSetProperty("name", name, PropertyAttributes.FullAccess);
            if (message != null)
                this.FastSetProperty("message", message, PropertyAttributes.FullAccess);
        }



        //     .NET ACCESSOR PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the internal class name of the object.  Used by the default toString()
        /// implementation.
        /// </summary>
        protected override string InternalClassName
        {
            get { return "Error"; }
        }

        /// <summary>
        /// Gets the name for the type of error.
        /// </summary>
        public string Name
        {
            get { return TypeConverter.ToString(this["name"]); }
        }

        /// <summary>
        /// Gets a human-readable description of the error.
        /// </summary>
        public string Message
        {
            get { return TypeConverter.ToString(this["message"]); }
        }

        /// <summary>
        /// Gets the stack trace.  Note that this is populated when the object is thrown, NOT when
        /// it is initialized.
        /// </summary>
        public string Stack
        {
            get { return TypeConverter.ToString(this["stack"]); }
        }

        /// <summary>
        /// Sets the stack trace information.
        /// </summary>
        /// <param name="errorName"> The name of the error (e.g. "ReferenceError"). </param>
        /// <param name="message"> The error message. </param>
        /// <param name="path"> The path of the javascript source file that is currently executing. </param>
        /// <param name="function"> The name of the currently executing function. </param>
        /// <param name="line"> The line number of the statement that is currently executing. </param>
        internal void SetStackTrace(string path, string function, int line)
        {
            var stackTrace = this.Engine.FormatStackTrace(this.Name, this.Message, path, function, line);
            this.FastSetProperty("stack", stackTrace, PropertyAttributes.FullAccess);
        }



        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Returns a string representing the current object.
        /// </summary>
        /// <returns> A string representing the current object. </returns>
        [JSInternalFunction(Name = "toString")]
        public string ToStringJS()
        {
            if (string.IsNullOrEmpty(this.Message))
                return this.Name;
            else if (string.IsNullOrEmpty(this.Name))
                return this.Message;
            else
                return string.Format("{0}: {1}", this.Name, this.Message);
        }
    }
}
