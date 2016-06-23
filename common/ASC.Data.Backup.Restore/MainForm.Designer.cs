/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


namespace ASC.Data.Backup.Restore
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxCoreConfig = new System.Windows.Forms.TextBox();
            this.buttonCoreConfig = new System.Windows.Forms.Button();
            this.buttonWebConfig = new System.Windows.Forms.Button();
            this.textBoxWebConfig = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonBackup = new System.Windows.Forms.Button();
            this.textBoxBackup = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonRestore = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.openFileDialogCoreConfig = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogBackup = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogWebConfig = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Core configuration file:";
            // 
            // textBoxCoreConfig
            // 
            this.textBoxCoreConfig.Location = new System.Drawing.Point(12, 64);
            this.textBoxCoreConfig.Name = "textBoxCoreConfig";
            this.textBoxCoreConfig.Size = new System.Drawing.Size(484, 20);
            this.textBoxCoreConfig.TabIndex = 3;
            this.textBoxCoreConfig.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // buttonCoreConfig
            // 
            this.buttonCoreConfig.Location = new System.Drawing.Point(498, 64);
            this.buttonCoreConfig.Name = "buttonCoreConfig";
            this.buttonCoreConfig.Size = new System.Drawing.Size(25, 20);
            this.buttonCoreConfig.TabIndex = 4;
            this.buttonCoreConfig.Text = "...";
            this.buttonCoreConfig.UseVisualStyleBackColor = true;
            this.buttonCoreConfig.Click += new System.EventHandler(this.buttonConfig_Click);
            // 
            // buttonWebConfig
            // 
            this.buttonWebConfig.Location = new System.Drawing.Point(498, 103);
            this.buttonWebConfig.Name = "buttonWebConfig";
            this.buttonWebConfig.Size = new System.Drawing.Size(25, 20);
            this.buttonWebConfig.TabIndex = 6;
            this.buttonWebConfig.Text = "...";
            this.buttonWebConfig.UseVisualStyleBackColor = true;
            this.buttonWebConfig.Click += new System.EventHandler(this.buttonConfig_Click);
            // 
            // textBoxWebConfig
            // 
            this.textBoxWebConfig.Location = new System.Drawing.Point(12, 103);
            this.textBoxWebConfig.Name = "textBoxWebConfig";
            this.textBoxWebConfig.Size = new System.Drawing.Size(484, 20);
            this.textBoxWebConfig.TabIndex = 5;
            this.textBoxWebConfig.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Web configuration file:";
            // 
            // buttonBackup
            // 
            this.buttonBackup.Location = new System.Drawing.Point(498, 25);
            this.buttonBackup.Name = "buttonBackup";
            this.buttonBackup.Size = new System.Drawing.Size(25, 20);
            this.buttonBackup.TabIndex = 2;
            this.buttonBackup.Text = "...";
            this.buttonBackup.UseVisualStyleBackColor = true;
            this.buttonBackup.Click += new System.EventHandler(this.buttonConfig_Click);
            // 
            // textBoxBackup
            // 
            this.textBoxBackup.Location = new System.Drawing.Point(12, 25);
            this.textBoxBackup.Name = "textBoxBackup";
            this.textBoxBackup.Size = new System.Drawing.Size(484, 20);
            this.textBoxBackup.TabIndex = 1;
            this.textBoxBackup.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Teamlab backup file:";
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(11, 189);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(512, 5);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // buttonRestore
            // 
            this.buttonRestore.Enabled = false;
            this.buttonRestore.Location = new System.Drawing.Point(320, 200);
            this.buttonRestore.Name = "buttonRestore";
            this.buttonRestore.Size = new System.Drawing.Size(122, 23);
            this.buttonRestore.TabIndex = 7;
            this.buttonRestore.Text = "&Restore Backup";
            this.buttonRestore.UseVisualStyleBackColor = true;
            this.buttonRestore.Click += new System.EventHandler(this.buttonRestore_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(448, 200);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 156);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(511, 19);
            this.progressBar.TabIndex = 0;
            // 
            // labelStatus
            // 
            this.labelStatus.Location = new System.Drawing.Point(12, 140);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(511, 13);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "<status>";
            // 
            // openFileDialogCoreConfig
            // 
            this.openFileDialogCoreConfig.FileName = "TeamLabSvc.exe.config";
            this.openFileDialogCoreConfig.Filter = "Core configuration file|TeamLabSvc.exe.config|All files|*.*";
            // 
            // openFileDialogWebConfig
            // 
            this.openFileDialogWebConfig.FileName = "Web.config";
            this.openFileDialogWebConfig.Filter = "Web configuration file|Web.config|All files|*.*";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(535, 234);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonRestore);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonBackup);
            this.Controls.Add(this.textBoxBackup);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonWebConfig);
            this.Controls.Add(this.textBoxWebConfig);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonCoreConfig);
            this.Controls.Add(this.textBoxCoreConfig);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Teamlab Backup Restore";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCoreConfig;
        private System.Windows.Forms.Button buttonCoreConfig;
        private System.Windows.Forms.Button buttonWebConfig;
        private System.Windows.Forms.TextBox textBoxWebConfig;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonBackup;
        private System.Windows.Forms.TextBox textBoxBackup;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonRestore;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.OpenFileDialog openFileDialogCoreConfig;
        private System.Windows.Forms.OpenFileDialog openFileDialogBackup;
        private System.Windows.Forms.OpenFileDialog openFileDialogWebConfig;
    }
}

