using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Sm5sh.GUI.Helpers
{
    /// <summary>
    /// SilentObservableCollection is a ObservableCollection with some extensions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        public RangeObservableCollection()
        {

        }

        public RangeObservableCollection(IEnumerable<T> list)
            :base(list)
        {

        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        public void AddRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            _suppressNotification = true;

            foreach (T item in list)
            {
                Add(item);
            }
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list null");

            _suppressNotification = true;

            foreach (T item in list)
            {
                if (Contains(item) == true)
                {
                    Remove(item);
                }
            }
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
