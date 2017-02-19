// Search.cs
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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using picibird.hbs.ldu;
using picibird.hbs.ldu.pages;
using picibird.hbs.resources;
using picibird.hbs.viewmodels;
using picibird.shelfhub;
using picibits.core;
using picibits.core.extension;
using picibits.core.mvvm;
using picibits.core.util;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace picibird.hbs
{
    public class Search : Model
    {
        protected DateTime SearchEndTime;

        protected DateTime SearchStartTime;

        public Search()
        {
            Pages = new Pages();
        }

        #region SearchString

        public string SearchText { get; protected set; }

        #endregion SearchString

        protected CancellationTokenSource cts { get; set; }

        public SearchSession Session { get; protected set; }
        public Pages Pages { get; }

        public event EventHandler<SearchStartingEventArgs> SearchStarting;
        public event SimpleEventHandler<object> SearchFinished;

        protected void OnSearchResultsListUpdated(object sender)
        {
        }

        public virtual async Task Start(string searchText)
        {
            //abort if search text is the same as last one
            //if (searchText.Equals(SearchText))
            //{
            //    Pici.Log.info(typeof(Search), String.Format("aborting search since text {0} is equal to last", searchText));
            //    return false;
            //}

            SearchText = searchText;

            if (cts != null)
                cts.Cancel();
            cts = new CancellationTokenSource();
            //create search references before OnSearchStarting
            ResultCount = 0;
            Callback = new SearchCallback<SearchStatus>(cts.Token);
            Callback.ResultCountChanged += OnSearchCallbackResultCountChanged;

            FilterList = Callback.FilterList;

            Callback.StatusChanged += OnSearchStatusChanged;

            SearchRequest = new SearchRequest(searchText);
            SearchRequest.ItemsPerPage = PageItemsCount;
            SearchRequest.SortingChanged += OnSearchRequestSortOrderChanged;
            SearchRequest.FilterListChanged += OnSearchRequestFilterChanged;

            Session = new SearchSession();
            Pages.Session = Session;

            OnSearchStarting(SearchStartingReason.NewSearch, searchText, FilterList);
            try
            {
                await Session.Start(SearchRequest, Callback);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Pici.Log.debug(typeof(HBSViewModel), "Search Request cancelled!");
                }
                else if (ex is HttpRequestException)
                {
                    Pici.Log.error(typeof(SearchSession),
                        "\r\nA Web Exception occured! Internet available? Server down?\r\n\r\n", ex);
                    //MessageBox.Show("Service is not available!\r\nInternet active? Server down?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Pici.Log.error(typeof(SearchSession), "An unexpected error occured!", ex);
                }
            }
        }



        public void StartFake(string text, List<Hit> hits)
        {
            SearchText = "";

            if (cts != null)
                cts.Cancel();

            cts = new CancellationTokenSource();
            Callback = new SearchCallback<SearchStatus>(cts.Token);
            Callback.ResultCountChanged += OnSearchCallbackResultCountChanged;
            FilterList = Callback.FilterList;

            SearchRequest = new SearchRequest(text);
            SearchRequest.ItemsPerPage = PageItemsCount;

            Session = new SearchSession();
            Pages.Session = Session;
            OnSearchStarting(SearchStartingReason.NewSearch, text, FilterList);
            Session.Start(hits, SearchRequest, Callback);
        }

        protected void OnSearchRequestSortOrderChanged(object sender, PropertyChangedEventArgs e)
        {
            OnSearchStarting(SearchStartingReason.SortChanged, SearchText, FilterList);
        }

        protected void OnSearchRequestFilterChanged(object sender, PropertyChangedEventArgs e)
        {
            OnSearchStarting(SearchStartingReason.FiltersUpdated, SearchText, FilterList);
        }

        protected void OnSearchStarting(SearchStartingReason reason, string searchText, FilterList<FilterCategory> filters)
        {
            if (SearchStarting != null)
                SearchStarting(this, new SearchStartingEventArgs(reason, searchText, filters));
            SearchStartTime = DateTime.Now;
            Pici.Log.warn(typeof(Search), "SEARCH STARTING");
        }

        protected void OnIsSearchingChanged(bool isSearching)
        {
            if (isSearching)
            {
            }
            else
            {
                SearchEndTime = DateTime.Now;
                Duration = (SearchEndTime - SearchStartTime).TotalSeconds;
                Pici.Log.warn(typeof(Search), "SEARCH COMPLETED");
                if (SearchFinished != null)
                    SearchFinished(null);
            }
        }


        protected void OnSearchCallbackResultCountChanged(object sender, PropertyChangedEventArgs e)
        {
            ResultCount = Callback.ResultCount;
            Duration = (DateTime.Now - SearchStartTime).TotalSeconds;
        }

        protected void OnSearchStatusChanged(object sender, SearchStatus e)
        {
            Progress = e.progress;
            IsSearching = Progress < 1;
        }

        #region FilterList

        protected FilterList<FilterCategory> mFilterList;

        public FilterList<FilterCategory> FilterList
        {
            get { return mFilterList; }
            set
            {
                if (mFilterList != value)
                {
                    var old = mFilterList;
                    mFilterList = value;
                    OnFilterListChanged(old, value);
                }
            }
        }

        protected virtual void OnFilterListChanged(FilterList<FilterCategory> oldFilterList,
            FilterList<FilterCategory> newFilterList)
        {
            RaisePropertyChanged("FilterList", oldFilterList, newFilterList);
            if (oldFilterList != null)
            {
                oldFilterList.ListUpdated -= OnSearchFilterListListUpdated;
            }
            if (newFilterList != null)
            {
                newFilterList.ListUpdated += OnSearchFilterListListUpdated;
            }
        }

        protected void OnSearchFilterListListUpdated(object sender)
        {
        }

        #endregion FilterList

        #region SearchRequest

        protected SearchRequest mSearchRequest;

        public SearchRequest SearchRequest
        {
            get { return mSearchRequest; }
            set
            {
                if (mSearchRequest != value)
                {
                    var old = mSearchRequest;
                    mSearchRequest = value;
                    OnSearchRequestChanged(old, value);
                }
            }
        }

        protected virtual void OnSearchRequestChanged(SearchRequest oldSearchRequest, SearchRequest newSearchRequest)
        {
            RaisePropertyChanged("SearchRequest", oldSearchRequest, newSearchRequest);
            if (oldSearchRequest != null)
            {
                //manage old View here
            }
            if (newSearchRequest != null)
            {
                //manage new View here
            }
        }

        #endregion SearchRequest

        #region PageItemsCount

        protected int mPageItemsCount;

        public int PageItemsCount
        {
            get { return mPageItemsCount; }
            set
            {
                if (mPageItemsCount != value)
                {
                    var old = mPageItemsCount;
                    mPageItemsCount = value;
                    OnPageItemsCountChanged(old, value);
                }
            }
        }

        protected virtual void OnPageItemsCountChanged(int oldPageItemsCount, int newPageItemsCount)
        {
            RaisePropertyChanged("PageItemsCount", oldPageItemsCount, newPageItemsCount);
        }

        #endregion PageItemsCount

        #region Progress

        protected double mProgress;

        public double Progress
        {
            get { return mProgress; }
            set
            {
                if (mProgress != value)
                {
                    var old = mProgress;
                    mProgress = value;
                    RaisePropertyChanged("Progress", old, value);
                }
            }
        }

        #endregion Progress

        #region IsSearching

        protected bool mIsSearching;

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
                    OnIsSearchingChanged(value);
                }
            }
        }

        #endregion IsSearching

        #region ResultCount

        protected int mResultCount;

        public int ResultCount
        {
            get { return mResultCount; }
            set
            {
                if (mResultCount != value)
                {
                    var old = mResultCount;
                    mResultCount = value;
                    RaisePropertyChanged("ResultCount", old, value);
                }
            }
        }

        #endregion ResultCount

        #region SearchDuration

        protected double mSearchDuration;

        public double Duration
        {
            get { return mSearchDuration; }
            set
            {
                if (mSearchDuration != value)
                {
                    var old = mSearchDuration;
                    mSearchDuration = value;
                    RaisePropertyChanged("SearchDuration", old, value);
                }
            }
        }

        #endregion SearchDuration

        #region Callback

        protected SearchCallback<SearchStatus> mCallback;

        public SearchCallback<SearchStatus> Callback
        {
            get { return mCallback; }
            set
            {
                if (mCallback != value)
                {
                    var old = mCallback;
                    mCallback = value;
                    RaisePropertyChanged("Callback", old, value);
                }
            }
        }

        #endregion Callback
    }

    public class SearchStartingEventArgs
    {
        public SearchStartingEventArgs(SearchStartingReason reason, string searchText,
            FilterList<FilterCategory> filters)
        {
            Reason = reason;
            SearchText = searchText;
            Filters = filters;
        }

        public SearchStartingReason Reason { get; protected set; }

        public string SearchText { get; protected set; }
        public FilterList<FilterCategory> Filters { get; protected set; }
    }

    public enum SearchStartingReason
    {
        NewSearch,
        FiltersUpdated,
        SortChanged,
        Fake
    }
}