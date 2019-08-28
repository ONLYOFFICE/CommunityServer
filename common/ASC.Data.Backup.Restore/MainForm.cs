/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
