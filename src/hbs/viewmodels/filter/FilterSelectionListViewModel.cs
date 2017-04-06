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

using System;
using System.Linq;
using System.Reflection;
using picibird.hbs.ldu;
using picibird.shelfhub;
using picibits.app.services;
using picibits.app.window;
using picibits.core;
using picibits.core.collection;
using picibits.core.controls;
using picibits.core.export.instances;
using picibits.core.mvvm;
using picibits.core.util;
using Filter = picibird.hbs.ldu.Filter;

namespace picibird.hbs.viewmodels.filter
{
    public class FilterSelectionListViewModel : ItemsViewModel
    {

        public static Size MainWindowActualSize
        {
            get
            {
                return HBS.ViewModel.ActualSize;
            }
        }

        #region MaxHeight

        private double _mMaxHeight;

        public double MaxHeight
        {
            get
            {
                if (_mMaxHeight <= 0)
                {
                    int activeFilterCount = HBS.ViewModel.Filters.Items.Count;
                    double afShrinkFactor = (activeFilterCount - 1) * (0.15 * MainWindowActualSize.Height);
                    _mMaxHeight = MainWindowActualSize.Height * 0.65 - afShrinkFactor;
                }
                    
                return _mMaxHeight;
            }
            set
            {
                if (_mMaxHeight != value)
                {
                    var old = _mMaxHeight;
                    _mMaxHeight = value;
                    RaisePropertyChanged("MaxHeight", old, value);
                }
            }
        }

        #endregion MaxHeight

        public FilterSelectionListViewModel(Facet filterCategory,
            PiciObservableCollection<FacetValue> selectedFilter)
        {
            CategoryName = filterCategory.Key;
            Style = new ViewStyle("FilterSelectionListViewStyle");
            VisualState = "Expanded";

            SelectedFilter = selectedFilter;
            SelectedFilter.ItemAdded += OnSelectedFilterItemAdded;
            SelectedFilter.ItemRemoved += OnSelectedFilterItemRemoved;

            foreach (var filter in filterCategory.Values)
                OnCategoryFilterAdded(filter);

            HBS.Search.FilterList.ItemAdded += OnSearchFilterListItemAdded;
            HBS.Search.FilterList.RemovedAll += OnSearchFilterListRemovedAll;
        }

        public PiciObservableCollection<FacetValue> SelectedFilter { get; }

        public string CategoryName { get; }

        private void OnSearchFilterListRemovedAll(PiciObservableCollection<Facet> sender)
        {
            Items.RemoveAll();
        }

        private void OnSearchFilterListItemAdded(object sender, Facet item)
        {
            if (item.Key.Equals(CategoryName))
            {
                foreach (var filter in item.Values)
                    OnCategoryFilterAdded(filter);
            }
        }

        public event SimpleEventHandler<FacetValue> CategoryAdded;

        protected virtual void OnCategoryFilterAdded(FacetValue filter)
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

        private void OnSelectedFilterItemAdded(object sender, FacetValue item)
        {
            Pici.Log.info(typeof(FilterSelectionListViewModel), string.Format("selected filter {0}", item.Name));
        }

        private void OnSelectedFilterItemRemoved(object sender, FacetValue item)
        {
            Pici.Log.info(typeof(FilterSelectionListViewModel), string.Format("unselected filter {0}", item.Name));
        }

        public FilterSelectionItemViewModel GetItemForFilter(FacetValue filter)
        {
            return Items.Cast<FilterSelectionItemViewModel>().FirstOrDefault(f => f.Filter.Value.Equals(filter.Value));
        }

        public int ItemIndexOf(FacetValue filter)
        {
            return Items.IndexOf(GetItemForFilter(filter));
        }
    }
}