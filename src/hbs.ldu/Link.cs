// Link.cs
// Date Created: 21.01.2016
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

using picibits.core;
using picibits.core.helper;
using picibits.core.intent;
using picibits.core.mvvm;

namespace picibird.hbs.ldu
{
    public class Link : Model
    {

        public static readonly string TYPE_TEXT = "Volltext";
        public static readonly string TYPE_LINK = "Link";

        public string Type { get; private set; }
        public Uri Uri { get; private set; }
        
        public string Title { get; private set; }
        public string Subtitle { get; private set; }

        public Hit Hit { get; private set; }

        public string UriString
        {
            get { return Uri.AbsoluteUri; }
        }

        #region ClickCommand

        private DelegateCommand mClickCommand;
        public DelegateCommand ClickCommand
        {
            get
            {
                if (mClickCommand == null)
                    mClickCommand = new DelegateCommand((param) => OnClick());
                return mClickCommand;
            }
        }

        #endregion ClickCommand


        public Link(string type, string uri, string title, string subtitle, Hit hit)
        {
            this.Type = type;
            this.Uri = new Uri(uri);

            this.Title = title;
            this.Subtitle = subtitle;

            this.Hit = hit;
        }

        private void OnClick()
        {
            Intent openUrlIntent = new Intent(Intent.ACTION_OPEN, Uri);
            openUrlIntent.AddExtra("title", Title);
            openUrlIntent.AddExtra("subtitle", Subtitle);
            Pici.Intent.Send(openUrlIntent);
        }

    }
}
