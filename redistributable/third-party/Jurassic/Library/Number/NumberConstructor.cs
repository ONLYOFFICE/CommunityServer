using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the built-in javascript Number object.
    /// </summary>
    [Serializable]
    public class NumberConstructor : ClrFunction
    {

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new Number object.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        internal NumberConstructor(ObjectInstance prototype)
            : base(prototype, "Number", new NumberInstance(prototype.Engine.Object.InstancePrototype, 0))
        {
        }



        //     JAVASCRIPT INTERNAL METHODS
        //_________________________________________________________________________________________

        /// <summary>
        /// Called when the Number object is invoked like a function, e.g. var x = Number("5").
        /// Returns zero.
        /// </summary>
        [JSCallFunction]
        public double Call()
        {
            return 0;
        }

        /// <summary>
        /// Called when the Number object is invoked like a function, e.g. var x = Number("5").
        /// Converts the given argument into a number value (not a Number object).
        /// </summary>
        [JSCallFunction]
        public double Call(double value)
        {
            return value;
        }

        /// <summary>
        /// Creates a new Number instance and initializes it to zero.
        /// </summary>
        [JSConstructorFunction]
        public NumberInstance Construct()
        {
            return new NumberInstance(this.InstancePrototype, 0);
        }

        /// <summary>
        /// Creates a new Number instance and initializes it to the given value.
        /// </summary>
        /// <param name="value"> The value to initialize to. </param>
        [JSConstructorFunction]
        public NumberInstance Construct(double value)
        {
            return new NumberInstance(this.InstancePrototype, value);
        }



        //     JAVASCRIPT FIELDS
        //_________________________________________________________________________________________

        /// <summary>
        /// The largest representable number, approximately 1.8e+308.
        /// </summary>
        [JSField]
        public const double MAX_VALUE = double.MaxValue;

        /// <summary>
        /// The smallest positive representable number, approximately 5e-324.
        /// </summary>
        [JSField]
        public const double MIN_VALUE = double.Epsilon;

        /// <summary>
        /// Special "not a number" value.
        /// </summary>
        [JSField]
        public const double NaN = double.NaN;

        /// <summary>
        /// Special value representing negative infinity.
        /// </summary>
        [JSField]
        public const double NEGATIVE_INFINITY = double.NegativeInfinity;

        /// <summary>
        /// Special value representing positive infinity.
        /// </summary>
        [JSField]
        public const double POSITIVE_INFINITY = double.PositiveInfinity;

    }
}
