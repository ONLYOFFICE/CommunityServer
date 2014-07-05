namespace AppLimit.CloudComputing.SharpBox.UI
{
    partial class Sandbox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sandbox));
            this.viewFolders = new System.Windows.Forms.TreeView();
            this.workerLoadFolders = new System.ComponentModel.BackgroundWorker();
            this.pbLoading = new System.Windows.Forms.ProgressBar();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.picLoading = new System.Windows.Forms.PictureBox();
            this.lstFiles = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colModified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.workerLoadFiles = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // viewFolders
            // 
            resources.ApplyResources(this.viewFolders, "viewFolders");
            this.viewFolders.Name = "viewFolders";
            this.viewFolders.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.viewFolders_AfterSelect);
            // 
            // workerLoadFolders
            // 
            this.workerLoadFolders.WorkerReportsProgress = true;
            this.workerLoadFolders.DoWork += new System.ComponentModel.DoWorkEventHandler(this.workerLoadFolders_DoWork);
            this.workerLoadFolders.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.workerLoadFolders_ProgressChanged);
            this.workerLoadFolders.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.workerLoadFolders_RunWorkerCompleted);
            // 
            // pbLoading
            // 
            resources.ApplyResources(this.pbLoading, "pbLoading");
            this.pbLoading.MarqueeAnimationSpeed = 20;
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pbLoading);
            this.splitContainer1.Panel1.Controls.Add(this.viewFolders);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.picLoading);
            this.splitContainer1.Panel2.Controls.Add(this.lstFiles);
            // 
            // picLoading
            // 
            resources.ApplyResources(this.picLoading, "picLoading");
            this.picLoading.Image = global::AppLimit.CloudComputing.SharpBox.Properties.Resources.UILoader;
            this.picLoading.Name = "picLoading";
            this.picLoading.TabStop = false;
            // 
            // lstFiles
            // 
            this.lstFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colSize,
            this.colModified});
            resources.ApplyResources(this.lstFiles, "lstFiles");
            this.lstFiles.FullRowSelect = true;
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.UseCompatibleStateImageBehavior = false;
            this.lstFiles.View = System.Windows.Forms.View.Details;
            this.lstFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstFiles_ColumnClick);
            // 
            // colName
            // 
            resources.ApplyResources(this.colName, "colName");
            // 
            // colSize
            // 
            resources.ApplyResources(this.colSize, "colSize");
            // 
            // colModified
            // 
            resources.ApplyResources(this.colModified, "colModified");
            // 
            // workerLoadFiles
            // 
            this.workerLoadFiles.WorkerReportsProgress = true;
            this.workerLoadFiles.DoWork += new System.ComponentModel.DoWorkEventHandler(this.workerLoadFiles_DoWork);
            this.workerLoadFiles.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.workerLoadFiles_ProgressChanged);
            this.workerLoadFiles.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.workerLoadFiles_RunWorkerCompleted);
            // 
            // Sandbox
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "Sandbox";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView viewFolders;
        private System.ComponentModel.BackgroundWorker workerLoadFolders;
        private System.Windows.Forms.ProgressBar pbLoading;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView lstFiles;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.ColumnHeader colModified;
        private System.ComponentModel.BackgroundWorker workerLoadFiles;
        private System.Windows.Forms.PictureBox picLoading;
    }
}
