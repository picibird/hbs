// FilterSelectionListViewModel.cs
// Date Created: 20.01.2016
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

using System.Linq;
using picibird.hbs.ldu;
using picibits.core;
using picibits.core.collection;
using picibits.core.controls;
using picibits.core.mvvm;
using picibits.core.util;

namespace picibird.hbs.viewmodels.filter
{
    public class FilterSelectionListViewModel : ItemsViewModel
    {
        public FilterSelectionListViewModel(FilterCategory filterCategory,
            PiciObservableCollection<Filter> selectedFilter)
        {
            CategoryName = filterCategory.Id;
            Style = new ViewStyle("FilterSelectionListViewStyle");
            VisualState = "Expanded";

            SelectedFilter = selectedFilter;
            SelectedFilter.ItemAdded += OnSelectedFilterItemAdded;
            SelectedFilter.ItemRemoved += OnSelectedFilterItemRemoved;

            foreach (var filter in filterCategory.Filter)
                OnCategoryFilterAdded(filter);

            HBS.Search.FilterList.ItemAdded += OnSearchFilterListItemAdded;
            HBS.Search.FilterList.RemovedAll += OnSearchFilterListRemovedAll;
        }

        public PiciObservableCollection<Filter> SelectedFilter { get; }

        public FilterCategoryId CategoryName { get; }

        private void OnSearchFilterListRemovedAll(PiciObservableCollection<FilterCategory> sender)
        {
            Items.RemoveAll();
        }

        private void OnSearchFilterListItemAdded(object sender, FilterCategory item)
        {
            if (item.Id.Equals(CategoryName))
            {
                foreach (var filter in item.Filter)
                    OnCategoryFilterAdded(filter);
            }
        }

        public event SimpleEventHandler<Filter> CategoryAdded;

        protected virtual void OnCategoryFilterAdded(Filter filter)
        {
            var item = new FilterSelectionItemViewModel(filter);
            foreach (var selected in SelectedFilter)
            {
                if (filter.Equals(selected))
                {
                    item.IsChecked = true;
                    break;
                }
            }
            item.IsCheckedChanged += OnIsCheckedChanged;
            Items.Add(item);
            //fire event
            if (CategoryAdded != null)
                CategoryAdded(filter);
        }

        private void OnAllCategoryFilterRemoved(PiciObservableCollection<Filter> sender)
        {
            Items.RemoveAll();
        }

        public void OnIsCheckedChanged(object sender, bool e)
        {
            var item = sender as FilterSelectionItemViewModel;
            if (item.IsChecked)
                SelectedFilter.Add(item.Filter);
            else
                SelectedFilter.Remove(item.Filter);
        }

        private void OnSelectedFilterItemAdded(object sender, Filter item)
        {
            Pici.Log.info(typeof(FilterSelectionListViewModel), string.Format("selected filter {0}", item.Name));
        }

        private void OnSelectedFilterItemRemoved(object sender, Filter item)
        {
            Pici.Log.info(typeof(FilterSelectionListViewModel), string.Format("unselected filter {0}", item.Name));
        }

        public FilterSelectionItemViewModel GetItemForFilter(Filter filter)
        {
            return Items.Cast<FilterSelectionItemViewModel>().FirstOrDefault(f => f.Filter.Id.Equals(filter.Id));
        }

        public int ItemIndexOf(Filter filter)
        {
            return Items.IndexOf(GetItemForFilter(filter));
        }
    }
}