// FilterViewModel.cs
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
using picibird.hbs.ldu;
using picibits.app.mvvm;
using picibits.core.collection;
using picibits.core.helper;
using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.filter
{
    public class FilterViewModel : ButtonViewModel
    {
        public FilterViewModel(FilterCategory category = null)
        {
            Category = category;
            SelectedFilter = new PiciObservableCollection<Filter>();
            Style = new ViewStyle("FilterViewStyle");
            VisualState = FilterVisualStates.EDIT;
            if (category != null)
            {
                FilterName = Category.Name;
                foreach (var filter in category.Filter)
                    OnCategoryFilterAdded(filter);
            }
            TapBehaviour.Tap += OnTap;
            ApplyButtonViewModel.TapBehaviour.Tap += OnApplyTap;
        }

        #region Category

        public FilterCategory Category { get; }

        #endregion Category

        public PiciObservableCollection<Filter> SelectedFilter { get; protected set; }

        private void OnCategoryFilterAdded(Filter filter)
        {
        }

        private void OnTap(object sender, EventArgs e)
        {
            if (VisualState == FilterVisualStates.NORMAL)
                VisualState = FilterVisualStates.Delete;
            else if (VisualState == FilterVisualStates.Delete)
                VisualState = FilterVisualStates.NORMAL;
        }

        private void OnApplyTap(object sender, EventArgs e)
        {
            if (VisualState == FilterVisualStates.EDIT)
                VisualState = FilterVisualStates.NORMAL;
        }

        protected override void OnVisualStateChanged(string oldVisualState, string newVisualState)
        {
            base.OnVisualStateChanged(oldVisualState, newVisualState);
            //dis/enabled taps according to state
            var isFilterOpen =
                Pointing.IsEnabled =
                    newVisualState.Equals(FilterVisualStates.NORMAL) || newVisualState.Equals(FilterVisualStates.Delete);
            ApplyButtonViewModel.Pointing.IsEnabled = newVisualState.Equals(FilterVisualStates.EDIT) ||
                                                      newVisualState.Equals(FilterVisualStates.Delete);
            if (newVisualState.Equals(FilterVisualStates.Delete))
            {
                ApplyButtonViewModel.Text = "Löschen";
                Timer.OnceAfter(3000, () =>
                {
                    if (VisualState == FilterVisualStates.Delete)
                    {
                        VisualState = FilterVisualStates.NORMAL;
                    }
                });
            }
            if (newVisualState.Equals(FilterVisualStates.EDIT))
            {
                ApplyButtonViewModel.Text = "Anwenden";
            }
        }

        #region FilterName

        private string mFilterName;

        public string FilterName
        {
            get { return mFilterName; }
            set
            {
                if (mFilterName != value)
                {
                    var old = mFilterName;
                    mFilterName = value;
                    RaisePropertyChanged("FilterName", old, value);
                }
            }
        }

        #endregion FilterName

        #region AppliedInfoString

        private string mAppliedInfoString = "";

        public string AppliedInfoString
        {
            get { return mAppliedInfoString; }
            set
            {
                if (mAppliedInfoString != value)
                {
                    var old = mAppliedInfoString;
                    mAppliedInfoString = value;
                    RaisePropertyChanged("AppliedInfoString", old, value);
                }
            }
        }

        #endregion AppliedInfoString

        #region ApplyButtonViewModel

        private ButtonViewModel mApplyButtonViewModel;

        public ButtonViewModel ApplyButtonViewModel
        {
            get
            {
                if (mApplyButtonViewModel == null)
                {
                    mApplyButtonViewModel = new ButtonViewModel("Anwenden");
                    mApplyButtonViewModel.Style = new ViewStyle("FilterButtonViewStyle");
                    mApplyButtonViewModel.TapBehaviour.Tap += OnApplyTap;
                }
                return mApplyButtonViewModel;
            }
        }

        #endregion ApplyButtonViewModel
    }

    public static class FilterVisualStates
    {
        public static readonly string NORMAL = "Normal";
        public static readonly string EDIT = "Edit";
        public static readonly string Delete = "Delete";
    }
}