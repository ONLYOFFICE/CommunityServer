using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents an instance of the JavaScript Boolean object.
    /// </summary>
    [Serializable]
    public class BooleanInstance : ObjectInstance
    {
        private bool value;



        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new boolean instance.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="value"> The value to initialize the instance with. </param>
        public BooleanInstance(ObjectInstance prototype, bool value)
            : base(prototype)
        {
            this.value = value;
        }



        //     .NET ACCESSOR PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the internal class name of the object.  Used by the default toString()
        /// implementation.
        /// </summary>
        protected override string InternalClassName
        {
            get { return "Boolean"; }
        }

        /// <summary>
        /// Gets the primitive value of this object.
        /// </summary>
        public bool Value
        {
            get { return this.value; }
        }




        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Returns the underlying primitive value of the current object.
        /// </summary>
        /// <returns> The underlying primitive value of the current object. </returns>
        [JSInternalFunction(Name = "valueOf")]
        public new bool ValueOf()
        {
            return this.value;
        }

        /// <summary>
        /// Returns a string representing this object.
        /// </summary>
        /// <returns> A string representing this object. </returns>
        [JSInternalFunction(Name = "toString")]
        public string ToStringJS()
        {
            return this.value ? "true" : "false";
        }
    }
}
