using System;
using System.Globalization;
using Newtonsoft.Json;

namespace OpenStack.Serialization
{
    /// <summary>
    /// Acts like Json.NET's [JsonConverter] but allows for constructor arguments
    /// </summary>
    /// <exclude />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Parameter)]
    public sealed class JsonConverterWithConstructorAttribute : Attribute
    {
        private readonly Type _converterType;
        private readonly object[] _constructorArguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonConverterWithConstructorAttribute"/> class.
        /// </summary>
        /// <param name="converterType">Type of the converter.</param>
        /// <param name="constructorArguments">The constructor arguments.</param>
        /// <exception cref="ArgumentNullException">The converterType argument is null.</exception>
        /// <exception cref="ArgumentException">No constructor arguments were specified.</exception>
        public JsonConverterWithConstructorAttribute(Type converterType, params object[] constructorArguments)
        {
            if(converterType == null)
                throw new ArgumentNullException("converterType");
            if(constructorArguments.Length == 0)
                throw new ArgumentException("No constructor arguments were specified. If none are required, use JsonConverterAttribute instead", "constructorArguments");

            _converterType = converterType;
            _constructorArguments = constructorArguments;
        }

        /// <summary />
        public Type ConverterType
        {
            get { return _converterType; }
        }

        /// <summary />
        public object[] ConstructorArguments
        {
            get { return _constructorArguments; }
        }

        /// <summary>
        /// Creates the converter instance.
        /// </summary>
        /// <exception cref="Exception">The converter could not be instantiated.</exception>
        public JsonConverter CreateJsonConverterInstance()
        {
            try
            {
                return (JsonConverter)Activator.CreateInstance(_converterType, _constructorArguments);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "Error creating {0}", _converterType), ex);
            }
        }
    }
}