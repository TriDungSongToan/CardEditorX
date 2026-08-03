using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Xaml.Behaviors;
using CardEditor.Collections;

namespace CardEditor.Behaviors
{
    /// <summary>
    /// Behavior cho phép binding TwoWay SelectedItems của ListBox trong MVVM.
    /// Hỗ trợ SelectionMode="Extended" và "Multiple".
    /// Xử lý tốt memory leak, hỗ trợ cả ObservableCollection và IList.
    /// </summary>
    public class ListBoxMultiSelectBehavior : Behavior<ListBox>
    {
        #region Dependency Property - SelectedItems

        /// <summary>
        /// Định nghĩa Dependency Property để binding với ViewModel.
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
                nameof(SelectedItems),
                typeof(IList),
                typeof(ListBoxMultiSelectBehavior),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedItemsPropertyChanged));

        /// <summary>
        /// Danh sách các item được chọn - bind với ViewModel.
        /// Phải là IList (có thể là ObservableCollection, ArrayList, List{T}, ...)
        /// </summary>
        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        #endregion

        #region Private Fields

        private bool _isUpdatingFromViewModel; // Tránh vòng lặp vô hạn
        private bool _isUpdatingFromView;
        private INotifyCollectionChanged _notifyCollection; // Để bắt sự kiện thay đổi từ ViewModel

        #endregion

        #region Override Methods - Attach/Detach

        protected override void OnAttached()
        {
            base.OnAttached();

            // Kiểm tra SelectionMode
            if (AssociatedObject.SelectionMode != SelectionMode.Extended &&
                AssociatedObject.SelectionMode != SelectionMode.Multiple)
            {
                throw new InvalidOperationException(
                    "ListBoxMultiSelectBehavior yêu cầu ListBox có SelectionMode là Extended hoặc Multiple.");
            }

            // Đăng ký sự kiện - sử dụng INotifyCollectionChanged thay vì IList
            var selectedItems = AssociatedObject.SelectedItems as INotifyCollectionChanged;
            if (selectedItems != null)
            {
                selectedItems.CollectionChanged += OnViewSelectionChanged;
            }

            AssociatedObject.Loaded += OnListBoxLoaded;
            AssociatedObject.Unloaded += OnListBoxUnloaded;

            // Đồng bộ lần đầu (nếu ViewModel đã có dữ liệu trước khi Behavior attach)
            if (SelectedItems != null && SelectedItems.Count > 0)
            {
                SyncViewModelToView();
            }
        }

        protected override void OnDetaching()
        {
            // Ngắt kết nối sự kiện để tránh memory leak
            if (AssociatedObject != null)
            {
                var selectedItems = AssociatedObject.SelectedItems as INotifyCollectionChanged;
                if (selectedItems != null)
                {
                    selectedItems.CollectionChanged -= OnViewSelectionChanged;
                }

                AssociatedObject.Loaded -= OnListBoxLoaded;
                AssociatedObject.Unloaded -= OnListBoxUnloaded;
            }

            if (_notifyCollection != null)
            {
                _notifyCollection.CollectionChanged -= OnViewModelCollectionChanged;
                _notifyCollection = null;
            }

            base.OnDetaching();
        }

        #endregion

        #region Event Handlers - View Events

        /// <summary>
        /// Khi ListBox được Load, đồng bộ từ ViewModel sang View.
        /// </summary>
        private void OnListBoxLoaded(object sender, RoutedEventArgs e)
        {
            SyncViewModelToView();
        }

        /// <summary>
        /// Khi ListBox Unload, cleanup để tránh memory leak.
        /// </summary>
        private void OnListBoxUnloaded(object sender, RoutedEventArgs e)
        {
            if (_notifyCollection != null)
            {
                _notifyCollection.CollectionChanged -= OnViewModelCollectionChanged;
                _notifyCollection = null;
            }
        }

        /// <summary>
        /// Bắt sự kiện thay đổi selection từ phía View (người dùng click chọn).
        /// </summary>
        private void OnViewSelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Tránh vòng lặp: nếu đang update từ ViewModel, bỏ qua
            if (_isUpdatingFromViewModel) return;

            _isUpdatingFromView = true;

            try
            {
                // Đồng bộ từ View sang ViewModel
                var newSelectedItems = AssociatedObject.SelectedItems.Cast<object>().ToList();

                // Tạm thời ngắt sự kiện của ViewModel collection để tránh vòng lặp
                if (_notifyCollection != null)
                {
                    _notifyCollection.CollectionChanged -= OnViewModelCollectionChanged;
                }

                // Cập nhật ViewModel collection
                if (!TryReplaceBulk(newSelectedItems))
                {
                    SelectedItems.Clear();

                    foreach (var item in newSelectedItems)
                    {
                        SelectedItems.Add(item);
                    }
                }

                // Kết nối lại sự kiện
                if (_notifyCollection != null)
                {
                    _notifyCollection.CollectionChanged += OnViewModelCollectionChanged;
                }
            }
            finally
            {
                _isUpdatingFromView = false;
            }
        }
        private bool TryReplaceBulk(IEnumerable<object> items)
        {
            if (SelectedItems == null)
                return false;

            var type = SelectedItems.GetType();

            if (!type.IsGenericType)
                return false;

            if (type.GetGenericTypeDefinition() != typeof(BulkObservableCollection<>))
                return false;

            var method = type.GetMethod("ReplaceAll");

            method?.Invoke(SelectedItems, new object[] { items });

            return true;
        }
        #endregion

        #region Event Handlers - ViewModel Events

        /// <summary>
        /// Bắt sự kiện thay đổi từ ViewModel (nếu SelectedItems là ObservableCollection).
        /// </summary>
        private void OnViewModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Tránh vòng lặp
            if (_isUpdatingFromView)
                return;

            _isUpdatingFromViewModel = true;

            try
            {
                SyncViewModelToView();
            }
            finally
            {
                _isUpdatingFromViewModel = false;
            }
        }

        #endregion

        #region Property Changed Callback

        /// <summary>
        /// Callback khi Dependency Property SelectedItems thay đổi từ ViewModel.
        /// </summary>
        private static void OnSelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (ListBoxMultiSelectBehavior)d;

            // Ngắt kết nối sự kiện cũ
            if (behavior._notifyCollection != null)
            {
                behavior._notifyCollection.CollectionChanged -= behavior.OnViewModelCollectionChanged;
                behavior._notifyCollection = null;
            }

            // Kết nối sự kiện mới nếu là INotifyCollectionChanged
            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                behavior._notifyCollection = newCollection;
                newCollection.CollectionChanged += behavior.OnViewModelCollectionChanged;
            }

            // Đồng bộ từ ViewModel sang View
            if (behavior.AssociatedObject != null)
            {
                behavior.SyncViewModelToView();
            }
        }

        #endregion

        #region Core Sync Methods

        /// <summary>
        /// Đồng bộ từ ViewModel sang View.
        /// </summary>
        private void SyncViewModelToView()
        {
            if (_isUpdatingFromView)
                return;

            if (AssociatedObject == null || SelectedItems == null)
                return;

            _isUpdatingFromViewModel = true;

            try
            {
                // Lấy danh sách item hiện tại trong View
                var currentSelected = AssociatedObject.SelectedItems.Cast<object>().ToList();
                var targetSelected = SelectedItems.Cast<object>().ToList();

                // OPTIMIZATION: Nếu dùng BulkObservableCollection, có thể so sánh nhanh hơn
                if (SelectedItems is BulkObservableCollection<object>)
                {
                    // Nếu số lượng khác nhau hoặc phần tử khác nhau, đồng bộ lại toàn bộ
                    if (currentSelected.Count != targetSelected.Count ||
                        !currentSelected.SequenceEqual(targetSelected))
                    {
                        AssociatedObject.SelectedItems.Clear();
                        foreach (var item in targetSelected)
                        {
                            AssociatedObject.SelectedItems.Add(item);
                        }
                    }
                    return;
                }

                // Code cũ cho các collection thông thường
                var itemsToAdd = targetSelected.Except(currentSelected).ToList();
                foreach (var item in itemsToAdd)
                {
                    if (!AssociatedObject.SelectedItems.Contains(item))
                    {
                        AssociatedObject.SelectedItems.Add(item);
                    }
                }

                var itemsToRemove = currentSelected.Except(targetSelected).ToList();
                foreach (var item in itemsToRemove)
                {
                    if (AssociatedObject.SelectedItems.Contains(item))
                    {
                        AssociatedObject.SelectedItems.Remove(item);
                    }
                }
            }
            finally
            {
                _isUpdatingFromViewModel = false;
            }
        }

        #endregion
    }
}