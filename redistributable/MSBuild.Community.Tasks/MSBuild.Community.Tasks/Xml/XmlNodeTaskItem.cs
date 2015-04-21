
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using System.Xml.XPath;

namespace MSBuild.Community.Tasks.Xml
{
    /// <summary>
    /// Represents a single XmlNode selected using an XML task.
    /// </summary>
    /// <exclude />
    public class XmlNodeTaskItem : ITaskItem
    {
        Dictionary<string, string> metaData = new Dictionary<string, string>();
        readonly string ReservedMetaDataPrefix;

        /// <summary>
        /// Initializes a new instance of an XmlNodeTaskItem
        /// </summary>
        /// <param name="xpathNavigator">The selected XmlNode</param>
        /// <param name="reservedMetaDataPrefix">The prefix to attach to the reserved metadata properties.</param>
        public XmlNodeTaskItem(XPathNavigator xpathNavigator, string reservedMetaDataPrefix)
        {
            this.ReservedMetaDataPrefix = reservedMetaDataPrefix;

            switch (xpathNavigator.NodeType)
            {
                case XPathNodeType.Attribute:
                    itemSpec = xpathNavigator.Value;
                    break;
                default:
                    itemSpec = xpathNavigator.Name;
                    break;
            }
            metaData.Add(ReservedMetaDataPrefix + "value", xpathNavigator.Value);
            metaData.Add(ReservedMetaDataPrefix + "innerXml", xpathNavigator.InnerXml);
            metaData.Add(ReservedMetaDataPrefix + "outerXml", xpathNavigator.OuterXml);

            if (xpathNavigator.MoveToFirstAttribute())
            {
                do
                {
                    metaData.Add(xpathNavigator.Name, xpathNavigator.Value);
                } while (xpathNavigator.MoveToNextAttribute());
            }
        }

        /// <summary>
        /// Returns a string representation of the XmlNodeTaskItem.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return itemSpec;
        }

        /// <summary>
        /// Returns the ItemSpec when the XmlNodeTaskItem is explicitly cast as a <see cref="String"/>
        /// </summary>
        /// <param name="taskItemToCast">The XmlNodeTaskItem</param>
        /// <returns></returns>
        public static explicit operator string(XmlNodeTaskItem taskItemToCast)
        {
            return taskItemToCast.ItemSpec;
        }


        #region ITaskItem Members

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public System.Collections.IDictionary CloneCustomMetadata()
        {
            return new Dictionary<string, string>(metaData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinationItem"></param>
        public void CopyMetadataTo(ITaskItem destinationItem)
        {
            foreach (string metaKey in metaData.Keys)
            {
                destinationItem.SetMetadata(metaKey, metaData[metaKey]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metadataName"></param>
        /// <returns></returns>
        public string GetMetadata(string metadataName)
        {
            return metaData[metadataName];
        }

        private string itemSpec;
        /// <summary>
        /// 
        /// </summary>
        public string ItemSpec
        {
            get
            {
                return itemSpec;
            }
            set
            {
                itemSpec = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int MetadataCount
        {
            get { return metaData.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        public System.Collections.ICollection MetadataNames
        {
            get { return metaData.Keys; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metadataName"></param>
        public void RemoveMetadata(string metadataName)
        {
            metaData.Remove(metadataName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metadataName"></param>
        /// <param name="metadataValue"></param>
        public void SetMetadata(string metadataName, string metadataValue)
        {
            metaData[metadataName] = metadataValue;
        }

        #endregion
    }
}
