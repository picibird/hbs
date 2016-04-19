// SortOrderFunction.cs
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
using System.Collections.Generic;
using picibird.hbs.ldu;

using picibits.core;
using picibits.core.extension;

namespace picibird.hbs.viewmodels.infoShield
{
    public class SortOrderFunction
    {
        private static readonly Dictionary<SortOrder, SortOrderFunction> SORT_ORDER_FUNCTION_SINGELTONS =
            new Dictionary<SortOrder, SortOrderFunction>();

        public SortOrderFunction(SortOrder enumValue)
        {
            EnumValue = enumValue;
            StringValue = Pici.Resources.Find("sortorder_" + enumValue);
        }

        public string StringValue { get; private set; }
        public SortOrder EnumValue { get; }

        public static SortOrderFunction GetSingleton(SortOrder order)
        {
            SortOrderFunction func = null;
            if (!SORT_ORDER_FUNCTION_SINGELTONS.TryGetValue(order, out func))
            {
                func = new SortOrderFunction(order);
                SORT_ORDER_FUNCTION_SINGELTONS.Add(order, func);
            }
            return func;
        }

        public string GetRepresentative(Hit hit)
        {
            string repr = null;
            if (HasProperty(hit))
            {
                repr = GetProperty(hit).ToString();
                //if alphabetical, return first representative letter
                if (IsAlphabetical())
                {
                    repr = repr.ToLower();
                    //remove leading striung sequences that are ignored when sorting

                    repr = repr.TrimStart(new[]
                    {
                        " ",
                        "a ",
                        "the ",
                        "der ",
                        "die ",
                        "den ",
                        "des ",
                        "an ",
                        "(",
                        ")",
                        @"""",
                        "[",
                        "]",
                        "{",
                        "}"
                    });
                    repr = repr.Substring(0, 1).ToUpper();
                }
                else
                {
                    if (EnumValue == SortOrder.relevance)
                        repr = repr.FormatNumberWithThousandSeperator();
                }
            }
            return repr;
        }

        public bool IsAlphabetical()
        {
            var isAlpha = false;
            switch (EnumValue)
            {
                case SortOrder.relevance:
                    isAlpha = false;
                    break;
                case SortOrder.date:
                    isAlpha = false;
                    break;
                case SortOrder.author:
                    isAlpha = true;
                    break;
                case SortOrder.title:
                    isAlpha = true;
                    break;
            }
            return isAlpha;
        }

        public bool HasProperty(Hit hit)
        {
            var prop = GetProperty(hit);
            if (prop is string)
                return !string.IsNullOrEmpty((string) prop);
            return false;
        }

        public object GetProperty(Hit hit)
        {
            object prop = null;
            switch (EnumValue)
            {
                case SortOrder.relevance:
                    prop = hit.relevance;
                    break;
                case SortOrder.date:
                    prop = hit.date;
                    break;
                case SortOrder.author:
                    var author = hit.mergeAuthorString;
                    if (author.ToLower().Equals("zzz"))
                        author = "";
                    prop = author;
                    break;
                case SortOrder.title:
                    prop = hit.title;
                    break;
            }
            return prop;
        }

        public override bool Equals(object obj)
        {
            if (obj is SortOrderFunction)
            {
                var other = obj as SortOrderFunction;
                return EnumValue.Equals(other.EnumValue);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return EnumValue.GetHashCode();
        }
    }
}