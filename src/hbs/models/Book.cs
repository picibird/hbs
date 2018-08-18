// Book.cs
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
using System.Linq;
using picibird.hbs.config;
using picibird.hbs.ldu;
using picibits.app.services;
using picibits.core;
using picibits.core.intent;
using picibits.core.mvvm;

namespace picibird.hbs.models
{
    public class Book : Model
    {
        public void LoadQrCode()
        {
            try
            {
                var qrCodes = Pici.Services.Get<IQRCodes>();
                string webshelfUri = Hit.WebshelfUris.DefaultIfEmpty().First();
                if (!string.IsNullOrEmpty(webshelfUri))
                {
                    var qrCodeSize = (int) Config.Shelf3D.DefaultSpineWidth;
                    var qrCodeImage = qrCodes.Encode(webshelfUri, qrCodeSize, qrCodeSize,
                        Hit.CoverColorScheme.Light, Hit.CoverColorScheme.Dark);
                    Hit.QRCodeImage = qrCodeImage;
                }
            }
            catch (Exception)
            {
                //if no IQrCode is injected
            }
        }

        public void WriteNfcUri()
        {
            if (Hit.WebshelfUris?.Count > 0)
            {
                string webshelfUri = Hit.WebshelfUris.First();
                if (!string.IsNullOrEmpty(webshelfUri))
                {
                    Pici.Intent.Send(new Intent(Intent.ACTION_WRITE_NFC_URI, new Uri(webshelfUri)));
                }
            }
        }

        #region Hit

        private Hit mHit;

        public Hit Hit
        {
            get { return mHit; }
            set
            {
                if (mHit != value)
                {
                    var old = mHit;
                    mHit = value;
                    RaisePropertyChanged("Hit", old, value);
                }
            }
        }

        #endregion Hit

        #region CoverWidth

        private double mCoverWidth;

        public double CoverWidth
        {
            get { return mCoverWidth; }
            set
            {
                if (mCoverWidth != value)
                {
                    var old = mCoverWidth;
                    mCoverWidth = value;
                    RaisePropertyChanged("CoverWidth", old, value);
                }
            }
        }

        #endregion CoverWidth

        #region CoverHeight

        private double mCoverHeight;

        public double CoverHeight
        {
            get { return mCoverHeight; }
            set
            {
                if (mCoverHeight != value)
                {
                    var old = mCoverHeight;
                    mCoverHeight = value;
                    RaisePropertyChanged("CoverHeight", old, value);
                }
            }
        }

        #endregion CoverHeight

        #region SpineWidth

        private double mSpineWidth;

        public double SpineWidth
        {
            get { return mSpineWidth; }
            set
            {
                if (mSpineWidth != value)
                {
                    var old = mSpineWidth;
                    mSpineWidth = value;
                    RaisePropertyChanged("SpineWidth", old, value);
                }
            }
        }

        #endregion SpineWidth
    }
}