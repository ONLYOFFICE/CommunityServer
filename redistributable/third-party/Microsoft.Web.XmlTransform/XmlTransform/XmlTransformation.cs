using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Xml;
using System.IO;

namespace Microsoft.Web.XmlTransform
{
    public class XmlTransformation : IServiceProvider, IDisposable
    {
        internal static readonly string TransformNamespace = "http://schemas.microsoft.com/XML-Document-Transform";
        internal static readonly string SupressWarnings = "SupressWarnings";

        #region private data members
        private string transformFile;

        private XmlDocument xmlTransformation;
        private XmlDocument xmlTarget;
        private XmlTransformableDocument xmlTransformable;

        private XmlTransformationLogger logger = null;

        private NamedTypeFactory namedTypeFactory;
        private ServiceContainer transformationServiceContainer = new ServiceContainer();
        private ServiceContainer documentServiceContainer = null;

        private bool hasTransformNamespace = false;
        #endregion

        public XmlTransformation(string transformFile)
            : this(transformFile, true,  null) {
        }

        public XmlTransformation(string transform, IXmlTransformationLogger logger)
            : this(transform, true, logger)
        {
        }

        public XmlTransformation(string transform, bool isTransformAFile, IXmlTransformationLogger logger) {
            this.transformFile = transform;
            this.logger = new XmlTransformationLogger(logger);

            xmlTransformation = new XmlFileInfoDocument();
            if (isTransformAFile)
            {
                xmlTransformation.Load(transform);
            }
            else
            {
                xmlTransformation.LoadXml(transform);
            }

            InitializeTransformationServices();

            PreprocessTransformDocument();
        }

        public XmlTransformation(Stream transformStream, IXmlTransformationLogger logger)
        {
            this.logger = new XmlTransformationLogger(logger);
            this.transformFile = String.Empty;

            xmlTransformation = new XmlFileInfoDocument();
            xmlTransformation.Load(transformStream);

            InitializeTransformationServices();

            PreprocessTransformDocument();
        }

        public bool HasTransformNamespace
        {
            get
            {
                return hasTransformNamespace;
            }
        }

        private void InitializeTransformationServices() {
            // Initialize NamedTypeFactory
            namedTypeFactory = new NamedTypeFactory(transformFile);
            transformationServiceContainer.AddService(namedTypeFactory.GetType(), namedTypeFactory);

            // Initialize TransformationLogger
            transformationServiceContainer.AddService(logger.GetType(), logger);
        }

        private void InitializeDocumentServices(XmlDocument document) {
            Debug.Assert(documentServiceContainer == null);
            documentServiceContainer = new ServiceContainer();

            if (document is IXmlOriginalDocumentService) {
                documentServiceContainer.AddService(typeof(IXmlOriginalDocumentService), document);
            }
        }

        private void ReleaseDocumentServices() {
            if (documentServiceContainer != null) {
                documentServiceContainer.RemoveService(typeof(IXmlOriginalDocumentService));
                documentServiceContainer = null;
            }
        }

        private void PreprocessTransformDocument() {
            hasTransformNamespace = false;
            foreach (XmlAttribute attribute in xmlTransformation.SelectNodes("//namespace::*")) {
                if (attribute.Value.Equals(TransformNamespace, StringComparison.Ordinal)) {
                    hasTransformNamespace = true;
                    break;
                }
            }

            if (hasTransformNamespace) {
                // This will look for all nodes from our namespace in the document,
                // and do any initialization work
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
                namespaceManager.AddNamespace("xdt", TransformNamespace);
                XmlNodeList namespaceNodes = xmlTransformation.SelectNodes("//xdt:*", namespaceManager);

                foreach (XmlNode node in namespaceNodes) {
                    XmlElement element = node as XmlElement;
                    if (element == null) {
                        Debug.Fail("The XPath for elements returned something that wasn't an element?");
                        continue;
                    }

                    XmlElementContext context = null;
                    try {
                        switch (element.LocalName) {
                            case "Import":
                                context = CreateElementContext(null, element);
                                PreprocessImportElement(context);
                                break;
                            default:
                                logger.LogWarning(element, SR.XMLTRANSFORMATION_UnknownXdtTag, element.Name);
                                break;
                        }
                    }
                    catch (Exception ex) {
                        if (context != null) {
                            ex = WrapException(ex, context);
                        }

                        logger.LogErrorFromException(ex);
                        throw new XmlTransformationException(SR.XMLTRANSFORMATION_FatalTransformSyntaxError, ex);
                    }
                    finally {
                        context = null;
                    }
                }
            }
        }

        public void AddTransformationService(System.Type serviceType, object serviceInstance)
        {
            transformationServiceContainer.AddService(serviceType, serviceInstance);
        }

        public void RemoveTransformationService(System.Type serviceType)
        {
            transformationServiceContainer.RemoveService(serviceType);
        }

        public bool Apply(XmlDocument xmlTarget) {
            Debug.Assert(this.xmlTarget == null, "This method should not be called recursively");

            if (this.xmlTarget == null) {
                // Reset the error state
                logger.HasLoggedErrors = false;

                this.xmlTarget = xmlTarget;
                this.xmlTransformable = xmlTarget as XmlTransformableDocument;
                try {
                    if (hasTransformNamespace) {
                        InitializeDocumentServices(xmlTarget);

                        TransformLoop(xmlTransformation);
                    }
                    else {
                        logger.LogMessage(MessageType.Normal, "The expected namespace {0} was not found in the transform file", TransformNamespace);
                    }
                }
                catch (Exception ex) {
                    HandleException(ex);
                }
                finally {
                    ReleaseDocumentServices();

                    this.xmlTarget = null;
                    this.xmlTransformable = null;
                }

                return !logger.HasLoggedErrors;
            }
            else {
                return false;
            }
        }

