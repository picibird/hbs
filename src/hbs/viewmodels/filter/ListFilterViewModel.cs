// ListFilterViewModel.cs
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
using System.Text;
using picibird.shelfhub;
using picibits.core.mvvm;
using Filter = picibird.hbs.ldu.Filter;

namespace picibird.hbs.viewmodels.filter
{
    public class ListFilterViewModel : FilterViewModel
    {
        public ListFilterViewModel(Facet category = null)
            : base(category)
        {
            Style = new ViewStyle("ListFilterViewStyle");
            FilterSelectionViewModel = new FilterSelectionListViewModel(category, SelectedFilter);
            FilterSelectionViewModel.SelectedFilter.ItemAdded += OnSelectedFilterItemAdded;
            FilterSelectionViewModel.SelectedFilter.ItemRemoved += OnSelectedFilterItemRemoved;
        }

        #region FilterSelectionView

        public FilterSelectionListViewModel FilterSelectionViewModel { get; }

        #endregion FilterSelectionView

        private void OnSelectedFilterItemAdded(object sender, FacetValue item)
        {
            OnSelectedFiltersChanged(FilterSelectionViewModel.SelectedFilter);
        }

        private void OnSelectedFilterItemRemoved(object sender, FacetValue item)
        {
            OnSelectedFiltersChanged(FilterSelectionViewModel.SelectedFilter);
        }

        private void OnSelectedFiltersChanged(IEnumerable<FacetValue> filter)
        {
            //update has selected
            HasSelected = FilterSelectionViewModel.SelectedFilter.Count > 0;
            //string to show to user
            if (FilterSelectionViewModel.SelectedFilter.Count > 0)
            {
                var sb = new StringBuilder();
                var ordFilter = filter.OrderBy(
                    ordF => FilterSelectionViewModel.ItemIndexOf(ordF)).ToArray();
                if (ordFilter.Length == 0)
                {
                    AppliedInfoString = "";
                }
                else if (ordFilter.Length == 1)
                {
                    AppliedInfoString = ordFilter[0].Name;
                }
                else
                {
                    for (int i = 0; i <= ordFilter.Length - 2; i++)
                    {
                        sb.Append(ordFilter[i].Name);
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(" und ");
                    sb.Append(ordFilter[ordFilter.Length - 1].Name);
                    AppliedInfoString = sb.ToString();
                }
            }
            else
            {
                AppliedInfoString = "";
            }
        }

        #region HasSelected

        private bool mHasSelected;

        public bool HasSelected
        {
            get { return mHasSelected; }
            set
            {
                if (mHasSelected != value)
                {
                    var old = mHasSelected;
                    mHasSelected = value;
                    RaisePropertyChanged("HasSelected", old, value);
                }
            }
        }

        #endregion HasSelected
    }
}