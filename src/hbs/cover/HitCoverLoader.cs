// HitCoverLoader.cs
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

using System.Threading;
using System.Threading.Tasks;
using picibird.hbs.ldu;
using picibits.bib;
using picibits.core;

namespace picibird.hbs.cover
{
    public class HitCoverLoader : IHitCoverLoader
    {
        private readonly SemaphoreSlim CoverLoadSemaphore = new SemaphoreSlim(10);

        private readonly ICoverService CoverService = Pici.Injections.GetInstance<ICoverService>();

        public void Load(Hit hit, string url)
        {
            var currentScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Run(async () =>
            {
                //load cover
                CoverLoadSemaphore.Wait();
                var coverData = new CoverData();
                //laod cover
                coverData.Image = await CoverService.LoadCoverAsync(url);
                CoverLoadSemaphore.Release();
                return coverData;
            }).ContinueWith(coverTask =>
            {
                //if successfull
                if (coverTask.IsCompleted && coverTask.Result != null)
                {
                    //set image
                    var coverData = coverTask.Result;
                    hit.CoverImage = coverData.Image;
                }
            }, currentScheduler);
        }
    }
}