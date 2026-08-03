#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CardEditor.Collections
{
    public class SearchOptions
    {
        public bool UseParallel { get; set; }
        public int ParallelThreshold { get; set; } = 1000;
        public string? CacheKey { get; set; }
        public bool UseCache { get; set; }
    }
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        private int _suppressLevel = 0;
        public BulkObservableCollection() : base() { }
        public BulkObservableCollection(IEnumerable<T> items)
        {
            AddRange(items);
        }
        public void BeginUpdate()
        {
            _suppressLevel++;
        }
        public void EndUpdate()
        {
            if (_suppressLevel > 0) _suppressLevel--;
            if (_suppressLevel == 0) ForceReset();
        }
        public void AddSilent(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            CheckReentrancy();
            _suppressLevel++;
            try
            {
                Items.Add(item);
            }
            finally
            {
                _suppressLevel--;
            }

            if (_suppressLevel == 0)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
        }
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (items is ICollection<T> collection && collection.Count == 0) return;
            CheckReentrancy();
            _suppressLevel++;

            try
            {
                foreach (var item in items)
                    Items.Add(item);
            }
            finally
            {
                _suppressLevel--;
            }

            // Single notification
            if (_suppressLevel == 0)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        public void ReplaceAll(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            // if (items is ICollection<T> collection && collection.Count == 0) return;
            CheckReentrancy();
            _suppressLevel++;

            try
            {
                Items.Clear();
                foreach (var item in items)
                    Items.Add(item);
            }
            finally
            {
                _suppressLevel--;
            }
            // Single notification
            if (_suppressLevel == 0)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        public void RemoveRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            var removeSet = new HashSet<T>(items);
            if (removeSet.Count == 0) return;
            CheckReentrancy();
            _suppressLevel++;

            try
            {
                var remaining = Items.Where(x => !removeSet.Contains(x)).ToList();
                Items.Clear();
                foreach (var item in remaining)
                    Items.Add(item);
            }
            finally
            {
                _suppressLevel--;
            }
            // Single notification
            if (_suppressLevel == 0)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        public void Shuffle()
        {
            if (Items.Count <= 1) return;

            CheckReentrancy();
            _suppressLevel++;

            try
            {
                var random = new Random();
                int n = Items.Count;

                // Fisher-Yates shuffle in-place
                for (int i = n - 1; i > 0; i--)
                {
                    int j = random.Next(0, i + 1);
                    var temp = Items[i];
                    Items[i] = Items[j];
                    Items[j] = temp;
                }
            }
            finally
            {
                _suppressLevel--;
            }
            if (_suppressLevel == 0)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        public void Sort(ICollectionView collectionView)
        {
            if (collectionView == null) return;
            var sortedList = collectionView.Cast<T>().ToList();
            ReplaceAll(sortedList);
        }
        public void SortSilent(ICollectionView collectionView)
        {
            if (collectionView == null || collectionView.SourceCollection != this) return;

            _suppressLevel++;
            try
            {
                var sortedList = collectionView.Cast<T>().ToList();

                for (int i = 0; i < sortedList.Count; i++)
                {
                    int currentIndex = IndexOf(sortedList[i]);
                    if (currentIndex != i && currentIndex >= 0)
                    {
                        MoveItem(currentIndex, i);
                    }
                }
            }
            finally
            {
                _suppressLevel--;
            }
        }
        public void SortSilent(IComparer<T> comparer)
        {
            _suppressLevel++;
            try
            {
                var sortedList = Items.OrderBy(x => x, comparer).ToList();

                for (int i = 0; i < sortedList.Count; i++)
                {
                    int currentIndex = Items.IndexOf(sortedList[i]);
                    if (currentIndex != i)
                    {
                        MoveItem(currentIndex, i);
                    }
                }
            }
            finally
            {
                _suppressLevel--;
            }
        }
        public void ForceReset()
        {
            CheckReentrancy();
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IEnumerable<T> Search(Func<T, bool> predicate, SearchOptions? options = null)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            options ??= new SearchOptions();

            // Try cache first
            if (options.UseCache && !string.IsNullOrEmpty(options.CacheKey))
                return CachedSearch(options.CacheKey!, predicate);

            // Use parallel if needed
            if (options.UseParallel && Items.Count >= options.ParallelThreshold)
            {
                var snapshot = Items.ToList();
                return snapshot.AsParallel().Where(predicate);
            }

            // Default sequential
            return Items.Where(predicate);
        }
        private Dictionary<string, List<T>>? _searchCache;
        private const int MaxCacheSize = 50;
        public IEnumerable<T> CachedSearch(string cacheKey, Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            _searchCache ??= new Dictionary<string, List<T>>();

            if (!string.IsNullOrEmpty(cacheKey) && _searchCache.TryGetValue(cacheKey, out var cached))
                return cached;

            var results = Items.Where(predicate).ToList();

            if (!string.IsNullOrEmpty(cacheKey))
            {
                if (_searchCache.Count >= MaxCacheSize)
                    _searchCache.Clear(); // Simple eviction

                _searchCache[cacheKey] = results;
            }

            return results;
        }
        public void InvalidateSearchCache() => _searchCache?.Clear();

        public new void Clear()
        {
            if (Items.Count == 0) return;
            CheckReentrancy();
            _suppressLevel++;

            try
            {
                Items.Clear();
            }
            finally
            {
                _suppressLevel--;
            }

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public void ClearSilent()
        {
            if (Items.Count == 0) return;
            CheckReentrancy();
            Items.Clear();
        }
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            InvalidateSearchCache();
        }
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            InvalidateSearchCache();
        }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressLevel == 0)
                base.OnCollectionChanged(e);
        }
    }

    public class OptimizedBulkObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var itemsList = items.ToList();
            if (!itemsList.Any()) return;

            CheckReentrancy();
            _suppressNotification = true;

            try
            {
                foreach (var item in itemsList)
                {
                    Items.Add(item);
                }
            }
            finally
            {
                _suppressNotification = false;
            }

            // Chỉ raise event một lần duy nhất
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }
        public new void Clear()
        {
            if (Items.Count == 0) return;

            CheckReentrancy();
            _suppressNotification = true;

            try
            {
                Items.Clear();
            }
            finally
            {
                _suppressNotification = false;
            }

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }
    }
}
