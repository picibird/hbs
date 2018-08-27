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
using picibird.shelfhub;
using picibits.app.mvvm;
using picibits.core;
using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.filter
{
    public class ChooserButtonViewModel : ButtonViewModel
    {

        public ChooserButtonViewModel(string filterCategory, ViewStyle style)
        {
            CategoryKey = filterCategory;
            Name = Pici.Resources.Find(filterCategory);
            Style = style;
            IsEnabled = false;
        }

        public virtual void OnFilterChanged(Facet filterCategory)
        {
            if (filterCategory.Key == CategoryKey)
            {
                var locKey = Pici.Resources.Find(filterCategory.Key);
                if (locKey != filterCategory.Key)
                {
                    Name = locKey;
                }
                else
                {
                    Name = Pici.Resources.Find(Name);
                }
                
                if (filterCategory.Values != null)
                {
                    Frequency = filterCategory.Values.Count;
                }
                else
                {
                    Frequency = 0;
                }
            }
        }

        protected virtual void OnFrequencyChanged(int frequency)
        {
            IsEnabled = frequency > 0;
            if (frequency > 1)
                Style = new ViewStyle("ChooserButtonWithCountViewStyle");
            else
                Style = new ViewStyle("ChooserButtonViewStyle");
        }

        protected virtual void OnCategoryNameEnumChanged(string category)
        {
            Name = Pici.Resources.Find(category);
        }

        #region Category

        private string mCategoryName;

        public string CategoryKey
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