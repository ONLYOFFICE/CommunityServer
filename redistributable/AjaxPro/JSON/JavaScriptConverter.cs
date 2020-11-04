/*
 * MS	06-05-30	removed this code
 * 
 * 
 * 
 */
//using System;
//using System.Text;
//using System.Collections;

//namespace AjaxPro
//{
//    /// <summary>
//    /// The two directions AJAX will convert objects.
//    /// </summary>
//    internal enum JavaScriptConverterDirectionType
//    {
//        Serialize,
//        Deserialize
//    }

//    /// <summary>
//    /// Provides methods to get converters for JSON strings or .NET objects.
//    /// </summary>
//    public class JavaScriptConverter
//    {
//        /// <summary>
//        /// Get a IJavaScriptConverter that will handle the serialization of the specified data type.
//        /// </summary>
//        /// <param name="t">The type to handle.</param>
//        /// <returns>Returns an instance of an IJavaScriptConverter.</returns>
//        public static IJavaScriptConverter GetSerializableConverter(Type t)
//        {
//            return Utility.Settings.JavaScriptConverters.GetConverter(t, JavaScriptConverterDirectionType.Serialize);
//        }

//        /// <summary>
//        /// Get a IJavaScriptConverter that will handle the deserialization of the specified data type.
//        /// </summary>
//        /// <param name="t">The type to handle.</param>
//        /// <returns>Returns an instance of an IJavaScriptConverter.</returns>
//        public static IJavaScriptConverter GetDeserializableConverter(Type t)
//        {
//            return Utility.Settings.JavaScriptConverters.GetConverter(t, JavaScriptConverterDirectionType.Deserialize);
//        }
//    }
//}