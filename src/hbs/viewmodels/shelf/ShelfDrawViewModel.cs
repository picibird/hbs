// ShelfDrawViewModel.cs
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

using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.shelf
{
    public class ShelfDrawViewModel : ViewModel
    {
        public ShelfDrawViewModel()
        {
            Rows = 3;
            ShelfHeight = 40;
            ShelfStandWidth = 15;
            DepthX = 20;
            DepthY = 30;
            InfoShieldWidth = 200;
        }

        #region Rows

        private int mRows;

        public int Rows
        {
            get { return mRows; }
            set
            {
                if (mRows != value)
                {
                    var old = mRows;
                    mRows = value;
                    RaisePropertyChanged("Rows", old, value);
                }
            }
        }

        #endregion Rows

        #region RowHeight

        private double mRowHeight;

        public double RowHeight
        {
            get { return mRowHeight; }
            set
            {
                if (mRowHeight != value)
                {
                    var old = mRowHeight;
                    mRowHeight = value;
                    RaisePropertyChanged("RowHeight", old, value);
                }
            }
        }

        #endregion RowHeight

        #region ShelfHeight

        private int mShelfHeight;

        public int ShelfHeight
        {
            get { return mShelfHeight; }
            set
            {
                if (mShelfHeight != value)
                {
                    var old = mShelfHeight;
                    mShelfHeight = value;
                    RaisePropertyChanged("ShelfHeight", old, value);
                }
            }
        }

        #endregion ShelfHeight

        #region DepthX

        private double mDepthX;

        public double DepthX
        {
            get { return mDepthX; }
            set
            {
                if (mDepthX != value)
                {
                    var old = mDepthX;
                    mDepthX = value;
                    RaisePropertyChanged("DepthX", old, value);
                }
            }
        }

        #endregion DepthX

        #region DepthY

        private double mDepthY;

        public double DepthY
        {
            get { return mDepthY; }
            set
            {
                if (mDepthY != value)
                {
                    var old = mDepthY;
                    mDepthY = value;
                    RaisePropertyChanged("DepthY", old, value);
                }
            }
        }

        #endregion DepthY

        #region ShelfStandWidth

        private double mShelfStandWidth;

        public double ShelfStandWidth
        {
            get { return mShelfStandWidth; }
            set
            {
                if (mShelfStandWidth != value)
                {
                    var old = mShelfStandWidth;
                    mShelfStandWidth = value;
                    RaisePropertyChanged("ShelfStandWidth", old, value);
                }
            }
        }

        #endregion ShelfStandWidth

        #region InfoShieldWidth

        private double mInfoShieldWidth;

        public double InfoShieldWidth
        {
            get { return mInfoShieldWidth; }
            set
            {
                if (mInfoShieldWidth != value)
                {
                    var old = mInfoShieldWidth;
                    mInfoShieldWidth = value;
                    RaisePropertyChanged("InfoShieldWidth", old, value);
                }
            }
        }

        #endregion InfoShieldWidth
    }
}