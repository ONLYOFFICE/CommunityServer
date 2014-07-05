using System.Windows;

namespace HAPExplorer
{
    /// <summary>
    /// Interaction logic for UrlDialog.xaml
    /// </summary>
    public partial class UrlDialog
    {
        #region Constructors

        public UrlDialog()
        {
            InitializeComponent();
            textBox1.Focus();
        }

        #endregion

        #region Properties

        public string Url
        {
            get { return textBox1.Text.Trim(); }
        }

        #endregion

        #region Private Methods

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = !textBox1.Text.IsEmpty();

            return;
        }

        #endregion
    }
}