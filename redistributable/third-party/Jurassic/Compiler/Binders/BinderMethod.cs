using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Jurassic.Library;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a single method that a binder can call.
    /// </summary>
    [Serializable]
    internal class BinderMethod
#if !SILVERLIGHT
        : System.Runtime.Serialization.ISerializable
#endif
    {
        private bool initialized;
        private int requiredParameterCount;
        private int optionalParameterCount;
        private Type paramArrayElementType;



        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new BinderMethod instance.
        /// </summary>
        /// <param name="method"> The method to encapsulate. </param>
        public BinderMethod(MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            this.Method = method;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init()
        {
            var parameters = this.GetParameters();

            // Count the number of required and optional parameters.
            bool pastRequiredParams = false;
            for (int i = 0; i < parameters.Length; i++)
            {
                // Check if the last parameter is a ParamArray parameter.
                if (i == parameters.Length - 1 && Attribute.IsDefined(parameters[i], typeof(ParamArrayAttribute)) == true)
                {
                    if (parameters[i].ParameterType.IsArray == false)
                        throw new NotSupportedException("Parameters marked with [ParamArray] must be arrays.");
                    this.paramArrayElementType = parameters[i].ParameterType.GetElementType();
                }
                else if ((parameters[i].Attributes & ParameterAttributes.Optional) == ParameterAttributes.Optional)
                {
                    this.optionalParameterCount++;
                    pastRequiredParams = true;
                }
                else
                {
                    if (pastRequiredParams == true)
                        throw new NotSupportedException("Optional parameters are only supported at the end of the formal parameter list.");
                    this.requiredParameterCount++;
                }
            }

            // The class is now initialized.
            this.initialized = true;
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
        protected BinderMethod(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            // Get the type which declared the method.
            var typeName = info.GetString("methodType");
            var type = Type.GetType(typeName, true, false);

            // Get the method name.
            var methodName = info.GetString("methodName");

            // Get the method attributes and convert it into binding flags.
            var attributes = (MethodAttributes)info.GetInt32("methodAttributes");
            BindingFlags bindingFlags = 0;
            if ((attributes & MethodAttributes.Public) != 0)
                bindingFlags |= BindingFlags.Public;
            else
                bindingFlags |= BindingFlags.NonPublic;
            if ((attributes & MethodAttributes.Static) != 0)
                bindingFlags |= BindingFlags.Static;
            else
                bindingFlags |= BindingFlags.Instance;

            // Get the argument types.
            var argumentTypeNames = (string[])info.GetValue("methodArgumentTypes", typeof(string[]));
            var argumentTypes = new Type[argumentTypeNames.Length];
            for (int i = 0; i < argumentTypeNames.Length; i ++)
                argumentTypes[i] = Type.GetType(argumentTypeNames[i], true, false); 

            // Resolve the above information into a method.
            this.Method = type.GetMethod(methodName, bindingFlags, null, argumentTypes, null);
        }

        /// <summary>
        /// Sets the SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info"> The SerializationInfo that holds the serialized object data about
        /// the exception being thrown. </param>
        /// <param name="context"> The StreamingContext that contains contextual information about
        /// the source or destination. </param>
        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            // Save the object state.
            info.AddValue("methodType", this.Method.DeclaringType.AssemblyQualifiedName);
            info.AddValue("methodName", this.Method.Name);
            info.AddValue("methodAttributes", this.Method.Attributes);
            var parameters = this.Method.GetParameters();
            var argumentTypeNames = new string[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                argumentTypeNames[i] = parameters[i].ParameterType.AssemblyQualifiedName;
            info.AddValue("methodArgumentTypes", argumentTypeNames);
        }

#endif



        //     PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets a reference to the method.
        /// </summary>
        protected MethodBase Method
        {
            get;
            private set;
        }

        /// <summary>
        /// Implicitly cast an instance of this class to a MethodBase.
        /// </summary>
        /// <param name="method"> The BinderMethod instance. </param>
        /// <returns> A MethodBase instance. </returns>
        public static implicit operator MethodBase(BinderMethod method)
        {
            return method.Method;
        }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string Name
        {
            get { return this.Method.Name; }
        }

        /// <summary>
        /// Gets the type the method is declared on.
        /// </summary>
        public Type DeclaringType
        {
            get { return this.Method.DeclaringType; }
        }

        /// <summary>
        /// Gets the type of value pushed onto the stack after calling this method.
        /// </summary>
        public Type ReturnType
        {
            get
            {
                if (this.Method is MethodInfo)
                    return ((MethodInfo)this.Method).ReturnType;
                else if (this.Method is ConstructorInfo)
                    return this.DeclaringType;
                else
                    throw new NotImplementedException("Unsupported MethodBase type.");
            }
        }

        /// <summary>
        /// Gets the number of required parameters.
        /// </summary>
        public int RequiredParameterCount
        {
            get
            {
                if (this.initialized == false)
                    Init();
                return this.requiredParameterCount;
            }
        }

        /// <summary>
        /// Gets the number of optional parameters.
        /// </summary>
        public int OptionalParameterCount
        {
            get
            {
                if (this.initialized == false)
                    Init();
                return this.optionalParameterCount;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the last parameter is a ParamArray.
        /// </summary>
        public bool HasParamArray
        {
            get
            {
                if (this.initialized == false)
                    Init();
                return this.paramArrayElementType != null;
            }
        }

        /// <summary>
        /// Gets the type of element in the ParamArray array.
        /// </summary>
        private Type ParamArrayElementType
        {
            get
            {
                if (this.initialized == false)
                    Init();
                return this.paramArrayElementType;
            }
        }



        //     METHODS
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets an array of method parameters.
        /// </summary>
        /// <returns> An array of ParameterInfo instances describing the method parameters. </returns>
        protected virtual ParameterInfo[] GetParameters()
        {
            return this.Method.GetParameters();
        }

        /// <summary>
        /// Determines if this method can be called with the given number of arguments.
        /// </summary>
        /// <param name="argumentCount"> The desired number of arguments. </param>
        /// <returns> <c>true</c> if this method can be called with the given number of arguments;
        /// <c>false</c> otherwise. </returns>
        public bool IsArgumentCountCompatible(int argumentCount)
        {
            if (this.HasParamArray == true)
                return argumentCount >= this.RequiredParameterCount;
            return argumentCount >= this.RequiredParameterCount && argumentCount <= this.RequiredParameterCount + this.OptionalParameterCount;
        }

        /// <summary>
        /// Gets an enumerable list of argument objects, equal in size to
        /// <paramref name="argumentCount"/>.
        /// </summary>
        /// <param name="argumentCount"> The number of arguments to return. </param>
        /// <returns> An enumerable list of argument objects. </returns>
        public virtual IEnumerable<BinderArgument> GetArguments(int argumentCount)
        {
            if (IsArgumentCountCompatible(argumentCount) == false)
                throw new ArgumentException("This method cannot be called with the given number of arguments.", "argumentCount");

            // The first argument is the "this" object, if the method is an instance method.
            if (this.Method is MethodInfo && this.Method.IsStatic == false)
                yield return new BinderArgument(BinderArgumentSource.ThisValue, this.DeclaringType);

            int index = 0, paramIndex = 0;
            var parameters = this.GetParameters();

            // Return the required arguments.
            for (int i = 0; i < this.RequiredParameterCount; i++)
                yield return new BinderArgument(parameters[paramIndex++], index++);

            // Return the optional arguments.
            for (int i = 0; i < Math.Min(argumentCount - this.RequiredParameterCount, this.OptionalParameterCount); i++)
                yield return new BinderArgument(parameters[paramIndex++], index++);

            // Return the ParamArray arguments.
            for (int i = 0; i < argumentCount - this.OptionalParameterCount - this.RequiredParameterCount; i++)
                yield return new BinderArgument(BinderArgumentSource.InputParameter, parameters[paramIndex].ParameterType.GetElementType(), index++);
        }

        /// <summary>
        /// Gets an enumerable list of argument objects, equal in size to
        /// <paramref name="argumentCount"/> while generating code to prepare those arguments for
        /// a method call.
        /// </summary>
        /// <param name="argumentCount"> The number of arguments to return. </param>
        /// <param name="generator"> The IL generator used to create an array if the method has a
        /// ParamArray parameter. </param>
        /// <returns> An enumerable list of argument objects. </returns>
        public IEnumerable<BinderArgument> GenerateArguments(ILGenerator generator, int argumentCount)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");

            int paramArrayIndex = 0;
            foreach (var argument in this.GetArguments(argumentCount))
            {
                if (argument.IsParamArrayArgument == true)
                {
                    if (paramArrayIndex == 0)
                    {
                        // This is the start of the ParamArray arguments.
                        // Create an array.
                        int paramArraySize = Math.Max(0, argumentCount - this.OptionalParameterCount - this.RequiredParameterCount);
                        generator.LoadInt32(paramArraySize);
                        generator.NewArray(argument.Type);
                    }

                    // Load the array and index.
                    generator.Duplicate();
                    generator.LoadInt32(paramArrayIndex ++);
                }

                // Yield will have the side effect of generating a value.
                yield return argument;

                if (argument.IsParamArrayArgument == true)
                {
                    // Store the value in the ParamArray array.
                    generator.StoreArrayElement(argument.Type);
                }
            }

            // Populate any missing optional arguments with the default value.
            if (this.RequiredParameterCount + this.OptionalParameterCount - argumentCount > 0)
            {
                var parameters = this.GetParameters();
                for (int i = 0; i < this.RequiredParameterCount + this.OptionalParameterCount - argumentCount; i++)
                {
                    var optionalParameter = parameters[argumentCount + i];
                    if ((optionalParameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault)
                        // Emit the default value.
                        EmitHelpers.EmitValue(generator, new BinderArgument(optionalParameter, 0).DefaultValue);
                    else
                        // Emit default(T).
                        EmitHelpers.EmitDefaultValue(generator, optionalParameter.ParameterType);
                }
            }

            // Create an empty array if a ParamArray argument exists but no arguments were provided.
            if (this.HasParamArray == true && paramArrayIndex == 0)
            {
                // Create an array.
                generator.LoadInt32(0);
                generator.NewArray(this.ParamArrayElementType);
            }
        }

        /// <summary>
        /// Generates code to call the method.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        public void GenerateCall(ILGenerator generator)
        {
            if (this.Method is MethodInfo)
                generator.Call((MethodInfo)this.Method);
            else if (this.Method is ConstructorInfo)
                generator.NewObject((ConstructorInfo)this.Method);
            else
                throw new NotImplementedException("Unsupported MethodBase type.");
        }



        //     OBJECT OVERRIDES
        //_________________________________________________________________________________________

        /// <summary>
        /// Returns a string representing this object.
        /// </summary>
        /// <returns> A string representing this object. </returns>
        public override string ToString()
        {
            return this.Method.ToString();
        }
    }
    
    internal enum BinderArgumentSource
    {
        ScriptEngine,
        ThisValue,
        InputParameter,
    }

    /// <summary>
    /// Represents a single method argument.
    /// </summary>
    internal class BinderArgument
    {
        private ParameterInfo parameterInfo;

        internal BinderArgument(BinderArgumentSource source, Type type, int index = -1)
        {
            this.Source = source;
            this.InputParameterIndex = index;
            this.Type = type;
            this.IsParamArrayArgument = index >= 0;
        }

        internal BinderArgument(ParameterInfo parameterInfo, int index)
        {
            this.Source = BinderArgumentSource.InputParameter;
            this.InputParameterIndex = index;
            this.Type = parameterInfo.ParameterType;
            this.parameterInfo = parameterInfo;
        }

        /// <summary>
        /// Gets the intended source of the argument.
        /// </summary>
        public BinderArgumentSource Source
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the argument index, starting from zero.  Only valid if Source is InputParameter.
        /// </summary>
        public int InputParameterIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type of the argument.
        /// </summary>
        public Type Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value that indicates whether this argument will be rolled up into an array.
        /// </summary>
        public bool IsParamArrayArgument
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value that indicates whether this argument has a default value.
        /// </summary>
        public bool HasDefaultValue
        {
            get { return this.parameterInfo == null ? false : (this.parameterInfo.Attributes & ParameterAttributes.HasDefault) != 0; }
        }

        /// <summary>
        /// Gets the default value for this argument.
        /// </summary>
        public object DefaultValue
        {
            get
            {
                if (this.parameterInfo == null || this.HasDefaultValue == false)
                    return null;
                var attribute = GetCustomAttribute<DefaultParameterValueAttribute>();
                if (attribute == null)
                    throw new InvalidOperationException(string.Format("Expected [DefaultParameterValue] on parameter '{0}'.", this.parameterInfo.Name));
                if (attribute.Value == null && this.Type.IsValueType == true)
                    throw new InvalidOperationException(string.Format("Null is not a valid default value for parameter '{0}'.", this.parameterInfo.Name));
                if (attribute.Value != null && attribute.Value.GetType() != this.Type)
                    throw new InvalidOperationException(string.Format("Default value for parameter '{0}' should be '{1}'.", this.parameterInfo.Name, this.Type));
                return attribute.Value;
            }
        }

        /// <summary>
        /// Gets an attribute instance of the given type, if it exists on the argument.
        /// </summary>
        /// <typeparam name="T"> The type of attribute to retrieve. </typeparam>
        /// <returns> An attribute instance, or <c>null</c> if the attribute does not exist on the
        /// argument. </returns>
        public T GetCustomAttribute<T>() where T : Attribute
        {
            if (this.parameterInfo == null)
                return default(T);
            return (T)Attribute.GetCustomAttribute(this.parameterInfo, typeof(T));
        }
    }

}
