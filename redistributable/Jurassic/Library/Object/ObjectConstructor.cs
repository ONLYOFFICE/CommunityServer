using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the built-in javascript Object object.
    /// </summary>
    [Serializable]
    public class ObjectConstructor : ClrFunction
    {
        
        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new Object object.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="instancePrototype"> The prototype for instances created by this function. </param>
        internal ObjectConstructor(ObjectInstance prototype, ObjectInstance instancePrototype)
            : base(prototype, "Object", instancePrototype)
        {
        }



        //     JAVASCRIPT INTERNAL FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new Object instance.
        /// </summary>
        [JSConstructorFunction]
        public ObjectInstance Construct()
        {
            return ObjectInstance.CreateRawObject(this.InstancePrototype);
        }

        /// <summary>
        /// Converts the given argument to an object.
        /// </summary>
        /// <param name="obj"> The value to convert. </param>
        [JSConstructorFunction]
        public ObjectInstance Construct(object obj)
        {
            if (obj == null || obj == Undefined.Value || obj == Null.Value)
                return this.Engine.Object.Construct();
            return TypeConverter.ToObject(this.Engine, obj);
        }

        /// <summary>
        /// Converts the given argument to an object.
        /// </summary>
        /// <param name="obj"> The value to convert. </param>
        [JSCallFunction]
        public ObjectInstance Call(object obj)
        {
            if (obj == null || obj == Undefined.Value || obj == Null.Value)
                return this.Construct();
            return TypeConverter.ToObject(this.Engine, obj);
        }



        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Retrieves the next object in the prototype chain for the given object.
        /// </summary>
        /// <param name="obj"> The object to retrieve the prototype from. </param>
        /// <returns> The next object in the prototype chain for the given object, or <c>null</c>
        /// if the object has no prototype chain. </returns>
        [JSInternalFunction(Name = "getPrototypeOf")]
        public static object GetPrototypeOf([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj)
        {
            var result = obj.Prototype;
            if (result == null)
                return Null.Value;
            return result;
        }

        /// <summary>
        /// Gets an object that contains details of the property with the given name.
        /// </summary>
        /// <param name="obj"> The object to retrieve property details for. </param>
        /// <param name="propertyName"> The name of the property to retrieve details for. </param>
        /// <returns> An object containing some of the following properties: configurable,
        /// writable, enumerable, value, get and set. </returns>
        [JSInternalFunction(Name = "getOwnPropertyDescriptor")]
        public static ObjectInstance GetOwnPropertyDescriptor([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj, string propertyName)
        {
            var descriptor = obj.GetOwnPropertyDescriptor(propertyName);
            if (descriptor.Exists == false)
                return null;
            return descriptor.ToObject(obj.Engine);
        }

        /// <summary>
        /// Creates an array containing the names of all the properties on the object (even the
        /// non-enumerable ones).
        /// </summary>
        /// <param name="obj"> The object to retrieve the property names for. </param>
        /// <returns> An array containing the names of all the properties on the object. </returns>
        [JSInternalFunction(Name = "getOwnPropertyNames")]
        public static ArrayInstance GetOwnPropertyNames([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj)
        {
            var result = obj.Engine.Array.New();
            foreach (var property in ((ObjectInstance)obj).Properties)
                result.Push(property.Name);
            return result;
        }

        /// <summary>
        /// Creates an object with the given prototype and, optionally, a set of properties.
        /// </summary>
        /// <param name="prototype"> A reference to the next object in the prototype chain for the
        /// created object. </param>
        /// <param name="properties"> An object containing one or more property descriptors. </param>
        /// <returns> A new object instance. </returns>
        [JSInternalFunction(Name = "create", Flags = JSFunctionFlags.HasEngineParameter)]
        public static ObjectInstance Create(ScriptEngine engine, object prototype, [DefaultParameterValue(null)] ObjectInstance properties = null)
        {
            if ((prototype is ObjectInstance) == false && prototype != Null.Value)
                throw new JavaScriptException(engine, "TypeError", "object prototype must be an object or null");
            ObjectInstance result;
            if (prototype == Null.Value)
                result = ObjectInstance.CreateRootObject(engine);
            else
                result = ObjectInstance.CreateRawObject((ObjectInstance)prototype);
            if (properties != null)
                DefineProperties(result, properties);
            return result;
        }

        /// <summary>
        /// Modifies the value and attributes of a property.
        /// </summary>
        /// <param name="obj"> The object to define the property on. </param>
        /// <param name="propertyName"> The name of the property to modify. </param>
        /// <param name="attributes"> A property descriptor containing some of the following
        /// properties: configurable, writable, enumerable, value, get and set. </param>
        /// <returns> The object with the property. </returns>
        [JSInternalFunction(Name = "defineProperty")]
        public static ObjectInstance DefineProperty([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj, string propertyName, [JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance attributes)
        {
            var defaults = obj.GetOwnPropertyDescriptor(propertyName);
            var descriptor = PropertyDescriptor.FromObject(attributes, defaults);
            obj.DefineProperty(propertyName, descriptor, true);
            return obj;
        }

        /// <summary>
        /// Modifies multiple properties on an object.
        /// </summary>
        /// <param name="obj"> The object to define the properties on. </param>
        /// <param name="properties"> An object containing one or more property descriptors. </param>
        /// <returns> The object with the properties. </returns>
        [JSInternalFunction(Name = "defineProperties")]
        public static ObjectInstance DefineProperties([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj, ObjectInstance properties)
        {
            foreach (var property in properties.Properties)
                if (property.IsEnumerable == true)
                {
                    var descriptor = property.Value;
                    if ((descriptor is ObjectInstance) == false)
                        throw new JavaScriptException(obj.Engine, "TypeError", "Invalid property descriptor");
                    DefineProperty(obj, property.Name, TypeConverter.ToObject(obj.Engine, descriptor as ObjectInstance));
                }
            return obj;
        }

        /// <summary>
        /// Prevents the addition or deletion of any properties on the given object.
        /// </summary>
        /// <param name="obj"> The object to modify. </param>
        /// <returns> The object that was affected. </returns>
        [JSInternalFunction(Name = "seal")]
        public static ObjectInstance Seal([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj)
        {
            var properties = new List<PropertyNameAndValue>();
            foreach (var property in obj.Properties)
                properties.Add(property);
            foreach (var property in properties)
            {
                obj.FastSetProperty(property.Name, property.Value,
                    property.Attributes & ~PropertyAttributes.Configurable, overwriteAttributes: true);
            }
            obj.IsExtensible = false;
            return obj;
        }

        /// <summary>
        /// Prevents the addition, deletion or modification of any properties on the given object.
        /// </summary>
        /// <param name="obj"> The object to modify. </param>
        /// <returns> The object that was affected. </returns>
        [JSInternalFunction(Name = "freeze")]
        public static ObjectInstance Freeze([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj)
        {
            var properties = new List<PropertyNameAndValue>();
            foreach (var property in obj.Properties)
                properties.Add(property);
            foreach (var property in properties)
            {
                obj.FastSetProperty(property.Name, property.Value,
                    property.Attributes & ~(PropertyAttributes.NonEnumerable), overwriteAttributes: true);
            }
            obj.IsExtensible = false;
            return obj;
        }

        /// <summary>
        /// Prevents the addition of any properties on the given object.
        /// </summary>
        /// <param name="obj"> The object to modify. </param>
        /// <returns> The object that was affected. </returns>
        [JSInternalFunction(Name = "preventExtensions")]
        public static ObjectInstance PreventExtensions([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj)
        {
            obj.IsExtensible = false;
            return obj;
        }

        /// <summary>
        /// Determines if addition or deletion of any properties on the object is allowed.
        /// </summary>
        /// <param name="obj"> The object to check. </param>
        /// <returns> <c>true</c> if properties can be added or at least one property can be
        /// deleted; <c>false</c> otherwise. </returns>
        [JSInternalFunction(Name = "isSealed")]
        public static bool IsSealed([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj)
        {
            foreach (var property in obj.Properties)
                if (property.IsConfigurable == true)
                    return false;
            return obj.IsExtensible == false;
        }

        /// <summary>
        /// Determines if addition, deletion or modification of any properties on the object is
        /// allowed.
        /// </summary>
        /// <param name="obj"> The object to check. </param>
        /// <returns> <c>true</c> if properties can be added or at least one property can be
        /// deleted or modified; <c>false</c> otherwise. </returns>
        [JSInternalFunction(Name = "isFrozen")]
        public static bool IsFrozen([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj)
        {
            foreach (var property in obj.Properties)
                if (property.IsConfigurable == true || property.IsWritable == true)
                    return false;
            return obj.IsExtensible == false;
        }

        /// <summary>
        /// Determines if addition of properties on the object is allowed.
        /// </summary>
        /// <param name="obj"> The object to check. </param>
        /// <returns> <c>true</c> if properties can be added to the object; <c>false</c> otherwise. </returns>
        [JSInternalFunction(Name = "isExtensible")]
        public static new bool IsExtensible([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj)
        {
            return obj.IsExtensible;
        }

        /// <summary>
        /// Creates an array containing the names of all the enumerable properties on the object.
        /// </summary>
        /// <param name="obj"> The object to retrieve the property names for. </param>
        /// <returns> An array containing the names of all the enumerable properties on the object. </returns>
        [JSInternalFunction(Name = "keys")]
        public static ArrayInstance Keys([JSParameter(JSParameterFlags.DoNotConvert)] ObjectInstance obj)
        {
            var result = obj.Engine.Array.New();
            foreach (var property in obj.Properties)
                if (property.IsEnumerable == true)
                    result.Push(property.Name);
            return result;
        }
    }
}
