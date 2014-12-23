using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents a property name and value.
    /// </summary>
    public sealed class PropertyNameAndValue
    {
        private string name;
        private PropertyDescriptor descriptor;

        public PropertyNameAndValue(string name, PropertyDescriptor descriptor)
        {
            this.name = name;
            this.descriptor = descriptor;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public object Value
        {
            get { return this.descriptor.Value; }
        }

        /// <summary>
        /// Gets the property attributes.  These attributes describe how the property can
        /// be modified.
        /// </summary>
        public PropertyAttributes Attributes
        {
            get { return this.descriptor.Attributes; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the property value can be set.
        /// </summary>
        public bool IsWritable
        {
            get { return this.descriptor.IsWritable; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the property value will be included during an
        /// enumeration.
        /// </summary>
        public bool IsEnumerable
        {
            get { return this.descriptor.IsEnumerable; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the property can be deleted.
        /// </summary>
        public bool IsConfigurable
        {
            get { return this.descriptor.IsConfigurable; }
        }
    }
}
