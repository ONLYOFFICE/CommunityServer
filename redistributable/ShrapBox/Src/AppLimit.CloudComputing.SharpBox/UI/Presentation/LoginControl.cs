using System;
using System.Windows.Forms;

namespace AppLimit.CloudComputing.SharpBox.UI
{
    /// <summary>
    /// Generic Login control
    /// </summary>
    public partial class LoginControl : UserControl
    {
        /// <summary>
        /// Standard constructor
        /// </summary>
        public LoginControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Username typed
        /// </summary>
        public String User { get { return txtUserName.Text; } }

        /// <summary>
        /// Password typed
        /// </summary>
        public String Password { get { return txtPassword.Text; } }

        /// <summary>
        /// End of login event
        /// </summary>
        public event EventHandler LoginCompleted;
        
        /// <summary>
        /// Login completed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnLoginCompleted(Object sender, EventArgs e)
        {
            if (LoginCompleted != null)
            {
                LoginCompleted(this, new EventArgs());
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            OnLoginCompleted(sender, e);
        }
    }
}
