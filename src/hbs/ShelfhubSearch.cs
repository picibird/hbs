using System;
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

namespace picibird.hbs
{
    public class ShelfhubSearch : Search, IShelfhubSearch
    {


        public QueryParams QueryParams { get; protected set; }

        public ShelfhubSearch()
        {
            ShelfhubHelper.Search = this;
        }

        public override async Task Start(string searchText)
        {
            try
            {
                BeforeShelfhubSearch(searchText);
                Shelfhub shelfhub = createShelfhubClient();
                QueryParams = new QueryParams()
                {
                    Query = searchText,
                    Offset = 0,
                    Limit = 68,
                    Shelfhub = new ShelfhubParams()
                    {
                        Service = "ch.swissbib.solr.basel"
                    }
                };
                var queryResult = await shelfhub.QueryAsync(QueryParams);
                AfterShelfhubSearch(queryResult);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task QueryPages(IEnumerable<Page> pages)
        {
            Page first = pages.First();
            Page last = pages.Last();
            Shelfhub shelfhub = createShelfhubClient();
            var queryResult = await shelfhub.QueryAsync(QueryParams);
        }

        private void BeforeShelfhubSearch(string text)
        {
            if (cts != null)
                cts.Cancel();

            cts = new CancellationTokenSource();
            Callback = new SearchCallback<SearchStatus>(cts.Token);
            Callback.ResultCountChanged += OnSearchCallbackResultCountChanged;
            FilterList = Callback.FilterList;

            SearchText = text;

            SearchRequest = new SearchRequest(text);
            SearchRequest.ItemsPerPage = PageItemsCount;

            Session = new ShelfhubSearchSession();
            Pages.Session = Session;
            OnSearchStarting(SearchStartingReason.NewSearch, text, FilterList);
        }

        private void AfterShelfhubSearch(QueryResponse response)
        {
            //convert shelfhubitems to hits and call Session
            var hits = response.Items.ToHits();
            Session.Start(hits, SearchRequest, Callback);
            Callback.ResultCount = response.ItemsFound;
            Callback.MaxPageIndex = hits.Count;
            RequestCovers(response.Items, hits).ContinueWith((t) =>
            {
            });
        }

        public async Task RequestCovers(IList<ShelfhubItem> shelfhubItems, IList<Hit> hits)
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
