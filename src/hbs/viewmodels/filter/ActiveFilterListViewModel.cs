// ActiveFilterListViewModel.cs
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

using System.Collections.Generic;
using System.Linq;
using picibird.shelfhub;
using picibits.core.collection;
using picibits.core.controls;
using picibits.core.helper;
using picibits.core.mvvm;
using Filter = picibird.hbs.ldu.Filter;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace picibird.hbs.viewmodels.filter
{
    public class ActiveFilterListViewModel : ItemsViewModel
    {
        public ActiveFilterListViewModel()
        {
            Style = new ViewStyle("ActiveFilterListViewStyle");
            Items.ItemAdded += OnFilterAdded;
            Items.ItemRemoved += OnFilterRemoved;
            HBS.Search.SearchStarting += OnSearchStarting;
        }

        private void OnSearchStarting(object sender, SearchStartingEventArgs e)
        {
            if (e.Reason == SearchStartingReason.NewSearch)
            {
                Items.RemoveAll();
            }
        }


        protected override void OnViewIsLoadedChanged(bool oldViewIsLoaded, bool newViewIsLoaded)
        {
            base.OnViewIsLoadedChanged(oldViewIsLoaded, newViewIsLoaded);
            if (newViewIsLoaded)
            {
                Items.Add(new FilterContainerViewModel());
            }
        }

        public void OnFilterAdded(object sender, ViewModel item)
        {
            var fc = item as FilterContainerViewModel;
            fc.Chooser.PropertyChanged += OnFilterChooserPropertyChanged;
            fc.PropertyChanged += OnFilterPropertyChanged;
            fc.FiltersApplied += OnFiltersApplied;
        }

        public void OnFilterRemoved(object sender, ViewModel item)
        {
            var fc = item as FilterContainerViewModel;
            fc.Chooser.PropertyChanged -= OnFilterChooserPropertyChanged;
            fc.PropertyChanged -= OnFilterPropertyChanged;
            fc.FiltersApplied -= OnFiltersApplied;
            Events.OnIdleOnce(() => { EnsureFilterSelection(); });
            if (fc?.Filter?.SelectedFilter != null)
                RemoveActiveFiltersNotSelected(fc, fc.Filter.SelectedFilter);
        }

        private bool EnsureFilterSelection()
        {
            var filters = Items.Cast<FilterContainerViewModel>();

            if (!filters.Any(f => f.Chooser != null && f.Chooser.VisualState == FilterChooserStates.CLOSED) &&
                !filters.Any(f => f.Chooser != null && f.Chooser.VisualState == FilterChooserStates.OPENED) &&
                !filters.Any(f => f.Filter != null && f.Filter.VisualState == FilterVisualStates.EDIT))
            {
                Items.Add(new FilterContainerViewModel());
                return true;
            }
            return false;
        }

        private void OnFilterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var fcvm = sender as FilterContainerViewModel;
            var args = e as global::picibits.core.mvvm.PropertyChangedEventArgs;
            if (e.PropertyName.Equals("VisualState"))
            {
                if (fcvm.VisualState == FilterContainerVisualStates.DISCARDED)
                {
                    Items.Remove(fcvm);
                }
            }
            if (e.PropertyName.Equals("Filter"))
            {
                var oldFvm = args.Old as FilterViewModel;
                if (oldFvm != null) oldFvm.PropertyChanged -= OnFilterFilterPropertyChanged;
                var newFvm = args.New as FilterViewModel;
                if (newFvm != null) newFvm.PropertyChanged += OnFilterFilterPropertyChanged;
            }
        }

        private void OnFilterChooserPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("VisualState"))
            {
                EnsureFilterSelection();
            }
        }

        private void OnFilterFilterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("VisualState"))
            {
                EnsureFilterSelection();
            }
        }

        public List<FacetValue> GetAllSelectedFilter()
        {
            IEnumerable<FacetValue> allSelectedFilter = new List<FacetValue>();
            var validFilter = Items.Cast<FilterContainerViewModel>().Where(f => f.Filter != null);
            foreach (var fc in validFilter)
            {
                allSelectedFilter = allSelectedFilter.Union(fc.Filter.SelectedFilter);
            }
            return allSelectedFilter.ToList();
        }

        private void OnFiltersApplied(FilterContainerViewModel filterContainer, PiciObservableCollection<FacetValue> filter)
        {

            Facet facet = filterContainer.Filter.Category;

            HBS.Search.SearchRequest.AddFilter(facet.Key, filter.ToList());
            //RemoveActiveFiltersNotSelected();
        }

        private void RemoveActiveFiltersNotSelected(FilterContainerViewModel filterContainer, PiciObservableCollection<FacetValue> removed)
        {
            //var allSelectedFilter = GetAllSelectedFilter();
            //var activeFilter = HBS.Search.SearchRequest.GetActiveFilter();
            //var removed = activeFilter.Where(f => !allSelectedFilter.Contains(f)).ToList();

            Facet facet = filterContainer.Filter.Category;
            HBS.Search.SearchRequest.RemoveFilter(facet.Key, removed.ToList());
        }
    }
}