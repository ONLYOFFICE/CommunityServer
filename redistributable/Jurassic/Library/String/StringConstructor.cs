using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the built-in javascript String object.
    /// </summary>
    [Serializable]
    public class StringConstructor : ClrFunction
    {
        
        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new String object.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        internal StringConstructor(ObjectInstance prototype)
            : base(prototype, "String", new StringInstance(prototype.Engine.Object.InstancePrototype, string.Empty))
        {
        }



        //     JAVASCRIPT INTERNAL FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Called when the String object is invoked like a function, e.g. var x = String().
        /// Returns an empty string.
        /// </summary>
        [JSCallFunction]
        public string Call()
        {
            return string.Empty;
        }

        /// <summary>
        /// Called when the String object is invoked like a function, e.g. var x = String(NaN).
        /// Converts the given argument into a string value (not a String object).
        /// </summary>
        [JSCallFunction]
        public string Call(string value)
        {
            return value;
        }

        /// <summary>
        /// Creates a new String instance and initializes it to the empty string.
        /// </summary>
        [JSConstructorFunction]
        public StringInstance Construct()
        {
            return new StringInstance(this.InstancePrototype);
        }

        /// <summary>
        /// Creates a new String instance and initializes it to the given value.
        /// </summary>
        /// <param name="value"> The value to initialize to. </param>
        [JSConstructorFunction]
        public StringInstance Construct(string value)
        {
            return new StringInstance(this.InstancePrototype, value);
        }



        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Returns a string created by using the specified sequence of Unicode values.
        /// </summary>
        /// <param name="charCodes"></param>
        /// <returns></returns>
        [JSInternalFunction(Name = "fromCharCode")]
        public static string FromCharCode(params double[] charCodes)
        {
            // Note: charCodes must be an array of doubles, because the default marshalling
            // rule to int uses ToInteger() and there are no marshalling rules for short, ushort
            // or uint.  ToInteger() doesn't preserve the wrapping behaviour we need.
            var result = new System.Text.StringBuilder(charCodes.Length);
            foreach (double charCode in charCodes)
                result.Append((char)TypeConverter.ToUint16(charCode));
            return result.ToString();
        }

    }
}
