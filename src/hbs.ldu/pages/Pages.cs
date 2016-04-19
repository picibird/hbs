// Pages.cs
// Date Created: 21.01.2016
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;

using picibits.core;
using picibits.core.extension;
using picibits.core.mvvm;

namespace picibird.hbs.ldu.pages
{
    public class Pages : Model
    {

        internal Dictionary<int, Page> RequestedPages = new Dictionary<int, Page>();

        public int PageOffset { get; private set; }

        private object PageLock = new object();

        #region Session

        private SearchSession mSession;
        public SearchSession Session
        {
            get { return mSession; }
            set
            {
                if (mSession != value)
                {
                    SearchSession old = mSession;
                    mSession = value;
                    OnSessionChanged(old, value);
                }
            }
        }

        protected virtual void OnSessionChanged(SearchSession oldSession, SearchSession newSession)
        {
            RaisePropertyChanged("Session", oldSession, newSession);
            if (oldSession != null)
            {
                oldSession.RequestChanged -= OnSessionRequestChanged;
                oldSession.StatusChanged -= OnSessionStatusChanged;
                newSession.QueryStarted -= OnQueryStarted;
            }
            if (newSession != null)
            {
                newSession.RequestChanged += OnSessionRequestChanged;
                newSession.StatusChanged += OnSessionStatusChanged;
                newSession.QueryStarted += OnQueryStarted;
            }
        }

        #endregion Session

        public Pages()
        {
        }

        private void OnSessionStatusChanged(object sender, PropertyChangedEventArgs<SearchStatus> args)
        {
            SearchStatus oldStatus = args.Old;
            SearchStatus newStatus = args.New;
            if (newStatus == null || newStatus.progress < 0)
                return;
            if (oldStatus.progress < newStatus.progress ||
                oldStatus.hits < newStatus.hits)
            {
                var cancelToken = Session.Callback.CancellationToken.Value;
                Task pageUpdateTask = RunUpdateLoop(cancelToken);
            }
        }

        void OnSessionRequestChanged(object sender, PropertyChangedEventArgs<SearchRequest> requestChanged)
        {
            if (requestChanged.Old != null)
            {
                requestChanged.New.PropertyChanged -= OnRequestPropertyChanged;
            }
            if (requestChanged.New != null)
            {
                requestChanged.New.PropertyChanged += OnRequestPropertyChanged;
                PageOffset = requestChanged.New.ItemsPerPage;
            }
        }

        private void OnQueryStarted(SearchRequest sender)
        {
            ResetRequestedPages();
        }

        protected void OnRequestPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            if (e.PropertyName.Equals("PageIdx"))
            {
                SearchRequest sr = sender as SearchRequest;
                try
                {
                    Task t = RunUpdateLoop(Session.Callback.CancellationToken.Value);
                }
                catch (OperationCanceledException)
                {
                    Pici.Log.info(typeof(SearchSession), "canceled page index changed update");
                }
                catch (Exception ex)
                {
                    Pici.Log.error(typeof(SearchSession), "An Exception occoured while updating page " + sr.PageIdx, ex);
                }
            }
        }

        public Page RequestPage(int pageIndex)
        {
            Page requestedPage = null;
            lock (PageLock)
            {
                if (!RequestedPages.TryGetValue(pageIndex, out requestedPage))
                {
                    requestedPage = new Page(pageIndex);
                    RequestedPages.Add(pageIndex, requestedPage);
                    return requestedPage;
                }
            }
            throw new ArgumentException("page already requested");

        }

        public void ReleasePage(Page page)
        {
            lock (PageLock)
            {
                if (RequestedPages.ContainsKey(page.Index))
                {
                    RequestedPages.Remove(page.Index);
                    return;
                }
            }
            throw new ArgumentException("cannot release, page not requested");
        }

        public void ResetRequestedPages()
        {
            lock (PageLock)
            {
                Pici.Log.warn(typeof(Pages), "RESET PAGES");
                //reset
                foreach (Page page in RequestedPages.Values)
                {
                    page.Reset();
                }
            }
        }

