// ChooserButtonViewModel.cs
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

using picibird.hbs.ldu;
using picibits.app.mvvm;
using picibits.core;
using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.filter
{
    public class ChooserButtonViewModel : ButtonViewModel
    {
        public ChooserButtonViewModel(FilterCategoryId filterCategory, ViewStyle style)
        {
            CategoryName = filterCategory;
            Init(style);
        }

        public ChooserButtonViewModel(string filterCategory, ViewStyle style)
        {
            Name = filterCategory;
            Init(style);
        }

        public virtual void OnFilterChanged(FilterCategory filterCategory)
        {
            if (filterCategory.Id == CategoryName)
            {
                if (filterCategory.Filter != null)
                    Frequency = filterCategory.Filter.Count;
                else
                    Frequency = 0;
            }
        }

        private void Init(ViewStyle style)
        {
            IsEnabled = false;
            Style = style;
        }

        protected virtual void OnFrequencyChanged(int frequency)
        {
            IsEnabled = frequency >= 0;
        }

        protected virtual void OnCategoryNameEnumChanged(FilterCategoryId category)
        {
            Name = Pici.Resources.Find(category.ToString());
        }

        #region Category

        private FilterCategoryId mCategoryName;

        public FilterCategoryId CategoryName
        {
            get { return mCategoryName; }
            set
            {
                if (mCategoryName != value)
                {
                    var old = mCategoryName;
                    mCategoryName = value;
                    RaisePropertyChanged("Category", old, value);
                    OnCategoryNameEnumChanged(mCategoryName);
                }
            }
        }

        #endregion Category

        #region Frequencies

        private int mFrequency;

        public int Frequency
        {
            get { return mFrequency; }
            set
            {
                if (mFrequency != value)
                {
                    var old = mFrequency;
                    mFrequency = value;
                    OnFrequencyChanged(value);
                    RaisePropertyChanged("Frequency", old, value);
                }
            }
        }

        #endregion Frequencies
    }
}