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

        internal Page(int index)
        {
            this.Index = index;
            this.IndexReadable = index + 1;
            this.Hits = new ItemList<Hit>();
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

        internal void UpdateList(PazPar2Show pp2Show)
        {
            LastMergeCount = pp2Show.merged;
            LastUpdateTime = DateTime.Now;
            List<Hit> sList = pp2Show.Hits;

            Hits.RemoveAll();
            foreach (Hit hit in pp2Show.Hits)
                Hits.Add(hit);

            Pici.Log.debug(typeof(Page), "FireListUpdated for page " + Index);
            Hits.FireListUpdated();

            //bool isUpdated = false;
            //ItemList<Hit> tList = Hits;
            ////remove items not in current list
            //IEnumerable<Hit> toRemove = from sHit in sList where !tList.Contains<Hit>(sHit) select sHit;
            //foreach (var item in toRemove.ToList())
            //{
            //    isUpdated = isUpdated || tList.Remove(item);
            //}

            ////insert missing items 
            //for (int idx = 0; idx < sList.Count; idx++)
            //{
            //    Hit sItem = sList.ElementAt(idx);
            //    if (tList.Count <= idx || !sItem.Equals(tList.ElementAt(idx)))
            //    {
            //        tList.Insert(idx, sItem);
            //        isUpdated = true;
            //        //Task t = await  PP2_Record(sItem));
            //    }
            //}

            ////remove 'tail' of sourceList (items which moved up in position are inserted before and the old instance is still at the end of the list)
            //while (tList.Count > sList.Count)
            //{
            //    tList.RemoveAt(sList.Count);
            //    isUpdated = true;
            //}

            //if (isUpdated)
            //{
            //    Pici.Log.debug(typeof(Page), "FireListUpdated for page " + Index);
            //    tList.FireListUpdated();
            //}
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
