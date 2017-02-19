// SearchSession.cs
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
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Flurl.Http;
using picibird.hbs.ldu.Helper;
using picibits.core;
using picibits.core.extension;
using picibits.core.mvvm;
using picibits.core.util;

namespace picibird.hbs.ldu
{
    public class SearchSession : Model, IDisposable
    {
        private static readonly XmlSerializer serializerPazPar2Init = new XmlSerializer(typeof(PazPar2Init));
        private static readonly XmlSerializer serializerPazPar2Stat = new XmlSerializer(typeof(SearchStatus));
        private static readonly XmlSerializer serializerPazPar2Show = new XmlSerializer(typeof(PazPar2Show));
        private static readonly XmlSerializer serializerPazPar2Termlist = new XmlSerializer(typeof(Pazpar2Termlist));
        private static readonly XmlSerializer serializerRecord = new XmlSerializer(typeof(Record));

        public SearchCallback<SearchStatus> Callback;

        #region Request

        public event PropertyChangedHandler<SearchRequest> RequestChanged;

        private SearchRequest mRequest;

        public SearchRequest Request
        {
            get { return mRequest; }
            set
            {
                if (mRequest != value)
                {
                    SearchRequest old = mRequest;
                    mRequest = value;
                    OnRequestChanged(old, value);
                }
            }
        }

        protected virtual void OnRequestChanged(SearchRequest oldRequest, SearchRequest newRequest)
        {
            RaisePropertyChanged("Request", oldRequest, newRequest);
            if (RequestChanged != null)
                RequestChanged(this, new PropertyChangedEventArgs<SearchRequest>("Request", oldRequest, newRequest));
        }

        #endregion Request

        #region Status

        public event PropertyChangedHandler<SearchStatus> StatusChanged;

        private SearchStatus mStatus;

        public SearchStatus Status
        {
            get { return mStatus; }
            set
            {
                if (mStatus != value)
                {
                    SearchStatus old = mStatus;
                    mStatus = value;
                    OnStatusChanged(old, value);
                    //debug
                    //Pici.Log.warn(typeof(SearchSession), " STAT: " + value.ToString());
                }
            }
        }

        protected virtual void OnStatusChanged(SearchStatus oldStatus, SearchStatus newStatus)
        {
            RaisePropertyChanged("Status", oldStatus, newStatus);
            if (StatusChanged != null)
                StatusChanged(this, new PropertyChangedEventArgs<SearchStatus>("Status", oldStatus, newStatus));
            Callback.ReportStatus(newStatus);
        }

        #endregion Status

        public int ID { get; private set; }

        public List<Hit> FakeHits { get; private set; }


        public SearchSession()
        {
        }

        public virtual async Task Start(SearchRequest sr, SearchCallback<SearchStatus> statusCallback)
        {
            this.Callback = statusCallback;
            this.Request = sr;
            this.Request.FilterListChanged += OnFilterListChanged;
            this.Request.SortingChanged += OnSortOrderChanged;

            FakeHits = null;


            Pici.Log.debug(typeof(SearchSession), "searching:" + sr.SearchString);
            ID = await PP2_Init();
            if (ID > 0)
            {
                //run query if successfully initiated session
                try
                {
                    Callback.ThrowIfCancellationRequested();
                    //start session ping loop
                    Task pingTask = StartPingLoop();
                    //run request
                    await RunQuery(String.Format("searching {0}", sr.SearchString), true, true);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                finally
                {
                    Pici.Log.info(typeof(SearchSession), "running inital search finished");
                }
            }
        }

        public virtual void Start(List<Hit> hits, SearchRequest sr, SearchCallback<SearchStatus> statusCallback)
        {
            WaitUntilQueryFinishes(statusCallback.CancellationToken.Value);
            this.Callback = statusCallback;
            this.Request = sr;
            FakeHits = hits;
            Status = new SearchStatus();
            if (QueryStarted != null)
                QueryStarted(null);
            Status = new SearchStatus()
            {
                progress = 1.0,
                hits = hits.Count
            };
        }

        protected virtual async void OnSortOrderChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await RunQuery("sort order changed", false, false);
        }

        protected virtual async void OnFilterListChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await RunQuery("filter list changed", false, false);
        }

        #region PP2 Init

