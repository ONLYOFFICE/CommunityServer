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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Reflection;

namespace ASC.Common.DependencyInjection
{
    [TypeConverter(typeof(DictionaryElementTypeConverter))]
    public class DictionaryElementCollection : ConfigurationElementCollection<ListItemElement>
    {
        public DictionaryElementCollection()
          : base("item")
        {
        }

        private class DictionaryElementTypeConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
            {
                var instantiableType = GetInstantiableType(destinationType);
                var elementCollection = value as DictionaryElementCollection;
                if (elementCollection == null || !(instantiableType != null))
                    return base.ConvertTo(context, culture, value, destinationType);
                var dictionary = (IDictionary)Activator.CreateInstance(instantiableType);
                var genericArguments = instantiableType.GetGenericArguments();
                foreach (var listItemElement in elementCollection)
                {
                    if (string.IsNullOrEmpty(listItemElement.Key))
                        throw new ConfigurationErrorsException("Key cannot be null in a dictionary element.");
                    var compatibleType1 = TypeManipulation.ChangeToCompatibleType(listItemElement.Key, genericArguments[0], null);
                    var compatibleType2 = TypeManipulation.ChangeToCompatibleType(listItemElement.Value, genericArguments[1], null);
                    dictionary.Add(compatibleType1, compatibleType2);
                }
                return dictionary;
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
            {
                return GetInstantiableType(destinationType) != null || base.CanConvertTo(context, destinationType);
            }

            private static Type GetInstantiableType(System.Type destinationType)
            {
                if (!typeof(IDictionary).IsAssignableFrom(destinationType) &&
                    (!destinationType.IsGenericType ||
                     !typeof(IDictionary<,>).IsAssignableFrom(destinationType.GetGenericTypeDefinition())))
                    return null;
                Type[] typeArray1;
                if (!destinationType.IsGenericType)
                    typeArray1 = new[]
                    {
                        typeof (string),
                        typeof (object)
                    };
                else
                    typeArray1 = destinationType.GetGenericArguments();
                var typeArray2 = typeArray1;
                if (typeArray2.Length != 2)
                    return null;
                var c = typeof(Dictionary<,>).MakeGenericType(typeArray2);
                return destinationType.IsAssignableFrom(c) ? c : null;
            }
        }
    }
}
