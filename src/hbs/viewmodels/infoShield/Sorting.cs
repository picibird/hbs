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
using System.Collections.Generic;
using System.Linq;
using picibird.hbs.ldu;

using picibits.core;
using picibits.core.mvvm;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace picibird.hbs.viewmodels.infoShield
{
    public class Sorting : Model
    {
        public static readonly List<SortOrderFunction> AllSortOrderFunctions = new List<SortOrderFunction>(new[]
        {
            new SortOrderFunction(SortOrder.relevance),
            new SortOrderFunction(SortOrder.date),
            new SortOrderFunction(SortOrder.author),
            new SortOrderFunction(SortOrder.title)
            //new SortOrderFunction(SortOrder.position)
        });

        public Sorting()
        {
            HBS.Search.PropertyChanged += OnSearchPropertyChanged;
        }


        public List<SortOrderFunction> SortOrderFunctions
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
            switch (newSelectedSortOrderFunction.EnumValue)
            {
                case SortOrder.relevance:
                case SortOrder.date:
                    IsAscendingSelected = false;
                    IsDescendingSelected = true;
                    break;
                case SortOrder.title:
                case SortOrder.author:
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
                    Pici.Log.debug(typeof (InfoShieldVM), string.Format("sort changed to {0} {1}", order, direction));
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