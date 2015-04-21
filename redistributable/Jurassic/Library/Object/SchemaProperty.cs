using System;
using System.Collections.Generic;

namespace Jurassic.Library
{

    /// <summary>
    /// Represents the information stored about a property in the class schema.
    /// </summary>
    [Serializable]
    internal struct SchemaProperty
    {
        /// <summary>
        /// Creates a new SchemaProperty instance.
        /// </summary>
        /// <param name="index"> The index of the property in the
        /// <see cref="ObjectInstance.Values"/> array. </param>
        /// <param name="attributes"> The property attributes.  These attributes describe how the
        /// property can be modified. </param>
        public SchemaProperty(int index, PropertyAttributes attributes)
            : this()
        {
            this.Index = index;
            this.Attributes = attributes;
        }

        /// <summary>
        /// Gets a value that indicates that a property doesn't exist.
        /// </summary>
        public readonly static SchemaProperty Undefined = new SchemaProperty(-1, PropertyAttributes.Sealed);

        /// <summary>
        /// Gets the index of the property in the <see cref="ObjectInstance.Values"/> array.
        /// </summary>
        public int Index
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the property attributes.  These attributes describe how the property can
        /// be modified.
        /// </summary>
        public PropertyAttributes Attributes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value that indicates whether the property exists.
        /// </summary>
        public bool Exists
        {
            get { return Index >= 0; }
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
        /// Gets a value that indicates whether the value is computed using accessor functions.
        /// </summary>
        public bool IsAccessor
        {
            get { return (this.Attributes & PropertyAttributes.IsAccessorProperty) != 0; }
        }

        /// <summary>
        /// Gets a value that indicates whether the property is the magic length property.
        /// </summary>
        public bool IsLength
        {
            get { return (this.Attributes & PropertyAttributes.IsLengthProperty) != 0; }
        }
    }

}