        protected virtual async Task<int> PP2_Init()
        {
            try
            {
                // create session
                Stream stream = await UrlHelper.GetInitUrl().GetStreamAsync();
                PazPar2Init pInit =
                    await
                        Async.DeserializeXml<PazPar2Init>(serializerPazPar2Init, stream,
                            Pazpar2Settings.LOG_HTTP_RESPONSES);
                Callback.ThrowIfCancellationRequested();
                return pInit.Session;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Pici.Log.error(typeof(SearchSession), "pazpar2 init error", ex);
                throw;
            }
        }

        #endregion PP2 Init

        #region QUERY

        protected virtual async Task RunQuery(string reason, bool throwCancel, bool throwError)
        {
            try
            {
                await RunQuery(Request);
            }
            catch (Exception ex)
            {
                if (ex is FlurlHttpException || ex is OperationCanceledException)
                {
                    Pici.Log.debug(typeof(SearchSession), "canceled query: " + reason);
                }
                else
                {
                    Pici.Log.error(typeof(SearchSession), String.Format("error running query: " + reason, ex.Message),
                        ex);
                }
                if (throwError)
                    throw;
            }
        }

        private SemaphoreSlim RunQuerySemaphore = new SemaphoreSlim(1);
        private CancellationTokenSource RunQueryCancelTokenSource = new CancellationTokenSource();

        public bool IsRunningQuery { get; private set; }

        public event SimpleEventHandler<SearchRequest> QueryStarted;

        public async void WaitUntilQueryFinishes(CancellationToken token)
        {
            try
            {
                await RunQuerySemaphore.WaitAsync(token);
            }
            catch (Exception)
            {
            }
            finally
            {
                RunQuerySemaphore.Release();
            }
        }

