// HistomatSearchColors.cs
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

using Newtonsoft.Json;

namespace picibits.bib
{
    public class HistomatSearchColors
    {
        [JsonProperty("search_color_one")]
        public string Primary { get; set; }

        [JsonProperty("search_color_two")]
        public string Secondary { get; set; }

        [JsonProperty("search_color_three")]
        public string Tertiary { get; set; }

        public static HistomatSearchColors DEFAULT = new HistomatSearchColors
        {
            Primary = "#FFFF0000",
            Secondary = "#FF00FF00",
            Tertiary = "FF0000FF"
        };
    }
}