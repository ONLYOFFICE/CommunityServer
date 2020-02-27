/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
