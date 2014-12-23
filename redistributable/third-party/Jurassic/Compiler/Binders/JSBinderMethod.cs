using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Jurassic.Library;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a single method that the JS function binder can call.
    /// </summary>
    [Serializable]
    internal class JSBinderMethod : BinderMethod
    {

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new FunctionBinderMethod instance.
        /// </summary>
        /// <param name="method"> The method to call. </param>
        /// <param name="flags"> Flags that modify the binding process. </param>
        public JSBinderMethod(MethodInfo method, JSFunctionFlags flags = JSFunctionFlags.None)
            : base(method)
        {
            Init(flags);
        }

        /// <summary>
        /// Creates a new FunctionBinderMethod instance.
        /// </summary>
        /// <param name="method"> The method to call. </param>
        /// <param name="flags"> Flags that modify the binding process. </param>
        private void Init(JSFunctionFlags flags)
        {
            this.Flags = flags;
            this.HasEngineParameter = (flags & JSFunctionFlags.HasEngineParameter) != 0;
            if (this.HasEngineParameter == true && this.Method.IsStatic == false)
                throw new InvalidOperationException(string.Format("The {0} flag cannot be used on the instance method '{1}'.", JSFunctionFlags.HasEngineParameter, this.Name));
            this.HasExplicitThisParameter = (flags & JSFunctionFlags.HasThisObject) != 0;
            if (this.HasExplicitThisParameter == true && this.Method.IsStatic == false)
                throw new InvalidOperationException(string.Format("The {0} flag cannot be used on the instance method '{1}'.", JSFunctionFlags.HasThisObject, this.Name));

            var parameters = this.Method.GetParameters();

            // If HasEngineParameter is specified, the first parameter must be of type ScriptEngine.
            if (this.HasEngineParameter == true)
            {
                if (parameters.Length == 0)
                    throw new InvalidOperationException(string.Format("The method '{0}' does not have enough parameters.", this.Name));
                if (parameters[0].ParameterType != typeof(ScriptEngine))
                    throw new InvalidOperationException(string.Format("The first parameter of the method '{0}' must be of type ScriptEngine.", this.Name));
            }

            // If there is a "this" parameter, it must be of type ObjectInstance (or derived from it).
            if (this.HasExplicitThisParameter == true)
            {
                if (parameters.Length <= (this.HasEngineParameter ? 1 : 0))
                    throw new InvalidOperationException(string.Format("The method '{0}' does not have enough parameters.", this.Name));
                this.ExplicitThisType = parameters[this.HasEngineParameter ? 1 : 0].ParameterType;
            }
            /*else if (method.IsStatic == false)
            {
                this.ThisType = method.DeclaringType;
            }*/

            // Calculate the min and max parameter count.

            // Check the parameter types (the this parameter has already been checked).
            // Only certain types are supported.
            int start = (this.HasExplicitThisParameter ? 1 : 0) + (this.HasEngineParameter ? 1 : 0);
            for (int i = start; i < parameters.Length; i++)
            {
                Type type = parameters[i].ParameterType;

                // ParamArray types must be an array of a supported type.
                if (this.HasParamArray == true && i == parameters.Length - 1)
                {
                    if (type.IsArray == false)
                        throw new NotImplementedException(string.Format("Unsupported varargs parameter type '{0}'.", type));
                    type = type.GetElementType();
                }

                if (type != typeof(bool) &&
                    type != typeof(int) &&
                    type != typeof(double) &&
                    type != typeof(string) &&
                    type != typeof(object) &&
                    typeof(ObjectInstance).IsAssignableFrom(type) == false)
                    throw new NotImplementedException(string.Format("Unsupported parameter type '{0}'.", type));
            }
        }



        //     SERIALIZATION
        //_________________________________________________________________________________________

#if !SILVERLIGHT

        /// <summary>
        /// Initializes a new instance of the FunctionBinderMethod class with serialized data.
        /// </summary>
        /// <param name="info"> The SerializationInfo that holds the serialized object data about
        /// the exception being thrown. </param>
        /// <param name="context"> The StreamingContext that contains contextual information about
        /// the source or destination. </param>
        protected JSBinderMethod(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            // Restore the flags.
            var flags = (JSFunctionFlags)info.GetInt32("flags");

            // Initialize the object.
            Init(flags);
        }

        /// <summary>
        /// Sets the SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info"> The SerializationInfo that holds the serialized object data about
        /// the exception being thrown. </param>
        /// <param name="context"> The StreamingContext that contains contextual information about
        /// the source or destination. </param>
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            // Save the base class state.
            base.GetObjectData(info, context);

            // Save the object state.
            info.AddValue("flags", this.Flags);
        }

#endif



        //     PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the flags that were passed to the constructor.
        /// </summary>
        public JSFunctionFlags Flags
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value that indicates whether the script engine should be passed as the first
        /// parameter.  Always false for instance methods.
        /// </summary>
        public bool HasEngineParameter
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value that indicates whether the "this" object should be passed as the first
        /// parameter (or the second parameter if HasEngineParameter is <c>true</c>).  Always false
        /// for instance methods.
        /// </summary>
        public bool HasExplicitThisParameter
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type of the explicit "this" value passed to this method.  Will be <c>null</c>
        /// if there is no explicit this value.
        /// </summary>
        public Type ExplicitThisType
        {
            get;
            private set;
        }

        ///// <summary>
        ///// Gets a value that indicates whether the "this" object should be passed to the method.
        ///// Always true for instance methods.
        ///// </summary>
        //public bool HasThisParameter
        //{
        //    get { return this.ThisType != null; }
        //}

        /// <summary>
        /// Gets the maximum number of parameters that this method requires (excluding the implicit
        /// this parameter).
        /// </summary>
        public int MaxParameterCount
        {
            get { return this.HasParamArray ? int.MaxValue : this.RequiredParameterCount + this.OptionalParameterCount; }
        }

        


        //     METHODS
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets an array of method parameters.
        /// </summary>
        /// <returns> An array of ParameterInfo instances describing the method parameters. </returns>
        protected override ParameterInfo[] GetParameters()
        {
            // Pull out the first and/or second parameters.
            var result = base.GetParameters();
            int offset = (this.HasEngineParameter ? 1 : 0) + (this.HasExplicitThisParameter ? 1 : 0);
            if (offset == 0)
                return result;
            ParameterInfo[] newArray = new ParameterInfo[result.Length - offset];
            Array.Copy(result, offset, newArray, 0, result.Length - offset);
            return newArray;
        }

        /// <summary>
        /// Gets an enumerable list of argument objects, equal in size to
        /// <paramref name="argumentCount"/>.
        /// </summary>
        /// <param name="argumentCount"> The number of arguments to return. </param>
        /// <returns> An enumerable list of argument objects. </returns>
        public override IEnumerable<BinderArgument> GetArguments(int argumentCount)
        {
            // If there is an engine parameter, return that first.
            if (this.HasEngineParameter == true)
                yield return new BinderArgument(BinderArgumentSource.ScriptEngine, typeof(ScriptEngine));

            // If there is an explicit this parameter, return that next.
            if (this.HasExplicitThisParameter == true)
                yield return new BinderArgument(BinderArgumentSource.ThisValue, this.ExplicitThisType);

            // Delegate to the base class.
            foreach (var arg in base.GetArguments(argumentCount))
                yield return arg;
        }
    }

}
