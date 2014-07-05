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
    /// Interaction logic for HtmlAttributeViewer.xaml
    /// </summary>
    public partial class HtmlAttributeViewer : UserControl
    {
        public HtmlAttributeViewer()
        {
            InitializeComponent();
        }

        private void btnCheckXpath_Click(object sender, RoutedEventArgs e)
        {
            var node = DataContext as HtmlAttribute;
            if (node == null) return;
            var doc = node.OwnerDocument.DocumentNode;
            var nn = doc.SelectSingleNode(node.XPath);
            if (nn == node.OwnerNode)
                MessageBox.Show("Pass");
            else
                MessageBox.Show("Fail");
        }
    }
}
