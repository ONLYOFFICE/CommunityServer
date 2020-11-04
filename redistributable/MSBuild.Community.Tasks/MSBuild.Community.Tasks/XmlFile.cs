//-----------------------------------------------------------------------
// <copyright file="XmlFile.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.Community.Tasks
{
    using System.Globalization;
    using System.Xml;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>AddAttribute</i> (<b>Required: </b>File, Element)</para>
    /// <para><i>AddElement</i> (<b>Required: </b>File, Element, ParentElement, Key, Value)</para>
    /// <para><i>RemoveAttribute</i> (<b>Required: </b>File, Element, Key)</para>
    /// <para><i>RemoveElement</i> (<b>Required: </b>File, Element, ParentElement)</para>
    /// <para><b>Remote Execution Support:</b> NA</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Project ToolsVersion="3.5" DefaultTargets="Default" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    ///     <PropertyGroup>
    ///         <TPath>$(MSBuildProjectDirectory)\..\MSBuild.ExtensionPack.tasks</TPath>
    ///         <TPath Condition="Exists('$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks')">$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks</TPath>
    ///     </PropertyGroup>
    ///     <Import Project="$(TPath)"/>
    ///     <ItemGroup>
    ///         <ConfigSettingsToDeploy Include="c:\machine.config">
    ///             <Action>RemoveElement</Action>
    ///             <Element>processModel</Element>
    ///             <ParentElement>/configuration/system.web</ParentElement>
    ///         </ConfigSettingsToDeploy>
    ///         <ConfigSettingsToDeploy Include="c:\machine.config">
    ///             <Action>AddElement</Action>
    ///             <Element>processModel</Element>
    ///             <ParentElement>/configuration/system.web</ParentElement>
    ///         </ConfigSettingsToDeploy>
    ///         <ConfigSettingsToDeploy Include="c:\machine.config">
    ///             <Action>AddAttribute</Action>
    ///             <Key>enable</Key>
    ///             <ValueToAdd>true</ValueToAdd>
    ///             <Element>/configuration/system.web/processModel</Element>
    ///         </ConfigSettingsToDeploy>
    ///         <ConfigSettingsToDeploy Include="c:\machine.config">
    ///             <Action>AddAttribute</Action>
    ///             <Key>timeout</Key>
    ///             <ValueToAdd>Infinite</ValueToAdd>
    ///             <Element>/configuration/system.web/processModel</Element>
    ///         </ConfigSettingsToDeploy>
    ///         <ConfigSettingsToDeploy Include="c:\machine.config">
    ///             <Action>RemoveAttribute</Action>
    ///             <Key>timeout</Key>
    ///             <Element>/configuration/system.web/processModel</Element>
    ///         </ConfigSettingsToDeploy>
    ///     </ItemGroup>
    ///     <Target Name="Default">
    ///         <MSBuild.ExtensionPack.Xml.XmlFile TaskAction="%(ConfigSettingsToDeploy.Action)" File="%(ConfigSettingsToDeploy.Identity)" Key="%(ConfigSettingsToDeploy.Key)" Value="%(ConfigSettingsToDeploy.ValueToAdd)" Element="%(ConfigSettingsToDeploy.Element)" ParentElement="%(ConfigSettingsToDeploy.ParentElement)" Condition="'%(ConfigSettingsToDeploy.Identity)'!=''"/>
    ///     </Target>
    /// </Project>
    /// ]]></code>    
    /// </example>
    public class XmlFile : Task
    {
        private const string AddAttributeTaskAction = "AddAttribute";
        private const string AddElementTaskAction = "AddElement";
        private const string RemoveAttributeTaskAction = "RemoveAttribute";
        private const string RemoveElementTaskAction = "RemoveElement";
        private XmlDocument xmlFileDoc;
        private XmlNamespaceManager nsManager;

        /// <summary>
        /// 
        /// </summary>
        public string TaskAction
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the element.
        /// </summary>
        [Required]
        public string Element { get; set; }

        /// <summary>
        /// Sets the parent element.
        /// </summary>
        public string ParentElement { get; set; }

        /// <summary>
        /// Sets the key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Sets the key value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the default namespace.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the prefix to associate with the namespace being added.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Sets the file.
        /// </summary>
        [Required]
        public ITaskItem File { get; set; }

        /// <summary>
        /// Performs the action of this task.
        /// </summary>
        public override bool Execute()
        {
            if (!System.IO.File.Exists(File.ItemSpec))
            {
                Log.LogError("File not found: {0}", File.ItemSpec);
                return false;
            }

            xmlFileDoc = new XmlDocument();
            xmlFileDoc.Load(File.ItemSpec);
            if (!string.IsNullOrEmpty(Namespace))
            {
                nsManager = new XmlNamespaceManager(xmlFileDoc.NameTable);
                nsManager.AddNamespace(Prefix ?? string.Empty, Namespace);
            }

            switch (TaskAction)
            {
                case AddElementTaskAction:
                    AddElement();
                    return true;
                case AddAttributeTaskAction:
                    AddAttribute();
                    return true;
                case RemoveAttributeTaskAction:
                    RemoveAttribute();
                    return true;
                case RemoveElementTaskAction:
                    RemoveElement();
                    return true;
                default:
                    Log.LogError("Invalid TaskAction passed: {0}", TaskAction);
                    return false;
            }
        }

        private void RemoveAttribute()
        {
            Log.LogMessage("Remove Attribute: {0} from {1}", Key, File.ItemSpec);

            var elementNode = xmlFileDoc.SelectSingleNode(Element);
            if (elementNode == null)
            {
                Log.LogError("Element not found: {0}", Element);
                return;
            }

            var attNode = elementNode.Attributes.GetNamedItem(Key) as XmlAttribute;
            if (attNode != null)
            {
                elementNode.Attributes.Remove(attNode);
                xmlFileDoc.Save(File.ItemSpec);
            }
        }

        private void AddAttribute()
        {
            Log.LogMessage("Set Attribute: {0}={1} for {2}", Key, Value, File.ItemSpec);

            xmlFileDoc.Save(File.ItemSpec);

            var elementNode = SelectSingleNode(Element);
            if (elementNode == null)
            {
                Log.LogWarning(string.Format(CultureInfo.CurrentUICulture, "Element not found: {0}", Element));
                return;
            }

            var attNode = elementNode.Attributes.GetNamedItem(Key) as XmlAttribute;
            if (attNode == null)
            {
                attNode = xmlFileDoc.CreateAttribute(Key);
                attNode.Value = Value;
                elementNode.Attributes.Append(attNode);
            }
            else
            {
                attNode.Value = Value;
            }

            xmlFileDoc.Save(File.ItemSpec);
        }

        private void AddElement()
        {
            Log.LogMessage("Add Element: {0} to {1}", Element, File.ItemSpec);

            var parentNode = SelectSingleNode(ParentElement);
            if (parentNode == null)
            {
                Log.LogWarning("ParentElement not found: " + ParentElement);
                return;
            }

            // Ensure node does not already exist
            var newNode = SelectSingleNode(ParentElement + "/" + Element);
            if (newNode == null)
            {
                parentNode.AppendChild(xmlFileDoc.CreateElement(Element));
                xmlFileDoc.Save(File.ItemSpec);
            }
        }

        private void RemoveElement()
        {
            Log.LogMessage("Remove Element: {0} from {1}", Element, File.ItemSpec);

            var parentNode = SelectSingleNode(ParentElement);
            if (parentNode == null)
            {
                Log.LogWarning("ParentElement not found: " + ParentElement);
                return;
            }

            var nodeToRemove = SelectSingleNode(ParentElement + "/" + Element);
            if (nodeToRemove != null)
            {
                parentNode.RemoveChild(nodeToRemove);
                xmlFileDoc.Save(File.ItemSpec);
            }
        }

        private XmlNode SelectSingleNode(string path)
        {
            return nsManager == null ? xmlFileDoc.SelectSingleNode(path) : xmlFileDoc.SelectSingleNode(path, nsManager);
        }
    }
}