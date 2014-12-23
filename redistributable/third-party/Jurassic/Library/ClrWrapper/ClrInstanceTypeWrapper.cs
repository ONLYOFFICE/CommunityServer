using System;
using System.Collections.Generic;
using System.Reflection;
using Jurassic.Compiler;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the instance portion of a CLR type that cannot be exposed directly but instead
    /// must be wrapped.
    /// </summary>
    [Serializable]
    internal class ClrInstanceTypeWrapper : ObjectInstance
    {

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Retrieves a ClrInstanceTypeWrapper object from the cache, if possible, or creates it
        /// otherwise.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        /// <param name="type"> The CLR type to wrap. </param>
        public static ClrInstanceTypeWrapper FromCache(ScriptEngine engine, Type type)
        {
            if (!engine.EnableExposedClrTypes)
                throw new JavaScriptException(engine, "TypeError", "Unsupported type: CLR types are not supported.  Enable CLR types by setting the ScriptEngine's EnableExposedClrTypes property to true.");

            ClrInstanceTypeWrapper cachedInstance;
            if (engine.InstanceTypeWrapperCache.TryGetValue(type, out cachedInstance) == true)
                return cachedInstance;
            var newInstance = new ClrInstanceTypeWrapper(engine, type);
            engine.InstanceTypeWrapperCache.Add(type, newInstance);
            return newInstance;
        }

        /// <summary>
        /// Creates a new ClrInstanceTypeWrapper object.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        /// <param name="type"> The CLR type to wrap. </param>
        private ClrInstanceTypeWrapper(ScriptEngine engine, Type type)
            : base(engine, GetPrototypeObject(engine, type))
        {
            this.WrappedType = type;

            // Populate the fields, properties and methods.
            ClrStaticTypeWrapper.PopulateMembers(this, type, BindingFlags.Instance);
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
            return ClrInstanceTypeWrapper.FromCache(engine, type.BaseType);
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
