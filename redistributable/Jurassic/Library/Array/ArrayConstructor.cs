using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the built-in javascript Array object.
    /// </summary>
    [Serializable]
    public class ArrayConstructor : ClrFunction
    {

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new Array object.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        internal ArrayConstructor(ObjectInstance prototype)
            : base(prototype, "Array", new ArrayInstance(prototype.Engine.Object.InstancePrototype, 0, 0))
        {
        }


        /// <summary>
        /// Creates a new Array instance.
        /// </summary>
        public ArrayInstance New()
        {
            return new ArrayInstance(this.InstancePrototype, 0, 10);
        }

        /// <summary>
        /// Creates a new Array instance.
        /// </summary>
        /// <param name="elements"> The initial elements of the new array. </param>
        public ArrayInstance New(object[] elements)
        {
            // Copy the array if it is not an object array (for example, if it is a string[]).
            if (elements.GetType() != typeof(object[]))
            {
                var temp = new object[elements.Length];
                Array.Copy(elements, temp, elements.Length);
                return new ArrayInstance(this.InstancePrototype, elements);
            }

            return new ArrayInstance(this.InstancePrototype, elements);
        }



        //     JAVASCRIPT INTERNAL FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new Array instance and initializes the contents of the array.
        /// Called when the Array object is invoked like a function, e.g. var x = Array(length).
        /// </summary>
        /// <param name="elements"> The initial elements of the new array. </param>
        [JSCallFunction]
        public ArrayInstance Call(params object[] elements)
        {
            return Construct(elements);
        }

        /// <summary>
        /// Creates a new Array instance and initializes the contents of the array.
        /// Called when the new expression is used on this object, e.g. var x = new Array(length).
        /// </summary>
        /// <param name="elements"> The initial elements of the new array. </param>
        [JSConstructorFunction]
        public ArrayInstance Construct(params object[] elements)
        {
            if (elements.Length == 1)
            {
                if (TypeUtilities.IsNumeric(elements[0]))
                {
                    double specifiedLength = TypeConverter.ToNumber(elements[0]);
                    uint actualLength = TypeConverter.ToUint32(elements[0]);
                    if (specifiedLength != (double)actualLength)
                        throw new JavaScriptException(this.Engine, "RangeError", "Invalid array length");
                    return new ArrayInstance(this.InstancePrototype, actualLength, actualLength);
                }
            }

            // Transform any nulls into undefined.
            for (int i = 0; i < elements.Length; i++)
                if (elements[i] == null)
                    elements[i] = Undefined.Value;

            return New(elements);
        }



        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Tests if the given value is an Array instance.
        /// </summary>
        /// <param name="value"> The value to test. </param>
        /// <returns> <c>true</c> if the given value is an Array instance, <c>false</c> otherwise. </returns>
        [JSInternalFunction(Name = "isArray")]
        public static bool IsArray(object value)
        {
            return value is ArrayInstance;
        }

    }
}
