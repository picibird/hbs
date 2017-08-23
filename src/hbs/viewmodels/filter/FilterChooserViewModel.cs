// FilterChooserViewModel.cs
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
using picibird.hbs.ldu;
using picibird.shelfhub;
using picibits.app.mvvm;
using picibits.core.collection;
using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.filter
{
    public class FilterChooserViewModel : ButtonViewModel
    {
        public FilterChooserViewModel()
        {
            Style = new ViewStyle("FilterChooserViewStyle");

            TapBehaviour.Tap += OnTap;
            VisualState = FilterChooserStates.CLOSED;


            HBS.Search.SearchStarting += OnSearchStarting;
            if (HBS.Search.FilterList != null)
            {
                HBS.Search.FilterList.ItemAdded += OnSearchFilterListItemAdded;
                HBS.Search.FilterList.ItemRemoved += OnSearchFilterListItemRemoved;
            }
            UpdateChoosers();
        }

        public void UpdateChoosers()
        {
            var fl = HBS.Search.FilterList;
            if (fl != null)
                UpdateChoosers(fl);
        }

        public void UpdateChoosers(FilterList<Facet> categories)
        {
            foreach (var category in categories)
            {
                UpdateChooser(category);
            }
        }

        public void UpdateChooser(Facet category, bool forceDisable = false)
        {
            foreach (var chooser in Choosers)
            {
                chooser.OnFilterChanged(category);
            }
        }

        public ChooserButtonViewModel GetChooser(string category)
        {
            return Choosers.FirstOrDefault(c => c.CategoryName == category);
        }

        private void OnSearchStarting(object sender, SearchStartingEventArgs e)
        {
            e.Filters.ItemAdded += OnSearchFilterListItemAdded;
            e.Filters.ItemRemoved += OnSearchFilterListItemRemoved;
        }

        private void OnSearchFilterListItemAdded(object sender, Facet category)
        {
            UpdateChooser(category);
        }

        private void OnSearchFilterListItemRemoved(object sender, Facet category)
        {
            UpdateChooser(category);
        }

        private void OnChooserTap(object sender, EventArgs e)
        {
            var chooser = sender as ChooserButtonViewModel;
            if (chooser.Frequency > 0)
            {
                SelectedFilterCategory = chooser.CategoryName;
                VisualState = FilterChooserStates.CHOSEN;
            }
        }

        protected override void OnVisualStateChanged(string oldVisualState, string newVisualState)
        {
            base.OnVisualStateChanged(oldVisualState, newVisualState);
            var isClosed = newVisualState == FilterChooserStates.CLOSED;
            var isOpened = newVisualState == FilterChooserStates.OPENED;
            var isChosen = newVisualState == FilterChooserStates.CHOSEN;
            //chooser view
            Pointing.IsEnabled = isClosed;
            //chooser button
            foreach (var chButton in Choosers)
            {
                chButton.Pointing.IsEnabled = isOpened;
            }
            //chosen
            if (isChosen)
            {
                //discard
            }
        }

        private void OnTap(object sender, EventArgs e)
        {
            VisualState = FilterChooserStates.OPENED;
        }

        #region Department

        private ChooserButtonViewModel mDepartmentChooser;

        public ChooserButtonViewModel DepartmentChooser
        {
            get
            {
                if (mDepartmentChooser == null)
                {
                    var filterCat = "navSub_orange";
                    if(ShelfhubSearch.PROFILE_ACTIVE.Service == ShelfhubSearch.PROFILE_SWISSBIB_ZUERICH)
                    {
                        filterCat = "classif_ddc";
                    }
                    mDepartmentChooser = new ChooserButtonViewModel(filterCat,
                        new ViewStyle("ChooserButtonWithCountViewStyle"));
                    mDepartmentChooser.Name = "Thema";
                    mDepartmentChooser.TapBehaviour.Tap += OnChooserTap;
                }

                return mDepartmentChooser;
            }
        }

        #endregion Department

        #region MediaChooser

        private ChooserButtonViewModel mMediaChooser;

        public ChooserButtonViewModel MediaChooser
        {
            get
            {
                if (mMediaChooser == null)
                {
                    mMediaChooser = new ChooserButtonViewModel("format",
                        new ViewStyle("ChooserButtonWithCountViewStyle"));
                    mMediaChooser.TapBehaviour.Tap += OnChooserTap;
                }
                return mMediaChooser;
            }
        }

        #endregion MediaChooser

        #region LanguageChooser

        private ChooserButtonViewModel mLanguageChooser;

        public ChooserButtonViewModel LanguageChooser
        {
            get
            {
                if (mLanguageChooser == null)
                {
                    mLanguageChooser = new ChooserButtonViewModel("language",
                        new ViewStyle("ChooserButtonWithCountViewStyle"));
                    mLanguageChooser.TapBehaviour.Tap += OnChooserTap;
                }

                return mLanguageChooser;
            }
        }

        #endregion LanguageChooser

        #region DateChooser

        private ChooserButtonViewModel mDateChooser;

        public ChooserButtonViewModel DateChooser
        {
            get
            {
                if (mDateChooser == null)
                {
                    mDateChooser = new ChooserButtonViewModel("publishDate",
                        new ViewStyle("ChooserButtonWithCountViewStyle"));
                    mDateChooser.TapBehaviour.Tap += OnChooserTap;
                }
                return mDateChooser;
            }
        }

        #endregion DateChooser

        #region OnlyAvailableChooser

        private ChooserButtonViewModel mOnlyAvailableChooser;

        public ChooserButtonViewModel OnlyAvailableChooser
        {
            get
            {
                if (mOnlyAvailableChooser == null)
                {
                    mOnlyAvailableChooser = new ChooserButtonViewModel("available",
                        new ViewStyle("ChooserButtonViewStyle"));
                    mOnlyAvailableChooser.TapBehaviour.Tap += OnChooserTap;
                }

                return mOnlyAvailableChooser;
            }
        }

        #endregion OnlyAvailableChooser

        #region OnlyDigitalChooser

        private ChooserButtonViewModel mOnlyDigitalChooser;

        public ChooserButtonViewModel OnlyDigitalChooser
        {
            get
            {
                if (mOnlyDigitalChooser == null)
                {
                    mOnlyDigitalChooser = new DigitalChooserButtonVM(new ViewStyle("ChooserButtonViewStyle"));
                    mOnlyDigitalChooser.TapBehaviour.Tap += OnChooserTap;
                }

                return mOnlyDigitalChooser;
            }
        }

        #endregion OnlyDigitalChooser

        #region Choosers

        private PiciObservableCollection<ChooserButtonViewModel> mChoosers;

        public PiciObservableCollection<ChooserButtonViewModel> Choosers
        {
            get
            {
                if (mChoosers == null)
                {
                    mChoosers = new PiciObservableCollection<ChooserButtonViewModel>();
                    mChoosers.Add(DepartmentChooser);
                    mChoosers.Add(MediaChooser);
                    mChoosers.Add(LanguageChooser);
                    mChoosers.Add(OnlyAvailableChooser);
                    mChoosers.Add(OnlyDigitalChooser);
                    mChoosers.Add(DateChooser);
                }
                return mChoosers;
            }
        }

        #endregion Choosers

        #region SelectedFilterCategory

        private string mSelectedFilterCategory;

        public string SelectedFilterCategory
        {
            get { return mSelectedFilterCategory; }
            set
            {
                if (mSelectedFilterCategory != value)
                {
                    var old = mSelectedFilterCategory;
                    mSelectedFilterCategory = value;
                    RaisePropertyChanged("SelectedFilterCategory", old, value);
                }
            }
        }

        #endregion SelectedFilterCategory
    }


    public class FilterChooserStates
    {
        public static readonly string CLOSED = "Closed";
        public static readonly string OPENED = "Opened";
        public static readonly string CHOSEN = "Chosen";
        public static readonly string DISCARDED = "Discarded";
    }
}