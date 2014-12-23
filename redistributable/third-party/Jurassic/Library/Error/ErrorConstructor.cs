using System;
using System.Collections.Generic;
using System.Text;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents a constructor for one of the error types: Error, RangeError, SyntaxError, etc.
    /// </summary>
    [Serializable]
    public class ErrorConstructor : ClrFunction
    {

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new derived error function.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="typeName"> The name of the error object, e.g. "Error", "RangeError", etc. </param>
        internal ErrorConstructor(ObjectInstance prototype, string typeName)
            : base(prototype, typeName, GetInstancePrototype(prototype.Engine, typeName))
        {
        }

        /// <summary>
        /// Determine the instance prototype for the given error type.
        /// </summary>
        /// <param name="engine"> The script engine associated with this object. </param>
        /// <param name="typeName"> The name of the error object, e.g. "Error", "RangeError", etc. </param>
        /// <returns> The instance prototype. </returns>
        private static ObjectInstance GetInstancePrototype(ScriptEngine engine, string typeName)
        {
            if (typeName == "Error")
            {
                // This constructor is for regular Error objects.
                // Prototype chain: Error instance -> Error prototype -> Object prototype
                return new ErrorInstance(engine.Object.InstancePrototype, typeName, string.Empty);
            }
            else
            {
                // This constructor is for derived Error objects like RangeError, etc.
                // Prototype chain: XXXError instance -> XXXError prototype -> Error prototype -> Object prototype
                return new ErrorInstance(engine.Error.InstancePrototype, typeName, string.Empty);
            }
        }



        //     JAVASCRIPT INTERNAL FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Called when the Error object is invoked like a function, e.g. var x = Error("oh no").
        /// Creates a new derived error instance with the given message.
        /// </summary>
        /// <param name="message"> A description of the error. </param>
        [JSCallFunction]
        public ErrorInstance Call([DefaultParameterValue("")] string message = "")
        {
            return new ErrorInstance(this.InstancePrototype, null, message);
        }

        /// <summary>
        /// Creates a new derived error instance with the given message.
        /// </summary>
        /// <param name="message"> A description of the error. </param>
        [JSConstructorFunction]
        public ErrorInstance Construct([DefaultParameterValue("")] string message = "")
        {
            return new ErrorInstance(this.InstancePrototype, null, message);
        }

    }
}
