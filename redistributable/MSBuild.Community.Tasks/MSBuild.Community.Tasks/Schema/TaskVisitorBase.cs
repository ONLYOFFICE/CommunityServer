//Copyright © 2006, Jonathan de Halleux
//http://blog.dotnetwiki.org/default,month,2005-07.aspx

using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;



namespace MSBuild.Community.Tasks.Schema
{
    internal abstract class TaskVisitorBase<T> 
        where T : ITask
    {
        private T parent;
        private Assembly taskAssembly;
        public TaskVisitorBase(T parent, Assembly taskAssembly )
        {
            this.parent = parent;
            this.taskAssembly = taskAssembly;
        }

        public T Parent
        {
            get { return this.parent; }
        }

        public Assembly TaskAssembly
        {
            get { return this.taskAssembly; }
            set { this.taskAssembly = value; }
        }

        protected IEnumerable<Type> GetTaskTypes()
        {
            Type[] types = this.TaskAssembly.GetExportedTypes();
            Array.Sort<Type>(types, CompareType);

            foreach (Type type in types)
            {
                if (!typeof(ITask).IsAssignableFrom(type) || type.IsAbstract)
                    continue;
                yield return type;
            }
        }

        protected IEnumerable<PropertyInfo> GetProperties(Type taskType)
        {
            PropertyInfo[] properties = taskType.GetProperties();
            Array.Sort<PropertyInfo>(properties, ComparePropertyInfo);

            foreach (PropertyInfo property in properties)
            {
                if (property.DeclaringType == typeof(Task))
                    continue;
                yield return property;
            }
        }

        static int CompareType(Type x, Type y)
        {
            return x.FullName.CompareTo(y.FullName);
        }

        static int ComparePropertyInfo(PropertyInfo x, PropertyInfo y)
        {
            return x.Name.CompareTo(y.Name);
        }

    }
}