        private void TransformLoop(XmlDocument xmlSource) {
            TransformLoop(new XmlNodeContext(xmlSource));
        }

        private void TransformLoop(XmlNodeContext parentContext) {
            foreach (XmlNode node in parentContext.Node.ChildNodes) {
                XmlElement element = node as XmlElement;
                if (element == null) {
                    continue;
                }

                XmlElementContext context = CreateElementContext(parentContext as XmlElementContext, element);
                try {
                    HandleElement(context);
                }
                catch (Exception ex) {
                    HandleException(ex, context);
                }
            }
        }

        private XmlElementContext CreateElementContext(XmlElementContext parentContext, XmlElement element) {
            return new XmlElementContext(parentContext, element, xmlTarget, this);
        }

        private void HandleException(Exception ex) {
            logger.LogErrorFromException(ex);
        }

        private void HandleException(Exception ex, XmlNodeContext context) {
            HandleException(WrapException(ex, context));
        }

        private Exception WrapException(Exception ex, XmlNodeContext context) {
            return XmlNodeException.Wrap(ex, context.Node);
        }

        private void HandleElement(XmlElementContext context) {
            string argumentString;
            Transform transform = context.ConstructTransform(out argumentString);
            if (transform != null) {

                bool fOriginalSupressWarning = logger.SupressWarnings;

                XmlAttribute SupressWarningsAttribute = context.Element.Attributes.GetNamedItem(XmlTransformation.SupressWarnings, XmlTransformation.TransformNamespace) as XmlAttribute;
                if (SupressWarningsAttribute != null)
                {
                    bool fSupressWarning = System.Convert.ToBoolean(SupressWarningsAttribute.Value, System.Globalization.CultureInfo.InvariantCulture);
                    logger.SupressWarnings = fSupressWarning;
                }

                try
                {
                    OnApplyingTransform();

                    transform.Execute(context, argumentString);

                    OnAppliedTransform();
                }
                catch (Exception ex)
                {
                    HandleException(ex, context);
                }
                finally
                {
                    // reset back the SupressWarnings back per node
                    logger.SupressWarnings = fOriginalSupressWarning;
                }
            }

            // process children
            TransformLoop(context);
        }

        private void OnApplyingTransform() {
            if (xmlTransformable != null) {
                xmlTransformable.OnBeforeChange();
            }
        }

        private void OnAppliedTransform() {
            if (xmlTransformable != null) {
                xmlTransformable.OnAfterChange();
            }
        }

        private void PreprocessImportElement(XmlElementContext context) {
            string assemblyName = null;
            string nameSpace = null;
            string path = null;

            foreach (XmlAttribute attribute in context.Element.Attributes) {
                if (attribute.NamespaceURI.Length == 0) {
                    switch (attribute.Name) {
                        case "assembly":
                            assemblyName = attribute.Value;
                            continue;
                        case "namespace":
                            nameSpace = attribute.Value;
                            continue;
                        case "path":
                            path = attribute.Value;
                            continue;
                    }
                }

                throw new XmlNodeException(string.Format(System.Globalization.CultureInfo.CurrentCulture,SR.XMLTRANSFORMATION_ImportUnknownAttribute, attribute.Name), attribute);
            }

            if (assemblyName != null && path != null) {
                throw new XmlNodeException(string.Format(System.Globalization.CultureInfo.CurrentCulture,SR.XMLTRANSFORMATION_ImportAttributeConflict), context.Element);
            }
            else if (assemblyName == null && path == null) {
                throw new XmlNodeException(string.Format(System.Globalization.CultureInfo.CurrentCulture,SR.XMLTRANSFORMATION_ImportMissingAssembly), context.Element);
            }
            else if (nameSpace == null) {
                throw new XmlNodeException(string.Format(System.Globalization.CultureInfo.CurrentCulture,SR.XMLTRANSFORMATION_ImportMissingNamespace), context.Element);
            }
            else {
                if (assemblyName != null) {
                    namedTypeFactory.AddAssemblyRegistration(assemblyName, nameSpace);
                }
                else {
                    namedTypeFactory.AddPathRegistration(path, nameSpace);
                }
            }
        }

        #region IServiceProvider Members

        public object GetService(Type serviceType) {
            object service = null;
            if (documentServiceContainer != null) {
                service = documentServiceContainer.GetService(serviceType);
            }
            if (service == null) {
                service = transformationServiceContainer.GetService(serviceType);
            }
            return service;
        }

        #endregion

        #region Dispose Pattern
        protected virtual void Dispose(bool disposing)
        {
            if (transformationServiceContainer != null)
            {
                transformationServiceContainer.Dispose();
                transformationServiceContainer = null;
            }

            if (documentServiceContainer != null)
            {
                documentServiceContainer.Dispose();
                documentServiceContainer = null;
            }

            if (xmlTransformable!= null)
            {
                xmlTransformable.Dispose();
                xmlTransformable = null;
            }

            if (xmlTransformation as XmlFileInfoDocument != null)
            {
                (xmlTransformation as XmlFileInfoDocument).Dispose();
                xmlTransformation = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);        
        }

        ~XmlTransformation()
        {
            Debug.Fail("call dispose please");
            Dispose(false);
        }
        #endregion

    }
}
