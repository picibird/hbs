// SearchCallback.cs
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

using System.ComponentModel;
using System.Threading;
using picibits.core.util;

namespace picibird.hbs.ldu
{
    public class SearchCallback<SearchStatus> : AsyncCallback<SearchStatus>
    {
        public FilterList<FilterCategory> FilterList { get; private set; }

        public event PropertyChangedEventHandler ResultCountChanged;
        public event PropertyChangedEventHandler MaxPageIndexChanged;

        private int _ResultCount;


        public int ResultCount
        {
            get { return _ResultCount; }

            set
            {
                if (value != _ResultCount)
                {
                    _ResultCount = value;
                    //post event
                    Post(new SendOrPostCallback((state) =>
                    {
                        if (ResultCountChanged != null)
                            ResultCountChanged(state, new PropertyChangedEventArgs("ResultCount"));
                    }), _ResultCount);
                }
            }
        }

        private int _MaxPageIndex;

        public int MaxPageIndex
        {
            get { return _MaxPageIndex; }

            set
            {
                if (value != _MaxPageIndex)
                {
                    _MaxPageIndex = value;
                    //post event
                    Post(new SendOrPostCallback((state) =>
                    {
                        if (MaxPageIndexChanged != null)
                            MaxPageIndexChanged(state, new PropertyChangedEventArgs("MaxPageIndex"));
                    }), _MaxPageIndex);
                }
            }
        }

        //public Page GetPage(int pageIdx)
        //{
        //    return BusinessModel.GetPage(pageIdx);
        //}

        public SearchCallback(CancellationToken? ct)
            : base(ct)
        {
            this.FilterList = new FilterList<FilterCategory>();
        }
    }
}