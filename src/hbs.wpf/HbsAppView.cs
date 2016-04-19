// HbsAppView.cs
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

using picibird.wpf.app;
using picibird.wpf.core.views;
using picibits.core;

namespace picibird.hbs.wpf
{
    public class HbsAppView : AppView
    {
        private ContentViewAdapter Adapter;

        public HbsAppView()
        {
            ValidateThreadpool();
        }

        private void ValidateThreadpool()
        {
            var workerThreads = 0;
            var completionPortThreads = 0;
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
            Pici.Log.info(typeof (HbsAppView),
                string.Format("AvailableWorkerThreads={0}; AvailableCompletionPortThreads={1}", workerThreads,
                    completionPortThreads));
            //ThreadPool.SetMaxThreads((int)(workerThreads * 0.5), (int)(completionPortThreads * 0.5));
        }
    }
}