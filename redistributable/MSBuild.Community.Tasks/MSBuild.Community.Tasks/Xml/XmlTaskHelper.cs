
using Microsoft.Build.Framework;
using System.Xml;
using System;

namespace MSBuild.Community.Tasks.Xml
{
    /// <summary>
    /// Provides methods used by all of the XML tasks
    /// </summary>
    /// <exclude />
    public class XmlTaskHelper
    {
        /// <summary>
        /// Concatenates the string value of a list of ITaskItems into a single string
        /// </summary>
        /// <param name="items">The items to concatenate</param>
        /// <returns>A single string containing the values from all of the items</returns>
        public static string JoinItems(ITaskItem[] items)
        {
            System.Text.StringBuilder joinedItems = new System.Text.StringBuilder();
            foreach (ITaskItem item in items)
            {
                joinedItems.AppendLine(item.ToString());
            }
            return joinedItems.ToString();
        }

        /// <summary>
        /// Uses the prefix=namespace definitions in <paramref name="definitions"/> to populate <paramref name="namespaceManager"/>
        /// </summary>
        /// <param name="namespaceManager">The <see cref="XmlNamespaceManager"/> to populate.</param>
        /// <param name="definitions">The prefix=namespace definitions.</param>
        public static void LoadNamespaceDefinitionItems(XmlNamespaceManager namespaceManager, ITaskItem[] definitions)
        {
            if (definitions == null || definitions.Length == 0) return;
            if (namespaceManager == null) throw new ArgumentNullException("namespaceManager");

            foreach (ITaskItem namespaceDefinition in definitions)
            {
                string[] definitionParts = namespaceDefinition.ToString().Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (definitionParts.Length < 2)
                {
                    throw new ArgumentException("Each namespace definition must include a prefix, followed by an equal sign, followed by a Uri. Example: custom=http://example.com", "NamespaceDefinitions");
                }
                namespaceManager.AddNamespace(definitionParts[0], definitionParts[1]);
            }
        }

    }
}
