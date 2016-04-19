// TitleButtonVM.cs
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

using picibits.app.mvvm;
using picibits.app.tools;
using picibits.core;
using picibits.core.intent;
using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.buttons
{
    public class TitleButtonVM : ButtonViewModel
    {
        private static readonly string TITLE = "Hybrid Bookshelf";

        public TitleButtonVM()
        {
            Style = new ViewStyle("TitleButtonStyle");
            Text = TITLE;
            TapBehaviour.Tap += OnTap;
        }


        private void OnTap(object sender, EventArgs e)
        {
#if DEBUG
            Pici.CrashApplicationNow();
#endif
            //abort if kiosk is currently enabled
            IKiosk kiosk = Pici.Injections.Container.GetInstance<IKiosk>();
            if (kiosk.IsEnabled) return;
            //send intent
            var infoIntent = new Intent("info", new Uri("http://www.hybridbookshelf.de/"));
            Pici.Intent.Send(infoIntent);
        }
    }
}