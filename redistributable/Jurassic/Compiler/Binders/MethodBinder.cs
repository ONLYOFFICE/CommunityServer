using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Selects a method from a list of candidates and performs type conversion from actual
    /// argument type to formal argument type.
    /// </summary>
    [Serializable]
    internal abstract class MethodBinder : Binder
    {
        private string name;
        private Type declaringType;
        private int functionLength;
        



        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new Binder instance.
        /// </summary>
        /// <param name="targetMethod"> A method to bind to. </param>
        protected MethodBinder(BinderMethod targetMethod)
        {
            if (targetMethod == null)
                throw new ArgumentNullException("targetMethod");
            this.name = targetMethod.Name;
            this.declaringType = targetMethod.DeclaringType;
            this.functionLength = targetMethod.RequiredParameterCount +
                targetMethod.OptionalParameterCount + (targetMethod.HasParamArray ? 1 : 0);
        }

        /// <summary>
        /// Creates a new Binder instance.
        /// </summary>
        /// <param name="targetMethods"> An enumerable list of methods to bind to.  At least one
        /// method must be provided.  Every method must have the same name and declaring type. </param>
        protected MethodBinder(IEnumerable<BinderMethod> targetMethods)
        {
            if (targetMethods == null)
                throw new ArgumentNullException("targetMethods");

            // At least one method must be provided.
            // Every method must have the same name and declaring type.
            foreach (var method in targetMethods)
            {
                if (this.Name == null)
                {
                    this.name = method.Name;
                    this.declaringType = method.DeclaringType;
                }
                else
                {
                    if (this.Name != method.Name)
                        throw new ArgumentException("Every method must have the same name.", "targetMethods");
                    if (this.declaringType != method.DeclaringType)
                        throw new ArgumentException("Every method must have the same declaring type.", "targetMethods");
                }
                this.functionLength = Math.Max(this.FunctionLength, method.RequiredParameterCount +
                    method.OptionalParameterCount + (method.HasParamArray ? 1 : 0));
            }
            if (this.Name == null)
                throw new ArgumentException("At least one method must be provided.", "targetMethods");
        }




        //     PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the name of the target methods.
        /// </summary>
        public override string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets the full name of the target methods, including the type name.
        /// </summary>
        public override string FullName
        {
            get { return string.Format("{0}.{1}", this.declaringType, this.name); }
        }

        /// <summary>
        /// Gets the maximum number of arguments of any of the target methods.  Used to set the
        /// length property on the function.
        /// </summary>
        public override int FunctionLength
        {
            get { return this.functionLength; }
        }
    }
}
