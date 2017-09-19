// BookConf.cs
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

using Newtonsoft.Json;
using picibits.core.settings;

namespace picibird.hbs.config
{
    public class BookConf : SettingsModel
    {
        public BookConf()
        {
            ShelfRotationY = -70;
            ShelfRotationX = 18;
            ShelfAdditionalScale = 0.8;

            DefaultCoverWidth = 660;
            DefaultCoverHeight = 790;
            DefaultSpineWidth = 165;

            ShelfMarginLeft = 30;
            ShelfMarginTop = 0;
            ShelfMarginRight = 30;
            ShelfMarginBottom = 0;

            ShelfBoardHeight = 12.0d;

            var minScale = 0.8;
            var maxScale = 1/minScale;

            MinCoverWidth = DefaultCoverWidth*minScale;
            MinCoverHeight = DefaultCoverHeight*minScale;
            MinSpineWidth = DefaultSpineWidth*minScale;

            MaxCoverWidth = DefaultCoverWidth*maxScale;
            MaxCoverHeight = DefaultCoverHeight*maxScale;
            MaxSpineWidth = DefaultSpineWidth*maxScale;
        }

        [JsonProperty]
        public double DefaultCoverWidth { get; set; }

        [JsonProperty]
        public double DefaultCoverHeight { get; set; }

        [JsonProperty]
        public double DefaultSpineWidth { get; set; }

        [JsonProperty]
        public double ShelfRotationX { get; set; }

        [JsonProperty]
        public double ShelfRotationY { get; set; }

        [JsonProperty]
        public double ShelfAdditionalScale { get; set; }

        [JsonProperty]
        public double ShelfMarginLeft { get; set; }

        [JsonProperty]
        public double ShelfMarginTop { get; set; }

        [JsonProperty]
        public double ShelfMarginRight { get; set; }

        [JsonProperty]
        public double ShelfMarginBottom { get; set; }

        [JsonProperty]
        public double ShelfBoardHeight { get; set; }

        public double MinCoverWidth { get; private set; }
        public double MinCoverHeight { get; private set; }
        public double MinSpineWidth { get; private set; }

        public double MaxCoverWidth { get; private set; }
        public double MaxCoverHeight { get; private set; }
        public double MaxSpineWidth { get; private set; }
    }
}