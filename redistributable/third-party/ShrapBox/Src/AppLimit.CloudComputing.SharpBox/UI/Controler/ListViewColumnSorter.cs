using System.Collections;
using System.Windows.Forms;

namespace AppLimit.CloudComputing.SharpBox.UI
{
    /// <summary>
    /// Class to sort a listview control by column
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        private int ColumnToSort;
        private SortOrder OrderOfSort;
        private CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Constructor
        /// </summary>
        public ListViewColumnSorter()
        {
            ColumnToSort = 0;
            OrderOfSort = SortOrder.Ascending;
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// Compare two objects
        /// </summary>
        /// <param name="x">Object 1</param>
        /// <param name="y">Object 2</param>
        /// <returns>
        /// 0 if equals
        /// 1 if object X is before object Y
        /// -1 if object X is after object Y
        /// </returns>
        public int Compare(object x, object y)
        {
            int compareResult = 0;
            ListViewItem listviewX, listviewY;
            ListViewItem.ListViewSubItem subItemX, subItemY;

            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;
            subItemX = listviewX.SubItems[ColumnToSort];
            subItemY = listviewY.SubItems[ColumnToSort];

            // If tag is null, compare by text based. Else compare tag
            if (subItemX.Tag == null)
            {
                compareResult = ObjectCompare.Compare(subItemX.Text, subItemY.Text);
            }
            else
            {
                compareResult = ObjectCompare.Compare(subItemX.Tag, subItemY.Tag);
            }

            if (OrderOfSort == SortOrder.Ascending)
            {
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                return (-compareResult);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Last sorted column
        /// </summary>
        public int SortColumn
        {
            set
            {
                ColumnToSort = value;
            }
            get
            {
                return ColumnToSort;
            }
        }

        /// <summary>
        /// Last sort order
        /// </summary>
        public SortOrder Order
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }
    }
}
