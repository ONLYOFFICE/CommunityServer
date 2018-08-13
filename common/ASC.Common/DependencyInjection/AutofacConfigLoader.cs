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
using System.Configuration;
using Autofac;

namespace ASC.Common.DependencyInjection
{
    public class AutofacConfigLoader
    {
        public static ContainerBuilder Load(string containerName)
        {
            var container = new ContainerBuilder();

            var autofacConfigurationSection = (AutofacConfigurationSection) ConfigurationManager.GetSection("autofac");

            foreach (var component in autofacConfigurationSection.GetComponents(containerName))
            {
                var builder = container.RegisterType(Type.GetType(component.Type)).AsSelf();

                if (!string.IsNullOrEmpty(component.Service))
                {
                    var serviceType = Type.GetType(component.Service);
                    builder.As(serviceType);

                    if (!string.IsNullOrEmpty(component.Name))
                    {
                        builder.Named(component.Name, serviceType);
                    }
                }

                if (component.Parameters != null && component.Parameters.Count > 0)
                {
                    foreach (var parameter in component.Parameters)
                    {
                        builder.WithParameter(new NamedParameter(parameter.Name, parameter.Value));
                    }
                }

                switch (component.InstanceScope)
                {
                    case "single-instance":
                        builder.SingleInstance();
                        break;
                    case "per-lifetime-scope":
                        builder.InstancePerLifetimeScope();
                        break;
                    case "per-request":
                        builder.InstancePerRequest();
                        break;
                }

                if (component.InjectProperties == "yes")
                {
                    builder.PropertiesAutowired();
                }
            }

            return container;
        }
    }
}
