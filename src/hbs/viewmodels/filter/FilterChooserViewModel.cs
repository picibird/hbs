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
using picibits.core;

namespace picibird.hbs.viewmodels.filter
{
    public class FilterChooserViewModel : ButtonViewModel
    {
        public FilterChooserViewModel()
        {
            Style = new ViewStyle("FilterChooserViewStyle");

            TapBehaviour.Tap += OnTap;
            VisualState = FilterChooserStates.CLOSED;
            
            if (HBS.Search.FilterList != null)
            {
                HBS.Search.FilterList.ItemAdded += OnSearchFilterListItemAdded;
                HBS.Search.FilterList.ItemRemoved += OnSearchFilterListItemRemoved;
                HBS.Search.FilterList.RemovedAll += OnFilterListRemovedAll;
            }
            HBS.Search.PropertyChanged += OnFilterListChanged;
            UpdateChoosers();
        }

        public override void DisposeManaged()
        {
            foreach (var chooser in Choosers)
            {
                chooser.Dispose();
            }
            if (HBS.Search.FilterList != null)
            {
                HBS.Search.FilterList.ItemAdded -= OnSearchFilterListItemAdded;
                HBS.Search.FilterList.ItemRemoved -= OnSearchFilterListItemRemoved;
                HBS.Search.FilterList.RemovedAll -= OnFilterListRemovedAll;
            }
            HBS.Search.PropertyChanged -= OnFilterListChanged;
            base.DisposeManaged();
        }

        private void OnFilterListChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "FilterList")
            {
                var ev = e as PropertyChangedEventArgs;
                var oldFilterList = ev.Old as FilterList<Facet>;
                if (oldFilterList != null)
                {
                    oldFilterList.ItemAdded -= OnSearchFilterListItemAdded;
                    oldFilterList.ItemRemoved -= OnSearchFilterListItemRemoved;
                    oldFilterList.RemovedAll -= OnFilterListRemovedAll;
                }
                var newFilterList = ev.New as FilterList<Facet>;
                if (newFilterList != null)
                {
                    newFilterList.ItemAdded += OnSearchFilterListItemAdded;
                    newFilterList.ItemRemoved += OnSearchFilterListItemRemoved;
                    newFilterList.RemovedAll += OnFilterListRemovedAll;  
                }
            }
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

        private void OnSearchFilterListItemAdded(object sender, Facet category)
        {
            UpdateChooser(category);
        }

        private void OnSearchFilterListItemRemoved(object sender, Facet category)
        {
            category.Values = null;
            UpdateChooser(category);
        }

        private void OnFilterListRemovedAll(PiciObservableCollection<Facet> sender)
        {
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

        private string CHOOSER_VIEW_STYLE = "ChooserButtonViewStyle";

        #region Department

        private ChooserButtonViewModel mDepartmentChooser;

        public ChooserButtonViewModel DepartmentChooser
        {
            get
            {
                if (mDepartmentChooser == null)
                {
                    var filterCat = "navSub_orange";
                    if (ShelfhubSearch.PROFILE_ACTIVE.Service.Contains("swissbib.zuerich"))
                    {
                        filterCat = "classif_ddc_3";
                    }
                    if (ShelfhubSearch.PROFILE_ACTIVE.Service.Contains("swissbib.stgallen"))
                    {
                        filterCat = "classif_rvk";
                    }
                    var name  = Pici.Resources.Find(filterCat);
                    mDepartmentChooser = new ChooserButtonViewModel(filterCat,
                        new ViewStyle(CHOOSER_VIEW_STYLE));
                    mDepartmentChooser.Name = name;
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
                        new ViewStyle(CHOOSER_VIEW_STYLE));
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
                        new ViewStyle(CHOOSER_VIEW_STYLE));
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
                        new ViewStyle(CHOOSER_VIEW_STYLE));
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