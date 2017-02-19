﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using picibird.hbs.ldu;
using picibird.shelfhub;
using picibits.core.extension;
using picibits.core.collection;
using picibits.core.controls3D;
using Link = picibird.hbs.ldu.Link;
using System.Threading;
using picibits.core;
using picibird.hbs.ldu.pages;
using picibits.core.helper;
using picibits.core.util;

namespace picibird.hbs
{
    public class ShelfhubSearch : Search, IShelfhubSearch
    {
        private SynchronizationContext syncContext { get; set; }

        private readonly ConcurrentCache<int, ItemList<Hit>> PAGE_HITS_CACHE;

        public QueryParams QueryParams { get; protected set; }

        public ShelfhubSearch()
        {
            syncContext = SynchronizationContext.Current;
            ShelfhubHelper.Search = this;
            PAGE_HITS_CACHE = new ConcurrentCache<int, ItemList<Hit>>(async key => await LoadPageHitsAsync(key));
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

        public override async Task Start(string searchText)
        {
            try
            {
                BeforeShelfhubSearch(searchText);
                PAGE_HITS_CACHE.Clear();
                Shelfhub shelfhub = createShelfhubClient();
                QueryParams = new QueryParams()
                {
                    Query = searchText,
                    Offset = 0,
                    Limit = 34,
                    Shelfhub = new ShelfhubParams()
                    {
                        Service = "ch.swissbib.solr.basel"
                    }
                };
                var queryResult = await shelfhub.QueryAsync(QueryParams);
                await AfterShelfhubSearch(queryResult);
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
            Callback.StatusChanged += OnSearchStatusChanged;
            FilterList = Callback.FilterList;

            SearchText = text;

            SearchRequest = new SearchRequest(text);
            SearchRequest.ItemsPerPage = PageItemsCount;

            Session = new ShelfhubSearchSession();
            Pages.Session = Session;
            OnSearchStarting(SearchStartingReason.NewSearch, text, FilterList);
            Session.Start(null, SearchRequest, Callback);
            Session.Status = new SearchStatus();
        }

        private async Task AfterShelfhubSearch(QueryResponse response)
        {
            //convert shelfhubitems to hits and call Session
            var items = response.Items;
            var hits = items.ToHits();
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
            //finalize session
            Session.Status = new SearchStatus()
            {
                progress = 1.0,
                hits = hits.Count
            };
            Callback.ResultCount = response.ItemsFound;
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
                        var coverResponse = await shelfhub.CoverAsync(coverParams);
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
            shelfhub.BaseUrl = @"http://localhost:8080/api";
#endif
            return shelfhub;
        }

    }
}
