using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MSBuild.Community.Tasks.DependencyGraph
{
    /// <summary>
    /// Very simple parser that gets reference and assembly name information from project files
    /// </summary>
    public class ProjectFileParser
    {
        private XmlDocument _xml;

        private XmlElement DocumentElement
        {
            get { return _xml.DocumentElement ?? _xml.CreateElement("root"); }
        }

        /// <summary>
        /// Creates new parser, based on project file specified by stream
        /// </summary>
        /// <param name="stream">A stream pointing to the project file content</param>
        public ProjectFileParser(Stream stream)
        {
            ParseXml(stream);
        }

        private void ParseXml(Stream stream)
        {
            _xml = new XmlDocument();
            _xml.Load(stream);            
        }

        /// <summary>
        /// Returns the Assembly Name for the project file
        /// </summary>
        /// <returns></returns>
        public string GetAssemblyName()
        {
            return GetProperty("AssemblyName");
        }

        /// <summary>
        /// Returns the ProjectGuid for the project file
        /// </summary>
        /// <returns></returns>
        public string GetGuid()
        {
            return GetProperty("ProjectGuid");
        }

        private string GetProperty(string name)
        {
            var firstOrDefault = DocumentElement.ChildNodes
                .Cast<XmlNode>()
                .Where(n => n.Name == "PropertyGroup")
                .SelectMany(n => n.ChildNodes.Cast<XmlNode>())
                .FirstOrDefault(n => n.Name == name);

            return firstOrDefault != null ? firstOrDefault.InnerText : string.Empty;
        }

        /// <summary>
        /// Return referenced assemblies
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AssemblyReference> GetAssemblyReferences()
        {
            return GetReference("Reference", NodeToAssemblyReference);
        }

        /// <summary>
        /// Return referenced projects
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ProjectReference> GetProjectReferences()
        {
            return GetReference("ProjectReference", NodeToProjectReference);
        }

        private IEnumerable<T> GetReference<T>(string referenceName, Func<XmlNode, T> converter)
        {
            return DocumentElement.ChildNodes
                .Cast<XmlNode>()
                .Where(n => n.Name == "ItemGroup")
                .SelectMany(n => n.ChildNodes.Cast<XmlNode>())
                .Where(n => n.Name == referenceName)
                .Select(converter)
                .Where(reference => reference != null)
                .ToList();
        }

        private AssemblyReference NodeToAssemblyReference(XmlNode node)
        {
            var include = SafeAttributeValue(node, "Include");
            var hintPath = SafeNodeValue(node, "HintPath");

            return new AssemblyReference(include, hintPath);
        }

        private ProjectReference NodeToProjectReference(XmlNode node)
        {
            var include = SafeAttributeValue(node, "Include");
            var project = SafeNodeValue(node, "Project");
            var name = SafeNodeValue(node, "Name");

            return new ProjectReference(include, project, name);
        }

        private string SafeAttributeValue(XmlNode parentNode, string attrName)
        {
            var attribute = parentNode.Attributes != null ? parentNode.Attributes[attrName] : null;
            return attribute != null ? attribute.InnerText : string.Empty;
        }

        private string SafeNodeValue(XmlNode parentNode, string nodeName)
        {
            var node = parentNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == nodeName);
            return node != null ? node.InnerText : string.Empty;
        }

        /// <summary>
        /// Given an assembly name in the form "Ionic.Zip.Reduced, Version=1.9.1.8, Culture=neutral, PublicKeyToken=edbe51ad942a3f5c, processorArchitecture=MSIL"
        /// returns only the Ionic.Zip.Reduced part.
        /// </summary>
        /// <param name="fullAssemblyName"></param>
        /// <returns></returns>
        public static string GetAssemblyNameFromFullName(string fullAssemblyName)
        {
            return fullAssemblyName.Split(',')[0].Trim();
        }
    }
}
