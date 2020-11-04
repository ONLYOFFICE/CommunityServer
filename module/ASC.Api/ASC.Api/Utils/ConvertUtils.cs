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