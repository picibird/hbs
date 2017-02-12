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
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace picibird.hbs
{
    public class Search : Model
    {
        private DateTime SearchEndTime;

        private DateTime SearchStartTime;

        public Search()
        {
            Pages = new Pages();
        }

        #region SearchString

        public string SearchText { get; private set; }

        #endregion SearchString

        private CancellationTokenSource cts { get; set; }

        public SearchSession Session { get; private set; }
        public Pages Pages { get; }

        public event EventHandler<SearchStartingEventArgs> SearchStarting;

        private void OnSearchResultsListUpdated(object sender)
        {
        }

        public async Task<bool> Start(string searchText)
        {
            await StartShelfhubSearch(searchText);
            return true;

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
                return false;
            }
            return true;
        }

        public async Task StartShelfhubSearch(string text)
        {
            try
            {
                BeforeShelfhubSearch(text);
                Shelfhub shelfhub = new Shelfhub();
#if DEBUG
                shelfhub.BaseUrl = @"http://localhost:8080/api";
#endif
                var queryResult = await shelfhub.QueryAsync(new QueryParams()
                {
                    Query = text,
                    Offset = 0,
                    Limit = 34,
                    Shelfhub = new ShelfhubParams()
                    {
                        Service = "ch.swissbib.solr.basel"
                    }
                });
                AfterShelfhubSearch(queryResult.Items, queryResult.Items.ToHits());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private void BeforeShelfhubSearch(string text)
        {
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
        }

        private readonly Shelfhub _shelfhub = new Shelfhub()
        {
#if DEBUG
            BaseUrl = @"http://localhost:8080/api"
#endif
        };

        private void AfterShelfhubSearch(IList<ShelfhubItem> items, List<Hit> hits)
        {
            Session.Start(hits, SearchRequest, Callback);
            var isbns = from m in items
                        where m.Isbn != null && m.Isbn.Count > 0
                        select m.Isbn[0];
            _shelfhub.CoverAsync(new CoverParams()
            {
                Ids = new ObservableCollection<string>(isbns),
                IdType = CoverParamsIdType.ISBN,
                PageItemCount = 17
            }).ContinueWithCurrentContext((t) =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    var covers = t.Result.Covers;
                    foreach (Cover c in covers)
                    {
                        var hit = hits[c.Index];
                        hit.CoverIsbn = c.Id;
                        hit.CoverImageUrl = c.ImageLarge;
                    }
                }
                else
                {
                    var ex = t.Exception;
                }
            });
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

        private void OnSearchRequestSortOrderChanged(object sender, PropertyChangedEventArgs e)
        {
            OnSearchStarting(SearchStartingReason.SortChanged, SearchText, FilterList);
        }

        private void OnSearchRequestFilterChanged(object sender, PropertyChangedEventArgs e)
        {
            OnSearchStarting(SearchStartingReason.FiltersUpdated, SearchText, FilterList);
        }

        private void OnSearchStarting(SearchStartingReason reason, string searchText, FilterList<FilterCategory> filters)
        {
            if (SearchStarting != null)
                SearchStarting(this, new SearchStartingEventArgs(reason, searchText, filters));
            SearchStartTime = DateTime.Now;
            Pici.Log.warn(typeof(Search), "SEARCH STARTING");
        }

        private void OnIsSearchingChanged(bool isSearching)
        {
            if (isSearching)
            {
            }
            else
            {
                SearchEndTime = DateTime.Now;
                Duration = (SearchEndTime - SearchStartTime).TotalSeconds;
                Pici.Log.warn(typeof(Search), "SEARCH COMPLETED");
            }
        }


        private void OnSearchCallbackResultCountChanged(object sender, PropertyChangedEventArgs e)
        {
            ResultCount = Callback.ResultCount;
            Duration = (DateTime.Now - SearchStartTime).TotalSeconds;
        }

        private void OnSearchStatusChanged(object sender, SearchStatus e)
        {
            Progress = e.progress;
            IsSearching = Progress < 1;
        }

        #region FilterList

        private FilterList<FilterCategory> mFilterList;

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

        private void OnSearchFilterListListUpdated(object sender)
        {
        }

        #endregion FilterList

        #region SearchRequest

        private SearchRequest mSearchRequest;

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

        private int mPageItemsCount;

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

        private double mProgress;

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
                    OnIsSearchingChanged(value);
                }
            }
        }

        #endregion IsSearching

        #region ResultCount

        private int mResultCount;

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

        private double mSearchDuration;

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

        private SearchCallback<SearchStatus> mCallback;

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

        public SearchStartingReason Reason { get; private set; }

        public string SearchText { get; private set; }
        public FilterList<FilterCategory> Filters { get; private set; }
    }

    public enum SearchStartingReason
    {
        NewSearch,
        FiltersUpdated,
        SortChanged,
        Fake
    }

    public static class LduToShelfhubExtensions
    {
        public static Hit ToHit(this ShelfhubItem m)
        {

            Hit hit = new Hit()
            {
                recid = m.Id,
                title = m.Title,
                title_remainder = m.Subtitle,
                author = m.Authors?.ToList(),
                language = new string[] { m.Language }.ToList(),
            };
            if (m.Isbn != null)
                hit.ISBNs = String.Join("\n", m.Isbn);
            else
            {
                hit.ISBNs = String.Empty;   
            }
            return hit;
        }

        public static List<Hit> ToHits(this IList<ShelfhubItem> items)
        {
            var hits = new List<Hit>(items.Count);
            foreach (ShelfhubItem item in items)
            {
                hits.Add(item.ToHit());
            }
            return hits;
        }

    }
}