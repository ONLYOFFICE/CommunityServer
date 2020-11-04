/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
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
                config = Path.GetFullPath(!string.IsNullOrEmpty(config) ? config : "TeamLabSvc.exe.config");
                if (File.Exists(config)) textBoxCoreConfig.Text = config;

                config = ConfigurationManager.AppSettings["webConfig"];
                config = Path.GetFullPath(!string.IsNullOrEmpty(config) ? config : "..\\WebStudio\\Web.Config");
                if (File.Exists(config))
                {
                    textBoxWebConfig.Text = config;
                }
                else
                {
                    config = Path.GetFullPath("..\\..\\WebStudio\\Web.Config");
                    if (File.Exists(config)) textBoxWebConfig.Text = config;
                }
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
