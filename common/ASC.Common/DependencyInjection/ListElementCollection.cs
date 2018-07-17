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