        private IEnumerable<Page> GetPagesToUpdate()
        {
            lock (PageLock)
            {
                
                Pici.Log.warn(typeof(Pages), "GET PAGES TO UPDATE");
		
                List<Page> copy = new List<Page>(RequestedPages.Values.ToList<Page>());
                //order by closest to index
                Func<Page, int> closeToIndexFunc = (p) => Math.Abs(p.Index - Session.Request.PageIdx);
                IEnumerable<Page> closeToIndex = copy.Where((p) => closeToIndexFunc.Invoke(p) <= 1).OrderBy(closeToIndexFunc);
                //return all pages that are not filled
                IEnumerable<Page> notFilledPages = closeToIndex.Where((p) => p.Hits.Count < Session.Request.ItemsPerPage);
                if (notFilledPages.Count() > 0)
                {
                    //return first page that has no hits yet
                    return notFilledPages;
                }
                //return sorted oldest since last update
                List<Page> oldestUpdated = closeToIndex.ToList<Page>();
                oldestUpdated.Sort(new PageUpdateTimeComparer());
                if (oldestUpdated.Count() > 0)
                {
                    //return first page that has no hits yet
                    return oldestUpdated;
                }
                return new List<Page>();
            }
        }


        private SemaphoreSlim UpdateLoopSemaphore = new SemaphoreSlim(1);
        private CancellationTokenSource UpdateCancelTokenSource = new CancellationTokenSource();

        public async Task RunUpdateLoop(CancellationToken token)
        {
            try
            {
                //cancel running
                UpdateCancelTokenSource.Cancel();
                UpdateCancelTokenSource = new CancellationTokenSource();
                CancellationTokenSource CancelTokenSource = UpdateCancelTokenSource.JoinToLinked(token);
                //wait for cancel
                await UpdateLoopSemaphore.WaitAsync(token);
                token.ThrowIfCancellationRequested();
                //get pages to update
                IEnumerable<Page> pages = GetPagesToUpdate();
                foreach (Page page in pages)
                {
                    await UpdatePage(page, token);
                    token.ThrowIfCancellationRequested();
                }
            }
            catch (Exception ex)
            {
                if (ex is FlurlHttpException || ex is OperationCanceledException)
                {
                    Pici.Log.debug(typeof(SearchSession), "canceled update loop. " + ex.Message);
                }
                else
                {
                    Pici.Log.error(typeof(SearchSession), "error inside page update loop ", ex);
                    throw;
                }
            }
            finally
            {
                UpdateLoopSemaphore.Release();
                //Pici.Log.debug(typeof(Pages), "stopped page update loop");
            }
        }

        private async Task UpdatePage(Page page, CancellationToken token)
        {
            //CancellationToken cancelToken = EnterRequestPage(page.Index).JoinToLinked(token).Token;
            try
            {
                double progress = Session.Status.progress;
                PazPar2Show pp2Show = await Session.PP2_Show(page.Index, PageOffset, token);
                if (pp2Show.merged > page.LastMergeCount)
                {
                    //Pici.Log.info(typeof(Pages), "updating page index " + page.Index);
                    page.UpdateList(pp2Show);
                }
            }
            catch (Exception ex)
            {
                if (ex is FlurlHttpException || ex is OperationCanceledException)
                {
                    Pici.Log.debug(typeof(SearchSession), "canceled page update. " + ex.Message);
                }
                else
                {
                    Pici.Log.error(typeof(SearchSession), "request page error", ex);
                    throw;
                }

            }
            finally
            {
                //LeaveRequestPage(page.Index);
            }
        }

        private Dictionary<int, CancellationTokenSource> cancelPageUpdateTokenDict = new Dictionary<int, CancellationTokenSource>();

        private CancellationToken EnterRequestPage(int pageIdx)
        {
            CancellationTokenSource cts = null;
            cancelPageUpdateTokenDict.TryGetValue(pageIdx, out cts);
            if (cts != null)
            {
                cts.Cancel();
                cancelPageUpdateTokenDict.Remove(pageIdx);
            }
            cts = new CancellationTokenSource();
            cancelPageUpdateTokenDict.Add(pageIdx, cts);
            return cts.Token;
        }

        private void LeaveRequestPage(int pageIdx)
        {
            cancelPageUpdateTokenDict.Remove(pageIdx);
        }



    }
}
