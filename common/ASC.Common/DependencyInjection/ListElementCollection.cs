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
using System.Globalization;

namespace ASC.Common.DependencyInjection
{
    [TypeConverter(typeof(ListElementTypeConverter))]
    public class ListElementCollection : ConfigurationElementCollection<ListItemElement>
    {
        public ListElementCollection()
          : base("item")
        {
        }

        private class ListElementTypeConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                var instantiableType = GetInstantiableType(destinationType);
                var elementCollection = (ListElementCollection) value;
                if (elementCollection == null || !(instantiableType != null))
                    return base.ConvertTo(context, culture, value, destinationType);
                var genericArguments = instantiableType.GetGenericArguments();
                var list = (IList)Activator.CreateInstance(instantiableType);
                foreach (var listItemElement in elementCollection)
                    list.Add(TypeManipulation.ChangeToCompatibleType(listItemElement.Value, genericArguments[0], null));
                return list;
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return GetInstantiableType(destinationType) != null || base.CanConvertTo(context, destinationType);
            }

            private static Type GetInstantiableType(Type destinationType)
            {
                if (!typeof(IEnumerable).IsAssignableFrom(destinationType)) return null;
                var typeArray1 = !destinationType.IsGenericType ? new[] { typeof(object) } : destinationType.GetGenericArguments();
                var typeArray2 = typeArray1;
                if (typeArray2.Length != 1)
                    return null;
                var c = typeof(List<>).MakeGenericType(typeArray2);
                return destinationType.IsAssignableFrom(c) ? c : null;
            }
        }
    }
}
