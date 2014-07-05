/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ASC.Data.Backup.Restore
{
    public partial class MainForm : Form
    {
        private bool progress = false;


        public MainForm()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            InitializeComponent();

            labelStatus.Text = string.Empty;
            buttonCancel.Click += (s, e) => Application.Exit();
            this.FormClosing += (s, e) => e.Cancel = progress;
            try
            {
                var config = ConfigurationManager.AppSettings["coreConfig"];
                config = Path.GetFullPath(!string.IsNullOrEmpty(config) ? config : "TeamLabSvc.exe.Config");
                if (File.Exists(config)) textBoxCoreConfig.Text = config;

                config = ConfigurationManager.AppSettings["webConfig"];
                config = Path.GetFullPath(!string.IsNullOrEmpty(config) ? config : "..\\WebStudio\\Web.Config");
                if (File.Exists(config)) textBoxWebConfig.Text = config;
            }
            catch { }
        }

        private void buttonConfig_Click(object sender, EventArgs e)
        {
            var dialog = openFileDialogCoreConfig;
            var textBox = textBoxCoreConfig;
            if (sender == buttonWebConfig)
            {
                dialog = openFileDialogWebConfig;
                textBox = textBoxWebConfig;
            }
            if (sender == buttonBackup)
            {
                dialog = openFileDialogBackup;
                textBox = textBoxBackup;
            }
            
            var fileName = textBox.Text;
            if (!string.IsNullOrEmpty(fileName))
            {
                if (File.Exists(fileName)) dialog.FileName = fileName;
                if (Directory.Exists(fileName)) dialog.InitialDirectory = fileName;
            }
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = dialog.FileName;
            }
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            buttonRestore.Enabled = 0 < textBoxCoreConfig.TextLength && 0 < textBoxWebConfig.TextLength && 0 < textBoxBackup.TextLength;
        }


        private void buttonRestore_Click(object sender, EventArgs e)
        {
            var backuper = new BackupManager(textBoxBackup.Text, textBoxCoreConfig.Text, textBoxWebConfig.Text);
            backuper.AddBackupProvider(new Restarter());
            backuper.ProgressChanged += backuper_ProgressChanged;

            try
            {
                progress = true;
                labelStatus.Text = string.Empty;
                buttonRestore.Enabled = buttonCancel.Enabled = false;

                backuper.Load();

                labelStatus.Text = "Complete";
            }
            catch (Exception error)
            {
                labelStatus.Text = "Error: " + error.Message.Replace("\r\n", " ");
            }
            finally
            {
                progress = false;
                progressBar.Value = 0;
                buttonRestore.Enabled = buttonCancel.Enabled = true;
            }
        }

        private void backuper_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<ProgressChangedEventArgs>(backuper_ProgressChanged), sender, e);
            }
            else
            {
                labelStatus.Text = e.Status;
                progressBar.Value = (int)e.Progress;
                Application.DoEvents();
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (name.Name == "ASC.Data.Backup")
            {
                var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Backup");
                assemblyPath = Path.Combine(assemblyPath, name.Name + ".dll");
                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }
            return null;
        }
    }
}
