using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the built-in JSON object.
    /// </summary>
    [Serializable]
    public class JSONObject : ObjectInstance
    {

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new JSON object.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        internal JSONObject(ObjectInstance prototype)
            : base(prototype)
        {
        }



        //     .NET ACCESSOR PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the internal class name of the object.  Used by the default toString()
        /// implementation.
        /// </summary>
        protected override string InternalClassName
        {
            get { return "JSON"; }
        }



        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Parses the JSON source text and transforms it into a value.
        /// </summary>
        /// <param name="text"> The JSON text to parse. </param>
        /// <param name="reviver"> A function that will be called for each value. </param>
        /// <returns> The value of the JSON text. </returns>
        [JSInternalFunction(Name = "parse", Flags = JSFunctionFlags.HasEngineParameter)]
        public static object Parse(ScriptEngine engine, string text, [DefaultParameterValue(null)] object reviver = null)
        {
            var parser = new JSONParser(engine, new JSONLexer(engine, new System.IO.StringReader(text)));
            parser.ReviverFunction = reviver as FunctionInstance;
            return parser.Parse();
        }

        /// <summary>
        /// Serializes a value into a JSON string.
        /// </summary>
        /// <param name="value"> The value to serialize. </param>
        /// <param name="replacer"> Either a function that can transform each value before it is
        /// serialized, or an array of the names of the properties to serialize. </param>
        /// <param name="spacer"> Either the number of spaces to use for indentation, or a string
        /// that is used for indentation. </param>
        /// <returns> The JSON string representing the value. </returns>
        [JSInternalFunction(Name = "stringify", Flags = JSFunctionFlags.HasEngineParameter)]
        public static string Stringify(ScriptEngine engine, object value, [DefaultParameterValue(null)] object replacer = null, [DefaultParameterValue(null)] object spacer = null)
        {
            var serializer = new JSONSerializer(engine);

            // The replacer object can be either a function or an array.
            serializer.ReplacerFunction = replacer as FunctionInstance;
            if (replacer is ArrayInstance)
            {
                var replacerArray = (ArrayInstance)replacer;
                var serializableProperties = new HashSet<string>(StringComparer.Ordinal);
                foreach (object elementValue in replacerArray.ElementValues)
                {
                    if (elementValue is string || elementValue is int || elementValue is double || elementValue is StringInstance || elementValue is NumberInstance)
                        serializableProperties.Add(TypeConverter.ToString(elementValue));
                }
                serializer.SerializableProperties = serializableProperties;
            }

            // The spacer argument can be the number of spaces or a string.
            if (spacer is NumberInstance)
                spacer = ((NumberInstance)spacer).Value;
            else if (spacer is StringInstance)
                spacer = ((StringInstance)spacer).Value;
            if (spacer is double)
                serializer.Indentation = new string(' ', Math.Max(Math.Min(TypeConverter.ToInteger((double)spacer), 10), 0));
            else if (spacer is int)
                serializer.Indentation = new string(' ', Math.Max(Math.Min(TypeConverter.ToInteger((int)spacer), 10), 0));
            else if (spacer is string)
                serializer.Indentation = ((string)spacer).Substring(0, Math.Min(((string)spacer).Length, 10));

            // Serialize the value.
            return serializer.Serialize(value);
        }

    }
}
