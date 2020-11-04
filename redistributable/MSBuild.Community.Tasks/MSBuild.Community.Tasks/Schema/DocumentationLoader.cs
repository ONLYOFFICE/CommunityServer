//Copyright © 2006, Jonathan de Halleux
//http://blog.dotnetwiki.org/default,month,2005-07.aspx

using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;




namespace MSBuild.Community.Tasks.Schema
{
    sealed class DocumentationLoader
    {
        private Dictionary<Assembly, XmlDocument> assemblyDocumentations = new Dictionary<Assembly, XmlDocument>();

        public Dictionary<Assembly, XmlDocument> AssemblyDocumentations
        {
            get { return this.assemblyDocumentations; }
        }

        public bool HasDocumentation(Assembly assembly)
        {
            return GetDocumentation(assembly) != null;
        }

        public XmlDocument GetDocumentation(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            XmlDocument document = null;
            if (this.AssemblyDocumentations.TryGetValue(assembly, out document))
                return document;

            // let's try to load the document
            string documentationLocation =
                assembly.Location.Substring(0, assembly.Location.Length - 3) + "xml";
            if (File.Exists(documentationLocation))
            {
                document = new XmlDocument();
                document.Load(documentationLocation);

                this.AssemblyDocumentations.Add(assembly, document);
            }
            else
                this.AssemblyDocumentations.Add(assembly, null);

            return document;
        }

        public XmlElement GetTypeDocumentation(Type type)
        {
            return GetDocumentationElement(type.Assembly, type.FullName, 'T');
        }

        public XmlElement GetPropertyDocumentation(PropertyInfo propertyInfo)
        {
            return GetDocumentationElement(
                propertyInfo.DeclaringType.Assembly,
                String.Format("{0}.{1}", propertyInfo.DeclaringType.FullName, propertyInfo.Name)
                , 'P');
        }

        public XmlElement GetFieldDocumentation(FieldInfo fieldInfo)
        {
            return GetDocumentationElement(
                fieldInfo.DeclaringType.Assembly,
                String.Format("{0}.{1}", fieldInfo.DeclaringType.FullName, fieldInfo.Name)
                , 'F');
        }

        private XmlElement GetDocumentationElement(Assembly assembly, string typeFullName, char nodeType)
        {
            XmlDocument document = this.GetDocumentation(assembly);
            if (document == null)
                return null;

            string xpath = string.Format("//member[@name=\"{0}:{1}\"]", nodeType, typeFullName);
            return document.SelectSingleNode(xpath) as XmlElement;
        }

        public string GetTypeSummary(Type type)
        {
            return GetDocumentationSummary(type.Assembly, type.FullName, 'T');
        }

        public string GetPropertySummary(PropertyInfo propertyInfo)
        {
            return GetDocumentationSummary(
                propertyInfo.DeclaringType.Assembly,
                String.Format("{0}.{1}", propertyInfo.DeclaringType.FullName, propertyInfo.Name)
                , 'P');
        }
        public string GetFieldSummary(FieldInfo fieldInfo)
        {
            return GetDocumentationSummary(
                fieldInfo.DeclaringType.Assembly,
                String.Format("{0}.{1}", fieldInfo.DeclaringType.FullName, fieldInfo.Name)
                , 'F');
        }

        private string GetDocumentationSummary(Assembly assembly, string typeFullName, char nodeType)
        {
            XmlElement el = this.GetDocumentationElement(assembly, typeFullName, nodeType);
            if (el == null)
                return null;
            else
            {
                XmlElement summary = el.SelectSingleNode("summary") as XmlElement;
                if (summary == null)
                    return null;
                else
                    return summary.InnerText.Trim();
            }
        }

        public Type GetEnumType(PropertyInfo property)
        {
            XmlElement el = this.GetPropertyDocumentation(property);
            if (el == null)
                return null;
            else
            {
                XmlElement enumElement = el.SelectSingleNode("enum") as XmlElement;
                if (enumElement == null)
                    return null;
                else
                {
                    // get cref attribute
                    string enumTypeName = enumElement.GetAttribute("cref");
                    if (String.IsNullOrEmpty(enumTypeName))
                        return null;
                    if (!enumTypeName.StartsWith("T"))
                        return null;

                    enumTypeName = enumTypeName.Substring(2).Trim();
                    // do we have an assembly name ?
                    string assemblyName = enumElement.GetAttribute("assembly-name");
                    if (!string.IsNullOrEmpty(assemblyName))
                    {
                        enumTypeName = Assembly.CreateQualifiedName(assemblyName, enumTypeName);
                    }

                    return Type.GetType(enumTypeName, false, false);
                }
            }
        }

    }
}
