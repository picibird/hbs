// HBSViewModel.cs
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
using System.Threading.Tasks;
using picibird.hbs.Intents;
using picibird.hbs.viewmodels.buttons;
using picibird.hbs.viewmodels.filter;
using picibird.hbs.viewmodels.search;
using picibird.hbs.viewmodels.shelf;
using picibird.hbs.viewmodels.osk;

using picibits.core;
using picibits.core.export.services;
using picibits.core.helper;
using picibits.core.intent;
using picibits.core.mvvm;
using picibits.core.pointer;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace picibird.hbs.viewmodels
{
    public class HBSViewModel : ViewModel
    {
        private bool mIsSearchEnabled = true;
        public bool IsSearchEnabled
        {
            get
            {
                return mIsSearchEnabled;
            }
            set
            {
                var old = mIsSearchEnabled;
                mIsSearchEnabled = value;
                SearchBoxTextViewModel.IsEnabled = value;
                SearchButtonViewModel.IsEnabled = value;
                RaisePropertyChanged(nameof(IsSearchEnabled), old, value);
            }
        }

        private bool mIsFilteringEnabled = true;
        public bool IsFilteringEnabled
        {
            get
            {
                return mIsFilteringEnabled;
            }
            set
            {
                mIsFilteringEnabled = value;
                Filters.IsEnabled = value;
            }
        }

        public HBSViewModel()
            : base("HBSRoot")
        {
            HBS.ViewModel = this;
            HBS.Search.SearchStarting += OnSearchStarting;
            HBS.Search.PropertyChanged += OnSearchPropertyChanged;

            LastPointerDownTime = DateTime.Now;
            PointerMode = PointerModeOptions.GLOBAL_SYSTEM;
            Pointing.IsEnabled = true;
            Pointing.Event += OnPointingEvent;

            Style = new ViewStyle("HBSRootStyle");
            SearchBoxTextViewModel.EnterSearchCommand.OnEnter += OnSearchEnter;
            SearchButtonViewModel.TapBehaviour.Tap += OnSearchButtonTap;
            SearchButtonViewModel.Clicked += OnSearchButtonClicked;

            HBS.Search.PageItemsCount = HBS.RowCount*HBS.ColumnCount - 1;
            var init = ShelfViewModel.IsRotating;

            BlackBlendingViewModel.TapBehaviour.Tap += OnBlackBlendingTap;
        }

        public PointerType LastPointerDownType { get; private set; }
        public DateTime LastPointerDownTime { get; private set; }

        private void OnPointingEvent(object sender, PointerEventArgs e)
        {
            if (e.Type == PointerEventType.DOWN)
            {
                var dt = DateTime.Now;
                dt.AddMilliseconds(e.TimeStamp - Environment.TickCount);
                LastPointerDownTime = dt;
                LastPointerDownType = e.Pointer.Type;
            }
        }

        private void OnSearchEnter(EnterSearchCommand sender)
        {
            //manual focus clear on enter to deselect textbox and trigger windows soft keyboard
            Events.OnIdleOnce(() => { Pici.Services.Get<IKeyboard>().ClearFocus(); });
            OnSearch();
        }

        private void OnSearchButtonTap(object sender, EventArgs args)
        {
            //OnSearch();
        }

        private void OnSearchButtonClicked(object sender, EventArgs e)
        {
            OnSearch();
        }

        public void OnSearch()
        {
            OnSearch(SearchBoxTextViewModel.SearchText);
        }

        public virtual void OnSearch(string searchText)
        {
            SearchBoxTextViewModel.SearchText = searchText;
            Pici.Log.debug(typeof(HBSViewModel), string.Format("searching for {0}", searchText));
            Task t = HBS.Search.Start(searchText);
        }

        private void OnSearchStarting(object sender, SearchStartingEventArgs args)
        {
            SearchInfo = Pici.Resources.Find("searching");
        }


        private void OnSearchPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var showTime = false;
            if (e.PropertyName.Equals("ResultCount") || e.PropertyName.Equals("SearchDuration"))
            {
                if (showTime)
                {
                    if (HBS.Search.Duration > 0)
                    {
                        SearchInfo = string.Format(Pici.Resources.Find("found_results_seconds"), HBS.Search.ResultCount,
                            HBS.Search.Duration);
                    }
                }
                else
                {
                    if (HBS.Search.Duration >= 1 && HBS.Search.ResultCount == 0)
                    {
                        SearchInfo = Pici.Resources.Find("found_no_results");
                    }
                    else
                    {
                        SearchInfo = string.Format(Pici.Resources.Find("found_results"), HBS.Search.ResultCount);
                    }
                }
            }
            if (e.PropertyName.Equals("IsSearching"))
            {
                IsSearching = HBS.Search.IsSearching;
            }
        }

        private void OnBlackBlendingTap(object sender, EventArgs args)
        {
            Pici.Intent.Send(new Intent(HbsIntents.ACTION_CLOSE_BOOK));
        }

        #region TitleButton

        private TitleButtonVM mTitleButton;

        public TitleButtonVM TitleButton
        {
            get
            {
                if (mTitleButton == null)
                    mTitleButton = new TitleButtonVM(32d);
                return mTitleButton;
            }
        }

        #endregion TitleButton


        #region SubTitleButton

        private TitleButtonVM mSubTitleButton;

        public TitleButtonVM SubTitleButton
        {
            get
            {
                if (mSubTitleButton == null)
                    mSubTitleButton = new TitleButtonVM(24d);
                return mSubTitleButton;
            }
        }

        #endregion SubTitleButton

        #region SearchBoxTextViewModel

        private SearchTextViewModel mSearchBoxTextViewModel;

        public SearchTextViewModel SearchBoxTextViewModel
        {
            get
            {
                if (mSearchBoxTextViewModel == null)
                    mSearchBoxTextViewModel = new SearchTextViewModel();
                return mSearchBoxTextViewModel;
            }
        }

        #endregion SearchBoxTextViewModel

        #region SearchButtonViewModel

        private SearchButtonViewModel mSearchButtonViewModel;

        public SearchButtonViewModel SearchButtonViewModel
        {
            get
            {
                if (mSearchButtonViewModel == null)
                {
                    mSearchButtonViewModel = new SearchButtonViewModel();
                    mSearchButtonViewModel.Style = new ViewStyle("SearchButtonStyle");
                }
                return mSearchButtonViewModel;
            }
        }

        #endregion SearchButtonViewModel


        #region SearchButtonViewModel

        private LanguageButtonViewModel mLanguageButtonViewModel;

        public LanguageButtonViewModel LanguageButtonViewModel
        {
            get
            {
                if (mLanguageButtonViewModel == null)
                {
                    mLanguageButtonViewModel = new LanguageButtonViewModel();
                    mLanguageButtonViewModel.Style = new ViewStyle("LanguageButtonStyle");
                }
                return mLanguageButtonViewModel;
            }
        }

        #endregion SearchButtonViewModel

        #region OskButtonVM

        private OskButtonVM mOskButtonVM;

        public OskButtonVM OskButtonVM
        {
            get
            {
                if (mOskButtonVM == null)
                {
                    mOskButtonVM = new OskButtonVM();
                    mOskButtonVM.Style = new ViewStyle("OskButtonStyle");
                }
                return mOskButtonVM;
            }
        }

        #endregion OskButtonVM


        #region OskButtonVM

        private BackButtonVM mBackButtonVM;

        public BackButtonVM BackButtonVM
        {
            get
            {
                if (mBackButtonVM == null)
                {
                    mBackButtonVM = new BackButtonVM();
                }
                return mBackButtonVM;
            }
        }

        #endregion OskButtonVM

        #region BlackBlendingViewModel

        private BlackBlendingViewModel mBlackBlendingViewModel;

        public BlackBlendingViewModel BlackBlendingViewModel
        {
            get
            {
                if (mBlackBlendingViewModel == null)
                    mBlackBlendingViewModel = new BlackBlendingViewModel();
                return mBlackBlendingViewModel;
            }
        }

        #endregion BlackBlendingViewModel

        #region ShelfViewModel

        private ShelfViewModel mShelfViewModel;

        public ShelfViewModel ShelfViewModel
        {
            get
            {
                if (mShelfViewModel == null)
                {
                    mShelfViewModel = new ShelfViewModel();
                    mShelfViewModel.CreateShelf();
                }

                return mShelfViewModel;
            }
        }

        #endregion ShelfViewModel

        #region BookFlyout3dVM

        private BookFlyout3dVM mBookFlyout3dVM;

        public BookFlyout3dVM BookFlyout3dVM
        {
            get
            {
                if (mBookFlyout3dVM == null)
                    mBookFlyout3dVM = new BookFlyout3dVM();
                return mBookFlyout3dVM;
            }
        }

        #endregion BookFlyout3dVM

        #region SearchInfo

        private string mSearchInfo;

        public string SearchInfo
        {
            get { return mSearchInfo; }
            set
            {
                if (mSearchInfo != value)
                {
                    var old = mSearchInfo;
                    mSearchInfo = value;
                    RaisePropertyChanged("SearchInfo", old, value);
                }
            }
        }

        #endregion SearchInfo

        #region Filters

        private ActiveFilterListViewModel mFilters;

        public ActiveFilterListViewModel Filters
        {
            get
            {
                if (mFilters == null)
                    mFilters = new ActiveFilterListViewModel();
                return mFilters;
            }
        }

        #endregion Filters

        #region OpenedBookLayer

        private OpenedBookLayerViewModel mOpened;

        public OpenedBookLayerViewModel Opened
        {
            get
            {
                if (mOpened == null)
                    mOpened = new OpenedBookLayerViewModel();
                return mOpened;
            }
        }

        #endregion OpenedBookLayer

        #region IsSearching

        private bool mIsSearching;

        public bool IsSearching
        {
            get { return mIsSearching; }
            set
            {
                if (mIsSearching != value)
                {
                    var old = mIsSearching;
                    mIsSearching = value;
                    RaisePropertyChanged("IsSearching", old, value);
                }
            }
        }

        #endregion IsSearching
    }
}