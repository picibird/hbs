// FilterCategory.cs
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
using System.Xml.Serialization;

using picibits.core;
using picibits.core.collection;
using picibits.core.mvvm;

namespace picibird.hbs.ldu
{
    public class FilterCategory : Model
    {


        [XmlAttribute("name")]
        public FilterCategoryId Id { get; set; }

        [XmlElement("term")]
        public PiciObservableCollection<Filter> Filter { get; set; }

        [XmlIgnore]
        public int Count
        {
            get
            {
                int sum = 0;
                foreach (Filter f in Filter)
                {
                    sum += f.Frequency;
                }
                return sum;
            }
        }

        // the name to be displayed

        [XmlIgnore]
        public string Name
        {
            get
            {
                return Pici.Resources.Find(Id.ToString());
            }
        }

        public override string ToString()
        {
            return "\r\n" + Name + "\r\n====\r\n\t" + String.Join("\r\n\t", Filter) + "\r\n";
        }
    }
}
