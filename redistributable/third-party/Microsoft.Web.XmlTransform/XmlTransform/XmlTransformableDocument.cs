using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Microsoft.Web.XmlTransform
{
    public class XmlTransformableDocument : XmlFileInfoDocument, IXmlOriginalDocumentService
    {
        #region private data members
        private XmlDocument xmlOriginal = null;
        #endregion

        #region public interface
        public XmlTransformableDocument() {
        }

        public bool IsChanged {
            get {
                if (xmlOriginal == null) {
                    // No transformation has occurred
                    return false;
                }

                return !IsXmlEqual(xmlOriginal, this);
            }
        }
        #endregion

        #region Change support
        internal void OnBeforeChange() {
            if (xmlOriginal == null) {
                CloneOriginalDocument();
            }
        }

        internal void OnAfterChange() {
        }
        #endregion

        #region Helper methods
        private void CloneOriginalDocument() {
            xmlOriginal = (XmlDocument)this.Clone();
        }

        private bool IsXmlEqual(XmlDocument xmlOriginal, XmlDocument xmlTransformed) {
            // FUTURE: Write a comparison algorithm to see if xmlLeft and
            // xmlRight are different in any significant way. Until then,
            // assume there's a difference.
            return false;
        }
        #endregion

        #region IXmlOriginalDocumentService Members
        XmlNodeList IXmlOriginalDocumentService.SelectNodes(string xpath, XmlNamespaceManager nsmgr) {
            if (xmlOriginal != null) {
                return xmlOriginal.SelectNodes(xpath, nsmgr);
            }
            else {
                return null;
            }
        }
        #endregion
    }
}
