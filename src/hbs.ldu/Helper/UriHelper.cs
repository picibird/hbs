// UriHelper.cs
// Date Created: 28.01.2016
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
using System.Collections.Generic;
using System.Linq;
using Flurl;
using Flurl.Http;
using picibits.core;

namespace picibird.hbs.ldu.Helper
{
    public class UrlHelper
    {
        private static void LogReq(HttpCall obj)
        {
            if (Pazpar2Settings.LOG_HTTP_REQUESTS)
            {
                Pici.Log.debug(typeof(UrlHelper),
                    String.Format("\r\n\tREQUEST:  {0}\r\n\t{1} {2}", obj.GetHashCode(), obj.Request.Method, obj.Url));
            }
        }

        private static void LogResp(HttpCall obj)
        {
            if (Pazpar2Settings.LOG_HTTP_RESPONSES)
            {
                if (obj.Response != null)
                {
                    Pici.Log.debug(typeof(UrlHelper),
                        String.Format("\r\n\tRESPONSE: {0}\r\n\t{1}", obj.GetHashCode(), obj.Response));
                }
                else
                {
                    Pici.Log.debug(typeof(UrlHelper),
                        String.Format("\r\n\tRESPONSE: {0}\r\n\t{1}", obj.GetHashCode(), obj));
                }
            }
        }

        public static Url GetShowUrl(string sid, int start, int num, string sort)
        {
            return new Url(Pazpar2Settings.PAZPAR2_URL)
                .SetQueryParam("command", "show")
                .SetQueryParam("session", sid)
                .SetQueryParam("start", start)
                .SetQueryParam("sort", sort)
                .SetQueryParam("num", num)
                .SetQueryParam("block", 1);
        }

        public static Url GetQueryUrl(string sid, SearchRequest searchRequest)
        {
            Url res = new Url(Pazpar2Settings.PAZPAR2_URL)
                .SetQueryParam("command", "search")
                .SetQueryParam("session", sid)
                .SetQueryParam("sort", CreateSortStringFromSearchRequest(searchRequest))
                .SetQueryParam("query", CreateQueryStringFromSearchRequest(searchRequest));

            if (searchRequest.MaximumRecords.HasValue)
            {
                res.SetQueryParam("maxrecs", searchRequest.MaximumRecords.Value);
            }
            else
            {
                res.SetQueryParam("maxrecs", Pazpar2Settings.MAX_RECORDS);
            }

            string limit = CreateLimitFilterStringFromSearchRequest(searchRequest);
            if (!String.IsNullOrEmpty(limit))
            {
                res.SetQueryParam("limit", limit);
            }

            //if (searchRequest.HasSourceFilter())
            //{
            //    string sourceFilterIds = String.Join("|", searchRequest.GetSourceFilterIds());
            //    res.SetQueryParam("filter", String.Format("pz:id={0}", sourceFilterIds));
            //}
            //else
            //{
            //    //if has no user source filter applied, check if settings has exclusive filter applied
            //    if (Pazpar2Settings.LduSourceUsage != LduSourceUsage.ALL)
            //    {
            //        string sourceFilterString = LduSourceUsageHelper.ToFilterString(Pazpar2Settings.LduSourceUsage);
            //        res.SetQueryParam("filter", String.Format("pz:id={0}", sourceFilterString));
            //        Pici.Log.warn(typeof(UrlHelper), String.Format("SETTING LDU ONLY SOURCE TO", sourceFilterString));
            //    }
            //}
            return res;
        }

        private static string CreateLimitFilterStringFromSearchRequest(SearchRequest searchRequest)
        {
            //IEnumerable<IGrouping<FilterCategoryId, Filter>> queryFiltersByCategory =
            //    from f in searchRequest.GetActiveFilters()
            //    group f by f.Catgegory
            //    into fcat
            //    where fcat.Key != FilterCategoryId.author
            //          && fcat.Key != FilterCategoryId.subject
            //          && fcat.Key != FilterCategoryId.xtargets
            //    select fcat;

            //List<string> cat = new List<string>();
            //foreach (IGrouping<FilterCategoryId, Filter> fcat in queryFiltersByCategory)
            //{
            //    cat.Add(String.Format("{0}={1}", fcat.Key,
            //        String.Join("|", (from f in fcat select f.Id).ToList<string>())));
            //}

            //return String.Join(",", cat);
            return "";
        }

        private static string CreateQueryStringFromSearchRequest(SearchRequest searchRequest)
        {
            List<string> list = searchRequest.GetActiveQueryFilterStrings();
            list.Insert(0, searchRequest.SearchString);
            return String.Join(" and ", list);
        }

        public static string CreateSortStringFromSearchRequest(SearchRequest searchRequest)
        {
            return String.Format("{0}:{1}", searchRequest.SortOrder.ToPazpar2ParameterString(),
                (searchRequest.SortDirection == SortDirection.descending) ? 0 : 1);
        }


        public static Url GetInitUrl()
        {
            return new Url(Pazpar2Settings.PAZPAR2_URL)
                .SetQueryParam("command", "init");
        }

        public static Url GetStatUrl(string sid)
        {
            return new Url(Pazpar2Settings.PAZPAR2_URL)
                .SetQueryParam("command", "stat")
                .SetQueryParam("session", sid);
        }

        public static Url GetTermlistUrl(string sid)
        {
            return new Url(Pazpar2Settings.PAZPAR2_URL)
                .SetQueryParam("command", "termlist")
                .SetQueryParam("session", sid);
        }

        public static Url GetRecordUrl(string sid, string recid)
        {
            return new Url(Pazpar2Settings.PAZPAR2_URL)
                .SetQueryParam("command", "record")
                .SetQueryParam("session", sid)
                .SetQueryParam("id", recid);
        }

        public static Url GetPingUrl(string sid)
        {
            return new Url(Pazpar2Settings.PAZPAR2_URL)
                .SetQueryParam("command", "ping")
                .SetQueryParam("session", sid);
        }
    }
}