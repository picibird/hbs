// Covers.cs
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Nito.AsyncEx;

using picibits.app.bitmap;
using picibits.bib;
using picibits.core;
using picibits.core.util;
using Async = picibits.app.util.Async;

namespace picibird.hbs.cover
{
    public class Covers : ICoverService
    {
        private readonly ConcurrentCache<string, IBitmapImage> COVER_CACHE;

        private SemaphoreSlim LoadCoverSemaphore = new SemaphoreSlim(15);

        public Covers()
        {
            COVER_CACHE = new ConcurrentCache<string, IBitmapImage>(async key => await LoadCoverAsync(key));
        }

        public void ValidateCacheSize()
        {
            while (COVER_CACHE.Count > 250)
            {
                AsyncLazy<IBitmapImage> removed;
                COVER_CACHE.ConcurrentDictionary.TryRemove(COVER_CACHE.ConcurrentDictionary.Keys.First(), out removed);
            }
            Pici.Log.warn(typeof (Covers), string.Format("validating cache size: {0} covers", COVER_CACHE.Count));
        }

        public Task<IBitmapImage> LoadCoverAsyncLazyCached(string url)
        {
            return COVER_CACHE.GetAsync(url);
        }

        public async Task<bool> HasCover(string coverUrl)
        {
            if (string.IsNullOrEmpty(coverUrl))
                return false;
            try
            {
                var message =
                    await coverUrl.WithTimeout((int) ldu.Pazpar2Settings.WEB_REQUEST_TIMEOUT.TotalSeconds).HeadAsync();
                var statusCodeInt = (int) message.StatusCode;
                if (statusCodeInt >= 200 && statusCodeInt <= 399)
                {
                    return true;
                }
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (FlurlHttpTimeoutException)
            {
                Pici.Log.warn(typeof (Covers), "Timeout loading HasCover()");
                return false;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && !(ex.InnerException is OperationCanceledException))
                    Pici.Log.error(typeof (Covers), string.Format("has cover {0} failed", coverUrl), ex);
                return false;
            }
        }

        public async Task<IBitmapImage> LoadCoverAsync(string url)
        {
            // do an async wait until we can schedule again
            //await LoadCoverSemaphore.WaitAsync();
            try
            {
                if (await HasCover(url))
                {
                    var ImageLoadingArgs = new AsyncCallback<double>();
                    return await Async.LoadBitmap(new Uri(url, UriKind.Absolute), ImageLoadingArgs);
                }
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Pici.Log.error(typeof (Covers), string.Format("loading cover {0} failed", url), ex);
                return null;
            }
            finally
            {
                //LoadCoverSemaphore.Release();
            }
        }
    }
}