using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    [Flags]
    public enum PropertyAttributes
    {
        /// <summary>
        /// Indicates the property value is not writable, enumerable or configurable.  This is
        /// the default for built-in properties.
        /// </summary>
        Sealed = 0,

        /// <summary>
        /// Indicates the property value is writable.  If this flag is not set, attempting to
        /// modify the property will fail.  Not used if the property is an accessor property.
        /// </summary>
        Writable = 1,

        /// <summary>
        /// Indicates the property will be enumerated by a for-in enumeration. Otherwise, the
        /// property is said to be non-enumerable.
        /// </summary>
        Enumerable = 2,

        /// <summary>
        /// Indicates the property can be deleted or changed to an accessor property or have it's
        /// flags changed.
        /// </summary>
        Configurable = 4,

        /// <summary>
        /// Indicates the property can be modified and deleted but will not be enumerated.
        /// </summary>
        NonEnumerable = Writable | Configurable,

        /// <summary>
        /// Indicates the property is read-write, enumerable and configurable.  This is the default
        /// for user-created properties.
        /// </summary>
        FullAccess = Writable | Enumerable | Configurable,

        /// <summary>
        /// Indicates the property is an accessor property (i.e. it has a getter or a setter).
        /// </summary>
        IsAccessorProperty = 8,

        /// <summary>
        /// Indicates the property is the "magic" length property (only found on arrays).
        /// </summary>
        IsLengthProperty = 16,
    }
}
