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
using picibird.shelfhub;
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
            StringValue = Pici.Resources.Find(enumValue.Label);
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
                    repr = repr.Substring(0, 1).ToUpper();
                }
                else
                {
                    //if (EnumValue == SortOrder.relevance)
                    //    repr = repr.FormatNumberWithThousandSeperator();
                }
            }
            return repr;
        }

        public bool IsAlphabetical()
        {
            return EnumValue.Type == SortFieldType.Alphabetical;
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
            switch (EnumValue.Type)
            {
                case SortFieldType.Alphabetical:
                    prop = hit.title;
                    break;
                case SortFieldType.Score:
                case SortFieldType.Date:
                case SortFieldType.Numerical:
                    prop = hit.date;
                    break;
                default:
                    prop = hit.date;
                    break;
            }
            return prop;
        }

        public override bool Equals(object obj)
        {
            if (obj is SortOrderFunction sof)
            {
                return EnumValue.Equals(sof.EnumValue);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return EnumValue.GetHashCode();
        }
    }
}