        protected virtual async Task RunQuery(SearchRequest sr)
        {
            bool successful = false;
            try
            {
                //cancel running request
                RunQueryCancelTokenSource.Cancel();
                RunQueryCancelTokenSource = new CancellationTokenSource();
                CancellationTokenSource CancelTokenSource =
                    RunQueryCancelTokenSource.JoinToLinked(Callback.CancellationToken.Value);
                //wait & lock
                await RunQuerySemaphore.WaitAsync(CancelTokenSource.Token);
                //reset progress
                IsRunningQuery = true;
                Status = new SearchStatus();
                if (QueryStarted != null)
                    QueryStarted(sr);
                //running query
                Pici.Log.debug(typeof(SearchSession), "running query");
                await PP2_Query(sr);
                CancelTokenSource.ThrowIfCancellationRequested();
                // run 'termlist' loop
                Task termlistLoopTask = RunTermslistLoopAsync(CancelTokenSource.Token);
                // run 'stat' loop (show, record)
                Task progressLoopTask = RunProgressLoopAsync(CancelTokenSource.Token);
                //wait all
                Task[] tasks = new Task[] {termlistLoopTask, progressLoopTask};
                await Task.WhenAll(tasks);
                CancelTokenSource.ThrowIfCancellationRequested();
                //prolong status progress updates to get changes after 100%
                //Task prolongStatusProgress = ProlongStatusProgressAfterFinish(CancelTokenSource.Token);
                successful = true;
            }
            catch (OperationCanceledException)
            {
                Pici.Log.debug(typeof(SearchSession), "query cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                Pici.Log.error(typeof(SearchSession), "error running query", ex);
                throw;
            }
            finally
            {
                //update final status
                //if (!successful)
                //    Status.progress = 2;
                IsRunningQuery = false;
                //unlock
                RunQuerySemaphore.Release();
            }
        }

        protected virtual async Task PP2_Query(SearchRequest searchRequest)
        {
            try
            {
                // send query
                await UrlHelper.GetQueryUrl(ID.ToString(), searchRequest).GetAsync();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Pici.Log.error(typeof(SearchSession), "query request error", ex);
                throw;
            }
        }

        #endregion QUERY

        #region PING LOOP

        internal bool IsRunningPingLoop { get; private set; }

        protected virtual async Task StartPingLoop()
        {
            //return if ping loop already running
            if (IsRunningPingLoop)
            {
                Pici.Log.warn(typeof(SearchSession), "already running pung loop");
                return;
            }
            try
            {
                //start ping loop
                Pici.Log.info(typeof(SearchSession), "starting ping loop");
                IsRunningPingLoop = true;
                //run ping loop
                while (!Callback.IsCancellationRequested())
                {
                    //get qequest ping
                    HttpResponseMessage m = await UrlHelper.GetPingUrl(ID.ToString()).GetAsync();
                    //log response
                    Pici.Log.info(typeof(SearchSession), String.Format("ping status code: {0}", m.StatusCode));
                    //wait async until next ping
                    await Task.Delay(Pazpar2Settings.DELAY_PING, Callback.CancellationToken.Value);
                }
                //end ping loop
                IsRunningPingLoop = false;
            }
            catch (OperationCanceledException)
            {
                //end ping loop on cancelation
                IsRunningPingLoop = false;
                throw;
            }
            catch (Exception ex)
            {
                //some error occured
                Pici.Log.error(typeof(SearchSession), "ping error", ex);
                throw;
            }
            finally
            {
                //log ping loop ending reason
                string reason = IsRunningPingLoop ? "ERROR" : "CANCELATION";
                Pici.Log.info(typeof(SearchSession), String.Format("ping loop stopped because of {0}", reason));
            }
        }

        #endregion PING LOOP

        protected virtual async Task<SearchStatus> PP2_Stat(CancellationToken cancelToken)
        {
            try
            {
                cancelToken.ThrowIfCancellationRequested();
                using (Stream stream = await UrlHelper.GetStatUrl(ID.ToString()).GetStreamAsync())
                {
                    SearchStatus newStatus =
                        await
                            Async.DeserializeXml<SearchStatus>(serializerPazPar2Stat, stream,
                                Pazpar2Settings.LOG_HTTP_RESPONSES);
                    //cancel if requested
                    cancelToken.ThrowIfCancellationRequested();
                    return newStatus;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ex.IsCanceledException())
                {
                    //do nothing here
                }
                else
                {
                    Pici.Log.error(typeof(SearchSession), "stat request error", ex);
                }
                throw;
            }
        }

        protected virtual async Task ProlongStatusProgressAfterFinish(CancellationToken cancelToken)
        {
            int updateDelay = 1000;
            Func<Task, Task> statusUpdateTask = async (t) =>
            {
                SearchStatus newStatus = await PP2_Stat(cancelToken);
                //let progress grow by 0.01 after 100% was reached once
                if (Status.progress >= 1.0d && newStatus.progress == 1.0d)
                {
                    newStatus.progress = Status.progress + 0.01;
                    Status = newStatus;
                }
            };

            await Task.Delay(updateDelay, cancelToken)
                .ContinueWith(statusUpdateTask)
                .ContinueWith(async (t) => await Task.Delay(updateDelay, cancelToken))
                .ContinueWith(statusUpdateTask)
                .ContinueWith(async (t) => await Task.Delay(updateDelay, cancelToken))
                .ContinueWith(statusUpdateTask)
                .ContinueWith(async (t) => await Task.Delay(updateDelay, cancelToken))
                .ContinueWith(statusUpdateTask);
        }


        public virtual async Task<PazPar2Show> PP2_Show(int pageIdx, int itemCount, CancellationToken cancelToken)
        {
            try
            {
                int start = pageIdx*itemCount;
                PazPar2Show pp2Show = null;
                if (FakeHits != null)
                {
                    int count = itemCount;
                    pp2Show = new PazPar2Show();
                    pp2Show.merged = FakeHits.Count;
                    if (FakeHits.Count < start + count)
                    {
                        count = Math.Max(FakeHits.Count - start, 0);
                    }
                    if (FakeHits.Count >= (start + count))
                    {
                        pp2Show.Hits = FakeHits.GetRange(start, count);
                    }
                    else
                    {
                        pp2Show.Hits = new List<Hit>();
                    }
                }
                else
                {
                    string sort = UrlHelper.CreateSortStringFromSearchRequest(Request);
                    string showUrl = UrlHelper.GetShowUrl(ID.ToString(), start, itemCount, sort);
                    using (Stream stream = await showUrl.GetStreamAsync())
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        pp2Show =
                            await
                                Async.DeserializeXml<PazPar2Show>(serializerPazPar2Show, stream,
                                    Pazpar2Settings.LOG_HTTP_RESPONSES);
                        //debug
                        //Pici.Log.warn(typeof(SearchSession), "SHOW: " + pp2Show.ToString());
                        //cancel if
                        cancelToken.ThrowIfCancellationRequested();
                    }
                }
                //update
                var resultCount = pp2Show.merged;
                int maxPageIndex = (int) Math.Ceiling(Callback.ResultCount*(1.0d/itemCount)) - 1;
                maxPageIndex = Math.Max(0, maxPageIndex);
                Callback.MaxPageIndex = maxPageIndex;

                return pp2Show;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (FlurlHttpException ex)
            {
                if (ex.InnerException != null && ex.InnerException is OperationCanceledException)
                {
                    throw ex.InnerException;
                }
                else
                {
                    Pici.Log.error(typeof(SearchSession), "Error on show request.", ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                Pici.Log.error(typeof(SearchSession), "Error on show request.", ex);
                throw;
            }
        }

        internal async Task<Record> PP2_Record(string recid)
        {
            try
            {
                string recordUrl = UrlHelper.GetRecordUrl(ID.ToString(), recid);
                using (Stream stream = await recordUrl.GetStreamAsync())
                {
                    return
                        await Async.DeserializeXml<Record>(serializerRecord, stream, Pazpar2Settings.LOG_HTTP_RESPONSES);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Pici.Log.error(typeof(SearchSession), "Error on Record request. (Detail)", ex);
                throw;
            }
        }

        protected virtual async Task<Pazpar2Termlist> PP2_Termlist()
        {
            try
            {
                string termListUrl = UrlHelper.GetTermlistUrl(ID.ToString());
                using (Stream stream = await termListUrl.GetStreamAsync())
                {
                    return
                        await
                            Async.DeserializeXml<Pazpar2Termlist>(serializerPazPar2Termlist, stream,
                                Pazpar2Settings.LOG_HTTP_RESPONSES);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Pici.Log.error(typeof(SearchSession), "Error on termlist request.", ex);
                throw;
            }
        }

        protected virtual async Task RunTermslistLoopAsync(CancellationToken cancelToken)
        {
            Pazpar2Termlist pp2Termlist = null;
            //Pici.Log.debug(typeof(SearchSession), "enter termlist loop");

            int refreshAfterFinish = 0;

            while (!cancelToken.IsCancellationRequested && refreshAfterFinish < 3)
            {
                if (Status.progress >= 1)
                {
                    refreshAfterFinish++;
                }

                pp2Termlist = await PP2_Termlist();
                cancelToken.ThrowIfCancellationRequested();

                //setting category for terms
                foreach (FilterCategory tc in pp2Termlist.filterCategories)
                {
                    foreach (Filter f in tc.Filter)
                    {
                        f.Catgegory = tc.Id;
                    }
                }

                // update termList in callback


                Callback.FilterList.RemoveAll();

                // todo update filters 

                foreach (FilterCategory fcat in pp2Termlist.filterCategories)
                {
                    Callback.FilterList.Add(fcat);
                }
                //delay with cancellation
                await Task.Delay(Pazpar2Settings.DELAY_TERMLIST_REQUEST, cancelToken);
            }
            //Pici.Log.debug(typeof(SearchSession), "exit termlist loop \r\n");
        }


        protected virtual async Task RunProgressLoopAsync(CancellationToken cancelToken)
        {
            //Pici.Log.debug(typeof(SearchSession), "enter progress loop");
            try
            {
                while (!cancelToken.IsCancellationRequested && Status.progress < 1)
                {
                    SearchStatus newStatus = await PP2_Stat(cancelToken);
                    if (Status.progress < newStatus.progress ||
                        Status.hits < newStatus.hits ||
                        Status.records < newStatus.records)
                    {
                        Status = newStatus;
                    }
                    //delay with cancellation
                    await Task.Delay(Pazpar2Settings.DELAY_STAT_REQUEST, cancelToken);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Pici.Log.error(typeof(SearchSession), "ping loop error ", ex);
                throw;
            }
            //Pici.Log.debug(typeof(SearchSession), "exit progress loop \r\n");
        }

        public override void DisposeManaged()
        {
            base.DisposeManaged();
            RunQueryCancelTokenSource.Cancel();
            RunQueryCancelTokenSource.Dispose();
        }
    }
}