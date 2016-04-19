// ShelfContentInfo.cs
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

namespace picibird.hbs.viewmodels.infoShield
{
    public class ShelfContentInfo : Model
    {
        #region Text

        private string mText;

        public string Text
        {
            get { return mText; }
            set
            {
                if (mText != value)
                {
                    var old = mText;
                    mText = value;
                    RaisePropertyChanged("Text", old, value);
                }
            }
        }

        #endregion Text

        #region From

        private string mFrom;

        public string From
        {
            get { return mFrom; }
            set
            {
                if (mFrom != value)
                {
                    var old = mFrom;
                    mFrom = value;
                    RaisePropertyChanged("From", old, value);
                }
            }
        }

        #endregion From

        #region To

        private string mTo;

        public string To
        {
            get { return mTo; }
            set
            {
                if (mTo != value)
                {
                    var old = mTo;
                    mTo = value;
                    RaisePropertyChanged("To", old, value);
                }
            }
        }

        #endregion To
    }
}