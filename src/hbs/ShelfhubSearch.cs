using System;
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

namespace picibird.hbs
{
    public class ShelfhubSearch : Search, IShelfhubSearch
    {

        public static string SHELFHUB_SERVER_URI_OVERRIDE;

        private SynchronizationContext syncContext { get; set; }

        private readonly ConcurrentCache<int, ItemList<Hit>> PAGE_HITS_CACHE;

        public QueryParams QueryParams { get; protected set; }

        public ShelfhubSearch()
        {
            syncContext = SynchronizationContext.Current;
            ShelfhubHelper.Search = this;
            PAGE_HITS_CACHE = new ConcurrentCache<int, ItemList<Hit>>(async key => await LoadPageHitsAsync(key));
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
            Shelfhub shelfhub = createShelfhubClient();
            var queryParams = new QueryParams()
            {
                Query = QueryParams.Query,
                Offset = index * 17,
                Filters = activeFilters?.ToObservableCollection(),
                Limit = 17,
                Shelfhub = QueryParams.Shelfhub
            };
            var response = await shelfhub.QueryAsync(queryParams);
            var items = response.Items;
            var hits = items.ToHits(syncContext);
            //request covers
            RequestCovers(items, hits);
            return hits;
        }


        private const string SWISSBIB_BASEL = "swissbib.basel";
        private const string SWISSBIB_ZUERICH = "swissbib.zuerich";
        private const string SWISSBIB_STGALLEN = "swissbib.stgallen";

        public static readonly ShelfhubParams PROFILE_ACTIVE = new ShelfhubParams()
        {
            Service = SWISSBIB_STGALLEN
        };

        public override async Task Start(string searchText, SearchStartingReason reason = SearchStartingReason.NewSearch)
        {
            try
            {
                BeforeShelfhubSearch(searchText, reason);
                PAGE_HITS_CACHE.Clear();
                Shelfhub shelfhub = createShelfhubClient();
                QueryParams = new QueryParams()
                {
                    Query = searchText,
                    Filters = activeFilters?.ToObservableCollection(),
                    Offset = 0,
                    Limit = 34,
                    Shelfhub = PROFILE_ACTIVE
                };
                var queryResult = await shelfhub.QueryAsync(QueryParams);
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
            //fill first two pages
            for (int i = 0; i < 2; i++)
            {

                int skip = i * 17;
                int take = (i + 1) * 17;
                var pageItems = items.Skip(skip).Take(take).ToList();
                var pageHits = new ItemList<Hit>(syncContext);
                foreach (Hit hit in hits.Skip(skip).Take(take))
                {
                    pageHits.Add(hit);
                }
                PAGE_HITS_CACHE.AddOrUpdate(i, pageHits);
                //request covers
                RequestCovers(pageItems, pageHits);
            }
            //apply facets
            foreach (var facet in facets)
            {
                FilterList.Add(facet);
            }

            //finalize session
            Session.Status = new SearchStatus()
            {
                progress = 1.0,
                hits = hits.Count
            };
            Callback.ResultCount = (int)response.ItemsFound;
            double maxPageIndex= Math.Ceiling(response.ItemsFound / 17.0d) - 1.0d;
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
                        var coverParams = new CoverParams()
                        {
                            Ids = new ObservableCollection<CoverId>(coverIds),
                            PageItemCount = 34
                        };
                        Shelfhub shelfhub = createShelfhubClient();
                        var coverResponse = await shelfhub.GetCoversAsync(coverParams);
                        var covers = coverResponse.Covers;
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

                }
                catch (Exception ex)
                {
                    Pici.Log.info(typeof(Search), ex.Message);
                }
            });
        }

        public static Shelfhub createShelfhubClient()
        {
            var shelfhub = new Shelfhub();
#if DEBUG
            //shelfhub.BaseUrl = @"http://localhost:8080/api";
            //shelfhub.BaseUrl = @"http://dev.shelfhub.io/api";
#endif
            if (SHELFHUB_SERVER_URI_OVERRIDE != null)
                shelfhub.BaseUrl = SHELFHUB_SERVER_URI_OVERRIDE;
            return shelfhub;
        }

    }
}
