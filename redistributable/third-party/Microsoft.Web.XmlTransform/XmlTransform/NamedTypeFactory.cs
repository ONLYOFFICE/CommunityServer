using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace Microsoft.Web.XmlTransform
{
    internal class NamedTypeFactory
    {
        private string relativePathRoot;
        private List<Registration> registrations = new List<Registration>();

        internal NamedTypeFactory(string relativePathRoot) {
            this.relativePathRoot = relativePathRoot;

            CreateDefaultRegistrations();
        }

        private void CreateDefaultRegistrations() {
            AddAssemblyRegistration(GetType().Assembly, GetType().Namespace);
        }

        internal void AddAssemblyRegistration(Assembly assembly, string nameSpace) {
            registrations.Add(new Registration(assembly, nameSpace));
        }

        internal void AddAssemblyRegistration(string assemblyName, string nameSpace) {
            registrations.Add(new AssemblyNameRegistration(assemblyName, nameSpace));
        }

        internal void AddPathRegistration(string path, string nameSpace) {
            if (!Path.IsPathRooted(path)) {
                // Resolve a relative path
                path = Path.Combine(Path.GetDirectoryName(relativePathRoot), path);
            }

            registrations.Add(new PathRegistration(path, nameSpace));
        }

        internal ObjectType Construct<ObjectType>(string typeName) where ObjectType : class {
            if (!String.IsNullOrEmpty(typeName)) {
                Type type = GetType(typeName);
                if (type == null) {
                    throw new XmlTransformationException(string.Format(System.Globalization.CultureInfo.CurrentCulture,SR.XMLTRANSFORMATION_UnknownTypeName, typeName, typeof(ObjectType).Name));
                }
                else if (!type.IsSubclassOf(typeof(ObjectType))) {
                    throw new XmlTransformationException(string.Format(System.Globalization.CultureInfo.CurrentCulture,SR.XMLTRANSFORMATION_IncorrectBaseType, type.FullName, typeof(ObjectType).Name));
                }
                else {
                    ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor == null) {
                        throw new XmlTransformationException(string.Format(System.Globalization.CultureInfo.CurrentCulture,SR.XMLTRANSFORMATION_NoValidConstructor, type.FullName));
                    }
                    else {
                        return constructor.Invoke(new object[] { }) as ObjectType;
                    }
                }
            }

            return null;
        }

        private Type GetType(string typeName) {
            Type foundType = null;
            foreach (Registration registration in registrations) {
                if (registration.IsValid) {
                    Type regType = registration.Assembly.GetType(String.Concat(registration.NameSpace, ".", typeName));
                    if (regType != null) {
                        if (foundType == null) {
                            foundType = regType;
                        }
                        else {
                            throw new XmlTransformationException(string.Format(System.Globalization.CultureInfo.CurrentCulture,SR.XMLTRANSFORMATION_AmbiguousTypeMatch, typeName));
                        }
                    }
                }
            }
            return foundType;
        }

        private class Registration
        {
            private Assembly assembly = null;
            private string nameSpace;

            public Registration(Assembly assembly, string nameSpace) {
                this.assembly = assembly;
                this.nameSpace = nameSpace;
            }

            public bool IsValid {
                get {
                    return assembly != null;
                }
            }

            public string NameSpace {
                get {
                    return nameSpace;
                }
            }

            public Assembly Assembly {
                get {
                    return assembly;
                }
            }
        }

        private class AssemblyNameRegistration : Registration
        {
            public AssemblyNameRegistration(string assemblyName, string nameSpace)
                : base(Assembly.Load(assemblyName), nameSpace) {
            }
        }

        private class PathRegistration : Registration
        {
            public PathRegistration(string path, string nameSpace)
                : base(Assembly.LoadFile(path), nameSpace) {
            }
        }
    }
}
