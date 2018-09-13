// SearchRequest.cs
// Date Created: 21.01.2016
// 
// Copyright (c) 2016, picibird GmbH 
// All rights reserved.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using picibird.hbs.ldu.Helper;
using picibird.shelfhub;

namespace picibird.hbs.ldu
{
    public class SearchRequest : NotifyPropertyChanged
    {
        private int itemsPerPage = 17;

        private List<shelfhub.Filter> activeFilters = new List<shelfhub.Filter>();
        public event PropertyChangedEventHandler FilterListChanged;

        public event PropertyChangedEventHandler SortingChanged;

        public SortOrder SortOrder = new SortOrder();
        public SortDirection SortDirection = SortDirection.descending;

        public int? MaximumRecords { get; set; }

        public string SearchString { get; private set; }

        public void SetSortOrder(SortOrder f, SortDirection order)
        {
            bool changed = false;
            if (SortDirection != order)
            {
                SortDirection = order;
                changed = true;
            }
            if (SortOrder != f)
            {
                SortOrder = f;
                changed = true;
            }
            if (changed)
            {
                SortingChanged?.Invoke(this, new PropertyChangedEventArgs("sortorder"));
            }
        }

        public void AddFilter(string key, List<FacetValue> filter)
        {
            IEnumerable<string> values = filter.Select((fv) => fv.Value);
            activeFilters.Add(new shelfhub.Filter()
            {
                Key = key,
                Values = new ObservableCollection<string>(values)
            });
            notifyFilterListChanged();
        }

        public void RemoveFilter(string key, List<FacetValue> filter)
        {
            var removedfilter = activeFilters.FirstOrDefault((f) => f.Key == key);
            activeFilters.Remove(removedfilter);
            notifyFilterListChanged();
        }

        public List<shelfhub.Filter> GetActiveFilter()
        {
            return new List<shelfhub.Filter>(activeFilters);
        }

        //public bool HasSourceFilter()
        //{
        //    foreach (FacetValue f in activeFilters)
        //    {
        //        if (f.Value == "xtargets")
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public List<string> GetSourceFilterIds()
        //{
        //    return
        //        (from f in activeFilters where f.Value == "xtargets" select f.Value)
        //            .ToList<string>();
        //}

        public void ClearAllFilters()
        {
            activeFilters.Clear();
            notifyFilterListChanged();
        }

        private void notifyFilterListChanged()
        {

            FilterListChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs("activeFilters"));
        }

        public int ItemsPerPage
        {
            get { return itemsPerPage; }
            set { SetProperty(ref itemsPerPage, value); }
        }

        private int pageIdx;

        public int PageIdx
        {
            get { return pageIdx; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value == pageIdx)
                {
                    return;
                }
                SetProperty(ref pageIdx, value);
            }
        }

        public SearchRequest(string searchString)
        {
            this.SearchString = searchString;
        }


        internal List<string> GetActiveQueryFilterStrings()
        {
            return new List<string>();
        }

        internal List<shelfhub.Filter> GetActiveFilters()
        {
            return activeFilters;
        }
    }
}