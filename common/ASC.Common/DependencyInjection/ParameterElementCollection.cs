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
