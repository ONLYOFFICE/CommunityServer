/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
