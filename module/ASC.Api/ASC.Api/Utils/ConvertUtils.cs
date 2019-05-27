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


using ASC.Api.Exceptions;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace ASC.Api.Utils
{
    public static class ConvertUtils
    {

        public static object GetConverted(string value, ParameterInfo parameterInfo)
        {
            try
            {
                return GetConverted(value, parameterInfo.ParameterType);
            }
            catch (ApiArgumentMismatchException e)
            {
                throw new ApiArgumentMismatchException(parameterInfo.Name,
                                                             parameterInfo.ParameterType, e);
            }
        }

        public static object GetConverted(string value, PropertyInfo propertyInfo)
        {
            try
            {
                return GetConverted(value, propertyInfo.PropertyType);
            }
            catch (ApiArgumentMismatchException e)
            {
                throw new ApiArgumentMismatchException(propertyInfo.Name,
                                                             propertyInfo.PropertyType, e);
            }
        }

        public static object GetConverted(string value, Type type)
        {
            try
            {
                return type == typeof(object) ? value : TypeDescriptor.GetConverter(type).ConvertFromString(new DummyTypeDescriptorContext(), CultureInfo.InvariantCulture, value);
            }
            catch (Exception e)
            {
                throw new ApiArgumentMismatchException(value,type,e);
            }
        }

        internal class DummyTypeDescriptorContext : ITypeDescriptorContext
        {
            private readonly IServiceProvider _serviceProvider = null;
            private readonly object _component = null;
            private readonly PropertyDescriptor _propDescriptor = null;

            public IContainer Container
            {
                get
                {
                    return (IContainer)null;
                }
            }

            public object Instance
            {
                get
                {
                    return this._component;
                }
            }

            public PropertyDescriptor PropertyDescriptor
            {
                get
                {
                    return this._propDescriptor;
                }
            }


            public void OnComponentChanged()
            {
            }

            public bool OnComponentChanging()
            {
                return true;
            }

            public object GetService(Type serviceType)
            {
                if (this._serviceProvider != null)
                    return this._serviceProvider.GetService(serviceType);
                else
                    return (object)null;
            }
        }
    }
}