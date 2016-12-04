// FilterSelectionItemViewModel.cs
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
using picibird.hbs.ldu;
using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.filter
{
    public class FilterSelectionItemViewModel : ViewModel
    {
        public FilterSelectionItemViewModel(Filter filter)
        {
            Filter = filter;
            Name = string.Format("{0} {1} ", filter.Name, filter.Frequency);
            Style = new ViewStyle("FilterSelectionItemViewStyle");
        }

        public Filter Filter { get; private set; }

        public event EventHandler<bool> IsCheckedChanged;

        #region IsChecked

        private bool mIsChecked;

        public bool IsChecked
        {
            get { return mIsChecked; }
            set
            {
                if (mIsChecked != value)
                {
                    var old = mIsChecked;
                    mIsChecked = value;
                    RaisePropertyChanged("IsChecked", old, value);
                    if (IsCheckedChanged != null)
                        IsCheckedChanged(this, value);
                }
            }
        }

        #endregion IsChecked
    }
}