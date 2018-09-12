// Sorting.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using picibird.hbs.ldu;
using picibird.shelfhub;
using picibits.core;
using picibits.core.mvvm;
using picibits.core.util;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace picibird.hbs.viewmodels.infoShield
{
    public class Sorting : Model
    {
        public static readonly ObservableCollection<SortOrderFunction> AllSortOrderFunctions = new ObservableCollection<SortOrderFunction>(new[]
        {
            new SortOrderFunction(new SortOrder())
        });

        public static event SimpleEventHandler<bool> IsSortingEnabledChanged;
        private static Action<bool> OnIsEnabledChanged = (value) =>
        {
            mIsSortingEnabled = value;
            IsSortingEnabledChanged?.Invoke(value);
        };

        private static bool mIsSortingEnabled;
        public static bool IsSortingEnabled
        {
            get => mIsSortingEnabled;
            set => OnIsEnabledChanged(value);
        }

        public Sorting()
        {
            HBS.Search.PropertyChanged += OnSearchPropertyChanged;
            IsSortingEnabledChanged += (value) => IsVisible = value;
        }


        public ObservableCollection<SortOrderFunction> SortOrderFunctions
        {
            get { return AllSortOrderFunctions; }
        }

        #region SelectedSortOrderFunction

        private SortOrderFunction mSelectedSortOrderFunction = AllSortOrderFunctions.First();

        public SortOrderFunction SelectedSortOrderFunction
        {
            get { return mSelectedSortOrderFunction; }
            set
            {
                if (mSelectedSortOrderFunction != value)
                {
                    var old = mSelectedSortOrderFunction;
                    mSelectedSortOrderFunction = value;
                    OnSelectedSortOrderFunctionChanged(old, value);
                }
            }
        }

        protected virtual void OnSelectedSortOrderFunctionChanged(SortOrderFunction oldSelectedSortOrderFunction,
            SortOrderFunction newSelectedSortOrderFunction)
        {
            RaisePropertyChanged("SelectedSortOrderFunction", oldSelectedSortOrderFunction, newSelectedSortOrderFunction);
            //set prefered direction for specific order function
            //block search updates while setting direction
            blockUpdateSearchSorting = true;
            switch (newSelectedSortOrderFunction.EnumValue.Type)
            {
                case SortFieldType.Score:
                case SortFieldType.Date:
                case SortFieldType.Numerical:
                    IsAscendingSelected = false;
                    IsDescendingSelected = true;
                    break;
                case SortFieldType.Alphabetical:
                    IsAscendingSelected = true;
                    IsDescendingSelected = false;
                    break;
            }
            blockUpdateSearchSorting = false;
            //update search
            UpdateSearchSorting(newSelectedSortOrderFunction.EnumValue, SelectedSortDirection);
        }

        #endregion SelectedSortOrderFunction

        #region PagesCount

        private int mPagesCount;

        public int PagesCount
        {
            get { return mPagesCount; }
            set
            {
                if (mPagesCount != value)
                {
                    var old = mPagesCount;
                    mPagesCount = value;
                    RaisePropertyChanged("PagesCount", old, value);
                }
            }
        }

        #endregion PagesCount

        #region IsEnabled

        private bool mIsVisible = false;

        public bool IsVisible
        {
            get { return mIsVisible; }
            set
            {
                if (mIsVisible != value)
                {
                    var old = mIsVisible;
                    mIsVisible = value;
                    RaisePropertyChanged(nameof(IsVisible), old, value);
                }
            }
        }

        #endregion IsEnabled

        #region IsAscendingSelected

        private bool mIsAscendingSelected;

        public bool IsAscendingSelected
        {
            get { return mIsAscendingSelected; }
            set
            {
                if (mIsAscendingSelected != value)
                {
                    var old = mIsAscendingSelected;
                    mIsAscendingSelected = value;
                    OnIsAscendingSelectedChanged(old, value);
                }
            }
        }

        protected virtual void OnIsAscendingSelectedChanged(bool oldIsAscendingSelected, bool newIsAscendingSelected)
        {
            RaisePropertyChanged("IsAscendingSelected", oldIsAscendingSelected, newIsAscendingSelected);
            SelectedSortDirection = SortDirection.ascending;
        }

        #endregion IsAscendingSelected

        #region IsDescendingSelected

        private bool mIsDescendingSelected;

        public bool IsDescendingSelected
        {
            get { return mIsDescendingSelected; }
            set
            {
                if (mIsDescendingSelected != value)
                {
                    var old = mIsDescendingSelected;
                    mIsDescendingSelected = value;
                    OnIsDescendingSelectedChanged(old, value);
                }
            }
        }

        protected virtual void OnIsDescendingSelectedChanged(bool oldIsDescendingSelected, bool newIsDescendingSelected)
        {
            RaisePropertyChanged("IsDescendingSelected", oldIsDescendingSelected, newIsDescendingSelected);
            SelectedSortDirection = SortDirection.descending;
        }

        #endregion IsDescendingSelected

        #region SelectedSortDirection

        private SortDirection mSelectedSortDirection;

        public SortDirection SelectedSortDirection
        {
            get { return mSelectedSortDirection; }
            set
            {
                if (mSelectedSortDirection != value)
                {
                    var old = mSelectedSortDirection;
                    mSelectedSortDirection = value;
                    OnSelectedSortDirectionChanged(old, value);
                }
            }
        }

        protected virtual void OnSelectedSortDirectionChanged(SortDirection oldSelectedSortOrder,
            SortDirection newSelectedSortDirection)
        {
            RaisePropertyChanged("SelectedSortDirection", oldSelectedSortOrder, newSelectedSortDirection);
            UpdateSearchSorting(SelectedSortOrderFunction.EnumValue, newSelectedSortDirection);
        }

        #endregion SelectedSortDirection

        #region Methods

        private bool blockUpdateSearchSorting;

        private void UpdateSearchSorting(SortOrder order, SortDirection direction)
        {
            if (HBS.Search.SearchRequest != null && !blockUpdateSearchSorting)
            {
                var request = HBS.Search.SearchRequest;
                if (request.SortOrder != order || request.SortDirection != direction)
                {
                    request.SetSortOrder(order, direction);
                    Pici.Log.debug(typeof(InfoShieldVM), string.Format("sort changed to {0} {1}", order, direction));
                }
            }
        }

        private void OnSearchPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Callback"))
            {
                PagesCount = HBS.Search.Callback.MaxPageIndex + 1;
                HBS.Search.Callback.MaxPageIndexChanged +=
                    (s, arg) => { PagesCount = HBS.Search.Callback.MaxPageIndex + 1; };
            }
            if (e.PropertyName.Equals("SearchRequest"))
            {
                var searchRequest = HBS.Search.SearchRequest;
                OnSearchSortingChanged(HBS.Search, null);
                searchRequest.SortingChanged += OnSearchSortingChanged;
            }
        }

        private void OnSearchSortingChanged(object sender, PropertyChangedEventArgs e)
        {
            blockUpdateSearchSorting = true;
            var searchRequest = HBS.Search.SearchRequest;
            var sortOrder = AllSortOrderFunctions.First(sof => sof.EnumValue.Equals(searchRequest.SortOrder));
            var sortDirection = searchRequest.SortDirection;
            IsVisible = IsSortingEnabled;
            //order
            SelectedSortOrderFunction = sortOrder;
            //direction
            IsAscendingSelected = sortDirection == SortDirection.ascending;
            IsDescendingSelected = sortDirection == SortDirection.descending;
            SelectedSortDirection = sortDirection;
            blockUpdateSearchSorting = false;
        }

        #endregion Methods
    }
}