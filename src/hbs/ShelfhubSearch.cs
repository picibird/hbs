﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using picibird.hbs.ldu;
using picibird.shelfhub;
using System.Threading;
using picibits.core;
using picibird.hbs.ldu.pages;
using picibits.core.helper;
using picibits.core.util;
using Filter = picibird.shelfhub.Filter;
using Nito.AsyncEx;

namespace picibird.hbs
{
    public class ShelfhubSearch : Search, IShelfhubSearch
    {

        public static string SHELFHUB_SERVER_URI_OVERRIDE;

        private SynchronizationContext syncContext { get; set; }

        private readonly ConcurrentCache<int, ItemList<Hit>> PAGE_HITS_CACHE;
        private readonly ConcurrentCache<CoverId[], List<Cover>> COVER_CACHE;

        public QueryParams QueryParams { get; protected set; }
        private SemaphoreSlim QueryLock = new SemaphoreSlim(1);
        private CancellationTokenSource QueryLockToken = new CancellationTokenSource();

        /*
         * PROFILES
         */

        public const string PROFILE_SWISSBIB_BASEL = "swissbib.basel";
        public const string PROFILE_SWISSBIB_ZUERICH = "swissbib.zuerich";
        public const string PROFILE_SWISSBIB_STGALLEN = "swissbib.stgallen";

        public static readonly ShelfhubParams PROFILE_ACTIVE = new ShelfhubParams() { Service = PROFILE_SWISSBIB_BASEL};

        public static picibird.shelfhub.Shelfhub createShelfhubClient()
        {
            var shelfhub = new picibird.shelfhub.Shelfhub();
            if (SHELFHUB_SERVER_URI_OVERRIDE != null)
                shelfhub.BaseUrl = SHELFHUB_SERVER_URI_OVERRIDE;
#if DEBUG
            //shelfhub.BaseUrl = @"http://localhost:8080/api";
#endif
            return shelfhub;
        }

        public ShelfhubSearch()
        {
            syncContext = SynchronizationContext.Current;
            ShelfhubHelper.Search = this;
            PAGE_HITS_CACHE = new ConcurrentCache<int, ItemList<Hit>>(async key => await LoadPageHitsAsync(key));
            COVER_CACHE = new ConcurrentCache<CoverId[], List<Cover>>(async key => await RequestCoversCached(key), new CoverIdComparer());

        }



        private List<Filter> activeFilters;

        protected override void OnSearchRequestFilterChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnSearchRequestFilterChanged(sender, e);
            activeFilters = (sender as SearchRequest).GetActiveFilter();
            Start(SearchText, SearchStartingReason.FiltersUpdated);
        }

