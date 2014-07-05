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
    sealed class TaskSchemaAnalyser : TaskVisitorBase<TaskSchema>
    {
        private DocumentationLoader documentationLoader = new DocumentationLoader();
        private XmlSchema schema;
        private XmlSchemaType taskSchemaType;
        private XmlQualifiedName taskQualifiedName;
        private Dictionary<Type, XmlSchemaSimpleType> enumTypes = new Dictionary<Type, XmlSchemaSimpleType>();

        private XmlDocument usingDocument;

        public TaskSchemaAnalyser(TaskSchema parent, Assembly taskAssembly)
            : base(parent, taskAssembly)
        {
            //let's try loading the documentation
            if (!Parent.IgnoreDocumentation)
                this.documentationLoader.GetDocumentation(this.TaskAssembly);
        }

        public XmlSchema Schema
        {
            get { return this.schema; }
        }

        public XmlDocument UsingDocument
        {
            get { return this.usingDocument; }
        }

        public void CreateSchema()
        {
            this.schema = new XmlSchema();
            this.schema.TargetNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
            this.schema.ElementFormDefault = XmlSchemaForm.Qualified;
            this.schema.Namespaces.Add("xs", "http://www.w3.org/2001/XMLSchema");
            this.schema.Namespaces.Add("msb", "http://schemas.microsoft.com/developer/msbuild/2003");

            this.AddMsBuildInclude();
            this.AddIncludes();

            this.taskQualifiedName = new XmlQualifiedName("msb:Task");

            this.taskSchemaType = new XmlSchemaType();
            this.taskSchemaType.Name = "msb:TaskType";


            foreach (Type type in this.GetTaskTypes())
            {
                CreateTaskElement(type);
            }
        }

        private void AddMsBuildInclude()
        {
            if (this.Parent.IgnoreMsBuildSchema)
                return;
            // let's find msbuild
            string frameworkFolder = System.IO.Path.GetDirectoryName(typeof(Object).Assembly.Location);
            frameworkFolder = Path.Combine(frameworkFolder, @"MSBuild\Microsoft.Build.Commontypes.xsd");
            AddInclude(frameworkFolder);
        }

        private void AddInclude(string path)
        {
            XmlSchemaInclude include = new XmlSchemaInclude();
            include.SchemaLocation = path;
            this.schema.Includes.Add(include);
        }

        private void AddIncludes()
        {
            if (this.Parent.Includes != null)
            {
                foreach (ITaskItem item in this.Parent.Includes)
                {
                    string itemPath = item.ItemSpec;
                    this.Parent.Log.LogMessage("Adding include {0}", itemPath);
                    AddInclude(itemPath);
                }
            }
        }

        private void SetAssemblyInUsingTask(XmlElement el)
        {
            TaskListAssemblyFormatType ft = (TaskListAssemblyFormatType)
                Enum.Parse(typeof(TaskListAssemblyFormatType), this.Parent.TaskListAssemblyFormat);
            switch (ft)
            {
                case TaskListAssemblyFormatType.AssemblyFileName:
                    el.SetAttribute(
                        "AssemblyFile",
                        Path.GetFileName(this.TaskAssembly.Location));
                    break;
                case TaskListAssemblyFormatType.AssemblyFileFullPath:
                    el.SetAttribute(
                        "AssemblyFile",
                        this.TaskAssembly.Location
                        );
                    break;
                case TaskListAssemblyFormatType.AssemblyFullName:
                    el.SetAttribute(
                        "AssemblyName",
                        this.TaskAssembly.FullName);
                    break;
                case TaskListAssemblyFormatType.AssemblyName:
                    el.SetAttribute(
                        "AssemblyName",
                        this.TaskAssembly.GetName().Name);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void CreateUsingDocument()
        {
            this.usingDocument = new XmlDocument();
            XmlElement projectElement = this.usingDocument.CreateElement("Project");
            projectElement.SetAttribute("xmlns", "http://schemas.microsoft.com/developer/msbuild/2003");
            this.usingDocument.AppendChild(projectElement);

            foreach (Type type in this.GetTaskTypes())
            {
                XmlElement usingElement = this.usingDocument.CreateElement("UsingTask");
                projectElement.AppendChild(usingElement);
                usingElement.SetAttribute(
                    "TaskName", type.FullName);
                this.SetAssemblyInUsingTask(usingElement);
            }
        }

        private void CreateTaskElement(Type taskType)
        {
            Parent.Log.LogMessage("Analyzing {0}", taskType.FullName);

            XmlSchemaElement taskElement = new XmlSchemaElement();
            taskElement.Name = taskType.Name;
            taskElement.SubstitutionGroup = this.taskQualifiedName;
            this.schema.Items.Add(taskElement);

            // can we add documentation ?
            AnnotateTaskElement(taskType, taskElement);

            XmlSchemaComplexType taskElementType = new XmlSchemaComplexType();
            taskElement.SchemaType = taskElementType;
            taskElementType.ContentModel = new XmlSchemaComplexContent();

            XmlSchemaComplexContentExtension extension = new XmlSchemaComplexContentExtension();
            taskElementType.ContentModel.Content = extension;
            extension.BaseTypeName = new XmlQualifiedName("msb:TaskType");

            foreach (PropertyInfo property in this.GetProperties(taskType))
            {
                XmlSchemaAttribute attribute = new XmlSchemaAttribute();
                attribute.Name = property.Name;
                // is it required ?
                if (ReflectionHelper.IsRequired(property))
                    attribute.Use = XmlSchemaUse.Required;


                // can we add documentation ?
                AnnotateTaskAttribute(property, attribute);

                // can we specify the Enum type ?
                if (property.PropertyType == typeof(string))
                {
                    Type enumType = this.documentationLoader.GetEnumType(property);
                    if (enumType != null)
                    {
                        AddEnumRestriction(attribute, enumType);
                    }
                    else
                    {
                        SetTypeQualifiedName(attribute, property.PropertyType);
                    }
                }

                extension.Attributes.Add(attribute);
            }
        }

        private void AddEnumRestriction(XmlSchemaAttribute attribute, Type enumType)
        {
            XmlSchemaSimpleType simpleType;
            if (!this.enumTypes.TryGetValue(enumType, out simpleType))
            {
                simpleType = new XmlSchemaSimpleType();
                simpleType.Name = enumType.Name + "Type";

                // let's try to get the documentation
                XmlDocument enumDocumentation = null;
                if (!this.Parent.IgnoreDocumentation)
                    enumDocumentation = this.documentationLoader.GetDocumentation(enumType.Assembly);

                // we create an union around it...
                XmlSchemaSimpleTypeUnion union = new XmlSchemaSimpleTypeUnion();
                union.MemberTypes = new XmlQualifiedName[] { new XmlQualifiedName("msb:non_empty_string") };
                simpleType.Content = union;

                XmlSchemaSimpleType restrictionType = new XmlSchemaSimpleType();
                union.BaseTypes.Add(restrictionType);
                XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction();
                restrictionType.Content = restriction;
                restriction.BaseTypeName = new XmlQualifiedName("xs:string");

                foreach (string enumValue in Enum.GetNames(enumType))
                {
                    XmlSchemaEnumerationFacet facet = new XmlSchemaEnumerationFacet();
                    facet.Value = enumValue;
                    restriction.Facets.Add(facet);

                    if (enumDocumentation != null)
                        AnnotateFacet(enumType, enumValue, enumDocumentation, facet);
                }

                this.enumTypes.Add(enumType, simpleType);
                this.schema.Items.Add(simpleType);
            }

            attribute.SchemaTypeName = new XmlQualifiedName("msb:" + simpleType.Name);
        }

        private void AnnotateFacet(Type enumType, string enumValue, XmlDocument enumDocumentation, XmlSchemaFacet facet)
        {
            // let's find the element
            string xpath = string.Format(
                "//member[@name=\"F:{0}.{1}\"]",
                enumType.FullName,
                enumValue);
            XmlElement el = enumDocumentation.SelectSingleNode(xpath) as XmlElement;
            if (el == null)
                return;

            facet.Annotation = new XmlSchemaAnnotation();
            XmlSchemaDocumentation doc = new XmlSchemaDocumentation();
            facet.Annotation.Items.Add(doc);
            XmlText text = enumDocumentation.CreateTextNode(el.InnerText);
            doc.Markup = new XmlNode[] { text };
        }

        private void AnnotateTaskElement(Type taskType, XmlSchemaAnnotated taskElement)
        {
            if (!this.documentationLoader.HasDocumentation(taskType.Assembly))
                return;
            // let's find the element
            string summary = this.documentationLoader.GetTypeSummary(taskType);
            if (string.IsNullOrEmpty(summary))
                return;

            taskElement.Annotation = new XmlSchemaAnnotation();
            XmlSchemaDocumentation doc = new XmlSchemaDocumentation();
            taskElement.Annotation.Items.Add(doc);
            XmlText text = new XmlDocument().CreateTextNode(summary);
            if (ReflectionHelper.HasAttribute<ObsoleteAttribute>(taskType))
                text.Value = "[Obsolete] " + text.Value;
            doc.Markup = new XmlNode[] { text };
        }

        private void AnnotateTaskAttribute(PropertyInfo property, XmlSchemaAttribute taskAttribute)
        {
            if (!this.documentationLoader.HasDocumentation(property.DeclaringType.Assembly))
                return;

            // let's find the element
            string summary = this.documentationLoader.GetPropertySummary(property);
            if (string.IsNullOrEmpty(summary))
                return;

            taskAttribute.Annotation = new XmlSchemaAnnotation();
            XmlSchemaDocumentation doc = new XmlSchemaDocumentation();
            taskAttribute.Annotation.Items.Add(doc);
            SetTypeQualifiedName(taskAttribute, property.PropertyType);

            XmlText text = new XmlDocument().CreateTextNode(summary);
            if (ReflectionHelper.HasAttribute<ObsoleteAttribute>(property))
                text.Value = "[Obsolete] " + text.Value;
            if (ReflectionHelper.IsOutput(property))
                text.Value = "[Output] " + text.Value;
            if (!ReflectionHelper.IsRequired(property))
                text.Value = "[Optional] " + text.Value;
            doc.Markup = new XmlNode[] { text };
        }

        public ITaskItem WriteSchema(string schemaFileName)
        {
            this.Parent.Log.LogMessage("Creating Schema {0}", schemaFileName);
            using (StreamWriter writer = new StreamWriter(schemaFileName))
            {
                this.Schema.Write(writer);
            }

            return new TaskItem(schemaFileName);
        }

        public ITaskItem WriteUsingDocument(string usingDocumentName)
        {
            this.Parent.Log.LogMessage("Create Task list {0}", usingDocumentName);
            using (StreamWriter writer = new StreamWriter(usingDocumentName))
            {
                this.usingDocument.Save(writer);
            }

            return new TaskItem(usingDocumentName);
        }

        private void SetTypeQualifiedName(
            XmlSchemaAttribute attribute,
            Type type
            )
        {
            if (type == typeof(bool))
                attribute.SchemaTypeName = new XmlQualifiedName("msb:non_empty_string");
            else
            {
                if (attribute.Use == XmlSchemaUse.Required)
                    attribute.SchemaTypeName = new XmlQualifiedName("msb:non_empty_string");
                else
                    attribute.SchemaTypeName = new XmlQualifiedName("xs:string");
            }
        }
    }
}
