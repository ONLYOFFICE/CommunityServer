using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace AppLimit.CloudComputing.SharpBox.UI
{
    /// <summary>
    /// Control that provide management of cloud folders
    /// </summary>
    public partial class Sandbox : UserControl
    {
        #region ATTRIBUTES
        private readonly WindowsFormsSynchronizationContext _syncContext;
        private CloudStorage _storage;
        private ListViewColumnSorter listViewFilesSorter;

        /// <summary>
        /// Event indicate that cloud folders have been loaded
        /// </summary>
        public event EventHandler SandboxLoadCompleted;

        /// <summary>
        /// Constructor of Sandbox control
        /// </summary>
        public Sandbox()
        {
            InitializeComponent();

            _syncContext = AsyncOperationManager.SynchronizationContext as WindowsFormsSynchronizationContext;
            _storage = null;

            listViewFilesSorter = new ListViewColumnSorter();
            lstFiles.ListViewItemSorter = listViewFilesSorter;
            lstFiles.Sort();
        } 
        #endregion

        /// <summary>
        /// Assign cloud storage
        /// </summary>
        /// <param name="storage">Cloud storage object</param>
        public void SetCloudStorage(CloudStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Method to load cloud folders
        /// </summary>
        public void LoadSandBox()
        {
            if (!workerLoadFolders.IsBusy)
            {
                workerLoadFolders.RunWorkerAsync();
            }
        }

        #region EVENTS
        private void OnSandboxLoadCompleted(object sender, EventArgs e)
        {
            EventHandler Current = (EventHandler)SandboxLoadCompleted;
            if (Current != null)
            {
                Current(this, new EventArgs());
            }
        }

        private void viewFolders_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!workerLoadFiles.IsBusy)
            {
                lstFiles.Items.Clear();
                picLoading.Visible = true;

                workerLoadFiles.RunWorkerAsync(e.Node.Tag);
             }
        }

        private void lstFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // check if the selected column is already sorted
            if (e.Column == listViewFilesSorter.SortColumn)
            {
                // Inverse sort order
                if (listViewFilesSorter.Order == SortOrder.Ascending)
                {
                    listViewFilesSorter.Order = SortOrder.Descending;
                }
                else
                {
                    listViewFilesSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // sort ascending on this column
                listViewFilesSorter.SortColumn = e.Column;
                listViewFilesSorter.Order = SortOrder.Ascending;
            }

            // Sort list
            lstFiles.Sort();
        }
        #endregion

        #region METHODS
        private void ListCloudDirectoryEntry(ICloudDirectoryEntry cloudEntry)
        {
            foreach (var subCloudEntry in cloudEntry)
            {
                if (subCloudEntry is ICloudDirectoryEntry)
                {
                    workerLoadFolders.ReportProgress(2, subCloudEntry);
                    ListCloudDirectoryEntry((ICloudDirectoryEntry)subCloudEntry);
                }
            }
        }
        private void AddCloudDirectory(ICloudDirectoryEntry cloudEntry, TreeNode trParent)
        {
            TreeNode newNode = null;
            if (trParent == null)
            {
                newNode = viewFolders.Nodes.Add(cloudEntry.GetPropertyValue("path"), cloudEntry.Name);
            }
            else
            {
                newNode = trParent.Nodes.Add(cloudEntry.GetPropertyValue("path"), cloudEntry.Name);
            }
            newNode.Tag = cloudEntry;
        }
        #endregion

        #region WORKER FOLDERS
        private void workerLoadFolders_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (_storage == null)
            {
                throw new TypeInitializationException(typeof(CloudStorage).ToString(), null);
            }
            if (_storage.IsOpened == false)
            {
                throw new Exception("Storage is not opened");
            }
            if (e.Argument == null)
            {
                // Clear
                workerLoadFolders.ReportProgress(0);
                // Load roots
                ICloudDirectoryEntry cloudEntries = _storage.GetRoot();
                // List roots
                foreach (var cloudEntry in cloudEntries)
                {
                    if (cloudEntry is ICloudDirectoryEntry)
                    {
                        workerLoadFolders.ReportProgress(1, cloudEntry);

                        ListCloudDirectoryEntry((ICloudDirectoryEntry)cloudEntry);
                    }
                }
            }
        }

        private void workerLoadFolders_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // For thread synchronization
            _syncContext.Post(workerLoadFolders_ProgressChanged_Callback, e);
        }
        private void workerLoadFolders_ProgressChanged_Callback(object ev)
        {
            ProgressChangedEventArgs e = (ProgressChangedEventArgs)ev;
            if (e.ProgressPercentage == 0)
            {
                // Clear tree view
                viewFolders.Nodes.Clear();
                pbLoading.Visible = true;
            }
            else if (e.ProgressPercentage == 1)
            {
                ICloudDirectoryEntry cloudEntry = (ICloudDirectoryEntry)e.UserState;
                AddCloudDirectory(cloudEntry, null);
            }
            else if (e.ProgressPercentage == 2)
            {
                ICloudDirectoryEntry cloudEntry = (ICloudDirectoryEntry)e.UserState;
                // Find parent root
                TreeNode[] findedNodes = viewFolders.Nodes.Find(cloudEntry.Parent.GetPropertyValue("path"), true);
                if (findedNodes.Length == 1)
                {
                    // add sub folders
                    AddCloudDirectory(cloudEntry, findedNodes[0]);
                }
            }
        }

        private void workerLoadFolders_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // For thread synchronization
            _syncContext.Post(workerLoadFolders_RunWorkerCompleted_Callback, e);
        }
        private void workerLoadFolders_RunWorkerCompleted_Callback(object ev)
        {
            RunWorkerCompletedEventArgs e = (RunWorkerCompletedEventArgs)ev;
            pbLoading.Visible = false;
            OnSandboxLoadCompleted(this, new EventArgs());
        }
        #endregion

        #region WORKER FILES
        private void workerLoadFiles_DoWork(object sender, DoWorkEventArgs e)
        {
            ICloudDirectoryEntry cloudEntry = (ICloudDirectoryEntry)e.Argument;

            // Loop on each entries
            IEnumerator cloudEntries = cloudEntry.GetEnumerator();
            while (cloudEntries.MoveNext())
            {
                var entry = cloudEntries.Current;
                if (entry is ICloudDirectoryEntry)
                {
                    // Nothing, it is a folder
                }
                else if (entry is ICloudFileSystemEntry)
                {
                    ICloudFileSystemEntry fsEntry = (ICloudFileSystemEntry)entry;
                    // Build new listviewitem object
                    ListViewItem newEntry = new ListViewItem(fsEntry.Name);
                    // Add size (add size to tag for sorting)
                    ListViewItem.ListViewSubItem subItems = newEntry.SubItems.Add(FileSizeFormat.Format(fsEntry.Length));
                    subItems.Tag = fsEntry.Length;
                    // Add modified date (add date to tag for sorting)
                    subItems = newEntry.SubItems.Add(fsEntry.Modified.ToString());
                    subItems.Tag = fsEntry.Modified;
                    // Add object to tag
                    newEntry.Tag = fsEntry;
                    // Report progress new entry to worker
                    workerLoadFiles.ReportProgress(0, newEntry);
                }
            }
        }

        private void workerLoadFiles_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // For thread synchronization
            _syncContext.Post(workerLoadFiles_ProgressChanged_Callback, e);
        }
        private void workerLoadFiles_ProgressChanged_Callback(object ev)
        {
            ProgressChangedEventArgs e = (ProgressChangedEventArgs)ev;
            if (e.ProgressPercentage == 0)
            {
                if (e.UserState != null)
                {
                    lstFiles.Items.Add((ListViewItem)e.UserState);
                }
            }
        }

        private void workerLoadFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // For thread synchronization
            _syncContext.Post(workerLoadFiles_RunWorkerCompleted_Callback, e);
        }
        private void workerLoadFiles_RunWorkerCompleted_Callback(object ev)
        {
            RunWorkerCompletedEventArgs e = (RunWorkerCompletedEventArgs)ev;
            picLoading.Visible = false;
        } 
        #endregion
    }
}