        public Page LoadPage(int index)
        {
            Page page = new Page(index, syncContext);
            if (QueryParams != null)
            {
                PAGE_HITS_CACHE.GetAsync(index).ContinueWith((t) =>
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        var pageHits = t.Result;
                        page.UpdateList(pageHits);
                    }
                });
            }
            return page;
        }

        private async Task<ItemList<Hit>> LoadPageHitsAsync(int index)
        {
            QueryResponse response = null;
            try
            {
                await QueryLock.WaitAsync(QueryLockToken.Token);
                var shelfhub = createShelfhubClient();
                var queryParams = new QueryParams()
                {
                    Query = QueryParams.Query,
                    Offset = index * 17,
                    Filters = activeFilters?.ToObservableCollection(),
                    FiltersEnabled = false,
                    Limit = 17,
                    Shelfhub = QueryParams.Shelfhub,
                    Locale = Pici.Resources.CultureInfo.Name
                };
                response = await shelfhub.QueryAsync(queryParams);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                QueryLock.Release();
            }
            var items = response.Items;
            var hits = items.ToHits(syncContext);
            //request covers
            RequestCovers(items, hits);
            return hits;
        }

        public override async Task Start(string searchText, SearchStartingReason reason = SearchStartingReason.NewSearch)
        {

            try
            {
                QueryLockToken.Cancel();
                QueryLockToken = new CancellationTokenSource();
                PAGE_HITS_CACHE.Clear();

                BeforeShelfhubSearch(searchText, reason);

                var shelfhub = createShelfhubClient();
                QueryParams = new QueryParams()
                {
                    Query = searchText,
                    Filters = activeFilters?.ToObservableCollection(),
                    FiltersEnabled = false,
                    Offset = 0,
                    Limit = 17,
                    Shelfhub = PROFILE_ACTIVE,
                    Locale = Pici.Resources.CultureInfo.Name
                };

                QueryResponse queryResult = null;
                queryResult = await shelfhub.QueryAsync(QueryParams);

                await AfterShelfhubSearch(queryResult, reason);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void BeforeShelfhubSearch(string text, SearchStartingReason reason)
        {
            if (cts != null)
                cts.Cancel();

            cts = new CancellationTokenSource();
            Callback = new SearchCallback<SearchStatus>(cts.Token);
            Callback.ResultCountChanged += OnSearchCallbackResultCountChanged;
            Callback.StatusChanged += OnSearchStatusChanged;

            if (FilterList != null) FilterList.RemoveAll();
            FilterList = Callback.FilterList;

            SearchText = text;

            if (reason == SearchStartingReason.NewSearch)
            {
                SearchRequest = new SearchRequest(text);
                SearchRequest.ItemsPerPage = PageItemsCount;
                SearchRequest.FilterListChanged += OnSearchRequestFilterChanged;
            }

            Session = new ShelfhubSearchSession();
            Pages.Session = Session;

            OnSearchStarting(reason, text, FilterList);
            Session.Start(null, SearchRequest, Callback);
            Session.Status = new SearchStatus();
        }

        private async Task AfterShelfhubSearch(QueryResponse response, SearchStartingReason reason)
        {
            //convert shelfhubitems to hits and call Session
            var items = response.Items;
            var hits = items.ToHits();
            var facets = response.Facets;
            //fill first page
            PAGE_HITS_CACHE.AddOrUpdate(0, hits);
            //request covers
            RequestCovers(items, hits);
            //apply facets
            foreach (var facet in facets)
            {
                foreach (var fv in facet.Values)
                    fv.Name = Pici.Resources.Find(fv.Name);
                FilterList.Add(facet);
            }

            //finalize session
            Session.Status = new SearchStatus()
            {
                progress = 1.0,
                hits = hits.Count
            };
            Callback.ResultCount = (int)response.ItemsFound;
            double maxPageIndex = Math.Ceiling(response.ItemsFound / 17.0d) - 1.0d;
            maxPageIndex = Math.Max(maxPageIndex, 0);
            Callback.MaxPageIndex = (int)maxPageIndex;
        }



        public void RequestCovers(IList<ShelfhubItem> shelfhubItems, IList<Hit> hits)
        {

            Events.OnRenderOnce(async () =>
            {
                try
                {
                    //cover
                    var items = from m in shelfhubItems
                                where m.Isbn != null && m.Isbn.Count > 0
                                select new string[] { m.Id, m.Isbn[0] };
                    if (items.Any())
                    {
                        var coverIds = from i in items
                                       select new CoverId()
                                       {
                                           Id = i[1],
                                           ItemId = i[0],
                                           IdType = CoverIdIdType.ISBN
                                       };

                        var covers = await COVER_CACHE.GetAsync(coverIds.ToArray());

                        foreach (Cover c in covers)
                        {
                            var hit = hits.FirstOrDefault((h) => h.id == c.ItemId);
                            if (hit != null)
                            {
                                hit.CoverIsbn = c.Id;
                                hit.CoverImageUrl = c.ImageLarge;
                            }
                        }
                    }
                    ValidateCoverCacheSize();
                }
                catch (Exception ex)
                {
                    Pici.Log.info(typeof(Search), ex.Message);
                }
            });
        }

        public void ValidateCoverCacheSize()
        {
            bool ok = true;
            while (ok && COVER_CACHE.Count > 1000)
            {
                AsyncLazy<List<Cover>> removed;
                ok = COVER_CACHE.ConcurrentDictionary.TryRemove(COVER_CACHE.ConcurrentDictionary.Keys.First(), out removed);
            }
        }

        public async Task<List<Cover>> RequestCoversCached(CoverId[] coverIds)
        {
            var coverParams = new CoverParams()
            {
                Ids = new ObservableCollection<CoverId>(coverIds),
                PageItemCount = 34
            };
            var shelfhub = createShelfhubClient();
            var coverResponse = await shelfhub.GetCoversAsync(coverParams);
            return coverResponse.Covers.ToList();
        }

        

    }


    public class CoverIdComparer : IEqualityComparer<CoverId[]>
    {
        public bool Equals(CoverId[] c1, CoverId[] c2)
        {
            if (c1 == null || c2 == null)
                return false;
            var c1Str = string.Join("", c1.Select((c) => c.Id).ToArray<string>());
            var c2Str = string.Join("", c2.Select((c) => c.Id).ToArray<string>());
            if (c1Str.Equals(c2Str))
                return true;
            return false;
        }

        public int GetHashCode(CoverId[] cIds)
        {
            return string.Join("", cIds.Select((c) => c.Id).ToArray<string>()).GetHashCode();
        }
    }
}
