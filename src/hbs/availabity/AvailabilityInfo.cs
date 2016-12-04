// AvailabilityInfo.cs
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

namespace picibird.hbs.availabity
{
    public class AvailabilityInfo : Model
    {
        #region Signature

        private string mSignature;

        public string Signature
        {
            get { return mSignature; }
            set
            {
                if (mSignature != value)
                {
                    var old = mSignature;
                    mSignature = value;
                    RaisePropertyChanged("Signature", old, value);
                }
            }
        }

        #endregion Signature

        #region Status

        private string mStatus;

        public string Status
        {
            get { return mStatus; }
            set
            {
                if (mStatus != value)
                {
                    var old = mStatus;
                    mStatus = value;
                    RaisePropertyChanged("Status", old, value);
                }
            }
        }

        #endregion Status

        #region OriginalStatus

        private string mOriginalStatus;

        public string OriginalStatus
        {
            get { return mOriginalStatus; }
            set
            {
                if (mOriginalStatus != value)
                {
                    var old = mOriginalStatus;
                    mOriginalStatus = value;
                    RaisePropertyChanged("OriginalStatus", old, value);
                }
            }
        }

        #endregion OriginalStatus

        #region Info

        private string mInfo;

        public string Info
        {
            get { return mInfo; }
            set
            {
                if (mInfo != value)
                {
                    var old = mInfo;
                    mInfo = value;
                    RaisePropertyChanged("Info", old, value);
                }
            }
        }

        #endregion Info

        private string mHref;

        public string Href
        {
            get { return mHref; }
            set
            {
                if (mHref != value)
                {
                    var old = mInfo;
                    mHref = value;
                    RaisePropertyChanged(nameof(Href), old, value);
                }
            }
        }
    }
}