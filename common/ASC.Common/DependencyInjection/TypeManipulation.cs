/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace ASC.Common.DependencyInjection
{
    internal class TypeManipulation
    {
        public static object ChangeToCompatibleType(object value, Type destinationType, ICustomAttributeProvider memberInfo)
        {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");
            if (value == null)
            {
                if (!destinationType.IsValueType)
                    return null;
                return Activator.CreateInstance(destinationType);
            }
            if (memberInfo != null)
            {
                var converterAttribute = memberInfo.GetCustomAttributes(typeof(TypeConverterAttribute), true).Cast<TypeConverterAttribute>().FirstOrDefault();
                if (converterAttribute != null && !string.IsNullOrEmpty(converterAttribute.ConverterTypeName))
                {
                    var converterFromName = GetTypeConverterFromName(converterAttribute.ConverterTypeName);
                    if (converterFromName.CanConvertFrom(value.GetType()))
                        return converterFromName.ConvertFrom(value);
                }
            }
            var converter1 = TypeDescriptor.GetConverter(value.GetType());
            if (converter1.CanConvertTo(destinationType))
                return converter1.ConvertTo(value, destinationType);
            if (destinationType.IsInstanceOfType(value))
                return value;
            var converter2 = TypeDescriptor.GetConverter(destinationType);
            if (converter2.CanConvertFrom(value.GetType()))
                return converter2.ConvertFrom(value);
            if (value is string)
            {
                var method = destinationType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public);
                if (method != null)
                {
                    var parameters = new[] { value, null };
                    if ((bool)method.Invoke(null, parameters))
                        return parameters[1];
                }
            }
            throw new ConfigurationErrorsException("");
        }

        private static TypeConverter GetTypeConverterFromName(string converterTypeName)
        {
            var typeConverter = Activator.CreateInstance(Type.GetType(converterTypeName, true)) as TypeConverter;
            if (typeConverter == null)
                throw new ConfigurationErrorsException("");
            return typeConverter;
        }
    }
}
