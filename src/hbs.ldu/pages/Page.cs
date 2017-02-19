// Page.cs
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
using picibits.core;

namespace picibird.hbs.ldu.pages
{
    public class Page
    {
        public ItemList<Hit> Hits { get; internal set; }

        public int Index { get; private set; }
        public int IndexReadable { get; private set; }

        public double LastMergeCount { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public Page(int index, SynchronizationContext context = null)
        {
            this.Index = index;
            this.IndexReadable = index + 1;
            this.Hits = new ItemList<Hit>(context);
            this.LastMergeCount = 0;
            this.LastUpdateTime = DateTime.Now;
        }

        internal void Reset()
        {
            LastUpdateTime = DateTime.Now;
            LastMergeCount = 0;
            Hits.Clear();
            Hits.FireListUpdated();
        }

        public void UpdateList(PazPar2Show pp2Show)
        {
            LastMergeCount = pp2Show.merged;
            List<Hit> sList = pp2Show.Hits;
            UpdateList(sList);
        }

        public void UpdateList(IList<Hit> hits)
        {
            LastUpdateTime = DateTime.Now;

            Hits.RemoveAll();
            foreach (Hit hit in hits)
                Hits.Add(hit);

            Pici.Log.debug(typeof(Page), "FireListUpdated for page " + Index);
            Hits.FireListUpdated();
        }
    }

    public class PageUpdateTimeComparer : IComparer<Page>
    {
        public int Compare(Page x, Page y)
        {
            return DateTime.Compare(x.LastUpdateTime, y.LastUpdateTime);
        }
    }
}