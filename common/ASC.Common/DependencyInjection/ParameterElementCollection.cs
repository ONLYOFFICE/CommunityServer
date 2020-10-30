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


using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;

namespace ASC.Common.DependencyInjection
{
    public class ParameterElementCollection : NamedConfigurationElementCollection<ParameterElement>
    {
        public ParameterElementCollection()
          : base("parameter", "name")
        {
        }

        public IEnumerable<Parameter> ToParameters()
        {
            foreach (var parameterElement in this)
            {
                var localParameter = parameterElement;
                yield return new ResolvedParameter((pi, c) => pi.Name == localParameter.Name, (pi, c) => TypeManipulation.ChangeToCompatibleType(localParameter.CoerceValue(), pi.ParameterType, (ICustomAttributeProvider)pi));
            }
        }
    }
}
