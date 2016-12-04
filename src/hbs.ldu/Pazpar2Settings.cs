// Pazpar2Settings.cs
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
using Flurl;

namespace picibird.hbs.ldu
{
    public static class Pazpar2Settings
    {
        public static Url COVER_PROVIDER_URL = new Url("http://hbs-kn.ldu.bsz-bw.de/coverdata.php");
        public static Url PAZPAR2_URL = new Url("http://hbs-kn.ldu.bsz-bw.de/pazpar2/search.pz2");
        public static Url BIBSHELF_URL = new Url("http://kn.bibshelf.de/");

        public static TimeSpan DELAY_STAT_REQUEST = TimeSpan.FromSeconds(0.5);
        public static TimeSpan DELAY_SHOW_REQUEST = TimeSpan.FromSeconds(1);
        public static TimeSpan DELAY_SHOW_REQUEST_AFTER_FINISH = TimeSpan.FromSeconds(2);
        public static TimeSpan DELAY_TERMLIST_REQUEST = TimeSpan.FromSeconds(1);
        public static TimeSpan WEB_REQUEST_TIMEOUT = TimeSpan.FromSeconds(20);
        public static TimeSpan DELAY_PING = TimeSpan.FromSeconds(30);

        public static bool LOG_HTTP_REQUESTS = false;
        public static bool LOG_HTTP_RESPONSES = false;
        public static int RESULTS_PER_PAGE = 18;


        public static int MAX_RECORDS = 100;


        /*
         * HISTOMAT 
         */
        public static bool LOAD_COVER_COLOR_SCHEME = true;
        // so far available: primary, contrast
        public static string COVER_COLOR_SCHEME_METHOD = "contrast";

        public static LduSourceUsage LduSourceUsage = LduSourceUsage.ALL;
    }
}