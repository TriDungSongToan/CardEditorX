using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace CardEditor.Behaviors
{
    public static class MultiSelectBehavior
    {
        private static readonly Dictionary<DataGrid, NotifyCollectionChangedEventHandler> _handlers = new Dictionary<DataGrid, NotifyCollectionChangedEventHandler>();
        private static bool _isUpdatingFromViewModel = false;
        private static bool _isUpdatingFromDataGrid = false;


        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(MultiSelectBehavior),
                new PropertyMetadata(null, OnSelectedItemsChanged));

        public static void SetSelectedItems(DependencyObject element, IList value)
        {
            element.SetValue(SelectedItemsProperty, value);
        }

        public static IList GetSelectedItems(DependencyObject element)
        {
            return (IList)element.GetValue(SelectedItemsProperty);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (_handlers.TryGetValue(dataGrid, out var oldHandler) && e.OldValue is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= oldHandler;
                    _handlers.Remove(dataGrid);
                }

                if (e.NewValue is INotifyCollectionChanged newCollection)
                {
                    NotifyCollectionChangedEventHandler handler = (sender, args) =>
                    OnViewModelCollectionChanged(dataGrid, args);

                    newCollection.CollectionChanged += handler;
                    _handlers[dataGrid] = handler;
                }

                dataGrid.SelectionChanged -= OnDataGridSelectionChanged;
                dataGrid.SelectionChanged += OnDataGridSelectionChanged;

                dataGrid.Unloaded -= OnDataGridUnloaded;
                dataGrid.Unloaded += OnDataGridUnloaded;

                SyncFromViewModelToDataGrid(dataGrid);

                //// Unsubscribe from old collection
                //if (e.OldValue is INotifyCollectionChanged oldCollection)
                //{
                //    oldCollection.CollectionChanged -= (sender, args) => OnViewModelCollectionChanged(dataGrid, args);
                //}

                //// Subscribe to new collection
                //if (e.NewValue is INotifyCollectionChanged newCollection)
                //{
                //    newCollection.CollectionChanged += (sender, args) => OnViewModelCollectionChanged(dataGrid, args);
                //}

                //dataGrid.SelectionChanged -= OnDataGridSelectionChanged;
                //dataGrid.SelectionChanged += OnDataGridSelectionChanged;

                //// Initial sync from ViewModel to DataGrid
                //SyncFromViewModelToDataGrid(dataGrid);
            }
        }

        private static void OnDataGridUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                dataGrid.SelectionChanged -= OnDataGridSelectionChanged;
                dataGrid.Unloaded -= OnDataGridUnloaded;

                if (_handlers.TryGetValue(dataGrid, out var handler) && GetSelectedItems(dataGrid) is INotifyCollectionChanged collection)
                {
                    collection.CollectionChanged -= handler;
                    _handlers.Remove(dataGrid);
                }
            }
        }

        private static void OnDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && !_isUpdatingFromViewModel)
            {
                var selectedItems = GetSelectedItems(dataGrid);
                if (selectedItems != null)
                {
                    // Always defer to avoid reentrancy
                    Application.Current.Dispatcher.BeginInvoke(
                         DispatcherPriority.Background,
                         new Action(() =>
                         {
                             if (!_isUpdatingFromViewModel && !_isUpdatingFromDataGrid)
                             {
                                 _isUpdatingFromDataGrid = true;
                                 try
                                 {
                                     var newSelection = dataGrid.SelectedItems.Cast<object>().ToList();
                                     UpdateCollectionSafely(selectedItems, newSelection);
                                 }
                                 finally
                                 {
                                     _isUpdatingFromDataGrid = false;
                                 }
                             }
                         }));
                }
            }
        }

        private static void OnViewModelCollectionChanged(DataGrid dataGrid, NotifyCollectionChangedEventArgs e)
        {
            if (_isUpdatingFromDataGrid || _isUpdatingFromViewModel) return;

            _isUpdatingFromViewModel = true;
            try
            {
                SyncFromViewModelToDataGrid(dataGrid);
            }
            finally
            {
                _isUpdatingFromViewModel = false;
            }
        }

        private static void SyncFromViewModelToDataGrid(DataGrid dataGrid)
        {
            var selectedItems = GetSelectedItems(dataGrid);
            if (selectedItems == null)
                return;

            var targetSet = selectedItems.Cast<object>().ToHashSet();
            var currentSet = dataGrid.SelectedItems.Cast<object>().ToHashSet();

            if (targetSet.SetEquals(currentSet))
                return;

            dataGrid.SelectedItems.Clear();
            foreach (var item in selectedItems)
            {
                if (dataGrid.Items.Contains(item))
                {
                    dataGrid.SelectedItems.Add(item);
                }
            }

            //if (selectedItems != null)
            //{
            //    // Clear DataGrid selection without triggering events
            //    var currentSelection = dataGrid.SelectedItems.Cast<object>().ToList();

            //    // Only update if different
            //    var newSelection = selectedItems.Cast<object>().ToHashSet();
            //    var currentSelectionSet = currentSelection.ToHashSet();

            //    if (!newSelection.SetEquals(currentSelectionSet))
            //    {
            //        dataGrid.SelectedItems.Clear();
            //        foreach (var item in selectedItems)
            //        {
            //            if (dataGrid.Items.Contains(item))
            //            {
            //                dataGrid.SelectedItems.Add(item);
            //            }
            //        }
            //    }
            //}
        }

        private static void UpdateCollectionSafely(IList targetCollection, List<object> newItems)
        {
            try
            {
                var currentItems = targetCollection.Cast<object>().ToHashSet();
                var newItemsSet = newItems.ToHashSet();

                if (currentItems.SetEquals(newItemsSet)) return;

                var toRemove = currentItems.Except(newItemsSet).ToList();
                var toAdd = newItemsSet.Except(currentItems).ToList();

                foreach (var item in toRemove)
                {
                    targetCollection.Remove(item);
                }

                if (toAdd.Any())
                {
                    var collectionType = targetCollection.GetType();
                    var addRangeMethod = collectionType.GetMethod("AddRange");

                    if (addRangeMethod != null)
                    {
                        var itemType = collectionType.GetInterfaces()
                            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            ?.GetGenericArguments()[0];

                        if (itemType != null)
                        {
                            var casted = typeof(Enumerable)
                                .GetMethod("Cast")
                                .MakeGenericMethod(itemType)
                                .Invoke(null, new object[] { toAdd });

                            var toList = typeof(Enumerable)
                                .GetMethod("ToList")
                                .MakeGenericMethod(itemType)
                                .Invoke(null, new object[] { casted });

                            addRangeMethod.Invoke(targetCollection, new object[] { toList });
                            return;
                        }
                    }
                    else
                    {
                        foreach (var item in toAdd)
                        {
                            targetCollection.Add(item);
                        }
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MultiSelectBehavior] Reentrancy detected, skipping update. {ex.Message}");
            }
        }
    }

    public static class ListBoxSelectedItemsBehavior
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(ListBoxSelectedItemsBehavior),
                new PropertyMetadata(null, OnSelectedItemsChanged));

        public static IList GetSelectedItems(DependencyObject obj)
            => (IList)obj.GetValue(SelectedItemsProperty);

        public static void SetSelectedItems(DependencyObject obj, IList value)
            => obj.SetValue(SelectedItemsProperty, value);

        private class SyncState
        {
            public bool IsUpdating;
            public IList? Source;
            public NotifyCollectionChangedEventHandler? Handler;
        }

        private static readonly ConditionalWeakTable<ListBox, SyncState> _states = new();

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBox listBox) return;

            var state = _states.GetOrCreateValue(listBox);

            // Bỏ subscribe collection cũ
            if (state.Source is INotifyCollectionChanged oldNotify && state.Handler != null)
                oldNotify.CollectionChanged -= state.Handler;

            // Lần đầu tiên gắn behavior -> hook sự kiện của ListBox
            if (e.OldValue == null)
            {
                listBox.SelectionChanged += (s, args) => OnListBoxSelectionChanged(listBox, args, state);
                listBox.Unloaded += (s, args) => Cleanup(state);
            }

            state.Source = e.NewValue as IList;

            if (e.NewValue is INotifyCollectionChanged newNotify)
            {
                state.Handler = (s, args) => OnSourceCollectionChanged(listBox, args, state);
                newNotify.CollectionChanged += state.Handler;
            }

            // Đồng bộ ban đầu: VM -> ListBox
            SyncListBoxFromSource(listBox, state);
        }

        private static void OnListBoxSelectionChanged(ListBox listBox, SelectionChangedEventArgs args, SyncState state)
        {
            if (state.IsUpdating || state.Source == null) return;

            state.IsUpdating = true;
            try
            {
                if (state.Source is CardEditor.Collections.BulkObservableCollection<object> bulk)
                {
                    bulk.BeginUpdate();
                    try
                    {
                        foreach (var item in args.RemovedItems) bulk.Remove(item);
                        foreach (var item in args.AddedItems)
                            if (!bulk.Contains(item)) bulk.Add(item);
                    }
                    finally { bulk.EndUpdate(); }
                }
                else
                {
                    foreach (var item in args.RemovedItems) state.Source.Remove(item);
                    foreach (var item in args.AddedItems)
                        if (!state.Source.Contains(item)) state.Source.Add(item);
                }
            }
            finally
            {
                state.IsUpdating = false;
            }
        }

        private static void OnSourceCollectionChanged(ListBox listBox, NotifyCollectionChangedEventArgs args, SyncState state)
        {
            if (state.IsUpdating) return;

            state.IsUpdating = true;
            try
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in args.NewItems!)
                            if (!listBox.SelectedItems.Contains(item))
                                listBox.SelectedItems.Add(item);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in args.OldItems!)
                            listBox.SelectedItems.Remove(item);
                        break;

                    default: // Reset, Replace, Move...
                        SyncListBoxFromSource(listBox, state);
                        break;
                }
            }
            finally
            {
                state.IsUpdating = false;
            }
        }

        private static void SyncListBoxFromSource(ListBox listBox, SyncState state)
        {
            listBox.SelectedItems.Clear();
            if (state.Source == null) return;
            foreach (var item in state.Source)
                listBox.SelectedItems.Add(item);
        }

        private static void Cleanup(SyncState state)
        {
            if (state.Source is INotifyCollectionChanged notify && state.Handler != null)
                notify.CollectionChanged -= state.Handler;
        }
    }

}
