﻿// SortOrder.cs
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

using picibird.shelfhub;

namespace picibird.hbs.ldu
{

    public class SortOrder : SortField
    {

        public SortOrder(SortField sortField = null): base()
        {
            if(sortField == null)
            {
                sortField = new SortField();
            }
            this.Key = sortField.Key;
            this.Label = sortField.Label;
            this.Type = sortField.Type;
            this.PropPath = sortField.PropPath;
        }

        public override bool Equals(object obj)
        {
            if(obj is SortOrder sortOrder)
            {
                return sortOrder.Key == this.Key;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}