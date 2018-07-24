using System.Collections;
using System.Windows.Forms;

namespace Engine.Extensions.ListView
{
    /// <summary>
    ///     This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ColumnSorter : IComparer
    {
        /// <summary>
        ///     Specifies the column to be sorted
        /// </summary>
        private int ColumnToSort;

        /// <summary>
        ///     Case insensitive comparer object
        /// </summary>
        private readonly CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        ///     Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder OrderOfSort;

        /// <summary>
        ///     Class constructor.  Initializes various elements
        /// </summary>
        public ColumnSorter()
        {
            // Initialize the column to '0'
            ColumnToSort = 0;

            // Initialize the sort order to 'none'
            OrderOfSort = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        ///     Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            set => ColumnToSort = value;
            get => ColumnToSort;
        }

        /// <summary>
        ///     Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set => OrderOfSort = value;
            get => OrderOfSort;
        }

        /// <summary>
        ///     This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive
        ///     comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>
        ///     The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater
        ///     than 'y'
        /// </returns>
        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem) x;
            listviewY = (ListViewItem) y;

            // Compare the two items
            compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text,
                listviewY.SubItems[ColumnToSort].Text);

            // Calculate correct return value based on object comparison
            if (OrderOfSort == SortOrder.Ascending)
                return compareResult;
            if (OrderOfSort == SortOrder.Descending)
                return -compareResult;
            return 0;
        }
    }
}