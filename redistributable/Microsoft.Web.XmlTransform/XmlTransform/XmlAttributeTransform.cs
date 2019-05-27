using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Microsoft.Web.XmlTransform
{
    public abstract class AttributeTransform : Transform
    {
        #region private data members
        private XmlNode transformAttributeSource = null;
        private XmlNodeList transformAttributes = null;
        private XmlNode targetAttributeSource = null;
        private XmlNodeList targetAttributes = null;
        #endregion

        protected AttributeTransform()
            : base(TransformFlags.ApplyTransformToAllTargetNodes) {
        }

        protected XmlNodeList TransformAttributes {
            get {
                if (transformAttributes == null || transformAttributeSource != TransformNode) {
                    transformAttributeSource = TransformNode;
                    transformAttributes = GetAttributesFrom(TransformNode);
                }
                return transformAttributes;
            }
        }

        protected XmlNodeList TargetAttributes {
            get {
                if (targetAttributes == null || targetAttributeSource != TargetNode) {
                    targetAttributeSource = TargetNode;
                    targetAttributes = GetAttributesFrom(TargetNode);
                }
                return targetAttributes;
            }
        }

        private XmlNodeList GetAttributesFrom(XmlNode node) {
            if (Arguments == null || Arguments.Count == 0) {
                return GetAttributesFrom(node, "*", false);
            }
            else if (Arguments.Count == 1) {
                return GetAttributesFrom(node, Arguments[0], true);
            }
            else {
                // First verify all the arguments
                foreach (string argument in Arguments) {
                    GetAttributesFrom(node, argument, true);
                }

                // Now return the complete XPath and return the combined list
                return GetAttributesFrom(node, Arguments, false);
            }
        }

        private XmlNodeList GetAttributesFrom(XmlNode node, string argument, bool warnIfEmpty) {
            return GetAttributesFrom(node, new string[1] { argument }, warnIfEmpty);
        }

        private XmlNodeList GetAttributesFrom(XmlNode node, IList<string> arguments, bool warnIfEmpty) {
            string[] array = new string[arguments.Count];
            arguments.CopyTo(array, 0);
            string xpath = String.Concat("@", String.Join("|@", array));
            
            XmlNodeList attributes = node.SelectNodes(xpath);
            if (attributes.Count == 0 && warnIfEmpty) {
                Debug.Assert(arguments.Count == 1, "Should only call warnIfEmpty==true with one argument");
                if (arguments.Count == 1) {
                    Log.LogWarning(SR.XMLTRANSFORMATION_TransformArgumentFoundNoAttributes, arguments[0]);
                }
            }

            return attributes;
        }
    }
}
