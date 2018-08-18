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
using Nito.AsyncEx;

namespace picibird.hbs
{
    public class ShelfhubSearch : Search, IShelfhubSearch
    {

#if DEBUG
        public static bool IS_DEBUG = true;
#else
        public static bool IS_DEBUG = false;
#endif
        public static bool USES_SHELFHUB_PRODUCTION_SERVER;
        public static string SHELFHUB_SERVER_URI_OVERRIDE;
        public static string SHELFHUB_PROFILE_OVERRIDE;

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
        public const string PROFILE_SWISSBIB_BASEL_NEW = "swissbib.basel.new";
        public const string PROFILE_SWISSBIB_ZUERICH = "swissbib.zuerich";
        public const string PROFILE_SWISSBIB_ZUERICH_NEW = "swissbib.zuerich.new";
        public const string PROFILE_SWISSBIB_STGALLEN = "swissbib.stgallen";
        public const string PROFILE_SWISSBIB_PHZH = "swissbib.phzh";
        public const string PROFILE_OCLC_PEPPERDINE = "oclc.pepperdine";
        public const string PROFILE_GVI_AALEN = "gvi.aalen";
        public const string PROFILE_GVI_KONSTANZ = "gvi.konstanz";
        public const string PROFILE_GVI_ULM = "gvi.ulm";

