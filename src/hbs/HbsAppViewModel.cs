// HbsAppViewModel.cs
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

using picibird.hbs.Intents;
using picibird.hbs.viewmodels;
using picibits.app;
using picibits.core;
using picibits.core.mvvm;

namespace picibird.hbs
{
    public class HbsAppViewModel : AppViewModel
    {
        public HbsAppViewModel()
        {
            Name = "Hybrid Bookshelf";
            //Full HD
            AppResolutionX = 1920;
            AppResolutionY = 1080;
            //4K
            //AppResolutionX = 3840;
            //AppResolutionY = 2160;

            Style = new ViewStyle("HBSAppStyleFixedResolution");
            //Style = new ViewStyle("HBSAppStyle");

            RegisterIntents();
            InitContent();
        }

        protected virtual void InitContent()
        {
            Content = new HBSViewModel();
        }

        protected virtual void RegisterIntents()
        {
            Pici.Intent.AddHandler(new OpenBookIntentHandler());
            Pici.Intent.AddHandler(new CloseBookIntentHandler());
        }

        #region AppResolutionX

        private double mAppResolutionX;

        public double AppResolutionX
        {
            get { return mAppResolutionX; }
            set
            {
                if (mAppResolutionX != value)
                {
                    var old = mAppResolutionX;
                    mAppResolutionX = value;
                    RaisePropertyChanged("AppResolutionX", old, value);
                }
            }
        }

        #endregion AppResolutionX

        #region AppResolutionY

        private double mAppResolutionY;

        public double AppResolutionY
        {
            get { return mAppResolutionY; }
            set
            {
                if (mAppResolutionY != value)
                {
                    var old = mAppResolutionY;
                    mAppResolutionY = value;
                    RaisePropertyChanged("AppResolutionY", old, value);
                }
            }
        }

        #endregion AppResolutionY
    }
}