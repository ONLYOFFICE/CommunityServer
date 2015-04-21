using System;
using System.Collections.Generic;
using System.Reflection;
using Jurassic.Compiler;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the static portion of a CLR type that cannot be exposed directly but instead
    /// must be wrapped.
    /// </summary>
    [Serializable]
    internal class ClrStaticTypeWrapper : FunctionInstance
    {
        private ClrBinder constructBinder;




        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Retrieves a ClrStaticTypeWrapper object from the cache, if possible, or creates it
        /// otherwise.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        /// <param name="type"> The CLR type to wrap. </param>
        public static ClrStaticTypeWrapper FromCache(ScriptEngine engine, Type type)
        {
            if (!engine.EnableExposedClrTypes)
                throw new JavaScriptException(engine, "TypeError", "Unsupported type: CLR types are not supported.  Enable CLR types by setting the ScriptEngine's EnableExposedClrTypes property to true.");

            ClrStaticTypeWrapper cachedInstance;
            if (engine.StaticTypeWrapperCache.TryGetValue(type, out cachedInstance) == true)
                return cachedInstance;
            var newInstance = new ClrStaticTypeWrapper(engine, type);
            engine.StaticTypeWrapperCache.Add(type, newInstance);
            return newInstance;
        }

        /// <summary>
        /// Creates a new ClrStaticTypeWrapper object.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        /// <param name="type"> The CLR type to wrap. </param>
        /// <param name="flags"> <c>BindingFlags.Static</c> to populate static methods;
        /// <c>BindingFlags.Instance</c> to populate instance methods. </param>
        private ClrStaticTypeWrapper(ScriptEngine engine, Type type)
            : base(engine, GetPrototypeObject(engine, type))
        {
            this.WrappedType = type;

            // Pick up the public constructors, if any.
            var constructors = type.GetConstructors();
            if (constructors.Length > 0)
                this.constructBinder = new ClrBinder(constructors);
            else
            {
                // The built-in primitive types do not have constructors, but we still want to
                // allow their construction since there is no way to construct them otherwise.
                // Pretend that a constructor does exist.
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int32:
                        this.constructBinder = new ClrBinder(ReflectionHelpers.Convert_ToInt32_Double);
                        break;
                }
            }

            this.FastSetProperty("name", type.Name);
            if (this.constructBinder != null)
                this.FastSetProperty("length", this.constructBinder.FunctionLength);

            // Populate the fields, properties and methods.
            PopulateMembers(this, type, BindingFlags.Static);
        }
        
        /// <summary>
        /// Returns an object instance to serve as the next object in the prototype chain.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        /// <param name="type"> The CLR type to wrap. </param>
        /// <returns> The next object in the prototype chain. </returns>
        private static ObjectInstance GetPrototypeObject(ScriptEngine engine, Type type)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.BaseType == null)
                return null;
            return ClrStaticTypeWrapper.FromCache(engine, type.BaseType);
        }




        //     PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the .NET type this object represents.
        /// </summary>
        public Type WrappedType
        {
            get;
            private set;
        }




        //     JAVASCRIPT INTERNAL FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Calls this function, passing in the given "this" value and zero or more arguments.
        /// </summary>
        /// <param name="thisObject"> The value of the "this" keyword within the function. </param>
        /// <param name="argumentValues"> An array of argument values. </param>
        /// <returns> The value that was returned from the function. </returns>
        public override object CallLateBound(object thisObject, params object[] argumentValues)
        {
            throw new JavaScriptException(this.Engine, "TypeError", "CLR types cannot be called like methods");
        }

        /// <summary>
        /// Creates an object, using this function as the constructor.
        /// </summary>
        /// <param name="argumentValues"> An array of argument values. </param>
        /// <returns> The object that was created. </returns>
        public override ObjectInstance ConstructLateBound(params object[] argumentValues)
        {
            if (this.constructBinder == null)
                throw new JavaScriptException(this.Engine, "TypeError", string.Format("The type '{0}' has no public constructors", this.WrappedType));
            var result = this.constructBinder.Call(this.Engine, this, argumentValues);
            if (result is ObjectInstance)
                return (ObjectInstance)result;
            return new ClrInstanceWrapper(this.Engine, result);
        }




        //     METHODS
        //_________________________________________________________________________________________


        /// <summary>
        /// Populates the given object with properties, field and methods based on the given .NET
        /// type.
        /// </summary>
        /// <param name="target"> The object to populate. </param>
        /// <param name="type"> The .NET type to search for methods. </param>
        /// <param name="flags"> <c>BindingFlags.Static</c> to populate static methods;
        /// <c>BindingFlags.Instance</c> to populate instance methods. </param>
        internal static void PopulateMembers(ObjectInstance target, Type type, BindingFlags flags)
        {
            // Register static methods as functions.
            var methodGroups = new Dictionary<string, List<MethodBase>>();
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.DeclaredOnly | flags))
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Method:
                        MethodInfo method = (MethodInfo)member;
                        List<MethodBase> methodGroup;
                        if (methodGroups.TryGetValue(method.Name, out methodGroup) == true)
                            methodGroup.Add(method);
                        else
                            methodGroups.Add(method.Name, new List<MethodBase>() { method });
                        break;

                    case MemberTypes.Property:
                        PropertyInfo property = (PropertyInfo)member;
                        var getMethod = property.GetGetMethod();
                        ClrFunction getter = getMethod == null ? null : new ClrFunction(target.Engine.Function.InstancePrototype, new ClrBinder(getMethod));
                        var setMethod = property.GetSetMethod();
                        ClrFunction setter = setMethod == null ? null : new ClrFunction(target.Engine.Function.InstancePrototype, new ClrBinder(setMethod));
                        target.DefineProperty(property.Name, new PropertyDescriptor(getter, setter, PropertyAttributes.NonEnumerable), false);

                        // Property getters and setters also show up as methods, so remove them here.
                        // NOTE: only works if properties are enumerated after methods.
                        if (getMethod != null)
                            methodGroups.Remove(getMethod.Name);
                        if (setMethod != null)
                            methodGroups.Remove(setMethod.Name);
                        break;
 
                    case MemberTypes.Field:
                        FieldInfo field = (FieldInfo)member;
                        ClrFunction fieldGetter = new ClrFunction(target.Engine.Function.InstancePrototype, new FieldGetterBinder(field));
                        ClrFunction fieldSetter = new ClrFunction(target.Engine.Function.InstancePrototype, new FieldSetterBinder(field));
                        target.DefineProperty(field.Name, new PropertyDescriptor(fieldGetter, fieldSetter, PropertyAttributes.NonEnumerable), false);
                        break;

                    case MemberTypes.Constructor:
                    case MemberTypes.NestedType:
                    case MemberTypes.Event:
                    case MemberTypes.TypeInfo:
                        // Support not yet implemented.
                        break;
                }
                
            }
            foreach (var methodGroup in methodGroups.Values)
            {
                var binder = new ClrBinder(methodGroup);
                var function = new ClrFunction(target.Engine.Function.InstancePrototype, binder);
                target.FastSetProperty(binder.Name, function, PropertyAttributes.NonEnumerable, overwriteAttributes: true);
            }
        }




        //     OBJECT OVERRIDES
        //_________________________________________________________________________________________

        /// <summary>
        /// Returns a textual representation of this object.
        /// </summary>
        /// <returns> A textual representation of this object. </returns>
        public override string ToString()
        {
            return this.WrappedType.ToString();
        }
    }
}
