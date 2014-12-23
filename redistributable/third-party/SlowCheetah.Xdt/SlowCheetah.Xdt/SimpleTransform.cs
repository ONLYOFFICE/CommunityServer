namespace SlowCheetah.Xdt {
    using Microsoft.Web.XmlTransform;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This is a wrapper to TransformXml that makes calling it much easier
    /// </summary>
    public class SimpleTransform {
        public bool TransformXml(string sourceFile, string transformFile, string destFile) {

            FileInfo source = new FileInfo(sourceFile);
            FileInfo transform = new FileInfo(transformFile);
            FileInfo dest = new FileInfo(destFile);

            if (!source.Exists) { throw new FileNotFoundException("source file not found", sourceFile); }
            if (!transform.Exists) { throw new FileNotFoundException("transform file not found", transformFile); }


            bool succeeded = true;

            XmlTransformation transformation = null;
            XmlTransformableDocument document = null;
            
            try {
                document = OpenSourceFile(source.FullName);
                transformation = OpenTransformFile(transform.FullName, null);
                succeeded = transformation.Apply(document);
                if (succeeded) {
                    document.Save(dest.FullName);
                }
            }
            finally {
                if (transformation != null) {
                    transformation.Dispose();
                }
                if (document != null) {
                    document.Dispose();
                }
            }

            return succeeded;
        }

        // TODO: These methods are shared with TransformXml, we should refactor these to share the same code
        private XmlTransformableDocument OpenSourceFile(string sourceFile) {
            try {
                XmlTransformableDocument document = new XmlTransformableDocument();

                document.PreserveWhitespace = true;
                document.Load(sourceFile);

                return document;
            }
            catch (System.Xml.XmlException) {
                throw;
            }
            catch (Exception ex) {
                throw new Exception(
                    string.Format(System.Globalization.CultureInfo.CurrentCulture,
                    "Could not open Source file: {0}", ex.Message),
                    ex);
            }
        }

        private XmlTransformation OpenTransformFile(string transformFile, IXmlTransformationLogger logger) {
            try {
                return new XmlTransformation(transformFile, logger);
            }
            catch (System.Xml.XmlException) {
                throw;
            }
            catch (Exception ex) {
                throw new Exception(
                    string.Format(System.Globalization.CultureInfo.CurrentCulture,
                    "Could not open Transform file: {0}", ex.Message),
                    ex);
            }
        }
    }
}
