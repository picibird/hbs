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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
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

        public IList<Hit> FakeHits { get; private set; }


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
        }

        public virtual void Start(IList<Hit> hits, SearchRequest sr, SearchCallback<SearchStatus> statusCallback)
        {
            WaitUntilQueryFinishes(statusCallback.CancellationToken.Value);
            this.Callback = statusCallback;
            this.Request = sr;
            FakeHits = hits;
            if (QueryStarted != null)
                QueryStarted(null);
        }

        protected virtual async void OnSortOrderChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await RunQuery("sort order changed", false, false);
        }

        protected virtual async void OnFilterListChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await RunQuery("filter list changed", false, false);
        }

        #region QUERY

        protected virtual async Task RunQuery(string reason, bool throwCancel, bool throwError)
        {
            try
            {
                await RunQuery(Request);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
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

        #endregion QUERY


        public override void DisposeManaged()
        {
            base.DisposeManaged();
            RunQueryCancelTokenSource.Cancel();
            RunQueryCancelTokenSource.Dispose();
        }
    }
}