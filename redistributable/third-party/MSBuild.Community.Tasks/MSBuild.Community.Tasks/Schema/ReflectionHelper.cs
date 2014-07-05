//Copyright © 2006, Jonathan de Halleux
//http://blog.dotnetwiki.org/default,month,2005-07.aspx

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;



namespace MSBuild.Community.Tasks.Schema
{
    internal static class ReflectionHelper
    {
        public static T GetAttribute<T>(ICustomAttributeProvider t)
            where T : Attribute
        {
            object[] attributes = t.GetCustomAttributes(typeof(T), true);
            if (attributes != null && attributes.Length > 0)
                return (T)attributes[0];
            else
                return null;
        }

        public static bool HasAttribute<T>(ICustomAttributeProvider t)
            where T : Attribute
        {
            object[] attributes = t.GetCustomAttributes(typeof(T), true);
            return attributes != null && attributes.Length > 0;
        }

        public static bool IsOutput(ICustomAttributeProvider t)
        {
            return HasAttribute<OutputAttribute>(t);
        }

        public static bool IsRequired(ICustomAttributeProvider t)
        {
            return HasAttribute<RequiredAttribute>(t);
        }

        public static Assembly LoadAssembly(TaskLoggingHelper logger, ITaskItem assemblyItem)
        {
            try
            {
                string assemblyFullName = assemblyItem.GetMetadata("FullPath");
                Assembly taskAssembly = Assembly.LoadFrom(assemblyFullName);
                return taskAssembly;
            }
            catch (FileLoadException ex)
            {
                logger.LogErrorFromException(ex);
                return null;
            }
            catch (BadImageFormatException ex)
            {
                logger.LogErrorFromException(ex);
                return null;
            }
            catch (TypeLoadException ex)
            {
                logger.LogErrorFromException(ex);
                return null;
            }
        }
    }
}
