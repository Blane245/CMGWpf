using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace CMGWpf.Utilities
{
    /// <summary>
    /// Helper class for synchronizing ListBox multi-selection with an ObservableCollection
    /// </summary>
    public static class ListBoxHelper
    {
        /// <summary>
        /// Sets up two-way synchronization between a ListBox's SelectedItems and an ObservableCollection. Call this in the dialog's constructor or Loaded event.
        /// </summary>
        /// <param name="listBox">The ListBox control with SelectionMode="Multiple"</param>
        /// <param name="boundCollection">The ObservableCollection to sync with</param>
        public static void BindSelectedItems(ListBox listBox, ObservableCollection<string> boundCollection)
        {
            if (listBox == null || boundCollection == null)
                return;

            bool isUpdating = false; // Guard flag to prevent circular updates

            // Initial sync: populate ListBox selection from bound collection
            SyncListBoxFromCollection(listBox, boundCollection);

            // Listen to ListBox selection changes and update bound collection
            listBox.SelectionChanged += (s, e) =>
            {
                if (isUpdating) return; // Ignore if we're syncing FROM collection TO ListBox

                isUpdating = true;
                try
                {
                    // Remove items that were deselected
                    foreach (var item in e.RemovedItems)
                    {
                        if (item is string str && boundCollection.Contains(str))
                            boundCollection.Remove(str);
                    }

                    // Add items that were selected
                    foreach (var item in e.AddedItems)
                    {
                        if (item is string str && !boundCollection.Contains(str))
                            boundCollection.Add(str);
                    }
                }
                finally
                {
                    isUpdating = false;
                }
            };

            // Listen to bound collection changes and update ListBox selection
            boundCollection.CollectionChanged += (s, e) =>
            {
                if (isUpdating) return; // Ignore if we're syncing FROM ListBox TO collection

                isUpdating = true;
                try
                {
                    SyncListBoxFromCollection(listBox, boundCollection);
                }
                finally
                {
                    isUpdating = false;
                }
            };
        }

        private static void SyncListBoxFromCollection(ListBox listBox, ObservableCollection<string> boundCollection)
        {
            listBox.SelectedItems.Clear();
            foreach (var item in boundCollection)
            {
                if (listBox.Items.Contains(item))
                    listBox.SelectedItems.Add(item);
            }
        }
    }
}