        public static ShelfhubParams PROFILE_ACTIVE
        {
            get
            {
                ShelfhubParams p = new ShelfhubParams() { Service = PROFILE_GVI_KONSTANZ };
                if (!String.IsNullOrEmpty(SHELFHUB_PROFILE_OVERRIDE))
                    p.Service = SHELFHUB_PROFILE_OVERRIDE;
                return p;
            }
        }

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
            QueryParams queryParams = null;
            try
            {
                await QueryLock.WaitAsync(QueryLockToken.Token);
                var shelfhub = createShelfhubClient();
                queryParams = new QueryParams()
                {
                    Query = QueryParams.Query,
                    Offset = index * 17,
                    Filters = activeFilters?.ToObservableCollection(),
                    FiltersEnabled = false,
                    Limit = 17,
                    Shelfhub = PROFILE_ACTIVE,
                    Locale = Pici.Resources.CultureInfo.Name
                };
                response = await shelfhub.QueryAsync(queryParams);
            }
            catch (Exception ex)
            {
                Pici.Log.error(typeof(ShelfhubSearch), "shelfhub request secondary search page failed", ex, queryParams);
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
#if !DEBUG
                GeneralSettings generalSettings = Pici.Settings.Get<GeneralSettings>();
                bool isProduction = !shelfhub.BaseUrl.Contains("dev") && !shelfhub.BaseUrl.Contains("localhost");
                if (searchText != generalSettings.StartupSearch && isProduction)
                {
                    QueryParams.Shelfhub.Tracking = true;
                }
#endif

                QueryResponse queryResult = null;
                queryResult = await shelfhub.QueryAsync(QueryParams);

                await AfterShelfhubSearch(queryResult, reason);
            }
            catch (Exception ex)
            {
                Pici.Log.error(typeof(ShelfhubSearch), "shelfhub search query failed", ex, QueryParams);
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
            if (facets != null)
            {
                foreach (var facet in facets)
                {
                    foreach (var fv in facet.Values)
                        fv.Name = Pici.Resources.Find(fv.Name);
                    FilterList.Add(facet);
                }
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

        private Hit FindHitWithIsbn(IList<Hit> hits, string isbn)
        {
            return hits.FirstOrDefault((h) =>
            {
                var shelfhubItem = ((ShelfhubItem)h.shelfhubItem);
                if (shelfhubItem.Isbn != null)
                {
                    bool isbnMatches = shelfhubItem.Isbn.Any(i => i == isbn);
                    if (isbnMatches)
                        return true;
                }
                if (shelfhubItem.IsbnRelated != null)
                {
                    bool isbnRelated = shelfhubItem.IsbnRelated.Any(i => i == isbn);
                    if (isbnRelated)
                        return true;
                }
                return false;
            });
        }

        public void RequestCovers(IList<ShelfhubItem> shelfhubItems, IList<Hit> hits)
        {

            Events.OnRenderOnce(async () =>
            {
                //set covers that are already available
                var coversAlreadyExisting = from m in shelfhubItems
                                            where m.Cover != null && m.Cover.ImageLarge != null && m.Cover.ImageLarge.Length > 0
                                            select m.Cover;
                foreach (Cover c in coversAlreadyExisting)
                {
                    var hit = FindHitWithIsbn(hits, c.Id);
                    if (hit != null)
                    {
                        hit.CoverIsbn = c.Id;
                        hit.CoverImageUrl = c.ImageLarge;
                    }
                }

                //set colors that are already available
                var colorsAlreadyExisting = from m in shelfhubItems
                                            where m.Colors != null
                                            select m.Colors;
                foreach (UIColors uicolors in colorsAlreadyExisting)
                {
                    var hit = FindHitWithIsbn(hits, uicolors.Id);
                    if (hit != null)
                    {
                        hit.CoverColorScheme = new picibits.bib.HistomatColorScheme()
                        {
                            Primary = uicolors.Background,
                            Secondary = uicolors.Foreground,
                            Light = uicolors.Light,
                            Dark = uicolors.Dark
                        };
                    }
                }

                CoverId[] coverIdsArray = null;
                try
                {
                    //cover
                    var items = from m in shelfhubItems
                                where m.Cover == null && m.Isbn != null && m.Isbn.Count > 0
                                select m;
                    var itemsArray = items.ToArray();

                    List<Cover> covers = null;

                    if (itemsArray.Any())
                    {
                        var coverIds = from i in itemsArray
                                       select new CoverId()
                                       {
                                           Id = i.Isbn[0],
                                           ItemId = i.Id,
                                           RelatedIds = i.Isbn.Skip(1).Concat(i.IsbnRelated ?? new ObservableCollection<string>()).ToObservableCollection(),
                                           IdType = CoverIdIdType.ISBN
                                       };
                        coverIdsArray = coverIds.ToArray();
                        covers = await RequestCoversCached(coverIdsArray);
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
                    //query colors
                    var imageUrls = coversAlreadyExisting.Union(covers ?? new List<Cover>(0)).Select(c => new ImageUrl()
                    {
                        Id = c.Id,
                        Url = c.ImageLarge
                    })
                    .Where(iurl => iurl.Url != null) // remove all urls that are null
                    .Where(c => !colorsAlreadyExisting.Any(color => color.Id == c.Id)) //remove all colors that were already received
                    .ToObservableCollection();
                    if (imageUrls.Any())
                    {
                        var imageColorParams = new ImageColorsParams() { Images = imageUrls };
                        var shelfhub = createShelfhubClient();
                        var colorResponse = await shelfhub.ImagecolorsAsync(imageColorParams);
                        foreach (UIColors uicolors in colorResponse)
                        {
                            var hit = FindHitWithIsbn(hits, uicolors.Id);
                            if (hit != null)
                            {
                                hit.CoverColorScheme = new picibits.bib.HistomatColorScheme()
                                {
                                    Primary = uicolors.Background,
                                    Secondary = uicolors.Foreground,
                                    Light = uicolors.Light,
                                    Dark = uicolors.Dark
                                };
                            }
                        }
                    }
                    //validate cache
                    ValidateCoverCacheSize();
                }
                catch (Exception ex)
                {
                    Pici.Log.error(typeof(ShelfhubSearch), "shelfhub cover request failed", ex, coverIdsArray);
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

        public static void TrackTap(string action, string name = null)
        {
            Track("tap", action, name);
            Track("interaction", "tap", action);
        }

        public static void TrackOpen(string action, string name = null)
        {
            Track("open", action, name);
            Track("interaction", "open", action);
        }

        public static void TrackClose(string action, string name = null)
        {
            Track("close", action, name);
            Track("interaction", "close", action);
        }

        public static void TrackSwipe(string action, string name = null)
        {
            Track("swipe", action, name);
            Track("interaction", "swipe", action);
        }

        public static void Track(string category, string action, string name = null)
        {

            if (IS_DEBUG || !USES_SHELFHUB_PRODUCTION_SERVER)
            {
                return;
            }

            var shelfhub = ShelfhubSearch.createShelfhubClient();
            shelfhub.TrackAsync(new picibird.shelfhub.TrackingParams()
            {
                Category = category,
                ActionType = action,
                ActionName = name,
                Shelfhub = ShelfhubSearch.PROFILE_ACTIVE
            }).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    Pici.Log.info(typeof(ShelfhubSearch), "tracking info", task.Result.Success);
                }
                else
                {
                    Pici.Log.warn(typeof(ShelfhubSearch), "tracking failed");
                }
            });
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
