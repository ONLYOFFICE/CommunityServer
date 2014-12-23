using System;
using System.Collections.Generic;
using Jurassic.Compiler;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents a JavaScript function that throws a type error.
    /// </summary>
    [Serializable]
    internal sealed class ThrowTypeErrorFunction : FunctionInstance
    {
        private string message;



        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new ThrowTypeErrorFunction instance.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        internal ThrowTypeErrorFunction(ObjectInstance prototype)
            : this(prototype, "It is illegal to access the 'callee' or 'caller' property in strict mode")
        {
        }

        /// <summary>
        /// Creates a new ThrowTypeErrorFunction instance.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="message"> The TypeError message. </param>
        internal ThrowTypeErrorFunction(ObjectInstance prototype, string message)
            : base(prototype)
        {
            this.FastSetProperty("length", 0);
            this.IsExtensible = false;
            this.message = message;
        }


        //     OVERRIDES
        //_________________________________________________________________________________________

        /// <summary>
        /// Calls this function, passing in the given "this" value and zero or more arguments.
        /// </summary>
        /// <param name="thisObject"> The value of the "this" keyword within the function. </param>
        /// <param name="arguments"> An array of argument values to pass to the function. </param>
        /// <returns> The value that was returned from the function. </returns>
        public override object CallLateBound(object thisObject, params object[] argumentValues)
        {
            throw new JavaScriptException(this.Engine, "TypeError", this.message);
        }

        /// <summary>
        /// Returns a string representing this object.
        /// </summary>
        /// <returns> A string representing this object. </returns>
        public override string ToString()
        {
            return "function ThrowTypeError() {{ [native code] }}";
        }
    }
}
