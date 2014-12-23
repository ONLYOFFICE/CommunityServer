using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents either a named data property, or a named accessor property.
    /// </summary>
    public struct PropertyDescriptor
    {
        private PropertyAttributes attributes;
        private object value;

        /// <summary>
        /// Creates a new PropertyDescriptor instance.
        /// </summary>
        /// <param name="value"> The initial value for the property. </param>
        /// <param name="attributes"> The property attributes. </param>
        public PropertyDescriptor(object value, PropertyAttributes attributes)
        {
            this.attributes = attributes;
            this.value = value;

            // If the property is an accessor property, the state of the writable flag is dependant
            // whether the setter function exists. 
            if (this.value is PropertyAccessorValue)
            {
                this.attributes |= PropertyAttributes.IsAccessorProperty;
                if (this.Setter != null)
                    this.attributes |= PropertyAttributes.Writable;
                else
                    this.attributes &= ~PropertyAttributes.Writable;
            }
        }

        /// <summary>
        /// Creates a new PropertyDescriptor instance with a getter function and, optionally, a
        /// setter function.
        /// </summary>
        /// <param name="getter"> The function to call to retrieve the property value. </param>
        /// <param name="setter"> The function to call to set the property value. </param>
        /// <param name="attributes"> The property attributes (whether the property is writable or
        /// not is implied by whether there is a setter function). </param>
        public PropertyDescriptor(FunctionInstance getter, FunctionInstance setter, PropertyAttributes attributes)
            : this(new PropertyAccessorValue(getter, setter), attributes)
        {
        }

        /// <summary>
        /// Used in several APIs to indicate that a property doesn't exist.
        /// </summary>
        internal static readonly PropertyDescriptor Undefined = new PropertyDescriptor(null, PropertyAttributes.Sealed);

        /// <summary>
        /// Gets a value that indicates whether the property exists.
        /// </summary>
        public bool Exists
        {
            get { return this.value != null; }
        }

        /// <summary>
        /// Gets the property attributes.  These attributes describe how the property can
        /// be modified.
        /// </summary>
        public PropertyAttributes Attributes
        {
            get { return this.attributes; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the property value can be set.
        /// </summary>
        public bool IsWritable
        {
            get { return (this.Attributes & PropertyAttributes.Writable) != 0; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the property value will be included during an
        /// enumeration.
        /// </summary>
        public bool IsEnumerable
        {
            get { return (this.Attributes & PropertyAttributes.Enumerable) != 0; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the property can be deleted.
        /// </summary>
        public bool IsConfigurable
        {
            get { return (this.Attributes & PropertyAttributes.Configurable) != 0; }
        }

        /// <summary>
        /// Gets the raw property value.  Does not call the get accessor, even if one is present.
        /// </summary>
        public object Value
        {
            get { return this.value; }
        }

        ///// <summary>
        ///// Gets the property value, calling the get accessor, if present.
        ///// </summary>
        ///// <param name="thisObject"> The context of the get accessor, if present. </param>
        ///// <returns> The property value. </returns>
        //public object GetValue(ObjectInstance thisObject)
        //{
        //    // Get the property value.
        //    object result = this.Value;

        //    // Deal with accessor properties.
        //    if (result is PropertyAccessorValue)
        //    {
        //        if (((PropertyAccessorValue)result).GetAccessor == null)
        //            return Undefined.Value;
        //        return ((PropertyAccessorValue)result).GetAccessor.CallLateBound(thisObject);
        //    }

        //    // Return the value.
        //    return result;
        //}

        /// <summary>
        /// Returns a string representing the current object.
        /// </summary>
        /// <returns> A string representing the current object. </returns>
        public override string ToString()
        {
            if (this.Value == null)
                return "null";
            return this.Value.ToString();
        }



        //     GET AND SET ACCESSORS
        //_________________________________________________________________________________________



        /// <summary>
        /// Gets a value that indicates whether the value is computed using accessor functions.
        /// </summary>
        public bool IsAccessor
        {
            get { return this.value is PropertyAccessorValue; }
        }

        /// <summary>
        /// Gets the function that is called when the property value is retrieved, assuming this
        /// property value is computed using accessor functions.  Returns <c>null</c> if the
        /// property is not a accessor property.
        /// </summary>
        public FunctionInstance Getter
        {
            get
            {
                var accessor = this.value as PropertyAccessorValue;
                if (accessor == null)
                    return null;
                return accessor.Getter;
            }
        }

        /// <summary>
        /// Gets the function that is called when the property value is modified, assuming this
        /// property value is computed using accessor functions.  Returns <c>null</c> if the
        /// property is not a accessor property.
        /// </summary>
        public FunctionInstance Setter
        {
            get
            {
                var accessor = this.value as PropertyAccessorValue;
                if (accessor == null)
                    return null;
                return accessor.Setter;
            }
        }



        //     OBJECT SERIALIZATION AND DESERIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a property descriptor from an object containing any of the following
        /// properties: configurable, writable, enumerable, value, get, set.
        /// </summary>
        /// <param name="obj"> The object to get the property values from. </param>
        /// <param name="defaults"> The values to use if the relevant value is not specified. </param>
        /// <returns> A PropertyDescriptor that corresponds to the object. </returns>
        public static PropertyDescriptor FromObject(ObjectInstance obj, PropertyDescriptor defaults)
        {
            if (obj == null)
                return PropertyDescriptor.Undefined;

            // Read configurable attribute.
            bool configurable = defaults.IsConfigurable;
            if (obj.HasProperty("configurable"))
                configurable = TypeConverter.ToBoolean(obj["configurable"]);

            // Read writable attribute.
            bool writable = defaults.IsWritable;
            if (obj.HasProperty("writable"))
                writable = TypeConverter.ToBoolean(obj["writable"]);

            // Read enumerable attribute.
            bool enumerable = defaults.IsEnumerable;
            if (obj.HasProperty("enumerable"))
                enumerable = TypeConverter.ToBoolean(obj["enumerable"]);

            // Read property value.
            object value = defaults.Value;
            if (obj.HasProperty("value"))
                value = obj["value"];

            // The descriptor is an accessor if get or set is present.
            bool isAccessor = false;

            // Read get accessor.
            FunctionInstance getter = defaults.Getter;
            if (obj.HasProperty("get"))
            {
                if (obj.HasProperty("value"))
                    throw new JavaScriptException(obj.Engine, "TypeError", "Property descriptors cannot have both 'get' and 'value' set");
                if (obj.HasProperty("writable"))
                    throw new JavaScriptException(obj.Engine, "TypeError", "Property descriptors with 'get' or 'set' defined must not have 'writable' set");
                if (obj["get"] is FunctionInstance)
                    getter = (FunctionInstance)obj["get"];
                else if (TypeUtilities.IsUndefined(obj["get"]) == true)
                    getter = null;
                else
                    throw new JavaScriptException(obj.Engine, "TypeError", "Property descriptor 'get' must be a function");
                isAccessor = true;
            }

            // Read set accessor.
            FunctionInstance setter = defaults.Setter;
            if (obj.HasProperty("set"))
            {
                if (obj.HasProperty("value"))
                    throw new JavaScriptException(obj.Engine, "TypeError", "Property descriptors cannot have both 'set' and 'value' set");
                if (obj.HasProperty("writable"))
                    throw new JavaScriptException(obj.Engine, "TypeError", "Property descriptors with 'get' or 'set' defined must not have 'writable' set");
                if (obj["set"] is FunctionInstance)
                    setter = (FunctionInstance)obj["set"];
                else if (TypeUtilities.IsUndefined(obj["set"]) == true)
                    setter = null;
                else
                    throw new JavaScriptException(obj.Engine, "TypeError", "Property descriptor 'set' must be a function");
                isAccessor = true;
            }

            // Build up the attributes enum.
            PropertyAttributes attributes = PropertyAttributes.Sealed;
            if (configurable == true)
                attributes |= PropertyAttributes.Configurable;
            if (writable == true)
                attributes |= PropertyAttributes.Writable;
            if (enumerable == true)
                attributes |= PropertyAttributes.Enumerable;

            // Either a value or an accessor is possible.
            object descriptorValue = value;
            if (isAccessor == true)
                descriptorValue = new PropertyAccessorValue(getter, setter);

            // Create the new property descriptor.
            return new PropertyDescriptor(descriptorValue, attributes);
        }

        /// <summary>
        /// Populates an object with the following properties: configurable, writable, enumerable,
        /// value, get, set.
        /// </summary>
        /// <param name="engine"> The script engine used to create a new object. </param>
        /// <returns> An object with the information in this property descriptor set as individual
        /// properties. </returns>
        public ObjectInstance ToObject(ScriptEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            var result = engine.Object.Construct();
            if (this.IsAccessor == false)
            {
                result["value"] = this.Value;
                result["writable"] = this.IsWritable;
            }
            else
            {
                result["get"] = this.Getter;
                result["set"] = this.Setter;
            }
            result["enumerable"] = this.IsEnumerable;
            result["configurable"] = this.IsConfigurable;
            return result;
        }
    }

}
