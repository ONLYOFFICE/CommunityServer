#region

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HtmlAgilityPack;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace HAPExplorer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        #region Fields

        private OpenFileDialog _fileDialog = new OpenFileDialog();
        private HtmlDocument _html = new HtmlDocument();

        #endregion

        #region Constructors

        public Window1()
        {
            InitializeComponent();
            try
            {
                txtHtml.Text = File.ReadAllText("mshome.htm");
            }catch
            {
            }
            InitializeFileDialog();
        }

        #endregion

        #region Private Methods

        private void btnParse_Click(object sender, RoutedEventArgs e)
        {
            ParseHtml();
        }

        private void ParseHtml()
        {
            if (txtHtml.Text.IsEmpty()) return;

            _html = new HtmlDocument();
            _html.LoadHtml(txtHtml.Text);

            hapTree.BaseNode = _html.DocumentNode;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            HtmlNode node = _html.DocumentNode;
            if(chkFromCurrent.IsChecked == true
                && hapTree.SelectedItem != null
                && hapTree.SelectedItem is TreeViewItem 
                && ((TreeViewItem)hapTree.SelectedItem).DataContext is HtmlNode)
            node = ((TreeViewItem)hapTree.SelectedItem).DataContext as HtmlNode;

            SearchFromNode(node);
        }

        private void btnTestCode_Click(object sender, RoutedEventArgs e)
        {
            var mainPage = GetHtml("http://htmlagilitypack.codeplex.com");
            var homepage = new HtmlDocument();
            homepage.LoadHtml(mainPage);

            var nodes =
                homepage.DocumentNode.Descendants("a").Where(x => x.Id.ToLower().Contains("releasestab")).FirstOrDefault
                    ();
            var link = nodes.Attributes["href"].Value;

            var dc = new HtmlDocument();
            try
            {
                Cursor = Cursors.Wait;
                var req = (HttpWebRequest) WebRequest.Create(link);
                using (var resp = req.GetResponse().GetResponseStream())
                using (var read = new StreamReader(resp))
                {
                    dc.LoadHtml(read.ReadToEnd());
                    var span =
                        dc.DocumentNode.Descendants("span").Where(
                            x => x.Id.ToLower().Contains("releasedownloadsliteral")).FirstOrDefault();
                    MessageBox.Show(
                        int.Parse(span.InnerHtml.ToLower().Replace("downloads", string.Empty).Trim()).ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK);
            }
        }

        private string GetHtml(string link)
        {
            var req = (HttpWebRequest) WebRequest.Create(link);
            using (var resp = req.GetResponse().GetResponseStream())
            using (var read = new StreamReader(resp))
            {
                return read.ReadToEnd();
            }
        }


        private void hapTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!(e.NewValue is TreeViewItem)) return;

            var t = e.NewValue as TreeViewItem;

            if (t.DataContext is HtmlNode)
            {
                HtmlAttributeViewer1.Visibility = Visibility.Hidden;
                HtmlNodeViewer1.DataContext = null;
                HtmlNodeViewer1.Visibility = Visibility.Visible;
                HtmlNodeViewer1.DataContext = t.DataContext;
                return;
            }
            if (t.DataContext is HtmlAttribute)
            {
                HtmlNodeViewer1.Visibility = Visibility.Hidden;
                HtmlAttributeViewer1.DataContext = null;
                HtmlAttributeViewer1.Visibility = Visibility.Visible;
                HtmlAttributeViewer1.DataContext = t.DataContext;
                return;
            }
        }

        private void InitializeFileDialog()
        {
            _fileDialog.FileName = "Document"; // Default file name
            _fileDialog.DefaultExt = ".html"; // Default file extension
            _fileDialog.Filter = "Text documents (.html,.htm,.aspx)|*.html;*.htm;*.aspx"; // Filter files by extension
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mnuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var result = _fileDialog.ShowDialog();
            if (result != true) return;
            try
            {
                Cursor = Cursors.Wait;
                var file = _fileDialog.FileName;
                txtHtml.Text = File.ReadAllText(file);
            }
            catch (FileNotFoundException fEx)
            {
                MessageBox.Show("Error loading file: " + fEx.Message, "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error, MessageBoxResult.OK);
            }
            catch (FileLoadException fEx)
            {
                MessageBox.Show("Error loading file: " + fEx.Message, "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error, MessageBoxResult.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void mnuOpenUrl_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UrlDialog();
            if (dialog.ShowDialog() != true) return;
            try
            {
                Cursor = Cursors.Wait;
                //var req = (HttpWebRequest) WebRequest.Create(dialog.Url);
                //using (var resp = req.GetResponse().GetResponseStream())
                //using (var read = new StreamReader(resp))
                //{
                //    var txt = read.ReadToEnd();
                //    txtHtml.Text = txt;
                //}
                var hw = new HtmlWeb();
                _html = hw.Load(dialog.Url);
                hapTree.BaseNode = _html.DocumentNode;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void SearchFromNode(HtmlNode baseNode)
        {
            IEnumerable<HtmlNode> nodes = Enumerable.Empty<HtmlNode>();

            if (!_html.DocumentNode.HasChildNodes) 
                ParseHtml();

            if(chkXPath.IsChecked == true)
                nodes = baseNode.SelectNodes(txtSearchTag.Text);
            else
                nodes = baseNode.Descendants(txtSearchTag.Text);

            if (nodes == null) return;

            listResults.Items.Clear();

            foreach (var node in nodes)
            {
                var tr = new NodeTreeView {BaseNode = node};
                var lvi = new ListBoxItem();
                var pnl = new StackPanel();
                pnl.Children.Add(new Label
                                     {
                                         Content =
                                             string.Format("id:{0} name:{1} children{2}", node.Id, node.Name,
                                                           node.ChildNodes.Count),
                                         FontWeight = FontWeights.Bold
                                     });
                pnl.Children.Add(tr);
                lvi.Content = pnl;
                listResults.Items.Add(lvi);
            }
            tabControl1.SelectedItem = tabSearchResults;
        }

        #endregion
    }
}