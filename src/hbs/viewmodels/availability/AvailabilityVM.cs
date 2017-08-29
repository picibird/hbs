// AvailabilityVM.cs
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
using picibird.hbs.availabity;
using picibits.bib;
using picibits.core;
using picibits.core.intent;
using picibits.core.mvvm;
using picibits.app.behaviour;
using picibits.app.mvvm;
using picibits.core.helper;

namespace picibird.hbs.viewmodels.availability
{
    public class AvailabilityVM : ViewModel
    {

        public event EventHandler Clicked;

        #region ClickCommand

        private DelegateCommand mClickCommand;

        public DelegateCommand ClickCommand
        {
            get
            {
                if (mClickCommand == null)
                    mClickCommand = new DelegateCommand((param) => Clicked?.Invoke(this, EventArgs.Empty));
                return mClickCommand;
            }
        }

        #endregion ClickCommand

        public AvailabilityVM(HistomatColorScheme colorScheme, AvailabilityInfo info)
        {
            Style = new ViewStyle("AvailabilityViewStyle");
            CoverColorScheme = colorScheme;
            Model = info;
            Clicked += OnClicked;
        }

        private void OnClicked(object sender, EventArgs e)
        {
            var av = Model as AvailabilityInfo;
            if (av != null)
            {
                Uri uri = null;
                if (Uri.TryCreate(av.Url, UriKind.Absolute, out uri))
                {
                    Intent openUrlIntent = new Intent(Intent.ACTION_OPEN, uri);
                    openUrlIntent.AddExtra("title", av.Signature);
                    openUrlIntent.AddExtra("subtitle", av.Url);
                    Pici.Intent.Send(openUrlIntent);
                }
                else
                {
                    Pici.Log.warn(typeof(AvailabilityInfo), "cannot parse availability href to uri: {0}", av.Url);
                }
            }
        }


        public HistomatColorScheme CoverColorScheme { get; private set; }
    }
}