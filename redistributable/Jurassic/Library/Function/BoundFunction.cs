using System;
using System.Collections.Generic;
using Jurassic.Compiler;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents a function that has bound arguments.
    /// </summary>
    [Serializable]
    internal class BoundFunction : FunctionInstance
    {

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new instance of a user-defined function.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="targetFunction"> The function that was bound. </param>
        /// <param name="boundThis"> The value of the "this" parameter when the target function is called. </param>
        /// <param name="boundArguments"> Zero or more bound argument values. </param>
        internal BoundFunction(FunctionInstance targetFunction, object boundThis, object[] boundArguments)
            : base(targetFunction.Prototype)
        {
            if (targetFunction == null)
                throw new ArgumentNullException("targetFunction");
            if (boundArguments == null)
                boundArguments = new object[0];
            this.TargetFunction = targetFunction;
            this.BoundThis = boundThis;
            this.BoundArguments = boundArguments;

            // Add function properties.
            this.FastSetProperty("name", targetFunction.Name);
            this.FastSetProperty("length", Math.Max(targetFunction.Length - boundArguments.Length, 0));
            this.FastSetProperty("prototype", this.Engine.Object.Construct(), PropertyAttributes.Writable);
            this.InstancePrototype.FastSetProperty("constructor", this, PropertyAttributes.NonEnumerable);
            
            // Caller and arguments cannot be accessed.
            var thrower = new ThrowTypeErrorFunction(this.Engine.Function, "The 'caller' or 'arguments' properties cannot be accessed on a bound function.");
            var accessor = new PropertyAccessorValue(thrower, thrower);
            this.FastSetProperty("caller", accessor, PropertyAttributes.IsAccessorProperty, overwriteAttributes: true);
            this.FastSetProperty("arguments", accessor, PropertyAttributes.IsAccessorProperty, overwriteAttributes: true);
        }



        //     PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the function that is being bound.
        /// </summary>
        public FunctionInstance TargetFunction
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the value of the "this" parameter when the target function is called.
        /// </summary>
        public object BoundThis
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets an array of zero or more bound argument values.
        /// </summary>
        public object[] BoundArguments
        {
            get;
            private set;
        }



        //     OVERRIDES
        //_________________________________________________________________________________________

        /// <summary>
        /// Determines whether the given object inherits from this function.  More precisely, it
        /// checks whether the prototype chain of the object contains the prototype property of
        /// this function.  Used by the "instanceof" operator.
        /// </summary>
        /// <param name="instance"> The instance to check. </param>
        /// <returns> <c>true</c> if the object inherits from this function; <c>false</c>
        /// otherwise. </returns>
        public override bool HasInstance(object instance)
        {
            return this.TargetFunction.HasInstance(instance);
        }

        /// <summary>
        /// Calls this function, passing in the given "this" value and zero or more arguments.
        /// </summary>
        /// <param name="thisObject"> The value of the "this" keyword within the function. </param>
        /// <param name="arguments"> An array of argument values to pass to the function. </param>
        /// <returns> The value that was returned from the function. </returns>
        public override object CallLateBound(object thisObject, params object[] argumentValues)
        {
            // Append the provided argument values to the end of the existing bound argument values.
            var resultingArgumentValues = argumentValues;
            if (this.BoundArguments.Length > 0)
            {
                if (argumentValues == null || argumentValues.Length == 0)
                    resultingArgumentValues = this.BoundArguments;
                else
                {
                    resultingArgumentValues = new object[this.BoundArguments.Length + argumentValues.Length];
                    Array.Copy(this.BoundArguments, resultingArgumentValues, this.BoundArguments.Length);
                    Array.Copy(argumentValues, 0, resultingArgumentValues, this.BoundArguments.Length, argumentValues.Length);
                }
            }

            // Call the target function.
            return this.TargetFunction.CallLateBound(this.BoundThis, resultingArgumentValues);
        }

        /// <summary>
        /// Creates an object, using this function as the constructor.
        /// </summary>
        /// <param name="arguments"> An array of argument values to pass to the function. </param>
        /// <returns> The object that was created. </returns>
        public override ObjectInstance ConstructLateBound(params object[] argumentValues)
        {
            // Append the provided argument values to the end of the existing bound argument values.
            var resultingArgumentValues = argumentValues;
            if (this.BoundArguments.Length > 0)
            {
                if (argumentValues == null || argumentValues.Length == 0)
                    resultingArgumentValues = this.BoundArguments;
                else
                {
                    resultingArgumentValues = new object[this.BoundArguments.Length + argumentValues.Length];
                    Array.Copy(this.BoundArguments, resultingArgumentValues, this.BoundArguments.Length);
                    Array.Copy(argumentValues, 0, resultingArgumentValues, this.BoundArguments.Length, argumentValues.Length);
                }
            }

            // Call the target function.
            return this.TargetFunction.ConstructLateBound(resultingArgumentValues);
        }

        /// <summary>
        /// Returns a string representing this object.
        /// </summary>
        /// <returns> A string representing this object. </returns>
        public override string ToString()
        {
            return this.TargetFunction.ToString();
        }
    }
}
