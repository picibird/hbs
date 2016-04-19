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
using System.ComponentModel;
using System.Linq;
using picibird.hbs.ldu.Helper;

namespace picibird.hbs.ldu
{
    public class SearchRequest : NotifyPropertyChanged
    {
        private int itemsPerPage = Pazpar2Settings.RESULTS_PER_PAGE;
        
        private HashSet<Filter> activeFilters = new HashSet<Filter>();
        public event PropertyChangedEventHandler FilterListChanged;

        public event PropertyChangedEventHandler SortingChanged;

        public SortOrder SortOrder = SortOrder.relevance;
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
                SortingChanged(this, new PropertyChangedEventArgs("sortorder"));
            }
        }

        public bool AddFilter(Filter filter)
        {
            if (activeFilters.Add(filter))
            {
                notifyFilterListChanged();
                return true;
            }
            return false;
        }

        public bool AddFilter(List<Filter> filter)
        {
            if (filter.Count > 0)
            {
                int count = activeFilters.Count;
                activeFilters.UnionWith(filter);
                if (activeFilters.Count > count)
                {
                    notifyFilterListChanged();
                    return true;
                }
            }
            return false;
        }

        public bool RemoveFilter(Filter filter)
        {
            if (activeFilters.Remove(filter))
            {
                notifyFilterListChanged();
                return true;
            }
            return false;
        }

        public bool RemoveFilter(List<Filter> filter)
        {
            if (filter.Count > 0)
            {
                int count = activeFilters.Count;
                activeFilters.RemoveWhere(f => filter.Any(f2 => f.Equals(f2)));
                if (activeFilters.Count < count)
                {
                    notifyFilterListChanged();
                    return true;
                }
            }
            return false;
        }

        public HashSet<Filter> GetActiveFilter()
        {
            return new HashSet<Filter>(activeFilters);
        }

        public bool HasSourceFilter()
        {
            foreach (Filter f in activeFilters)
            {
                if (f.Catgegory == FilterCategoryId.xtargets)
                {
                    return true;
                }
            }
            return false;
        }

        public List<string> GetSourceFilterIds()
        {
            return (from f in activeFilters where f.Catgegory == FilterCategoryId.xtargets select f.TargetId).ToList<string>();
        }

        public void ClearAllFilters()
        {
            activeFilters.Clear();
            notifyFilterListChanged();
        }

        private void notifyFilterListChanged()
        {
            var handler = FilterListChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs("activeFilters"));
            }
        }

        public int ItemsPerPage
        {
            get { return itemsPerPage; }
            set
            {
                SetProperty(ref itemsPerPage, value);
            }
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
            return (from filter in activeFilters where filter.Catgegory == FilterCategoryId.author || filter.Catgegory == FilterCategoryId.subject select filter.CatgegoryKey + "=" + filter.Id).ToList<string>();
        }

        internal HashSet<Filter> GetActiveFilters()
        {
            return activeFilters;
        }
    }


}
