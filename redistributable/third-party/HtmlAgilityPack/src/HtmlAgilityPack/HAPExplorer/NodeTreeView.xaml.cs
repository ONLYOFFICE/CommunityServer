using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;

namespace HAPExplorer
{
    /// <summary>
    /// Interaction logic for NodeTreeView.xaml
    /// </summary>
    public partial class NodeTreeView : TreeView
    {
        private HtmlNode baseNode;
        public NodeTreeView()
        {
            InitializeComponent();
        }

        public HtmlNode BaseNode
        {
            get { return baseNode; }
            set { baseNode = value;
                PopulateTreeview();
            }
        }

        private TreeViewItem BuildTree(HtmlNode htmlNode)
        {
            //Create the main treeview node for this htmlnode
            var item = new TreeViewItem { DataContext = htmlNode }; //preserve reference to _html node for databinding

            //if we have psuedo element, show it's text
            if (htmlNode.NodeType == HtmlNodeType.Text || htmlNode.NodeType == HtmlNodeType.Comment)
                item.Header = string.Format("<{0}> = {1}", htmlNode.OriginalName, htmlNode.InnerText.Trim());
            else
                item.Header = string.Format("<{0}>", htmlNode.OriginalName);

            //Create Attribute collection
            PopulateItem(htmlNode, item);

            return item;
        }
        private void PopulateItem(HtmlNode htmlNode, ItemsControl item)
        {
            var attributes = new TreeViewItem { Header = "Attributes" };
            foreach (var att in htmlNode.Attributes)
                attributes.Items.Add(new TreeViewItem
                {
                    Header = string.Format("{0} = {1}", att.OriginalName, att.Value),
                    DataContext = att
                });
            //If we don't have any attributes, don't add the node
            if (attributes.Items.Count > 0)
                item.Items.Add(attributes);

            //Create the Elements Collection
            var elements = new TreeViewItem { Header = "Elements", DataContext = htmlNode };
            foreach (var node in htmlNode.ChildNodes)
            {
                //If there are no attributes, no need to add a node inbetween the parent in the treeview
                if (attributes.Items.Count > 0)
                    elements.Items.Add(BuildTree(node));
                else
                    item.Items.Add(BuildTree(node));
            }

            //If there are no nodes in the elements collection, don't add to the parent 
            if (elements.Items.Count > 0)
                item.Items.Add(elements);
        }
        public void PopulateTreeview()
        {
            this.Items.Clear();
            var header = baseNode.NodeType == HtmlNodeType.Document ? "DocumentElement" : baseNode.OriginalName;
            //We create the base node here, that way as new nodes are added we can animate them ;)
            var document = new TreeViewItem { Header = header, DataContext = baseNode, };
            this.Items.Add(document);
            PopulateItem(baseNode, document);
        }
    }
}
